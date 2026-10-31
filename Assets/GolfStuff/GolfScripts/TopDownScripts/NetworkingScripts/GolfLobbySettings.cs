using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

// Golf's game settings, chosen on the title screen when hosting. GameplayManagerTopDownGolf reads them when the game starts.
// The Steam lobby itself is SteamLobby's.
public class GolfLobbySettings : MonoBehaviour
{
    public static GolfLobbySettings instance;

    [Header("Golf Game Settings")]
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
    [SerializeField] public ulong CourseWorkshopID;


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
    // Stores the settings, then has SteamLobby create the Golf lobby. An empty lobby name gets SteamLobby's default.
    public void CreateLobby(string lobbyName, int numberOfPlayers, bool friendsOnly, bool powerUpsEnabled, bool spawnStatues, bool strokeLimitEnabled, int strokeLimitNumber, string rainMode, string windMode, string courseHoleSelection, List<int> customHolesSelected, bool parFavorPenalty, string selectedCourseId, string selectedCourseName, ulong selectedCourseWorkshopID)
    {
        Debug.Log("GolfLobbySettings: CreateLobby: number of players: " + numberOfPlayers.ToString() + " selected course id: " + selectedCourseId + " and a workshop ID of: " + selectedCourseWorkshopID);

        ResetGameSettingsToDefault();

        // set the number of players for a lobby
        if (numberOfPlayers < MinNumberOfPlayers)
            numberOfPlayers = MinNumberOfPlayers;
        else if (numberOfPlayers > MaxNumberOfPlayers)
            numberOfPlayers = MaxNumberOfPlayers;
        NumberOfPlayers = numberOfPlayers;

        ELobbyType newLobbyType = ELobbyType.k_ELobbyTypePublic;
        if (friendsOnly)
        {
            Debug.Log("GolfLobbySettings: CreateLobby: friendsOnlyToggle is on. Making lobby friends only.");
            newLobbyType = ELobbyType.k_ELobbyTypeFriendsOnly;
        }

        this.PowerUpsEnabled = powerUpsEnabled;
        this.WeatherStatuesEnabled = spawnStatues;
        this.StrokeLimitEnabled = strokeLimitEnabled;
        this.StrokeLimitNumber = Mathf.Abs(strokeLimitNumber);
        this.GameRainMode = GetRainMode(rainMode);
        this.GameWindMode = GetWindMode(windMode);

        this.SelectedCourseID = selectedCourseId;
        Debug.Log("GolfLobbySettings: CreateLobby: this.SelectedCourseID: " + this.SelectedCourseID);

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
        this.CourseWorkshopID = selectedCourseWorkshopID;

        // A lobby of one is effectively single player - lobbies cannot grow once started - so it runs offline on
        // loopback Tugboat instead of creating a Steam lobby nobody else could ever join.
        if (numberOfPlayers == 1)
            SteamLobby.instance.StartSoloGolfSession();
        else
            SteamLobby.instance.CreateGolfLobby(newLobbyType, numberOfPlayers, lobbyName, this.CourseName);
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
}
