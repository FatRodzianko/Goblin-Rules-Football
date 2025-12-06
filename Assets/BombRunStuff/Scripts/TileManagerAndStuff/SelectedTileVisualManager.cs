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
    [SerializeField] private Color _invalidActionColor = Color.red;

    [Header("Mouse Grid Position")]
    [SerializeField] private GridPosition _currentSelectedGridPosition;
    private void Start()
    {
        MouseWorld.instance.OnMouseGridPositionChange += MouseWorld_OnMouseGridPositionChange;
        UnitActionSystem.OnPlayerClickInvalidPosition += UnitActionSystem_OnPlayerClickInvalidPosition;
    }    

    private void OnDisable()
    {
        MouseWorld.instance.OnMouseGridPositionChange -= MouseWorld_OnMouseGridPositionChange;
        UnitActionSystem.OnPlayerClickInvalidPosition -= UnitActionSystem_OnPlayerClickInvalidPosition;
    }

    private void MouseWorld_OnMouseGridPositionChange(object sender, GridPosition gridPosition)
    {
        if (_currentSelectedGridPosition == gridPosition)
            return;

        if (BombRunTileMapManager.Instance.IsWallOnThisPosition(gridPosition))
        {
            if (UnitActionSystem.Instance.GetSelectedAction() == null || UnitActionSystem.Instance.GetSelectedAction().GetActionType() != ActionType.LookAt)
            {
                if (LevelGrid.Instance.GetSeenByPlayer(gridPosition))
                {
                    return;
                }
            }           
            
        }
        _selectedTileVisualTileMap.SetTile(new Vector3Int(_currentSelectedGridPosition.x, _currentSelectedGridPosition.y, 0), null);
        _selectedTileVisualTileMap.SetTile(new Vector3Int(gridPosition.x, gridPosition.y, 0), _selectedTileVisualTile);

        _currentSelectedGridPosition = gridPosition;
    }
    private void UnitActionSystem_OnPlayerClickInvalidPosition(object sender, EventArgs e)
    {        
        StartCoroutine(FlashSelectedTileIndicator(_invalidActionColor, Color.white));
    }
    private IEnumerator FlashSelectedTileIndicator(Color newColor, Color oldColor)
    {
        Debug.Log("FlashSelectedTileIndicator");
        _selectedTileVisualTileMap.color = newColor;
        yield return new WaitForSeconds(0.25f);
        _selectedTileVisualTileMap.color = oldColor;
    }
}
