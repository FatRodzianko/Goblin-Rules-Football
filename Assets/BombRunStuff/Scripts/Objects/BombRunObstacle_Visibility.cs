using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunObstacle_Visibility : MonoBehaviour
{
    [Header("Grid Object/Position")]
    [SerializeField] private BaseBombRunObstacle _obstacle;
    [SerializeField] private GridObject _gridObject;

    [Header("Sprite Stuff")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteMask _spriteMask;

    private void Start()
    {
        _obstacle.OnVisibleToPlayerChanged += Obstacle_OnVisibleToPlayerChanged;
    }
    private void OnDisable()
    {
        _obstacle.OnVisibleToPlayerChanged -= Obstacle_OnVisibleToPlayerChanged;
    }

    private void Obstacle_OnVisibleToPlayerChanged(object sender, bool isVisibleToPlayer)
    {
        if (isVisibleToPlayer)
        {
            Debug.Log("Obstacle_OnVisibleToPlayerChanged: for " + this.name + " changing sprite to: " + _spriteRenderer.sprite.name);
            _spriteMask.sprite = _spriteRenderer.sprite;
            _spriteMask.enabled = true;
        }
        else
        {
            _spriteMask.enabled = false;
        }
    }
}
