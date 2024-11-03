using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    public static LevelGrid Instance { get; private set; }

    [SerializeField] private Transform _gridDebugObjectPrefab;
    private GridSystem<GridObject> _gridSystem;

    [SerializeField] private int _width = 10;
    [SerializeField] private int _height = 10;
    [SerializeField] private float _cellSize = 2f;

    // events
    public event EventHandler OnAnyUnitMovedGridPosition;

    private void Awake()
    {
        MakeInstance();
        _gridSystem = new GridSystem<GridObject>(_width, _height, _cellSize, 
            (GridSystem<GridObject> g, GridPosition gridPosition) => new GridObject(g, gridPosition)); // delegate function to GridSystem. The GridSystem (g) will be of type GridObject, and includes a GridPosition. This will create a new GridObject with the gridsystem and grid object
        //_gridSystem.CreateDebugObjects(_gridDebugObjectPrefab);
    }

    void MakeInstance()
    {
        if (Instance != null)
        {
            Debug.Log("MakeInstance: more than one LevelGrid. Destroying...");
            Destroy(this);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        // Set the tiles on the tile maps
        BombRunTileMapManager.Instance.SetGridSystem(_gridSystem);
        BombRunTileMapManager.Instance.AddFloorTilesFromGridSystem(_gridSystem);
        BombRunTileMapManager.Instance.AddGridVisualDefaultFromGridSystem(_gridSystem);

        // Create the pathfinding grid
        PathFinding.Instance.Setup(_width, _height, _cellSize);
    }
    public void AddUnitAtGridPosition(GridPosition gridPosition, BombRunUnit unit)
    {
        GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
        gridObject.AddUnit(unit);
    }
    public List<BombRunUnit> GetUnitListAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
        return gridObject.GetUnitList();
    }
    public void RemoveUnitAtGridPosition(GridPosition gridPosition, BombRunUnit unit)
    {
        GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
        gridObject.RemoveUnit(unit);
    }
    public void UnitMovedGridPosition(BombRunUnit unit, GridPosition fromGridPosition, GridPosition toGridPosition)
    {
        RemoveUnitAtGridPosition(fromGridPosition, unit);
        AddUnitAtGridPosition(toGridPosition, unit);

        OnAnyUnitMovedGridPosition?.Invoke(this, EventArgs.Empty);
    }
    public GridPosition GetGridPositon(Vector3 worldPosition)
    {
        return _gridSystem.GetGridPositon(worldPosition);
    }
    public Vector3 GetWorldPosition(GridPosition gridPosition)
    {
        return _gridSystem.GetWorldPosition(gridPosition);
    }
    public bool IsValidGridPosition(GridPosition gridPosition)
    {
        return _gridSystem.IsValidGridPosition(gridPosition);
    }
    public bool HasAnyUnitOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
        return gridObject.HasAnyUnit();
    }
    public int GetWidth()
    {
        return _gridSystem.GetWidth();
    }
    public int GetHeight()
    {
        return _gridSystem.GetHeight();
    }
    public GridSystem<GridObject> GetGridObjectGridSystem()
    {
        return _gridSystem;
    }
}
