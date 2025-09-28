using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class VisibileUnitAndDiscoverer
{
    public BombRunUnit VisibileUnit; // the key of the dictionary
    public BombRunUnit Discoverer;
}
[Serializable]
public class VisibleGridPositionsByUnit
{
    public BombRunUnit Discoverer;
    public GridPosition VisibleGridPosition;
}
public class UnitVisibilityManager_BombRun : MonoBehaviour
{
    public static UnitVisibilityManager_BombRun Instance { get; private set; }

    // list of visibile units for player
    // list of visibile units for enemy
    [Header("Unit Lists")]
    [SerializeField] private List<VisibileUnitAndDiscoverer> _unitsVisibleToPlayer = new List<VisibileUnitAndDiscoverer>(); 
    [SerializeField] private List<VisibileUnitAndDiscoverer> _unitsVisibileToEnemy = new List<VisibileUnitAndDiscoverer>();

    [Header("Visibile Grid Positions")]
    [SerializeField] private List<VisibleGridPositionsByUnit> _friendlyUnitVisibileGridPositions = new List<VisibleGridPositionsByUnit>();
    [SerializeField] private List<VisibleGridPositionsByUnit> _enemyUnitVisibileGridPositions = new List<VisibleGridPositionsByUnit>();

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

        if (!CheckIfMovedUnitCanBeSeen(unit))
        {
            RemoveUnitFromVisibilityList(unit);
        }

    }
    //private async void OnAnyUnitMovedGridPosition(BombRunUnit unit)
    //{
    //    bool canMovedUnitBeSeen = await CheckIfMovedUnitCanBeSeenAsTask(unit);
    //    if (!canMovedUnitBeSeen)
    //    {
    //        RemoveUnitFromVisibilityList(unit);
    //    }
    //}
    //private async Task<bool> CheckIfMovedUnitCanBeSeenAsTask(BombRunUnit unit, BombRunUnit skipUnit = null)
    //{
    //    return await Task.Run(() => CheckIfMovedUnitCanBeSeen(unit, skipUnit));
    //}
    private bool CheckIfMovedUnitCanBeSeen(BombRunUnit unit, BombRunUnit skipUnit = null)
    {
        List<BombRunUnit> unitsToCheck = new List<BombRunUnit>();
        if (unit.IsEnemy())
        {
            unitsToCheck.AddRange(BombRunUnitManager.Instance.GetFriendlyUnitList());
        }
        else
        {
            unitsToCheck.AddRange(BombRunUnitManager.Instance.GetEnemyUnitList());
        }

        if (unitsToCheck.Count == 0)
            return false;

        if (unit == null)
            return false;


        foreach (BombRunUnit unitToCheck in unitsToCheck)
        {
            if (skipUnit != null)
            {
                if (unitToCheck == skipUnit)
                    continue;
            }
            if (unitToCheck.CanUnitSeeThisUnit(unit))
            {
                AddUnitToVisibilityList(unit, unitToCheck);
                return true;
            }
        }

        return false;
    }
    public void AddUnitToVisibilityList(BombRunUnit visibleUnit, BombRunUnit discoveringUnit)
    {
        if (visibleUnit.IsEnemy())
        {
            if (_unitsVisibleToPlayer.Exists(x => x.VisibileUnit == visibleUnit))
            {
                VisibileUnitAndDiscoverer visibileUnitAndDiscoverer = _unitsVisibleToPlayer.FirstOrDefault(x => x.VisibileUnit == visibleUnit);
                visibileUnitAndDiscoverer.Discoverer = discoveringUnit;
            }
            else
            {
                VisibileUnitAndDiscoverer visibileUnitAndDiscoverer = new VisibileUnitAndDiscoverer { VisibileUnit = visibleUnit, Discoverer = discoveringUnit };
                _unitsVisibleToPlayer.Add(visibileUnitAndDiscoverer);
                visibleUnit.SetUnitVisibility(true);
            }
        }
        else
        {
            if (_unitsVisibileToEnemy.Exists(x => x.VisibileUnit == visibleUnit))
            {
                VisibileUnitAndDiscoverer visibileUnitAndDiscoverer = _unitsVisibileToEnemy.FirstOrDefault(x => x.VisibileUnit == visibleUnit);
                visibileUnitAndDiscoverer.Discoverer = discoveringUnit;
            }
            else
            {
                VisibileUnitAndDiscoverer visibileUnitAndDiscoverer = new VisibileUnitAndDiscoverer { VisibileUnit = visibleUnit, Discoverer = discoveringUnit };
                _unitsVisibileToEnemy.Add(visibileUnitAndDiscoverer);
            }
        }
    }
    public void RemoveUnitFromVisibilityList(BombRunUnit visibleUnit)
    {

        if (visibleUnit.IsEnemy())
        {
            if (_unitsVisibleToPlayer.Exists(x => x.VisibileUnit == visibleUnit))
            {
                VisibileUnitAndDiscoverer visibileUnitAndDiscoverer = _unitsVisibleToPlayer.FirstOrDefault(x => x.VisibileUnit == visibleUnit);
                _unitsVisibleToPlayer.Remove(visibileUnitAndDiscoverer);
                visibleUnit.SetUnitVisibility(false);
            }
        }
        else
        {
            if (_unitsVisibileToEnemy.Exists(x => x.VisibileUnit == visibleUnit))
            {
                VisibileUnitAndDiscoverer visibileUnitAndDiscoverer = _unitsVisibileToEnemy.FirstOrDefault(x => x.VisibileUnit == visibleUnit);
                _unitsVisibileToEnemy.Remove(visibileUnitAndDiscoverer);
            }
        }
    }
    public void EnemyLeftObserverFOV(BombRunUnit enemyUnit, BombRunUnit observerUnit)
    {
        if (!CheckIfMovedUnitCanBeSeen(enemyUnit, observerUnit))
        {
            RemoveUnitFromVisibilityList(enemyUnit);
        }
    }
    public void UnitCompletedFOVCheck(BombRunUnit discoveringUnit, List<BombRunUnit> spottedUnits)
    {
        // remove any units this unit previously discovered but isn't in it's new list of spotted units
        List<BombRunUnit> newSpottedUnits = new List<BombRunUnit>();
        newSpottedUnits.AddRange(spottedUnits);
        List<BombRunUnit> unitsToRemove = new List<BombRunUnit>();
        List<VisibileUnitAndDiscoverer> visibileUnitsToCheck = new List<VisibileUnitAndDiscoverer>();
        if (discoveringUnit.IsEnemy())
        {
            visibileUnitsToCheck = _unitsVisibileToEnemy;
        }
        else
        {
            visibileUnitsToCheck = _unitsVisibleToPlayer;
        }

        foreach (VisibileUnitAndDiscoverer visibileUnitAndDiscoverer in visibileUnitsToCheck)
        {
            if (visibileUnitAndDiscoverer.Discoverer == discoveringUnit)
            {
                if (spottedUnits.Contains(visibileUnitAndDiscoverer.VisibileUnit))
                {
                    newSpottedUnits.Remove(visibileUnitAndDiscoverer.VisibileUnit);
                }
                else
                {
                    //RemoveUnitFromVisibilityList(visibileUnitAndDiscoverer.VisibileUnit);
                    unitsToRemove.Add(visibileUnitAndDiscoverer.VisibileUnit);
                }
            }
        }
        if (unitsToRemove.Count > 0)
        {
            foreach (BombRunUnit unitToRemove in unitsToRemove)
            {
                RemoveUnitFromVisibilityList(unitToRemove);
            }
        }
        if (newSpottedUnits.Count == 0)
            return;
        foreach (BombRunUnit spottedUnit in newSpottedUnits)
        {
            AddUnitToVisibilityList(spottedUnit, discoveringUnit);
        }
    }
}
