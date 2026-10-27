using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;   

public class GameplayManager : NetworkBehaviour
{
    public static GameplayManager instance;

    [Header("Game Info")]
    private readonly SyncVar<bool> _syncIs1v1 = new SyncVar<bool>();
    public bool is1v1 { get => _syncIs1v1.Value; set => _syncIs1v1.Value = value; }
    private readonly SyncVar<bool> _syncIsSinglePlayer = new SyncVar<bool>();
    public bool isSinglePlayer { get => _syncIsSinglePlayer.Value; set => _syncIsSinglePlayer.Value = value; }
    private readonly SyncVar<int> _syncSecondsPerHalf = new SyncVar<int>();
    public int secondsPerHalf { get => _syncSecondsPerHalf.Value; set => _syncSecondsPerHalf.Value = value; }
    private readonly SyncVar<bool> _syncPowerUpsEnabled = new SyncVar<bool>();
    public bool powerUpsEnabled { get => _syncPowerUpsEnabled.Value; set => _syncPowerUpsEnabled.Value = value; }
    private readonly SyncVar<bool> _syncRandomEventsEnabled = new SyncVar<bool>();
    public bool randomEventsEnabled { get => _syncRandomEventsEnabled.Value; set => _syncRandomEventsEnabled.Value = value; }
    private readonly SyncVar<bool> _syncSpawnObstaclesEnabled = new SyncVar<bool>();
    public bool spawnObstaclesEnabled { get => _syncSpawnObstaclesEnabled.Value; set => _syncSpawnObstaclesEnabled.Value = value; }
    private readonly SyncVar<bool> _syncMercyRuleEnabled = new SyncVar<bool>();
    public bool mercyRuleEnabled { get => _syncMercyRuleEnabled.Value; set => _syncMercyRuleEnabled.Value = value; }
    private readonly SyncVar<int> _syncMercyRulePointDifferential = new SyncVar<int>();
    public int mercyRulePointDifferential { get => _syncMercyRulePointDifferential.Value; set => _syncMercyRulePointDifferential.Value = value; }

    [Header("Local GamePlayers")]
    [SerializeField] private GameObject LocalGamePlayer;
    [SerializeField] private GamePlayer LocalGamePlayerScript;

    [Header("UI Canvases")]
    [SerializeField] private GameObject gameplayCanvas;
    [SerializeField] private GameObject coinTossCanvas;

    [Header("Gameplay Statuses")]
    private readonly SyncVar<string> _syncGamePhase = new SyncVar<string>();
    public string gamePhase { get => _syncGamePhase.Value; set => _syncGamePhase.Value = value; }
    private readonly SyncVar<bool> _syncFirstHalfCompleted = new SyncVar<bool>();
    public bool firstHalfCompleted { get => _syncFirstHalfCompleted.Value; set => _syncFirstHalfCompleted.Value = value; } // tracks when the first half has completed and when the game should transition to halftime or game over
    private readonly SyncVar<string> _syncFirstHalfKickingTeam = new SyncVar<string>();
    public string firstHalfKickingTeam { get => _syncFirstHalfKickingTeam.Value; set => _syncFirstHalfKickingTeam.Value = value; } // name of the team that kicked in the first half (green or grey). That team will receive to start the second half

    [Header("Kickoff Info")]
    public GamePlayer kickingPlayer;
    public GamePlayer receivingPlayer;
    public Team kickingTeam;
    public Team receivingTeam;

    [Header("Touchdown")]
    [SerializeField] GameObject TouchDownPanel;
    [SerializeField] TextMeshProUGUI touchDownText;
    [SerializeField] TextMeshProUGUI touchDownTeamText;

    [Header("Kick After Attempt")]
    public GamePlayer scoringPlayer;
    public GamePlayer blockingPlayer;
    public Team scoringTeam;
    public Team blockingTeam;
    private readonly SyncVar<float> _syncYPositionOfKickAfter = new SyncVar<float>();
    public float yPositionOfKickAfter { get => _syncYPositionOfKickAfter.Value; set => _syncYPositionOfKickAfter.Value = value; }
    private readonly SyncVar<int> _syncScoringGoblinNetId = new SyncVar<int>(ObjectLookup.NoObject);
    public int scoringGoblinNetId { get => _syncScoringGoblinNetId.Value; set => _syncScoringGoblinNetId.Value = value; }
    [SerializeField] GameObject KickAfterPositionControlsPanel;
    [SerializeField] GameObject KickAfterTimerBeforeKickPanel;
    [SerializeField] GameObject TimerInstructionsBoard;
    [SerializeField] TextMeshProUGUI TimerInstructionsText;
    [SerializeField] TextMeshProUGUI KickAfterTimerText;
    private readonly SyncVar<int> _syncKickAfterTimer = new SyncVar<int>();
    private int KickAfterTimer { get => _syncKickAfterTimer.Value; set => _syncKickAfterTimer.Value = value; }
    [SerializeField] GameObject KickAfterWasKickGoodPanel;
    [SerializeField] TextMeshProUGUI TheKickWasText;
    [SerializeField] TextMeshProUGUI KickWasGoodText;
    [SerializeField] TextMeshProUGUI KickWasNotGoodText;
    [SerializeField] TextMeshProUGUI KickWasBlockedText;
    bool kickAfterRoutineRunning = false;
    [SerializeField] bool _wasKickGoodOrBadSubmitted = false;

    [Header("Xtra Time")]
    private readonly SyncVar<bool> _syncIsXtraTime = new SyncVar<bool>();
    public bool isXtraTime { get => _syncIsXtraTime.Value; set => _syncIsXtraTime.Value = value; }


    [Header("Game timer")]
    [SerializeField] float lengthOfHalves;
    [SerializeField] TextMeshProUGUI timerText;
    private readonly SyncVar<float> _syncTimeLeftInGame = new SyncVar<float>();
    public float timeLeftInGame { get => _syncTimeLeftInGame.Value; set => _syncTimeLeftInGame.Value = value; }
    IEnumerator gameTimeRoutine;
    public bool isGameTimerRunning = false;
    string minutes;
    string seconds;

    [Header("Score")]
    private readonly SyncVar<int> _syncGreenScore = new SyncVar<int>();
    public int greenScore { get => _syncGreenScore.Value; set => _syncGreenScore.Value = value; }
    private readonly SyncVar<int> _syncGreyScore = new SyncVar<int>();
    public int greyScore { get => _syncGreyScore.Value; set => _syncGreyScore.Value = value; }
    [SerializeField] TextMeshProUGUI ScoreGreenText;
    [SerializeField] TextMeshProUGUI ScoreGreyText;
    private readonly SyncVar<string> _syncWinningTeam = new SyncVar<string>();
    private string winningTeam { get => _syncWinningTeam.Value; set => _syncWinningTeam.Value = value; }

    [Header("Game Over")]
    [SerializeField] GameObject GameOverScorePanel;
    [SerializeField] TextMeshProUGUI theFinalScoreText;
    [SerializeField] TextMeshProUGUI greenTeamNameText;
    [SerializeField] TextMeshProUGUI greyTeamNameText;
    [SerializeField] TextMeshProUGUI greenTeamScoreText;
    [SerializeField] TextMeshProUGUI greyTeamScoreText;
    [SerializeField] TextMeshProUGUI teamWinnerText;
    [SerializeField] GameObject GameOverScrollPanelObject;
    public bool gameWinnersNotified = false;
    public bool mercyRuleResultsSent = false;
    // Set by HandleTeamAbandoned when a team has no players left, and read by DetermineWinnerOfGame. Server only: the
    // clients hear about a forfeit through RpcWinnerOfGameDetermined.
    string forfeitWinner = "";

    [Header("UI Stuff")]
    [SerializeField] GameObject PossessionFootballGreen;
    [SerializeField] GameObject PossessionFootballGrey;
    [SerializeField] GameObject PossessionBarGreen;
    [SerializeField] GameObject PossessionBarGrey;

    [Header("UI to update for gamepad/keyboard controls")]
    [SerializeField] private TextMeshProUGUI MoveLeftText;
    [SerializeField] private TextMeshProUGUI MoveRightText;
    [SerializeField] private TextMeshProUGUI EnterToSubmitText;

    [Header("Field Parameters")]
    [SerializeField] public float maxY; // 5. 6f
    [SerializeField] public float minY; // -6. 5f
    [SerializeField] public float maxX; // 44. 4f
    [SerializeField] public float minX; // -44. 5f

    [Header("Event System Stuff")]
    [SerializeField] EventSystem eventSystem;
    [SerializeField] InputSystemUIInputModule inputSystem;

    [Header("AIPlayer Stuff")]
    [SerializeField] GameObject aiPlayerPrefab;

    [Header("Pause/Resume Game")]
    [SerializeField] GameObject GamePausedTextObject;
    [SerializeField] TextMeshProUGUI GamePausedSecondsLeftText;
    private readonly SyncVar<bool> _syncIsGamePaused = new SyncVar<bool>();
    public bool isGamePaused { get => _syncIsGamePaused.Value; set => _syncIsGamePaused.Value = value; }
    public GamePlayer playerWhoPaused;
    private readonly SyncVar<int> _syncPlayerWhoPausedNetId = new SyncVar<int>(ObjectLookup.NoObject);
    public int playerWhoPausedNetId { get => _syncPlayerWhoPausedNetId.Value; set => _syncPlayerWhoPausedNetId.Value = value; }
    private readonly SyncVar<float> _syncLastPauseTimeStamp = new SyncVar<float>();
    public float lastPauseTimeStamp { get => _syncLastPauseTimeStamp.Value; set => _syncLastPauseTimeStamp.Value = value; }
    bool isGamePausedTimeoutRoutineRunning = false;
    IEnumerator GamePausedTimeout;
    public float greenPauseTimeStamp = -30f;
    public float greyPauseTimeStamp = -30f;

    [Header("Timeouts: UI Stuff")]
    [SerializeField] GameObject TimeoutTimerHolder;
    [SerializeField] TextMeshProUGUI timeoutTimerText;

    [Header("Timeouts: Kick-after")]
    public bool isKickAfterTimerCountDownRunning = false;
    public bool isKickAfterTimerRunning = false;
    IEnumerator timeoutKickAfter;
    private readonly SyncVar<int> _syncKickAfterTimerTimeLeft = new SyncVar<int>(30);
    private int kickAfterTimerTimeLeft { get => _syncKickAfterTimerTimeLeft.Value; set => _syncKickAfterTimerTimeLeft.Value = value; }

    [Header("Timeouts: Kick Off")]
    public bool isKickOffTimerCountDownRunning = false;
    IEnumerator timeoutKickOff;
    private readonly SyncVar<int> _syncKickOffTimerTimeLeft = new SyncVar<int>(30);
    private int kickOffTimerTimeLeft { get => _syncKickOffTimerTimeLeft.Value; set => _syncKickOffTimerTimeLeft.Value = value; }

    [Header("Error Messages Display")]
    [SerializeField] GameObject errorCanvas;
    [SerializeField] TextMeshProUGUI errorCanvasText;
    public bool isErrorMessageDisplayed;
    IEnumerator displayErrorMessageFromServer;

