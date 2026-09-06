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
    [SerializeField] private bool _mouseOver;

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
    [SerializeField] private Color _notSelectedColor;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _mouseOverColor;

    // static Events
    public static event EventHandler<int> OnAnyItemSlotLeftClickedOn;
    public static event EventHandler OnAnyItemIsSelected;
    public static event EventHandler<int> OnAnyItemMousedOver;
    public static event EventHandler<int> OnAnyItemMouseExit;

    // events
    private event EventHandler<bool> OnItemSlotSelected;

    private void Awake()
    {
        InventoryItemSlot.OnAnyItemSlotLeftClickedOn += InventoryItemSlot_OnAnyItemSlotLeftClickedOn;
        this.OnItemSlotSelected += InventoryItemSlot_OnItemSlotSelected;
    }

    

    void OnDisable()
    {
        InventoryItemSlot.OnAnyItemSlotLeftClickedOn -= InventoryItemSlot_OnAnyItemSlotLeftClickedOn;
        this.OnItemSlotSelected -= InventoryItemSlot_OnItemSlotSelected;
    }
    
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
        //Debug.Log("UpdateIsEquippedIndicator: " + isEquipped + " " + this._name + " at index: " + this._slotIndex);
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
        OnItemSlotSelected?.Invoke(this, _isSelected);        
    }
    private void InventoryItemSlot_OnItemSlotSelected(object sender, bool selected)
    {
        if (selected)
        {
            this._backgroundImage.color = _selectedColor;
            OnAnyItemIsSelected?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            //if (this._backgroundImage.color == _mouseOverColor)
            //{
            //    return;
            //}
            //this._backgroundImage.color = _notSelectedColor;
            if (_mouseOver)
            {
                this._backgroundImage.color = _mouseOverColor;
            }
            else
            {
                this._backgroundImage.color = _notSelectedColor;
            }
            
        }
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
        OnAnyItemSlotLeftClickedOn?.Invoke(this, this._slotIndex);
    }
    private void InventoryItemSlot_OnAnyItemSlotLeftClickedOn(object sender, int index)
    {
        if (index == this._slotIndex)
        {
            
        }
    }
    private void OnRightClick()
    {
        
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        _mouseOver = true;
        OnAnyItemMousedOver?.Invoke(this, _slotIndex);
        if (this._isSelected)
        {
            return;
        }

        this._backgroundImage.color = _mouseOverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _mouseOver = false;
        OnAnyItemMouseExit?.Invoke(this, _slotIndex);
        if (this._isSelected)
        {
            return;
        }
        this._backgroundImage.color = _notSelectedColor;
    }
    public bool MouseOver()
    {
        return _mouseOver;
    }
}
