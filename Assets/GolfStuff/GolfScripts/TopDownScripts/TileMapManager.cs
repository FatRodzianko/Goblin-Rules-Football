
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
using PathCreation;

public class TileMapManager : MonoBehaviour
{
    [SerializeField] private Tilemap _greenMap, _fairwayMap, _roughMap, _deepRoughMap, _sandTrapMap, _waterTrapMap, _edgesMap, _directionTilesMap, _miniGolfWalls;
    [SerializeField] private List<Tilemap> _allMaps = new List<Tilemap>();
    [SerializeField] private Tile _nullTile;
    [SerializeField] private int _holeIndex;
    [SerializeField] private string _courseName;
    [SerializeField] private int _holePar;
    [SerializeField] private float _cameraZoomValue;
    [SerializeField] private string _clubToUse;
    [SerializeField] private bool _isTeeOffChallenge;
    [Header("MiniGolf Stuff")]
    public bool IsMiniGolf = false;

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
        newHole.MiniGolfWallTiles = GetTilesFromMap(_miniGolfWalls).ToList();

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
        //newHole.TeeOffAimPoint = aimPoint.transform.position;
        FindHoleAimPoints(newHole);

        if (!string.IsNullOrEmpty(_clubToUse))
            newHole.ClubToUse = _clubToUse;
        newHole.IsTeeOffChallenge = this._isTeeOffChallenge;
        newHole.IsMiniGolf = this.IsMiniGolf;

        // Save the statues for the hole
        newHole.Statues = SaveAllStatues(GameObject.FindGameObjectsWithTag("Statue")).ToList();
        newHole.BalloonPowerUps = SaveAllBalloonPowerUps(GameObject.FindGameObjectsWithTag("BalloonPowerUp")).ToList();
        newHole.Tubes = SaveAllTubes(GameObject.FindGameObjectsWithTag("TubeGolf")).ToList();

