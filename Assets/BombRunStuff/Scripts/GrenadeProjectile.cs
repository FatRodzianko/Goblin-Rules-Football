using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GrenadeProjectile : MonoBehaviour
{
    private GridPosition _targetGridPosition;
    private Vector3 _targetWorldPosition;
    [SerializeField] private float _throwSpeed = 15f;
    private float _reachedTargetDistance = 0.2f;

    [Header("Grenade Stats")]
    [SerializeField] private int damageRadius = 1;
    [SerializeField] private int grenadeDamage = 30;

    private Action _onGrenadeBehaviorComplete;


    private void Update()
    {
        Vector3 moveDirection = (_targetWorldPosition - this.transform.position).normalized;
        this.transform.position += moveDirection * _throwSpeed * Time.deltaTime;

        if (Vector3.Distance(_targetWorldPosition, this.transform.position) < _reachedTargetDistance)
        {
            // Check for goblins on target position tile and all adjacent tiles
            StartGrenadeDamage();

            _onGrenadeBehaviorComplete();
            Destroy(this.gameObject);
        }
    }
    public void Setup(GridPosition targetGridPosition, Action onGrenadeBehaviorComplete)
    {
        this._onGrenadeBehaviorComplete = onGrenadeBehaviorComplete;
        _targetWorldPosition = LevelGrid.Instance.GetWorldPosition(targetGridPosition);
        _targetGridPosition = targetGridPosition;
    }
    private void StartGrenadeDamage()
    {
        // get list of neighbor grid positions for the blast radius
        List<GridPosition> neighborGridPositons = LevelGrid.Instance.GetValidNeighborGridPositions(_targetGridPosition, damageRadius, true);
        neighborGridPositons.Add(_targetGridPosition);
        DamageUnitsWithGrenade(neighborGridPositons);
    }
    private void DamageUnitsWithGrenade(List<GridPosition> gridPositions)
    {
        if (gridPositions.Count < 1)
            return;

        for (int i = 0; i < gridPositions.Count; i++)
        {
            List<BombRunUnit> units = LevelGrid.Instance.GetUnitListAtGridPosition(gridPositions[i]);
            if (units.Count < 1)
                continue;
            Debug.Log("DamageUnitsWithGrenade: " + units.Count + " units at position: " + gridPositions[i].ToString());
            for (int x = 0; x < units.Count; x++)
            {
                units[x].Damage(grenadeDamage);
            }
        }
    }
}
