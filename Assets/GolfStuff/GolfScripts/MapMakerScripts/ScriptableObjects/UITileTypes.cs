using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapMaker", menuName = "MapMakerObjects/Create UI Tile Type")]
public class UITileTypes : ScriptableObject
{
    [SerializeField] int _siblingIndex = 0;
    [SerializeField] Color _backgroundColor;

    public int SiblingIndex 
    {
        get {
            return _siblingIndex;
        }
    }
    public Color BackgroundColor
    {
        get {
            return _backgroundColor;
        }
    }
}
