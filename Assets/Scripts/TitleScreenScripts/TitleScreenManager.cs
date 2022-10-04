using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Steamworks;

public class TitleScreenManager : MonoBehaviour
{
    public static TitleScreenManager instance;
    [SerializeField] private NetworkManagerGRF networkManager;
    [SerializeField] EventSystem eventSystem;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject PlayerNamePanel;
    [SerializeField] private GameObject HostOrJoinPanel;
    [SerializeField] private GameObject EnterIPAddressPanel;
    [SerializeField] private GameObject CreateGameInfoPanel;

    [Header("PlayerName UI")]
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private Toggle GamepadToggle;

    [Header("Enter IP UI")]
    [SerializeField] private TMP_InputField IpAddressField;

    [Header("Misc. UI")]
    [SerializeField] private Button returnToMainMenu;

    [Header("Create Game Options")]
    [SerializeField] private Toggle toggle3v3;

    [Header("Singleplayer Game Option Objects")]
    [SerializeField] private TMP_InputField singlePlayerSecondsPerHalfInputField;
    [SerializeField] private Toggle singlePlayerPowerUpsToggle;
    [SerializeField] private Toggle singlePlayerRandomEventsToggle;
    [SerializeField] private Toggle singlePlayerSpawnObstacles;
    [SerializeField] private Toggle singlePlayerMercyRuleToggle;
    [SerializeField] private TMP_InputField singlePlayerMercyRuleInputField;

    [Header("Multiplayer Game Option Objects")]
    [SerializeField] private TMP_InputField multiplayerLobbyNameInputField;
    [SerializeField] private Toggle multiplayer1v1Toggle;
    [SerializeField] private Toggle multiplayer3v3Toggle;
    [SerializeField] private TMP_InputField multiplayerSecondsPerHalfInputField;
    [SerializeField] private Toggle multiplayerPowerUpsToggle;
    [SerializeField] private Toggle multiplayerRandomEventsToggle;
    [SerializeField] private Toggle multiplayerSpawnObstacles;
    [SerializeField] private Toggle multiplayerMercyRuleToggle;
    [SerializeField] private TMP_InputField multiplayerMercyRuleInputField;


    [Header("Game Option Values")]
    public string lobbyName;
    public bool is1v1;
    public int secondsPerHalf;
    public bool powerUpsEnabled;
    public bool randomEventsEnabled;
    public bool spawnObstaclesEnabled;
    public bool mercyRuleEnabled;
    public int mercyRulePointDifferential;


    private const string PlayerPrefsNameKey = "PlayerName";

    [Header("First Selected UI stuff?")]
    [SerializeField] GameObject startGameButton;
    [SerializeField] GameObject confirmNameButton;
    [SerializeField] GameObject hostGameButton;
    [SerializeField] GameObject connectToIPButton;
    [SerializeField] GameObject createLobbyButton;

