using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Managing;
using FishNet.Managing.Scened;
using Steamworks;
using TMPro;
using System;

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
    [SerializeField] public string LobbyName;
    [SerializeField] public int NumberOfPlayers = 1;
    public int MinNumberOfPlayers = 1;
    public int MaxNumberOfPlayers = 10;
    [SerializeField] public bool FriendsOnly;
    [SerializeField] public bool PowerUpsEnabled;
    [SerializeField] public bool WeatherStatuesEnabled;
    [SerializeField] public bool StrokeLimitEnabled;
    [SerializeField] public int StrokeLimitNumber;
    int _strokeLimitMin = 6;
    [SerializeField] int _strokeLimitDefault = 12;
    [SerializeField] public RainManager.RainMode GameRainMode = RainManager.RainMode.ControlledByPlayerFavor;
    [SerializeField] public WindManager.WindMode GameWindMode = WindManager.WindMode.ControlledByPlayerFavor;
    [SerializeField] public string SelectedCourseID;
    [SerializeField] public string CourseHoleSelection;
    [SerializeField] public bool ParFavorPenalty = false;

    [Header("Golf Course Settings")]
    [SerializeField] public string SelectedCourseId;
    [SerializeField] public string CourseName;
    [SerializeField] public string CourseHolesToPlay;
    [SerializeField] public List<int> CustomHolesToPlay = new List<int>();



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
    public void CreateLobby(string lobbyName, int numberOfPlayers, bool friendsOnly, bool powerUpsEnabled, bool spawnStatues, bool strokeLimitEnabled, int strokeLimitNumber, string rainMode, string windMode, string courseHoleSelection, List<int> customHolesSelected, bool parFavorPenalty, string selectedCourseId, string selectedCourseName)
    {
        Debug.Log("GolfSteamLobby: CreateLobby: number of players: " + numberOfPlayers.ToString() + " selected course id: " + selectedCourseId);

        ResetGameSettingsToDefault();
        // set the lobby name
        if (string.IsNullOrEmpty(lobbyName))
        {
            string steamUserName = SteamFriends.GetPersonaName().ToString();
            if (steamUserName.Length > 12)
                steamUserName = steamUserName.Substring(0, 12);
            LobbyName = steamUserName + "'s lobby";
        }
        else
        {
            LobbyName = lobbyName;
        }

        // set the number of players for a lobby
        if (numberOfPlayers < MinNumberOfPlayers)
            numberOfPlayers = MinNumberOfPlayers;
        else if (numberOfPlayers > MaxNumberOfPlayers)
            numberOfPlayers = MaxNumberOfPlayers;
        NumberOfPlayers = numberOfPlayers;

        ELobbyType newLobbyType = ELobbyType.k_ELobbyTypePublic; 
        if (friendsOnly)
        {
            Debug.Log("GolfSteamLobby: CreateLobby: friendsOnlyToggle is on. Making lobby friends only.");
            newLobbyType = ELobbyType.k_ELobbyTypeFriendsOnly;
        }

        this.PowerUpsEnabled = powerUpsEnabled;
        this.WeatherStatuesEnabled = spawnStatues;
        this.StrokeLimitEnabled = strokeLimitEnabled;
        this.StrokeLimitNumber = Mathf.Abs(strokeLimitNumber);
        this.GameRainMode = GetRainMode(rainMode);
        this.GameWindMode = GetWindMode(windMode);

        this.SelectedCourseID = selectedCourseId;
        Debug.Log("GolfSteamLobby: CreateLobby: this.SelectedCourseID: " + this.SelectedCourseID);

        if (string.IsNullOrEmpty(courseHoleSelection))
            this.CourseHoleSelection = "Front 3 (holes 1-3)";
        else
            this.CourseHoleSelection = courseHoleSelection;

        this.CustomHolesToPlay.Clear();
        if (courseHoleSelection.StartsWith("Custom"))
        {
            if (customHolesSelected.Count > 0)
            {
                this.CustomHolesToPlay.AddRange(customHolesSelected);

            }
            else
            {
                this.CourseHoleSelection = "Front 3 (holes 1-3)";
            }
        }

        // If the player enabled the stroke limit but didn't enter a value, make sure the default value is set
        if (this.StrokeLimitEnabled && this.StrokeLimitNumber <= 0)
        {
            this.StrokeLimitNumber = _strokeLimitDefault;
        }
        else if (this.StrokeLimitEnabled && this.StrokeLimitNumber < _strokeLimitMin && this.StrokeLimitNumber > 0)
        {
            this.StrokeLimitNumber = _strokeLimitMin;
        }
        this.ParFavorPenalty = parFavorPenalty;

        this.CourseName = selectedCourseName;

        SteamMatchmaking.CreateLobby(newLobbyType, numberOfPlayers);
    }
    void ResetGameSettingsToDefault()
    {
        this.PowerUpsEnabled = true;
        this.WeatherStatuesEnabled = true;
        this.StrokeLimitEnabled = false;
        this.StrokeLimitNumber = 10;
    }
    RainManager.RainMode GetRainMode(string rainModeText)
    {
        if (string.IsNullOrEmpty(rainModeText))
            return RainManager.RainMode.ControlledByPlayerFavor;
        else if (rainModeText == "Random")
            return RainManager.RainMode.Random;
        else if (rainModeText == "No Rain")
            return RainManager.RainMode.NoRain;
        else
            return RainManager.RainMode.ControlledByPlayerFavor;
    }
    WindManager.WindMode GetWindMode(string windModeText)
    {
        if (string.IsNullOrEmpty(windModeText))
            return WindManager.WindMode.ControlledByPlayerFavor;
        else if (windModeText == "Random")
            return WindManager.WindMode.Random;
        else if (windModeText == "No Wind")
            return WindManager.WindMode.NoWind;
        else
            return WindManager.WindMode.ControlledByPlayerFavor;
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
        //SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "name", SteamFriends.GetPersonaName().ToString() + "'s lobby");
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "name", this.LobbyName);
        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "GameName",
            "GRF");
        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "GameMode",
            "Golf");
        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "CourseName",
            this.CourseName);
        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "GameStatus",
            "Lobby");



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

        string lobbyGameStatus = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "GameStatus");
        Debug.Log("GolfSteamLobby: OnLobbyEntered: game status: " + lobbyGameStatus);
        if (!lobbyGameStatus.Equals("Lobby"))
            return;
        SceneLoadData sld = new SceneLoadData("Golf-prototype-topdown");
        sld.ReplaceScenes = ReplaceOption.All;
        FishNet.InstanceFinder.SceneManager.LoadGlobalScenes(sld);

        _fishySteamWorks.StartConnection(false);
    }
    public void LeaveLobby()
    {
        try
        {
            if (CurrentLobbyID == 0)
                return;
            SteamMatchmaking.LeaveLobby((CSteamID)CurrentLobbyID);
            CurrentLobbyID = 0;
        }
        catch (Exception e)
        {
            Debug.Log("GolfSteamLobby: LeaveLobby: failed to leave lobby? Error: " + e);
        }
        
    }
    public void SetGameStatusToInGame()
    {
        Debug.Log("SetGameStatusToInGame: for lobby ID: " + CurrentLobbyID.ToString());
        SteamMatchmaking.SetLobbyData(
            new CSteamID(CurrentLobbyID),
            "GameStatus",
            "InGame");
    }
}
