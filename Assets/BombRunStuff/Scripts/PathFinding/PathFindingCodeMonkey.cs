using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

public class PathFindingCodeMonkey : MonoBehaviour
{

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private void Start()
    {

    }
    public void FindPath(int width, int height, GridPosition startPostion, GridPosition endPosition, int maxMoveDistance = int.MaxValue)
    {
        float startTime = Time.realtimeSinceStartup;

        //int findPathJobCount = 5;
        //NativeArray<JobHandle> jobHandleArray = new NativeArray<JobHandle>(findPathJobCount, Allocator.TempJob);

        //for (int i = 0; i < findPathJobCount; i++)
        //{
        //    FindPathJob findPathJob = new FindPathJob
        //    {
        //        startPosition = new int2(startPostion.x, startPostion.y),
        //        endPosition = new int2(endPosition.x, endPosition.y),
        //        gridSize = new int2(width, height),
        //        maxMoveDistance = maxMoveDistance
        //    };
        //    jobHandleArray[i] = findPathJob.Schedule();
        //}

        //JobHandle.CompleteAll(jobHandleArray);
        //jobHandleArray.Dispose();

        FindPathJob findPathJob = new FindPathJob
        {
            startPosition = new int2(startPostion.x, startPostion.y),
            endPosition = new int2(endPosition.x, endPosition.y),
            gridSize = new int2(width, height),
            maxMoveDistance = maxMoveDistance
        };
        //FindPathJob findPathJob = new FindPathJob
        //{
        //    startPosition = new int2(0, 0),
        //    endPosition = new int2(19,19),
        //    gridSize = new int2(20, 20),
        //    maxMoveDistance = maxMoveDistance
        //};
        JobHandle handle = findPathJob.Schedule();
        handle.Complete();

        Debug.Log("PathfindingCodeMonkey: Time: " + ((Time.realtimeSinceStartup - startTime) * 1000f));
    }

    [BurstCompile]
    private struct FindPathJob : IJob
    {

        public int2 startPosition;
        public int2 endPosition;

        public int2 gridSize;

        public int maxMoveDistance;

        public void Execute()
        {

            NativeArray<PathNode> PathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    PathNode PathNode = new PathNode();
                    PathNode.x = x;
                    PathNode.y = y;
                    PathNode.index = CalculateIndex(x, y, gridSize.x);

                    PathNode.gCost = int.MaxValue;
                    //PathNode.hCost = CalculateDistanceCost(new int2(x, y), endPosition);
                    PathNode.hCost = 0;
                    PathNode.CalculateFCost();

                    PathNode.isWalkable = true;
                    //PathNode.isWalkable = PathFinding.Instance.IsPathNodeWalkable(x, y);
                    PathNode.isClosed = false;
                    PathNode.isOpen = false;
                    PathNode.cameFromNodeIndex = -1;

                    PathNodeArray[PathNode.index] = PathNode;
                }
            }


            NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Temp);
            neighbourOffsetArray[0] = new int2(-1, 0); // Left
            neighbourOffsetArray[1] = new int2(+1, 0); // Right
            neighbourOffsetArray[2] = new int2(0, +1); // Up
            neighbourOffsetArray[3] = new int2(0, -1); // Down
            neighbourOffsetArray[4] = new int2(-1, -1); // Left Down
            neighbourOffsetArray[5] = new int2(-1, +1); // Left Up
            neighbourOffsetArray[6] = new int2(+1, -1); // Right Down
            neighbourOffsetArray[7] = new int2(+1, +1); // Right Up

            int endNodeIndex = CalculateIndex(endPosition.x, endPosition.y, gridSize.x);

            PathNode startNode = PathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridSize.x)];
            startNode.gCost = 0;
            startNode.CalculateFCost();
            PathNodeArray[startNode.index] = startNode;

            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            //NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

            openList.Add(startNode.index);

            while (openList.Length > 0)
            {
                int currentNodeIndex = GetLowestCostFNodeIndex(openList, PathNodeArray);
                PathNode currentNode = PathNodeArray[currentNodeIndex];

                if (currentNodeIndex == endNodeIndex)
                {
                    // Reached our destination!
                    break;
                }

                // Remove current node from Open List
                for (int i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == currentNodeIndex)
                    {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }

                // mark the current node as checked?
                currentNode.isClosed = true;

                //closedList.Add(currentNodeIndex);

                for (int i = 0; i < neighbourOffsetArray.Length; i++)
                {
                    int2 neighbourOffset = neighbourOffsetArray[i];
                    int2 neighbourPosition = new int2(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y);

                    if (!IsPositionInsideGrid(neighbourPosition, gridSize))
                    {
                        // Neighbour not valid position
                        continue;
                    }

                    int neighbourNodeIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y, gridSize.x);


                    PathNode neighbourNode = PathNodeArray[neighbourNodeIndex];
                    if (neighbourNode.isClosed)
                    {
                        continue;
                    }
                    if (!neighbourNode.isWalkable)
                    {
                        // Not walkable
                        continue;
                    }

                    // check the distance to the neighbor node from the start position. If it is greater than the max move distance, ignore. Check this by seeing if the X or Y coordinate of the neighbor node is greater than startNode.x/y + max move distance
                    if (math.abs(neighbourNode.x - startPosition.x) > maxMoveDistance)
                    {
                        continue;
                    }
                    if (math.abs(neighbourNode.y - startPosition.y) > maxMoveDistance)
                    {
                        continue;
                    }

                    int2 currentNodePosition = new int2(currentNode.x, currentNode.y);

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePosition, neighbourPosition);
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.cameFromNodeIndex = currentNodeIndex;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.CalculateFCost();
                        PathNodeArray[neighbourNodeIndex] = neighbourNode;

                        //if (!openList.Contains(neighbourNode.index))
                        //{
                        //    openList.Add(neighbourNode.index);
                        //}
                        if (!neighbourNode.isOpen)
                        {
                            openList.Add(neighbourNode.index);
                            neighbourNode.isOpen = true;
                        }
                    }

                }
            }

            PathNode endNode = PathNodeArray[endNodeIndex];
            if (endNode.cameFromNodeIndex == -1)
            {
                // Didn't find a path!
                //Debug.Log("Didn't find a path!");
            }
            else
            {
                // Found a path
                NativeList<int2> path = CalculatePath(PathNodeArray, endNode);
                /*
                foreach (int2 pathPosition in path) {
                    Debug.Log(pathPosition);
                }
                */
                path.Dispose();
            }

            PathNodeArray.Dispose();
            neighbourOffsetArray.Dispose();
            openList.Dispose();
            //closedList.Dispose();
        }

        private NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
        {
            if (endNode.cameFromNodeIndex == -1)
            {
                // Couldn't find a path!
                return new NativeList<int2>(Allocator.Temp);
            }
            else
            {
                // Found a path
                NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
                path.Add(new int2(endNode.x, endNode.y));

                PathNode currentNode = endNode;
                while (currentNode.cameFromNodeIndex != -1)
                {
                    PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                    path.Add(new int2(cameFromNode.x, cameFromNode.y));
                    currentNode = cameFromNode;
                }

                return path;
            }
        }

        private bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize)
        {
            return
                gridPosition.x >= 0 &&
                gridPosition.y >= 0 &&
                gridPosition.x < gridSize.x &&
                gridPosition.y < gridSize.y;
        }

        private int CalculateIndex(int x, int y, int gridWidth)
        {
            return x + y * gridWidth;
        }

        private int CalculateDistanceCost(int2 aPosition, int2 bPosition)
        {
            int xDistance = math.abs(aPosition.x - bPosition.x);
            int yDistance = math.abs(aPosition.y - bPosition.y);
            int remaining = math.abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }


        private int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
        {
            PathNode lowestCostPathNode = pathNodeArray[openList[0]];
            for (int i = 1; i < openList.Length; i++)
            {
                PathNode testPathNode = pathNodeArray[openList[i]];
                if (testPathNode.fCost < lowestCostPathNode.fCost)
                {
                    lowestCostPathNode = testPathNode;
                }
            }
            return lowestCostPathNode.index;
        }

        private struct PathNode
        {
            public int x;
            public int y;

            public int index;

            public int gCost;
            public int hCost;
            public int fCost;

            public bool isWalkable;
            public bool isClosed;
            public bool isOpen;

            public int cameFromNodeIndex;

            public void CalculateFCost()
            {
                fCost = gCost + hCost;
            }

            public void SetIsWalkable(bool isWalkable)
            {
                this.isWalkable = isWalkable;
            }
        }

    }

}
