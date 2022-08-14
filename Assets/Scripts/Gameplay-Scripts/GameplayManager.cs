using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
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
    [SyncVar] public bool is1v1 = false;
    [SyncVar] public bool isSinglePlayer = false;
    [SyncVar] public int secondsPerHalf;
    [SyncVar] public bool powerUpsEnabled;
    [SyncVar] public bool randomEventsEnabled;
    [SyncVar] public bool spawnObstaclesEnabled;
    [SyncVar] public bool mercyRuleEnabled;
    [SyncVar] public int mercyRulePointDifferential;

    [Header("Local GamePlayers")]
    [SerializeField] private GameObject LocalGamePlayer;
    [SerializeField] private GamePlayer LocalGamePlayerScript;

    [Header("UI Canvases")]
    [SerializeField] private GameObject gameplayCanvas;
    [SerializeField] private GameObject coinTossCanvas;

    [Header("Gameplay Statuses")]
    [SyncVar(hook=nameof(HandleGamePhase))] public string gamePhase;
    [SyncVar] public bool firstHalfCompleted = false; // tracks when the first half has completed and when the game should transition to halftime or game over
    [SyncVar] public string firstHalfKickingTeam; // name of the team that kicked in the first half (green or grey). That team will receive to start the second half

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
    [SyncVar] public float yPositionOfKickAfter;
    [SyncVar] public uint scoringGoblinNetId;
    [SerializeField] GameObject KickAfterPositionControlsPanel;
    [SerializeField] GameObject KickAfterTimerBeforeKickPanel;
    [SerializeField] GameObject TimerInstructionsBoard;
    [SerializeField] TextMeshProUGUI TimerInstructionsText;
    [SerializeField] TextMeshProUGUI KickAfterTimerText;
    [SyncVar(hook = nameof(HandleKickAfterTimerUpdate))] int KickAfterTimer;
    [SerializeField] GameObject KickAfterWasKickGoodPanel;
    [SerializeField] TextMeshProUGUI TheKickWasText;
    [SerializeField] TextMeshProUGUI KickWasGoodText;
    [SerializeField] TextMeshProUGUI KickWasNotGoodText;
    [SerializeField] TextMeshProUGUI KickWasBlockedText;
    bool kickAfterRoutineRunning = false;

    [Header("Xtra Time")]
    [SyncVar] public bool isXtraTime;


    [Header("Game timer")]
    [SerializeField] float lengthOfHalves;
    [SerializeField] TextMeshProUGUI timerText;
    [SyncVar(hook = nameof(HandleGameTimerUpdate))] public float timeLeftInGame;
    IEnumerator gameTimeRoutine;
    public bool isGameTimerRunning = false;
    string minutes;
    string seconds;

    [Header("Score")]
    [SyncVar(hook = nameof(HandleGreenScoreUpdate))] public int greenScore;
    [SyncVar(hook = nameof(HandleGreyScoreUpdate))] public int greyScore;
    [SerializeField] TextMeshProUGUI ScoreGreenText;
    [SerializeField] TextMeshProUGUI ScoreGreyText;
    [SyncVar] string winningTeam;

    [Header("Game Over")]
    [SerializeField] GameObject GameOverScorePanel;
    [SerializeField] TextMeshProUGUI theFinalScoreText;
    [SerializeField] TextMeshProUGUI greenTeamNameText;
    [SerializeField] TextMeshProUGUI greyTeamNameText;
    [SerializeField] TextMeshProUGUI greenTeamScoreText;
    [SerializeField] TextMeshProUGUI greyTeamScoreText;
    [SerializeField] TextMeshProUGUI teamWinnerText;
    [SerializeField] GameObject GameOverScrollPanelObject;

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
    public GameObject aiPlayerObject;
    public GamePlayer aiGamePlayerScript;
    public AIPlayer aiPlayerScript;

    [Header("Pause/Resume Game")]
    [SerializeField] GameObject GamePausedTextObject;
    [SerializeField] TextMeshProUGUI GamePausedSecondsLeftText;
    [SyncVar (hook = nameof(HandleIsGamePaused))] public bool isGamePaused = false;
    public GamePlayer playerWhoPaused;
    [SyncVar] public uint playerWhoPausedNetId;
    [SyncVar] public float lastPauseTimeStamp;
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
    [SyncVar (hook = nameof(HandleKickAfterTimerTimeLeft))] int kickAfterTimerTimeLeft = 30;

    [Header("Timeouts: Kick Off")]
    public bool isKickOffTimerCountDownRunning = false;
    IEnumerator timeoutKickOff;
    [SyncVar(hook = nameof(HandleKickOffTimerTimeLeft))] int kickOffTimerTimeLeft = 30;

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
    // Start is called before the first frame update
    void Start()
    {
        if (isClient)
        {
            GetLocalGamePlayer();
            UpdateKickAfterControlsUIForGamepad(GamepadUIManager.instance.gamepadUI);
        }
        // Spawn the AIPlayer if this is a singleplayer game
        if (isServer && this.isSinglePlayer && !this.is1v1)
        {
            SpawnAIPlayer();
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
        HandleGamePhase(gamePhase, "cointoss");
        //timeLeftInGame = 300f;
        //timeLeftInGame = 30f;
        HandleGameTimerUpdate(0f, secondsPerHalf);
        greenScore = 0;
        greyScore = 0;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    [ServerCallback]
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
    void SpawnAIPlayer()
    {
        aiPlayerObject = Instantiate(aiPlayerPrefab);
        NetworkServer.Spawn(aiPlayerObject,LocalGamePlayerScript.connectionToClient);
        aiGamePlayerScript = aiPlayerObject.GetComponent<GamePlayer>();
        aiGamePlayerScript.isSinglePlayer = LocalGamePlayerScript.isSinglePlayer;
        aiGamePlayerScript.isTeamGrey = !LocalGamePlayerScript.isTeamGrey;
        aiGamePlayerScript.SetPlayerName("AIPlayer");
        aiGamePlayerScript.SetConnectionId(-1);
        aiGamePlayerScript.SetPlayerNumber(2);
        aiGamePlayerScript.InitializeAIPlayer();
        aiPlayerScript = aiPlayerObject.GetComponent<AIPlayer>();
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
        if (isServer)
        {
            gamePhase = newValue;
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
                    GoblinScript goblinThatKicked = NetworkIdentity.spawned[scoringGoblinNetId].GetComponent<GoblinScript>();
                    goblinThatKicked.isKickAfterPositionSet = false;
                    goblinThatKicked.isKickAfterGoblin = false;
                    scoringGoblinNetId = 0;
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
        if (isClient)
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
                    LocalGamePlayerScript.GetGoblinTeammatesFor3v3();
            }
            if (newValue == "kick-after-attempt")
            {
                HideTouchDownText();

                // Have the AI start their kick after attempt if they are the scoring player
                if (this.isSinglePlayer && !this.is1v1)
                {
                    if (scoringPlayer == aiGamePlayerScript)
                        AIKickAfterKickingPlayer();
                }
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
                player.RpcRepositionTeamForKickOff3v3(player.connectionToClient, true, isThisTheKickingPlayer);
                player.gameplayActionControlsOnServer = false;
                player.goblinMovementControlsOnServer = false;
                player.powerupsControlsOnServer = false;
                player.kickingControlsOnServer = isThisTheKickingPlayer;
                player.kickOffAimArrowControlsOnServer = isThisTheKickingPlayer;
                player.qeSwitchingControlsOnServer = false;
            }
            foreach (GamePlayer player in receivingTeam.teamPlayers)
            {

                player.RpcRepositionTeamForKickOff3v3(player.connectionToClient, false, false);
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
    public void TouchDownScored(bool wasGrey, uint goblinNetId, float yPosition)
    {
        Debug.Log("TouchDownScored: " + goblinNetId.ToString());
        //Remove this place holder phase if it causes issues???
        HandleGamePhase(this.gamePhase, "touchdown-transition");

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
                HandleGamePhase(this.gamePhase, "gameover");
                return;
            }            
        }
        // If mercy rule is enabled, check if the new score is greater than mercy rule differential
        if (this.mercyRuleEnabled)
        {
            if (CheckForMercyRule())
            {
                HandleGamePhase(this.gamePhase, "gameover");
                return;
            }
        }

        RpcTouchDownScored(wasGrey, goblinNetId);
        IEnumerator touchdownToKickAfterTransition = GameplayToKickAfterTransition();
        StartCoroutine(touchdownToKickAfterTransition);
        StopPossessionRoutinesForPlayers();

        // Stop Goblins from running?
        ResetAllGoblinStatuses();
    }
    [ClientRpc]
    void RpcTouchDownScored(bool wasGrey, uint goblinId)
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
        GameObject scoringGoblin = NetworkIdentity.spawned[goblinId].gameObject;
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
        if (this.isSinglePlayer && this.aiPlayerScript != null)
        {
            try
            {
                aiPlayerScript.ActivateAIPowerUpRoutine(activate);
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
            // Don't count down the time if the game is paused?
            if (this.isGamePaused)
                continue;

            yield return new WaitForSeconds(1.0f);
            timeLeftTracker = timeLeftInGame - 1;
            
            HandleGameTimerUpdate(timeLeftInGame, timeLeftTracker);
            if (timeLeftTracker <= 0)
            {
                HandleGameTimerUpdate(timeLeftInGame, 0);
                isGameTimerRunning = false;
                HandleGamePhase(this.gamePhase, "xtra-time");
            }
        }
        yield break;        
    }
    void HandleGameTimerUpdate(float oldValue, float newValue)
    {
        if (isServer)
            timeLeftInGame = newValue;
        if (isClient)
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
        if (isServer)
        {
            greenScore = newValue;
            //CheckForMercyRule();
        }   
        if (isClient)
        {
            ScoreGreenText.text = newValue.ToString("00");
            greenTeamScoreText.text = newValue.ToString("00");
        }
    }
    void HandleGreyScoreUpdate(int oldValue, int newValue)
    {
        if (isServer)
        {
            greyScore = newValue;
            //CheckForMercyRule();
        }   
        if (isClient)
        {
            ScoreGreyText.text = newValue.ToString("00");
            greyTeamScoreText.text = newValue.ToString("00");
        }
    }
    [Server]
    IEnumerator GameplayToKickAfterTransition()
    {
        yield return new WaitForSeconds(3.0f);
        HandleGamePhase(gamePhase, "kick-after-attempt");
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
    void SetKickAfterPlayers(uint scoringGoblin)
    {
        Debug.Log("SetKickAfterPlayers: the scoring goblin was: " + scoringGoblin.ToString());
        GoblinScript scoringGoblinScript = NetworkIdentity.spawned[scoringGoblin].GetComponent<GoblinScript>();
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
                if (scoringGoblinScript.ownerConnectionId == player.ConnectionId)
                {
                    scoringPlayer = player;
                    kickingPlayer = player;
                    player.RpcRepositionForKickAfter(player.connectionToClient, true, scoringGoblin, yPositionOfKickAfter);
                }
                else
                {
                    blockingPlayer = player;
                    receivingPlayer = player;
                    player.RpcRepositionForKickAfter(player.connectionToClient, false, scoringGoblin, yPositionOfKickAfter);
                }
            }
        }
        else if (this.isSinglePlayer && !this.is1v1)
        {
            foreach (GamePlayer player in Game.GamePlayers)
            {
                if (scoringGoblinScript.ownerConnectionId == player.ConnectionId)
                {
                    scoringPlayer = player;
                    kickingPlayer = player;
                    player.RpcRepositionForKickAfter(player.connectionToClient, true, scoringGoblin, yPositionOfKickAfter);
                }
                else
                {
                    blockingPlayer = player;
                    receivingPlayer = player;
                    player.RpcRepositionForKickAfter(player.connectionToClient, false, scoringGoblin, yPositionOfKickAfter);
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
            // Set kicking and receiving team/player based on who scored and who will be blocking
            kickingTeam = scoringTeam;
            kickingPlayer = scoringTeam.captain;
            receivingTeam = blockingTeam;
            receivingPlayer = blockingTeam.captain;
            blockingPlayer = blockingTeam.captain;
            // get the owner of the goblin who scored to know who to give kick after controls to and so on
            foreach (GamePlayer player in Game.GamePlayers)
            {
                if (scoringGoblinScript.ownerConnectionId == player.ConnectionId)
                {
                    scoringPlayer = player;
                    break;
                }
            }
            int yModifier = 1;
            // Reposition goblins for scoring team
            foreach (GamePlayer player in scoringTeam.teamPlayers)
            {
                if (player == scoringPlayer)
                    player.RpcRepositionForKickAfterFor3v3(player.connectionToClient, true, true, scoringGoblin, yPositionOfKickAfter, 1);
                else
                {
                    yModifier *= -1;
                    player.RpcRepositionForKickAfterFor3v3(player.connectionToClient, false, true, scoringGoblin, yPositionOfKickAfter, yModifier);
                }
            }
            foreach (GamePlayer player in blockingTeam.teamPlayers)
            {
                player.RpcRepositionForKickAfterFor3v3(player.connectionToClient, false, false, scoringGoblin, yPositionOfKickAfter, 1);
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
            scoringPlayer.RpcDisableKickAfterPositioningControls(scoringPlayer.connectionToClient);
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
            scoringPlayer.RpcStartKickAfterTimer(scoringPlayer.connectionToClient, true);
            blockingPlayer.RpcStartKickAfterTimer(blockingPlayer.connectionToClient, false);
        }
        else if (this.isSinglePlayer && !this.is1v1)
        { 
            // Only have the player be sent the RpcStartKickAfterTimer stuff, not the AI. So, check if the player is the scoring player or blocking player and send approriate message
            if(scoringPlayer == LocalGamePlayerScript)
                scoringPlayer.RpcStartKickAfterTimer(scoringPlayer.connectionToClient, true);
            else if (blockingPlayer == LocalGamePlayerScript)
                blockingPlayer.RpcStartKickAfterTimer(blockingPlayer.connectionToClient, false);
        }
        else
        {
            foreach (GamePlayer player in scoringTeam.teamPlayers)
            {
                player.RpcStartKickAfterTimer(player.connectionToClient, true);
            }
            foreach (GamePlayer player in blockingTeam.teamPlayers)
            {
                player.RpcStartKickAfterTimer(player.connectionToClient, false);
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
            scoringPlayer.RpcActivateKickAfterKickingControls(scoringPlayer.connectionToClient, true);
            blockingPlayer.RpcActivateKickAfterKickingControls(blockingPlayer.connectionToClient, false);

            scoringPlayer.RpcActivateKickAfterBlockingControls(scoringPlayer.connectionToClient, false);
            blockingPlayer.RpcActivateKickAfterBlockingControls(blockingPlayer.connectionToClient, true);

            scoringPlayer.RpcKickAfterUpdateInsctructionsText(scoringPlayer.connectionToClient, true);
            blockingPlayer.RpcKickAfterUpdateInsctructionsText(blockingPlayer.connectionToClient, false);
        }
        else if (this.isSinglePlayer && !this.is1v1)
        {
            // Only have the player be sent the RpcStartKickAfterTimer stuff, not the AI. So, check if the player is the scoring player or blocking player and send approriate message
            if (scoringPlayer == LocalGamePlayerScript)
            {
                scoringPlayer.RpcActivateKickAfterKickingControls(scoringPlayer.connectionToClient, true);
                scoringPlayer.RpcActivateKickAfterBlockingControls(scoringPlayer.connectionToClient, false);
                scoringPlayer.RpcKickAfterUpdateInsctructionsText(scoringPlayer.connectionToClient, true);
            }
            else if (blockingPlayer == LocalGamePlayerScript)
            {
                blockingPlayer.RpcActivateKickAfterKickingControls(blockingPlayer.connectionToClient, false);
                blockingPlayer.RpcActivateKickAfterBlockingControls(blockingPlayer.connectionToClient, true);
                blockingPlayer.RpcKickAfterUpdateInsctructionsText(blockingPlayer.connectionToClient, false);
            }
            if (blockingPlayer == aiGamePlayerScript)
            {
                AIBlockingPlayer();
            }
            else if (scoringPlayer == aiGamePlayerScript)
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
                    player.RpcActivateKickAfterKickingControls(player.connectionToClient, true);
                }
                player.RpcActivateKickAfterBlockingControls(player.connectionToClient, false);
                player.RpcKickAfterUpdateInsctructionsText(player.connectionToClient, true);
            }
            foreach (GamePlayer player in blockingTeam.teamPlayers)
            {
                player.RpcActivateKickAfterKickingControls(player.connectionToClient, false);
                player.RpcActivateKickAfterBlockingControls(player.connectionToClient, true);
                player.RpcKickAfterUpdateInsctructionsText(player.connectionToClient, false);
            }
        }
        
        Debug.Log("KickAfterTimerCountDown: ended");
    }
    void HandleKickAfterTimerUpdate(int oldValue, int newValue)
    {
        if (isServer)
            KickAfterTimer = newValue;
        if (isClient)
        {
            KickAfterTimerText.text = newValue.ToString();
        }
    }
    [ClientRpc]
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
        }
        RpcKickAfterWasKickGoodOrBad(isKickGood, isScoringPlayerGrey);
        TeamManager.instance.KickAfterAttempts(scoringPlayer.isTeamGrey, isKickGood);
        if (this.isSinglePlayer && !this.is1v1)
        {
            AIStopBlockingKickAfter();
        }
    }
    [ClientRpc]
    void RpcKickAfterWasKickGoodOrBad(bool isKickGood, bool isScoringPlayerGrey)
    {
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
                scoringPlayer.RpcActivateKickAfterKickingControls(scoringPlayer.connectionToClient, false);
                blockingPlayer.RpcActivateKickAfterBlockingControls(blockingPlayer.connectionToClient, false);
            }
            else if (this.isSinglePlayer && !this.is1v1)
            { 
                if(scoringPlayer == LocalGamePlayerScript)
                    scoringPlayer.RpcActivateKickAfterKickingControls(scoringPlayer.connectionToClient, false);
                else if (blockingPlayer == LocalGamePlayerScript)
                    blockingPlayer.RpcActivateKickAfterBlockingControls(blockingPlayer.connectionToClient, false);
            }
            else
            {
                foreach (GamePlayer player in scoringTeam.teamPlayers)
                {
                    player.RpcActivateKickAfterKickingControls(player.connectionToClient, false);
                }
                foreach (GamePlayer player in blockingTeam.teamPlayers)
                {
                    player.RpcActivateKickAfterBlockingControls(player.connectionToClient, false);
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
            HandleGamePhase(this.gamePhase, "gameover");
        }
        else if (isXtraTime && firstHalfCompleted)
        {
            GameplayManager.instance.HandleGamePhase(GameplayManager.instance.gamePhase, "gameover");
        }
        else if (isXtraTime && !firstHalfCompleted)
        {
            Debug.Log("TransitionFromKickAfterAttemptToKickOffRoutine: it should be halftime now!");
            GameplayManager.instance.HandleGamePhase(GameplayManager.instance.gamePhase, "halftime");
        }
        else
        {
            Debug.Log("TransitionFromKickAfterAttemptToKickOffRoutine: not halftime. Kickoff time!");
            GameplayManager.instance.HandleGamePhase(GameplayManager.instance.gamePhase, "kickoff");
        }
        kickAfterRoutineRunning = false;        
    }
    [ClientRpc]
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
        if (this.isSinglePlayer)
            aiGamePlayerScript.areGoblinsRepositionedForKickAfter = false;
    }
    [Server]
    public void KickAfterAttemptWasBlocked()
    {
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
    [ClientRpc]
    void RpcKickAfterAttemptWasBlocked()
    {
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
        if (greenScore > greyScore)
        {
            winningTeam = "green";
        }
        else if (greenScore < greyScore)
        {
            winningTeam = "grey";
        }
        else
        {
            winningTeam = "draw";
        }
        RpcWinnerOfGameDetermined(winningTeam);
    }
    [ClientRpc]
    void RpcWinnerOfGameDetermined(string winnerOfGame)
    {
        theFinalScoreText.text = "THE FINAL SCORE";
        greenTeamScoreText.text = greenScore.ToString();
        greyTeamScoreText.text = greyScore.ToString();
        GameOverScorePanel.SetActive(true);
        
        if (winnerOfGame == "green")
        {
            teamWinnerText.text = "GREEN WINS!!!";
            //teamWinnerText.GetComponent<TouchDownTextGradient>().ActivateGradient();
        }
        else if (winnerOfGame == "grey")
        {
            teamWinnerText.text = "GREY WINS!!!";
            //teamWinnerText.GetComponent<TouchDownTextGradient>().ActivateGradient();
        }
        else if (winnerOfGame == "draw")
        {
            teamWinnerText.text = "IT'S A DRAW...";
            teamWinnerText.GetComponent<TouchDownTextGradient>().SetGreenOrGreyColor(true);
        }
        GameOverScrollPanelObject.GetComponent<ImageAnimation>().UnScrollEndOfGame(winnerOfGame);

    }
    [Server]
    public void DidWinningGoblinDiveInXtraTimeToEndGame(bool isGoblinGrey)
    {
        if ((isGoblinGrey && greyScore > greenScore) || (!isGoblinGrey && greyScore < greenScore))
        {
            HandleGamePhase(this.gamePhase, "gameover");
        }

    }
    [ServerCallback]
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
    [ServerCallback]
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
        GoblinScript scoringGoblinScript = NetworkIdentity.spawned[scoringGoblinNetId].GetComponent<GoblinScript>();
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
    [ClientRpc]
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
    [ServerCallback]
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
    [ServerCallback]
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
    [ServerCallback]
    public void SaveFirstHalfKickingTeam()
    {
        Debug.Log("SaveFirstHalfKickingTeam: team that is kicking is: " + kickingPlayer.teamName);
        if (!String.IsNullOrWhiteSpace(kickingPlayer.teamName))
        {
            firstHalfKickingTeam = kickingPlayer.teamName;
        }

    }
    [ServerCallback]
    void TransitionToSecondHalf()
    {
        GetSecondHalfKickingTeam();
        ResetAllGoblinStatuses();
        IEnumerator transitionToSecondHalfRoutine = TransitionToSecondHalfRoutine();
        StartCoroutine(transitionToSecondHalfRoutine);
    }
    [ServerCallback]
    void GetSecondHalfKickingTeam()
    {
        Debug.Log("GetSecondHalfKickingTeam");

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
    [ServerCallback]
    IEnumerator TransitionToSecondHalfRoutine()
    {
        RpcHalfTimeTransition(true, kickingPlayer.teamName);
        yield return new WaitForSeconds(7f);
        RpcHalfTimeTransition(false, kickingPlayer.teamName);
        yield return new WaitForSeconds(1f);
        this.firstHalfCompleted = true;
        ResetAllGoblinStatuses();
        this.HandleGamePhase(this.gamePhase, "kickoff");
        this.HandleGameTimerUpdate(this.timeLeftInGame, secondsPerHalf);
    }
    [ClientRpc]
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
    [ServerCallback]
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
    [ClientCallback]
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
    public void AIChoosesCoinToss()
    {
        if (this.isSinglePlayer)
        {
            Debug.Log("AIChoosesCoinToss: The AI will choose the coin toss, not the player");
            IEnumerator aiWaitToChooseCoin = AIWaitToChooseCoin();
            StartCoroutine(aiWaitToChooseCoin);
        }
    }
    IEnumerator AIWaitToChooseCoin()
    {
        yield return new WaitForSeconds(2.0f);
        string[] headsTails = new[]
        { "heads","tails"};
        var rng = new System.Random();
        //aiGamePlayerScript.headsOrTailsPlayer = headsTails[rng.Next(headsTails.Length)];
        aiGamePlayerScript.headsOrTailsPlayer = "tails";
        aiGamePlayerScript.SubmitCoinSelection();
    }
    public void AIChoosesKickOrReceive()
    {
        if (this.isSinglePlayer)
        {
            Debug.Log("AIChoosesKickOrReceive: The AI will choose to kick or receive, not the player");
            IEnumerator aiWaitToChooseKickOrReceive = AIWaitToChooseKickOrReceive();
            StartCoroutine(aiWaitToChooseKickOrReceive);
        }
    }
    IEnumerator AIWaitToChooseKickOrReceive()
    {
        yield return new WaitForSeconds(5.0f);
        string[] headsTails = new[]
        { "kick","receive"};
        var rng = new System.Random();
        aiGamePlayerScript.kickOrReceivePlayer = headsTails[rng.Next(headsTails.Length)];
        //aiGamePlayerScript.kickOrReceivePlayer = "kick";
        aiGamePlayerScript.SubmitKickOrReceiveSelection();
    }
    public void AIPlayerKickOff()
    {
        Debug.Log("AIPlayerKickOff");
        IEnumerator aiKickOffSequence = AIKickOffSequence();
        StartCoroutine(aiKickOffSequence);
    }
    IEnumerator AIKickOffSequence()
    {
        yield return new WaitForSeconds(3.0f);
        aiPlayerScript.KickOffSequence();
    }
    public void AIStartKickAfterPositioning()
    {
        Debug.Log("AIStartKickAfterPositioning");
        if (scoringPlayer == aiGamePlayerScript)
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
    IEnumerator AIBlockKickDelay()
    {
        yield return new WaitForSeconds(0.25f);
        aiPlayerScript.blockKick = true;
    }
    void AIStopBlockingKickAfter()
    {
        aiPlayerScript.blockKick = false;
        aiPlayerScript.StopBlockingGoblinFromRunning();
    }
    void AIKickAfterKickingPlayer()
    {
        Debug.Log("AIKickAfterKickingPlayer");
        IEnumerator aiKickAfterKickingPlayerDelay = AIKickAfterKickingPlayerDelay();
        StartCoroutine(aiKickAfterKickingPlayerDelay);
    }
    IEnumerator AIKickAfterKickingPlayerDelay()
    {
        aiPlayerScript.kickAfterGoblin = NetworkIdentity.spawned[scoringGoblinNetId].GetComponent<GoblinScript>();
        yield return new WaitForSeconds(1.0f);
        Debug.Log("AIKickAfterKickingPlayerDelay");
        aiPlayerScript.finalPositionReached = false;
        aiPlayerScript.kickKickAfter = true;
        
        
    }
    void AIKickAfterAttempt()
    {
        Debug.Log("AIKickAfterAttempt");
        aiPlayerScript.KickAfterAttempt();
    }
    [ClientRpc]
    void RpcKickOffWhistle()
    {
        SoundManager.instance.PlaySound("ref-whistle", 1.0f);
    }
    [ServerCallback]
    public void PauseGameOnServer(GamePlayer pausingPlayer, uint pausingPlayerNetId)
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
                pausingPlayer.RpcErrorMessageToDisplayFromServer(pausingPlayer.connectionToClient, "Your team must wait 30 seconds of game time to pause again.");
                return;
            }
        }

        playerWhoPaused = pausingPlayer;
        playerWhoPausedNetId = pausingPlayerNetId;
        PauseGameForAllPlayers(true);
        this.HandleIsGamePaused(this.isGamePaused, true);
        if (!this.isSinglePlayer)
        {
            GamePausedTimeout = PauseGameTimeoutRoutine();
            StartCoroutine(GamePausedTimeout);
        }
        //Time.timeScale = 0f;
    }
    [ServerCallback]
    public void ResumeGameOnServer(GamePlayer pausingPlayer, uint pausingPlayerNetId)
    {
        if (!this.isGamePaused)
            return;
        Debug.Log("ResumeGameOnServer: Player with a netid of: " + pausingPlayerNetId.ToString() + " wants to RESUME the game.");
        if (pausingPlayerNetId == this.playerWhoPausedNetId)
        {
            Debug.Log("ResumeGameOnServer: Player with a netid of: " + pausingPlayerNetId.ToString() + " matches netid of player who paused: " + this.playerWhoPausedNetId.ToString() + " will resume game.");
            playerWhoPaused = null;
            playerWhoPausedNetId = 0;
            PauseGameForAllPlayers(false);
            this.HandleIsGamePaused(this.isGamePaused, false);
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
                pausingPlayer.RpcErrorMessageToDisplayFromServer(pausingPlayer.connectionToClient, "Only the player who paused the game can resume the game.");
        }
    }
    public void HandleIsGamePaused(bool oldValue, bool newValue)
    {
        if (isServer)
            isGamePaused = newValue;
        if (isClient)
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
    [ServerCallback]
    void PauseGameForAllPlayers(bool pauseGame)
    {
        foreach (GamePlayer player in Game.GamePlayers)
        {
            player.RpcGamePausedByServer(player.connectionToClient, pauseGame);
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
            playerWhoPausedNetId = 0;
            PauseGameForAllPlayers(false);
            this.HandleIsGamePaused(this.isGamePaused, false);
            isGamePausedTimeoutRoutineRunning = false;
        }
        yield break;
    }
    [ClientRpc]
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
        
        this.HandleKickAfterTimerTimeLeft(this.kickAfterTimerTimeLeft, 30);
        RpcActivateTheTimeoutTimer(true);
        int timeLeftTracker;
        while (isKickAfterTimerRunning)
        {
            // Don't count down the time if the game is paused?
            if (this.isGamePaused)
                continue;
            if (this.gamePhase != "kick-after-attempt")
            {
                isKickAfterTimerRunning = false;
                break;
            }
            yield return new WaitForSeconds(1.0f);
            if (isKickAfterTimerCountDownRunning)
                continue;
            timeLeftTracker = kickAfterTimerTimeLeft - 1;

            HandleKickAfterTimerTimeLeft(this.kickAfterTimerTimeLeft, timeLeftTracker);
            if (timeLeftTracker <= 0)
            {
                HandleKickAfterTimerTimeLeft(this.kickAfterTimerTimeLeft, 0);
                isKickAfterTimerRunning = false;
                // code to block kick automatically?
                try
                {
                    GoblinScript goblin = NetworkIdentity.spawned[scoringGoblinNetId].GetComponent<GoblinScript>();
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
        if (isServer)
            kickAfterTimerTimeLeft = newValue;
        if (isClient)
        {
            timeoutTimerText.text = ":" + newValue.ToString();
        }
    }
    [ClientRpc]
    void RpcActivateTheTimeoutTimer(bool activate)
    {
        TimeoutTimerHolder.SetActive(activate);
    }
    [ServerCallback]
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

        this.HandleKickOffTimerTimeLeft(this.kickOffTimerTimeLeft, 30);
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

            HandleKickOffTimerTimeLeft(this.kickOffTimerTimeLeft, timeLeftTracker);
            if (timeLeftTracker <= 0)
            {
                HandleKickOffTimerTimeLeft(this.kickOffTimerTimeLeft, 0);
                isKickOffTimerCountDownRunning = false;
                // code to block kick automatically?
                try
                {
                    kickingPlayer.serverSelectGoblin.RpcKickOffTimeoutForceKick(kickingPlayer.serverSelectGoblin.connectionToClient);
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
        if (isServer)
            kickOffTimerTimeLeft = newValue;
        if (isClient)
        {
            timeoutTimerText.text = ":" + newValue.ToString();
        }
    }
    [ServerCallback]
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
    [ServerCallback]
    void SendErrorMessageToTeam(bool isTeamGrey, string message)
    {
        if (isTeamGrey)
        {
            foreach (GamePlayer player in TeamManager.instance.greyTeam.teamPlayers)
            {
                player.RpcErrorMessageToDisplayFromServer(player.connectionToClient, message);
            }
        }
        else
        {
            foreach (GamePlayer player in TeamManager.instance.greenTeam.teamPlayers)
            {
                player.RpcErrorMessageToDisplayFromServer(player.connectionToClient, message);
            }
        }
    }
    [ClientCallback]
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
    [ClientCallback]
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
    [ServerCallback]
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
                    foreach (uint goblinNetId in teamToIterate.goblinNetIds)
                    {
                        NetworkIdentity.spawned[goblinNetId].GetComponent<GoblinScript>().UpdateGamePadUIMarkersForGoblins(enableGamepadUI);
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
