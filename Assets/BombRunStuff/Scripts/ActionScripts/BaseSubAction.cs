using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSubAction : MonoBehaviour
{
    [Header("Base Stuff")]
    [SerializeField] protected BombRunUnit _unit;
    [SerializeField] protected ActionType _parentActionType;
    [SerializeField] protected BaseAction _parentAction;

    [Header("Sub Action State")]
    [SerializeField] protected bool _isActive;
    protected Action _onSubActionComplete;

    [Header("Sub Action Parameters")]
    [SerializeField] protected string _subActionPlayerNotificationText;
    [SerializeField] protected GridPosition _gridPosition;

    // Static Actions
    public static event EventHandler OnAnySubActionStarted;
    public static event EventHandler OnAnySubActionCompleted;
    public static event EventHandler OnAnySubActionCancelled;

    protected virtual void Awake()
    {
        _unit = transform.parent.GetComponent<BombRunUnit>();        
    }
    protected virtual void Start()
    {
        _parentAction = _unit.GetActionByActionType(_parentActionType);
        _parentAction.SetSubAction(this);
        _parentAction.SetHasSubAction(true);
    }
    public virtual void TakeSubAction(GridPosition gridPosition, Action onSubActionComplete)
    {
        this._gridPosition = gridPosition;
    }
    public abstract void TakeActionFromParentAction();
    protected void SubActionStart(Action onSubActionComplete)
    {
        Debug.Log("SubActionStart: " + this.name + " has started the sub action!");
        _isActive = true;
        this._onSubActionComplete = onSubActionComplete;

        OnAnySubActionStarted?.Invoke(this, EventArgs.Empty);
    }
    protected void SubActionComplete()
    {
        Debug.Log("SubActionComplete: " + this.name + " has completed the sub action!");
        _isActive = false;
        // pass this on to the parent action instead of ending it?
        //_onSubActionComplete();

        OnAnySubActionCompleted?.Invoke(this, EventArgs.Empty);
        //_parentAction.TakeAction(this._gridPosition, _onSubActionComplete);
        TakeActionFromParentAction();
    }
    protected void SubActionCancelled()
    {
        _isActive = false;
        _onSubActionComplete();

        OnAnySubActionCancelled?.Invoke(this, EventArgs.Empty);
    }
}
