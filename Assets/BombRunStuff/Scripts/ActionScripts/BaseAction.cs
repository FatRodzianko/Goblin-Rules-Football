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
}
