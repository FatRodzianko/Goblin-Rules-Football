using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SneakAltAction : BaseAltAction
{
    [Header("Modifers to base action")]
    [SerializeField] private float _maxMoveDistanceReduction = 0.5f;
    [SerializeField] private int _reducedNoiseDistanceAmount = 0;
    [SerializeField] private float _actionPointCostIncrease = 2f;

    

    public override void UpdateBaseActionForThisAltAction()
    {
        MoveAction moveAction = _parentAction as MoveAction;

        //moveAction.SetMaxMoveDistance((int)(moveAction.GetMaxMoveDistance() * _maxMoveDistanceReduction));
        moveAction.SetActionPointDefaultCost((int)(moveAction.GetActionPointsCost() * _actionPointCostIncrease));
        moveAction.SetMakesNoise(false);
        moveAction.SetActionName(this._altActionName);

        moveAction.BaseActionUpdateByAltAction();
    }
    public override bool CanPlayerSpendActionPointsForAltAction()
    {
        MoveAction moveAction = _parentAction as MoveAction;

        if (this._parentAction.GetUnit().GetActionPoints() >= this._parentAction.CalculateActionPointCostForValue((int)(moveAction.GetCachedActionPointDefaultCost() * _actionPointCostIncrease)))
        {
            Debug.Log("CanPlayerSpendActionPointsForAltAction: true for: " + this._altActionName + " cost was: " + this._parentAction.GetUnit().GetActionPoints().ToString());
            return true;
        }
        else
        {
            Debug.Log("CanPlayerSpendActionPointsForAltAction: false for: " + this._altActionName + " cost was: " + this._parentAction.GetUnit().GetActionPoints().ToString());
            return false;
        }
    }
}
