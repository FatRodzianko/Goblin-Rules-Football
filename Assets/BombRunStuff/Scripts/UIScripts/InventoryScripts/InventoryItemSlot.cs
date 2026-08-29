using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemSlot : MonoBehaviour
{
    [Header("Item Details")]
    [SerializeField] private Sprite _sprite;
    [SerializeField] private string _name;
    [SerializeField] private string _description;

    [Header("UI Objects")]
    [SerializeField] private Image _itemImage;

    public void AddItemToSlot(Sprite sprite, string name, string description)
    {
        this._sprite = sprite;
        this._name = name;
        this._description = description;

        AddItemImage(sprite);
    }
    public void AddItemImage(Sprite sprite)
    {
        this._itemImage.sprite = sprite;
        this._itemImage.color = new Color(1, 1, 1, 1);
    }
    public void ClearItem()
    {
        this._sprite = null;
        this._name = "";
        this._description = "";

        this._itemImage.sprite = null;
        this._itemImage.color = new Color(1, 1, 1, 0);
    }
}
