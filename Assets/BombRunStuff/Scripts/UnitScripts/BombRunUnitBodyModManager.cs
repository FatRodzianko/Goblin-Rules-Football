using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InventoryType
{
    None,
    BodyMods,
    BodyModComponents,
}
[Serializable]
public class BombRunUnitBodyModManager
{
    private BombRunUnit _unit;
    [SerializeField] private int _maxInventoryCount;


    [Header("All Body Mods")]
    [SerializeField] private List<BodyMod_Class> _bodyMods = new List<BodyMod_Class>();
    [SerializeField] private Dictionary<BodyMod_Class, BodyMod_InventoryItem> _bodyModDict = new Dictionary<BodyMod_Class, BodyMod_InventoryItem>();
    
    [Header("Equiped Body Mods")]
    [SerializeField] private List<BodyMod_Class> _equippedBodyMods = new List<BodyMod_Class>();

    [Header("Body Mod Components")]
    [SerializeField] private List<BodyModComponent_Class> _bodyModComponents = new List<BodyModComponent_Class>();
    [SerializeField] private Dictionary<BodyModComponent_Class, BodyMod_InventoryItem> _bodyModComponentDict = new Dictionary<BodyModComponent_Class, BodyMod_InventoryItem>();

    [Header("Inventories")]
    [SerializeField] private Dictionary<int, BodyMod_Class> _inventoryBodyMods = new Dictionary<int, BodyMod_Class>(); // the int key is meant to cache where the item is in the inventory list?
    [SerializeField] private Dictionary<int, BodyModComponent_Class> _inventoryBodyModComponents = new Dictionary<int, BodyModComponent_Class>(); // the int key is meant to cache where the item is in the inventory list?

