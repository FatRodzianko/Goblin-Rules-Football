using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System;

public class TileMapManager : MonoBehaviour
{
    [SerializeField] private Tilemap _greenMap, _fairwayMap, _roughMap, _deepRoughMap, _sandTrapMap, _waterTrapMap, _edgesMap, _directionTilesMap;
    [SerializeField] private int _holeIndex;
    [SerializeField] private string _courseName;
    [SerializeField] private int _holePar;

    [Header("Prefabs for Objects like holes/tee ball things")]
    [SerializeField] GameObject _holePrefab;
    [SerializeField] GameObject _teeMarkerPrefab;
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

        // Find all Environment obstacles and save their locations
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("GolfEnvironmentObstacle");
        newHole.SavedObstacles = SaveAllObstacles(obstacles).ToList();

        // Save the scriptable object to a file
        ScriptableObjectUtility.SaveHoleToFile(newHole);

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
    }
    public void ClearMap()
    {
        // Clear all tilemaps
        var maps = FindObjectsOfType<Tilemap>();
        foreach (var tilemap in maps)
        {
            tilemap.ClearAllTiles();
        }
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
            GameObject[] obstacles = GameObject.FindGameObjectsWithTag("GolfEnvironmentObstacle");
            DeleteObjects(obstacles);
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
        
    }
    public void LoadMap()
    {
        var hole = Resources.Load<ScriptableHole>($"Holes/{_courseName}_{_holeIndex}");
        if (hole == null)
        {
            Debug.LogError($"{_courseName}_{_holeIndex} does not exist.");
            return;
        }
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
        // Spawn the obstacles
        foreach (SavedObstacle savedObstacle in hole.SavedObstacles)
        {
            Instantiate(savedObstacle.ObstacleScriptableObject.ObstaclePrefab, savedObstacle.ObstaclePos, Quaternion.identity);
        }
        // LATER save the tee off position to the GameManager
        // LATER save the hole PAR value to the GameManager

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
    public void DeleteObjects(GameObject[] objects)
    {
        if (objects.Length <= 0)
            return;
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject toDelete = objects[i];
            DestroyImmediate(toDelete);
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

