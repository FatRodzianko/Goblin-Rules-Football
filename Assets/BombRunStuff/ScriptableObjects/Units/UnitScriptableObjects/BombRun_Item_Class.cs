using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BombRun_Item_Class
{
    [Header("Base Item ")]
    [SerializeField] protected ScriptableItem _itemScriptable;

    [Header("Base Item Details")]
    [SerializeField] protected string _name;
    [SerializeField] protected Sprite _sprite;
    [SerializeField] protected string _description;
    

    [Header("Base Item Stat Modifiers")]
    [SerializeField] protected BodyPart _bodyPart;

    [Header("Item Stack/Count")]
    [SerializeField] protected bool _stackable;
    [SerializeField] protected int _stackSize;

    // events
    public event EventHandler OnStackSizeChanged;


    public BombRun_Item_Class(ScriptableItem itemScript)
    {
        this._itemScriptable = itemScript;

        this._name = itemScript.Name();
        this._sprite = itemScript.Sprite();
        this._description = itemScript.Description();

        this._stackable = itemScript.Stackable();
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
    public int StackSize()
    {
        return _stackSize;
    }
    public void AddToStack(int amount)
    {
        _stackSize += amount;
        Debug.Log("BombRun_Item_Class: AddToStack: " + this._name + " Adding: " + amount + " to stack. Stack size is now: " + _stackSize);
        OnStackSizeChanged?.Invoke(this, EventArgs.Empty);
    }
    public void RemoveFromItemCount(int amount)
    {
        _stackSize -= amount;
        OnStackSizeChanged?.Invoke(this, EventArgs.Empty);
    }
    public void SetItemCount(int newCount)
    {
        _stackSize = newCount;
    }
    public bool Stackable()
    {
        return _stackable;
    }
    public void SetStackable(bool newStackable)
    {
        _stackable = newStackable;
    }
}
