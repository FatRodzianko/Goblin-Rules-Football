using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BombRunUnitBodyModManager
{
    private BombRunUnit _unit;

    [SerializeField] private List<ScriptableBodyMod> _bodyMods = new List<ScriptableBodyMod>();

    // Our class's constructor. Takes a ScriptableBombRunUnitBaseStats as an argument.
    public BombRunUnitBodyModManager(BombRunUnit unit, List<ScriptableBodyMod> bodyMods)
    {
        this._unit = unit;
        this._bodyMods.AddRange(bodyMods);
    }

    public void AddBodyMod(ScriptableBodyMod bodyMod)
    {
        if (!_bodyMods.Contains(bodyMod))
        {
            _bodyMods.Add(bodyMod);
        }
    }
    public List<ScriptableBodyMod> GetAllBodyMods()
    {
        return _bodyMods;
    }
    public List<ScriptableBodyMod> GetAllBodyMods_ModifyNoise()
    {
        return _bodyMods.Where(x => x.ModifiesNoise()).ToList();
    }
}
