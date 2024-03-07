using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class MapMakerBuilder : SingletonInstance<MapMakerBuilder>
{
    MapMakerGolfControls _playerInput;

    // Tile maps?
    [SerializeField] Tilemap _previewMap;

    // Selected objects?
    [SerializeField] MapMakerGroundTileBase _selectedObject;
    [SerializeField] TileBase _selectedTileBase;

    // mouse position stuff?
    Vector2 _mousePos;
    Vector3Int _currentGridPosition;
    Vector3Int _previousGridPosition;

    // Camera stuff? UI stuff?
    Camera _camera;

    protected override void Awake()
    {
        base.Awake();
        _playerInput = new MapMakerGolfControls();
        _camera = Camera.main;
    }

    private void OnEnable()
    {
        _playerInput.Enable();

        _playerInput.MapMaker.MousePosition.performed += OnMouseMove;
        _playerInput.MapMaker.MouseLeftClick.performed += OnLeftClick;
        _playerInput.MapMaker.MouseRightClick.performed += OnRightClick;
    }
    private void OnDisable()
    {
        _playerInput.Disable();
        _playerInput.MapMaker.MousePosition.performed -= OnMouseMove;
        _playerInput.MapMaker.MouseLeftClick.performed -= OnLeftClick;
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
                UpdateGridPosition(_mousePos) ;

            UpdatePreview();
        }
    }
    public void ObjectSelected(MapMakerGroundTileBase obj)
    {
        SelectedObject = obj;
    }
    void UpdatePreview()
    {
        //Remove old tile if exisiting
        _previewMap.SetTile(_previousGridPosition, null);
        //Set Current tile
        _previewMap.SetTile(_currentGridPosition, _selectedTileBase);
    }
}
