using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    private GridPosition _gridPosition;

    // A star values
    private int _gCost;
    private int _hCost;
    private int _fCost;

    // reference to previous node
    private PathNode _cameFromPathNode; // this is the node the algorithm "came from" to reach this node. Used when building out what the final path will be by "walking back" from the final node to each "_cameFrom" node

    public PathNode(GridPosition gridPosition)
    {
        this._gridPosition = gridPosition;
    }
    public override string ToString()
    {
        return _gridPosition.ToString();
    }
    public int GetGCost()
    {
        return _gCost;
    }
    public int GetHCost()
    {
        return _hCost;
    }
    public int GetFCost()
    {
        return _fCost;
    }
}
