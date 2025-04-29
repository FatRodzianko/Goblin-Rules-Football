using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadAction : BaseAction
{
    [SerializeField] private float _reloadTime = 1.5f;
    [SerializeField] private float _reloadCounter;
    private void Update()
    {
        if (!_isActive)
        {
            return;
        }

        _reloadCounter -= Time.deltaTime;
        if (_reloadCounter <= 0)
        {
            ReloadWeapons();
            _isActive = false;
            _onActionComplete();
        }
    }
    public override string GetActionName()
    {
        return "Reload";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = _unit.GetGridPosition();

        return new List<GridPosition>
        {
            unitGridPosition
        };
    }
    public override BombRunEnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        int actionValue = 0;
        BaseAction[] baseActionArray = _unit.GetBaseActionArray();
        if (baseActionArray.Length > 0)
        {
            for (int i = 0; i < baseActionArray.Length; i++)
            {
                if (baseActionArray[i].GetIsReloadable())
                {
                    if (baseActionArray[i].GetRemainingAmmo() == 0 && baseActionArray[i].GetMaxAmmo() > 0)
                    {
                        actionValue = 1500;
                    }
                }
            }
        }
        
        return new BombRunEnemyAIAction
        {
            _GridPosition = this._unit.GetGridPosition(),
            _ActionValue = actionValue,
        };
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        Debug.Log("TakeAction: ReloadAction");
        _reloadCounter = _reloadTime;
        ActionStart(onActionComplete);
    }
    public override bool CanTakeAction(int actionPointsAvailable)
    {
        if (actionPointsAvailable > _actionPointsCost)
        {
            BaseAction[] baseActionArray = _unit.GetBaseActionArray();
            if (baseActionArray.Length == 0)
                return false;
            for (int i = 0; i < baseActionArray.Length; i++)
            {
                if (baseActionArray[i].GetIsReloadable())
                {
                    if (baseActionArray[i].GetRemainingAmmo() < baseActionArray[i].GetMaxAmmo())
                    {
                        Debug.Log("ReloadAction: CanTakeAction:  can reload: " + baseActionArray[i].GetActionName() + " Current Ammo: " + baseActionArray[i].GetRemainingAmmo().ToString() + " Max Ammo: " + baseActionArray[i].GetMaxAmmo().ToString());
                        return true;
                    }
                }
            }

            return false;
        }

        else
        {
            return false;
        }
    }
    private void ReloadWeapons()
    {
        BaseAction[] baseActionArray = _unit.GetBaseActionArray();
        if (baseActionArray.Length == 0)
            return;
        for (int i = 0; i < baseActionArray.Length; i++)
        {
            if (baseActionArray[i].GetIsReloadable())
            {
                baseActionArray[i].ReloadAmmo();
            }
        }
    }
}
