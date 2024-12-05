using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BombRunObstacleManager : MonoBehaviour
{
    [Serializable]
    public struct ObstaclePositionMapping
    {
        public GridPosition _GridPosititon;
        public Transform _ObstacleTransform;
        public BombRunObstacleType _BombRunObstacleType;

        public ObstaclePositionMapping(GridPosition gridPisition, Transform obstacleTransform, BombRunObstacleType bombRunObstacleType)
        {
            this._GridPosititon = gridPisition;
            this._ObstacleTransform = obstacleTransform;
            this._BombRunObstacleType = bombRunObstacleType;
        }
    }

    [Header("Obstacle Positions")]
    [SerializeField] private List<GridPosition> _obstacleGridPositions = new List<GridPosition>();
    [SerializeField] private List<ObstaclePositionMapping> _obstaclePositionMapping = new List<ObstaclePositionMapping>();

    [Header("Scriptable Obstacles")]
    [SerializeField] List<ScriptableBombrunObstacle> _scriptableBombRunObstacles = new List<ScriptableBombrunObstacle>();

    public void AddObstacleToPosition(GridPosition gridPosition, TileBase gridTile)
    {
        Debug.Log("AddObstacleToPosition: at position: " + gridPosition.ToString() + " for tile: " + gridTile.name);
        ScriptableBombrunObstacle obstacle = _scriptableBombRunObstacles.FirstOrDefault(x => x.Tile == gridTile);
        if (obstacle == null)
        {
            Debug.Log("AddObstacleToPosition: nothing found for " + gridTile.name);
            return;
        }            

        _obstacleGridPositions.Add(gridPosition);
        Transform obstacleTransform = Instantiate(obstacle.BombRunObstaclePrefab, LevelGrid.Instance.GetWorldPosition(gridPosition), Quaternion.identity);
        _obstaclePositionMapping.Add(new ObstaclePositionMapping(gridPosition, obstacleTransform, obstacle.BombRunObstacleType));

        AddObstacleToGridObject(gridPosition, obstacleTransform);
    }
    void AddObstacleToGridObject(GridPosition gridPosition, Transform obstacleTransform)
    {
        // currently takes the obstacle transform and adds that to the grid object. Later will likely have a BombRunObstacle class/monobehavior that is added instead
        if (obstacleTransform == null)
            return;

        LevelGrid.Instance.AddObstacleAtGridPosition(gridPosition, obstacleTransform);
    }
    public Transform GetObstacleAtGridPosition(GridPosition gridPostion)
    {
        return _obstaclePositionMapping.FirstOrDefault(x => x._GridPosititon == gridPostion)._ObstacleTransform;
    }
    public bool IsObstacleAtGridPosition(GridPosition gridPosition)
    {
        return _obstaclePositionMapping.Any(x => x._GridPosititon == gridPosition);
    }
}
