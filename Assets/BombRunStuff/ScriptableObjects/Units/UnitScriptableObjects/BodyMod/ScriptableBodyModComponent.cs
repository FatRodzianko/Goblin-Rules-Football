using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BodyModComponentType
{
    None,
    Fastener, // how the component is attached to the body?
    Compute, // computer chips for things that need those?
    Electronics, // things like wires / boards / cables / whatever
    Enhancer, // things doing the "enhancement." Lense for your eyes. Motor that makes you move faster. Battery/power source for extra power?
    PowerSource
}

[CreateAssetMenu(fileName = "ScriptableBodyModComponent", menuName = "BombRun/BodyMods/New Scriptable BodyMod Component")]
public class ScriptableBodyModComponent : ScriptableObject
{
    [Header("Details")]
    [SerializeField] private string _name;
    [SerializeField] private Sprite _sprite;
    [SerializeField] private string _description;

    [Header("Stat Modifiers")]
    [SerializeField] private BodyPart _bodyPart;

    [Header("Component Specifics")]
    [SerializeField] private BodyModComponentType _bodyModComponentType;
    [SerializeField] private int _componentTierLevel;
    [SerializeField] private bool _hasAnimationEffect;

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
        return _bodyModComponentType;
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
