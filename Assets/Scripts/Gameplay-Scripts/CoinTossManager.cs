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

    [Header("Coin Toss Results")]
    [SyncVar] public bool didPlayerSelectCoin;
    [SyncVar(hook = nameof(HandleHeadsOrTailsOnServer))] public string headsOrTailsServer;


    public override void OnStartClient()
    {
        base.OnStartClient();
        GetLocalGamePlayer();
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
        HandleHeadsOrTailsOnServer(headsOrTailsServer, result);
    }
    void HandleHeadsOrTailsOnServer(string oldValue, string newValue)
    {
        if (isServer)
            headsOrTailsServer = newValue;
        if (isClient)
        {
            coinTossResultsText.gameObject.SetActive(true);
            coinTossResultsText.text = newValue.ToUpper();
        }
    }
}
