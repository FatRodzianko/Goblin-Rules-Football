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
    MiniGolfWalls
}

[CreateAssetMenu (fileName = "MapMaker", menuName = "MapMakerObjects/Create Tile")]
public class MapMakerGroundTileBase : ScriptableObject
{
    [SerializeField] GroundTileType _groundTileType;
    [SerializeField] TileBase _tileBase;
    [SerializeField] Tile _tile;

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
}
