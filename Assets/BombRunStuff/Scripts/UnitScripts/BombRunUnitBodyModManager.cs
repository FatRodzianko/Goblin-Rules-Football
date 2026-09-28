using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BodyPartInventorySlots
{
    public BodyPart BodyPart;
    public int InventoryCount;

    public BodyPartInventorySlots(BodyPart bodyPart, int inventoryCount)
    {
        BodyPart = bodyPart;
        InventoryCount = inventoryCount;
    }
    public BodyPartInventorySlots Clone()
    {
        return new BodyPartInventorySlots(BodyPart, InventoryCount);
    }
}
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

    [Header("Inventory / Equipment Counts")]
    [SerializeField] private int _maxInventoryCount;
    private Dictionary<BodyPart, int> _maxEquippedModsPerBodyPart = new Dictionary<BodyPart, int>();


    [Header("All Body Mods")]
    [SerializeField] private List<BodyMod_Class> _bodyMods = new List<BodyMod_Class>();
    //private Dictionary<BodyMod_Class, BodyMod_InventoryItem> _bodyModDict = new Dictionary<BodyMod_Class, BodyMod_InventoryItem>();
    
    [Header("Equiped Body Mods")]
    [SerializeField] private List<BodyMod_Class> _equippedBodyMods = new List<BodyMod_Class>();

    [Header("Body Mod Components")]
    [SerializeField] private List<BodyModComponent_Class> _bodyModComponents = new List<BodyModComponent_Class>();
    //private Dictionary<BodyModComponent_Class, BodyMod_InventoryItem> _bodyModComponentDict = new Dictionary<BodyModComponent_Class, BodyMod_InventoryItem>();

    [Header("Inventories")]
    private Dictionary<int, BodyMod_Class> _inventoryBodyMods = new Dictionary<int, BodyMod_Class>(); // the int key is meant to cache where the item is in the inventory list?
    private Dictionary<int, BodyModComponent_Class> _inventoryBodyModComponents = new Dictionary<int, BodyModComponent_Class>(); // the int key is meant to cache where the item is in the inventory list?

    [Header("Equipped Body Mods")]
    private Dictionary<BodyPart, Dictionary<int, BodyMod_Class>> _equippedInventories = new Dictionary<BodyPart, Dictionary<int, BodyMod_Class>>();

    [Header("Equipped Inventory Additions")]
    [SerializeField] private List<BodyPartInventorySlots> _additionalBodyPartInventorySlots = new List<BodyPartInventorySlots>();

    private Dictionary<int, BodyMod_Class> _equippedBodyModsHead = new Dictionary<int, BodyMod_Class>();
    private Dictionary<int, BodyMod_Class> _equippedBodyModsArms = new Dictionary<int, BodyMod_Class>();
    private Dictionary<int, BodyMod_Class> _equippedBodyModsLegs = new Dictionary<int, BodyMod_Class>();

    public event EventHandler OnInventoryItemsUpdated;

    // Our class's constructor. Takes a ScriptableBombRunUnitBaseStats as an argument.
    public BombRunUnitBodyModManager(BombRunUnit unit, List<ScriptableBodyMod> bodyMods, int maxInventoryCount)
    {
        this._unit = unit;
        //this._bodyMods.AddRange(bodyMods);

        SetMaxInventoryCount(maxInventoryCount);
        CreateInventoryDictionary(maxInventoryCount);

        CreateEquippedInventories(unit);

        CreateBodyModClassObjects(bodyMods, _unit);
        
    }
    private void CreateBodyModClassObjects(List<ScriptableBodyMod> bodyMods, BombRunUnit unit)
    {
        foreach (ScriptableBodyMod bodyMod in bodyMods)
        {
            BodyMod_Class bodyModClass = new BodyMod_Class(bodyMod, unit);
            AddBodyMod(bodyModClass);
            //if (!_bodyMods.Contains(bodyModClass))

            //if(!_bodyMods.Any(x => x.ItemScriptable() == bodyModClass.ItemScriptable()))
            //{
            //    //_bodyMods.Add(bodyModClass);
            //    AddBodyMod(bodyModClass);
            //    //bodyModClass.OnItemEquipped += BodyModClass_OnItemEquipped;
            //    //bodyModClass.OnItemUnEquipped += BodyModClass_OnItemUnEquipped;
            //    //bodyModClass.OnItemDestroyed += BodyModClass_OnItemDestroyed;

            //    //bodyModClass.EquipItem();
            //}
        }
    }
    private void CreateEquippedInventories(BombRunUnit unit)
    {
        foreach (BodyPartInventorySlots bodyPartInventory in unit.ScriptableBombRunUnit().InventorySlotsPerBodyPart())
        {
            BodyPart bodyPart = bodyPartInventory.BodyPart;
            if (!_equippedInventories.ContainsKey(bodyPart))
            {
                Dictionary<int, BodyMod_Class> newInventory = new Dictionary<int, BodyMod_Class>();

                for (int i = 0; i < bodyPartInventory.InventoryCount; i++)
                {
                    newInventory.Add(i, null);
                }
                _equippedInventories.Add(bodyPart, newInventory);
            }
        }
    }
    public int GetInventorySizeByBodyPart(BodyPart bodyPart)
    {
        if (_equippedInventories.TryGetValue(bodyPart, out Dictionary<int, BodyMod_Class> equippedInventory))
        {
            return equippedInventory.Count;
        }
        else
        {
            return -1;
        }
    }
    void AddBodyMod(ScriptableBodyMod bodyMod)
    {
        BodyMod_Class bodyModClass = new BodyMod_Class(bodyMod, _unit);
        bodyModClass.OnItemEquipped += BodyModClass_OnItemEquipped;
        bodyModClass.OnItemUnEquipped += BodyModClass_OnItemUnEquipped;
        bodyModClass.OnItemDestroyed += BombRun_Item_Class_OnItemDestroyed;
        AddBodyMod(bodyModClass);
    }
    public void AddBodyMod(BodyMod_Class bodyMod)
    {
        //BodyMod_Class bodyModClass = new BodyMod_Class(bodyMod, this._unit);
        //if (!_bodyMods.Contains(bodyMod))
        //{
        //    _bodyMods.Add(bodyMod);
        //}

        //if (_bodyModDict.TryGetValue(bodyMod, out BodyMod_InventoryItem inventoryItem))
        //{
        //    inventoryItem.AddToStack();
        //}
        //else
        //{
        //    if (CanAddItemToInventory(_inventoryBodyMods, bodyMod, out int index))
        //    {
        //        //_inventoryBodyMods[index] = bodyMod;
        //        SetInventoryItemAtIndex(index, bodyMod, InventoryType.BodyMods);

        //        _bodyMods.Add(bodyMod);

        //        bodyMod.OnItemEquipped += BodyModClass_OnItemEquipped;
        //        bodyMod.OnItemUnEquipped += BodyModClass_OnItemUnEquipped;
        //        bodyMod.OnItemDestroyed += BombRun_Item_Class_OnItemDestroyed;

        //        BodyMod_InventoryItem newInventoryItem = new BodyMod_InventoryItem(bodyMod);
        //        _bodyModDict.Add(bodyMod, newInventoryItem);

        //        this.OnInventoryItemsUpdated?.Invoke(this, EventArgs.Empty);
        //    }

        //    //_bodyMods.Add(bodyMod);
        //    //BodyMod_InventoryItem newInventoryItem = new BodyMod_InventoryItem(bodyMod);
        //    //_bodyModDict.Add(bodyMod, newInventoryItem);
        //}
        if (_bodyMods.Contains(bodyMod))
        {
            Debug.Log("AddBodyMod: " + _unit.name + " already has a: " + bodyMod.Name() + " in their body mod inventory...");
            return;
        }           

        if (CanAddItemToInventory(_inventoryBodyMods, bodyMod, out int index))
        {
            Debug.Log("AddBodyMod: " + _unit.name + " adding: " + bodyMod.Name() + " to bodymod inventory!");
            //_inventoryBodyMods[index] = bodyMod;
            SetInventoryItemAtIndex(index, bodyMod, InventoryType.BodyMods);

            _bodyMods.Add(bodyMod);

            bodyMod.OnItemEquipped += BodyModClass_OnItemEquipped;
            bodyMod.OnItemUnEquipped += BodyModClass_OnItemUnEquipped;
            bodyMod.OnItemDestroyed += BombRun_Item_Class_OnItemDestroyed;

            BodyMod_InventoryItem newInventoryItem = new BodyMod_InventoryItem(bodyMod);

            this.OnInventoryItemsUpdated?.Invoke(this, EventArgs.Empty);
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

                bodyModComponent.OnStackSizeChanged += BodyModComponent_OnStackSizeChanged;
                bodyModComponent.OnItemDestroyed += BombRun_Item_Class_OnItemDestroyed;
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

                bodyModComponent.OnStackSizeChanged += BodyModComponent_OnStackSizeChanged;
                bodyModComponent.OnItemDestroyed += BombRun_Item_Class_OnItemDestroyed;
            }
        }
        this.OnInventoryItemsUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void BodyModComponent_OnStackSizeChanged(object sender, EventArgs e)
    {
        this.OnInventoryItemsUpdated?.Invoke(this, EventArgs.Empty);
    }
    private void RemoveItemFromInventory(BombRun_Item_Class inventoryItem)
    {
        InventoryType inventoryType = GetInventoryTypeByItem(inventoryItem);

        switch (inventoryType)
        {
            case InventoryType.BodyMods:
                RemoveBodyMod(inventoryItem as BodyMod_Class);
                break;
            case InventoryType.BodyModComponents:
                RemoveBodyModComponent(inventoryItem as BodyModComponent_Class);
                break;
        }
    }
    public void RemoveBodyMod(BodyMod_Class bodyMod)
    {
        //if (_bodyModDict.TryGetValue(bodyMod, out BodyMod_InventoryItem inventoryItem))
        //{
        //    inventoryItem.RemoveFromStack();
        //    if (inventoryItem.StackSize() <= 0)
        //    {
        //        Debug.Log("RemoveBodyMod: No more of: " + bodyMod.Name() + " left in inventory. Removing...");
        //        _bodyMods.Remove(bodyMod);
        //        _bodyModDict.Remove(bodyMod);

        //        RemoveItemFromInventoryByItem(bodyMod, InventoryType.BodyMods);
        //    }
        //    else
        //    {
        //        Debug.Log("RemoveBodyMod: " + bodyMod.Name() + " still has " + inventoryItem.StackSize() + " items left in the inventory.");
        //    }
        //}
        //else
        //{
        //    return;
        //}

        if (!_bodyMods.Contains(bodyMod))
            return;

        _bodyMods.Remove(bodyMod);
        RemoveItemFromInventoryByItem(bodyMod, InventoryType.BodyMods);

        if (_equippedBodyMods.Contains(bodyMod))
        {
            UnEquipItem(bodyMod);
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
                OnInventoryItemsUpdated?.Invoke(this, EventArgs.Empty);
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
            this.OnInventoryItemsUpdated?.Invoke(this, EventArgs.Empty);
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
            Debug.Log("CanAddItemToInventory: item was null?");
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
                Debug.Log("CanAddItemToInventory: Can add at index: " + stackableId + " as a stackable item.");
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
        Debug.Log("CanAddItemToInventory: CANNOT add item " + "(" + itemClass.Name() + ")" + " to the inventory. Inventory is full.");
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
    private void BodyModClass_OnItemEquipped(object sender, EventArgs e)
    {
        EquipItem(sender as BodyMod_Class);
    }
    private void BodyModClass_OnItemUnEquipped(object sender, EventArgs e)
    {
        UnEquipItem(sender as BodyMod_Class);
        
    }
    public void EquipInventoryItem(InventoryType invetoryType, int itemIndex)
    {
        // for testing. Later grab the appropriate inventory type?
        if (invetoryType != InventoryType.BodyMods)
            return;

        Debug.Log("EquipInventoryItem: " + invetoryType + ": " + itemIndex);
        if (_inventoryBodyMods.TryGetValue(itemIndex, out BodyMod_Class bodyMod))
        {
            if (bodyMod.IsEquipped())
            {

                bodyMod.UnEquipItem();
            }
            else
            {
                if (CanAddItemToInventory(GetEquippedInventoryForBodyPart(bodyMod.BodyPart()), bodyMod, out int index))
                {
                    EquipItemAtIndex(bodyMod, index);
                    Debug.Log("EquipInventoryItem: equipped " + bodyMod.Name() + " at index " + index + " of " + bodyMod.BodyPart() + " inventory. Inventory now: " + GetEquippedInventoryForBodyPart(bodyMod.BodyPart())[index].Name());
                    bodyMod.EquipItem();
                }
                else
                {
                    Debug.Log("EquipInventoryItem: Could not quip " + bodyMod.Name() + " on the " + bodyMod.BodyPart() + " inventory.");
                }
                
            }
        }
        else
        {
            Debug.Log("EquipInventoryItem: no item at: " + itemIndex);
        }
    }
    //private bool CanInventoryItemBeEquipped(BodyMod_Class bodyMod)
    //{
    //    bool canEquip = false;

    //    Dictionary<int, BodyMod_Class> equippedInventory = GetEquippedInventoryForBodyPart(bodyMod.BodyPart());

    //    if (equippedInventory.Count < 1)
    //        return false;

    //    CanAddItemToInventory(equippedInventory, bodyMod, out int id);

    //    return canEquip;
    //}
    
    void EquipItem(BodyMod_Class bodyMod)
    {
        Debug.Log("EquipItem: " + bodyMod.Name());
        if (_equippedBodyMods.Contains(bodyMod))
            return;
        
        _equippedBodyMods.Add(bodyMod);
        //GetEquippedInventoryForBodyPart(bodyMod.BodyPart)

        foreach (BodyModStatModifier statModifier in bodyMod.BodyStatModifiers())
        {
            this._unit.StatModifierUpdated(statModifier.StatType);
        }

    }
    private void EquipItemAtIndex(BodyMod_Class bodyMod, int index)
    {
        Debug.Log("EquipItemAtIndex: " + bodyMod.BodyPart() + ": index: " + index + " BodyMod: " + bodyMod.Name());
        Dictionary<int, BodyMod_Class> equippedInventory = GetEquippedInventoryForBodyPart(bodyMod.BodyPart());
        equippedInventory[index] = bodyMod;
    }
    public void UnEquipItemAtIndex(int index, InventoryType inventoryType)
    {
        BombRun_Item_Class itemAtIndex = GetInventoryItemAtIndex(index, inventoryType);
        if (itemAtIndex == null)
            return;

        if (!itemAtIndex.IsEquipped())
            return;

        itemAtIndex.UnEquipItem();
    }
    void UnEquipItem(BodyMod_Class bodyMod)
    {
        if (_equippedBodyMods.Contains(bodyMod))
        {
            _equippedBodyMods.Remove(bodyMod);
            foreach (BodyModStatModifier statModifier in bodyMod.BodyStatModifiers())
            {
                this._unit.StatModifierUpdated(statModifier.StatType);
            }
        }

        Dictionary<int, BodyMod_Class> equippedInventory = GetEquippedInventoryForBodyPart(bodyMod.BodyPart());
        int indexToRemove = -1;
        foreach (KeyValuePair<int, BodyMod_Class> inventoryItem in equippedInventory)
        {
            if (inventoryItem.Value == bodyMod)
            {
                Debug.Log("UnEquipItem: " + bodyMod.Name() + " found at index " + inventoryItem.Key + ". Removing...");
                indexToRemove = inventoryItem.Key;
                break;
            }
        }
        if (indexToRemove > -1)
        {
            equippedInventory[indexToRemove] = null;
        }
        Debug.Log("EquipInventoryItem: equipped " + bodyMod.Name() + " at index " + indexToRemove + " of " + bodyMod.BodyPart() + " inventory. Inventory now: " + (GetEquippedInventoryForBodyPart(bodyMod.BodyPart())[indexToRemove] is null));

    }
    public void DropItemAtIndex(int index, InventoryType inventoryType)
    {
        Debug.Log("DropItemAtIndex: " + inventoryType + ": " + index);
        BombRun_Item_Class itemAtIndex = GetInventoryItemAtIndex(index, inventoryType);
        if (itemAtIndex == null)
            return;
        Debug.Log("DropItemAtIndex: " + itemAtIndex.Name() + " found at index: " + index);
        itemAtIndex.DropItem();
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
    public void UnEquipItemTest()
    {
        if (_equippedBodyMods.Count < 1)
            return;

        _equippedBodyMods[0].UnEquipItem();
    }
    public void EquipItemTest()
    {
        if (_bodyMods.Count < 1)
            return;

        foreach (BodyMod_Class bodyMod in _bodyMods)
        {
            if (!_equippedBodyMods.Contains(bodyMod))
            {
                bodyMod.EquipItem();
                break;
            }
        }
    }
    public void DestroyItemTest()
    {
        Debug.Log("DestroyItemTest: _bodyMods.Count: " + _bodyMods.Count.ToString());
        if (_bodyMods.Count < 1)
            return;

        _bodyMods[0].DestroyItem();
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
    private void BombRun_Item_Class_OnItemDestroyed(object sender, EventArgs e)
    {
        BombRun_Item_Class inventoryItem = sender as BombRun_Item_Class;

        inventoryItem.OnItemEquipped -= BodyModClass_OnItemEquipped;
        inventoryItem.OnItemUnEquipped -= BodyModClass_OnItemUnEquipped;
        inventoryItem.OnItemDestroyed -= BombRun_Item_Class_OnItemDestroyed;

        RemoveItemFromInventory(inventoryItem);
        //RemoveBodyMod(inventoryItem);
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
    public void SetMaxInventoryCount(int newCount)
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
    public InventoryType GetInventoryTypeByItem(BombRun_Item_Class item)
    {
        if (item.GetType() == typeof(BodyMod_Class))
            return InventoryType.BodyMods;
        else if (item.GetType() == typeof(BodyModComponent_Class))
            return InventoryType.BodyModComponents;

        return InventoryType.None;
    }
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
    private Dictionary<int, BodyMod_Class> GetEquippedInventoryForBodyPart(BodyPart bodyPart)
    {
        if (_equippedInventories.ContainsKey(bodyPart))
        {
            return _equippedInventories[bodyPart];
        }
        return new Dictionary<int, BodyMod_Class>();
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
                //if (_inventoryBodyMods.Count <= index)
                //    return null;
                //return _inventoryBodyMods[index];
                _inventoryBodyMods.TryGetValue(index, out BodyMod_Class bodyMod);
                return bodyMod;
            case InventoryType.BodyModComponents:
                //if (_inventoryBodyModComponents.Count <= index)
                //    return null;
                //return _inventoryBodyModComponents[index];
                _inventoryBodyModComponents.TryGetValue(index, out BodyModComponent_Class component);
                return component;
            default:
                return null;
        }
        
    }
    
    public void SetInventoryItemAtIndex(int index, BombRun_Item_Class item, InventoryType inventoryType)
    {
        switch (inventoryType)
        {
            case InventoryType.BodyMods:
                if (_inventoryBodyMods.Count <= index)
                    return;
                _inventoryBodyMods[index] = item as BodyMod_Class;
                break;
            case InventoryType.BodyModComponents:
                if (_inventoryBodyModComponents.Count <= index)
                    return;
                _inventoryBodyModComponents[index] = item as BodyModComponent_Class;
                break;
        }
    }
}
