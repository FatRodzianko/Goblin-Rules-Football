using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Golf's rows on the Single Player / Host Game panel. Course and hole choices come from
/// <see cref="CustomGolfCourseLoader"/>, so built-in and workshop courses both appear.
///
/// A solo game is a lobby of one, which is what sends it down SteamLobby's loopback path, so the
/// only difference between the two modes here is the player count and the rows that go with it.
/// </summary>
public class GolfSetupPanel : GameSetupPanel
{
    private const int SoloPlayerCount = 1;
    private const int MinHostedPlayers = 2;
    private const int MaxPlayers = 10;
    private const int DefaultHostedPlayers = 10;
    private const int DefaultStrokeLimit = 12;

    [Header("Rows shown only when hosting")]
    [SerializeField] private GameObject[] _hostOnlyRows;
    [SerializeField] private TMP_InputField _lobbyName;
    [SerializeField] private TMP_InputField _numberOfPlayers;
    [SerializeField] private Toggle _friendsOnly;

    [Header("Course")]
    [SerializeField] private TMP_Dropdown _course;
    [SerializeField] private TMP_Dropdown _holeSelection;
    [SerializeField] private GameObject _customHolesRow;
    [SerializeField] private Toggle[] _customHoles;

    [Header("Rules")]
    [SerializeField] private Toggle _powerUps;
    [SerializeField] private Toggle _weatherStatues;
    [SerializeField] private Toggle _strokeLimit;
    [SerializeField] private TMP_InputField _strokeLimitNumber;
    [SerializeField] private TMP_Dropdown _rainMode;
    [SerializeField] private TMP_Dropdown _windMode;
    [SerializeField] private Toggle _parPenaltyFavour;

    [Header("Hole selection labels, as GolfLobbySettings expects them")]
    [SerializeField] private string _front3 = "Front 3 (holes 1-3)";
    [SerializeField] private string _middle3 = "Middle 3 (holes 4-6)";
    [SerializeField] private string _back3 = "Back 3 (holes 7-9)";
    [SerializeField] private string _allNine = "All Nine Holes";
    [SerializeField] private string _custom = "Custom Selection";

    private CustomGolfCourseLoader _loader;
    private string _selectedCourseId = string.Empty;
    private string _selectedCourseName = string.Empty;
    private ulong _selectedCourseWorkshopId;

    private void Start()
    {
        _loader = CustomGolfCourseLoader.GetInstance();
        if (_loader != null)
            _loader.GetAllAvailableCourses();

        if (_course != null)
            _course.onValueChanged.AddListener(OnCourseChanged);
        if (_holeSelection != null)
            _holeSelection.onValueChanged.AddListener(OnHoleSelectionChanged);

        RefreshCourses();
    }

    private void OnDestroy()
    {
        if (_course != null)
            _course.onValueChanged.RemoveListener(OnCourseChanged);
        if (_holeSelection != null)
            _holeSelection.onValueChanged.RemoveListener(OnHoleSelectionChanged);
    }

    public override void SetHosting(bool isHosting)
    {
        if (_hostOnlyRows == null)
            return;
        for (int i = 0; i < _hostOnlyRows.Length; i++)
        {
            if (_hostOnlyRows[i] != null)
                _hostOnlyRows[i].SetActive(isHosting);
        }
    }

    public override void CreateGame(bool isHosting)
    {
        if (GolfLobbySettings.instance == null)
        {
            Debug.LogWarning("GolfSetupPanel: no GolfLobbySettings in the scene.");
            return;
        }
        if (string.IsNullOrEmpty(_selectedCourseId))
        {
            Debug.LogWarning("GolfSetupPanel: no course selected, so there is nothing to play.");
            return;
        }

        // A lobby of one is how a solo round is asked for; hosting always leaves room for someone else.
        int players = isHosting
            ? ReadClampedInt(_numberOfPlayers, DefaultHostedPlayers, MinHostedPlayers, MaxPlayers)
            : SoloPlayerCount;

        string holeSelection = DropdownText(_holeSelection, _front3);
        List<int> customHoles = new List<int>();
        if (holeSelection.StartsWith("Custom"))
            customHoles.AddRange(SelectedCustomHoles());

        GolfLobbySettings.instance.CreateLobby(
            isHosting && _lobbyName != null ? _lobbyName.text : string.Empty,
            players,
            isHosting && IsOn(_friendsOnly),
            IsOn(_powerUps),
            IsOn(_weatherStatues),
            IsOn(_strokeLimit),
            ReadClampedInt(_strokeLimitNumber, DefaultStrokeLimit, 1, int.MaxValue),
            DropdownText(_rainMode, string.Empty),
            DropdownText(_windMode, string.Empty),
            holeSelection,
            customHoles,
            IsOn(_parPenaltyFavour),
            _selectedCourseId,
            _selectedCourseName,
            _selectedCourseWorkshopId);
    }

