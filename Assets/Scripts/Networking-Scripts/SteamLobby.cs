using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using Steamworks;

// The one script that talks to Steam lobbies, for Football and Golf: creating, listing, joining and leaving them. The lobby's
// "GameMode" data decides which game's session starts. Football's session is NetworkManagerGRF's; Golf's host start is here.
// Golf's game settings are on GolfLobbySettings.
public class SteamLobby : MonoBehaviour
{
    public static SteamLobby instance;

    public const string GameModeFootball = "Football";
    public const string GameModeGolf = "Golf";
    private const string HostAddressKey = "HostAddress";
    private const string GolfSceneName = "Golf-prototype-topdown";

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<LobbyMatchList_t> Callback_lobbyList;
    protected Callback<LobbyDataUpdate_t> Callback_lobbyInfo;

    public List<CSteamID> lobbyIDS = new List<CSteamID>();

    // The Steam lobby this player is in, or 0.
    public ulong CurrentLobbyID { get; private set; }
    public string CurrentLobbyName { get; private set; }

    private NetworkManager _networkManager;
    // What the lobby being created is for, kept until Steam reports that it was created.
    private string _pendingGameMode;
    private string _pendingLobbyName;
    private string _pendingCourseName;
    // The last lobby this player created. Steam reports entering it right after creating it.
    private ulong _hostedLobbyID;
    // Golf's host: the server starts, then the Golf scene loads, then the host's own client starts.
    private bool _startingGolfHost = false;
    private bool _startHostClientWhenGolfSceneLoads = false;

