using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.SceneManagement;

public class MapMakerBuilder : SingletonInstance<MapMakerBuilder>
{
    MapMakerGolfControls _playerInput;

    // Drawing mode
    [SerializeField] PlaceType _currentDrawingMode = PlaceType.Single;

    // Tile maps?
    [SerializeField] Tilemap _previewMap, _greenMap, _fairwayMap, _currentTileSelectedDisplayMap;
    [SerializeField] TileMapReferenceHolder _tileMapReferenceHolder;

    // Selected objects?
    [SerializeField] MapMakerGroundTileBase _selectedObject;
    [SerializeField] TileBase _selectedTileBase;
    [SerializeField] TileBase _currentSelectedTileIcon;

    // mouse position stuff?
    Vector2 _mousePos;
    Vector3Int _currentGridPosition;
    Vector3Int _previousGridPosition;

    // Camera stuff? UI stuff?
    Camera _camera;

    // mouse click stuff? When holding down the mouse
    [SerializeField] bool _holdActive;
    [SerializeField] Vector3Int _holdStartPosition;

    // Drawing rectangle stuff
    [SerializeField] BoundsInt _rectangleBounds;

    // Drawing a line stuff?
    [SerializeField] List<Vector2Int> _linePoints = new List<Vector2Int>();

    // Displaying Obstacles
    [SerializeField] GameObject _obstaclePreview;

    // Dictionary of place obstacles with their position being the key
    Dictionary<Vector3Int, GameObject> _placeObstaclesByPostion = new Dictionary<Vector3Int, GameObject>();
    Dictionary<Vector3Int, MapMakerGroundTileBase> _obstalceGroundTileBaseByPosition = new Dictionary<Vector3Int, MapMakerGroundTileBase>();
    [SerializeField] Transform _obstacleHolder;

    // History
    MapMakerHistory _mapMakerHistory;

    // Mapping of Tiles to MapMakerGroundTileBases. Originall in SaveHandler.cs
    Dictionary<string, Tilemap> _tileMaps = new Dictionary<string, Tilemap>();
    Dictionary<TileBase, MapMakerGroundTileBase> _tileBaseToMapMakerObject = new Dictionary<TileBase, MapMakerGroundTileBase>();
    Dictionary<string, TileBase> _guidToTileBase = new Dictionary<string, TileBase>();

    // Preview Handling Stuff?
    PreviewHandler _previewHandler = new PreviewHandler();

    //DrawModeHandler
    [SerializeField] DrawingModeHandler _drawModeHandler;
    [SerializeField] MapMakerGroundTileBase _eraser;
    [SerializeField] MinimizeMaximizeManager _minMaxManager;

    // MapMakerUIManager
    [SerializeField] MapMakerUIManager _mapMakerUIManager;

    // Course Markers - Tee Off Location
    [SerializeField] bool _hasTeeOffLocationBeenPlaced = false;
    [SerializeField] Vector3Int _teeOffLocationPosition;

    // Course Markers - Aim Point
    [SerializeField] bool _atLeastOneAimPoint = false;
    [SerializeField] List<Vector3Int> _aimPoints = new List<Vector3Int>();

    // Tee Markers (the blue balls)
    [SerializeField] bool _bothTeeMarkersPlaced = false;
    [SerializeField] List<Vector3Int> _teeOffMarkerPositions = new List<Vector3Int>();

    // Hole/Flag placement
    [SerializeField] bool _hasHoleBeenPlaced = false;
    [SerializeField] Vector3Int _holePosition = Vector3Int.zero;


    protected override void Awake()
    {
        base.Awake();
        _playerInput = new MapMakerGolfControls();
        _camera = Camera.main;
        _mapMakerHistory = MapMakerHistory.GetInstance();
    }

    private void OnEnable()
    {
        _playerInput.Enable();

        // mouse movement events
        _playerInput.MapMaker.MousePosition.performed += OnMouseMove;

        // left click events
        _playerInput.MapMaker.MouseLeftClick.performed += OnLeftClick;
        _playerInput.MapMaker.MouseLeftClick.started += OnLeftClick;
        _playerInput.MapMaker.MouseLeftClick.canceled += OnLeftClick;

        // right click events
        _playerInput.MapMaker.MouseRightClick.performed += OnRightClick;

        // Keyboard shortcuts
        _playerInput.MapMaker.EraserSelect.performed += EForEraserPressed;
        _playerInput.MapMaker.SingleDrawMode.performed += SForSingleModePressed;
        _playerInput.MapMaker.LineDrawMode.performed += LForLineModePressed;
        _playerInput.MapMaker.FillDrawMode.performed += FForFillModePressed;
        _playerInput.MapMaker.RectangleDrawMode.performed += RForRectangleModePressed;
        _playerInput.MapMaker.MaximizeWindow.performed += MaximizeShortCut;
        _playerInput.MapMaker.MinimizeWindow.performed += MinimizeShortCut;

    }
    private void OnDisable()
    {
        _playerInput.Disable();

        // mouse movement events
        _playerInput.MapMaker.MousePosition.performed -= OnMouseMove;

        // left click events
        _playerInput.MapMaker.MouseLeftClick.performed -= OnLeftClick;
        _playerInput.MapMaker.MouseLeftClick.started -= OnLeftClick;
        _playerInput.MapMaker.MouseLeftClick.canceled -= OnLeftClick;

        // right click events
        _playerInput.MapMaker.MouseRightClick.performed -= OnRightClick;

        // Keyboard shortcuts
        _playerInput.MapMaker.EraserSelect.performed -= EForEraserPressed;
        _playerInput.MapMaker.SingleDrawMode.performed -= SForSingleModePressed;
        _playerInput.MapMaker.LineDrawMode.performed -= LForLineModePressed;
        _playerInput.MapMaker.FillDrawMode.performed -= FForFillModePressed;
        _playerInput.MapMaker.RectangleDrawMode.performed -= RForRectangleModePressed;
        _playerInput.MapMaker.MaximizeWindow.performed -= MaximizeShortCut;
        _playerInput.MapMaker.MinimizeWindow.performed -= MinimizeShortCut;

        // Scene manager thing?
        SceneManager.activeSceneChanged -= ChangedActiveScene;
        
    }
    private void Start()
    {
        if (!_obstacleHolder)
            _obstacleHolder = GameObject.FindGameObjectWithTag("EnvironmentObstacleHolder").transform;

        InitTilemaps();
        InitTileReferences();
        if (_mapMakerUIManager == null)
        {
            _mapMakerUIManager = GameObject.FindGameObjectWithTag("MapMakerUIManager").GetComponent<MapMakerUIManager>();
        }
        SceneManager.activeSceneChanged += ChangedActiveScene;
    }
    private void ChangedActiveScene(Scene current, Scene next)
    {
        Debug.Log("MapMakerBuilder: ChangedActiveScene: " + current.name + " to " + next.name);
        if (next.name != "Golf-MapMaker")
        {
            Destroy(this);
        }
    }
    void InitTilemaps()
    {
        foreach (Tilemap map in _tileMapReferenceHolder.AllMaps)
        {
            _tileMaps.Add(map.name, map);
        }
    }
    void InitTileReferences()
    {
        MapMakerGroundTileBase[] mapMakerTiles = Resources.LoadAll<MapMakerGroundTileBase>("MapMakerGolf/GroundTileScriptables");
        foreach (MapMakerGroundTileBase tile in mapMakerTiles)
        {
            if (_tileBaseToMapMakerObject.ContainsKey(tile.TileBase))
            {
                Debug.LogError("InitTileReferences: Tilebase: " + tile.TileBase.name + " is already in use by: " + _tileBaseToMapMakerObject[tile.TileBase].name);
                continue;
            }
            Debug.Log("InitTileReferences: tile Guid: " + tile.Guid);
            _tileBaseToMapMakerObject.Add(tile.TileBase, tile);
            _guidToTileBase.Add(tile.Guid, tile.TileBase);

        }
    }
    public MapMakerGolfControls PlayerInput
    {
        get
        {
            return _playerInput;
        }
    }
    public Dictionary<string, Tilemap> GetTileMapNameToTileMapMapping()
    {
        return _tileMaps;
    }
    public bool DoesTileMapExistInMapping(string tileMapName)
    {
        return _tileMaps.ContainsKey(tileMapName);
    }
    public Tilemap GetTileMapFromTileMapName(string tileMapName)
    {
        if (string.IsNullOrEmpty(tileMapName))
            return null;
        if (!_tileMaps.ContainsKey(tileMapName))
            return null;
        return _tileMaps[tileMapName];
    }
    public Dictionary<TileBase, MapMakerGroundTileBase> GetTileBaseToMapMakerGroundTileBaseMapping()
    {
        return _tileBaseToMapMakerObject;
    }
    public bool DoesMapMakerGroundTileBaseExistForTileBaseInMapping(TileBase tileBase)
    {
        return _tileBaseToMapMakerObject[tileBase];
    }
    public MapMakerGroundTileBase GetMapMakerGroundTileBaseFromTileBase(TileBase tileBase)
    {
        if (tileBase == null)
            return null;

        if (!_tileBaseToMapMakerObject.ContainsKey(tileBase))
            return null;

        return _tileBaseToMapMakerObject[tileBase];
    }
    public Dictionary<string, TileBase> GetGUIDToTileBaseMapping()
    {
        return _guidToTileBase;
    }
    public bool DoesGUIDExistForTileBaseInMapping(string guid)
    {
        return _guidToTileBase.ContainsKey(guid);
    }
    public TileBase GetTileBaseFromGUID(string guid)
    {
        if (string.IsNullOrEmpty(guid))
            return null;
        if (!_guidToTileBase.ContainsKey(guid))
            return null;

        return _guidToTileBase[guid];
    }
    public bool HasTeeOffLocationBeenPlaced
    {
        get {
            return _hasTeeOffLocationBeenPlaced;
        }
    }
    public Vector3Int TeeOffLocationPosition
    {
        get { 
            return _teeOffLocationPosition;
        }
    }

