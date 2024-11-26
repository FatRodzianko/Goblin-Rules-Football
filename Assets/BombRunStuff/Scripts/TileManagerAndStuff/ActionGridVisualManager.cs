using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class ActionGridVisualManager : MonoBehaviour
{
    [Serializable]
    public struct GridVisualTypeColor
    {
        public GridVisualType gridVisualType;
        public Color color;
        public Tile tile;
    }
    public enum GridVisualType
    {
        White,
        Blue,
        Red,
        Yellow,
        RedSoft
    }

    [Header("Tilemaps")]
    [SerializeField] private Tilemap _actionVisualsTileMap;


    [Header("Grid Visual Tiles")]
    [SerializeField] private Tile _actionVisualTile;

    [Header("Tile List")]
    [SerializeField] private List<GridPosition> _actionVisualPositions = new List<GridPosition>();

    [Header("Grid System Stuff")]
    [SerializeField] private GridSystem<GridObject> _gridSystem;

    [SerializeField] private List<GridVisualTypeColor> _gridVisualTypeColorList;

    private void Start()
    {
        // event subscriptions
        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged;
        UnitActionSystem.Instance.OnBusyChanged += UnitActionSystem_OnBusyChanged;

        // cache the grid system?
        _gridSystem = LevelGrid.Instance.GetGridObjectGridSystem();

        // Update the action visuals for the initially selected unit and selected action
        UpdateActionVisuals();
    }

    

    private void OnDisable()
    {
        // event subscriptions
        UnitActionSystem.Instance.OnSelectedActionChanged -= UnitActionSystem_OnSelectedActionChanged;
        UnitActionSystem.Instance.OnBusyChanged -= UnitActionSystem_OnBusyChanged;
    }
    

    public void AddActionVisualToGridPosition(GridPosition gridPosition, Tile gridVisual, Color color)
    {
        gridVisual.color = color;
        _actionVisualsTileMap.SetTile(new Vector3Int(gridPosition.x, gridPosition.y, 0), gridVisual);
        _actionVisualPositions.Add(gridPosition);
    }
    public void RemoveActionVisualToGridPosition(GridPosition gridPosition)
    {
        _actionVisualsTileMap.SetTile(new Vector3Int(gridPosition.x, gridPosition.y, 0), null);
    }
    private void ShowGridPositionRange(GridPosition gridPosition, int range, GridVisualType gridVisualType)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                GridPosition testGridPosition = gridPosition + new GridPosition(x, y);
                if (testGridPosition == gridPosition)
                {
                    continue;
                }
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }
                int testDistance = Mathf.Abs(x) + Mathf.Abs(y);
                if (testDistance > range)
                {
                    continue;
                }
                
                gridPositionList.Add(testGridPosition);
            }
        }
        ShowActionVisualsFromList(gridPositionList, gridVisualType);
    }
    public void ShowActionVisualsFromList(List<GridPosition> gridPositions, GridVisualType gridVisualType)
    {
        foreach (GridPosition gridPosition in gridPositions)
        {
            AddActionVisualToGridPosition(gridPosition, GetGridVisualTypeTile(gridVisualType), GetGridVisualTypeColor(gridVisualType));
        }
    }
    public void HideAllActionVisuals()
    {
        foreach (GridPosition gridPosition in _actionVisualPositions)
        {
            RemoveActionVisualToGridPosition(gridPosition);
        }
        _actionVisualPositions.Clear();
    }
    private void UnitActionSystem_OnSelectedActionChanged(object sender, EventArgs e)
    {
        UpdateActionVisuals();
    }
    private void UnitActionSystem_OnBusyChanged(object sender, bool busy)
    {
        if (busy)
        {
            return;
        }
        UpdateActionVisuals();
    }
    private void UpdateActionVisuals()
    {
        HideAllActionVisuals();

        BombRunUnit unit = UnitActionSystem.Instance.GetSelectedUnit();
        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();

        GridVisualType gridVisualType;
        switch (selectedAction)
        {
            default:
            case MoveAction moveAction:
                gridVisualType = GridVisualType.White;
                break;
            case SpinAction spinAction:
                gridVisualType = GridVisualType.Blue;
                break;
            case DefendAction defendAction:
                gridVisualType = GridVisualType.Red;
                break;
        }

        List<GridPosition> actionVisualPositions = selectedAction.GetValidActionGridPositionList();

        //ShowActionVisualsFromList(actionVisualPositions, _actionVisualTile, GetGridVisualTypeColor(gridVisualType));
        ShowActionVisualsFromList(actionVisualPositions, gridVisualType);
    }
    private Color GetGridVisualTypeColor(GridVisualType gridVisualType)
    {
        foreach (GridVisualTypeColor gridVisualTypeColor in _gridVisualTypeColorList)
        {
            if (gridVisualTypeColor.gridVisualType == gridVisualType)
            {
                return gridVisualTypeColor.color;
            }
        }
        Debug.LogError("Could not find GetGridVisualTypeColor for GridVisualType: " + gridVisualType);
        return Color.white;
    }
    private Tile GetGridVisualTypeTile(GridVisualType gridVisualType)
    {
        foreach (GridVisualTypeColor gridVisualTypeColor in _gridVisualTypeColorList)
        {
            if (gridVisualTypeColor.gridVisualType == gridVisualType)
            {
                return gridVisualTypeColor.tile;
            }
        }
        Debug.LogError("Could not find GetGridVisualTypeTile for GridVisualType: " + gridVisualType);
        return null;
    }
}
