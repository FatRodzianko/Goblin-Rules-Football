using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GridSystem<TGridObject>
{
    private int _width;
    private int _height;
    private float _cellSize;
    private TGridObject[,] _gridObjectArray;

    public GridSystem(int width, int height, float cellsize, Func<GridSystem<TGridObject>, GridPosition, TGridObject> createGridObject)
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
                _gridObjectArray[x, y] = createGridObject(this, gridPosition);
            }
        }
    }
    public Vector3 GetWorldPosition(GridPosition gridPosition)
    {
        return new Vector3(gridPosition.x, gridPosition.y, 0) * _cellSize;
    }
    public GridPosition GetGridPositon(Vector3 worldPosition)
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
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                GridPosition gridPosition = new GridPosition(x, y);

                Transform debugTransform = GameObject.Instantiate(debugPrefab, GetWorldPosition(gridPosition), Quaternion.identity);
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
}
