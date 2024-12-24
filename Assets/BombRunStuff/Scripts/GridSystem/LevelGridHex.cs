using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGridHex : MonoBehaviour
{
    public static LevelGridHex Instance { get; private set; }

    [SerializeField] private Transform _gridDebugObjectPrefab;
    private GridSystemHex<GridObject> _gridSystem;

    [SerializeField] private int _width = 10;
    [SerializeField] private int _height = 10;
    [SerializeField] private float _cellSize = 2f;

    // events
    public event EventHandler OnAnyUnitMovedGridPosition;

    private void Awake()
    {
        MakeInstance();
        _gridSystem = new GridSystemHex<GridObject>(_width, _height, _cellSize,
            (GridSystem<GridObject> g, GridPosition gridPosition) => new GridObject(g, gridPosition)); // delegate function to GridSystem. The GridSystem (g) will be of type GridObject, and includes a GridPosition. This will create a new GridObject with the gridsystem and grid object
        //_gridSystem.CreateDebugObjects(_gridDebugObjectPrefab);
    }

    void MakeInstance()
    {
        if (Instance != null)
        {
            Debug.Log("MakeInstance: more than one LevelGridHex. Destroying...");
            Destroy(this);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        // Set the tiles on the tile maps
        //BombRunTileMapManager.Instance.SetGridSystem(_gridSystem);
        //BombRunTileMapManager.Instance.AddFloorTilesFromGridSystem(_gridSystem);
        //BombRunTileMapManager.Instance.AddGridVisualDefaultFromGridSystem(_gridSystem);

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
    public GridSystemHex<GridObject> GetGridObjectGridSystem()
    {
        return _gridSystem;
    }
    public List<GridPosition> GetValidNeighborGridPositions(GridPosition startingGridPosition, int distanceFromStartingPosition, bool makeCircular = false)
    {
        List<GridPosition> allNeighborPositions = GridPosition.GetNeighborGridPositions(startingGridPosition, distanceFromStartingPosition, makeCircular, true);
        List<GridPosition> validNeighborPositions = new List<GridPosition>();

        for (int i = 0; i < allNeighborPositions.Count; i++)
        {
            if (IsValidGridPosition(allNeighborPositions[i]))
            {
                validNeighborPositions.Add(allNeighborPositions[i]);
            }
        }
        return validNeighborPositions;
    }
    public void AddObstacleAtGridPosition(GridPosition gridPosition, BaseBombRunObstacle obstacle)
    {
        Debug.Log("AddObstacleAtGridPosition: adding: " + obstacle.name + " to: " + gridPosition.ToString());
        GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
        gridObject.AddObstacle(obstacle);
    }
    public BaseBombRunObstacle GetObstacleAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
        return gridObject.GetObstacle();
    }
    public void RemoveObstacleAtGridPosition(GridPosition gridPosition, BaseBombRunObstacle obstacle)
    {
        GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
        gridObject.RemoveObstacle(obstacle);
        Debug.Log("RemoveObstacleAtGridPosition: Removed obstacle: " + obstacle.name + " at position: " + gridPosition.ToString());
    }
    public bool HasAnyObstacleOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
        return gridObject.HasAnyObstacle();
    }
    public void AddInteractableAtGridPosition(GridPosition gridPosition, IInteractable interactable)
    {
        Debug.Log("AddInteractableAtGridPosition: adding: " + interactable + " to: " + gridPosition.ToString());
        GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
        gridObject.AddInteractable(interactable);
    }
    public IInteractable GetInteractableAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
        return gridObject.GetInteractable();
    }
    public void RemoveInteractableAtGridPosition(GridPosition gridPosition, IInteractable interactable)
    {
        GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
        gridObject.RemoveInteractable(interactable);
        Debug.Log("RemoveInteractableAtGridPosition: Removed interactable: " + interactable + " at position: " + gridPosition.ToString());
    }
    public bool HasAnyInteractableOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
        return gridObject.HasAnyInteractable();
    }
}
