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
    public void Eraser(Vector3Int[] positions, out MapMakerHistoryStep historyStep)
    {
        Debug.Log("MapMakerToolController: Eraser: delete how many positions? " + positions.Length);

        List<MapMakerHistoryItem> items = new List<MapMakerHistoryItem>();

        // loop through every position in the positions array
        foreach (Vector3Int position in positions)
        {
            // OLD FROM VIDEO
            //MapMakerHistoryItem item = null;

            //// Check to see if any of the tile maps have a tile set. If so, create a new history item to save. Then set the tile to null
            //_tileMapReferenceHolder.AllMaps.Any(map =>
            //{
            //    if (map.HasTile(position))
            //    {
            //        MapMakerGroundTileBase mapMakerGroundTileBase = null;

            //        if (map.name == "Object")
            //        {
            //            MapMakerBuilder builder = MapMakerBuilder.GetInstance();
            //            mapMakerGroundTileBase = builder.GetObstacleAtPosition(position);
            //            builder.RemoveObstacle(position);
            //        }

            //        item = new MapMakerHistoryItem(map, map.GetTile(position), null, position, mapMakerGroundTileBase);

            //        // Set the tile to "null" to remove it from the map
            //        map.SetTile(position, null);

            //        return true;
            //    }
            //    else
            //        return false;
            //});
            // OLD FROM VIDEO

            List<Tilemap> maps = _tileMapReferenceHolder.AllMaps.FindAll(map => map.HasTile(position));
            if (maps.Count > 0)
            {
                foreach (Tilemap map in maps)
                {
                    MapMakerHistoryItem item = null;
                    if (map.HasTile(position))
                    {
                        MapMakerGroundTileBase mapMakerGroundTileBase = null;

                        if (map.name == "Object")
                        {
                            MapMakerBuilder builder = MapMakerBuilder.GetInstance();
                            mapMakerGroundTileBase = builder.GetObstacleAtPosition(position);
                            builder.RemoveObstacle(position);
                        }

                        item = new MapMakerHistoryItem(map, map.GetTile(position), null, position, mapMakerGroundTileBase);

                        // Set the tile to "null" to remove it from the map
                        map.SetTile(position, null);
                    }
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
            }

            // OLD FROM VIDEO
            //if (item != null)
            //{
            //    items.Add(item);
            //}
            // OLD FROM VIDEO

            //_tileMapReferenceHolder.AllMaps.FindAll(map => map.HasTile(position));


        }

        historyStep = new MapMakerHistoryStep(items.ToArray());

    }
}
