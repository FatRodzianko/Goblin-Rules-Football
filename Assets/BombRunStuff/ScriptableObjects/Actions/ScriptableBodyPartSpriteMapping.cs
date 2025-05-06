using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Body Part Sprite Mapping", menuName = "Body Part Sprite Mapping")]
public class ScriptableBodyPartSpriteMapping : ScriptableObject
{
    public List<BodyPartSpriteMapping> BodyPartSpriteMappingDictionary = new List<BodyPartSpriteMapping>();

    [Serializable]
    public class BodyPartSpriteMapping
    {
        public BodyPart BodyPart; // the key of the dictionary
        public Sprite Sprite;
    }
}
