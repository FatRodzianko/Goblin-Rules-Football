using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnitSpawner : MonoBehaviour
{
    [SerializeField] private List<ScriptableBombRunUnit> _unitsToSpawn = new List<ScriptableBombRunUnit>();

    private void Start()
    {
        
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
