using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapMakerHistory : SingletonInstance<MapMakerHistory>
{
    //public static MapMakerHistory instance;


    //List<MapMakerHistoryItem> _history = new List<MapMakerHistoryItem>();
    List<MapMakerHistoryStep> _history = new List<MapMakerHistoryStep>();
    int _currentIndex = -1;

    public bool CanUndo => _currentIndex >= 0;
    public bool CanRedo => _currentIndex < (_history.Count - 1);


    public delegate void CanUndoChange(bool canUndo);
    public event CanUndoChange CanUndoChanged;

    public delegate void CanRedoChange(bool canRedo);
    public event CanRedoChange CanRedoChanged;

    protected override void Awake()
    {
        base.Awake();

        CanUndoChanged = CanUndoChangedFunction;
        CanRedoChanged = CanRedoChangedFunction;

    }
    void CanUndoChangedFunction(bool canUndo)
    {
        //Debug.Log("CanUndoChangedFunction: the new weather effect is: " + canUndo);
    }
    void CanRedoChangedFunction(bool canRedo)
    {
        //Debug.Log("CanRedoChangedFunction: the new weather effect is: " + canRedo);
    }
    //void MakeInstance()
    //{
    //    if (instance == null)
    //        instance = this;
    //    else
    //        Destroy(gameObject);
    //}

    //public void Add(MapMakerHistoryItem entry)
    //{
    //    // Remove any entries ahead of the current index in the event we are already "back" in the history. player has undone twice, then makes a new action. Destroy the action that would be ahead of the current index
    //    _history.RemoveRange(_currentIndex + 1, _history.Count - (_currentIndex + 1));
    //    _history.Add(entry);
    //    _currentIndex++;
    //}
    public void Add(MapMakerHistoryStep entry)
    {
        // Remove any entries ahead of the current index in the event we are already "back" in the history. player has undone twice, then makes a new action. Destroy the action that would be ahead of the current index
        _history.RemoveRange(_currentIndex + 1, _history.Count - (_currentIndex + 1));
        _history.Add(entry);
        _currentIndex++;
        SetUndoRedoBools();
    }
    public void UndoStep()
    {
        if (_currentIndex <= -1)
            return;

        _history[_currentIndex].Undo();
        _currentIndex--;
        SetUndoRedoBools();
    }

    public void RedoStep()
    {
        // make sure the current index isn't at the last action/entry made. Since you will be "redoing" the step *after* this current step, you need to make sure there are actions remaining to "redo"
        if (_currentIndex < _history.Count - 1)
        {
            _currentIndex++;
            _history[_currentIndex].Redo();
        }
        SetUndoRedoBools();
    }
    void SetUndoRedoBools()
    {
        CanUndoChanged(this.CanUndo);
        CanRedoChanged(this.CanRedo);
    }
}
public class MapMakerHistoryStep
{
    private MapMakerHistoryItem[] _historyItems;

    // constructor with multiple tilemaps, in event a "Step" stretches across multiple tilemaps
    public MapMakerHistoryStep(Tilemap[] maps, TileBase[] previousTiles, TileBase[] newTiles, Vector3Int[] positions, MapMakerGroundTileBase[] prevMapMakerTileBases, MapMakerGroundTileBase[] newMapMakerTileBases)
    {
        _historyItems = new MapMakerHistoryItem[positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            //_historyItems[i] = new MapMakerHistoryItem(maps[i], previousTiles[i], newTiles[i], positions[i], mapMakerTileBases[i]);
            _historyItems[i] = new MapMakerHistoryItem(maps[i], previousTiles[i], newTiles[i], positions[i], prevMapMakerTileBases[i], newMapMakerTileBases[i]);
        }
    }
    // constructor with only one tilemap. Most drawing should be on just one tilemap... Note: "Tilemap[] maps" from the above constructor becomes Tilemap map to make it a singular tilemap
    //public MapMakerHistoryStep(Tilemap map, TileBase[] previousTiles, TileBase[] newTiles, Vector3Int[] positions, MapMakerGroundTileBase[] mapMakerTileBases)
    public MapMakerHistoryStep(Tilemap map, TileBase[] previousTiles, TileBase[] newTiles, Vector3Int[] positions, MapMakerGroundTileBase[] prevMapMakerTileBases, MapMakerGroundTileBase[] newMapMakerTileBases)
    {
        _historyItems = new MapMakerHistoryItem[positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            _historyItems[i] = new MapMakerHistoryItem(map, previousTiles[i], newTiles[i], positions[i], prevMapMakerTileBases[i], newMapMakerTileBases[i]);
        }
    }

    // constructor to take array of MapMakerHistoryItems and return a MapMakerHistoryStep?
    public MapMakerHistoryStep(MapMakerHistoryItem[] items)
    {
        _historyItems = items;
    }
    public void Undo()
    {
        foreach (MapMakerHistoryItem item in _historyItems)
        {
            item.Undo();
        }
    }
    public void Redo()
    {
        foreach (MapMakerHistoryItem item in _historyItems)
        {
            item.Redo();
        }
    }
}
public class MapMakerHistoryItem
{
    private Tilemap _map;
    private Vector3Int _position;
    private TileBase _previousTile;
    private TileBase _newTile;
    private MapMakerGroundTileBase _previousMapMakerTileBase;
    private MapMakerGroundTileBase _newMapMakerTileBase;

