using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject
{
    private GridSystem<GridObject> _gridSystem;
    private GridPosition _gridPosition;

    private List<BombRunUnit> _unitList = new List<BombRunUnit>();
    private BaseBombRunObstacle _obstacle;
    private IInteractable _interactable;

    public GridObject(GridSystem<GridObject> gridSystem, GridPosition gridPosition)
    {
        this._gridSystem = gridSystem;
        this._gridPosition = gridPosition;
    }
    //public GridObject(GridPosition gridPosition)
    //{
    //    this._gridPosition = gridPosition;
    //}
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
    public BaseBombRunObstacle GetObstacle()
    {
        return _obstacle;
    }
    public void AddObstacle(BaseBombRunObstacle obstacle)
    {
        // can't have more than one obstacle, so if there is already an obstacle on this grid object, just skip it for now?
        if (_obstacle != null)
        {
            Debug.Log("AddObstacle: obstacle already exists at: " + this._gridPosition.ToString() + " obstacle: " + _obstacle.name + ". Skipping...");
            return;
        }

        _obstacle = obstacle;

        // old
        //if (!_obstacleList.Contains(obstacle))
        //{
        //    _obstacleList.Add(obstacle);
        //}
    }
    public void RemoveObstacle(BaseBombRunObstacle obstacle)
    {
        if (_obstacle == obstacle)
        {
            _obstacle = null;
        }
    }
    public bool HasAnyObstacle()
    {
        return _obstacle != null;
    }
    public IInteractable GetInteractable()
    {
        return _interactable;
    }
    public void AddInteractable(IInteractable interactable)
    {
        // can't have more than one obstacle, so if there is already an obstacle on this grid object, just skip it for now?
        if (_interactable != null)
        {
            Debug.Log("AddInterActable: interactable already exists at: " + this._gridPosition.ToString() + " interatable: " + interactable + ". Skipping...");
            return;
        }

        _interactable = interactable;
    }
    public void RemoveInteractable(IInteractable interactable)
    {
        if (_interactable == interactable)
        {
            _interactable = null;
        }
    }
    public bool HasAnyInteractable()
    {
        return _interactable != null;
    }
}

