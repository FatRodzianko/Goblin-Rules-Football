using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    public static PathFinding Instance { get; private set; }

    [SerializeField] private Transform _gridDebugObjectPrefab;
    private int _width;
    private int _height;
    private float _cellSize;

    // grid system
    GridSystem<PathNode> _gridSystem;

    private void Awake()
    {
        MakeInstance();

        //_gridSystem = new GridSystem<PathNode>(10, 10, 2f, 
        //    (GridSystem<PathNode> g, GridPosition gridPosition) => new PathNode(gridPosition));

        //_gridSystem.CreateDebugObjects(_gridDebugObjectPrefab);
    }
    void MakeInstance()
    {
        if (Instance != null)
        {
            Debug.Log("MakeInstance: more than one PathFinding. Destroying...");
            Destroy(this);
            return;
        }
        Instance = this;
    }
    public void Setup(int width, int height, float cellSize)
    {
        this._width = width;
        this._height = height;
        this._cellSize = cellSize;

        _gridSystem = new GridSystem<PathNode>(_width, _height, _cellSize,
            (GridSystem<PathNode> g, GridPosition gridPosition) => new PathNode(gridPosition));

        //_gridSystem.CreateDebugObjects(_gridDebugObjectPrefab);

        InitializeIsWalkable();
    }
    void InitializeIsWalkable()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                GridPosition gridPosition = new GridPosition(x, y);
                if (BombRunTileMapManager.Instance.IsWallOnThisPosition(gridPosition))
                {
                    GetNode(x, y).SetIsWalkable(false);
                }
                else if (BombRunTileMapManager.Instance.IsObstacleOnThisPosition(gridPosition))
                {
                    // Check if the obstale is walkable or not?
                    GetNode(x, y).SetIsWalkable(false);
                }
                else
                {
                    GetNode(x, y).SetIsWalkable(true);
                }
            }
        }
    }
    public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength)
    {
        if (!LevelGrid.Instance.IsValidGridPosition(endGridPosition))
        {
            //Debug.Log("FindPath: " + endGridPosition.ToString() + " is not a valid position");
            pathLength = 0;
            return null;
        }

        List<PathNode> openList = new List<PathNode>(); // used for nodes waiting to be searched
        List<PathNode> closedList = new List<PathNode>(); // used for nodes that have already been searched

        // add the starting node to the open list
        PathNode startNode = _gridSystem.GetGridObject(startGridPosition);
        PathNode endNode = _gridSystem.GetGridObject(endGridPosition);

        // check to make sure the endNode is walkable
        if (!endNode.IsWalkable())
        {
            //Debug.Log("FindPath: " + endGridPosition.ToString() + " is not walkable.");
            pathLength = 0;
            return null;
        }

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
            // set the current node to the node in the list with the lowest f cost. The node with the lowest f cost is what we'd want to prioritize in path calculations?
            PathNode currentNode = GetLowestFCostPathNode(openList);

            // check if this is the current node is at the end node
            if (currentNode == endNode)
            {
                // reach final node
                pathLength = endNode.GetFCost();
                return CalculatePath(endNode);
            }

            // remove current node from openList and add to closeList
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // Get Neighbors to search through
            // consider "caching" neighbor nodes in the nodes themselves so this doesn't need to be recalculated every time? When the path nodes are created, get all the neighbor nodes for each node?
            foreach (PathNode neighborNode in GetNeighborList(currentNode))
            {
                // check if the neighborNode is already in closed list. If it is, it was already searched and can be skippeed
                if (closedList.Contains(neighborNode))
                {
                    continue;
                }
                // check if the node is walkable. If not, it is not a valid point on the path
                if (!neighborNode.IsWalkable())
                {
                    closedList.Add(neighborNode);
                    continue;
                }

                // Get the tenative G cost of the neighbor node by taking the current node's g cost, and then adding the distance to the neighbor node.
                // this should be currentNode.gCost + 10 if it is adjacent to the current node, and currentNode.gCost + 14 if it is diagonal to the current node
                int tentativeGCost = currentNode.GetGCost() + CalculateDistance(currentNode.GetGridPosition(), neighborNode.GetGridPosition());

                // Check if the tenative Gcost is lower than the neighbor node's current g cost. If it is, update the nodes "came from path node" value as well as it's G, H, and F costs
                // if the tenative g cost is lower than the neighbor node's current g cost, that means a better/shorter path was found to the neighbor node
                // example: Before the path was two "straight" movements, like up then right, for a movement cost of 20. The new tenative g cost is a direct diagonal going up/right, for a cost of 14
                if (tentativeGCost < neighborNode.GetGCost())
                {
                    neighborNode.SetCameFromPathNode(currentNode);
                    neighborNode.SetGCost(tentativeGCost);
                    neighborNode.SetHCost(CalculateDistance(neighborNode.GetGridPosition(), endGridPosition));
                    neighborNode.CalculateFCost();

                    // add the neighbor node to the open list of nodes to check
                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }
            }
        }

        // If you are here, no path was found to the end grid position
        //Debug.Log("FindPath: no valid path found");
        pathLength = 0;
        return null;
    }
    public int CalculateDistance(GridPosition a, GridPosition b)
    {
        return GridPosition.CalculateDistance(a, b);
    }
    //public int CalculateDistance(GridPosition a, GridPosition b)
    //{
    //    GridPosition gridPositionDistance = a - b;
    //    int distance = Mathf.Abs(gridPositionDistance.x) + Mathf.Abs(gridPositionDistance.y);

    //    // Get the "x distance" and "z distance." Basically how far do you need to move in the X axis and how far do you move in the Z axis to get from point a to b
    //    int xDistance = Mathf.Abs(gridPositionDistance.x);
    //    int yDistance = Mathf.Abs(gridPositionDistance.y);

    //    // get the distance that will be traveled diagonally by getting the "overlap" between the x and z distances.
    //    // Ex.: If you move to a position that is 1 distance on the x and 2 on the z, then you'd go diagonally 1 time, then straight 1 additional time
    //    // Ex.: if you moved 2 on x, and 5 on z, 
    //    int diagonalDistance = Mathf.Min(xDistance, yDistance);

    //    // Get the remaining "Straight" distance by subtracting the x distance from z distance
    //    int remainingStraightDistance = Mathf.Abs(xDistance - yDistance);

    //    return (diagonalDistance * MOVE_DIAGONAL_COST) + (remainingStraightDistance * MOVE_STRAIGHT_COST);
    //}
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

        // get grid position of current node
        GridPosition gridPosition = currentNode.GetGridPosition();

        // Check if the "Left" neighbors will be valid by checking if gridPosition.x is greater than 0
        if (gridPosition.x > 0)
        {
            // get the node directly to the left of the current node by subtracting 1 from the current node's .x position
            neighborList.Add(GetNode(gridPosition.x - 1, gridPosition.y + 0));

            // Diagonal nodes
            // make sure the "up" node is not at the highest possible value
            if (gridPosition.y + 1 < _gridSystem.GetHeight())
            {
                // Left Up
                neighborList.Add(GetNode(gridPosition.x - 1, gridPosition.y + 1));
            }
            // Make sure the "down" nodes are not below 0
            if (gridPosition.y > 0)
            {
                // Left Down
                neighborList.Add(GetNode(gridPosition.x - 1, gridPosition.y - 1));
            }
        }

        // check if the "Right" neighbors will be valid by checking if gridposition.x + 1 is less than the width (+1 because the if the width is 10, the highest x value for the grid will be 9 since grid coordinates start at 0)
        if (gridPosition.x + 1 < _gridSystem.GetWidth())
        {
            // right node
            neighborList.Add(GetNode(gridPosition.x + 1, gridPosition.y + 0));

            // diagonals
            if (gridPosition.y + 1 < _gridSystem.GetHeight())
            {
                // right Up
                neighborList.Add(GetNode(gridPosition.x + 1, gridPosition.y + 1));
            }
            // Make sure the "down" nodes are not below 0
            if (gridPosition.y > 0)
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
        if (gridPosition.y > 0)
        {
            neighborList.Add(GetNode(gridPosition.x + 0, gridPosition.y - 1));
        }

        return neighborList;
    }
    private List<GridPosition> CalculatePath(PathNode endNode)
    {
        List<PathNode> pathNodeList = new List<PathNode>();
        pathNodeList.Add(endNode);

        PathNode currentNode = endNode;

        // The starting node will have a "null" value for _cameFromPathNode, so you know you've reached the end of the path when the current node has a null _cameFromPathNode value
        while (currentNode.GetCameFromPathNode() != null)
        {
            // Add the _cameFromPathNode value from the current node to the path list. Then, set the current node to that same _cameFromPathNode value
            pathNodeList.Add(currentNode.GetCameFromPathNode());
            currentNode = currentNode.GetCameFromPathNode();
        }

        // reverse the path node list since it currently has the end node at position 0
        pathNodeList.Reverse();

        // convert the path nodes to grid positions
        List<GridPosition> gridPositionList = new List<GridPosition>();
        foreach (PathNode pathNode in pathNodeList)
        {
            gridPositionList.Add(pathNode.GetGridPosition());
        }

        return gridPositionList;
    }
    public bool IsWalkableGridPosition(GridPosition gridPosition)
    {
        return _gridSystem.GetGridObject(gridPosition).IsWalkable();
    }
    public void SetIsWalkableGridPosition(GridPosition gridPosition, bool isWalkable)
    {
        _gridSystem.GetGridObject(gridPosition).SetIsWalkable(isWalkable);
    }
    public bool HasPath(GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength)
    {
        return FindPath(startGridPosition, endGridPosition, out pathLength) != null;
    }
    public int GetPathLength(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        FindPath(startGridPosition, endGridPosition, out int pathLength);
        return pathLength;
    }
}