    private void Awake()
    {
        MakeInstance();
        _networkManager = GetComponent<NetworkManager>();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    private void Start()
    {
        if (instance != this)
            return;
        _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
        _networkManager.SceneManager.OnLoadEnd += SceneManager_OnLoadEnd;

        if (!SteamManager.Initialized) { return; }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        Callback_lobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbiesList);
        Callback_lobbyInfo = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyInfo);
    }
    private void OnDestroy()
    {
        if (instance != this || _networkManager == null)
            return;
        _networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
        _networkManager.SceneManager.OnLoadEnd -= SceneManager_OnLoadEnd;
    }
    bool IsSteamReady()
    {
        if (SteamManager.Initialized)
            return true;
        Debug.LogWarning("SteamLobby: Steam isn't running, so Steam lobbies can't be used.");
        return false;
    }
    // A server or client is running, or this player is in a Steam lobby that is about to start one.
    bool IsInSession()
    {
        return _networkManager.ServerManager.Started || _networkManager.ClientManager.Started || CurrentLobbyID != 0;
    }

    public void CreateFootballLobby(ELobbyType lobbyType, string lobbyName)
    {
        CreateLobby(GameModeFootball, lobbyType, NetworkManagerGRF.singleton.maxConnections, lobbyName, string.Empty);
    }
    public void CreateGolfLobby(ELobbyType lobbyType, int maxPlayers, string lobbyName, string courseName)
    {
        CreateLobby(GameModeGolf, lobbyType, maxPlayers, lobbyName, courseName);
    }
    void CreateLobby(string gameMode, ELobbyType lobbyType, int maxPlayers, string lobbyName, string courseName)
    {
        if (!IsSteamReady())
            return;
        _pendingGameMode = gameMode;
        _pendingLobbyName = string.IsNullOrEmpty(lobbyName) ? DefaultLobbyName() : lobbyName;
        _pendingCourseName = courseName ?? string.Empty;
        Debug.Log("SteamLobby: CreateLobby: " + gameMode + " lobby \"" + _pendingLobbyName + "\" for " + maxPlayers + " players");
        SteamMatchmaking.CreateLobby(lobbyType, maxPlayers);
    }
    string DefaultLobbyName()
    {
        string steamUserName = SteamFriends.GetPersonaName();
        if (steamUserName.Length > 12)
            steamUserName = steamUserName.Substring(0, 12);
        return steamUserName + "'s lobby";
    }
    public void JoinLobby(CSteamID lobbyId)
    {
        if (!IsSteamReady())
            return;
        Debug.Log("JoinLobby: Will try to join lobby with steam id: " + lobbyId.ToString());
        SteamMatchmaking.JoinLobby(lobbyId);
    }
    public void GetListOfLobbies()
    {
        if (!IsSteamReady())
            return;

        if (lobbyIDS.Count > 0)
            lobbyIDS.Clear();

        SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(1);

        SteamAPICall_t try_getList = SteamMatchmaking.RequestLobbyList();
    }
    public void LeaveLobby()
    {
        if (CurrentLobbyID == 0)
            return;
        try
        {
            Debug.Log("SteamLobby: LeaveLobby: " + CurrentLobbyID.ToString());
            SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyID));
        }
        catch (Exception e)
        {
            // Steam may already have shut down when the game quits.
            Debug.Log("SteamLobby: LeaveLobby: failed to leave lobby? Error: " + e);
        }
        CurrentLobbyID = 0;
        CurrentLobbyName = string.Empty;
    }
    // Hides the lobby from the lobby list and refuses new joins. Players stay in it until their session ends.
    public void SetGameStatusToInGame()
    {
        if (CurrentLobbyID == 0)
            return;
        Debug.Log("SetGameStatusToInGame: for lobby ID: " + CurrentLobbyID.ToString());
        SteamMatchmaking.SetLobbyData(
            new CSteamID(CurrentLobbyID),
            "GameStatus",
            "InGame");
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        string gameMode = _pendingGameMode;
        _pendingGameMode = null;
        if (string.IsNullOrEmpty(gameMode))
            return;
        Debug.Log("SteamLobby: OnLobbyCreated: " + callback.m_eResult.ToString() + " for a " + gameMode + " lobby");
        if (callback.m_eResult != EResult.k_EResultOK)
            return;

        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        _hostedLobbyID = callback.m_ulSteamIDLobby;
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        CurrentLobbyName = _pendingLobbyName;

        SteamMatchmaking.SetLobbyData(lobbyId, HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(lobbyId, "name", _pendingLobbyName);
        // Only needed when doing testing with the Spacewar Appid
        SteamMatchmaking.SetLobbyData(lobbyId, "GameName", "GRF");
        SteamMatchmaking.SetLobbyData(lobbyId, "GameMode", gameMode);
        if (gameMode == GameModeGolf)
            SteamMatchmaking.SetLobbyData(lobbyId, "CourseName", _pendingCourseName);
        SteamMatchmaking.SetLobbyData(lobbyId, "GameStatus", "Lobby");

        if (gameMode == GameModeFootball)
        {
            NetworkManagerGRF.singleton.StartHost();
            if (!NetworkManagerGRF.singleton.IsFootballSession)
                LeaveLobby();
        }
        else
        {
            StartGolfHost();
        }
    }
    // Steam invites and "Join Game" in the Steam overlay.
    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("OnGameLobbyJoinRequested: " + callback.m_steamIDLobby.ToString());
        if (IsInSession())
        {
            Debug.Log("OnGameLobbyJoinRequested: already in a game. Leave it first, then accept the invite again.");
            return;
        }
        JoinLobby(callback.m_steamIDLobby);
    }
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (callback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            Debug.LogWarning("SteamLobby: couldn't enter lobby " + callback.m_ulSteamIDLobby.ToString() + ": " + ((EChatRoomEnterResponse)callback.m_EChatRoomEnterResponse).ToString());
            return;
        }
        Debug.Log("OnLobbyEntered for lobby with id: " + callback.m_ulSteamIDLobby.ToString());

        lobbyIDS.Clear();
        if (TitleScreenManager.instance != null && TitleScreenManager.instance.listOfLobbyListItems.Count > 0)
            TitleScreenManager.instance.DestroyOldLobbyListItems();

        // The host entering its own lobby. OnLobbyCreated already stored it and started hosting.
        if (callback.m_ulSteamIDLobby == _hostedLobbyID)
            return;

        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        CurrentLobbyName = SteamMatchmaking.GetLobbyData(lobbyId, "name");
        string gameStatus = SteamMatchmaking.GetLobbyData(lobbyId, "GameStatus");
        string gameMode = SteamMatchmaking.GetLobbyData(lobbyId, "GameMode");
        string hostAddress = SteamMatchmaking.GetLobbyData(lobbyId, HostAddressKey);
        Debug.Log("OnLobbyEntered: game mode: " + gameMode + " game status: " + gameStatus);

        if (gameStatus != "Lobby" || string.IsNullOrEmpty(hostAddress))
        {
            Debug.LogWarning("SteamLobby: lobby " + CurrentLobbyID.ToString() + " can't be joined. Game status: \"" + gameStatus + "\"");
            LeaveLobby();
            return;
        }
        if (gameMode == GameModeFootball)
        {
            NetworkManagerGRF.singleton.networkAddress = hostAddress;
            NetworkManagerGRF.singleton.StartClient();
            if (!NetworkManagerGRF.singleton.IsFootballSession)
                LeaveLobby();
        }
        else if (gameMode == GameModeGolf)
        {
            if (!_networkManager.ClientManager.StartConnection(hostAddress))
                LeaveLobby();
        }
        else
        {
            Debug.LogWarning("SteamLobby: lobby " + CurrentLobbyID.ToString() + " has an unknown game mode: \"" + gameMode + "\"");
            LeaveLobby();
        }
    }
    void StartGolfHost()
    {
        _startingGolfHost = true;
        if (!_networkManager.ServerManager.StartConnection())
        {
            _startingGolfHost = false;
            Debug.LogWarning("SteamLobby: the Golf server didn't start.");
            LeaveLobby();
        }
    }
    private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            if (!_startingGolfHost)
                return;
            _startingGolfHost = false;
            // As with Football's host (NetworkManagerGRF), load the scene now and start the host's own client once it has
            // loaded on the server. Started any earlier, FishNet could spawn the host's NetworkPlayer while the scene was
            // still loading: before LobbyManagerGolf existed, or into FishNet's temporary MovedObjectsHolder scene, where
            // other players never see it.
            SceneLoadData sld = new SceneLoadData(GolfSceneName);
            sld.ReplaceScenes = ReplaceOption.All;
            _networkManager.SceneManager.LoadGlobalScenes(sld);
            _startHostClientWhenGolfSceneLoads = true;
        }
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            _startingGolfHost = false;
            _startHostClientWhenGolfSceneLoads = false;
            LeaveLobby();
        }
    }
    // Every way out of a session ends here: the menus, the host quitting, and a join that failed or was refused.
    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Stopped)
            LeaveLobby();
    }
    private void SceneManager_OnLoadEnd(SceneLoadEndEventArgs args)
    {
        if (!_startHostClientWhenGolfSceneLoads || !args.QueueData.AsServer)
            return;
        foreach (UnityEngine.SceneManagement.Scene loadedScene in args.LoadedScenes)
        {
            if (loadedScene.name == GolfSceneName)
            {
                _startHostClientWhenGolfSceneLoads = false;
                _networkManager.ClientManager.StartConnection();
                return;
            }
        }
    }
    void OnGetLobbiesList(LobbyMatchList_t result)
    {
        Debug.Log("Found " + result.m_nLobbiesMatching + " lobbies!");
        if (TitleScreenManager.instance == null)
            return;
        if (TitleScreenManager.instance.listOfLobbyListItems.Count > 0)
            TitleScreenManager.instance.DestroyOldLobbyListItems();
        for (int i = 0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIDS.Add(lobbyID);
            SteamMatchmaking.RequestLobbyData(lobbyID);

        }

    }
    void OnGetLobbyInfo(LobbyDataUpdate_t result)
    {
        // Steam also sends these for the lobby you're in, after the title screen is gone.
        if (TitleScreenManager.instance == null)
            return;
        TitleScreenManager.instance.DisplayLobbies(lobbyIDS, result);
    }
}
