using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Steamworks;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;

// Football's session manager. With Mirror this was the NetworkManager itself. Football now shares Golf's FishNet
// NetworkManager, so this sits on the same object and listens to its events instead. Its public members are unchanged,
// so the rest of Football still reaches it through NetworkManagerGRF.singleton.
public class NetworkManagerGRF : MonoBehaviour
{
    public static NetworkManagerGRF singleton;

    // These two came from Mirror's NetworkManager.
    public string networkAddress = "localhost";
    public int maxConnections = 100;

    [SerializeField] public int minPlayers = 2;
    [SerializeField] private LobbyPlayer lobbyPlayerPrefab;
    [SerializeField] private GamePlayer gamePlayerPrefab;
    

    [Header("Waiting for players to load scene stuff")]
    [SerializeField] private GameObject WaitingForPlayersCanvas;
    public GameObject waitingForPlayersObject;
    public List<int> playersFinishedLoading = new List<int>();
    public bool areAllPlayersLoaded = false;
    public Dictionary<int, int> numberOfGoblinsLoaded = new Dictionary<int, int>();
    public bool areAllGoblinsLoaded = false;
    public List<int> playersWithFootballSpawned = new List<int>();
    public bool areFootballsSpawned = false;

    [Header("Game Info")]
    public bool is1v1 = false;
    public bool isSinglePlayer = false;
    public bool isTutorial = false;
    public string lobbyName;
    public int secondsPerHalf;
    public bool powerUpsEnabled;
    public bool randomEventsEnabled;
    public bool spawnObstaclesEnabled;
    public bool mercyRuleEnabled;
    public int mercyRulePointDifferential;

    [Header("Pause/Resume Game")]
    public bool isGamePaused = false;
    public GamePlayer playerWhoPaused;
    public uint playerWhoPausedNetId;
    public float lastPauseTimeStamp;


    public List<GamePlayer> GamePlayers { get; } = new List<GamePlayer>();
    public List<LobbyPlayer> LobbyPlayers { get; } = new List<LobbyPlayer>();

    private NetworkManager _networkManager;
    // True from StartHost/StartClient until the connection stops, so Golf sessions on the shared NetworkManager are left alone.
    private bool _isFootballSession = false;
    private bool _startHostClientWhenLobbyLoads = false;
    public bool IsFootballSession => _isFootballSession;

