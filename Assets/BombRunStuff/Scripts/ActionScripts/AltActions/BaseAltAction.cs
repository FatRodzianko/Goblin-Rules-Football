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
        UpdateActionName();
    }
    private void UpdateActionName()
    {
        _parentAction.SetActionName(_altActionName);
    }

}
