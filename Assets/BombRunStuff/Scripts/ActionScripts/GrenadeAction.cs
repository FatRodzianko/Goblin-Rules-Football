using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeAction : BaseAction
{
    [SerializeField] private int _maxThrowDistance = 7;
    [SerializeField] private Transform _grenadeProjectilePrefab;
    [SerializeField] DamageMode _damageMode;

    protected override void Start()
    {
        base.Start();
        GetGrenadeDamageMode();
    }    

    private void Update()
    {
        if (!_isActive)
        {
            return;
        }

    }
    private void GetGrenadeDamageMode()
    {
        if (_unit.GetDamageMode() == DamageMode.Medic)
            _damageMode = DamageMode.Heal;
        else
            _damageMode = DamageMode.Damage;
    }
    public override string GetActionName()
    {
        switch (_damageMode)
        {
            default:
            case DamageMode.Damage:
                return "Grenade";
            case DamageMode.Heal:
            case DamageMode.Medic:
                return "Healing\nGrenade";
        }
    }

    public override BombRunEnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        // if I keep this as an action will need to add logic to find enemy units in range to to throw at. Right now, it just throws at the 0,0 grid position
        return new BombRunEnemyAIAction
        {
            _GridPosition = gridPosition,
            _ActionValue = 0,
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
        for (int x = -_maxThrowDistance; x <= _maxThrowDistance; x++)
        {
            for (int y = -_maxThrowDistance; y <= _maxThrowDistance; y++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, y);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                // check to see if the test position is a valid grid position
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }
                if (BombRunTileMapManager.Instance.GetWallGridPositions().Contains(testGridPosition))
                {
                    continue;
                }
                // calculate the distance to the grid position to make sure it is in the shooting radius. Right now the for loops form a big square around selected unit. This will make it more circular
                if (LevelGrid.Instance.CalculateDistance(unitGridPosition, testGridPosition) > _maxThrowDistance * 10)
                {
                    continue;
                }
                validGridPositionList.Add(testGridPosition);
            }
        }
        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete, BodyPart bodyPart = BodyPart.None)
    {
        Debug.Log("TakeAction: Grenade Action");
        _unit.SetActionDirection(LevelGrid.Instance.GetWorldPosition(gridPosition) - LevelGrid.Instance.GetWorldPosition(_unit.GetGridPosition()));
        // Spawn the grenade projectile
        Transform grenadeProjectileTransform = Instantiate(_grenadeProjectilePrefab, _unit.GetWorldPosition(), Quaternion.identity);
        GrenadeProjectile grenadeProjectileScript = grenadeProjectileTransform.GetComponent<GrenadeProjectile>();
        grenadeProjectileScript.Setup(gridPosition, OnGrenadeBehaviorComplete, _damageMode, this._unit);

        ActionStart(onActionComplete);

        if (_requiresAmmo)
        {
            UseAmmo(this._ammoCost);
        }
    }
    private void OnGrenadeBehaviorComplete()
    {
        ActionComplete();
    }
}
