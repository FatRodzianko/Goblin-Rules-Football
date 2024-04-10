using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapInitializer : SingletonInstance<TileMapInitializer>
{
    [SerializeField] List<MapMakerTileTypes> _tileTypesForTileMaps;
    [SerializeField] Transform _grid;

    private void Start()
    {
        //CreateMaps();
        //AssignTileMaps();
    }
    protected override void Awake()
    {
        base.Awake();
        AssignTileMaps();
    }
    void CreateMaps()
    {
        foreach (MapMakerTileTypes tileTypes in _tileTypesForTileMaps)
        {
            // create object
            GameObject obj = new GameObject("Tilemap_" + tileTypes.name);
            // assign tilemap features
            Tilemap map = obj.AddComponent<Tilemap>();
            TilemapRenderer tr = obj.AddComponent<TilemapRenderer>();
            TilemapCollider2D collider = obj.AddComponent<TilemapCollider2D>();
            collider.isTrigger = true;

            if (tileTypes.name != "Edges" && tileTypes.name != "DirectionTiles")
            {
                GroundTopDown groundTopDown = obj.AddComponent<GroundTopDown>();
                groundTopDown.groundType = tileTypes.name.ToLower();
                groundTopDown.MyTiles = map;
            }

            //set parent
            obj.transform.SetParent(_grid);

            // add settings for the tilemap
            tr.sortingOrder = tileTypes.SortingOrder;

            tileTypes.Tilemap = map;
        }
    }
    void AssignTileMaps()
    {
        foreach (MapMakerTileTypes tileTypes in _tileTypesForTileMaps)
        {
            foreach (Transform child in _grid)
            {
                if (child.name == tileTypes.name)
                {
                    tileTypes.Tilemap = child.GetComponent<Tilemap>();
                    Debug.Log("AssignTileMaps: tiletype: " + tileTypes.name + " and tilemap: " + tileTypes.Tilemap.name);
                }
            }
        }
    }
}
