using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Managing;
using TMPro;
using UnityEngine.UI;
using Steamworks;
using FishNet.Managing.Scened;
using UnityEngine.SceneManagement;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("Golf Player Stuff")]
    [SerializeField] GameObject _golfPlayerPrefab;
    [SerializeField] GolfPlayerTopDown _golfPlayerScript;
    [SerializeField] [SyncVar(OnChange = nameof(SyncPlayerSteamID))] public ulong PlayerSteamID;

    [Header("Color Picker Stuff")]
    [SerializeField] [SyncVar(OnChange = nameof(SyncBallColor))] Color _ballColor = Color.white;
    [SerializeField] BallColorPicker _ballColorPicker;

    [Header("UI Stuff")]
    [SerializeField] GameObject _playerUICanvas;
    [SerializeField] Button _readyButton;
    [SerializeField] Button _startGameButton;
    [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] Button _submitPlayerNameButton;
    [SerializeField] TextMeshProUGUI _welcomeText;
    [SerializeField] GameObject _uiHolder;
    [SerializeField] TextMeshProUGUI _lobbyNameText;
    [SerializeField] GameObject _promptPlayerToDownloadHolder;
    [SerializeField] TextMeshProUGUI _promptPlayerToDownloadText;



    [Header("Player List Item")]
    [SerializeField] GameObject _lobbyPlayerListItemPrefab;
    [SerializeField] GameObject _myLobbyPlayerListObject;
    [SerializeField] GolfPlayerListItem _myLobbyPlayerListScript;

    [Header("Player Status")]
    [SerializeField] [SyncVar(OnChange = nameof(SyncIsReady))] public bool IsReady = false;
    [SerializeField] [SyncVar(OnChange = nameof(SyncPlayerName))] public string PlayerName;

    [Header("Prompt Player To Download")]
    [SerializeField] GameObject _promptPlayerHolder;
    [SerializeField] TextMeshProUGUI _promptPlayerText;
    [SerializeField] ulong _workshopPublishedItemID;

    [Header("Networking stuff?")]
    [SerializeField] NetworkManager _networkManager;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDestroy()
    {
        if (_networkManager != null)
            _networkManager.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        
        
        if (!_networkManager)
        {
            _networkManager = GameObject.FindGameObjectWithTag("GolfNetworkManager").GetComponent<NetworkManager>();
        }
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
            Debug.Log("OnStartClient: DEactivating player canvas for player with id: " + this.ObjectId);
            _playerUICanvas.SetActive(false);
            _readyButton.gameObject.SetActive(false);
            _playerNameInput.gameObject.SetActive(false);
            _submitPlayerNameButton.gameObject.SetActive(false);
            _welcomeText.gameObject.SetActive(false);

            gameObject.tag = "NetworkPlayer";
        }
        else
        {
            Debug.Log("OnStartClient: ACTIVATING player canvas for player with id: " + this.ObjectId);
            _playerUICanvas.SetActive(true);
            _readyButton.gameObject.SetActive(true);
            _playerNameInput.gameObject.SetActive(false);
            _submitPlayerNameButton.gameObject.SetActive(false);
            
            if (!_networkManager)
            {
                _networkManager = GameObject.FindGameObjectWithTag("GolfNetworkManager").GetComponent<NetworkManager>();
            }

            gameObject.name = "LocalNetworkPlayer";
            gameObject.tag = "LocalNetworkPlayer";

            SetPlayerName();
            _ballColorPicker.GetPlayerPrefValues();
            SetPlayerSteamID();
        }
        
        _startGameButton.gameObject.SetActive(false);
        SpawnLobbyPlayerListItem();
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
        LobbyManagerGolf.instance.RemoveLobbyPlayerListItem(_myLobbyPlayerListObject);
        if (!this.IsOwner)
            return;

        //SceneLoadData sld = new SceneLoadData("TitleScreen");
        //sld.ReplaceScenes = ReplaceOption.All;
        //FishNet.InstanceFinder.SceneManager.LoadGlobalScenes(sld);
        UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScreen");
        GolfSteamLobby.instance.LeaveLobby();

    }
    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
    {
        Debug.Log("NetworkPlayer: SceneManager_OnClientLoadedStartScenes: as server: " + asServer.ToString());
        SetLobbyNameText(GolfSteamLobby.instance.CurrentLobbyName);
    }
    void SetLobbyNameText(string lobbyName)
    {
        if (string.IsNullOrEmpty(lobbyName))
            return;
        _lobbyNameText.text = lobbyName;
        _lobbyNameText.gameObject.SetActive(true);
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
        if (asServer)
            return;
        _myLobbyPlayerListScript.UpdatePlayerItemReadyStatus(next);
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
        GolfSteamLobby.instance.SetGameStatusToInGame();
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
            //else
            //    playerName += " (Client)";
        }
        _golfPlayerScript.PlayerName = playerName;
        _golfPlayerScript.IsGameLeader = this.Owner.IsHost;
        _golfPlayerScript.OwnerNetId = this.OwnerId;
        _golfPlayerScript.BallColor = this._ballColor;
        InstanceFinder.ServerManager.Spawn(_golfPlayerScript.gameObject, this.Owner);
    }
    [TargetRpc]
    public void HideLobbyUIStuff(NetworkConnection conn)
    {
        if (!this.IsOwner)
            return;
        _playerUICanvas.SetActive(false);
        LobbyManagerGolf.instance.HideUIStuff();
        GameObject networkingTestHudObject = GameObject.FindGameObjectWithTag("NetworkingTestHud");
        if (networkingTestHudObject)
            networkingTestHudObject.SetActive(false);
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
    void SetPlayerName()
    {
        if (!this.IsOwner)
            return;
        CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());
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
    void SetPlayerSteamID()
    {
        if (!this.IsOwner)
            return;
        CmdSetSteamID(SteamUser.GetSteamID().m_SteamID);
    }
    [ServerRpc]
    void CmdSetSteamID(ulong id)
    {
        this.PlayerSteamID = id;
    }
    string RandomPlayerName()
    {
        int rand = UnityEngine.Random.Range(0, 100);
        string name = "Player[" + rand.ToString() + "]";
        return name;
    }
    void SyncPlayerName(string prev, string next, bool asServer)
    {
        if (asServer)
            return;
        _myLobbyPlayerListScript.UpdatePlayerName(next);
        if (!this.IsOwner)
            return;
        _welcomeText.text = "Welcome " + next + "!";
        _welcomeText.gameObject.SetActive(true);
        
    }
    public void UpdateBallColorValue(Color newColor)
    {
        Debug.Log("UpdateBallColorValue: Is owner? " + this.IsOwner.ToString());
        if (!this.IsOwner)
            return;
        Debug.Log("UpdateBallColorValue: " + newColor.ToString());
        CmdUpdateBallColorValue(newColor);
    }
    [ServerRpc]
    void CmdUpdateBallColorValue(Color newColor)
    {
        Debug.Log("CmdUpdateBallColorValue: " + newColor.ToString());
        this._ballColor = newColor;
    }
    void SpawnLobbyPlayerListItem()
    {
        Debug.Log("SpawnLobbyPlayerListItem: ");
        _myLobbyPlayerListObject = Instantiate(_lobbyPlayerListItemPrefab);
        _myLobbyPlayerListScript = _myLobbyPlayerListObject.GetComponent<GolfPlayerListItem>();
        _myLobbyPlayerListScript.PlayerName = this.PlayerName;
        _myLobbyPlayerListScript.ConnectionId = this.ObjectId;
        _myLobbyPlayerListScript.UpdatePlayerItemReadyStatus(this.IsReady);

        LobbyManagerGolf.instance.AddLobbyPlayerListItem(_myLobbyPlayerListObject);

    }
    
    void SyncPlayerSteamID(ulong prev, ulong next, bool asServer)
    {
        if (asServer)
            return;

        if (_myLobbyPlayerListScript)
        {
            _myLobbyPlayerListScript.playerSteamId = next;
            _myLobbyPlayerListScript.GetPlayerAvatar();
        }
        else
        {
            StartCoroutine(DelayForSteamIDSync(next));
        }
    }
    IEnumerator DelayForSteamIDSync(ulong newID)
    {
        yield return new WaitForSeconds(0.5f);
        _myLobbyPlayerListScript.playerSteamId = newID;
        _myLobbyPlayerListScript.GetPlayerAvatar();
    }
    void SyncBallColor(Color prev, Color next, bool asServer)
    {
        if (asServer)
            return;

        _myLobbyPlayerListScript.UpdateColorIcon(next);
    }
    public void OnClick_Disconnect()
    {
        if (base.IsServer)
        {
            _networkManager.ServerManager.StopConnection(true);
            GolfSteamLobby.instance.LeaveLobby();
        }


        if (base.IsClient)
        {
            _networkManager.ClientManager.StopConnection();
            GolfSteamLobby.instance.LeaveLobby();
        }
            
    }
    public void PlayerClickedDisconnect()
    {
        OnClick_Disconnect();
    }
    public void PromptPlayerToDownloadCourse(string courseName)
    {
        if (!this.IsOwner)
            return;
        _promptPlayerHolder.SetActive(true);
        _promptPlayerText.text = "You do not have the '" + courseName + "' course. Do you want to download '" + courseName + "' from the steam workshop?";
        _uiHolder.SetActive(false);
    }
    public void OnClick_DownloadCourse()
    {
        CmdGetWorkshopIDFromServer();
    }
    [ServerRpc]
    void CmdGetWorkshopIDFromServer()
    {
        GameplayManagerTopDownGolf.instance.GetCourseWorkshopIDForPlayer(this.Owner);
    }
    public void ServerSentCourseWorkshopID(ulong workshopID)
    {
        Debug.Log("ServerSentCourseWorkshopID: " + workshopID);
        if (workshopID == 0)
        {
            // will need to have an error displayed here saying the course wasn't found in the workshop and you will disconnect in 5 seconds...
            OnClick_Disconnect();
        }

        CustomGolfCourseLoader customGolfCourseLoader = CustomGolfCourseLoader.GetInstance();
        customGolfCourseLoader.DownloadNewCourse(workshopID);
        //Debug.Log("ServerSentCourseWorkshopID: re-enabling UI");
        //_promptPlayerHolder.SetActive(false);
        //_uiHolder.SetActive(true);
    }
    public void CustomCourseAdded()
    {
        if (!this.IsOwner)
            return;

        Debug.Log("CustomCourseAdded: re-enabling UI");
        _promptPlayerHolder.SetActive(false);
        _uiHolder.SetActive(true);

        CmdTellServerCusomeCourseAdded();
    }
    [ServerRpc]
    void CmdTellServerCusomeCourseAdded()
    {
        Debug.Log("CmdTellServerCusomeCourseAdded: ");
        GameplayManagerTopDownGolf.instance.PlayerDownDownloadingCustomCourse(this.Owner);
    }
}
