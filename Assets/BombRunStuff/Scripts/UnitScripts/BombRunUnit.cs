using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnit : MonoBehaviour
{
    private const int ACTION_POINTS_MAX = 2;

    // static events
    public static event EventHandler OnAnyActionPointsChanged;

    [Header("GridPosition stuff")]
    private GridPosition _gridPosition;

    [Header("Actions")]
    [SerializeField] private BaseAction[] _baseActionArray;
    [SerializeField] private int _actionPoints = ACTION_POINTS_MAX;

    private void Awake()
    {
        _baseActionArray = GetComponents<BaseAction>();
    }
    private void Start()
    {
        _gridPosition = LevelGrid.Instance.GetGridPositon(this.transform.position);
        LevelGrid.Instance.AddUnitAtGridPosition(_gridPosition, this);

        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
    }
    private void OnDisable()
    {
        TurnSystem.Instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
    }
    private void Update()
    {
        GridPosition newGridPosition = LevelGrid.Instance.GetGridPositon(this.transform.position);
        if (newGridPosition != _gridPosition)
        {
            // unit changed grid position
            LevelGrid.Instance.UnitMovedGridPosition(this, _gridPosition, newGridPosition);
            _gridPosition = newGridPosition;
        }
    }
    public T GetAction<T>() where T : BaseAction
    {
        foreach (BaseAction baseAction in _baseActionArray)
        {
            if (baseAction is T)
            {
                return (T)baseAction;
            }
        }
        return null;
    }
    public GridPosition GetGridPosition()
    {
        return _gridPosition;
    }
    public BaseAction[] GetBaseActionArray()
    {
        return _baseActionArray;
    }
    public bool TrySpendActionPointsToTakeAction(BaseAction baseAction)
    {
        if (CanSpendActionPointsToTakeAction(baseAction))
        {
            SpendActionPoints(baseAction.GetActionPointsCost());
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool CanSpendActionPointsToTakeAction(BaseAction baseAction)
    {
        if (_actionPoints >= baseAction.GetActionPointsCost())
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void SpendActionPoints(int cost)
    {
        _actionPoints -= cost;
        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }
    public int GetActionPoints()
    {
        return _actionPoints;
    }
    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        _actionPoints = ACTION_POINTS_MAX;
        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }
}
