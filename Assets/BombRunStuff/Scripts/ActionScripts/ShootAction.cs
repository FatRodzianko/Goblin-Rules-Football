using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShootAction : BaseAction
{
    // events
    public event EventHandler<OnStartShootingEventArgs> OnStartShooting;
    public event EventHandler OnStopShooting;

    public class OnStartShootingEventArgs : EventArgs 
    {
        public BombRunUnit TargetUnit;
        public BombRunUnit ShootingUnit;
    }

    private enum State
    {
        Aiming,
        Shooting,
        Cooloff,
    }
    private State _state;
    private float _stateTimer;
    private float _aimingStateTime = 0.5f;
    private float _shootStateTime = 0.1f;
    private float _coolOffStateTime = 0.5f;

    [SerializeField] private int _maxShootDistance = 10;

    private BombRunUnit _targetUnit;
    private Vector3 _targetUnitWorldPosition;
    private bool _canShootBullet = false;

    // Animation stuff
    private float _totalSpinAmount;
    private float _maxSpinAmount = 360f;
    private bool _aiming = false;

    [Header("Shooting Stuff?")]
    [SerializeField] private List<LayerMask> _shotBlockerLayerMasks = new List<LayerMask>();
    [SerializeField] private float _minDistanceToTravelThroughObstacle = 0.25f;
    [SerializeField] private float _circleCastRadius = 0.2f;

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
                    //SpinSprite();
                    TurnTowardTarget(_targetUnitWorldPosition);
                    // add animation for unit to get into shooting position. If kneeling, stand up? Hold gun to shoulder, whatever
                    // once that animation completes, then set _aiming = false to go to NextState?
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
    private void TurnTowardTarget(Vector3 targetPostion)
    {
        Vector3 directionToTarget = (targetPostion - _unit.transform.position).normalized;
        if (directionToTarget.x < 0)
        {
            _bombRunUnitAnimator.FlipSprite(true);
        }
        else
        {
            _bombRunUnitAnimator.FlipSprite(false);
        }
    }
    private void SpinSprite()
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
        OnStartShooting?.Invoke(this, new OnStartShootingEventArgs { 
            TargetUnit = _targetUnit,
            ShootingUnit = _unit
        });
        //_targetUnit.Damage(35);
    }
    public override string GetActionName()
    {
        return "Shoot";
    }
    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = _unit.GetGridPosition();
        return GetValidActionGridPositionList(unitGridPosition);
    }
    public List<GridPosition> GetValidActionGridPositionList(GridPosition gridPosition)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        //GridPosition unitGridPosition = _unit.GetGridPosition();
        GridPosition unitGridPosition = gridPosition;

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
                if (DoesShotHitWallOrObstacle(unitGridPosition, testGridPosition))
                {
                    continue;
                }
                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }
    //
    // OLD
    //
    //private bool DoesShotHitWallOrObstacle(GridPosition unitPosition, GridPosition targetPosition)
    //{
    //    //Debug.Log("DoesShotHitWallOrObstacle: Unit Position: " + unitPosition + " Target position: " + targetPosition);
    //    Vector2 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitPosition);
    //    Vector2 targetWorldPosition = LevelGrid.Instance.GetWorldPosition(targetPosition);
    //    Vector2 shootDirection = (targetWorldPosition - unitWorldPosition).normalized;

    //    BombRunUnit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(targetPosition);

    //    float distance = Vector2.Distance(unitWorldPosition, targetWorldPosition);
    //    //Debug.Log("DoesShotHitWallOrObstacle: Unit Position: " + unitWorldPosition + " Target position: " + targetWorldPosition);

    //    Physics2D.queriesStartInColliders = false;
    //    foreach (LayerMask mask in _shotBlockerLayerMasks)
    //    {
    //        //RaycastHit2D[] hits = Physics2D.RaycastAll(unitWorldPosition, shootDirection, distance, mask);
    //        RaycastHit2D forwardHit = Physics2D.Raycast(unitWorldPosition, shootDirection, distance, mask);
    //        if (forwardHit.collider != null)
    //        {
    //            RaycastHit2D reverseHit = Physics2D.Raycast(targetWorldPosition, (unitWorldPosition - targetWorldPosition).normalized, distance, mask);


    //            Vector2 hit = forwardHit.point;

    //            if (reverseHit.collider != null)
    //            {
    //                Vector2 reverseHitPoint = reverseHit.point;

    //                Debug.Log("DoesShotHitWallOrObstacle: Target Position: " + targetWorldPosition + " Hit collider: " + forwardHit.collider.name + ":" + hit + " Reverse hit collider: " + reverseHit.collider.name + ":" + reverseHitPoint);

    //                if (hit == reverseHitPoint)
    //                {
    //                    Debug.Log("DoesShotHitWallOrObstacle: Hit Wall. BUT the reverse hit is the same. This means a corner was hit? First hit position: " + hit + " Second hit position: " + reverseHitPoint + " Target position: " + targetWorldPosition + " hit object: " + forwardHit.collider.name);
    //                    continue;
    //                }
    //                float distanceBetweenHits = Vector2.Distance(hit, reverseHitPoint);
    //                if (distanceBetweenHits < _minDistanceToTravelThroughObstacle)
    //                {
    //                    Debug.Log("DoesShotHitWallOrObstacle: Hit Wall. BUT the distance between hit and reverse hit is less than " + _minDistanceToTravelThroughObstacle + ". This means a corner was hit? First hit position: " + hit + " Second hit position: " + reverseHitPoint + " Distance between the hits: " + distanceBetweenHits + " Target position: " + targetWorldPosition);
    //                    continue;
    //                }
    //            }

    //            //Debug.Log("DoesShotHitWallOrObstacle: distanceBetweenHits: " + distanceBetweenHits + " Hit point: " + hit + " reverse hit point: " + reverseHitPoint + " hit object?: " + forwardHit.collider.name);
    //            if (forwardHit.collider.CompareTag("BombRunWall"))
    //            {
    //                //Debug.Log("DoesShotHitWallOrObstacle: Hit Wall. Wall position: " + LevelGrid.Instance.GetGridPositon(forwardHit.point) + " Target position: " + targetPosition);
    //                Debug.Log("DoesShotHitWallOrObstacle: Hit Wall. Wall position: " + forwardHit.point + " Target position: " + targetWorldPosition);
    //                return true;
    //            }
    //            if (forwardHit.collider.CompareTag("BombRunObstacle"))
    //            {
    //                if (forwardHit.collider.GetComponent<BaseBombRunObstacle>().GetObstacleCoverType() == ObstacleCoverType.Full)
    //                {
    //                    //Debug.Log("DoesShotHitWallOrObstacle: Hit Obstacle with Full Cover. Obstacle position: " + LevelGrid.Instance.GetGridPositon(forwardHit.point) + " Target position: " + targetPosition);
    //                    Debug.Log("DoesShotHitWallOrObstacle: Hit Obstacle with Full Cover. Obstacle position: " + forwardHit.point + " Target position: " + targetWorldPosition);
    //                    return true;
    //                }
    //            }
    //            if (forwardHit.collider.CompareTag("Goblin"))
    //            {
    //                if (forwardHit.collider.TryGetComponent<BombRunUnit>(out BombRunUnit hitGoblin))
    //                {
    //                    if (hitGoblin == this._unit)
    //                    {
    //                        Debug.Log("DoesShotHitWallOrObstacle: Hit goblin but it is the shooting goblin?. Goblin position: " + forwardHit.point + " Target position: " + targetWorldPosition);
    //                        continue;
    //                    }
    //                    if (hitGoblin != targetUnit)
    //                    {
    //                        Debug.Log("DoesShotHitWallOrObstacle: Hit goblin that was not the target. Goblin position: " + forwardHit.point + " Target position: " + targetWorldPosition);
    //                        return true;
    //                    }
    //                }

    //            }


    //        }
    //    }
    //    Physics2D.queriesStartInColliders = true;
    //    Debug.Log("DoesShotHitWallOrObstacle: No walls or obstacles hit for position: " + targetWorldPosition);

    //    return false;
    //}
    //
    // OLD
    //

    //
    // NEWER 3/25/2025
    //
    //private bool DoesShotHitWallOrObstacle(GridPosition unitPosition, GridPosition targetPosition)
    //{
    //    //Debug.Log("DoesShotHitWallOrObstacle: Unit Position: " + unitPosition + " Target position: " + targetPosition);
    //    Vector2 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitPosition);
    //    Vector2 targetWorldPosition = LevelGrid.Instance.GetWorldPosition(targetPosition);
    //    Vector2 shootDirection = (targetWorldPosition - unitWorldPosition).normalized;

    //    BombRunUnit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(targetPosition);

    //    float distance = Vector2.Distance(unitWorldPosition, targetWorldPosition);
    //    //Debug.Log("DoesShotHitWallOrObstacle: Unit Position: " + unitWorldPosition + " Target position: " + targetWorldPosition);

    //    foreach (LayerMask mask in _shotBlockerLayerMasks)
    //    {
    //        RaycastHit2D[] hits = Physics2D.RaycastAll(unitWorldPosition, shootDirection, distance, mask);
    //        //RaycastHit2D hit = Physics2D.Raycast(unitWorldPosition, shootDirection, distance, mask);
    //        if (hits.Length > 0)
    //        {
    //            RaycastHit2D[] reverseHits = Physics2D.RaycastAll(targetWorldPosition, (unitWorldPosition - targetWorldPosition).normalized, distance, mask);

    //            if (hits.Length != reverseHits.Length)
    //            {
    //                List<RaycastHit2D> forwardHitList = hits.ToList();
    //                List<RaycastHit2D> reverseHitList = reverseHits.ToList();
    //                Debug.Log("DoesShotHitWallOrObstacle: forward and reverse hit counts do not match???. Checking to see if any are hitting obstacles...");
    //                foreach (RaycastHit2D hit in reverseHitList)
    //                {
    //                    if (!forwardHitList.Contains(hit))
    //                    {
    //                        Debug.Log("DoesShotHitWallOrObstacle: hit from reverse hit list: " + hit.point + " not found in forward hit list. returning true.");
    //                        return true;
    //                    }
    //                }
    //                foreach (RaycastHit2D hit in forwardHitList)
    //                {
    //                    if (!reverseHitList.Contains(hit))
    //                    {
    //                        Debug.Log("DoesShotHitWallOrObstacle: hit from forward hit list: " + hit.point + " not found in reverse hit list. returning true.");
    //                        return true;
    //                    }
    //                }
    //            }

    //            for (int i = 0; i < hits.Length; i++)
    //            {
    //                Vector2 hit = hits[i].point;
    //                Vector2 reverseHit = reverseHits[reverseHits.Length - 1 - i].point;
    //                //Vector2 reverseHit = reverseHits[i].point;

    //                //Debug.Log("DoesShotHitWallOrObstacle: Hit collider: " + hits[i].collider.name + ":" + hit + " Reverse hit collider: " + reverseHits[i].collider.name + ":" + reverseHit);
    //                Debug.Log("DoesShotHitWallOrObstacle: Target Position: " + targetWorldPosition + " Hit collider: " + hits[i].collider.name + ":" + hit + " Reverse hit collider: " + reverseHits[reverseHits.Length - 1 - i].collider.name + ":" + reverseHit + " Total hits: " + hits.Length + ":" + reverseHits.Length);



    //                if (hit == reverseHit)
    //                {
    //                    Debug.Log("DoesShotHitWallOrObstacle: Hit Wall. BUT the reverse hit is the same. This means a corner was hit? First hit position: " + hit + " Second hit position: " + reverseHit + " Target position: " + targetWorldPosition + " hit object: " + hits[i].collider.name);
    //                    continue;
    //                }
    //                float distanceBetweenHits = Vector2.Distance(hit, reverseHit);
    //                if (distanceBetweenHits < _minDistanceToTravelThroughObstacle)
    //                {
    //                    Debug.Log("DoesShotHitWallOrObstacle: Hit Wall. BUT the distance between hit and reverse hit is less than " + _minDistanceToTravelThroughObstacle + ". This means a corner was hit? First hit position: " + hit + " Second hit position: " + reverseHit + " Distance between the hits: " + distanceBetweenHits + " Target position: " + targetWorldPosition);
    //                    continue;
    //                }
    //                //Debug.Log("DoesShotHitWallOrObstacle: distanceBetweenHits: " + distanceBetweenHits + " Hit point: " + hit + " reverse hit point: " + reverseHit + " hit object?: " + hits[i].collider.name);
    //                if (hits[i].collider.CompareTag("BombRunWall"))
    //                {
    //                    //Debug.Log("DoesShotHitWallOrObstacle: Hit Wall. Wall position: " + LevelGrid.Instance.GetGridPositon(hits[i].point) + " Target position: " + targetPosition);
    //                    Debug.Log("DoesShotHitWallOrObstacle: Hit Wall. Wall position: " + hits[i].point + " Target position: " + targetWorldPosition);
    //                    return true;
    //                }
    //                if (hits[i].collider.CompareTag("BombRunObstacle"))
    //                {
    //                    if (hits[i].collider.GetComponent<BaseBombRunObstacle>().GetObstacleCoverType() == ObstacleCoverType.Full)
    //                    {
    //                        //Debug.Log("DoesShotHitWallOrObstacle: Hit Obstacle with Full Cover. Obstacle position: " + LevelGrid.Instance.GetGridPositon(hits[i].point) + " Target position: " + targetPosition);
    //                        Debug.Log("DoesShotHitWallOrObstacle: Hit Obstacle with Full Cover. Obstacle position: " + hits[i].point + " Target position: " + targetWorldPosition);
    //                        return true;
    //                    }
    //                }
    //                if (hits[i].collider.CompareTag("Goblin"))
    //                {
    //                    if (hits[i].collider.TryGetComponent<BombRunUnit>(out BombRunUnit hitGoblin))
    //                    {
    //                        if (hitGoblin == this._unit)
    //                        {
    //                            continue;
    //                        }
    //                        if (hitGoblin != targetUnit)
    //                        {
    //                            Debug.Log("DoesShotHitWallOrObstacle: Hit goblin that was not the target. Goblin position: " + hits[i].point + " Target position: " + targetWorldPosition);
    //                            return true;
    //                        }
    //                    }

    //                }
    //            }

    //        }
    //    }

    //    Debug.Log("DoesShotHitWallOrObstacle: No walls or obstacles hit for position: " + targetWorldPosition);

    //    return false;
    //}
    //
    // NEWER 3/25/2025
    //
    private bool DoesShotHitWallOrObstacle(GridPosition unitPosition, GridPosition targetPosition)
    {

        Vector2 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitPosition);
        Vector2 targetWorldPosition = LevelGrid.Instance.GetWorldPosition(targetPosition);
        Vector2 shootDirection = (targetWorldPosition - unitWorldPosition).normalized;

        BombRunUnit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(targetPosition);

        float distance = Vector2.Distance(unitWorldPosition, targetWorldPosition);

        Debug.Log("DoesShotHitWallOrObstacle: shooter position: " + unitWorldPosition + " target position: " + targetWorldPosition);

        foreach (LayerMask mask in _shotBlockerLayerMasks)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(unitWorldPosition, shootDirection, distance, mask);

            if (hits.Length > 0)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    Debug.Log("DoesShotHitWallOrObstacle: hit " + hits[i].collider.name + " collider at: " + hits[i].point.ToString("0.00000000"));
                    //hits[i].collider.bounds
                    // check if the hit is on a corner?
                    if (IsCornerHit(hits[i].point))
                    {
                        Debug.Log("DoesShotHitWallOrObstacle: Hit corner of a collider at: " + hits[i].point.ToString());
                        // check to see if the shoot direction is moving away or toward the center of the hit collider
                        // if moving toward center of collider, it is a direct hit
                        // if moving away from center of collider, might be glancing blow on corner to ignore?
                        Vector2 directionToCenterOfCollider = ((Vector2)hits[i].collider.bounds.center - hits[i].point).normalized;
                        float angle = Vector2.Angle(directionToCenterOfCollider, shootDirection);
                        Debug.Log("DoesShotHitWallOrObstacle: Angle of corner hit: " + angle.ToString() + " hit point: " + hits[i].point + " shoot direction: " + shootDirection + " direction from hitpoint to collider center: " + directionToCenterOfCollider);

                        if (angle < 45f)
                        {
                            Debug.Log("DoesShotHitWallOrObstacle: angle indicates ray is moving TOWARD the collider. Corner hit into the wall. Wall hit?");
                            return true;
                        }
                        else
                        {
                            Debug.Log("DoesShotHitWallOrObstacle: angle indicates ray is moving AWAY from the collider. Glancing corner hit? Checking for surrounding colliders...");
                            RaycastHit2D[] circleCastHits = Physics2D.CircleCastAll(hits[i].point, _circleCastRadius, Vector2.zero, 0f, mask);
                            if (circleCastHits.Length > 0)
                            {
                                for (int z = 0; z < circleCastHits.Length; z++)
                                {
                                    if (circleCastHits[z].collider != hits[i].collider)
                                    {
                                        Debug.Log("DoesShotHitWallOrObstacle: Glancing cornder hit, but collider is near another collider. Should be blocked? Hit collider: " + hits[i].collider + " second collider: " + circleCastHits[z].collider.name);
                                        return true;
                                    }
                                }
                            }
                            // if the code makes it here, no other collider was near the corner. Glancing blow to skip rest of checks with "continue?"    
                            Debug.Log("DoesShotHitWallOrObstacle: Glancing cornder hit, AND no surrounding collider. Skipping this hit?");
                            continue;                            
                        }
                    }
                    else
                    {
                        Debug.Log("DoesShotHitWallOrObstacle: DID NOT Hit corner of a collider at: " + hits[i].point.ToString("0.0000000000"));
                    }
                    
                    if (hits[i].collider.CompareTag("BombRunWall"))
                    {
                        //Debug.Log("DoesShotHitWallOrObstacle: Hit Wall. Wall position: " + LevelGrid.Instance.GetGridPositon(hits[i].point) + " Target position: " + targetPosition);
                        Debug.Log("DoesShotHitWallOrObstacle: Hit Wall. Wall position: " + hits[i].point + " Target position: " + targetWorldPosition);
                        return true;
                    }
                    if (hits[i].collider.CompareTag("BombRunObstacle"))
                    {
                        if (hits[i].collider.GetComponent<BaseBombRunObstacle>().GetObstacleCoverType() == ObstacleCoverType.Full)
                        {
                            //Debug.Log("DoesShotHitWallOrObstacle: Hit Obstacle with Full Cover. Obstacle position: " + LevelGrid.Instance.GetGridPositon(hits[i].point) + " Target position: " + targetPosition);
                            Debug.Log("DoesShotHitWallOrObstacle: Hit Obstacle with Full Cover. Obstacle position: " + hits[i].point + " Target position: " + targetWorldPosition);
                            return true;
                        }
                    }
                    if (hits[i].collider.CompareTag("Goblin"))
                    {
                        if (hits[i].collider.TryGetComponent<BombRunUnit>(out BombRunUnit hitGoblin))
                        {
                            if (hitGoblin == this._unit)
                            {
                                continue;
                            }
                            if (hitGoblin != targetUnit)
                            {
                                Debug.Log("DoesShotHitWallOrObstacle: Hit goblin that was not the target. Goblin position: " + hits[i].point + " Target position: " + targetWorldPosition);
                                return true;
                            }
                        }

                    }
                    
                }
            }
        }

        Debug.Log("DoesShotHitWallOrObstacle: No walls or obstacles hit for position: " + targetWorldPosition);
        return false;
    }
    private bool IsCornerHit(Vector2 hitPoint)
    {
        if ((hitPoint.x + 1) % 2 == 0 && (hitPoint.y + 1) % 2 == 0)
        {
            return true;
        }
        return false;
    }
    private ObstacleCoverType GetMaxCoverTypeForShoot(GridPosition unitPosition, GridPosition targetPosition)
    {
        ObstacleCoverType maxOBstacleCoverType = ObstacleCoverType.None;

        return maxOBstacleCoverType;
    }
    public override bool CanTakeAction(int actionPointsAvailable)
    {
        if (actionPointsAvailable > _actionPointsCost)
        {
            if (_requiresAmmo)
            {
                if (_remainingAmmo > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }
    public override void TakeAction(GridPosition gridPosition, Action onSpinComplete)
    {
        _onActionComplete = onSpinComplete;

        _state = State.Aiming;
        _stateTimer = _aimingStateTime;

        // get target to shoot out
        _targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        _targetUnitWorldPosition = _targetUnit.GetWorldPosition();
        _canShootBullet = true;
        _aiming = true;
        _totalSpinAmount = 0;

        //TurnTowardTarget(_targetUnit.GetWorldPosition());
        //TurnTowardTarget(_targetUnitWorldPosition);

        _isActive = true;

        if (_requiresAmmo)
        {
            UseAmmo(this._ammoCost);
        }
            
    }
    public override BombRunEnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        int targetRemainingHealth = GetTargetUnitRemainingHealth(gridPosition);
        if (targetRemainingHealth == -1)
        {
            return new BombRunEnemyAIAction
            {
                _GridPosition = gridPosition,
                _ActionValue = 0,
            };
        }

        int actionValue = 1000 - targetRemainingHealth;
        return new BombRunEnemyAIAction
        {
            _GridPosition = gridPosition,
            _ActionValue = actionValue,
        };
    }
    public int GetMaxShootDistance()
    {
        return _maxShootDistance;
    }
    public BombRunUnit GetTargetUnit()
    {
        return _targetUnit;
    }
    public Vector3 GetTargetUnitWorldPosition()
    {
        return _targetUnitWorldPosition;
    }
    public void ProjectileHitTarget()
    {
        _targetUnit.Damage(35);
    }
    int GetTargetUnitRemainingHealth(GridPosition gridPosition)
    {
        BombRunUnit target = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        if (target == null)
            return -1;

        return target.GetRemainingHealth();
    }
    public int GetTargetCountAtGridPosition(GridPosition gridPosition)
    {
        List<GridPosition> targetPosition = GetValidActionGridPositionList(gridPosition);

        Debug.Log("GetTargetCountAtGridPosition: " + targetPosition.Count + " targets at: " + gridPosition);
        return targetPosition.Count;
    }
}
