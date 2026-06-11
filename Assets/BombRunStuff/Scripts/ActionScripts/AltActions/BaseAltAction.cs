using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAltAction : MonoBehaviour
{
    [Header("Base Stuff")]
    [SerializeField] protected BombRunUnit _unit;
    [SerializeField] protected ActionType _parentActionType;
    [SerializeField] protected BaseAction _parentAction;

    [Header("Alt Action Info")]
    [SerializeField] protected string _altActionName;
    [SerializeField] protected ActionType _actionType;
    [SerializeField] protected int _baseActionCost;

    protected virtual void Start()
    {
        _parentAction = _unit.GetActionByActionType(_parentActionType);
        if (_parentAction == null)
            return;
        _parentAction.AddAltActionToAltActionList(this);
        _parentAction.SetHasAltAction(true);
        _parentAction.AddAltActionToAltActionList(this);

    }
    public abstract void UpdateBaseActionForThisAltAction();
    public abstract bool CanPlayerSpendActionPointsForAltAction();

}
