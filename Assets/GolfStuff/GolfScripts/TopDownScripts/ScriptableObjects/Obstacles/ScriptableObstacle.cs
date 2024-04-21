using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Golf Environment Obstacle", menuName = "Golf Environment Obstacle")]
public class ScriptableObstacle : ScriptableObject
{
    public GameObject ObstaclePrefab;
    public GameObject MapMakerPrefab;
    public string SoftBounceSoundType;
    public string HardBounceSoundType;

}
