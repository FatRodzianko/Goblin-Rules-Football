using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem<TGridObject>
{
    private int _width;
    private int _height;
    private float _cellSize;
    private TGridObject[,] _gridObjectArray;

    public GridSystem(int width, int height, float cellsize)
    {
        this._width = width;
        this._height = height;
        this._cellSize = cellsize;

        //for (int x = 0; x < _width; x++)
        //{
        //    for (int y = 0; y < _height; y++)
        //    {
        //        Debug.DrawLine(GetWorldPosition(x,y), GetWorldPosition(x, y) + Vector3.right * 0.2f, Color.white, 1000);
        //    }
        //}
    }
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y, 0) * _cellSize;
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
}
