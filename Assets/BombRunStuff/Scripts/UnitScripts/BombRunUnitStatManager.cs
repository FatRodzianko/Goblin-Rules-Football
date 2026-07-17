using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    None,
    MaxMoveDistance,
    SightDistance,
    FOV,
    HearingSensitivity
}
[Serializable]
public class ActionModifyingStat
{
    public BaseAction Action;
    public StatType StatType;
    public float StatModifier;

    public ActionModifyingStat(BaseAction action, StatType statType, float statModifier)
    {
        Action = action;
        StatType = statType;
        StatModifier = statModifier;
    }
}
[Serializable]
public class BombRunUnitStatManager 
{
    private BombRunUnit _unit;
    [SerializeField] private ScriptableBombRunUnitBaseStats _baseStats;

    [SerializeField] private List<ActionModifyingStat> _actionsModifyingStatsAdditive = new List<ActionModifyingStat>();
    [SerializeField] private List<ActionModifyingStat> _actionsModifyingStatsMultiply = new List<ActionModifyingStat>();

    // events
    public EventHandler OnMaxMovementDistanceChanged;
    public EventHandler OnSightDistanceChanged;
    public EventHandler OnFOVChanged;
    public EventHandler OnHearingSensitivityChanged;

    // updating stats trackers
    private bool _maxMoveDistanceWasUpdated = true;

    // cached stat values
    private int _maxMoveDistanceCached = 0;

    // Our class's constructor. Takes a ScriptableBombRunUnitBaseStats as an argument.
    public BombRunUnitStatManager(BombRunUnit unit, ScriptableBombRunUnitBaseStats baseStats)
    {
        this._unit = unit;
        this._baseStats = baseStats;
        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged;
    }

