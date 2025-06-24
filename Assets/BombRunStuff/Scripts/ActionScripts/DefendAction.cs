using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefendAction : BaseAction
{
    [SerializeField] private int _maxReinforceDistance = 1;
    [SerializeField] private int _enemyNearbyDistance = 3;
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

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete, BodyPart bodyPart = BodyPart.None)
    {
        //_onActionComplete = onActionComplete;
        //_isActive = true;
        ActionStart(onActionComplete);
        FinishAction();
    }
    private void FinishAction()
    {
        //_isActive = false;
        //_onActionComplete();
        ActionComplete();
    }
    public int GetMaxReinforceDistance()
    {
        return _maxReinforceDistance;
    }
    public override BombRunEnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        int unitsNearby = GetNumberOfNearbyEnemyUnits(gridPosition);
        return new BombRunEnemyAIAction
        {
            _GridPosition = gridPosition,
            _ActionValue = unitsNearby * 5,
        };
    }
    private int GetNumberOfNearbyEnemyUnits(GridPosition gridPosition)
    {
        int unitsNearby = 0;
        
        List<BombRunUnit> units = new List<BombRunUnit>();

        if (this._unit.IsEnemy())
        {
            units.AddRange(BombRunUnitManager.Instance.GetFriendlyUnitList());
        }
        else
        {
            units.AddRange(BombRunUnitManager.Instance.GetEnemyUnitList());
        }

        foreach (BombRunUnit unit in units)
        {
            int gridDistance = LevelGrid.Instance.CalculateDistance(gridPosition, unit.GetGridPosition());
            if (gridDistance <= _enemyNearbyDistance)
            {
                unitsNearby++;
            }
        }

        return unitsNearby;
    }
}
