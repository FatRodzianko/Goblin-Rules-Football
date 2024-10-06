using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefendAction : BaseAction
{
    public override string GetActionName()
    {
        return "Defend";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = _unit.GetGridPosition();

        return new List<GridPosition>
        {
            unitGridPosition
        };
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        _onActionComplete = onActionComplete;
        _isActive = true;
        FinishAction();
    }
    private void FinishAction()
    {
        _isActive = false;
        _onActionComplete();
    }
}
