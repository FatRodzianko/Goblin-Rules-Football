
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using Cinemachine;
using System.Threading.Tasks;
using System.IO;

public class TileMapManager : MonoBehaviour
{
    [SerializeField] private Tilemap _greenMap, _fairwayMap, _roughMap, _deepRoughMap, _sandTrapMap, _waterTrapMap, _edgesMap, _directionTilesMap;
    [SerializeField] private List<Tilemap> _allMaps = new List<Tilemap>();
    [SerializeField] private Tile _nullTile;
    [SerializeField] private int _holeIndex;
    [SerializeField] private string _courseName;
    [SerializeField] private int _holePar;
    [SerializeField] private float _cameraZoomValue;

    [Header("Prefabs for Objects like holes/tee ball things")]
    [SerializeField] GameObject _holePrefab;
    [SerializeField] GameObject _teeMarkerPrefab;
    [SerializeField] GameObject _cameraBoundingBox;
    [SerializeField] GameObject _environmentObstacleHolderPrefab;
    [SerializeField] GameObject _environmentObstacleHolderObject;
    [SerializeField] GameObject _golfAimPoint;
    [SerializeField] GameObject _zoomedOutPosition;
    [SerializeField] GameObject _teeOffPosition;


    [Header("Camera stuff?")]
    [SerializeField] CinemachineVirtualCamera _myCamera;

    private void Start()
    {
        if(!_myCamera)
            _myCamera = GameObject.FindGameObjectWithTag("camera").GetComponent<CinemachineVirtualCamera>();
        if (!_environmentObstacleHolderObject)
            _environmentObstacleHolderObject = GameObject.FindGameObjectWithTag("EnvironmentObstacleHolder");
    }

