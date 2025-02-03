using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAction : BaseAction
{
    private enum State
    {
        Aiming,
        Shooting,
        Cooloff,
    }
    private State _state;
    private float _stateTimer;
    private float _aimingStateTime = 5f;
    private float _shootStateTime = 0.1f;
    private float _coolOffStateTime = 0.5f;

    [SerializeField] private int _maxShootDistance = 10;

    private BombRunUnit _targetUnit;
    private bool _canShootBullet = false;

    // Animation stuff
    private float _totalSpinAmount;
    private float _maxSpinAmount = 360f;
    private bool _aiming = false;

    private void Update()
    {
        if (!_isActive)
        {
            return;
        }

        _stateTimer -= Time.deltaTime;
        switch (_state)
        {
            case State.Aiming:
                if (_aiming)
                {
                    AimTowardTarget();
                }
                else
                {
                    NextState();
                }
                break;
            case State.Shooting:
                break;
            case State.Cooloff:
                break;
        }
        if (_stateTimer <= 0f)
        {
            NextState();
        }
    }
    void NextState()
    {
        switch (_state)
        {
            case State.Aiming:
                _state = State.Shooting;
                _stateTimer = _shootStateTime;                
                break;
            case State.Shooting:
                if (_canShootBullet)
                {
                    Shoot();
                    _canShootBullet = false;
                }
                _state = State.Cooloff;
                _stateTimer = _coolOffStateTime;
                break;
            case State.Cooloff:
                ActionComplete();
                break;
        }
    }
    private void AimTowardTarget()
    {
        float spinAddAmount = _maxSpinAmount * Time.deltaTime;

        if (_totalSpinAmount + spinAddAmount >= _maxSpinAmount)
        {
            spinAddAmount = _maxSpinAmount - _totalSpinAmount;
            _totalSpinAmount += spinAddAmount;
            _aiming = false;
        }
        else
        {
            _totalSpinAmount += spinAddAmount;
        }

        transform.eulerAngles += new Vector3(0, spinAddAmount, 0);
    }
    private void Shoot()
    {
        Debug.Log("Shoot");
        _targetUnit.Damage(35);
    }
    public override string GetActionName()
    {
        return "Shoot";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = _unit.GetGridPosition();

        for (int x = -_maxShootDistance; x <= _maxShootDistance; x++)
        {
            for (int y = -_maxShootDistance; y <= _maxShootDistance; y++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, y);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                // check to see if test position is the unit's current position. Skip if so because player can't move to the position they are already on
                if (testGridPosition == unitGridPosition)
                {
                    continue;
                }
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }
                if (unitGridPosition == testGridPosition)
                {
                    // same position unit is already at
                    continue;
                }
                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    // no unit to shoot at
                    continue;
                }
                if (LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition).IsEnemy() == _unit.IsEnemy())
                {
                    // no friendly fire... unless?
                    continue;
                }
                // check the distance to the target grid position. This will be the distance assuming no walls or anything. If distance with that is greater than distance max, skip
                int pathFindingDistanceMultiplier = 10;
                if (LevelGrid.Instance.CalculateDistance(unitGridPosition, testGridPosition) > _maxShootDistance * pathFindingDistanceMultiplier)
                {
                    continue;
                }
                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }
    public override void TakeAction(GridPosition gridPosition, Action onSpinComplete)
    {
        _onActionComplete = onSpinComplete;

        _state = State.Aiming;
        _stateTimer = _aimingStateTime;

        // get target to shoot out
        _targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        _canShootBullet = true;
        _aiming = true;
        _totalSpinAmount = 0;


        _isActive = true;
    }
    public override BombRunEnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new BombRunEnemyAIAction
        {
            _GridPosition = this._unit.GetGridPosition(),
            _ActionValue = 0,
        };
    }
    public int GetMaxShootDistance()
    {
        return _maxShootDistance;
    }
}
