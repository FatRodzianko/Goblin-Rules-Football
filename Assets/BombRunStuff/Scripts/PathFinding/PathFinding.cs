using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using System;

public class PathFinding : MonoBehaviour
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    public static PathFinding Instance { get; private set; }

    [SerializeField] private Transform _gridDebugObjectPrefab;
    protected int _width;
    protected int _height;
    protected float _cellSize;

    // grid system
    protected GridSystem<PathNode> _gridSystem;
    [SerializeField] protected int _pathFindingDistanceMultiplier = 10;

    // cache modified nodes from FindPath
    List<PathNode> _modifiedNodes = new List<PathNode>();


    //// DOTS grid system?
    //[SerializeField] PathFindingCodeMonkey _pathfindingCodeMonkey;
    //[SerializeField] PathFindingFindValidPositionJob _pathFindingFindValidPositionJob;

    // events
    public event EventHandler<GridPosition> IsWalkableUpdated;

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
    public virtual void Setup(int width, int height, float cellSize)
    {
        this._width = width;
        this._height = height;
        this._cellSize = cellSize;

        _gridSystem = new GridSystem<PathNode>(_width, _height, _cellSize,
            (GridSystem<PathNode> g, GridPosition gridPosition) => new PathNode(gridPosition));

        //_gridSystem.CreateDebugObjects(_gridDebugObjectPrefab);

        InitializeIsWalkable();
        float startTime = Time.realtimeSinceStartup;
        List<GridPosition> testPath = FindPathDots(new GridPosition(1, 1), new GridPosition(2, 17), out int pathLength);
        Debug.Log("PathFinding: Time: Setup: Dots: " + ((Time.realtimeSinceStartup - startTime) * 1000f) + " path length: " + pathLength);
        //string pathString = "";
        //for (int i = 0; i < testPath.Count; i++)
        //{
        //    pathString += i.ToString() + ": " + testPath[i].ToString() + " ";
        //}
        ////Debug.Log("PathFinding: Setup test path: " + pathString);

        testPath.Clear();
        startTime = Time.realtimeSinceStartup;
        testPath = FindPath(new GridPosition(1, 1), new GridPosition(2, 17), out pathLength);
        Debug.Log("PathFinding: Time: Setup: Not-Dots: " + ((Time.realtimeSinceStartup - startTime) * 1000f) + " path length: " + pathLength);

        //_pathfindingCodeMonkey.FindPath(this._width, this._height, new GridPosition(1, 1), new GridPosition(2, 17), int.MaxValue, _gridSystem);

    }
    protected void InitializeIsWalkable()
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
                    if (LevelGrid.Instance.GetObstacleAtGridPosition(gridPosition).IsWalkable())
                    {
                        GetNode(x, y).SetIsWalkable(true);
                    }
                    else
                    {
                        GetNode(x, y).SetIsWalkable(false);
                    }
                }
                else
                {
                    GetNode(x, y).SetIsWalkable(true);
                }
                PathNode newNode = GetNode(x, y);
                ResetPathNode(newNode);
                List<PathNode> neighborList = GetNeighborList(newNode);
                newNode.SetNeighborList(neighborList);
                foreach (PathNode neighborNode in neighborList)
                {
                    newNode.AddNeighborToNeighborNodeCostDictionary(neighborNode, CalculateDistance(gridPosition, neighborNode.GetGridPosition()));
                }
            }
        }
    }
    public virtual List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength, int maxMoveDistance = int.MaxValue)
    {
        if (startGridPosition == endGridPosition)
        {
            pathLength = 0;
            return null;
        }
        if (!LevelGrid.Instance.IsValidGridPosition(endGridPosition))
        {
            //Debug.Log("FindPath: " + endGridPosition.ToString() + " is not a valid position");
            pathLength = 0;
            return null;
        }

        List<PathNode> openList = new List<PathNode>(); // used for nodes waiting to be searched
        //List<PathNode> closedList = new List<PathNode>(); // used for nodes that have already been searched
        

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

        // cycle through all nodes and reset their state
        // todo: instead of iterating through every possible node and resetting it, instead track what nodes were updated in FindPath
        //       then, go through the list of updated nodes and reset those
        //for (int x = 0; x < _gridSystem.GetWidth(); x++)
        //{
        //    for (int y = 0; y < _gridSystem.GetHeight(); y++)
        //    {

        //        GridPosition gridPosition = new GridPosition(x, y);
        //        PathNode pathNode = _gridSystem.GetGridObject(gridPosition);

        //        ResetPathNode(pathNode);
        //        //// set the g cost to infinite and h cost to 0
        //        //pathNode.SetGCost(int.MaxValue);
        //        //pathNode.SetHCost(0);
        //        //// calculate the f cost with new g and h values
        //        //pathNode.CalculateFCost();

        //        //// reset the "came from node" value
        //        //pathNode.ResetCameFromPathNode();
        //    }
        //}
        ResetModifiedNodes(_modifiedNodes);
        _modifiedNodes.Clear();

        // set up the starting node's values
        startNode.SetGCost(0);
        startNode.SetHCost(CalculateDistance(startGridPosition, endGridPosition));
        startNode.CalculateFCost();

        _modifiedNodes.Add(startNode);
        _modifiedNodes.Add(endNode);

        openList.Add(startNode);
        //SortedDictionary<int, PathNode> openListDictionary = new SortedDictionary<int, PathNode>();
        //openListDictionary.Add(startNode.GetFCost(), startNode);

        // search for a path until the openList count is 0
        while (openList.Count > 0)
        {
            // set the current node to the node in the list with the lowest f cost. The node with the lowest f cost is what we'd want to prioritize in path calculations?
            PathNode currentNode = GetLowestFCostPathNode(openList);
            // instead of this, create a "sorted dictionary" thats SortedDictionary<int, PathNode> where the int is the f score of the node. This way, the node with the lowest f score will always be first entry in the dictionary?
            //int firstKey = openListDictionary.Keys.First();
            //PathNode currentNode = openListDictionary[firstKey];

            // check if this is the current node is at the end node
            if (currentNode == endNode)
            {
                // reach final node
                pathLength = endNode.GetFCost();
                //pathLength = endNode.GetGCost();
                return CalculatePath(endNode);
            }

            // remove current node from openList and add to closeList
            openList.Remove(currentNode);
            //openListDictionary.Remove(firstKey);

            //closedList.Add(currentNode);
            currentNode.SetIsClosed(true);

            // Get Neighbors to search through
            // consider "caching" neighbor nodes in the nodes themselves so this doesn't need to be recalculated every time? When the path nodes are created, get all the neighbor nodes for each node?
            //foreach (PathNode neighborNode in GetNeighborList(currentNode))
            foreach (PathNode neighborNode in currentNode.GetNeighborNodes())
            {
                // check if the neighborNode is already in closed list. If it is, it was already searched and can be skippeed
                //if (closedList.Contains(neighborNode))
                //{
                //    continue;
                //}
                if (neighborNode.IsClosed())
                {
                    continue;
                }
                // check if the node is walkable. If not, it is not a valid point on the path
                if (!neighborNode.IsWalkable())
                {
                    //closedList.Add(neighborNode);
                    //neighborNode.SetWasChecked(true);
                    continue;
                }

                // check the distance to the neighbor node from the start position. If it is greater than the max move distance, ignore. Check this by seeing if the X or Y coordinate of the neighbor node is greater than startNode.x/y + max move distance
                int neighborX = neighborNode.GetGridPosition().x;
                int neighborY = neighborNode.GetGridPosition().y;
                if (MathF.Abs(neighborX - startGridPosition.x) > maxMoveDistance)
                {
                    continue;
                }
                if (MathF.Abs(neighborY - startGridPosition.y) > maxMoveDistance)
                {
                    continue;
                }

                // Get the tenative G cost of the neighbor node by taking the current node's g cost, and then adding the distance to the neighbor node.
                // this should be currentNode.gCost + 10 if it is adjacent to the current node, and currentNode.gCost + 14 if it is diagonal to the current node
                //int tentativeGCost = currentNode.GetGCost() + CalculateDistance(currentNode.GetGridPosition(), neighborNode.GetGridPosition());
                int tentativeGCost = GetTenativeGCost(currentNode,neighborNode);

                


                // Check if the tenative Gcost is lower than the neighbor node's current g cost. If it is, update the nodes "came from path node" value as well as it's G, H, and F costs
                // if the tenative g cost is lower than the neighbor node's current g cost, that means a better/shorter path was found to the neighbor node
                // example: Before the path was two "straight" movements, like up then right, for a movement cost of 20. The new tenative g cost is a direct diagonal going up/right, for a cost of 14
                if (tentativeGCost < neighborNode.GetGCost())
                {
                    neighborNode.SetCameFromPathNode(currentNode);
                    neighborNode.SetGCost(tentativeGCost);
                    neighborNode.SetHCost(CalculateDistance(neighborNode.GetGridPosition(), endGridPosition));
                    neighborNode.CalculateFCost();                    

                    //if (!_modifiedNodes.Contains(neighborNode))
                    //    _modifiedNodes.Add(neighborNode);
                    _modifiedNodes.Add(neighborNode);

                    // The fcost is the distance from the start node to this node. If fcost is greater than max move distance, player can't get to this node
                    // if the player can't get to this node, there is no reason to add to openlist to check?
                    if (neighborNode.GetFCost() > maxMoveDistance * 10)
                    {
                        continue;
                    }

                    if (!neighborNode.IsOpen())
                    {
                        openList.Add(neighborNode);

                        //try
                        //{
                        //    openListDictionary.Add(neighborNode.GetFCost(), neighborNode);
                        //}
                        //catch (Exception e)
                        //{
                        //    Debug.Log("FindPath: Exception writing to sorted dictionary for key: " + neighborNode.GetFCost().ToString() + ". Error: " + e);
                        //}

                        neighborNode.SetIsOpen(true);
                    }
                }
            }
        }

        // If you are here, no path was found to the end grid position
        //Debug.Log("FindPath: no valid path found");
        if (endGridPosition.x == 4 && endGridPosition.y == 4)
        {
            Debug.Log("FindPath: No path found to: " + endGridPosition.ToString() + " pathlength was: " + GetNode(endGridPosition.x,endGridPosition.y).GetFCost().ToString());
        }
        ResetModifiedNodes(_modifiedNodes);
        pathLength = 0;
        return null;
    }
    public virtual int GetTenativeGCost(PathNode currentNode, PathNode neighborNode)
    {
        return currentNode.GetGCost() + CalculateDistance(currentNode.GetGridPosition(), neighborNode.GetGridPosition());
        //return currentNode.GetGCost() + currentNode.GetNeighborNodeCostDictionary()[neighborNode];
    }
    public virtual int CalculateDistance(GridPosition a, GridPosition b)
    {
        //return LevelGrid.Instance.CalculateDistance(a, b);
        int xDistance = math.abs(a.x - b.x);
        int yDistance = math.abs(a.y - b.y);
        int remaining = math.abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
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
    protected PathNode GetLowestFCostPathNode(List<PathNode> pathNodeList)
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
    protected PathNode GetLowestFCostPathNode(HashSet<PathNode> pathNodeList)
    {
        return pathNodeList.OrderBy(x => x.GetFCost()).First();
    }
    protected PathNode GetNode(int x, int y)
    {
        return _gridSystem.GetGridObject(new GridPosition(x, y));
    }
    public virtual List<PathNode> GetNeighborList(PathNode currentNode)
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
    //private List<PathNode> GetNeighborList(PathNode currentNode)
    //{
    //    List<PathNode> neighborList = new List<PathNode>();

    //    // get grid position of current node
    //    GridPosition gridPosition = currentNode.GetGridPosition();

    //    List<GridPosition> neighborGridPositions = _gridSystem.GetNeighborGridPositions(gridPosition, 1, false);
    //    foreach (GridPosition neighborGridPosition in neighborGridPositions)
    //    {
    //        if (!_gridSystem.IsValidGridPosition(neighborGridPosition))
    //            continue;
    //        neighborList.Add(GetNode(neighborGridPosition.x, neighborGridPosition.y));
    //    }

    //    return neighborList;
    //}
    protected List<GridPosition> CalculatePath(PathNode endNode)
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
        IsWalkableUpdated?.Invoke(this, gridPosition);
    }
    public int GetGridWidth()
    {
        return _width;
    }
    public int GetGridHeight()
    {
        return _height;
    }
    public Vector2 GetGridSize()
    {
        return new Vector2(_width, _height);
    }
    public int GetPathFindingDistanceMultiplier()
    {
        return _pathFindingDistanceMultiplier;
    }
    public void SetPathFindingDistanceMultiplier(int multiplier)
    {
        this._pathFindingDistanceMultiplier = multiplier;
    }
    public bool HasPath(GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength, int maxMoveDistance = int.MaxValue)
    {
        return FindPath(startGridPosition, endGridPosition, out pathLength, maxMoveDistance) != null;
        //return FindPathDots(startGridPosition, endGridPosition, out pathLength) != null;
    }
    public int GetPathLength(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        FindPath(startGridPosition, endGridPosition, out int pathLength);
        //FindPathDots(startGridPosition, endGridPosition, out int pathLength);
        return pathLength;
    }
    void ResetModifiedNodes(List<PathNode> modifiedNodes)
    {
        if (modifiedNodes.Count == 0)
            return;


        for (int i = 0, n = modifiedNodes.Count; i < n; i++)
        {
            ResetPathNode(modifiedNodes[i]);
        }
    }
    void ResetPathNode(PathNode pathNode)
    {
        // set the g cost to infinite and h cost to 0
        pathNode.SetGCost(int.MaxValue);
        pathNode.SetHCost(0);
        // calculate the f cost with new g and h values
        pathNode.CalculateFCost();

        // reset the "came from node" value
        pathNode.ResetCameFromPathNode();
        pathNode.SetIsClosed(false);
        pathNode.SetIsOpen(false);
    }
    public bool IsPathNodeWalkable(int x, int y)
    {
        if (!LevelGrid.Instance.IsValidGridPosition(new GridPosition(x, y)))
            return false;
        return GetNode(x, y).IsWalkable();
    }
    //public List<GridPosition> FindValidPositionsFromList(List<GridPosition> gridPositions, int maxDistance, GridPosition startPosition)
    //{
    //    List<GridPosition> validGridPostions = _pathFindingFindValidPositionJob.GetValidGridPositionsJob(gridPositions, maxDistance, startPosition);

    //    return validGridPostions;
    //}
    public virtual List<GridPosition> FindPathDots(GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength)
    {
        if (!LevelGrid.Instance.IsValidGridPosition(endGridPosition))
        {
            Debug.Log("FindPathDots: " + endGridPosition.ToString() + " is not a valid position");
            pathLength = 0;
            return null;
        }

        int2 startPosition = new int2(startGridPosition.x, startGridPosition.y);
        int2 endPosition = new int2(endGridPosition.x, endGridPosition.y);

        // create the grid?
        int2 gridSize = new int2(_gridSystem.GetWidth(), _gridSystem.GetHeight());
        // store path nodes in a nativearray
        NativeArray<PathNodeDots> pathNodeArray = new NativeArray<PathNodeDots>(gridSize.x * gridSize.y, Allocator.Temp);

        //create path nodes for the grid and initialize values
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                PathNodeDots pathNode = new PathNodeDots();
                pathNode.x = x;
                pathNode.y = y;
                pathNode.index = CalculateDotsNodeIndex(x, y, gridSize.x);

                // initialize path cost values
                pathNode.gCost = int.MaxValue;
                pathNode.hCost = 0;
                //pathNode.hCost = 0;
                pathNode.CalculateFCost();

                pathNode.isWalkable = _gridSystem.GetGridObject(new GridPosition(x, y)).IsWalkable();
                pathNode.wasChecked = false;

                pathNode.cameFromNodeIndex = -1;

                // insert the pathNode into the NativeArray / FlatArray
                pathNodeArray[pathNode.index] = pathNode;
            }
        }

        // get the end node's index
        int endNodeIndex = CalculateDotsNodeIndex(endPosition.x, endPosition.y, gridSize.x);
        PathNodeDots endNode = pathNodeArray[endNodeIndex];
        // check if the endnode is walkable. If it isn't cannot find path to the node. Return null
        if (!endNode.isWalkable)
        {
            Debug.Log("FindPathDots: " + endGridPosition.ToString() + " is not walkable");
            pathLength = 0;
            return null;
        }

        // get the starting node
        PathNodeDots startNode = pathNodeArray[CalculateDotsNodeIndex(startPosition.x, startPosition.y, gridSize.x)];
        // reset the startNode's gCost to 0
        startNode.gCost = 0;
        startNode.CalculateFCost();
        // the modifications above were to a copy of the data in the array. The array at the node's index needs to be updated
        pathNodeArray[startNode.index] = startNode;

        // create the open and closed lists to track what has been checked in the path calculation
        // the lists will be the int index value of path nodes in the pathNodeArray. It is a list of ints, not pathnodes
        NativeList<int> openList = new NativeList<int>(Allocator.Temp);
        NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

        // add the start node's index to the open list
        openList.Add(startNode.index);

        // loop through the openList to find the path!
        while (openList.Length > 0)
        {
            int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodeArray);
            PathNodeDots currentNode = pathNodeArray[currentNodeIndex];

            // check to see if the current node is the end node in the path meaning you've reached the destination
            if (currentNodeIndex == endNodeIndex)
            {
                // reached final node
                pathLength = currentNode.fCost;
                //Debug.Log("FindPathDots: Path found! to: " + endGridPosition.ToString() + " with a path length of: " + pathLength.ToString() + " . end node postion: " + currentNode.x + "," + currentNode.y + " gcost: " + currentNode.gCost + " fcost: " + currentNode.fCost + " hcost: " + currentNode.hCost);
                return CalculatePathDots(pathNodeArray, currentNode);
            }

            // did not reach destination so keep searching

            // remove the current node from the open list
            for (int i = 0; i < openList.Length; i++)
            {
                if (openList[i] == currentNodeIndex)
                {
                    openList.RemoveAtSwapBack(i);
                    break;
                }
            }

            // mark the current node as checked?
            currentNode.wasChecked = true;
            pathNodeArray[currentNodeIndex] = currentNode;

            // add current node to the closed list so it is not searched again
            closedList.Add(currentNodeIndex);

            // Get the neighbors of the current node
            NativeArray<int2> neighborOffsetArray = GetNeighborOffsetArray(currentNode);

            // cycle through neighbor offsets and get the neighbor node positions
            for (int i = 0; i < neighborOffsetArray.Length; i++)
            {
                int2 neighborOffset = neighborOffsetArray[i];
                int2 neighborPosition = new int2(currentNode.x + neighborOffset.x, currentNode.y + neighborOffset.y);

                // make sure the neighbor position is a valid position in the grid. If it is not in the grid, skip
                if (!IsPositionInsideGrid(neighborPosition, gridSize))
                    continue;

                // get the index of the neighbor node. If the index is in the closed list, skip as that node has already been checked
                int neighborNodeIndex = CalculateDotsNodeIndex(neighborPosition.x, neighborPosition.y, gridSize.x);
                //if (closedList.Contains(neighborNodeIndex))
                //{
                //    continue;
                //}

                // check if the neighbor node is walkable. If not, skip
                PathNodeDots neighborNode = pathNodeArray[neighborNodeIndex];
                if (neighborNode.wasChecked)
                {
                    continue;
                }

                if (!neighborNode.isWalkable)
                {
                    continue;
                }

                // Check if the tenative Gcost is lower than the neighbor node's current g cost. If it is, update the nodes "came from path node" value as well as it's G, H, and F costs
                // if the tenative g cost is lower than the neighbor node's current g cost, that means a better/shorter path was found to the neighbor node
                // example: Before the path was two "straight" movements, like up then right, for a movement cost of 20. The new tenative g cost is a direct diagonal going up/right, for a cost of 14
                //int2 currentNodePosition = new int2(currentNode.x, currentNode.y);
                int tenativeGCost = GetTenativeGCostDots(currentNode, neighborNode);
                if (tenativeGCost < neighborNode.gCost)
                {
                    neighborNode.cameFromNodeIndex = currentNodeIndex;
                    neighborNode.gCost = tenativeGCost;
                    neighborNode.CalculateFCost();
                    // remember to update the struct that is at the neighbor node's index with the values that were just updated. So far all changes were made to a copy of that struct
                    pathNodeArray[neighborNodeIndex] = neighborNode;


                    // add the neighbor noded to the openlist so its neighbor's will be checked
                    if (!openList.Contains(neighborNodeIndex))
                    {
                        openList.Add(neighborNode.index);
                    }
                }
            }
            neighborOffsetArray.Dispose();
        }
        // native arrays need to be disposed
        pathNodeArray.Dispose();
        openList.Dispose();
        closedList.Dispose();

        // if you're here, no path was found D:

        pathLength = 0;
        return null;
        
    }
    private List<GridPosition> CalculatePathDots(NativeArray<PathNodeDots> pathNodeArray, PathNodeDots endNode)
    {
        if (endNode.cameFromNodeIndex == -1)
        {
            return null;
        }

        // starting from the end node, "walk backwards" to find the path
        // get the end node's cameFromNodeIndex. Get node at that index. Then check that node's cameFromNodeIndex and so on
        // go until you hit a node with a cameFromNodeIndex of -1. This will be the start node
        NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
        path.Add(new int2(endNode.x, endNode.y));

        PathNodeDots currentNode = endNode;
        while (currentNode.cameFromNodeIndex != -1)
        {
            PathNodeDots cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
            path.Add(new int2(cameFromNode.x, cameFromNode.y));
            currentNode = cameFromNode;
        }
        List<GridPosition> pathGridPositions = ConvertPathNodeDotsToGridPositions(path);
        path.Dispose();
        return pathGridPositions;
    }
    private List<GridPosition> ConvertPathNodeDotsToGridPositions(NativeList<int2> path)
    {
        List<GridPosition> pathGridPositions = new List<GridPosition>();

        for (int i = 0; i < path.Length; i++)
        {
            pathGridPositions.Add(new GridPosition(path[i].x, path[i].y));
        }

        pathGridPositions.Reverse();
        return pathGridPositions;
    }
    private int CalculateDotsNodeIndex(int x, int y, int gridWidth)
    {
        // convert x and y position to a "flat index"
        return x + y * gridWidth;
    }
    private int CalculateDistanceDots(int2 aPosition, int2 bPosition)
    {
        int xDistance = math.abs(aPosition.x - bPosition.x);
        int yDistance = math.abs(aPosition.y - bPosition.y);
        int remaining = math.abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }
    private int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNodeDots> pathNodeArray)
    {
        PathNodeDots lowestCostPathNode = pathNodeArray[openList[0]];

        for (int i = 1; i < openList.Length; i++)
        {
            PathNodeDots testPathNode = pathNodeArray[openList[i]];
            if (testPathNode.fCost < lowestCostPathNode.fCost)
            {
                lowestCostPathNode = testPathNode;
            }
        }

        return lowestCostPathNode.index;
    }
    private NativeArray<int2> GetNeighborOffsetArray(PathNodeDots pathNode)
    {
        NativeArray<int2> neighborOffsetArray = new NativeArray<int2>(8, Allocator.Temp);
        neighborOffsetArray[0] = new int2(-1, 0); // Left
        neighborOffsetArray[1] = new int2(+1, 0); // Right
        neighborOffsetArray[2] = new int2(0, +1); // Up
        neighborOffsetArray[3] = new int2(0, -1); // Down
        neighborOffsetArray[4] = new int2(-1, -1); // Left Down
        neighborOffsetArray[5] = new int2(-1, +1); // Left Up
        neighborOffsetArray[6] = new int2(+1, -1); // Right Down
        neighborOffsetArray[7] = new int2(+1, +1); // Right Up

        return neighborOffsetArray;
    }
    private bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize)
    {
        return
            gridPosition.x >= 0 &&
            gridPosition.y >= 0 &&
            gridPosition.x < gridSize.x &&
            gridPosition.y < gridSize.y;
    }
    private int GetTenativeGCostDots(PathNodeDots currentNode, PathNodeDots neighborNode)
    {
        return currentNode.gCost + CalculateDistanceDots(new int2(currentNode.x, currentNode.y), new int2(neighborNode.x, neighborNode.y));
    }
    private struct PathNodeDots {
        public int x;
        public int y;

        public int index;

        public int gCost;
        public int hCost;
        public int fCost;

        public bool isWalkable;
        public bool wasChecked;

        public int cameFromNodeIndex;

        public void CalculateFCost()
        {
            fCost = gCost + hCost ;
        }
        public int GetXPosition()
        {
            return x;
        }
    }


}
