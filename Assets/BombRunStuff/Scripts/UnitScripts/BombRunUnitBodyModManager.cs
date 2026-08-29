using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BombRunUnitBodyModManager
{
    private BombRunUnit _unit;
    private int _maxInventoryCount;


    [Header("All Body Mods")]
    [SerializeField] private List<BodyMod_Class> _bodyMods = new List<BodyMod_Class>();

    [SerializeField] private Dictionary<BodyMod_Class, BodyMod_InventoryItem> _bodyModDict = new Dictionary<BodyMod_Class, BodyMod_InventoryItem>();
    
    [Header("Equiped Body Mods")]
    [SerializeField] private List<BodyMod_Class> _equippedBodyMods = new List<BodyMod_Class>();

    // Our class's constructor. Takes a ScriptableBombRunUnitBaseStats as an argument.
    public BombRunUnitBodyModManager(BombRunUnit unit, List<ScriptableBodyMod> bodyMods, int maxInventoryCount)
    {
        this._unit = unit;
        //this._bodyMods.AddRange(bodyMods);
        CreateBodyModClassObjects(bodyMods, _unit);
        SetMaxInvetoryCount(maxInventoryCount);
    }
    private void CreateBodyModClassObjects(List<ScriptableBodyMod> bodyMods, BombRunUnit unit)
    {
        foreach (ScriptableBodyMod bodyMod in bodyMods)
        {
            BodyMod_Class bodyModClass = new BodyMod_Class(bodyMod, unit);
            if (!_bodyMods.Contains(bodyModClass))
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
            _bodyMods.Add(bodyMod);
            BodyMod_InventoryItem newInventoryItem = new BodyMod_InventoryItem(bodyMod);
            _bodyModDict.Add(bodyMod, newInventoryItem);
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
    public void SetMaxInvetoryCount(int newCount)
    {
        this._maxInventoryCount = newCount;
    }
}
