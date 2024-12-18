using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum BombRunObstacleType
{
    Crate,
    Door,
    Misc
}

[CreateAssetMenu(fileName = "BombRunObstacle", menuName = "BombRun/Obstacles/Create Obstacle")]
public class ScriptableBombrunObstacle : ScriptableObject
{
    [SerializeField] private Transform _bombRunObstaclePrefab;
    [SerializeField] private BombRunObstacleType _bombRunObstacleType;
    [SerializeField] private TileBase _tile;
    [SerializeField] private bool _isDestrucable;
    [SerializeField] private bool _isInteractable;

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
