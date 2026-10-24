using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using System;

public class CoinTossManager : NetworkBehaviour
{
    public static CoinTossManager instance;

    [Header("Local gameplayer stuff")]
    public GameObject LocalGamePlayer;
    public GamePlayer LocalGamePlayerScript;

    [Header("UI Stuff")]
    [SerializeField] GameObject HeadsSelectionArrow;
    [SerializeField] GameObject TailsSelectionArrow;
    [SerializeField] TextMeshProUGUI SelectionBoardText;
    [SerializeField] TextMeshProUGUI coinTossResultsText;
    [SerializeField] GameObject HeadsCoinImageGroup;
    [SerializeField] GameObject TailsCoinImageGroup;
    [SerializeField] GameObject EnterToSubmitGroup;
    [SerializeField] GameObject[] SelectStuffToShowtoCoinSelecter;

    [Header("Kick or Receive UI")]    
    [SerializeField] GameObject ReceiveGroup;
    [SerializeField] GameObject KickGroup;
    [SerializeField] GameObject ReceiveSelectionArrow;
    [SerializeField] GameObject KickSelectionArrow;
    [SerializeField] GameObject[] SelectStuffToShowtoReceiveOrKickSelecter;
    IEnumerator ActivateKickOrReceiveChoice;
    IEnumerator StartKickOffPhase;

    [Header("Coin Toss Results")]
    private readonly SyncVar<bool> _syncDidPlayerSelectCoin = new SyncVar<bool>();
    public bool didPlayerSelectCoin { get => _syncDidPlayerSelectCoin.Value; set => _syncDidPlayerSelectCoin.Value = value; }
    private readonly SyncVar<string> _syncPlayerSelectedCoin = new SyncVar<string>();
    public string playerSelectedCoin { get => _syncPlayerSelectedCoin.Value; set => _syncPlayerSelectedCoin.Value = value; }
    private readonly SyncVar<string> _syncHeadsOrTailsServer = new SyncVar<string>();
    public string headsOrTailsServer { get => _syncHeadsOrTailsServer.Value; set => _syncHeadsOrTailsServer.Value = value; }
    

    [Header("Kick or Receive Results")]
    private readonly SyncVar<bool> _syncDidPlayerSelectKickOrReceive = new SyncVar<bool>();
    public bool didPlayerSelectKickOrReceive { get => _syncDidPlayerSelectKickOrReceive.Value; set => _syncDidPlayerSelectKickOrReceive.Value = value; }
    private readonly SyncVar<string> _syncPlayerKickOrReceive = new SyncVar<string>();
    public string playerKickOrReceive { get => _syncPlayerKickOrReceive.Value; set => _syncPlayerKickOrReceive.Value = value; }

    [Header("Coin Toss Animation Object")]
    [SerializeField] GameObject coinTossAnimationPrefab;
    [SerializeField] GameObject coinTossAnimationObject;
    [SerializeField] Animator myAnimator;
    IEnumerator coinAnimationRoutine;

    [Header("UI to update for gamepad/keyboard controls")]
    [SerializeField] private TextMeshProUGUI SelectHeadsText;
    [SerializeField] private TextMeshProUGUI SelectTailsText;
    [SerializeField] private TextMeshProUGUI EnterToSubmitText;
    [SerializeField] private TextMeshProUGUI SelectReceiveText;
    [SerializeField] private TextMeshProUGUI SelectKickText;

    [Header("Timeout UI Objects")]
    [SerializeField] GameObject TimeoutTimerHolder;
    [SerializeField] TextMeshProUGUI timeoutTimerText;

    [Header("Timeouts: Cointoss")]
    public bool isCoinTossTimerRunning = false;
    IEnumerator timeOutCoinToss;
    private readonly SyncVar<int> _syncCointossTimerTimeLeft = new SyncVar<int>(30);
    private int cointossTimerTimeLeft { get => _syncCointossTimerTimeLeft.Value; set => _syncCointossTimerTimeLeft.Value = value; }

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

