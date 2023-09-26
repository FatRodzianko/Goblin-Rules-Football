using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby instance;

    private List<GameObject> listOfLobbyListItems = new List<GameObject>();

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<LobbyMatchList_t> Callback_lobbyList;
    protected Callback<LobbyDataUpdate_t> Callback_lobbyInfo;

    public ulong current_lobbyID;
    public List<CSteamID> lobbyIDS = new List<CSteamID>();

    private const string HostAddressKey = "HostAddress";

    public bool JoiningMirror = false;

    private NetworkManager networkManager;

    struct LobbyMetaData
    {
        public string m_Key;
        public string m_Value;
    }

    struct LobbyMembers
    {
        public CSteamID m_SteamID;
        public LobbyMetaData[] m_Data;
    }
    struct Lobby
    {
        public CSteamID m_SteamID;
        public CSteamID m_Owner;
        public LobbyMembers[] m_Members;
        public int m_MemberLimit;
        public LobbyMetaData[] m_Data;
    }

    // Start is called before the first frame update
    private void Start()
    {
        networkManager = GetComponent<NetworkManager>();

        if (!SteamManager.Initialized) { return; }
        MakeInstance();

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        Callback_lobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbiesList);
        Callback_lobbyInfo = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyInfo);
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    public void HostLobby()
    {

    }
    public void JoinLobby(CSteamID lobbyId)
    {
        Debug.Log("JoinLobby: Will try to join lobby with steam id: " + lobbyId.ToString());
        SteamMatchmaking.JoinLobby(lobbyId);
    }
    public void GetListOfLobbies()
    {

        if (lobbyIDS.Count > 0)
            lobbyIDS.Clear();

        SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(1);

        SteamAPICall_t try_getList = SteamMatchmaking.RequestLobbyList();
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (!JoiningMirror)
            return;
        Debug.Log("OnLobbyCreated");
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            return;
        }

        networkManager.StartHost();

        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            HostAddressKey,
            SteamUser.GetSteamID().ToString());
        if (TitleScreenManager.instance.didPlayerNameTheLobby)
        {
            SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "name",
            TitleScreenManager.instance.lobbyName);
            TitleScreenManager.instance.didPlayerNameTheLobby = false;
        }
        else
        {
            string lobbyName;
            string steamUserName = SteamFriends.GetPersonaName().ToString();
            if (steamUserName.Length > 12)
            {
                lobbyName = steamUserName.Substring(0, 12);
            }
            else
                lobbyName = steamUserName;

            SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "name",
            lobbyName + "'s lobby");
        }
        // Only needed when doing testing with the Spacewar Appid
        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "GameName",
            "GRF");
        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "GameMode",
            "Football");

    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        if (!JoiningMirror)
            return;
        Debug.Log("OnGameLobbyJoinRequested");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (!JoiningMirror)
            return;
        current_lobbyID = callback.m_ulSteamIDLobby;
        Debug.Log("OnLobbyEntered for lobby with id: " + current_lobbyID.ToString());
        if (NetworkServer.active) { return; }

        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            HostAddressKey);

        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();
        lobbyIDS.Clear();
        if (GameObject.Find("TitleScreenManager"))
        {
            if (TitleScreenManager.instance.listOfLobbyListItems.Count > 0)
                TitleScreenManager.instance.DestroyOldLobbyListItems();
        }
    }
    void OnGetLobbiesList(LobbyMatchList_t result)
    {
        Debug.Log("Found " + result.m_nLobbiesMatching + " lobbies!");
        if (TitleScreenManager.instance.listOfLobbyListItems.Count > 0)
            TitleScreenManager.instance.DestroyOldLobbyListItems();
        for (int i = 0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIDS.Add(lobbyID);
            SteamMatchmaking.RequestLobbyData(lobbyID);

        }

    }
    void OnGetLobbyInfo(LobbyDataUpdate_t result)
    {
        TitleScreenManager.instance.DisplayLobbies(lobbyIDS, result);
    }
    void DestroyOldLobbyListItems()
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

    public void CreateNewLobby(ELobbyType lobbyType)
    {
        SteamMatchmaking.CreateLobby(lobbyType, networkManager.maxConnections);
    }
}
