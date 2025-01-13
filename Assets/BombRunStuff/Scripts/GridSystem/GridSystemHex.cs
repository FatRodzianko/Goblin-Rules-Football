using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GridSystemHex<TGridObject> : GridSystem<TGridObject>
{
    private const float HEX_VERTICAL_OFFSET_MULTIPLIER = 0.75f;
    private const float HEX_HORIZONTAL_ODD_ROW_OFFSET = 0.5f;

    private const int MOVE_STRAIGHT_COST = 10;

    private int _width;
    private int _height;
    private float _cellSize;
    private TGridObject[,] _gridObjectArray;

    public GridSystemHex(int width, int height, float cellsize, Func<GridSystem<TGridObject>, GridPosition, TGridObject> createGridObject) : base(width, height, cellsize, createGridObject)
    {
        this._width = width;
        this._height = height;
        this._cellSize = cellsize;

        this._gridObjectArray = new TGridObject[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                GridPosition gridPosition = new GridPosition(x, y);
                _gridObjectArray[x, y] = createGridObject(this, gridPosition); // using the delegate function createGridObject to create the grid object
            }
        }
    }
    public override Vector3 GetWorldPosition(GridPosition gridPosition)
    {
        float xPostion = gridPosition.x * _cellSize;
        float yPosition = gridPosition.y * _cellSize * HEX_VERTICAL_OFFSET_MULTIPLIER;

        if (gridPosition.y % 2 == 1)
        {
            xPostion += _cellSize * HEX_HORIZONTAL_ODD_ROW_OFFSET;
        }
        return new Vector3(xPostion, yPosition, 0f );
    }
    public override GridPosition GetGridPositon(Vector3 worldPosition)
    {
        // For hex, get "rough" estimate through rounding first
        GridPosition roughXY = new GridPosition(
           Mathf.RoundToInt(worldPosition.x / _cellSize),
           Mathf.RoundToInt(worldPosition.y / _cellSize / HEX_VERTICAL_OFFSET_MULTIPLIER)
           );

        List<GridPosition> neighborGridPositionList = GetNeighborGridPositions(roughXY);


        // test each neighbor to see which is closest to the worldPosition value
        GridPosition closestGridPosition = roughXY;
        float closestDistance = Vector2.Distance(worldPosition, this.GetWorldPosition(roughXY));
        foreach (GridPosition neighborGridPosition in neighborGridPositionList)
        {
            float distance = Vector2.Distance(worldPosition, this.GetWorldPosition(neighborGridPosition));
            if (distance < closestDistance)
            {
                closestGridPosition = neighborGridPosition;
                closestDistance = distance;
            }
        }
        return closestGridPosition;
    }
    public List<GridPosition> GetNeighborGridPositions(GridPosition gridPosition)
    {
        // Check if the roughXZ is in an odd row or not. This is important for finding the positions neighbors
        bool oddRow = gridPosition.y % 2 == 1;
        // Get "neighbors" of the grid position at roughXZ to find the "true" grid position
        List<GridPosition> neighborGridPositionList = new List<GridPosition>
        {
            gridPosition + new GridPosition(-1,0), // left
            gridPosition + new GridPosition(+1,0), // right

            gridPosition + new GridPosition(0,+1), // up. For odd rows, this is up and left. Even, up and right
            gridPosition + new GridPosition(0,-1), // down. For odd rows, this is down and left. Even, down and right

            gridPosition + new GridPosition(oddRow ? +1 : -1 ,+1), // up. For odd rows, this is up and right. If even, up and left
            gridPosition + new GridPosition(oddRow ? +1 : -1 ,-1), // down. For odd rows, this is down and right. If even, down and left

        };

        return neighborGridPositionList;
    }
    public override List<GridPosition> GetNeighborGridPositions(GridPosition startingGridPosition, int distanceFromStartingPosition, bool makeCircular = false)
    {
        List<GridPosition> neighborGridPositions = GetNeighborGridPositions(startingGridPosition);       

        if (distanceFromStartingPosition > 1)
        {
            List<GridPosition> mostRecentNeighbors = new List<GridPosition>();
            mostRecentNeighbors.AddRange(neighborGridPositions);

            for (int i = 1; i < distanceFromStartingPosition; i++)
            {
                // used to track neighbors to look at for next turn
                List<GridPosition> newRecentNeighbors = new List<GridPosition>();
                foreach (GridPosition recentNeighbor in mostRecentNeighbors)
                {
                    List<GridPosition> newNeighbors = GetNeighborGridPositions(recentNeighbor);
                    foreach (GridPosition newNeighbor in newNeighbors)
                    {
                        if (!neighborGridPositions.Contains(newNeighbor))
                        {
                            neighborGridPositions.Add(newNeighbor);
                            newRecentNeighbors.Add(newNeighbor);
                        }
                    }
                }
                mostRecentNeighbors.Clear();
                mostRecentNeighbors.AddRange(newRecentNeighbors);
            }
        }        
        return neighborGridPositions;
    }
    public override int CalculateDistance(GridPosition a, GridPosition b)
    {
        return Mathf.RoundToInt(MOVE_STRAIGHT_COST * Vector3.Distance(GetWorldPosition(a), GetWorldPosition(b)));
    }
}
