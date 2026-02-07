using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InitializeLevelManager_BombRun : MonoBehaviour
{
    public static InitializeLevelManager_BombRun Instance { get; private set; }
    // Control the order of objects/other managers initializing when starting the level to make things more consistent and predictable at the beginning

    // static events
    public static event EventHandler OnInitializationBegin;
    private void Awake()
    {
        MakeInstance();
    }

    void MakeInstance()
    {
        if (Instance != null)
        {
            Debug.Log("MakeInstance: more than one InitializeLevelManager_BombRun. Destroying...");
            Destroy(this);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        OnInitializationBegin?.Invoke(this, EventArgs.Empty);
        // create the grid system
        LevelGrid.Instance.CreateLevelGrid();

        Debug.Log("InitializeLevelManager_BombRun: LevelGrid GridSystem created");

        

        // Set the tiles on the tile maps
        //LevelGrid.Instance.SetTilesOnTileMaps();
        BombRunTileMapManager.Instance.SetGridSystem(LevelGrid.Instance.GetGridObjectGridSystem());
        BombRunTileMapManager.Instance.AddFloorTilesFromGridSystem(LevelGrid.Instance.GetGridObjectGridSystem());
        Debug.Log("InitializeLevelManager_BombRun: BombRunTileMapManager floor tiles added");
        BombRunTileMapManager.Instance.AddGridVisualDefaultFromGridSystem(LevelGrid.Instance.GetGridObjectGridSystem());
        Debug.Log("InitializeLevelManager_BombRun: BombRunTileMapManager grid visuals added");

        // Create the pathfinding grid
        PathFinding.Instance.Setup(LevelGrid.Instance.GetGridWidth(), LevelGrid.Instance.GetGridHeight(), LevelGrid.Instance.GetGridCellSize());
        LevelGrid.Instance.SetPathFindingDistanceMultiplier(PathFinding.Instance.GetPathFindingDistanceMultiplier());

        // InitializeActionGridVisualManager()
        ActionGridVisualManager.Instance.InitializeActionGridVisualManager();
        Debug.Log("InitializeLevelManager_BombRun: ActionGridVisualManager action grid visuals initialized");

        // Prompt player to choose pawn location for units
        PlayerMessageManager_BombRun.Instance.PromptPlayerToChooseUnitSpawnLocations();

        // "Spawn" all the units for the map
        BombRunUnitManager.Instance.InitializeBombRunUnits();

        // Update visibility for all units? Calculate their current FOVs and what they can see?
        UnitVisibilityManager_BombRun.Instance.InitializeFOVForAllUnits(BombRunUnitManager.Instance.GetAllUnitList());
    }
}
