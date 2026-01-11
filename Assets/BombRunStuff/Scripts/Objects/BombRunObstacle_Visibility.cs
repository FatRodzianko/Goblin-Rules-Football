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

    [Header("Shadow")]
    [SerializeField] private SpriteRenderer _shadowSpriteRenderer;

    [Header("Invisible Obstacle Placeholder")]
    [SerializeField] private Transform _invisibleObjectPlaceHolderPrefab;


    private void Start()
    {
        _obstacle.OnVisibleToPlayerChanged += Obstacle_OnVisibleToPlayerChanged;
        _spriteRenderer.RegisterSpriteChangeCallback(SpriteChanged);
    }

    

    private void OnDisable()
    {
        _obstacle.OnVisibleToPlayerChanged -= Obstacle_OnVisibleToPlayerChanged;
        _spriteRenderer.UnregisterSpriteChangeCallback(SpriteChanged);
    }

    private void Obstacle_OnVisibleToPlayerChanged(object sender, bool isVisibleToPlayer)
    {
        if (isVisibleToPlayer)
        {
            //Debug.Log("Obstacle_OnVisibleToPlayerChanged: for " + this.name + " changing sprite to: " + _spriteRenderer.sprite.name);
            _spriteRenderer.enabled = true;
            _spriteMask.sprite = _spriteRenderer.sprite;
            _spriteMask.enabled = true;

            if (_shadowSpriteRenderer != null)
            {
                _shadowSpriteRenderer.enabled = true;
            }
        }
        else
        {
            _spriteMask.enabled = false;
            _spriteRenderer.enabled = false;

            if (_shadowSpriteRenderer != null)
            {
                _shadowSpriteRenderer.enabled = false;
            }

            SpawnInvisiblePlaceHolder();
        }
    }
    private void SpawnInvisiblePlaceHolder()
    {
        if (_invisibleObjectPlaceHolderPrefab == null)
            return;

        BombRunObstacle_InvisibleObstaclePlaceHolder placeholder = Instantiate(_invisibleObjectPlaceHolderPrefab, this.transform.position, Quaternion.identity).GetComponent<BombRunObstacle_InvisibleObstaclePlaceHolder>();
        placeholder.InitializeInvisibleObstaclePlaceHolder(this._obstacle, this._spriteRenderer.sprite);

    }
    private void SpriteChanged(SpriteRenderer spriteRenderer)
    {
        _spriteMask.sprite = spriteRenderer.sprite;
    }
}
