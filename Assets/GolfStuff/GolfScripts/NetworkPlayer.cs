using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine.UI;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("Golf Player Stuff")]
    [SerializeField] GameObject _golfPlayerPrefab;
    [SerializeField] GolfPlayerTopDown _golfPlayerScript;

    [Header("UI Stuff")]
    [SerializeField] GameObject _playerUICanvas;
    [SerializeField] Button _readyButton;
    [SerializeField] Button _startGameButton;
    [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] Button _submitPlayerNameButton;
    [SerializeField] TextMeshProUGUI _welcomeText;
    [SerializeField] GameObject _uiHolder;

    [Header("Player Status")]
    [SerializeField] [SyncVar(OnChange = nameof(SyncIsReady))] public bool IsReady = false;
    [SerializeField] [SyncVar(OnChange = nameof(SyncPlayerName))] public string PlayerName;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        GameplayManagerTopDownGolf.instance.AddNetworkPlayer(this);
        CheckIfHostCanStartGame();
    }
    public override void OnStopServer()
    {
        base.OnStopServer();
        GameplayManagerTopDownGolf.instance.RemoveNetworkPlayer(this);
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!this.IsOwner)
        {
            _playerUICanvas.SetActive(false);
            _readyButton.gameObject.SetActive(false);
            _playerNameInput.gameObject.SetActive(false);
            _submitPlayerNameButton.gameObject.SetActive(false);
        }
        else
        {
            _playerUICanvas.SetActive(true);
            _readyButton.gameObject.SetActive(true);
            _playerNameInput.gameObject.SetActive(true);
            _submitPlayerNameButton.gameObject.SetActive(true);
        }
        _welcomeText.gameObject.SetActive(false);
        _startGameButton.gameObject.SetActive(false);
    }
    public void PlayerReadyUp()
    {
        if (!this.IsOwner)
            return;
        this.CmdPlayerReadyUp();
    }
    [ServerRpc]
    void CmdPlayerReadyUp()
    {
        this.IsReady = !this.IsReady;
        CheckIfHostCanStartGame();

    }
    [Server]
    void CheckIfHostCanStartGame()
    {
        if (AreAllPlayersReady())
        {
            TellHostTheyCanStartGame();
        }
        else
        {
            CannotStartGameYet();
        }
    }
    void SyncIsReady(bool prev, bool next, bool asServer)
    {
        if (!this.IsOwner)
            return;

        if (next)
        {
            _readyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Unready";
        }
        else
        { 
            _readyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Ready up";
        }
    }
    [Server]
    bool AreAllPlayersReady()
    {
        bool allReady = false;
        foreach (NetworkPlayer player in GameplayManagerTopDownGolf.instance.NetworkPlayersServer)
        {
            if (player.IsReady)
            {
                allReady = true;
                Debug.Log("AreAllPlayersReady: Player: " + player.PlayerName + ":" + player.OwnerId + " ready status: " + player.IsReady.ToString());
            }
            else
            {
                Debug.Log("AreAllPlayersReady: Player: " + player.PlayerName + ":" + player.OwnerId + " ready status: " + player.IsReady.ToString());
                allReady = false;
                break;
            }  
        }
        Debug.Log("AreAllPlayersReady: returning: " + allReady.ToString());
        return allReady;
    }
    [Server]
    void TellHostTheyCanStartGame()
    {
        Debug.Log("TellHostTheyCanStartGame");
        NetworkPlayer hostPlayer = GetHostPlayer();
        if (!hostPlayer)
            return;
        hostPlayer.RpcHostCanStartGame(hostPlayer.Owner);
    }
    [Server]
    void CannotStartGameYet()
    {
        Debug.Log("CannotStartGameYet");
        NetworkPlayer hostPlayer = GetHostPlayer();
        if (!hostPlayer)
            return;
        hostPlayer.RpcCannotStartGameYet(hostPlayer.Owner);
    }
    [Server]
    NetworkPlayer GetHostPlayer()
    {
        NetworkPlayer hostPlayer = null;
        foreach (NetworkPlayer player in GameplayManagerTopDownGolf.instance.NetworkPlayersServer)
        {
            if (player.Owner.IsHost)
            {
                Debug.Log("GetHostPlayer: Host player is: " + player.PlayerName + ":" + player.OwnerId);
                hostPlayer = player;
                break;
            }
        }
        return hostPlayer;
    }
    [TargetRpc]
    public void RpcHostCanStartGame(NetworkConnection conn)
    {
        if (!this.IsOwner)
            return;

        _uiHolder.SetActive(false);
        _startGameButton.gameObject.SetActive(true);

    }
    [TargetRpc]
    public void RpcCannotStartGameYet(NetworkConnection conn)
    {
        if (!this.IsOwner)
            return;
        _uiHolder.SetActive(true);
        _startGameButton.gameObject.SetActive(false);
    }

    public void HostStartGame()
    {
        if (!this.IsOwner)
            return;
        CmdHostStartGame();
    }
    [ServerRpc]
    void CmdHostStartGame()
    {
        if (!this.IsHost)
            return;
        SpawnGolfPlayerObjectsForEachPlayer();
        GameplayManagerTopDownGolf.instance.HostStartGame(base.Owner);
    }
    [Server]
    void SpawnGolfPlayerObjectsForEachPlayer()
    {
        foreach (NetworkPlayer player in GameplayManagerTopDownGolf.instance.NetworkPlayersServer)
        {
            player.SpawnGolfPlayer();
            player.HideLobbyUIStuff(player.Owner);
        }
    }
    [Server]
    public void SpawnGolfPlayer()
    {
        GameObject playerObject = Instantiate(_golfPlayerPrefab);
        _golfPlayerScript = playerObject.GetComponent<GolfPlayerTopDown>();
        string playerName = this.PlayerName;
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = RandomPlayerName();
            if (this.Owner.IsHost)
                playerName += " (Host)";
            else
                playerName += " (Client)";
        }
        _golfPlayerScript.PlayerName = playerName;
        _golfPlayerScript.IsGameLeader = this.Owner.IsHost;
        _golfPlayerScript.OwnerNetId = this.OwnerId;
        InstanceFinder.ServerManager.Spawn(_golfPlayerScript.gameObject, this.Owner);
    }
    [TargetRpc]
    public void HideLobbyUIStuff(NetworkConnection conn)
    {
        if (!this.IsOwner)
            return;
        _playerUICanvas.SetActive(false);
    }
    public void SubmitName()
    {
        if (!this.IsOwner)
            return;
        string name;
        if (!string.IsNullOrEmpty(_playerNameInput.text))
        {
            name = _playerNameInput.text;
        }
        else
        {
            name = RandomPlayerName();
        }

        CmdSetPlayerName(name);
    }
    [ServerRpc]
    void CmdSetPlayerName(string newName)
    {
        string name = newName;
        if (string.IsNullOrWhiteSpace(name))
            name = RandomPlayerName();
        if (this.Owner.IsHost)
            name += " (Host)";
        else
            name += " (Client)";

        this.PlayerName = name;
    }
    string RandomPlayerName()
    {
        int rand = UnityEngine.Random.Range(0, 10);
        string name = "Player[" + rand.ToString() + "]";
        return name;
    }
    void SyncPlayerName(string prev, string next, bool asServer)
    {
        if (!this.IsOwner)
            return;
        _welcomeText.gameObject.SetActive(true);
        _welcomeText.text = next;
    }
}
