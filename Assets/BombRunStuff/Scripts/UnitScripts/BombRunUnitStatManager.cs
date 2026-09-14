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
    private bool _sightDistanceWasUpdated = true;
    private bool _fovWasUpdated = true;
    private bool _hearingSensitivtyWasUpdate = true;

    // cached stat values
    private int _maxMoveDistanceCached = 0;
    private int _sightDistanceCached = 0;
    private float _fovCached = 0f;
    private float _hearingSensitivityCached = 0f;

    // Our class's constructor. Takes a ScriptableBombRunUnitBaseStats as an argument.
    public BombRunUnitStatManager(BombRunUnit unit, ScriptableBombRunUnitBaseStats baseStats)
    {
        this._unit = unit;
        this._baseStats = baseStats;
        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged;
    }

    public int GetMaxMoveDistance()
    {
        //Debug.Log("BombRunUnitStatManager: GetMaxMoveDistance");
        //return _baseStats.BaseMaxMoveDistance();
        if (_maxMoveDistanceWasUpdated)
        {
            Debug.Log("BombRunUnitStatManager: GetMaxMoveDistance: _maxMoveDistanceWasUpdated: " + _maxMoveDistanceWasUpdated.ToString() + " getting a new max move distance value...");
            _maxMoveDistanceCached = CalculateMaxMoveDistance();
            _maxMoveDistanceWasUpdated = false;
        }
        //Debug.Log("BombRunUnitStatManager: GetMaxMoveDistance: " + _maxMoveDistanceCached);
        return _maxMoveDistanceCached;
        
    }
    public int CalculateMaxMoveDistance()
    {
        return (int)((_baseStats.BaseMaxMoveDistance() + GetAdditiveStatModifier(StatType.MaxMoveDistance)) * GetMultiplyingStatModifier(StatType.MaxMoveDistance));
    }
    public int GetSightDistance()
    {
        if (_sightDistanceWasUpdated)
        {
            int newSightDistance = CalculateSightDistance();
            Debug.Log("BombRunUnitStatManager: GetSightDistance: _sightDistanceWasUpdated: " + _sightDistanceWasUpdated.ToString() + " for " + this._unit + " old value: " + _sightDistanceCached + " new value: " + newSightDistance);
            _sightDistanceCached = newSightDistance;
            _sightDistanceWasUpdated = false;
        }
        return _sightDistanceCached;
    }
    public int CalculateSightDistance()
    {
        //return (int)((_baseStats.BaseSightDistance() + GetAdditiveStatModifier(StatType.SightDistance)) * GetMultiplyingStatModifier(StatType.SightDistance));
        int sightDistance = (int)((_baseStats.BaseSightDistance() + GetAdditiveStatModifier(StatType.SightDistance)) * GetMultiplyingStatModifier(StatType.SightDistance));
        switch (this._unit.GetUnitHealthSystem().GetBodyPartFrozenState(BodyPart.Head))
        {
            case BodyPartFrozenState.FullFrozen:
                sightDistance = 0;
                break;
            case BodyPartFrozenState.HalfFrozen:
                sightDistance = (int)(sightDistance / 2);
                break;
        }
        return sightDistance;
    }
    public float GetFOV()
    {
        if (_fovWasUpdated)
        {
            float newFOV = CalculateFOV();
            Debug.Log("BombRunUnitStatManager: GetFOV: _fovWasUpdated: " + _fovWasUpdated.ToString() + " for " + this._unit + " old value: " +_fovCached + " new value: " + newFOV);
            _fovCached = newFOV;
            _fovWasUpdated = false;
        }
        return _fovCached;
    }
    public float CalculateFOV()
    {
        return (int)((_baseStats.BaseFOV() + GetAdditiveStatModifier(StatType.FOV)) * GetMultiplyingStatModifier(StatType.FOV));
        //int fov = (int)((_baseStats.BaseFOV() + GetAdditiveStatModifier(StatType.FOV)) * GetMultiplyingStatModifier(StatType.FOV));
        //switch (this._unit.GetUnitHealthSystem().GetBodyPartFrozenState(BodyPart.Head))
        //{
        //    case BodyPartFrozenState.FullFrozen:
        //        fov = 0;
        //        break;
        //    case BodyPartFrozenState.HalfFrozen:
        //        fov = (int)(fov / 2);
        //        break;
        //}
        //return fov;
    }
    public float GetHearingSensitivity()
    {
        if (_hearingSensitivtyWasUpdate)
        {
            float newHearingSensitivity = CalculateHearingSensitivity();
            Debug.Log("BombRunUnitStatManager: GetHearingSensitivity: _hearingSensitivtyWasUpdate: " + _fovWasUpdated.ToString() + " for " + this._unit + " old value: " + _hearingSensitivityCached + " new value: " + newHearingSensitivity);
            _hearingSensitivityCached = newHearingSensitivity;
            _hearingSensitivtyWasUpdate = false;
        }
        return _hearingSensitivityCached;
    }
    public float CalculateHearingSensitivity()
    {
        //return (_baseStats.BaseHearingSensitivity() + GetAdditiveStatModifier(StatType.HearingSensitivity)) * GetMultiplyingStatModifier(StatType.HearingSensitivity);
        float hearingSensitivity = (_baseStats.BaseHearingSensitivity() + GetAdditiveStatModifier(StatType.HearingSensitivity)) * GetMultiplyingStatModifier(StatType.HearingSensitivity);
        switch (this._unit.GetUnitHealthSystem().GetBodyPartFrozenState(BodyPart.Head))
        {
            case BodyPartFrozenState.FullFrozen:
                hearingSensitivity = 0f;
                break;
            case BodyPartFrozenState.HalfFrozen:
                hearingSensitivity = (hearingSensitivity / 2);
                break;
        }
        return hearingSensitivity;
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
        modifier += _unit.BodyModManager().GetAdditiveStatModifierFromBodyMods(statType);
        return modifier;
    }
    private float GetMultiplyingStatModifier(StatType statType)
    {
        float modifier = 1f;
        foreach (ActionModifyingStat actionModifyingStat in _actionsModifyingStatsMultiply)
        {
            if (actionModifyingStat.StatType == statType)
            {
                modifier *= actionModifyingStat.StatModifier;
            }
        }
        modifier *= _unit.BodyModManager().GetMultiplyingStatModifierFromBodyMods(statType);
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
    public void BodyPartFrozenStateUpdated(BodyPart bodyPart)
    {
        switch (bodyPart)
        {
            case BodyPart.Head:
                StatTypeChanged(StatType.HearingSensitivity);
                StatTypeChanged(StatType.SightDistance);
                StatTypeChanged(StatType.FOV);
                break;
        }
    }
    public void StatTypeChanged(StatType statType)
    {
        switch (statType)
        {
            case StatType.MaxMoveDistance:
                _maxMoveDistanceWasUpdated = true;
                OnMaxMovementDistanceChanged?.Invoke(this, EventArgs.Empty);
                break;
            case StatType.SightDistance:
                _sightDistanceWasUpdated = true;
                OnSightDistanceChanged?.Invoke(this, EventArgs.Empty);
                break;
            case StatType.FOV:
                _fovWasUpdated = true;
                OnFOVChanged?.Invoke(this, EventArgs.Empty);
                break;
            case StatType.HearingSensitivity:
                OnHearingSensitivityChanged?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

}
