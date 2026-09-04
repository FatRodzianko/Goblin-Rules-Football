using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


public class InventoryItemSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int _slotIndex;
    [SerializeField] private bool _isSelected;

    [Header("Item Details")]
    [SerializeField] private Sprite _sprite;
    [SerializeField] private string _name;
    [SerializeField] private string _description;
    [SerializeField] private int _itemCount;
    [SerializeField] private bool _hasItem;
    [SerializeField] private bool _isEquipped;

    [Header("UI Objects")]
    [SerializeField] private Image _itemImage;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private TextMeshProUGUI _itemCountText;
    [SerializeField] private TextMeshProUGUI _equippedIndicator;

    [Header("Selection Colors")]
    [SerializeField] private Color _notSelected;
    [SerializeField] private Color _selected;
    [SerializeField] private Color _mouseOver;

    // Events
    public static event EventHandler<int> OnAnyItemSlotClickedOn;

    public void AddItemToSlot(Sprite sprite, string name, string description, int itemCount = 1)
    {
        this._sprite = sprite;
        this._name = name;
        this._description = description;
        this._itemCount = itemCount;
        this._hasItem = true;

        AddItemImage(sprite);
        UpdateItemCountText(itemCount);
    }
    public void AddItemImage(Sprite sprite)
    {
        this._itemImage.sprite = sprite;
        this._itemImage.color = new Color(1, 1, 1, 1);
    }
    public void UpdateItemCountText(int itemCount)
    {
        if (this._itemCount <= 1)
        {
            _itemCountText.text = "";
            _itemCountText.enabled = false;
            return;
        }

        _itemCountText.text = itemCount.ToString();
        _itemCountText.enabled = true;
    }
    public void UpdateIsEquippedIndicator(bool isEquipped)
    {
        Debug.Log("UpdateIsEquippedIndicator: " + isEquipped + " " + this._name + " at index: " + this._slotIndex);
        this._equippedIndicator.gameObject.SetActive(isEquipped);
    }
    public void ClearItem()
    {
        this._sprite = null;
        this._name = "";
        this._description = "";
        this._itemCount = 0;

        this._hasItem = false;
        UpdateItemCountText(this._itemCount);

        this._isEquipped = false;
        UpdateIsEquippedIndicator(this._isEquipped);

        this._itemImage.sprite = null;
        this._itemImage.color = new Color(1, 1, 1, 0);
    }
    public Sprite Sprite()
    {
        return _sprite;
    }
    public string Name()
    {
        return _name;
    }
    public string Description()
    {
        return _description;
    }
    public int ItemCount()
    {
        return _itemCount;
    }
    public bool HasItem()
    {
        return _hasItem;
    }
    public void SetSlotIndex(int index)
    {
        this._slotIndex = index;
    }
    public int SlotIndex()
    {
        return _slotIndex;
    }
    public bool IsEquipped()
    {
        return _isEquipped;
    }
    public void SetIsEquipped(bool isEquipped)
    {
        this._isEquipped = isEquipped;
        UpdateIsEquippedIndicator(this._isEquipped);
    }
    public bool IsSelected()
    {
        return _isSelected;
    }
    public void SetIsSelected(bool isSelected)
    {
        _isSelected = isSelected;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }
    private void OnLeftClick()
    {
        OnAnyItemSlotClickedOn?.Invoke(this, this._slotIndex);
    }
    private void OnRightClick()
    {
        
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // mmouse over
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //mouse off
    }
}
