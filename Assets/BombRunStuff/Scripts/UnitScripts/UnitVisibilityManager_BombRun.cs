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
    public BombRunUnit LastObservedBy;
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

    [SerializeField] private List<GridPosition> _gridPositionsVisibleToPlayer = new List<GridPosition>();
    [SerializeField] private List<GridPosition> _gridPositionsVisibleToEnemy = new List<GridPosition>();

    [SerializeField] private List<Vector2> _vector2PositionsVisibleToPlayer = new List<Vector2>();
    [SerializeField] private List<Vector2> _vector2PositionsVisibleToEnemy = new List<Vector2>();

    //[Header("Invisible Unit Placeholder stuff")]
    //[SerializeField] private Transform _invisibleUnitPlaceHolderPrefab;
    //private Dictionary<BombRunUnit, GridPosition> _invisibleUnitPlaceHolderGridPositionDictionary = new Dictionary<BombRunUnit, GridPosition>();
    //private Dictionary<BombRunUnit, Transform> _invisibleUnitPlaceHolderTransformnDictionary = new Dictionary<BombRunUnit, Transform>();

    // Static events?
    public static event EventHandler<GridPosition> OnMakeGridPositionVisibleToPlayer;
    public static event EventHandler<GridPosition> OnMakeGridPositionNotVisibleToPlayer;

    public static event EventHandler<BombRunUnit> OnAnyUnitBecameVisibile;
    public static event EventHandler<BombRunUnit> OnAnyUnitBecameInVisibile;

    public event EventHandler<GridPosition> OnMakeGridPositionVisibleToEnemy;
    public event EventHandler<GridPosition> OnMakeGridPositionNotVisibleToEnemy;

    public event EventHandler<BombRunUnit> OnEnemyUnitBecameVisible;
    public event EventHandler<BombRunUnit> OnEnemyUnitBecameInVisible;

    public event EventHandler<List<GridPosition>> OnEnemyFovVisibleChanged;

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
        this.OnEnemyUnitBecameVisible += UnitVisibilityManager_BombRun_OnEnemyUnitBecameVisible;
        this.OnEnemyUnitBecameInVisible += UnitVisibilityManager_BombRun_OnEnemyUnitBecameInVisible;
    }

    

    private void OnDisable()
    {
        BombRunUnit.OnAnyUnitMovedGridPosition -= Unit_OnAnyUnitMovedGridPosition;
        this.OnEnemyUnitBecameVisible -= UnitVisibilityManager_BombRun_OnEnemyUnitBecameVisible;
        this.OnEnemyUnitBecameInVisible -= UnitVisibilityManager_BombRun_OnEnemyUnitBecameInVisible;
    }

    private void Unit_OnAnyUnitMovedGridPosition(object sender, EventArgs e)
    {
        BombRunUnit unit = sender as BombRunUnit;

        if (!CheckIfMovedUnitCanBeSeen(unit))
        {
            Debug.Log("Unit_OnAnyUnitMovedGridPosition: Removing Unit from Visibility");
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
    public void CheckIfUnitCanBeSeenByOpposingTeam(BombRunUnit unit)
    {
        if (!CheckIfMovedUnitCanBeSeen(unit))
        {
            Debug.Log("CheckIfUnitCanBeSeenByOpposingTeam: Removing Unit from Visibility");
            RemoveUnitFromVisibilityList(unit);
        }
    }
    public bool CheckIfMovedUnitCanBeSeen(BombRunUnit unit, BombRunUnit skipUnit = null)
    {
        List<BombRunUnit> unitsToCheck = GetUnitsEnemies(unit);

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
            OnEnemyUnitBecameVisible?.Invoke(this, visibleUnit);
            OnAnyUnitBecameVisibile?.Invoke(this, visibleUnit);
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
                OnEnemyUnitBecameInVisible?.Invoke(this, visibleUnit);
                OnAnyUnitBecameInVisibile?.Invoke(this, visibleUnit);
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
            Debug.Log("EnemyLeftObserverFOV: Removing Unit from Visibility");
            RemoveUnitFromVisibilityList(enemyUnit);
        }
    }
    //public void UnitCompletedFOVCheck(BombRunUnit discoveringUnit, List<BombRunUnit> spottedUnits)
    //{
    //    // remove any units this unit previously discovered but isn't in it's new list of spotted units
    //    List<BombRunUnit> newSpottedUnits = new List<BombRunUnit>();
    //    newSpottedUnits.AddRange(spottedUnits);
    //    List<BombRunUnit> unitsToRemove = new List<BombRunUnit>();
    //    List<VisibileUnitAndDiscoverer> visibileUnitsToCheck = new List<VisibileUnitAndDiscoverer>();
    //    if (discoveringUnit.IsEnemy())
    //    {
    //        visibileUnitsToCheck = _unitsVisibileToEnemy;
    //    }
    //    else
    //    {
    //        visibileUnitsToCheck = _unitsVisibleToPlayer;
    //    }

    //    foreach (VisibileUnitAndDiscoverer visibileUnitAndDiscoverer in visibileUnitsToCheck)
    //    {
    //        if (visibileUnitAndDiscoverer.Discoverer == discoveringUnit)
    //        {
    //            if (spottedUnits.Contains(visibileUnitAndDiscoverer.VisibileUnit))
    //            {
    //                newSpottedUnits.Remove(visibileUnitAndDiscoverer.VisibileUnit);
    //            }
    //            else
    //            {
    //                //RemoveUnitFromVisibilityList(visibileUnitAndDiscoverer.VisibileUnit);
    //                unitsToRemove.Add(visibileUnitAndDiscoverer.VisibileUnit);
    //            }
    //        }
    //    }
    //    if (unitsToRemove.Count > 0)
    //    {
    //        foreach (BombRunUnit unitToRemove in unitsToRemove)
    //        {
    //            RemoveUnitFromVisibilityList(unitToRemove);
    //        }
    //    }
    //    if (newSpottedUnits.Count == 0)
    //        return;
    //    foreach (BombRunUnit spottedUnit in newSpottedUnits)
    //    {
    //        AddUnitToVisibilityList(spottedUnit, discoveringUnit);
    //    }
    //}
    public void UnitUpdatedVisibleGridPositions(BombRunUnit discoveringUnit, List<GridPosition> newGridPositions)
    {
        List<VisibleGridPositionsByUnit> currentVisibleGridPositions = new List<VisibleGridPositionsByUnit>();
        if (discoveringUnit.IsEnemy())
        {
            currentVisibleGridPositions = _enemyUnitVisibileGridPositions;
        }
        else
        {
            currentVisibleGridPositions = _friendlyUnitVisibileGridPositions;
        }
        // Find the positions that the discovering unit is the LastObservedByUnit
        // Check those positions to see if any teammates can see them
        // if they can, update that position's LastObservedBy
        // if no teammates can see the position, add to RemoveFromVisibleListw
        List<GridPosition> lastObservedByDiscoverer = currentVisibleGridPositions.FindAll(x => x.LastObservedBy == discoveringUnit).Select(o => o.VisibleGridPosition).ToList();
        List<GridPosition> removeFromVisibleList = new List<GridPosition>();
        if (lastObservedByDiscoverer.Count > 0)
        {
            List<BombRunUnit> teammates = GetUnitsTeammates(discoveringUnit);
            foreach (GridPosition gridPosition in lastObservedByDiscoverer)
            {
                bool seenByTeammate = false;
                foreach (BombRunUnit teammate in teammates)
                {
                    if (teammate == discoveringUnit)
                        continue;

                    if (teammate.CanUnitSeeThisPosition(gridPosition))
                    {
                        currentVisibleGridPositions.FirstOrDefault(x => x.VisibleGridPosition == gridPosition).LastObservedBy = teammate;
                        seenByTeammate = true;
                        break;
                    }
                }
                if (seenByTeammate)
                {
                    removeFromVisibleList.Add(gridPosition);
                }
            }
        }

        
    }
    public void UpdateTeamsVisibleGridPositions(BombRunUnit unit, List<GridPosition> unitsVisibleGridPositions, List<GridPosition> gridRadiusNotVisibile)
    {
        List<BombRunUnit> teammates = GetUnitsTeammates(unit);
        List<GridPosition> newGridPositions = new List<GridPosition>();
        newGridPositions.AddRange(unitsVisibleGridPositions);

        List<GridPosition> previousGridPositions = GetTeamsVisibileGridPositions(unit);

        foreach (BombRunUnit teammate in teammates)
        {
            if (teammate == unit)
            {
                continue;
            }
            newGridPositions.AddRange(teammate.GetUnitsVisibileGridPositions());
        }

        List<GridPosition> newGridPositionsUnique = newGridPositions.Distinct().ToList();
        List<GridPosition> teamVisibilityList = new List<GridPosition>();
        teamVisibilityList.AddRange(GetTeamsVisibileGridPositions(unit));

        // add and remove lists to send to the fog of war tilemap manager to update tiles as needed
        List<GridPosition> removeFromVisibleGridPositions = new List<GridPosition>();
        List<GridPosition> addToVisibileGridPosition = new List<GridPosition>();
        foreach (GridPosition gridPosition in teamVisibilityList)
        {
            if (!newGridPositionsUnique.Contains(gridPosition))
            {
                removeFromVisibleGridPositions.Add(gridPosition);
            }
        }
        foreach (GridPosition gridPosition in newGridPositionsUnique)
        {
            if (!teamVisibilityList.Contains(gridPosition))
            {
                addToVisibileGridPosition.Add(gridPosition);
            }
        }
        foreach (GridPosition gridPosition in gridRadiusNotVisibile)
        {
            if (!teamVisibilityList.Contains(gridPosition))
            {
                removeFromVisibleGridPositions.Add(gridPosition);
            }
        }

        if (unit.IsEnemy())
        {
            _gridPositionsVisibleToEnemy.Clear();
            _gridPositionsVisibleToEnemy.AddRange(newGridPositionsUnique);
            _vector2PositionsVisibleToEnemy.Clear();
            foreach (GridPosition gridPosition in newGridPositionsUnique)
            {
                _vector2PositionsVisibleToEnemy.Add(new Vector2(gridPosition.x, gridPosition.y));
            }

        }
        else
        {
            _gridPositionsVisibleToPlayer.Clear();
            _gridPositionsVisibleToPlayer.AddRange(newGridPositionsUnique);
            _vector2PositionsVisibleToPlayer.Clear();
            foreach (GridPosition gridPosition in newGridPositionsUnique)
            {
                _vector2PositionsVisibleToPlayer.Add(new Vector2(gridPosition.x, gridPosition.y));
            }
            // update the tile maps by call event on the remove list
            foreach (GridPosition gridPosition in removeFromVisibleGridPositions)
            {
                OnMakeGridPositionNotVisibleToPlayer?.Invoke(this, gridPosition);
            }
            // update the tile maps by call event on the add list
            foreach (GridPosition gridPosition in addToVisibileGridPosition)
            {
                OnMakeGridPositionVisibleToPlayer?.Invoke(this, gridPosition);
            }
            //UpdateEnemyUnitVisibility(unit);
        }
        UpdateEnemyFOVTiles();
    }
    public void UpdateEnemyUnitVisibility(BombRunUnit unit)
    {
        List<BombRunUnit> enemyUnits = new List<BombRunUnit>();
        enemyUnits.AddRange(GetUnitsEnemies(unit));

        List<GridPosition> enemyVisibleGridPositionToRemove = new List<GridPosition>();
        List<GridPosition> enemyVisibleGridPositionToAdd = new List<GridPosition>();

        foreach (BombRunUnit enemy in enemyUnits)
        {
            if (enemy.GetUnitVisibility())
            {
                enemyVisibleGridPositionToAdd.AddRange(enemy.GetUnitsVisibileGridPositions());
            }
            else
            {
                enemyVisibleGridPositionToRemove.AddRange(enemy.GetUnitsVisibileGridPositions());
            }
        }

        //// update the tile maps by call event on the remove list
        foreach (GridPosition gridPosition in enemyVisibleGridPositionToRemove)
        {
            OnMakeGridPositionNotVisibleToEnemy?.Invoke(this, gridPosition);
        }
        // update the tile maps by call event on the add list
        foreach (GridPosition gridPosition in enemyVisibleGridPositionToAdd)
        {
            OnMakeGridPositionVisibleToEnemy?.Invoke(this, gridPosition);
        }

    }
    public void UpdateEnemyFOVTiles()
    {
        List<BombRunUnit> enemyUnits = new List<BombRunUnit>();
        // hard coded right now to just be the enemy list, and not the opposite "team" of the player?
        enemyUnits.AddRange(BombRunUnitManager.Instance.GetEnemyUnitList());
        List<GridPosition> enemyFovPositionsPlayerCanSee = new List<GridPosition>();
        foreach (BombRunUnit enemy in enemyUnits)
        {
            if (!enemy.GetUnitVisibility())
            {
                continue;
            }

            foreach (GridPosition gridPosition in enemy.GetUnitsVisibileGridPositions())
            {
                if (_gridPositionsVisibleToPlayer.Contains(gridPosition))
                {
                    enemyFovPositionsPlayerCanSee.Add(gridPosition);
                }
            }
        }
        List<GridPosition> enemyFovPositionsPlayerCanSeeUnique = enemyFovPositionsPlayerCanSee.Distinct().ToList();
        OnEnemyFovVisibleChanged?.Invoke(this, enemyFovPositionsPlayerCanSeeUnique);
    }
    private void UnitVisibilityManager_BombRun_OnEnemyUnitBecameInVisible(object sender, BombRunUnit unit)
    {
        Debug.Log("UnitVisibilityManager_BombRun_OnEnemyUnitBecameInVisible: unit: " + unit.name + " became invisible at: " + unit.GetGridPosition());
        //Debug.Break();

        //if (_invisibleUnitPlaceHolderDictionary.ContainsKey(unit))
        //{

        //}

        SpawnInvisibleUnitPlaceHolder(unit);

    }
    private void SpawnInvisibleUnitPlaceHolder(BombRunUnit unit)
    {
        //if (unit == null)
        //{
        //    return;
        //}

        //// Get Invisible Unit spawn position
        //Vector3 spawnPosition = LevelGrid.Instance.GetWorldPosition(unit.GetGridPosition());
        //if (unit.GetUnitAnimationState() == UnitAnimationState.Moving)
        //{
        //    spawnPosition = unit.GetWorldPosition();
        //}

        //// spawn the invisilbe unit placeholder
        //Transform invibleUnitPlaceHolderObject = Instantiate(_invisibleUnitPlaceHolderPrefab, spawnPosition, Quaternion.identity, this.transform);

        //// update the invisible unit placeholder
        //BombRunUnit_InvisibleUnitPlaceHolder invisibleUnitPlaceHolderScript = invibleUnitPlaceHolderObject.GetComponent<BombRunUnit_InvisibleUnitPlaceHolder>();
        //invisibleUnitPlaceHolderScript.InitializeInvisibleUnitPlaceHolder(unit);

        //// save placeholder to dictionaries so it can be deleted as needed?
        //_invisibleUnitPlaceHolderTransformnDictionary.Add(unit, invibleUnitPlaceHolderObject);
        //_invisibleUnitPlaceHolderGridPositionDictionary.Add(unit, unit.GetGridPosition());

    }
    private void UpdateInvisibleUnitPlaceHolders()
    {
        
    }
    private void UnitVisibilityManager_BombRun_OnEnemyUnitBecameVisible(object sender, BombRunUnit unit)
    {
        RemoveFromInvisibleUnitPlaceHolderDictionary(unit);
    }
    private void RemoveFromInvisibleUnitPlaceHolderDictionary(BombRunUnit unit)
    {
        //Transform unitPlaceHolderToDelete = null;
        //if (_invisibleUnitPlaceHolderTransformnDictionary.ContainsKey(unit))
        //{
        //    unitPlaceHolderToDelete = _invisibleUnitPlaceHolderTransformnDictionary[unit];
        //    _invisibleUnitPlaceHolderTransformnDictionary.Remove(unit);
        //}
        //if (_invisibleUnitPlaceHolderGridPositionDictionary.ContainsKey(unit))
        //{
        //    _invisibleUnitPlaceHolderGridPositionDictionary.Remove(unit);
        //}

        //if (unitPlaceHolderToDelete != null)
        //{
        //    GameObject.Destroy(unitPlaceHolderToDelete.gameObject);
        //}
    }
    //public void UpdateTeamsVisibleGridPositions(BombRunUnit unit, List<GridPosition> newVisibleGridPositions, List<GridPosition> previousVisibleGridPositions)
    //{

    //    // get team's visibleGridPositionList
    //    List<GridPosition> teamVisibilityList = new List<GridPosition>();
    //    teamVisibilityList.AddRange(GetTeamsVisibileGridPositions(unit));// GetTeamsVisibileGridPositions(unit);
    //    Debug.Log("UpdateTeamsVisibleGridPositions: " + unit + " newVisibleGridPosition.Count: " + newVisibleGridPositions.Count() + " previousVisibleGridPosition.Count: " + previousVisibleGridPositions.Count() + " teamVisibilityList.Count: " + teamVisibilityList.Count());
    //    List<GridPosition> removeFromVisibleGridPositions = new List<GridPosition>();
    //    removeFromVisibleGridPositions.AddRange(previousVisibleGridPositions);
    //    List<GridPosition> addToVisibileGridPosition = new List<GridPosition>();
    //    // Go through each previousVisibleGridPosition, check if in newVisibleGridPosition. If it is NOT, add to removeFromVisibleGridPositions
    //    foreach (GridPosition gridPosition in previousVisibleGridPositions)
    //    {
    //        // // if position from previousVisibleGridPosition IS in newVisibleGridPosition, remove from newVisibleGridPosition since we know those are already in the team's visibleGridPosition list?
    //        if (newVisibleGridPositions.Contains(gridPosition))
    //        {
    //            newVisibleGridPositions.Remove(gridPosition);
    //            removeFromVisibleGridPositions.Remove(gridPosition);
    //        }            
    //    }

    //    // check each removeFromVisibleGridPositions against each teammate's HasThisUnitSeenThisGridPosition. If true, remove from removeFromVisibleGridPositions
    //    List<GridPosition> checkIfTeammatesCanSee = new List<GridPosition>();
    //    checkIfTeammatesCanSee.AddRange(removeFromVisibleGridPositions);
    //    List<BombRunUnit> teammates = GetUnitsTeammates(unit);
    //    foreach (GridPosition gridPosition in checkIfTeammatesCanSee)
    //    {
    //        foreach (BombRunUnit teammate in teammates)
    //        {
    //            if (teammate == unit)
    //                continue;
    //            if (teammate.CanUnitSeeThisPosition(gridPosition))
    //            {
    //                removeFromVisibleGridPositions.Remove(gridPosition);
    //                break;
    //            }
    //        }
    //    }
    //    // next, go through newVisibleGridPosition and check against team's visibleGridPositionList
    //    foreach (GridPosition gridPosition in newVisibleGridPositions)
    //    {
    //        // // if it is already in visibleGridPositionList, ignore
    //        // // if it is NOT in visibleGridPositionList, add to addToVisibileGridPosition
    //        if (!teamVisibilityList.Contains(gridPosition))
    //        {
    //            addToVisibileGridPosition.Add(gridPosition);
    //        }
    //    }

    //    // remove removeFromVisibleGridPositions from team's visibleGridPositionList
    //    foreach (GridPosition gridPosition in removeFromVisibleGridPositions)
    //    {
    //        teamVisibilityList.Remove(gridPosition);
    //    }
    //    // add addToVisibileGridPosition to team's visibleGridPositionList
    //    foreach (GridPosition gridPosition in addToVisibileGridPosition)
    //    {
    //        teamVisibilityList.Add(gridPosition);
    //    }
    //    if (unit.IsEnemy())
    //    {
    //        _gridPositionsVisibleToEnemy.Clear();
    //        _gridPositionsVisibleToEnemy.AddRange(teamVisibilityList);
    //        _vector2PositionsVisibleToEnemy.Clear();
    //        foreach (GridPosition gridPosition in teamVisibilityList)
    //        {
    //            _vector2PositionsVisibleToEnemy.Add(new Vector2(gridPosition.x, gridPosition.y));
    //        }
    //    }
    //    else
    //    {
    //        _gridPositionsVisibleToPlayer.Clear();
    //        _gridPositionsVisibleToPlayer.AddRange(teamVisibilityList);
    //        _vector2PositionsVisibleToPlayer.Clear();
    //        foreach (GridPosition gridPosition in teamVisibilityList)
    //        {
    //            _vector2PositionsVisibleToPlayer.Add(new Vector2(gridPosition.x, gridPosition.y));
    //        }
    //    }
    //}
    private void UpdateTileMapVisibility(BombRunUnit unit, List<GridPosition> makeNotVisible, List<GridPosition> makeVisible)
    {
        // make tiles not visible
    }
    public bool CanUnitTeamSeeGridPosition(BombRunUnit unit, GridPosition gridPosition)
    {
        return GetTeamsVisibileGridPositions(unit).Contains(gridPosition);
    }
    private List<GridPosition> GetTeamsVisibileGridPositions(BombRunUnit unit)
    {
        if (unit.IsEnemy())
        {
            return _gridPositionsVisibleToEnemy;
        }
        else
        {
            return _gridPositionsVisibleToPlayer;
        }
    }
    private List<BombRunUnit> GetUnitsTeammates(BombRunUnit unit)
    {
        if (unit.IsEnemy())
        {
            return BombRunUnitManager.Instance.GetEnemyUnitList();
        }
        else
        {
            return BombRunUnitManager.Instance.GetFriendlyUnitList();
        }
    }
    private List<BombRunUnit> GetUnitsEnemies(BombRunUnit unit)
    {
        if (unit.IsEnemy())
        {
            return BombRunUnitManager.Instance.GetFriendlyUnitList();
        }
        else
        {
            return BombRunUnitManager.Instance.GetEnemyUnitList();
        }
    }
}
