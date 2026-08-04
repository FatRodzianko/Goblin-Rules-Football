using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActionModifyingNoise
{
    public BaseAction Action;
    public BodyPart BodyPart;
    public float StatModifier;

    public ActionModifyingNoise(BaseAction action, BodyPart bodyPart, float statModifier)
    {
        Action = action;
        BodyPart = bodyPart;
        StatModifier = statModifier;
    }
}
[Serializable]
public class BombRunUnitNoiseManager
{
    private BombRunUnit _unit;

    [SerializeField] private List<ActionModifyingNoise> _actionsModifyingNoiseAdditive = new List<ActionModifyingNoise>();
    [SerializeField] private List<ActionModifyingNoise> _actionsModifyingNoiseMultiply = new List<ActionModifyingNoise>();


    // events
    public EventHandler OnBodyPartChanged_Legs;
    public EventHandler OnBodyPartChanged_Arms;
    public EventHandler OnBodyPartChanged_Head;
    public EventHandler OnBodyPartChanged_None;

    // Our class's constructor. Takes a ScriptableBombRunUnitBaseStats as an argument.
    public BombRunUnitNoiseManager(BombRunUnit unit)
    {
        this._unit = unit;
        
        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged;
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
    public int GetNoiseDistance(int baseNoise, BodyPart bodyPartMakingNoise)
    {
        //Debug.Log("BombRunUnitNoiseManager: GetNoiseDistance: for " + this._unit.name + "'s " + bodyPartMakingNoise + " with base of: " + baseNoise);
        return (int)((baseNoise + GetAdditiveNoiseModifier(bodyPartMakingNoise)) * GetMultiplyingNoiseModifier(bodyPartMakingNoise));
    }
    private float GetAdditiveNoiseModifier(BodyPart bodyPart)
    {
        float modifier = 0f;
        foreach (ActionModifyingNoise actionModifyingNoise in _actionsModifyingNoiseAdditive)
        {
            if (actionModifyingNoise.BodyPart == bodyPart)
            {
                modifier += actionModifyingNoise.StatModifier;
            }
        }

        // Get from body mods?
        modifier += _unit.BodyModManager().GetAdditiveNoiseModifierFromBodyMods(bodyPart);
        return modifier;
    }
    private float GetMultiplyingNoiseModifier(BodyPart bodyPart)
    {
        float modifier = 1f;
        foreach (ActionModifyingNoise actionModifyingNoise in _actionsModifyingNoiseMultiply)
        {
            if (actionModifyingNoise.BodyPart == bodyPart)
            {
                modifier *= actionModifyingNoise.StatModifier;
            }
        }

        // Get from body mods?
        modifier *= _unit.BodyModManager().GetMultiplyingNoiseModifierFromBodyMods(bodyPart);
        return modifier;
    }
    public void AddActionModifyingNoiseAdditive(BaseAction action, BodyPart bodyPart, float statModifier)
    {
        Debug.Log("BombRunUnitNoiseManager: AddActionModifyingNoiseAdditive: ");
        _actionsModifyingNoiseAdditive.Add(new ActionModifyingNoise(action, bodyPart, statModifier));
        BodyPartChanged(bodyPart);
    }
    public void AddActionModifyingNoiseMultiply(BaseAction action, BodyPart bodyPart, float statModifier)
    {
        Debug.Log("BombRunUnitNoiseManager: AddActionModifyingNoiseMultiply: ");
        _actionsModifyingNoiseMultiply.Add(new ActionModifyingNoise(action, bodyPart, statModifier));
        BodyPartChanged(bodyPart);
    }
    public void RemoveActionModifyingNoiseByAction(BaseAction action)
    {
        Debug.Log("BombRunUnitNoiseManager: RemoveActionModifyingNoiseByAction: " + action.GetType().ToString());
        List<BodyPart> bodyPartsUpdated = new List<BodyPart>();
        if (_actionsModifyingNoiseAdditive.Count > 0)
        {
            foreach (ActionModifyingNoise actionModifyingNoise in _actionsModifyingNoiseAdditive)
            {
                if (actionModifyingNoise.Action.GetType() == action.GetType())
                {
                    if (!bodyPartsUpdated.Contains(actionModifyingNoise.BodyPart))
                    {
                        bodyPartsUpdated.Add(actionModifyingNoise.BodyPart);
                    }
                }
            }
        }
        if (_actionsModifyingNoiseMultiply.Count > 0)
        {
            foreach (ActionModifyingNoise actionModifyingNoise in _actionsModifyingNoiseMultiply)
            {
                if (actionModifyingNoise.Action.GetType() == action.GetType())
                {
                    if (!bodyPartsUpdated.Contains(actionModifyingNoise.BodyPart))
                    {
                        bodyPartsUpdated.Add(actionModifyingNoise.BodyPart);
                    }
                }
            }
        }

        _actionsModifyingNoiseAdditive.RemoveAll(x => x.Action.GetType() == action.GetType());
        _actionsModifyingNoiseMultiply.RemoveAll(x => x.Action.GetType() == action.GetType());

        foreach (BodyPart bodyPart in bodyPartsUpdated)
        {
            BodyPartChanged(bodyPart);
        }
    }
    public void RemoveActionModifyingNoiseByActionInverse(BaseAction action)
    {
        Debug.Log("BombRunUnitNoiseManager: RemoveActionModifyingNoiseByActionInverse: " + action.GetType().ToString());
        List<BodyPart> bodyPartsUpdated = new List<BodyPart>();
        if (_actionsModifyingNoiseAdditive.Count > 0)
        {
            foreach (ActionModifyingNoise actionModifyingNoise in _actionsModifyingNoiseAdditive)
            {
                if (actionModifyingNoise.Action.GetType() != action.GetType())
                {
                    if (!bodyPartsUpdated.Contains(actionModifyingNoise.BodyPart))
                    {
                        bodyPartsUpdated.Add(actionModifyingNoise.BodyPart);
                    }
                }
            }
        }
        if (_actionsModifyingNoiseMultiply.Count > 0)
        {
            foreach (ActionModifyingNoise actionModifyingNoise in _actionsModifyingNoiseMultiply)
            {
                if (actionModifyingNoise.Action.GetType() != action.GetType())
                {
                    if (!bodyPartsUpdated.Contains(actionModifyingNoise.BodyPart))
                    {
                        bodyPartsUpdated.Add(actionModifyingNoise.BodyPart);
                    }
                }
            }
        }

        _actionsModifyingNoiseAdditive.RemoveAll(x => x.Action.GetType() != action.GetType());
        _actionsModifyingNoiseMultiply.RemoveAll(x => x.Action.GetType() != action.GetType());

        foreach (BodyPart bodyPart in bodyPartsUpdated)
        {
            BodyPartChanged(bodyPart);
        }
    }
    private void BodyPartChanged(BodyPart bodyPart)
    {
        switch (bodyPart)
        {
            case BodyPart.Legs:
                OnBodyPartChanged_Legs?.Invoke(this, EventArgs.Empty);
                break;
            case BodyPart.Arms:
                OnBodyPartChanged_Arms?.Invoke(this, EventArgs.Empty);
                break;
            case BodyPart.Head:
                OnBodyPartChanged_Head?.Invoke(this, EventArgs.Empty);
                break;
            case BodyPart.None:
                OnBodyPartChanged_None?.Invoke(this, EventArgs.Empty);
                break;
        }
    }
}
