using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    [SerializeField] private Transform _gridDebugObjectPrefab;
    private int _width;
    private int _height;
    private float _cellSize;

    // grid system
    GridSystem<PathNode> _gridSystem;

    private void Awake()
    {
        _gridSystem = new GridSystem<PathNode>(10, 10, 2f, 
            (GridSystem<PathNode> g, GridPosition gridPosition) => new PathNode(gridPosition));

        _gridSystem.CreateDebugObjects(_gridDebugObjectPrefab);
    }

}
