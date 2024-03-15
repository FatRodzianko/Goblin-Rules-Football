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
    [SerializeField] Tilemap _previewMap, _greenMap, _fairwayMap;

    // Selected objects?
    [SerializeField] MapMakerGroundTileBase _selectedObject;
    [SerializeField] TileBase _selectedTileBase;

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

    protected override void Awake()
    {
        base.Awake();
        _playerInput = new MapMakerGolfControls();
        _camera = Camera.main;
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
            // update tile map preview?
            UpdatePreview();
            // maybe move to OnMouseMove instead of constantly calling it in update?

            if (_holdActive)
            {
                //Debug.Log("UpdateGridPosition: calling HandleDrawing with _holdActive as true");
                HandleDrawing();
            }
        }
    }
    private void OnMouseMove(InputAction.CallbackContext ctx)
    {
        _mousePos = ctx.ReadValue<Vector2>();

        if (!_selectedObject)
            return;
        
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
    }
    private MapMakerGroundTileBase SelectedObject
    {
        set {
            _selectedObject = value;

            // if selected object is not null, set the selected tile base. If it is null, set selected tile base to null
            _selectedTileBase = _selectedObject != null ? _selectedObject.TileBase : null;

            if (_selectedObject)
            {
                UpdateGridPosition(_mousePos);
                //_previewMap.GetComponent<TilemapRenderer>().sortingOrder = _selectedObject.MapMakerTileType.SortingOrder;
            }
            else
            {
                //_previewMap.GetComponent<TilemapRenderer>().sortingOrder = 0;
            }
                

            UpdatePreview();
        }
    }
    public void ObjectSelected(MapMakerGroundTileBase obj)
    {
        SelectedObject = obj;
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
        //Set Current tile
        _previewMap.SetTile(_currentGridPosition, _selectedTileBase);
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
    void DrawItem(Tilemap map, Vector3Int position, TileBase tileBase)
    {
        // TODO: automatically select the correct tilemap
        //if (!_selectedObject)
        //    return;

        // OLD BEGIN
        //if (_selectedObject.GroundTileType == GroundTileType.Green)
        //{
        //    _greenMap.SetTile(_currentGridPosition, _selectedTileBase);
        //}
        //else if (_selectedObject.GroundTileType == GroundTileType.Fairway)
        //{
        //    _fairwayMap.SetTile(_currentGridPosition, _selectedTileBase);
        //}
        // OLD END

        //_tilemap.SetTile(_currentGridPosition, _selectedTileBase);

        if (_selectedObject.GetType() == typeof(MapMakerTool))
        {
            MapMakerTool tool = (MapMakerTool)_selectedObject;
            tool.Use(_currentGridPosition);
            return;
        }
        
        map.SetTile(position, tileBase);

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
        for (int x = _rectangleBounds.xMin; x <= _rectangleBounds.xMax; x++)
        {
            for (int y = _rectangleBounds.yMin; y <= _rectangleBounds.yMax; y++)
            {
                //map.SetTile(new Vector3Int(x, y, 0), _selectedTileBase);
                DrawItem(map, new Vector3Int(x, y, 0), _selectedTileBase);
            }
        }
    }
    void DrawLine(Tilemap map)
    {
        if (_linePoints.Count > 0)
        {
            foreach (Vector2Int linePoint in _linePoints)
            {
                //map.SetTile((Vector3Int)linePoint, _selectedTileBase);
                DrawItem(map, (Vector3Int)linePoint, _selectedTileBase);
            }
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
