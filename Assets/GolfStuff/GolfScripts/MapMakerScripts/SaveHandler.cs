using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class SaveHandler : MonoBehaviour
{
    Dictionary<string, Tilemap> _tileMaps = new Dictionary<string, Tilemap>();
    [SerializeField] BoundsInt _bounds;
    [SerializeField] string _filename = "tilemapData.json";
    [SerializeField] List<Tilemap> _allMaps = new List<Tilemap>();

    private void Start()
    {
        InitTilemaps();
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

        foreach (Tilemap map in _allMaps)
        {
            _tileMaps.Add(map.name, map);
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

                    if (tile != null)
                    {
                        TileInfo ti = new TileInfo(tile, pos);
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

            if (mapData.tiles != null && mapData.tiles.Count > 0)
            {
                foreach (TileInfo tile in mapData.tiles)
                {
                    map.SetTile(tile.position, tile.tile);
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
    public TileBase tile;
    public Vector3Int position;

    public TileInfo(TileBase tile, Vector3Int pos)
    {
        this.tile = tile;
        position = pos;
    }
}
