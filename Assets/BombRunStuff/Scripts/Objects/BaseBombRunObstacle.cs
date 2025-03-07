using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseBombRunObstacle : MonoBehaviour
{
    [SerializeField] private GridPosition _gridPosition;

    [Header("Obstacle Properties")]
    [SerializeField] private bool _isDestructible = false;
    [SerializeField] private bool _isInteractable = false;
    [SerializeField] private bool _isWalkable = false;

    [SerializeField] private BombRunObstacleType _bombRunObstacleType;

    [Header("Obstacle Cover")]
    [SerializeField] private ObstacleCoverType _obstacleCoverType;

    // Actions
    public static event EventHandler<GridPosition> OnAnyObstacleDestroyed;

    private void Start()
    {
        _gridPosition = LevelGrid.Instance.GetGridPositon(this.transform.position);
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
