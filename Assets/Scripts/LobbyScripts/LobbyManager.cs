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
    [SerializeField] private Button ReadyUpButton;
    [SerializeField] private Button StartGameButton;

    public bool havePlayerListItemsBeenCreated = false;
    private List<PlayerListItem> playerListItems = new List<PlayerListItem>();
    public GameObject localLobbyPlayerObject;
    public LobbyPlayer localLobbyPlayerScript;

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
        ReadyUpButton.gameObject.SetActive(true);
        ReadyUpButton.GetComponentInChildren<TextMeshProUGUI>().text = "Ready up";
        StartGameButton.gameObject.SetActive(false);
        FindLobbiesPanel.SetActive(true);
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
            Debug.Log("CreatePlayerListItems: Creating playerlistitem for player: " + player.PlayerName);
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
        havePlayerListItemsBeenCreated = true;
    }
    private void CreateNewPlayerListItems()
    {
        Debug.Log("Executing CreateNewPlayerListItems");
        foreach (LobbyPlayer player in Game.LobbyPlayers)
        {
            if (!playerListItems.Any(b => b.ConnectionId == player.ConnectionId))
            {
                Debug.Log("CreateNewPlayerListItems: Player not found in playerListItems: " + player.PlayerName);
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
        if (areAllPlayersReady)
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
        localLobbyPlayerScript.QuitLobby();
    }
}
