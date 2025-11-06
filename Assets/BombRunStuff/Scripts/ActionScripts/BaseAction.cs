using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public enum ActionType
{
    None,
    Move,
    Shoot,
    Spin,
    Defend,
    Interact,
    Grenade,
    Sword,
    Reload,
    SwitchShootingMode
}
public enum DamageMode
{
    Damage,
    Heal,
    Medic
}
public enum VisionTypeRequired
{
    None,
    Team,
    Unit
}
public abstract class BaseAction : MonoBehaviour
{
    //public class BaseParameters { } //this can be extended to have a "generic" base parameter for the TakeAction method

    [Header(" Unit Info ")]
    [SerializeField] protected BombRunUnit _unit;

    [Header("Action State")]
    [SerializeField] protected bool _isActive;
    protected Action _onActionComplete;

    [Header("Action Info")]
    [SerializeField] protected ActionType _actionType;
    [SerializeField] protected int _actionPointDefaultCost = 1;
    [SerializeField] protected int _actionPointsCost = 1;
    [SerializeField] protected Sprite _actionSymbolSprite;
    [SerializeField] protected BodyPart _actionBodyPart;
    [SerializeField] private bool _hasSubAction;
    [SerializeField] private bool _canTargetFriendlyUnits = false;
    [SerializeField] private bool _canGetValidListAsTask = false;
    //[SerializeField] private bool _requiresVision = true;
    [SerializeField] private VisionTypeRequired _visionTypeRequired = VisionTypeRequired.None;
    [SerializeField] private bool _canTakeActionInFogOfWar = false;

    [Header("Action Grid Visuals")]
    [SerializeField] GridVisualType _gridRangeVisualType = GridVisualType.RedSoft;
    [SerializeField] int _gridVisualRange = -1;
    [SerializeField] bool _squareGridRange = false;
    [SerializeField] bool _showGridVisualRange = false;

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

    [Header("Sub Action Stuff")]
    [SerializeField] private BaseSubAction _subAction;

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
    public abstract void TakeAction(GridPosition gridPosition, Action onActionComplete, BodyPart bodyPart = BodyPart.None);
    //public abstract void TakeAction(BaseParameters baseParameters, Action onActionComplete);
    public virtual bool IsValidActionGridPosition(GridPosition gridPosition)
    {
        List<GridPosition> validGridPositionList = GetValidActionGridPositionList();
        return validGridPositionList.Contains(gridPosition);
    }
    public async Task<List<GridPosition>> GetValidActionGridPositionListAsTask()
    {
        return await Task.Run(() => GetValidActionGridPositionList());
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
            // later have a check for unit type so certain units favor certain actions, regardless of action value. Ex: Medic should always do a shoot/heal action if possible
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
    public virtual bool CanTakeAction(int actionPointsAvailable, GridPosition actionPosition)
    {

        // Check if the applicable body part is frozen. If so, cannot take the action
        if (_unit.GetUnitHealthSystem().GetBodyPartFrozenState(_actionBodyPart) == BodyPartFrozenState.FullFrozen)
            return false;

        if (!DoesUnitHaveVisionRequiredForActionPosition(actionPosition))
        {
            return false;
        }

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
    private bool DoesUnitHaveVisionRequiredForActionPosition(GridPosition actionPosition)
    {
        if (this._visionTypeRequired == VisionTypeRequired.None)
        {
            return true;
        }
        if (_unit.GetUnitHealthSystem().GetBodyPartFrozenState(BodyPart.Head) != BodyPartFrozenState.FullFrozen)
        {
            return true;
        }
        Debug.Log("CanTakeAction: action requires vision and head is frozen.");
        switch (this._visionTypeRequired)
        {
            case VisionTypeRequired.Unit:
                if (!_unit.CanUnitSeeThisPosition(actionPosition))
                {
                    return false;
                }
                break;
            case VisionTypeRequired.Team:
                if (!UnitVisibilityManager_BombRun.Instance.CanUnitTeamSeeGridPosition(_unit, actionPosition))
                {
                    return false;
                }
                break;
        }
        return true;
    }
    public virtual GridPosition GetNearestValidGridPosition(GridPosition gridPosition)
    {
        Debug.Log("GetNearestValidGridPosition");
        return gridPosition;
    }
    public bool GetHasSubAction()
    {
        return _hasSubAction;
    }
    public void SetHasSubAction(bool newHasSubAction)
    {
        this._hasSubAction = newHasSubAction;
    }
    public BaseSubAction GetSubAction()
    {
        return _subAction;
    }
    public void SetSubAction(BaseSubAction newSubAction)
    {
        this._subAction = newSubAction;
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
    public ActionType GetActionType()
    {
        return _actionType;
    }
    public void SetActionType(ActionType newActionType)
    {
        this._actionType = newActionType;
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
    public bool CanTargetFriendlyUnits()
    {
        return _canTargetFriendlyUnits;
    }
    public void SetCanTargetFriendlyUnits(bool canTarget)
    {
        this._canTargetFriendlyUnits = canTarget;
    }
    public bool CanGetValidListAsTask()
    {
        return _canGetValidListAsTask;

    }
    public GridVisualType GridRangeVisualType()
    {
        return _gridRangeVisualType;
    }
    public int GridVisualRange()
    {
        return _gridVisualRange;
    }
    public bool SquareGridRange()
    {
        return _squareGridRange;
    }
    public bool ShowGridVisualRange()
    {
        return _showGridVisualRange;
    }
    public bool CanTakeActionInFogOfWar()
    {
        return _canTakeActionInFogOfWar;
    }
    public void SetCanTakeActionInFogOfWar(bool canTakeActionInFogOfWar)
    {
        this._canTakeActionInFogOfWar = canTakeActionInFogOfWar;
    }
}
