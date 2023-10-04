using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfEscMenuManager : MonoBehaviour
{
    public static GolfEscMenuManager instance;
    NetworkPlayer _localNetworkPlayer;

    public bool IsMenuOpen = false;

    [Header("UI Components")]
    [SerializeField] GameObject _escMenuPanel;
    // Start is called before the first frame update
    private void Awake()
    {
        MakeInstance();
        IsMenuOpen = false;
        _escMenuPanel.SetActive(false);
        IsMenuOpen = false;
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
            _escMenuPanel.SetActive(false);
            IsMenuOpen = false;
        }
        else
        {
            _escMenuPanel.SetActive(true);
            IsMenuOpen = true;
        }
    }
    public void OnClick_Disconnect()
    {
        if (!_localNetworkPlayer)
            _localNetworkPlayer = GameObject.FindGameObjectWithTag("LocalNetworkPlayer").GetComponent<NetworkPlayer>();
        // have players confirm disconnect/quit before actually doing it? A pop up that says "Are you sure?" or whatever
        _localNetworkPlayer.PlayerClickedDisconnect();
    }
    public void ExitGame()
    {
        if (!_localNetworkPlayer)
            _localNetworkPlayer = GameObject.FindGameObjectWithTag("LocalNetworkPlayer").GetComponent<NetworkPlayer>();
        // have players confirm disconnect/quit before actually doing it? A pop up that says "Are you sure?" or whatever
        _localNetworkPlayer.PlayerClickedDisconnect();
        // have players confirm disconnect/quit before actually doing it? A pop up that says "Are you sure?" or whatever
        Application.Quit();
    }
}
