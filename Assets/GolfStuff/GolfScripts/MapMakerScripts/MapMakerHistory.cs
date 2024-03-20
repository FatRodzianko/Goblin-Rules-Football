using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapMakerHistory : SingletonInstance<MapMakerHistory>
{
    //public static MapMakerHistory instance;


    List<MapMakerHistoryItem> _history = new List<MapMakerHistoryItem>();
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

    public void Add(MapMakerHistoryItem entry)
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

public class MapMakerHistoryItem
{
    private Tilemap _map;
    private Vector3Int _position;
    private TileBase _previousTile;
    private TileBase _newTile;
    private MapMakerGroundTileBase _mapMakerTileBase;

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
        if (_mapMakerTileBase.GetType() == typeof(MapMakerObstacle))
        {
            MapMakerBuilder builder = MapMakerBuilder.GetInstance();
            builder.RemoveObstacle(_position);
        }
    }
    public void Redo()
    {
        _map.SetTile(_position, _newTile);
        if (_mapMakerTileBase.GetType() == typeof(MapMakerObstacle))
        {
            MapMakerBuilder builder = MapMakerBuilder.GetInstance();
            builder.PlaceObstacle(_position, (MapMakerObstacle)_mapMakerTileBase);
        }
    }
}
