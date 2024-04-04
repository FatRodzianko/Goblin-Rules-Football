using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "MapMaker", menuName = "MapMakerObjects/Create Tile Type")]
public class MapMakerTileTypes : ScriptableObject
{
    [SerializeField] PlaceType _placeType;
    [SerializeField] int _sortingOrder = 0;
    [SerializeField] Tilemap _tileMap;
    [SerializeField] List<MapMakerTileTypes> _placementRestrictions;

    public List<MapMakerTileTypes> PlacementRestrictions
    {
        get {
            return _placementRestrictions;
        }
    }

    public PlaceType PlaceType
    {
        get {
            return _placeType;
        }
    }
    public Tilemap Tilemap
    {
        get
        {
            return _tileMap;
        }
        set
        {
            Debug.Log("MapMakerTileTypes: setting tilemap to: " + Tilemap + " for: " + this.name);
            _tileMap = value;
        }
    }
    public int SortingOrder
    {
        get
        {
            return _sortingOrder;
        }
    }
}
