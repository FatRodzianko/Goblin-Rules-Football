using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameplayManager : NetworkBehaviour
{
    public static GameplayManager instance;

    [Header("Local GamePlayers")]
    [SerializeField] private GameObject LocalGamePlayer;
    [SerializeField] private GamePlayer LocalGamePlayerScript;

    [Header("UI Canvases")]
    [SerializeField] private GameObject gameplayCanvas;
    [SerializeField] private GameObject coinTossCanvas;

    [Header("Gameplay Statuses")]
    [SyncVar(hook=nameof(HandleGamePhase))] public string gamePhase;

    [Header("Kickoff Info")]
    public GamePlayer kickingPlayer;
    public GamePlayer receivingPlayer;

    [Header("Touchdown")]
    [SerializeField] GameObject TouchDownPanel;
    [SerializeField] TextMeshProUGUI touchDownText;
    [SerializeField] TextMeshProUGUI touchDownTeamText;

    [Header("Kick After Attempt")]
    public GamePlayer scoringPlayer;
    public GamePlayer blockingPlayer;
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

    [Header("Xtra Time")]
    [SyncVar] public bool isXtraTime;


    [Header("Game timer")]
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
        }
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        //gamePhase = "cointoss";
        HandleGamePhase(gamePhase, "cointoss");
        //timeLeftInGame = 300f;
        //timeLeftInGame = 30f;
        HandleGameTimerUpdate(0f, 150f);
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
        if (isServer)
        {
            gamePhase = newValue;
            if (newValue == "kickoff")
                RepositionTeamsForKickOff();
            if (newValue == "gameplay")
            {
                ActivateGameplayControls(true);
                PowerUpManager.instance.StartGeneratingPowerUps(true);
                RandomEventManager.instance.StartGeneratingRandomEvents(true);            }
            if (oldValue == "gameplay")
            {
                PowerUpManager.instance.StartGeneratingPowerUps(false);
                RandomEventManager.instance.StartGeneratingRandomEvents(false);
            }                
            if (newValue == "kick-after-attempt")
            {
                SetKickAfterPlayers(scoringGoblinNetId);
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
                
            }
            if (newValue == "xtra-time")
            {
                isXtraTime = true;
            }
            if (newValue == "gameover")
            {
                Debug.Log("GameplayManager on Server: It's game over!");
                DetermineWinnerOfGame();
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
        receivingPlayer.RpcRepositionTeamForKickOff(false);
        kickingPlayer.RpcRepositionTeamForKickOff(true);
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
    public void TouchDownScored(bool wasGrey, uint goblinNetId, float yPosition)
    {
        Debug.Log("TouchDownScored: " + goblinNetId.ToString());
        
        //SetKickAfterPlayers(scoringGoblinNetId);
        this.scoringGoblinNetId = goblinNetId;

        //update scores:
        if (wasGrey)
            greyScore += 5;
        else
            greenScore += 5;

        yPositionOfKickAfter = yPosition;

        ActivateGameplayControls(false);
        if (isXtraTime)
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
    }
    [Server]
    void SetKickAfterPlayers(uint scoringGoblin)
    {
        Debug.Log("SetKickAfterPlayers: the scoring goblin was: " + scoringGoblin.ToString());
        GoblinScript scoringGoblinScript = NetworkIdentity.spawned[scoringGoblin].GetComponent<GoblinScript>();
        scoringGoblinScript.isKickAfterGoblin = true;
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
        scoringPlayer.RpcStartKickAfterTimer(scoringPlayer.connectionToClient, true);
        blockingPlayer.RpcStartKickAfterTimer(blockingPlayer.connectionToClient, false);
        IEnumerator kickAfterCountdown = KickAfterTimerCountDown();
        StartCoroutine(kickAfterCountdown);
    }
    public void ActivateKickAfterTimerUI(bool isKickingPlayer)
    {
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
        KickAfterTimer = 3;
        yield return new WaitForSeconds(1.0f);
        KickAfterTimer = 2;
        yield return new WaitForSeconds(1.0f);
        KickAfterTimer = 1;
        yield return new WaitForSeconds(1.0f);
        RpcDisableKickAfterTimerUI();

        scoringPlayer.RpcActivateKickAfterKickingControls(scoringPlayer.connectionToClient, true);
        blockingPlayer.RpcActivateKickAfterKickingControls(blockingPlayer.connectionToClient, false);

        scoringPlayer.RpcActivateKickAfterBlockingControls(scoringPlayer.connectionToClient, false);
        blockingPlayer.RpcActivateKickAfterBlockingControls(blockingPlayer.connectionToClient, true);

        scoringPlayer.RpcKickAfterUpdateInsctructionsText(scoringPlayer.connectionToClient, true);
        blockingPlayer.RpcKickAfterUpdateInsctructionsText(blockingPlayer.connectionToClient, false);


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
            TimerInstructionsText.text = "\"Tab\" to submit kick accuracy and power!";
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
        IEnumerator TransitionFromKickAfterAttemptToKickOff = TransitionFromKickAfterAttemptToKickOffRoutine();
        StartCoroutine(TransitionFromKickAfterAttemptToKickOff);
    }
    [Server]
    public void DisableKickAfterAttemptControls()
    {
        try
        {
            scoringPlayer.RpcActivateKickAfterKickingControls(scoringPlayer.connectionToClient, false);
            blockingPlayer.RpcActivateKickAfterBlockingControls(blockingPlayer.connectionToClient, false);
        }
        catch (Exception e)
        {
            Debug.Log("DisableKickAfterAttemptControls: failed for reason: " + e);
        }
        
    }
    IEnumerator TransitionFromKickAfterAttemptToKickOffRoutine()
    {
        yield return new WaitForSeconds(3.0f);        
        RpcDisableKickAfterWasKickGoodOrBadUI();
        if (isXtraTime)
        {
            GameplayManager.instance.HandleGamePhase(GameplayManager.instance.gamePhase, "gameover");
        }
        else
        {
            GameplayManager.instance.HandleGamePhase(GameplayManager.instance.gamePhase, "kickoff");
        }
        
    }
    [ClientRpc]
    void RpcDisableKickAfterWasKickGoodOrBadUI()
    {
        KickAfterWasKickGoodPanel.SetActive(false);
        TheKickWasText.gameObject.SetActive(false);
        KickWasNotGoodText.gameObject.SetActive(false);
        KickWasGoodText.gameObject.SetActive(false);
        KickWasBlockedText.gameObject.SetActive(false);
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
        greenTeamScoreText.text = greenScore.ToString();
        greyTeamScoreText.text = greyScore.ToString();
        GameOverScorePanel.SetActive(true);
        if (winnerOfGame == "green")
        {
            teamWinnerText.text = "GREEN WINS!!!";
            teamWinnerText.GetComponent<TouchDownTextGradient>().ActivateGradient();
        }
        else if (winnerOfGame == "grey")
        {
            teamWinnerText.text = "GREY WINS!!!";
            teamWinnerText.GetComponent<TouchDownTextGradient>().ActivateGradient();
        }
        else if (winnerOfGame == "draw")
        {
            teamWinnerText.text = "IT'S A DRAW...";
            teamWinnerText.GetComponent<TouchDownTextGradient>().SetGreenOrGreyColor(true);
        }
        
    }
    [Server]
    public void DidWinningGoblinDiveInXtraTimeToEndGame(bool isGoblinGrey)
    {
        if ((isGoblinGrey && greyScore > greenScore) || (!isGoblinGrey && greyScore < greenScore))
        {
            HandleGamePhase(this.gamePhase, "gameover");
        }

    }
}
