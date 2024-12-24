using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GridSystemHex<TGridObject> : GridSystem<TGridObject>
{
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
}
