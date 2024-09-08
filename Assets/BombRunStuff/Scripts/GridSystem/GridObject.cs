using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject
{
    private GridSystem<GridObject> _gridSystem;
    private GridPosition _gridPosition;

    public GridObject(GridSystem<GridObject> gridSystem, GridPosition gridPosition)
    {
        this._gridSystem = gridSystem;
        this._gridPosition = gridPosition;
    }
}
