using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;
using TMPro;

public class LobbyManager : MonoBehaviour
{

    public static LobbyManager instance;

    [Header("Lobby UI Elements")]
    [SerializeField] private GameObject FindLobbiesPanel;
    [SerializeField] private TextMeshProUGUI LobbyNameText;
    [SerializeField] private GameObject ContentPanel;
    [SerializeField] private GameObject PlayerListItemPrefab;
    [SerializeField] private GameObject PlayerListItem3v3Prefab;
    [SerializeField] private Button ReadyUpButton;
    [SerializeField] private Button StartGameButton;

    public bool havePlayerListItemsBeenCreated = false;
    private List<PlayerListItem> playerListItems = new List<PlayerListItem>();
    public GameObject localLobbyPlayerObject;
    public LobbyPlayer localLobbyPlayerScript;

    [Header("1v1 or 3v3")]
    public bool is1v1 = false;


    [Header("1v1 UI")]
    [SerializeField] GameObject panel1v1;

    [Header("3v3 UI")]
    [SerializeField] GameObject panel3v3;
    [SerializeField] GameObject greenContentPanel;
    [SerializeField] GameObject greyContentPanel;
    private List<PlayerListItem> greenPlayerListItems = new List<PlayerListItem>();
    private List<PlayerListItem> greyPlayerListItems = new List<PlayerListItem>();
    [SerializeField] GameObject GreenChangeGoblinButton;
    [SerializeField] GameObject GreyChangeGoblinButton;

    [Header("3v3 Team Stuff?")]
    public List<string> GreenGoblinsAvailable = new List<string>() { "Grenadier", "Berserker", "Skirmisher" };
    public List<string> GreyGoblinsAvailable = new List<string>() { "Grenadier", "Berserker", "Skirmisher" };
    public List<string> GreenGoblinsSelected = new List<string>();
    public List<string> GreyGoblinsSelected = new List<string>();
    public List<LobbyPlayer> GreenTeamMembers = new List<LobbyPlayer>();
    public List<LobbyPlayer> GreyTeamMembers = new List<LobbyPlayer>();

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
        
