using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum GroundTileType { 
    None,
    Green,
    Fairway,
    Rough,
    DeepRough,
    Sand,
    Water,
    Edges,
    DirectionTiles,
    MiniGolfWalls
}

public enum PlaceType { 
    Single,
    Line,
    Rectangle
}

[CreateAssetMenu (fileName = "MapMaker", menuName = "MapMakerObjects/Create Tile")]
public class MapMakerGroundTileBase : ScriptableObject
{
    [SerializeField] GroundTileType _groundTileType;
    [SerializeField] TileBase _tileBase;
    [SerializeField] Tile _tile;
    [SerializeField] PlaceType _placeType;

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
    public Tile Tile
    {
        get
        {
            return _tile;
        }
    }
    public PlaceType PlaceType
    {
        get 
        {
            return _placeType;
        }
    }
}
