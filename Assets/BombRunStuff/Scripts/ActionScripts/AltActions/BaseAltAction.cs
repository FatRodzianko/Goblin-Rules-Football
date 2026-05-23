using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAltAction : MonoBehaviour
{
    [Header("Base Stuff")]
    [SerializeField] protected BombRunUnit _unit;
    [SerializeField] protected ActionType _parentActionType;
    [SerializeField] protected BaseAction _parentAction;

    [Header("Alt Action Info")]
    [SerializeField] protected string _altActionName;
    [SerializeField] protected ActionType _actionType;

    private void Start()
    {
        _parentAction = _unit.GetActionByActionType(_parentActionType);
        if (_parentAction == null)
            return;
        _parentAction.AddAltActionToAltActionList(this);
        _parentAction.SetHasAltAction(true);
        _parentAction.AddAltActionToAltActionList(this);

    }
    private void UpdateActionName()
    {
        _parentAction.SetActionName(_altActionName);
    }


}
