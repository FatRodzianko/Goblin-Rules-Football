using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnitManager : MonoBehaviour
{
    public static BombRunUnitManager Instance { get; private set; }

    private List<BombRunUnit> _unitList = new List<BombRunUnit>();
    private List<BombRunUnit> _friendlyUnitList = new List<BombRunUnit>();
    private List<BombRunUnit> _enemyUnitList = new List<BombRunUnit>();

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
}

