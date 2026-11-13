#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.SceneManagement;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.Tugboat;

// Testing only. Steam allows one instance of the game per account, so a 3v3 can't be tested with the Steam transport:
// six players need six accounts on six machines. This starts the same Football session over Tugboat instead, so any
// number of instances on one machine can play together. It joins the normal LobbyScene, so team and goblin selection,
// the match settings and everything after them work exactly as they do over Steam.
//
// The whole file is compiled out of a release build, and nothing else references it. The one thing it leaves behind in
// shipped code is NetworkManagerGRF.isDevSession, which stays false.
//
// Use: launch two instances for a 1v1 or six for a 3v3, press a Host button in one and Join in the others, then play
// as normal. The overlay only appears on the title screen.
//
// Golf works the same way: Host Golf in one instance and Join Golf in up to nine others. A dev Golf lobby plays the
// first built-in course's front three holes with the default rules; a Steam lobby's settings come from the title
// screen's Golf panel instead.
//
// Scripted test runs: these command-line options let a script start instances that act on their own, for cases that
// happen too quickly to do by hand.
//   -devhost 1v1|3v3|golf     host as soon as the title screen is up
//   -devjoin [address]        join a dev Football host (127.0.0.1 if no address is given)
//   -devjoingolf [address]    join a dev Golf host (127.0.0.1 if no address is given)
//   -devleavemove [seconds]   press Leave on the moving-lobby screen this long after it appears (at once if no time)
//   -devready                 ready up once this player is in the lobby
//   -devgoblin [type]         pick this goblin (Grenadier/Berserker/Skirmisher; Skirmisher if none is given) in the
//                             lobby just before -devready readies. In a scripted 3v3, humans picking non-grenadiers
//                             leaves both grenadiers to bots, whose AI then runs the coin toss and every kickoff, so
//                             the match plays itself into real gameplay with no human input.
//   -devbots                  as the 3v3 host, fill every empty seat with a bot once a second player has arrived
//   -devtornado               as a Golf host, spawn a tornado once the hole is under way, so the spawn can be
//                             watched from a client (it is otherwise down to chance or a broken statue)
//   -devswing                 in a Golf game, take a plain default swing whenever it is this player's turn: start
//                             the turn, aim at the hole's aim point, and submit a mid-power hit, so a scripted
//                             Golf pair plays itself through the tee-off challenge and the holes
//   -devstart                 press Start Game once everyone in the lobby is ready (the host; Golf waits for two)
//   -devquitonload [seconds]  quit the game this long after the Football match scene loads (at once if no time)
//   -devjitter [ms]           once a session runs, oscillate FishNet's latency simulator between 10 ms and this
//                             (180 if no amount is given), 8 s each, for the session's first 60 s, then stay calm,
//                             to fake a spiky connection that recovers. On a host it delays everything sent to
//                             clients, so their ping jumps; AdaptiveRemoteSmoothing reacts there. The phases are
//                             8 s because FishNet's RoundTripTime is a moving average: a faster wave averages flat.
// An instance started with any of them runs in the background, since a script's instances are never all focused, and
// hosts on loopback only, so a build in a new folder doesn't raise a firewall prompt.
public class DevDirectConnect : MonoBehaviour
{
    const string AddressKey = "DevDirectConnect.Address";

    string _address = "127.0.0.1";
    string _status = "";
    NetworkManager _networkManager;

