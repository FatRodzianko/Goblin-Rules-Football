using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FogOfWarTileMapManager : MonoBehaviour
{
    public static FogOfWarTileMapManager Instance { get; private set; }

    [Header("Tilemaps")]
    [SerializeField] private Tilemap _blackoutFogOfWarTileMap;
    [SerializeField] private Tilemap _greyedOutFogOfWarTileMap;
    [SerializeField] private Tilemap _enemyFogOfWarTileMap;

    [Header("Tiles")]
    [SerializeField] private Tile _fogOfWarTile;
    [SerializeField] private Tile _fogOfWarWhiteTile;


    private void Awake()
    {
        MakeInstance();
    }
    void MakeInstance()
    {
        if (Instance != null)
        {
            Debug.Log("MakeInstance: more than one FogOfWarTileMapManager. Destroying...");
            Destroy(this);
            return;
        }
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        //UnitVisibilityManager_BombRun.Instance.OnMakeGridPositionVisibleToPlayer += UnitVisibilityManager_BombRun_OnMakeGridPositionVisibleToPlayer;
        UnitVisibilityManager_BombRun.OnMakeGridPositionVisibleToPlayer += UnitVisibilityManager_BombRun_OnMakeGridPositionVisibleToPlayer;
        //UnitVisibilityManager_BombRun.Instance.OnMakeGridPositionNotVisibleToPlayer += UnitVisibilityManager_BombRun_OnMakeGridPositionNotVisibleToPlayer;
        UnitVisibilityManager_BombRun.OnMakeGridPositionNotVisibleToPlayer += UnitVisibilityManager_BombRun_OnMakeGridPositionNotVisibleToPlayer;

        UnitVisibilityManager_BombRun.Instance.OnMakeGridPositionVisibleToEnemy += UnitVisibilityManager_BombRun_OnMakeGridPositionVisibleToEnemy;
        UnitVisibilityManager_BombRun.Instance.OnMakeGridPositionNotVisibleToEnemy += UnitVisibilityManager_BombRun_OnMakeGridPositionNotVisibleToEnemy;

        UnitVisibilityManager_BombRun.Instance.OnEnemyFovVisibleChanged += UnitVisibilityManager_BombRun_OnEnemyFovVisibleChanged;
    }

    

    private void OnDisable()
    {
        //UnitVisibilityManager_BombRun.Instance.OnMakeGridPositionVisibleToPlayer -= UnitVisibilityManager_BombRun_OnMakeGridPositionVisibleToPlayer;
        UnitVisibilityManager_BombRun.OnMakeGridPositionVisibleToPlayer -= UnitVisibilityManager_BombRun_OnMakeGridPositionVisibleToPlayer;
        //UnitVisibilityManager_BombRun.Instance.OnMakeGridPositionNotVisibleToPlayer -= UnitVisibilityManager_BombRun_OnMakeGridPositionNotVisibleToPlayer;
        UnitVisibilityManager_BombRun.OnMakeGridPositionNotVisibleToPlayer -= UnitVisibilityManager_BombRun_OnMakeGridPositionNotVisibleToPlayer;

        UnitVisibilityManager_BombRun.Instance.OnMakeGridPositionVisibleToEnemy -= UnitVisibilityManager_BombRun_OnMakeGridPositionVisibleToEnemy;
        UnitVisibilityManager_BombRun.Instance.OnMakeGridPositionNotVisibleToEnemy -= UnitVisibilityManager_BombRun_OnMakeGridPositionNotVisibleToEnemy;

        UnitVisibilityManager_BombRun.Instance.OnEnemyFovVisibleChanged -= UnitVisibilityManager_BombRun_OnEnemyFovVisibleChanged;
    }
    private void UnitVisibilityManager_BombRun_OnMakeGridPositionNotVisibleToPlayer(object sender, GridPosition e)
    {
        _blackoutFogOfWarTileMap.SetTile(new Vector3Int(e.x, e.y, 0), null);
        _greyedOutFogOfWarTileMap.SetTile(new Vector3Int(e.x, e.y, 0), _fogOfWarTile);
    }

    private void UnitVisibilityManager_BombRun_OnMakeGridPositionVisibleToPlayer(object sender, GridPosition e)
    {
        _greyedOutFogOfWarTileMap.SetTile(new Vector3Int(e.x, e.y, 0), null);
        _blackoutFogOfWarTileMap.SetTile(new Vector3Int(e.x, e.y, 0), null);
    }
    private void UnitVisibilityManager_BombRun_OnMakeGridPositionNotVisibleToEnemy(object sender, GridPosition e)
    {
        _enemyFogOfWarTileMap.SetTile(new Vector3Int(e.x, e.y, 0), null);
    }

    private void UnitVisibilityManager_BombRun_OnMakeGridPositionVisibleToEnemy(object sender, GridPosition e)
    {
        _enemyFogOfWarTileMap.SetTile(new Vector3Int(e.x, e.y, 0), _fogOfWarWhiteTile);
    }
    private void UnitVisibilityManager_BombRun_OnEnemyFovVisibleChanged(object sender, List<GridPosition> gridPositions)
    {
        _enemyFogOfWarTileMap.ClearAllTiles();
        if (gridPositions.Count == 0)
        {
            return;
        }

        foreach (GridPosition gridPosition in gridPositions)
        {
            _enemyFogOfWarTileMap.SetTile(new Vector3Int(gridPosition.x, gridPosition.y, 0), _fogOfWarWhiteTile);
        }
    }
}