    public List<Vector3Int> AimPoints
    {
        get {
            return _aimPoints;
        }
    }
    public bool HaveBothTeeOffMarkersBeenPlaced
    {
        get{
            return _bothTeeMarkersPlaced;
        }
    }
    public List<Vector3Int> TeeOffMarkerPositions
    {
        get {
            return  _teeOffMarkerPositions;
        }
    }
    public bool HasHoleBeenPlaced
    {
        get {
            return _hasHoleBeenPlaced;
        }
    }
    public Vector3Int HolePositon
    {
        get {
            return _holePosition;
        }
    }

    private void Update()
    {
        // everything below this was previously in Update!!!
        //if (!_selectedObject)
        //    return;
        //Vector3 pos = _camera.ScreenToWorldPoint(_mousePos);
        //Vector3Int gridPos = _previewMap.WorldToCell(pos);

        //if (gridPos != _currentGridPosition)
        //{
        //    _previousGridPosition = _currentGridPosition;
        //    _currentGridPosition = gridPos;
        //    // update tile map preview?
        //    UpdatePreview();

        //    if (_holdActive)
        //    {
        //        Debug.Log("UpdateGridPosition: calling HandleDrawing with _holdActive as true");
        //        HandleDrawing();
        //    }
        //    // maybe move to OnMouseMove instead of constantly calling it in update?
        //}
    }
    void UpdateGridPosition(Vector3 currentPos)
    {
        Vector3 pos = _camera.ScreenToWorldPoint(_mousePos);
        //Vector3Int gridPos = _previewMap.WorldToCell(pos);
        Vector3Int gridPos = _tilemap.WorldToCell(pos);
        //Debug.Log("UpdateGridPosition: gridPos: " + gridPos.ToString() + " _currentGridPosition" + _currentGridPosition.ToString());

        if (gridPos != _currentGridPosition)
        {
            _previousGridPosition = _currentGridPosition;
            _currentGridPosition = gridPos;
            //Debug.Log("UpdateGridPosition: gridPos: " + gridPos.ToString() + " _currentGridPosition" + _currentGridPosition.ToString() + " previous position: " + _previousGridPosition.ToString());

            if (_selectedObject != null)
            {
                // update tile map preview?
                //UpdatePreview();
                // maybe move to OnMouseMove instead of constantly calling it in update?

                if (_holdActive)
                {
                    //Debug.Log("UpdateGridPosition: calling HandleDrawing with _holdActive as true");
                    HandleDrawing();
                }
                else
                {
                    UpdatePreview();
                }
            }
            

            UpdateCurrentSelectedTileIcon(_previousGridPosition, _currentGridPosition);
        }
    }
    void UpdateCurrentSelectedTileIcon(Vector3Int prevPos, Vector3Int currentPos)
    {
        _currentTileSelectedDisplayMap.SetTile(prevPos, null);
        _currentTileSelectedDisplayMap.SetTile(currentPos, _currentSelectedTileIcon);
    }
    private void OnMouseMove(InputAction.CallbackContext ctx)
    {
        _mousePos = ctx.ReadValue<Vector2>();

        //if (!_selectedObject)
        //    return;
        
        UpdateGridPosition(_mousePos);
    }
    private void OnLeftClick(InputAction.CallbackContext ctx)
    {
        //Debug.Log(ctx.interaction + " / " + ctx.phase);
        //if (!_selectedObject)
        //    return;

        //if (EventSystem.current.IsPointerOverGameObject())
        //{
        //    _previewHandler.ResetPreview();
        //    _holdActive = false;
        //    return;
        //}

        if (_selectedObject != null && !EventSystem.current.IsPointerOverGameObject())
        {
            if (ctx.phase == InputActionPhase.Started)
            {
                _holdActive = true;

                if (ctx.interaction is TapInteraction)
                {
                    _holdStartPosition = _currentGridPosition;
                }

                HandleDrawing();
            }
            else
            {
                // performed or cancelled
                if (ctx.interaction is SlowTapInteraction || ctx.interaction is TapInteraction && ctx.phase == InputActionPhase.Performed)
                {
                    _holdActive = false;
                    // Draw on release?
                    HandleDrawRelease();
                }
                else if (ctx.interaction is TapInteraction && ctx.phase == InputActionPhase.Performed)
                {
                    HandleDrawRelease();
                }

            }
        }
        else
        {
            _previewHandler.ResetPreview();
            _holdActive = false;
        }


        //if (ctx.phase == InputActionPhase.Started)
        //{
        //    _holdActive = true;

        //    if (ctx.interaction is TapInteraction)
        //    {
        //        _holdStartPosition = _currentGridPosition;
        //    }

        //    HandleDrawing();
        //}
        //else
        //{
        //    // performed or cancelled
        //    if (ctx.interaction is SlowTapInteraction || ctx.interaction is TapInteraction && ctx.phase == InputActionPhase.Performed)
        //    {
        //        _holdActive = false;
        //        // Draw on release?
        //        HandleDrawRelease();
        //    }
        //    else if (ctx.interaction is TapInteraction && ctx.phase == InputActionPhase.Performed)
        //    {
        //        HandleDrawRelease();
        //    }

        //}

        //HandleDrawing();
    }
    private void OnRightClick(InputAction.CallbackContext ctx)
    {
        //_previewHandler.ResetPreview();
        //// Unselect any tiles when right clicking?
        //EnableObjectTileMap(false);
        //SelectedObject = null;
        //_holdActive = false;
        //if (_obstaclePreview != null)
        //{
        //    Destroy(_obstaclePreview);
        //    _obstaclePreview = null;
        //}
        //if (_drawModeHandler != null)
        //    _drawModeHandler.ResetToSingle();
        ResetForRightClickAndOthers();
    }
    void ResetForRightClickAndOthers()
    {
        _previewHandler.ResetPreview();
        // Unselect any tiles when right clicking?
        EnableObjectTileMap(false);
        SelectedObject = null;
        _holdActive = false;
        if (_obstaclePreview != null)
        {
            Destroy(_obstaclePreview);
            _obstaclePreview = null;
        }
        if (_drawModeHandler != null)
            _drawModeHandler.ResetToSingle();
    }
    private void EForEraserPressed(InputAction.CallbackContext ctx)
    {
        ResetForRightClickAndOthers();
        ObjectSelected(_eraser);
    }
    private void SForSingleModePressed(InputAction.CallbackContext ctx)
    {
        if (_drawModeHandler != null)
            _drawModeHandler.ButtonClicked(PlaceType.Single);
    }
    private void LForLineModePressed(InputAction.CallbackContext ctx)
    {
        if (_drawModeHandler != null)
            _drawModeHandler.ButtonClicked(PlaceType.Line);
    }
    private void FForFillModePressed(InputAction.CallbackContext ctx)
    {
        if (_drawModeHandler != null)
            _drawModeHandler.ButtonClicked(PlaceType.FloodFill);
    }
    private void RForRectangleModePressed(InputAction.CallbackContext ctx)
    {
        if (_drawModeHandler != null)
            _drawModeHandler.ButtonClicked(PlaceType.Rectangle);
    }
    public void SetMinMaxManager(MinimizeMaximizeManager minMax)
    {
        this._minMaxManager = minMax;
    }
    private void MaximizeShortCut(InputAction.CallbackContext ctx)
    {
        if (_minMaxManager != null)
            _minMaxManager.MaximizeShortCut();
    }
    private void MinimizeShortCut(InputAction.CallbackContext ctx)
    {
        if (_minMaxManager != null)
            _minMaxManager.MinimizeShortCut();
    }
    private MapMakerGroundTileBase SelectedObject
    {
        set {
            if (_selectedObject != value)
            {
                if (_drawModeHandler != null)
                    _drawModeHandler.ResetToSingle();
            }
            _selectedObject = value;

            // if selected object is not null, set the selected tile base. If it is null, set selected tile base to null
            _selectedTileBase = _selectedObject != null ? _selectedObject.TileBase : null;

            //if (_selectedObject)
            //{
            //    UpdateGridPosition(_mousePos);
            //    //_previewMap.GetComponent<TilemapRenderer>().sortingOrder = _selectedObject.MapMakerTileType.SortingOrder;
            //}
            //else
            //{
            //    //_previewMap.GetComponent<TilemapRenderer>().sortingOrder = 0;
            //}

            UpdateGridPosition(_mousePos);

            // Update the preview?
            _previewHandler.UpdateTile(_tilemap, _selectedTileBase);
            UpdatePreview();
            if (_mapMakerUIManager != null)
                _mapMakerUIManager.PlayerSelectedTileObject(_selectedObject);
        }
    }
    public void ObjectSelected(MapMakerGroundTileBase obj)
    {
        SelectedObject = obj;
        if (obj.GetType() == typeof(MapMakerObstacle))
        {
            if (_obstaclePreview != null)
            {
                Destroy(_obstaclePreview);
                _obstaclePreview = null;
            }

            MapMakerObstacle obstacle = (MapMakerObstacle)obj;
            //_obstaclePreview = Instantiate(obstacle.ScriptableObstacle.ObstaclePrefab, _currentGridPosition, Quaternion.identity);
            _obstaclePreview = Instantiate(GetObstaclePrefabToSpawn(obstacle), _currentGridPosition, Quaternion.identity);
        }
        else
        {
            if (_obstaclePreview != null)
            {
                Destroy(_obstaclePreview);
                _obstaclePreview = null;
            }
        }
    }
    GameObject GetObstaclePrefabToSpawn(MapMakerObstacle obstacle)
    {
        if (obstacle == null)
            return null;
        if (obstacle.ScriptableObstacle.MapMakerPrefab != null)
            return obstacle.ScriptableObstacle.MapMakerPrefab;

        return obstacle.ScriptableObstacle.ObstaclePrefab;
    }
    public void SetCurrentDrawingMode(PlaceType newDrawingMode)
    {
        Debug.Log("SetCurrentDrawingMode: current mode: " + _currentDrawingMode.ToString() + " new mode: " + newDrawingMode.ToString());
        if (_currentDrawingMode != newDrawingMode)
        {
            if (_selectedObject != null)
            {
                if (_selectedObject.GetType() == typeof(MapMakerCourseMarker))
                {
                    _drawModeHandler.ResetToSingle();
                    return;
                }
            }
            _currentDrawingMode = newDrawingMode;
        }
    }
    private Tilemap _tilemap
    {
        get {
            if (_selectedObject != null && _selectedObject.MapMakerTileType != null && _selectedObject.MapMakerTileType.Tilemap != null)
            {
                return _selectedObject.MapMakerTileType.Tilemap;
            }
            //return _greenMap;
            return _previewMap;
        }
    }
    void UpdatePreview()
    {
        //Debug.Log("UpdatePreview: current position: " + _currentGridPosition.ToString() + " previous position: " + _previousGridPosition.ToString());
        // Remove old tile from preview if exisiting
        //_previewMap.SetTile(_previousGridPosition, null);
        _previewHandler.ResetPreview(_previousGridPosition);


        //if (_selectedObject != null && _selectedObject.GetType() != typeof(MapMakerTool))
        if (_selectedObject != null && _selectedObject.GetType() == typeof(MapMakerObstacle))
        {
            //Debug.Log("UpdatePreview: IsPlacementForbidden: " + IsPlacementForbidden(_currentGridPosition));
            if (IsPlacementForbidden(_currentGridPosition))
            {

                if (_obstaclePreview != null)
                {
                    Destroy(_obstaclePreview);
                    _obstaclePreview = null;
                }
                    
            }                
            //else if (_selectedObject.GetType() == typeof(MapMakerObstacle))
            else
            {
                if (_obstaclePreview == null)
                {
                    MapMakerObstacle obstacle = (MapMakerObstacle)_selectedObject;
                    //_obstaclePreview = Instantiate(obstacle.ScriptableObstacle.ObstaclePrefab, _currentGridPosition, Quaternion.identity);
                    _obstaclePreview = Instantiate(GetObstaclePrefabToSpawn(obstacle), _currentGridPosition, Quaternion.identity);
                }
                _obstaclePreview.transform.position = _currentGridPosition;
            }
        }

        //Set Current tile
        //_previewMap.SetTile(_currentGridPosition, _selectedTileBase);
        //if (_selectedObject != null)
        //    _previewHandler.SetPreview(_currentGridPosition, IsPlacementForbidden(_currentGridPosition));
        _previewHandler.SetPreview(_currentGridPosition, IsPlacementForbidden(_currentGridPosition));

        // update obstacle preview

    }
    bool IsPlacementForbidden(Vector3Int position)
    {
        if (_selectedObject == null)
            return false;


        if (_selectedObject.CanOnlyBePlacedOnTheseGroundTypes.Count > 0)
        {
            List<MapMakerTileTypes> allowedCategories = _selectedObject.CanOnlyBePlacedOnTheseGroundTypes;
            List<Tilemap> allowedMaps = allowedCategories.ConvertAll(category => category.Tilemap);
            if (allowedMaps.Count > 0)
            {
                //return !DoesTilemapMatchCanOnlyBePlacedOnTheseGroundTypes(allowedMaps, position);
                if (!DoesTilemapMatchCanOnlyBePlacedOnTheseGroundTypes(allowedMaps, position))
                {
                    return true;
                }
            }
        }

        //if (_selectedObject.GetType() == typeof(MapMakerCourseMarker))
        //{
        //    MapMakerCourseMarker marker = (MapMakerCourseMarker)_selectedObject;
        //    if (marker.CourseMarkerType != CourseMarkerType.TeeOffLocation)
        //        return IsThereATeeOffMarkerAtThisPoint(_selectedObject.MapMakerTileType.Tilemap, position);
        //}

        List<MapMakerTileTypes> restrictedCategories = _selectedObject.PlacementRestrictions;
        List<Tilemap> restrictedMaps = restrictedCategories.ConvertAll(category => category.Tilemap);
        //Debug.Log("IsPlacementForbidden: length of restrictedMaps maps: " + restrictedMaps.Count() + " length of restrictedCategories: " + restrictedCategories.Count());


        List <Tilemap> allMaps = _tileMapReferenceHolder.ForbiddenPlacingWithMaps.Concat(restrictedMaps).ToList();
        //Debug.Log("IsPlacementForbidden: length of all maps: " + allMaps.Count());
        return allMaps.Any(map => {
            if (map.HasTile(position))
            {
                return map.GetTile(position) != _selectedTileBase;
            }

            return false;
        });
    }
    bool DoesTilemapMatchCanOnlyBePlacedOnTheseGroundTypes(List<Tilemap> allowedMaps, Vector3Int position)
    {
        return allowedMaps.Any(map => {
            if (map.HasTile(position))
            {
                return map.GetTile(position) != _selectedTileBase;
            }

            return false;
        });
    }
    void HandleDrawing()
    {
        if (!_selectedObject)
            return;

        //switch (_selectedObject.PlaceType)
        switch (this._currentDrawingMode)
        {
            case PlaceType.Single:
            default:
                //DrawItem();
                DrawItem(_tilemap, _currentGridPosition, _selectedTileBase);
                break;
            case PlaceType.Line:
                //LineRenderer();
                //LineRendererTwoElectricBoogaloo();
                _linePoints.Clear();
                _linePoints = DrawRenderer.LineRenderer(_holdStartPosition, _currentGridPosition);
                DrawLine(_tilemap, true);
                break;
            case PlaceType.Rectangle:
                //RectangleRenderer();
                _rectangleBounds = DrawRenderer.RectangleRenderer(_holdStartPosition, _currentGridPosition);
                DrawBounds(_tilemap, true);
                break;
            case PlaceType.FloodFill:
                _previewHandler.ResetPreview();
                FloodFill();
                break;
        }
        EnableObjectTileMap(true);
    }
    void HandleDrawRelease()
    {
        if (!_selectedObject)
            return;

        //switch (_selectedObject.PlaceType)
        switch (this._currentDrawingMode)
        {
            case PlaceType.Line:
                _previewHandler.ResetPreview();
                SaveLineDrawing();
                //_previewMap.ClearAllTiles();                
                break;
            case PlaceType.Rectangle:
                _previewHandler.ResetPreview();
                SaveBounds();
                //_previewMap.ClearAllTiles();                
                break;
            //case PlaceType.FloodFill:
            //    _previewHandler.ResetPreview();
            //    FloodFill();
            //    break;
        }
        EnableObjectTileMap(false);
    }
    void EnableObjectTileMap(bool enable)
    {
        if (_selectedObject == null)
            return;
        if (_selectedObject.GroundTileType == GroundTileType.Object)
        {
            Color newColor = _tilemap.color;
            if (enable)
                newColor.a = 1f;
            else
                newColor.a = 0;
            _tilemap.color = newColor;
        }

    }
    // OLD DRAW ITEM BEFORE MAKING HISTORY STEPS TO INCLUDE RECTANGLE/LINE AS A SINGLE STEP
    //void DrawItem(Tilemap map, Vector3Int position, TileBase tileBase)
    //{
    //    // TODO: automatically select the correct tilemap
    //    //if (!_selectedObject)
    //    //    return;

