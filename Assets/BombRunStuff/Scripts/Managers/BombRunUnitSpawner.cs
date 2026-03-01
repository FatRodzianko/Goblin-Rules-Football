using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnitSpawner : MonoBehaviour
{
    [SerializeField] private List<ScriptableBombRunUnit> _unitsToSpawn = new List<ScriptableBombRunUnit>();
    [SerializeField] private int _numberOfUnitsSpawned = 0;

    // static events
    public static event EventHandler OnSpawnLocationSelectedForAllPlayerUnits;
    public static event EventHandler OnSpawnLocationsFinalized;
    private void Awake()
    {
        GameplayManager_BombRun.OnGameStateChanged += GameplayManager_BombRun_OnGameStateChanged;
    }    

    private void Start()
    {
        UnitActionSystem.Instance.OnSpawnLocationSelected += UnitActionSystem_OnSpawnLocationSelected;
        UnitSpawningUI.OnUnitSpawnUIStartGameButtonPressed += UnitSpawningUI_OnUnitSpawnUIStartGameButtonPressed;
    }
    private void OnDisable()
    {
        GameplayManager_BombRun.OnGameStateChanged -= GameplayManager_BombRun_OnGameStateChanged;
        UnitActionSystem.Instance.OnSpawnLocationSelected -= UnitActionSystem_OnSpawnLocationSelected;
        UnitSpawningUI.OnUnitSpawnUIStartGameButtonPressed -= UnitSpawningUI_OnUnitSpawnUIStartGameButtonPressed;
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
            OnSpawnLocationSelectedForAllPlayerUnits?.Invoke(this, EventArgs.Empty);
            Debug.Log("SpawnLocationSelected: Player has spawned all their units");
            PlayerMessageManager_BombRun.Instance.ShowGamePromptForPlayer("All Units Spawned. Finalize Positions Before Starting Game", 0f);
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

        if (isEnemy)
        {
            unitTransform.gameObject.name = "Enemy_" + unitTransform.gameObject.name;
            unitScript.SetActionDirection(new Vector2(-1, 0));
        }

        Debug.Log("BombRunUnitSpawner: SpawnUnit: " + unitTransform.gameObject.name + " : " + gridPosition);

        unitScript.InitializeBombRunUnit();
    }
    private void UnitSpawningUI_OnUnitSpawnUIStartGameButtonPressed(object sender, EventArgs e)
    {
        // Spawn enemy units first, then finalize/end the SetSpawnLocation game state
        SpawnEnemyUnits();
        OnSpawnLocationsFinalized?.Invoke(this, EventArgs.Empty);
    }
    private void SpawnEnemyUnits()
    {
        List<GridPosition> spawnPositions = LevelGrid.Instance.GetEnemySpawnPositions();
        int spawnLimit = _unitsToSpawn.Count;
        if (spawnLimit > spawnPositions.Count)
        {
            spawnLimit = spawnPositions.Count;
        }

        for (int i = 0; i < spawnLimit; i++)
        {
            SpawnUnit(_unitsToSpawn[i], spawnPositions[i], true, i);
        }
    }
}
