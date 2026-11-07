using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Managing;
using TMPro;
using UnityEngine.UI;
using Steamworks;
using FishNet.Managing.Scened;
using UnityEngine.SceneManagement;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("Golf Player Stuff")]
    [SerializeField] GameObject _golfPlayerPrefab;
    [SerializeField] GolfPlayerTopDown _golfPlayerScript;
    private readonly SyncVar<ulong> _syncPlayerSteamID = new SyncVar<ulong>();
    public ulong PlayerSteamID { get => _syncPlayerSteamID.Value; set => _syncPlayerSteamID.Value = value; }

    [Header("Color Picker Stuff")]
    private readonly SyncVar<Color> _ballColor = new SyncVar<Color>(Color.white);
    [SerializeField] BallColorPicker _ballColorPicker;

    [Header("UI Stuff")]
    [SerializeField] GameObject _playerUICanvas;
    [SerializeField] Button _readyButton;
    [SerializeField] Button _startGameButton;
    [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] Button _submitPlayerNameButton;
    [SerializeField] TextMeshProUGUI _welcomeText;
    [SerializeField] GameObject _uiHolder;
    [SerializeField] TextMeshProUGUI _lobbyNameText;
    [SerializeField] GameObject _promptPlayerToDownloadHolder;
    [SerializeField] TextMeshProUGUI _promptPlayerToDownloadText;



    [Header("Player List Item")]
    [SerializeField] GameObject _lobbyPlayerListItemPrefab;
    [SerializeField] GameObject _myLobbyPlayerListObject;
    [SerializeField] GolfPlayerListItem _myLobbyPlayerListScript;

    [Header("Player Status")]
    private readonly SyncVar<bool> _syncIsReady = new SyncVar<bool>();
    public bool IsReady { get => _syncIsReady.Value; set => _syncIsReady.Value = value; }
    private readonly SyncVar<string> _syncPlayerName = new SyncVar<string>();
    public string PlayerName { get => _syncPlayerName.Value; set => _syncPlayerName.Value = value; }
    // The order players joined in, from GameModePlayerSpawner before the spawn. A dev lobby whose host leaves moves to the
    // remaining player who joined first; FishNet's object ids don't follow join order. The host's own player is always
    // HostJoinNumber, since after a move other players can connect before the new host's own client does.
    public const int HostJoinNumber = 0;
    private readonly SyncVar<int> _syncJoinNumber = new SyncVar<int>();
    public int JoinNumber { get => _syncJoinNumber.Value; set => _syncJoinNumber.Value = value; }

    [Header("Prompt Player To Download")]
    [SerializeField] GameObject _promptPlayerHolder;
    [SerializeField] TextMeshProUGUI _promptPlayerText;
    [SerializeField] ulong _workshopPublishedItemID;

    [Header("Networking stuff?")]
    [SerializeField] NetworkManager _networkManager;

    private void Awake()
    {
        _syncPlayerSteamID.OnChange += SyncPlayerSteamID;
        _ballColor.OnChange += SyncBallColor;
        _syncIsReady.OnChange += SyncIsReady;
        _syncPlayerName.OnChange += SyncPlayerName;
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        
        
        if (!_networkManager)
        {
            _networkManager = GameObject.FindGameObjectWithTag("GolfNetworkManager").GetComponent<NetworkManager>();
        }
        // The PlayerSpawner can start this before the Golf scene's GameplayManager exists or has started on the server.
        // In that case the GameplayManager registers this player in its own OnStartServer.
        GameplayManagerTopDownGolf gameplayManager = GameplayManagerTopDownGolf.instance;
        if (gameplayManager != null && gameplayManager.IsServerInitialized)
            gameplayManager.AddNetworkPlayer(this);

    }
    // GameModePlayerSpawner calls this once the spawn is done. FishNet only adds a new object's observers after its start
    // callbacks, so a TargetRpc sent from OnStartServer reaches nobody ("Target is not an observer"); the host's own
    // player lost its "can't start yet" that way.
    [Server]
    public void ServerSpawned()
    {
        // Each player keeps the host's settings, so whoever takes over the lobby if the host leaves can start it with
        // the same course and rules.
        if (GolfLobbySettings.instance != null)
            RpcReceiveGolfSettings(this.Owner, GolfLobbySettings.instance.CurrentSettings());
        GameplayManagerTopDownGolf gameplayManager = GameplayManagerTopDownGolf.instance;
        if (gameplayManager == null || !gameplayManager.IsServerInitialized)
            return;

        // A hole being tested from the map maker has nobody to wait for, so it starts here instead
        // of through the lobby's ready up and Start Game.
        if (MapMakerDropInSession.IsActive && this.Owner.IsHost)
        {
            Debug.Log("ServerSpawned: starting a map maker drop-in.");
            MapMakerDropInWeather.Create();
            // A normal session learns its course from an RPC sent when the gameplay manager starts,
            // and gets to it in the seconds the lobby is open. Starting here is far earlier than
            // that, so the drop-in's course is handed straight over.
            gameplayManager.CourseToPlay = MapMakerDropInSession.CourseToPlay;
            SpawnGolfPlayerObjectsForEachPlayer();
            gameplayManager.HostStartGame(this.Owner);
            return;
        }

        CheckIfHostCanStartGame();
    }
    public override void OnStopServer()
    {
        base.OnStopServer();
        GameplayManagerTopDownGolf gameplayManager = GameplayManagerTopDownGolf.instance;
        if (gameplayManager != null && gameplayManager.IsServerInitialized)
            gameplayManager.RemoveNetworkPlayer(this);
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!this.IsOwner)
        {
            Debug.Log("OnStartClient: DEactivating player canvas for player with id: " + this.ObjectId);
            _playerUICanvas.SetActive(false);
            _readyButton.gameObject.SetActive(false);
            _playerNameInput.gameObject.SetActive(false);
            _submitPlayerNameButton.gameObject.SetActive(false);
            _welcomeText.gameObject.SetActive(false);

            gameObject.tag = "NetworkPlayer";
        }
        else
        {
            Debug.Log("OnStartClient: ACTIVATING player canvas for player with id: " + this.ObjectId);
            _playerUICanvas.SetActive(true);
            _readyButton.gameObject.SetActive(true);
            _playerNameInput.gameObject.SetActive(false);
            _submitPlayerNameButton.gameObject.SetActive(false);
            
            if (!_networkManager)
            {
                _networkManager = GameObject.FindGameObjectWithTag("GolfNetworkManager").GetComponent<NetworkManager>();
            }

            gameObject.name = "LocalNetworkPlayer";
            gameObject.tag = "LocalNetworkPlayer";

            SetPlayerName();
            _ballColorPicker.GetPlayerPrefValues();
            SetPlayerSteamID();
            // The Steam lobby was entered before this player's connection started, so the name is known by now. This used
            // to wait for OnClientLoadedStartScenes, but that fires before the player is spawned, so it never ran.
            SetLobbyNameText(SteamLobby.instance.CurrentLobbyName);

            // Back in a lobby that moved to a new host; a new host's own player also has the lobby pointed at it.
            if (LobbyHostMigration.instance != null)
            {
                LobbyHostMigration.instance.ArrivedInLobby();
                if (IsServerStarted)
                    LobbyHostMigration.instance.FinishTakingOver();
            }
        }

        _startGameButton.gameObject.SetActive(false);
        SpawnLobbyPlayerListItem();
        if (LobbyHostMigration.instance != null)
            LobbyHostMigration.instance.RememberGolfLobby(null);
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
        LobbyManagerGolf.instance.RemoveLobbyPlayerListItem(_myLobbyPlayerListObject);
        if (!this.IsOwner)
        {
            if (LobbyHostMigration.instance != null)
                LobbyHostMigration.instance.RememberGolfLobby(this);
            return;
        }
        // The host left the lobby: it carries on under a new host, which takes this player to the title screen itself
        // and keeps the Steam lobby (LobbyHostMigration, once the connection reports it has stopped).
        if (LobbyHostMigration.instance != null && LobbyHostMigration.instance.WillMoveLobby())
            return;

        //SceneLoadData sld = new SceneLoadData("TitleScreen");
        //sld.ReplaceScenes = ReplaceOption.All;
        //FishNet.InstanceFinder.SceneManager.LoadGlobalScenes(sld);
        // A session the map maker started to test a hole goes back to the map maker, not the title screen
        UnityEngine.SceneManagement.SceneManager.LoadScene(MapMakerDropInSession.ReturnScene);
        SteamLobby.instance.LeaveLobby();

    }
    void SetLobbyNameText(string lobbyName)
    {
        if (string.IsNullOrEmpty(lobbyName))
            return;
        _lobbyNameText.text = lobbyName;
        _lobbyNameText.gameObject.SetActive(true);
    }
    public void PlayerReadyUp()
    {
        if (!this.IsOwner)
            return;
        this.CmdPlayerReadyUp();
    }
    [ServerRpc]
    void CmdPlayerReadyUp()
    {
        this.IsReady = !this.IsReady;
        CheckIfHostCanStartGame();

    }
    [Server]
    public void CheckIfHostCanStartGame()
    {
        if (AreAllPlayersReady())
        {
            TellHostTheyCanStartGame();
        }
        else
        {
            CannotStartGameYet();
        }
    }
    void SyncIsReady(bool prev, bool next, bool asServer)
    {
        if (asServer)
            return;
        _myLobbyPlayerListScript.UpdatePlayerItemReadyStatus(next);
        if (!this.IsOwner)
            return;

        if (next)
        {
            _readyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Unready";
        }
        else
        { 
            _readyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Ready up";
        }
    }
    [Server]
    bool AreAllPlayersReady()
    {
        bool allReady = false;
        foreach (NetworkPlayer player in GameplayManagerTopDownGolf.instance.NetworkPlayersServer)
        {
            if (player.IsReady)
            {
                allReady = true;
                Debug.Log("AreAllPlayersReady: Player: " + player.PlayerName + ":" + player.OwnerId + " ready status: " + player.IsReady.ToString());
            }
            else
            {
                Debug.Log("AreAllPlayersReady: Player: " + player.PlayerName + ":" + player.OwnerId + " ready status: " + player.IsReady.ToString());
                allReady = false;
                break;
            }  
        }
        Debug.Log("AreAllPlayersReady: returning: " + allReady.ToString());
        return allReady;
    }
    [Server]
    void TellHostTheyCanStartGame()
    {
        Debug.Log("TellHostTheyCanStartGame");
        NetworkPlayer hostPlayer = GetHostPlayer();
        if (!hostPlayer)
            return;
        hostPlayer.RpcHostCanStartGame(hostPlayer.Owner);
    }
    [Server]
    void CannotStartGameYet()
    {
        Debug.Log("CannotStartGameYet");
        NetworkPlayer hostPlayer = GetHostPlayer();
        if (!hostPlayer)
            return;
        hostPlayer.RpcCannotStartGameYet(hostPlayer.Owner);
    }
    [Server]
    NetworkPlayer GetHostPlayer()
    {
        NetworkPlayer hostPlayer = null;
        foreach (NetworkPlayer player in GameplayManagerTopDownGolf.instance.NetworkPlayersServer)
        {
            if (player.Owner.IsHost)
            {
                Debug.Log("GetHostPlayer: Host player is: " + player.PlayerName + ":" + player.OwnerId);
                hostPlayer = player;
                break;
            }
        }
        return hostPlayer;
    }
    [TargetRpc]
    public void RpcHostCanStartGame(NetworkConnection conn)
    {
        if (!this.IsOwner)
            return;

        _uiHolder.SetActive(false);
        _startGameButton.gameObject.SetActive(true);

    }
    [TargetRpc]
    public void RpcCannotStartGameYet(NetworkConnection conn)
    {
        if (!this.IsOwner)
            return;
        _uiHolder.SetActive(true);
        _startGameButton.gameObject.SetActive(false);
    }

    public void HostStartGame()
    {
        if (!this.IsOwner)
            return;
        CmdHostStartGame();
    }
    [ServerRpc]
    void CmdHostStartGame()
    {
        if (!this.IsHost)
            return;
        SteamLobby.instance.SetGameStatusToInGame();
        SpawnGolfPlayerObjectsForEachPlayer();
        GameplayManagerTopDownGolf.instance.HostStartGame(base.Owner);
    }
    [Server]
    void SpawnGolfPlayerObjectsForEachPlayer()
    {
        foreach (NetworkPlayer player in GameplayManagerTopDownGolf.instance.NetworkPlayersServer)
        {
            player.SpawnGolfPlayer();
            player.HideLobbyUIStuff(player.Owner);
        }
    }
    [Server]
    public void SpawnGolfPlayer()
    {
        GameObject playerObject = Instantiate(_golfPlayerPrefab);
        _golfPlayerScript = playerObject.GetComponent<GolfPlayerTopDown>();
        string playerName = this.PlayerName;
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = RandomPlayerName();
            if (this.Owner.IsHost)
                playerName += " (Host)";
            //else
            //    playerName += " (Client)";
        }
        _golfPlayerScript.PlayerName = playerName;
        _golfPlayerScript.IsGameLeader = this.Owner.IsHost;
        _golfPlayerScript.OwnerNetId = this.OwnerId;
        _golfPlayerScript.BallColor = this._ballColor.Value;
        InstanceFinder.ServerManager.Spawn(_golfPlayerScript.gameObject, this.Owner);
    }
    [TargetRpc]
    public void HideLobbyUIStuff(NetworkConnection conn)
    {
        if (!this.IsOwner)
            return;
        GolfLobbySettings.instance.HasGameStarted = true;
        _playerUICanvas.SetActive(false);
        LobbyManagerGolf.instance.HideUIStuff();
        GameObject networkingTestHudObject = GameObject.FindGameObjectWithTag("NetworkingTestHud");
        if (networkingTestHudObject)
            networkingTestHudObject.SetActive(false);
    }
    public void SubmitName()
    {
        if (!this.IsOwner)
            return;
        string name;
        if (!string.IsNullOrEmpty(_playerNameInput.text))
        {
            name = _playerNameInput.text;
        }
        else
        {
            name = RandomPlayerName();
        }

        CmdSetPlayerName(name);
    }
    void SetPlayerName()
    {
        if (!this.IsOwner)
            return;
        // GetPersonaName throws when Steam isn't running (solo golf needs no Steam); CmdSetPlayerName already
        // falls back to a random name for an empty one. Dev instances on one machine share a Steam account, so the
        // process id tells them apart.
        if (GolfLobbySettings.instance != null && GolfLobbySettings.instance.IsDevSession)
            CmdSetPlayerName("Dev-" + (System.Diagnostics.Process.GetCurrentProcess().Id % 10000).ToString());
        else if (SteamManager.Initialized)
            CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());
        else
            CmdSetPlayerName(string.Empty);
    }
    [ServerRpc]
    void CmdSetPlayerName(string newName)
    {
        string name = newName;
        if (string.IsNullOrWhiteSpace(name))
            name = RandomPlayerName();
        if (this.Owner.IsHost)
            name += " (Host)";
        else
            name += " (Client)";

        this.PlayerName = name;
    }
    // Each player keeps the host's settings, so the lobby can carry on under a new host if this one leaves.
    [TargetRpc]
    public void RpcReceiveGolfSettings(NetworkConnection target, GolfMatchSettings settings)
    {
        GolfLobbySettings.instance.ReceivedSettings = settings;
        GolfLobbySettings.instance.HasReceivedSettings = true;
    }
    void SetPlayerSteamID()
    {
        if (!this.IsOwner)
            return;
        // GetSteamID throws when Steam isn't running; the id stays 0 and the avatar lookup skips itself.
        if (!SteamManager.Initialized)
            return;
        CmdSetSteamID(SteamUser.GetSteamID().m_SteamID);
    }
    [ServerRpc]
    void CmdSetSteamID(ulong id)
    {
        this.PlayerSteamID = id;
    }
    string RandomPlayerName()
    {
        int rand = UnityEngine.Random.Range(0, 100);
        string name = "Player[" + rand.ToString() + "]";
        return name;
    }
    void SyncPlayerName(string prev, string next, bool asServer)
    {
        if (asServer)
            return;
        _myLobbyPlayerListScript.UpdatePlayerName(next);
        // The name goes on the moving-lobby screen if this player takes the lobby over.
        if (LobbyHostMigration.instance != null)
            LobbyHostMigration.instance.RememberGolfLobby(null);
        if (!this.IsOwner)
            return;
        _welcomeText.text = "Welcome " + next + "!";
        _welcomeText.gameObject.SetActive(true);
        
    }
    public void UpdateBallColorValue(Color newColor)
    {
        Debug.Log("UpdateBallColorValue: Is owner? " + this.IsOwner.ToString());
        if (!this.IsOwner)
            return;
        Debug.Log("UpdateBallColorValue: " + newColor.ToString());
        CmdUpdateBallColorValue(newColor);
    }
    [ServerRpc]
    void CmdUpdateBallColorValue(Color newColor)
    {
        Debug.Log("CmdUpdateBallColorValue: " + newColor.ToString());
        this._ballColor.Value = newColor;
    }
    void SpawnLobbyPlayerListItem()
    {
        Debug.Log("SpawnLobbyPlayerListItem: ");
        _myLobbyPlayerListObject = Instantiate(_lobbyPlayerListItemPrefab);
        _myLobbyPlayerListScript = _myLobbyPlayerListObject.GetComponent<GolfPlayerListItem>();
        _myLobbyPlayerListScript.PlayerName = this.PlayerName;
        _myLobbyPlayerListScript.ConnectionId = this.ObjectId;
        _myLobbyPlayerListScript.UpdatePlayerItemReadyStatus(this.IsReady);

        LobbyManagerGolf.instance.AddLobbyPlayerListItem(_myLobbyPlayerListObject);

    }
    
    void SyncPlayerSteamID(ulong prev, ulong next, bool asServer)
    {
        if (asServer)
            return;

        if (_myLobbyPlayerListScript)
        {
            _myLobbyPlayerListScript.playerSteamId = next;
            _myLobbyPlayerListScript.GetPlayerAvatar();
        }
        else
        {
            StartCoroutine(DelayForSteamIDSync(next));
        }
    }
    IEnumerator DelayForSteamIDSync(ulong newID)
    {
        yield return new WaitForSeconds(0.5f);
        _myLobbyPlayerListScript.playerSteamId = newID;
        _myLobbyPlayerListScript.GetPlayerAvatar();
    }
    void SyncBallColor(Color prev, Color next, bool asServer)
    {
        if (asServer)
            return;

        _myLobbyPlayerListScript.UpdateColorIcon(next);
    }
    public void OnClick_Disconnect()
    {
        GolfLobbySettings.instance.IsLeavingOnPurpose = true;
        if (base.IsServer)
        {
            _networkManager.ServerManager.StopConnection(true);
            SteamLobby.instance.LeaveLobby();
        }


        if (base.IsClient)
        {
            _networkManager.ClientManager.StopConnection();
            SteamLobby.instance.LeaveLobby();
        }
            
    }
    public void PlayerClickedDisconnect()
    {
        OnClick_Disconnect();
    }
    public void PromptPlayerToDownloadCourse(string courseName)
    {
        if (!this.IsOwner)
            return;
        _promptPlayerHolder.SetActive(true);
        _promptPlayerText.text = "You do not have the '" + courseName + "' course. Do you want to download '" + courseName + "' from the steam workshop?";
        _uiHolder.SetActive(false);
    }
    public void OnClick_DownloadCourse()
    {
        CmdGetWorkshopIDFromServer();
    }
    [ServerRpc]
    void CmdGetWorkshopIDFromServer()
    {
        GameplayManagerTopDownGolf.instance.GetCourseWorkshopIDForPlayer(this.Owner);
    }
    public void ServerSentCourseWorkshopID(ulong workshopID)
    {
        Debug.Log("ServerSentCourseWorkshopID: " + workshopID);
        if (workshopID == 0)
        {
            // will need to have an error displayed here saying the course wasn't found in the workshop and you will disconnect in 5 seconds...
            OnClick_Disconnect();
        }

        CustomGolfCourseLoader customGolfCourseLoader = CustomGolfCourseLoader.GetInstance();
        customGolfCourseLoader.DownloadNewCourse(workshopID);
        //Debug.Log("ServerSentCourseWorkshopID: re-enabling UI");
        //_promptPlayerHolder.SetActive(false);
        //_uiHolder.SetActive(true);
    }
    public void CustomCourseAdded()
    {
        if (!this.IsOwner)
            return;

        Debug.Log("CustomCourseAdded: re-enabling UI");
        _promptPlayerHolder.SetActive(false);
        _uiHolder.SetActive(true);

        CmdTellServerCusomeCourseAdded();
    }
    [ServerRpc]
    void CmdTellServerCusomeCourseAdded()
    {
        Debug.Log("CmdTellServerCusomeCourseAdded: ");
        GameplayManagerTopDownGolf.instance.PlayerDownDownloadingCustomCourse(this.Owner);
    }
}
