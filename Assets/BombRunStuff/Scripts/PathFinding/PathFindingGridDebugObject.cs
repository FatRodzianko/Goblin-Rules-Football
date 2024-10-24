using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PathFindingGridDebugObject : GridDebugObject
{
    [SerializeField] private TextMeshPro _gCostText;
    [SerializeField] private TextMeshPro _hCostText;
    [SerializeField] private TextMeshPro _fCostText;

    private PathNode _pathNode;

    public override void SetGridObject(object gridObject)
    {
        _pathNode = (PathNode)gridObject;

        base.SetGridObject(gridObject);
    }
    protected override void Update()
    {
        base.Update();
        _gCostText.text = _pathNode.GetGCost().ToString();
        _hCostText.text = _pathNode.GetHCost().ToString();
        _fCostText.text = _pathNode.GetFCost().ToString();
    }
}
