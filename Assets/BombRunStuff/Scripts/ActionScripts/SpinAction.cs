using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpinAction : BaseAction
{
    //public class SpinBaseParameters : BaseParameters // override the base parameter for the spin class?
    //{

    //} 
    private float _totalSpinAmount;
    private float _maxSpinAmount = 360f;

    private void Update()
    {
        if (!_isActive)
        {
            return;
        }


        float spinAddAmount = _maxSpinAmount * Time.deltaTime;

        if (_totalSpinAmount + spinAddAmount >= _maxSpinAmount)
        {
            spinAddAmount = _maxSpinAmount - _totalSpinAmount;
            _totalSpinAmount += spinAddAmount;
            _isActive = false;
            _onActionComplete();
        }
        else
        {
            _totalSpinAmount += spinAddAmount;
        }

        transform.eulerAngles += new Vector3(0, spinAddAmount, 0);
    }
    public override void TakeAction(GridPosition gridPosition, Action onSpinComplete)
    {
        _onActionComplete = onSpinComplete;
        _totalSpinAmount = 0;
        _isActive = true;        
    }
    public override string GetActionName()
    {
        return "Spin";
    }
    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = _unit.GetGridPosition();

        return new List<GridPosition>
        {
            unitGridPosition
        };
    }
    public override int GetActionPointsCost()
    {
        return 2;
    }
}