        if (this.IsMiniGolf)
        {
            newHole.MiniGolfPipes = SaveAllMiniGolfPipes(GameObject.FindGameObjectsWithTag("MiniGolfPipe")).ToList();
        }

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
        IEnumerable<SavedTube> SaveAllTubes(GameObject[] tubesToSave)
        {
            if (tubesToSave.Length != 2)
            {
                Debug.Log("SaveAllTubes: There needs to be 2 tubes to save. " + tubesToSave.Length.ToString());
                yield break;
            }
            bool primarySet = false;
            for (int i = 0; i < tubesToSave.Length; i++)
            {
                if (tubesToSave[i].GetComponent<TubeScript>().IsPrimaryTube)
                {
                    primarySet = true;
                    break;
                }
            }
            if (!primarySet)
            {
                Debug.Log("SaveAllTubes: No primary tube set. Cancling the saving of tubes...");
                yield break;
            }
            for (int i = 0; i < tubesToSave.Length; i++)
            {
                if (tubesToSave[i].GetComponent<TubeScript>() == null)
                    continue;

                yield return new SavedTube()
                {
                    TubePosition = tubesToSave[i].transform.position,
                    IsPrimaryTube = tubesToSave[i].GetComponent<TubeScript>().IsPrimaryTube,
                    TubeScriptableObstacle = tubesToSave[i].GetComponent<EnvironmentObstacleTopDown>().myScriptableObject
                };
            }
        }
        IEnumerable<SavedMiniGolfPipe> SaveAllMiniGolfPipes(GameObject[] pipesToSave)
        {
            Debug.Log("SaveAllMiniGolfPipes: pipesToSave: " + pipesToSave.Length.ToString());
            if (pipesToSave.Length < 2)
            {
                Debug.Log("pipesToSave: There needs to be at least 2 pipes to save. " + pipesToSave.Length.ToString());
                yield break;
            }

            for (int i = 0; i < pipesToSave.Length; i++)
            {
                if (pipesToSave[i].GetComponent<PipeMiniGolfScript>() == null)
                    continue;

                PipeMiniGolfScript pipeScript = pipesToSave[i].GetComponent<PipeMiniGolfScript>();
                if (!pipeScript.IsEntryPipe)
                {
                    Debug.Log("pipesToSave: Is this an entry pipe?: " + pipeScript.IsEntryPipe.ToString() + " Skipping...");
                    continue;
                }


                if (!pipeScript.MyExitPipe)
                {
                    Debug.Log("pipesToSave: MyExitPipe does not exist. Skipping...");
                    continue;
                }


                yield return new SavedMiniGolfPipe()
                {
                    EntryPipePosition = pipeScript.gameObject.transform.position,
                    ExitPipePosition = pipeScript.MyExitPipe.transform.position,
                    MiniGolfPipeScriptableObstacle = pipeScript.myScriptableObject,
                    MiniGolfExitPipeScriptableObstacle = pipeScript.MyExitPipe.myScriptableObject,
                    //PathPoints = pipeScript.MyPath.path.localPoints
                    PathPoints = GetPipePathPoints(pipeScript)
                };
            }
        }
    }
    Vector3[] GetPipePathPoints(PipeMiniGolfScript pipeScript)
    {
        List<Vector3> pipePathPoints = new List<Vector3>();

        //pipePathPoints.Add(pipeScript.gameObject.transform.position);
        if (pipeScript.WayPointHolder.childCount > 0)
        {
            for (int i = 0; i < pipeScript.WayPointHolder.childCount; i++)
            {
                pipePathPoints.Add(pipeScript.WayPointHolder.GetChild(i).position);
            }
        }
        pipePathPoints.Add(pipeScript.MyExitPipe.transform.position);
        return pipePathPoints.ToArray();
    }
    void FindHoleAimPoints(ScriptableHole hole)
    {
        GameObject[] aimPoints = GameObject.FindGameObjectsWithTag("GolfAimPoint");
        if (aimPoints.Length <= 0)
        {
            Debug.LogError("FindHoleAimPoints: no golf aim points found. Cannot continue?");
            return;
        }

        // Add all the aim point scripts to a list
        List<GolfAimPointScript> aimPointScripts = new List<GolfAimPointScript>();
        for (int i = 0; i < aimPoints.Length; i++)
        {
            aimPointScripts.Add(aimPoints[i].GetComponent<GolfAimPointScript>());
        }
        aimPointScripts.Sort((x, y) => x.AimPointIndex.CompareTo(y.AimPointIndex));
        List<Vector3> aimPointPositions = new List<Vector3>();
        foreach (GolfAimPointScript script in aimPointScripts)
        {
            Debug.Log("FindHoleAimPoints: Golf aim point index: " + script.AimPointIndex.ToString() + ":" + script.transform.position);
            aimPointPositions.Add(script.transform.position);
        }
        hole.CourseAimPoints = aimPointPositions.ToArray();
        hole.TeeOffAimPoint = aimPointPositions[0];
        ////sort list by the saved "aim point index" value
        //GolfAimPointScript[] aimPointArray = aimPointScripts.OrderByDescending(x => x.AimPointIndex).ToArray();
        //hole.TeeOffAimPoint = aimPointArray[0].transform.position;
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
        _miniGolfWalls.ClearAllTiles();

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
        try
        {
            GameObject[] tubes = GameObject.FindGameObjectsWithTag("TubeGolf");
            DeleteObjects(tubes);
        }
        catch (Exception e)
        {
            Debug.Log("ClearMap: Could not find/delete tube objects. Error: " + e);
        }
        try
        {
            GameObject[] aimPoints = GameObject.FindGameObjectsWithTag("GolfAimPoint");
            DeleteObjects(aimPoints);
        }
        catch (Exception e)
        {
            Debug.Log("ClearMap: Could not find/delete aim point objects. Error: " + e);
        }
        try
        {
            GameObject[] miniGolfPipes = GameObject.FindGameObjectsWithTag("MiniGolfPipe");
            DeleteObjects(miniGolfPipes);
        }
        catch (Exception e)
        {
            Debug.Log("ClearMap: Could not find/delete minigolf pipe objects. Error: " + e);
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
        if (hole.CourseAimPoints.Length > 0)
        {
            for (int i = 0; i < hole.CourseAimPoints.Length; i++)
            {
                Debug.Log("LoadMapFromEditor: Spawning golf aim point: " + i.ToString() + ":" + hole.CourseAimPoints[i].ToString());
                //SetPositionOfMapMarker(hole.CourseAimPoints[i], "GolfAimPoint", _golfAimPoint);
                GameObject newAimPoint = Instantiate(_golfAimPoint, hole.CourseAimPoints[i], Quaternion.identity);
                newAimPoint.GetComponent<GolfAimPointScript>().AimPointIndex = i;
            }
        }
        else
        {
            SetPositionOfMapMarker(hole.TeeOffAimPoint, "GolfAimPoint", _golfAimPoint);
        }
        
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
        if (!string.IsNullOrEmpty(hole.ClubToUse))
            this._clubToUse = hole.ClubToUse;
        else
            this._clubToUse = "";

        this._isTeeOffChallenge = hole.IsTeeOffChallenge;
        this.IsMiniGolf = hole.IsMiniGolf;

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
        SetTileOnTileMap(_miniGolfWalls, hole.MiniGolfWallTiles);

        // Make sure that the DirectionTileManager saves all its sloped tile positions. This will only be called if not in the editor but in game??? Don't know if the below works or not...
        if (Application.isPlaying)
        {
            Debug.Log("LoadMap: Application.isPlaying is true. Calling GetTilesWithSlopes()...");
            DirectionTileManager.instance.GetTilesWithSlopes();
        }


        // Spawn the holes
        foreach (Vector3 pos in hole.HolePositions)
        {
            
            GameObject newHole = Instantiate(_holePrefab, pos, Quaternion.identity);
            // LATER save these positions to the GameManager for the hole?
            try
            {
                GameplayManagerTopDownGolf.instance.HoleHoleObjects.Add(newHole);
            }
            catch (Exception e)
            {
                Debug.Log("LoadMap: could not add hole objects to gameplaymanager. Error: " + e);
            }
            
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
        // Spawn the tube objects and make sure to set their companion tube values and the IsPrimary value
        SpawnTubeObjects(hole.Tubes);
        if (hole.IsMiniGolf)
        {
            try
            {
                SpawnMiniGolfPipes(hole.MiniGolfPipes);
            }
            catch (Exception e)
            {
                Debug.Log("LoadMap: Error spawning minigolf pipes. Error: " + e);
            }
            
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
    void SpawnTubeObjects(List<SavedTube> tubes)
    {
        if (tubes.Count != 2)
            return;

        TubeScript tube1 = null;
        TubeScript tube2 = null;
        for (int i = 0; i < tubes.Count; i++)
        {
            GameObject newTube = Instantiate(tubes[i].TubeScriptableObstacle.ObstaclePrefab, tubes[i].TubePosition, Quaternion.identity);
            TubeScript newTubeScript = newTube.GetComponent<TubeScript>();
            newTubeScript.IsPrimaryTube = tubes[i].IsPrimaryTube;

            if (i == 0)
            {
                tube1 = newTubeScript;
            }
            else
            { 
                tube2 = newTubeScript;
            }
        }

        tube1.CompanionTube = tube2;
        tube2.CompanionTube = tube1;
    }
    void SpawnMiniGolfPipes(List<SavedMiniGolfPipe> miniGolfPipes)
    {
        if (miniGolfPipes.Count < 1)
            return;
        for (int i = 0; i < miniGolfPipes.Count; i++)
        {
            GameObject entryHole = Instantiate(miniGolfPipes[i].MiniGolfPipeScriptableObstacle.ObstaclePrefab, miniGolfPipes[i].EntryPipePosition, Quaternion.identity);
            GameObject exitPipe = Instantiate(miniGolfPipes[i].MiniGolfExitPipeScriptableObstacle.ObstaclePrefab, miniGolfPipes[i].ExitPipePosition, Quaternion.identity);

            PipeMiniGolfScript entryPipeScript = entryHole.GetComponent<PipeMiniGolfScript>();
            entryPipeScript.SetExitPipe(exitPipe.GetComponent<PipeMiniGolfScript>());
            entryPipeScript.SetPipePathWayPoints(miniGolfPipes[i].PathPoints);


            //Vector3[] newPathPoints = miniGolfPipes[i].PathPoints;
            //newPathPoints[0] = Vector3.zero;
            //newPathPoints[newPathPoints.Length - 1] = entryHole.transform.InverseTransformPoint(entryPipeScript.ExitPipeExitPoint);

            //entryPipeScript.MyPath.bezierPath = new BezierPath(miniGolfPipes[i].PathPoints, false, PathSpace.xy);
            //entryPipeScript.MyPath.bezierPath = new BezierPath(miniGolfPipes[i].PathPoints, false, PathSpace.xy);
            //entryPipeScript.MyPath.bezierPath.AutoControlLength = 0f;
        }
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
        Debug.Log("SetTileOnTileMap: map type: " + map.name + " and tiles to set: " + savedTiles.Count.ToString());
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

