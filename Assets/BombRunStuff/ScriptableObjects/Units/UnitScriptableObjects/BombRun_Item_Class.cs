using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BombRun_Item_Class
{
    [Header("Base Item ")]
    [SerializeField] protected BombRunUnit _unit;
    [SerializeField] protected ScriptableItem _itemScriptable;

    [Header("Base Item Details")]
    [SerializeField] protected string _name;
    [SerializeField] protected Sprite _sprite;
    [SerializeField] protected string _description;

    [Header("Base Item Stat Modifiers")]
    [SerializeField] protected BodyPart _bodyPart;

    public BombRun_Item_Class(ScriptableItem itemScript, BombRunUnit unit)
    {
        this._itemScriptable = itemScript;
        this._unit = unit;

        this._name = itemScript.Name();
        this._sprite = itemScript.Sprite();
        this._description = itemScript.Description();
    }
    public BombRunUnit Unit()
    {
        return _unit;
    }
    public string Name()
    {
        return _name;
    }
    public void SetName(string newName)
    {
        this._name = newName;
    }
    public Sprite Sprite()
    {
        return _sprite;
    }
    public void SetSprite(Sprite newSprite)
    {
        this._sprite = newSprite;
    }
    public string Description()
    {
        return _description;
    }
    public void SetDescription(string newDescription)
    {
        this._description = newDescription;
    }
    public BodyPart BodyPart()
    {
        return _bodyPart;
    }
    public void SetBodyPart(BodyPart newBodyPart)
    {
        this._bodyPart = newBodyPart;
    }
    public ScriptableItem ItemScriptable()
    {
        return _itemScriptable;
    }
    public void SetScriptableItem(ScriptableItem scriptableItem)
    {
        this._itemScriptable = scriptableItem;
    }
}
