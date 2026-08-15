using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BodyModComponent_Class
{
    [SerializeField] private ScriptableBodyModComponent _bodyModComponentScriptableObject;
    [SerializeField] private BodyMod_Class _bodyMod;

    [Header("Details")]
    [SerializeField] private string _name;
    [SerializeField] private Sprite _sprite;
    [SerializeField] private string _description;

    [Header("Stat Modifiers")]
    [SerializeField] private BodyPart _bodyPart;

    [Header("Component Specifics")]
    [SerializeField] private BodyModComponentType _bodyPartComponentType;
    [SerializeField] private int _componentTierLevel;
    [SerializeField] private bool _hasAnimationEffect;

    // Our class's constructor. Takes a ScriptableBombRunUnitBaseStats as an argument.
    public BodyModComponent_Class(ScriptableBodyModComponent bodyModComponentScriptableObject, BodyMod_Class bodyMod)
    {
        this._bodyModComponentScriptableObject = bodyModComponentScriptableObject;
        this._bodyMod = bodyMod;

        this._name = _bodyModComponentScriptableObject.Name();
        this._sprite = _bodyModComponentScriptableObject.Sprite();
        this._description = _bodyModComponentScriptableObject.Description();

        this._bodyPart = _bodyModComponentScriptableObject.BodyPart();

        this._bodyPartComponentType = _bodyModComponentScriptableObject.BodyPartComponentType();
        this._componentTierLevel = _bodyModComponentScriptableObject.ComponentTierLevel();
        this._hasAnimationEffect = _bodyModComponentScriptableObject.HasAnimationEffect();

    }
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
    public BodyModComponentType BodyPartComponentType()
    {
        return _bodyPartComponentType;
    }
    public bool HasAnimationEffect()
    {
        return _hasAnimationEffect;
    }
    public int ComponentTierLevel()
    {
        return _componentTierLevel;
    }
}