    public override void OnStartClient()
    {
        base.OnStartClient();
        StartWithLocalGamePlayer();
        UpdateUIForGamepad(GamepadUIManager.instance.gamepadUI);
    }
    // Needs the local GamePlayer. FishNet 4 can start this scene object before that player, so the player's own
    // OnStartClient calls this again once it exists. Runs once.
    public void StartWithLocalGamePlayer()
    {
        if (LocalGamePlayerScript != null || GameObject.Find("LocalGamePlayer") == null)
            return;
        GetLocalGamePlayer();
    }
    // Start is called before the first frame update
    void Awake()
    {
        _syncHeadsOrTailsServer.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleHeadsOrTailsOnServer(prev, next); };
        _syncCointossTimerTimeLeft.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleCointossTimerTimeLeft(prev, next); };
        MakeInstance();   
    }
    void MakeInstance()
    {
        Debug.Log("CoinTossManager MakeInstance.");
        if (instance == null)
            instance = this;
    }
    // Update is called once per frame
    void Update()
    {
        
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
    public void ActivateSelectStuffToShowtoCoinSelecter(bool activate)
    {
        foreach (GameObject thing in SelectStuffToShowtoCoinSelecter)
            thing.SetActive(activate);
    }
    [Client]
    public void SelectionArrow(string headsOrTails)
    {
        if (headsOrTails == "heads")
        {
            HeadsSelectionArrow.SetActive(true);
            TailsSelectionArrow.SetActive(false);
        }
        else
        {
            HeadsSelectionArrow.SetActive(false);
            TailsSelectionArrow.SetActive(true);
        }
        SoundManager.instance.PlaySound("ui-click", 1.0f);
    }
    public void SetInitialSelectionText(bool isPlayerSelecting)
    {
        if (isPlayerSelecting)
        {
            SelectionBoardText.text = "Grey Team: Select heads or tails";
        }
        else
        {
            SelectionBoardText.text = "Grey Team is selecting heads or tails...";
        }
    }
    public void PlayerSelectedCoin(string headsOrTails)
    {
        ActivateSelectStuffToShowtoCoinSelecter(false);
        SelectionArrow(headsOrTails);
        SelectionBoardText.text = "Grey Team has selected: " + headsOrTails;
    }
    [Server]
    public void ServerPlayerSelectedTheirCoin()
    {
        didPlayerSelectCoin = true;
    }
    [Server]
    public void FlipCoin()
    {
        string[] headsTails = new[]
        { "heads","tails"};
        var rng = new System.Random();

        
        string result = headsTails[rng.Next(headsTails.Length)];
        //string result = "tails";

        Debug.Log("FlipCoin: " + result);
        headsOrTailsServer = result;
    }
    void HandleHeadsOrTailsOnServer(string oldValue, string newValue)
    {
        if (IsServerInitialized)
        {
            StartCoinTossAnimation(newValue);
        }            
        if (IsClientInitialized)
        {
            //coinTossResultsText.gameObject.SetActive(true);
            //coinTossResultsText.text = newValue.ToUpper();
        }
    }
    void StartCoinTossAnimation(string headsOrTails)
    {
        if (!coinTossAnimationObject)
        {
            coinTossAnimationObject = Instantiate(coinTossAnimationPrefab);
            InstanceFinder.ServerManager.Spawn(coinTossAnimationObject);
            myAnimator = coinTossAnimationObject.GetComponent<Animator>();
            RpcCoinResultsText("-");
            coinAnimationRoutine = RunCoinTossAnimation(headsOrTails);
            StartCoroutine(coinAnimationRoutine);
        }
    }
    [ObserversRpc]
    void RpcCoinResultsText(string textResults)
    {
        EnterToSubmitGroup.SetActive(false);
        coinTossResultsText.gameObject.SetActive(true);
        coinTossResultsText.text = textResults.ToUpper();
    }
    [Server]
    public IEnumerator RunCoinTossAnimation(string headsOrTails)
    {
        yield return new WaitForSeconds(1.0f);
        RpcHideHeadsOrTailsChoices(playerSelectedCoin);
        yield return new WaitForSeconds(1.0f);
        DidPlayerWinCoinToss();
        RpcCoinResultsText(headsOrTails);
        if(headsOrTails == "heads")
            myAnimator.SetBool("Heads", true);
        else
            myAnimator.SetBool("Tails", true);
    }
    [ObserversRpc]
    void RpcHideHeadsOrTailsChoices(string headsOrTailsSelected)
    {
        if(headsOrTailsSelected == "tails")
            HeadsCoinImageGroup.SetActive(false);
        else
            TailsCoinImageGroup.SetActive(false);
    }
    [Server]
    void DidPlayerWinCoinToss()
    {
        bool didGreyWinCoinToss;
        if (playerSelectedCoin == headsOrTailsServer)
        {
            Debug.Log("DidPlayerWinCoinToss: Grey team WON coin toss");
            RpcWinnerOfCoinToss(true);
            if (GameplayManager.instance.is1v1)
            {
                foreach (GamePlayer player in Game.GamePlayers)
                {
                    if (player.teamName == "Grey")
                    {
                        player.doesPlayerChooseKickOrReceive = true;
                        player.RpcCoinTossAndKickOrReceiveControllerActivation(false, true);
                        player.coinTossControlsOnServer = false;
                        player.kickOrReceiveControlsOnServer = true;
                    }
                    else
                    {
                        player.doesPlayerChooseKickOrReceive = false;
                        player.RpcCoinTossAndKickOrReceiveControllerActivation(false, false);
                        player.coinTossControlsOnServer = false;
                        player.kickOrReceiveControlsOnServer = false;
                    }
                }
            }
            else // set the team captain as the one to choose kick or receive. Loop through teams and then teamplayers to set the control scheme correctly
            {
                foreach (Team team in TeamManager.instance.teams)
                {
                    if (team.isGrey)
                    {
                        foreach (GamePlayer teamPlayer in team.teamPlayers)
                        {
                            if (teamPlayer == team.captain)
                            {
                                teamPlayer.doesPlayerChooseKickOrReceive = true;
                                teamPlayer.RpcCoinTossAndKickOrReceiveControllerActivation(false, true);
                                teamPlayer.coinTossControlsOnServer = false;
                                teamPlayer.kickOrReceiveControlsOnServer = true;
                            }
                            else
                            {
                                teamPlayer.doesPlayerChooseKickOrReceive = false;
                                teamPlayer.RpcCoinTossAndKickOrReceiveControllerActivation(false, false);
                                teamPlayer.coinTossControlsOnServer = false;
                                teamPlayer.kickOrReceiveControlsOnServer = false;
                            }
                        }
                    }
                    else
                    {
                        foreach (GamePlayer teamPlayer in team.teamPlayers)
                        {
                            teamPlayer.doesPlayerChooseKickOrReceive = false;
                            teamPlayer.RpcCoinTossAndKickOrReceiveControllerActivation(false, false);
                            teamPlayer.coinTossControlsOnServer = false;
                            teamPlayer.kickOrReceiveControlsOnServer = false;
                        }
                    }

                }
            }
            
            didGreyWinCoinToss = true;
        }
        else // set the team captain as the one to choose kick or receive. Loop through teams and then teamplayers to set the control scheme correctly
        {
            RpcWinnerOfCoinToss(false);
            Debug.Log("DidPlayerWinCoinToss: Grey team LOST coin toss");
            if (GameplayManager.instance.is1v1)
            {
                foreach (GamePlayer player in Game.GamePlayers)
                {
                    if (player.teamName == "Grey")
                    {
                        player.doesPlayerChooseKickOrReceive = false;
                        player.RpcCoinTossAndKickOrReceiveControllerActivation(false, false);
                        player.coinTossControlsOnServer = false;
                        player.kickOrReceiveControlsOnServer = false;
                    }
                    else
                    {
                        player.doesPlayerChooseKickOrReceive = true;
                        player.RpcCoinTossAndKickOrReceiveControllerActivation(false, true);
                        player.coinTossControlsOnServer = false;
                        player.kickOrReceiveControlsOnServer = true;
                    }
                }
            }
            else
            {
                foreach (Team team in TeamManager.instance.teams)
                {
                    if (!team.isGrey)
                    {
                        foreach (GamePlayer teamPlayer in team.teamPlayers)
                        {
                            if (teamPlayer == team.captain)
                            {
                                teamPlayer.doesPlayerChooseKickOrReceive = true;
                                teamPlayer.RpcCoinTossAndKickOrReceiveControllerActivation(false, true);
                                teamPlayer.coinTossControlsOnServer = false;
                                teamPlayer.kickOrReceiveControlsOnServer = true;
                            }
                            else
                            {
                                teamPlayer.doesPlayerChooseKickOrReceive = false;
                                teamPlayer.RpcCoinTossAndKickOrReceiveControllerActivation(false, false);
                                teamPlayer.coinTossControlsOnServer = false;
                                teamPlayer.kickOrReceiveControlsOnServer = false;
                            }
                        }
                    }
                    else
                    {
                        foreach (GamePlayer teamPlayer in team.teamPlayers)
                        {
                            teamPlayer.doesPlayerChooseKickOrReceive = false;
                            teamPlayer.RpcCoinTossAndKickOrReceiveControllerActivation(false, false);
                            teamPlayer.coinTossControlsOnServer = false;
                            teamPlayer.kickOrReceiveControlsOnServer = false;
                        }
                    }

                }
            }
            
            didGreyWinCoinToss = false;
        }
        ActivateKickOrReceiveChoice = ActivateKickOrReceive(didGreyWinCoinToss);
        StartCoroutine(ActivateKickOrReceiveChoice);
    }
    [ObserversRpc]
    void RpcWinnerOfCoinToss(bool didSelectingPlayerWin)
    {
        if (didSelectingPlayerWin)
        {
            if (LocalGamePlayerScript.doesPlayerChooseCoin)
            {
                SelectionBoardText.text = "You WON the coin toss!";
            }
            else
                SelectionBoardText.text = "Grey team won the coin toss...";

        }
        else
        {
            if (LocalGamePlayerScript.doesPlayerChooseCoin)
            {
                SelectionBoardText.text = "You lost the coin toss...";
            }
            else
                SelectionBoardText.text = "Grey team LOST the coin toss!";
        }
    }
    [Server]
    public IEnumerator ActivateKickOrReceive(bool didGreyWinCoinToss)
    {
        yield return new WaitForSeconds(2.0f);
        RpcActivateKickOrReceiveGUI(didGreyWinCoinToss);
        InstanceFinder.ServerManager.Despawn(coinTossAnimationObject);
    }
    [ObserversRpc]
    void RpcActivateKickOrReceiveGUI(bool didGreyWinCoinToss)
    {
        HeadsCoinImageGroup.SetActive(false);
        TailsCoinImageGroup.SetActive(false);
        ReceiveGroup.SetActive(true);
        KickGroup.SetActive(true);
        if (didGreyWinCoinToss)
        {
            SelectionBoardText.text = "GREY will choose to kick or receive the ball";
        }
        else
        {
            SelectionBoardText.text = "GREEN will choose to kick or receive the ball";
        }
        coinTossResultsText.text = "";
    }
    public void ActivateSelectStuffToShowtoReceiveOrKickSelecter(bool activate)
    {
        
        foreach (GameObject thing in SelectStuffToShowtoReceiveOrKickSelecter)
            thing.SetActive(activate);
        EnterToSubmitGroup.SetActive(activate);
    }
    [Client]
    public void KickOrReceiveSelectionArrow(string kickOrReceive)
    {
        Debug.Log("KickOrReceiveSelectionArrow: " + kickOrReceive);
        if (kickOrReceive == "receive")
        {
            ReceiveSelectionArrow.SetActive(true);
            KickSelectionArrow.SetActive(false);
        }
        else
        {
            ReceiveSelectionArrow.SetActive(false);
            KickSelectionArrow.SetActive(true);
        }
        SoundManager.instance.PlaySound("ui-click", 1.0f);
    }
    public void PlayerSelectedKickOrReceive(string kickOrReceive, string teamName)
    {
        Debug.Log("PlayerSelectedKickOrReceive: " + kickOrReceive);
        ActivateSelectStuffToShowtoReceiveOrKickSelecter(false);
        KickOrReceiveSelectionArrow(kickOrReceive);
        SelectionBoardText.text = teamName.ToUpper() + " has selected to " + kickOrReceive + " the ball";
    }
    [Server]
    public void ServerPlayerSelectedKickOrReceive()
    {
        didPlayerSelectKickOrReceive = true;
        bool didPlayerChooseReceive = false;

        if (GameplayManager.instance.is1v1)
        {
            foreach (GamePlayer player in Game.GamePlayers)
            {
                if (player.doesPlayerChooseKickOrReceive)
                {
                    if (player.kickOrReceivePlayer == "receive")
                    {
                        GameplayManager.instance.receivingPlayer = player;
                        didPlayerChooseReceive = true;
                    }
                    else
                    {
                        GameplayManager.instance.kickingPlayer = player;
                        didPlayerChooseReceive = false;
                    }
                    break;
                }
            }
            foreach (GamePlayer player in Game.GamePlayers)
            {
                if (!player.doesPlayerChooseKickOrReceive)
                {
                    if (didPlayerChooseReceive)
                    {
                        GameplayManager.instance.kickingPlayer = player;
                    }
                    else
                    {
                        GameplayManager.instance.receivingPlayer = player;
                    }
                    break;
                }
            }
        }
        else // for 3v3 games, set the kickingTeam and receivingTeam variables on gameplay manager. Set the captain of the kicking team to the kickingPlayer
        {
            bool isGreyKickingTeam = false;
            foreach (GamePlayer player in Game.GamePlayers)
            {
                if (player.doesPlayerChooseKickOrReceive)
                {
                    if (player.kickOrReceivePlayer == "receive")
                    {
                        GameplayManager.instance.receivingPlayer = player;
                        didPlayerChooseReceive = true;
                        if (player.isTeamGrey)
                            isGreyKickingTeam = false;
                        else
                            isGreyKickingTeam = true;
                    }
                    else
                    {
                        //Debug.Log("ServerPlayerSelectedKickOrReceive: setting kickingPlayer to: " + player.PlayerName);
                        //GameplayManager.instance.kickingPlayer = player;
                        didPlayerChooseReceive = false;
                        if (player.isTeamGrey)
                            isGreyKickingTeam = true;
                        else
                            isGreyKickingTeam = false;
                    }
                    break;
                }
            }
            if (isGreyKickingTeam)
            {
                Debug.Log("ServerPlayerSelectedKickOrReceive: Grey team is kicking." + isGreyKickingTeam.ToString() + " setting kickingPlayer to kicking team captain is: " + TeamManager.instance.greyTeam.captain.PlayerName);
                GameplayManager.instance.kickingTeam = TeamManager.instance.greyTeam;
                GameplayManager.instance.kickingPlayer = TeamManager.instance.greyTeam.captain;
                GameplayManager.instance.receivingTeam = TeamManager.instance.greenTeam;
                GameplayManager.instance.receivingPlayer = TeamManager.instance.greenTeam.captain;
            }
            else
            {
                Debug.Log("ServerPlayerSelectedKickOrReceive: Green team is kicking." + isGreyKickingTeam.ToString() + " setting kickingPlayer to kicking team captain is: " + TeamManager.instance.greenTeam.captain.PlayerName);
                GameplayManager.instance.kickingTeam = TeamManager.instance.greenTeam;
                GameplayManager.instance.kickingPlayer = TeamManager.instance.greenTeam.captain;
                GameplayManager.instance.receivingTeam = TeamManager.instance.greyTeam;
                GameplayManager.instance.receivingPlayer = TeamManager.instance.greyTeam.captain;
            }
                
        }
        if (GameplayManager.instance.kickingPlayer != null && GameplayManager.instance.receivingPlayer != null)
        {
            GameplayManager.instance.SaveFirstHalfKickingTeam();
            StartKickOffPhase = StartKickOffPhaseRoutine();
            StartCoroutine(StartKickOffPhase);
        }

    }
    [Server]
    public IEnumerator StartKickOffPhaseRoutine()
    {
        yield return new WaitForSeconds(2.0f);
        GameplayManager.instance.gamePhase = "kickoff";
    }
    [Client]
    public void UpdateUIForGamepad(bool usingGamepad)
    {
        if (usingGamepad)
        {
            SelectHeadsText.text = "Lb to Select";
            SelectTailsText.text = "Rb to Select";
            EnterToSubmitText.text = "A to Submit";
            SelectReceiveText.text = "Lb to Select";
            SelectKickText.text = "Rb to Select";
        }
        else
        {
            SelectHeadsText.text = "A to Select";
            SelectTailsText.text = "D to Select";
            EnterToSubmitText.text = "\"Enter\" to Submit";
            SelectReceiveText.text = "A to Select";
            SelectKickText.text = "D to Select";
        }
    }
    public void HandleCointossTimerTimeLeft(int oldValue, int newValue)
    {
        if (IsClientInitialized)
        {
            timeoutTimerText.text = ":" + newValue.ToString();
        }
    }
    IEnumerator TimeoutCoinTossRoutine()
    {
        isCoinTossTimerRunning = true;

        this.cointossTimerTimeLeft = 30;
        RpcActivateTheTimeoutTimer(true);
        yield return new WaitForSeconds(1.0f);
        int timeLeftTracker;
        while (isCoinTossTimerRunning)
        {
            if (GameplayManager.instance.gamePhase != "cointoss")
            {
                isCoinTossTimerRunning = false;
                break;
            }
            yield return new WaitForSeconds(1.0f);

            timeLeftTracker = cointossTimerTimeLeft - 1;

            cointossTimerTimeLeft = timeLeftTracker;
            if (timeLeftTracker <= 0)
            {
                cointossTimerTimeLeft = 0;
                isCoinTossTimerRunning = false;
                // code to block kick automatically?
                try
                {
                    if (!didPlayerSelectCoin)
                    {
                        foreach (GamePlayer player in Game.GamePlayers)
                        {
                            if (player.doesPlayerChooseCoin)
                            {
                                player.RpcForceSelectCoin(player.Owner);
                            }
                        }
                    }
                    else
                    {
                        foreach (GamePlayer player in Game.GamePlayers)
                        {
                            if (player.doesPlayerChooseKickOrReceive)
                            {
                                player.RpcForceSelectKickOrReceive(player.Owner);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("TimeoutCoinTossRoutine: could not access the scoringGoblinNetId. Error: " + e);
                }

                //this.KickAfterAttemptWasBlocked();
                RpcActivateTheTimeoutTimer(false);
            }
        }
        RpcActivateTheTimeoutTimer(false);
        yield break;
    }
    [ObserversRpc]
    void RpcActivateTheTimeoutTimer(bool activate)
    {
        TimeoutTimerHolder.SetActive(activate);
    }
    [Server]
    public void StopTimeoutCointossRoutine()
    {
        if (GameplayManager.instance.isSinglePlayer)
            return;
        if (isCoinTossTimerRunning)
            isCoinTossTimerRunning = false;
        try
        {
            StopCoroutine(timeOutCoinToss);
        }
        catch (Exception e)
        {
            Debug.Log("StopTimeoutKickOffRoutine: Could not stop coroutine. Error: " + e);
        }

        RpcActivateTheTimeoutTimer(false);
    }
    public void StartCoinTossTimeOutTimer()
    {
        if (!GameplayManager.instance.isSinglePlayer)
        {
            if (isCoinTossTimerRunning)
            {
                try
                {
                    isCoinTossTimerRunning = false;
                    StopCoroutine(timeOutCoinToss);
                }
                catch (Exception e)
                {
                    Debug.Log("StartCoinTossTimeOutTimer: Could not access the coroutine. Error: " + e);
                }
            }
            timeOutCoinToss = TimeoutCoinTossRoutine();
            StartCoroutine(timeOutCoinToss);
        }
    }
}
