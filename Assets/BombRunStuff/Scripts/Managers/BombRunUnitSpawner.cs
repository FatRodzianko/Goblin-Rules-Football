using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnitSpawner : MonoBehaviour
{
    [SerializeField] private List<ScriptableBombRunUnit> _unitsToSpawn = new List<ScriptableBombRunUnit>();
    [SerializeField] private int _numberOfUnitsSpawned = 0;

    // static events
    public static event EventHandler SpawnLocationSelectedForAllPlayerUnits;
    private void Awake()
    {
        GameplayManager_BombRun.OnGameStateChanged += GameplayManager_BombRun_OnGameStateChanged;
    }    

    private void Start()
    {
        UnitActionSystem.Instance.OnSpawnLocationSelected += UnitActionSystem_OnSpawnLocationSelected;
    }
    private void OnDisable()
    {
        GameplayManager_BombRun.OnGameStateChanged -= GameplayManager_BombRun_OnGameStateChanged;
        UnitActionSystem.Instance.OnSpawnLocationSelected -= UnitActionSystem_OnSpawnLocationSelected;
    }

    

    private void GameplayManager_BombRun_OnGameStateChanged(object sender, GameState_BombRun gameState)
    {
        if (gameState == GameState_BombRun.SetSpawnLocation)
        {
            PromptPlayerToSpawnUnits();
        }
    }
    private void PromptPlayerToSpawnUnits()
    {
        PlayerMessageManager_BombRun.Instance.ShowGamePromptForPlayer("Choose Spawn Location For Your Units", 0f);
    }
    public void SpawnUnits()
    {
        Debug.Log("BombRunUnitSpawner: SpawnUnits");
        int count = 0;
        foreach (ScriptableBombRunUnit unit in _unitsToSpawn)
        {
            GridPosition gridPosition = new GridPosition((count + 10), (count + 10));
            SpawnUnit(unit, gridPosition, count % 2 == 1, count);
            count++;
        }
    }
    private void UnitActionSystem_OnSpawnLocationSelected(object sender, GridPosition gridPosition)
    {
        if (!DoesPlayerHaveMoreUnitsToSpawn())
        {
            return;
        }
        SpawnLocationSelected(gridPosition);
    }
    public bool DoesPlayerHaveMoreUnitsToSpawn()
    {
        return _numberOfUnitsSpawned - (_unitsToSpawn.Count - 1) <= 0;
    }
    public void SpawnLocationSelected(GridPosition gridPosition)
    {
        SpawnUnit(_unitsToSpawn[_numberOfUnitsSpawned], gridPosition, false, _numberOfUnitsSpawned);
        _numberOfUnitsSpawned++;

        if (!DoesPlayerHaveMoreUnitsToSpawn())
        {
            SpawnLocationSelectedForAllPlayerUnits?.Invoke(this, EventArgs.Empty);
            Debug.Log("SpawnLocationSelected: Player has spawned all their units");
        }
    }
    private void SpawnUnit(ScriptableBombRunUnit unit, GridPosition gridPosition, bool isEnemy, int count = 0)
    {
        
        Transform unitTransform = Instantiate(unit.UnitPrefab());
        BombRunUnit unitScript = unitTransform.GetComponent<BombRunUnit>();

        unitScript.SetUnitType(unit.UnitType());
        unitScript.SetDamageMode(unit.DamageMode());
        unitScript.SetUnitSightRange(unit.SightRange());
        unitScript.SetIsEnemy(isEnemy);

        unitTransform.position = LevelGrid.Instance.GetWorldPosition(gridPosition);
        unitTransform.gameObject.name = "TestSpawnUnit_" + count;

        Debug.Log("BombRunUnitSpawner: SpawnUnit: " + unitTransform.gameObject.name + " : " + gridPosition);

        unitScript.InitializeBombRunUnit();
    }
}
