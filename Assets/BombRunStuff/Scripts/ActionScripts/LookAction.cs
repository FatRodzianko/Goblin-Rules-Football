using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAction : BaseAction
{
    [SerializeField] private List<GridPosition> _lookAtPositions = new List<GridPosition>();
    [SerializeField] private GridPosition _targetLookAtPosition;
    [SerializeField] private float _timer = 0f;
    [SerializeField] private float _lookAtDelay = 0.5f;
    private void Update()
    {
        if (!_isActive)
            return;

        _timer -= Time.deltaTime;
        

        if (_timer <= 0f)
        {
            _unit.SetActionDirection(LevelGrid.Instance.GetWorldPosition(_targetLookAtPosition) - LevelGrid.Instance.GetWorldPosition(_unit.GetGridPosition()));
            ActionComplete();
        }

    }
    public override string GetActionName()
    {
        return "Look At";
    }

    public override BombRunEnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new BombRunEnemyAIAction
        {
            _GridPosition = gridPosition,
            _ActionValue = -10000

        };
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = _unit.GetGridPosition();
        return GetValidActionGridPositionList(unitGridPosition);
    }
    public List<GridPosition> GetValidActionGridPositionList(GridPosition unitGridPosition)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        // From the player's starting position, cycle through the grid in the x and z axises and check if a valid grid position exists there.
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                GridPosition offsetGridPosition = new GridPosition(x, y);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;
                validGridPositionList.Add(testGridPosition);
            }
        }
        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete, BodyPart bodyPart = BodyPart.None)
    {
        Debug.Log("TakeAction: LookAtAction");

        _targetLookAtPosition = gridPosition;

        ActionStart(onActionComplete);
    }

}
