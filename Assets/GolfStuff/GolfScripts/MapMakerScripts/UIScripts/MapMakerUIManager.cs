using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;
using System;
using System.Linq;

public class MapMakerUIManager : MonoBehaviour
{
    [SerializeField] MapMakerBuilder _mapMakerBuilder;
    [SerializeField] SaveHandler _saveHandler;
    MapMakerHistory _mapMakerHistory;

    [Header("Scriptables")]
    [SerializeField] List<MapMakerGroundTileBase> _green;
    [SerializeField] List<MapMakerGroundTileBase> _fairway;

    [Header("UI Prefabs")]
    [SerializeField] GameObject _groundTypeItemPrefab;
    [SerializeField] GameObject _uiTileTypePrefab;

    [Header("Map Tiles UI")]
    [SerializeField] GameObject _greenItems;
    [SerializeField] GameObject _fairwayItems;

    // https://www.youtube.com/watch?v=dCrkOIylNSw&list=PLJBcv4t1EiSz-wA35-dWpcI98pNiyK6an&index=4
    [Header("UI Elements (from video")]
    [SerializeField] List<UITileTypes> _uiTileTypes;
    [SerializeField] Transform _groundTileTypeHolder;

    Dictionary<UITileTypes, GameObject> _uiElements = new Dictionary<UITileTypes, GameObject>();
    Dictionary<GameObject, Transform> _elementItemSlot = new Dictionary<GameObject, Transform>();

    [Header("UI Objects")]
    Dictionary<GameObject, MapMakerGroundTileBase> _allTileObjectsByTileBase = new Dictionary<GameObject, MapMakerGroundTileBase>();
    [SerializeField] List<GameObject> _manualTileObjects = new List<GameObject>();
    [SerializeField] List<GameObject> _ruleTileObjects = new List<GameObject>();

    [Header("Tiling Mode UI")]
    [SerializeField] Button _autoTileButton;
    [SerializeField] Button _manualTileButton;
    [SerializeField] Color _tileModeSelectedColor = Color.yellow;

    [Header("Current Tile Map UI")]
    [SerializeField] TMP_Dropdown _currentTileMapDropDown;
    Dictionary<string, Tilemap> _tileMapsToSelect = new Dictionary<string, Tilemap>();

    [Header("Current Course UI")]
    [SerializeField] TMP_Dropdown _currentCourseDropDown;
    [SerializeField] Button _createNewCourseButton;    

    [Header("Create New Course UI")]
    [SerializeField] GameObject _createNewCoursePanel;
    [SerializeField] TMP_InputField _nameOfCourseTextInput;
    [SerializeField] Toggle _isMiniGolfToggle;

    [Header("Current Hole UI")]
    [SerializeField] GameObject _currentHoleText;
    [SerializeField] TMP_Dropdown _currentHoleDropDown;
    [SerializeField] Button _createNewHoleButton;

    [Header("Selected Course")]
    [SerializeField] CourseData _selectedCourse;
    private List<CourseData> _allCustomCourses = new List<CourseData>();
    public delegate void NewCourseSelected(CourseData newCourse);
    public event NewCourseSelected NewCourseSelectedChanged;

    [Header("Selected Hole")]
    [SerializeField] int _selectedHoleNumber;
    [SerializeField] HoleData _selectedHole;
    public delegate void NewHoleSelected(int holeNumber);
    public event NewHoleSelected NewHoleSelectedChanged;

    [Header("Create/Edit Hole UI")]
    [SerializeField] GameObject _createNewHolePanel;
    [SerializeField] TextMeshProUGUI _createNewHoleText;
    [SerializeField] TMP_InputField _newHoleParTextInput;    
    [SerializeField] Button _saveNewHoleDetailsButton;
    [SerializeField] Button _cancelNewHoleDetailsButton;


    [Header("Save/Load Hole UI")]
    [SerializeField] Button _saveHoleButton;
    [SerializeField] Button _loadHoleButton;


