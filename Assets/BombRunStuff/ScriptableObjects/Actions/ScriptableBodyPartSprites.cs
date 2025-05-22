using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Body Part Sprites Scriptable", menuName = "Body Part Sprites Scriptable")]
public class ScriptableBodyPartSprites : ScriptableObject
{
    [SerializeField] private Sprite _noneSprite;
    [SerializeField] private Sprite _notFrozenSprite;
    [SerializeField] private Sprite _halfFrozenSprite;
    [SerializeField] private Sprite _fullFrozenSprite;

    public Sprite NoneSprite()
    {
        return _noneSprite;
    }
    public Sprite NotFrozenSprite()
    {
        return _notFrozenSprite;
    }
    public Sprite HalfFrozenSprite()
    {
        return _halfFrozenSprite;
    }
    public Sprite FullFrozenSprite()
    {
        return _fullFrozenSprite;
    }
    public Sprite GetSpriteForState(BodyPartFrozenState bodyPartFrozenState)
    {
        switch (bodyPartFrozenState)
        {
            default:
            case BodyPartFrozenState.NotFrozen:
                return _notFrozenSprite;
                break;
            case BodyPartFrozenState.HalfFrozen:
                return _halfFrozenSprite;
                break;
            case BodyPartFrozenState.FullFrozen:
                return _fullFrozenSprite;
                break;
        }
    }
}