    // change this so it is a "previous" mapMakerTileBase and "new" mapMakerTileBase. For undo, only spawn obstalce if "previous mapMakerTileBase" is an obstacle. For redo, only if the "new mapMakerTileBase" is an obstacle?
    public MapMakerHistoryItem(Tilemap map, TileBase prevTile, TileBase newTile, Vector3Int pos, MapMakerGroundTileBase prevMapMakerTileBase, MapMakerGroundTileBase newMapMakerTileBase)
    {
        this._map = map;
        this._previousTile = prevTile;
        this._newTile = newTile;
        this._position = pos;
        this._previousMapMakerTileBase = prevMapMakerTileBase;
        this._newMapMakerTileBase = newMapMakerTileBase;
    }

    public void Undo()
    {
        _map.SetTile(_position, _previousTile);

        //if (_mapMakerTileBase == null)
        //    return;
        //// remove obstacle if it exists.
        //if (_mapMakerTileBase.GetType() == typeof(MapMakerObstacle))
        //{
        //    MapMakerBuilder builder = MapMakerBuilder.GetInstance();
        //    if (_newTile == null)
        //    {
        //        Debug.Log("MapMakerHistoryItem: Undo: _newTile was null. Obstacle was deleted and needs to be respawned?");
        //        builder.PlaceObstacle(_position, (MapMakerObstacle)_mapMakerTileBase);
        //    }
        //    else
        //    {
        //        Debug.Log("MapMakerHistoryItem: Undo: _newTile was NOT null. Obstacle will be removed.");
        //        builder.RemoveObstacle(_position);
        //    }
        //}

        // Undo means go back to what the previous tile was. If that tile had an object on it, then _previousMapMakerTileBase should be a MapMakerObstacle. If that is true, respawn the  object
        if (_previousMapMakerTileBase != null)
        {
            if (_previousMapMakerTileBase.GetType() == typeof(MapMakerObstacle))
            {
                MapMakerBuilder builder = MapMakerBuilder.GetInstance();
                Debug.Log("MapMakerHistoryItem: Undo: _previousMapMakerTileBase" + _previousMapMakerTileBase.name + " was NOT null and was an obstacle. Obstacle was deleted and needs to be respawned? Position: " + _position.ToString() + " new tile was: " + _newMapMakerTileBase);
                builder.PlaceObstacle(_position, (MapMakerObstacle)_previousMapMakerTileBase);
            }
        }
        // Undo also means to REMOVE the "new" tile that had been placed at this position. If that new tile had an object on it, _newMapMakerTileBase shoud be a MapMakerObstacle. If that is true, destroy the obstacle
        if (_newMapMakerTileBase != null)
        {
            if (_newMapMakerTileBase.GetType() == typeof(MapMakerObstacle))
            {
                MapMakerBuilder builder = MapMakerBuilder.GetInstance();
                Debug.Log("MapMakerHistoryItem: Undo: _newMapMakerTileBase " + _newMapMakerTileBase.name + " was NOT null and was an obstacle. Obstacle will be removed. Position: " + _position.ToString() + " previous tile was: " + _previousMapMakerTileBase);
                builder.RemoveObstacle(_position);
            }
        }
    }
    public void Redo()
    {
        _map.SetTile(_position, _newTile);

        //if (_mapMakerTileBase == null)
        //    return;
        //// place obstacle if it exists.
        //if (_mapMakerTileBase.GetType() == typeof(MapMakerObstacle))
        //{
        //    MapMakerBuilder builder = MapMakerBuilder.GetInstance();
        //    if (_newTile == null)
        //    {
        //        Debug.Log("MapMakerHistoryItem: Redo: _newTile was null. Obstacle will be removed.");
        //        builder.RemoveObstacle(_position);
        //    }
        //    else
        //    {
        //        Debug.Log("MapMakerHistoryItem: Redo: _newTile was NOT null. Respawning obstacle");

        //        builder.PlaceObstacle(_position, (MapMakerObstacle)_mapMakerTileBase);
        //    }
        //}

        // For Redo, you want to perform the "new" action again. You undid it once, and want it to be performed again. If the new tile was an object, _newMapMakerTileBase should be a MapMakerObstacle. If that is true, respawn the obstacle.
        if (_newMapMakerTileBase != null)
        {
            if (_newMapMakerTileBase.GetType() == typeof(MapMakerObstacle))
            {
                MapMakerBuilder builder = MapMakerBuilder.GetInstance();
                Debug.Log("MapMakerHistoryItem: Redo: _newMapMakerTileBase was NOT null and was an obstacle. Respawning obstacle");
                builder.PlaceObstacle(_position, (MapMakerObstacle)_newMapMakerTileBase, true);
            }   
        }
        // Redoing an eraser action can be "tricky" If you erased an obstacle, and then undid it, the obstacle would be spawned back since you undid the erasure. If you then 'redo' the erasing, you want to make sure to remove the obstacle again.
        // So, check if the _previousMapMakerTileBase was an MapMakerObstacle. If it was, only remove it IF the new tile is "null," which indicates it was erased.
        // However, You DO NOT want to remove an obstacle if the _previousMapMakerTileBase was an MapMakerObstacle AND the new tile is something other than null, such as the player placed an obstacle, then drew some fairway underneath it.
        if (_previousMapMakerTileBase != null)
        {
            if (_previousMapMakerTileBase.GetType() == typeof(MapMakerObstacle) && _newTile == null)
            {
                MapMakerBuilder builder = MapMakerBuilder.GetInstance();
                Debug.Log("MapMakerHistoryItem: Redo: _previousMapMakerTileBase was NOT null and was an obstacle. _newTile IS null, making the new tile an 'eraser.' Obstacle will be removed.");
                builder.RemoveObstacle(_position);
            }
        }

    }

    public static implicit operator List<object>(MapMakerHistoryItem v)
    {
        throw new NotImplementedException();
    }
}