    private void Awake()
    {
        // FishNet keeps the first NetworkManager and destroys copies that load later, such as when the title screen loads again.
        // This component is on that same object, so keep the first one too.
        if (singleton != null && singleton != this)
            return;
        singleton = this;
        _networkManager = GetComponent<NetworkManager>();
    }
    private void Start()
    {
        if (singleton != this)
            return;
        _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
        _networkManager.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
        _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
        _networkManager.SceneManager.OnLoadEnd += SceneManager_OnLoadEnd;
    }
    private void OnDestroy()
    {
        if (singleton != this)
            return;
        singleton = null;
        if (_networkManager == null)
            return;
        _networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
        _networkManager.ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
        _networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
        _networkManager.SceneManager.OnLoadEnd -= SceneManager_OnLoadEnd;
    }
    // Mirror's StartHost loaded the online scene (LobbyScene) and then added the host's player there. Start the server now;
    // the host's own client starts once the lobby has loaded, in SceneManager_OnLoadEnd.
    public void StartHost()
    {
        _isFootballSession = true;
        _startHostClientWhenLobbyLoads = true;
        if (!_networkManager.ServerManager.StartConnection())
            EndFootballSession();
    }
    public void StartClient()
    {
        _isFootballSession = true;
        if (!_networkManager.ClientManager.StartConnection(networkAddress))
            EndFootballSession();
    }
    public void StopHost()
    {
        _networkManager.ServerManager.StopConnection(true);
        _networkManager.ClientManager.StopConnection();
    }
    public void StopClient()
    {
        _networkManager.ClientManager.StopConnection();
    }
    private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs args)
    {
        if (!_isFootballSession)
            return;
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            // Mirror changed to the online scene here. Clients that join later load it automatically.
            FishNet.Managing.Scened.SceneLoadData lobbyScene = new FishNet.Managing.Scened.SceneLoadData("LobbyScene");
            lobbyScene.ReplaceScenes = FishNet.Managing.Scened.ReplaceOption.All;
            _networkManager.SceneManager.LoadGlobalScenes(lobbyScene);
        }
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            Debug.Log("Stopping the server");
            LobbyPlayers.Clear();
            GamePlayers.Clear();
            // Without a client there is no client stop to return to the title screen, so do it here.
            if (!_networkManager.IsClientStarted)
                EndFootballSession();
        }
    }
    private void ServerManager_OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (!_isFootballSession)
            return;
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            Debug.Log("Connecting to server...");
            if (LobbyPlayers.Count >= maxConnections) // prevents players joining if the game is full
            {
                Debug.Log("Too many players. Disconnecting user.");
                conn.Disconnect(true);
                return;
            }
            if (SceneManager.GetActiveScene().name == "TitleScreen" || SceneManager.GetActiveScene().name == "LobbyScene") // prevents players from joining a game that has already started. When the game starts, the scene will no longer be the "TitleScreen"
            {
                Debug.Log("Player loaded from scene: " + SceneManager.GetActiveScene().name);
            }
            else
            {
                Debug.Log("Player did not load from correct scene. Disconnecting user. Player loaded from scene: " + SceneManager.GetActiveScene().name);
                conn.Disconnect(true);
                return;
            }
            Debug.Log("Server Connected");
        }
        else if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            // FishNet despawns the player's objects as well, and their LobbyPlayer removes itself in OnStopClient.
            LobbyPlayers.RemoveAll(player => player == null || player.OwnerId == conn.ClientId);
        }
    }
    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (!_isFootballSession)
            return;
        if (args.ConnectionState == LocalConnectionState.Stopped)
            EndFootballSession();
    }
    // Mirror loaded its offline scene, the title screen, when the client stopped.
    private void EndFootballSession()
    {
        _isFootballSession = false;
        _startHostClientWhenLobbyLoads = false;
        if (SceneManager.GetActiveScene().name != "TitleScreen")
            SceneManager.LoadScene("TitleScreen");
    }
    private void SceneManager_OnLoadEnd(FishNet.Managing.Scened.SceneLoadEndEventArgs args)
    {
        if (!_startHostClientWhenLobbyLoads || !args.QueueData.AsServer)
            return;
        foreach (Scene loadedScene in args.LoadedScenes)
        {
            if (loadedScene.name == "LobbyScene")
            {
                _startHostClientWhenLobbyLoads = false;
                _networkManager.ClientManager.StartConnection();
                return;
            }
        }
    }
    // Called by GameModePlayerSpawner on the server once a connection has loaded the lobby. This was OnServerAddPlayer with Mirror.
    public void SpawnLobbyPlayer(NetworkConnection conn)
    {
        Debug.Log("Checking if player is in correct scene. Player's scene name is: " + SceneManager.GetActiveScene().name.ToString() + ". Correct scene name is: TitleScreen");
        if (SceneManager.GetActiveScene().name == "TitleScreen" || SceneManager.GetActiveScene().name == "LobbyScene")
        {
            bool isGameLeader = LobbyPlayers.Count == 0; // isLeader is true if the player count is 0, aka when you are the first player to be added to a server/room

            LobbyPlayer lobbyPlayerInstance = Instantiate(lobbyPlayerPrefab);

            lobbyPlayerInstance.IsGameLeader = isGameLeader;
            lobbyPlayerInstance.ConnectionId = conn.ClientId;
            lobbyPlayerInstance.playerNumber = LobbyPlayers.Count + 1;
            lobbyPlayerInstance.playerSteamId = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.instance.current_lobbyID, LobbyPlayers.Count);
            lobbyPlayerInstance.is1v1 = this.is1v1;
            lobbyPlayerInstance.isSinglePlayer = this.isSinglePlayer;

            _networkManager.ServerManager.Spawn(lobbyPlayerInstance.gameObject, conn);
            Debug.Log("Player added. Player name: " + lobbyPlayerInstance.PlayerName + ". Player connection id: " + lobbyPlayerInstance.ConnectionId.ToString());
        }
    }
    public void StartGame()
    {
        if (CanStartGame() && SceneManager.GetActiveScene().name == "LobbyScene")
        {
            // Loading the gameplay scene, and spawning a GamePlayer for each LobbyPlayer, moves to FishNet in the
            // core match step of the conversion. Until then the lobby stays open.
            Debug.Log("StartGame: everyone is ready, but gameplay isn't converted to FishNet yet. Staying in the lobby.");
        }
    }
    private bool CanStartGame()
    {
        Debug.Log("NetworkManager: CanStartGame");
        if (LobbyPlayers.Count < minPlayers)
        {
            Debug.Log("CanStartGame: Not enough players to start the game. total players: " + LobbyPlayers.Count.ToString() + " need this many players: " + minPlayers.ToString());
            return false;
        }
        foreach (LobbyPlayer player in LobbyPlayers)
        {
            if (!player.isPlayerReady)
                return false;
        }
        return true;
    }
    public void HostShutDownServer()
    {
        // Mirror's version destroyed the NetworkManager and started a new one. The FishNet NetworkManager is shared with Golf,
        // so only stop the connections. Stopping the client returns everyone to the title screen.
        StopHost();
    }
    [Server]
    public void ReportPlayerFinishedLoading(int playerNumber)
    {
        Debug.Log("ReportPlayerFinishedLoading for player " + playerNumber.ToString());
        if (!playersFinishedLoading.Contains(playerNumber))
        {
            playersFinishedLoading.Add(playerNumber);
        }

        // Check if all players have finished loading
        foreach (GamePlayer player in GamePlayers)
        {
            if (!playersFinishedLoading.Contains(player.playerNumber))
            {
                areAllPlayersLoaded = false;
                break;
            }               
            else
                areAllPlayersLoaded = true;

        }
        Debug.Log("ReportPlayerFinishedLoading: Are all players loaded?: " + areAllPlayersLoaded.ToString());
        CheckIfAllGameplayStuffIsLoaded();
    }
    [Server]
    void CheckIfAllGameplayStuffIsLoaded()
    {
        if (areAllPlayersLoaded && areAllGoblinsLoaded && areFootballsSpawned)
        {
            Debug.Log("CheckIfAllGameplayStuffIsLoaded: true");
            foreach (GamePlayer player in GamePlayers)
                player.DoneLoadingGameplayStuff();
            ResetWaitingForPlayersToLoadStuff();
        }
        else
        {
            Debug.Log("CheckIfAllGameplayStuffIsLoaded: false");
        }
    }
    public void ResetWaitingForPlayersToLoadStuff()
    {
        playersFinishedLoading.Clear();
        areAllGoblinsLoaded = false;
        areAllPlayersLoaded = false;
        numberOfGoblinsLoaded.Clear();
        playersWithFootballSpawned.Clear();
        areFootballsSpawned = false;
    }
    public void ClearLobbyAndGamePlayerList()
    {
        LobbyPlayers.Clear();
        GamePlayers.Clear();
    }
    [Server]
    public void ReportFootballSpawnedForPlayer(int playerNumber)
    {
        Debug.Log("ReportFootballSpawnedForPlayer for player " + playerNumber.ToString());
        if (!playersWithFootballSpawned.Contains(playerNumber))
        {
            playersWithFootballSpawned.Add(playerNumber);
        }

        // Check if all players have finished loading
        foreach (GamePlayer player in GamePlayers)
        {
            if (!playersWithFootballSpawned.Contains(player.playerNumber))
            {
                areFootballsSpawned = false;
                break;
            }
            else
                areFootballsSpawned = true;

        }
        Debug.Log("ReportFootballSpawnedForPlayer: Do all players have their football spawned?: " + areFootballsSpawned.ToString());
        CheckIfAllGameplayStuffIsLoaded();
    }
    [Server]
    public void ReportGoblinSpawnedForPlayer(int playerNumber)
    {
        if (numberOfGoblinsLoaded.ContainsKey(playerNumber))
        {
            int goblinNumber = numberOfGoblinsLoaded[playerNumber] + 1;
            numberOfGoblinsLoaded[playerNumber] = goblinNumber;
        }
        else
        {
            numberOfGoblinsLoaded.Add(playerNumber, 1);
            Debug.Log("ReportGoblinSpawnedForPlayer: Player number: " + playerNumber + " has spawned " + numberOfGoblinsLoaded[playerNumber] + " goblins");
        }

        // Check if each player has loaded all three goblins
        if (numberOfGoblinsLoaded.Count != GamePlayers.Count  && !this.isSinglePlayer)
            return;
        else
        {
            foreach (GamePlayer player in GamePlayers)
            {
                if (this.isSinglePlayer && player.playerNumber == 2)
                    continue;
                if (!numberOfGoblinsLoaded.ContainsKey(player.playerNumber))
                {
                    areAllGoblinsLoaded = false;
                    break;
                }
                else if (numberOfGoblinsLoaded[player.playerNumber] != 6)
                {
                    areAllGoblinsLoaded = false;
                    break;
                }
                else
                {
                    areAllGoblinsLoaded = true;
                }

            }
        }
        Debug.Log("ReportGoblinSpawnedForPlayer: Do all players have all goblins spawned?: " + areAllGoblinsLoaded.ToString());
        CheckIfAllGameplayStuffIsLoaded();
    }
    public void DestroyWaitingForPlayersCanvas()
    {
        try
        {
            Destroy(waitingForPlayersObject);
        }
        catch
        {
            Debug.Log("Could not destroy WaitingForPlayersCanvas");
        }
    }
    public void PauseGame()
    {
        Time.timeScale = 0f;
    }
    public void ResumeGame()
    {
        Time.timeScale = 1.0f;
    }

}
