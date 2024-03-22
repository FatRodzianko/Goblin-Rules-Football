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


    protected override void Awake()
    {
        base.Awake();
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
    }
    public void UndoStep()
    {
        if (_currentIndex <= -1)
            return;

        _history[_currentIndex].Undo();
        _currentIndex--;
    }

    public void RedoStep()
    {
        // make sure the current index isn't at the last action/entry made. Since you will be "redoing" the step *after* this current step, you need to make sure there are actions remaining to "redo"
        if (_currentIndex < _history.Count - 1)
        {
            _currentIndex++;
            _history[_currentIndex].Redo();
        }
    }
}
public class MapMakerHistoryStep
{
    private MapMakerHistoryItem[] _historyItems;

    // constructor with multiple tilemaps, in event a "Step" stretches across multiple tilemaps
    public MapMakerHistoryStep(Tilemap[] maps, TileBase[] previousTiles, TileBase[] newTiles, Vector3Int[] positions, MapMakerGroundTileBase[] mapMakerTileBases)
    {
        _historyItems = new MapMakerHistoryItem[positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            _historyItems[i] = new MapMakerHistoryItem(maps[i], previousTiles[i], newTiles[i], positions[i], mapMakerTileBases[i]);
        }
    }
    // constructor with only one tilemap. Most drawing should be on just one tilemap... Note: "Tilemap[] maps" from the above constructor becomes Tilemap map to make it a singular tilemap
    public MapMakerHistoryStep(Tilemap map, TileBase[] previousTiles, TileBase[] newTiles, Vector3Int[] positions, MapMakerGroundTileBase[] mapMakerTileBases)
    {
        _historyItems = new MapMakerHistoryItem[positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            _historyItems[i] = new MapMakerHistoryItem(map, previousTiles[i], newTiles[i], positions[i], mapMakerTileBases[i]);
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
    private MapMakerGroundTileBase _mapMakerTileBase;

    // change this so it is a "previous" mapMakerTileBase and "new" mapMakerTileBase. For undo, only spawn obstalce if "previous mapMakerTileBase" is an obstacle. For redo, only if the "new mapMakerTileBase" is an obstacle?
    public MapMakerHistoryItem(Tilemap map, TileBase prevTile, TileBase newTile, Vector3Int pos, MapMakerGroundTileBase mapMakerTileBase)
    {
        this._map = map;
        this._previousTile = prevTile;
        this._newTile = newTile;
        this._position = pos;
        this._mapMakerTileBase = mapMakerTileBase;
    }

    public void Undo()
    {
        _map.SetTile(_position, _previousTile);

        if (_mapMakerTileBase == null)
            return;
        // remove obstacle if it exists.
        if (_mapMakerTileBase.GetType() == typeof(MapMakerObstacle))
        {
            MapMakerBuilder builder = MapMakerBuilder.GetInstance();
            if (_newTile == null)
            {
                Debug.Log("MapMakerHistoryItem: Undo: _newTile was null. Obstacle was deleted and needs to be respawned?");
                builder.PlaceObstacle(_position, (MapMakerObstacle)_mapMakerTileBase);
            }
            else
            {
                Debug.Log("MapMakerHistoryItem: Undo: _newTile was NOT null. Obstacle will be removed.");
                builder.RemoveObstacle(_position);
            }
            
        }
    }
    public void Redo()
    {
        _map.SetTile(_position, _newTile);

        if (_mapMakerTileBase == null)
            return;
        // place obstacle if it exists.
        if (_mapMakerTileBase.GetType() == typeof(MapMakerObstacle))
        {
            MapMakerBuilder builder = MapMakerBuilder.GetInstance();
            if (_newTile == null)
            {
                Debug.Log("MapMakerHistoryItem: Redo: _newTile was null. Obstacle will be removed.");
                builder.RemoveObstacle(_position);
            }
            else
            {
                Debug.Log("MapMakerHistoryItem: Redo: _newTile was NOT null. Respawning obstacle");
                
                builder.PlaceObstacle(_position, (MapMakerObstacle)_mapMakerTileBase);
            }
        }
    }

    public static implicit operator List<object>(MapMakerHistoryItem v)
    {
        throw new NotImplementedException();
    }
}
