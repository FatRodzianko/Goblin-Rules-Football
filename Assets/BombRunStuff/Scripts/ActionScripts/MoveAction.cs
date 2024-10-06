using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveAction : BaseAction
{
    //public class MoveActionBaseParameters : BaseParameters // override the base parameter for the spin class?
    //{
    //    private GridPosition gridPosition;
    //    public MoveActionBaseParameters(GridPosition gridPosition)
    //    {
    //        this.gridPosition = gridPosition;
    //    }
    //    public GridPosition GetGridPosition()
    //    {
    //        return this.gridPosition;
    //    }
    //} 

    [Header("Moving")]
    [SerializeField] private int _maxMoveDistance = 4;
    private Vector3 _targetPosition;
    private float _moveSpeed = 4f;
    private float _stoppingDistance = 0.05f;
    

    [Header("Animation")]
    [SerializeField] private Animator _unitAnimator;


    protected override void Awake()
    {
        base.Awake();
        _targetPosition = this.transform.position;
    }
    private void Update()
    {
        if (!_isActive)
            return;


        if (Vector2.Distance(transform.position, _targetPosition) > _stoppingDistance)
        {
            Vector3 moveDirection = (_targetPosition - this.transform.position).normalized;
            transform.position += moveDirection * _moveSpeed * Time.deltaTime;
        }
        else
        {
            _isActive = false;
            _onActionComplete();
        }
    }
    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        _onActionComplete = onActionComplete;
        _targetPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
        _isActive = true;
    }
    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = _unit.GetGridPosition();
        for (int x = -_maxMoveDistance; x <= _maxMoveDistance; x++)
        {
            for (int y = -_maxMoveDistance; y <= _maxMoveDistance; y++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, y);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }
                if (unitGridPosition == testGridPosition)
                {
                    // same position unit is already at
                    continue;
                }
                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    // Grid position already occupied by a unit
                    // later will check if number of units is below max allowed on a tile
                    // Will need to take into account if the number of selected units would put the testGridPosition above the max units allowed
                    continue;
                }

                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }

    public override string GetActionName()
    {
        return "Move";
    }
}
