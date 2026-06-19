using System;
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
        return _baseStats.BaseMaxMoveDistance();
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

        RemoveActionModifyingStatByActionInverse(UnitActionSystem.Instance.GetSelectedAction());
        
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
