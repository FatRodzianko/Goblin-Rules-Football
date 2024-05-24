using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ground Tile Order", menuName = "Ground Tile Order")]
public class GroundTileOrder : ScriptableObject
{
    public List<GroundTileOrderItem> GroundTileOrderDictionary = new List<GroundTileOrderItem>();


    [Serializable]
    public class GroundTileOrderItem
    {
        public string GroundType; // the key of the dictionary
        public int GroundOrderValue;
    }
}
