using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GrenadeProjectile : MonoBehaviour
{
    [SerializeField] private Transform _grenadeExplosionVfxPrefab;
    [SerializeField] private TrailRenderer _trailRenderer;
    [SerializeField] private AnimationCurve _arcYAnimationCurve;

    private GridPosition _targetGridPosition;
    private Vector3 _targetWorldPosition;
    
    private float _reachedTargetDistance = 0.2f;

    [Header("Grenade Stats")]
    [SerializeField] private float _throwSpeed = 15f;
    [SerializeField] private int damageRadius = 1;
    [SerializeField] private int grenadeDamage = 69;
    

    [Header("Sprite Objects")]
    [SerializeField] private Transform _grenadeSpriteObject;
    [SerializeField] private Transform _grenadeShadowSpriteObject;

    [Header("Grenade Movement Stuff")]
    [SerializeField] Vector3[] _trajectoryPoints = new Vector3[3];
    [SerializeField] private float _trajectoryPathCount = 0f;
    private float _trajectoryModifier = 0.5f;
    private float _timeInAir = 0f;
    private float _totalDistance;
    [SerializeField] private Vector3 _positionXYZ = Vector3.zero;


    private Action _onGrenadeBehaviorComplete;

    // events
    public static event EventHandler OnAnyGrenadeExploded;


    private void Update()
    {
        //Vector3 moveDirection = (_targetWorldPosition - _positionXYZ).normalized;
        //_positionXYZ += moveDirection * _throwSpeed * Time.deltaTime;

        //// get current distance traveled
        //float distance = Vector3.Distance(_positionXYZ, _targetWorldPosition);
        //float distanceNormalized = 1 - (distance / _totalDistance);

        //float maxHeight = _totalDistance / 4f;
        //float positionY = _arcYAnimationCurve.Evaluate(distanceNormalized) * maxHeight;
        //transform.position = new Vector3(_positionXYZ.x, _positionXYZ.y + positionY, _positionXYZ.z);

        //if (Vector3.Distance(_targetWorldPosition, this.transform.position) < _reachedTargetDistance)
        //{
        //    // Check for goblins on target position tile and all adjacent tiles
        //    StartGrenadeDamage();

        //    _onGrenadeBehaviorComplete();
        //    OnAnyGrenadeExploded?.Invoke(this, EventArgs.Empty);

        //    // deal with grenade explosion and trail stuff
        //    _trailRenderer.transform.parent = null;
        //    Instantiate(_grenadeExplosionVfxPrefab, _targetWorldPosition, Quaternion.identity);
        //    Destroy(this.gameObject);
        //}
        MoveGrenadeOnTrajectory();
    }
    public void Setup(GridPosition targetGridPosition, Action onGrenadeBehaviorComplete)
    {
        this._onGrenadeBehaviorComplete = onGrenadeBehaviorComplete;
        _targetWorldPosition = LevelGrid.Instance.GetWorldPosition(targetGridPosition);
        _targetGridPosition = targetGridPosition;

        _positionXYZ = this.transform.position;
        _positionXYZ.z = 0f;

        _trajectoryPathCount = 0f;
        _trajectoryPoints = GetCurveTrajectory.GetBasicCurveTrajectory(_positionXYZ, _targetWorldPosition);
        _trajectoryPoints[0].z += 4f; // adjust the starting z position to be at middle of goblin sprite?
        //_timeInAir = GetCurveTrajectory.CalculateFlightTime(_throwSpeed, 45f);

        _totalDistance = Vector2.Distance(_positionXYZ, _targetWorldPosition);
        _timeInAir = _totalDistance / _throwSpeed;
        _trajectoryModifier = 1 / _timeInAir;

        
    }
    private void StartGrenadeDamage()
    {
        // get list of neighbor grid positions for the blast radius
        List<GridPosition> neighborGridPositons = LevelGrid.Instance.GetValidNeighborGridPositions(_targetGridPosition, damageRadius, true);
        Debug.Log("StartGrenadeDamage: neighborGridPositons: " + neighborGridPositons.Count.ToString() + " damageRadius: " + damageRadius.ToString());
        neighborGridPositons.Add(_targetGridPosition);
        FindUnitsAndObstaclesHitByGrenade(neighborGridPositons);
    }
    private void FindUnitsAndObstaclesHitByGrenade(List<GridPosition> gridPositions)
    {
        if (gridPositions.Count < 1)
            return;

        for (int i = 0; i < gridPositions.Count; i++)
        {
            List<BombRunUnit> units = LevelGrid.Instance.GetUnitListAtGridPosition(gridPositions[i]);
            DamageUnitsHitByGrenade(units, gridPositions[i]);
            if (LevelGrid.Instance.HasAnyObstacleOnGridPosition(gridPositions[i]))
            {
                BaseBombRunObstacle obstacle = LevelGrid.Instance.GetObstacleAtGridPosition(gridPositions[i]);
                DamageObstaclessHitByGrenade(obstacle, gridPositions[i]);
            }
            
        }
    }
    private void DamageUnitsHitByGrenade(List<BombRunUnit> units, GridPosition gridPosition)
    {
        if (units.Count < 1)
            return;
        Debug.Log("DamageUnitsHitByGrenade: " + units.Count + " units at position: " + gridPosition.ToString());
        for (int x = 0; x < units.Count; x++)
        {
            ////units[x].Damage(grenadeDamage);
            //List<BombRunUnitBodyPartAndFrozenState> bodyPartsAndFrozenStates = units[x].GetUnitHealthSystem().GetAllBodyPartsAndFrozenState();
            //foreach (BombRunUnitBodyPartAndFrozenState bodyPartAndFrozenState in bodyPartsAndFrozenStates)
            //{
            //    units[x].DamageBodyPart(bodyPartAndFrozenState.BodyPart);
            //}
            units[x].DamageAllBodyParts();
        }
    }
    private void DamageObstaclessHitByGrenade(BaseBombRunObstacle obstacle, GridPosition gridPosition)
    {
        if (obstacle == null)
            return;
        if (!obstacle.IsDestructible())
            return;
        Debug.Log("DamageObstaclessHitByGrenade: " + obstacle.name + " obstacle at position: " + gridPosition.ToString());
        obstacle.DamageObstacle(grenadeDamage);

        // old
        //for (int x = 0; x < obstacle.Count; x++)
        //{
        //    obstacle[x].DamageObstacle(grenadeDamage);
        //}
    }
    void MoveGrenadeOnTrajectory()
    {
        // Move ball along its trajectory?
        if (_trajectoryPathCount < 1.0f)
        {
            //_trajectoryPathCount += hitBallModifer * Time.deltaTime * _rocketHitBallModifier;
            _trajectoryPathCount += _trajectoryModifier * Time.deltaTime;
            _timeInAir += Time.deltaTime;
            Vector3 m1 = Vector3.Lerp(_trajectoryPoints[0], _trajectoryPoints[1], _trajectoryPathCount);
            Vector3 m2 = Vector3.Lerp(_trajectoryPoints[1], _trajectoryPoints[2], _trajectoryPathCount);
            _positionXYZ = Vector3.Lerp(m1, m2, _trajectoryPathCount);

            this.transform.position = new Vector3(_positionXYZ.x, _positionXYZ.y, this.transform.position.z) ;
            AdjustGrenadeHieghtAboveShadow(_positionXYZ.z);

            if (_trajectoryPathCount >= 1.0f)
            {

                this.transform.position = _trajectoryPoints[2];
                TrajectoryCompleted();
            }

        }
        else if (_trajectoryPathCount >= 1.0f)
        {
            //rb.MovePosition(_trajectoryPoints[2]);
            //ResetBallInfo(true);

            this.transform.position = _trajectoryPoints[2];
            TrajectoryCompleted();
        }
    }
    void TrajectoryCompleted()
    {
        AdjustGrenadeHieghtAboveShadow(0f);

        // Check for goblins on target position tile and all adjacent tiles
        StartGrenadeDamage();

        _onGrenadeBehaviorComplete();
        OnAnyGrenadeExploded?.Invoke(this, EventArgs.Empty);

        // deal with grenade explosion and trail stuff
        _trailRenderer.transform.parent = null;
        Instantiate(_grenadeExplosionVfxPrefab, _targetWorldPosition, Quaternion.identity);
        Destroy(this.gameObject);
    }
    void AdjustGrenadeHieghtAboveShadow(float zValue)
    {
        Vector3 grenadeSpritePosition = _grenadeSpriteObject.localPosition;

        grenadeSpritePosition.y = GetCurveTrajectory.GetGrenadeHeightYValue(zValue, 4f);

        _grenadeSpriteObject.localPosition = grenadeSpritePosition;
    }

}
