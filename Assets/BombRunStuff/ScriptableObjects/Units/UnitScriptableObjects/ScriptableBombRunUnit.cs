using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableBombRunUnit", menuName = "BombRun/Units/New Scriptable Unit")]
public class ScriptableBombRunUnit : ScriptableObject
{
    [SerializeField] private Transform _unitPrefab;
    [SerializeField] private UnitType _unitType;
    [SerializeField] private DamageMode _defaultDamageMode;
    [SerializeField] private int _sightRange;
    //[SerializeField] private List<ScriptableBaseActions> _actionList = new List<ScriptableBaseActions>();

    [SerializeField] private RuntimeAnimatorController _animatorController;

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

}