    private void RefreshCourses()
    {
        if (_course == null || _loader == null || _loader.AllAvailableCourses == null)
            return;
        List<ScriptableCourse> courses = _loader.AllAvailableCourses.Courses;
        if (courses == null || courses.Count == 0)
            return;

        List<string> options = new List<string>();
        for (int i = 0; i < courses.Count; i++)
            options.Add(courses[i].CourseName);

        _course.ClearOptions();
        _course.AddOptions(options);
        _course.SetValueWithoutNotify(0);
        OnCourseChanged(0);
    }

    private void OnCourseChanged(int index)
    {
        if (_loader == null || _loader.AllAvailableCourses == null)
            return;
        List<ScriptableCourse> courses = _loader.AllAvailableCourses.Courses;
        if (courses == null || index < 0 || index >= courses.Count)
            return;

        ScriptableCourse course = courses[index];
        _selectedCourseId = course.id;
        _selectedCourseName = course.CourseName;
        _selectedCourseWorkshopId = course.WorkshopID;

        int holes = course.HolesInCourse != null ? course.HolesInCourse.Length : 0;

        List<string> holeOptions = new List<string>();
        if (holes >= 3)
            holeOptions.Add(_front3);
        if (holes >= 6)
            holeOptions.Add(_middle3);
        if (holes >= 9)
        {
            holeOptions.Add(_back3);
            holeOptions.Add(_allNine);
        }
        holeOptions.Add(_custom);

        if (_holeSelection != null)
        {
            _holeSelection.ClearOptions();
            _holeSelection.AddOptions(holeOptions);
            _holeSelection.SetValueWithoutNotify(0);
        }

        // Only offer the holes this course actually has.
        if (_customHoles != null)
        {
            for (int i = 0; i < _customHoles.Length; i++)
            {
                if (_customHoles[i] == null)
                    continue;
                bool exists = i < holes;
                if (!exists)
                    _customHoles[i].isOn = false;
                _customHoles[i].gameObject.SetActive(exists);
            }
        }

        OnHoleSelectionChanged(0);
    }

    private void OnHoleSelectionChanged(int index)
    {
        if (_customHolesRow == null)
            return;
        _customHolesRow.SetActive(DropdownText(_holeSelection, _front3).StartsWith("Custom"));
    }

    private List<int> SelectedCustomHoles()
    {
        List<int> holes = new List<int>();
        if (_customHoles == null)
            return holes;
        for (int i = 0; i < _customHoles.Length; i++)
        {
            if (_customHoles[i] != null && _customHoles[i].gameObject.activeSelf && _customHoles[i].isOn)
                holes.Add(i + 1);
        }
        return holes;
    }

    private static string DropdownText(TMP_Dropdown dropdown, string fallback)
    {
        if (dropdown == null || dropdown.options == null || dropdown.options.Count == 0)
            return fallback;
        int value = Mathf.Clamp(dropdown.value, 0, dropdown.options.Count - 1);
        return dropdown.options[value].text;
    }

    private static bool IsOn(Toggle toggle)
    {
        return toggle != null && toggle.isOn;
    }

    private static int ReadClampedInt(TMP_InputField field, int fallback, int min, int max)
    {
        if (field == null || string.IsNullOrWhiteSpace(field.text))
            return fallback;
        int value;
        if (!int.TryParse(field.text, out value))
            return fallback;
        return Mathf.Clamp(value, min, max);
    }
}
