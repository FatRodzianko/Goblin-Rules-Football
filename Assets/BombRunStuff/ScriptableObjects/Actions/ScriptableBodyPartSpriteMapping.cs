using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Body Part Sprite Mapping", menuName = "Body Part Sprite Mapping")]
public class ScriptableBodyPartSpriteMapping : ScriptableObject
{
    [SerializeField] private List<BodyPartSpriteMapping> _bodyPartSpriteMappingDictionary = new List<BodyPartSpriteMapping>();

    [Serializable]
    public class BodyPartSpriteMapping
    {
        public BodyPart BodyPart; // the key of the dictionary
        public ScriptableBodyPartSprites Sprites;
    }

    public List<BodyPartSpriteMapping> BodyPartSpriteMappingDictionary()
    {
        return _bodyPartSpriteMappingDictionary;
    }
    public BodyPartSpriteMapping GetBodyPartSpriteMappingForBodyPart(BodyPart bodyPart)
    {
        if (!_bodyPartSpriteMappingDictionary.Exists(x => x.BodyPart == bodyPart))
        {
            Debug.Log("FreezeBodyPart: could not find body part: " + bodyPart.ToString());
            return null;
        }

        return _bodyPartSpriteMappingDictionary.First(x => x.BodyPart == bodyPart);
    }
}
