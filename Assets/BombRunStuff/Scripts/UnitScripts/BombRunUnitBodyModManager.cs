using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BombRunUnitBodyModManager
{
    private BombRunUnit _unit;

    [SerializeField] private List<BodyMod_Class> _bodyMods = new List<BodyMod_Class>();

    // Our class's constructor. Takes a ScriptableBombRunUnitBaseStats as an argument.
    public BombRunUnitBodyModManager(BombRunUnit unit, List<ScriptableBodyMod> bodyMods)
    {
        this._unit = unit;
        //this._bodyMods.AddRange(bodyMods);
        CreateBodyModClassObjects(bodyMods, _unit);
    }
    private void CreateBodyModClassObjects(List<ScriptableBodyMod> bodyMods, BombRunUnit unit)
    {
        foreach (ScriptableBodyMod bodyMod in bodyMods)
        {
            BodyMod_Class bodyModClass = new BodyMod_Class(bodyMod, unit);
            if (!_bodyMods.Contains(bodyModClass))
            {
                _bodyMods.Add(bodyModClass);
            }
        }
    }

    public void AddBodyMod(ScriptableBodyMod bodyMod)
    {
        BodyMod_Class bodyModClass = new BodyMod_Class(bodyMod, this._unit);
        if (!_bodyMods.Contains(bodyModClass))
        {
            _bodyMods.Add(bodyModClass);
        }
    }
    public List<BodyMod_Class> GetAllBodyMods()
    {
        return _bodyMods;
    }
    public List<BodyMod_Class> GetAllBodyMods_ModifyNoise()
    {
        return _bodyMods.Where(x => x.ModifiesNoise()).ToList();
    }
    public float GetAdditiveStatModifierFromBodyMods(StatType statType)
    {
        //Debug.Log("GetAdditiveNoiseModifierFromBodyMods: " + bodyPart + " on: " + _unit);
        float statModifier = 0f;

        foreach (BodyMod_Class bodyMod in _bodyMods)
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

        foreach (BodyMod_Class bodyMod in _bodyMods)
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
}
