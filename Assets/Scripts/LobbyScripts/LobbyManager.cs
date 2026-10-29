using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;
using System;
using Steamworks;

public class LobbyManager : MonoBehaviour
{

    public static LobbyManager instance;
    [SerializeField] EventSystem eventSystem;

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
    public bool isSinglePlayer;


    [Header("1v1 UI")]
    [SerializeField] GameObject panel1v1;

    [Header("3v3 UI")]
    [SerializeField] GameObject panel3v3;
    [SerializeField] GameObject greenContentPanel;
    [SerializeField] GameObject greyContentPanel;
    // The "+ ADD BOT" row the host sees for each empty 3v3 slot.
    [SerializeField] GameObject AddBotListItemPrefab;
    private List<GameObject> addBotRows = new List<GameObject>();
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

    [Header("Single Player UI")]
    [SerializeField] GameObject selectGreenButton;
    [SerializeField] GameObject singlePlayerPanel;
    [SerializeField] Image greenGoblinImage;
    [SerializeField] Image greyGoblinImage;
    [SerializeField] Sprite greenGoblinNotSelected;
    [SerializeField] Sprite greyGoblinNotSelected;
    [SerializeField] Sprite greenGoblinSelected;
    [SerializeField] Sprite greyGoblinSelected;
    public bool isTeamSelectedYet = false;

