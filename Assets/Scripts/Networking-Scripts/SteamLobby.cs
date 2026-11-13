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

    // A lobby browser subscribes to these instead of being called into by name. Raised alongside
    // the existing TitleScreenManager calls, so both title screens can be live at once.
    public event System.Action LobbyListCleared;
    public event System.Action<CSteamID> LobbyInfoReceived;
    private const string HostAddressKey = "HostAddress";
    public const string GolfSceneName = "Golf-prototype-topdown";

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<LobbyMatchList_t> Callback_lobbyList;
    protected Callback<LobbyDataUpdate_t> Callback_lobbyInfo;

    public List<CSteamID> lobbyIDS = new List<CSteamID>();

    // The Steam lobby this player is in, or 0.
    public ulong CurrentLobbyID { get; private set; }
    public string CurrentLobbyName { get; private set; }
    // The Steam id of the Golf host this player last joined, so a lobby moving to a new host doesn't reconnect to the
    // one that left (LobbyHostMigration). Football's is NetworkManagerGRF.networkAddress.
    public string GolfHostAddress { get; private set; }

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
    // Someone who entered a Football lobby while it was moving to a new host waits this long for the move to finish.
    private const float JoinMovingLobbyTimeoutSeconds = 20f;
    private Coroutine _joinWhenHostReadyRoutine;

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

        // Steam is how the game connects unless a test session says otherwise. Multipass logs an error if anything reads
        // ClientTransport before it has been set, and FishNet reads it earlier than the first host or join, so set it here.
        UseSteamTransport();

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
        _pendingLobbyName = string.IsNullOrWhiteSpace(lobbyName) ? DefaultLobbyName() : lobbyName.Trim();
        _pendingCourseName = courseName ?? string.Empty;
        Debug.Log("SteamLobby: CreateLobby: " + gameMode + " lobby \"" + _pendingLobbyName + "\" for " + maxPlayers + " players");
        SteamMatchmaking.CreateLobby(lobbyType, maxPlayers);
    }
    string DefaultLobbyName()
    {
        string steamUserName = SteamFriends.GetPersonaName();
        if (string.IsNullOrWhiteSpace(steamUserName))
            return "Player's Lobby";
        steamUserName = steamUserName.Trim();
        if (steamUserName.Length > 12)
            steamUserName = steamUserName.Substring(0, 12);
        return steamUserName + "'s Lobby";
    }
    public void JoinLobby(CSteamID lobbyId)
    {
        if (!IsSteamReady())
            return;
        Debug.Log("JoinLobby: Will try to join lobby with steam id: " + lobbyId.ToString());
        // A lobby joined from the list or an invite is never one being created, even one this player created earlier: a
        // lobby now outlives its creator by moving to a new host. OnLobbyEntered took the creator coming back for the
        // creator entering it right after creating it, and returned before joining or setting CurrentLobbyID, so the
        // player sat in the Steam lobby unseen for the rest of the run. That filled a 1v1 lobby, which dropped it from
        // the list, and kept every old lobby alive with one member nobody could reach.
        _hostedLobbyID = 0;
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
        if (_joinWhenHostReadyRoutine != null)
        {
            StopCoroutine(_joinWhenHostReadyRoutine);
            _joinWhenHostReadyRoutine = null;
        }
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

        UseSteamTransport();
        if (gameMode == GameModeFootball)
        {
            NetworkManagerGRF.singleton.StartHost();
            if (!NetworkManagerGRF.singleton.IsFootballSession)
                LeaveLobby();
        }
        else
        {
            if (!StartGolfHost(false))
                LeaveLobby();
        }
    }
    // The NetworkManager's transport is Multipass, holding FishySteamworks and Tugboat. The server opens both, but a client
    // has to be told which one to connect with, and DevDirectConnect may have left it on Tugboat. Everything that goes
    // through a Steam lobby belongs on FishySteamworks, so say so before each host or join.
    private void UseSteamTransport()
    {
        FishNet.Transporting.Multipass.Multipass multipass = _networkManager.TransportManager.Transport as FishNet.Transporting.Multipass.Multipass;
        if (multipass != null)
            multipass.SetClientTransport<FishySteamworks.FishySteamworks>();
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
        if (LobbyListCleared != null)
            LobbyListCleared();

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
        UseSteamTransport();
        if (gameMode == GameModeFootball || gameMode == GameModeGolf)
        {
            // A lobby whose host just left is moving to Steam's new owner, and HostAddress still names the old host until
            // the new one is ready (LobbyHostMigration). Connecting now would only fail.
            if (!IsHostAddressCurrent(lobbyId))
            {
                Debug.Log("OnLobbyEntered: the lobby is moving to a new host. Waiting for it before joining.");
                _joinWhenHostReadyRoutine = StartCoroutine(JoinWhenHostReadyRoutine(callback.m_ulSteamIDLobby, gameMode));
                return;
            }
            JoinHost(gameMode, hostAddress);
        }
        else
        {
            Debug.LogWarning("SteamLobby: lobby " + CurrentLobbyID.ToString() + " has an unknown game mode: \"" + gameMode + "\"");
            LeaveLobby();
        }
    }
    void JoinHost(string gameMode, string hostAddress)
    {
        if (gameMode == GameModeGolf)
            JoinGolfHost(hostAddress);
        else
            JoinFootballHost(hostAddress);
    }
    // Also how LobbyHostMigration reconnects to a moved lobby's new host.
    public void JoinFootballHost(string hostAddress)
    {
        UseSteamTransport();
        NetworkManagerGRF.singleton.networkAddress = hostAddress;
        NetworkManagerGRF.singleton.StartClient();
        if (!NetworkManagerGRF.singleton.IsFootballSession)
            LeaveLobby();
    }
    // Also how LobbyHostMigration reconnects to a moved Golf lobby's new host.
    public void JoinGolfHost(string hostAddress)
    {
        UseSteamTransport();
        GolfHostAddress = hostAddress;
        if (!StartGolfClient(hostAddress, false))
            LeaveLobby();
    }
    // Joins a Golf host over whichever transport the client is pointed at: Steam's, or Tugboat for DevDirectConnect's dev
    // sessions. A null address reconnects to the last one, which is how a dev lobby moving to a new host on the same
    // machine finds it.
    public bool StartGolfClient(string hostAddress, bool isDevSession)
    {
        if (GolfLobbySettings.instance != null)
            GolfLobbySettings.instance.BeginSession(isDevSession);
        if (hostAddress == null)
            return _networkManager.ClientManager.StartConnection();
        return _networkManager.ClientManager.StartConnection(hostAddress);
    }
    IEnumerator JoinWhenHostReadyRoutine(ulong lobbyID, string gameMode)
    {
        CSteamID lobbyId = new CSteamID(lobbyID);
        float giveUpTime = Time.realtimeSinceStartup + JoinMovingLobbyTimeoutSeconds;
        while (Time.realtimeSinceStartup < giveUpTime)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            if (CurrentLobbyID != lobbyID)
                yield break;
            if (IsHostAddressCurrent(lobbyId))
            {
                _joinWhenHostReadyRoutine = null;
                Debug.Log("JoinWhenHostReadyRoutine: the lobby has its new host. Joining.");
                JoinHost(gameMode, SteamMatchmaking.GetLobbyData(lobbyId, HostAddressKey));
                yield break;
            }
        }
        _joinWhenHostReadyRoutine = null;
        Debug.LogWarning("SteamLobby: the lobby's new host didn't appear within " + JoinMovingLobbyTimeoutSeconds + " seconds. Leaving it.");
        LeaveLobby();
    }
    // HostAddress names the lobby's current owner: the host is where players should connect.
    bool IsHostAddressCurrent(CSteamID lobbyId)
    {
        return SteamMatchmaking.GetLobbyData(lobbyId, HostAddressKey) == SteamMatchmaking.GetLobbyOwner(lobbyId).ToString();
    }
    // For LobbyHostMigration, whose lobby lost its host. Steam hands ownership to a remaining member on its own.
    public bool IsThisPlayerLobbyOwner()
    {
        if (CurrentLobbyID == 0 || !SteamManager.Initialized)
            return false;
        return SteamMatchmaking.GetLobbyOwner(new CSteamID(CurrentLobbyID)) == SteamUser.GetSteamID();
    }
    // Who Steam names as the lobby's owner, and their Steam name, for the moving-lobby screen. Steam knows the names of
    // everyone in a lobby this player is in.
    public bool TryGetLobbyOwner(out string ownerAddress, out string ownerName)
    {
        ownerAddress = "";
        ownerName = "";
        if (CurrentLobbyID == 0 || !SteamManager.Initialized)
            return false;
        CSteamID owner = SteamMatchmaking.GetLobbyOwner(new CSteamID(CurrentLobbyID));
        ownerAddress = owner.ToString();
        ownerName = SteamFriends.GetFriendPersonaName(owner);
        return true;
    }
    // The new host, once it has said it is ready: HostAddress names the lobby's owner, and that owner isn't this player.
    public bool TryGetLobbyHost(out string hostAddress)
    {
        hostAddress = "";
        if (CurrentLobbyID == 0 || !SteamManager.Initialized)
            return false;
        CSteamID lobbyId = new CSteamID(CurrentLobbyID);
        if (!IsHostAddressCurrent(lobbyId) || IsThisPlayerLobbyOwner())
            return false;
        hostAddress = SteamMatchmaking.GetLobbyData(lobbyId, HostAddressKey);
        return true;
    }
    // A player who took over a moved lobby, once its own player is in: point everyone at it. Only the owner can write
    // lobby data, and Steam made this player the owner.
    public void AnnounceThisPlayerAsHost()
    {
        if (!IsThisPlayerLobbyOwner())
            return;
        Debug.Log("SteamLobby: this player now hosts lobby " + CurrentLobbyID.ToString() + ".");
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), HostAddressKey, SteamUser.GetSteamID().ToString());
    }
    // A one-player golf lobby can never gain peers, so it skips Steam entirely: no Steam lobby is created, and the
    // session runs on Tugboat bound to loopback, like Football's single player - it works with Steam closed and a
    // shipped build raises no firewall prompt. CurrentLobbyID stays 0, so every lobby-dependent call (LeaveLobby,
    // SetGameStatusToInGame) already no-ops, and the course and settings come from GolfLobbySettings as always -
    // the lobby data only ever fed the lobby list.
    public void StartSoloGolfSession()
    {
        if (!NetworkManagerGRF.singleton.UseLoopbackTransport())
        {
            Debug.LogWarning("SteamLobby: could not point the transport at loopback Tugboat for solo golf.");
            return;
        }
        StartGolfHost(false);
    }
    // Golf's host start, for a Steam lobby, solo golf, a dev session (DevDirectConnect) and a lobby moving to a new host
    // (LobbyHostMigration), with GolfLobbySettings already filled in. Only the transport the local client is pointed at
    // is started: the aggregate StartConnection fails outright whenever FishySteamworks can't start, which it can't
    // without Steam (the bcfa0fe7 lesson), and a Steam lobby never needs Tugboat.
    public bool StartGolfHost(bool isDevSession)
    {
        GolfLobbySettings.instance.BeginSession(isDevSession);
        _startingGolfHost = true;
        if (!NetworkManagerGRF.singleton.StartServerConnection())
        {
            _startingGolfHost = false;
            Debug.LogWarning("SteamLobby: the Golf server didn't start.");
            return false;
        }
        return true;
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
    // Except Football's: NetworkManagerGRF ends that session and leaves the lobby itself. A lobby of either game whose
    // host left stays in the Steam lobby while it moves to the new host (LobbyHostMigration).
    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState != LocalConnectionState.Stopped)
            return;
        if (NetworkManagerGRF.singleton != null && NetworkManagerGRF.singleton.IsFootballSession)
            return;
        if (LobbyHostMigration.instance != null)
        {
            if (LobbyHostMigration.instance.HandleClientStopped())
                return;
            LobbyHostMigration.instance.Forget();
        }
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
        // Steam also sends these once the title screen is gone, with nothing listening.
        if (TitleScreenManager.instance == null && LobbyListCleared == null)
            return;
        if (TitleScreenManager.instance != null && TitleScreenManager.instance.listOfLobbyListItems.Count > 0)
            TitleScreenManager.instance.DestroyOldLobbyListItems();
        if (LobbyListCleared != null)
            LobbyListCleared();
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
        if (TitleScreenManager.instance != null)
            TitleScreenManager.instance.DisplayLobbies(lobbyIDS, result);
        if (LobbyInfoReceived != null)
            LobbyInfoReceived(new CSteamID(result.m_ulSteamIDLobby));
    }
}
