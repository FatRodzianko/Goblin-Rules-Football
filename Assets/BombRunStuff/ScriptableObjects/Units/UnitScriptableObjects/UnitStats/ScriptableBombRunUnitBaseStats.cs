using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Base Stats", menuName = "BombRun/Units/New Scriptable Unit Base Stats")]
public class ScriptableBombRunUnitBaseStats : ScriptableObject
{
    [Header("Base Stats")]
    [Header("Vision")]
    [SerializeField] private int _baseSightDistance;
    [SerializeField] private float _baseFOV;

    [Header("Movement")]
    [SerializeField] private int _baseMaxMoveDistance;

    [Header("Noise")]
    [SerializeField] private float _baseHearingSensitivity;

    public int BaseSightDistance()
    {
        return _baseSightDistance;
    }
    public int BaseMaxMoveDistance()
    {
        return _baseMaxMoveDistance;
    }
    public float BaseHearingSensitivity()
    {
        return _baseHearingSensitivity;
    }
    public float BaseFOV()
    {
        return _baseFOV;
    }
}
