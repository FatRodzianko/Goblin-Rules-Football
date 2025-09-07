using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnitManager : MonoBehaviour
{
    public static BombRunUnitManager Instance { get; private set; }

    private List<BombRunUnit> _unitList = new List<BombRunUnit>();
    [SerializeField] private List<BombRunUnit> _friendlyUnitList = new List<BombRunUnit>();
    [SerializeField] private List<BombRunUnit> _enemyUnitList = new List<BombRunUnit>();

    [SerializeField] private BombRunUnitActionValueManager _bombRunUnitActionValueManager;

    private void Awake()
    {
        MakeInstance();
    }
    void MakeInstance()
    {
        if (Instance != null)
        {
            Debug.Log("MakeInstance: more than one BombRunUnitManager. Destroying...");
            Destroy(this);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        BombRunUnit.OnAnyUnitSpawned += BombRunUnit_OnAnyUnitSpawned;
        BombRunUnit.OnAnyUnitDied += BombRunUnit_OnAnyUnitDied;
    }
    private void OnDisable()
    {
        BombRunUnit.OnAnyUnitSpawned -= BombRunUnit_OnAnyUnitSpawned;
        BombRunUnit.OnAnyUnitDied -= BombRunUnit_OnAnyUnitDied;
    }
    public void InitializeBombRunUnits()
    {
        // Place holder for now. In a real game the units would be spawned from level parameters. Right now, just find the units that exist in the scene, then "initialize" them
        //SpawnUnits();
        FindAndInitializeAllUnits();
    }
    private void FindAndInitializeAllUnits()
    {
        GameObject[] units = GameObject.FindGameObjectsWithTag("Goblin");
        if (units.Length == 0)
        {
            Debug.LogError("BombRunUnitManager: FindAndInitializeAllUnits: No units found?");
            return;
        }
        for (int i = 0; i < units.Length; i++)
        {
            units[i].GetComponent<BombRunUnit>().InitializeBombRunUnit();
        }
    }
    private void BombRunUnit_OnAnyUnitSpawned(object sender, EventArgs e)
    {
        BombRunUnit unit = sender as BombRunUnit;

        if (!_unitList.Contains(unit))
        {
            _unitList.Add(unit);
        }

        if (unit.IsEnemy())
        {
            if (!_enemyUnitList.Contains(unit))
            {
                _enemyUnitList.Add(unit);
            }
        }
        else
        {
            if (!_friendlyUnitList.Contains(unit))
            {
                _friendlyUnitList.Add(unit);
            }
        }
    }
    private void BombRunUnit_OnAnyUnitDied(object sender, EventArgs e)
    {
        BombRunUnit unit = sender as BombRunUnit;

        if (_unitList.Contains(unit))
        {
            _unitList.Remove(unit);
        }

        if (unit.IsEnemy())
        {
            if (_enemyUnitList.Contains(unit))
            {
                _enemyUnitList.Remove(unit);
            }
        }
        else
        {
            if (_friendlyUnitList.Contains(unit))
            {
                _friendlyUnitList.Remove(unit);
            }
        }
    }
    public List<BombRunUnit> GetUnitList()
    {
        return _unitList;
    }
    public List<BombRunUnit> GetFriendlyUnitList()
    {
        return _friendlyUnitList;
    }
    public List<BombRunUnit> GetEnemyUnitList()
    {
        return _enemyUnitList;
    }
    public bool IsUnitAnEnemy(BombRunUnit unit)
    {
        return _enemyUnitList.Contains(unit);
    }
    public int GetUnitBodyPartActionValue(UnitType unitType, BodyPart bodyPart)
    {
        return _bombRunUnitActionValueManager.GetUnitBodyPartActionValue(unitType, bodyPart);
    }
}

