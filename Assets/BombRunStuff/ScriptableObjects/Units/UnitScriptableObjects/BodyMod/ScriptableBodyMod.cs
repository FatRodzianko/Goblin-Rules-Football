using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class BodyModStatModifier
{
    public StatType StatType;
    public float StatModifier;
    public bool IsAdditive;

    public BodyModStatModifier(StatType statType, float statModifier, bool isAdditive)
    {
        StatType = statType;
        StatModifier = statModifier;
        IsAdditive = isAdditive;
    }
    public BodyModStatModifier Clone()
    {
        return new BodyModStatModifier(StatType, StatModifier, IsAdditive);
    }
}
[CreateAssetMenu(fileName = "ScriptableBodyMod", menuName = "BombRun/BodyMods/New Scriptable BodyMod")]
public class ScriptableBodyMod : ScriptableObject
{
    [Header("Details")]
    [SerializeField] private string _name;
    [SerializeField] private Sprite _sprite;
    [SerializeField] private string _description;

    [Header("Stat Modifiers")]
    [SerializeField] private BodyPart _bodyPart;
    [SerializeField] private List<BodyModStatModifier> _bodyModStatModifiers = new List<BodyModStatModifier>();

    [Header("Noise Modifiers")]
    [SerializeField] private bool _modifiesNoise;
    [SerializeField] private float _noiseModifier = 0f;
    [SerializeField] private bool _isNoiseModifierAdditive;

    public string Name()
    {
        return _name;
    }
    public Sprite Sprite()
    {
        return _sprite;
    }
    public string Description()
    {
        return _description;
    }
    public BodyPart BodyPart()
    {
        return _bodyPart;
    }
    public float NoiseModifier()
    {
        return _noiseModifier;
    }
    public bool ModifiesNoise()
    {
        return _modifiesNoise;
    }
    public bool IsNoiseModifierAdditive()
    {
        return _isNoiseModifierAdditive;
    }
    public List<BodyModStatModifier> BodyModStatModifiers()
    {
        return _bodyModStatModifiers;
    }
    
}
