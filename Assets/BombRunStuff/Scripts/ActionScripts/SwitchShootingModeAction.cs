using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchShootingModeAction : BaseAction
{
    [Header("Switch Shooting Mode")]
    [SerializeField] private string _actionName = "Switch To Healing";
    [SerializeField] private bool _healingMode = false;

    [SerializeField] private float _switchModeTime = 1f;
    [SerializeField] private float _switchModeCounter;


    // events
    public event EventHandler<bool> OnSwitchShootModeStarted;

    private void Update()
    {
        if (!_isActive)
        {
            return;
        }

        _switchModeCounter -= Time.deltaTime;
        if (_switchModeCounter <= 0)
        {
            SwitchShootingMode();
            ActionComplete();
        }
    }
    public override string GetActionName()
    {
        return _actionName;
    }

    public override BombRunEnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new BombRunEnemyAIAction
        {
            _GridPosition = this._unit.GetGridPosition(),
            _ActionValue = 0,
        };
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
        Debug.Log("TakeAction: Switch Shooting Mode");
        _switchModeCounter = _switchModeTime;
        ActionStart(onActionComplete);
    }
    void SwitchShootingMode()
    {
        _healingMode = !_healingMode;
        SetActionNameText(_healingMode);

        //OnSwitchShootModeStarted?.Invoke(this, _healingMode);

        if (_healingMode)
            _unit.SetDamageMode(DamageMode.Heal);
        else
            _unit.SetDamageMode(DamageMode.Damage);
    }
    void SetActionNameText(bool healingMode)
    {
        if (healingMode)
        {
            _actionName = "Switch To Damage";
        }
        else
        {
            _actionName = "Switch To Healing";
        }
    }
}