    //    // OLD BEGIN
    //    //if (_selectedObject.GroundTileType == GroundTileType.Green)
    //    //{
    //    //    _greenMap.SetTile(_currentGridPosition, _selectedTileBase);
    //    //}
    //    //else if (_selectedObject.GroundTileType == GroundTileType.Fairway)
    //    //{
    //    //    _fairwayMap.SetTile(_currentGridPosition, _selectedTileBase);
    //    //}
    //    // OLD END

    //    //_tilemap.SetTile(_currentGridPosition, _selectedTileBase);

    //    if (map != _previewMap && _selectedObject.GetType() == typeof(MapMakerTool))
    //    {
    //        MapMakerTool tool = (MapMakerTool)_selectedObject;
    //        tool.Use(position);
    //    }
    //    else
    //    {
    //        if (_selectedObject.GetType() == typeof(MapMakerTool))
    //        {
    //            map.SetTile(position, tileBase);
    //        }
    //        else if (!IsPlacementForbidden(position))
    //        {

    //            // Check to see if an obstacle should be spawned here or not
    //            //if (map != _previewMap && _selectedObject.GetType() == typeof(MapMakerObstacle))
    //            //{
    //            //    PlaceObstacle(position, (MapMakerObstacle)_selectedObject);
    //            //}
    //            if (map != _previewMap)
    //            {
    //                // Add an object to map maker history for undo/redo
    //                _mapMakerHistory.Add(new MapMakerHistoryItem(map, map.GetTile(position), tileBase, position, _selectedObject));
    //                if (_selectedObject.GetType() == typeof(MapMakerObstacle))
    //                {
    //                    Debug.Log("DrawItem: Placing obstacle at: " + position.ToString());
    //                    PlaceObstacle(position, (MapMakerObstacle)_selectedObject);
    //                }                    
    //            }

