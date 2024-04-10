using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapMaker", menuName = "MapMakerObjects/Create UI Tile Type")]
public class UITileTypes : ScriptableObject
{
    [SerializeField] int _siblingIndex = 0;
    [SerializeField] Color _backgroundColor;
    [SerializeField] string _uiName;

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
    public string UIName
    {
        get
        {
            if (string.IsNullOrEmpty(this._uiName))
                return this.name;
            else
                return _uiName.Replace("\\n", "\n");
        }
    }
}
