using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapMaker", menuName = "MapMakerObjects/Create UI Tile Type")]
public class UITileTypes : ScriptableObject
{
    [SerializeField] int _siblingIndex = 0;
    [SerializeField] Color _backgroundColor;
    [SerializeField] string _uiName;
    [SerializeField] bool _allowedInMiniGolf;
    [SerializeField] bool _miniGolfOnly;

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
    public bool AllowedInMiniGolf
    {
        get
        {
            return _allowedInMiniGolf;
        }
    }
    public bool MiniGolfOnly
    {
        get
        {
            return _miniGolfOnly;
        }
    }
}
