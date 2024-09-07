using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BombRunTileMapManager : MonoBehaviour
{
    public static BombRunTileMapManager Instance { get; private set; }

    [Header("Tilemaps")]
    [SerializeField] private Tilemap _floorTileMap;
    [SerializeField] private Tilemap _wallTileMap;
    [SerializeField] private Tilemap _gridVisualsTileMap;

    [Header("Tiles")]
    [SerializeField] private Tile _floorTile;
    [SerializeField] private Tile _wallTile;
    [SerializeField] private Tile _gridVisualDefaulTile;

    [Header("Tile List")]
    [SerializeField] private List<GridPosition> _floorTilePositions = new List<GridPosition>();
    [SerializeField] private List<GridPosition> _wallTilePositions = new List<GridPosition>();


    [Header("Grid System Stuff")]
    [SerializeField] private GridSystem<GridObject> _gridSystem;


    private void Awake()
    {
        MakeInstance();
    }

    void MakeInstance()
    {
        if (Instance != null)
        {
            Debug.Log("MakeInstance: more than one BombRunTileMapManager. Destroying...");
            Destroy(this);
            return;
        }
        Instance = this;
    }
    public void SetGridSystem(GridSystem<GridObject> gridSystem)
    {
        _gridSystem = gridSystem;
    }
    public void AddFloorTilesFromGridSystem(GridSystem<GridObject> gridSystem)
    {
        for (int x = 0; x < gridSystem.GetWidth(); x++)
        {
            for (int y = 0; y < gridSystem.GetHeight(); y++)
            {
                AddFloorTileToGridPosition(new GridPosition(x, y));
            }
        }
        AddWallsOutSideOfFloors();
    }
    public void AddFloorTileToGridPosition(GridPosition gridPosition)
    {
        if (gridPosition == new GridPosition(5, 5) || gridPosition == new GridPosition(7, 8))
            return;
        _floorTileMap.SetTile(new Vector3Int(gridPosition.x, gridPosition.y, 0), _floorTile);
        if (!_floorTilePositions.Contains(gridPosition))
        {
            _floorTilePositions.Add(gridPosition);
        }
    }
    public void AddGridVisualDefaultFromGridSystem(GridSystem<GridObject> gridSystem)
    {
        for (int x = 0; x < gridSystem.GetWidth(); x++)
        {
            for (int y = 0; y < gridSystem.GetHeight(); y++)
            {
                AddGridVisualDefaultToGridPosition(new GridPosition(x, y));
            }
        }
    }
    public void AddGridVisualDefaultToGridPosition(GridPosition gridPosition)
    {
        _gridVisualsTileMap.SetTile(new Vector3Int(gridPosition.x, gridPosition.y, 0), _gridVisualDefaulTile);
    }
    public void AddWallsOutSideOfFloors()
    {
        if (_floorTilePositions.Count <= 0)
            return;
        foreach (GridPosition floorPosition in _floorTilePositions)
        {
            if (!_floorTileMap.HasTile(new Vector3Int(floorPosition.x + 1, floorPosition.y + 0, 0)))
            {
                AddWallTileFromGridPosition(new GridPosition(floorPosition.x + 1, floorPosition.y + 0));
            }
            if (!_floorTileMap.HasTile(new Vector3Int(floorPosition.x - 1, floorPosition.y + 0, 0)))
            {
                AddWallTileFromGridPosition(new GridPosition(floorPosition.x - 1, floorPosition.y + 0));
            }
            if (!_floorTileMap.HasTile(new Vector3Int(floorPosition.x + 0, floorPosition.y + 1, 0)))
            {
                AddWallTileFromGridPosition(new GridPosition(floorPosition.x + 0, floorPosition.y + 1));
            }
            if (!_floorTileMap.HasTile(new Vector3Int(floorPosition.x + 1, floorPosition.y + 1, 0)))
            {
                AddWallTileFromGridPosition(new GridPosition(floorPosition.x + 1, floorPosition.y + 1));
            }
            if (!_floorTileMap.HasTile(new Vector3Int(floorPosition.x - 1, floorPosition.y + 1, 0)))
            {
                AddWallTileFromGridPosition(new GridPosition(floorPosition.x - 1, floorPosition.y + 1));
            }
            if (!_floorTileMap.HasTile(new Vector3Int(floorPosition.x + 0, floorPosition.y - 1, 0)))
            {
                AddWallTileFromGridPosition(new GridPosition(floorPosition.x + 0, floorPosition.y - 1));
            }
            if (!_floorTileMap.HasTile(new Vector3Int(floorPosition.x + 1, floorPosition.y - 1, 0)))
            {
                AddWallTileFromGridPosition(new GridPosition(floorPosition.x + 1, floorPosition.y - 1));
            }
            if (!_floorTileMap.HasTile(new Vector3Int(floorPosition.x - 1, floorPosition.y - 1, 0)))
            {
                AddWallTileFromGridPosition(new GridPosition(floorPosition.x - 1, floorPosition.y - 1));
            }

        }
    }
    public void AddWallTileFromGridPosition(GridPosition gridPosition)
    {
        _wallTileMap.SetTile(new Vector3Int(gridPosition.x, gridPosition.y, 0), _wallTile);
        if (!_wallTilePositions.Contains(gridPosition))
        {
            _wallTilePositions.Add(gridPosition);
        }
    }
}
