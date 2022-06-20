using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
            int ranNumber = Random.Range(1, 70);
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
        if (toggle3v3.isOn)
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
        networkManager.StartHost();
        CreateGameInfoPanel.SetActive(false);
        returnToMainMenu.gameObject.SetActive(false);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
