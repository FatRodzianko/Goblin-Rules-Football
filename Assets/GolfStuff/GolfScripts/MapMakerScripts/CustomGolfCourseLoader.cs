using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.IO;

public class CustomGolfCourseLoader : SingletonInstance<CustomGolfCourseLoader>
{
    [SerializeField] SteamWorkshopCourseDownloader _steamWorkshopCourseDownloader;
    [Header("Default/Builtin Courses")]
    [SerializeField] AvailableCourses _builtinCourses;

    [Header("Custom Courses")]
    [SerializeField] bool _haveCustomCoursesBeenLoaded = false;
    [SerializeField] List<ScriptableCourse> _customCourses = new List<ScriptableCourse>();

    [Header("All Available Courses")]
    [SerializeField] AvailableCourses _allAvailableCourses;

    // Tilebase mapping for loading custom courses
    Dictionary<TileBase, MapMakerGroundTileBase> _tileBaseToMapMakerObject = new Dictionary<TileBase, MapMakerGroundTileBase>();
    Dictionary<string, TileBase> _guidToTileBase = new Dictionary<string, TileBase>();

    public bool HaveCustomCoursesBeenLoaded
    {
        get
        {
            return _haveCustomCoursesBeenLoaded;
        }
    }
    public List<ScriptableCourse> CustomCourses
    {
        get
        {
            return _customCourses;
        }
    }
    public AvailableCourses AllAvailableCourses
    {
        get {
            return _allAvailableCourses;
        }
    }
    protected override void Awake()
    {
        base.Awake();
        InitTileReferences();
    }
    private void Start()
    {
        SceneManager.activeSceneChanged += ChangedActiveScene;

    }
    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= ChangedActiveScene;
    }
    private void ChangedActiveScene(Scene current, Scene next)
    {
        Debug.Log("CustomGolfCourseLoader: ChangedActiveScene: " + current.name + " to " + next.name);
        if (TitleScene.Is(next.name))
        {
            GetAllAvailableCourses(true);
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
    public void GetAllAvailableCourses(bool forceRecheck = false)
    {
        Debug.Log("GetAllAvailableCourses: forceRecheck: " + forceRecheck);
        if (!_haveCustomCoursesBeenLoaded || forceRecheck)
        {
            InitializeAllAvailableCourses();
            _steamWorkshopCourseDownloader.SyncSubscribedToCourses();
            AddBuiltinCourses();

            if (forceRecheck)
                _customCourses.Clear();

            LoadAllCustomCourses();
            
            AddCustomCourses();
            _haveCustomCoursesBeenLoaded = true;
        }        
        
    }
    void InitializeAllAvailableCourses()
    {
        //_allAvailableCourses = new AvailableCourses();
        _allAvailableCourses = ScriptableObject.CreateInstance<AvailableCourses>();
    }
    void AddBuiltinCourses()
    {
        if (_builtinCourses == null)
            return;
        if (_builtinCourses.Courses.Count <= 0)
            return;
        _allAvailableCourses.Courses.AddRange(_builtinCourses.Courses);
    }
    void AddCustomCourses()
    {
        if (_customCourses == null)
            return;
        if (_customCourses.Count <= 0)
            return;

        //_allAvailableCourses.Courses.AddRange(_customCourses);
        foreach (ScriptableCourse course in _customCourses)
        {
            if (_allAvailableCourses.Courses.Any(x => x.id == course.id))
            {
                Debug.Log("AddCustomCourses: Course with ID of: " + course.id + " is already in list of available courses. Skipping...");
                continue;
            }

            if (course.HolesInCourse.Length <= 0)
            {
                Debug.Log("UpdateCustomCourses: Custom Course with ID of: " + course.id + " does not have any holes. Skipping...");
                continue;
            }
            _allAvailableCourses.Courses.Add(course);
        }

    }

    public void LoadAllCustomCourses()
    {
        Debug.Log("LoadAllCustomCourses: ");
        GetAllCustomCourseFilePaths();        
    }

    void GetAllCustomCourseFilePaths()
    {
        List<string> filePathsForCourses = FileHandler.FindAllCustomCourses();
        Debug.Log("GetAllCustomCourse: " + filePathsForCourses.Count.ToString() + " course found in " + Application.persistentDataPath.ToString());
        LoadCoursesFromFilePath(filePathsForCourses);
    }
    void LoadCoursesFromFilePath(List<string> filePathsForCourses)
    {
        if (filePathsForCourses.Count <= 0)
            return;

        List<CourseData> coursesToLoad = new List<CourseData>();
        foreach (string filepath in filePathsForCourses)
        {
            Debug.Log("LoadCoursesFromFilePath: course found at: " + filepath);
            try
            {
                CourseData courseToLoad = FileHandler.ReadFromJSONFile<CourseData>(filepath, false);
                //courseNames.Add(courseToLoad.CourseName);
                if (courseToLoad != null)
                {
                    coursesToLoad.Add(courseToLoad);
                }
            }
            catch (Exception e)
            {
                Debug.Log("LoadCoursesFromFilePath: could not read file at: " + filepath + ". Error: " + e);
            }
        }
        AddValidCoursesToCustomCourseList(coursesToLoad);
    }
    void AddValidCoursesToCustomCourseList(List<CourseData> courses)
    {
        if (courses.Count <= 0)
            return;

        foreach (CourseData course in courses)
        {
            if (course.HolesInCourse.Count <= 0)
                continue;
            if (course.HolesInCourse.Count > CustomCourseLimits.MaxHolesPerCourse)
            {
                Debug.LogWarning("AddValidCoursesToCustomCourseList: course " + course.CourseId + " claims " + course.HolesInCourse.Count + " holes, past the limit of " + CustomCourseLimits.MaxHolesPerCourse + ". Skipping it.");
                continue;
            }
            if (course.HolesInCourse.Any(x => x.IsHoleCompleted == true))
            {
                Debug.Log("AddValidCoursesToCustomCourseList: Course with  with ID of: " + course.CourseId + " named: " + course.CourseName + " and has " + course.HolesInCourse.Count + " number of holes.");
                ScriptableCourse newCustomCourse = new ScriptableCourse();
                newCustomCourse.id = course.CourseId;
                // don't add a course that has already been added?

                if (_allAvailableCourses.Courses.Any(x => x.id == newCustomCourse.id))
                {
                    Debug.Log("AddValidCoursesToCustomCourseList: Course with  with ID of: " + course.CourseId + " is already in _allAvailableCourses. Skipping...");
                    continue;
                }
                    
                if (_customCourses.Any(x => x.id == newCustomCourse.id))
                {
                    Debug.Log("AddValidCoursesToCustomCourseList: Course with  with ID of: " + course.CourseId + " is already in _customCourses. Skipping...");
                    continue;
                }

                // Drawn with TextMeshPro, and this one came out of somebody else's file
                newCustomCourse.CourseName = CustomCourseLimits.SanitizeCourseName(course.CourseName);

                List<ScriptableHole> holesInCustomCourse = new List<ScriptableHole>();
                foreach (HoleData holeData in course.HolesInCourse)
                {
                    if (!holeData.IsHoleCompleted)
                        continue;

                    holesInCustomCourse.Add(ParseHoleDataIntoScriptableHole(holeData, course.CourseName));
                }
                newCustomCourse.HolesInCourse = holesInCustomCourse.ToArray();
                newCustomCourse.IsCustomCourse = true;
                newCustomCourse.WorkshopID = course.WorkshopPublishedItemID;
                _customCourses.Add(newCustomCourse);
            }
        }
    }
    // Drop-in play. Builds a one hole course from the hole currently open in the map maker and makes
    // it available to a golf session like any other course, by the id the session asks for. Nothing
    // is written to disk, and the IsHoleCompleted filter that LoadAllCustomCourses applies is
    // deliberately skipped: the point is to test a hole that is not finished.
    public ScriptableCourse RegisterTemporaryCourse(HoleData hole, string courseName, string courseId)
    {
        if (hole == null || string.IsNullOrEmpty(courseId))
            return null;

        RemoveTemporaryCourse(courseId);

        if (_allAvailableCourses == null)
            GetAllAvailableCourses();

        ScriptableCourse tempCourse = ScriptableObject.CreateInstance<ScriptableCourse>();
        tempCourse.id = courseId;
        tempCourse.CourseName = courseName;
        tempCourse.IsCustomCourse = true;
        tempCourse.WorkshopID = 0;
        tempCourse.HolesInCourse = new ScriptableHole[] { ParseHoleDataIntoScriptableHole(hole, courseName) };

        _allAvailableCourses.Courses.Add(tempCourse);
        Debug.Log("RegisterTemporaryCourse: added temporary course " + courseName + " with id: " + courseId);
        return tempCourse;
    }
    public void RemoveTemporaryCourse(string courseId)
    {
        if (string.IsNullOrEmpty(courseId) || _allAvailableCourses == null)
            return;

        _allAvailableCourses.Courses.RemoveAll(x => x != null && x.id == courseId);
        _customCourses.RemoveAll(x => x != null && x.id == courseId);
    }
    ScriptableHole ParseHoleDataIntoScriptableHole(HoleData holeData, string courseName)
    {
        ScriptableHole newHole = new ScriptableHole();

        // Set hole data that is saved easily?
        newHole.HoleIndex = holeData.HoleIndex;
        newHole.CourseName = courseName;
        newHole.HolePar = holeData.HolePar;
        newHole.HolePositions = ShiftListOfPostions(new List<Vector3> { holeData.HolePosition }) ;
        //newHole.TeeOffLocation = holeData.TeeOffLocation;
        newHole.TeeOffLocation = new Vector3(holeData.TeeOffLocation.x + 0.5f, holeData.TeeOffLocation.y + 0.5f, 0f);
        //newHole.TeeMarkerPositions = holeData.TeeOffMarkerPositions;
        newHole.TeeMarkerPositions = ShiftListOfPostions(holeData.TeeOffMarkerPositions);
        newHole.PolygonPoints = holeData.PolygonPoints;
        newHole.ZoomedOutPos = holeData.ZoomOutPosition;
        newHole.CameraZoomValue = 11f; // hard coded for now since the map maker doesn't set this (yet, hopefully)

        newHole.CourseAimPoints = GetCourseAimPoints(holeData.CourseAimPoints, holeData.HolePosition).ToArray();
        newHole.TeeOffAimPoint = newHole.CourseAimPoints[0];

        newHole.Tubes = new List<SavedTube>();
        newHole.MiniGolfPipes = new List<SavedMiniGolfPipe>();

        newHole.IsTeeOffChallenge = false;
        newHole.ClubToUse = "";
        //newHole.IsMiniGolf = false;
        newHole.IsMiniGolf = holeData.IsMiniGolf;

        newHole.CameraBoundingBoxPos = Vector3.zero;

        // go through all tilemap data in the hole data and set tile positions appropriately
        InitializeHoleSavedTileLists(newHole);
        ParseTileMapDataToSavedTilesAndSavedStuff(newHole, holeData.HoleTileMapData);
        return newHole;
    }
    List<Vector3> ShiftListOfPostions(List<Vector3> markerPositions)
    {
        List<Vector3> shiftedPositions = new List<Vector3>();
        foreach (Vector3 pos in markerPositions)
        {
            Vector3 shifted = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f);
            shiftedPositions.Add(shifted);
        }
        return shiftedPositions;
    }
    Vector3 ShiftSinglePosition(Vector3 position)
    {
        return new Vector3(position.x + 0.5f, position.y + 0.5f, 0f);
    }
    List<Vector3> GetCourseAimPoints(Vector3[] aimPoints, Vector3 holePosition)
    {
        List<Vector3> newAimpoints = new List<Vector3>();

        // A hole with no aim points has a null array, not an empty one. Reading one back from a
        // file hides that, because JsonUtility writes null arrays as [], but a hole handed straight
        // over in memory - which is what drop-in does - keeps the null.
        if (aimPoints != null && aimPoints.Length > 0)
        {
            for (int i = 0; i < aimPoints.Length; i++)
            {
                // Shifted to the middle of the cell, like the tee off location, the tee markers and
                // the hole. A saved aim point is a cell coordinate, so without this an aim lands on
                // the corner where four tiles meet instead of on the object standing there - most
                // visibly for the last aim point, which is the hole.
                Vector3 shiftedAimPoint = ShiftSinglePosition(aimPoints[i]);
                if (newAimpoints.Contains(shiftedAimPoint))
                    continue;

                newAimpoints.Add(shiftedAimPoint);
            }
        }

        Vector3 shiftedHolePosition = ShiftSinglePosition(holePosition);
        if (!newAimpoints.Contains(shiftedHolePosition))
        {
            newAimpoints.Add(shiftedHolePosition);
        }

        return newAimpoints;
    }
    void InitializeHoleSavedTileLists(ScriptableHole hole)
    {
        hole.GreenTiles = new List<SavedTile>();
        hole.FairwayTiles = new List<SavedTile>();
        hole.RoughTiles = new List<SavedTile>();
        hole.DeepRoughTiles = new List<SavedTile>();
        hole.SandTrapTiles = new List<SavedTile>();
        hole.WaterTrapTiles = new List<SavedTile>();
        hole.EdgesTiles = new List<SavedTile>();
        hole.DirectionTiles = new List<SavedTile>();
        hole.MiniGolfWallTiles = new List<SavedTile>();

        // Only ever filled in when the hole has something on its Object tilemap, and the game walks
        // all three whether it does or not. Every saved course happened to have at least one
        // object, so a hole with none - a new one, or one being tested from the map maker - left
        // these null and broke loading.
        hole.SavedObstacles = new List<SavedObstacle>();
        hole.Statues = new List<SavedStatue>();
        hole.BalloonPowerUps = new List<SavedBalloonPowerUp>();
    }
    void ParseTileMapDataToSavedTilesAndSavedStuff(ScriptableHole hole, List<TilemapData> tileMapData)
    {
        foreach (TilemapData mapData in tileMapData)
        {
            if (mapData.tiles != null && mapData.tiles.Count > 0)
            {
                if (mapData.key == "Green")
                {
                    hole.GreenTiles = ConvertTileInfoToSavedTile(mapData.tiles);
                }
                else if (mapData.key == "Fairway")
                {
                    hole.FairwayTiles = ConvertTileInfoToSavedTile(mapData.tiles);
                }
                else if (mapData.key == "Rough")
                {
                    hole.RoughTiles = ConvertTileInfoToSavedTile(mapData.tiles);
                }
                else if (mapData.key == "Sand")
                {
                    hole.SandTrapTiles = ConvertTileInfoToSavedTile(mapData.tiles);
                }
                else if (mapData.key == "Water")
                {
                    hole.WaterTrapTiles = ConvertTileInfoToSavedTile(mapData.tiles);
                }
                else if (mapData.key == "DirectionTiles")
                {
                    hole.DirectionTiles = ConvertTileInfoToSavedTile(mapData.tiles);
                }
                else if (mapData.key == "MiniGolfWalls")
                {
                    hole.MiniGolfWallTiles = ConvertTileInfoToSavedTile(mapData.tiles);
                }
                else if (mapData.key == "Object")
                {
                    GetObstaclesAndOthersFromTileInfo(hole, mapData.tiles);
                }
            }
        }
    }
    List<SavedTile> ConvertTileInfoToSavedTile(List<TileInfo> tiles)
    {
        List<SavedTile> newSavedTiles = new List<SavedTile>();

        int skippedTiles = 0;
        int outOfRangeTiles = 0;

        if (tiles.Count > 0)
        {
            foreach (TileInfo tile in tiles)
            {
                // A course can name a tile this build doesn't have: an older save, or a workshop
                // course from a different version. Skip that tile rather than throw, which would
                // abandon the whole list of custom courses.
                if (!_guidToTileBase.ContainsKey(tile.GuidForTile))
                {
                    skippedTiles++;
                    continue;
                }

                // A tile far outside the playable area makes the tilemap allocate storage covering
                // the whole distance to it, which hangs the game on a file from anybody else.
                if (!CustomCourseLimits.IsTilePositionInRange(tile.position))
                {
                    outOfRangeTiles++;
                    continue;
                }

                SavedTile newTile = new SavedTile();
                newTile.TilePos = tile.position;
                newTile.MyTile = _guidToTileBase[tile.GuidForTile];
                newSavedTiles.Add(newTile);

                if (newSavedTiles.Count >= CustomCourseLimits.MaxTilesPerTileMap)
                {
                    Debug.LogWarning("ConvertTileInfoToSavedTile: stopping at the limit of " + CustomCourseLimits.MaxTilesPerTileMap + " tiles for one tilemap.");
                    break;
                }
            }
        }

        if (skippedTiles > 0)
            Debug.LogWarning("ConvertTileInfoToSavedTile: skipped " + skippedTiles + " tile(s) whose guid is not in this build.");
        if (outOfRangeTiles > 0)
            Debug.LogWarning("ConvertTileInfoToSavedTile: skipped " + outOfRangeTiles + " tile(s) placed outside the allowed cell range.");



        return newSavedTiles;
    }
    void GetObstaclesAndOthersFromTileInfo(ScriptableHole hole, List<TileInfo> tiles)
    {
        List<SavedObstacle> obstacles = new List<SavedObstacle>();
        List<SavedStatue> statues = new List<SavedStatue>();
        List<SavedBalloonPowerUp> balloonPowerUps = new List<SavedBalloonPowerUp>();

        if (tiles.Count > 0)
        {
            foreach (TileInfo tile in tiles)
            {
                if (_guidToTileBase.ContainsKey(tile.GuidForTile))
                {
                    TileBase tileBase = _guidToTileBase[tile.GuidForTile];
                    if (_tileBaseToMapMakerObject.ContainsKey(tileBase))
                    {
                        MapMakerGroundTileBase mapMakerGroundTileBase = _tileBaseToMapMakerObject[tileBase];
                        if (mapMakerGroundTileBase.GetType() == typeof(MapMakerObstacle))
                        {
                            MapMakerObstacle mapMakerObstacle = (MapMakerObstacle)mapMakerGroundTileBase;

                            if (mapMakerObstacle.ObstacleType == ObstacleType.StatueGoodWeather)
                            {
                                SavedStatue newStatue = new SavedStatue();
                                //newStatue.StatuePosition = tile.position;
                                newStatue.StatuePosition = ShiftSinglePosition(tile.position);
                                newStatue.StatueType = "good-weather";
                                newStatue.StatueScriptableObstacle = mapMakerObstacle.ScriptableObstacle;
                                statues.Add(newStatue);
                                //hole.Statues.Add(newStatue);
                            }
                            else if (mapMakerObstacle.ObstacleType == ObstacleType.StatueBadWeather)
                            {
                                SavedStatue newStatue = new SavedStatue();
                                //newStatue.StatuePosition = tile.position;
                                newStatue.StatuePosition = ShiftSinglePosition(tile.position);
                                newStatue.StatueType = "bad-weather";
                                newStatue.StatueScriptableObstacle = mapMakerObstacle.ScriptableObstacle;
                                statues.Add(newStatue);
                                //hole.Statues.Add(newStatue);
                            }
                            else if (mapMakerObstacle.ObstacleType == ObstacleType.BalloonPowerUp)
                            {
                                SavedBalloonPowerUp balloon = new SavedBalloonPowerUp();
                                //balloon.BalloonPosition = tile.position;
                                balloon.BalloonPosition = ShiftSinglePosition(tile.position);
                                balloon.BalloonHeight = "";
                                balloon.BalloonScriptableObstacle = mapMakerObstacle.ScriptableObstacle;
                                balloonPowerUps.Add(balloon);
                                //hole.BalloonPowerUps.Add(balloon);
                            }
                            else if (mapMakerObstacle.ObstacleType == ObstacleType.Tree)
                            {
                                SavedObstacle obstacle = new SavedObstacle();
                                //obstacle.ObstaclePos = tile.position;
                                obstacle.ObstaclePos = ShiftSinglePosition(tile.position);
                                obstacle.ObstacleScriptableObject = mapMakerObstacle.ScriptableObstacle;
                                obstacles.Add(obstacle);
                                //hole.SavedObstacles.Add(obstacle);
                            }
                        }
                    }
                }
                
            }
        }
        hole.SavedObstacles = obstacles;
        hole.Statues = statues;
        hole.BalloonPowerUps = balloonPowerUps;
    }
    public void DownloadNewCourse(ulong courseWorkshopID)
    {
        Debug.Log("DownloadNewCourse: " + courseWorkshopID);
        this._steamWorkshopCourseDownloader.DownloadNewCourse(courseWorkshopID);
    }
    public void LoadNewCustomCourse(string filePath)
    {
        if (!File.Exists(Application.persistentDataPath + "/" + filePath))
            return;
        Debug.Log("LoadNewCustomCourse: " + filePath);
        List<string> courseToLoad = new List<string> { filePath };
        LoadCoursesFromFilePath(courseToLoad);
        AddCustomCourses();
        GameObject.FindGameObjectWithTag("LocalNetworkPlayer").GetComponent<NetworkPlayer>().CustomCourseAdded();
    }
    public void NewCustomCourseFinishedDownloading()
    {   
        Debug.Log("NewCustomCourseFinishedDownloading: ");
        LoadAllCustomCourses();
        AddCustomCourses();
    }

}
