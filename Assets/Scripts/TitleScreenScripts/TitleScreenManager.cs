using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

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

    private void Awake()
    {
        MakeInstance();
        ReturnToMainMenu();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
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
        /*networkManager.StartHost();
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
            networkManager.is1v1 = false;
            networkManager.isSinglePlayer = false;
            networkManager.maxConnections = 6;
            networkManager.minPlayers = 6;
        }
        else
        {
            networkManager.is1v1 = true;
            networkManager.isSinglePlayer = false;
            networkManager.maxConnections = 2;
            networkManager.minPlayers = 2;
        }
        SetGameSettings(false);
        networkManager.StartHost();
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
            networkManager.networkAddress = IpAddressField.text;
            networkManager.StartClient();
        }
        EnterIPAddressPanel.SetActive(false);
        returnToMainMenu.gameObject.SetActive(false);
    }
    public void SinglePlayerGame()
    {
        Debug.Log("TitleScreenManager: Player has chosen a single player game");
        networkManager.is1v1 = false;
        networkManager.isSinglePlayer = true;
        networkManager.maxConnections = 1;
        networkManager.minPlayers = 1;

        try
        {
            SetGameSettings(true);
        }
        catch (Exception e)
        {
            Debug.Log("SinglePlayerGame: Failed to set new game values. Error: " + e);
        }

        networkManager.StartHost();
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
                        networkManager.secondsPerHalf = 60;
                    }
                    else
                    {
                        int.TryParse(singlePlayerSecondsPerHalfInputField.text, out secondsPerHalf);
                        if (secondsPerHalf < 30)
                            secondsPerHalf = 30;
                        else if (secondsPerHalf > 300)
                            secondsPerHalf = 300;
                        networkManager.secondsPerHalf = secondsPerHalf;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("SetGameSettings: failed to get text from seconds per half input field. Error: " + e);
                    networkManager.secondsPerHalf = 60;
                }

                // set power ups bool
                networkManager.powerUpsEnabled = singlePlayerPowerUpsToggle.isOn;

                // set the random events enabled bool
                networkManager.randomEventsEnabled = singlePlayerRandomEventsToggle.isOn;

                // set spawn obstacles bool
                networkManager.spawnObstaclesEnabled = singlePlayerSpawnObstacles.isOn;

                // set Mercy rule bool
                networkManager.mercyRuleEnabled = singlePlayerMercyRuleToggle.isOn;

                // Set mercy rule point differential
                try
                {
                    if (String.IsNullOrWhiteSpace(singlePlayerMercyRuleInputField.text))
                    {
                        networkManager.mercyRulePointDifferential = 21;
                    }
                    else
                    {
                        int.TryParse(singlePlayerMercyRuleInputField.text, out mercyRulePointDifferential);
                        if (mercyRulePointDifferential < 21)
                            mercyRulePointDifferential = 21;
                        networkManager.mercyRulePointDifferential = mercyRulePointDifferential;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("SetGameSettings: failed to get text from mercy rule point differential input field. Error: " + e);
                    networkManager.mercyRulePointDifferential = 21;
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
                        networkManager.lobbyName = "Player's Lobby";
                    }
                    else
                    {
                        if (multiplayerLobbyNameInputField.text.Length > 15)
                            networkManager.lobbyName = multiplayerLobbyNameInputField.text.Substring(0, 15);
                        else
                            networkManager.lobbyName = multiplayerLobbyNameInputField.text;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("SetGameSettings: failed to get text from lobby name input field. Error: " + e);
                    networkManager.lobbyName = "Player's Lobby";
                }

                // set seconds per game
                //int secondsPerHalf = multiplayerSecondsPerHalfInputField.text;
                try
                {
                    if (String.IsNullOrWhiteSpace(multiplayerSecondsPerHalfInputField.text))
                    {
                        networkManager.secondsPerHalf = 60;
                    }
                    else
                    {
                        int.TryParse(multiplayerSecondsPerHalfInputField.text, out secondsPerHalf);
                        if (secondsPerHalf < 30)
                            secondsPerHalf = 30;
                        else if (secondsPerHalf > 300)
                            secondsPerHalf = 300;
                        networkManager.secondsPerHalf = secondsPerHalf;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("SetGameSettings: failed to get text from seconds per half input field. Error: " + e);
                    networkManager.secondsPerHalf = 60;
                }

                // set power ups bool
                networkManager.powerUpsEnabled = multiplayerPowerUpsToggle.isOn;

                // set the random events enabled bool
                networkManager.randomEventsEnabled = multiplayerRandomEventsToggle.isOn;

                // set spawn obstacles bool
                networkManager.spawnObstaclesEnabled = multiplayerSpawnObstacles.isOn;

                // set Mercy rule bool
                networkManager.mercyRuleEnabled = multiplayerMercyRuleToggle.isOn;

                // Set mercy rule point differential
                try
                {
                    if (String.IsNullOrWhiteSpace(multiplayerMercyRuleInputField.text))
                    {
                        networkManager.mercyRulePointDifferential = 21;
                    }
                    else
                    {
                        int.TryParse(multiplayerMercyRuleInputField.text, out mercyRulePointDifferential);
                        if (mercyRulePointDifferential < 21)
                            mercyRulePointDifferential = 21;
                        networkManager.mercyRulePointDifferential = mercyRulePointDifferential;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("SetGameSettings: failed to get text from mercy rule point differential input field. Error: " + e);
                    networkManager.mercyRulePointDifferential = 21;
                }
                
            }
            catch (Exception e)
            {
                Debug.Log("SetGameSettings: Failed to set single pleyer settings. Error: " + e);
            }
        }
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
