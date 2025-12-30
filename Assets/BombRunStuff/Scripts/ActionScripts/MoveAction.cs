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

    // events?
    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;
    public event EventHandler<bool> OnChangeDirection;

    [Header("Moving")]
    [SerializeField] private int _maxMoveDistance = 4;
    private List<Vector3> _positionList;
    private Vector3 _targetPosition;
    private int _currentPositionIndex = 0;
    private float _moveSpeed = 4f;
    private float _stoppingDistance = 0.05f;
    

    // cache the last valid action list so it doesn't need to be recalculated for every mouse click?
    private Dictionary<GridPosition, List<GridPosition>> _cachedValidActionList = new Dictionary<GridPosition, List<GridPosition>>();

    protected override void Start()
    {
        base.Start();
        PathFinding.Instance.IsWalkableUpdated += PathFinding_IsWalkableUpdated;
        BombRunUnit.OnAnyActionPointsChanged += BombRunUnit_OnAnyActionPointsChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        PathFinding.Instance.IsWalkableUpdated -= PathFinding_IsWalkableUpdated;
        BombRunUnit.OnAnyActionPointsChanged -= BombRunUnit_OnAnyActionPointsChanged;
    }
    private void PathFinding_IsWalkableUpdated(object sender, GridPosition gridPosition)
    {
        ResetCachedValidPositionList();
    }
    private void BombRunUnit_OnAnyActionPointsChanged(object sender, EventArgs e)
    {
        ResetCachedValidPositionList();
    }
    private void Update()
    {
        if (!_isActive)
            return;

        _targetPosition = _positionList[_currentPositionIndex];
        Vector3 unitPosition = this.transform.position;
        if (Vector2.Distance(unitPosition, _targetPosition) > _stoppingDistance)
        {
            Vector3 moveDirection = (_targetPosition - unitPosition).normalized;

            //CheckIfSpriteShouldFlip(moveDirection);

            transform.position += moveDirection * _moveSpeed * Time.deltaTime;
        }
        else
        {
            _currentPositionIndex++;
            // check if the position index is larger than the position list. If so, action has completed
            if (_currentPositionIndex >= _positionList.Count())
            {
                //_isActive = false;
                OnStopMoving?.Invoke(this, EventArgs.Empty);
                _unit.SetUnitState(UnitState.Idle);
                //_onActionComplete();
                ActionComplete();
            }
            else
            {
                _unit.SetActionDirection(_positionList[_currentPositionIndex] - LevelGrid.Instance.GetWorldPosition(_unit.GetGridPosition()));
            }
            
        }
    }
    protected override void BombRunUnitHealthSystem_OnBodyPartFrozenStateChanged(object sender, BodyPart bodyPart)
    {
        base.BombRunUnitHealthSystem_OnBodyPartFrozenStateChanged(sender, bodyPart);

        if (bodyPart == BodyPart.Legs)
        {
            if (_unit.GetUnitHealthSystem().GetBodyPartFrozenState(bodyPart) != BodyPartFrozenState.FullFrozen)
            {
                ResetCachedValidPositionList();
            }
            
        }

    }
    private void CheckIfSpriteShouldFlip(Vector3 moveDirection)
    {
        if (moveDirection.x < 0)
        {
            _bombRunUnitAnimator.FlipSprite(true);
        }
        else
        {
            _bombRunUnitAnimator.FlipSprite(false);
        }
    }
    public int GetMaxMoveDistance()
    {
        return _maxMoveDistance;
    }
    public void SetMaxMoveDistance(int maxMoveDistance)
    {
        if (maxMoveDistance < 0)
            return;
        _maxMoveDistance = maxMoveDistance;
        ResetCachedValidPositionList();
    }
    void ResetCachedValidPositionList()
    {
        _cachedValidActionList.Clear();
    }
    public override void TakeAction(GridPosition gridPosition, Action onActionComplete, BodyPart bodyPart = BodyPart.None)
    {
        // Get the path to the end position
        List<GridPosition> pathGridPositionList =  PathFinding.Instance.FindPath(_unit.GetGridPosition(), gridPosition, out int pathLength, _maxMoveDistance);

        _currentPositionIndex = 0;
        _positionList = new List<Vector3>();

        if (pathGridPositionList == null)
        {
            {
                _onActionComplete = onActionComplete;
                StartCoroutine(WaitForBadMove());
                return;
            }

        }
        foreach (GridPosition pathGridPosition in pathGridPositionList)
        {
            _positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPosition));
        }

        ActionStart(onActionComplete);
        OnStartMoving?.Invoke(this, EventArgs.Empty);
        _unit.SetUnitState(UnitState.Moving);
    }
    public override List<GridPosition> GetValidActionGridPositionList()
    {
        //float startTime = Time.realtimeSinceStartup;
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        // For testing jobs?
        //List<GridPosition> gridPositionsToTest = new List<GridPosition>();

        GridPosition unitGridPosition = _unit.GetGridPosition();

        if (_cachedValidActionList.ContainsKey(unitGridPosition))
        {
            Debug.Log("GetValidActionGridPositionList: repeating for grid position: " + unitGridPosition.ToString() + " returning cached list?");
            //Debug.Log("Time: Not-Dots: " + ((Time.realtimeSinceStartup - startTime) * 1000f));
            return _cachedValidActionList[unitGridPosition];
        }
        _cachedValidActionList.Clear();
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
                // check the distance to the target grid position. This will be the distance assuming no walls or anything. If distance with that is greater than distance max, skip
                int pathFindingDistanceMultiplier = 10;
                if (LevelGrid.Instance.CalculateDistance(unitGridPosition, testGridPosition) > _maxMoveDistance * pathFindingDistanceMultiplier)
                {
                    continue;
                }
                if (!PathFinding.Instance.HasPath(unitGridPosition, testGridPosition, out int pathLength, _maxMoveDistance))
                {
                    continue;
                }
                //Debug.Log("GetValidActionGridPositionList: Valid position at: " + testGridPosition.ToString() + " with a length of: " + pathLength.ToString());
                // Get the length of the path and make sure it does not exceed the unit's max moving distance
                
                if (pathLength > _maxMoveDistance * pathFindingDistanceMultiplier) // pathLength was returned by the HasPath call above. Doing this instead of calling PathFinding.Instance.GetPathLength so that the same path isn't calculated twice
                {
                    // path length is too long
                    //Debug.Log("GetValidActionGridPositionList: Path length from: " + unitGridPosition.ToString() + " to: " + testGridPosition.ToString() + " is: " + pathLength + " which is too far! Max movedistance is: " + _maxMoveDistance + " ("+ (_maxMoveDistance * pathFindingDistanceMultiplier).ToString() +")");
                    continue;
                }
                validGridPositionList.Add(testGridPosition);
            }
        }
        //Debug.Log("Time: Not-Dots: " + ((Time.realtimeSinceStartup - startTime) * 1000f));
        _cachedValidActionList.Add(unitGridPosition, validGridPositionList);

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
            // first check for targets to shoot at?
            actionValue = GetActionValueOfMove(gridPosition);
            if (actionValue == 0)
            {
                Debug.Log("GetEnemyAIAction: MoveAction: Did not find any shootable targets at position: " + gridPosition + " Checking for other move options...");
                actionValue = GetActionValueOfMove(enemyGridPositions, gridPosition);
            }            
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
    private int GetActionValueOfMove(GridPosition gridPosition)
    {
        ShootAction unitShootAction = _unit.GetAction<ShootAction>();
        if (unitShootAction == null)
        {
            return 0;
        }

        int targetsAtPosition = unitShootAction.GetTargetCountAtGridPosition(gridPosition);
        return 100 + (targetsAtPosition * 10);
    }
    // This is the check to see where to move when the unit cannot move to a new position that has an enemy to shoot at
    // should prioritize getting closer to enemies
    // should also prioritize finding a position with "cover" once I implement that...

    // Another movement check - if there are no valid moves found, find the nearest enemy
    // calculate the path to that enemy with no max movement restriction. If there is one that exists, pick the furthest tile furthest along that path that the unit can move to
    // this is to try and get "better" movement actions from the AI when there is a wall between them and a player unit, and going around the wall would be beyond one movement move?
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
    IEnumerator WaitForBadMove()
    {
        Debug.Log("WaitForBadMove: ");
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(2f);
        _onActionComplete();
    }
    public override GridPosition GetNearestValidGridPosition(GridPosition targetGridPosition)
    {
        //Debug.Log("GetNearestValidGridPosition: MoveAction: " + targetGridPosition);
        GridPosition nearestPosition = targetGridPosition;
        GridPosition unitGridPosition = _unit.GetGridPosition();
        List<GridPosition> gridPositionList = new List<GridPosition>();

        if (_cachedValidActionList.ContainsKey(unitGridPosition))
        {
            if (_cachedValidActionList[unitGridPosition].Count == 0)
            {
                //Debug.Log("GetNearestValidGridPosition: MoveAction: _cachedValidActionList's grid position list is empty for: " + unitGridPosition + " no valid moves?");
                return nearestPosition;
            }
            gridPositionList.AddRange(_cachedValidActionList[unitGridPosition]);
        }
        else
        {
            //Debug.Log("GetNearestValidGridPosition: MoveAction: _cachedValidActionList is empty. Calculate the grid position list for unit's current position?");
            gridPositionList.AddRange(GetValidActionGridPositionList());
        }

        if (gridPositionList.Count == 0)
        {
            Debug.Log("GetNearestValidGridPosition: MoveAction: no valid move positions? gridPositionList is empty. target position: " + targetGridPosition);
            return nearestPosition;
        }
        BombRunUnitFieldOfView bombRunUnitFieldOfView = _unit.GetBombRunUnitFieldOfView();
        Vector3 targetWorldPosition = LevelGrid.Instance.GetWorldPosition(targetGridPosition);
        Vector3 unitWorldPosition = _unit.GetWorldPosition();
        Vector3 directionToTargetWorldPosition = bombRunUnitFieldOfView.ConvertDirectionToAngleVector((targetWorldPosition - unitWorldPosition), true);
        float fovAngle = 90f; // just for testing now? grab this from somewhere else eventually?
        bool firstPositionInFOVFound = false;
        //float distanceToUnitTargetPosition = Vector2.Distance(targetWorldPosition, _unit.GetWorldPosition());
        float closestDistance = 0f;

        foreach (GridPosition testGridPosition in gridPositionList)
        {
            Vector3 testWorldPosition = LevelGrid.Instance.GetWorldPosition(testGridPosition);
            Vector3 directionToTestPosition = (testWorldPosition - unitWorldPosition).normalized;
            
            if (bombRunUnitFieldOfView.IsGridPositionWithinFOV(directionToTestPosition, fovAngle, directionToTargetWorldPosition))
            {
                //Debug.Log("GetNearestValidGridPosition: MoveAction: IN FIELD OF VIEW: Test Position: " + testGridPosition + " direction to test position: " + directionToTestPosition + " fov angle: " + fovAngle + " direction to target grid position: " + directionToTargetWorldPosition);
                if (!firstPositionInFOVFound)
                {
                    //Debug.Log("GetNearestValidGridPosition: MoveAction: First test position check. Test position: " + testGridPosition);
                    nearestPosition = testGridPosition;
                    closestDistance = Vector2.Distance(targetWorldPosition, testWorldPosition);
                    firstPositionInFOVFound = true;

                }

                float testPositionDistance = Vector2.Distance(targetWorldPosition, testWorldPosition);
                if (testPositionDistance < closestDistance)
                {
                    //Debug.Log("GetNearestValidGridPosition: MoveAction: NEW closest test position check. Test position: " + testGridPosition);
                    nearestPosition = testGridPosition;
                    closestDistance = testPositionDistance;
                }
            }
        }

        return nearestPosition;
    }
}
