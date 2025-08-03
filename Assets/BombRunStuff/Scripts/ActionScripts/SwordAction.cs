using System;
using System.Linq;
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
    private BodyPart _targetBodyPart;

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
                //_targetUnit.Damage(100);
                DamageTarget();
                
                OnAnySwordHit?.Invoke(this, EventArgs.Empty);
                break;
            case State.SwiningSwordAfterHit:
                OnSwordActionCompleted?.Invoke(this, EventArgs.Empty);
                ActionComplete();
                break;
        }
    }

    private void DamageTarget()
    {
        if (_targetBodyPart == BodyPart.None)
        {
            _targetUnit.DamageAllBodyParts();
            return;
        }

        // x2 damage from sword?
        _targetUnit.DamageBodyPart(_targetBodyPart);
        _targetUnit.DamageBodyPart(_targetBodyPart);
    }

    public override string GetActionName()
    {
        return "Sword";
    }

    public override BombRunEnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        BombRunUnit aiTarget = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        if (aiTarget == null)
        {
            return new BombRunEnemyAIAction
            {
                _GridPosition = gridPosition,
                _ActionValue = 0,
            };
        }

        // don't sword your own teammate
        if (aiTarget.IsEnemy() == this._unit.IsEnemy())
        {
            return new BombRunEnemyAIAction
            {
                _GridPosition = gridPosition,
                _ActionValue = 0,
            };
        }
        // Check if all body parts are frozen aka not a valid target
        BombRunUnitHealthSystem targetHealthSystem = aiTarget.GetUnitHealthSystem();
        if (targetHealthSystem.AreAllBodyPartsFrozen())
        {
            Debug.Log("SwordAction: GetEnemyAIAction: All body parts for target at: " + gridPosition.ToString() + " are frozen");
            return new BombRunEnemyAIAction
            {
                _GridPosition = gridPosition,
                _ActionValue = 0,
            };
        }

        // set initial action value to a base level of 1000, adjust to prioritize units that are closer
        int actionValue = 1200;

        // get unit's action points remaining. For the sword action prioritize this if it is the last remaining action?
        if (_unit.GetActionPoints() <= 1)
        {
            actionValue += 100;
        }
        // target body part to save. Default to legs?
        BombRunUnitBodyPartAndFrozenState targetBodyPartAndFrozenState = new BombRunUnitBodyPartAndFrozenState { BodyPart = BodyPart.Legs, BodyPartFrozenState = BodyPartFrozenState.NotFrozen };


        // Check each body part to see if they are half frozen or not frozen. If half frozen, save as a possible target
        List<BodyPart> notFrozenBodyParts = new List<BodyPart>();
        List<BodyPart> halfFrozenBodyParts = new List<BodyPart>();
        List<BodyPart> cannotTargetBodyParts = new List<BodyPart>();
        List<BombRunUnitBodyPartAndFrozenState> targetBombRunUnitBodyPartAndFrozenStates = targetHealthSystem.GetAllBodyPartsAndFrozenState();
        foreach (BombRunUnitBodyPartAndFrozenState x in targetBombRunUnitBodyPartAndFrozenStates)
        {
            if (x.BodyPartFrozenState == BodyPartFrozenState.HalfFrozen)
            {
                if (!halfFrozenBodyParts.Contains(x.BodyPart))
                {
                    Debug.Log("SwordAction: GetEnemyAIAction: Half frozen bodypart found for target at: " + gridPosition.ToString() + ": " + x.BodyPart.ToString());
                    halfFrozenBodyParts.Add(x.BodyPart);
                }
            }
            else if (x.BodyPartFrozenState == BodyPartFrozenState.NotFrozen)
            {
                if (!notFrozenBodyParts.Contains(x.BodyPart))
                {
                    Debug.Log("SwordAction: GetEnemyAIAction: NOT frozen bodypart found for target at: " + gridPosition.ToString() + ": " + x.BodyPart.ToString());
                    notFrozenBodyParts.Add(x.BodyPart);
                }
            }
            else if (x.BodyPartFrozenState == BodyPartFrozenState.FullFrozen)
            {
                cannotTargetBodyParts.Add(x.BodyPart);
            }
        }

        // if any body parts are half frozen, use the "Body.None" target to target all body parts
        if (halfFrozenBodyParts.Count > 0)
        {
            Debug.Log("SwordAction: GetEnemyAIAction: at least one half frozen body part. Will target 'BodyPart.None' to hit all body parts once");
            targetBodyPartAndFrozenState.BodyPart = BodyPart.None;
            targetBodyPartAndFrozenState.BodyPartFrozenState = BodyPartFrozenState.HalfFrozen;

            // increase action value for each half frozen body part since that means you will fully freeze more bodyparts with this hit
            actionValue += halfFrozenBodyParts.Count * 750;

        }
        else
        {
            // check for what body part to hit twice


            // place holder:
            // later check for the unit type and weigh different body parts for different unit types. Scouts target legs? Medics arms? or something?
            // probably should have each unity type have a "AI Target Body Part" to just pull from

            // Priority list is: Arms, then legs, then head?
            if (!cannotTargetBodyParts.Contains(BodyPart.Arms))
            {
                targetBodyPartAndFrozenState.BodyPart = BodyPart.Arms;
                targetBodyPartAndFrozenState.BodyPartFrozenState = targetBombRunUnitBodyPartAndFrozenStates.First(x => x.BodyPart == BodyPart.Arms).BodyPartFrozenState;
            }
            else if (!cannotTargetBodyParts.Contains(BodyPart.Legs))
            {
                targetBodyPartAndFrozenState.BodyPart = BodyPart.Legs;
                targetBodyPartAndFrozenState.BodyPartFrozenState = targetBombRunUnitBodyPartAndFrozenStates.First(x => x.BodyPart == BodyPart.Legs).BodyPartFrozenState;
            }
            else
            {
                targetBodyPartAndFrozenState.BodyPart = BodyPart.Head;
                targetBodyPartAndFrozenState.BodyPartFrozenState = targetBombRunUnitBodyPartAndFrozenStates.First(x => x.BodyPart == BodyPart.Head).BodyPartFrozenState;
            }

            // adjust the action value to prioritize BodyParts
            // Pull the body part prioritization from GetUnitBodyPartActionValue so it is unity type specific?
            actionValue += BombRunUnitManager.Instance.GetUnitBodyPartActionValue(aiTarget.GetUnitType(), targetBodyPartAndFrozenState.BodyPart);
            //switch (targetBodyPartAndFrozenState.BodyPart)
            //{
            //    default:
            //    case BodyPart.Arms:
            //        actionValue += BombRunUnitManager.Instance.GetUnitBodyPartActionValue(aiTarget.GetUnitType(), targetBodyPartAndFrozenState.BodyPart);
            //        break;
            //    case BodyPart.Legs:
            //        actionValue += 100;
            //        break;
            //    case BodyPart.Head:
            //        actionValue += 50;
            //        break;
            //}
        }

        Debug.Log("GetEnemyAIAction: Sword Action: " + this._unit.name + ": Action Value: " + actionValue);
        return new BombRunEnemyAIAction
        {
            _GridPosition = gridPosition,
            _ActionValue = actionValue,
            _TargetBodyPart = targetBodyPartAndFrozenState.BodyPart,
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
                if (targetUnit.GetUnitHealthSystem().AreAllBodyPartsFrozen())
                {
                    continue;
                }
                // calculate the distance to the grid position to make sure it is in the shooting radius. Right now the for loops form a big square around selected unit. This will make it more circular
                //if (LevelGrid.Instance.CalculateDistance(unitGridPosition, testGridPosition) > _maxSwordDistance * 10)
                //{
                //    continue;
                //}
                Debug.Log("SwordAction: GetValidActionGridPositionList: Valid position found at: " + testGridPosition.ToString());
                validGridPositionList.Add(testGridPosition);
            }
        }
        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete, BodyPart bodyPart = BodyPart.None)
    {
        Debug.Log("TakeAction: SwordAction");

        _targetBodyPart = bodyPart;

        _totalSpinAmount = 0f;
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
