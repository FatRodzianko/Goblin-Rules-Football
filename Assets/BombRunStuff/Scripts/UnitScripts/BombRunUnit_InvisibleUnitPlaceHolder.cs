using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnit_InvisibleUnitPlaceHolder : MonoBehaviour
{
    // events
    public event EventHandler OnGridPositionBecameVisible;

    [SerializeField] private BombRunUnit _unit;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteMask _spriteMask;
    [SerializeField] private float _alphaValue;
    private GridPosition _gridPostion;

    

    private void Start()
    {
        UnitVisibilityManager_BombRun.OnMakeGridPositionVisibleToPlayer += UnitVisibilityManager_BombRun_OnMakeGridPositionVisibleToPlayer;
    }
    

    private void OnDisable()
    {
        UnitVisibilityManager_BombRun.OnMakeGridPositionVisibleToPlayer -= UnitVisibilityManager_BombRun_OnMakeGridPositionVisibleToPlayer;
    }
    public void InitializeInvisibleUnitPlaceHolder(BombRunUnit unit)
    {
        this._unit = unit;
        this._gridPostion = unit.GetGridPosition();

        Sprite sprite = unit.GetCurrentSprite();
        this._spriteRenderer.sprite = sprite;
        this._spriteMask.sprite = sprite;


        // set alpha of sprite to 0.5 or something to make it transparent
        Color newColor = unit.GetSpriteRenderer().color;
        newColor.a = _alphaValue;
        this._spriteRenderer.color = newColor;

        if (unit.GetSpriteFlipX())
        {
            Vector3 newLocalScale = this.transform.localScale;
            newLocalScale.x *= -1;
            this.transform.localScale = newLocalScale;
        }
    }
    private void UnitVisibilityManager_BombRun_OnMakeGridPositionVisibleToPlayer(object sender, GridPosition gridPosition)
    {
        if (this._gridPostion == gridPosition)
        {
            Debug.Log("UnitVisibilityManager_BombRun_OnMakeGridPositionVisibleToPlayer: GridPosition (" + gridPosition + ") is now visible. Destroying " + name + "...");
            //GameObject.Destroy(this.gameObject);
            // check if the unit is still at this grid position.
            // If the unit is at this grid position, check if the unit is defending
            // if the unit is defending, check if that defending unit can be seen
            if (this._unit.GetGridPosition() == gridPosition)
            {
                if (this._unit.GetUnitState() == UnitState.Defending)
                {
                    if (UnitVisibilityManager_BombRun.Instance.CheckIfMovedUnitCanBeSeen(this._unit))
                    {
                        OnGridPositionBecameVisible?.Invoke(this, EventArgs.Empty);
                    }
                }
            }            
            else
            {
                OnGridPositionBecameVisible?.Invoke(this, EventArgs.Empty);
            }
                       
        }
    }
}
