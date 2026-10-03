using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventoryTypeButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private InventoryType _inventoryType = InventoryType.None;

    [Header("UI Objects")]
    [SerializeField] private TextMeshProUGUI _inventoryName;
    [SerializeField] private GameObject _selectedPanelBackground;

    [Header("Colors")]
    [SerializeField] private Color _notSelectedColor = Color.white;
    [SerializeField] private Color _selectedColor = Color.yellow;

    public static event EventHandler<InventoryType> OnInventoryTypeButtonClicked;

    // Start is called before the first frame update
    void Start()
    {
        
        //SetInventoryTypeText(this._inventoryType.ToString());
    }
    private void OnEnable()
    {
        BodyModInventoryUIManager.OnCurrentInventoryTypeChanged += BodyModInventoryUIManager_OnCurrentInventoryTypeChanged;
        BodyModInventoryUIManager.OnInventoryMenuOpened += BodyModInventoryUIManager_OnInventoryMenuOpened;
    }

    private void OnDisable()
    {
        BodyModInventoryUIManager.OnCurrentInventoryTypeChanged -= BodyModInventoryUIManager_OnCurrentInventoryTypeChanged;
        BodyModInventoryUIManager.OnInventoryMenuOpened -= BodyModInventoryUIManager_OnInventoryMenuOpened;
    }

    

    public void OnPointerClick(PointerEventData eventData)
    {
        OnInventoryTypeButtonClicked?.Invoke(this, _inventoryType);
    }
    public InventoryType GetInventoryType()
    {
        return _inventoryType;
    }
    private void BodyModInventoryUIManager_OnCurrentInventoryTypeChanged(object sender, InventoryType currentInventoryType)
    {
        //Debug.Log("BodyModInventoryUIManager_OnCurrentInventoryTypeChanged: currentInventoryType" + currentInventoryType + " my inventory type: " + _inventoryType);
        if (this._inventoryType == currentInventoryType)
        {
            SetTextColor(_selectedColor);
            SetSelectedPanel(true);
        }
        else
        {
            SetTextColor(_notSelectedColor);
            SetSelectedPanel(false);
        }
    }
    private void BodyModInventoryUIManager_OnInventoryMenuOpened(object sender, EventArgs e)
    {
        if ((sender as BodyModInventoryUIManager).GetInventoryType() == this._inventoryType)
        {
            SetTextColor(_selectedColor);
            SetSelectedPanel(true);
        }
        else
        {
            SetTextColor(_notSelectedColor);
            SetSelectedPanel(false);
        }
    }
    private void SetInventoryTypeText(string inventoryTypeText)
    {
        this._inventoryName.text = inventoryTypeText;
    }
    private void SetTextColor(Color color)
    {
        this._inventoryName.color = color;
    }
    private void SetSelectedPanel(bool selected)
    {
        _selectedPanelBackground.SetActive(selected);
    }
}
