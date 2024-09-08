using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingBombRun : MonoBehaviour
{
    [SerializeField] private Transform _gridDebugObjectPrefab;
    private GridSystem<GridObject> _gridSystem;
    private void Start()
    {
        _gridSystem = new GridSystem<GridObject>(10, 10, 2f, (GridSystem<GridObject> g, GridPosition gridPosition) => new GridObject(g, gridPosition));
        _gridSystem.CreateDebugObjects(_gridDebugObjectPrefab);
        BombRunTileMapManager.Instance.SetGridSystem(_gridSystem);
        BombRunTileMapManager.Instance.AddFloorTilesFromGridSystem(_gridSystem);
        BombRunTileMapManager.Instance.AddGridVisualDefaultFromGridSystem(_gridSystem);
    }
    private void Update()
    {
        //Debug.Log(_gridSystem.GetGridPositon(MouseWorld.GetPosition()));
    }
}
