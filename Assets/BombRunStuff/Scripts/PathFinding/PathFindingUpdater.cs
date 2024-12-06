using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingUpdater : MonoBehaviour
{
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
        PathFinding.Instance.SetIsWalkableGridPosition(gridPosition, true);
    }
}
