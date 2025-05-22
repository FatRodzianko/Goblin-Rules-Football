using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class BodyPartToImageObjectMapping
{
    public BodyPart BodyPart;
    public Image Image;
}
public class ActionButtonBodyPartSpriteHolderScript : MonoBehaviour
{
    [SerializeField] private ScriptableBodyPartSpriteMapping _scriptableBodyPartSpriteMapping;
    [SerializeField] private ActionButtonUI _actionButtonUI;
    [SerializeField] private BodyPart _bodyPartType;
    [SerializeField] private List<BodyPartToImageObjectMapping> _bodyPartToImageObjectMappingList = new List<BodyPartToImageObjectMapping>();

    public void SetBodyPartType(BodyPart bodyPart)
    {
        this._bodyPartType = bodyPart;
        UpdateBodyPartImage();
    }
    public void UpdateBodyPartImage()
    {
        foreach (BodyPartToImageObjectMapping bodyPartToImageObjectMapping in _bodyPartToImageObjectMappingList)
        {
            ScriptableBodyPartSprites scriptableBodyPartSprites = _scriptableBodyPartSpriteMapping.GetBodyPartSpriteMappingForBodyPart(bodyPartToImageObjectMapping.BodyPart).Sprites;
            if (bodyPartToImageObjectMapping.BodyPart == this._bodyPartType)
            {
                BodyPartFrozenState state = UnitActionSystem.Instance.GetSelectedUnit().GetUnitHealthSystem().GetBodyPartFrozenState(this._bodyPartType);
                bodyPartToImageObjectMapping.Image.sprite = scriptableBodyPartSprites.GetSpriteForState(state);
            }
            else
            {
                bodyPartToImageObjectMapping.Image.sprite = scriptableBodyPartSprites.NoneSprite();
            }
        }
    }
}