    //            map.SetTile(position, tileBase);
    //        }

    //    }
    //}
    // OLD DRAW ITEM BEFORE MAKING HISTORY STEPS TO INCLUDE RECTANGLE/LINE AS A SINGLE STEP
    void DrawItem(Tilemap map, Vector3Int position, TileBase tileBase)
    {
        Vector3Int[] positions = new Vector3Int[] { position };
        DrawItem(map, positions, tileBase);
    }
    void DrawItem(Tilemap map, Vector3Int[] positions, TileBase tileBase)
    {

        //if (map != _previewMap && _selectedObject.GetType() == typeof(MapMakerTool))
        if (_selectedObject.GetType() == typeof(MapMakerTool))
        {
            // new
            _previewHandler.ResetPreview();

            MapMakerTool tool = (MapMakerTool)_selectedObject;
            Tilemap currentSelectedTileMap = null;
            if (_mapMakerUIManager != null)
                currentSelectedTileMap = _mapMakerUIManager.GetCurrentSelectedTileMap();

            //tool.Use(positions, out MapMakerHistoryStep historyStep);
            tool.Use(positions, currentSelectedTileMap, out MapMakerHistoryStep historyStep);

            if (historyStep != null)
            {
                _mapMakerHistory.Add(historyStep);
            }
        } // should have some logic where if the map you are placing on is a "restricted" tilemap, like water or sand, you should delete any non-restricted tile types. Have it call the eraser and create the eraser history step, then draw the water/sand?
        else
        {
            // Create arrays required for the History Steps
            TileBase[] previousTiles = new TileBase[positions.Length];
            TileBase[] newTiles = new TileBase[positions.Length];
            //MapMakerGroundTileBase[] mapMakerTileBases = new MapMakerGroundTileBase[positions.Length];
            MapMakerGroundTileBase[] prevMapMakerTileBases = new MapMakerGroundTileBase[positions.Length];
            MapMakerGroundTileBase[] newMapMakerTileBases = new MapMakerGroundTileBase[positions.Length];

            for (int i = 0; i < positions.Length; i++)
            {
                //previousTiles[i] = map.GetTile(positions[i]);
                previousTiles[i] = _previewHandler.GetPreviousTile(positions[i]);
                //mapMakerTileBases[i] = _selectedObject;
                prevMapMakerTileBases[i] = GetMapMakerGroundTileBaseFromTileBase(previousTiles[i]);
                newMapMakerTileBases[i] = _selectedObject;

                if (_selectedObject.GetType() == typeof(MapMakerTool))
                {
                    map.SetTile(positions[i], tileBase);
                }
                else if (!IsPlacementForbidden(positions[i]))
                {
                    newTiles[i] = tileBase;
                    map.SetTile(positions[i], tileBase);

                    //if (_selectedObject.GetType() == typeof(MapMakerObstacle) && map != _previewMap)
                    if (_selectedObject.GetType() == typeof(MapMakerObstacle))
                    {
                        Debug.Log("DrawItem: Placing obstacle at: " + positions[i].ToString());
                        PlaceObstacle(positions[i], (MapMakerObstacle)_selectedObject);
                    }
                    else if (_selectedObject.GetType() == typeof(MapMakerCourseMarker))
                    {
                        Debug.Log("DrawItem: Placing MapMakerCourseMarker at: " + positions[i].ToString());
                        PlaceCourseMarker(positions[i], (MapMakerCourseMarker)_selectedObject);
                    }
                }
                else
                {
                    newTiles[i] = previousTiles[i];
                }
            }

            //if (map != _previewMap)
            //{
            //    //_mapMakerHistory.Add(new MapMakerHistoryStep(map, previousTiles, newTiles, positions, mapMakerTileBases));
            //    _mapMakerHistory.Add(new MapMakerHistoryStep(map, previousTiles, newTiles, positions, prevMapMakerTileBases, newMapMakerTileBases));
            //}
            _mapMakerHistory.Add(new MapMakerHistoryStep(map, previousTiles, newTiles, positions, prevMapMakerTileBases, newMapMakerTileBases));

            //if (_selectedObject.GetType() == typeof(MapMakerTool))
            //{
            //    map.SetTile(position, tileBase);
            //}
            //else if (!IsPlacementForbidden(position))
            //{

            //    if (map != _previewMap)
            //    {
            //        // Add an object to map maker history for undo/redo
            //        _mapMakerHistory.Add(new MapMakerHistoryItem(map, map.GetTile(position), tileBase, position, _selectedObject));
            //if (_selectedObject.GetType() == typeof(MapMakerObstacle))
            //{
            //    Debug.Log("DrawItem: Placing obstacle at: " + position.ToString());
            //    PlaceObstacle(position, (MapMakerObstacle)_selectedObject);
            //}
            //    }

            //    map.SetTile(position, tileBase);
            //}

            _previewHandler.ClearPreview();
        }

        
        _previewHandler.SetPreview(_currentGridPosition, IsPlacementForbidden(_currentGridPosition));
    }

