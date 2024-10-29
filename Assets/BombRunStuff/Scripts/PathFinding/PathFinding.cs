using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 10;

    [SerializeField] private Transform _gridDebugObjectPrefab;
    private int _width;
    private int _height;
    private float _cellSize;

    // grid system
    GridSystem<PathNode> _gridSystem;

    private void Awake()
    {
        _gridSystem = new GridSystem<PathNode>(10, 10, 2f, 
            (GridSystem<PathNode> g, GridPosition gridPosition) => new PathNode(gridPosition));

        _gridSystem.CreateDebugObjects(_gridDebugObjectPrefab);
    }
    public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        List<PathNode> openList = new List<PathNode>(); // used for nodes waiting to be searched
        List<PathNode> closedList = new List<PathNode>(); // used for nodes that have already been searched

        // add the starting node to the open list
        PathNode startNode = _gridSystem.GetGridObject(startGridPosition);
        PathNode endNode = _gridSystem.GetGridObject(endGridPosition);

        openList.Add(startNode);

        // cycle through all nodes and reset their state
        for (int x = 0; x < _gridSystem.GetWidth(); x++)
        {
            for (int y = 0; y < _gridSystem.GetHeight(); y++)
            {
                GridPosition gridPosition = new GridPosition(x, y);
                PathNode pathNode = _gridSystem.GetGridObject(gridPosition);

                // set the g cost to infinite and h cost to 0
                pathNode.SetGCost(int.MaxValue);
                pathNode.SetHCost(0);
                // calculate the f cost with new g and h values
                pathNode.CalculateFCost();

                // reset the "came from node" value
                pathNode.ResetCameFromPathNode();
            }
        }

        // set up the starting node's values
        startNode.SetGCost(0);
        startNode.SetHCost(CalculateDistance(startGridPosition, endGridPosition));
        startNode.CalculateFCost();

        // search for a path until the openList count is 0
        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostPathNode(openList);

            // check if this is the current node is at the end node
            if (currentNode == endNode)
            {
                // reach final node
                return CalculatePath(endNode);
            }

            // remove current node from openList and add to closeList
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // Get Neighbors to search through
        }

        return new List<GridPosition>();
    }
    public int CalculateDistance(GridPosition a, GridPosition b)
    {
        GridPosition gridPositionDistance = a - b;
        int distance = Mathf.Abs(gridPositionDistance.x) + Mathf.Abs(gridPositionDistance.y);
        return distance * MOVE_STRAIGHT_COST;
    }
    private PathNode GetLowestFCostPathNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostPathNode = pathNodeList[0];
        int pathNodeCount = pathNodeList.Count;

        if (pathNodeCount <= 1)
        {
            return lowestFCostPathNode;
        }
        for (int i = 0; i < pathNodeCount; i++)
        {
            if (pathNodeList[i].GetFCost() < lowestFCostPathNode.GetFCost())
            {
                lowestFCostPathNode = pathNodeList[i];
            }
        }
        return lowestFCostPathNode;
    }
    private PathNode GetNode(int x, int y)
    {
        return _gridSystem.GetGridObject(new GridPosition(x, y));
    }
    private List<PathNode> GetNeighborList(PathNode currentNode)
    {
        List<PathNode> neighborList = new List<PathNode>();

        GridPosition gridPosition = currentNode.GetGridPosition();

        if (gridPosition.x - 1 >= 0)
        {
            // node to the left
            neighborList.Add(GetNode(gridPosition.x - 1, gridPosition.y + 0));
            if (gridPosition.y + 1 < _gridSystem.GetHeight())
            {
                // left up
                neighborList.Add(GetNode(gridPosition.x - 1, gridPosition.y + 1));
            }            
            if (gridPosition.y - 1 >= 0)
            {
                // left down
                neighborList.Add(GetNode(gridPosition.x - 1, gridPosition.y - 1));
            }

        }
        if (gridPosition.x + 1 < _gridSystem.GetWidth())
        {
            // right
            neighborList.Add(GetNode(gridPosition.x + 1, gridPosition.y + 0));
            if (gridPosition.y + 1 < _gridSystem.GetHeight())
            {
                // right up
                neighborList.Add(GetNode(gridPosition.x + 1, gridPosition.y + 1));
            }
            if (gridPosition.y - 1 >= 0)
            {
                // right down
                neighborList.Add(GetNode(gridPosition.x + 1, gridPosition.y - 1));
            }            
        }
        // up
        if (gridPosition.y + 1 < _gridSystem.GetHeight())
        {
            neighborList.Add(GetNode(gridPosition.x + 0, gridPosition.y + 1));
        }
        // down
        if (gridPosition.y - 1 >= 0)
        {
            neighborList.Add(GetNode(gridPosition.x + 0, gridPosition.y - 1));
        }

        return neighborList;
    }
}
