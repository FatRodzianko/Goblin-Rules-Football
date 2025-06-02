using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class BaseAction : MonoBehaviour
{
    //public class BaseParameters { } //this can be extended to have a "generic" base parameter for the TakeAction method

    [Header(" Unit Info ")]
    [SerializeField] protected BombRunUnit _unit;

    [Header("Action State")]
    [SerializeField] protected bool _isActive;
    protected Action _onActionComplete;

    [Header("Action Info")]
    [SerializeField] protected int _actionPointDefaultCost = 1;
    [SerializeField] protected int _actionPointsCost = 1;
    [SerializeField] private Sprite _actionSymbolSprite;
    [SerializeField] private BodyPart _actionBodyPart;

    [Header("Animation Stuff")]
    [SerializeField] protected BombRunUnitAnimator _bombRunUnitAnimator;

    [Header("Ammo / Reloadable stuff?")]
    [SerializeField] private bool _isReloadable;
    [SerializeField] protected bool _requiresAmmo;
    [SerializeField] protected int _maxAmmo;
    [SerializeField] protected int _remainingAmmo = 0;
    [SerializeField] protected int _ammoCost;

    [Header("UI Stuff")]
    [SerializeField] protected bool _hideWhenCantUse = false;

    // Static Actions
    public static event EventHandler OnAnyActionStarted;
    public static event EventHandler OnAnyActionCompleted;
    public static event EventHandler OnAnyReloadableFired;
    public static event EventHandler OnAnyAmmoRemainingChanged;

    // Non-static Actions


    protected virtual void Awake()
    {
        _unit = GetComponent<BombRunUnit>();
        if (_bombRunUnitAnimator == null)
        {
            //_bombRunUnitAnimator = GetComponent<BombRunUnitAnimator>();
            _bombRunUnitAnimator = _unit.GetUnitAnimator();
        }
        SetInitialAmmo();
    }
    protected virtual void Start()
    {
        _unit.GetUnitHealthSystem().OnBodyPartFrozenStateChanged += BombRunUnitHealthSystem_OnBodyPartFrozenStateChanged;
    }

    protected virtual void OnDisable()
    {
        _unit.GetUnitHealthSystem().OnBodyPartFrozenStateChanged -= BombRunUnitHealthSystem_OnBodyPartFrozenStateChanged;
    }
    public abstract string GetActionName();
    public abstract void TakeAction(GridPosition gridPosition, Action onActionComplete);
    //public abstract void TakeAction(BaseParameters baseParameters, Action onActionComplete);
    public virtual bool IsValidActionGridPosition(GridPosition gridPosition)
    {
        List<GridPosition> validGridPositionList = GetValidActionGridPositionList();
        return validGridPositionList.Contains(gridPosition);
    }
    public abstract List<GridPosition> GetValidActionGridPositionList();
    public virtual int GetActionPointsCost()
    {
        return _actionPointsCost;
    }
    protected void ActionStart(Action onActionComplete)
    {
        _isActive = true;
        this._onActionComplete = onActionComplete;

        this._unit.SpendActionPoints(this._actionPointsCost);
        this._unit.AddActionTakenThisTurn(this);

        OnAnyActionStarted?.Invoke(this, EventArgs.Empty);
    }
    protected void ActionComplete()
    {
        _isActive = false;
        _onActionComplete();

        OnAnyActionCompleted?.Invoke(this, EventArgs.Empty);
    }

    public virtual Sprite GetActionSymbolSprite()
    {
        return _actionSymbolSprite;
    }
    public BombRunEnemyAIAction GetBestEnemyAIAction()
    {
        List<BombRunEnemyAIAction> enemyAIActionList = new List<BombRunEnemyAIAction>();
        List<GridPosition> validActionGridPositionList = GetValidActionGridPositionList();

        foreach (GridPosition gridPosition in validActionGridPositionList)
        {
            BombRunEnemyAIAction enemyAIAction = GetEnemyAIAction(gridPosition);
            enemyAIActionList.Add(enemyAIAction);
        }
        if (enemyAIActionList.Count > 0)
        {
            // sort list by action value?
            enemyAIActionList.Sort((BombRunEnemyAIAction a, BombRunEnemyAIAction b) => b._ActionValue - a._ActionValue);
            return enemyAIActionList[0];
        }
        else
        {
            // no possible Enemy AI actions
            return null;
        }

    }
    public abstract BombRunEnemyAIAction GetEnemyAIAction(GridPosition gridPosition);
    public virtual bool CanTakeAction(int actionPointsAvailable)
    {
        //if (actionPointsAvailable >= _actionPointsCost)
        //{
        //    return true;
        //}

        //else
        //{
        //    return false;
        //}

        // Check if the applicable body part is frozen. If so, cannot take the action
        if (_unit.GetUnitHealthSystem().GetBodyPartFrozenState(_actionBodyPart) == BodyPartFrozenState.FullFrozen)
            return false;

        if (actionPointsAvailable >= _actionPointsCost)
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
    public bool GetIsReloadable()
    {
        return _isReloadable;
    }
    private void SetInitialAmmo()
    {
        if (!_requiresAmmo)
            return;

        _remainingAmmo = _maxAmmo;
    }
    public void SetRequiresAmmo(bool required)
    {
        _requiresAmmo = required;
    }
    public bool GetRequiresAmmo()
    {
        return _requiresAmmo;
    }
    public void SetMaxAmmo(int newMaxAmmo)
    {
        _maxAmmo = newMaxAmmo;
    }
    public int GetMaxAmmo()
    {
        return _maxAmmo;
    }
    public void SetRemainingAmmo(int newRemainingAmmo)
    {
        _remainingAmmo = newRemainingAmmo;
    }
    public int GetRemainingAmmo()
    {
        return _remainingAmmo;
    }
    public int GetAmmoCost()
    {
        return _ammoCost;
    }
    public void SetAmmoCost(int newCost)
    {
        _ammoCost = newCost;
    }
    protected void UseAmmo(int amountUsed)
    {
        _remainingAmmo -= amountUsed;
        //OnAnyAmmoRemainingChanged?.Invoke(this, EventArgs.Empty);
    }
    public void ReloadAmmo()
    {
        if (!_isReloadable)
            return;

        _remainingAmmo = _maxAmmo;
        //OnAnyAmmoRemainingChanged?.Invoke(this, EventArgs.Empty);
    }
    public bool GetHideWhenCantUse()
    {
        return _hideWhenCantUse;
    }
    public void SetHideWhenCantUse(bool newhideWhenCantUse)
    {
        _hideWhenCantUse = newhideWhenCantUse;
    }
    public BodyPart GetActionBodyPart()
    {
        return _actionBodyPart;
    }
    public void SetActionBodyPart(BodyPart bodyPart)
    {
        this._actionBodyPart = bodyPart;
    }
    protected virtual void BombRunUnitHealthSystem_OnBodyPartFrozenStateChanged(object sender, BodyPart bodyPart)
    {
        if (this._actionBodyPart != bodyPart)
            return;

        switch (_unit.GetUnitHealthSystem().GetBodyPartFrozenState(bodyPart))
        {
            case BodyPartFrozenState.NotFrozen:
                _actionPointsCost = _actionPointDefaultCost;
                break;
            case BodyPartFrozenState.HalfFrozen:
                _actionPointsCost = _actionPointDefaultCost * 2;
                break;
            case BodyPartFrozenState.FullFrozen:
                _actionPointsCost = int.MaxValue;
                break;
        }
    }
}
