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
        int actionValue = 0;
        // skip for medics
        if(_unit.GetUnitType() == UnitType.Medic)
        {
            return new BombRunEnemyAIAction
            {
                _GridPosition = this._unit.GetGridPosition(),
                _ActionValue = actionValue,
            };
        }
        // disincentivize taking this on your last action
        if (_unit.GetActionPoints() <= 1 || _unit.GetUnitHealthSystem().GetBodyPartFrozenState(this._actionBodyPart) == BodyPartFrozenState.FullFrozen)
        {
            actionValue -= 500;
        }

        // get targets for damage mode and healing mode. Start by getting the shoot action
        ShootAction shootAction = _unit.GetAction<ShootAction>();
        if (shootAction == null)
        {
            return new BombRunEnemyAIAction
            {
                _GridPosition = this._unit.GetGridPosition(),
                _ActionValue = actionValue,
            };
        }
        // do this relative to the units current damage mode?
        List<GridPosition> targetsForDamageMode = new List<GridPosition>();
        List<GridPosition> targetsForHealMode = new List<GridPosition>();
        DamageMode currentDamageMode = _unit.GetDamageMode();

        // keep "score" for each damage mode to track which one is the better "option" over the other
        int pointsForHeal = 0;
        int pointsForDamage = 0;

        // add points for mode that matches units current mode, to weight
        if (currentDamageMode == DamageMode.Damage)
        {
            pointsForDamage += 100;
        }
        else
        {
            pointsForHeal += 100;
        }

        // reward the mode with the greatest number of targets
        if (targetsForDamageMode.Count < targetsForHealMode.Count)
        {
            pointsForHeal += 100;
        }
        else
        {
            pointsForDamage += 100;
        }

        
        // get list of targets for each healing mode
        targetsForDamageMode = shootAction.GetValidActionGridPositionList(_unit.GetGridPosition(), true, DamageMode.Damage);
        targetsForHealMode = shootAction.GetValidActionGridPositionList(_unit.GetGridPosition(), true, DamageMode.Heal);

        // go through each target and check its health system to determine the "value" of the targets
        pointsForDamage += GetValueOfTargetList(targetsForDamageMode, DamageMode.Damage);
        pointsForHeal += GetValueOfTargetList(targetsForHealMode, DamageMode.Heal);

        // get list of nearby teammates and check if any are medics or already in healing mode
        List<BombRunUnit> nearbyTeammates = GetNearbyTeammates(_unit.GetGridPosition(), shootAction);
        if (nearbyTeammates.Count > 0)
        {
            int numberOfDamageModeTeammates = 0;
            foreach (BombRunUnit teammate in nearbyTeammates)
            {
                UnitType unitType = teammate.GetUnitType();
                if (unitType == UnitType.Medic)
                {
                    pointsForHeal -= 500;
                }
                else if (teammate.GetDamageMode() == DamageMode.Heal)
                {
                    pointsForHeal -= 100;
                }
                else if (teammate.GetDamageMode() == DamageMode.Damage)
                {
                    numberOfDamageModeTeammates += 1;
                }
            }

            if (numberOfDamageModeTeammates < 1)
            {
                pointsForDamage += 100;
            }
        }        


        // check which damage mode currently has the highest value
        DamageMode winningDamageMode = DamageMode.Damage;
        int winningDamagePoints = pointsForDamage;
        if (pointsForDamage < pointsForHeal)
        {
            winningDamageMode = DamageMode.Heal;
            winningDamagePoints = pointsForHeal;
        }

        // if the winning damage is same as current damage mode, then return action with low value
        if (winningDamageMode == currentDamageMode)
        {
            return new BombRunEnemyAIAction
            {
                _GridPosition = this._unit.GetGridPosition(),
                _ActionValue = 0,
            };
        }

        // if the return doesn't happen above, then return calculated action value by adding the winning mode's value
        return new BombRunEnemyAIAction
        {
            _GridPosition = this._unit.GetGridPosition(),
            _ActionValue = actionValue + winningDamagePoints,
        };
    }
    private int GetValueOfTargetList(List<GridPosition> targets, DamageMode damageMode)
    {
        int valueOfTargets = 0;

        foreach (GridPosition gridPosition in targets)
        {
            BombRunUnit target = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
            if (target == null)
                continue;

            BombRunUnitHealthSystem healthSystem = target.GetUnitHealthSystem();
            List<BombRunUnitBodyPartAndFrozenState> allBodyPartsAndFrozenState = healthSystem.GetAllBodyPartsAndFrozenState();
            foreach (BombRunUnitBodyPartAndFrozenState bodyPartFrozenState in allBodyPartsAndFrozenState)
            {
                if (damageMode == DamageMode.Damage)
                {
                    if (bodyPartFrozenState.BodyPartFrozenState == BodyPartFrozenState.HalfFrozen)
                    {
                        valueOfTargets += 100;
                    }
                }
                else
                {
                    if (bodyPartFrozenState.BodyPartFrozenState == BodyPartFrozenState.HalfFrozen)
                    {
                        valueOfTargets += 50;
                    }
                    else if (bodyPartFrozenState.BodyPartFrozenState == BodyPartFrozenState.FullFrozen)
                    {
                        valueOfTargets += 100;
                    }
                }
            }
            
        }
        return valueOfTargets;
    }
    private List<BombRunUnit> GetNearbyTeammates(GridPosition gridPosition, ShootAction shootAction)
    {
        List<BombRunUnit> nearbyTeammates = new List<BombRunUnit>();

        for (int x = -shootAction.GetMaxShootDistance(); x <= shootAction.GetMaxShootDistance(); x++)
        {
            for (int y = -shootAction.GetMaxShootDistance(); y <= shootAction.GetMaxShootDistance(); y++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, y);
                GridPosition testGridPosition = gridPosition + offsetGridPosition;

                if (testGridPosition == gridPosition)
                {
                    continue;
                }
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }
                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    // save target unit
                    BombRunUnit testGridUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);
                    if (testGridUnit.IsEnemy() == this._unit.IsEnemy())
                    {
                        nearbyTeammates.Add(testGridUnit);
                    }
                }
            }
        }

        return nearbyTeammates;
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
