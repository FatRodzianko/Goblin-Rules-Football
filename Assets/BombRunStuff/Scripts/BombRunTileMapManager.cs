using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BombRunTileMapManager : MonoBehaviour
{
    public static BombRunTileMapManager Instance { get; private set; }

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
    [SerializeField] private Tilemap _floorTileMap;
    [SerializeField] private Tilemap _wallTileMap;
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
    [SerializeField] private List<GridPosition> _actionVisualPositions = new List<GridPosition>();


    [Header("Grid System Stuff")]
    [SerializeField] private GridSystem<GridObject> _gridSystem;

    [SerializeField] private List<GridVisualTypeColor> _gridVisualTypeMaterialList;


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
        // event subscriptions
        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged;
        UnitActionSystem.Instance.OnBusyChanged += UnitActionSystem_OnBusyChanged;

        // get the grid system from LevelGrid that is created during its Awake function
        //_gridSystem = LevelGrid.Instance.GetGridObjectGridSystem();
        SetGridSystem(LevelGrid.Instance.GetGridObjectGridSystem());

        // Create all the tiles needed for the grid system
        AddFloorTilesFromGridSystem(_gridSystem);
        AddGridVisualDefaultFromGridSystem(_gridSystem);

        // Update the action visuals for the initially selected unit and selected action
        UpdateActionVisuals();
    }

    

    private void OnDisable()
    {
        UnitActionSystem.Instance.OnSelectedActionChanged -= UnitActionSystem_OnSelectedActionChanged;
        UnitActionSystem.Instance.OnBusyChanged -= UnitActionSystem_OnBusyChanged;
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
        if (gridPosition == new GridPosition(3, 9) || gridPosition == new GridPosition(2, 4))
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
                GridPosition gridPosition = new GridPosition(x, y);
                if (_wallTilePositions.Contains(gridPosition))
                    continue;
                AddGridVisualToGridPosition(gridPosition, _gridVisualDefaulTile);
            }
        }
    }
    public void AddGridVisualToGridPosition(GridPosition gridPosition, Tile gridVisual)
    {
        _gridVisualTileMap.SetTile(new Vector3Int(gridPosition.x, gridPosition.y, 0), gridVisual);
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
    public void ShowActionVisualsFromList(List<GridPosition> gridPositions, Tile gridVisual, Color color)
    {
        foreach (GridPosition gridPosition in gridPositions)
        {
            AddActionVisualToGridPosition(gridPosition, gridVisual, color);
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

        List<GridPosition> actionVisualPositions = UnitActionSystem.Instance.GetSelectedAction().GetValidActionGridPositionList();
        
        ShowActionVisualsFromList(actionVisualPositions, _actionVisualTile, Color.white);
    }
}
