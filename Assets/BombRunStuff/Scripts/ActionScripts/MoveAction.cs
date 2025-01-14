using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    private List<Vector3> _positionList;
    private Vector3 _targetPosition;
    private int _currentPositionIndex = 0;
    private float _moveSpeed = 4f;
    private float _stoppingDistance = 0.05f;
    

    [Header("Animation")]
    [SerializeField] private Animator _unitAnimator;


    private void Update()
    {
        if (!_isActive)
            return;

        _targetPosition = _positionList[_currentPositionIndex];
        if (Vector2.Distance(transform.position, _targetPosition) > _stoppingDistance)
        {
            Vector3 moveDirection = (_targetPosition - this.transform.position).normalized;
            transform.position += moveDirection * _moveSpeed * Time.deltaTime;
        }
        else
        {
            _currentPositionIndex++;
            // check if the position index is larger than the position list. If so, action has completed
            if (_currentPositionIndex >= _positionList.Count())
            {
                _isActive = false;
                _onActionComplete();
            }
            
        }
    }
    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        // Get the path to the end position
        //List<GridPosition> pathGridPositionList =  PathFinding.Instance.FindPath(_unit.GetGridPosition(), gridPosition, out int pathLength);
        List<GridPosition> pathGridPositionList = PathFinding.Instance.FindPathDots(_unit.GetGridPosition(), gridPosition, out int pathLength);

        //string pathString = "";
        //for (int i = 0; i < pathGridPositionList.Count; i++)
        //{
        //    pathString += i.ToString() + ": " + pathGridPositionList[i].ToString() + " ";
        //}
        //Debug.Log("PathFinding: Setup test path: " + pathString);

        //Debug.Log("MoveAction: TakeAction: Path starting at: " + _unit.GetGridPosition() + " and ending at: " + gridPosition + ": path length: " + pathLength + " and now the path: " + pathString);
        _currentPositionIndex = 0;
        _positionList = new List<Vector3>();

        foreach (GridPosition pathGridPosition in pathGridPositionList)
        {
            _positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPosition));
        }

        _onActionComplete = onActionComplete;
        _isActive = true;
    }
    public override List<GridPosition> GetValidActionGridPositionList()
    {
        float startTime = Time.realtimeSinceStartup;
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = _unit.GetGridPosition();
        for (int x = -_maxMoveDistance; x <= _maxMoveDistance; x++)
        {
            for (int y = -_maxMoveDistance; y <= _maxMoveDistance; y++)
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
                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    // Grid position already occupied by a unit
                    // later will check if number of units is below max allowed on a tile
                    // Will need to take into account if the number of selected units would put the testGridPosition above the max units allowed
                    continue;
                }
                if (!PathFinding.Instance.IsWalkableGridPosition(testGridPosition))
                {
                    continue;
                }
                if (!PathFinding.Instance.HasPath(unitGridPosition, testGridPosition, out int pathLength))
                {
                    continue;
                }
                //Debug.Log("GetValidActionGridPositionList: Valid position at: " + testGridPosition.ToString() + " with a length of: " + pathLength.ToString());
                // Get the length of the path and make sure it does not exceed the unit's max moving distance
                int pathFindingDistanceMultiplier = 10;
                if (pathLength > _maxMoveDistance * pathFindingDistanceMultiplier) // pathLength was returned by the HasPath call above. Doing this instead of calling PathFinding.Instance.GetPathLength so that the same path isn't calculated twice
                {
                    // path length is too long
                    continue;
                }
                validGridPositionList.Add(testGridPosition);
            }
        }
        Debug.Log("Time: Not-Dots: " + ((Time.realtimeSinceStartup - startTime) * 1000f));
        return validGridPositionList;
        // sort positions by closest to the unit position?
        //return validGridPositionList.OrderBy(gp => GridPosition.Distance(unitGridPosition, gp)).ToList();
    }

    public override string GetActionName()
    {
        return "Move";
    }
    public override BombRunEnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        int actionValue = 0;
        // Get the list of Grid Positions that have enemies on them
        List<GridPosition> enemyGridPositions = GetGridPositionsWithEnemyUnits();
        if (enemyGridPositions.Count > 0)
        {
            actionValue = GetActionValueOfMove(enemyGridPositions, gridPosition);
        }

        return new BombRunEnemyAIAction
        {
            _GridPosition = gridPosition,
            _ActionValue = actionValue,
        };
    }
    private List<GridPosition> GetGridPositionsWithEnemyUnits()
    {
        List<GridPosition> enemyUnitGridPositions = new List<GridPosition>();
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
            GridPosition unitGridPosition = unit.GetGridPosition();

            if (!LevelGrid.Instance.IsValidGridPosition(unitGridPosition))
                continue;
            if (enemyUnitGridPositions.Contains(unitGridPosition))
                continue;

            // later add a check to make sure this unit can "see" this grid position aka the position is not hidden by fog of war

            enemyUnitGridPositions.Add(unitGridPosition);
        }

        return enemyUnitGridPositions;
    }
    private int GetActionValueOfMove(List<GridPosition> enemyGridPositions, GridPosition gridPosition)
    {
        int actionValue = 0;
        // Get the number of units at each grid position
        // compare to number of friendly units at this position?
        bool firstCheck = true;
        foreach (GridPosition enemyGridPosition in enemyGridPositions)
        {
            // in the future instead of just comparing number of units on the position, it would test for the combat power of all units at the given position?
            // should also take into account tiles immediately near the enemy position to account for possible reinforcements? Same for immediate grid positions by gridPosition for friendly reinforcements
            // Will also want to check for things like: How many friendly units can also move to this gridPosition? And what is their combined combat power?
            // // maintain list of friendly units that "can move" to this gridPosition. Calculate maximum combat power at the gridPosition given unit limits on the gridPosition
            // // then look at remaining friendly units that weren't calculated in the move to gridPosition. Can any of them move to immediate nearby tiles? If so, how many can, and what is the maximum combat power of the reinforcement units?
            // How many enemy units (that this unit can see) can move to the enemyGridPosition position, and what is their combined combat power?
            // // same calculations as above. Track what enemy units were "moved" to the enemyGridPosition, maximize their combat power, and then check nearby tiles and how many enemy units can reinforce from there?
            // some calculation or "guess" on if an enemy unit will move during the turn? Will they move, or defend? Baseed on nearby friendly units, distance to the end goal, nearby enemy units?

            int numberOfEnemiesOnPosition = LevelGrid.Instance.GetUnitListAtGridPosition(enemyGridPosition).Count;
            int numberOfFriendlyUnits = LevelGrid.Instance.GetUnitListAtGridPosition(gridPosition).Count + LevelGrid.Instance.GetUnitListAtGridPosition(_unit.GetGridPosition()).Count;
            // old pre path-finding
            //int distanceFromEnemy = GridPosition.CalculateDistance(gridPosition, enemyGridPosition);
            // old pre-pathfinding

            // using path finding to get length of the path to this point
            int distanceFromEnemy = PathFinding.Instance.GetPathLength(gridPosition, enemyGridPosition);

            //int otherDistance = GridPosition.CalculateDistance(gridPosition, enemyGridPosition);
            //Debug.Log("GetActionValueOfMove: distance calculations: Positions: " + gridPosition.ToString() + ":" + enemyGridPosition.ToString() + " distances: " + distanceFromEnemy.ToString() + " : " + otherDistance.ToString());


            int newActionValue = 100 + ((numberOfFriendlyUnits - numberOfEnemiesOnPosition) * 100) - distanceFromEnemy;

            Debug.Log("GetActionValueOfMove: to move to: " + gridPosition.ToString() + ": " + newActionValue.ToString() + " compared to: " + actionValue.ToString());
            Debug.Log("GetActionValueOfMove: calculation: numberOfEnemiesOnPosition: " + numberOfEnemiesOnPosition.ToString() + " numberOfFriendlyUnits: " + numberOfFriendlyUnits.ToString() + " distanceFromEnemy: " + distanceFromEnemy.ToString());
            if (firstCheck)
            {
                actionValue = newActionValue;
                firstCheck = false;
            }
            if (newActionValue > actionValue)
            {
                actionValue = newActionValue;
            }
        }

        return actionValue;
    }
}
