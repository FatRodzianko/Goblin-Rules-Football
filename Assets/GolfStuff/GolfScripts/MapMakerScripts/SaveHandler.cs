using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class SaveHandler : MonoBehaviour
{
    Dictionary<string, Tilemap> _tileMaps = new Dictionary<string, Tilemap>();

    // Dictionaries to map MapMakerGroundTileBase objects to a tile object (saving), and then mapping that tile object to a GUID (loading)
    Dictionary<TileBase, MapMakerGroundTileBase> _tileBaseToMapMakerObject = new Dictionary<TileBase, MapMakerGroundTileBase>();
    Dictionary<string, TileBase> _guidToTileBase = new Dictionary<string, TileBase>();

    [SerializeField] BoundsInt _bounds;
    [SerializeField] string _filename = "tilemapData.json";
    //[SerializeField] List<Tilemap> _allMaps = new List<Tilemap>();
    [SerializeField] TileMapReferenceHolder _tileMapReferenceHolder;
    [SerializeField] MapMakerBuilder _mapMakerBuilder;

    private void Start()
    {
        InitTilemaps();
        InitTileReferences();

        if (!_tileMapReferenceHolder)
            _tileMapReferenceHolder = this.transform.GetComponent<TileMapReferenceHolder>();
        if (!_mapMakerBuilder)
            _mapMakerBuilder = MapMakerBuilder.GetInstance();
    }
    void InitTilemaps()
    {
        // get all tilemaps from the scene
        //Tilemap[] maps = FindObjectsOfType<Tilemap>();

        //foreach (var map in maps)
        //{
        //    if (_tileMaps.ContainsKey(map.name))
        //        return;
        //    _tileMaps.Add(map.name, map);
        //}

        foreach (Tilemap map in _tileMapReferenceHolder.AllMaps)
        {
            _tileMaps.Add(map.name, map);
        }
    }
    void InitTileReferences()
    {
        MapMakerGroundTileBase[] mapMakerTiles = Resources.LoadAll<MapMakerGroundTileBase>("MapMakerGolf/GroundTileScriptables");
        foreach (MapMakerGroundTileBase tile in mapMakerTiles)
        {
            if (_tileBaseToMapMakerObject.ContainsKey(tile.TileBase))
            {
                Debug.LogError("InitTileReferences: Tilebase: " + tile.TileBase.name + " is already in use by: " + _tileBaseToMapMakerObject[tile.TileBase].name);
                continue;
            }
            Debug.Log("InitTileReferences: tile Guid: " + tile.Guid);
            _tileBaseToMapMakerObject.Add(tile.TileBase, tile);
            _guidToTileBase.Add(tile.Guid, tile.TileBase);

        }
    }
    public void OnSave()
    {
        List<TilemapData> data = new List<TilemapData>();

        // foreach existing tilemap
        foreach (var mapObj in _tileMaps)
        {
            TilemapData mapData = new TilemapData();
            mapData.key = mapObj.Key;

            BoundsInt boundsForThisMap = mapObj.Value.cellBounds;

            for (int x = boundsForThisMap.xMin; x < boundsForThisMap.xMax; x++)
            {
                for (int y = boundsForThisMap.yMin; y < boundsForThisMap.yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    TileBase tile = mapObj.Value.GetTile(pos);

                    if (tile != null && _tileBaseToMapMakerObject.ContainsKey(tile))
                    {
                        string guid = _tileBaseToMapMakerObject[tile].Guid;
                        TileInfo ti = new TileInfo(pos, guid);
                        mapData.tiles.Add(ti);
                    }
                }
            }
            data.Add(mapData);
        }

        // save the tilemaps to a file
        FileHandler.SaveToJSON<TilemapData>(data, _filename);
        
    }
    public void OnLoad()
    {
        List<TilemapData> data = FileHandler.ReadListFromJSON<TilemapData>(_filename);

        foreach (var mapData in data)
        {
            if (!_tileMaps.ContainsKey(mapData.key))
            {
                Debug.LogError("OnLoad: found saved data for tilemap called: '" + ",' but corresponding tilemap does not exist. Skipping...");
                continue;
            }

            // get tilemap
            var map = _tileMaps[mapData.key];
            map.ClearAllTiles();
            _mapMakerBuilder.ClearAllObstacles();

            if (mapData.tiles != null && mapData.tiles.Count > 0)
            {
                //old way before asset database
                foreach (TileInfo tile in mapData.tiles)
                {
                    if (!_guidToTileBase.ContainsKey(tile.GuidForTile))
                    {
                        Debug.LogError("OnLoad: Could not find GUID: '" + tile.GuidForTile + " in _guidToTileBase dictionary. Skipping...");
                        continue;
                    }

                    TileBase tileBase = _guidToTileBase[tile.GuidForTile];
                    //map.SetTile(tile.position, _guidToTileBase[tile.GuidForTile]);
                    map.SetTile(tile.position, tileBase);

                    // Check if the tile is being placed on the object map. If so, spawn the appropriate object
                    if (map.name == "Object")
                    {
                        MapMakerGroundTileBase groundTileBase = _tileBaseToMapMakerObject[tileBase];
                        if (groundTileBase.GetType() == typeof(MapMakerObstacle))
                        {
                            _mapMakerBuilder.PlaceObstacle(tile.position, (MapMakerObstacle)groundTileBase);
                        }
                    }
                }

            }
        }
    }
}

[Serializable]
public class TilemapData
{
    public string key; // the key of the dictionary
    public List<TileInfo> tiles = new List<TileInfo>();
}

[Serializable]
public class TileInfo
{
    public string GuidForTile;
    public Vector3Int position;

    public TileInfo(Vector3Int pos, string guid)
    {
        position = pos;
        GuidForTile = guid;
    }
}
