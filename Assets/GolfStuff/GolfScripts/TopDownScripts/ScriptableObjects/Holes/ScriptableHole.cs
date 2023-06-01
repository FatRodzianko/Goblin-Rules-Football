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
    [Header("Camera and Bounding Box")]
    public Vector3 CameraBoundingBoxPos;
    public Vector2[] PolygonPoints;
    public Vector3 ZoomedOutPos;
    public float CameraZoomValue;
    // Line camera should take in the "intro" video?
    // "Hit points" for the player to aim at when their turn starts. Have a few of the "intended route"
    [Header("Course Aim Points")]
    public Vector3 TeeOffAimPoint;
    //public Vector3[] 
    [Header("Statue Positions")]
    //public Vector3[] BadStatuePositions;
    //public Vector3[] GoodStatuePositions;
    public List<SavedStatue> Statues;
    public List<SavedBalloonPowerUp> BalloonPowerUps;

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
[Serializable]
public class SavedStatue
{
    public Vector3 StatuePosition;
    public string StatueType;
    public ScriptableObstacle StatueScriptableObstacle;
}
[Serializable]
public class SavedBalloonPowerUp
{
    public Vector3 BalloonPosition;
    public string BalloonHeight;
    public ScriptableObstacle BalloonScriptableObstacle;
}
