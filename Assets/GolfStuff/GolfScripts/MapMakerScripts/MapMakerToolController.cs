using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class MapMakerToolController : MonoBehaviour
{
    public static MapMakerToolController instance;
    [SerializeField] TileMapReferenceHolder _tileMapReferenceHolder;

    private void Awake()
    {
        MakeInstance();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        if (!_tileMapReferenceHolder)
            _tileMapReferenceHolder = this.transform.GetComponent<TileMapReferenceHolder>();
    }
    //public void Eraser(Vector3Int position)
    //{
    //    Debug.Log("MapMakerToolController: Eraser: " + position);

    //    foreach (Tilemap map in _tileMapReferenceHolder.AllMaps)
    //    {
    //        if (map.HasTile(position))
    //        {
    //            map.SetTile(position, null);

    //            // remove object if it was erased
    //            if (map.name == "Object")
    //            {
    //                MapMakerBuilder builder = MapMakerBuilder.GetInstance();
    //                builder.RemoveObstacle(position);
    //            }
    //        }

    //    }
    //}
    public void Eraser(Vector3Int[] positions, Tilemap tilemapToUse, out MapMakerHistoryStep historyStep)
    {
        Debug.Log("MapMakerToolController: Eraser: delete how many positions? " + positions.Length + " is tileMaptoUse null? " + tilemapToUse);

        List<MapMakerHistoryItem> items = new List<MapMakerHistoryItem>();

        // loop through every position in the positions array
        foreach (Vector3Int position in positions)
        {
            // This is for deleting all maps. Eventually have it so it only deletes from your selected map?
            //List<Tilemap> maps = _tileMapReferenceHolder.AllMaps.FindAll(map => map.HasTile(position));
            List<Tilemap> maps = new List<Tilemap>();
            if (tilemapToUse == null)
            {
                maps.AddRange(_tileMapReferenceHolder.AllMaps.FindAll(map => map.HasTile(position)));
            }
            else
            {
                maps.Add(tilemapToUse);
            }   

            if (maps.Count > 0)
            {
                foreach (Tilemap map in maps)
                {
                    MapMakerHistoryItem item = null;
                    if (map.HasTile(position))
                    {
                        MapMakerBuilder builder = MapMakerBuilder.GetInstance();
                        //MapMakerGroundTileBase mapMakerGroundTileBase = null;
                        TileBase prevTileBase = map.GetTile(position);
                        MapMakerGroundTileBase prevMapMakerGroundTileBase = builder.GetMapMakerGroundTileBaseFromTileBase(prevTileBase);

                        if (map.name == "Object")
                        {
                            //MapMakerBuilder builder = MapMakerBuilder.GetInstance();
                            //mapMakerGroundTileBase = builder.GetObstacleAtPosition(position);
                            builder.RemoveObstacle(position);
                        }

                        //item = new MapMakerHistoryItem(map, map.GetTile(position), null, position, mapMakerGroundTileBase);
                        item = new MapMakerHistoryItem(map, map.GetTile(position), null, position, prevMapMakerGroundTileBase, null);

                        // Set the tile to "null" to remove it from the map
                        map.SetTile(position, null);
                    }
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
            }
        }

        historyStep = new MapMakerHistoryStep(items.ToArray());

    }
}
