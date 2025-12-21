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
    [SerializeField] private bool _trackVisibilityToPlayer;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteMask _spriteMask;

    private void Start()
    {
        _gridObject = LevelGrid.Instance.GetGridObjectAtPosition(_obstacle.GetGridPosition());
        if (_gridObject != null)
        {
            _gridObject.OnSeenByPlayer += GridObject_OnSeenByPlayer;
        }
    }
    private void OnDisable()
    {
        if (_gridObject != null)
        {
            _gridObject.OnSeenByPlayer -= GridObject_OnSeenByPlayer;
            _gridObject.OnVisibleToPlayerChanged -= GridObject_OnVisibleToPlayerChanged;
        }
    }

    private void GridObject_OnSeenByPlayer(object sender, EventArgs e)
    {
        _trackVisibilityToPlayer = true;
        _spriteMask.sprite = _spriteRenderer.sprite;
        _spriteMask.enabled = true;

        if (_gridObject != null)
        {
            _gridObject.OnVisibleToPlayerChanged += GridObject_OnVisibleToPlayerChanged;
        }
    }

    private void GridObject_OnVisibleToPlayerChanged(object sender, bool isVisibleToPlayer)
    {
        if (isVisibleToPlayer)
        {
            _spriteMask.sprite = _spriteRenderer.sprite;
            _spriteMask.enabled = true;
        }
        else
        {
            _spriteMask.enabled = false;
        }
    }
}
