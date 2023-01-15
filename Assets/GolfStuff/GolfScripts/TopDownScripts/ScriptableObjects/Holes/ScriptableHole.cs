using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

public class ScriptableHole : ScriptableObject
{
    [Header("Hole Information")]
    public int HoleIndex;
    public string CourseName;
    public int HolePar;
    public List<Vector3> HolePositions;
    public Vector3 TeeOffLocation;
    public List<Vector3> TeeMarkerPositions;
    [Header("Saved Tiles")]
    public List<SavedTile> GreenTiles;
    public List<SavedTile> FairwayTiles;
    public List<SavedTile> RoughTiles;
    public List<SavedTile> DeepRoughTiles;
    public List<SavedTile> SandTrapTiles;
    public List<SavedTile> WaterTrapTiles;
    public List<SavedTile> EdgesTiles;
    public List<SavedTile> DirectionTiles;
    [Header("Saved Obstacles")]
    public List<SavedObstacle> SavedObstacles;
    [Header("Camera Bounding Box")]
    public Vector3 CameraBoundingBoxPos;
    public Vector2[] PolygonPoints;

}
[Serializable]
public class SavedTile 
{
    public Vector3Int TilePos;
    public Tile MyTile; 
}
[Serializable]
public class SavedObstacle
{
    public Vector3 ObstaclePos;
    public ScriptableObstacle ObstacleScriptableObject;
}