    public void PlaceObstacle(Vector3Int position, MapMakerObstacle obstacle, bool checkIsForbidden = false)
    {
        if (_placeObstaclesByPostion.ContainsKey(position))
        {
            Debug.Log("PlaceObstacle: Object has already been placeed at this position: " + position + " : " + _placeObstaclesByPostion[position].name);
            return;
        }

        if (checkIsForbidden)
        {
            if (IsPlacementForbidden(position))
                return;
        }

        //MapMakerObstacle obj = (MapMakerObstacle)_selectedObject;
        //GameObject gameObject = Instantiate(obj.ScriptableObstacle.ObstaclePrefab, position, Quaternion.identity);

        //GameObject gameObject = Instantiate(obstacle.ScriptableObstacle.ObstaclePrefab, position, Quaternion.identity);
        GameObject gameObject = Instantiate(GetObstaclePrefabToSpawn(obstacle), position, Quaternion.identity);
        _placeObstaclesByPostion.Add(position, gameObject);
        gameObject.transform.SetParent(_obstacleHolder);

        // Save the MapMakerGroundTileType of the obstacle to save for History Steps and stuff
        _obstalceGroundTileBaseByPosition.Add(position, obstacle);

        // Check if the obstacle is a hole or not that needs to have its position saved?
        CheckIfPlacingHoleFlag(position, obstacle);

        Debug.Log("PlaceObstacle: NEW OBJECT has been placeed at this position: " + position + " : " + _placeObstaclesByPostion[position].name);
    }
    public void RemoveObstacle(Vector3Int position, bool removeFromDict = true)
    {
        if (!_placeObstaclesByPostion.ContainsKey(position))
        {
            Debug.Log("RemoveObstacle: No obstacle found at: " + position);
            return;
        }

        Debug.Log("RemoveObstacle: Removing obstacle: " + _placeObstaclesByPostion[position].name + " found at position: " + position);
        GameObject objToRemove = _placeObstaclesByPostion[position];
        Destroy(objToRemove);

        if (removeFromDict)
        {
            CheckIfRemovingHoleFlag(position);
            _placeObstaclesByPostion.Remove(position);
            _obstalceGroundTileBaseByPosition.Remove(position);
        }

            
    }

