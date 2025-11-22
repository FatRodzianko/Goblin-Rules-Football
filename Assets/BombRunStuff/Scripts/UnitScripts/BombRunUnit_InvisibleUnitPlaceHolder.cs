using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnit_InvisibleUnitPlaceHolder : MonoBehaviour
{
    [SerializeField] private BombRunUnit _unit;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteMask _spriteMask;

    public void InitializeInvisibleUnitPlaceHolder(BombRunUnit unit)
    {
        this._unit = unit;
        Sprite sprite = unit.GetCurrentSprite();
        this._spriteRenderer.sprite = sprite;
        this._spriteMask.sprite = sprite;

        // get color of sprite
        // set alpha of sprite to 0.5 or something to make it transparent

        if (unit.GetSpriteFlipX())
        {
            Vector3 newLocalScale = this.transform.localScale;
            newLocalScale.x *= -1;
            this.transform.localScale = newLocalScale;
        }
    }
}
