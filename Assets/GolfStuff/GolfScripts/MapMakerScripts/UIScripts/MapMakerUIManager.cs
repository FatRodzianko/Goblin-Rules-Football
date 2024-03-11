using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;

public class MapMakerUIManager : SingletonInstance<MapMakerUIManager>
{
    [Header("Scriptables")]
    [SerializeField] List<MapMakerGroundTileBase> _green;
    [SerializeField] List<MapMakerGroundTileBase> _fairway;

    [Header("UI Prefabs")]
    [SerializeField] GameObject _groundTypeItemPrefab;
    [SerializeField] GameObject _uiTileTypePrefab;

    [Header("Map Tiles UI")]
    [SerializeField] GameObject _greenItems;
    [SerializeField] GameObject _fairwayItems;

    // https://www.youtube.com/watch?v=dCrkOIylNSw&list=PLJBcv4t1EiSz-wA35-dWpcI98pNiyK6an&index=4
    [Header("UI Elements (from video")]
    [SerializeField] List<UITileTypes> _uiTileTypes;
    [SerializeField] Transform _groundTileTypeHolder;

    Dictionary<UITileTypes, GameObject> _uiElements = new Dictionary<UITileTypes, GameObject>();
    Dictionary<GameObject, Transform> _elementItemSlot = new Dictionary<GameObject, Transform>();


    // Start is called before the first frame update
    void Start()
    {
        //LoadGroundTileTypesForUI();
        BuildUI();
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
    void BuildUI()
    {
        foreach (UITileTypes ui in _uiTileTypes)
        {
            if (_uiElements.ContainsKey(ui))
                continue;
            var inst = Instantiate(_uiTileTypePrefab, Vector3.zero, Quaternion.identity);
            inst.transform.SetParent(_groundTileTypeHolder, false);

            inst.name = ui.name;

            _uiElements[ui] = inst;
            _elementItemSlot[inst] = inst.GetComponent<UITileTypeScript>().ItemHolder;

            TextMeshProUGUI text = inst.GetComponentInChildren<TextMeshProUGUI>();
            text.text = ui.name;

            inst.transform.SetSiblingIndex(ui.SiblingIndex);

            Image img = inst.GetComponentInChildren<Image>();
            img.color = ui.BackgroundColor;
        }

        MapMakerGroundTileBase[] groundTiles = GetAllGroundTiles();

        foreach (MapMakerGroundTileBase groundTileBase in groundTiles)
        {
            if (groundTileBase.UITileType == null)
                continue;


            var itemsParent = _elementItemSlot[_uiElements[groundTileBase.UITileType]];
            var inst = Instantiate(_groundTypeItemPrefab, Vector3.zero, Quaternion.identity);
            inst.transform.SetParent(itemsParent,false);

            // name in hierarchy
            inst.name = groundTileBase.name;

            // Get the tile's sprite from the TileBase
            Image img = inst.GetComponent<Image>();
            Tile t = (Tile)groundTileBase.TileBase;
            img.sprite = t.sprite;

            // Apply BuildingObjectBase to Button handler script thing?
            var script = inst.GetComponent<TileButtonHandler>();
            script.SetGroundTileItem(groundTileBase);


        }
    }
    MapMakerGroundTileBase[] GetAllGroundTiles()
    {
        return Resources.LoadAll<MapMakerGroundTileBase>("MapMakerGolf/GroundTileScriptables");
    }
}
