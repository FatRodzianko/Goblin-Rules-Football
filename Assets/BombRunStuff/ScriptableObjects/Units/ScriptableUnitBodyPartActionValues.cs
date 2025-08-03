using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BodyPartActionValueMapping
{
    public BodyPart BodyPart; // the key of the dictionary
    public int ActionValueModifer;
}
[CreateAssetMenu(fileName = "UnitBodyPartActionValues", menuName = "BombRun/Units/Unit Body Part Action Values")]
public class ScriptableUnitBodyPartActionValues : ScriptableObject
{
    [SerializeField] private List<BodyPartActionValueMapping> _bodyPartActionValueMappingDictionary = new List<BodyPartActionValueMapping>();

    public List<BodyPartActionValueMapping> BodyPartActionValueMappingDictionary()
    {
        return _bodyPartActionValueMappingDictionary;
    }
    public int GetActionValueForBodyPart(BodyPart bodyPart)
    {
        if (!_bodyPartActionValueMappingDictionary.Exists(x => x.BodyPart == bodyPart))
        {
            Debug.Log("GetBodyPartSpriteMappingForBodyPart: could not find body part: " + bodyPart.ToString());
            return 0;
        }

        return _bodyPartActionValueMappingDictionary.First(x => x.BodyPart == bodyPart).ActionValueModifer;
    }
}