    private NetworkManagerGRF game;
    private NetworkManagerGRF Game
    {
        get
        {
            if (game != null)
            {
                return game;
            }
            return game = NetworkManagerGRF.singleton as NetworkManagerGRF;
        }
    }
    private void Awake()
    {
        _syncGamePhase.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleGamePhase(prev, next); };
        _syncKickAfterTimer.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleKickAfterTimerUpdate(prev, next); };
        _syncTimeLeftInGame.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleGameTimerUpdate(prev, next); };
        _syncGreenScore.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleGreenScoreUpdate(prev, next); };
        _syncGreyScore.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleGreyScoreUpdate(prev, next); };
        _syncIsGamePaused.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleIsGamePaused(prev, next); };
        _syncKickAfterTimerTimeLeft.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleKickAfterTimerTimeLeft(prev, next); };
        _syncKickOffTimerTimeLeft.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleKickOffTimerTimeLeft(prev, next); };
        MakeInstance();
        gameplayCanvas.SetActive(false);
        coinTossCanvas.SetActive(false);
        TimeoutTimerHolder.SetActive(false);
    }
    void MakeInstance()
    {
        Debug.Log("GameplayManager MakeInstance.");
        if (instance == null)
            instance = this;
    }
    // This was Start() with Mirror. FishNet can run Start before this scene object has started on the network, so it
    // happens here instead. On the host this runs after OnStartServer, so isSinglePlayer is already set.
    public override void OnStartClient()
    {
        base.OnStartClient();
        UpdateKickAfterControlsUIForGamepad(GamepadUIManager.instance.gamepadUI);
        StartWithLocalGamePlayer();
    }
    // Needs the local GamePlayer. FishNet 4 can start this scene object before that player, so the player's own
    // OnStartClient calls this again once it exists. Runs once.
    public void StartWithLocalGamePlayer()
    {
        if (LocalGamePlayerScript != null || GameObject.Find("LocalGamePlayer") == null)
            return;
        GetLocalGamePlayer();
        // Spawn the AIPlayer if this is a singleplayer game
        if (IsServerInitialized && this.isSinglePlayer && !this.is1v1)
        {
            SpawnAIPlayer(!LocalGamePlayerScript.isTeamGrey, 2, "AIPlayer", "");
        }
        else if (IsServerInitialized && !this.is1v1 && !this.isSinglePlayer)
        {
            InitializeWaitingAIPlayers();
        }
    }
    // A 3v3 bot is spawned in the lobby, before this scene and its managers exist, so it could not set up its team,
    // coin role or goblins there. Now it can. Single player's AI never waits: GameplayManager spawns it from here.
    void InitializeWaitingAIPlayers()
    {
        foreach (GamePlayer player in Game.GamePlayers)
        {
            if (player != null && player.isAIPlayer && !player.aiInitialized)
            {
                Debug.Log("InitializeWaitingAIPlayers: setting up the bot playing the " + player.goblinType + " for the "
                    + (player.isTeamGrey ? "grey" : "green") + " team.");
                player.InitializeAIPlayer();
            }
        }
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        this.is1v1 = Game.is1v1;
        this.isSinglePlayer = Game.isSinglePlayer;
        this.secondsPerHalf = Game.secondsPerHalf;
        this.powerUpsEnabled = Game.powerUpsEnabled;
        this.randomEventsEnabled = Game.randomEventsEnabled;
        this.spawnObstaclesEnabled = Game.spawnObstaclesEnabled;
        this.mercyRuleEnabled = Game.mercyRuleEnabled;
        this.mercyRulePointDifferential = Game.mercyRulePointDifferential;

        Debug.Log("GameplayManager: is 1v1 is set to: " + this.is1v1);
        //gamePhase = "cointoss";
        gamePhase = "cointoss";
        //timeLeftInGame = 300f;
        //timeLeftInGame = 30f;
        timeLeftInGame = secondsPerHalf;
        greenScore = 0;
        greyScore = 0;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    [Server(Logging = LoggingType.Off)]
    private void FixedUpdate()
    {
        /*if (isGameTimerRunning)
        {
            timeLeftInGame -= Time.fixedDeltaTime;
        }*/
    }
    [Client]
    void GetLocalGamePlayer()
    {
        Debug.Log("GetLocalGamePlayer: Trying to find local game player");
        LocalGamePlayer = GameObject.Find("LocalGamePlayer");
        LocalGamePlayerScript = LocalGamePlayer.GetComponent<GamePlayer>();
        if (LocalGamePlayerScript)
        {
            LocalGamePlayerScript.InitializeLocalGamePlayer();
        }
        
    }
    // Step 3 calls this once per goblin nobody picked, so the team, number and name are the caller's choice. The
    // values are still set after Spawn: SetPlayerName and the others are [Server] methods, which FishNet skips
    // until the object is network-initialized.
    GamePlayer SpawnAIPlayer(bool isTeamGrey, int playerNumber, string playerName, string goblinType)
    {
        GameObject aiPlayerObject = Instantiate(aiPlayerPrefab);
        InstanceFinder.ServerManager.Spawn(aiPlayerObject,LocalGamePlayerScript.Owner);
        GamePlayer aiGamePlayerScript = aiPlayerObject.GetComponent<GamePlayer>();
        aiGamePlayerScript.isSinglePlayer = LocalGamePlayerScript.isSinglePlayer;
        aiGamePlayerScript.isTeamGrey = isTeamGrey;
        aiGamePlayerScript.goblinType = goblinType;
        aiGamePlayerScript.SetPlayerName(playerName);
        aiGamePlayerScript.SetConnectionId(-1);
        aiGamePlayerScript.SetPlayerNumber(playerNumber);
        // InitializeAIPlayer sends ServerRpcs, which FishNet only sends once the host's client side has started this
        // object, so the AI GamePlayer calls it from its own OnStartClient.
        return aiGamePlayerScript;
    }

    public void ActivateCoinTossUI(bool activate)
    {
        Debug.Log("ActivateCoinTossUI: " + activate.ToString());
        coinTossCanvas.SetActive(activate);
    }
    public void EnableGoblinMovement()
    {
        Debug.Log("EnableGoblinMovement");
        LocalGamePlayerScript.EnableGoblinMovement(true);
        LocalGamePlayerScript.EnableGoblinMovementControlsServerValues(true);
    }
    public void DisableGoblinMovement()
    {
        Debug.Log("EnableGoblinMovement");
        LocalGamePlayerScript.EnableGoblinMovement(false);
        LocalGamePlayerScript.EnableGoblinMovementControlsServerValues(false);
    }
    public void HandleGamePhase(string oldValue, string newValue)
    {
        // Possible Game phases should be: cointoss, kickoff, gameplay, touchdown-transition, kick-after-attempt, xtra-time, halftime, gameover
        if (IsServerInitialized)
        {
            if (newValue == "kickoff")
            {
                RepositionTeamsForKickOff();
                ResetGoblinKickAfterRespositioningBool();
                RpcNoTeamWithFootball();
                ObstacleManager.instance.DisableCollidersOnObjects(false);
                EnableGoblinCollidersOnServer(false);
                StopPossessionRoutinesForPlayers();
            }
            if (newValue == "gameplay")
            {
                ActivateGameplayControls(true);
                RpcKickOffWhistle();
                PowerUpManager.instance.StartGeneratingPowerUps(true);
                RandomEventManager.instance.StartGeneratingRandomEvents(true);
                ObstacleManager.instance.DisableCollidersOnObjects(true);
                CowboyManager.instance.StartGeneratingCowboys(true);
                EnableGoblinCollidersOnServer(true);
                if (isXtraTime)
                    isXtraTime = false;
            }
            if (oldValue == "gameplay")
            {
                PowerUpManager.instance.StartGeneratingPowerUps(false);
                RandomEventManager.instance.StartGeneratingRandomEvents(false);
                ObstacleManager.instance.DisableCollidersOnObjects(false);
                CowboyManager.instance.StartGeneratingCowboys(false);
                EnableGoblinCollidersOnServer(false);
            }                
            if (newValue == "kick-after-attempt")
            {
                // Before anything else: a blockKick left set by the previous kick-after would send that skirmisher at
                // the kicker the instant this one starts, before a position has been chosen, and block it. This is the
                // server's hook, which runs ahead of the client one and of the blocking being armed.
                ClearAllAIBlockKick();
                SetKickAfterPlayers(scoringGoblinNetId);
                RpcNoTeamWithFootball();
                ObstacleManager.instance.KickAfterWaitToEnableObstacleColliders();
                DisableGoblinColliderForKickAfter();
                StopPossessionRoutinesForPlayers();
            }
            if (oldValue == "kick-after-attempt")
            {
                try
                {
                    GoblinScript goblinThatKicked = ObjectLookup.Get(scoringGoblinNetId).GetComponent<GoblinScript>();
                    goblinThatKicked.isKickAfterPositionSet = false;
                    goblinThatKicked.isKickAfterGoblin = false;
                    scoringGoblinNetId = ObjectLookup.NoObject;
                }
                catch (Exception e)
                {
                    Debug.Log("GameplayManager: HandleGamePhase: failed to reset scoring goblin info after kick attempt. Reason: " + e);
                }
                ObstacleManager.instance.DisableCollidersOnObjects(false);
                EnableGoblinCollidersOnServer(false);
            }
            if (newValue == "xtra-time")
            {
                isXtraTime = true;
                EnableGoblinCollidersOnServer(true);
                ObstacleManager.instance.DisableCollidersOnObjects(true);
            }
            if (newValue == "halftime")
            {
                Debug.Log("GameplayManager on Server: Halftime reached. Starting transition to the second half.");
                ActivateGameplayControls(false);
                TransitionToSecondHalf();
                StopPossessionRoutinesForPlayers();
            }
            if (newValue == "gameover")
            {
                Debug.Log("GameplayManager on Server: It's game over!");
                DetermineWinnerOfGame();
                ResetAllGoblinStatuses();
                ActivateGameplayControls(false);
                ActivateMenuNavigationControls(true);
            }
            
        }            
        if (IsClientInitialized)
        {
            if (newValue == "cointoss")
            {
                if (LocalGamePlayerScript)
                {
                    try
                    {
                        LocalGamePlayerScript.CoinTossControlls(true);
                    }
                    catch
                    {
                        Debug.Log("GameplayManager.cs: Could not find local game player script");
                    }
                }
            }
            if (newValue == "kickoff")
            {
                ActivateCoinTossUI(false);
                if (!gameplayCanvas.activeInHierarchy)
                    gameplayCanvas.SetActive(true);
                Debug.Log("Starting the Kickoff Phase");
                //LocalGamePlayerScript.EnableGoblinMovement(true);
                //LocalGamePlayerScript.ResetCameraPositionForKickOff();
                LocalGamePlayerScript.FollowSelectedGoblin(LocalGamePlayerScript.selectGoblin.transform);
                LocalGamePlayerScript.CoinTossControlls(false);
                LocalGamePlayerScript.KickOrReceiveControls(false);
                LocalGamePlayerScript.DisableKickOrReceiveControls();
                if(!this.is1v1 && !this.isSinglePlayer)
                {
                    LocalGamePlayerScript.GetGoblinTeammatesFor3v3();
                    // A bot needs the same Q/E mapping, and this was only ever called for the person's own player, so
                    // a bot's qGoblin and eGoblin stayed null: AIPlayer picked a teammate to pass to, matched neither
                    // slot and dropped the pass. Only for the bots this machine owns, since the mapping is per player
                    // and a client has no business building one for a bot.
                    foreach (GamePlayer player in Game.GamePlayers)
                    {
                        if (player != null && player.isAIPlayer && player.IsOwner)
                            player.GetGoblinTeammatesFor3v3();
                    }
                }
            }
            if (newValue == "kick-after-attempt")
            {
                HideTouchDownText();

                // Have the AI start their kick after attempt if they are the scoring player. The mode check this used
                // to carry said "single player", which stopped a 3v3 bot from ever taking its own kick-after.
                if (scoringPlayer != null && scoringPlayer.isAIPlayer)
                    AIKickAfterKickingPlayer();
            }
            if (oldValue == "kick-after-attempt")
            {
                ResetKickAfterPositionBoolValue();
            }
            if (newValue == "xtra-time")
            {
                //timerText.GetComponent<TimeText>().StartXtraTime();
            }
            if (newValue == "halftime")
            {
                timerText.GetComponent<TimeText>().EndFirstHalfExtraTime();
            }
            if (newValue == "gameover")
            {
                timerText.GetComponent<TimeText>().EndXtraTime();
            }
        }
    }
    [Server]
    void RepositionTeamsForKickOff()
    {
        Debug.Log("RepositionTeamsForKickOff in gamemanager");
        ResetAllGoblinStatuses();
        if (this.is1v1 || this.isSinglePlayer)
        {
            receivingPlayer.RpcRepositionTeamForKickOff(false);
            receivingPlayer.gameplayActionControlsOnServer = false;
            receivingPlayer.goblinMovementControlsOnServer = false;
            receivingPlayer.powerupsControlsOnServer = false;
            receivingPlayer.kickingControlsOnServer = false;
            receivingPlayer.kickOffAimArrowControlsOnServer = false;            
            receivingPlayer.qeSwitchingControlsOnServer = true;

            kickingPlayer.RpcRepositionTeamForKickOff(true);
            kickingPlayer.gameplayActionControlsOnServer = false;
            kickingPlayer.goblinMovementControlsOnServer = false;
            kickingPlayer.powerupsControlsOnServer = false;
            kickingPlayer.kickingControlsOnServer = true;
            kickingPlayer.kickOffAimArrowControlsOnServer = true;
            kickingPlayer.qeSwitchingControlsOnServer = false;
        }
        else
        {
            foreach (GamePlayer player in kickingTeam.teamPlayers)
            {
                bool isThisTheKickingPlayer = false;
                if (player == kickingPlayer)
                    isThisTheKickingPlayer = true;
                player.RpcRepositionTeamForKickOff3v3(player.Owner, true, isThisTheKickingPlayer);
                player.gameplayActionControlsOnServer = false;
                player.goblinMovementControlsOnServer = false;
                player.powerupsControlsOnServer = false;
                player.kickingControlsOnServer = isThisTheKickingPlayer;
                player.kickOffAimArrowControlsOnServer = isThisTheKickingPlayer;
                player.qeSwitchingControlsOnServer = false;
            }
            foreach (GamePlayer player in receivingTeam.teamPlayers)
            {

                player.RpcRepositionTeamForKickOff3v3(player.Owner, false, false);
                player.gameplayActionControlsOnServer = false;
                player.goblinMovementControlsOnServer = false;
                player.powerupsControlsOnServer = false;
                player.kickingControlsOnServer = false;
                player.kickOffAimArrowControlsOnServer = false;
                player.qeSwitchingControlsOnServer = true;
            }
        }
        if (!this.isSinglePlayer)
        {
            if (isKickOffTimerCountDownRunning)
            {
                try
                {
                    isKickOffTimerCountDownRunning = false;
                    StopCoroutine(timeoutKickOff);
                }
                catch (Exception e)
                {
                    Debug.Log("RepositionTeamsForKickOff: Tried and failed to stop Kick Off Timer coroutine. Error: " + e);
                }
            }
            timeoutKickOff = TimeoutKickOffRoutine();
            StartCoroutine(timeoutKickOff);
        }
        
    }
    [Server]
    void ActivateGameplayControls(bool activate)
    {
        foreach (GamePlayer player in Game.GamePlayers)
        {
            player.RpcActivateGameplayControls(activate);
            player.kickOffAimArrowControlsOnServer = false;
            player.qeSwitchingControlsOnServer = activate;
            player.kickingControlsOnServer = activate;
            player.goblinMovementControlsOnServer = activate;
            player.gameplayActionControlsOnServer = activate;
            player.powerupsControlsOnServer = activate;
        }
    }
    [Server]
    void ActivateMenuNavigationControls(bool activate)
    {
        foreach (GamePlayer player in Game.GamePlayers)
        {
            player.RpcActivateMenuNavigationControls(activate);
        }
    }
    [Server]
    public void TouchDownScored(bool wasGrey, int goblinNetId, float yPosition)
    {
        Debug.Log("TouchDownScored: " + goblinNetId.ToString());
        //Remove this place holder phase if it causes issues???
        gamePhase = "touchdown-transition";

        //SetKickAfterPlayers(scoringGoblinNetId);
        this.scoringGoblinNetId = goblinNetId;

        //update scores:
        if (wasGrey)
            greyScore += 5;
        else
            greenScore += 5;

        yPositionOfKickAfter = yPosition;

        TeamManager.instance.TouchdownScored(wasGrey);
        ActivateGameplayControls(false);
        if (isXtraTime && firstHalfCompleted)
        {
            int differenceInScores = greenScore - greyScore;
            if (Mathf.Abs(differenceInScores) > 2)
            {
                gamePhase = "gameover";
                return;
            }            
        }
        // If mercy rule is enabled, check if the new score is greater than mercy rule differential
        if (this.mercyRuleEnabled)
        {
            if (CheckForMercyRule())
            {
                gamePhase = "gameover";
                if (greenScore > greyScore)
                {
                    if (!mercyRuleResultsSent)
                    {
                        TeamManager.instance.greenTeam.TellTeamMembersMercyRuleEndedGame(true);
                        TeamManager.instance.greyTeam.TellTeamMembersMercyRuleEndedGame(false);
                        mercyRuleResultsSent = true;
                    }
                }
                else if (greyScore > greenScore)
                {
                    if (!mercyRuleResultsSent)
                    {
                        TeamManager.instance.greenTeam.TellTeamMembersMercyRuleEndedGame(false);
                        TeamManager.instance.greyTeam.TellTeamMembersMercyRuleEndedGame(true);
                        mercyRuleResultsSent = true;
                    }
                }
                return;
            }
        }

        RpcTouchDownScored(wasGrey, goblinNetId);
        IEnumerator touchdownToKickAfterTransition = GameplayToKickAfterTransition();
        StartCoroutine(touchdownToKickAfterTransition);
        StopPossessionRoutinesForPlayers();

        try
        {
            GoblinScript scoringGoblinScript = ObjectLookup.Get(goblinNetId).GetComponent<GoblinScript>();
            scoringGoblinScript.TouchdownScoredAchievement();
        }
        catch (Exception e)
        {
            Debug.Log("TouchDownScored: Could not get goblinscript from goblinNetId. Error: " + e);
        }
        

        // Stop Goblins from running?
        ResetAllGoblinStatuses();
    }
    [ObserversRpc]
    void RpcTouchDownScored(bool wasGrey, int goblinId)
    {
        Debug.Log("RpcTouchDownScored: " + wasGrey.ToString() + " and goblin netid of : " + goblinId.ToString());
        touchDownText.text = "TOUCHDOWN!";
        touchDownText.gameObject.SetActive(true);
        if (wasGrey)
        {
            touchDownTeamText.text = "GREY";
        }
        else
        {
            touchDownTeamText.text = "GREEN";
        }
        touchDownTeamText.gameObject.SetActive(true);
        TouchDownPanel.SetActive(true);
        touchDownText.GetComponent<TouchDownTextGradient>().ActivateGradient();
        touchDownTeamText.GetComponent<TouchDownTextGradient>().SetGreenOrGreyColor(wasGrey);
        GameObject scoringGoblin = ObjectLookup.Get(goblinId).gameObject;
        LocalGamePlayerScript.FollowSelectedGoblin(scoringGoblin.transform);
        SoundManager.instance.PlaySound("touchdown-cheer", 0.75f);
        SoundManager.instance.PlaySound("touchdown-touchdown", 1.0f);
    }
    [Server]
    public void ActivateGameTimer(bool activate)
    {
        if (activate)
        {
            Debug.Log("ActivateGameTimer: " + activate.ToString());
            gameTimeRoutine = GameTimerCountdown();
            StartCoroutine(gameTimeRoutine);
        }
        else
        {
            Debug.Log("ActivateGameTimer: " + activate.ToString());
            isGameTimerRunning = false;
            StopCoroutine(gameTimeRoutine);
        }
        foreach (GamePlayer aiPlayer in Game.GamePlayers)
        {
            if (aiPlayer == null || !aiPlayer.isAIPlayer || aiPlayer.myAIPlayer == null)
                continue;
            try
            {
                aiPlayer.myAIPlayer.ActivateAIPowerUpRoutine(activate);
            }
            catch (Exception e)
            {
                Debug.Log("ActivateGameTimer: Unable to access the AIPlayer script to start their powerup routine. Error: " + e);
            }
        }
    }
    [Server]
    IEnumerator GameTimerCountdown()
    {
        isGameTimerRunning = true;
        float timeLeftTracker;
        while (isGameTimerRunning)
        {
            // Don't count down the time if the game is paused. Wait a frame first: a bare continue never yields, so a pause
            // seen here spun this loop forever and froze the host.
            if (this.isGamePaused)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(1.0f);
            timeLeftTracker = timeLeftInGame - 1;
            
            timeLeftInGame = timeLeftTracker;
            if (timeLeftTracker <= 0)
            {
                timeLeftInGame = 0;
                isGameTimerRunning = false;
                gamePhase = "xtra-time";
            }
        }
        yield break;        
    }
    void HandleGameTimerUpdate(float oldValue, float newValue)
    {
        if (IsClientInitialized)
        {
            /*minutes = Mathf.Floor(newValue / 60).ToString("0");
            seconds = Mathf.Floor(newValue % 60).ToString("00");
            timerText.text = minutes + ":" + seconds;*/
            SetTimerText(newValue);
        }
    }
    public void SetTimerText(float timeToSet)
    {
        if (timeToSet > 0)
        {
            minutes = Mathf.Floor(timeToSet / 60).ToString("0");
            seconds = Mathf.Floor(timeToSet % 60).ToString("00");
            timerText.text = minutes + ":" + seconds;
        }
        else
        {
            timerText.GetComponent<TimeText>().StartXtraTime();
        }
        
    }
    void HandleGreenScoreUpdate(int oldValue, int newValue)
    {
        if (IsServerInitialized)
        {
            //CheckForMercyRule();
        }   
        if (IsClientInitialized)
        {
            ScoreGreenText.text = newValue.ToString("00");
            greenTeamScoreText.text = newValue.ToString("00");
        }
    }
    void HandleGreyScoreUpdate(int oldValue, int newValue)
    {
        if (IsServerInitialized)
        {
            //CheckForMercyRule();
        }   
        if (IsClientInitialized)
        {
            ScoreGreyText.text = newValue.ToString("00");
            greyTeamScoreText.text = newValue.ToString("00");
        }
    }
    [Server]
    IEnumerator GameplayToKickAfterTransition()
    {
        yield return new WaitForSeconds(3.0f);
        gamePhase = "kick-after-attempt";
    }
    void HideTouchDownText()
    {
        Debug.Log("HideTouchDownText");
        TouchDownPanel.SetActive(false);
        touchDownText.gameObject.SetActive(false);
        touchDownTeamText.gameObject.SetActive(false);
        touchDownText.GetComponent<TouchDownTextGradient>().isColorChangeRunning = false;
    }
    [Server]
    void SetKickAfterPlayers(int scoringGoblin)
    {
        Debug.Log("SetKickAfterPlayers: the scoring goblin was: " + scoringGoblin.ToString());
        GoblinScript scoringGoblinScript = ObjectLookup.Get(scoringGoblin).GetComponent<GoblinScript>();
        scoringGoblinScript.isKickAfterGoblin = true;
        
        // Make sure all goblins are healed after the touchdown. This is to make sure that kicking and blocking goblins are not still knocked out.
        ResetAllGoblinStatuses();
        // This is to double check and make sure that the kicking player has the ball for the kick-after attempt
        if (!scoringGoblinScript.doesCharacterHaveBall)
        {
            //Football football = GameObject.FindGameObjectWithTag("football").GetComponent<Football>();
            //football.MoveFootballToKickoffGoblin(scoringGoblinScript.GetComponent<NetworkIdentity>().netId);
        }

        if (this.is1v1)
        {
            foreach (GamePlayer player in Game.GamePlayers)
            {
                // ConnectionId is -1 on every bot's GamePlayer, so it cannot identify one; ownerNetId can.
                if (scoringGoblinScript.ownerNetId == player.ObjectId)
                {
                    scoringPlayer = player;
                    kickingPlayer = player;
                    player.RpcRepositionForKickAfter(player.Owner, true, scoringGoblin, yPositionOfKickAfter);
                }
                else
                {
                    blockingPlayer = player;
                    receivingPlayer = player;
                    player.RpcRepositionForKickAfter(player.Owner, false, scoringGoblin, yPositionOfKickAfter);
                }
            }
        }
        else if (this.isSinglePlayer && !this.is1v1)
        {
            foreach (GamePlayer player in Game.GamePlayers)
            {
                // ConnectionId is -1 on every bot's GamePlayer, so it cannot identify one; ownerNetId can.
                if (scoringGoblinScript.ownerNetId == player.ObjectId)
                {
                    scoringPlayer = player;
                    kickingPlayer = player;
                    player.RpcRepositionForKickAfter(player.Owner, true, scoringGoblin, yPositionOfKickAfter);
                }
                else
                {
                    blockingPlayer = player;
                    receivingPlayer = player;
                    player.RpcRepositionForKickAfter(player.Owner, false, scoringGoblin, yPositionOfKickAfter);
                }
            }
        }
        else
        {
            if (scoringGoblinScript.isGoblinGrey)
            {
                scoringTeam = TeamManager.instance.greyTeam;
                blockingTeam = TeamManager.instance.greenTeam;
            }
            else
            {
                scoringTeam = TeamManager.instance.greenTeam;
                blockingTeam = TeamManager.instance.greyTeam;
            }
            if (IsTeamMissingItsCaptain(scoringTeam) || IsTeamMissingItsCaptain(blockingTeam))
                return;
            // Set kicking and receiving team/player based on who scored and who will be blocking
            kickingTeam = scoringTeam;
            kickingPlayer = scoringTeam.captain;
            receivingTeam = blockingTeam;
            receivingPlayer = blockingTeam.captain;
            blockingPlayer = blockingTeam.captain;
            // Get the owner of the goblin who scored, to know who to give the kick after controls to and so on.
            // This matched ownerConnectionId against each player's ConnectionId, which cannot tell host-owned bots
            // apart: every bot's GamePlayer is given ConnectionId -1, so their goblins carry -1 too, and the first
            // bot in GamePlayers won - often one on the other team. Nobody on the scoring team was then the kicking
            // player, so all three of its goblins took the "not the kicker" branch and lined up at midfield with no
            // kick ever taken. ownerNetId is the owning GamePlayer's own ObjectId, which is unique.
            if (ObjectLookup.TryGet(scoringGoblinScript.ownerNetId, out GamePlayer scoringGoblinOwner) && scoringGoblinOwner != null)
                scoringPlayer = scoringGoblinOwner;
            int yModifier = 1;
            // Reposition goblins for scoring team
            foreach (GamePlayer player in scoringTeam.teamPlayers)
            {
                if (player == scoringPlayer)
                    player.RpcRepositionForKickAfterFor3v3(player.Owner, true, true, scoringGoblin, yPositionOfKickAfter, 1);
                else
                {
                    yModifier *= -1;
                    player.RpcRepositionForKickAfterFor3v3(player.Owner, false, true, scoringGoblin, yPositionOfKickAfter, yModifier);
                }
            }
            foreach (GamePlayer player in blockingTeam.teamPlayers)
            {
                player.RpcRepositionForKickAfterFor3v3(player.Owner, false, false, scoringGoblin, yPositionOfKickAfter, 1);
            }
        }      
    }
    [Server]
    void ResetAllGoblinStatuses()
    {
        foreach (GameObject goblin in GameObject.FindGameObjectsWithTag("Goblin"))
        {
            goblin.GetComponent<GoblinScript>().StartHealNormal();
            goblin.GetComponent<GoblinScript>().RpcResetGoblinStatuses();
        }
    }
    public void ActivateKickAfterPositionControlsPanel(bool activate)
    {
        KickAfterPositionControlsPanel.SetActive(activate);
    }
    [Server]
    public void DisableKickAfterPositioningControls()
    {
        try
        {
            scoringPlayer.RpcDisableKickAfterPositioningControls(scoringPlayer.Owner);
        }
        catch (Exception e)
        {
            Debug.Log("DisableKickAfterPositioningControls: Could not disable kick after position controls for player: " + scoringPlayer.PlayerName + ": " + e);
        }
        
    }
    [Server]
    public void StartKickAfterTimer()
    {
        Debug.Log("StartKickAfterTimer: for 1v1? " + this.is1v1.ToString());
        if (this.is1v1)
        {
            scoringPlayer.RpcStartKickAfterTimer(scoringPlayer.Owner, true);
            blockingPlayer.RpcStartKickAfterTimer(blockingPlayer.Owner, false);
        }
        else if (this.isSinglePlayer && !this.is1v1)
        { 
            // Only have the player be sent the RpcStartKickAfterTimer stuff, not the AI. So, check if the player is the scoring player or blocking player and send approriate message
            if(scoringPlayer == LocalGamePlayerScript)
                scoringPlayer.RpcStartKickAfterTimer(scoringPlayer.Owner, true);
            else if (blockingPlayer == LocalGamePlayerScript)
                blockingPlayer.RpcStartKickAfterTimer(blockingPlayer.Owner, false);
        }
        else
        {
            foreach (GamePlayer player in scoringTeam.teamPlayers)
            {
                player.RpcStartKickAfterTimer(player.Owner, true);
            }
            foreach (GamePlayer player in blockingTeam.teamPlayers)
            {
                player.RpcStartKickAfterTimer(player.Owner, false);
            }
        }
        IEnumerator kickAfterCountdown = KickAfterTimerCountDown();
        StartCoroutine(kickAfterCountdown);
    }
    public void ActivateKickAfterTimerUI(bool isKickingPlayer)
    {
        Debug.Log("ActivateKickAfterTimerUI: is kicking playeR? " + isKickingPlayer.ToString());
        KickAfterPositionControlsPanel.SetActive(false);
        KickAfterTimerBeforeKickPanel.SetActive(true);
        KickAfterTimerText.gameObject.SetActive(true);
        if (isKickingPlayer)
        {
            TimerInstructionsText.text = "You will kick after the countdown!";
            TimerInstructionsText.color = Color.white;
        }
        else
        {
            TimerInstructionsText.text = "You can try and block the kick after the countdown!";
            TimerInstructionsText.color = Color.white;
        }
            
    }
    [Server]
    IEnumerator KickAfterTimerCountDown()
    {
        Debug.Log("KickAfterTimerCountDown: started");
        isKickAfterTimerCountDownRunning = true;
        KickAfterTimer = 3;
        yield return new WaitForSeconds(1.0f);
        KickAfterTimer = 2;
        yield return new WaitForSeconds(1.0f);
        KickAfterTimer = 1;
        yield return new WaitForSeconds(1.0f);
        RpcDisableKickAfterTimerUI();
        isKickAfterTimerCountDownRunning = false;
        if (this.is1v1)
        {
            scoringPlayer.RpcActivateKickAfterKickingControls(scoringPlayer.Owner, true);
            blockingPlayer.RpcActivateKickAfterKickingControls(blockingPlayer.Owner, false);

            scoringPlayer.RpcActivateKickAfterBlockingControls(scoringPlayer.Owner, false);
            blockingPlayer.RpcActivateKickAfterBlockingControls(blockingPlayer.Owner, true);

            scoringPlayer.RpcKickAfterUpdateInsctructionsText(scoringPlayer.Owner, true);
            blockingPlayer.RpcKickAfterUpdateInsctructionsText(blockingPlayer.Owner, false);
        }
        else if (this.isSinglePlayer && !this.is1v1)
        {
            // Only have the player be sent the RpcStartKickAfterTimer stuff, not the AI. So, check if the player is the scoring player or blocking player and send approriate message
            if (scoringPlayer == LocalGamePlayerScript)
            {
                scoringPlayer.RpcActivateKickAfterKickingControls(scoringPlayer.Owner, true);
                scoringPlayer.RpcActivateKickAfterBlockingControls(scoringPlayer.Owner, false);
                scoringPlayer.RpcKickAfterUpdateInsctructionsText(scoringPlayer.Owner, true);
            }
            else if (blockingPlayer == LocalGamePlayerScript)
            {
                blockingPlayer.RpcActivateKickAfterKickingControls(blockingPlayer.Owner, false);
                blockingPlayer.RpcActivateKickAfterBlockingControls(blockingPlayer.Owner, true);
                blockingPlayer.RpcKickAfterUpdateInsctructionsText(blockingPlayer.Owner, false);
            }
            // This asked whether the blocking team's captain is a bot. The captain is the Grenadier's owner,
            // while the goblin that blocks is the skirmisher, so a person playing the Grenadier left the bot
            // skirmisher behind it unarmed and nothing contested the kick. Ask who actually has the skirmisher.
            if (BlockingSkirmisherAI() != null)
            {
                AIBlockingPlayer();
            }
            else if (scoringPlayer != null && scoringPlayer.isAIPlayer)
            {
                AIKickAfterAttempt();
            }
        }
        else
        {
            foreach (GamePlayer player in scoringTeam.teamPlayers)
            {
                if (player == scoringPlayer)
                {
                    Debug.Log("KickAfterTimerCountDown: send true to RpcActivateKickAfterKickingControls for scoring playeR: " + player.PlayerName);
                    player.RpcActivateKickAfterKickingControls(player.Owner, true);
                }
                player.RpcActivateKickAfterBlockingControls(player.Owner, false);
                player.RpcKickAfterUpdateInsctructionsText(player.Owner, true);
            }
            foreach (GamePlayer player in blockingTeam.teamPlayers)
            {
                player.RpcActivateKickAfterKickingControls(player.Owner, false);
                player.RpcActivateKickAfterBlockingControls(player.Owner, true);
                player.RpcKickAfterUpdateInsctructionsText(player.Owner, false);
            }
            // A bot on either end of a 3v3 kick-after has to be told to act, the same as the single player branch
            // above does. Without this a kick-after between two bot captains never resolved. The blocker is asked
            // for by its skirmisher rather than by the captain, for the reason given on the branch above.
            if (BlockingSkirmisherAI() != null)
            {
                AIBlockingPlayer();
            }
            if (scoringPlayer != null && scoringPlayer.isAIPlayer)
            {
                AIKickAfterAttempt();
            }
        }
        
        Debug.Log("KickAfterTimerCountDown: ended");
    }
    void HandleKickAfterTimerUpdate(int oldValue, int newValue)
    {
        if (IsClientInitialized)
        {
            KickAfterTimerText.text = newValue.ToString();
        }
    }
    [ObserversRpc]
    void RpcDisableKickAfterTimerUI()
    {
        KickAfterTimerText.gameObject.SetActive(false);
    }
    public void KickAfterUpdateInsctructionsText(bool isKickingPlayer)
    {
        if (isKickingPlayer)
        {
            if (GamepadUIManager.instance.gamepadUI)
            {
                TimerInstructionsText.text = "\"A\" to submit kick accuracy and power!";
            }
            else
            {
                TimerInstructionsText.text = "\"Tab\" to submit kick accuracy and power!";
            }
            
            TimerInstructionsText.color = Color.yellow;
        }
        else
        {
            TimerInstructionsText.text = "Run/slide into the kicker to block the kick!";
            TimerInstructionsText.color = Color.yellow;
        }            
    }
    [Server]
    public void KickAfterWasKickGoodOrBad(bool isKickGood)
    {
        if (_wasKickGoodOrBadSubmitted)
        {
            Debug.Log("KickAfterWasKickGoodOrBad: _wasKickGoodOrBadSubmitted has already been set. Returning... ");
            return;
        }
            
        Debug.Log("KickAfterWasKickGoodOrBad: Was the kick after attempt good? " + isKickGood.ToString());
        bool isScoringPlayerGrey = false;
        if (scoringPlayer.teamName == "Grey")
            isScoringPlayerGrey = true;

        if (isKickGood)
        {
            if (scoringPlayer.teamName == "Grey")
            {
                greyScore += 2;
                //isScoringPlayerGrey = true;
            }
            else
            {
                greenScore += 2;
            }
            // Kick after attempt achievement checks
            if (scoringPlayer)
            {
                scoringPlayer.RpcKickAfterAttemptAchievement(scoringPlayer.Owner);
            }
        }
        _wasKickGoodOrBadSubmitted = true;
        StartCoroutine(WasKickGoodOrBadSubmittedCooldownRoutine());
        RpcKickAfterWasKickGoodOrBad(isKickGood, isScoringPlayerGrey);
        TeamManager.instance.KickAfterAttempts(scoringPlayer.isTeamGrey, isKickGood);
        if (this.isSinglePlayer && !this.is1v1)
        {
            AIStopBlockingKickAfter();
        }
    }
    IEnumerator WasKickGoodOrBadSubmittedCooldownRoutine()
    {
        yield return new WaitForSeconds(0.2f);
        _wasKickGoodOrBadSubmitted = false;
    }
    [ObserversRpc]
    void RpcKickAfterWasKickGoodOrBad(bool isKickGood, bool isScoringPlayerGrey)
    {
        // in case the kick was good/was blocked thing happen at the same time or one after the other, turn off the kcik was blocked text stuff
        KickAfterWasKickGoodPanel.SetActive(false);
        TheKickWasText.gameObject.SetActive(false);
        KickWasBlockedText.gameObject.SetActive(false);
        SoundManager.instance.StopSound("kick-blocked");

        KickAfterPositionControlsPanel.SetActive(false);
        KickAfterTimerBeforeKickPanel.SetActive(false);
        KickAfterWasKickGoodPanel.SetActive(true);
        TheKickWasText.gameObject.SetActive(true);
        if (isKickGood)
        {
            KickWasNotGoodText.gameObject.SetActive(false);
            KickWasGoodText.gameObject.SetActive(true);
            KickWasGoodText.GetComponent<TouchDownTextGradient>().ActivateGradient();
            SoundManager.instance.PlaySound("kick-its-good", 1.0f);
            SoundManager.instance.PlaySound("touchdown-cheer", 0.75f);
        }
        else
        {
            KickWasNotGoodText.gameObject.SetActive(true);
            KickWasGoodText.gameObject.SetActive(false);
            KickWasNotGoodText.GetComponent<TouchDownTextGradient>().SetGreenOrGreyColor(isScoringPlayerGrey);
            SoundManager.instance.PlaySound("kick-no-good", 1.0f);
            SoundManager.instance.PlaySound("touchdown-cheer", 0.75f);
        }
    }
    [Server]
    public void TransitionFromKickAfterAttemptToKickOff()
    {
        /*if (isXtraTime && !firstHalfCompleted)
        {
            HandleGamePhase(this.gamePhase, "halftime");
        }
        else
        {
            IEnumerator TransitionFromKickAfterAttemptToKickOff = TransitionFromKickAfterAttemptToKickOffRoutine();
            StartCoroutine(TransitionFromKickAfterAttemptToKickOff);
        }*/
        Debug.Log("TransitionFromKickAfterAttemptToKickOff running on the server. kickAfterRoutineRunning is set to: " + kickAfterRoutineRunning.ToString());
        if (!kickAfterRoutineRunning)
        {
            IEnumerator TransitionFromKickAfterAttemptToKickOff = TransitionFromKickAfterAttemptToKickOffRoutine();
            StartCoroutine(TransitionFromKickAfterAttemptToKickOff);
        }
        

    }
    [Server]
    public void DisableKickAfterAttemptControls()
    {
        try
        {
            if (this.is1v1)
            {
                scoringPlayer.RpcActivateKickAfterKickingControls(scoringPlayer.Owner, false);
                blockingPlayer.RpcActivateKickAfterBlockingControls(blockingPlayer.Owner, false);
            }
            else if (this.isSinglePlayer && !this.is1v1)
            { 
                if(scoringPlayer == LocalGamePlayerScript)
                    scoringPlayer.RpcActivateKickAfterKickingControls(scoringPlayer.Owner, false);
                else if (blockingPlayer == LocalGamePlayerScript)
                    blockingPlayer.RpcActivateKickAfterBlockingControls(blockingPlayer.Owner, false);
            }
            else
            {
                foreach (GamePlayer player in scoringTeam.teamPlayers)
                {
                    player.RpcActivateKickAfterKickingControls(player.Owner, false);
                }
                foreach (GamePlayer player in blockingTeam.teamPlayers)
                {
                    player.RpcActivateKickAfterBlockingControls(player.Owner, false);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("DisableKickAfterAttemptControls: failed for reason: " + e);
        }
        
    }
    IEnumerator TransitionFromKickAfterAttemptToKickOffRoutine()
    {
        kickAfterRoutineRunning = true;
        yield return new WaitForSeconds(3.0f);        
        RpcDisableKickAfterWasKickGoodOrBadUI();
        if (this.mercyRuleEnabled && CheckForMercyRule())
        {
            gamePhase = "gameover";
        }
        else if (isXtraTime && firstHalfCompleted)
        {
            GameplayManager.instance.gamePhase = "gameover";
        }
        else if (isXtraTime && !firstHalfCompleted)
        {
            Debug.Log("TransitionFromKickAfterAttemptToKickOffRoutine: it should be halftime now!");
            GameplayManager.instance.gamePhase = "halftime";
        }
        else
        {
            Debug.Log("TransitionFromKickAfterAttemptToKickOffRoutine: not halftime. Kickoff time!");
            GameplayManager.instance.gamePhase = "kickoff";
        }
        kickAfterRoutineRunning = false;        
    }
    [ObserversRpc]
    void RpcDisableKickAfterWasKickGoodOrBadUI()
    {
        KickAfterWasKickGoodPanel.SetActive(false);
        TheKickWasText.gameObject.SetActive(false);
        KickWasNotGoodText.gameObject.SetActive(false);
        KickWasGoodText.gameObject.SetActive(false);
        KickWasBlockedText.gameObject.SetActive(false);
        KickWasGoodText.GetComponent<TouchDownTextGradient>().isColorChangeRunning = false;
    }
    [Client]
    void ResetKickAfterPositionBoolValue()
    {
        LocalGamePlayerScript.areGoblinsRepositionedForKickAfter = false;
        foreach (GamePlayer aiPlayer in Game.GamePlayers)
        {
            if (aiPlayer != null && aiPlayer.isAIPlayer)
                aiPlayer.areGoblinsRepositionedForKickAfter = false;
        }
    }
    [Server]
    public void KickAfterAttemptWasBlocked()
    {
        if (_wasKickGoodOrBadSubmitted)
        {
            Debug.Log("KickAfterAttemptWasBlocked: _wasKickGoodOrBadSubmitted was already set. Returning...");
            return;
        }
        TransitionFromKickAfterAttemptToKickOff();
        DisableKickAfterPositioningControls();
        DisableKickAfterAttemptControls();
        RpcKickAfterAttemptWasBlocked();
        if (this.is1v1)
        {
            TeamManager.instance.BlockedKick(blockingPlayer.isTeamGrey);
            TeamManager.instance.KickAfterAttempts(scoringPlayer.isTeamGrey, false);
        }
        else if (this.isSinglePlayer && !this.is1v1)
        {
            TeamManager.instance.BlockedKick(blockingPlayer.isTeamGrey);
            TeamManager.instance.KickAfterAttempts(scoringPlayer.isTeamGrey, false);
            AIStopBlockingKickAfter();
        }
        else
        {
            TeamManager.instance.BlockedKick(blockingTeam.isGrey);
            TeamManager.instance.KickAfterAttempts(scoringTeam.isGrey, false);
        }
        if (!this.isSinglePlayer)
        {
            this.StopTimeoutKickAfterRoutine();
        }

    }
    [ObserversRpc]
    void RpcKickAfterAttemptWasBlocked()
    {
        // incase there is an issue with the kick was blocked/kick was good thing happening at the same time:
        KickAfterWasKickGoodPanel.SetActive(false);
        TheKickWasText.gameObject.SetActive(false);
        KickWasNotGoodText.gameObject.SetActive(false);
        KickWasNotGoodText.gameObject.SetActive(false);
        SoundManager.instance.StopSound("kick-its-good");
        SoundManager.instance.StopSound("kick-no-good");

        // now display the kick was blocked stuff
        KickAfterPositionControlsPanel.SetActive(false);
        KickAfterTimerBeforeKickPanel.SetActive(false);
        KickAfterWasKickGoodPanel.SetActive(true);
        TheKickWasText.gameObject.SetActive(true);
        KickWasBlockedText.gameObject.SetActive(true);
        SoundManager.instance.PlaySound("kick-blocked", 1.0f);
        SoundManager.instance.PlaySound("touchdown-cheer", 0.75f);
    }
    [Server]
    void DetermineWinnerOfGame()
    {
        // A team with no players left loses whatever the score was, so quitting while behind can't avoid the loss.
        if (forfeitWinner != "")
        {
            winningTeam = forfeitWinner;
            RpcWinnerOfGameDetermined(winningTeam, true);
            return;
        }
        if (greenScore > greyScore)
        {
            winningTeam = "green";
            /*if (!gameWinnersNotified)
            {
                TeamManager.instance.greenTeam.TellTeamMembersIfTheyWonOrLost(true);
                TeamManager.instance.greyTeam.TellTeamMembersIfTheyWonOrLost(false);
            }
            gameWinnersNotified = true;*/
            
        }
        else if (greenScore < greyScore)
        {
            winningTeam = "grey";
            /*if (!gameWinnersNotified)
            {
                TeamManager.instance.greenTeam.TellTeamMembersIfTheyWonOrLost(false);
                TeamManager.instance.greyTeam.TellTeamMembersIfTheyWonOrLost(true);
            }
            gameWinnersNotified = true;*/
        }
        else
        {
            winningTeam = "draw";
            //gameWinnersNotified = true;
        }
        RpcWinnerOfGameDetermined(winningTeam, false);
    }
    [ObserversRpc]
    void RpcWinnerOfGameDetermined(string winnerOfGame, bool wasForfeit)
    {
        Debug.Log("RpcWinnerOfGameDetermined: " + winnerOfGame + ", by forfeit? " + wasForfeit.ToString());
        theFinalScoreText.text = "THE FINAL SCORE";
        greenTeamScoreText.text = greenScore.ToString();
        greyTeamScoreText.text = greyScore.ToString();
        GameOverScorePanel.SetActive(true);
        
        if (winnerOfGame == "green")
        {
            teamWinnerText.text = "GREEN WINS!!!";
            //teamWinnerText.GetComponent<TouchDownTextGradient>().ActivateGradient();
            if (!gameWinnersNotified)
            {
                // A win by forfeit doesn't count toward achievements, so they can't be farmed with a second account.
                if (!wasForfeit)
                    LocalGamePlayerScript.WinnerOfGameSteamAchievementChecks(winnerOfGame);
                gameWinnersNotified = true;
            }
        }
        else if (winnerOfGame == "grey")
        {
            teamWinnerText.text = "GREY WINS!!!";
            //teamWinnerText.GetComponent<TouchDownTextGradient>().ActivateGradient();
            if (!gameWinnersNotified)
            {
                if (!wasForfeit)
                    LocalGamePlayerScript.WinnerOfGameSteamAchievementChecks(winnerOfGame);
                gameWinnersNotified = true;
            }
        }
        else if (winnerOfGame == "draw")
        {
            teamWinnerText.text = "IT'S A DRAW...";
            teamWinnerText.GetComponent<TouchDownTextGradient>().SetGreenOrGreyColor(true);
            gameWinnersNotified = true;
        }
        GameOverScrollPanelObject.GetComponent<ImageAnimation>().UnScrollEndOfGame(winnerOfGame);
    }
    [Server]
    public void DidWinningGoblinDiveInXtraTimeToEndGame(bool isGoblinGrey)
    {
        if ((isGoblinGrey && greyScore > greenScore) || (!isGoblinGrey && greyScore < greenScore))
        {
            gamePhase = "gameover";
        }

    }
    [Server]
    void ResetGoblinKickAfterRespositioningBool()
    {
        foreach (GamePlayer player in Game.GamePlayers)
        {
            foreach (GoblinScript goblin in player.goblinTeamOnServer)
            {
                goblin.hasGoblinBeenRepositionedForKickAfter = false;
            }
        }
    }
    // Check to see if all goblins have be repositioned by the clients. If they have, wait 0.25f seconds and then check to make sure that the scoring goblin has the football
    [Server]
    public void AreAllGoblinsRepositionedForKickAfter()
    {
        bool doneRepositioning = true;
        foreach (GamePlayer player in Game.GamePlayers)
        {
            if (doneRepositioning)
            {
                foreach (GoblinScript goblin in player.goblinTeamOnServer)
                {
                    if (!goblin.hasGoblinBeenRepositionedForKickAfter)
                    {
                        doneRepositioning = false;
                        break;
                    }
                }
            }
        }
        if (doneRepositioning)
        {
            Debug.Log("AreAllGoblinsRepositionedForKickAfter: All goblins have hasGoblinBeenRepositionedForKickAfter set to true. Continuing with kickafter");
            IEnumerator waitForRepositioningSyncUp = WaitForRepositioningSyncUp();
            StartCoroutine(waitForRepositioningSyncUp);
        }
        else
        {
            Debug.Log("AreAllGoblinsRepositionedForKickAfter: NOT all goblins have hasGoblinBeenRepositionedForKickAfter set to true. Waiting for kick after setup to finish...");
        }
    }
    IEnumerator WaitForRepositioningSyncUp()
    {
        yield return new WaitForSeconds(0.25f);
        GoblinScript scoringGoblinScript = ObjectLookup.Get(scoringGoblinNetId).GetComponent<GoblinScript>();
        if (!scoringGoblinScript.doesCharacterHaveBall)
        {
            Football football = GameObject.FindGameObjectWithTag("football").GetComponent<Football>();
            football.MoveFootballToKickoffGoblin(scoringGoblinNetId);
        }
        if (this.isSinglePlayer && !this.is1v1)
        {
            AIStartKickAfterPositioning();
        }
        if (!this.isSinglePlayer)
        {
            timeoutKickAfter = TimeoutKickAfterRoutine();
            StartCoroutine(timeoutKickAfter);
        }
    }
    [Client]
    public void UpdatePossessionOfFootballtoTeam(bool doesGreyTeamHaveBall)
    {
        Debug.Log("UpdatePossessionOfFootballtoTeam: Does grey team have the ball? " + doesGreyTeamHaveBall.ToString());
        if (doesGreyTeamHaveBall)
        {
            PossessionFootballGrey.SetActive(true);
            PossessionFootballGreen.SetActive(false);
        }
        else
        {
            PossessionFootballGrey.SetActive(false);
            PossessionFootballGreen.SetActive(true);
        }
    }
    [ObserversRpc]
    public void RpcNoTeamWithFootball()
    {
        NoTeamWithFootball();
    }
    public void NoTeamWithFootball()
    {
        PossessionFootballGrey.SetActive(false);
        PossessionFootballGreen.SetActive(false);
    }
    void UpdateKickAfterControlsUIForGamepad(bool usingGamepad)
    {
        if (usingGamepad)
        {
            MoveLeftText.text = "Lb to Move Left";
            MoveRightText.text = "Rb to Move Right";
            EnterToSubmitText.text = "A to Submit";
        }
        else
        {
            MoveLeftText.text = "<- to Move Left";
            MoveRightText.text = "-> to Move Right";
            EnterToSubmitText.text = "\"Enter\" to Submit";
        }
    }
    [Server]
    void EnableGoblinCollidersOnServer(bool enable)
    {
        GameObject[] goblinObjects = GameObject.FindGameObjectsWithTag("Goblin");
        if (goblinObjects.Length > 0)
        {
            foreach (GameObject goblinObject in goblinObjects)
            {
                goblinObject.GetComponent<GoblinScript>().canCollide = enable;
            }
        }
    }
    [Server]
    void DisableGoblinColliderForKickAfter()
    {
        GameObject[] goblinObjects = GameObject.FindGameObjectsWithTag("Goblin");
        if (goblinObjects.Length > 0)
        {
            foreach (GameObject goblinObject in goblinObjects)
            {
                goblinObject.GetComponent<GoblinScript>().KickAfterWaitToEnableObstacleColliders();
            }
        }
    }
    [Server]
    public void SaveFirstHalfKickingTeam()
    {
        Debug.Log("SaveFirstHalfKickingTeam: team that is kicking is: " + kickingPlayer.teamName);
        if (!String.IsNullOrWhiteSpace(kickingPlayer.teamName))
        {
            firstHalfKickingTeam = kickingPlayer.teamName;
        }

    }
    [Server]
    void TransitionToSecondHalf()
    {
        GetSecondHalfKickingTeam();
        ResetAllGoblinStatuses();
        IEnumerator transitionToSecondHalfRoutine = TransitionToSecondHalfRoutine();
        StartCoroutine(transitionToSecondHalfRoutine);
    }
    [Server]
    void GetSecondHalfKickingTeam()
    {
        Debug.Log("GetSecondHalfKickingTeam");
        if (IsTeamMissingItsCaptain(TeamManager.instance.greenTeam) || IsTeamMissingItsCaptain(TeamManager.instance.greyTeam))
            return;

        /*foreach (GamePlayer player in Game.GamePlayers)
        {
            if (player.teamName == firstHalfKickingTeam)
            {
                receivingPlayer = player;
            }
            else
            {
                kickingPlayer = player;
            }
        }*/
        if (firstHalfKickingTeam.ToLower().Contains("green"))
        {
            receivingTeam = TeamManager.instance.greenTeam;
            receivingPlayer = receivingTeam.captain;
            kickingTeam = TeamManager.instance.greyTeam;
            kickingPlayer = kickingTeam.captain;
        }
        else
        {
            receivingTeam = TeamManager.instance.greyTeam;
            receivingPlayer = receivingTeam.captain;
            kickingTeam = TeamManager.instance.greenTeam;
            kickingPlayer = kickingTeam.captain;
        }
    }
    [Server]
    IEnumerator TransitionToSecondHalfRoutine()
    {
        RpcHalfTimeTransition(true, kickingPlayer.teamName);
        yield return new WaitForSeconds(7f);
        RpcHalfTimeTransition(false, kickingPlayer.teamName);
        yield return new WaitForSeconds(1f);
        this.firstHalfCompleted = true;
        ResetAllGoblinStatuses();
        this.gamePhase = "kickoff";
        this.timeLeftInGame = secondsPerHalf;
    }
    [ObserversRpc]
    void RpcHalfTimeTransition(bool enable, string kickingTeam)
    {
        Debug.Log("RpcHalfTimeTransition: enable? " + enable.ToString());
        theFinalScoreText.text = "End of First Half. " + kickingTeam + " will kick to start the Second Half.";
        greenTeamScoreText.text = greenScore.ToString();
        greyTeamScoreText.text = greyScore.ToString();
        if(enable)
            GameOverScorePanel.SetActive(enable);
        if(enable)
            GameOverScrollPanelObject.GetComponent<ImageAnimation>().UnScrollHalfTime();
        else
            GameOverScrollPanelObject.GetComponent<ImageAnimation>().ReRollScroll();

        // sounds stuff?
        if (enable)
        {
            SoundManager.instance.PlaySound("ref-whistle", 1.0f);
        }
    }
    [Server]
    void StopPossessionRoutinesForPlayers()
    {
        Debug.Log("StopPossessionRoutinesForPlayers");
        /*foreach (GamePlayer player in Game.GamePlayers)
        {
            player.StopAllPossessionRoutines();
        }*/
        TeamManager.instance.greenTeam.StopAllPossessionRoutines();
        TeamManager.instance.greyTeam.StopAllPossessionRoutines();
    }
    [Client]
    public void UpdatePossessionBar(bool isGreen, float possession)
    {
        if (isGreen)
            PossessionBarGreen.GetComponent<PossessionBar>().UpdatePossessionBar(possession);
        else
            PossessionBarGrey.GetComponent<PossessionBar>().UpdatePossessionBar(possession);
    }
    [Client]
    public void PlayPossessionChangedSFX(bool doesGreyTeamHaveBall)
    {
        if (LocalGamePlayerScript)
        {
            if (LocalGamePlayerScript.isTeamGrey == doesGreyTeamHaveBall)
            {
                Debug.Log("PlayPossessionChangedSFX: team GAINED possession of the ball.");
                SoundManager.instance.PlaySound("gain-possession", 1.0f);
            }
            else
            {
                Debug.Log("PlayPossessionChangedSFX: team LOST possession of the ball.");
                SoundManager.instance.PlaySound("lose-possession", 1.0f);
            }
        }
    }
    public void MainMenuButton()
    {
        LocalGamePlayerScript.ExitToMainMenu();
    }
    public void ExitToDesktopButton()
    {
        Application.Quit();
    }
    // The bot whose hook fired passes itself in, so this works whether it is single player's one AI or a 3v3
    // team's bot captain.
    public void AIChoosesCoinToss(GamePlayer chooser)
    {
        if (chooser == null)
            return;
        Debug.Log("AIChoosesCoinToss: The AI will choose the coin toss, not the player");
        IEnumerator aiWaitToChooseCoin = AIWaitToChooseCoin(chooser);
        StartCoroutine(aiWaitToChooseCoin);
    }
    IEnumerator AIWaitToChooseCoin(GamePlayer chooser)
    {
        yield return new WaitForSeconds(2.0f);
        string[] headsTails = new[]
        { "heads","tails"};
        var rng = new System.Random();
        chooser.headsOrTailsPlayer = headsTails[rng.Next(headsTails.Length)];
        chooser.SubmitCoinSelection();
    }
    public void AIChoosesKickOrReceive(GamePlayer chooser)
    {
        if (chooser == null)
            return;
        Debug.Log("AIChoosesKickOrReceive: The AI will choose to kick or receive, not the player");
        IEnumerator aiWaitToChooseKickOrReceive = AIWaitToChooseKickOrReceive(chooser);
        StartCoroutine(aiWaitToChooseKickOrReceive);
    }
    IEnumerator AIWaitToChooseKickOrReceive(GamePlayer chooser)
    {
        yield return new WaitForSeconds(5.0f);
        string[] headsTails = new[]
        { "kick","receive"};
        var rng = new System.Random();
        chooser.kickOrReceivePlayer = headsTails[rng.Next(headsTails.Length)];
        chooser.SubmitKickOrReceiveSelection();
    }
    public void AIPlayerKickOff(GamePlayer kickingAIPlayer)
    {
        Debug.Log("AIPlayerKickOff");
        if (kickingAIPlayer == null || kickingAIPlayer.myAIPlayer == null)
            return;
        IEnumerator aiKickOffSequence = AIKickOffSequence(kickingAIPlayer.myAIPlayer);
        StartCoroutine(aiKickOffSequence);
    }
    IEnumerator AIKickOffSequence(AIPlayer kicker)
    {
        yield return new WaitForSeconds(3.0f);
        kicker.KickOffSequence();
    }
    public void AIStartKickAfterPositioning()
    {
        Debug.Log("AIStartKickAfterPositioning");
        if (scoringPlayer != null && scoringPlayer.isAIPlayer)
        {
            Debug.Log("AIStartKickAfterPositioning: The AI will be doing the kick after attempt. Starting on choosing position for kick");
        }
    }
    void AIBlockingPlayer()
    {
        Debug.Log("AIBlockingPlayer");
        IEnumerator aiBlockKickDelay = AIBlockKickDelay();
        StartCoroutine(aiBlockKickDelay);
    }
    // AIPlayer.BlockKickAfterAttempt looks through its own goblins for the skirmisher, because in single player
    // and a 1v1 one player owns all three. In a 3v3 each player owns one, and the blocking player is the team
    // captain, so it searched a single Grenadier, found no skirmisher and nothing blocked the kick. Ask the team
    // who actually has the skirmisher.
    // The AI that was told to block this kick-after. BlockingSkirmisherAI resolves from the current blockingTeam,
    // which is the other team by the next kick-after, so the stop has to clear the one that was actually set.
    private AIPlayer _blockingAI;
    AIPlayer BlockingSkirmisherAI()
    {
        if (blockingTeam != null)
        {
            foreach (GamePlayer player in blockingTeam.teamPlayers)
            {
                if (player == null || !player.isAIPlayer || player.myAIPlayer == null)
                    continue;
                foreach (GoblinScript goblin in player.goblinTeam)
                {
                    if (goblin != null && goblin.soundType == "skirmisher")
                        return player.myAIPlayer;
                }
            }
        }
        // The blocking player owns all three goblins in a 1v1 or single player, so it is the blocker there,
        // but only if it really has the skirmisher. In a 3v3 it owns the Grenadier alone, and arming it sends
        // nothing at the kicker, since AIPlayer.BlockKickAfterAttempt only ever moves a skirmisher.
        if (blockingPlayer != null && blockingPlayer.isAIPlayer && blockingPlayer.myAIPlayer != null)
        {
            foreach (GoblinScript goblin in blockingPlayer.goblinTeam)
            {
                if (goblin != null && goblin.soundType == "skirmisher")
                    return blockingPlayer.myAIPlayer;
            }
        }
        return null;
    }
    IEnumerator AIBlockKickDelay()
    {
        _blockingAI = BlockingSkirmisherAI();
        yield return new WaitForSeconds(0.25f);
        if (_blockingAI != null)
            _blockingAI.blockKick = true;
    }
    void AIStopBlockingKickAfter()
    {
        if (_blockingAI == null)
            return;
        _blockingAI.blockKick = false;
        _blockingAI.StopBlockingGoblinFromRunning();
        _blockingAI = null;
    }
    // Belt and braces for the above: a blockKick left set from a previous kick-after sends that skirmisher at the
    // kicker the moment the next one begins, before the kicker has even chosen a position, and blocks it.
    void ClearAllAIBlockKick()
    {
        _blockingAI = null;
        foreach (GamePlayer player in Game.GamePlayers)
        {
            if (player != null && player.isAIPlayer && player.myAIPlayer != null)
            {
                player.myAIPlayer.blockKick = false;
                player.myAIPlayer.StopBlockingGoblinFromRunning();
            }
        }
    }
    void AIKickAfterKickingPlayer()
    {
        Debug.Log("AIKickAfterKickingPlayer");
        IEnumerator aiKickAfterKickingPlayerDelay = AIKickAfterKickingPlayerDelay();
        StartCoroutine(aiKickAfterKickingPlayerDelay);
    }
    IEnumerator AIKickAfterKickingPlayerDelay()
    {
        AIPlayer kicker = scoringPlayer != null ? scoringPlayer.myAIPlayer : null;
        if (kicker == null)
            yield break;
        // The goblin that kicks is the one the reposition put on the spot, which is the kicking player's own selected
        // goblin. This used to look the goblin up from scoringGoblinNetId, which in a 3v3 handed the AI a different
        // goblin altogether - observed as a grenadier at midfield while the skirmisher stood on the kick spot - so the
        // AI walked and kicked with the wrong one and the ball came in from across the pitch.
        kicker.kickAfterGoblin = scoringPlayer.selectGoblin;
        if (kicker.kickAfterGoblin == null)
            yield break;
        yield return new WaitForSeconds(1.0f);
        Debug.Log("AIKickAfterKickingPlayerDelay");
        kicker.finalPositionReached = false;
        kicker.kickKickAfter = true;
        
        
    }
    void AIKickAfterAttempt()
    {
        Debug.Log("AIKickAfterAttempt");
        if (scoringPlayer != null && scoringPlayer.myAIPlayer != null)
            scoringPlayer.myAIPlayer.KickAfterAttempt();
    }
    [ObserversRpc]
    void RpcKickOffWhistle()
    {
        SoundManager.instance.PlaySound("ref-whistle", 1.0f);
    }
    [Server]
    public void PauseGameOnServer(GamePlayer pausingPlayer, int pausingPlayerNetId)
    {
        if (this.isGamePaused)
            return;
        if (this.gamePhase == "gameover")
            return;
        Debug.Log("PauseGameOnServer: Player with a netid of: " + pausingPlayerNetId.ToString() + " wants to PAUSE the game.");

        // prevent pause spamming by making it so a team can only pause once every 30 seconds of game time. Ignore for singleplayer.
        if (!this.isSinglePlayer)
        {
            bool canPlayerPause = false;
            if (pausingPlayer.isTeamGrey)
            {
                if (Time.time > greyPauseTimeStamp + 30f)
                {
                    canPlayerPause = true;
                    greyPauseTimeStamp = Time.time;
                }
                else
                {
                    Debug.Log("PauseGameOnServer: GREY Player/team has tried to pause too recently after they already paused. Current time: " + Time.time.ToString() + " and last pause time: " + greyPauseTimeStamp.ToString());
                }
            }
            else
            {
                if (Time.time > greenPauseTimeStamp + 30f)
                {
                    canPlayerPause = true;
                    greenPauseTimeStamp = Time.time;
                }
                else
                {
                    Debug.Log("PauseGameOnServer: GREEN Player/team has tried to pause too recently after they already paused. Current time: " + Time.time.ToString() + " and last pause time: " + greenPauseTimeStamp.ToString());
                }
            }
            if (!canPlayerPause)
            {
                //SendErrorMessageToTeam(pausingPlayer.isTeamGrey, "Team must wait 30 seconds of game time to pause again.");
                pausingPlayer.RpcErrorMessageToDisplayFromServer(pausingPlayer.Owner, "Your team must wait 30 seconds of game time to pause again.");
                return;
            }
        }

        playerWhoPaused = pausingPlayer;
        playerWhoPausedNetId = pausingPlayerNetId;
        PauseGameForAllPlayers(true);
        this.isGamePaused = true;
        if (!this.isSinglePlayer)
        {
            GamePausedTimeout = PauseGameTimeoutRoutine();
            StartCoroutine(GamePausedTimeout);
        }
        //Time.timeScale = 0f;
    }
    [Server]
    public void ResumeGameOnServer(GamePlayer pausingPlayer, int pausingPlayerNetId)
    {
        if (!this.isGamePaused)
            return;
        Debug.Log("ResumeGameOnServer: Player with a netid of: " + pausingPlayerNetId.ToString() + " wants to RESUME the game.");
        if (pausingPlayerNetId == this.playerWhoPausedNetId)
        {
            Debug.Log("ResumeGameOnServer: Player with a netid of: " + pausingPlayerNetId.ToString() + " matches netid of player who paused: " + this.playerWhoPausedNetId.ToString() + " will resume game.");
            playerWhoPaused = null;
            playerWhoPausedNetId = ObjectLookup.NoObject;
            PauseGameForAllPlayers(false);
            this.isGamePaused = false;
            if (isGamePausedTimeoutRoutineRunning)
            {
                StopCoroutine(GamePausedTimeout);
                isGamePausedTimeoutRoutineRunning = false;
            }
            //Time.timeScale = 1.0f;
        }
        else
        {
            if (!this.isSinglePlayer)
                pausingPlayer.RpcErrorMessageToDisplayFromServer(pausingPlayer.Owner, "Only the player who paused the game can resume the game.");
        }
    }
    public void HandleIsGamePaused(bool oldValue, bool newValue)
    {
        if (IsClientInitialized)
        {
            //GamePausedTextObject.SetActive(newValue);
            /*if (newValue)
            {
                Debug.Log("HandleIsGamePaused: time scale on client set to 0");
                Time.timeScale = 0f;
            }
            else
            {
                Debug.Log("HandleIsGamePaused: time scale on client set to 1");
                Time.timeScale = 1f;
            }*/   
        }
    }
    [Server]
    void PauseGameForAllPlayers(bool pauseGame)
    {
        foreach (GamePlayer player in Game.GamePlayers)
        {
            player.RpcGamePausedByServer(player.Owner, pauseGame);
        }
    }
    public void ActivateGamePausedText(bool activate)
    {
        GamePausedTextObject.SetActive(activate);
        if (!this.isSinglePlayer)
        {
            if (activate)
            {
                GamePausedSecondsLeftText.text = "Resuming in 90 seconds...";
            }
            GamePausedSecondsLeftText.gameObject.SetActive(activate);
        }
        else
        {
            GamePausedSecondsLeftText.gameObject.SetActive(false);
        }
    }
    [Server]
    IEnumerator PauseGameTimeoutRoutine()
    {
        isGamePausedTimeoutRoutineRunning = true;
        //yield return new WaitForSecondsRealtime(90f);
        int secondsRemainingForPause = 90;
        RpcUpdateGamePausedSecondsLeftText(secondsRemainingForPause);
        while (secondsRemainingForPause > 0)
        {
            yield return new WaitForSecondsRealtime(1f);
            secondsRemainingForPause--;
            RpcUpdateGamePausedSecondsLeftText(secondsRemainingForPause);
        }
        if (this.isGamePaused)
        {
            Debug.Log("PauseGameTimeoutRoutine: Reached paused game timeout");
            playerWhoPaused = null;
            playerWhoPausedNetId = ObjectLookup.NoObject;
            PauseGameForAllPlayers(false);
            this.isGamePaused = false;
            isGamePausedTimeoutRoutineRunning = false;
        }
        yield break;
    }
    [ObserversRpc]
    public void RpcUpdateGamePausedSecondsLeftText(int secondsLeft)
    {
        if (!this.isSinglePlayer)
        {
            GamePausedSecondsLeftText.text = "Resuming in " + secondsLeft.ToString() + " seconds...";
        }
    }
    IEnumerator TimeoutKickAfterRoutine()
    {
        isKickAfterTimerRunning = true;
        
        this.kickAfterTimerTimeLeft = 30;
        RpcActivateTheTimeoutTimer(true);
        int timeLeftTracker;
        while (isKickAfterTimerRunning)
        {
            // Don't count down the time if the game is paused. Wait a frame first, as in GameTimerCountdown.
            if (this.isGamePaused)
            {
                yield return null;
                continue;
            }
            if (this.gamePhase != "kick-after-attempt")
            {
                isKickAfterTimerRunning = false;
                break;
            }
            yield return new WaitForSeconds(1.0f);
            if (isKickAfterTimerCountDownRunning)
                continue;
            timeLeftTracker = kickAfterTimerTimeLeft - 1;

            kickAfterTimerTimeLeft = timeLeftTracker;
            if (timeLeftTracker <= 0)
            {
                kickAfterTimerTimeLeft = 0;
                isKickAfterTimerRunning = false;
                // code to block kick automatically?
                try
                {
                    GoblinScript goblin = ObjectLookup.Get(scoringGoblinNetId).GetComponent<GoblinScript>();
                    goblin.KnockOutGoblin(false);
                }
                catch (Exception e)
                {
                    Debug.Log("TimeoutKickAfterRoutine: could not access the scoringGoblinNetId. Error: " + e);
                }
                
                //this.KickAfterAttemptWasBlocked();
                RpcActivateTheTimeoutTimer(false);
            }
        }
        RpcActivateTheTimeoutTimer(false);
        yield break;
    }
    void HandleKickAfterTimerTimeLeft(int oldValue, int newValue)
    {
        if (IsClientInitialized)
        {
            timeoutTimerText.text = ":" + newValue.ToString();
        }
    }
    [ObserversRpc]
    void RpcActivateTheTimeoutTimer(bool activate)
    {
        TimeoutTimerHolder.SetActive(activate);
    }
    [Server]
    public void StopTimeoutKickAfterRoutine()
    {
        if (isKickAfterTimerRunning)
            isKickAfterTimerRunning = false;
        try
        {
            StopCoroutine(timeoutKickAfter);
        }
        catch (Exception e)
        {
            Debug.Log("StopTimeoutKickAfterRoutine: Could not stop coroutine. Error: " + e);
        }
        
        RpcActivateTheTimeoutTimer(false);
    }
    IEnumerator TimeoutKickOffRoutine()
    {
        isKickOffTimerCountDownRunning = true;

        this.kickOffTimerTimeLeft = 30;
        RpcActivateTheTimeoutTimer(true);
        int timeLeftTracker;
        while (isKickOffTimerCountDownRunning)
        {
            if (this.gamePhase != "kickoff")
            {
                isKickOffTimerCountDownRunning = false;
                break;
            }
            yield return new WaitForSeconds(1.0f);
            // Don't count down the time if the game is paused?
            if (this.isGamePaused)
                continue;

            timeLeftTracker = kickOffTimerTimeLeft - 1;

            kickOffTimerTimeLeft = timeLeftTracker;
            if (timeLeftTracker <= 0)
            {
                kickOffTimerTimeLeft = 0;
                isKickOffTimerCountDownRunning = false;
                // code to block kick automatically?
                try
                {
                    kickingPlayer.serverSelectGoblin.RpcKickOffTimeoutForceKick(kickingPlayer.serverSelectGoblin.Owner);
                }
                catch (Exception e)
                {
                    Debug.Log("TimeoutKickAfterRoutine: could not access the scoringGoblinNetId. Error: " + e);
                }

                //this.KickAfterAttemptWasBlocked();
                RpcActivateTheTimeoutTimer(false);
            }
        }
        RpcActivateTheTimeoutTimer(false);
        yield break;
    }
    void HandleKickOffTimerTimeLeft(int oldValue, int newValue)
    {
        if (IsClientInitialized)
        {
            timeoutTimerText.text = ":" + newValue.ToString();
        }
    }
    [Server]
    public void StopTimeoutKickOffRoutine()
    {
        if (isKickOffTimerCountDownRunning)
            isKickOffTimerCountDownRunning = false;
        try
        {
            StopCoroutine(timeoutKickOff);
        }
        catch (Exception e)
        {
            Debug.Log("StopTimeoutKickOffRoutine: Could not stop coroutine. Error: " + e);
        }

        RpcActivateTheTimeoutTimer(false);
    }
    [Server]
    void SendErrorMessageToTeam(bool isTeamGrey, string message)
    {
        if (isTeamGrey)
        {
            foreach (GamePlayer player in TeamManager.instance.greyTeam.teamPlayers)
            {
                player.RpcErrorMessageToDisplayFromServer(player.Owner, message);
            }
        }
        else
        {
            foreach (GamePlayer player in TeamManager.instance.greenTeam.teamPlayers)
            {
                player.RpcErrorMessageToDisplayFromServer(player.Owner, message);
            }
        }
    }
    [Client]
    public void DisplayErrorMessage(string message)
    {
        if (isErrorMessageDisplayed)
        {
            try
            {
                StopCoroutine(displayErrorMessageFromServer);
                errorCanvas.SetActive(false);
                errorCanvasText.text = "";
            }
            catch (Exception e)
            {
                Debug.Log("DisplayErrorMessage: could not stop coroutine and/or access the messageCanvas. Error: " + e);
            }
        }
        displayErrorMessageFromServer = DisplayErrorMessageFromServer(message);
        StartCoroutine(displayErrorMessageFromServer);
    }
    [Client]
    IEnumerator DisplayErrorMessageFromServer(string message)
    {
        Debug.Log("DisplayErrorMessageFromServer");
        errorCanvasText.text = message;
        errorCanvas.SetActive(true);
        isErrorMessageDisplayed = true;
        yield return new WaitForSecondsRealtime(5f);
        errorCanvas.SetActive(false);
        errorCanvasText.text = "";
        isErrorMessageDisplayed = false;
    }
    // Called from GamePlayer.OnStopServer once the last player on a team has left, and from IsTeamMissingItsCaptain.
    // That team can't kick off or take a kick-after, since both read team.captain, so the match ends and the other
    // team takes the win. This is the one place that decides what an abandoned team means: when the "forfeit, or let
    // an AI take over?" prompt is added, it replaces the body of this method and nothing else has to change.
    [Server]
    public void HandleTeamAbandoned(bool isTeamGrey)
    {
        if (this.gamePhase == "gameover" || forfeitWinner != "")
            return;
        Debug.Log("HandleTeamAbandoned: the " + (isTeamGrey ? "grey" : "green") + " team has no players left. Ending the game.");
        forfeitWinner = isTeamGrey ? "green" : "grey";
        SendErrorMessageToTeam(!isTeamGrey, "Your opponent left the game.");
        this.gamePhase = "gameover";
    }
    // team.captain is null only if the captain left and no teammate was there to take over, which PromoteNewCaptain
    // normally handles. Both kickoff paths read it, so check before reading rather than throwing on the next line.
    [Server]
    bool IsTeamMissingItsCaptain(Team team)
    {
        if (team == null || team.captain != null)
            return false;
        Debug.Log("IsTeamMissingItsCaptain: the " + (team.isGrey ? "grey" : "green") + " team has no captain left.");
        HandleTeamAbandoned(team.isGrey);
        return true;
    }
    [Server]
    bool CheckForMercyRule()
    {
        bool beMerciful = false;
        
        if (!this.mercyRuleEnabled)
            return beMerciful;

        if (this.mercyRulePointDifferential < 21)
            return beMerciful;

        int pointDiff = Mathf.Abs(greenScore - greyScore);
        if (pointDiff >= mercyRulePointDifferential)
        {
            //this.HandleGamePhase(this.gamePhase, "gameover");
            Debug.Log("CheckForMercyRule: Point differential is: " + pointDiff.ToString() + " ending game due to mercy rule...");
            this.SendErrorMessageToTeam(true, "Game ended due to mercy rule.");
            this.SendErrorMessageToTeam(false, "Game ended due to mercy rule.");
            beMerciful = true;
        }
        else
            beMerciful = false;
        return beMerciful;
    }
    public void EnableGamepadUIFromSettingsMenu(bool enableGamepadUI)
    {
        Debug.Log("EnableGamepadUIFromSettingsMenu: Enable gamepad UI elements? " + enableGamepadUI.ToString());
        GamepadUIManager.instance.gamepadUI = enableGamepadUI;

        // Update Camera marker UI
        Camera.main.GetComponent<CameraMarker>().SetGamepadUIStuff(enableGamepadUI);

        // Update markers on goblins
        try
        {
            if (this.is1v1 || this.isSinglePlayer)
            {
                foreach (GoblinScript goblin in LocalGamePlayerScript.goblinTeam)
                {
                    goblin.UpdateGamePadUIMarkersForGoblins(enableGamepadUI);
                }
            }
            else
            {
                Team teamToIterate;
                if (LocalGamePlayerScript.isTeamGrey)
                    teamToIterate = TeamManager.instance.greyTeam;
                else
                    teamToIterate = TeamManager.instance.greenTeam;

                if (teamToIterate != null)
                {
                    foreach (int goblinNetId in teamToIterate.goblinNetIds)
                    {
                        ObjectLookup.Get(goblinNetId).GetComponent<GoblinScript>().UpdateGamePadUIMarkersForGoblins(enableGamepadUI);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("EnableGamepadUIFromSettingsMenu: Failed to access local player script or something like that. Error: " + e);
        }

        // Reset the power up images
        PowerUpManager.instance.UpdatePowerUpBoardUIForGamepad(enableGamepadUI);

        // Update UI text:
        this.UpdateKickAfterControlsUIForGamepad(enableGamepadUI);
        CoinTossManager.instance.UpdateUIForGamepad(enableGamepadUI);
        if (this.gamePhase == "kick-after-attempt")
        {
            try
            {
                this.KickAfterUpdateInsctructionsText(LocalGamePlayerScript.localIsKickingPlayer);
            }
            catch (Exception e)
            {
                Debug.Log("EnableGamepadUIFromSettingsMenu: Could not access local player script for whatever reason. Error: " + e);
            }
            
        }
    }
}
