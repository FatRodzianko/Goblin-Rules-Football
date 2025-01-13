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
    private bool _wasChecked = false;

    // reference to previous node
    private PathNode _cameFromPathNode; // this is the node the algorithm "came from" to reach this node. Used when building out what the final path will be by "walking back" from the final node to each "_cameFrom" node

    private bool _isWalkable = true;

    // cached neighbors
    List<PathNode> _neighborNodes = new List<PathNode>();
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
    public void SetGCost(int gCost)
    {
        this._gCost = gCost;
    }
    public void SetHCost(int hCost)
    {
        this._hCost = hCost;
    }
    public void CalculateFCost()
    {
        _fCost = _gCost + _hCost;
    }
    public void ResetCameFromPathNode()
    {
        _cameFromPathNode = null;
    }
    public GridPosition GetGridPosition()
    {
        return _gridPosition;
    }
    public void SetCameFromPathNode(PathNode node)
    {
        _cameFromPathNode = node;
    }
    public PathNode GetCameFromPathNode()
    {
        return _cameFromPathNode;
    }
    public bool IsWalkable()
    {
        return _isWalkable;
    }
    public void SetIsWalkable(bool isWalkable)
    {
        this._isWalkable = isWalkable;
    }
    public void SetWasChecked(bool wasChecked)
    {
        this._wasChecked = wasChecked;
    }
    public bool GetWasChecked()
    {
        return _wasChecked;
    }
    public void SetNeighborList(List<PathNode> newNeighbors)
    {
        this._neighborNodes.AddRange(newNeighbors);
    }
    public List<PathNode> GetNeighborNodes()
    {
        return _neighborNodes;
    }
}
