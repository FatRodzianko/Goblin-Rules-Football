using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum ObstacleType
{
    None,
    Tree,
    Hole
}
[CreateAssetMenu(fileName = "MapMaker", menuName = "MapMakerObjects/Create Obstacle")]
public class MapMakerObstacle : MapMakerGroundTileBase
{
    [SerializeField] public ScriptableObstacle ScriptableObstacle;
}
