using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Linq;

public class SaveHandler : MonoBehaviour
{
    //Dictionary<string, Tilemap> _tileMaps = new Dictionary<string, Tilemap>();

    // Dictionaries to map MapMakerGroundTileBase objects to a tile object (saving), and then mapping that tile object to a GUID (loading)
    //Dictionary<TileBase, MapMakerGroundTileBase> _tileBaseToMapMakerObject = new Dictionary<TileBase, MapMakerGroundTileBase>();
    //Dictionary<string, TileBase> _guidToTileBase = new Dictionary<string, TileBase>();

    [SerializeField] BoundsInt _bounds;
    [SerializeField] string _filename = "tilemapData.json";
    //[SerializeField] List<Tilemap> _allMaps = new List<Tilemap>();
    [SerializeField] TileMapReferenceHolder _tileMapReferenceHolder;
    [SerializeField] MapMakerBuilder _mapMakerBuilder;
    // MapMakerUIManager
    [SerializeField] MapMakerUIManager _mapMakerUIManager;

    private void Start()
    {
        //InitTilemaps();
        //InitTileReferences();

        if (!_tileMapReferenceHolder)
            _tileMapReferenceHolder = this.transform.GetComponent<TileMapReferenceHolder>();
        if (!_mapMakerBuilder)
            _mapMakerBuilder = MapMakerBuilder.GetInstance();
        if (_mapMakerUIManager == null)
        {
            _mapMakerUIManager = GameObject.FindGameObjectWithTag("MapMakerUIManager").GetComponent<MapMakerUIManager>();
        }
    }
    //void InitTilemaps()
    //{
    //    // get all tilemaps from the scene
    //    //Tilemap[] maps = FindObjectsOfType<Tilemap>();

    //    //foreach (var map in maps)
    //    //{
    //    //    if (_tileMaps.ContainsKey(map.name))
    //    //        return;
    //    //    _tileMaps.Add(map.name, map);
    //    //}

    //    foreach (Tilemap map in _tileMapReferenceHolder.AllMaps)
    //    {
    //        _tileMaps.Add(map.name, map);
    //    }
    //}
    //void InitTileReferences()
    //{
    //    MapMakerGroundTileBase[] mapMakerTiles = Resources.LoadAll<MapMakerGroundTileBase>("MapMakerGolf/GroundTileScriptables");
    //    foreach (MapMakerGroundTileBase tile in mapMakerTiles)
    //    {
    //        if (_tileBaseToMapMakerObject.ContainsKey(tile.TileBase))
    //        {
    //            Debug.LogError("InitTileReferences: Tilebase: " + tile.TileBase.name + " is already in use by: " + _tileBaseToMapMakerObject[tile.TileBase].name);
    //            continue;
    //        }
    //        Debug.Log("InitTileReferences: tile Guid: " + tile.Guid);
    //        _tileBaseToMapMakerObject.Add(tile.TileBase, tile);
    //        _guidToTileBase.Add(tile.Guid, tile.TileBase);

    //    }
    //}

    //
    // START OLD FROM TUTORIAL
    //
    //public void OnSave()
    //{
    //    List<TilemapData> data = new List<TilemapData>();

    //    // foreach existing tilemap
    //    foreach (var mapObj in _mapMakerBuilder.GetTileMapNameToTileMapMapping())
    //    {
    //        TilemapData mapData = new TilemapData();
    //        mapData.key = mapObj.Key;

    //        BoundsInt boundsForThisMap = mapObj.Value.cellBounds;

    //        for (int x = boundsForThisMap.xMin; x < boundsForThisMap.xMax; x++)
    //        {
    //            for (int y = boundsForThisMap.yMin; y < boundsForThisMap.yMax; y++)
    //            {
    //                Vector3Int pos = new Vector3Int(x, y, 0);
    //                TileBase tile = mapObj.Value.GetTile(pos);

    //                //if (tile != null && _tileBaseToMapMakerObject.ContainsKey(tile))
    //                //{
    //                //    string guid = _tileBaseToMapMakerObject[tile].Guid;
    //                //    TileInfo ti = new TileInfo(pos, guid);
    //                //    mapData.tiles.Add(ti);
    //                //}

    //                MapMakerGroundTileBase mapMakerGroundTileBase = _mapMakerBuilder.GetMapMakerGroundTileBaseFromTileBase(tile);
    //                if (tile != null && mapMakerGroundTileBase != null)
    //                {
    //                    string guid = mapMakerGroundTileBase.Guid;
    //                    TileInfo ti = new TileInfo(pos, guid);
    //                    mapData.tiles.Add(ti);
    //                }
    //            }
    //        }
    //        data.Add(mapData);
    //    }

    //    // save the tilemaps to a file
    //    FileHandler.SaveToJSON<TilemapData>(data, _filename);