    [Header("Lobby List UI")]
    [SerializeField] private GameObject LobbyListCanvas;
    [SerializeField] private GameObject LobbyListItemPrefab;
    [SerializeField] private GameObject ContentPanel;
    [SerializeField] private GameObject LobbyListScrollRect;
    [SerializeField] private TMP_InputField searchBox;
    public bool didPlayerSearchForLobbies = false;
    [Header("Create Lobby UI")]
    [SerializeField] private GameObject CreateLobbyCanvas;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private Toggle friendsOnlyToggle;
    public bool didPlayerNameTheLobby = false;
    public List<GameObject> listOfLobbyListItems = new List<GameObject>();

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
        ReturnToMainMenu();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        if (Time.timeScale != 1f)
            Time.timeScale = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ReturnToMainMenu()
    {
        mainMenuPanel.SetActive(true);
        PlayerNamePanel.SetActive(false);
        HostOrJoinPanel.SetActive(false);
        EnterIPAddressPanel.SetActive(false);
        returnToMainMenu.gameObject.SetActive(false);
        CreateGameInfoPanel.SetActive(false);
        eventSystem.SetSelectedGameObject(startGameButton, new BaseEventData(eventSystem));
    }
    public void StartGame()
    {
        //SceneManager.LoadScene("Gameplay");
        mainMenuPanel.SetActive(false);
        PlayerNamePanel.SetActive(true);
        eventSystem.SetSelectedGameObject(confirmNameButton, new BaseEventData(eventSystem));
        GetSavedPlayerName();
        returnToMainMenu.gameObject.SetActive(true);
        GamepadUIManager.instance.gamepadUI = GamepadToggle.isOn;
    }
    private void GetSavedPlayerName()
    {
        if (PlayerPrefs.HasKey(PlayerPrefsNameKey))
        {
            playerNameInputField.text = PlayerPrefs.GetString(PlayerPrefsNameKey);
        }
    }
    public void SavePlayerName()
    {
        string playerName = null;
        if (!string.IsNullOrEmpty(playerNameInputField.text) && playerNameInputField.text.Length < 13)
        {
            playerName = playerNameInputField.text;
            PlayerPrefs.SetString(PlayerPrefsNameKey, playerName);
            PlayerNamePanel.SetActive(false);
            HostOrJoinPanel.SetActive(true);
            eventSystem.SetSelectedGameObject(hostGameButton, new BaseEventData(eventSystem));
        }
        else if (string.IsNullOrEmpty(playerNameInputField.text))
        {
            // this is for if the player is using a gamepad and cant enter input?
            int ranNumber = UnityEngine.Random.Range(1, 70);
            playerName = "NoName" + ranNumber.ToString();
            PlayerPrefs.SetString(PlayerPrefsNameKey, playerName);
            PlayerNamePanel.SetActive(false);
            HostOrJoinPanel.SetActive(true);
            eventSystem.SetSelectedGameObject(hostGameButton, new BaseEventData(eventSystem));
        }
        //GamepadUIManager.instance.gamepadUI = GamepadToggle.isOn;
    }
    public void HostGame()
    {
        Debug.Log("Hosting a game...");
        /*Game.StartHost();
        HostOrJoinPanel.SetActive(false);
        returnToMainMenu.gameObject.SetActive(false);*/
        HostOrJoinPanel.SetActive(false);
        CreateGameInfoPanel.SetActive(true);
        eventSystem.SetSelectedGameObject(createLobbyButton, new BaseEventData(eventSystem));
    }
    public void CreateLobby()
    {
        //if (toggle3v3.isOn)
        if (multiplayer3v3Toggle.isOn)
        {
            Game.is1v1 = false;
            Game.isSinglePlayer = false;
            Game.maxConnections = 6;
            Game.minPlayers = 6;
        }
        else
        {
            Game.is1v1 = true;
            Game.isSinglePlayer = false;
            Game.maxConnections = 2;
            Game.minPlayers = 2;
        }
        SetGameSettings(false);
        Game.StartHost();
        CreateGameInfoPanel.SetActive(false);
        returnToMainMenu.gameObject.SetActive(false);
    }
    public void JoinGame()
    {
        HostOrJoinPanel.SetActive(false);
        EnterIPAddressPanel.SetActive(true);
        eventSystem.SetSelectedGameObject(connectToIPButton, new BaseEventData(eventSystem));
    }
    public void ConnectToGame()
    {

        if (!string.IsNullOrEmpty(IpAddressField.text))
        {
            Debug.Log("Client will connect to: " + IpAddressField.text);
            Game.networkAddress = IpAddressField.text;
            Game.StartClient();
        }
        EnterIPAddressPanel.SetActive(false);
        returnToMainMenu.gameObject.SetActive(false);
    }
    public void SinglePlayerGame()
    {
        Debug.Log("TitleScreenManager: Player has chosen a single player game");
        ResetGameServerSettings();
        /*Game.is1v1 = false;
        Game.isSinglePlayer = true;
        Game.maxConnections = 1;
        Game.minPlayers = 1;*/
        Game.is1v1 = false;
        Game.isSinglePlayer = true;
        Game.maxConnections = 1;
        Game.minPlayers = 1;

        try
        {
            SetGameSettings(true);
        }
        catch (Exception e)
        {
            Debug.Log("SinglePlayerGame: Failed to set new game values. Error: " + e);
        }

        //Game.StartHost();
        Game.StartHost();
        CreateGameInfoPanel.SetActive(false);
        returnToMainMenu.gameObject.SetActive(false);
    }
    public void SetGameSettings(bool isGameSinglePlayer)
    {
        if (isGameSinglePlayer)
        {
            try
            {
                // set seconds per game
                //int secondsPerHalf = singlePlayerSecondsPerHalfInputField.text;
                try
                {
                    if (String.IsNullOrWhiteSpace(singlePlayerSecondsPerHalfInputField.text))
                    {
                        //Game.secondsPerHalf = 60;
                        Game.secondsPerHalf = 60;
                    }
                    else
                    {
                        int.TryParse(singlePlayerSecondsPerHalfInputField.text, out secondsPerHalf);
                        if (secondsPerHalf < 30)
                            secondsPerHalf = 30;
                        else if (secondsPerHalf > 300)
                            secondsPerHalf = 300;
                        //Game.secondsPerHalf = secondsPerHalf;
                        Game.secondsPerHalf = secondsPerHalf;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("SetGameSettings: failed to get text from seconds per half input field. Error: " + e);
                    Game.secondsPerHalf = 60;
                }

                // set power ups bool
                //Game.powerUpsEnabled = singlePlayerPowerUpsToggle.isOn;
                Game.powerUpsEnabled = singlePlayerPowerUpsToggle.isOn;

                // set the random events enabled bool
                //Game.randomEventsEnabled = singlePlayerRandomEventsToggle.isOn;
                Game.randomEventsEnabled = singlePlayerRandomEventsToggle.isOn;

                // set spawn obstacles bool
                //Game.spawnObstaclesEnabled = singlePlayerSpawnObstacles.isOn;
                Game.spawnObstaclesEnabled = singlePlayerSpawnObstacles.isOn;

                // set Mercy rule bool
                //Game.mercyRuleEnabled = singlePlayerMercyRuleToggle.isOn;
                Game.mercyRuleEnabled = singlePlayerMercyRuleToggle.isOn;

                // Set mercy rule point differential
                try
                {
                    if (String.IsNullOrWhiteSpace(singlePlayerMercyRuleInputField.text))
                    {
                        //Game.mercyRulePointDifferential = 21;
                        Game.mercyRulePointDifferential = 21;
                    }
                    else
                    {
                        int.TryParse(singlePlayerMercyRuleInputField.text, out mercyRulePointDifferential);
                        if (mercyRulePointDifferential < 21)
                            mercyRulePointDifferential = 21;
                        //Game.mercyRulePointDifferential = mercyRulePointDifferential;
                        Game.mercyRulePointDifferential = mercyRulePointDifferential;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("SetGameSettings: failed to get text from mercy rule point differential input field. Error: " + e);
                    //Game.mercyRulePointDifferential = 21;
                    Game.mercyRulePointDifferential = 21;
                }
            }
            catch (Exception e)
            {
                Debug.Log("SetGameSettings: Failed to set single pleyer settings. Error: " + e);
            }
        }
        else
        {
            try
            {
                // set lobby name
                try
                {
                    if (String.IsNullOrWhiteSpace(multiplayerLobbyNameInputField.text))
                    {
                        //Game.lobbyName = "Player's Lobby";
                        Game.lobbyName = "Player's Lobby";
                    }
                    else
                    {
                        if (multiplayerLobbyNameInputField.text.Length > 15)
                            Game.lobbyName = multiplayerLobbyNameInputField.text.Substring(0, 15);
                        else
                            Game.lobbyName = multiplayerLobbyNameInputField.text;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("SetGameSettings: failed to get text from lobby name input field. Error: " + e);
                    Game.lobbyName = "Player's Lobby";
                }

                // set seconds per game
                //int secondsPerHalf = multiplayerSecondsPerHalfInputField.text;
                try
                {
                    if (String.IsNullOrWhiteSpace(multiplayerSecondsPerHalfInputField.text))
                    {
                        //Game.secondsPerHalf = 60;
                        Game.secondsPerHalf = 60;
                    }
                    else
                    {
                        int.TryParse(multiplayerSecondsPerHalfInputField.text, out secondsPerHalf);
                        if (secondsPerHalf < 30)
                            secondsPerHalf = 30;
                        else if (secondsPerHalf > 300)
                            secondsPerHalf = 300;
                        //Game.secondsPerHalf = secondsPerHalf;
                        Game.secondsPerHalf = secondsPerHalf;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("SetGameSettings: failed to get text from seconds per half input field. Error: " + e);
                    Game.secondsPerHalf = 60;
                }

                // set power ups bool
                Game.powerUpsEnabled = multiplayerPowerUpsToggle.isOn;

                // set the random events enabled bool
                Game.randomEventsEnabled = multiplayerRandomEventsToggle.isOn;

                // set spawn obstacles bool
                Game.spawnObstaclesEnabled = multiplayerSpawnObstacles.isOn;

                // set Mercy rule bool
                Game.mercyRuleEnabled = multiplayerMercyRuleToggle.isOn;

                // Set mercy rule point differential
                try
                {
                    if (String.IsNullOrWhiteSpace(multiplayerMercyRuleInputField.text))
                    {
                        Game.mercyRulePointDifferential = 21;
                    }
                    else
                    {
                        int.TryParse(multiplayerMercyRuleInputField.text, out mercyRulePointDifferential);
                        if (mercyRulePointDifferential < 21)
                            mercyRulePointDifferential = 21;
                        Game.mercyRulePointDifferential = mercyRulePointDifferential;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("SetGameSettings: failed to get text from mercy rule point differential input field. Error: " + e);
                    Game.mercyRulePointDifferential = 21;
                }
                
            }
            catch (Exception e)
            {
                Debug.Log("SetGameSettings: Failed to set single pleyer settings. Error: " + e);
            }
        }
    }
    public void StartTutorial()
    {
        Game.is1v1 = false;
        Game.isSinglePlayer = false;
        Game.maxConnections = 1;
        Game.minPlayers = 1;
        SceneManager.LoadScene("Tutorial");
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void GetListOfLobbies()
    {
        Debug.Log("Trying to get list of available lobbies ...");
        //buttons.SetActive(false);
        //LobbyListCanvas.SetActive(true);
        //eventSystem.SetSelectedGameObject(searchButton, new BaseEventData(eventSystem));
        SteamLobby.instance.GetListOfLobbies();
        GamepadUIManager.instance.gamepadUI = GamepadToggle.isOn;
    }
    public void DisplayLobbies(List<CSteamID> lobbyIDS, LobbyDataUpdate_t result)
    {
        Debug.Log("DisplayLobbies: Count of lobby ids: " + lobbyIDS.Count.ToString());
        for (int i = 0; i < lobbyIDS.Count; i++)
        {
            if (lobbyIDS[i].m_SteamID == result.m_ulSteamIDLobby)
            {
                Debug.Log("Lobby " + i + " :: " + SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "name") + " number of players: " + SteamMatchmaking.GetNumLobbyMembers((CSteamID)lobbyIDS[i].m_SteamID).ToString() + " max players: " + SteamMatchmaking.GetLobbyMemberLimit((CSteamID)lobbyIDS[i].m_SteamID).ToString());

                //if(true)
                if (SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "GameName").Equals("GRF"))
                {
                    if (didPlayerSearchForLobbies)
                    {
                        Debug.Log("OnGetLobbyInfo: Player searched for lobbies");
                        if (SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "name").ToLower().Contains(searchBox.text.ToLower()))
                        {
                            GameObject newLobbyListItem = Instantiate(LobbyListItemPrefab) as GameObject;
                            LobbyListItem newLobbyListItemScript = newLobbyListItem.GetComponent<LobbyListItem>();

                            newLobbyListItemScript.lobbySteamId = (CSteamID)lobbyIDS[i].m_SteamID;
                            newLobbyListItemScript.lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "name");
                            newLobbyListItemScript.numberOfPlayers = SteamMatchmaking.GetNumLobbyMembers((CSteamID)lobbyIDS[i].m_SteamID);
                            newLobbyListItemScript.maxNumberOfPlayers = SteamMatchmaking.GetLobbyMemberLimit((CSteamID)lobbyIDS[i].m_SteamID);
                            newLobbyListItemScript.SetLobbyItemValues();


                            newLobbyListItem.transform.SetParent(ContentPanel.transform);
                            newLobbyListItem.transform.localScale = Vector3.one;

                            listOfLobbyListItems.Add(newLobbyListItem);
                        }
                    }
                    else
                    {
                        Debug.Log("OnGetLobbyInfo: Player DID NOT search for lobbies");
                        GameObject newLobbyListItem = Instantiate(LobbyListItemPrefab) as GameObject;
                        LobbyListItem newLobbyListItemScript = newLobbyListItem.GetComponent<LobbyListItem>();

                        newLobbyListItemScript.lobbySteamId = (CSteamID)lobbyIDS[i].m_SteamID;
                        newLobbyListItemScript.lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "name");
                        newLobbyListItemScript.numberOfPlayers = SteamMatchmaking.GetNumLobbyMembers((CSteamID)lobbyIDS[i].m_SteamID);
                        newLobbyListItemScript.maxNumberOfPlayers = SteamMatchmaking.GetLobbyMemberLimit((CSteamID)lobbyIDS[i].m_SteamID);
                        newLobbyListItemScript.SetLobbyItemValues();


                        newLobbyListItem.transform.SetParent(ContentPanel.transform);
                        newLobbyListItem.transform.localScale = Vector3.one;

                        listOfLobbyListItems.Add(newLobbyListItem);
                    }
                }

                return;
            }
        }
        if (didPlayerSearchForLobbies)
            didPlayerSearchForLobbies = false;
    }
    
    public void DestroyOldLobbyListItems()
    {
        Debug.Log("DestroyOldLobbyListItems");
        foreach (GameObject lobbyListItem in listOfLobbyListItems)
        {
            GameObject lobbyListItemToDestroy = lobbyListItem;
            Destroy(lobbyListItemToDestroy);
            lobbyListItemToDestroy = null;
        }
        listOfLobbyListItems.Clear();
    }
    public void SearchForLobby()
    {
        if (!string.IsNullOrEmpty(searchBox.text))
        {
            didPlayerSearchForLobbies = true;
        }
        else
            didPlayerSearchForLobbies = false;
        SteamLobby.instance.GetListOfLobbies();
    }
    public void RefreshLobbyList()
    {
        Debug.Log("RefreshLobbyList");
        SteamLobby.instance.GetListOfLobbies();
    }
    public void CreateNewLobby()
    {
        ResetGameServerSettings();
        ELobbyType newLobbyType;
        if (friendsOnlyToggle.isOn)
        {
            Debug.Log("CreateNewLobby: friendsOnlyToggle is on. Making lobby friends only.");
            newLobbyType = ELobbyType.k_ELobbyTypeFriendsOnly;
        }
        else
        {
            Debug.Log("CreateNewLobby: friendsOnlyToggle is OFF. Making lobby public.");
            newLobbyType = ELobbyType.k_ELobbyTypePublic;
        }

        if (!string.IsNullOrEmpty(lobbyNameInputField.text))
        {
            Debug.Log("CreateNewLobby: player created a lobby name of: " + lobbyNameInputField.text);
            didPlayerNameTheLobby = true;
            lobbyName = lobbyNameInputField.text;
        }

        if (multiplayer3v3Toggle.isOn)
        {
            Game.is1v1 = false;
            Game.isSinglePlayer = false;
            Game.maxConnections = 6;
            Game.minPlayers = 6;
        }
        else
        {
            Game.is1v1 = true;
            Game.isSinglePlayer = false;
            Game.maxConnections = 2;
            Game.minPlayers = 2;
        }
        SetGameSettings(false);
        SteamLobby.instance.CreateNewLobby(newLobbyType);
    }
    public void ResetGameServerSettings()
    {
        Game.ResetWaitingForPlayersToLoadStuff();
        Game.ClearLobbyAndGamePlayerList();
    }

}