    string _commandLineHostMode = null;
    bool _commandLineJoin = false;
    bool _commandLineJoinGolf = false;
    // The Golf player -devready last readied: a moved lobby gives this player a new one.
    NetworkPlayer _readiedGolfPlayer;
    bool _isCommandLineRun = false;
    float _leaveMoveDelay = -1f;
    float _leaveMoveTime = -1f;
    bool _readyUp = false;
    float _readyUpTime = -1f;
    string _goblinType = null;
    bool _fillWithBots = false;
    int _lobbyCountAtLastBotAdd = -1;
    bool _devSwing = false;
    float _nextSwingTime = -1f;
    bool _devTornado = false;
    bool _startGame = false;
    float _quitOnLoadDelay = -1f;
    float _quitOnLoadTime = -1f;
    float _jitterAmplitudeMs = -1f;
    float _jitterSessionStart = -1f;
    // Lets the lobby settle after this player's row appears, as a person would.
    const float LobbySettleSeconds = 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        GameObject host = new GameObject("DevDirectConnect");
        host.AddComponent<DevDirectConnect>();
        DontDestroyOnLoad(host);
    }

    void Awake()
    {
        _address = PlayerPrefs.GetString(AddressKey, "127.0.0.1");
        ReadCommandLine();
    }

    void ReadCommandLine()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 1; i < args.Length; i++)
        {
            string value = i + 1 < args.Length && !args[i + 1].StartsWith("-") ? args[i + 1] : null;
            if (args[i] == "-devhost")
                _commandLineHostMode = value ?? "1v1";
            else if (args[i] == "-devjoin" || args[i] == "-devjoingolf")
            {
                _commandLineJoin = true;
                _commandLineJoinGolf = args[i] == "-devjoingolf";
                _address = value ?? "127.0.0.1";
            }
            else if (args[i] == "-devleavemove")
            {
                float seconds;
                _leaveMoveDelay = value != null && float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out seconds) ? seconds : 0f;
            }
            else if (args[i] == "-devready")
                _readyUp = true;
            else if (args[i] == "-devgoblin")
                _goblinType = value ?? "Skirmisher";
            else if (args[i] == "-devbots")
                _fillWithBots = true;
            else if (args[i] == "-devswing")
                _devSwing = true;
            else if (args[i] == "-devtornado")
                _devTornado = true;
            else if (args[i] == "-devstart")
                _startGame = true;
            else if (args[i] == "-devquitonload")
            {
                float seconds;
                _quitOnLoadDelay = value != null && float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out seconds) ? seconds : 0f;
            }
            else if (args[i] == "-devjitter")
            {
                float ms;
                _jitterAmplitudeMs = value != null && float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out ms) ? ms : 180f;
            }
        }
        _isCommandLineRun = _commandLineHostMode != null || _commandLineJoin || _leaveMoveDelay >= 0f || _readyUp || _startGame || _quitOnLoadDelay >= 0f || _jitterAmplitudeMs > 0f || _goblinType != null || _fillWithBots || _devSwing || _devTornado;
        if (_isCommandLineRun)
            Application.runInBackground = true;
    }

    void Update()
    {
        if ((_commandLineHostMode != null || _commandLineJoin) && IsReadyForCommandLineStart())
        {
            if (_commandLineHostMode == "golf")
                StartDevGolfHost();
            else if (_commandLineHostMode != null)
                StartDevHost(_commandLineHostMode != "3v3");
            else if (_commandLineJoinGolf)
                StartDevGolfClient();
            else
                StartDevClient();
            _commandLineHostMode = null;
            _commandLineJoin = false;
        }

        if (_leaveMoveDelay >= 0f && LobbyHostMigration.instance != null)
        {
            if (!LobbyHostMigration.instance.IsMigrating)
                _leaveMoveTime = -1f;
            else if (_leaveMoveTime < 0f)
                _leaveMoveTime = Time.realtimeSinceStartup + _leaveMoveDelay;
            else if (Time.realtimeSinceStartup >= _leaveMoveTime)
                PressMovingLobbyLeave();
        }

        if (SceneManager.GetActiveScene().name == SteamLobby.GolfSceneName)
        {
            UpdateGolfLobby();
            UpdateGolfSwing();
            UpdateGolfTornado();
            return;
        }

        if (_readyUp && LobbyManager.instance != null && LobbyManager.instance.localLobbyPlayerScript != null)
        {
            if (_readyUpTime < 0f)
                _readyUpTime = Time.realtimeSinceStartup + LobbySettleSeconds;
            else if (Time.realtimeSinceStartup >= _readyUpTime)
            {
                _readyUp = false;
                if (_goblinType != null)
                {
                    Debug.Log("DevDirectConnect: picking goblin " + _goblinType + ".");
                    LobbyManager.instance.localLobbyPlayerScript.SelectGoblin(_goblinType);
                    _goblinType = null;
                }
                if (!LobbyManager.instance.localLobbyPlayerScript.isPlayerReady)
                {
                    Debug.Log("DevDirectConnect: pressing Ready Up.");
                    LobbyManager.instance.PlayerReadyUp();
                }
            }
        }

        // The host fills the empty 3v3 seats once the second instance has its row. A spawned bot only shows up in
        // LobbyPlayers a tick later, so each add waits for the previous bot's row before the next; the lobby's own
        // EnforceTeamCapacity hands a bot's seat back if another human arrives later.
        // A full lobby keeps watching rather than stopping: the joining player's team can be settled after the last bot
        // lands, and the bot that gives way then leaves a seat that a 3v3 (which needs all six) cannot start without.
        // Filling stops when this instance presses Start Game, whose despawns would otherwise read as empty seats.
        if (_fillWithBots && LobbyManager.instance != null && NetworkManagerGRF.singleton != null
            && InstanceFinder.ServerManager != null && InstanceFinder.ServerManager.Started
            && NetworkManagerGRF.singleton.LobbyPlayers.Count >= 2)
        {
            int lobbyCount = NetworkManagerGRF.singleton.LobbyPlayers.Count;
            if (lobbyCount >= NetworkManagerGRF.singleton.maxConnections)
            {
                _lobbyCountAtLastBotAdd = -1;
            }
            else if (lobbyCount != _lobbyCountAtLastBotAdd)
            {
                _lobbyCountAtLastBotAdd = lobbyCount;
                int grey = 0;
                foreach (LobbyPlayer player in NetworkManagerGRF.singleton.LobbyPlayers)
                {
                    if (player != null && player.isTeamGrey)
                        grey++;
                }
                bool addToGrey = grey < NetworkManagerGRF.singleton.maxConnections / 2;
                Debug.Log("DevDirectConnect: adding a bot to the " + (addToGrey ? "grey" : "green") + " team.");
                LobbyManager.instance.AddBotPressed(addToGrey);
            }
        }

        if (_startGame && LobbyManager.instance != null && LobbyManager.instance.localLobbyPlayerScript != null && IsLobbyReady())
        {
            _startGame = false;
            _fillWithBots = false;
            Debug.Log("DevDirectConnect: pressing Start Game.");
            LobbyManager.instance.StartGame();
        }

        if (_jitterAmplitudeMs > 0f && InstanceFinder.TransportManager != null
            && (InstanceFinder.ServerManager.Started || InstanceFinder.ClientManager.Started))
        {
            FishNet.Managing.Transporting.LatencySimulator simulator = InstanceFinder.TransportManager.LatencySimulator;
            simulator.SetEnabled(true);
            if (_jitterSessionStart < 0f)
                _jitterSessionStart = Time.realtimeSinceStartup;
            // 8 seconds calm, 8 seconds spiky, in alternating stretches, then calm after a minute so one run shows
            // the smoothing buffer rise and come all the way back down. A spiky stretch re-rolls the delay every
            // frame: a link's jitter is each packet arriving a different amount late, and a fixed added delay would
            // only move every packet equally, which no buffer needs to absorb.
            float elapsed = Time.realtimeSinceStartup - _jitterSessionStart;
            bool spiky = elapsed < 60f && Mathf.FloorToInt(elapsed / 8f) % 2 == 1;
            long target = spiky ? (long)Random.Range(10f, _jitterAmplitudeMs) : 10L;
            if (simulator.GetLatency() != target)
                simulator.SetLatency(target);
        }

        if (_quitOnLoadDelay >= 0f && SceneManager.GetActiveScene().name.StartsWith("Gameplay"))
        {
            if (_quitOnLoadTime < 0f)
                _quitOnLoadTime = Time.realtimeSinceStartup + _quitOnLoadDelay;
            else if (Time.realtimeSinceStartup >= _quitOnLoadTime)
            {
                _quitOnLoadDelay = -1f;
                Debug.Log("DevDirectConnect: quitting while the match loads.");
                Application.Quit();
            }
        }
    }

    // The scripted ready-up and Start Game for a Golf lobby, whose lobby is the game scene itself. Ready-up happens again
    // in a lobby that moved to a new host, where every player starts unready, so a moved lobby can be started too.
    void UpdateGolfLobby()
    {
        if (!_readyUp && !_startGame)
            return;
        GameplayManagerTopDownGolf manager = GameplayManagerTopDownGolf.instance;
        if (manager == null)
            return;
        NetworkPlayer me = null;
        bool allReady = manager.NetworkPlayersServer.Count >= 2;
        foreach (NetworkPlayer player in manager.NetworkPlayersServer)
        {
            if (player == null)
                continue;
            if (player.IsOwner)
                me = player;
            if (!player.IsReady)
                allReady = false;
        }
        if (me == null)
            return;

        if (_readyUp && me != _readiedGolfPlayer)
        {
            if (_readyUpTime < 0f)
                _readyUpTime = Time.realtimeSinceStartup + LobbySettleSeconds;
            else if (Time.realtimeSinceStartup >= _readyUpTime)
            {
                _readyUpTime = -1f;
                _readiedGolfPlayer = me;
                if (!me.IsReady)
                {
                    Debug.Log("DevDirectConnect: pressing Ready Up.");
                    me.PlayerReadyUp();
                }
            }
        }

        if (_startGame && allReady && me.IsServerStarted)
        {
            _startGame = false;
            Debug.Log("DevDirectConnect: pressing Start Game.");
            me.HostStartGame();
        }
    }

    // Spawns one tornado on a Golf host, through WindManager's own spawn, so its arrival can be watched from a
    // client. The hole has to be under way or there is no player for it to pick a spawn spot beside.
    void UpdateGolfTornado()
    {
        if (!_devTornado || WindManager.instance == null || GameplayManagerTopDownGolf.instance == null)
            return;
        if (!WindManager.instance.IsServerStarted || GameplayManagerTopDownGolf.instance.CurrentPlayer == null)
            return;
        if (WindManager.instance.IsThereATorndao)
            return;
        // The spawn spot is picked beside a player's ball, so every player needs one before asking for a tornado.
        if (GameplayManagerTopDownGolf.instance.GolfPlayers.Count == 0)
            return;
        foreach (GolfPlayerTopDown player in GameplayManagerTopDownGolf.instance.GolfPlayers)
        {
            if (player == null || player.MyBall == null)
                return;
        }
        _devTornado = false;
        Debug.Log("DevDirectConnect: spawning a tornado.");
        typeof(WindManager).GetMethod("SpawnTornado", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(WindManager.instance, new object[0]);
    }

    // The scripted Golf swing: whenever it is this player's turn, start the turn, then submit a plain mid-power hit
    // at the hole's aim point through SubmitHitToBall - the real submission path, so tee-off bookkeeping, score and
    // the turn rotation all run as they would for a person. The swing fields it fills are public on
    // GolfPlayerTopDown; only the turn-start prompt's handler is private, reached by reflection.
    void UpdateGolfSwing()
    {
        if (!_devSwing || Time.realtimeSinceStartup < _nextSwingTime)
            return;
        GameplayManagerTopDownGolf manager = GameplayManagerTopDownGolf.instance;
        if (manager == null || manager.CurrentPlayer == null)
            return;
        GolfPlayerTopDown player = manager.CurrentPlayer;
        if (!player.IsOwner || player.MyBall == null)
            return;
        if (player.MyBall.isHit || player.MyBall.isBouncing || player.MyBall.isRolling)
            return;
        if (!player.IsPlayersTurn)
        {
            Debug.Log("DevDirectConnect: starting " + player.PlayerName + "'s turn for a scripted swing.");
            var startTurn = typeof(GolfPlayerTopDown).GetMethod("PlayerStartTurn",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (startTurn != null)
                startTurn.Invoke(player, new object[] { default(UnityEngine.InputSystem.InputAction.CallbackContext) });
            _nextSwingTime = Time.realtimeSinceStartup + 2f;
            return;
        }
        Vector3 aimPoint = manager.TeeOffAimPoint;
        Vector2 direction = ((Vector2)(aimPoint - player.MyBall.transform.position)).normalized;
        if (direction == Vector2.zero)
            direction = Vector2.right;
        player.HitPowerSubmitted = 40f;
        player.ModifiedHitDirection = direction;
        player.hitTopSpin = 0f;
        player.hitLeftOrRightspin = 0f;
        Debug.Log("DevDirectConnect: scripted swing for " + player.PlayerName + " toward " + aimPoint + ".");
        player.SubmitHitToBall();
        _nextSwingTime = Time.realtimeSinceStartup + 5f;
    }

    // The same test the lobby uses to offer Start Game.
    bool IsLobbyReady()
    {
        NetworkManagerGRF game = NetworkManagerGRF.singleton;
        if (game == null || game.LobbyPlayers.Count < game.minPlayers)
            return false;
        foreach (LobbyPlayer player in game.LobbyPlayers)
        {
            if (player == null || !player.isPlayerReady)
                return false;
        }
        return true;
    }

    // Clicks the screen's own button, so the prefab's On Click wiring is part of the test.
    void PressMovingLobbyLeave()
    {
        _leaveMoveDelay = -1f;
        MovingLobbyScreen screen = FindAnyObjectByType<MovingLobbyScreen>();
        UnityEngine.UI.Button leave = screen != null ? screen.GetComponentInChildren<UnityEngine.UI.Button>() : null;
        if (leave == null)
        {
            Debug.Log("DevDirectConnect: no Leave button on the moving-lobby screen.");
            return;
        }
        Debug.Log("DevDirectConnect: pressing Leave on the moving-lobby screen.");
        leave.onClick.Invoke();
    }

    void OnGUI()
    {
        // Out of the way during a match. The title screen is the only place a session can be started from anyway.
        // The ping label is the exception: the cellular internet tests needed a live number (Nov 3, 2026).
        if (!TitleScene.IsActive())
        {
            DrawPing();
            return;
        }

        GUILayout.BeginArea(new Rect(10f, 10f, 230f, 240f), GUI.skin.box);
        GUILayout.Label("Local testing (no Steam)");

        if (GUILayout.Button("Host 1v1"))
            StartDevHost(true);


        if (GUILayout.Button("Host 3v3"))
            StartDevHost(false);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Host:", GUILayout.Width(38f));
        _address = GUILayout.TextField(_address);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Join over Tugboat"))
            StartDevClient();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Host Golf"))
            StartDevGolfHost();
        if (GUILayout.Button("Join Golf"))
            StartDevGolfClient();
        GUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(_status))
            GUILayout.Label(_status);
        GUILayout.EndArea();
    }

    // Either title screen will do. A Golf host also needs the course loader, which loads the list when asked.
    bool IsReadyForCommandLineStart()
    {
        if (!TitleScene.IsActive() || NetworkManagerGRF.singleton == null)
            return false;
        return _commandLineHostMode != "golf" || CustomGolfCourseLoader.GetInstance() != null;
    }

    void StartDevHost(bool is1v1)
    {
        if (!Ready())
            return;
        NetworkManagerGRF.singleton.isDevSession = true;

        // Over Steam the mode and the match settings are set by the title screen just before it starts the host, and
        // nowhere else. Skipping that leaves the prefab's values: 3v3 with minPlayers 2, so the lobby
        // offers Start Game at two players and then hangs waiting for six, and halves of zero seconds.
        NetworkManagerGRF.singleton.ConfigureFootballLobby(is1v1);

        // The real settings panel, so a dev session uses whatever is typed there and the same fallbacks as a Steam lobby
        // (60 second halves, mercy rule at 21) when it has never been opened.
        if (TitleScreenManager.instance != null)
            TitleScreenManager.instance.SetGameSettings(false);
        else
            ApplyDefaultMatchSettings();

        NetworkManagerGRF.singleton.StartHost();
        _status = NetworkManagerGRF.singleton.IsFootballSession ? "Hosting " + (is1v1 ? "1v1." : "3v3.") : "Could not start the server.";
        Debug.Log("DevDirectConnect: " + _status);
    }

    // On a title screen that has no TitleScreenManager there is no settings panel to read, and the prefab's own values
    // are a half of zero seconds. These are the same fallbacks the settings panels use for a blank box.
    void ApplyDefaultMatchSettings()
    {
        NetworkManagerGRF game = NetworkManagerGRF.singleton;
        game.secondsPerHalf = 60;
        game.mercyRuleEnabled = true;
        game.mercyRulePointDifferential = 21;
        game.powerUpsEnabled = true;
        game.randomEventsEnabled = true;
        game.spawnObstaclesEnabled = true;
    }

    // A pure client's round trip to the host, in milliseconds, top-right during a session. FishNet measures it on
    // every transport, so Steam internet tests get a live number too. The value includes tick-rate latency. The
    // host is skipped: its round trip to itself says nothing.
    void DrawPing()
    {
        if (FishNet.InstanceFinder.ServerManager == null || FishNet.InstanceFinder.ServerManager.Started)
            return;
        if (FishNet.InstanceFinder.ClientManager == null || !FishNet.InstanceFinder.ClientManager.Started)
            return;
        // "buf" appears while AdaptiveRemoteSmoothing holds the drawing buffer above the prefab value.
        string buffer = AdaptiveRemoteSmoothing.CurrentInterpolation != AdaptiveRemoteSmoothing.BaseInterpolation
            ? "  buf " + AdaptiveRemoteSmoothing.CurrentInterpolation : "";
        GUI.Label(new Rect(Screen.width - 160f, 5f, 155f, 22f), "ping " + FishNet.InstanceFinder.TimeManager.RoundTripTime + " ms" + buffer);
    }
    void StartDevClient()
    {
        if (!Ready())
            return;
        PlayerPrefs.SetString(AddressKey, _address);
        NetworkManagerGRF.singleton.isDevSession = true;
        NetworkManagerGRF.singleton.networkAddress = _address;
        NetworkManagerGRF.singleton.StartClient();
        _status = NetworkManagerGRF.singleton.IsFootballSession ? "Connecting to " + _address + "." : "Could not start the client.";
        Debug.Log("DevDirectConnect: " + _status);
    }

    // Golf's session is SteamLobby's, not NetworkManagerGRF's, so it never sets isDevSession: GolfLobbySettings has its own.
    void StartDevGolfHost()
    {
        if (!Ready())
            return;
        // The title screen's Golf panel loads the course list when it opens, so a dev host that never opened it asks too.
        CustomGolfCourseLoader courses = CustomGolfCourseLoader.GetInstance();
        if (courses != null)
            courses.GetAllAvailableCourses();
        if (courses == null || courses.AllAvailableCourses == null || courses.AllAvailableCourses.Courses.Count == 0)
        {
            _status = "No golf courses loaded.";
            return;
        }
        // Over Steam these come from the title screen's Golf panel, through GolfLobbySettings.CreateLobby.
        ScriptableCourse course = courses.AllAvailableCourses.Courses[0];
        GolfMatchSettings settings = new GolfMatchSettings();
        settings.numberOfPlayers = GolfLobbySettings.instance.MaxNumberOfPlayers;
        settings.powerUpsEnabled = true;
        settings.weatherStatuesEnabled = true;
        settings.rainMode = RainManager.RainMode.ControlledByPlayerFavor;
        settings.windMode = WindManager.WindMode.ControlledByPlayerFavor;
        settings.courseId = course.id;
        settings.courseName = course.CourseName;
        settings.courseWorkshopId = course.WorkshopID;
        settings.courseHoleSelection = "Front 3 (holes 1-3)";
        settings.customHolesToPlay = new System.Collections.Generic.List<int>();
        GolfLobbySettings.instance.ApplySettings(settings);

        _status = SteamLobby.instance.StartGolfHost(true) ? "Hosting Golf (" + course.CourseName + ")." : "Could not start the server.";
        Debug.Log("DevDirectConnect: " + _status);
    }

    void StartDevGolfClient()
    {
        if (!Ready())
            return;
        PlayerPrefs.SetString(AddressKey, _address);
        _status = SteamLobby.instance.StartGolfClient(_address, true) ? "Connecting to Golf at " + _address + "." : "Could not start the client.";
        Debug.Log("DevDirectConnect: " + _status);
    }

    // Points Multipass at Tugboat. The server opens every transport it holds, so this only decides what the local client
    // connects with; SteamLobby.UseSteamTransport puts it back for a Steam session.
    bool Ready()
    {
        if (NetworkManagerGRF.singleton == null)
        {
            _status = "No NetworkManagerGRF yet.";
            return false;
        }
        if (_networkManager == null)
            _networkManager = InstanceFinder.NetworkManager;
        if (_networkManager == null)
        {
            _status = "No NetworkManager yet.";
            return false;
        }
        if (_networkManager.ServerManager.Started || _networkManager.ClientManager.Started)
        {
            _status = "Already in a session.";
            return false;
        }
        Multipass multipass = _networkManager.TransportManager.Transport as Multipass;
        if (multipass == null)
        {
            _status = "Transport isn't Multipass.";
            return false;
        }
        multipass.SetClientTransport<Tugboat>();
        // Single player binds Tugboat to loopback; clear that so a dev host is reachable from other machines.
        Tugboat tugboat = multipass.GetTransport<Tugboat>();
        if (tugboat != null)
        {
            tugboat.SetServerBindAddress(_isCommandLineRun ? "127.0.0.1" : string.Empty, FishNet.Transporting.IPAddressType.IPv4);
            tugboat.SetClientAddress("localhost");
        }
        _status = "";
        return true;
    }
}
#endif
