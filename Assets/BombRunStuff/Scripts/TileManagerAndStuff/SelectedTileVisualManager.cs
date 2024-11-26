using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SelectedTileVisualManager : MonoBehaviour
{
    [SerializeField] private Tilemap _selectedTileVisualTileMap;
    [Header("Selected Tile Visual Tiles")]
    [SerializeField] private Tile _selectedTileVisualTile;

    [Header("Mouse Grid Position")]
    [SerializeField] private GridPosition _currentSelectedGridPosition;
    private void Start()
    {
        MouseWorld.instance.OnMouseGridPositionChange += MouseWorld_OnMouseGridPositionChange;
    }

    private void MouseWorld_OnMouseGridPositionChange(object sender, GridPosition gridPosition)
    {
        if (_currentSelectedGridPosition == gridPosition)
            return;

        _selectedTileVisualTileMap.SetTile(new Vector3Int(_currentSelectedGridPosition.x, _currentSelectedGridPosition.y, 0), null);
        _selectedTileVisualTileMap.SetTile(new Vector3Int(gridPosition.x, gridPosition.y, 0), _selectedTileVisualTile);

        _currentSelectedGridPosition = gridPosition;
    }
}
