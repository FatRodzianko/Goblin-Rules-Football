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
    private bool _isClosed = false;
    private bool _isOpen = false;

    // reference to previous node
    private PathNode _cameFromPathNode; // this is the node the algorithm "came from" to reach this node. Used when building out what the final path will be by "walking back" from the final node to each "_cameFrom" node

    private bool _isWalkable = true;

    // cached neighbors
    List<PathNode> _neighborNodes = new List<PathNode>();
    Dictionary<PathNode, int> _neighborNodeCostDictionary = new Dictionary<PathNode, int>();

    // heap stuff
    private int _heapIndex;
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
    public void SetIsClosed(bool isClosed)
    {
        this._isClosed = isClosed;
    }
    public bool IsClosed()
    {
        return _isClosed;
    }
    public void SetIsOpen(bool isOpen)
    {
        this._isOpen = isOpen;
    }
    public bool IsOpen()
    {
        return _isOpen;
    }
    public void SetNeighborList(List<PathNode> newNeighbors)
    {
        this._neighborNodes.Clear();
        this._neighborNodes.AddRange(newNeighbors);
    }
    public List<PathNode> GetNeighborNodes()
    {
        return _neighborNodes;
    }
    public void AddNeighborToNeighborNodeCostDictionary(PathNode neighborNode, int distanceToNeighbor)
    {
        if (_neighborNodeCostDictionary.ContainsKey(neighborNode))
            return;
        _neighborNodeCostDictionary.Add(neighborNode, distanceToNeighbor);        
    }
    public Dictionary<PathNode, int> GetNeighborNodeCostDictionary()
    {
        return _neighborNodeCostDictionary;
    }
    //public int HeapIndex
    //{
    //    get {
    //        return _heapIndex;
    //    }
    //    set {
    //        this._heapIndex = value;
    //    }
    //}
    //// Compare the f costs of two path nodes?
    //public int CompareTo(PathNode pathNodeToCompare)
    //{
    //    int compare = _fCost.CompareTo(pathNodeToCompare.GetFCost());
    //    // if f costs are the same, compare the h costs?
    //    if (compare == 0)
    //    {
    //        compare = _hCost.CompareTo(pathNodeToCompare.GetHCost());
    //    }

    //    // set compare to negative because int.Compare() (what _fCost.CompareTo is doing) returns 1 if the int is higher than what you are comparing two. However, for the Heap stuff, the lower fCost should be the higher priority and return 1
    //    return -compare;
    //}
}
