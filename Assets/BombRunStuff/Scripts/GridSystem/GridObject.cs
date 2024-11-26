using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject
{
    private GridSystem<GridObject> _gridSystem;
    private GridPosition _gridPosition;

    private List<BombRunUnit> _unitList = new List<BombRunUnit>();
    private List<Transform> _obstacleList = new List<Transform>(); // change this to the obstacle script to be made later?

    public GridObject(GridSystem<GridObject> gridSystem, GridPosition gridPosition)
    {
        this._gridSystem = gridSystem;
        this._gridPosition = gridPosition;
    }
    public GridPosition GetGridPosition()
    {
        return _gridPosition;
    }
    public override string ToString()
    {
        string unitString = "";
        foreach (BombRunUnit unit in _unitList)
        {
            unitString += unit + "\n";
        }
        return _gridPosition.ToString() + "\n" + unitString;
    }
    public List<BombRunUnit> GetUnitList()
    {
        return _unitList;
    }
    public void AddUnit(BombRunUnit unit)
    {
        if (!_unitList.Contains(unit))
            _unitList.Add(unit);
    }
    public void RemoveUnit(BombRunUnit unit)
    {
        if (_unitList.Contains(unit))
            _unitList.Remove(unit);
    }
    public bool HasAnyUnit()
    {
        return _unitList.Count > 0;
    }
    public List<Transform> GetObstacleList()
    {
        return _obstacleList;
    }
    public void AddObstacle(Transform obstacle)
    {
        if (!_obstacleList.Contains(obstacle))
        {
            _obstacleList.Add(obstacle);
        }
    }
    public void RemoveObstacle(Transform obstacle)
    {
        if (_obstacleList.Contains(obstacle))
        {
            _obstacleList.Remove(obstacle);
        }
    }
    public bool HasAnyObstacle()
    {
        return _obstacleList.Count > 0;
    }
}

