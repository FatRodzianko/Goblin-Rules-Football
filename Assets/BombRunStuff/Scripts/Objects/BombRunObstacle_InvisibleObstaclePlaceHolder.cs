using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunObstacle_InvisibleObstaclePlaceHolder : MonoBehaviour
{
    [SerializeField] private BaseBombRunObstacle _obstacle;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteMask _spriteMask;
    [SerializeField] private float _alphaValue;

    private GridObject _gridObject;

    public void InitializeInvisibleObstaclePlaceHolder(BaseBombRunObstacle obstacle, Sprite sprite)
    {
        if (obstacle == null)
        {
            Destroy(this.gameObject);
        }

        _obstacle = obstacle;

        this._spriteRenderer.sprite = sprite;
        this._spriteMask.sprite = sprite;

        Color color = this._spriteRenderer.color;
        color.a = _alphaValue;
        this._spriteRenderer.color = color;

        _obstacle.OnVisibleToPlayerChanged += Obstacle_OnVisibleToPlayerChanged;
        _obstacle.OnThisObstacleDestroyed += Obstacle_OnThisObstacleDestroyed;

        _gridObject = LevelGrid.Instance.GetGridObjectAtPosition(_obstacle.GetGridPosition());

    }

    

    private void OnDisable()
    {
        if (_obstacle != null)
        {
            _obstacle.OnVisibleToPlayerChanged -= Obstacle_OnVisibleToPlayerChanged;
            _obstacle.OnThisObstacleDestroyed -= Obstacle_OnThisObstacleDestroyed;
        }
        if (_gridObject != null)
        {
            _gridObject.OnVisibleToPlayerChanged -= GridObject_OnVisibleToPlayerChanged;
        }
    }

    private void Obstacle_OnVisibleToPlayerChanged(object sender, bool isVisibile)
    {
        if (isVisibile)
        {
            Destroy(this.gameObject);
        }
    }
    private void Obstacle_OnThisObstacleDestroyed(object sender, GridPosition gridPosition)
    {
        if (_gridObject == null)
        {
            Destroy(this.gameObject);
            return;
        }
        _gridObject.OnVisibleToPlayerChanged += GridObject_OnVisibleToPlayerChanged;
    }

    private void GridObject_OnVisibleToPlayerChanged(object sender, bool isVisible)
    {
        if (isVisible)
        {
            Destroy(this.gameObject);
        }
    }
}
