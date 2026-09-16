using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableItem", menuName = "BombRun/BodyMods/New Scriptable Base Item")]
public class ScriptableItem : ScriptableObject
{
    [Header("Base Item Details")]
    [SerializeField] protected string _name;
    [SerializeField] protected Sprite _sprite;
    [SerializeField] protected string _description;
    [SerializeField] protected bool _equippable;

    [Header("Base Item Body Parts")]
    [SerializeField] protected BodyPart _bodyPart;

    [Header("Item Stack/Count")]
    [SerializeField] protected bool _stackable;

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
    public bool Stackable()
    {
        return _stackable;
    }
    public bool Equippable()
    {
        return _equippable;
    }
}