    public void ClearAllObstacles()
    {
        Debug.Log("ClearAllObstacles: ");
        if (_placeObstaclesByPostion.Count > 0)
        {
            foreach (KeyValuePair<Vector3Int, GameObject> _obstacles in _placeObstaclesByPostion)
            {
                RemoveObstacle(_obstacles.Key, false);
            }
            _placeObstaclesByPostion.Clear();
            _obstalceGroundTileBaseByPosition.Clear();
        }

        // Reset hole stuff?
        _hasHoleBeenPlaced = false;
        _holePosition = Vector3Int.zero;
    }
    void CheckIfPlacingHoleFlag(Vector3Int position, MapMakerObstacle obstacle)
    {
        if (obstacle == null)
            return;
        if (obstacle.ObstacleType != ObstacleType.Hole)
            return;

        Debug.Log("CheckIfPlacingHoleFlag: for position: " + position + " has hole already been placed on this hole?: " + _hasHoleBeenPlaced);
        if (_hasHoleBeenPlaced)
        {
            Debug.Log("CheckIfPlacingHoleFlag: _hasHoleBeenPlaced is: " + _hasHoleBeenPlaced + " removing hole at postion: " + _holePosition);
            GameObject holeToRemove = _placeObstaclesByPostion[_holePosition];
            if (holeToRemove != null)
            {
                MapMakerGroundTileBase holeToRemoveTileBase = _obstalceGroundTileBaseByPosition[_holePosition];
                if (holeToRemoveTileBase != null)
                {
                    if (holeToRemoveTileBase.GetType() == typeof(MapMakerObstacle))
                    {
                        MapMakerObstacle holeToRemoveObstacle = (MapMakerObstacle)holeToRemoveTileBase;
                        if (holeToRemoveObstacle.ObstacleType == ObstacleType.Hole)
                        {
                            Debug.Log("CheckIfPlacingHoleFlag: a hole is at position: " + position + ". Removing the old hole information for the new hole?");
                            
                            CreateHistoryEntryWhenRemovingRestrictedObjectOrTile(_holePosition, (MapMakerGroundTileBase)holeToRemoveObstacle);

                            // Remove the old hole obstacle
                            RemoveObstacle(_holePosition);

                        }
                    }
                }
            }
        }

        _hasHoleBeenPlaced = true;
        _holePosition = position;

    }
    void CheckIfRemovingHoleFlag(Vector3Int position)
    {
        if (!_hasHoleBeenPlaced)
            return;
        if (!_obstalceGroundTileBaseByPosition.ContainsKey(position))
            return;

        Debug.Log("CheckIfRemovingHoleFlag: for position: " + position + " has hole already been placed on this hole?: " + _hasHoleBeenPlaced);
        MapMakerGroundTileBase holeToRemoveTileBase = _obstalceGroundTileBaseByPosition[position];
        if (holeToRemoveTileBase != null)
        {
            if (holeToRemoveTileBase.GetType() == typeof(MapMakerObstacle))
            {
                MapMakerObstacle holeToRemoveObstacle = (MapMakerObstacle)holeToRemoveTileBase;
                if (holeToRemoveObstacle.ObstacleType == ObstacleType.Hole)
                {
                    Debug.Log("CheckIfRemovingHoleFlag: a hole is at position: " + position + ". Resetting hole placement information");
                    _hasHoleBeenPlaced = false;
                    _holePosition = Vector3Int.zero;
                }
            }
        }

    }
    public void PlaceCourseMarker(Vector3Int position, MapMakerCourseMarker courseMarker, bool checkIsForbidden = false)
    {
        if (courseMarker == null)
            return;

        switch (courseMarker.CourseMarkerType)
        {
            case CourseMarkerType.TeeOffLocation:
                PlaceTeeOffLocation(position, courseMarker);
                break;
            case CourseMarkerType.AimPoint:
                PlaceAimPoint(position, courseMarker);
                break;
            case CourseMarkerType.TeeOffMarker:
                PlaceTeeOffMarkerBall(position, courseMarker);
                break;
        }
    }
    public void RemoveCourseMarker(Vector3Int position, MapMakerCourseMarker courseMarker)
    {
        if (courseMarker == null)
            return;

        switch (courseMarker.CourseMarkerType)
        {
            case CourseMarkerType.TeeOffLocation:
                RemoveTeeOffLocation(position, courseMarker);
                break;
            case CourseMarkerType.AimPoint:
                RemoveAimPoint(position, courseMarker);
                break;
            case CourseMarkerType.TeeOffMarker:
                RemoveTeeOffMarkerBall(position, courseMarker);
                break;
        }
    }
    public void ClearAllMarkers()
    {
        // Tee off location
        _hasTeeOffLocationBeenPlaced = false;
        _teeOffLocationPosition = Vector3Int.zero;

        // Aim points
        _atLeastOneAimPoint = false;
        _aimPoints.Clear();

        // Tee Off Marker balls
        _bothTeeMarkersPlaced = false;
        _teeOffMarkerPositions.Clear();
    }
    void PlaceTeeOffLocation(Vector3Int position, MapMakerCourseMarker courseMarker)
    {
        if (courseMarker == null)
            return;
        Debug.Log("PlaceTeeOffLocation: " + position.ToString());

        // Remove any previously exisiting tilemap markers?
        if (_hasTeeOffLocationBeenPlaced)
        {
            //courseMarker.MapMakerTileType.Tilemap.SetTile(_teeOffLocationPosition, null);
            //_hasTeeOffLocationBeenPlaced = false;

            //// Add a "deletion" to the history?
            //// Create arrays required for the History Steps
            //List<TileBase> previousTiles = new List<TileBase>{courseMarker.TileBase};
            //TileBase[] newTiles = new List<TileBase> { null }.ToArray();
            //Vector3Int[] positions = new List<Vector3Int> { _teeOffLocationPosition }.ToArray();
            //MapMakerGroundTileBase[] prevMapMakerTileBases = new List<MapMakerGroundTileBase> { (MapMakerGroundTileBase)courseMarker }.ToArray();
            //MapMakerGroundTileBase[] newMapMakerTileBases = new List<MapMakerGroundTileBase> { null }.ToArray();

            //_mapMakerHistory.Add(new MapMakerHistoryStep(courseMarker.MapMakerTileType.Tilemap, previousTiles.ToArray(), newTiles, positions, prevMapMakerTileBases, newMapMakerTileBases));

            CreateHistoryEntryWhenRemovingRestrictedObjectOrTile(_teeOffLocationPosition, (MapMakerGroundTileBase)courseMarker);
            _hasTeeOffLocationBeenPlaced = false;
        }

        _teeOffLocationPosition = position;
        _hasTeeOffLocationBeenPlaced = true;
    }
    void RemoveTeeOffLocation(Vector3Int position, MapMakerCourseMarker courseMarker)
    {
        if (!_hasTeeOffLocationBeenPlaced)
            return;
        if (_teeOffLocationPosition != position)
            return;
        _hasTeeOffLocationBeenPlaced = false;
        _teeOffLocationPosition = Vector3Int.zero;
    }
    void PlaceAimPoint(Vector3Int position, MapMakerCourseMarker courseMarker)
    {
        if (courseMarker == null)
            return;

        Debug.Log("PlaceAimPoint: " + position + " there are already: " + _aimPoints.Count + " aim points on map");

        // If there are already 4 aim points, remove the "oldest" aimpoint?
        if (_aimPoints.Count >= 4 && !_aimPoints.Contains(position))
        {
            Vector3Int oldPoint = _aimPoints[0];
            if (!IsThereATeeOffMarkerAtThisPoint(courseMarker.MapMakerTileType.Tilemap, oldPoint))
            {
                //courseMarker.MapMakerTileType.Tilemap.SetTile(oldPoint, null);

                //// Add a "deletion" to the history?
                //// Create arrays required for the History Steps
                //List<TileBase> previousTiles = new List<TileBase> { courseMarker.TileBase };
                //TileBase[] newTiles = new List<TileBase> { null }.ToArray();
                //Vector3Int[] positions = new List<Vector3Int> { oldPoint }.ToArray();
                //MapMakerGroundTileBase[] prevMapMakerTileBases = new List<MapMakerGroundTileBase> { (MapMakerGroundTileBase)courseMarker }.ToArray();
                //MapMakerGroundTileBase[] newMapMakerTileBases = new List<MapMakerGroundTileBase> { null }.ToArray();

                //_mapMakerHistory.Add(new MapMakerHistoryStep(courseMarker.MapMakerTileType.Tilemap, previousTiles.ToArray(), newTiles, positions, prevMapMakerTileBases, newMapMakerTileBases));

                CreateHistoryEntryWhenRemovingRestrictedObjectOrTile(oldPoint, (MapMakerGroundTileBase)courseMarker);
            }
                

            _aimPoints.Remove(oldPoint);
        }

        if (_aimPoints.Contains(position))
            _aimPoints.Remove(position);

        _aimPoints.Add(position);
        _atLeastOneAimPoint = true;
                
    }
    bool IsThereATeeOffMarkerAtThisPoint(Tilemap map, Vector3Int position)
    {        
        if (map.HasTile(position))
        {
            TileBase oldTileBase = map.GetTile(position);
            if (_tileBaseToMapMakerObject.ContainsKey(oldTileBase))
            {
                MapMakerCourseMarker marker = (MapMakerCourseMarker)_tileBaseToMapMakerObject[oldTileBase];
                if (marker.CourseMarkerType == CourseMarkerType.TeeOffLocation)
                    return true;
            }
        }
        return false;
    }
    void RemoveAimPoint(Vector3Int position, MapMakerCourseMarker courseMarker)
    {
        if (_aimPoints.Contains(position))
            _aimPoints.Remove(position);

        // check if an aim point tile still exists here. If so, remove it?
        if (courseMarker.MapMakerTileType.Tilemap.HasTile(position))
        {
            TileBase tileBase = courseMarker.MapMakerTileType.Tilemap.GetTile(position);
            if (_tileBaseToMapMakerObject.ContainsKey(tileBase))
            {
                MapMakerGroundTileBase groundTileBase = _tileBaseToMapMakerObject[tileBase];
                if (groundTileBase.GetType() == typeof(MapMakerCourseMarker))
                {
                    MapMakerCourseMarker marker = (MapMakerCourseMarker)groundTileBase;
                    if (marker.CourseMarkerType == CourseMarkerType.AimPoint)
                        courseMarker.MapMakerTileType.Tilemap.SetTile(position, null);
                }
            }
            
        }

        if (_aimPoints.Count > 0)
            _atLeastOneAimPoint = true;
        else
            _atLeastOneAimPoint = false;
    }
    void PlaceTeeOffMarkerBall(Vector3Int position, MapMakerCourseMarker courseMarker)
    {
        if (courseMarker == null)
            return;

        Debug.Log("PlaceTeeOffMarkerBall: " + position + " there are already: " + _teeOffMarkerPositions.Count + " tee off marker balls on map");

        // If there are already 2 tee off markers, remove the "oldest" marker?
        if (_teeOffMarkerPositions.Count >= 2 && !_teeOffMarkerPositions.Contains(position))
        {
            Vector3Int oldPoint = _teeOffMarkerPositions[0];
            if (!IsThereATeeOffMarkerAtThisPoint(courseMarker.MapMakerTileType.Tilemap, oldPoint))
            {
                //courseMarker.MapMakerTileType.Tilemap.SetTile(oldPoint, null);

                //// Add a "deletion" to the history?
                //// Create arrays required for the History Steps
                //List<TileBase> previousTiles = new List<TileBase> { courseMarker.TileBase };
                //TileBase[] newTiles = new List<TileBase> { null }.ToArray();
                //Vector3Int[] positions = new List<Vector3Int> { oldPoint }.ToArray();
                //MapMakerGroundTileBase[] prevMapMakerTileBases = new List<MapMakerGroundTileBase> { (MapMakerGroundTileBase)courseMarker }.ToArray();
                //MapMakerGroundTileBase[] newMapMakerTileBases = new List<MapMakerGroundTileBase> { null }.ToArray();

                //_mapMakerHistory.Add(new MapMakerHistoryStep(courseMarker.MapMakerTileType.Tilemap, previousTiles.ToArray(), newTiles, positions, prevMapMakerTileBases, newMapMakerTileBases));

                CreateHistoryEntryWhenRemovingRestrictedObjectOrTile(oldPoint, (MapMakerGroundTileBase)courseMarker);
            }


            _teeOffMarkerPositions.Remove(oldPoint);
        }

        if (_teeOffMarkerPositions.Contains(position))
            _teeOffMarkerPositions.Remove(position);

        _teeOffMarkerPositions.Add(position);
        if (_teeOffMarkerPositions.Count == 2)
            _bothTeeMarkersPlaced = true;
        else
            _bothTeeMarkersPlaced = false;

    }
    void RemoveTeeOffMarkerBall(Vector3Int position, MapMakerCourseMarker courseMarker)
    {
        if (_teeOffMarkerPositions.Contains(position))
            _teeOffMarkerPositions.Remove(position);

        // check if an tee off marker tile still exists here. If so, remove it?
        if (courseMarker.MapMakerTileType.Tilemap.HasTile(position))
        {
            TileBase tileBase = courseMarker.MapMakerTileType.Tilemap.GetTile(position);
            if (_tileBaseToMapMakerObject.ContainsKey(tileBase))
            {
                MapMakerGroundTileBase groundTileBase = _tileBaseToMapMakerObject[tileBase];
                if (groundTileBase.GetType() == typeof(MapMakerCourseMarker))
                {
                    MapMakerCourseMarker marker = (MapMakerCourseMarker)groundTileBase;
                    if (marker.CourseMarkerType == CourseMarkerType.TeeOffMarker)
                        courseMarker.MapMakerTileType.Tilemap.SetTile(position, null);
                }
            }

        }

        if (_teeOffMarkerPositions.Count == 2)
            _bothTeeMarkersPlaced = true;
        else
            _bothTeeMarkersPlaced = false;
    }
    void CreateHistoryEntryWhenRemovingRestrictedObjectOrTile(Vector3Int position, MapMakerGroundTileBase oldTileBase)
    {
        if (oldTileBase == null)
            return;

        oldTileBase.MapMakerTileType.Tilemap.SetTile(position, null);

        // Add a "deletion" to the history?
        // Create arrays required for the History Steps
        List<TileBase> previousTiles = new List<TileBase> { oldTileBase.TileBase };
        TileBase[] newTiles = new List<TileBase> { null }.ToArray();
        Vector3Int[] positions = new List<Vector3Int> { position }.ToArray();
        MapMakerGroundTileBase[] prevMapMakerTileBases = new List<MapMakerGroundTileBase> { oldTileBase }.ToArray();
        MapMakerGroundTileBase[] newMapMakerTileBases = new List<MapMakerGroundTileBase> { null }.ToArray();

        _mapMakerHistory.Add(new MapMakerHistoryStep(oldTileBase.MapMakerTileType.Tilemap, previousTiles.ToArray(), newTiles, positions, prevMapMakerTileBases, newMapMakerTileBases));
    }
    public MapMakerGroundTileBase GetObstacleAtPosition(Vector3Int position)
    {
        if (!_obstalceGroundTileBaseByPosition.ContainsKey(position))
            return null;

        return _obstalceGroundTileBaseByPosition[position];
    }
    void RectangleRenderer()
    {
        //// Render preview of rectangle on the "preview map" then draw the real rectangle on mouse button release
        //_previewMap.ClearAllTiles();

        //// Get the "starting corner" of the rectangle to draw
        //_rectangleBounds.xMin = _currentGridPosition.x < _holdStartPosition.x ? _currentGridPosition.x : _holdStartPosition.x;
        //_rectangleBounds.xMax = _currentGridPosition.x > _holdStartPosition.x ? _currentGridPosition.x : _holdStartPosition.x;
        //_rectangleBounds.yMin = _currentGridPosition.y < _holdStartPosition.y ? _currentGridPosition.y : _holdStartPosition.y;
        //_rectangleBounds.yMax = _currentGridPosition.y > _holdStartPosition.y ? _currentGridPosition.y : _holdStartPosition.y;

        //DrawBounds(_previewMap);
    }
    void LineRenderer()
    {
        // render line preview on the preview tilemap. Draw the line on the correct tilemap on mouse button release

        //_previewMap.ClearAllTiles();

        //float diffX = Mathf.Abs(_currentGridPosition.x - _holdStartPosition.x);
        //float diffY = Mathf.Abs(_currentGridPosition.y - _holdStartPosition.y);

        //bool lineIsHorizontal = diffX >= diffY;

        //if (lineIsHorizontal)
        //{
        //    _rectangleBounds.xMin = _currentGridPosition.x < _holdStartPosition.x ? _currentGridPosition.x : _holdStartPosition.x;
        //    _rectangleBounds.xMax = _currentGridPosition.x > _holdStartPosition.x ? _currentGridPosition.x : _holdStartPosition.x;
        //    _rectangleBounds.yMin = _holdStartPosition.y;
        //    _rectangleBounds.yMax = _holdStartPosition.y;
        //}
        //else
        //{
        //    _rectangleBounds.xMin = _holdStartPosition.x;
        //    _rectangleBounds.xMax = _holdStartPosition.x;
        //    _rectangleBounds.yMin = _currentGridPosition.y < _holdStartPosition.y ? _currentGridPosition.y : _holdStartPosition.y;
        //    _rectangleBounds.yMax = _currentGridPosition.y > _holdStartPosition.y ? _currentGridPosition.y : _holdStartPosition.y;
        //}

        //DrawBounds(_previewMap);
    }
    void SaveBounds()
    {
        if (!_selectedObject)
            return;
        //if (_selectedObject.GroundTileType == GroundTileType.Green)
        //    DrawBounds(_greenMap);
        //else if (_selectedObject.GroundTileType == GroundTileType.Fairway)
        //    DrawBounds(_fairwayMap);
        DrawBounds(_tilemap);
    }
    void DrawBounds(Tilemap map, bool isPreview = false)
    {
        //Debug.Log("DrawBounds: " + map.name + " bounds: " + _rectangleBounds.xMin + ":" + _rectangleBounds.xMax + " x " + _rectangleBounds.yMin + ":" + _rectangleBounds.yMax);
        // List of positions for the History Steps
        List<Vector3Int> positions = new List<Vector3Int>();
        List<bool> isForbidden = new List<bool>();
        List<Vector3Int> forbiddenPositions = new List<Vector3Int>();
        for (int x = _rectangleBounds.xMin; x <= _rectangleBounds.xMax; x++)
        {
            for (int y = _rectangleBounds.yMin; y <= _rectangleBounds.yMax; y++)
            {
                //map.SetTile(new Vector3Int(x, y, 0), _selectedTileBase);

                // OLD before History Steps
                //DrawItem(map, new Vector3Int(x, y, 0), _selectedTileBase);
                // OLD before history steps

                // NEW for history steps
                Vector3Int newPos = new Vector3Int(x, y, 0);
                positions.Add(newPos);
                //isForbidden.Add(IsPlacementForbidden(newPos));
                bool forbidden = IsPlacementForbidden(newPos);
                isForbidden.Add(forbidden);
                if (forbidden)
                    forbiddenPositions.Add(newPos);
            }
        }

        if (isPreview)
        {
            _previewHandler.SetPreview(positions.ToArray(), isForbidden.ToArray());
            //_previewHandler.SetPreview(positions.ToArray(), isForbidden.ToArray(), _rectangleBounds);
        }
        else
        {
            if (forbiddenPositions.Count > 0)
            {
                foreach (Vector3Int forbidPos in forbiddenPositions)
                {
                    if (positions.Contains(forbidPos))
                        positions.Remove(forbidPos);
                }
            }
            // NEW for history steps
            DrawItem(map, positions.ToArray(), _selectedTileBase);
        }

        

    }
    void FloodFill()
    {
        
        if (!_selectedObject)
            return;

        Debug.Log("FloodFill: " + _tilemap.ToString() + " : " + _currentGridPosition.ToString() + " : " + _selectedTileBase.ToString());

        //_tilemap.FloodFill(_currentGridPosition, _selectedTileBase);
        //DrawItem(_tilemap, _currentGridPosition, _selectedTileBase);

        // Check to see if the current selected tile type is an eraser. If so, get the tilemap that the eraser wants to erase from to pass to GetFloodFillPoints
        // If not, then the map used will be the preview map, which is empty. Floodfill will then select all points in the preview map, and send those points to be deleted in Eraser function, causing all tiles in all maps to be deleted
        List<Tilemap> mapsToFill = new List<Tilemap>();

        // Get all maps for eraser tool
        if (_selectedObject.GetType() == typeof(MapMakerTool))
        {
            mapsToFill.AddRange(GetTileMapThatHasTileAtPoint(_currentGridPosition)) ;
        }
        else
        {
            mapsToFill.Add(_tilemap);
        }

        Debug.Log("FloodFill: total number of tilemaps: " + mapsToFill.Count.ToString());
        // Get all the fill points on each map?
        List<Vector3Int> fillPoints = new List<Vector3Int>();
        foreach (Tilemap map in mapsToFill)
        {
            fillPoints.AddRange(GetFloodFillPoints(map, _currentGridPosition, _selectedTileBase));
        }
        Debug.Log("FloodFill: number of fill points: " + fillPoints.Count());
        DrawItem(_tilemap, fillPoints.ToArray(), _selectedTileBase);

        // OLD
        ////List<Vector3Int> fillPoints = GetFloodFillPoints(_tilemap, _currentGridPosition, _selectedTileBase);
        //List<Vector3Int> fillPoints = GetFloodFillPoints(mapToFill, _currentGridPosition, _selectedTileBase);
        //Debug.Log("FloodFill: Number of fill points: " + fillPoints.Count());
        ////DrawItem(_tilemap, GetFloodFillPoints(_tilemap, _currentGridPosition, _selectedTileBase).ToArray(), _selectedTileBase);
        //DrawItem(_tilemap, fillPoints.ToArray(), _selectedTileBase);
        // OLD
    }
    private List<Vector3Int> GetFloodFillPoints(Tilemap map, Vector3Int startPosition, TileBase newTile)
    {
        List<Vector3Int> fillPoints = new List<Vector3Int>();
        //fillPoints.Add(startPosition);

        //TileBase targetTile = _tilemap.GetTile(startPosition);
        TileBase targetTile = map.GetTile(startPosition);
        if (newTile == targetTile)
        {
            Debug.Log("GetFloodFillPoints: User clicked on a tile that matches what they are filling with. Returning just the starting position of: " + startPosition.ToString() +" . Newtile: " + newTile.ToString() + " clicked on tile: " + targetTile.ToString());
            fillPoints.Add(startPosition);
            return fillPoints;
        }
        if (targetTile == null)
        {
            Debug.Log("GetFloodFillPoints: target tile is null");
        }

        
        // Get the current tile at startPosition. When going through the algorithm, check if new tiles match the start position. If yes, add it to the list of tiles to be updated
        // This should allow player to fill "empty space" by replacing all null tiles
        // If they click on a spot with a specific tile already, the algo will search for all tiles that match that tile, and replace those with the new tile?
        // If the user clicks on a tile that matches their selected tile, nothing should happen?

        // https://simpledevcode.wordpress.com/2015/12/29/flood-fill-algorithm-using-c-net/
        // Create a "Stack" of points to iterate through?
        Stack<Vector3Int> points = new Stack<Vector3Int>();
        // Add start position to end of stack
        points.Push(startPosition);
        // Set the "boundary" of what you will search through to the tilemap's bounds? The get width and height of the bounds
        BoundsInt bounds = map.cellBounds;
        //int boundsLength = bounds.xMax - bounds.xMin;
        //int boundsHeight = bounds.yMax - bounds.yMin;

        Debug.Log("GetFloodFillPoints: using tilemap: " + map.ToString() + " new tile to fill with is: " + newTile.ToString() + " Start position is: " + startPosition.ToString() + " bounds are: " + bounds.ToString());
        // loop through the tilemap until there are no more points to go through?
        while (points.Count > 0)
        {
            Vector3Int a = points.Pop();
            // make sure the point is within the bounds of the tilemap?
            if (a.x < bounds.xMax && a.x > bounds.xMin && a.y < bounds.yMax && a.y > bounds.yMin)
            {
                // Get the tile at this point
                TileBase tileBase = map.GetTile(a);

                // Compare to the targetTile. If they DO match, add this point to fillPoints
                if (tileBase == targetTile && !fillPoints.Contains(a))
                {
                    fillPoints.Add(a);

                    //Add points in North/East/South/West directions of this point to the points stack
                    points.Push(new Vector3Int(a.x, a.y + 1, 0));
                    points.Push(new Vector3Int(a.x + 1, a.y, 0));
                    points.Push(new Vector3Int(a.x, a.y - 1, 0));
                    points.Push(new Vector3Int(a.x -1, a.y, 0));
                }

            }
        }

        return fillPoints;
    }
    List<Tilemap> GetTileMapThatHasTileAtPoint(Vector3Int position)
    {
        List<Tilemap> maps = new List<Tilemap>();
        maps = _tileMapReferenceHolder.AllMaps.FindAll(map => map.HasTile(position));
        Debug.Log("GetTileMapThatHasTileAtPoint: Found " + maps.Count + " maps");
        return maps;
    }
    public void SetDrawModeHandler(DrawingModeHandler handler)
    {
        this._drawModeHandler = handler;
    }
    void DrawLine(Tilemap map, bool isPreview = false)
    {
        if (_linePoints.Count > 0)
        {
            // List of positions for the History Steps
            List<Vector3Int> positions = new List<Vector3Int>();
            List<bool> isForbidden = new List<bool>();
            foreach (Vector2Int linePoint in _linePoints)
            {
                //map.SetTile((Vector3Int)linePoint, _selectedTileBase);

                // OLD before History Steps
                //DrawItem(map, (Vector3Int)linePoint, _selectedTileBase);
                // OLD before History Steps

                // NEW for history steps
                positions.Add((Vector3Int)linePoint);
                isForbidden.Add(IsPlacementForbidden((Vector3Int)linePoint));
            }

            if (isPreview)
            {
                _previewHandler.SetPreview(positions.ToArray(), isForbidden.ToArray());
            }
            else
            {
                // NEW for history steps
                DrawItem(map, positions.ToArray(), _selectedTileBase);
            }
            
        }
    }

