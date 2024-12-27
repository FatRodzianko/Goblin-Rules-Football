using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnit : MonoBehaviour
{
    private const int ACTION_POINTS_MAX = 2;

    // static events
    public static event EventHandler OnAnyActionPointsChanged;
    public static event EventHandler OnAnyUnitSpawned;
    public static event EventHandler OnAnyUnitDied;

    // non-static events
    public event EventHandler<BaseAction> OnActionTaken;

    [SerializeField] private bool _isEnemy;


    [Header("GridPosition stuff")]
    private GridPosition _gridPosition;

    [Header("Actions")]
    [SerializeField] private BaseAction[] _baseActionArray;
    [SerializeField] private int _actionPoints = ACTION_POINTS_MAX;
    [SerializeField] private List<BaseAction> _actionsTakenThisTurn = new List<BaseAction>();

    private void Awake()
    {
        _baseActionArray = GetComponents<BaseAction>();
    }
    private void Start()
    {
        _gridPosition = LevelGrid.Instance.GetGridPositon(this.transform.position);
        Debug.Log("BombRunUnit: Start: " + this.name + ": Starting position at: " + _gridPosition.ToString());
        LevelGrid.Instance.AddUnitAtGridPosition(_gridPosition, this);

        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;

        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);
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
            GridPosition oldGridPosition = _gridPosition;
            _gridPosition = newGridPosition;
            LevelGrid.Instance.UnitMovedGridPosition(this, oldGridPosition, newGridPosition);
            
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
    public Vector3 GetWorldPosition()
    {
        return this.transform.position;
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
            AddActionTakenThisTurn(baseAction);
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
        if ((this.IsEnemy() && !TurnSystem.Instance.IsPlayerTurn()) || // if the unit IS an enemy and it IS NOT the player's turn aka, the unit is an enemy and it is the enemy turn
            (!this.IsEnemy() && TurnSystem.Instance.IsPlayerTurn())) // if the unit IS NOT an enemy and it IS the player's turn, aka the unit is the player's and it is the player's turn
        {
            _actionPoints = ACTION_POINTS_MAX;
            ClearActionsTakenThisTurn();
            OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
        }
        
    }
    private void ClearActionsTakenThisTurn()
    {
        _actionsTakenThisTurn.Clear();
    }
    public void AddActionTakenThisTurn(BaseAction action)
    {
        if (action == null)
            return;

        if (!_actionsTakenThisTurn.Contains(action))
        {
            _actionsTakenThisTurn.Add(action);
        }

        OnActionTaken?.Invoke(this, action);
    }
    public bool IsEnemy()
    {
        return _isEnemy;
    }
    public void Damage(int damage)
    {
        if (damage > 0)
            KillUnit();
    }
    private void KillUnit()
    {
        LevelGrid.Instance.RemoveUnitAtGridPosition(this._gridPosition, this);

        Destroy(this.gameObject);

        OnAnyUnitDied?.Invoke(this, EventArgs.Empty);
    }
    public SpinAction GetSpinAction()
    {
        for (int i = 0; i < _baseActionArray.Length; i++)
        {
            if (_baseActionArray[i] is SpinAction)
            {
                return (SpinAction)_baseActionArray[i];
            }
        }
        return null;
    }
}