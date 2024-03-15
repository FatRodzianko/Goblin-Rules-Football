using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

enum ToolType
{
    None,
    Eraser
}

[CreateAssetMenu(fileName = "MapMaker", menuName = "MapMakerObjects/Create Map Tool")]
public class MapMakerTool : MapMakerGroundTileBase
{
    [SerializeField] ToolType _toolType;

    public void Use(Vector3Int position)
    {
        MapMakerToolController tc = MapMakerToolController.instance;

        switch (_toolType)
        {
            case ToolType.Eraser:
                tc.Eraser(position);
                break;
            default:
                Debug.LogError("MapMakerTool: Tool type was not set.");
                break;
        }
    }
}