        ReadyUpButton.GetComponentInChildren<TextMeshProUGUI>().text = "Ready up";
        StartGameButton.gameObject.SetActive(false);
        //FindLobbiesPanel.SetActive(true);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
    public void FindLocalLobbyPlayer()
    {
        localLobbyPlayerObject = GameObject.Find("LocalLobbyPlayer");
        localLobbyPlayerScript = localLobbyPlayerObject.GetComponent<LobbyPlayer>();
        this.is1v1 = localLobbyPlayerScript.is1v1;
        ReadyUpButton.gameObject.SetActive(this.is1v1);
        Activate1v1or3v3Panels();
        /*if (!this.is1v1)
            localLobbyPlayerScript.StartUpdateTeamListsOnLobbyManager();*/
    }
    void Activate1v1or3v3Panels()
    {
        if (is1v1)
        {
            panel1v1.SetActive(true);
            panel3v3.SetActive(false);
        }
        else
        {
            panel1v1.SetActive(false);
            panel3v3.SetActive(true);
            ActivateChangeGoblinButton();
        }
            
    }
    public void UpdateLobbyName()
    {
       /* Debug.Log("UpdateLobbyName");
        currentLobbyId = Game.GetComponent<SteamLobby>().current_lobbyID;
        string lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)currentLobbyId, "name");
        Debug.Log("UpdateLobbyName: new lobby name will be: " + lobbyName);
        //LobbyNameText.SetText(lobbyName);
        //LobbyNameText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(lobbyName);
        //LobbyNameText.GetComponent<TextMeshPro>().text = lobbyName;
        LobbyNameText.SetText(lobbyName);*/
    }
    public void UpdateUI()
    {
        Debug.Log("Executing UpdateUI");
        if (!havePlayerListItemsBeenCreated)
            CreatePlayerListItems();
        if (playerListItems.Count < Game.LobbyPlayers.Count)
            CreateNewPlayerListItems();
        if (playerListItems.Count > Game.LobbyPlayers.Count)
            RemovePlayerListItems();
        if (playerListItems.Count == Game.LobbyPlayers.Count)
            UpdatePlayerListItems();
    }
    private void CreatePlayerListItems()
    {
        Debug.Log("Executing CreatePlayerListItems. This many players to create: " + Game.LobbyPlayers.Count.ToString());
        foreach (LobbyPlayer player in Game.LobbyPlayers)
        {
            if (is1v1 || player.is1v1)
            {
                Debug.Log("CreatePlayerListItems:  1v1 playerlistitem for player: " + player.PlayerName);
                GameObject newPlayerListItem = Instantiate(PlayerListItemPrefab) as GameObject;
                PlayerListItem newPlayerListItemScript = newPlayerListItem.GetComponent<PlayerListItem>();

                newPlayerListItemScript.PlayerName = player.PlayerName;
                newPlayerListItemScript.ConnectionId = player.ConnectionId;
                newPlayerListItemScript.isPlayerReady = player.isPlayerReady;
                //newPlayerListItemScript.playerSteamId = player.playerSteamId;
                newPlayerListItemScript.SetPlayerListItemValues();


                newPlayerListItem.transform.SetParent(ContentPanel.transform);
                newPlayerListItem.transform.localScale = Vector3.one;

                playerListItems.Add(newPlayerListItemScript);
            }
            else
            {
                Debug.Log("CreatePlayerListItems:  3v3 playerlistitem for player: " + player.PlayerName);
                GameObject newPlayerListItem = Instantiate(PlayerListItem3v3Prefab) as GameObject;
                PlayerListItem newPlayerListItemScript = newPlayerListItem.GetComponent<PlayerListItem>();

                newPlayerListItemScript.PlayerName = player.PlayerName;
                newPlayerListItemScript.ConnectionId = player.ConnectionId;
                newPlayerListItemScript.isPlayerReady = player.isPlayerReady;
                //newPlayerListItemScript.playerSteamId = player.playerSteamId;
                newPlayerListItemScript.SetPlayerListItemValues();
                /*if (player.isGoblinSelected && !string.IsNullOrWhiteSpace(player.goblinType))
                {
                    newPlayerListItemScript.ActivateGoblinSelectedText(player.isGoblinSelected);
                    newPlayerListItemScript.SetGoblinSelectedText(player.goblinType);
                }*/

                /*if (player.IsGameLeader)
                {
                    Debug.Log("CreatePlayerListItems:  3v3 playerlistitem player is game leader: " + player.PlayerName);
                    newPlayerListItem.transform.SetParent(greenContentPanel.transform);
                    newPlayerListItem.transform.localScale = Vector3.one;
                    greenPlayerListItems.Add(newPlayerListItemScript);
                    newPlayerListItemScript.SetPlayerTeam(false);
                }
                else
                {
                    if (greenPlayerListItems.Count > greyPlayerListItems.Count)
                    {
                        Debug.Log("CreatePlayerListItems:  3v3 playerlistitem more green team member than grey: "+ greenPlayerListItems.Count.ToString() +" " + greyPlayerListItems.Count.ToString() + " " + player.PlayerName);
                        newPlayerListItem.transform.SetParent(greyContentPanel.transform);
                        newPlayerListItem.transform.localScale = Vector3.one;
                        greyPlayerListItems.Add(newPlayerListItemScript);
                        newPlayerListItemScript.SetPlayerTeam(true);
                    }
                    else if (greenPlayerListItems.Count < greyPlayerListItems.Count)
                    {
                        Debug.Log("CreatePlayerListItems:  3v3 playerlistitem more grey team member than green: " + greenPlayerListItems.Count.ToString() + " " + greyPlayerListItems.Count.ToString() + " " + player.PlayerName);
                        newPlayerListItem.transform.SetParent(greenContentPanel.transform);
                        newPlayerListItem.transform.localScale = Vector3.one;
                        greenPlayerListItems.Add(newPlayerListItemScript);
                        newPlayerListItemScript.SetPlayerTeam(false);
                    }
                    else
                    {
                        Debug.Log("CreatePlayerListItems:  3v3 playerlistitem equal number on both teams: " + greenPlayerListItems.Count.ToString() + " " + greyPlayerListItems.Count.ToString() + " " + player.PlayerName);
                        string[] headsTails = new[]
                        { "green","grey"};
                        var rng = new System.Random();
                        string result = headsTails[rng.Next(headsTails.Length)];
                        if (result == "green")
                        {
                            newPlayerListItem.transform.SetParent(greenContentPanel.transform);
                            newPlayerListItem.transform.localScale = Vector3.one;
                            greenPlayerListItems.Add(newPlayerListItemScript);
                            newPlayerListItemScript.SetPlayerTeam(false);
                        }
                        else
                        {
                            newPlayerListItem.transform.SetParent(greyContentPanel.transform);
                            newPlayerListItem.transform.localScale = Vector3.one;
                            greyPlayerListItems.Add(newPlayerListItemScript);
                            newPlayerListItemScript.SetPlayerTeam(true);
                        }
                    }
                }*/
                /*newPlayerListItem.transform.SetParent(ContentPanel.transform);
                newPlayerListItem.transform.localScale = Vector3.one;*/

                if (player.isTeamGrey)
                {
                    newPlayerListItem.transform.SetParent(greyContentPanel.transform);
                    newPlayerListItem.transform.localScale = Vector3.one;
                    if (!greyPlayerListItems.Contains(newPlayerListItemScript))
                        greyPlayerListItems.Add(newPlayerListItemScript);
                }
                else
                {
                    newPlayerListItem.transform.SetParent(greenContentPanel.transform);
                    newPlayerListItem.transform.localScale = Vector3.one;
                    if (!greenPlayerListItems.Contains(newPlayerListItemScript))
                        greenPlayerListItems.Add(newPlayerListItemScript);
                }
                playerListItems.Add(newPlayerListItemScript);
            }
            
        }
        havePlayerListItemsBeenCreated = true;
    }
    private void CreateNewPlayerListItems()
    {
        Debug.Log("Executing CreateNewPlayerListItems");
        foreach (LobbyPlayer player in Game.LobbyPlayers)
        {
            if (!playerListItems.Any(b => b.ConnectionId == player.ConnectionId))
            {
                if (is1v1)
                {
                    Debug.Log("CreateNewPlayerListItems: Player not found in 1v1 playerListItems: " + player.PlayerName);
                    GameObject newPlayerListItem = Instantiate(PlayerListItemPrefab) as GameObject;
                    PlayerListItem newPlayerListItemScript = newPlayerListItem.GetComponent<PlayerListItem>();

                    newPlayerListItemScript.PlayerName = player.PlayerName;
                    newPlayerListItemScript.ConnectionId = player.ConnectionId;
                    newPlayerListItemScript.isPlayerReady = player.isPlayerReady;
                    //newPlayerListItemScript.playerSteamId = player.playerSteamId;
                    newPlayerListItemScript.SetPlayerListItemValues();


                    newPlayerListItem.transform.SetParent(ContentPanel.transform);
                    newPlayerListItem.transform.localScale = Vector3.one;

                    playerListItems.Add(newPlayerListItemScript);
                }
                else
                {
                    Debug.Log("CreateNewPlayerListItems: Player not found in 3v3 playerListItems: " + player.PlayerName);
                    GameObject newPlayerListItem = Instantiate(PlayerListItem3v3Prefab) as GameObject;
                    PlayerListItem newPlayerListItemScript = newPlayerListItem.GetComponent<PlayerListItem>();

                    newPlayerListItemScript.PlayerName = player.PlayerName;
                    newPlayerListItemScript.ConnectionId = player.ConnectionId;
                    newPlayerListItemScript.isPlayerReady = player.isPlayerReady;
                    //newPlayerListItemScript.playerSteamId = player.playerSteamId;
                    newPlayerListItemScript.SetPlayerListItemValues();
                    playerListItems.Add(newPlayerListItemScript);
                    /*if (player.isTeamGrey)
                    {
                        newPlayerListItem.transform.SetParent(greyContentPanel.transform);
                        newPlayerListItem.transform.localScale = Vector3.one;
                        if (!greyPlayerListItems.Contains(newPlayerListItemScript))
                            greyPlayerListItems.Add(newPlayerListItemScript);
                    }
                    else
                    {
                        newPlayerListItem.transform.SetParent(greenContentPanel.transform);
                        newPlayerListItem.transform.localScale = Vector3.one;
                        if (!greenPlayerListItems.Contains(newPlayerListItemScript))
                            greenPlayerListItems.Add(newPlayerListItemScript);
                    }*/
                    if (player.IsGameLeader)
                    {
                        Debug.Log("CreateNewPlayerListItems:  3v3 playerlistitem player is game leader: " + player.PlayerName);
                        newPlayerListItem.transform.SetParent(greenContentPanel.transform);
                        newPlayerListItem.transform.localScale = Vector3.one;
                        if (!greenPlayerListItems.Contains(newPlayerListItemScript))
                            greenPlayerListItems.Add(newPlayerListItemScript);
                        newPlayerListItemScript.SetPlayerTeam(false);
                    }
                    /*else
                    {
                        if (greenPlayerListItems.Count > greyPlayerListItems.Count)
                        {
                            Debug.Log("CreateNewPlayerListItems:  3v3 playerlistitem more green team member than grey: " + greenPlayerListItems.Count.ToString() + " " + greyPlayerListItems.Count.ToString() + " " + player.PlayerName);
                            newPlayerListItem.transform.SetParent(greyContentPanel.transform);
                            newPlayerListItem.transform.localScale = Vector3.one;
                            if (!greyPlayerListItems.Contains(newPlayerListItemScript))
                                greyPlayerListItems.Add(newPlayerListItemScript);
                            newPlayerListItemScript.SetPlayerTeam(true);
                        }
                        else if (greenPlayerListItems.Count < greyPlayerListItems.Count)
                        {
                            Debug.Log("CreateNewPlayerListItems:  3v3 playerlistitem more grey team member than green: " + greenPlayerListItems.Count.ToString() + " " + greyPlayerListItems.Count.ToString() + " " + player.PlayerName);
                            newPlayerListItem.transform.SetParent(greenContentPanel.transform);
                            newPlayerListItem.transform.localScale = Vector3.one;
                            if (!greenPlayerListItems.Contains(newPlayerListItemScript))
                                greenPlayerListItems.Add(newPlayerListItemScript);
                            newPlayerListItemScript.SetPlayerTeam(false);
                        }
                        else
                        {
                            Debug.Log("CreateNewPlayerListItems:  3v3 playerlistitem equal number on both teams: " + greenPlayerListItems.Count.ToString() + " " + greyPlayerListItems.Count.ToString() + " " + player.PlayerName);
                            string[] headsTails = new[]
                            { "green","grey"};
                            var rng = new System.Random();
                            string result = headsTails[rng.Next(headsTails.Length)];
                            if (result == "green")
                            {
                                newPlayerListItem.transform.SetParent(greenContentPanel.transform);
                                newPlayerListItem.transform.localScale = Vector3.one;
                                if (!greenPlayerListItems.Contains(newPlayerListItemScript))
                                    greenPlayerListItems.Add(newPlayerListItemScript);
                                newPlayerListItemScript.SetPlayerTeam(false);
                            }
                            else
                            {
                                newPlayerListItem.transform.SetParent(greyContentPanel.transform);
                                newPlayerListItem.transform.localScale = Vector3.one;
                                if (!greyPlayerListItems.Contains(newPlayerListItemScript))
                                    greyPlayerListItems.Add(newPlayerListItemScript);
                                newPlayerListItemScript.SetPlayerTeam(true);
                            }
                        }
                    }*/
                    /*newPlayerListItem.transform.SetParent(ContentPanel.transform);
                    newPlayerListItem.transform.localScale = Vector3.one;*/

                    //playerListItems.Add(newPlayerListItemScript);
                }
                
            }
        }

    }
    private void RemovePlayerListItems()
    {
        List<PlayerListItem> playerListItemsToRemove = new List<PlayerListItem>();
        foreach (PlayerListItem playerListItem in playerListItems)
        {
            if (!Game.LobbyPlayers.Any(b => b.ConnectionId == playerListItem.ConnectionId))
            {
                Debug.Log("RemovePlayerListItems: player list item fro connection id: " + playerListItem.ConnectionId.ToString() + " does not exist in the game players list");
                playerListItemsToRemove.Add(playerListItem);
            }
        }
        if (playerListItemsToRemove.Count > 0)
        {
            foreach (PlayerListItem playerListItemToRemove in playerListItemsToRemove)
            {
                GameObject playerListItemToRemoveObject = playerListItemToRemove.gameObject;
                playerListItems.Remove(playerListItemToRemove);
                Destroy(playerListItemToRemoveObject);
                playerListItemToRemoveObject = null;
            }
        }
    }
    private void UpdatePlayerListItems()
    {
        Debug.Log("Executing UpdatePlayerListItems");
        foreach (LobbyPlayer player in Game.LobbyPlayers)
        {
            foreach (PlayerListItem playerListItemScript in playerListItems)
            {
                if (playerListItemScript.ConnectionId == player.ConnectionId)
                {
                    playerListItemScript.PlayerName = player.PlayerName;
                    playerListItemScript.isPlayerReady = player.isPlayerReady;
                    playerListItemScript.SetPlayerListItemValues();

                    if (player == localLobbyPlayerScript)
                        ChangeReadyUpButtonText();
                    // If a 3v3 game, check if player changed teams
                    if (!is1v1)
                    {
                        if (player.isTeamGrey != playerListItemScript.isTeamGrey)
                        {
                            Debug.Log("UpdatePlayerListItems: ConnectionId: " +player.ConnectionId.ToString() + " Player " + player.PlayerName + "'s team is grey: " + player.isTeamGrey.ToString() + " and their list item is: " + playerListItemScript.isTeamGrey.ToString() + " Updating...");
                            playerListItemScript.isTeamGrey = player.isTeamGrey;
                            if (player.isTeamGrey)
                            {
                                if (greenPlayerListItems.Contains(playerListItemScript))
                                    greenPlayerListItems.Remove(playerListItemScript);
                                if (!greyPlayerListItems.Contains(playerListItemScript))
                                    greyPlayerListItems.Add(playerListItemScript);
                                playerListItemScript.gameObject.transform.SetParent(greyContentPanel.transform);
                                playerListItemScript.gameObject.transform.localScale = Vector3.one;
                            }
                            else
                            {
                                if (!greenPlayerListItems.Contains(playerListItemScript))
                                    greenPlayerListItems.Add(playerListItemScript);
                                if (greyPlayerListItems.Contains(playerListItemScript))
                                    greyPlayerListItems.Remove(playerListItemScript);
                                playerListItemScript.gameObject.transform.SetParent(greenContentPanel.transform);
                                playerListItemScript.gameObject.transform.localScale = Vector3.one;
                            }
                            ActivateChangeGoblinButton();
                        }
                        if (player.isGoblinSelected && !string.IsNullOrWhiteSpace(player.goblinType))
                        {
                            playerListItemScript.ActivateGoblinSelectedText(player.isGoblinSelected);
                            playerListItemScript.SetGoblinSelectedText(player.goblinType);
                        }
                        if (playerListItemScript.gameObject.transform.parent == null)
                        {
                            if (player.isTeamGrey)
                            {
                                playerListItemScript.gameObject.transform.SetParent(greyContentPanel.transform);
                                playerListItemScript.gameObject.transform.localScale = Vector3.one;
                                if (!greyPlayerListItems.Contains(playerListItemScript))
                                    greyPlayerListItems.Add(playerListItemScript);
                            }
                            else
                            {
                                playerListItemScript.gameObject.transform.SetParent(greenContentPanel.transform);
                                playerListItemScript.gameObject.transform.localScale = Vector3.one;
                                if (!greenPlayerListItems.Contains(playerListItemScript))
                                    greenPlayerListItems.Add(playerListItemScript);
                            }
                        }
                    }
                }
            }
        }
        CheckIfAllPlayersAreReady();
    }
    public void PlayerReadyUp()
    {
        Debug.Log("Executing PlayerReadyUp");
        localLobbyPlayerScript.ChangeReadyStatus();
    }
    void ChangeReadyUpButtonText()
    {
        if (localLobbyPlayerScript.isPlayerReady)
            ReadyUpButton.GetComponentInChildren<TextMeshProUGUI>().text = "Unready";
        else
            ReadyUpButton.GetComponentInChildren<TextMeshProUGUI>().text = "Ready Up";
    }
    void CheckIfAllPlayersAreReady()
    {
        Debug.Log("Executing CheckIfAllPlayersAreReady");
        bool areAllPlayersReady = false;
        foreach (LobbyPlayer player in Game.LobbyPlayers)
        {
            if (player.isPlayerReady)
            {
                areAllPlayersReady = true;
            }
            else
            {
                Debug.Log("CheckIfAllPlayersAreReady: Not all players are ready. Waiting for: " + player.PlayerName);
                areAllPlayersReady = false;
                break;
            }
        }
        if (areAllPlayersReady && Game.LobbyPlayers.Count >= Game.minPlayers)
        {
            Debug.Log("CheckIfAllPlayersAreReady: All players are ready!");
            if (localLobbyPlayerScript.IsGameLeader)
            {
                Debug.Log("CheckIfAllPlayersAreReady: Local player is the game leader. They can start the game now.");
                StartGameButton.gameObject.SetActive(true);
            }
        }
        else
        {
            if (StartGameButton.gameObject.activeInHierarchy)
                StartGameButton.gameObject.SetActive(false);
        }
    }
    public void JoinTeamGreen()
    {
        Debug.Log("JoinTeamGreen");
        localLobbyPlayerScript.UpdateTeam(false);
    }
    public void JoinTeamGrey()
    {
        Debug.Log("JoinTeamGrey");
        localLobbyPlayerScript.UpdateTeam(true);
    }
    public void ActivateChangeGoblinButton()
    {
        Debug.Log("ActivateChangeGoblinButton. Is local lobby player grey? " + localLobbyPlayerScript.isTeamGrey.ToString());
        if (localLobbyPlayerScript.isGoblinSelected)
        {
            ReadyUpButton.gameObject.SetActive(true);
            if (localLobbyPlayerScript.isTeamGrey)
            {
                GreyChangeGoblinButton.SetActive(true);
                GreyChangeGoblinButton.GetComponentInChildren<TextMeshProUGUI>().text = "Change Goblin";
                GreenChangeGoblinButton.SetActive(false);
            }
            else
            {
                GreyChangeGoblinButton.SetActive(false);
                GreenChangeGoblinButton.SetActive(true);
                GreenChangeGoblinButton.GetComponentInChildren<TextMeshProUGUI>().text = "Change Goblin";
            }
        }
        else
        {
            ReadyUpButton.gameObject.SetActive(false);
            if (localLobbyPlayerScript.isTeamGrey)
            {
                GreyChangeGoblinButton.SetActive(true);
                GreyChangeGoblinButton.GetComponentInChildren<TextMeshProUGUI>().text = "Select Goblin";
                GreenChangeGoblinButton.SetActive(false);
            }
            else
            {
                GreyChangeGoblinButton.SetActive(false);
                GreenChangeGoblinButton.SetActive(true);
                GreenChangeGoblinButton.GetComponentInChildren<TextMeshProUGUI>().text = "Select Goblin";
            }
        }
    }
    public void SelectGoblinButtonPressed()
    {
        Debug.Log("SelectGoblinButtonPressed");
        if (localLobbyPlayerScript.isGoblinSelected)
        {
            localLobbyPlayerScript.UnselectGoblin();
        }
        else
        {
            localLobbyPlayerScript.myPlayerListItem.SelectGoblinButton();
        }
    }
    public void UpdateGoblinSelectedTextOnPlayerListItems(LobbyPlayer player)
    {
        foreach (PlayerListItem playerListItem in playerListItems)
        {
            if (player.ConnectionId == playerListItem.ConnectionId && player.PlayerName == playerListItem.PlayerName)
            {
                playerListItem.SetGoblinSelectedText(player.goblinType);
                //playerListItem.ActivateGoblinSelectedText(player.isGoblinSelected);
            }
        }
    }
    public void UpdateGoblinSelectedBoolOnPlayerListItems(LobbyPlayer player)
    {
        foreach (PlayerListItem playerListItem in playerListItems)
        {
            if (player.ConnectionId == playerListItem.ConnectionId && player.PlayerName == playerListItem.PlayerName)
            {
                //playerListItem.SetGoblinSelectedText(player.goblinType);
                playerListItem.ActivateGoblinSelectedText(player.isGoblinSelected);
            }
        }
    }
    
    public void DestroyPlayerListItems()
    {
        foreach (PlayerListItem playerListItem in playerListItems)
        {
            GameObject playerListItemObject = playerListItem.gameObject;
            Destroy(playerListItemObject);
            playerListItemObject = null;
        }
        playerListItems.Clear();
    }
    public void StartGame()
    {
        localLobbyPlayerScript.CanLobbyStartGame();
    }
    public void PlayerQuitLobby()
    {
        localLobbyPlayerScript.UnselectGoblin();
        localLobbyPlayerScript.QuitLobby();
    }
}
