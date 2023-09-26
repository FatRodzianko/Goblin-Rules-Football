using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Managing;
using FishNet.Managing.Scened;
using Steamworks;
using TMPro;

public class GolfSteamLobby : MonoBehaviour
{
    public static GolfSteamLobby instance;

    [SerializeField] NetworkManager _networkManager;
    [SerializeField] private FishySteamworks.FishySteamworks _fishySteamWorks;

    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;

    [SerializeField] public static ulong CurrentLobbyID;
    [SerializeField] TextMeshProUGUI _lobbyNumber;
    [SerializeField] public string CurrentLobbyName;

    public bool JoiningFishNet;

    [Header("Golf Game Settings")]
    public bool IsThereAStrokeLimit = false;
    public int StrokeLimitNumber = 0;
    public string CourseName;

    private void Awake()
    {
        MakeInstance();
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
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void CreateLobby()
    {
        Debug.Log("GolfSteamLobby: CreateLobby: ");
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 10);
    }
    public void JoinLobby(CSteamID lobbyID)
    {
        Debug.Log("JoinLobby: Will try to join lobby with steam id: " + lobbyID.ToString());
        SteamMatchmaking.JoinLobby(lobbyID);
    }
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (!JoiningFishNet)
            return;
        Debug.Log("GolfSteamLobby: OnLobbyCreated: " + callback.m_eResult.ToString());
        if (callback.m_eResult != EResult.k_EResultOK)
            return;
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        CurrentLobbyName = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name");
        //_lobbyNumber.text = CurrentLobbyID.ToString();
        Debug.Log("GolfSteamLobby: OnLobbyCreated: Lobby ID" + CurrentLobbyID.ToString());
        Debug.Log("GolfSteamLobby: OnLobbyCreated: Current Steam UseR: " + SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "HostAddress", SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "name", SteamFriends.GetPersonaName().ToString() + "'s lobby");
        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "GameName",
            "GRF");
        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "GameMode",
            "Golf");
        _fishySteamWorks.SetClientAddress(SteamUser.GetSteamID().ToString());
        // start connection as the server
        
        _fishySteamWorks.StartConnection(true);
    }
    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        if (!JoiningFishNet)
            return;
        Debug.Log("GolfSteamLobby: OnJoinRequest: " + callback.m_steamIDLobby.ToString());
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (!JoiningFishNet)
            return;
        Debug.Log("GolfSteamLobby: OnLobbyEntered: " + callback.m_ulSteamIDLobby.ToString());
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        CurrentLobbyName = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name");
        //_lobbyNumber.text = CurrentLobbyID.ToString();

        if (GameObject.Find("TitleScreenManager"))
        {
            if (TitleScreenManager.instance.listOfLobbyListItems.Count > 0)
                TitleScreenManager.instance.DestroyOldLobbyListItems();
        }


        _fishySteamWorks.SetClientAddress(SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "HostAddress"));
        // start connection as the client
        SceneLoadData sld = new SceneLoadData("Golf-prototype-topdown");
        sld.ReplaceScenes = ReplaceOption.All;
        FishNet.InstanceFinder.SceneManager.LoadGlobalScenes(sld);

        _fishySteamWorks.StartConnection(false);
    }
}
