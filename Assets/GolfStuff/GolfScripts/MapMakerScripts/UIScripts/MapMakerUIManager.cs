using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MapMakerUIManager : MonoBehaviour
{
    [Header("Scriptables")]
    [SerializeField] List<MapMakerGroundTileBase> _green;
    [SerializeField] List<MapMakerGroundTileBase> _fairway;

    [Header("UI Prefabs")]
    [SerializeField] GameObject _groundTypeItemPrefab;

    [Header("Map Tiles UI")]
    [SerializeField] GameObject _greenItems;
    [SerializeField] GameObject _fairwayItems;

    // Start is called before the first frame update
    void Start()
    {
        LoadGroundTileTypesForUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void LoadGroundTileTypesForUI()
    {
        if (_green.Count > 0)
        {
            AddItemToUI(_green, _greenItems);
        }
        if (_fairway.Count > 0)
        {
            AddItemToUI(_fairway, _fairwayItems);
        }
    }
    void AddItemToUI(List<MapMakerGroundTileBase> tiles, GameObject uiHolder)
    {
        foreach (MapMakerGroundTileBase tile in tiles)
        {
            GameObject newTileItem = Instantiate(_groundTypeItemPrefab, uiHolder.transform);
            TileButtonHandler newTileItemButtonHandler = newTileItem.GetComponent<TileButtonHandler>();
            newTileItemButtonHandler.SetGroundTileItem(tile);
        }
    }
}
