using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BombRunUnitBodyModManager
{
    private BombRunUnit _unit;

    [SerializeField] private List<BodyMod_Class> _bodyMods = new List<BodyMod_Class>();

    // Our class's constructor. Takes a ScriptableBombRunUnitBaseStats as an argument.
    public BombRunUnitBodyModManager(BombRunUnit unit, List<ScriptableBodyMod> bodyMods)
    {
        this._unit = unit;
        //this._bodyMods.AddRange(bodyMods);
        CreateBodyModClassObjects(bodyMods, _unit);
    }
    private void CreateBodyModClassObjects(List<ScriptableBodyMod> bodyMods, BombRunUnit unit)
    {
        foreach (ScriptableBodyMod bodyMod in bodyMods)
        {
            BodyMod_Class bodyModClass = new BodyMod_Class(bodyMod, unit);
            if (!_bodyMods.Contains(bodyModClass))
            {
                _bodyMods.Add(bodyModClass);
            }
        }
    }

    public void AddBodyMod(ScriptableBodyMod bodyMod)
    {
        BodyMod_Class bodyModClass = new BodyMod_Class(bodyMod, this._unit);
        if (!_bodyMods.Contains(bodyModClass))
        {
            _bodyMods.Add(bodyModClass);
        }
    }
    public List<BodyMod_Class> GetAllBodyMods()
    {
        return _bodyMods;
    }
    public List<BodyMod_Class> GetAllBodyMods_ModifyNoise()
    {
        return _bodyMods.Where(x => x.ModifiesNoise()).ToList();
    }
    public void ModifyBodyMod()
    {
        Debug.Log("BombRunUnitBodyModManager: ModifyBodyMod");
        if (_bodyMods.Count < 1)
            return;

        _bodyMods[0].Modify_BodyModStatModifiers(Mathf.RoundToInt(UnityEngine.Random.Range(2f,10f)));
    }
}
