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

    [SerializeField] private Sprite _switchToHealingModeSprite;
    [SerializeField] private Sprite _switchToDamageModeSprite;


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
            //SwitchShootingMode();
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
        SwitchShootingMode();
        ActionStart(onActionComplete);
    }
    void SwitchShootingMode()
    {
        _healingMode = !_healingMode;
        SetActionNameText(_healingMode);

        //OnSwitchShootModeStarted?.Invoke(this, _healingMode);

        if (_healingMode)
        {
            _unit.SetDamageMode(DamageMode.Heal);
            //// setting the opposite because the _actionSymbolSprite is pulled by the UI as soon as the action starts, which is before this? Basically, preparing for the next time the mode is switched 
            //_actionSymbolSprite = _switchToDamageModeSprite;

            _actionSymbolSprite = _switchToHealingModeSprite;
        }
        else
        {
            _unit.SetDamageMode(DamageMode.Damage);
            //_actionSymbolSprite = _switchToHealingModeSprite;

            _actionSymbolSprite = _switchToDamageModeSprite;
        }
            
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
