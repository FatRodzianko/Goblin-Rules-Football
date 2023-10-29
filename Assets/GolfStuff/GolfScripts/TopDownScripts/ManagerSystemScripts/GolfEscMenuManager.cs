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

    [Header("UI Components")]
    [SerializeField] GameObject _escMenuPanel;
    [SerializeField] Button _turnMusicOnOffButton;
    // Start is called before the first frame update
    private void Awake()
    {
        MakeInstance();
        IsMenuOpen = false;
        _escMenuPanel.SetActive(false);
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
            InputManagerGolf.Controls.UI.Disable();
            _escMenuPanel.SetActive(false);
            IsMenuOpen = false;
        }
        else
        {
            InputManagerGolf.Controls.UI.Enable();
            SetMusicOnOffButtonText();
            _escMenuPanel.SetActive(true);
            IsMenuOpen = true;
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
}
