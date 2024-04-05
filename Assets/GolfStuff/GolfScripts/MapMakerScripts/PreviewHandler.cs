using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PreviewHandler
{
    Tilemap _tilemap;
    TileBase _tileBase;
    Dictionary<Vector3Int, TileBase> _tilesBeforePreviewOverride = new Dictionary<Vector3Int, TileBase>();

    public void UpdateTile(Tilemap map, TileBase tile)
    {
        _tilemap = map;
        _tileBase = tile;
    }
    public TileBase GetPreviousTile(Vector3Int position)
    {
        if (_tilesBeforePreviewOverride.ContainsKey(position))
        {
            return _tilesBeforePreviewOverride[position];
        }

        return null;
    }
    public void SetPreview(Vector3Int position, bool isForbidden)
    {
        //Debug.Log("SetPreview: position: " + position.ToString() + " isForbidden: " + isForbidden);
        if (_tilemap == null)
        {
            Debug.Log("SetPreview: _tilemap is null");
            return;
        }

        // Check if a tile already exists on this tilemap when the preview is at this position. If yes, save that tile and position so it can be restored to the tile map if the preview goes away
        // If no previous tile existed on this tilemap, add preview to the dictionary?
        if (!_tilesBeforePreviewOverride.ContainsKey(position))
        {
            _tilesBeforePreviewOverride.Add(position, _tilemap.GetTile(position));
            //Debug.Log("SetPreview: Adding: " + _tilemap.GetTile(position) + " at positon: " + position.ToString() + " on tilemap: " + _tilemap.name + " to _tilesBeforePreviewOverride");
        }

        // Don't draw the tile to the tilemap is placement is forbidden. We only need to update the _tilesBeforePreviewOverride, then can exit out?
        if (isForbidden)
            return;
        // check to see if the tile is already there. If not, don't bother setting it?
        //if (_tilemap.HasTile(position))
        //{
        //    if (_tilemap.GetTile(position) == _tileBase)
        //        return;
        //}
        _tilemap.SetTile(position, _tileBase);
    }
    public void SetPreview(Vector3Int[] positions, bool[] isForbidden)
    {
        ResetPreview();
        for (int i = 0; i < positions.Length; i++)
        {
            SetPreview(positions[i], isForbidden[i]);
        }
    }
    //public void SetPreview(Vector3Int[] positions, bool[] isForbidden, BoundsInt bounds)
    //{
    //    ResetPreview(bounds);
    //    for (int i = 0; i < positions.Length; i++)
    //    {
    //        SetPreview(positions[i], isForbidden[i]);
    //    }
    //}
    //void ResetPreview(BoundsInt bounds)
    //{
    //    // loop through the dictionary. reset every key in the dictionary to the saved "previous tile" to reset the preview
    //    foreach (var pair in _tilesBeforePreviewOverride)
    //    {
    //        if (bounds.Contains(pair.Key))
    //        {
    //            _tilemap.SetTile(pair.Key, pair.Value);
    //        }
    //        else
    //        {
    //            _tilemap.SetTile(pair.Key, null);
    //        }
            
    //    }

    //    ClearPreview();
    //}
    public void ResetPreview()
    {
        // loop through the dictionary. reset every key in the dictionary to the saved "previous tile" to reset the preview
        foreach (var pair in _tilesBeforePreviewOverride)
        {
            _tilemap.SetTile(pair.Key, pair.Value);
        }

        ClearPreview();
    }
    public void ResetPreview(Vector3Int position)
    {
        if (_tilesBeforePreviewOverride.ContainsKey(position))
        {
            _tilemap.SetTile(position, _tilesBeforePreviewOverride[position]);
            _tilesBeforePreviewOverride.Remove(position);
        }
        else
        {
            //Debug.Log("ResetPreview: Setting: " + position.ToString() + " to null on tilemap: " + _tilemap.name);
            _tilemap.SetTile(position, null);
        }
        
    }
    public void ClearPreview()
    {
        _tilesBeforePreviewOverride.Clear();
    }
}
