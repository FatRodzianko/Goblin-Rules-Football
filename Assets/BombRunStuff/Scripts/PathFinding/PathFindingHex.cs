using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingHex : PathFinding
{
    private const int MOVE_STRAIGHT_COST = 10;
    public override void Setup(int width, int height, float cellSize)
    {
        Debug.Log("PathFindingHex: Setup");
        this._width = width;
        this._height = height;
        this._cellSize = cellSize;

        _gridSystem = new GridSystemHex<PathNode>(_width, _height, _cellSize,
            (GridSystem<PathNode> g, GridPosition gridPosition) => new PathNode(gridPosition));

        //_gridSystem.CreateDebugObjects(_gridDebugObjectPrefab);

        InitializeIsWalkable();
    }

    public override List<PathNode> GetNeighborList(PathNode currentNode)
    {
        List<PathNode> neighborList = new List<PathNode>();

        GridPosition gridPosition = currentNode.GetGridPosition();

        if (gridPosition.x - 1 >= 0)
        {
            // Left
            neighborList.Add(GetNode(gridPosition.x - 1, gridPosition.y + 0));
        }

        if (gridPosition.x + 1 < _gridSystem.GetWidth())
        {
            // Right
            neighborList.Add(GetNode(gridPosition.x + 1, gridPosition.y + 0));
        }

        if (gridPosition.y - 1 >= 0)
        {
            // Down
            neighborList.Add(GetNode(gridPosition.x + 0, gridPosition.y - 1));
        }
        if (gridPosition.y + 1 < _gridSystem.GetHeight())
        {
            // Up
            neighborList.Add(GetNode(gridPosition.x + 0, gridPosition.y + 1));
        }

        bool oddRow = gridPosition.y % 2 == 1;

        if (oddRow)
        {
            if (gridPosition.x + 1 < _gridSystem.GetWidth())
            {
                if (gridPosition.y - 1 >= 0)
                {
                    neighborList.Add(GetNode(gridPosition.x + 1, gridPosition.y - 1));
                }
                if (gridPosition.y + 1 < _gridSystem.GetHeight())
                {
                    neighborList.Add(GetNode(gridPosition.x + 1, gridPosition.y + 1));
                }
            }
        }
        else
        {
            if (gridPosition.x - 1 >= 0)
            {
                if (gridPosition.y - 1 >= 0)
                {
                    neighborList.Add(GetNode(gridPosition.x - 1, gridPosition.y - 1));
                }
                if (gridPosition.y + 1 < _gridSystem.GetHeight())
                {
                    neighborList.Add(GetNode(gridPosition.x - 1, gridPosition.y + 1));
                }
            }
        }
        return neighborList;
    }
    public override int GetTenativeGCost(PathNode currentNode, PathNode neighborNode)
    {
        return currentNode.GetGCost() + MOVE_STRAIGHT_COST;
    }
    public override int CalculateDistance(GridPosition a, GridPosition b)
    {
        return Mathf.RoundToInt(MOVE_STRAIGHT_COST *
            Vector2.Distance(_gridSystem.GetWorldPosition(a), _gridSystem.GetWorldPosition(b)));
    }
    

}