    public ulong currentLobbyId;

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
        this.isSinglePlayer = localLobbyPlayerScript.isSinglePlayer;
        ReadyUpButton.gameObject.SetActive(this.is1v1);
        Activate1v1or3v3Panels();
        /*if (!this.is1v1)
            localLobbyPlayerScript.StartUpdateTeamListsOnLobbyManager();*/
    }
    void Activate1v1or3v3Panels()
    {
        if (is1v1 && !isSinglePlayer)
        {
            panel1v1.SetActive(true);
            panel3v3.SetActive(false);
            singlePlayerPanel.SetActive(false);
        }
        else if (isSinglePlayer && !is1v1)
        {
            panel1v1.SetActive(false);
            panel3v3.SetActive(false);
            ReadyUpButton.gameObject.SetActive(false);
            StartGameButton.gameObject.SetActive(false);
            InitializeSinglePlayerImages();
            singlePlayerPanel.SetActive(true);
            eventSystem.SetSelectedGameObject(selectGreenButton, new BaseEventData(eventSystem));
        }
        else
        {
            panel1v1.SetActive(false);
            panel3v3.SetActive(true);
            singlePlayerPanel.SetActive(false);
            ActivateChangeGoblinButton();
        }
            
    }
    public void UpdateLobbyName()
    {
        Debug.Log("UpdateLobbyName");
        // A dev session and single player have no Steam lobby to read a name from, and GetLobbyData throws outright
        // when Steam isn't running at all. This is called from LobbyPlayer.OnStartClient, where an exception aborts
        // the rest of FishNet's start callbacks, so the player never reached Game.LobbyPlayers and the lobby came up
        // empty.
        if (NetworkManagerGRF.singleton != null && !NetworkManagerGRF.singleton.HasSteamLobby)
        {
            LobbyNameText.SetText(NetworkManagerGRF.singleton.isDevSession ? "Local test lobby" : "Single player");
            return;
        }
        currentLobbyId = SteamLobby.instance.CurrentLobbyID;
        string lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)currentLobbyId, "name");
        Debug.Log("UpdateLobbyName: new lobby name will be: " + lobbyName);
        //LobbyNameText.SetText(lobbyName);
        //LobbyNameText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(lobbyName);
        //LobbyNameText.GetComponent<TextMeshPro>().text = lobbyName;
        LobbyNameText.SetText(lobbyName);
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
        SortTeamRows();
        UpdateAddBotRows();
    }
    // One "+ ADD BOT" row per empty slot on each team, for the host only. Rebuilt from scratch on every refresh, which
    // is simpler than reconciling and costs nothing at six rows.
    public void UpdateAddBotRows()
    {
        foreach (GameObject row in addBotRows)
        {
            if (row != null)
                Destroy(row);
        }
        addBotRows.Clear();
        if (AddBotListItemPrefab == null || this.is1v1 || this.isSinglePlayer)
            return;
        // Only the host can add a bot, so only the host is offered the row.
        if (localLobbyPlayerScript == null || !localLobbyPlayerScript.IsGameLeader)
            return;
        CreateAddBotRowsForTeam(false, greenContentPanel);
        CreateAddBotRowsForTeam(true, greyContentPanel);
    }
    void CreateAddBotRowsForTeam(bool isTeamGrey, GameObject contentPanel)
    {
        if (contentPanel == null)
            return;
        int freeSlots = 3 - Game.CountOnTeam(isTeamGrey);
        for (int i = 0; i < freeSlots; i++)
        {
            GameObject row = Instantiate(AddBotListItemPrefab);
            row.transform.SetParent(contentPanel.transform, false);
            row.transform.localScale = Vector3.one;
            // Below the players and the bots.
            row.transform.SetAsLastSibling();
            bool rowIsTeamGrey = isTeamGrey;
            Button rowButton = row.GetComponent<Button>();
            if (rowButton != null)
                rowButton.onClick.AddListener(() => AddBotPressed(rowIsTeamGrey));
            addBotRows.Add(row);
        }
    }
    // Only the host has these rows, and the host is the server, so the bot can be added directly.
    public void AddBotPressed(bool isTeamGrey)
    {
        Game.AddBot(isTeamGrey);
    }
    // Players always sit above bots in a team's list, whatever order they arrived in. Row order is sibling order, so
    // this just re-indexes: every human first, then every bot.
    void SortTeamRows()
    {
        SortRowsPlayersFirst(greenPlayerListItems);
        SortRowsPlayersFirst(greyPlayerListItems);
    }
    void SortRowsPlayersFirst(List<PlayerListItem> rows)
    {
        int index = 0;
        foreach (PlayerListItem row in rows)
        {
            if (row != null && !row.isBot)
                row.transform.SetSiblingIndex(index++);
        }
        foreach (PlayerListItem row in rows)
        {
            if (row != null && row.isBot)
                row.transform.SetSiblingIndex(index++);
        }
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
                newPlayerListItemScript.lobbyPlayerObjectId = player.ObjectId;
                newPlayerListItemScript.isBot = player.isBot;
                newPlayerListItemScript.isPlayerReady = player.isPlayerReady;
                newPlayerListItemScript.playerSteamId = player.playerSteamId;
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
                newPlayerListItemScript.lobbyPlayerObjectId = player.ObjectId;
                newPlayerListItemScript.isBot = player.isBot;
                newPlayerListItemScript.isPlayerReady = player.isPlayerReady;
                newPlayerListItemScript.playerSteamId = player.playerSteamId;
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
            if (!playerListItems.Any(b => b.lobbyPlayerObjectId == player.ObjectId))
            {
                if (is1v1)
                {
                    Debug.Log("CreateNewPlayerListItems: Player not found in 1v1 playerListItems: " + player.PlayerName);
                    GameObject newPlayerListItem = Instantiate(PlayerListItemPrefab) as GameObject;
                    PlayerListItem newPlayerListItemScript = newPlayerListItem.GetComponent<PlayerListItem>();

                    newPlayerListItemScript.PlayerName = player.PlayerName;
                    newPlayerListItemScript.ConnectionId = player.ConnectionId;
                    newPlayerListItemScript.lobbyPlayerObjectId = player.ObjectId;
                    newPlayerListItemScript.isBot = player.isBot;
                    newPlayerListItemScript.isPlayerReady = player.isPlayerReady;
                    newPlayerListItemScript.playerSteamId = player.playerSteamId;
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
                    newPlayerListItemScript.lobbyPlayerObjectId = player.ObjectId;
                    newPlayerListItemScript.isBot = player.isBot;
                    newPlayerListItemScript.isPlayerReady = player.isPlayerReady;
                    newPlayerListItemScript.playerSteamId = player.playerSteamId;
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
            if (!Game.LobbyPlayers.Any(b => b.ObjectId == playerListItem.lobbyPlayerObjectId))
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
                // The team lists held the row too, and only playerListItems was being cleaned, so every removal left a
                // destroyed entry behind in green or grey. Rare while only players left; routine once bots come and go.
                greenPlayerListItems.Remove(playerListItemToRemove);
                greyPlayerListItems.Remove(playerListItemToRemove);
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
                if (playerListItemScript.lobbyPlayerObjectId == player.ObjectId)
                {
                    playerListItemScript.PlayerName = player.PlayerName;
                    playerListItemScript.isPlayerReady = player.isPlayerReady;
                    playerListItemScript.isBot = player.isBot;
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
            try
            {
                if (localLobbyPlayerScript.IsGameLeader)
                {
                    Debug.Log("CheckIfAllPlayersAreReady: Local player is the game leader. They can start the game now.");
                    StartGameButton.gameObject.SetActive(true);
                }
            }
            catch (Exception e)
            {
                Debug.Log("CheckIfAllPlayersAreReady: error accessing localLobbyPlayerScript. Error: " + e);
            }
            
        }
        else
        {
            if (StartGameButton.gameObject.activeInHierarchy)
                StartGameButton.gameObject.SetActive(false);
        }
    }
    // Bots do not keep a player out of a team, so admission counts humans only. Pass a player as "ignore" to leave them
    // out: a player deciding which team to join must not count itself, since it sits on the default team until it has
    // decided and would otherwise tip its own balance.
    public int HumansOnTeam(bool isGrey, LobbyPlayer ignore = null)
    {
        int humans = 0;
        foreach (LobbyPlayer player in Game.LobbyPlayers)
        {
            if (player != null && player != ignore && !player.isBot && player.isTeamGrey == isGrey)
                humans++;
        }
        return humans;
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
        try
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
        catch (Exception e)
        {
            Debug.Log("ActivateChangeGoblinButton: error accessing localLobbyPlayerScript. Error: " + e);
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
            if (player.ObjectId == playerListItem.lobbyPlayerObjectId)
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
            if (player.ObjectId == playerListItem.lobbyPlayerObjectId)
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
        try
        {
            localLobbyPlayerScript.UnselectGoblin();
        }
        catch (Exception e)
        {
            Debug.Log("PlayerQuitLobby: failed to run localLobbyPlayerScript.UnselectGoblin(). Error: " + e);
        }
        
        localLobbyPlayerScript.QuitLobby();
    }
    void InitializeSinglePlayerImages()
    {
        greenGoblinImage.sprite = greenGoblinNotSelected;
        greyGoblinImage.sprite = greyGoblinNotSelected;
        isTeamSelectedYet = false;
    }
    public void SinglePlayerSelectGreen()
    {
        greenGoblinImage.sprite = greenGoblinSelected;
        greyGoblinImage.sprite = greyGoblinNotSelected;
        localLobbyPlayerScript.isTeamGrey = false;
        localLobbyPlayerScript.isPlayerReady = true;
        isTeamSelectedYet = true;
        StartGameButton.gameObject.SetActive(true);
    }
    public void SinglePlayerSelectGrey()
    {
        greenGoblinImage.sprite = greenGoblinNotSelected;
        greyGoblinImage.sprite = greyGoblinSelected;
        localLobbyPlayerScript.isTeamGrey = true;
        localLobbyPlayerScript.isPlayerReady = true;
        isTeamSelectedYet = true;
        StartGameButton.gameObject.SetActive(true);
    }
}
