using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapMakerToolController : MonoBehaviour
{
    public static MapMakerToolController instance;
    [SerializeField] TileMapReferenceHolder _tileMapReferenceHolder;

    private void Awake()
    {
        MakeInstance();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        if (!_tileMapReferenceHolder)
            _tileMapReferenceHolder = this.transform.GetComponent<TileMapReferenceHolder>();
    }
    public void Eraser(Vector3Int position)
    {
        Debug.Log("MapMakerToolController: Eraser: " + position);

        foreach (Tilemap map in _tileMapReferenceHolder.AllMaps)
        {
            map.SetTile(position, null);
        }
    }
}
