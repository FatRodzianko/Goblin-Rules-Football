using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum GroundTileType { 
    Green,
    Fairway,
    Rough,
    DeepRough,
    Sand,
    Water,
    Edges,
    DirectionTiles,
    MiniGolfWalls,
    Tool,
    Object,
    CourseMarkers
}

public enum PlaceType { 
    None,
    Single,
    Line,
    Rectangle,
    FloodFill
}

[CreateAssetMenu (fileName = "MapMaker", menuName = "MapMakerObjects/Create Tile")]
public class MapMakerGroundTileBase : ScriptableObject
{
    [ScriptableObjectId] public string Guid;
    [SerializeField] GroundTileType _groundTileType;
    [SerializeField] MapMakerTileTypes _mapMakerTileType;
    [SerializeField] UITileTypes _uiTileType;
    [SerializeField] TileBase _tileBase;
    [SerializeField] Tile _tile;
    [SerializeField] PlaceType _placeType;
    [SerializeField] bool _usePlacementRestrictions;
    [SerializeField] List<MapMakerTileTypes> _placementRestrictions;
    [SerializeField] bool _onlyAllowedOnSpecificGroundType;
    [SerializeField] List<MapMakerTileTypes> _canOnlyBePlacedOnTheseGroundTypes;
    [Header("Minigolf?")]
    [SerializeField] bool _allowedInMiniGolf;
    [SerializeField] bool _miniGolfOnly;

    public List<MapMakerTileTypes> PlacementRestrictions
    {
        get {
            return _usePlacementRestrictions ? _placementRestrictions : _mapMakerTileType.PlacementRestrictions;
        }
    }
    public bool OnlyAllowedOnSpecificGroundType
    {
        get
        {
            return _onlyAllowedOnSpecificGroundType;
        }
    }
    public List<MapMakerTileTypes> CanOnlyBePlacedOnTheseGroundTypes
    {
        get
        {
            return _onlyAllowedOnSpecificGroundType ? _canOnlyBePlacedOnTheseGroundTypes : new List<MapMakerTileTypes>();
        }
    }

    public TileBase TileBase
    {
        get 
        {
            return _tileBase;
        }
    }
    public GroundTileType GroundTileType
    {
        get
        {
            return _groundTileType;
        }
    }
    public MapMakerTileTypes MapMakerTileType
    {
        get
        {
            return _mapMakerTileType;
        }
    }
    public UITileTypes UITileType
    {
        get
        {
            return _uiTileType;
        }
    }
    //public Tile Tile
    //{
    //    get
    //    {
    //        return _tile;
    //    }
    //}
    public PlaceType PlaceType
    {
        get 
        {
            return _placeType == PlaceType.None ? _mapMakerTileType.PlaceType : _placeType ;
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
