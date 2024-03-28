using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class DrawRenderer
{
    public static BoundsInt RectangleRenderer(Vector3Int startPosition, Vector3Int currentPosition)
    {
        BoundsInt bounds = new BoundsInt();

        // Get the "starting corner" of the rectangle to draw
        // Get the "starting corner" of the rectangle to draw
        bounds.xMin = currentPosition.x < startPosition.x ? currentPosition.x : startPosition.x;
        bounds.xMax = currentPosition.x > startPosition.x ? currentPosition.x : startPosition.x;
        bounds.yMin = currentPosition.y < startPosition.y ? currentPosition.y : startPosition.y;
        bounds.yMax = currentPosition.y > startPosition.y ? currentPosition.y : startPosition.y;

        return bounds;
    }

    public static List<Vector2Int> LineRenderer(Vector3Int startPosition, Vector3Int currentPosition)
    {

        return MapMakerBuilder.GetPointsOnLine((Vector2Int)startPosition, (Vector2Int)currentPosition, false).ToList();

    }
}
