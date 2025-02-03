using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAction : BaseAction
{
    private enum State
    {
        SwingingSwordBeforeHit,
        SwiningSwordAfterHit
    }

    private State _state;
    private float _stateTimer;

    private int _maxSwordDistance = 1;

    // target stuff
    private BombRunUnit _targetUnit;
    private Vector3 _targetPosition;
    private Vector3 _unitPosition;

    // spinning stuff
    private float _totalSpinAmount;
    private float _maxSpinAmount = 360f;

    // events
    public event EventHandler OnSwordActionStarted;
    public event EventHandler OnSwordActionCompleted;

    public static event EventHandler OnAnySwordHit;

    private void Update()
    {
        if (!_isActive)
            return;

        _stateTimer -= Time.deltaTime;
        switch (_state)
        {
            case State.SwingingSwordBeforeHit:
                SpinBeforeSwordHit();
                break;
            case State.SwiningSwordAfterHit:
                break;
        }

        if (_stateTimer <= 0f && _state != State.SwingingSwordBeforeHit)
        {
            NextState();
        }

        //ActionComplete();
    }
    void NextState()
    {
        switch (_state)
        {
            case State.SwingingSwordBeforeHit:
                _state = State.SwiningSwordAfterHit;
                float afterHitStateTime = 0.5f;
                _stateTimer = afterHitStateTime;
                _targetUnit.Damage(100);
                OnAnySwordHit?.Invoke(this, EventArgs.Empty);
                break;
            case State.SwiningSwordAfterHit:
                OnSwordActionCompleted?.Invoke(this, EventArgs.Empty);
                ActionComplete();
                break;
        }
    }
    public override string GetActionName()
    {
        return "Sword";
    }

    public override BombRunEnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new BombRunEnemyAIAction
        {
            _GridPosition = gridPosition,
            _ActionValue = 200,
        };
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitPosition = _unit.GetGridPosition();
        return GetValidActionGridPositionList(unitPosition);
    }

    public List<GridPosition> GetValidActionGridPositionList(GridPosition unitGridPosition)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        // From the player's starting position, cycle through the grid in the x and z axises and check if a valid grid position exists there.
        for (int x = -_maxSwordDistance; x <= _maxSwordDistance; x++)
        {
            for (int y = -_maxSwordDistance; y <= _maxSwordDistance; y++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, y);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                // check to see if the test position is a valid grid position
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }
                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    continue;
                }
                BombRunUnit targetUnit = LevelGrid.Instance.GetUnitListAtGridPosition(testGridPosition)[0];
                if (targetUnit.IsEnemy() == this._unit.IsEnemy())
                {
                    continue;
                }
                // calculate the distance to the grid position to make sure it is in the shooting radius. Right now the for loops form a big square around selected unit. This will make it more circular
                if (LevelGrid.Instance.CalculateDistance(unitGridPosition, testGridPosition) > _maxSwordDistance * 10)
                {
                    continue;
                }
                Debug.Log("SwordAction: GetValidActionGridPositionList: Valid position found at: " + testGridPosition.ToString());
                validGridPositionList.Add(testGridPosition);
            }
        }
        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        Debug.Log("TakeAction: SwordAction");

        _state = State.SwingingSwordBeforeHit;
        float beforeHitStateTime = 0.7f;
        _stateTimer = beforeHitStateTime;

        _targetUnit = LevelGrid.Instance.GetUnitListAtGridPosition(gridPosition)[0];
        _targetPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
        _unitPosition = _unit.GetWorldPosition();

        OnSwordActionStarted?.Invoke(this, EventArgs.Empty);

        ActionStart(onActionComplete);
    }
    public int GetMaxSwordDistance()
    {
        return _maxSwordDistance;
    }
    private void RotateTowardTarget()
    {
        Vector3 targetDirection = (_targetPosition - _unitPosition).normalized;
        float rotationSpeed = 10f;
        transform.forward = Vector3.Lerp(transform.forward, targetDirection, Time.deltaTime * rotationSpeed);
    }
    private void SpinBeforeSwordHit()
    {
        float spinAddAmount = _maxSpinAmount * Time.deltaTime;

        if (_totalSpinAmount + spinAddAmount >= _maxSpinAmount)
        {
            spinAddAmount = _maxSpinAmount - _totalSpinAmount;
            _totalSpinAmount += spinAddAmount;
        }
        else
        {
            _totalSpinAmount += spinAddAmount;
        }

        if (_totalSpinAmount >= _maxSpinAmount)
        {
            NextState();
        }

        transform.eulerAngles += new Vector3(0, spinAddAmount, 0);
    }
}
