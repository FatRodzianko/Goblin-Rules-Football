using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System.Linq;

public class MapMakerBuilder : SingletonInstance<MapMakerBuilder>
{
    MapMakerGolfControls _playerInput;

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
    }
    private void Start()
    {
        if (!_obstacleHolder)
            _obstacleHolder = GameObject.FindGameObjectWithTag("EnvironmentObstacleHolder").transform;
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
        Vector3Int gridPos = _previewMap.WorldToCell(pos);

        if (gridPos != _currentGridPosition)
        {
            _previousGridPosition = _currentGridPosition;
            _currentGridPosition = gridPos;

            if (_selectedObject != null)
            {
                // update tile map preview?
                UpdatePreview();
                // maybe move to OnMouseMove instead of constantly calling it in update?

                if (_holdActive)
                {
                    //Debug.Log("UpdateGridPosition: calling HandleDrawing with _holdActive as true");
                    HandleDrawing();
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
        if (!_selectedObject)
            return;

        if (EventSystem.current.IsPointerOverGameObject())
            return;

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

        //HandleDrawing();
    }
    private void OnRightClick(InputAction.CallbackContext ctx)
    {
        // Unselect any tiles when right clicking?
        SelectedObject = null;
        if (_obstaclePreview != null)
            Destroy(_obstaclePreview);
    }
    private MapMakerGroundTileBase SelectedObject
    {
        set {
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
            UpdatePreview();
        }
    }
    public void ObjectSelected(MapMakerGroundTileBase obj)
    {
        SelectedObject = obj;
        if (obj.GetType() == typeof(MapMakerObstacle))
        {
            if (_obstaclePreview != null)
                Destroy(_obstaclePreview);

            MapMakerObstacle obstacle = (MapMakerObstacle)obj;
            _obstaclePreview = Instantiate(obstacle.ScriptableObstacle.ObstaclePrefab, _currentGridPosition, Quaternion.identity);
        }
        else
        {
            if (_obstaclePreview != null)
                Destroy(_obstaclePreview);
        }
    }
    private Tilemap _tilemap
    {
        get {
            if (_selectedObject != null && _selectedObject.MapMakerTileType != null && _selectedObject.MapMakerTileType.Tilemap != null)
            {
                return _selectedObject.MapMakerTileType.Tilemap;
            }
            return _greenMap;
        }
    }
    void UpdatePreview()
    {
        //Remove old tile if exisiting
        _previewMap.SetTile(_previousGridPosition, null);


        if (_selectedObject != null && _selectedObject.GetType() != typeof(MapMakerTool))
        {
            if (IsPlacementForbidden(_currentGridPosition))
            {
                if (_obstaclePreview != null)
                    Destroy(_obstaclePreview);
                return;
            }                
            else if (_selectedObject.GetType() == typeof(MapMakerObstacle))
            {
                if (_obstaclePreview == null)
                {
                    MapMakerObstacle obstacle = (MapMakerObstacle)_selectedObject;
                    _obstaclePreview = Instantiate(obstacle.ScriptableObstacle.ObstaclePrefab, _currentGridPosition, Quaternion.identity);
                }
                _obstaclePreview.transform.position = _currentGridPosition;
            }
        }
            
        //Set Current tile
        _previewMap.SetTile(_currentGridPosition, _selectedTileBase);

        // update obstacle preview
        
    }
    bool IsPlacementForbidden(Vector3Int position)
    {
        List<MapMakerTileTypes> restrictedCategories = _selectedObject.PlacementRestrictions;
        List<Tilemap> restrictedMaps = restrictedCategories.ConvertAll(category => category.Tilemap);

        List<Tilemap> allMaps = _tileMapReferenceHolder.ForbiddenPlacingWithMaps.Concat(restrictedMaps).ToList();

        if (_tileMapReferenceHolder.ForbiddenPlacingWithMaps.Contains(_tilemap))
        {
            return false;
        }
        else
        {
            //return _tileMapReferenceHolder.ForbiddenPlacingWithMaps.Any(map =>
            //{
            //    return map.HasTile(position);
            //});
            return allMaps.Any(map =>
            {
                return map.HasTile(position);
            });
        }
    }
    void HandleDrawing()
    {
        if (!_selectedObject)
            return;

        switch (_selectedObject.PlaceType)
        {
            case PlaceType.Single:
            default:
                //DrawItem();
                DrawItem(_tilemap, _currentGridPosition, _selectedTileBase);
                break;
            case PlaceType.Line:
                //LineRenderer();
                LineRendererTwoElectricBoogaloo();
                break;
            case PlaceType.Rectangle:
                RectangleRenderer();
                break;
        }
        
    }
    void HandleDrawRelease()
    {
        if (!_selectedObject)
            return;

        switch (_selectedObject.PlaceType)
        {
            case PlaceType.Line:
                SaveLineDrawing();
                _previewMap.ClearAllTiles();
                break;
            case PlaceType.Rectangle:
                SaveBounds();
                _previewMap.ClearAllTiles();
                break;
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

        if (map != _previewMap && _selectedObject.GetType() == typeof(MapMakerTool))
        {
            MapMakerTool tool = (MapMakerTool)_selectedObject;
            tool.Use(positions, out MapMakerHistoryStep historyStep);

            if (historyStep != null)
            {
                _mapMakerHistory.Add(historyStep);
            }
        }
        else
        {
            // Create arrays required for the History Steps
            TileBase[] previousTiles = new TileBase[positions.Length];
            TileBase[] newTiles = new TileBase[positions.Length];
            MapMakerGroundTileBase[] mapMakerTileBases = new MapMakerGroundTileBase[positions.Length];

            for (int i = 0; i < positions.Length; i++)
            {
                previousTiles[i] = map.GetTile(positions[i]);
                mapMakerTileBases[i] = _selectedObject;

                if (_selectedObject.GetType() == typeof(MapMakerTool))
                {
                    map.SetTile(positions[i], tileBase);
                }
                else if (!IsPlacementForbidden(positions[i]))
                {
                    newTiles[i] = tileBase;
                    map.SetTile(positions[i], tileBase);

                    if (_selectedObject.GetType() == typeof(MapMakerObstacle) && map != _previewMap)
                    {
                        Debug.Log("DrawItem: Placing obstacle at: " + positions[i].ToString());
                        PlaceObstacle(positions[i], (MapMakerObstacle)_selectedObject);
                    }
                }
                else
                {
                    newTiles[i] = previousTiles[i];
                }
            }

            if (map != _previewMap)
            {
                _mapMakerHistory.Add(new MapMakerHistoryStep(map, previousTiles, newTiles, positions, mapMakerTileBases));
            }
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

        }
    }
    
    public void PlaceObstacle(Vector3Int position, MapMakerObstacle obstacle)
    {
        if (_placeObstaclesByPostion.ContainsKey(position))
        {
            Debug.Log("PlaceObstacle: Object has already been placeed at this position: " + position + " : " + _placeObstaclesByPostion[position].name);
            return;
        }
        //MapMakerObstacle obj = (MapMakerObstacle)_selectedObject;
        //GameObject gameObject = Instantiate(obj.ScriptableObstacle.ObstaclePrefab, position, Quaternion.identity);
        GameObject gameObject = Instantiate(obstacle.ScriptableObstacle.ObstaclePrefab, position, Quaternion.identity);
        _placeObstaclesByPostion.Add(position, gameObject);
        gameObject.transform.SetParent(_obstacleHolder);

        // Save the MapMakerGroundTileType of the obstacle to save for History Steps and stuff
        _obstalceGroundTileBaseByPosition.Add(position, obstacle);

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
            _placeObstaclesByPostion.Remove(position);
            _obstalceGroundTileBaseByPosition.Remove(position);
        }
            
    }
    public void ClearAllObstacles()
    {
        if (_placeObstaclesByPostion.Count > 0)
        {
            foreach (KeyValuePair<Vector3Int, GameObject> _obstacles in _placeObstaclesByPostion)
            {
                RemoveObstacle(_obstacles.Key, false);
            }
            _placeObstaclesByPostion.Clear();
            _obstalceGroundTileBaseByPosition.Clear();
        }
    }
    public MapMakerGroundTileBase GetObstacleAtPosition(Vector3Int position)
    {
        if (!_obstalceGroundTileBaseByPosition.ContainsKey(position))
            return null;

        return _obstalceGroundTileBaseByPosition[position];
    }
    void RectangleRenderer()
    {
        // Render preview of rectangle on the "preview map" then draw the real rectangle on mouse button release
        _previewMap.ClearAllTiles();

        // Get the "starting corner" of the rectangle to draw
        _rectangleBounds.xMin = _currentGridPosition.x < _holdStartPosition.x ? _currentGridPosition.x : _holdStartPosition.x;
        _rectangleBounds.xMax = _currentGridPosition.x > _holdStartPosition.x ? _currentGridPosition.x : _holdStartPosition.x;
        _rectangleBounds.yMin = _currentGridPosition.y < _holdStartPosition.y ? _currentGridPosition.y : _holdStartPosition.y;
        _rectangleBounds.yMax = _currentGridPosition.y > _holdStartPosition.y ? _currentGridPosition.y : _holdStartPosition.y;

        DrawBounds(_previewMap);
    }
    void LineRenderer()
    {
        // render line preview on the preview tilemap. Draw the line on the correct tilemap on mouse button release

        _previewMap.ClearAllTiles();

        float diffX = Mathf.Abs(_currentGridPosition.x - _holdStartPosition.x);
        float diffY = Mathf.Abs(_currentGridPosition.y - _holdStartPosition.y);

        bool lineIsHorizontal = diffX >= diffY;

        if (lineIsHorizontal)
        {
            _rectangleBounds.xMin = _currentGridPosition.x < _holdStartPosition.x ? _currentGridPosition.x : _holdStartPosition.x;
            _rectangleBounds.xMax = _currentGridPosition.x > _holdStartPosition.x ? _currentGridPosition.x : _holdStartPosition.x;
            _rectangleBounds.yMin = _holdStartPosition.y;
            _rectangleBounds.yMax = _holdStartPosition.y;
        }
        else
        {
            _rectangleBounds.xMin = _holdStartPosition.x;
            _rectangleBounds.xMax = _holdStartPosition.x;
            _rectangleBounds.yMin = _currentGridPosition.y < _holdStartPosition.y ? _currentGridPosition.y : _holdStartPosition.y;
            _rectangleBounds.yMax = _currentGridPosition.y > _holdStartPosition.y ? _currentGridPosition.y : _holdStartPosition.y;
        }

        DrawBounds(_previewMap);
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
    void DrawBounds(Tilemap map)
    {
        //Debug.Log("DrawBounds: " + map.name + " bounds: " + _rectangleBounds.xMin + ":" + _rectangleBounds.xMax + " x " + _rectangleBounds.yMin + ":" + _rectangleBounds.yMax);

        // List of positions for the History Steps
        List<Vector3Int> positions = new List<Vector3Int>();

        for (int x = _rectangleBounds.xMin; x <= _rectangleBounds.xMax; x++)
        {
            for (int y = _rectangleBounds.yMin; y <= _rectangleBounds.yMax; y++)
            {
                //map.SetTile(new Vector3Int(x, y, 0), _selectedTileBase);

                // OLD before History Steps
                //DrawItem(map, new Vector3Int(x, y, 0), _selectedTileBase);
                // OLD before history steps

                // NEW for history steps
                positions.Add(new Vector3Int(x, y, 0));
            }
        }

        // NEW for history steps
        DrawItem(map, positions.ToArray(), _selectedTileBase);

    }
    void DrawLine(Tilemap map)
    {
        if (_linePoints.Count > 0)
        {
            // List of positions for the History Steps
            List<Vector3Int> positions = new List<Vector3Int>();
            foreach (Vector2Int linePoint in _linePoints)
            {
                //map.SetTile((Vector3Int)linePoint, _selectedTileBase);

                // OLD before History Steps
                //DrawItem(map, (Vector3Int)linePoint, _selectedTileBase);
                // OLD before History Steps

                // NEW for history steps
                positions.Add((Vector3Int)linePoint);
            }

            // NEW for history steps
            DrawItem(map, positions.ToArray(), _selectedTileBase);
        }
    }

    #region line drawing?
    void LineRendererTwoElectricBoogaloo()
    {
        _previewMap.ClearAllTiles();
        _linePoints.Clear();

        _linePoints = GetPointsOnLine((Vector2Int)_holdStartPosition, (Vector2Int)_currentGridPosition, false).ToList();

        DrawLine(_previewMap);
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