    //}
    //public void OnLoad()
    //{
    //    List<TilemapData> data = FileHandler.ReadListFromJSON<TilemapData>(_filename);

    //    foreach (var mapData in data)
    //    {
    //        //if (!_tileMaps.ContainsKey(mapData.key))
    //        if (!_mapMakerBuilder.DoesTileMapExistInMapping(mapData.key))
    //        {
    //            Debug.LogError("OnLoad: found saved data for tilemap called: '" + ",' but corresponding tilemap does not exist. Skipping...");
    //            continue;
    //        }

    //        // get tilemap
    //        //var map = _tileMaps[mapData.key];
    //        var map = _mapMakerBuilder.GetTileMapFromTileMapName(mapData.key);
    //        map.ClearAllTiles();
    //        _mapMakerBuilder.ClearAllObstacles();

    //        if (mapData.tiles != null && mapData.tiles.Count > 0)
    //        {
    //            //old way before asset database
    //            foreach (TileInfo tile in mapData.tiles)
    //            {
    //                //if (!_guidToTileBase.ContainsKey(tile.GuidForTile))
    //                if (!_mapMakerBuilder.DoesGUIDExistForTileBaseInMapping(tile.GuidForTile))
    //                {
    //                    Debug.LogError("OnLoad: Could not find GUID: '" + tile.GuidForTile + " in _guidToTileBase dictionary. Skipping...");
    //                    continue;
    //                }

    //                //TileBase tileBase = _guidToTileBase[tile.GuidForTile];
    //                TileBase tileBase = _mapMakerBuilder.GetTileBaseFromGUID(tile.GuidForTile);
    //                //map.SetTile(tile.position, _guidToTileBase[tile.GuidForTile]);
    //                map.SetTile(tile.position, tileBase);

    //                // Check if the tile is being placed on the object map. If so, spawn the appropriate object
    //                if (map.name == "Object")
    //                {
    //                    if (_mapMakerBuilder.DoesMapMakerGroundTileBaseExistForTileBaseInMapping(tileBase))
    //                    {
    //                        MapMakerGroundTileBase groundTileBase = _mapMakerBuilder.GetMapMakerGroundTileBaseFromTileBase(tileBase);
    //                        if (groundTileBase.GetType() == typeof(MapMakerObstacle))
    //                        {
    //                            _mapMakerBuilder.PlaceObstacle(tile.position, (MapMakerObstacle)groundTileBase);
    //                        }
    //                    }

    //                }
    //            }

