using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnitSpawner : MonoBehaviour
{
    [SerializeField] private List<ScriptableBombRunUnit> _unitsToSpawn = new List<ScriptableBombRunUnit>();

    private void Start()
    {
        int count = 0;
        foreach (ScriptableBombRunUnit unit in _unitsToSpawn)
        {
            Transform unitTransform = Instantiate(unit.UnitPrefab());
            BombRunUnit unitScript = unitTransform.GetComponent<BombRunUnit>();

            unitScript.SetUnitType(unit.UnitType());
            unitScript.SetDamageMode(unit.DamageMode());
            unitScript.SetUnitSightRange(unit.SightRange());
            unitScript.SetIsEnemy(count % 2 == 1);

            unitTransform.position = new Vector3((count + 10) * 2, (count + 10) * 2, 0f);

            unitTransform.gameObject.name = "TestSpawnUnit_" + count;

            count++;

        }
    }
}
