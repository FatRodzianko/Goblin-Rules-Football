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
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        this.is1v1 = Game.is1v1;
        Debug.Log("GameplayManager: is 1v1 is set to: " + this.is1v1);
        //gamePhase = "cointoss";
        HandleGamePhase(gamePhase, "cointoss");
        //timeLeftInGame = 300f;
        //timeLeftInGame = 30f;
        HandleGameTimerUpdate(0f, lengthOfHalves);
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
    public void ActivateCoinTossUI(bool activate)
    {
        Debug.Log("ActivateCoinTossUI: " + activate.ToString());
        coinTossCanvas.SetActive(activate);
    }
    public void EnableGoblinMovement()
    {
        Debug.Log("EnableGoblinMovement");
        LocalGamePlayerScript.EnableGoblinMovement(true);
    }
    public void DisableGoblinMovement()
    {
        Debug.Log("EnableGoblinMovement");
        LocalGamePlayerScript.EnableGoblinMovement(false);
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
                LocalGamePlayerScript.ResetCameraPositionForKickOff();
                LocalGamePlayerScript.FollowSelectedGoblin(LocalGamePlayerScript.selectGoblin.transform);
                LocalGamePlayerScript.CoinTossControlls(false);
                LocalGamePlayerScript.KickOrReceiveControls(false);
                if(!this.is1v1)
                    LocalGamePlayerScript.GetGoblinTeammatesFor3v3();
            }
            if (newValue == "kick-after-attempt")
            {
                HideTouchDownText();
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
        if (this.is1v1)
        {
            receivingPlayer.RpcRepositionTeamForKickOff(false);
            kickingPlayer.RpcRepositionTeamForKickOff(true);
        }
        else
        {
            foreach (GamePlayer player in kickingTeam.teamPlayers)
            {
                bool isThisTheKickingPlayer = false;
                if (player == kickingPlayer)
                    isThisTheKickingPlayer = true;
                player.RpcRepositionTeamForKickOff3v3(player.connectionToClient, true, isThisTheKickingPlayer);
            }
            foreach (GamePlayer player in receivingTeam.teamPlayers)
            {

                player.RpcRepositionTeamForKickOff3v3(player.connectionToClient, false, false);
            }
        }
        
    }
    [Server]
    void ActivateGameplayControls(bool activate)
    {
        foreach (GamePlayer player in Game.GamePlayers)
        {
            player.RpcActivateGameplayControls(activate);
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
        RpcTouchDownScored(wasGrey, goblinNetId);
        IEnumerator touchdownToKickAfterTransition = GameplayToKickAfterTransition();
        StartCoroutine(touchdownToKickAfterTransition);
        StopPossessionRoutinesForPlayers();
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
    }
    [Server]
    IEnumerator GameTimerCountdown()
    {
        isGameTimerRunning = true;
        float timeLeftTracker;
        while (isGameTimerRunning)
        {
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
            greenScore = newValue;
        if (isClient)
        {
            ScoreGreenText.text = newValue.ToString("00");
            greenTeamScoreText.text = newValue.ToString("00");
        }
    }
    void HandleGreyScoreUpdate(int oldValue, int newValue)
    {
        if (isServer)
            greyScore = newValue;
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
        KickAfterTimer = 3;
        yield return new WaitForSeconds(1.0f);
        KickAfterTimer = 2;
        yield return new WaitForSeconds(1.0f);
        KickAfterTimer = 1;
        yield return new WaitForSeconds(1.0f);
        RpcDisableKickAfterTimerUI();
        if (this.is1v1)
        {
            scoringPlayer.RpcActivateKickAfterKickingControls(scoringPlayer.connectionToClient, true);
            blockingPlayer.RpcActivateKickAfterKickingControls(blockingPlayer.connectionToClient, false);

            scoringPlayer.RpcActivateKickAfterBlockingControls(scoringPlayer.connectionToClient, false);
            blockingPlayer.RpcActivateKickAfterBlockingControls(blockingPlayer.connectionToClient, true);

            scoringPlayer.RpcKickAfterUpdateInsctructionsText(scoringPlayer.connectionToClient, true);
            blockingPlayer.RpcKickAfterUpdateInsctructionsText(blockingPlayer.connectionToClient, false);
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
        }
        else
        {
            KickWasNotGoodText.gameObject.SetActive(true);
            KickWasGoodText.gameObject.SetActive(false);
            KickWasNotGoodText.GetComponent<TouchDownTextGradient>().SetGreenOrGreyColor(isScoringPlayerGrey);
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
        if (isXtraTime && firstHalfCompleted)
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
        else
        {
            TeamManager.instance.BlockedKick(blockingTeam.isGrey);
            TeamManager.instance.KickAfterAttempts(scoringTeam.isGrey, false);
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
        this.HandleGameTimerUpdate(this.timeLeftInGame, lengthOfHalves);
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
            }
            else
            {
                Debug.Log("PlayPossessionChangedSFX: team LOST possession of the ball.");
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
}