    #region line drawing?
    void LineRendererTwoElectricBoogaloo()
    {
        //_previewMap.ClearAllTiles();
        //_linePoints.Clear();

        //_linePoints = GetPointsOnLine((Vector2Int)_holdStartPosition, (Vector2Int)_currentGridPosition, false).ToList();

        //DrawLine(_previewMap);
    }
    void SaveLineDrawing()
    {
        if (!_selectedObject)
            return;
        //if (_selectedObject.GroundTileType == GroundTileType.Green)
        //    DrawLine(_greenMap);
        //else if (_selectedObject.GroundTileType == GroundTileType.Fairway)
        //    DrawLine(_fairwayMap);
        DrawLine(_tilemap);
    }
    
    // all code from here? https://github.com/Unity-Technologies/2d-extras/blob/master/Editor/Brushes/LineBrush/LineBrush.cs
    /// <summary>
    /// Enumerates all the points between the start and end position which are
    /// linked diagonally or orthogonally.
    /// </summary>
    /// <param name="startPos">Start position of the line.</param>
    /// <param name="endPos">End position of the line.</param>
    /// <param name="fillGaps">Fills any gaps between the start and end position so that
    /// all points are linked only orthogonally.</param>
    /// <returns>Returns an IEnumerable which enumerates all the points between the start and end position which are
    /// linked diagonally or orthogonally.</returns>
    public static IEnumerable<Vector2Int> GetPointsOnLine(Vector2Int startPos, Vector2Int endPos, bool fillGaps)
    {
        var points = GetPointsOnLine(startPos, endPos);
        if (fillGaps)
        {
            var rise = endPos.y - startPos.y;
            var run = endPos.x - startPos.x;

            if (rise != 0 || run != 0)
            {
                var extraStart = startPos;
                var extraEnd = endPos;


                if (Mathf.Abs(rise) >= Mathf.Abs(run))
                {
                    // up
                    if (rise > 0)
                    {
                        extraStart.y += 1;
                        extraEnd.y += 1;
                    }
                    // down
                    else // rise < 0
                    {

                        extraStart.y -= 1;
                        extraEnd.y -= 1;
                    }
                }
                else // Mathf.Abs(rise) < Mathf.Abs(run)
                {

                    // right
                    if (run > 0)
                    {
                        extraStart.x += 1;
                        extraEnd.x += 1;
                    }
                    // left
                    else // run < 0
                    {
                        extraStart.x -= 1;
                        extraEnd.x -= 1;
                    }
                }

                var extraPoints = GetPointsOnLine(extraStart, extraEnd);
                extraPoints = extraPoints.Except(new[] { extraEnd });
                points = points.Union(extraPoints);
            }

        }

        return points;
    }