    // Our class's constructor. Takes a ScriptableBombRunUnitBaseStats as an argument.
    public BombRunUnitBodyModManager(BombRunUnit unit, List<ScriptableBodyMod> bodyMods, int maxInventoryCount)
    {
        this._unit = unit;
        //this._bodyMods.AddRange(bodyMods);

        SetMaxInvetoryCount(maxInventoryCount);
        CreateInventoryDictionary(maxInventoryCount);

        CreateBodyModClassObjects(bodyMods, _unit);
        
    }
    private void CreateBodyModClassObjects(List<ScriptableBodyMod> bodyMods, BombRunUnit unit)
    {
        foreach (ScriptableBodyMod bodyMod in bodyMods)
        {
            BodyMod_Class bodyModClass = new BodyMod_Class(bodyMod, unit);
            //if (!_bodyMods.Contains(bodyModClass))
            if(!_bodyMods.Any(x => x.ItemScriptable() == bodyModClass.ItemScriptable()))
            {
                //_bodyMods.Add(bodyModClass);
                AddBodyMod(bodyModClass);
                bodyModClass.OnBodyModEquipped += BodyModClass_OnBodyModEquipped;
                bodyModClass.OnBodyModUnEquipped += BodyModClass_OnBodyModUnEquipped;
                bodyModClass.OnBodyModDestroyed += BodyModClass_OnBodyModDestroyed;

                bodyModClass.EquipBodyMod();
            }
        }
    }
    void AddBodyMod(ScriptableBodyMod bodyMod)
    {
        BodyMod_Class bodyModClass = new BodyMod_Class(bodyMod, _unit);
        bodyModClass.OnBodyModEquipped += BodyModClass_OnBodyModEquipped;
        bodyModClass.OnBodyModUnEquipped += BodyModClass_OnBodyModUnEquipped;
        bodyModClass.OnBodyModDestroyed += BodyModClass_OnBodyModDestroyed;
        AddBodyMod(bodyModClass);
    }
    public void AddBodyMod(BodyMod_Class bodyMod)
    {
        //BodyMod_Class bodyModClass = new BodyMod_Class(bodyMod, this._unit);
        //if (!_bodyMods.Contains(bodyMod))
        //{
        //    _bodyMods.Add(bodyMod);
        //}

        if (_bodyModDict.TryGetValue(bodyMod, out BodyMod_InventoryItem inventoryItem))
        {
            inventoryItem.AddToStack();
        }
        else
        {
            if (CanAddItemToInventory(_inventoryBodyMods, bodyMod, out int index))
            {
                //_inventoryBodyMods[index] = bodyMod;
                SetInventoryItemAtIndex(index, bodyMod, InventoryType.BodyMods);

                _bodyMods.Add(bodyMod);
                BodyMod_InventoryItem newInventoryItem = new BodyMod_InventoryItem(bodyMod);
                _bodyModDict.Add(bodyMod, newInventoryItem);
            }

            //_bodyMods.Add(bodyMod);
            //BodyMod_InventoryItem newInventoryItem = new BodyMod_InventoryItem(bodyMod);
            //_bodyModDict.Add(bodyMod, newInventoryItem);
        }
    }
    public void CreateBodyModComponetClassObjects(List<ScriptableBodyModComponent> bodyModComponents)
    {
        foreach (ScriptableBodyModComponent component in bodyModComponents)
        {
            BodyModComponent_Class bodyModComponent = new BodyModComponent_Class(component, null);
            AddBodyModComponent(bodyModComponent);
        }
    }
    public void AddBodyModComponent(BodyModComponent_Class bodyModComponent)
    {
        if (bodyModComponent == null)
            return;
        if (CanAddItemToInventory(_inventoryBodyModComponents, bodyModComponent, out int index))
        {
            Debug.Log("AddBodyModComponent: CanAddItemToInventory: true " + bodyModComponent.Name());
            if (!_bodyModComponents.Any(x => x.ItemScriptable() == bodyModComponent.ItemScriptable()))
            {
                Debug.Log("AddBodyModComponent: _bodyModComponents does not contain: " + bodyModComponent.Name());
                _bodyModComponents.Add(bodyModComponent);
            }

            BombRun_Item_Class componentAtIndex = GetInventoryItemAtIndex(index, InventoryType.BodyModComponents);
            if (componentAtIndex == null)
            {
                Debug.Log("AddBodyModComponent: body mod component at index: " + index + " is null");
                SetInventoryItemAtIndex(index, bodyModComponent, InventoryType.BodyModComponents);
            }
            else if (componentAtIndex.ItemScriptable() != bodyModComponent.ItemScriptable())
            {
                Debug.Log("AddBodyModComponent: body mod component (" + componentAtIndex.Name() + ") at index: " + index + " does NOT match: " + bodyModComponent.Name());
                SetInventoryItemAtIndex(index, bodyModComponent, InventoryType.BodyModComponents);
            }

            _inventoryBodyModComponents[index].AddToStack(1);
        }
        else
        {
            Debug.Log("AddBodyModComponent: CanAddItemToInventory: false " + bodyModComponent.Name());
            if (!_bodyModComponents.Any(x => x.ItemScriptable() == bodyModComponent.ItemScriptable()))
            {
                _bodyModComponents.Add(bodyModComponent);

                index = _inventoryBodyModComponents.Count();
                _inventoryBodyModComponents.Add(index, null);
                SetInventoryItemAtIndex(index, bodyModComponent, InventoryType.BodyModComponents);
                _inventoryBodyModComponents[index].AddToStack(1);
            }

            
        }
    }
    public void RemoveBodyMod(BodyMod_Class bodyMod)
    {
        if (_bodyModDict.TryGetValue(bodyMod, out BodyMod_InventoryItem inventoryItem))
        {
            inventoryItem.RemoveFromStack();
            if (inventoryItem.StackSize() <= 0)
            {
                Debug.Log("RemoveBodyMod: No more of: " + bodyMod.Name() + " left in inventory. Removing...");
                _bodyMods.Remove(bodyMod);
                _bodyModDict.Remove(bodyMod);

                //int indexToRemove = -1;
                //foreach (KeyValuePair<int, BodyMod_Class> item in _inventoryBodyMods)
                //{
                //    if (item.Value == bodyMod)
                //    {
                //        indexToRemove = item.Key;
                //    }
                //}
                //if (indexToRemove > -1)
                //{
                //    //_inventoryBodyMods[indexToRemove] = null;
                //    SetInventoryItemAtIndex(indexToRemove, null, InventoryType.BodyMods);
                //}
                RemoveItemFromInventoryByItem(bodyMod, InventoryType.BodyMods);
            }
            else
            {
                Debug.Log("RemoveBodyMod: " + bodyMod.Name() + " still has " + inventoryItem.StackSize() + " items left in the inventory.");
            }
        }
        else
        {
            return;
        }
        // OLD
        //if (_bodyMods.Contains(bodyMod))
        //{
        //    _bodyMods.Remove(bodyMod);

        //    bodyMod.OnBodyModEquipped -= BodyModClass_OnBodyModEquipped;
        //    bodyMod.OnBodyModUnEquipped -= BodyModClass_OnBodyModUnEquipped;
        //    bodyMod.OnBodyModDestroyed -= BodyModClass_OnBodyModDestroyed;

        //}
        // OLD

        if (_equippedBodyMods.Contains(bodyMod))
        {
            UnEquipBodyMod(bodyMod);
        }
        bodyMod = null;
    }
    public void RemoveBodyModComponent(BodyModComponent_Class bodyModComponent)
    {
        var foundItem = _bodyModComponents.FirstOrDefault(x => x.ItemScriptable() == bodyModComponent.ItemScriptable());

        if (foundItem != null)
        {
            foundItem.RemoveFromItemCount(1);
            if (foundItem.StackSize() > 0)
            {
                Debug.Log("RemoveBodyModComponent: " + foundItem.Name() + " has " + foundItem.StackSize() + " items remaining.");
                return;
            }
            else
            {
                Debug.Log("RemoveBodyModComponent: " + foundItem.Name() + " fully removed");
                _bodyModComponents.Remove(foundItem);
                RemoveItemFromInventoryByItem(foundItem, InventoryType.BodyModComponents);
            }            
        }
        else
        {
            Debug.Log("RemoveBodyModComponent: No matching component was found in the list.");
        }
    }
    public void RemoveItemFromInventoryByItem(BombRun_Item_Class item, InventoryType inventoryType)
    {
        int indexToRemove = -1;
        foreach (KeyValuePair<int, BombRun_Item_Class> inventoryItem in GetInventoryByType(inventoryType))
        {
            if (inventoryItem.Value == item)
            {
                Debug.Log("RemoveItemFromInventoryByItem: Found item (" + item.Name() + ") at index: " + inventoryItem.Key);
                indexToRemove = inventoryItem.Key;
            }
        }
        if (indexToRemove > -1)
        {
            //_inventoryBodyMods[indexToRemove] = null;
            SetInventoryItemAtIndex(indexToRemove, null, inventoryType);
        }
    }
    private void CreateInventoryDictionary(int size)
    {
        _inventoryBodyMods.Clear();
        for (int i = 0; i < size; i++)
        {
            _inventoryBodyMods.Add(i, null);
        }
    }
    private bool CanAddItemToInventory<T>(Dictionary<int, T> inventory, T itemClass, out int id) where T : BombRun_Item_Class
    {        
        if (itemClass == null)
        {
            id = -1;
            return false;
        }
        // first check if a the item to add is stackable
        // if it is, check if that item already exists in the inventory
        // if it does, return that index
        if (itemClass.Stackable())
        {
            if (IsStackleItemAlreadyInInventory(inventory, itemClass, out int stackableId))
            {
                id = stackableId;
                return true;
            }
        }
        foreach (KeyValuePair<int, T> item in inventory)
        {
            if (item.Value == null)
            {
                Debug.Log("CanAddItemToInventory: Can add at index: " + item.Key);
                id = item.Key;
                return true;
            }
        }
        id = -1;
        return false;
    }
    private bool IsStackleItemAlreadyInInventory<T>(Dictionary<int, T> inventory, T itemClass, out int id) where T : BombRun_Item_Class
    {
        foreach (KeyValuePair<int, T> item in inventory)
        {
            if (item.Value == null)
            {
                continue;
            }

            if (item.Value.ItemScriptable() == itemClass.ItemScriptable())
            {
                Debug.Log("IsStackleItemAlreadyInInventory: Found stackable item in inventory at index: " + item.Key + " item: " + itemClass.Name() + " stackable: " + itemClass.Stackable());
                id = item.Key;
                return true;
            }
        }
        id = -1;
        return false;
    }
    private void BodyModClass_OnBodyModEquipped(object sender, EventArgs e)
    {
        EquipBodyMod(sender as BodyMod_Class);
    }
    private void BodyModClass_OnBodyModUnEquipped(object sender, EventArgs e)
    {
        UnEquipBodyMod(sender as BodyMod_Class);
    }
    void EquipBodyMod(BodyMod_Class bodyMod)
    {
        if (_equippedBodyMods.Contains(bodyMod))
            return;
        _equippedBodyMods.Add(bodyMod);
        foreach (BodyModStatModifier statModifier in bodyMod.BodyStatModifiers())
        {
            this._unit.StatModifierUpdated(statModifier.StatType);
        }

    }
    void UnEquipBodyMod(BodyMod_Class bodyMod)
    {
        if (_equippedBodyMods.Contains(bodyMod))
        {
            _equippedBodyMods.Remove(bodyMod);
            foreach (BodyModStatModifier statModifier in bodyMod.BodyStatModifiers())
            {
                this._unit.StatModifierUpdated(statModifier.StatType);
            }
        }
    }
    public List<BodyMod_Class> GetAllBodyMods()
    {
        return _bodyMods;
    }
    public List<BodyMod_Class> GetAllEquippedBodyMods()
    {
        return _equippedBodyMods;
    }
    public List<BodyMod_Class> GetAllUnEquippedBodyMods()
    {
        return _bodyMods.Where(x => !x.IsEquipped()).ToList();
    }
    public List<BodyMod_Class> GetAllBodyMods_ModifyNoise()
    {
        //return _bodyMods.Where(x => x.ModifiesNoise()).ToList();
        return _equippedBodyMods.Where(x => x.ModifiesNoise()).ToList();
    }
    public float GetAdditiveStatModifierFromBodyMods(StatType statType)
    {
        //Debug.Log("GetAdditiveNoiseModifierFromBodyMods: " + bodyPart + " on: " + _unit);
        float statModifier = 0f;

        foreach (BodyMod_Class bodyMod in _equippedBodyMods)
        {
            if (bodyMod.BodyStatModifiers().Count < 1)
                continue;

            foreach (BodyModStatModifier bodyStatModifier in bodyMod.BodyStatModifiers())
            {
                if (!bodyStatModifier.IsAdditive)
                    continue;
                if (bodyStatModifier.StatType == statType)
                {
                    Debug.Log("GetAdditiveStatModifierFromBodyMods: body stat modifier found: " + bodyStatModifier.StatType + ":" + bodyStatModifier.StatModifier);
                    statModifier += bodyStatModifier.StatModifier;
                }
            }
        }

        return statModifier;
    }
    public float GetMultiplyingStatModifierFromBodyMods(StatType statType)
    {
        //Debug.Log("GetAdditiveNoiseModifierFromBodyMods: " + bodyPart + " on: " + _unit);
        float statModifier = 1f;

        foreach (BodyMod_Class bodyMod in _equippedBodyMods)
        {
            if (bodyMod.BodyStatModifiers().Count < 1)
                continue;

            foreach (BodyModStatModifier bodyStatModifier in bodyMod.BodyStatModifiers())
            {
                if (bodyStatModifier.IsAdditive)
                    continue;
                if (bodyStatModifier.StatType == statType)
                {
                    Debug.Log("GetMultiplyingStatModifierFromBodyMods: body stat modifier found: " + bodyStatModifier.StatType + ":" + bodyStatModifier.StatModifier);
                    statModifier *= bodyStatModifier.StatModifier;
                }
            }
        }

        return statModifier;
    }
    public float GetAdditiveNoiseModifierFromBodyMods(BodyPart bodyPart)
    {
        //Debug.Log("GetAdditiveNoiseModifierFromBodyMods: " + bodyPart + " on: " + _unit);
        float noiseModifier = 0f;

        List<BodyMod_Class> noiseModifyingBodyMods = GetAllBodyMods_ModifyNoise();
        foreach (BodyMod_Class bodyMod in noiseModifyingBodyMods)
        {
            if (!bodyMod.IsNoiseModifierAdditive())
                continue;

            if (bodyMod.BodyPart() != bodyPart)
                continue;

            Debug.Log("GetAdditiveNoiseModifierFromBodyMods: noise modifier found: " + bodyMod.NoiseModifier());
            noiseModifier += bodyMod.NoiseModifier();
        }

        return noiseModifier;
    }
    public float GetMultiplyingNoiseModifierFromBodyMods(BodyPart bodyPart)
    {
        //Debug.Log("GetMultiplyingNoiseModifierFromBodyMods: " + bodyPart + " on: " + _unit);
        float noiseModifier = 1f;

        List<BodyMod_Class> noiseModifyingBodyMods = GetAllBodyMods_ModifyNoise();
        foreach (BodyMod_Class bodyMod in noiseModifyingBodyMods)
        {
            if (bodyMod.IsNoiseModifierAdditive())
                continue;

            if (bodyMod.BodyPart() != bodyPart)
                continue;

            Debug.Log("GetMultiplyingNoiseModifierFromBodyMods: noise modifier found: " + bodyMod.NoiseModifier());
            noiseModifier *= bodyMod.NoiseModifier();
        }

        return noiseModifier;
    }
    public void ModifyBodyMod()
    {
        Debug.Log("BombRunUnitBodyModManager: ModifyBodyMod");
        if (_bodyMods.Count < 1)
            return;

        _bodyMods[0].Modify_BodyModStatModifiers(Mathf.RoundToInt(UnityEngine.Random.Range(2f,10f)));
    }
    public void UnEquipBodyModTest()
    {
        if (_equippedBodyMods.Count < 1)
            return;

        _equippedBodyMods[0].UnEquipBodyMod();
    }
    public void EquipBodyModTest()
    {
        if (_bodyMods.Count < 1)
            return;

        foreach (BodyMod_Class bodyMod in _bodyMods)
        {
            if (!_equippedBodyMods.Contains(bodyMod))
            {
                bodyMod.EquipBodyMod();
                break;
            }
        }
    }
    public void DestroyBodyModTest()
    {
        Debug.Log("DestroyBodyModTest: _bodyMods.Count: " + _bodyMods.Count.ToString());
        if (_bodyMods.Count < 1)
            return;

        _bodyMods[0].DestroyBodyMod();
    }
    public void AddNewTestBodyMod(ScriptableBodyMod bodyMod)
    {
        AddBodyMod(bodyMod);
    }
    public void AddNewTestBodyModComponent(ScriptableBodyModComponent bodyModComponent)
    {
        AddBodyModComponent(new BodyModComponent_Class(bodyModComponent, null));
    }
    public void RemoveNewTestBodyModComponent()
    {
        if (_bodyModComponents.Count < 1)
            return;

        RemoveBodyModComponent(_bodyModComponents[0]);
    }
    private void BodyModClass_OnBodyModDestroyed(object sender, EventArgs e)
    {
        BodyMod_Class bodyMod = sender as BodyMod_Class;

        bodyMod.OnBodyModEquipped += BodyModClass_OnBodyModEquipped;
        bodyMod.OnBodyModUnEquipped += BodyModClass_OnBodyModUnEquipped;
        bodyMod.OnBodyModDestroyed += BodyModClass_OnBodyModDestroyed;

        RemoveBodyMod(bodyMod);
    }
    public int MaxInventoryCount()
    {
        return _maxInventoryCount;
    }
    public int InventorySlotCount(InventoryType inventoryType)
    {
        switch (inventoryType)
        {
            case InventoryType.BodyModComponents:
                return _inventoryBodyModComponents.Count;
            default:
                return MaxInventoryCount();
        }
    }
    public void SetMaxInvetoryCount(int newCount)
    {
        this._maxInventoryCount = newCount;
    }
    //public Dictionary<int, BombRun_Item_Class> GetInventoryByType(InventoryType inventoryType)
    //{
    //    switch (inventoryType)
    //    {
    //        case InventoryType.BodyMods:
    //            return (Dictionary<int, BombRun_Item_Class>)Inventory_BodyMods().Cast<BombRun_Item_Class>();
    //        case InventoryType.BodyModComponents:
    //            return (Dictionary<int, BombRun_Item_Class>)Inventory_BodyModComponents().Cast<BombRun_Item_Class>();
    //        default:
    //            return new Dictionary<int, BombRun_Item_Class>();
    //    }
    //}
    public Dictionary<int, BombRun_Item_Class> GetInventoryByType(InventoryType inventoryType)
    {
        var result = new Dictionary<int, BombRun_Item_Class>();

        switch (inventoryType)
        {
            case InventoryType.BodyMods:
                foreach (var kvp in Inventory_BodyMods())
                {
                    result.Add(kvp.Key, kvp.Value);
                }
                return result;

            case InventoryType.BodyModComponents:
                foreach (var kvp in Inventory_BodyModComponents())
                {
                    result.Add(kvp.Key, kvp.Value);
                }
                return result;
        }

        return result;
    }
    public Dictionary<int, BodyMod_Class> Inventory_BodyMods()
    {
        return _inventoryBodyMods;
    }
    public Dictionary<int, BodyModComponent_Class> Inventory_BodyModComponents()
    {
        return _inventoryBodyModComponents;
    }
    public BombRun_Item_Class GetInventoryItemAtIndex(int index, InventoryType inventoryType)
    {
        switch (inventoryType)
        {
            case InventoryType.BodyMods:
                return _inventoryBodyMods[index];
            case InventoryType.BodyModComponents:
                return _inventoryBodyModComponents[index];
            default:
                return null;
        }
    }
    public void SetInventoryItemAtIndex(int index, BombRun_Item_Class item, InventoryType inventoryType)
    {
        switch (inventoryType)
        {
            case InventoryType.BodyMods:
                _inventoryBodyMods[index] = item as BodyMod_Class;
                break;
            case InventoryType.BodyModComponents:
                _inventoryBodyModComponents[index] = item as BodyModComponent_Class;
                break;
        }
    }
}
