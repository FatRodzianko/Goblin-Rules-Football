using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    public static LevelGrid Instance { get; private set; }

    [SerializeField] private bool _isHex;

    [SerializeField] private Transform _gridDebugObjectPrefab;
    [SerializeField] private GridSystem<GridObject> _gridSystem;


    [SerializeField] private int _width = 10;
    [SerializeField] private int _height = 10;
    [SerializeField] private float _cellSize = 2f;

    // events
    public event EventHandler OnAnyUnitMovedGridPosition;

    private void Awake()
    {
        MakeInstance();

        if (_isHex)
        {
            _gridSystem = new GridSystemHex<GridObject>(_width, _height, _cellSize,
            (GridSystem<GridObject> g, GridPosition gridPosition) => new GridObject(g, gridPosition)); // delegate function to GridSystem. The GridSystem (g) will be of type GridObject, and includes a GridPosition. This will create a new GridObject with the gridsystem and grid object
        }
        else
        {
            _gridSystem = new GridSystem<GridObject>(_width, _height, _cellSize,
            (GridSystem<GridObject> g, GridPosition gridPosition) => new GridObject(g, gridPosition)); // delegate function to GridSystem. The GridSystem (g) will be of type GridObject, and includes a GridPosition. This will create a new GridObject with the gridsystem and grid object
        }
        


        //_gridSystem = new GridSystem<GridObject>(_width, _height, _cellSize,
        //   (GridSystem<GridObject> g, GridPosition gridPosition) => new GridObject(gridPosition)); // delegate function to GridSystem. The GridSystem (g) will be of type GridObject, and includes a GridPosition. This will create a new GridObject with the gridsystem and grid object


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
    public BombRunUnit GetUnitAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
        return gridObject.GetUnit();
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
    public List<GridPosition> GetValidNeighborGridPositions(GridPosition startingGridPosition, int distanceFromStartingPosition, bool makeCircular = false)
    {
        // change the GetNeighborGridPositions on the GridPosition class to the GridSystem class so there can be an override for hex?
        //List<GridPosition> allNeighborPositions = GridPosition.GetNeighborGridPositions(startingGridPosition, distanceFromStartingPosition, makeCircular);
        List<GridPosition> allNeighborPositions = _gridSystem.GetNeighborGridPositions(startingGridPosition, distanceFromStartingPosition, makeCircular);
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
    public int CalculateDistance(GridPosition a, GridPosition b)
    {
        return _gridSystem.CalculateDistance(a, b);
    }
    public int GetGridWidth()
    {
        return _width;
    }
    public int GetGridHeight()
    {
        return _height;
    }
    public float GetGridCellSize()
    {
        return _cellSize;
    }
}
