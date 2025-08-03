using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitTypeBodyPartActionValueMapping
{
    public UnitType UnitType; // the key of the dictionary
    public ScriptableUnitBodyPartActionValues UnitBodyPartActionValues;
}
public class BombRunUnitActionValueManager : MonoBehaviour
{
    [SerializeField] List<UnitTypeBodyPartActionValueMapping> _unitTypeBodyPartActionValueMapping = new List<UnitTypeBodyPartActionValueMapping>();
    [SerializeField] ScriptableUnitBodyPartActionValues _defaultUnitBodyPartActionValues;

    public int GetUnitBodyPartActionValue(UnitType unitType, BodyPart bodyPart)
    {
        if (!_unitTypeBodyPartActionValueMapping.Exists(x => x.UnitType == unitType))
        {
            Debug.Log("GetUnitBodyPartActionValue: could not Unit Body Part Action Values for unity type: " + unitType.ToString());
            return _defaultUnitBodyPartActionValues.GetActionValueForBodyPart(bodyPart);
        }


        return _unitTypeBodyPartActionValueMapping.First(x => x.UnitType == unitType).UnitBodyPartActionValues.GetActionValueForBodyPart(bodyPart);
    }
}
