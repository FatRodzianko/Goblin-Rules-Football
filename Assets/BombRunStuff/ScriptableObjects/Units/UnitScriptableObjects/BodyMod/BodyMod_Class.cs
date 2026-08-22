using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BodyMod_Class
{
    [SerializeField] private BombRunUnit _unit;
    [SerializeField] private ScriptableBodyMod _bodyModScriptable;
    [SerializeField] private bool _isEquipped;

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

    [Header("Body Mod Components")]
    [SerializeField] private List<BodyModComponent_Class> _bodyModComponents = new List<BodyModComponent_Class>();
    [SerializeField] private List<BodyModComponentRequirement> _bodyModComponentRequirements = new List<BodyModComponentRequirement>();
    [SerializeField] private List<ScriptableBodyModComponent> _requiredBodyModyComponents = new List<ScriptableBodyModComponent>();

    // events
    public event EventHandler OnBodyModEquipped;
    public event EventHandler OnBodyModUnEquipped;
    public event EventHandler OnBodyModDestroyed;

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

        AddBodyModComponentsFromScriptable(bodyModScript.BodyModComponents());
        AddBodyModComponentRequirements(bodyModScript.BodyModComponentRequirements());
        AddRequiredBodyModyComponents(bodyModScript.RequiredBodyModComponents());
    }
    private void AddBodyModStatModifiersFromScriptable(List<BodyModStatModifier> bodyModStatModifiers)
    {
        foreach (BodyModStatModifier bodyModStatModifier in bodyModStatModifiers)
        {
            this._bodyModStatModifiers.Add(bodyModStatModifier.Clone());
            //this._unit.StatModifierUpdated(bodyModStatModifier.StatType);
        }
    }
    private void AddBodyModComponentsFromScriptable(List<ScriptableBodyModComponent> bodyModComponetScriptableObjects)
    {
        foreach (ScriptableBodyModComponent bodyModComponetScriptableObject in bodyModComponetScriptableObjects)
        {
            BodyModComponent_Class bodyModComponent = new BodyModComponent_Class(bodyModComponetScriptableObject, this);
            _bodyModComponents.Add(bodyModComponent);
        }
    }
    private void AddBodyModComponentRequirements(List<BodyModComponentRequirement> bodyModComponentRequirements)
    {
        foreach (BodyModComponentRequirement bodyModComponentRequirement in bodyModComponentRequirements)
        {
            _bodyModComponentRequirements.Add(bodyModComponentRequirement.Clone());
        }
    }
    private void AddRequiredBodyModyComponents(List<ScriptableBodyModComponent> requiredBodyModComponents)
    {
        foreach (ScriptableBodyModComponent bodyModComponent in requiredBodyModComponents)
        {
            this._requiredBodyModyComponents.Add(bodyModComponent);
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
        this._unit.StatModifierUpdated(_bodyModStatModifiers[0].StatType);
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
    public List<BodyModStatModifier> BodyStatModifiers()
    {
        return _bodyModStatModifiers;
    }
    public List<BodyModComponentRequirement> BodyModComponentRequirements()
    {
        return _bodyModComponentRequirements;
    }
    public List<ScriptableBodyModComponent> RequiredBodyModComponents()
    {
        return _requiredBodyModyComponents;
    }
    public bool IsEquipped()
    {
        return _isEquipped;
    }
    public void EquipBodyMod()
    {
        _isEquipped = true;
        OnBodyModEquipped?.Invoke(this, EventArgs.Empty);
    }
    public void UnEquipBodyMod()
    {
        _isEquipped = false;
        OnBodyModUnEquipped?.Invoke(this, EventArgs.Empty);
    }
    public void DestroyBodyMod()
    {
        OnBodyModDestroyed?.Invoke(this, EventArgs.Empty);
    }

}
