using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject
{
    private GridSystem<GridObject> _gridSystem;
    private GridPosition _gridPosition;

    private List<BombRunUnit> _unitList = new List<BombRunUnit>();

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
}

