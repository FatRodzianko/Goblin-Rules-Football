using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SneakAltAction : BaseAltAction
{
    [Header("Modifers to base action")]
    [SerializeField] private float _maxMoveDistanceReduction = 0.5f;
    [SerializeField] private int _reducedNoiseDistanceAmount = 0;

    public override void UpdateBaseActionForThisAltAction()
    {
        MoveAction moveAction = _parentAction as MoveAction;

        moveAction.SetMaxMoveDistance((int)(moveAction.GetMaxMoveDistance() * _maxMoveDistanceReduction));
        moveAction.SetMakesNoise(false);
        moveAction.SetActionName(this._altActionName);

        moveAction.BaseActionUpdateByAltAction();
    }
}
