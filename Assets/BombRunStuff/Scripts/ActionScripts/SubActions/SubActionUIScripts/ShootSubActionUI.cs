using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BodyPartToShootSubAction_SelectionOutlineScriptMapping
{
    public BodyPart BodyPart;
    public ShootSubAction_SelectionOutlineScript SelectionScript;
}
public class ShootSubActionUI : MonoBehaviour
{
    [Header("Body Part Sprite References")]
    [SerializeField] private ScriptableBodyPartSpriteMapping _scriptableBodyPartSpriteMapping;
    [SerializeField] private List<BodyPartToShootSubAction_SelectionOutlineScriptMapping> _bodyPartToTransformMapping = new List<BodyPartToShootSubAction_SelectionOutlineScriptMapping>();

    [Header("Body Part Sprites")]
    [SerializeField] private List<BodyPartToSpriteObjectMapping> _bodyPartToSpriteObjectMapping = new List<BodyPartToSpriteObjectMapping>();

    private Action<BodyPart> _onPlayerSelection;

    [SerializeField] private ShootSubAction _shootSubAction;

    private void Start()
    {
        //UpdateBodyPartSprite();
        SetOnClickActions();
    }
    public void InitializeShootSubActionUI(ShootSubAction shootSubAction)
    {
        this._shootSubAction = shootSubAction;
        UpdateBodyPartSprite();
    }
    public void UpdateBodyPartSprite()
    {
        BombRunUnit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(this._shootSubAction.GetGridPosition());
        if (targetUnit == null)
            return;
        foreach (BodyPartToSpriteObjectMapping bodyPartToSpriteObjectMapping in _bodyPartToSpriteObjectMapping)
        {
            ScriptableBodyPartSprites scriptableBodyPartSprites = _scriptableBodyPartSpriteMapping.GetBodyPartSpriteMappingForBodyPart(bodyPartToSpriteObjectMapping.BodyPart).Sprites;
            
            BodyPartFrozenState state = targetUnit.GetUnitHealthSystem().GetBodyPartFrozenState(bodyPartToSpriteObjectMapping.BodyPart);
            bodyPartToSpriteObjectMapping.Sprite.sprite = scriptableBodyPartSprites.GetSpriteForState(state);

            for (int i = 0; i < _bodyPartToTransformMapping.Count; i++)
            {
                if (_bodyPartToTransformMapping[i].BodyPart == bodyPartToSpriteObjectMapping.BodyPart)
                {
                    _bodyPartToTransformMapping[i].SelectionScript.SetDisabled(state);
                }
            }
        }
    }
    void SetOnClickActions()
    {
        for (int i = 0; i < _bodyPartToTransformMapping.Count; i++)
        {
            _bodyPartToTransformMapping[i].SelectionScript.SetOnClickAction(ClickedOnBodyPart);
        }
    }
    private void ClickedOnBodyPart(BodyPart bodyPart)
    {
        Debug.Log("ClickedOnBodyPart: " + bodyPart);
        _onPlayerSelection(bodyPart);
    }
    public void SetPlayerSelectionCallback(Action<BodyPart> onPlayerSelection)
    {
        _onPlayerSelection = onPlayerSelection;
    }

}
