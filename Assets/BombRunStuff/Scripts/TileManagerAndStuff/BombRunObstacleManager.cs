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
        public BaseBombRunObstacle _ObstacleScript;
        public BombRunObstacleType _BombRunObstacleType;

        public ObstaclePositionMapping(GridPosition gridPosition, BaseBombRunObstacle obstacleScript, BombRunObstacleType bombRunObstacleType)
        {
            this._GridPosititon = gridPosition;
            this._ObstacleScript = obstacleScript;
            this._BombRunObstacleType = bombRunObstacleType;
        }
    }

    [Header("Obstacle Positions")]
    [SerializeField] private List<GridPosition> _obstacleGridPositions = new List<GridPosition>();
    [SerializeField] private List<ObstaclePositionMapping> _obstaclePositionMapping = new List<ObstaclePositionMapping>();

    [Header("Scriptable Obstacles")]
    [SerializeField] List<ScriptableBombrunObstacle> _scriptableBombRunObstacles = new List<ScriptableBombrunObstacle>();

    private void Start()
    {
        BaseBombRunObstacle.OnAnyObstacleDestroyed += BaseBombRunObstacle_OnAnyObstacleDestroyed;
    }    

    private void OnDisable()
    {
        BaseBombRunObstacle.OnAnyObstacleDestroyed -= BaseBombRunObstacle_OnAnyObstacleDestroyed;
    }
    private void BaseBombRunObstacle_OnAnyObstacleDestroyed(object sender, GridPosition gridPosition)
    {
        BaseBombRunObstacle obstacle = sender as BaseBombRunObstacle;
        LevelGrid.Instance.RemoveObstacleAtGridPosition(gridPosition, obstacle);
        RemoveObstacleToObstaclePositionMapping(gridPosition, obstacle, obstacle.GetBombRunObstacleType());
    }
    public void AddObstacleToPositionFromTile(GridPosition gridPosition, TileBase gridTile)
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
        BaseBombRunObstacle obstacleScript = obstacleTransform.GetComponent<BaseBombRunObstacle>();
        //_obstaclePositionMapping.Add(new ObstaclePositionMapping(gridPosition, obstacleScript, obstacle.BombRunObstacleType));
        AddObstacleToObstaclePositionMapping(gridPosition, obstacleScript, obstacle.BombRunObstacleType);
        AddObstacleToGridObject(gridPosition, obstacleScript);
    }
    void AddObstacleToObstaclePositionMapping(GridPosition gridPosition, BaseBombRunObstacle obstacleScript, BombRunObstacleType bombRunObstacleType)
    {
        _obstaclePositionMapping.Add(new ObstaclePositionMapping(gridPosition, obstacleScript, bombRunObstacleType));
    }
    void RemoveObstacleToObstaclePositionMapping(GridPosition gridPosition, BaseBombRunObstacle obstacleScript, BombRunObstacleType bombRunObstacleType)
    {
        if (obstacleScript == null)
            return;

        ObstaclePositionMapping obstacleMapping = _obstaclePositionMapping.FirstOrDefault(x => x._ObstacleScript == obstacleScript && x._GridPosititon == gridPosition);
        if (_obstaclePositionMapping.Contains(obstacleMapping))
        {
            _obstaclePositionMapping.Remove(obstacleMapping);
            Debug.Log("RemoveObstacleToObstaclePositionMapping: Removed obstacle: " + obstacleScript.name + " from _obstaclePositionMapping at position: " + gridPosition.ToString());
        }
    }
    void AddObstacleToGridObject(GridPosition gridPosition, BaseBombRunObstacle obstacle)
    {
        // currently takes the obstacle transform and adds that to the grid object. Later will likely have a BombRunObstacle class/monobehavior that is added instead
        if (obstacle == null)
            return;

        LevelGrid.Instance.AddObstacleAtGridPosition(gridPosition, obstacle);
        if (obstacle.IsInteractable())
        {
            LevelGrid.Instance.AddInteractableAtGridPosition(gridPosition, obstacle.GetComponent<IInteractable>());
        }
    }
    public BaseBombRunObstacle GetObstacleAtGridPosition(GridPosition gridPostion)
    {
        return _obstaclePositionMapping.FirstOrDefault(x => x._GridPosititon == gridPostion)._ObstacleScript;
    }
    public bool IsObstacleAtGridPosition(GridPosition gridPosition)
    {
        return _obstaclePositionMapping.Any(x => x._GridPosititon == gridPosition);
    }
}
