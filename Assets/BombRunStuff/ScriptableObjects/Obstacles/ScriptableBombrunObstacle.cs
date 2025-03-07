using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum BombRunObstacleType
{
    Crate,
    Door,
    Wall,
    Misc
}
public enum ObstacleCoverType
{
    None,
    Full,
    Partial    
}

[CreateAssetMenu(fileName = "BombRunObstacle", menuName = "BombRun/Obstacles/Create Obstacle")]
public class ScriptableBombrunObstacle : ScriptableObject
{
    [SerializeField] private Transform _bombRunObstaclePrefab;
    [SerializeField] private BombRunObstacleType _bombRunObstacleType;
    [SerializeField] private TileBase _tile;
    [SerializeField] private bool _isDestrucable;
    [SerializeField] private bool _isInteractable;

    [Header("Cover Stuff")]
    [SerializeField] private ObstacleCoverType _obstacleCoverType;

    public Transform BombRunObstaclePrefab
    {
        get
        {
            return _bombRunObstaclePrefab;
        }
    }
    public BombRunObstacleType BombRunObstacleType
    {
        get
        {
            return _bombRunObstacleType;
        }
    }
    public ObstacleCoverType ObstacleCoverType
    {
        get
        {
            return _obstacleCoverType;
        }
    }
    public TileBase Tile
    {
        get
        {
            return _tile;
        }
    }
    public bool IsDestrucable
    {
        get
        {
            return _isDestrucable;
        }
    }
    public bool IsInteractable
    {
        get
        {
            return _isInteractable;
        }
    }
}
