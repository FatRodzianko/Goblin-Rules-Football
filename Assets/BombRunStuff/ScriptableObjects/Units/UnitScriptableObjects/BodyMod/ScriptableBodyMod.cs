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
    public float NoiseModifier()
    {
        return _noiseModifier;
    }
    public void SetNoiseModifier(float newModifier)
    {
        this._noiseModifier = newModifier;
    }
    public bool ModifiesNoise()
    {
        return _modifiesNoise;
    }
    public void SetModifiesNoise(bool newModifiesNoise)
    {
        this._modifiesNoise = newModifiesNoise;
    }
    public bool IsNoiseModifierAdditive()
    {
        return _isNoiseModifierAdditive;
    }
    public void SetIsNoiseModifierAdditive(bool newIsNoiseModifierAdditive)
    {
        this._isNoiseModifierAdditive = newIsNoiseModifierAdditive;
    }
}