    public void SaveMap()
    {
        // Create a new scriptable object. Set info inputed into the Editor such as hole number, course name, and name of the object that will be created
        var newHole = ScriptableObject.CreateInstance<ScriptableHole>();
        newHole.HoleIndex = _holeIndex;
        newHole.CourseName = _courseName;
        newHole.name = _courseName + "_" + _holeIndex;
        newHole.HolePar = _holePar;

        // Save the tilemaps to the scriptable object
        newHole.GreenTiles = GetTilesFromMap(_greenMap).ToList();
        newHole.FairwayTiles = GetTilesFromMap(_fairwayMap).ToList();
        newHole.RoughTiles = GetTilesFromMap(_roughMap).ToList();
        newHole.DeepRoughTiles = GetTilesFromMap(_deepRoughMap).ToList();
        newHole.SandTrapTiles = GetTilesFromMap(_sandTrapMap).ToList();
        newHole.WaterTrapTiles = GetTilesFromMap(_waterTrapMap).ToList();
        newHole.EdgesTiles = GetTilesFromMap(_edgesMap).ToList();
        newHole.DirectionTiles = GetTilesFromMap(_directionTilesMap).ToList();

        // Find all the holes and save their locations
        GameObject[] holes = GameObject.FindGameObjectsWithTag("golfHole");
        if (holes.Length <= 0)
        {
            Debug.LogError("TileMapManager: SaveMap: No holes were found.");
            return;
        }
        newHole.HolePositions = GetObjectPositions(holes);
        // Find the tee off location for the hole
        newHole.TeeOffLocation = GameObject.FindGameObjectWithTag("teeOffPosition").transform.position;
        // Find the position of the tee marker objects
        GameObject[] teeMarkers = GameObject.FindGameObjectsWithTag("GolfTeeMarker");
        if (teeMarkers.Length > 0)
            newHole.TeeMarkerPositions = GetObjectPositions(teeMarkers);

        // Find the zoomed out position and save it for later use by the camera
        newHole.ZoomedOutPos = GameObject.FindGameObjectWithTag("ZoomedOutPosition").transform.position;
        if (this._cameraZoomValue != 0f)
            newHole.CameraZoomValue = this._cameraZoomValue;
        else
            newHole.CameraZoomValue = 9f;

        // Find all Environment obstacles and save their locations
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("GolfEnvironmentObstacle");
        newHole.SavedObstacles = SaveAllObstacles(obstacles).ToList();

        // Find the camera bounding box and save its object and position
        //BoxCollider2D boundingBox = GameObject.FindGameObjectWithTag("CameraBoundingBox").GetComponent<BoxCollider2D>();
        GameObject boundingBox = GameObject.FindGameObjectWithTag("CameraBoundingBox");
        PolygonCollider2D boundingBoxPolygonCollider = boundingBox.gameObject.GetComponent<PolygonCollider2D>();
        Vector2[] polygonPoints = boundingBoxPolygonCollider.points;
        newHole.PolygonPoints = polygonPoints;
        //newHole.CameraBoundingBoxObject = boundingBox;
        //newHole.CameraBoundingBoxOffset = boundingBox.offset;
        //newHole.CameraBoundingBoxSize = boundingBox.size;
        newHole.CameraBoundingBoxPos = boundingBox.transform.position;

        // Find the Tee Off Aim Point and save it
        GameObject aimPoint = GameObject.FindGameObjectWithTag("GolfAimPoint");
        newHole.TeeOffAimPoint = aimPoint.transform.position;

        // Save the statues for the hole
        newHole.Statues = SaveAllStatues(GameObject.FindGameObjectsWithTag("Statue")).ToList();
        newHole.BalloonPowerUps = SaveAllBalloonPowerUps(GameObject.FindGameObjectsWithTag("BalloonPowerUp")).ToList();

        // Save the scriptable object to a file
#if UNITY_EDITOR
        ScriptableObjectUtility.SaveHoleToFile(newHole);
        //try
        //{
        //    string json = JsonUtility.ToJson(newHole);
        //    string path = $"Assets/Resources/Holes/{newHole.name}.json";
        //    StreamWriter writer = new StreamWriter(path, true);
        //    writer.Write(json);
        //    writer.Close();
        //}
        //catch (Exception e)
        //{
        //    Debug.Log("SaveMap: could not save scriptable object as json. Error: " + e);
        //}
#endif

        // Local function to iterate through a tilemap, check if a tile exists at a location, and then save that tile's info + location
        IEnumerable<SavedTile> GetTilesFromMap(Tilemap map)
        {
            foreach (var pos in map.cellBounds.allPositionsWithin)
            {
                if (map.HasTile(pos))
                {
                    var levelTile = map.GetTile<LevelTile>(pos);
                    yield return new SavedTile()
                    {
                        TilePos = pos,
                        MyTile = map.GetTile<Tile>(pos)
                    };
                }
            }
        }
        // Local function to iterate through a tilemap, check if a tile exists at a location, and then save that tile's info + location
        IEnumerable<SavedObstacle> SaveAllObstacles(GameObject[] obstaclesToSave)
        {
            if (obstaclesToSave.Length > 0)
            {
                for (int i = 0; i < obstaclesToSave.Length; i++)
                {
                    if (obstaclesToSave[i].GetComponent<EnvironmentObstacleTopDown>() == null)
                        continue;

                    yield return new SavedObstacle()
                    {
                        ObstaclePos = obstaclesToSave[i].transform.position,
                        ObstacleScriptableObject = obstaclesToSave[i].GetComponent<EnvironmentObstacleTopDown>().myScriptableObject
                    };
                }
            }
        }
        // Local Function to iterate through all statue objects and save their position and statue type
        IEnumerable<SavedStatue> SaveAllStatues(GameObject[] statuesToSave)
        {
            if (statuesToSave.Length <= 0)
            {
                Debug.Log("SaveAllStatues: No statues to save. yield break?");
                yield break;
            }

            for (int i = 0; i < statuesToSave.Length; i++)
            {
                if (statuesToSave[i].GetComponent<Statue>() == null)
                    continue;

                yield return new SavedStatue()
                {
                    StatuePosition = statuesToSave[i].transform.position,
                    StatueType = statuesToSave[i].GetComponent<Statue>().StatueType,
                    StatueScriptableObstacle = statuesToSave[i].GetComponent<EnvironmentObstacleTopDown>().myScriptableObject
                };
            }
        }
        IEnumerable<SavedBalloonPowerUp> SaveAllBalloonPowerUps(GameObject[] balloonPowerUpsToSave)
        {
            if (balloonPowerUpsToSave.Length <= 0)
            {
                Debug.Log("SaveAllBalloonPowerUps: Nothing to save. yield break?");
                yield break;
            }

            for (int i = 0; i < balloonPowerUpsToSave.Length; i++)
            {
                if (balloonPowerUpsToSave[i].GetComponent<BalloonPowerUp>() == null)
                    continue;

                yield return new SavedBalloonPowerUp()
                {
                    BalloonPosition = balloonPowerUpsToSave[i].transform.position,
                    BalloonHeight = balloonPowerUpsToSave[i].GetComponent<BalloonPowerUp>().SavedHeightOfBalloon,
                    //BalloonScriptableObstacle = balloonPowerUpsToSave[i].GetComponent<EnvironmentObstacleTopDown>().myScriptableObject
                    //BalloonScriptableObstacle = balloonPowerUpsToSave[i].GetComponent<EnvironmentObstacleTopDown>().myScriptableObject
                    BalloonScriptableObstacle = balloonPowerUpsToSave[i].GetComponent<BalloonPowerUp>().myScriptableObject
                };
            }
        }
    }
    public void ClearMapFromEditor()
    {
        var hole = Resources.Load<ScriptableHole>($"Holes/{_courseName}_{_holeIndex}");
        if (hole == null)
        {
            Debug.LogError($"{_courseName}_{_holeIndex} does not exist.");
            return;
        }
        this.ClearMap();
    }
    public void ClearMap()
    {
        Debug.Log("ClearMap: start time: " + Time.time.ToString());
        // Clear all tilemaps
        /*var maps = FindObjectsOfType<Tilemap>();
        foreach (var tilemap in maps)
        {
            tilemap.ClearAllTiles();
        }*/

        /*NullTileOnTileMap(_greenMap, hole.GreenTiles);
        NullTileOnTileMap(_fairwayMap, hole.FairwayTiles);
        NullTileOnTileMap(_roughMap, hole.RoughTiles);
        NullTileOnTileMap(_deepRoughMap, hole.DeepRoughTiles);
        NullTileOnTileMap(_sandTrapMap, hole.SandTrapTiles);
        NullTileOnTileMap(_waterTrapMap, hole.WaterTrapTiles);
        NullTileOnTileMap(_edgesMap, hole.EdgesTiles);
        NullTileOnTileMap(_directionTilesMap, hole.DirectionTiles);*/
        _greenMap.ClearAllTiles();
        _fairwayMap.ClearAllTiles();
        _roughMap.ClearAllTiles();
        _deepRoughMap.ClearAllTiles();
        _sandTrapMap.ClearAllTiles();
        _waterTrapMap.ClearAllTiles();
        _edgesMap.ClearAllTiles();
        _directionTilesMap.ClearAllTiles();

        // Delete all hole objects
        try
        {
            GameObject[] holes = GameObject.FindGameObjectsWithTag("golfHole");
            DeleteObjects(holes);
        }
        catch (Exception e)
        {
            Debug.Log("ClearMap: Could not find/delete objects. Error: " + e);
        }
        // Delete all obstacle objects
        try
        {
            //GameObject[] obstacles = GameObject.FindGameObjectsWithTag("GolfEnvironmentObstacle");
            GameObject obstacleHolder = GameObject.FindGameObjectWithTag("EnvironmentObstacleHolder");
            GameObject[] obstacleChildren = new GameObject[obstacleHolder.transform.childCount];
            for (int i = 0; i < obstacleHolder.transform.childCount; i++)
            {
                obstacleChildren[i] = obstacleHolder.transform.GetChild(i).gameObject;
            }
            DeleteObjects(obstacleChildren);
        }
        catch (Exception e)
        {
            Debug.Log("ClearMap: Could not find/delete objects. Error: " + e);
        }
        // Delete all tee marker objects
        try
        {
            GameObject[] teeMarkers = GameObject.FindGameObjectsWithTag("GolfTeeMarker");
            DeleteObjects(teeMarkers);
        }
        catch (Exception e)
        {
            Debug.Log("ClearMap: Could not find/delete objects. Error: " + e);
        }
        // Delete the camera bounding box
        try
        {
            GameObject[] boundingBox = GameObject.FindGameObjectsWithTag("CameraBoundingBox");
            DeleteObjects(boundingBox);
        }
        catch (Exception e)
        {
            Debug.Log("ClearMap: Could not find/delete objects. Error: " + e);
        }
        // Delete Statue objects, but only from the editor. In game, the server will destroy the statue objects as they are all networked objects, and need to be networked objects to sync the animations and do other networkbehavior stuff
#if UNITY_EDITOR
        try
        {
            GameObject[] statues = GameObject.FindGameObjectsWithTag("Statue");
            DeleteObjects(statues);
            GameObject[] balloonPowerUps = GameObject.FindGameObjectsWithTag("BalloonPowerUp");
            DeleteObjects(balloonPowerUps);
        }
        catch (Exception e)
        {
            Debug.Log("ClearMap: Could not find/delete objects. Error: " + e);
        }
#endif

    }
    public void LoadMapFromEditor()
    {
        var hole = Resources.Load<ScriptableHole>($"Holes/{_courseName}_{_holeIndex}");
        if (hole == null)
        {
            Debug.LogError($"{_courseName}_{_holeIndex} does not exist.");
            
            return;
        }
        // Set the par value in the editor to the saved value from the hole
        this._holePar = hole.HolePar;
        // See if map markers such as tee off position or zoom out position exist in the scene. Then set their position to the saved position
        SetPositionOfMapMarker(hole.TeeOffLocation, "teeOffPosition", _teeOffPosition);
        SetPositionOfMapMarker(hole.ZoomedOutPos, "ZoomedOutPosition", _zoomedOutPosition);
        SetPositionOfMapMarker(hole.TeeOffAimPoint, "GolfAimPoint", _golfAimPoint);
        // Set the camera zoom value in the editor to the saved value from the hole
        this._cameraZoomValue = hole.CameraZoomValue;

        // If there are statues on the map, spawn them. Only use this script to spawn statues in the editor. During gameplay, the server spawns statues because they are networked objects
        if (hole.Statues.Count > 0)
        {
            SpawnStatuesForEditor(hole.Statues);
        }
        if (hole.BalloonPowerUps.Count > 0)
        {
            SpawnBalloonPowerUpsForEditor(hole.BalloonPowerUps);
        }

        this.LoadMap(hole);
    }
    void SpawnStatuesForEditor(List<SavedStatue> statuesToSpawn)
    {
        foreach (SavedStatue statue in statuesToSpawn)
        {
            Instantiate(statue.StatueScriptableObstacle.ObstaclePrefab, statue.StatuePosition, Quaternion.identity);
        }
    }
    void SpawnBalloonPowerUpsForEditor(List<SavedBalloonPowerUp> balloonPowerUpsToSpawn)
    {
        foreach (SavedBalloonPowerUp balloon in balloonPowerUpsToSpawn)
        {
            GameObject balloonObject = Instantiate(balloon.BalloonScriptableObstacle.ObstaclePrefab, balloon.BalloonPosition, Quaternion.identity);
            balloonObject.GetComponent<BalloonPowerUp>().SavedHeightOfBalloon = balloon.BalloonHeight;
        }
    }
    public void LoadMap(ScriptableHole hole)
    {
        Debug.Log("TileMapManager: LoadMap: Loading hole: " + hole.name);
        Debug.Log("LoadMap: start time: " + Time.time.ToString());
        // Set the tiles
        SetTileOnTileMap(_greenMap, hole.GreenTiles);
        SetTileOnTileMap(_fairwayMap, hole.FairwayTiles);
        SetTileOnTileMap(_roughMap, hole.RoughTiles);
        SetTileOnTileMap(_deepRoughMap, hole.DeepRoughTiles);
        SetTileOnTileMap(_sandTrapMap, hole.SandTrapTiles);
        SetTileOnTileMap(_waterTrapMap, hole.WaterTrapTiles);
        SetTileOnTileMap(_edgesMap, hole.EdgesTiles);
        SetTileOnTileMap(_directionTilesMap, hole.DirectionTiles);

        // Make sure that the DirectionTileManager saves all its sloped tile positions. This will only be called if not in the editor but in game??? Don't know if the below works or not...
        if (Application.isPlaying)
            DirectionTileManager.instance.GetTilesWithSlopes();


        // Spawn the holes
        foreach (Vector3 pos in hole.HolePositions)
        {
            Instantiate(_holePrefab, pos, Quaternion.identity);
            // LATER save these positions to the GameManager for the hole?
        }
        // Spawn the tee markers
        foreach (Vector3 pos in hole.TeeMarkerPositions)
        {
            Instantiate(_teeMarkerPrefab, pos, Quaternion.identity);
        }
        // Check to see if the EnvironmentObstacleHolder exists in the scene. If not, Instantiate it
        if (!DoesObjectWithTagExist("EnvironmentObstacleHolder"))
        {
            _environmentObstacleHolderObject = Instantiate(_environmentObstacleHolderPrefab);
        }
        // If the object exists but the _environmentObstacleHolderObject is empty/null, find the object by its tag and set _environmentObstacleHolderObject to it
        else if (!_environmentObstacleHolderObject)
        {
            _environmentObstacleHolderObject = GameObject.FindGameObjectWithTag("EnvironmentObstacleHolder");
        }
        // Spawn the obstacles and make them a child object of the EnvironmentObstacleHolder
        foreach (SavedObstacle savedObstacle in hole.SavedObstacles)
        {
            Instantiate(savedObstacle.ObstacleScriptableObject.ObstaclePrefab, savedObstacle.ObstaclePos, Quaternion.identity, _environmentObstacleHolderObject.transform);
        }
        // Spawn the camera bounding box and add it to the camera
        GameObject boundingBox = Instantiate(_cameraBoundingBox, hole.CameraBoundingBoxPos, Quaternion.identity);
        PolygonCollider2D polygonCollider = boundingBox.GetComponent<PolygonCollider2D>();
        // Check if TileMapManager has a reference to the camera or not
        if (!_myCamera)
            _myCamera = GameObject.FindGameObjectWithTag("camera").GetComponent<CinemachineVirtualCamera>();
        // Set the points of the polygon collider to what was saved
        polygonCollider.points = hole.PolygonPoints;
        // Add the polygon collider to the camera
        _myCamera.GetComponent<CinemachineConfiner2D>().m_BoundingShape2D = polygonCollider;
        // invalidate the cache of the confiner2d so that it updates for the new confining collider
        _myCamera.GetComponent<CinemachineConfiner2D>().InvalidateCache();

        // LATER save the tee off position to the GameManager
        //GameplayManagerTopDownGolf.instance.UpdateTeeOffPositionForNewHole(hole.TeeOffLocation);
        // LATER save the hole PAR value to the GameManager
        //GameplayManagerTopDownGolf.instance.UpdateParForNewHole(hole.HolePar);
        Debug.Log("LoadMap: end time: " + Time.time.ToString());

    }
    public async Task LoadMapAsTask(ScriptableHole hole)
    {
        ClearMap();
        LoadMap(hole);
        await Task.Yield();
    }
    public List<Vector3> GetObjectPositions(GameObject[] objects)
    {
        if (objects.Length > 0)
        {
            List<Vector3> objectPositions = new List<Vector3>();
            for (int i = 0; i < objects.Length; i++)
            {
                objectPositions.Add(objects[i].transform.position);
            }
            return objectPositions;
        }
        else return new List<Vector3>() { Vector3.zero };
    }
    public void SetTileOnTileMap(Tilemap map, List<SavedTile> savedTiles)
    {
        if (savedTiles.Count <= 0)
            return;
        foreach (SavedTile savedTile in savedTiles)
        {
            map.SetTile(savedTile.TilePos, savedTile.MyTile);
        }
    }
    public void NullTileOnTileMap(Tilemap map, List<SavedTile> savedTiles)
    {
        Debug.Log("NullTileOnTileMap: " + Time.time);
        if (savedTiles.Count <= 0)
            return;
        /*foreach (SavedTile savedTile in savedTiles)
        {
            map.SetTile(savedTile.TilePos, null);
        }*/
        for (int i = 0; i < savedTiles.Count; i++)
        {
            map.SetTile(savedTiles[i].TilePos, null);
        }
        
    }
    public void DeleteObjects(GameObject[] objects)
    {
        if (objects.Length <= 0)
            return;
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject toDelete = objects[i];
#if UNITY_EDITOR
            DestroyImmediate(toDelete);
#else
            toDelete.SetActive(false);
            Destroy(toDelete);
#endif

        }
    }
    bool DoesObjectWithTagExist(string tagName)
    {
        bool doesObjectExist = false;
        try
        {
            GameObject objectToFind = GameObject.FindGameObjectWithTag(tagName);
            if (objectToFind)
                doesObjectExist = true;
            else
                doesObjectExist = false;
        }
        catch (Exception e)
        {
            Debug.Log("DoesObjectWithTagExist: Could not find object with tag: " + tagName + ". Error: " + e);
            doesObjectExist = false;
        }
        return doesObjectExist;
    }
    void SetPositionOfMapMarker(Vector3 position, string tagName, GameObject prefabToSpawn)
    {
        if (DoesObjectWithTagExist(tagName))
        {
            GameObject.FindGameObjectWithTag(tagName).transform.position = position;
        }
        else
        {
            Instantiate(prefabToSpawn, position, Quaternion.identity);
        }
    }
}


#if UNITY_EDITOR
public static class ScriptableObjectUtility
{
    public static void SaveHoleToFile(ScriptableHole hole)
    {
        AssetDatabase.CreateAsset(hole,$"Assets/Resources/Holes/{hole.name}.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

#endif

