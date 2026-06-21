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
    [SerializeField] protected int _altActionIndex;

    protected virtual void Start()
    {
        _parentAction = _unit.GetActionByActionType(_parentActionType);
        if (_parentAction == null)
            return;
        this._altActionIndex = _parentAction.AddAltActionToAltActionList(this);
        _parentAction.SetHasAltAction(true);

        BaseAction.OnAnyActionCompleted += BaseAction_OnAnyActionCompleted;
    }
    private void OnDisable()
    {
        BaseAction.OnAnyActionCompleted -= BaseAction_OnAnyActionCompleted;
    }


    public abstract void UpdateBaseActionForThisAltAction();
    public abstract bool CanPlayerSpendActionPointsForAltAction();

    protected virtual void BaseAction_OnAnyActionCompleted(object sender, EventArgs e)
    {
        if (UnitActionSystem.Instance.GetSelectedAction() == null)
            return;
        if (UnitActionSystem.Instance.GetSelectedAction() != this._parentAction)
            return;
        if (this._parentAction.GetAltActionIndex() != this._altActionIndex)
            return;

        if (this.CanPlayerSpendActionPointsForAltAction())
            return;

        Debug.Log("BaseAction_OnAnyActionCompleted: " + this.GetType().ToString() + ": cannot afford alt action. resetting alt action index to 0.");
        _parentAction.SetAltActionIndex(0);
    }

}
