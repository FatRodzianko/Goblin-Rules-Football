using System;
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
    [SerializeField] private Tilemap _obstaclesTileMap;
    [SerializeField] private Tilemap _gridVisualTileMap;
    [SerializeField] private Tilemap _actionVisualsTileMap;    

    [Header("Tiles")]
    [Header("Floor Tiles")]
    [SerializeField] private Tile _floorTile;

    [Header("Wall Tiles")]
    [SerializeField] private Tile _wallTile;

    [Header("Grid Visual Tiles")]
    [SerializeField] private Tile _gridVisualDefaulTile;
    [SerializeField] private Tile _actionVisualTile;    

    [Header("Tile List")]
    [SerializeField] private List<GridPosition> _floorTilePositions = new List<GridPosition>();
    [SerializeField] private List<GridPosition> _wallTilePositions = new List<GridPosition>();
    [SerializeField] private List<GridPosition> _obstalceTilePositions = new List<GridPosition>();

    [Header("Grid System Stuff")]
    [SerializeField] private GridSystem<GridObject> _gridSystem;

    [Header("Obstacles")]
    [SerializeField] private BombRunObstacleManager _bombRunObstacleManager;

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
    private void Update()
    {

    }
    private void Start()
    {
        // get the grid system from LevelGrid that is created during its Awake function
        //_gridSystem = LevelGrid.Instance.GetGridObjectGridSystem();
        //SetGridSystem(LevelGrid.Instance.GetGridObjectGridSystem());

        // Create all the tiles needed for the grid system
        //AddFloorTilesFromGridSystem(_gridSystem);
        //AddGridVisualDefaultFromGridSystem(_gridSystem);
    }    

    private void OnDisable()
    {

    }


    public void SetGridSystem(GridSystem<GridObject> gridSystem)
    {
        _gridSystem = gridSystem;
    }
    public void AddFloorTilesFromGridSystem(GridSystem<GridObject> gridSystem)
    {
        // Get the walls that were drawn onto the wall tilemap
        // in the future a different function will generate the level that contains where the walls are?
        // OLD
        //_wallTilePositions = GetWallGridPositionsFromTileMap();
        // OLD

        ClearWallPositions();
        ClearObstaclePositions();
        UpdateWallAndObstaclePositionLists();
        HideObstacleTileMap();

        for (int x = 0; x < gridSystem.GetWidth(); x++)
        {
            for (int y = 0; y < gridSystem.GetHeight(); y++)
            {
                AddFloorTileToGridPosition(new GridPosition(x, y));
            }
        }

        // Can also use this if the level generator generates the floor instead of the walls?
        //AddWallsOutSideOfFloors();
    }
    private List<GridPosition> GetWallGridPositionsFromTileMap()
    {
        List<GridPosition> wallPositions = new List<GridPosition>();

        for (int x = 0; x < _gridSystem.GetWidth(); x++)
        {
            for (int y = 0; y < _gridSystem.GetHeight(); y++)
            {
                if (_wallTileMap.HasTile(new Vector3Int(x, y, 0)))
                {
                    wallPositions.Add(new GridPosition(x, y));
                }

            }
        }
        return wallPositions;
    }
    private void ClearWallPositions()
    {
        _wallTilePositions.Clear();
    }
    private void ClearObstaclePositions()
    {
        _obstalceTilePositions.Clear();
    }
    private void UpdateWallAndObstaclePositionLists()
    {
        for (int x = 0; x < _gridSystem.GetWidth(); x++)
        {
            for (int y = 0; y < _gridSystem.GetHeight(); y++)
            {
                Vector3Int newPosition = new Vector3Int(x, y, 0);
                if (_wallTileMap.HasTile(newPosition))
                {
                    //Debug.Log("UpdateWallAndObstaclePositionLists: found wall tile at: " + x.ToString() + ", " + y.ToString());
                    _wallTilePositions.Add(new GridPosition(x, y));
                }
                if (_obstaclesTileMap.HasTile(newPosition))
                {
                    //Debug.Log("UpdateWallAndObstaclePositionLists: found obstacle tile at: " + x.ToString() + ", " + y.ToString());
                    //_obstalceTilePositions.Add(new GridPosition(x, y));
                    GridPosition newGridPosition = new GridPosition(x, y);
                    // spawn the obstacle transform at the grid position. Get the obstacle to spawn based on the tile at that position and the mapping in BombRunObstacleManager
                    _bombRunObstacleManager.AddObstacleToPositionFromTile(newGridPosition, _obstaclesTileMap.GetTile(newPosition));
                }
            }
        }
    }
    private void HideObstacleTileMap()
    {
        _obstaclesTileMap.color = new Color(1f, 1f, 1f, 0f);
    }
    public void AddFloorTileToGridPosition(GridPosition gridPosition)
    {
        //// for testing that walls are added in for gaps in the floor map
        //if (gridPosition == new GridPosition(3, 8) || gridPosition == new GridPosition(2, 4) || gridPosition == new GridPosition(5, 2) || gridPosition == new GridPosition(5, 3) || gridPosition == new GridPosition(5, 4))
        //    return;
        //// for testing that walls are added in for gaps in the floor map

        if (IsWallOnThisPosition(gridPosition))
        {
            //Debug.Log("AddFloorTileToGridPosition: wall tile at: " + gridPosition.ToString() + ". Skipping...");
            //return;
        }


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
                GridPosition gridPosition = new GridPosition(x, y);
                if (IsWallOnThisPosition(gridPosition))
                    continue;
                AddGridVisualToGridPosition(gridPosition, _gridVisualDefaulTile);
            }
        }
    }
    public void AddGridVisualToGridPosition(GridPosition gridPosition, Tile gridVisual)
    {
        _gridVisualTileMap.SetTile(new Vector3Int(gridPosition.x, gridPosition.y, 0), gridVisual);
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
        if (!IsWallOnThisPosition(gridPosition))
        {
            _wallTilePositions.Add(gridPosition);
        }
    }
    public List<GridPosition> GetFloorGridPositions()
    {
        return _floorTilePositions;
    }
    public List<GridPosition> GetWallGridPositions()
    {
        return _wallTilePositions;
    }
    public bool IsWallOnThisPosition(GridPosition gridPosition)
    {
        return _wallTilePositions.Contains(gridPosition);
    }
    public bool IsObstacleOnThisPosition(GridPosition gridPosition)
    {
        return _bombRunObstacleManager.IsObstacleAtGridPosition(gridPosition);
    }
}
