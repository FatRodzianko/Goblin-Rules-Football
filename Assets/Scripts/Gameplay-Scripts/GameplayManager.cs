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
    [SerializeField] TextMeshProUGUI touchDownText;
    [SerializeField] TextMeshProUGUI touchDownTeamText;

    [Header("Kick After Attempt")]
    public GamePlayer scoringPlayer;
    public GamePlayer blockingPlayer;
    [SyncVar] public float yPositionOfKickAfter;
    [SyncVar] public uint scoringGoblinNetId;

    [Header("Game timer")]
    [SerializeField] TextMeshProUGUI timerText;
    [SyncVar(hook = nameof(HandleGameTimerUpdate))] public float timeLeftInGame;
    IEnumerator gameTimeRoutine;
    public bool isGameTimerRunning = false;
    string minutes;
    string seconds;

    [Header("Score")]
    [SyncVar(hook = nameof(HandleGreenScoreUpdate))] int greenScore;
    [SyncVar(hook = nameof(HandleGreyScoreUpdate))] int greyScore;
    [SerializeField] TextMeshProUGUI ScoreGreenText;
    [SerializeField] TextMeshProUGUI ScoreGreyText;


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
        timeLeftInGame = 300f;
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
            else if (newValue == "gameplay")
                ActivateGameplayControls(true);
            else if (newValue == "kick-after-attempt")
            {
                SetKickAfterPlayers(scoringGoblinNetId);
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
                LocalGamePlayerScript.FollowSelectedGoblin(LocalGamePlayerScript.selectGoblin.transform);
                LocalGamePlayerScript.CoinTossControlls(false);
                LocalGamePlayerScript.KickOrReceiveControls(false);
            }
            if (newValue == "kick-after-attempt")
            {
                HideTouchDownText();
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
        RpcTouchDownScored(wasGrey, goblinNetId);
        //SetKickAfterPlayers(scoringGoblinNetId);
        this.scoringGoblinNetId = goblinNetId;

        //update scores:
        if (wasGrey)
            greyScore += 5;
        else
            greenScore += 5;

        yPositionOfKickAfter = yPosition;

        ActivateGameplayControls(false);
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
        while (isGameTimerRunning)
        {
            yield return new WaitForSeconds(1.0f);
            HandleGameTimerUpdate(timeLeftInGame, timeLeftInGame - 1);
        }
        yield break;        
    }
    void HandleGameTimerUpdate(float oldValue, float newValue)
    {
        if (isServer)
            timeLeftInGame = newValue;
        if (isClient)
        {
            minutes = Mathf.Floor(newValue / 60).ToString("0");
            seconds = Mathf.Floor(newValue % 60).ToString("00");
            timerText.text = minutes + ":" + seconds;
        }
    }
    void HandleGreenScoreUpdate(int oldValue, int newValue)
    {
        if (isServer)
            greenScore = newValue;
        if (isClient)
        {
            ScoreGreenText.text = newValue.ToString("00");
        }
    }
    void HandleGreyScoreUpdate(int oldValue, int newValue)
    {
        if (isServer)
            greyScore = newValue;
        if (isClient)
        {
            ScoreGreyText.text = newValue.ToString("00");
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
        touchDownText.gameObject.SetActive(false);
        touchDownTeamText.gameObject.SetActive(false);
    }
    [Server]
    void SetKickAfterPlayers(uint scoringGoblin)
    {
        GoblinScript scoringGoblinScript = NetworkIdentity.spawned[scoringGoblin].GetComponent<GoblinScript>();
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


}
