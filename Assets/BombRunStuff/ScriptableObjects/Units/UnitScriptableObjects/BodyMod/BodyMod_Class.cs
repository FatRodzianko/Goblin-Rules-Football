using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BodyMod_Class
{
    [SerializeField] private BombRunUnit _unit;
    [SerializeField] private ScriptableBodyMod _bodyModScriptable;

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

    // Our class's constructor. Takes a ScriptableBombRunUnitBaseStats as an argument.
    public BodyMod_Class(ScriptableBodyMod bodyModScript, BombRunUnit unit)
    {
        this._bodyModScriptable = bodyModScript;
        this._unit = unit;

        this._name = bodyModScript.Name();
        this._sprite = bodyModScript.Sprite();
        this._description = bodyModScript.Description();

        this._bodyPart = bodyModScript.BodyPart();
        //this._bodyModStatModifiers.AddRange(bodyModScript.BodyModStatModifiers());
        //this._bodyModStatModifiers = new List<BodyModStatModifier>(bodyModScript.BodyModStatModifiers());
        AddBodyModStatModifiersFromScriptable(bodyModScript.BodyModStatModifiers());

        this._modifiesNoise = bodyModScript.ModifiesNoise();
        this._noiseModifier = bodyModScript.NoiseModifier();
        this._isNoiseModifierAdditive = bodyModScript.IsNoiseModifierAdditive();

    }
    private void AddBodyModStatModifiersFromScriptable(List<BodyModStatModifier> bodyModStatModifiers)
    {
        foreach (BodyModStatModifier bodyModStatModifier in bodyModStatModifiers)
        {
            this._bodyModStatModifiers.Add(bodyModStatModifier.Clone());
        }
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
    public void Modify_BodyModStatModifiers(float newValue)
    {
        Debug.Log("Modify_BodyModStatModifiers: " + this._unit + ":" + newValue);
        if (_bodyModStatModifiers.Count < 1)
            return;

        _bodyModStatModifiers[0].StatModifier = newValue;
    }
    public ScriptableBodyMod ScriptableBodyMod()
    {
        return _bodyModScriptable;
    }
    public void SetScriptableBodyMod(ScriptableBodyMod bodyModScriptable)
    {
        this._bodyModScriptable = bodyModScriptable;
    }
    public BombRunUnit Unit()
    {
        return this._unit;
    }
    public void SetBombRunUnit(BombRunUnit unit)
    {
        this._unit = unit;
    }
}
