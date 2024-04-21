using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObstacleType
{
    None,
    Tree,
    Hole,
    StatueGoodWeather,
    StatueBadWeather,
    BalloonPowerUp
}
[CreateAssetMenu(fileName = "MapMaker", menuName = "MapMakerObjects/Create Obstacle")]
public class MapMakerObstacle : MapMakerGroundTileBase
{
    [SerializeField] public ScriptableObstacle ScriptableObstacle;
    [SerializeField] ObstacleType _obstacleType;

    public ObstacleType ObstacleType
    {
        get
        {
            return _obstacleType;
        }
    }
}
