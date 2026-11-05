using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PreviewHandler
{
    Tilemap _tilemap;
    TileBase _tileBase;
    Dictionary<Vector3Int, TileBase> _tilesBeforePreviewOverride = new Dictionary<Vector3Int, TileBase>();

    // The cells that currently show the preview tile. Always a subset of the keys above: a cell the
    // preview could not draw on is remembered but never drawn, and the difference is what lets a
    // shape preview redraw only the cells that changed.
    HashSet<Vector3Int> _drawnCells = new HashSet<Vector3Int>();

    // All reused between calls, so moving the cursor across a large shape allocates nothing
    List<Vector3Int> _batchPositions = new List<Vector3Int>();
    List<TileBase> _batchTiles = new List<TileBase>();
    HashSet<Vector3Int> _cellsInShape = new HashSet<Vector3Int>();
    List<Vector3Int> _cellsToForget = new List<Vector3Int>();

    public void UpdateTile(Tilemap map, TileBase tile)
    {
        // The cached tiles belong to the map they were read from. Put them back before the preview
        // switches maps, otherwise they get restored onto the new map and the old map keeps the
        // preview tile.
        if (_tilemap != null && _tilemap != map)
            ResetPreview();

        _tilemap = map;
        _tileBase = tile;
    }
    // True only when the preview overwrote this cell, so "no entry" and "entry that was empty" stay
    // distinguishable. Callers that record undo history need that difference.
    public bool TryGetPreviousTile(Vector3Int position, out TileBase previousTile)
    {
        return _tilesBeforePreviewOverride.TryGetValue(position, out previousTile);
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
        _drawnCells.Add(position);
    }
    // Moving the cursor one cell changes only a sliver of a shape, so this restores the cells that
    // left it and draws the ones that joined, rather than clearing and redrawing the whole shape.
    // At a hundred cells on a side that is the difference between a couple of hundred tile writes
    // per cell of movement and twenty thousand.
    public void SetPreview(List<Vector3Int> positions, List<bool> isForbidden)
    {
        if (_tilemap == null)
        {
            Debug.Log("SetPreview: _tilemap is null");
            return;
        }

        _cellsInShape.Clear();
        for (int i = 0; i < positions.Count; i++)
        {
            _cellsInShape.Add(positions[i]);
        }

        _batchPositions.Clear();
        _batchTiles.Clear();

        // Cells the shape has left: put back what they held and forget them
        _cellsToForget.Clear();
        foreach (KeyValuePair<Vector3Int, TileBase> pair in _tilesBeforePreviewOverride)
        {
            if (_cellsInShape.Contains(pair.Key))
                continue;

            if (_drawnCells.Contains(pair.Key))
            {
                _batchPositions.Add(pair.Key);
                _batchTiles.Add(pair.Value);
            }
            _cellsToForget.Add(pair.Key);
        }
        for (int i = 0; i < _cellsToForget.Count; i++)
        {
            _tilesBeforePreviewOverride.Remove(_cellsToForget[i]);
            _drawnCells.Remove(_cellsToForget[i]);
        }

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3Int cell = positions[i];

            // Remember what the cell held before the preview touches it, forbidden or not, so it
            // can be restored and so the history knows the real previous tile
            if (!_tilesBeforePreviewOverride.ContainsKey(cell))
                _tilesBeforePreviewOverride.Add(cell, _tilemap.GetTile(cell));

            // A forbidden cell is remembered but not drawn, so the preview shows where the shape
            // will really land
            if (isForbidden[i])
            {
                if (_drawnCells.Remove(cell))
                {
                    _batchPositions.Add(cell);
                    _batchTiles.Add(_tilesBeforePreviewOverride[cell]);
                }
                continue;
            }

            if (_drawnCells.Contains(cell))
                continue;

            _batchPositions.Add(cell);
            _batchTiles.Add(_tileBase);
            _drawnCells.Add(cell);
        }

        WriteBatch();
    }
    // One write for everything that changed instead of one per cell
    void WriteBatch()
    {
        if (_batchPositions.Count > 0 && _tilemap != null)
            _tilemap.SetTiles(_batchPositions.ToArray(), _batchTiles.ToArray());

        _batchPositions.Clear();
        _batchTiles.Clear();
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
        // Put back every cell the preview drew on. Cells it only remembered were never changed.
        if (_tilemap != null && _drawnCells.Count > 0)
        {
            _batchPositions.Clear();
            _batchTiles.Clear();

            foreach (Vector3Int cell in _drawnCells)
            {
                _batchPositions.Add(cell);
                _batchTiles.Add(_tilesBeforePreviewOverride[cell]);
            }

            WriteBatch();
        }

        ClearPreview();
    }
    public void ResetPreview(Vector3Int position)
    {
        // Only a cell the preview actually overwrote may be reset. Clearing a cell that isn't in the
        // dictionary would delete a real tile the player drew.
        if (!_tilesBeforePreviewOverride.ContainsKey(position))
            return;

        if (_tilemap != null && _drawnCells.Contains(position))
            _tilemap.SetTile(position, _tilesBeforePreviewOverride[position]);

        _tilesBeforePreviewOverride.Remove(position);
        _drawnCells.Remove(position);
    }
    public void ClearPreview()
    {
        _tilesBeforePreviewOverride.Clear();
        _drawnCells.Clear();
    }
}
