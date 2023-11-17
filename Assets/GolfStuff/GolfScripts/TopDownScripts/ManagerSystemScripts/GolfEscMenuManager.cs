using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GolfEscMenuManager : MonoBehaviour
{
    public static GolfEscMenuManager instance;
    NetworkPlayer _localNetworkPlayer;

    public bool IsMenuOpen = false;
    public bool IsSettingsMenuOpen = false;

    [Header("UI Components")]
    [SerializeField] GameObject _escMenuCanvas;
    [SerializeField] Button _turnMusicOnOffButton;
    [SerializeField] GameObject _escMenuBasePanel;
    [SerializeField] GameObject _settingsPanel;
    // Start is called before the first frame update
    private void Awake()
    {
        MakeInstance();
        IsMenuOpen = false;
        _escMenuCanvas.SetActive(false);
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OpenOrCloseEscMenu()
    {
        if (IsMenuOpen)
        {
            if (IsSettingsMenuOpen) // close Settings Panel with Esc key, and go back to the base Esc Menu
            {
                _settingsPanel.SetActive(false);
                IsSettingsMenuOpen = false;
                _escMenuBasePanel.SetActive(true);
            }
            else
            {
                InputManagerGolf.Controls.UI.Disable();
                _escMenuCanvas.SetActive(false);
                IsMenuOpen = false;
            }
            
        }
        else
        {
            InputManagerGolf.Controls.UI.Enable();
            SetMusicOnOffButtonText();
            _escMenuCanvas.SetActive(true);
            IsMenuOpen = true;
            _escMenuBasePanel.SetActive(true);
            _settingsPanel.SetActive(false);
            IsSettingsMenuOpen = false;
        }
    }
    void SetMusicOnOffButtonText()
    {
        if (MusicManager.instance.IsMusicPlaying())
        {
            _turnMusicOnOffButton.GetComponentInChildren<TextMeshProUGUI>().text = "Turn Music Off";
        }
        else
        {
            _turnMusicOnOffButton.GetComponentInChildren<TextMeshProUGUI>().text = "Turn Music On";
        }
    }
    public void OnClick_Disconnect()
    {
        if (!_localNetworkPlayer)
            _localNetworkPlayer = GameObject.FindGameObjectWithTag("LocalNetworkPlayer").GetComponent<NetworkPlayer>();
        // have players confirm disconnect/quit before actually doing it? A pop up that says "Are you sure?" or whatever
        InputManagerGolf.Controls.UI.Disable();
        _localNetworkPlayer.PlayerClickedDisconnect();
    }
    public void ExitGame()
    {
        if (!_localNetworkPlayer)
            _localNetworkPlayer = GameObject.FindGameObjectWithTag("LocalNetworkPlayer").GetComponent<NetworkPlayer>();
        // have players confirm disconnect/quit before actually doing it? A pop up that says "Are you sure?" or whatever
        InputManagerGolf.Controls.UI.Disable();
        _localNetworkPlayer.PlayerClickedDisconnect();
        // have players confirm disconnect/quit before actually doing it? A pop up that says "Are you sure?" or whatever
        Application.Quit();
    }
    public void TurnMusicOnOff()
    {
        if (MusicManager.instance.IsMusicPlaying())
        {
            MusicManager.instance.TurnMusicOff();
            _turnMusicOnOffButton.GetComponentInChildren<TextMeshProUGUI>().text = "Turn Music On";
        }

        else
        {
            MusicManager.instance.TurnMusicOn();
            _turnMusicOnOffButton.GetComponentInChildren<TextMeshProUGUI>().text = "Turn Music Off";
        }
            
    }
    public void ReturnToGameButtonPressed()
    {
        OpenOrCloseEscMenu();
    }
    public void SettingsButtonPressed()
    {
        if (IsSettingsMenuOpen)
        {
            OpenOrCloseEscMenu();
        }
        else
        {
            OpenSettingsMenu();
        }
    }
    void OpenSettingsMenu()
    {
        _escMenuBasePanel.SetActive(false);
        _settingsPanel.SetActive(true);
        IsSettingsMenuOpen = true;
    }
}
