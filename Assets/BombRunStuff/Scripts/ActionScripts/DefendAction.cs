using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefendAction : BaseAction
{
    [Header("DefendAction")]
    [SerializeField] private int _maxReinforceDistance = 1;
    [SerializeField] private int _enemyNearbyDistance = 3;
    [SerializeField] private GridPosition _positionDefendingFrom;
    [SerializeField] private Vector2Int _positionDefendingFromVector2Int;

    protected override void Start()
    {
        base.Start();
        BaseBombRunObstacle.OnAnyObstacleDestroyed += BaseBombRunObstacle_OnAnyObstacleDestroyed;
        BaseBombRunObstacle.OnAnyObstacleCoverTypeChanged += BaseBombRunObstacle_OnAnyObstacleCoverTypeChanged;

        _unit.OnUnitStateChanged += Unit_OnUnitStateChanged;
    }

    

    protected override void OnDisable()
    {
        base.OnDisable();
        BaseBombRunObstacle.OnAnyObstacleDestroyed -= BaseBombRunObstacle_OnAnyObstacleDestroyed;
        BaseBombRunObstacle.OnAnyObstacleCoverTypeChanged -= BaseBombRunObstacle_OnAnyObstacleCoverTypeChanged;

        _unit.OnUnitStateChanged -= Unit_OnUnitStateChanged;
    }
    public override string GetActionName()
    {
        return "Defend";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = _unit.GetGridPosition();
        List<GridPosition> positionsToDefendFrom = GetPositionsToDefendFrom(unitGridPosition);

        return positionsToDefendFrom;
    }
    private List<GridPosition> GetPositionsToDefendFrom(GridPosition gridPosition)
    {
        List<GridPosition> positionsToCheck = LevelGrid.Instance.GetDefensePositions(gridPosition);
        List<GridPosition> validDefendPositions = new List<GridPosition>();

        foreach (GridPosition positionToCheck in positionsToCheck)
        {
            if (!LevelGrid.Instance.IsValidGridPosition(positionToCheck))
            {
                validDefendPositions.Add(positionToCheck);
                continue;
            }
            if (LevelGrid.Instance.HasWallOnGridPosition(positionToCheck))
            {
                validDefendPositions.Add(positionToCheck);
                continue;
            }
            if (LevelGrid.Instance.HasAnyObstacleOnGridPosition(positionToCheck))
            {
                BaseBombRunObstacle obstacle = LevelGrid.Instance.GetObstacleAtGridPosition(positionToCheck);
                if (obstacle.GetObstacleCoverType() == ObstacleCoverType.None)
                {
                    continue;
                }
                validDefendPositions.Add(positionToCheck);
                continue;
            }
        }

        return validDefendPositions;
    }
    private void BaseBombRunObstacle_OnAnyObstacleCoverTypeChanged(object sender, GridPosition gridPosition)
    {
        if (_unit.GetUnitState() != UnitState.Defending)
            return;

        if (gridPosition == _positionDefendingFrom)
        {
            _unit.SetUnitState(UnitState.Idle);
        }
    }

    private void BaseBombRunObstacle_OnAnyObstacleDestroyed(object sender, GridPosition gridPosition)
    {
        if (_unit.GetUnitState() != UnitState.Defending)
            return;

        if (gridPosition == _positionDefendingFrom)
        {
            _unit.SetUnitState(UnitState.Idle);
        }
    }
    private void Unit_OnUnitStateChanged(object sender, UnitState unitState)
    {
        if (unitState == UnitState.Defending)
        {
            return;
        }
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete, BodyPart bodyPart = BodyPart.None)
    {
        //_onActionComplete = onActionComplete;
        //_isActive = true;
        ActionStart(onActionComplete);
        //_unit.SetUnitState(UnitState.Defending);

        this._positionDefendingFrom = gridPosition;
        this._positionDefendingFromVector2Int = new Vector2Int(gridPosition.x, gridPosition.y);

        // have unit look away from the position they are defending against?
        _unit.SetActionDirection(LevelGrid.Instance.GetWorldPosition(_unit.GetGridPosition()) - LevelGrid.Instance.GetWorldPosition(gridPosition));

        UnitVisibilityManager_BombRun.Instance.CheckIfUnitCanBeSeenByOpposingTeam(this._unit);
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
    public GridPosition GetPositionDefendingFrom()
    {
        //return _positionDefendingFrom;
        return new GridPosition(_positionDefendingFromVector2Int.x, _positionDefendingFromVector2Int.y);
    }
}
