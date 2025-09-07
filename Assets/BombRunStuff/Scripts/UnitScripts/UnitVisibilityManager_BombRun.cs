using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitVisibilityManager_BombRun : MonoBehaviour
{
    public static UnitVisibilityManager_BombRun Instance { get; private set; }

    // list of visibile units for player
    // list of visibile units for enemy
    [Header("Unit Lists")]
    [SerializeField] private List<BombRunUnit> _visibileToPlayer = new List<BombRunUnit>();
    [SerializeField] private List<BombRunUnit> _visibileToEnemy = new List<BombRunUnit>();

    private void Awake()
    {
        MakeInstance();
    }
    void MakeInstance()
    {
        if (Instance != null)
        {
            Debug.Log("MakeInstance: more than one UnitVisibilityManager_BombRun. Destroying...");
            Destroy(this);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        // Change this so the event is fired from the BombRunUnit script and the unit is pased in the event? So the UnitVisibilityManager script will know what unit actually moved, and use their grid position for the checks?
       BombRunUnit.OnAnyUnitMovedGridPosition += Unit_OnAnyUnitMovedGridPosition;
    }
    private void OnDisable()
    {
        BombRunUnit.OnAnyUnitMovedGridPosition -= Unit_OnAnyUnitMovedGridPosition;
    }

    private void Unit_OnAnyUnitMovedGridPosition(object sender, EventArgs e)
    {
        BombRunUnit unit = sender as BombRunUnit;
        if (unit.IsEnemy())
        {
            CheckIfMovedUnitCanBeSeen(unit, BombRunUnitManager.Instance.GetFriendlyUnitList());
        }
        else
        {
            CheckIfMovedUnitCanBeSeen(unit, BombRunUnitManager.Instance.GetEnemyUnitList());
        }
    }
    private bool CheckIfMovedUnitCanBeSeen(BombRunUnit unit, List<BombRunUnit> unitsToCheck)
    {
        if (unitsToCheck.Count == 0)
            return false;

        if (unit == null)
            return false;


        foreach (BombRunUnit unitToCheck in unitsToCheck)
        {
            if (unitToCheck.CanUnitSeeThisPosition(unit.GetGridPosition()))
            {
                AddUnitToVisibilityList(unit);
                return true;
            }
        }

        // no units could see the moved unit
        RemoveUnitFromVisibilityList(unit);
        return false;
    }
    private void AddUnitToVisibilityList(BombRunUnit unit)
    {
        if (unit.IsEnemy())
        {
            if (!_visibileToPlayer.Contains(unit))
            {
                _visibileToPlayer.Add(unit);
            }
        }
        else
        {
            if (!_visibileToEnemy.Contains(unit))
            {
                _visibileToEnemy.Add(unit);
            }
        }
    }
    private void RemoveUnitFromVisibilityList(BombRunUnit unit)
    {
        if (unit.IsEnemy())
        {
            if (_visibileToPlayer.Contains(unit))
            {
                _visibileToPlayer.Remove(unit);
            }
        }
        else
        {
            if (_visibileToEnemy.Contains(unit))
            {
                _visibileToEnemy.Remove(unit);
            }
        }
    }
}
