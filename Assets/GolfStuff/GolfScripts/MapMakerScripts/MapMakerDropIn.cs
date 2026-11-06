using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Drop-in play: hands the hole currently being edited to a one player golf session so the player can
// hit a ball around it, then puts the map maker back as it was.
//
// The hole does not have to be finished and is not saved first. It is read off the tilemaps as they
// stand, registered as a temporary one hole course, and played through the same solo golf session
// the title screen uses for a single player round - loopback Tugboat, no Steam, no lobby.
public class MapMakerDropIn : MonoBehaviour
{
    [SerializeField] MapMakerUIManager _mapMakerUIManager;
    [SerializeField] MapMakerBuilder _mapMakerBuilder;
    [SerializeField] SaveHandler _saveHandler;
    [SerializeField] Button _dropInButton;

    [Header("Drop-in session settings")]
    [SerializeField] string _tempCourseName = "Map Maker Test";
    [SerializeField] int _defaultPar = 3;

    const string TempCourseId = "map-maker-drop-in";

    void Start()
    {
        if (!_mapMakerUIManager)
            _mapMakerUIManager = this.GetComponent<MapMakerUIManager>();
        if (!_saveHandler)
            _saveHandler = this.GetComponent<SaveHandler>();
        if (!_mapMakerBuilder)
            _mapMakerBuilder = MapMakerBuilder.GetInstance();

        if (_dropInButton != null)
        {
            _dropInButton.onClick.AddListener(StartDropIn);
            _mapMakerBuilder.HasTeeOffLocationBeenPlacedYetChanged += TeeOffOrHolePlacedChanged;
            _mapMakerBuilder.HasHoleBeenPlacedYetChanged += TeeOffOrHolePlacedChanged;
            UpdateDropInButtonInteractable();
        }

        StartCoroutine(RestoreHoleAfterDropIn());
    }
    void OnDestroy()
    {
        if (_dropInButton != null && _mapMakerBuilder != null)
        {
            _mapMakerBuilder.HasTeeOffLocationBeenPlacedYetChanged -= TeeOffOrHolePlacedChanged;
            _mapMakerBuilder.HasHoleBeenPlacedYetChanged -= TeeOffOrHolePlacedChanged;
        }
    }
    void TeeOffOrHolePlacedChanged(bool placed)
    {
        UpdateDropInButtonInteractable();
    }
    // A hole with no tee off location has nowhere to put the player, which is the one thing drop-in
    // cannot do without. Everything else about the hole may be missing.
    // A tee off location, because that is where the player appears, and a hole, because the golf
    // player aims at the nearest one and measures distance to it from half a dozen places. Nothing
    // else about the hole has to be finished.
    bool CanDropIn
    {
        get
        {
            return _mapMakerBuilder != null && _mapMakerBuilder.HasTeeOffLocationBeenPlaced && _mapMakerBuilder.HasHoleBeenPlaced;
        }
    }
    void UpdateDropInButtonInteractable()
    {
        bool canDropIn = CanDropIn;
        if (_dropInButton.interactable != canDropIn)
            _dropInButton.interactable = canDropIn;
    }
    public void StartDropIn()
    {
        if (_mapMakerBuilder == null || _mapMakerUIManager == null)
            return;

        if (!CanDropIn)
        {
            Debug.Log("StartDropIn: a hole needs a tee off location and a hole before it can be tested.");
            return;
        }

        if (GolfLobbySettings.instance == null)
        {
            Debug.LogWarning("StartDropIn: no GolfLobbySettings, so no session can be started.");
            return;
        }

        HoleData holeToTest = new HoleData();
        holeToTest.HoleIndex = 1;
        holeToTest.HolePar = _defaultPar;
        holeToTest.CourseName = _tempCourseName;
        _mapMakerUIManager.FillHoleDataFromCurrentMap(holeToTest, _mapMakerUIManager.IsSelectedCourseMiniGolf);

        CustomGolfCourseLoader loader = CustomGolfCourseLoader.GetInstance();
        if (loader == null)
        {
            Debug.LogWarning("StartDropIn: no CustomGolfCourseLoader, so the hole cannot be handed to a session.");
            return;
        }
        ScriptableCourse courseToPlay = loader.RegisterTemporaryCourse(holeToTest, _tempCourseName, TempCourseId);
        if (courseToPlay == null)
        {
            Debug.LogWarning("StartDropIn: the temporary course could not be built.");
            return;
        }

        // Remembered so the map maker can put this hole back when the session ends, unsaved edits
        // and all, so the golf session knows to come back here instead of the title screen, and so
        // the hole can start without waiting for the course to be looked up over the network.
        MapMakerDropInSession.Begin(holeToTest, TempCourseId, courseToPlay, _mapMakerUIManager.SelectedCourseId, _mapMakerUIManager.SelectedHoleNumber);

        // A lobby of one player runs the solo golf session: loopback Tugboat, no Steam lobby. No
        // weather, power ups or statues while testing, and no stroke limit.
        GolfLobbySettings.instance.CreateLobby(
            _tempCourseName,
            1,
            false,
            false,
            false,
            false,
            0,
            "No Rain",
            "No Wind",
            "Front 3 (holes 1-3)",
            new List<int>(),
            false,
            TempCourseId,
            _tempCourseName,
            0);
    }
    // Coming back from a drop-in session. The map maker scene has reloaded empty, so put back the
    // hole that was being tested.
    IEnumerator RestoreHoleAfterDropIn()
    {
        if (!MapMakerDropInSession.IsActive)
            yield break;

        // Restoring writes through MapMakerBuilder's tilemap lookup, which the builder fills in its
        // own Start, and the course list is built in MapMakerUIManager's. Waiting a frame puts this
        // after every Start rather than in a race with them.
        yield return null;

        HoleData holeToRestore = MapMakerDropInSession.HoleBeingTested;
        string courseId = MapMakerDropInSession.CourseIdBeingEdited;
        int holeNumber = MapMakerDropInSession.HoleNumberBeingEdited;

        CustomGolfCourseLoader loader = CustomGolfCourseLoader.GetInstance();
        if (loader != null)
            loader.RemoveTemporaryCourse(MapMakerDropInSession.TempCourseId);

        MapMakerDropInSession.End();

        if (holeToRestore == null || _saveHandler == null)
            yield break;

        // The course and hole first, so the restored tiles belong to the hole they came from and
        // Save Hole still knows where to put them.
        _mapMakerUIManager.ReselectCourseAndHole(courseId, holeNumber);

        Debug.Log("RestoreHoleAfterDropIn: putting the tested hole back on the tilemaps.");
        _saveHandler.LoadTileMapData(holeToRestore.HoleTileMapData);
        MapMakerHistory.GetInstance().ClearHistoryForNewHole();
    }
}
