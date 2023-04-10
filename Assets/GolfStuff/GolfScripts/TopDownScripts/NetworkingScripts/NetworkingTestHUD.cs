using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;

public class NetworkingTestHUD : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button _startServerButton;
    [SerializeField] private TextMeshProUGUI _startServerButtonText;
    [SerializeField] private TMP_InputField _serverAddressInputField;
    [SerializeField] private Button _startClientButton;
    [SerializeField] private TextMeshProUGUI _startClientButtonText;
    [SerializeField] private Button _disconnectButton;
    [SerializeField] GameObject _buttonHolder;

    private NetworkManager _networkManager;
    private LocalConnectionState _clientState = LocalConnectionState.Stopped;
    private LocalConnectionState _serverState = LocalConnectionState.Stopped;

    // Start is called before the first frame update
    void Start()
    {
        _networkManager = FindObjectOfType<NetworkManager>();
        if (_networkManager == null)
        {
            Debug.LogError("NetworkManager not found, HUD will not function.");
            return;
        }

        // Events to update the connection state
        _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;


        _disconnectButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnClick_Server()
    {
        if (_networkManager == null)
            return;

        if (_serverState != LocalConnectionState.Stopped)
            _networkManager.ServerManager.StopConnection(true);
        else
            _networkManager.ServerManager.StartConnection();

        // added this to automatically connect the host's client to the server
        if (_clientState != LocalConnectionState.Stopped)
            _networkManager.ClientManager.StopConnection();
        else
            _networkManager.ClientManager.StartConnection("localhost");

    }


    public void OnClick_Client()
    {
        if (_networkManager == null)
            return;

        if (_clientState != LocalConnectionState.Stopped)
            _networkManager.ClientManager.StopConnection();
        else
        {
            //_networkManager.ClientManager.StartConnection();

            string address = _serverAddressInputField.text;
            if (string.IsNullOrEmpty(address))
                _networkManager.ClientManager.StartConnection("localhost");
            else
                _networkManager.ClientManager.StartConnection(address);
        }
    }

    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
    {
        _clientState = obj.ConnectionState;
        if (_clientState != LocalConnectionState.Stopped)
        {
            ActivateServerClientButtons(false);
        }
        else
        {
            ActivateServerClientButtons(true);
        }
    }
    public void OnClick_Disconnect()
    {
        if (_serverState != LocalConnectionState.Stopped)
            _networkManager.ServerManager.StopConnection(true);

        if (_clientState != LocalConnectionState.Stopped)
            _networkManager.ClientManager.StopConnection();
    }
    void ActivateServerClientButtons(bool enable)
    {
        _buttonHolder.SetActive(enable);
        _disconnectButton.gameObject.SetActive(!enable);
    }

    private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
    {
        _serverState = obj.ConnectionState;
    }
    private void OnDestroy()
    {
        if (_networkManager == null)
            return;

        _networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
    }
}