    //        }
    //    }
    //}
    //
    // END OLD FROM TUTORIAL
    //
    public void CreateNewCourse(string courseName, bool isMiniGolf)
    {
        CourseData newCourse = new CourseData();
        newCourse.CourseName = courseName;
        newCourse.IsMiniGolf = isMiniGolf;
        newCourse.CourseId = Guid.NewGuid().ToString();
        //newCourse.HolesInCourseFileNames = new List<string>(); // setting as an empty string when creating a new course. Will add to this as new holes are created?
        newCourse.HolesInCourse = new List<HoleData>();

        string filename = "course_" + newCourse.CourseName.Replace(" ", string.Empty) + "_" + newCourse.CourseId + ".json";
        newCourse.RelativeFilePath = filename;

        Debug.Log("CreateNewCourse: Creating a new course with name: " + courseName + " and is minigolf?: " + isMiniGolf + " with a filename of: " + filename);
        FileHandler.SaveToJSONFile<CourseData>(newCourse, filename);

        _mapMakerUIManager.AddCourseToAvailableCustomCourses(newCourse, true);
    }
    public void ClearAllTilesForNewHole()
    {
        foreach (Tilemap map in _tileMapReferenceHolder.AllMaps)
        {
            map.ClearAllTiles();
            _mapMakerBuilder.ClearAllObstacles();
        }
        _mapMakerBuilder.ClearAllMarkers();

    }
    public HoleData CreateNewHole(string courseName, bool isMiniGolf, int holePar, int holeIndex)
    {
        HoleData newHole = new HoleData();
        newHole.HoleIndex = holeIndex;
        newHole.HolePar = holePar;
        newHole.CourseName = courseName;
        newHole.IsMiniGolf = isMiniGolf;
        //ClearAllTilesForNewHole();
        newHole.HoleTileMapData = GetAllTileMapData();

        return newHole;
    }
    public List<TilemapData> GetAllTileMapData()
    {
        List<TilemapData> data = new List<TilemapData>();

        // foreach existing tilemap
        foreach (var mapObj in _mapMakerBuilder.GetTileMapNameToTileMapMapping())
        {
            TilemapData mapData = new TilemapData();
            mapData.key = mapObj.Key;

            BoundsInt boundsForThisMap = mapObj.Value.cellBounds;
            //mapObj.Value.CompressBounds();
            //BoundsInt compressed = mapObj.Value.cellBounds;
            //Debug.Log("GetAllTileMapData: " + mapObj.Key.ToString() + ": Bounds before compression: xMin: " + boundsForThisMap.xMin + " xMax: " + boundsForThisMap.xMax + "yMin: " + boundsForThisMap.yMin + " yMax: " + boundsForThisMap.yMax + " bounds after compression: xMin: " + compressed.xMin + " xMax: " + compressed.xMax + "yMin: " + compressed.yMin + " yMax: " + compressed.yMax);

            for (int x = boundsForThisMap.xMin; x < boundsForThisMap.xMax; x++)
            {
                for (int y = boundsForThisMap.yMin; y < boundsForThisMap.yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    TileBase tile = mapObj.Value.GetTile(pos);

                    //if (tile != null && _tileBaseToMapMakerObject.ContainsKey(tile))
                    //{
                    //    string guid = _tileBaseToMapMakerObject[tile].Guid;
                    //    TileInfo ti = new TileInfo(pos, guid);
                    //    mapData.tiles.Add(ti);
                    //}

                    MapMakerGroundTileBase mapMakerGroundTileBase = _mapMakerBuilder.GetMapMakerGroundTileBaseFromTileBase(tile);
                    if (tile != null && mapMakerGroundTileBase != null)
                    {
                        string guid = mapMakerGroundTileBase.Guid;
                        TileInfo ti = new TileInfo(pos, guid);
                        mapData.tiles.Add(ti);
                    }
                }
            }
            data.Add(mapData);
        }

        // save the tilemaps to a file
        return data;
    }
    public List<Vector2> GetBoundsOfAllTileMaps()
    {
        List<Vector2> newBounds = new List<Vector2>();

        Dictionary<string, Tilemap> tileMaps = _mapMakerBuilder.GetTileMapNameToTileMapMapping();

        int xMin = 10000;
        int xMax = -10000;
        int yMin = 10000;
        int yMax = -10000;

        Vector3Int emptySize = new Vector3Int(0, 0, 1);

        foreach (var mapObj in _mapMakerBuilder.GetTileMapNameToTileMapMapping())
        {
            mapObj.Value.CompressBounds();
            BoundsInt boundsForThisMap = mapObj.Value.cellBounds;
            Debug.Log("GetBoundsOfAllTileMaps: " + mapObj.Key.ToString() + ": tilemap size: " + boundsForThisMap.size.ToString() + " xMin: " + boundsForThisMap.xMin + " xMax: " + boundsForThisMap.xMax + "yMin: " + boundsForThisMap.yMin + " yMax: " + boundsForThisMap.yMax);

            if (boundsForThisMap.size == emptySize)
            {
                Debug.Log("GetBoundsOfAllTileMaps: tile map is empty?");
                continue;
            }

            if (boundsForThisMap.xMin < xMin)
                xMin = boundsForThisMap.xMin;
            if(boundsForThisMap.xMax > xMax)
                xMax = boundsForThisMap.xMax;
            if (boundsForThisMap.yMin < yMin)
                yMin = boundsForThisMap.yMin;
            if (boundsForThisMap.yMax > yMax)
                yMax = boundsForThisMap.yMax;
        }

        // add padding?
        xMin -= 10;
        xMax += 10;
        yMin -= 10;
        yMax += 10;

        newBounds.Add(new Vector2(xMin, yMax));
        newBounds.Add(new Vector2(xMin, yMin));        
        newBounds.Add(new Vector2(xMax, yMin));
        newBounds.Add(new Vector2(xMax, yMax));


        return newBounds;
    }
    public Vector2 GetCenterOfHoleBounds(Vector2[] points)
    {
        if (points.Length < 4)
            return Vector2.zero;

        // Convert the polygon points to a boundsint to get its "center" property
        BoundsInt holeBounds = new BoundsInt();
        for (int i = 0; i < points.Length; i++)
        {
            if (i == 0)
            {
                holeBounds.xMax = (int)points[i].x;
                holeBounds.xMin = (int)points[i].x;
                holeBounds.yMax = (int)points[i].y;
                holeBounds.yMin = (int)points[i].y;

                continue;
            }

            int newX = (int)points[i].x;
            int newY = (int)points[i].y;

            // x values
            if (holeBounds.xMax < newX)
                holeBounds.xMax = newX;
            if (holeBounds.xMin > newX)
                holeBounds.xMin = newX;
            // y values
            if (holeBounds.yMax < newY)
                holeBounds.yMax = newY;
            if (holeBounds.yMin > newY)
                holeBounds.yMin = newY;
        }        

        // return the center of the bounds
        return holeBounds.center;
    }
    public List<Vector3> OrderAimPointsByDistanceToTeeOff(Vector3 startPoint, List<Vector3Int> points)
    {
        List<Vector3> ordered = new List<Vector3>();

        SortedDictionary<float, Vector3> distanceByPoints = new SortedDictionary<float, Vector3> ();
        foreach (Vector3 point in points)
        {
            float dist = Vector2.Distance(startPoint, point);
            distanceByPoints.Add(dist, point);
        }
        foreach (KeyValuePair<float, Vector3> entry in distanceByPoints)
        {
            ordered.Add(entry.Value);
        }

        return ordered;
    }
    public void LoadTileMapData(List<TilemapData> data)
    {
        // clear obstacles and markers?
        _mapMakerBuilder.ClearAllObstacles();
        _mapMakerBuilder.ClearAllMarkers();
        foreach (var mapData in data)
        {
            //if (!_tileMaps.ContainsKey(mapData.key))
            if (!_mapMakerBuilder.DoesTileMapExistInMapping(mapData.key))
            {
                Debug.LogError("OnLoad: found saved data for tilemap called: '" + ",' but corresponding tilemap does not exist. Skipping...");
                continue;
            }

            // get tilemap
            //var map = _tileMaps[mapData.key];
            var map = _mapMakerBuilder.GetTileMapFromTileMapName(mapData.key);
            map.ClearAllTiles();
            //_mapMakerBuilder.ClearAllObstacles();

            if (mapData.tiles != null && mapData.tiles.Count > 0)
            {
                //old way before asset database
                foreach (TileInfo tile in mapData.tiles)
                {
                    //if (!_guidToTileBase.ContainsKey(tile.GuidForTile))
                    if (!_mapMakerBuilder.DoesGUIDExistForTileBaseInMapping(tile.GuidForTile))
                    {
                        Debug.LogError("OnLoad: Could not find GUID: '" + tile.GuidForTile + " in _guidToTileBase dictionary. Skipping...");
                        continue;
                    }

                    //TileBase tileBase = _guidToTileBase[tile.GuidForTile];
                    TileBase tileBase = _mapMakerBuilder.GetTileBaseFromGUID(tile.GuidForTile);
                    //map.SetTile(tile.position, _guidToTileBase[tile.GuidForTile]);
                    map.SetTile(tile.position, tileBase);

                    // Check if the tile is being placed on the object map. If so, spawn the appropriate object
                    if (map.name == "Object")
                    {
                        if (_mapMakerBuilder.DoesMapMakerGroundTileBaseExistForTileBaseInMapping(tileBase))
                        {
                            MapMakerGroundTileBase groundTileBase = _mapMakerBuilder.GetMapMakerGroundTileBaseFromTileBase(tileBase);
                            if (groundTileBase.GetType() == typeof(MapMakerObstacle))
                            {
                                _mapMakerBuilder.PlaceObstacle(tile.position, (MapMakerObstacle)groundTileBase);
                            }
                        }

                    }
                    else if (map.name == "CourseMarkers")
                    {
                        if (_mapMakerBuilder.DoesMapMakerGroundTileBaseExistForTileBaseInMapping(tileBase))
                        {
                            MapMakerGroundTileBase groundTileBase = _mapMakerBuilder.GetMapMakerGroundTileBaseFromTileBase(tileBase);
                            if (groundTileBase.GetType() == typeof(MapMakerCourseMarker))
                            {
                                _mapMakerBuilder.PlaceCourseMarker(tile.position, (MapMakerCourseMarker)groundTileBase);
                            }
                        }
                    }
                }

            }
        }
    }
    public void SaveCourse(CourseData courseToSave, string filename)
    {
        if (courseToSave == null)
            return;
        FileHandler.SaveToJSONFile<CourseData>(courseToSave, filename);
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
[Serializable]
public class CourseData
{
    public string CourseId;
    public string CourseName;
    public string RelativeFilePath;
    //public List<string> HolesInCourseFileNames; // change this to just be a list of HoleData? CourseData has list of HoleData, which has list of TilemapData, which has list of TileInfo?
    public List<HoleData> HolesInCourse = new List<HoleData>();
    public bool IsMiniGolf;
}
[Serializable]
public class HoleData
{
    public int HoleIndex;
    public int HolePar;
    public string CourseName;
    public List<TilemapData> HoleTileMapData = new List<TilemapData>();
    public bool IsMiniGolf;
    public Vector2[] PolygonPoints;
    public Vector3 ZoomOutPosition;
    public Vector3 TeeOffLocation;
    public Vector3[] CourseAimPoints;
    public Vector3 HolePosition;
    public List<Vector3> TeeOffMarkerPositions = new List<Vector3>();
    public bool IsHoleCompleted = false;
}