    #region Setters and Getters
    Dictionary<string, Tilemap> TileMapsToSelect
    {
        get
        {
            return _tileMapsToSelect;
        }
    }
    #endregion
    private void Awake()
    {
        _autoTileButton.onClick.AddListener(SelectAutoTileMode);
        _manualTileButton.onClick.AddListener(SelectManualTileMode);
        _mapMakerHistory = MapMakerHistory.GetInstance();
        _mapMakerHistory.CanUndoChanged += EnableSaveHoleButton;

        // set select course and hole to null to start?
        _selectedCourse = null;
        _selectedHole = null;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (!_mapMakerBuilder)
            _mapMakerBuilder = MapMakerBuilder.GetInstance();
        //LoadGroundTileTypesForUI();
        InitializeCurrentTileDropDown();
        BuildUI();
        //Default to rule tile mode to begin
        SelectAutoTileMode();
        GetAllCustomCourse();
        if (_saveHandler == null)
            _saveHandler = this.GetComponent<SaveHandler>();
    }
    private void OnEnable()
    {
        NewCourseSelectedChanged += NewCourseSelectedChangedFunction;
        NewHoleSelectedChanged += NewHoleSelectedChangedFunction;
    }
    private void OnDisable()
    {
        NewCourseSelectedChanged -= NewCourseSelectedChangedFunction;
        NewHoleSelectedChanged -= NewHoleSelectedChangedFunction;
        _mapMakerHistory.CanUndoChanged -= EnableSaveHoleButton;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //void LoadGroundTileTypesForUI()
    //{
    //    if (_green.Count > 0)
    //    {
    //        AddItemToUI(_green, _greenItems);
    //    }
    //    if (_fairway.Count > 0)
    //    {
    //        AddItemToUI(_fairway, _fairwayItems);
    //    }
    //}
    //void AddItemToUI(List<MapMakerGroundTileBase> tiles, GameObject uiHolder)
    //{
    //    foreach (MapMakerGroundTileBase tile in tiles)
    //    {
    //        GameObject newTileItem = Instantiate(_groundTypeItemPrefab, uiHolder.transform);
    //        TileButtonHandler newTileItemButtonHandler = newTileItem.GetComponent<TileButtonHandler>();
    //        newTileItemButtonHandler.SetGroundTileItem(tile);
    //    }
    //}
    void BuildUI()
    {
        foreach (UITileTypes ui in _uiTileTypes)
        {
            if (_uiElements.ContainsKey(ui))
                continue;
            var inst = Instantiate(_uiTileTypePrefab, Vector3.zero, Quaternion.identity);
            inst.transform.SetParent(_groundTileTypeHolder, false);

            inst.name = ui.name;
            
            _uiElements[ui] = inst;
            _elementItemSlot[inst] = inst.GetComponent<UITileTypeScript>().ItemHolder;

            TextMeshProUGUI text = inst.GetComponentInChildren<TextMeshProUGUI>();
            //text.text = ui.name;
            text.text = ui.UIName;

            inst.transform.SetSiblingIndex(ui.SiblingIndex);

            Image img = inst.GetComponentInChildren<Image>();
            img.color = ui.BackgroundColor;
        }

        MapMakerGroundTileBase[] groundTiles = GetAllGroundTiles();

        foreach (MapMakerGroundTileBase groundTileBase in groundTiles)
        {
            if (groundTileBase.UITileType == null)
                continue;


            var itemsParent = _elementItemSlot[_uiElements[groundTileBase.UITileType]];
            var inst = Instantiate(_groundTypeItemPrefab, Vector3.zero, Quaternion.identity);
            inst.transform.SetParent(itemsParent,false);

            // name in hierarchy
            inst.name = groundTileBase.name;

            // Get the tile's sprite from the TileBase
            Image img = inst.GetComponent<Image>();

            // Check to see if the tile is a regular TileBase or a Rule sprite
            if (groundTileBase.TileBase is Tile)
            {
                
                Tile t = (Tile)groundTileBase.TileBase;
                img.sprite = t.sprite;
                //Debug.Log("BuildUI: groundTileBase.TileBase is Tile: " + groundTileBase.name + " sprite name is: " + t.sprite.name.ToString());

                // saving all tile objects that aren't rule tiles or obstacles to reference when player selects to see manual tiles
                if (groundTileBase.GetType() != typeof(MapMakerObstacle) && groundTileBase.GetType() != typeof(MapMakerTool) && groundTileBase.GroundTileType != GroundTileType.CourseMarkers && groundTileBase.GroundTileType != GroundTileType.DirectionTiles) 
                {
                    if (!_manualTileObjects.Contains(inst))
                        _manualTileObjects.Add(inst);
                }
            }
            else if (groundTileBase.TileBase is RuleTile)
            {
                
                RuleTile rt = (RuleTile)groundTileBase.TileBase;
                img.sprite = rt.m_DefaultSprite;
                //Debug.Log("BuildUI: groundTileBase.TileBase is RuleTile: " + groundTileBase.name + " sprite name is: " + rt.m_DefaultSprite.name.ToString());
                // saving the "rule tiles" to reference when player selects to see auto-tiling tiles
                if (!_ruleTileObjects.Contains(inst))
                    _ruleTileObjects.Add(inst);
            }
            else
            {
                Debug.LogError("MapMakerUIManager: BuildUI: Unknown type of tile base: " + groundTileBase.TileBase.GetType());
            }

            

            // Apply BuildingObjectBase to Button handler script thing?
            var script = inst.GetComponent<TileButtonHandler>();
            script.SetGroundTileItem(groundTileBase);

            if (!_allTileObjectsByTileBase.ContainsKey(inst))
            {
                _allTileObjectsByTileBase.Add(inst, groundTileBase);
            }

            // Build out the current tilemap UI?
            AddToCurrentTileMapDropDown(groundTileBase);

        }
    }
    MapMakerGroundTileBase[] GetAllGroundTiles()
    {
        return Resources.LoadAll<MapMakerGroundTileBase>("MapMakerGolf/GroundTileScriptables");
    }
    void EnableRegularTileObjects(bool enable)
    {
        Debug.Log("EnableRegularTileObjects: " + enable);
        if (_manualTileObjects.Count == 0)
            return;
        foreach (GameObject ob in _manualTileObjects)
        {
            // Add checks here for "course type." If the course type is minigolf, only enable _allowedInMiniGolf tiles. If it is regular golf, don't enable _miniGolfOnly tiles
            ob.SetActive(enable);
        }
    }
    void EnableRuleTileObjects(bool enable)
    {
        Debug.Log("EnableRuleTileObjects: " + enable);
        if (_ruleTileObjects.Count == 0)
            return;
        foreach (GameObject ob in _ruleTileObjects)
        {
            // Add checks here for "course type." If the course type is minigolf, only enable _allowedInMiniGolf tiles. If it is regular golf, don't enable _miniGolfOnly tiles
            ob.SetActive(enable);
        }
    }
    void SelectAutoTileMode()
    {
        Debug.Log("SelectAutoTileMode: ");
        EnableRuleTileObjects(true);
        EnableRegularTileObjects(false);
        _autoTileButton.GetComponent<Image>().color = _tileModeSelectedColor;
        _manualTileButton.GetComponent<Image>().color = Color.white;
    }
    void SelectManualTileMode()
    {
        Debug.Log("SelectManualTileMode: ");
        EnableRuleTileObjects(false);
        EnableRegularTileObjects(true);
        _autoTileButton.GetComponent<Image>().color = Color.white;
        _manualTileButton.GetComponent<Image>().color = _tileModeSelectedColor;
    }
    void InitializeCurrentTileDropDown()
    {
        _tileMapsToSelect.Add("All", null);
        _currentTileMapDropDown.AddOptions(new List<string> { "All" });
        _currentTileMapDropDown.interactable = false;
    }
    void AddToCurrentTileMapDropDown(MapMakerGroundTileBase groundTileBase)
    {
        if (groundTileBase == null)
            return;
        if (groundTileBase.MapMakerTileType.Tilemap == null)
        {
            Debug.Log("AddToCurrentTileMapDropDown: groundTileBase.MapMakerTileType.Tilemap is null for " + groundTileBase.name + ". skipping...");
            return;
        }
            
        if (groundTileBase.GetType() == typeof(MapMakerTool))
        {
            Debug.Log("AddToCurrentTileMapDropDown: ground tile base " + groundTileBase.name + " is a tool. skipping...");
            return;
        }

        Tilemap newTileMap = groundTileBase.MapMakerTileType.Tilemap;

        if (_tileMapsToSelect.ContainsKey(newTileMap.name))
        {
            Debug.Log("AddToCurrentTileMapDropDown: already added " + newTileMap.name + " skipping...");
            return;
        }
        _tileMapsToSelect.Add(newTileMap.name, newTileMap);
        _currentTileMapDropDown.AddOptions(new List<string> { newTileMap.name });

    }
    public void PlayerSelectedTileObject(MapMakerGroundTileBase selectedObject)
    {
        if (selectedObject == null)
            return;
        if (selectedObject.GetType() == typeof(MapMakerTool))
        {
            _currentTileMapDropDown.interactable = true;
        }
        else
        {
            _currentTileMapDropDown.interactable = false;
            if (selectedObject.MapMakerTileType.Tilemap == null)
            {
                Debug.Log("AddToCurrentTileMapDropDown: selectedObject.MapMakerTileType.Tilemap is null for " + selectedObject.name + ". skipping...");
                return;
            }
            string tilemapName = selectedObject.MapMakerTileType.Tilemap.name;
            for (int i = 0; i < _currentTileMapDropDown.options.Count; i++)
            {
                if (_currentTileMapDropDown.options[i].text == tilemapName)
                {
                    _currentTileMapDropDown.value = i;
                    break;
                }
            }
        }
    }
    public Tilemap GetCurrentSelectedTileMap()
    {
        return _tileMapsToSelect[_currentTileMapDropDown.options[_currentTileMapDropDown.value].text];
    }
    void GetAllCustomCourse()
    {
        List<string> courses = FileHandler.FindAllCustomCourses();
        Debug.Log("GetAllCustomCourse: " + courses.Count.ToString() + " course found in " + Application.persistentDataPath.ToString());
        InitializeCurrentCourseDropDown(courses);
    }
    void InitializeCurrentCourseDropDown(List<string> filePathsForCourses)
    {
        _currentCourseDropDown.AddOptions(new List<string> { "Create New Course" });
        CurrentCourseDropDownValueChanged(0);
        if (filePathsForCourses.Count > 0)
        {
            foreach (string filepath in filePathsForCourses)
            {
                Debug.Log("InitializeCurrentCourseDropDown: course found at: " + filepath);
                try
                {
                    CourseData courseToLoad = FileHandler.ReadFromJSONFile<CourseData>(filepath, false);
                    //courseNames.Add(courseToLoad.CourseName);
                    if (courseToLoad != null)
                    {
                        _allCustomCourses.Add(courseToLoad);
                        _currentCourseDropDown.AddOptions(new List<string> { courseToLoad.CourseName });
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("InitializeCurrentCourseDropDown: could not read file at: " + filepath + ". Error: " + e);
                }
            }
        }
    }
    //public void CreateNewCourse()
    //{
    //    Debug.Log("CreateNewCourse: ");
    //    CourseData newCourse = new CourseData();
    //    newCourse.CourseId = "123";
    //    newCourse.CourseName = "New Course";
    //    newCourse.HolesInCourseFileNames = new List<string>();
    //    newCourse.IsMiniGolf = false;

    //    FileHandler.SaveToJSON<CourseData>(newCourse, "course_" + newCourse.CourseName + "_" + newCourse.CourseId + ".json");
    //}
    public void CreateNewCourseButtonPressed()
    {
        Debug.Log("CreateNewCourseButtonPressed: ");
        _createNewCoursePanel.SetActive(true);
        _createNewCourseButton.gameObject.SetActive(false);
        _mapMakerBuilder.PlayerInput.Disable();
    }
    public void CreateNewCourse()
    {
        Debug.Log("CreateNewCourse: ");
        string newCourseName = _nameOfCourseTextInput.text;
        if (string.IsNullOrEmpty(newCourseName))
            return;
        if (newCourseName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
        {
            Debug.Log("CreateNewCourse: course name: " + newCourseName + " contains invalid characters for a file.");
            return;
        }

        if (_allCustomCourses.Any(x => x.CourseName == newCourseName))
        {
            Debug.Log("CreateNewCourse: Custom course already has same name as: " + newCourseName);
            // have an error displayed to the user that they already have a course with the same name. Right now just fails silently
            // Later, when users are able to download custom maps from a host, check if they have a course with the same name as the host. If yes, check the guid and maybe a hash of the course, see if the player's matches. If not, tell the player they don't match and give option to download?
            return;
        }

        
        bool isMiniGolf = _isMiniGolfToggle.isOn;

        // OLD: MOVED TO SAVEHANDLER.cs
        //CourseData newCourse = new CourseData();
        //newCourse.CourseName = newCourseName;
        //newCourse.IsMiniGolf = isMiniGolf;
        //newCourse.CourseId = Guid.NewGuid().ToString();
        //newCourse.HolesInCourseFileNames = new List<string>(); // setting as an empty string when creating a new course. Will add to this as new holes are created?

        //string filename = "course_" + newCourse.CourseName.Replace(" ", string.Empty) + "_" + newCourse.CourseId + ".json";
        //Debug.Log("CreateNewCourse: Creating a new course with name: " + newCourseName + " and is minigolf?: " + isMiniGolf + " with a filename of: " + filename);
        //FileHandler.SaveToJSON<CourseData>(newCourse, filename);
        // OLD: MOVED TO SAVEHANDLER.cs

        _saveHandler.CreateNewCourse(newCourseName, isMiniGolf);

        _createNewCoursePanel.SetActive(false);
        _createNewCourseButton.gameObject.SetActive(false);
        _mapMakerBuilder.PlayerInput.Enable();

        //AddCourseToAvailableCustomCourses(newCourse);
    }
    public void AddCourseToAvailableCustomCourses(CourseData newCourse, bool selectNewCourse = false)
    {
        if (newCourse == null)
            return;

        if (_allCustomCourses.Contains(newCourse))
            return;

        _allCustomCourses.Add(newCourse);
        _currentCourseDropDown.AddOptions(new List<string> { newCourse.CourseName });

        if (selectNewCourse)
        {
            _currentCourseDropDown.value = _currentCourseDropDown.options.Count - 1;
            CurrentCourseDropDownValueChanged(_currentCourseDropDown.options.Count - 1);
        }
            
    }
    public void CancelCreateNewCourse()
    {
        Debug.Log("CancelCreateNewCourse: ");
        _createNewCoursePanel.SetActive(false);
        _createNewCourseButton.gameObject.SetActive(true);
        _mapMakerBuilder.PlayerInput.Enable();
    }
    public void CurrentCourseDropDownValueChanged(int index)
    {
        Debug.Log("CurrentCourseDropDownValueChanged " + index.ToString());

        if (index == 0)
        {
            _createNewCourseButton.gameObject.SetActive(true);
            //_selectedCourse = null;
            NewCourseSelectedChanged(null);
        }
        else
        {
            _createNewCourseButton.gameObject.SetActive(false);
            //_selectedCourse = _allCustomCourses[index-1]; // index minus 1 because "Create New Course" is index 0 of the drop down
            NewCourseSelectedChanged(_allCustomCourses[index - 1]);
        }
    }
    void NewCourseSelectedChangedFunction(CourseData newCourse)
    {
        _selectedCourse = newCourse;
        if (newCourse == null)
        {
            Debug.Log("NewCourseSelectedChangedFunction: null");
            _currentHoleText.SetActive(false);
            _currentHoleDropDown.gameObject.SetActive(false);
            _createNewHoleButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("NewCourseSelectedChangedFunction: " + newCourse.CourseName + ":" + newCourse.CourseId);
            _currentHoleText.SetActive(true);
            _currentHoleDropDown.gameObject.SetActive(true);
            InitializeCurrentHoleDropDown(_selectedCourse);
            CurrentHoleDropDownValueChanged(0);
        }
            
    }
    void InitializeCurrentHoleDropDown(CourseData course)
    {
        if (course == null)
            return;

        // clear any old data from the dropdown
        _currentHoleDropDown.ClearOptions();

        // Add the "create new hole" item first
        _currentHoleDropDown.AddOptions(new List<string> { "Create New Hole" });

        if (course.HolesInCourse.Count > 0)
        {
            for (int i = 0; i < course.HolesInCourse.Count; i++)
            {
                //_currentHoleDropDown.AddOptions(new List<string> { (i + 1).ToString() });
                _currentHoleDropDown.AddOptions(new List<string> { course.HolesInCourse[i].HoleIndex.ToString() });
            }
        }
    }
    public void CurrentHoleDropDownValueChanged(int index)
    {
        Debug.Log("CurrentHoleDropDownValueChanged: " + index.ToString());
        NewHoleSelectedChanged(index);
    }
    public void CreateNewHoleButtonPressed()
    {
        Debug.Log("CreateNewHoleButtonPressed: ");
        //_createNewCoursePanel.SetActive(true);
        //_createNewCourseButton.gameObject.SetActive(false);

        // will need to disable controls when the player has to enter data for the hole, such as par. For now, ignore
        //_mapMakerBuilder.PlayerInput.Disable();

        //HoleData newhole = new HoleData();
        //newhole.HolePar = 3;
        //newhole.CourseName = _selectedCourse.CourseName;
        //newhole.IsMiniGolf = _selectedCourse.IsMiniGolf;
        //_saveHandler.ClearAllTilesForNewHole();
        //newhole.HoleTileMapData = _saveHandler.GetAllTileMapData();

        // OLD
        //_saveHandler.ClearAllTilesForNewHole();
        //HoleData newhole = _saveHandler.CreateNewHole(_selectedCourse.CourseName, _selectedCourse.IsMiniGolf, 3, _selectedCourse.HolesInCourse.Count() + 1);
        //_selectedCourse.HolesInCourse.Add(newhole);
        //_saveHandler.SaveCourse(_selectedCourse, _selectedCourse.RelativeFilePath);

        //// this should be added to a different function similar to how Create New Course flow of CreateNewCourseButtonPressed > CreateNewCourse. Player will be prompted to enter in Hole details, which they could cancel
        //_currentHoleDropDown.AddOptions(new List<string> { newhole.HoleIndex.ToString() });
        //CurrentHoleDropDownValueChanged(_selectedCourse.HolesInCourse.Count());
        //_currentHoleDropDown.value = _currentHoleDropDown.options.Count - 1;
        // OLD

        _createNewHolePanel.SetActive(true);
        _createNewHoleButton.gameObject.SetActive(false);
        // making the drop downs not interactable to avoid player changing current course or current hole while creating a new hole?
        _currentCourseDropDown.interactable = false;
        _currentHoleDropDown.interactable = false;
        _mapMakerBuilder.PlayerInput.Disable();
    }
    public void CreateNewHole()
    {
        Debug.Log("CreateNewHole: ");

        if (string.IsNullOrEmpty(_newHoleParTextInput.text))
            return;

        if (_selectedCourse == null)
            return;

        int par = 0;
        int.TryParse(_newHoleParTextInput.text, out par);
        if (par <= 0)
            return;


        // create a new empty hole and save to the course
        _saveHandler.ClearAllTilesForNewHole();
        HoleData newhole = _saveHandler.CreateNewHole(_selectedCourse.CourseName, _selectedCourse.IsMiniGolf, par, _selectedCourse.HolesInCourse.Count() + 1);
        _selectedCourse.HolesInCourse.Add(newhole);
        _saveHandler.SaveCourse(_selectedCourse, _selectedCourse.RelativeFilePath);

        // add hole to the drop down menu and select the hole
        _currentHoleDropDown.AddOptions(new List<string> { newhole.HoleIndex.ToString() });
        CurrentHoleDropDownValueChanged(_selectedCourse.HolesInCourse.Count());
        _currentHoleDropDown.value = _currentHoleDropDown.options.Count - 1;

        // updating UI again
        _currentCourseDropDown.interactable = true;
        _currentHoleDropDown.interactable = true;
        _createNewHolePanel.SetActive(false);
        _createNewHoleButton.gameObject.SetActive(false);
        _mapMakerBuilder.PlayerInput.Enable();

    }
    public void CancelCreateNewHole()
    {
        Debug.Log("CancelCreateNewHole: ");

        _createNewHoleButton.gameObject.SetActive(true);
        _createNewHolePanel.SetActive(false);
        _currentCourseDropDown.interactable = true;
        _currentHoleDropDown.interactable = true;
        _mapMakerBuilder.PlayerInput.Enable();
    }
    void NewHoleSelectedChangedFunction(int holeNumber)
    {
        Debug.Log("NewHoleSelectedChangedFunction: " + holeNumber);
        if (holeNumber > _selectedCourse.HolesInCourse.Count)
        {
            Debug.Log("NewHoleSelectedChangedFunction: " + holeNumber + " is more than number of holes in the course: " + _selectedCourse.HolesInCourse.Count);
            return;
        }
        _selectedHoleNumber = holeNumber;
        if (holeNumber == 0)
        {
            _selectedHole = null;
            _createNewHoleButton.gameObject.SetActive(true);
            _saveHoleButton.gameObject.SetActive(false);
            _loadHoleButton.gameObject.SetActive(false);
        }
        else
        {
            _selectedHole = _selectedCourse.HolesInCourse.FirstOrDefault(x => x.HoleIndex == _selectedHoleNumber);
            _createNewHoleButton.gameObject.SetActive(false);
            _saveHoleButton.gameObject.SetActive(false);
            _loadHoleButton.gameObject.SetActive(true);
        }
    }
    void EnableSaveHoleButton(bool enable)
    {
        Debug.Log("EnableSaveHoleButton: " + enable);
        if (_selectedCourse == null || _selectedHole == null || _selectedHoleNumber <= 0)
        {
            _saveHoleButton.gameObject.SetActive(false);
            return;
        }
        _saveHoleButton.gameObject.SetActive(enable);
    }
    public void SaveHole()
    {
        if (_selectedCourse == null)
            return;
        if (_selectedHole == null)
            return;

        Debug.Log("SaveHole: Course: " + _selectedCourse.CourseName + " hole #: " + _selectedHole.HoleIndex);
        _selectedHole.HoleTileMapData = _saveHandler.GetAllTileMapData();
        _selectedHole.PolygonPoints = _saveHandler.GetBoundsOfAllTileMaps().ToArray();
        _selectedHole.ZoomOutPosition = _saveHandler.GetCenterOfHoleBounds(_selectedHole.PolygonPoints);

        // Save the tee off location if it was set in the builder
        if (_mapMakerBuilder.HasTeeOffLocationBeenPlaced && _mapMakerBuilder.AimPoints.Count > 0)
        {
            _selectedHole.TeeOffLocation = _mapMakerBuilder.TeeOffLocationPosition;
            _selectedHole.CourseAimPoints = _saveHandler.OrderAimPointsByDistanceToTeeOff(_mapMakerBuilder.TeeOffLocationPosition, _mapMakerBuilder.AimPoints).ToArray();
        }
        else
        {
            _selectedHole.TeeOffLocation = Vector3.zero;
            _selectedHole.CourseAimPoints = null;
        }
        _saveHandler.SaveCourse(_selectedCourse, _selectedCourse.RelativeFilePath);
    }
    public void LoadHole()
    {
        if (_selectedCourse == null)
            return;
        if (_selectedHole == null)
            return;

        Debug.Log("LoadHole: Course: " + _selectedCourse.CourseName + " hole #: " + _selectedHole.HoleIndex);

        _mapMakerHistory.ClearHistoryForNewHole();
        _saveHandler.LoadTileMapData(_selectedHole.HoleTileMapData);
    }
}