    public int GetMaxMoveDistance()
    {
        Debug.Log("BombRunUnitStatManager: GetMaxMoveDistance");
        //return _baseStats.BaseMaxMoveDistance();
        if (_maxMoveDistanceWasUpdated)
        {
            Debug.Log("BombRunUnitStatManager: GetMaxMoveDistance: _maxMoveDistanceWasUpdated: " + _maxMoveDistanceWasUpdated.ToString() + " getting a new max move distance value...");
            _maxMoveDistanceCached = CalculateMaxMoveDistance();
            _maxMoveDistanceWasUpdated = false;
        }
        Debug.Log("BombRunUnitStatManager: GetMaxMoveDistance: " + _maxMoveDistanceCached);
        return _maxMoveDistanceCached;
        
    }
    public int CalculateMaxMoveDistance()
    {
        //int moveDistance = _baseStats.BaseMaxMoveDistance();
        //moveDistance += (int) GetAdditiveStatModifier(StatType.MaxMoveDistance);

        //if (_actionsModifyingStatsMultiply.Any((Func<ActionModifyingStat, bool>)(x => x.StatType == StatType.MaxMoveDistance)))
        //{
        //    moveDistance = (int)(moveDistance * GetMultiplingStatModifier(StatType.MaxMoveDistance));
        //}

        return (int)((_baseStats.BaseMaxMoveDistance() + GetAdditiveStatModifier(StatType.MaxMoveDistance)) * GetMultiplingStatModifier(StatType.MaxMoveDistance));
        
        
        //return moveDistance;
    }
    public int GetSightDistance()
    {
        return _baseStats.BaseSightDistance();
    }
    public float GetFOV()
    {
        return _baseStats.BaseFOV();
    }
    public float GetHearingSensitivity()
    {
        return _baseStats.BaseHearingSensitivity();
    }
    private float GetAdditiveStatModifier(StatType statType)
    {
        float modifier = 0f;
        foreach (ActionModifyingStat actionModifyingStat in _actionsModifyingStatsAdditive)
        {
            if (actionModifyingStat.StatType == statType)
            {
                modifier += actionModifyingStat.StatModifier;
            }
        }
        return modifier;
    }
    private float GetMultiplingStatModifier(StatType statType)
    {
        float modifier = 1f;
        foreach (ActionModifyingStat actionModifyingStat in _actionsModifyingStatsMultiply)
        {
            if (actionModifyingStat.StatType == statType)
            {
                modifier *= actionModifyingStat.StatModifier;
            }
        }
        return modifier;
    }
    public void UnsubscribeFromEvents()
    {
        UnitActionSystem.Instance.OnSelectedActionChanged -= UnitActionSystem_OnSelectedActionChanged;
    }
    private void UnitActionSystem_OnSelectedActionChanged(object sender, EventArgs e)
    {
        if (UnitActionSystem.Instance.GetSelectedAction() == null)
            return;
        if (TurnSystem.Instance.IsPlayerTurn() && _unit.IsEnemy())
            return;
        if (!TurnSystem.Instance.IsPlayerTurn() && !_unit.IsEnemy())
            return;

        if (UnitActionSystem.Instance.GetSelectedUnit() != _unit)
        {
            return;
        }

        //RemoveActionModifyingStatByActionInverse(UnitActionSystem.Instance.GetSelectedAction());
        
    }
    public void AddActionModifyingStatAdditive(BaseAction action, StatType statType, float statModifier)
    {
        Debug.Log("BombRunUnitStatManager: AddActionModifyingStatAdditive: ");
        _actionsModifyingStatsAdditive.Add(new ActionModifyingStat(action, statType, statModifier));
        StatTypeChanged(statType);
    }
    public void AddActionModifyingStatMultiply(BaseAction action, StatType statType, float statModifier)
    {
        Debug.Log("BombRunUnitStatManager: AddActionModifyingStatMultiply: ");
        _actionsModifyingStatsMultiply.Add(new ActionModifyingStat(action, statType, statModifier));
        StatTypeChanged(statType);
    }
    public void RemoveActionModifyingStatByAction(BaseAction action)
    {
        Debug.Log("BombRunUnitStatManager: RemoveActionModifyingStatByAction: " + action.GetType().ToString());
        List<StatType> statTypesUpdated = new List<StatType>();
        if (_actionsModifyingStatsAdditive.Count > 0)
        {
            foreach (ActionModifyingStat actionModifyingStat in _actionsModifyingStatsAdditive)
            {
                if (actionModifyingStat.Action.GetType() == action.GetType())
                {
                    if (!statTypesUpdated.Contains(actionModifyingStat.StatType))
                    {
                        statTypesUpdated.Add(actionModifyingStat.StatType);
                    }
                }
            }
        }
        if (_actionsModifyingStatsMultiply.Count > 0)
        {
            foreach (ActionModifyingStat actionModifyingStat in _actionsModifyingStatsMultiply)
            {
                if (actionModifyingStat.Action.GetType() == action.GetType())
                {
                    if (!statTypesUpdated.Contains(actionModifyingStat.StatType))
                    {
                        statTypesUpdated.Add(actionModifyingStat.StatType);
                    }
                }
            }
        }

        _actionsModifyingStatsAdditive.RemoveAll(x => x.Action.GetType() == action.GetType());
        _actionsModifyingStatsMultiply.RemoveAll(x => x.Action.GetType() == action.GetType());

        foreach (StatType statType in statTypesUpdated)
        {
            StatTypeChanged(statType);
        }
    }
    public void RemoveActionModifyingStatByActionInverse(BaseAction action)
    {
        Debug.Log("BombRunUnitStatManager: RemoveActionModifyingStatByActionInverse: " + action.GetType().ToString()) ;
        List<StatType> statTypesUpdated = new List<StatType>();
        if (_actionsModifyingStatsAdditive.Count > 0)
        {
            foreach (ActionModifyingStat actionModifyingStat in _actionsModifyingStatsAdditive)
            {
                if (actionModifyingStat.Action.GetType() != action.GetType())
                {
                    if (!statTypesUpdated.Contains(actionModifyingStat.StatType))
                    {
                        statTypesUpdated.Add(actionModifyingStat.StatType);
                    }
                }
            }
        }
        if (_actionsModifyingStatsMultiply.Count > 0)
        {
            foreach (ActionModifyingStat actionModifyingStat in _actionsModifyingStatsMultiply)
            {
                if (actionModifyingStat.Action.GetType() != action.GetType())
                {
                    if (!statTypesUpdated.Contains(actionModifyingStat.StatType))
                    {
                        statTypesUpdated.Add(actionModifyingStat.StatType);
                    }
                }
            }
        }

        _actionsModifyingStatsAdditive.RemoveAll(x => x.Action.GetType() != action.GetType());
        _actionsModifyingStatsMultiply.RemoveAll(x => x.Action.GetType() != action.GetType());

        foreach (StatType statType in statTypesUpdated)
        {
            StatTypeChanged(statType);
        }
    }
    private void StatTypeChanged(StatType statType)
    {
        switch (statType)
        {
            case StatType.MaxMoveDistance:
                _maxMoveDistanceWasUpdated = true;
                OnMaxMovementDistanceChanged?.Invoke(this, EventArgs.Empty);
                break;
            case StatType.SightDistance:
                OnSightDistanceChanged?.Invoke(this, EventArgs.Empty);
                break;
            case StatType.FOV:
                OnFOVChanged?.Invoke(this, EventArgs.Empty);
                break;
            case StatType.HearingSensitivity:
                OnHearingSensitivityChanged?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

}
