using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BodyMod_InventoryItem
{
    private BombRun_Item_Class _item;
    private int _stackSize;

    public BodyMod_InventoryItem(BombRun_Item_Class item)
    {
        this._item = item;
        AddToStack();
    }
    public void AddToStack()
    {
        _stackSize++;
    }
    public void RemoveFromStack()
    {
        _stackSize--;
    }
    public BombRun_Item_Class BodyMod()
    {
        return _item;
    }
    public int StackSize()
    {
        return _stackSize;
    }
}