    /// <summary>
    /// Gets an enumerable for all the cells directly between two points
    /// http://ericw.ca/notes/bresenhams-line-algorithm-in-csharp.html
    /// </summary>
    /// <param name="p1">A starting point of a line</param>
    /// <param name="p2">An ending point of a line</param>
    /// <returns>Gets an enumerable for all the cells directly between two points</returns>
    public static IEnumerable<Vector2Int> GetPointsOnLine(Vector2Int p1, Vector2Int p2)
    {
        int x0 = p1.x;
        int y0 = p1.y;
        int x1 = p2.x;
        int y1 = p2.y;

        bool steep = Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0);
        if (steep)
        {
            int t;
            t = x0; // swap x0 and y0
            x0 = y0;
            y0 = t;
            t = x1; // swap x1 and y1
            x1 = y1;
            y1 = t;
        }
        if (x0 > x1)
        {
            int t;
            t = x0; // swap x0 and x1
            x0 = x1;
            x1 = t;
            t = y0; // swap y0 and y1
            y0 = y1;
            y1 = t;
        }
        int dx = x1 - x0;
        int dy = Mathf.Abs(y1 - y0);
        int error = dx / 2;
        int ystep = (y0 < y1) ? 1 : -1;
        int y = y0;
        for (int x = x0; x <= x1; x++)
        {
            yield return new Vector2Int((steep ? y : x), (steep ? x : y));
            error = error - dy;
            if (error < 0)
            {
                y += ystep;
                error += dx;
            }
        }
        yield break;
    }
    #endregion
}
