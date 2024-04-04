using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileButtonHandler : MonoBehaviour
{
    [SerializeField] MapMakerGroundTileBase _item;
    [SerializeField] Button _button;
    [SerializeField] Image _myImage;
    [SerializeField] MapMakerBuilder _mapMakerBuilder;

    private void Awake()
    {
        if (!_button)
        {
            _button = this.GetComponent<Button>();
        }
        _button.onClick.AddListener(ButtonClicked);

        if (!_myImage)
            _myImage = this.GetComponent<Image>();

        _mapMakerBuilder = MapMakerBuilder.GetInstance();
    }
    void ButtonClicked()
    {
        Debug.Log("Button was clicked: " + _item.name);
        _mapMakerBuilder.ObjectSelected(_item);
    }
    public void SetGroundTileItem(MapMakerGroundTileBase newItem)
    {
        this._item = newItem;
        //this._myImage.sprite = newItem.Tile.sprite;
    }
}
