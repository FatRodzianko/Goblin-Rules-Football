using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

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
    [SyncVar] public bool didPlayerSelectCoin;
    [SyncVar] public string playerSelectedCoin;
    [SyncVar(hook = nameof(HandleHeadsOrTailsOnServer))] public string headsOrTailsServer;
    

    [Header("Kick or Receive Results")]
    [SyncVar] public bool didPlayerSelectKickOrReceive;
    [SyncVar] public string playerKickOrReceive;

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
        GetLocalGamePlayer();
        UpdateUIForGamepad(GamepadUIManager.instance.gamepadUI);
    }
    // Start is called before the first frame update
    void Awake()
    {
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

        Debug.Log("FlipCoin: " + result);
        HandleHeadsOrTailsOnServer(headsOrTailsServer, "tails");
    }
    void HandleHeadsOrTailsOnServer(string oldValue, string newValue)
    {
        if (isServer)
        {
            headsOrTailsServer = newValue;
            StartCoinTossAnimation(newValue);
        }            
        if (isClient)
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
            NetworkServer.Spawn(coinTossAnimationObject);
            myAnimator = coinTossAnimationObject.GetComponent<Animator>();
            RpcCoinResultsText("-");
            coinAnimationRoutine = RunCoinTossAnimation(headsOrTails);
            StartCoroutine(coinAnimationRoutine);
        }
    }
    [ClientRpc]
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
    [ClientRpc]
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
            foreach (GamePlayer player in Game.GamePlayers)
            {
                if (player.teamName == "Grey")
                {                    
                    player.HandleDoesPlayerChooseKickOrReceive(player.doesPlayerChooseKickOrReceive, true);
                    player.RpcCoinTossAndKickOrReceiveControllerActivation(false, true);
                }
                else
                {
                    player.HandleDoesPlayerChooseKickOrReceive(player.doesPlayerChooseKickOrReceive, false);
                    player.RpcCoinTossAndKickOrReceiveControllerActivation(false, false);
                }
            }
            didGreyWinCoinToss = true;
        }
        else
        {
            RpcWinnerOfCoinToss(false);
            Debug.Log("DidPlayerWinCoinToss: Grey team LOST coin toss");
            foreach (GamePlayer player in Game.GamePlayers)
            {
                if (player.teamName == "Grey")
                {
                    player.HandleDoesPlayerChooseKickOrReceive(player.doesPlayerChooseKickOrReceive, false);
                    player.RpcCoinTossAndKickOrReceiveControllerActivation(false, false);
                }
                else
                {
                    player.HandleDoesPlayerChooseKickOrReceive(player.doesPlayerChooseKickOrReceive, true);
                    player.RpcCoinTossAndKickOrReceiveControllerActivation(false, true);
                }
            }
            didGreyWinCoinToss = false;
        }
        ActivateKickOrReceiveChoice = ActivateKickOrReceive(didGreyWinCoinToss);
        StartCoroutine(ActivateKickOrReceiveChoice);
    }
    [ClientRpc]
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
        NetworkServer.Destroy(coinTossAnimationObject);
    }
    [ClientRpc]
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
    }
    public void PlayerSelectedKickOrReceive(string kickOrReceive, string teamName)
    {
        ActivateSelectStuffToShowtoReceiveOrKickSelecter(false);
        KickOrReceiveSelectionArrow(kickOrReceive);
        SelectionBoardText.text = teamName.ToUpper() + " has selected to " + kickOrReceive + " the ball";
    }
    [Server]
    public void ServerPlayerSelectedKickOrReceive()
    {
        didPlayerSelectKickOrReceive = true;
        bool didPlayerChooseReceive = false;
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
        if (GameplayManager.instance.kickingPlayer != null && GameplayManager.instance.receivingPlayer != null)
        {
            StartKickOffPhase = StartKickOffPhaseRoutine();
            StartCoroutine(StartKickOffPhase);
        }

    }
    [Server]
    public IEnumerator StartKickOffPhaseRoutine()
    {
        yield return new WaitForSeconds(2.0f);
        GameplayManager.instance.HandleGamePhase(GameplayManager.instance.gamePhase, "kickoff");
    }
    [Client]
    void UpdateUIForGamepad(bool usingGamepad)
    {
        if (usingGamepad)
        {
            SelectHeadsText.text = "<- to Select";
            SelectTailsText.text = "-> to Select";
            EnterToSubmitText.text = "A to Submit";
            SelectReceiveText.text = "<- to Select";
            SelectKickText.text = "-> to Select";
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
}
