using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprintAltAction : BaseAltAction
{
    [Header("Modifers to base action")]
    [SerializeField] private float _maxMoveDistanceIncrease = 1.5f;
    [SerializeField] private float _noiseDistanceIncrease = 2f;

    [Header("Sprint Action stuff")]
    [SerializeField] private bool _usedThisTurn = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
        BaseAction.OnAnyActionStarted += BaseAction_OnAnyActionStarted;
    }
    private void OnDisable()
    {
        TurnSystem.Instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
        BaseAction.OnAnyActionStarted -= BaseAction_OnAnyActionStarted;
    }

    

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        _usedThisTurn = false;
    }

    public override bool CanPlayerSpendActionPointsForAltAction()
    {
        if (_usedThisTurn)
            return false;

        MoveAction moveAction = _parentAction as MoveAction;

        Debug.Log("SprintAltAction: CanPlayerSpendActionPointsForAltAction: " + moveAction.CanTakeAction(_parentAction.GetUnit().GetActionPoints(), _parentAction.GetUnit().GetGridPosition()).ToString());
        return moveAction.CanTakeAction(_parentAction.GetUnit().GetActionPoints(), _parentAction.GetUnit().GetGridPosition()) ;
        //if (this._parentAction.GetUnit().GetActionPoints() >= this._parentAction.CalculateActionPointCostForValue(moveAction.GetCachedActionPointDefaultCost()))
        //{
        //    Debug.Log("SprintAltAction: CanPlayerSpendActionPointsForAltAction: true for: " + this._altActionName + " cost was: " + this._parentAction.GetUnit().GetActionPoints().ToString());
        //    return true;
        //}
        //else
        //{
        //    Debug.Log("SprintAltAction: CanPlayerSpendActionPointsForAltAction: false for: " + this._altActionName + " cost was: " + this._parentAction.GetUnit().GetActionPoints().ToString());
        //    return false;
        //}
    }

    public override void UpdateBaseActionForThisAltAction()
    {
        MoveAction moveAction = _parentAction as MoveAction;
        moveAction.ResetToBaseActionSettings();

        //moveAction.SetMaxMoveDistance((int)(moveAction.GetMaxMoveDistance() * _maxMoveDistanceIncrease));
        //moveAction.SetNoiseDistance()
        moveAction.SetMaxMoveDistanceModifer(this._maxMoveDistanceIncrease);
        moveAction.SetNoiseDistanceModifer(this._noiseDistanceIncrease);
        
        moveAction.SetActionName(this._altActionName);

        Debug.Log("SprintAltAction: UpdateBaseActionForThisAltAction: ");
        moveAction.BaseActionUpdateByAltAction();
    }
    private void BaseAction_OnAnyActionStarted(object sender, EventArgs e)
    {
        BaseAction action = sender as BaseAction;
        if (action == _parentAction)
        {
            if (_parentAction.GetAltActionIndex() == this._parentAction.GetIndexForAltAction(this))
            {
                this._usedThisTurn = true;
            }
        }
    }
}
