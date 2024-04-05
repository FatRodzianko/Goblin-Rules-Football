using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;

public class MapMakerUIManager : MonoBehaviour
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

    [Header("UI Objects")]
    Dictionary<GameObject, MapMakerGroundTileBase> _allTileObjectsByTileBase = new Dictionary<GameObject, MapMakerGroundTileBase>();
    [SerializeField] List<GameObject> _manualTileObjects = new List<GameObject>();
    [SerializeField] List<GameObject> _ruleTileObjects = new List<GameObject>();

    [Header("Tiling Mode UI")]
    [SerializeField] Button _autoTileButton;
    [SerializeField] Button _manualTileButton;
    [SerializeField] Color _tileModeSelectedColor = Color.yellow;

    private void Awake()
    {
        _autoTileButton.onClick.AddListener(SelectAutoTileMode);
        _manualTileButton.onClick.AddListener(SelectManualTileMode);
    }
    // Start is called before the first frame update
    void Start()
    {
        //LoadGroundTileTypesForUI();
        BuildUI();
        //Default to rule tile mode to begin
        SelectAutoTileMode();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //void LoadGroundTileTypesForUI()
    //{
    //    if (_green.Count > 0)
    //    {
    //        AddItemToUI(_green, _greenItems);
    //    }
    //    if (_fairway.Count > 0)
    //    {
    //        AddItemToUI(_fairway, _fairwayItems);
    //    }
    //}
    //void AddItemToUI(List<MapMakerGroundTileBase> tiles, GameObject uiHolder)
    //{
    //    foreach (MapMakerGroundTileBase tile in tiles)
    //    {
    //        GameObject newTileItem = Instantiate(_groundTypeItemPrefab, uiHolder.transform);
    //        TileButtonHandler newTileItemButtonHandler = newTileItem.GetComponent<TileButtonHandler>();
    //        newTileItemButtonHandler.SetGroundTileItem(tile);
    //    }
    //}
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

            // Check to see if the tile is a regular TileBase or a Rule sprite
            if (groundTileBase.TileBase is Tile)
            {
                
                Tile t = (Tile)groundTileBase.TileBase;
                img.sprite = t.sprite;
                //Debug.Log("BuildUI: groundTileBase.TileBase is Tile: " + groundTileBase.name + " sprite name is: " + t.sprite.name.ToString());

                // saving all tile objects that aren't rule tiles or obstacles to reference when player selects to see manual tiles
                if (groundTileBase.GetType() != typeof(MapMakerObstacle) && groundTileBase.GetType() != typeof(MapMakerTool)) 
                {
                    if (!_manualTileObjects.Contains(inst))
                        _manualTileObjects.Add(inst);
                }
            }
            else if (groundTileBase.TileBase is RuleTile)
            {
                
                RuleTile rt = (RuleTile)groundTileBase.TileBase;
                img.sprite = rt.m_DefaultSprite;
                //Debug.Log("BuildUI: groundTileBase.TileBase is RuleTile: " + groundTileBase.name + " sprite name is: " + rt.m_DefaultSprite.name.ToString());
                // saving the "rule tiles" to reference when player selects to see auto-tiling tiles
                if (!_ruleTileObjects.Contains(inst))
                    _ruleTileObjects.Add(inst);
            }
            else
            {
                Debug.LogError("MapMakerUIManager: BuildUI: Unknown type of tile base: " + groundTileBase.TileBase.GetType());
            }
            

            // Apply BuildingObjectBase to Button handler script thing?
            var script = inst.GetComponent<TileButtonHandler>();
            script.SetGroundTileItem(groundTileBase);

            if (!_allTileObjectsByTileBase.ContainsKey(inst))
            {
                _allTileObjectsByTileBase.Add(inst, groundTileBase);
            }
        }
    }
    MapMakerGroundTileBase[] GetAllGroundTiles()
    {
        return Resources.LoadAll<MapMakerGroundTileBase>("MapMakerGolf/GroundTileScriptables");
    }
    void EnableRegularTileObjects(bool enable)
    {
        Debug.Log("EnableRegularTileObjects: " + enable);
        if (_manualTileObjects.Count == 0)
            return;
        foreach (GameObject ob in _manualTileObjects)
        {
            // Add checks here for "course type." If the course type is minigolf, only enable _allowedInMiniGolf tiles. If it is regular golf, don't enable _miniGolfOnly tiles
            ob.SetActive(enable);
        }
    }
    void EnableRuleTileObjects(bool enable)
    {
        Debug.Log("EnableRuleTileObjects: " + enable);
        if (_ruleTileObjects.Count == 0)
            return;
        foreach (GameObject ob in _ruleTileObjects)
        {
            // Add checks here for "course type." If the course type is minigolf, only enable _allowedInMiniGolf tiles. If it is regular golf, don't enable _miniGolfOnly tiles
            ob.SetActive(enable);
        }
    }
    void SelectAutoTileMode()
    {
        Debug.Log("SelectAutoTileMode: ");
        EnableRuleTileObjects(true);
        EnableRegularTileObjects(false);
        _autoTileButton.GetComponent<Image>().color = _tileModeSelectedColor;
        _manualTileButton.GetComponent<Image>().color = Color.white;
    }
    void SelectManualTileMode()
    {
        Debug.Log("SelectManualTileMode: ");
        EnableRuleTileObjects(false);
        EnableRegularTileObjects(true);
        _autoTileButton.GetComponent<Image>().color = Color.white;
        _manualTileButton.GetComponent<Image>().color = _tileModeSelectedColor;
    }
}
