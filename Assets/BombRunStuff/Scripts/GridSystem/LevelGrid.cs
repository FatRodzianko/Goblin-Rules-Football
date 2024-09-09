using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    public static LevelGrid Instance { get; private set; }

    [SerializeField] private Transform _gridDebugObjectPrefab;
    private GridSystem<GridObject> _gridSystem;

    private void Awake()
    {
        MakeInstance();
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
        _gridSystem = new GridSystem<GridObject>(10, 10, 2f, (GridSystem<GridObject> g, GridPosition gridPosition) => new GridObject(g, gridPosition));
        _gridSystem.CreateDebugObjects(_gridDebugObjectPrefab);

        BombRunTileMapManager.Instance.SetGridSystem(_gridSystem);
        BombRunTileMapManager.Instance.AddFloorTilesFromGridSystem(_gridSystem);
        BombRunTileMapManager.Instance.AddGridVisualDefaultFromGridSystem(_gridSystem);
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
}
