using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BodyMod_InventoryItem
{
    private BodyMod_Class _bodyMod;
    private int _stackSize;

    public BodyMod_InventoryItem(BodyMod_Class bodyMod)
    {
        this._bodyMod = bodyMod;
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
    public BodyMod_Class BodyMod()
    {
        return _bodyMod;
    }
    public int StackSize()
    {
        return _stackSize;
    }
}
