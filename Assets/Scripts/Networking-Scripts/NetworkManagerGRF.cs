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
    // What a bot becomes at Start Game. The same prefab single player's AI uses: a GamePlayer with the AIPlayer brain
    // on it, which is what makes isAIPlayer true.
    [SerializeField] private GamePlayer aiGamePlayerPrefab;
    

    [Header("Waiting for players to load scene stuff")]
    [SerializeField] private GameObject WaitingForPlayersCanvas;
    public GameObject waitingForPlayersObject;
    // The loading handshake (simplified Nov 3, 2026; Chunk 2 audit item 7). Each player's client sends
    // CmdPlayerFinishedLoading AFTER its spawn requests (its football, its goblins), and ServerRpcs from one
    // connection run in order, so a player's report proves everything it asked for has been spawned on the server.
    // Once every seat has reported, every spawn exists, and the done-RPC sent after them reaches each client behind
    // those spawns on the reliable channel. The old version also counted the goblins and footballs each client
    // OBSERVED (6 per client, with AI special cases); those counts drifted and hung the waiting screen twice.
    public List<int> playersFinishedLoading = new List<int>();
    bool _loadingHandshakeComplete = false;

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
    // A client's copy of the host's settings, from LobbyPlayer.RpcReceiveMatchSettings. Kept apart from the fields above,
    // which only the host's code reads, so having the copy changes nothing until a lobby has to move to a new host.
    public FootballMatchSettings receivedMatchSettings;
    public bool hasReceivedMatchSettings = false;
    // Set when this player ends their own session (StopHost/StopClient), so LobbyHostMigration doesn't mistake it for
    // the host leaving.
    public bool isLeavingOnPurpose = false;
    private static readonly string[] GoblinTypes = { "Grenadier", "Berserker", "Skirmisher" };
    // Numbers handed out this frame. LobbyPlayers only gains a player once its spawn has been applied, a tick
    // later, so filling a team with bots in one go would otherwise give every one of them the same number.
    private List<int> reservedPlayerNumbers = new List<int>();
    // Bots spawned on the server that LobbyPlayers has not gained yet, for the same reason. A team counts them, or
    // two Add Bots before the first one lands both see a free seat and the team ends up four deep.
    private List<LobbyPlayer> _botsNotYetListed = new List<LobbyPlayer>();

    [Header("Pause/Resume Game")]
    public bool isGamePaused = false;
    public GamePlayer playerWhoPaused;
    public uint playerWhoPausedNetId;
    public float lastPauseTimeStamp;


    public List<GamePlayer> GamePlayers { get; } = new List<GamePlayer>();
    public List<LobbyPlayer> LobbyPlayers { get; } = new List<LobbyPlayer>();

    // Set by DevDirectConnect when a session was started over Tugboat instead of Steam, so the Steam lobby calls below are
    // skipped. Always false in a release build, where DevDirectConnect doesn't exist.
    public bool isDevSession = false;
    // Whether this session runs inside a Steam lobby. A dev session never has one, and neither does single player:
    // SinglePlayerGame starts the host directly, no lobby is ever created, so every call that reads lobby members,
    // lobby data or lobby status keys on this, not on isDevSession alone.
    public bool HasSteamLobby { get { return !isDevSession && !isSinglePlayer; } }

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
        _networkManager.SceneManager.OnLoadStart += SceneManager_OnLoadStart;
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
        _networkManager.SceneManager.OnLoadStart -= SceneManager_OnLoadStart;
        _networkManager.SceneManager.OnLoadEnd -= SceneManager_OnLoadEnd;
    }
    // Clears the leftovers of the previous session before a new one is set up. TitleScreenManager
    // did this inline as ResetGameServerSettings; it lives here so every caller gets both halves.
    public void ResetForNewSession()
    {
        ResetWaitingForPlayersToLoadStuff();
        ClearLobbyAndGamePlayerList();
    }
    // The mode fields below have to move together: a session that sets some of them and not the others gets the
    // prefab's defaults for the rest, which lights up Start Game at the wrong player count and then hangs the match
    // waiting for players who will never arrive. These three setters are the only supported way to pick a mode.
    public void ConfigureFootballLobby(bool is1v1Lobby)
    {
        is1v1 = is1v1Lobby;
        isSinglePlayer = false;
        maxConnections = is1v1Lobby ? 2 : 6;
        minPlayers = is1v1Lobby ? 2 : 6;
    }
    public void ConfigureSinglePlayerSession()
    {
        is1v1 = false;
        isSinglePlayer = true;
        maxConnections = 1;
        minPlayers = 1;
    }
    public void ConfigureTutorialSession()
    {
        is1v1 = false;
        isSinglePlayer = false;
        maxConnections = 1;
        minPlayers = 1;
    }
    // Mirror's StartHost loaded the online scene (LobbyScene) and then added the host's player there. Start the server now;
    // the host's own client starts once the lobby has loaded, in SceneManager_OnLoadEnd.
    public void StartHost()
    {
        _isFootballSession = true;
        isLeavingOnPurpose = false;
        _startHostClientWhenLobbyLoads = true;
        if (!StartServerConnection())
            EndFootballSession();
    }
    // Multipass's own StartConnection(true) starts every transport it holds and reports failure if any one of them fails.
    // That let FishySteamworks take the whole host down whenever Steam wasn't running, even though Tugboat had started,
    // which killed the Steam-less dev sessions. A session only ever uses one transport, the one the local client was
    // pointed at (SteamLobby sets FishySteamworks, DevDirectConnect sets Tugboat), so start just that one. A Steam
    // session no longer binds Tugboat's port either. ServerManager.StartConnection() is itself nothing more than
    // Transport.StartConnection(true), so going through Multipass directly skips no setup.
    // Public because SteamLobby's solo-golf host starts the server the same way.
    public bool StartServerConnection()
    {
        FishNet.Transporting.Multipass.Multipass multipass = _networkManager.TransportManager.Transport as FishNet.Transporting.Multipass.Multipass;
        if (multipass == null || multipass.ClientTransport == null)
            return _networkManager.ServerManager.StartConnection();
        return multipass.StartConnection(true, multipass.ClientTransport.Index);
    }
    // Single player has no remote peers, so it hosts on Tugboat bound to 127.0.0.1: FishySteamworks cannot start
    // without Steam running, and a loopback-only socket is unreachable from other machines, so a shipped build
    // raises no Windows firewall prompt. SteamLobby points the client back at FishySteamworks before every Steam
    // host and join, and DevDirectConnect clears the bind address again for its own LAN-reachable dev hosts.
    public bool UseLoopbackTransport()
    {
        FishNet.Transporting.Multipass.Multipass multipass = _networkManager.TransportManager.Transport as FishNet.Transporting.Multipass.Multipass;
        if (multipass == null)
            return false;
        FishNet.Transporting.Tugboat.Tugboat tugboat = multipass.GetTransport<FishNet.Transporting.Tugboat.Tugboat>();
        if (tugboat == null)
            return false;
        tugboat.SetServerBindAddress("127.0.0.1", FishNet.Transporting.IPAddressType.IPv4);
        tugboat.SetClientAddress("127.0.0.1");
        multipass.SetClientTransport<FishNet.Transporting.Tugboat.Tugboat>();
        return true;
    }
    public void StartClient()
    {
        _isFootballSession = true;
        isLeavingOnPurpose = false;
        if (!_networkManager.ClientManager.StartConnection(networkAddress))
            EndFootballSession();
    }
    // Everything that ends a session by choice goes through these two, so a lost host can be told apart from a player
    // leaving: only the first moves the lobby to a new host.
    public void StopHost()
    {
        isLeavingOnPurpose = true;
        _networkManager.ServerManager.StopConnection(true);
        _networkManager.ClientManager.StopConnection();
    }
    public void StopClient()
    {
        isLeavingOnPurpose = true;
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
            // Humans only: a lobby filled out with bots is not full. The bot gives way once the new player has a team,
            // in EnforceTeamCapacity. Counting everyone here turned away players from a lobby the host had filled.
            if (CountHumans() >= maxConnections) // prevents players joining if the game is full
            {
                Debug.Log("Too many players. Disconnecting user.");
                conn.Disconnect(true);
                return;
            }
            if (TitleScene.IsActive() || SceneManager.GetActiveScene().name == "LobbyScene") // prevents players from joining a game that has already started. When the game starts, the scene will no longer be a title screen
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
        if (args.ConnectionState != LocalConnectionState.Stopped)
            return;
        // The host left the lobby: it carries on under a new host instead of ending here.
        if (LobbyHostMigration.instance != null && LobbyHostMigration.instance.HandleClientStopped())
            return;
        EndFootballSession();
    }
    // Mirror loaded its offline scene, the title screen, when the client stopped.
    // Public for LobbyHostMigration, which ends the session itself if no new host turns up.
    public void EndFootballSession()
    {
        _isFootballSession = false;
        _startHostClientWhenLobbyLoads = false;
        isDevSession = false;
        isLeavingOnPurpose = false;
        hasReceivedMatchSettings = false;
        if (LobbyHostMigration.instance != null)
            LobbyHostMigration.instance.Forget();
        // SteamLobby leaves the Steam lobby when any other session's client stops, but leaves Football's to here: a
        // lobby moving to a new host keeps its Steam lobby, and only an ended session gives it up.
        if (SteamLobby.instance != null)
            SteamLobby.instance.LeaveLobby();
        reservedPlayerNumbers.Clear();
        _botsNotYetListed.Clear();
        // This object outlives the session, so clear what the last one left behind, on clients too.
        LobbyPlayers.Clear();
        GamePlayers.Clear();
        ResetWaitingForPlayersToLoadStuff();
        DestroyWaitingForPlayersCanvas();
        // A session the map maker started to test a hole goes back to the map maker, not the title screen
        string sceneToReturnTo = MapMakerDropInSession.ReturnScene;
        if (SceneManager.GetActiveScene().name != sceneToReturnTo)
            SceneManager.LoadScene(sceneToReturnTo);
    }
    // Shows the waiting-for-players screen while the gameplay scene loads, as Mirror's OnClientChangeScene did.
    // On the host FishNet may raise this for both its server and client side, so only show it once.
    private void SceneManager_OnLoadStart(FishNet.Managing.Scened.SceneLoadStartEventArgs args)
    {
        if (!_isFootballSession || !_networkManager.IsClientStarted || waitingForPlayersObject != null)
            return;
        foreach (FishNet.Managing.Scened.SceneLookupData sceneData in args.QueueData.SceneLoadData.SceneLookupDatas)
        {
            if (System.IO.Path.GetFileNameWithoutExtension(sceneData.FullName) == "Gameplay-768-432")
            {
                waitingForPlayersObject = Instantiate(WaitingForPlayersCanvas);
                DontDestroyOnLoad(waitingForPlayersObject);
                return;
            }
        }
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
    // The host adds a bot to a team from the lobby. Bots are LobbyPlayers owned by the host, with no connection of
    // their own and no goblin: they take whatever roles are left at Start Game, so they never compete with a player
    // for a goblin in the lobby.
    public LobbyPlayer AddBot(bool isTeamGrey)
    {
        if (!_networkManager.ServerManager.Started)
            return null;
        NetworkConnection hostConnection = HostConnection();
        if (hostConnection == null)
        {
            Debug.Log("AddBot: no host connection to own the bot.");
            return null;
        }
        if (CountOnTeam(isTeamGrey) >= 3)
        {
            Debug.Log("AddBot: the " + (isTeamGrey ? "grey" : "green") + " team is already full.");
            return null;
        }
        LobbyPlayer bot = Instantiate(lobbyPlayerPrefab);
        bot.isBot = true;
        bot.PlayerName = "BOT";
        bot.IsGameLeader = false;
        bot.ConnectionId = -1;
        bot.playerNumber = TakePlayerNumber();
        bot.isTeamGrey = isTeamGrey;
        bot.isPlayerReady = true;
        bot.is1v1 = this.is1v1;
        bot.isSinglePlayer = this.isSinglePlayer;
        _networkManager.ServerManager.Spawn(bot.gameObject, hostConnection);
        _botsNotYetListed.Add(bot);
        Debug.Log("AddBot: added a bot to the " + (isTeamGrey ? "grey" : "green") + " team.");
        return bot;
    }
    // Every player and bot needs a number of its own: the loading handshake keys its dictionaries on playerNumber,
    // so a duplicate stalls the start of a match behind the Waiting for players screen. LobbyPlayers.Count + 1 is
    // not enough on two counts - it repeats a number once a player in the middle has left, and it repeats one for
    // every bot added in the same frame, since the list only gains them a tick later.
    // No upper bound: a mid-match replacement bot needs a number past the lobby's seats, which are all reserved by
    // then. Stopping at maxConnections fell through to LobbyPlayers.Count + 1 - an empty list in the gameplay
    // scene - and gave every replacement the host's number 1.
    public int TakePlayerNumber()
    {
        for (int number = 1; ; number++)
        {
            if (reservedPlayerNumbers.Contains(number))
                continue;
            bool taken = false;
            foreach (LobbyPlayer player in LobbyPlayers)
            {
                if (player != null && player.playerNumber == number)
                {
                    taken = true;
                    break;
                }
            }
            if (!taken)
            {
                reservedPlayerNumbers.Add(number);
                return number;
            }
        }
    }
    public void RemoveBot(LobbyPlayer bot)
    {
        if (bot == null || !bot.isBot || !_networkManager.ServerManager.Started)
            return;
        Debug.Log("RemoveBot: removing a bot from the " + (bot.isTeamGrey ? "grey" : "green") + " team.");
        reservedPlayerNumbers.Remove(bot.playerNumber);
        LobbyPlayers.Remove(bot);
        _botsNotYetListed.Remove(bot);
        _networkManager.ServerManager.Despawn(bot.gameObject);
    }
    // A player always beats a bot to a slot. Called whenever team membership changes, so a joining or switching
    // player pushes a bot out rather than being turned away.
    public void EnforceTeamCapacity()
    {
        if (!_networkManager.ServerManager.Started || this.is1v1 || this.isSinglePlayer)
            return;
        for (int team = 0; team < 2; team++)
        {
            bool isTeamGrey = team == 1;
            while (CountOnTeam(isTeamGrey) > 3)
            {
                LobbyPlayer bot = FindBotOnTeam(isTeamGrey);
                if (bot == null)
                    break;
                RemoveBot(bot);
            }
        }
    }
    // Bots take the goblins the players did not pick, so a player is never competing with a bot for one in the lobby.
    // Done here, immediately before the GamePlayers are built, because that is the first moment every player's choice is
    // final. Any goblin still spare afterwards simply has no bot for it, which can only happen on a short-handed team.
    private void AssignGoblinsToBots(List<LobbyPlayer> lobbyPlayers)
    {
        for (int team = 0; team < 2; team++)
        {
            bool isTeamGrey = team == 1;
            foreach (string goblinType in GoblinTypes)
            {
                bool takenByAPlayer = false;
                foreach (LobbyPlayer lobbyPlayer in lobbyPlayers)
                {
                    if (lobbyPlayer != null && !lobbyPlayer.isBot && lobbyPlayer.isTeamGrey == isTeamGrey && lobbyPlayer.goblinType == goblinType)
                    {
                        takenByAPlayer = true;
                        break;
                    }
                }
                if (takenByAPlayer)
                    continue;
                foreach (LobbyPlayer lobbyPlayer in lobbyPlayers)
                {
                    if (lobbyPlayer != null && lobbyPlayer.isBot && lobbyPlayer.isTeamGrey == isTeamGrey && string.IsNullOrEmpty(lobbyPlayer.goblinType))
                    {
                        lobbyPlayer.goblinType = goblinType;
                        Debug.Log("AssignGoblinsToBots: a bot on the " + (isTeamGrey ? "grey" : "green") + " team will play the " + goblinType + ".");
                        break;
                    }
                }
            }
        }
    }
    public int CountHumans()
    {
        int humans = 0;
        foreach (LobbyPlayer player in LobbyPlayers)
        {
            if (player != null && !player.isBot)
                humans++;
        }
        return humans;
    }
    // The GamePlayers a person is actually playing. Bots have no client, so they never report loading progress.
    public int CountHumanGamePlayers()
    {
        int humans = 0;
        foreach (GamePlayer player in GamePlayers)
        {
            if (player != null && !player.isAIPlayer)
                humans++;
        }
        return humans;
    }
    public int CountOnTeam(bool isTeamGrey)
    {
        int count = 0;
        foreach (LobbyPlayer player in LobbyPlayers)
        {
            if (player != null && player.isTeamGrey == isTeamGrey)
                count++;
        }
        PruneListedBots();
        foreach (LobbyPlayer bot in _botsNotYetListed)
        {
            if (bot.isTeamGrey == isTeamGrey)
                count++;
        }
        return count;
    }
    private void PruneListedBots()
    {
        _botsNotYetListed.RemoveAll(bot => bot == null || LobbyPlayers.Contains(bot));
    }
    private LobbyPlayer FindBotOnTeam(bool isTeamGrey)
    {
        foreach (LobbyPlayer player in LobbyPlayers)
        {
            if (player != null && player.isBot && player.isTeamGrey == isTeamGrey)
                return player;
        }
        PruneListedBots();
        foreach (LobbyPlayer bot in _botsNotYetListed)
        {
            if (bot.isTeamGrey == isTeamGrey)
                return bot;
        }
        return null;
    }
    private NetworkConnection HostConnection()
    {
        foreach (LobbyPlayer player in LobbyPlayers)
        {
            if (player != null && player.IsGameLeader)
                return player.Owner;
        }
        return null;
    }
    public void SpawnLobbyPlayer(NetworkConnection conn)
    {
        Debug.Log("Checking if player is in correct scene. Player's scene name is: " + SceneManager.GetActiveScene().name.ToString() + ". Correct scene is a title screen or LobbyScene.");
        if (TitleScene.IsActive() || SceneManager.GetActiveScene().name == "LobbyScene")
        {
            // The host's own player leads, whenever it arrives. "First to connect" held only while nobody could join
            // faster than the host's own client, which a lobby moving to a new host breaks: everyone reconnects at once.
            bool isGameLeader = conn.IsHost;

            LobbyPlayer lobbyPlayerInstance = Instantiate(lobbyPlayerPrefab);

            lobbyPlayerInstance.IsGameLeader = isGameLeader;
            lobbyPlayerInstance.ConnectionId = conn.ClientId;
            lobbyPlayerInstance.playerNumber = TakePlayerNumber();
            // Only a session with a Steam lobby has Steam ids. Single player used to pass the isDevSession check and read
            // a junk id with Steam running, or throw with Steam closed, which aborted the spawn and left LobbyPlayers empty.
            if (HasSteamLobby)
                lobbyPlayerInstance.playerSteamId = SteamIdOf(conn);
            lobbyPlayerInstance.is1v1 = this.is1v1;
            lobbyPlayerInstance.isSinglePlayer = this.isSinglePlayer;

            _networkManager.ServerManager.Spawn(lobbyPlayerInstance.gameObject, conn);
            // Sent after the spawn, so it reaches the client after its LobbyPlayer exists. Every player keeps a copy, so
            // whoever takes over the lobby if the host leaves can start it with the same settings.
            lobbyPlayerInstance.RpcReceiveMatchSettings(conn, CurrentMatchSettings());
            Debug.Log("Player added. Player name: " + lobbyPlayerInstance.PlayerName + ". Player connection id: " + lobbyPlayerInstance.ConnectionId.ToString());
            EnforceTeamCapacity();
        }
    }
    // A player's Steam id, read from the connection rather than the lobby. FishySteamworks knows which Steam account each
    // remote connection is (its address is the Steam id), and the host's own connection is this machine's account.
    // Reading the lobby's member list by LobbyPlayers.Count assumed Steam lists members in the order they connected,
    // which breaks as soon as players reconnect in a different order, and put avatars on the wrong rows.
    private ulong SteamIdOf(NetworkConnection conn)
    {
        if (!SteamManager.Initialized)
            return 0;
        if (conn.IsHost)
            return SteamUser.GetSteamID().m_SteamID;
        ulong steamId;
        if (ulong.TryParse(conn.GetAddress(), out steamId))
            return steamId;
        Debug.LogWarning("SteamIdOf: no Steam id for connection " + conn.ClientId + ".");
        return 0;
    }
    // The settings a lobby was created with. Only the host has them, from the title screen; clients get a copy.
    public FootballMatchSettings CurrentMatchSettings()
    {
        FootballMatchSettings settings = new FootballMatchSettings();
        settings.is1v1 = this.is1v1;
        settings.lobbyName = this.lobbyName;
        settings.secondsPerHalf = this.secondsPerHalf;
        settings.powerUpsEnabled = this.powerUpsEnabled;
        settings.randomEventsEnabled = this.randomEventsEnabled;
        settings.spawnObstaclesEnabled = this.spawnObstaclesEnabled;
        settings.mercyRuleEnabled = this.mercyRuleEnabled;
        settings.mercyRulePointDifferential = this.mercyRulePointDifferential;
        return settings;
    }
    // A player taking over a lobby whose host left starts it with the old host's settings. The mode decides the player
    // counts, as it does in TitleScreenManager.CreateNewLobby.
    public void ApplyMatchSettings(FootballMatchSettings settings)
    {
        this.is1v1 = settings.is1v1;
        this.isSinglePlayer = false;
        this.maxConnections = settings.is1v1 ? 2 : 6;
        this.minPlayers = settings.is1v1 ? 2 : 6;
        this.lobbyName = settings.lobbyName;
        this.secondsPerHalf = settings.secondsPerHalf;
        this.powerUpsEnabled = settings.powerUpsEnabled;
        this.randomEventsEnabled = settings.randomEventsEnabled;
        this.spawnObstaclesEnabled = settings.spawnObstaclesEnabled;
        this.mercyRuleEnabled = settings.mercyRuleEnabled;
        this.mercyRulePointDifferential = settings.mercyRulePointDifferential;
    }
    public void StartGame()
    {
        if (CanStartGame() && SceneManager.GetActiveScene().name == "LobbyScene")
        {
            // Players stay in the Steam lobby until their session ends. This hides it from the lobby list.
            if (HasSteamLobby)
                SteamLobby.instance.SetGameStatusToInGame();

            // Mirror's ServerChangeScene override built a GamePlayer for each LobbyPlayer and swapped it in as that
            // connection's player. FishNet has no player slot, so each GamePlayer is spawned with the same owner and the
            // LobbyPlayer is removed (its OnDestroy still leaves the Steam lobby). GamePlayer is a global object, so it
            // survives the scene change.
            List<LobbyPlayer> lobbyPlayers = new List<LobbyPlayer>(LobbyPlayers);
            if (!this.is1v1 && !this.isSinglePlayer)
                AssignGoblinsToBots(lobbyPlayers);
            for (int i = lobbyPlayers.Count - 1; i >= 0; i--)
            {
                LobbyPlayer lobbyPlayer = lobbyPlayers[i];
                NetworkConnection conn = lobbyPlayer.Owner;
                // A bot becomes an AI GamePlayer: the same prefab single player's AI uses, which carries the AIPlayer
                // brain alongside the GamePlayer. Everything below is the same either way.
                GamePlayer prefabToSpawn = (lobbyPlayer.isBot && aiGamePlayerPrefab != null) ? aiGamePlayerPrefab : gamePlayerPrefab;
                GamePlayer gamePlayerInstance = Instantiate(prefabToSpawn);

                // Set before Spawn so the values go out with the spawn. The [Server] setters would skip here: the object
                // isn't spawned yet.
                gamePlayerInstance.PlayerName = lobbyPlayer.PlayerName;
                gamePlayerInstance.ConnectionId = lobbyPlayer.ConnectionId;
                gamePlayerInstance.playerNumber = lobbyPlayer.playerNumber;
                gamePlayerInstance.IsGameLeader = lobbyPlayer.IsGameLeader;
                gamePlayerInstance.playerSteamId = lobbyPlayer.playerSteamId;
                gamePlayerInstance.is1v1 = lobbyPlayer.is1v1;
                gamePlayerInstance.isSinglePlayer = lobbyPlayer.isSinglePlayer;
                if (!this.is1v1 && !this.isSinglePlayer)
                {
                    gamePlayerInstance.isTeamGrey = lobbyPlayer.isTeamGrey;
                    gamePlayerInstance.goblinType = lobbyPlayer.goblinType;
                }
                else if (this.isSinglePlayer)
                {
                    gamePlayerInstance.isTeamGrey = lobbyPlayer.isTeamGrey;
                }

                _networkManager.ServerManager.Spawn(gamePlayerInstance.gameObject, conn);
                _networkManager.ServerManager.Despawn(lobbyPlayer.gameObject);
                Debug.Log("Spawned new GamePlayer: " + gamePlayerInstance.PlayerName);
            }
            FishNet.Managing.Scened.SceneLoadData gameplayScene = new FishNet.Managing.Scened.SceneLoadData("Gameplay-768-432");
            gameplayScene.ReplaceScenes = FishNet.Managing.Scened.ReplaceOption.All;
            _networkManager.SceneManager.LoadGlobalScenes(gameplayScene);
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
        // A mid-match replacement bot reports too (InitializeAIPlayer); the handshake is long over by then.
        if (_loadingHandshakeComplete)
            return;
        if (!playersFinishedLoading.Contains(playerNumber))
        {
            playersFinishedLoading.Add(playerNumber);
        }
        // Until every seat's GamePlayer has spawned, a complete report list proves nothing: the first players load
        // while the rest are still arriving. Every LobbyPlayer gets a GamePlayer; single player has no lobby and is
        // always the host plus one AI.
        int expectedPlayers = this.isSinglePlayer ? 2 : LobbyPlayers.Count;
        if (GamePlayers.Count < expectedPlayers)
        {
            Debug.Log("ReportPlayerFinishedLoading: " + GamePlayers.Count + " of " + expectedPlayers + " players spawned so far.");
            return;
        }
        foreach (GamePlayer player in GamePlayers)
        {
            if (!playersFinishedLoading.Contains(player.playerNumber))
            {
                Debug.Log("ReportPlayerFinishedLoading: still waiting on player " + player.playerNumber + ".");
                return;
            }
        }
        Debug.Log("ReportPlayerFinishedLoading: every player has loaded; starting the match.");
        _loadingHandshakeComplete = true;
        playersFinishedLoading.Clear();
        foreach (GamePlayer player in GamePlayers)
            player.DoneLoadingGameplayStuff();
    }
    public void ResetWaitingForPlayersToLoadStuff()
    {
        playersFinishedLoading.Clear();
        _loadingHandshakeComplete = false;
    }
    public void ClearLobbyAndGamePlayerList()
    {
        LobbyPlayers.Clear();
        GamePlayers.Clear();
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
