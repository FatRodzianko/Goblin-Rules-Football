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
    [SerializeField] private int _actionPointsCost = 1;
    [SerializeField] private Sprite _actionSymbolSprite;

    // Actions
    public static event EventHandler OnAnyActionStarted;
    public static event EventHandler OnAnyActionCompleted;



    protected virtual void Awake()
    {
        _unit = GetComponent<BombRunUnit>();
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
}
