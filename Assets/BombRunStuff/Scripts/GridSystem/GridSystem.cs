using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GridSystem<TGridObject>
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private int _width;
    private int _height;
    private float _cellSize;
    private TGridObject[,] _gridObjectArray;
   

    public GridSystem(int width, int height, float cellsize, Func<GridSystem<TGridObject>, GridPosition, TGridObject> createGridObject) // the "Func" thing is a delegate function. Receives a GridSystem of type TGridObject, a GridPosition, and returns a TGridObject. createGridObject is the name of the delegate
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
    public virtual Vector3 GetWorldPosition(GridPosition gridPosition)
    {
        return new Vector3(gridPosition.x, gridPosition.y, 0) * _cellSize;
    }
    public virtual GridPosition GetGridPositon(Vector3 worldPosition)
    {
        return new GridPosition(
            Mathf.RoundToInt(worldPosition.x / _cellSize),
            Mathf.RoundToInt(worldPosition.y / _cellSize)
            );
    }
    public int GetWidth()
    {
        return _width;
    }
    public int GetHeight()
    {
        return _height;
    }
    public void CreateDebugObjects(Transform debugPrefab)
    {
        Transform debugObjectHolder = GameObject.FindGameObjectWithTag("DebugObjectHolder").transform;
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                GridPosition gridPosition = new GridPosition(x, y);

                Transform debugTransform = GameObject.Instantiate(debugPrefab, GetWorldPosition(gridPosition), Quaternion.identity);
                debugTransform.parent = debugObjectHolder;
                GridDebugObject gridDebugObject = debugTransform.GetComponent<GridDebugObject>();
                gridDebugObject.SetGridObject(GetGridObject(gridPosition));
                gridDebugObject.SetDebugText();
            }
        }
    }
    public TGridObject GetGridObject(GridPosition gridPosition)
    {
        return _gridObjectArray[gridPosition.x, gridPosition.y];
    }
    public bool IsValidGridPosition(GridPosition gridPosition)
    {
        return gridPosition.x >= 0 && 
            gridPosition.y >= 0 && 
            gridPosition.x < _width && 
            gridPosition.y < _height;
    }
    public virtual List<GridPosition> GetNeighborGridPositions(GridPosition startingGridPosition, int distanceFromStartingPosition, bool makeCircular = false)
    {
        List<GridPosition> neighborGridPositions = new List<GridPosition>();

        for (int x = -distanceFromStartingPosition; x <= distanceFromStartingPosition; x++)
        {
            for (int y = -distanceFromStartingPosition; y <= distanceFromStartingPosition; y++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, y);
                GridPosition newNeighborPosition = startingGridPosition + offsetGridPosition;

                // only needed for square grid. Hex grid won't need this?
                if (makeCircular)
                {
                    int testDistance = Mathf.Abs(x) + Mathf.Abs(y);
                    if (testDistance > distanceFromStartingPosition)
                    {
                        continue;
                    }
                }

                neighborGridPositions.Add(newNeighborPosition);
            }
        }
        return neighborGridPositions;
    }
    public virtual int CalculateDistance(GridPosition a, GridPosition b)
    {
        GridPosition gridPositionDistance = a - b;

        // Get the "x distance" and "z distance." Basically how far do you need to move in the X axis and how far do you move in the Z axis to get from point a to b
        int xDistance = Mathf.Abs(gridPositionDistance.x);
        int yDistance = Mathf.Abs(gridPositionDistance.y);

        // get the distance that will be traveled diagonally by getting the "overlap" between the x and z distances.
        // Ex.: If you move to a position that is 1 distance on the x and 2 on the z, then you'd go diagonally 1 time, then straight 1 additional time
        // Ex.: if you moved 2 on x, and 5 on z, 
        int diagonalDistance = Mathf.Min(xDistance, yDistance);

        // Get the remaining "Straight" distance by subtracting the x distance from z distance
        int remainingStraightDistance = Mathf.Abs(xDistance - yDistance);

        return (diagonalDistance * MOVE_DIAGONAL_COST) + (remainingStraightDistance * MOVE_STRAIGHT_COST);
    }
}
