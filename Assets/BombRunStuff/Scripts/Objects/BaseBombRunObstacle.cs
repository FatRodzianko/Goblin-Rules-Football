using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseBombRunObstacle : MonoBehaviour
{
    [SerializeField] private GridPosition _gridPosition;
    [SerializeField] private GridObject _gridObject;

    [Header("Obstacle Properties")]
    [SerializeField] private bool _isDestructible = false;
    [SerializeField] private bool _isInteractable = false;
    [SerializeField] private bool _isWalkable = false;

    [SerializeField] private BombRunObstacleType _bombRunObstacleType;

    [Header("Obstacle Cover")]
    [SerializeField] private ObstacleCoverType _obstacleCoverType;

    // Static Events
    public static event EventHandler<GridPosition> OnAnyObstacleDestroyed;
    public static event EventHandler<GridPosition> OnAnyObstacleCoverTypeChanged;

    public event EventHandler OnSeenByPlayer;
    public event EventHandler<bool> OnVisibleToPlayerChanged;

    protected virtual void Start()
    {
        _gridPosition = LevelGrid.Instance.GetGridPositon(this.transform.position);
        _gridObject = LevelGrid.Instance.GetGridObjectAtPosition(_gridPosition);
        if (_gridObject != null)
        {
            _gridObject.OnSeenByPlayer += GridObject_OnSeenByPlayer;
        }
    }

    private void GridObject_OnSeenByPlayer(object sender, EventArgs e)
    {
        OnSeenByPlayer?.Invoke(this, EventArgs.Empty);
        if (_gridObject != null)
        {
            _gridObject.OnVisibleToPlayerChanged += GridObject_OnVisibleToPlayerChanged;
        }
    }
    private void GridObject_OnVisibleToPlayerChanged(object sender, bool isVisibleToPlayer)
    {
        OnVisibleToPlayerChanged?.Invoke(this, isVisibleToPlayer);
    }
    public void SetGridPosition(GridPosition gridPosition)
    {
        this._gridPosition = gridPosition;
    }
    public GridPosition GetGridPosition()
    {
        return _gridPosition;
    }
    public void SetBombRunObstacleType(BombRunObstacleType bombRunObstacleType)
    {
        _bombRunObstacleType = bombRunObstacleType;
    }
    public BombRunObstacleType GetBombRunObstacleType()
    {
        return _bombRunObstacleType;
    }
    public bool IsDestructible()
    {
        return _isDestructible;
    }
    public bool IsInteractable()
    {
        return _isInteractable;
    }
    public bool IsWalkable()
    {
        return _isWalkable;
    }
    public void SetIsWalkable(bool isWalkable)
    {
        this._isWalkable = isWalkable;
    }
    public void SetObstacleCoverType(ObstacleCoverType newCoverType)
    {
        this._obstacleCoverType = newCoverType;
        OnAnyObstacleCoverTypeChanged?.Invoke(this, LevelGrid.Instance.GetGridPositon(this.transform.position));
    }
    public ObstacleCoverType GetObstacleCoverType()
    {
        return _obstacleCoverType;
    }
    public virtual void DamageObstacle(int damageAmount)
    {
        
    }
    public void DestroyObstacle()
    {
        Destroy(this.gameObject);

        OnAnyObstacleDestroyed?.Invoke(this, _gridPosition);
    }
}
