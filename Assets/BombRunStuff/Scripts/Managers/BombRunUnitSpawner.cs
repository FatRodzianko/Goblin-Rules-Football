using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCreateUIObjectForUnitToSpawnEventArgs : EventArgs
{
    public ScriptableBombRunUnit ScriptableBombRunUnit;
    public int Index;
}

public class BombRunUnitSpawner : MonoBehaviour
{
    [SerializeField] private List<ScriptableBombRunUnit> _unitsToSpawn = new List<ScriptableBombRunUnit>();
    [SerializeField] private List<int> _unitIndexesAlreadySpawned = new List<int>();
    [SerializeField] private int _numberOfUnitsSpawned = 0;
    [SerializeField] private int _unitToSpawnIndex = 0;

    // static events
    public static event EventHandler OnSpawnLocationSelectedForAllPlayerUnits;
    public static event EventHandler OnSpawnLocationsFinalized;

    // for creating UI stuff?
    public static event EventHandler<OnCreateUIObjectForUnitToSpawnEventArgs> OnCreateUIObjectForUnitToSpawn;
    public static event EventHandler<int> OnSpawnedUnitAtIndex;

    private void Awake()
    {
        GameplayManager_BombRun.OnGameStateChanged += GameplayManager_BombRun_OnGameStateChanged;
    }    

    private void Start()
    {
        UnitActionSystem.Instance.OnSpawnLocationSelected += UnitActionSystem_OnSpawnLocationSelected;
        UnitSpawningUI.OnUnitSpawnUIStartGameButtonPressed += UnitSpawningUI_OnUnitSpawnUIStartGameButtonPressed;
        UnitSpawningButtonUI.OnPlayerClickedUnitSpawnButton += UnitSpawningButtonUI_OnPlayerClickedUnitSpawnButton;

        
    }
    private void OnDisable()
    {
        GameplayManager_BombRun.OnGameStateChanged -= GameplayManager_BombRun_OnGameStateChanged;
        UnitActionSystem.Instance.OnSpawnLocationSelected -= UnitActionSystem_OnSpawnLocationSelected;
        UnitSpawningUI.OnUnitSpawnUIStartGameButtonPressed -= UnitSpawningUI_OnUnitSpawnUIStartGameButtonPressed;
        UnitSpawningButtonUI.OnPlayerClickedUnitSpawnButton -= UnitSpawningButtonUI_OnPlayerClickedUnitSpawnButton;

        UnitActionSystem.Instance.OnPlayerRightClicked -= UnitActionSystem_OnPlayerRightClicked;
        UnitActionSystem.Instance.OnSelectedUnitChanged -= UnitActionSystem_OnSelectedUnitChanged;
    }

    

    private void GameplayManager_BombRun_OnGameStateChanged(object sender, GameState_BombRun gameState)
    {
        if (gameState == GameState_BombRun.SetSpawnLocation)
        {
            PromptPlayerToSpawnUnits();
            CreateUnitSpawnUIObjects();
            UnitActionSystem.Instance.OnPlayerRightClicked += UnitActionSystem_OnPlayerRightClicked;
            UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged;
        }
        else
        {
            UnitActionSystem.Instance.OnPlayerRightClicked -= UnitActionSystem_OnPlayerRightClicked;
            UnitActionSystem.Instance.OnSelectedUnitChanged -= UnitActionSystem_OnSelectedUnitChanged;
        }
    }

    

    private void PromptPlayerToSpawnUnits()
    {
        PlayerMessageManager_BombRun.Instance.ShowGamePromptForPlayer("Choose Spawn Location For Your Units", 0f);
    }
    private void CreateUnitSpawnUIObjects()
    {
        int index = 0;
        foreach (ScriptableBombRunUnit unit in _unitsToSpawn)
        {
            OnCreateUIObjectForUnitToSpawn?.Invoke(this, new OnCreateUIObjectForUnitToSpawnEventArgs
            {
                ScriptableBombRunUnit = unit,
                Index = index
            });

            index++;
        }
    }
    //public void SpawnUnits()
    //{
    //    Debug.Log("BombRunUnitSpawner: SpawnUnits");
    //    int count = 0;
    //    foreach (ScriptableBombRunUnit unit in _unitsToSpawn)
    //    {
    //        GridPosition gridPosition = new GridPosition((count + 10), (count + 10));
    //        SpawnUnit(unit, gridPosition, count % 2 == 1, count);
    //        count++;
    //    }
    //}
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
        if (_unitToSpawnIndex < 0)
        {
            return;
        }
        if (_unitToSpawnIndex > _unitsToSpawn.Count - 1)
        {
            return;
        }
        if (_unitIndexesAlreadySpawned.Contains(_unitToSpawnIndex))
        {
            return;
        }

        SpawnUnit(_unitsToSpawn[_unitToSpawnIndex], gridPosition, false, _numberOfUnitsSpawned);
        _numberOfUnitsSpawned++;
        _unitIndexesAlreadySpawned.Add(_unitToSpawnIndex);
        OnSpawnedUnitAtIndex?.Invoke(this, _unitToSpawnIndex);

        SelectUnitToSpawnByIndex(-1);

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
        unitScript.SetUnitMaxMoveDistance(unit.MaxMoveDistance());
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
    private void UnitSpawningButtonUI_OnPlayerClickedUnitSpawnButton(object sender, int index)
    {
        SelectUnitToSpawnByIndex(index);
    }
    public void SelectUnitToSpawnByIndex(int index)
    {
        if (index > _unitsToSpawn.Count - 1)
            return;

        _unitToSpawnIndex = index;
    }
    private void UnitActionSystem_OnPlayerRightClicked(object sender, EventArgs e)
    {
        Debug.Log("BombRunUnitSpawner: UnitActionSystem_OnPlayerRightClicked");
        SelectUnitToSpawnByIndex(-1);
    }
    private void UnitActionSystem_OnSelectedUnitChanged(object sender, BombRunUnit unit)
    {
        if (unit == null)
        {
            return;
        }
        SelectUnitToSpawnByIndex(-1);
    }
}
