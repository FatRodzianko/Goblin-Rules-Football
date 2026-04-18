using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ScriptableBombRunUnit", menuName = "BombRun/Units/New Scriptable Unit")]
public class ScriptableBombRunUnit : ScriptableObject
{
    [Header("Unit Info")]
    [SerializeField] private Transform _unitPrefab;
    [SerializeField] private UnitType _unitType;
    [SerializeField] private DamageMode _defaultDamageMode;
    [SerializeField] private int _sightRange;
    [SerializeField] private int _maxMoveDistance;
    [SerializeField] private float _hearingSensitivity = 1.0f;

    [Header("Unit Visuals")]
    [SerializeField] private RuntimeAnimatorController _animatorController;
    [SerializeField] private Sprite _unitPortrait;

    

    public Transform UnitPrefab()
    {
        return _unitPrefab;
    }
    public UnitType UnitType()
    {
        return _unitType;
    }
    public DamageMode DamageMode()
    {
        return _defaultDamageMode;
    }
    public int SightRange()
    {
        return _sightRange;
    }
    public RuntimeAnimatorController AnimatorController()
    {
        return _animatorController;
    }
    public Sprite UnitPortrait()
    {
        return _unitPortrait;
    }
    public int MaxMoveDistance()
    {
        return _maxMoveDistance;
    }
    public float HearingSensitivity()
    {
        return _hearingSensitivity;
    }

}
