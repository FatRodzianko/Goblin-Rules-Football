using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class BodyModByItemSlot
{
    public int SlotIndex;
    public BodyMod_Class BodyMod;

    public BodyModByItemSlot(int slotIndex, BodyMod_Class bodyMod)
    {
        SlotIndex = slotIndex;
        BodyMod = bodyMod;
    }
}
public class BodyModInventoryUIManager : MonoBehaviour
{
    [SerializeField] GameObject _bodyModInventoryUIHolder;
    [SerializeField] bool _menuOpen;
    [SerializeField] BombRunUnitBodyModManager _bodyModManager;

    [Header("Item Slots")]
    [SerializeField] private Transform _itemSlotsHolder;
    [SerializeField] private Transform _itemSlotPrefab;
    [SerializeField] private List<InventoryItemSlot> _itemSlots = new List<InventoryItemSlot>();
    private Dictionary<int, BodyMod_Class> _bodyModByItemSlot = new Dictionary<int, BodyMod_Class>();
    //[SerializeField] private BodyMod_Class[] _bodyModArray;
    //[SerializeField] private List<BodyModByItemSlot> _bodyModByItemSlotClass = new List<BodyModByItemSlot>();
    [SerializeField] private int _numberOfSlots;

    [Header("Selected Item")]
    [SerializeField] private int _selectedItemIndex = 0;

    [Header("Item Description UI")]
    [SerializeField] private Image _itemDescription_Image;
    [SerializeField] private TextMeshProUGUI _itemDescription_NameText;
    [SerializeField] private TextMeshProUGUI _itemDescription_DescriptionText;


    // Start is called before the first frame update
    void Start()
    {
        CloseInventory();
        //CreateItemSlots();
        InventoryItemSlot.OnAnyItemSlotLeftClickedOn += InventoryItemSlot_OnAnyItemSlotClickedOn;
        InventoryItemSlot.OnAnyItemIsSelected += InventoryItemSlot_OnAnyItemIsSelected;
        InventoryItemSlot.OnAnyItemMousedOver += InventoryItemSlot_OnAnyItemMousedOver;
        InventoryItemSlot.OnAnyItemMouseExit += InventoryItemSlot_OnAnyItemMouseExit;
    }

    

    private void OnDisable()
    {
        InventoryItemSlot.OnAnyItemSlotLeftClickedOn -= InventoryItemSlot_OnAnyItemSlotClickedOn;
        InventoryItemSlot.OnAnyItemIsSelected -= InventoryItemSlot_OnAnyItemIsSelected;
        InventoryItemSlot.OnAnyItemMousedOver -= InventoryItemSlot_OnAnyItemMousedOver;
        InventoryItemSlot.OnAnyItemMouseExit -= InventoryItemSlot_OnAnyItemMouseExit;
    }

    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (_menuOpen)
            {
                CloseInventory();
            }
            else
            {
                OpenInventory();
            }
            
        }
    }
    private void CloseInventory()
    {
        _bodyModInventoryUIHolder.SetActive(false);
        _menuOpen = false;
        //_selectedItemIndex = 0;
        //SetSelectedItemIndex(0);
        ResetSelectedItem();
        _bodyModManager = null;
    }
    private void OpenInventory()
    {
        BombRunUnit unit = UnitActionSystem.Instance.GetSelectedUnit();
        if (unit == null)
            return;

        _bodyModInventoryUIHolder.SetActive(true);
        _menuOpen = true;

        DestroyItemSlots();
        ResetBodyModByItemSlot();
        SetBodyModManager(unit.BodyModManager());
        ClearItemDescriptionDetails();
        CreateItemSlots(_bodyModManager.MaxInventoryCount());
        GetInventoryItems(unit);
    }
    private void DestroyItemSlots()
    {
        for (int i = 0; i < _itemSlots.Count; i++)
        {
            Destroy(_itemSlots[i].gameObject);
        }
        _itemSlots.Clear();
    }
    private void ResetBodyModByItemSlot()
    {
        _bodyModByItemSlot.Clear();
        //_bodyModByItemSlotClass.Clear();
    }
    private void SetBodyModManager(BombRunUnitBodyModManager bodyModManager)
    {
        this._bodyModManager = bodyModManager;
    }
    private void CreateItemSlots(int numberOfSlots)
    {
        //Array.Resize(ref _bodyModArray, numberOfSlots);
        for (int i = 0; i < numberOfSlots; i++)
        {
            Transform newItemSlot = Instantiate(_itemSlotPrefab, _itemSlotsHolder);
            InventoryItemSlot itemSlot = newItemSlot.GetComponent<InventoryItemSlot>();
            itemSlot.SetSlotIndex(i);
            _itemSlots.Add(itemSlot);
            itemSlot.ClearItem();

        }
    }
    private void GetInventoryItems(BombRunUnit unit)
    {
        ///
        /// OLD
        ///
        //List<BodyMod_Class> bodyMods = unit.BodyModManager().GetAllBodyMods();

        //for (int i = 0; i < _itemSlots.Count; i++)
        //{
        //    //_itemSlots[i].ClearItem();

        //    if (bodyMods.Count > i)
        //    {
        //        _itemSlots[i].AddItemToSlot(bodyMods[i].Sprite(), bodyMods[i].Name(), bodyMods[i].Description());
        //        _itemSlots[i].SetIsEquipped(bodyMods[i].IsEquipped());

        //        _bodyModByItemSlot.Add(i, bodyMods[i]);
        //        //_bodyModByItemSlotClass.Add(new BodyModByItemSlot(i, bodyMods[i]));
        //        //_bodyModArray[i] = bodyMods[i];
        //    }
        //    else
        //    {
        //        _bodyModByItemSlot.Add(i, null);
        //        //_bodyModArray[i] = null;
        //        //_bodyModByItemSlotClass.Add(new BodyModByItemSlot(i, null));
        //    }
            
        //}
        //
        // OLD
        //

        for (int i = 0; i < _itemSlots.Count; i++)
        {
            if (unit.BodyModManager().Inventory_BodyMods()[i] == null)
            {
                _itemSlots[i].ClearItem();
                Debug.Log("GetInventoryItems: item NOT FOUND at index: " + i + " . Clearing...");
            }
            else
            {
                BodyMod_Class bodyMod = unit.BodyModManager().Inventory_BodyMods()[i];
                _itemSlots[i].AddItemToSlot(bodyMod.Sprite(), bodyMod.Name(), bodyMod.Description());
                _itemSlots[i].SetIsEquipped(bodyMod.IsEquipped());
                Debug.Log("GetInventoryItems: item found at index: " + i);
            }
        }


    }
    public int GetSelectedItemIndex()
    {
        return _selectedItemIndex;
    }
    private void SetSelectedItemIndex(int index)
    {
        _selectedItemIndex = index;
    }
    private void ResetSelectedItem()
    {
        _selectedItemIndex = 0;
        ClearItemDescriptionDetails();
    }
    private void InventoryItemSlot_OnAnyItemSlotClickedOn(object sender, int index)
    {
        if (!_menuOpen)
            return;
        if (index < 0)
            return;
        if (index >= _itemSlots.Count)
            return;

        if (_selectedItemIndex == index)
        {
            _itemSlots[_selectedItemIndex].SetIsSelected(!_itemSlots[_selectedItemIndex].IsSelected());
            CheckIfItemDescriptionShouldReset(_selectedItemIndex);
            return;
        }

        // make sure the inventorySlot at the selected index is currently selected
        // if it isn't selected, treat as "stale" and just select the new inventory slot
        if (!_itemSlots[_selectedItemIndex].IsSelected())
        {
            _itemSlots[index].SetIsSelected(true);
            //_selectedItemIndex = index;
            SetSelectedItemIndex(index);
            return;
        }

        // Check to see if player is trying to move an inventory item to a new slot
        // Check if _selectedItemIndex has an item in it
        if (_itemSlots[_selectedItemIndex].HasItem())
        {
            // Check if new index is empty, move the old selected item to the new inventory slot?
            if (!_itemSlots[index].HasItem())
            {
                //SwapInventoryItemsAtIndexes(_selectedItemIndex, index);
                SwapInventoryItemsAtIndexes_BodyModManager(_selectedItemIndex, index);
                _itemSlots[_selectedItemIndex].SetIsSelected(false);
                _itemSlots[index].SetIsSelected(false);
                //_selectedItemIndex = 0;
                //SetSelectedItemIndex(0);
                //ResetSelectedItem();
                CheckIfItemDescriptionShouldReset(index);
                return;
            }
        }

        // All other checks failed so de-select current slot and select new slot
        _itemSlots[_selectedItemIndex].SetIsSelected(false);
        _itemSlots[index].SetIsSelected(true);

        //_selectedItemIndex = index;
        SetSelectedItemIndex(index);
    }
    private void InventoryItemSlot_OnAnyItemIsSelected(object sender, EventArgs e)
    {
        InventoryItemSlot selectedItem = sender as InventoryItemSlot;
        if (!selectedItem.IsSelected())
            return;

        if (!selectedItem.HasItem())
        {
            ClearItemDescriptionDetails();
            return;
        }


        SetItemDescriptionDetails(selectedItem.Sprite(), selectedItem.Name(), selectedItem.Description());
    }
    private void InventoryItemSlot_OnAnyItemMousedOver(object sender, int index)
    {
        if (index > _itemSlots.Count)
        {
            return;
        }

        if (_itemSlots[index].HasItem())
        {
            SetItemDescriptionDetails(_itemSlots[index].Sprite(), _itemSlots[index].Name(), _itemSlots[index].Description());
        }
        else
        {
            if (_itemSlots[_selectedItemIndex].IsSelected())
            {
                if (_itemSlots[_selectedItemIndex].HasItem())
                {
                    SetItemDescriptionDetails(_itemSlots[_selectedItemIndex].Sprite(), _itemSlots[_selectedItemIndex].Name(), _itemSlots[_selectedItemIndex].Description());
                }
                else
                {
                    ClearItemDescriptionDetails();
                }
            }
            else
            {
                ClearItemDescriptionDetails();
            }
        }

    }
    private void InventoryItemSlot_OnAnyItemMouseExit(object sender, int index)
    {
        if (_itemSlots[_selectedItemIndex].IsSelected() && _itemSlots[_selectedItemIndex].HasItem())
        {
            SetItemDescriptionDetails(_itemSlots[_selectedItemIndex].Sprite(), _itemSlots[_selectedItemIndex].Name(), _itemSlots[_selectedItemIndex].Description());
        }
        else
        {
            ClearItemDescriptionDetails();
        }
    }
    private void ClearItemDescriptionDetails()
    {
        _itemDescription_Image.enabled = false;
        _itemDescription_NameText.text = "";
        _itemDescription_DescriptionText.text = "";
    }
    private void SetItemDescriptionDetails(Sprite sprite, string name, string description)
    {
        _itemDescription_Image.sprite = sprite;
        _itemDescription_NameText.text = name;
        _itemDescription_DescriptionText.text = description;

        _itemDescription_Image.enabled = true;
    }
    private void CheckIfItemDescriptionShouldReset(int index)
    {
        if (_itemSlots[index].HasItem())
        {
            SetItemDescriptionDetails(_itemSlots[index].Sprite(), _itemSlots[index].Name(), _itemSlots[index].Description());
        }
        else
        {
            ResetSelectedItem();
        }
    }
    private void SwapInventoryItemsAtIndexes(int previousIndex, int newIndex)
    {
        //BodyModByItemSlot previousIndexBodyModItemSlot = _bodyModByItemSlotClass?.First(x => x.SlotIndex == previousIndex);
        //BodyModByItemSlot newIndexBodyModItemSlot = _bodyModByItemSlotClass?.First(x => x.SlotIndex == newIndex);

        //BodyMod_Class previousIndexBodyMod = previousIndexBodyModItemSlot?.BodyMod;
        //BodyMod_Class newIndexBodyMod = newIndexBodyModItemSlot?.BodyMod;

        // dictionary?
        BodyMod_Class previousIndexBodyMod = _bodyModByItemSlot[previousIndex];
        BodyMod_Class newIndexBodyMod = _bodyModByItemSlot[newIndex];

        // array?
        //BodyMod_Class previousIndexBodyMod = _bodyModArray[previousIndex];
        //BodyMod_Class newIndexBodyMod = _bodyModArray[newIndex];

        // swap the item slot contents?
        if (newIndexBodyMod == null)
        {
            _itemSlots[previousIndex].ClearItem();
        }
        else
        {
            _itemSlots[previousIndex].AddItemToSlot(newIndexBodyMod.Sprite(), newIndexBodyMod.Name(), newIndexBodyMod.Description());
            _itemSlots[previousIndex].SetIsEquipped(newIndexBodyMod.IsEquipped());
        }

        if (previousIndexBodyMod == null)
        {
            _itemSlots[newIndex].ClearItem();
        }
        else
        {
            _itemSlots[newIndex].AddItemToSlot(previousIndexBodyMod.Sprite(), previousIndexBodyMod.Name(), previousIndexBodyMod.Description());
            _itemSlots[newIndex].SetIsEquipped(previousIndexBodyMod.IsEquipped());
        }

        // list of classes
        //previousIndexBodyModItemSlot.BodyMod = newIndexBodyMod;
        //newIndexBodyModItemSlot.BodyMod = previousIndexBodyMod;

        // dictionary
        _bodyModByItemSlot[previousIndex] = newIndexBodyMod;
        _bodyModByItemSlot[newIndex] = previousIndexBodyMod;

        // array
        //_bodyModArray[previousIndex] = newIndexBodyMod;
        //_bodyModArray[newIndex] = previousIndexBodyMod;
    }
    private void SwapInventoryItemsAtIndexes_BodyModManager(int previousIndex, int newIndex)
    {
        if (_bodyModManager == null)
            return;

        BodyMod_Class previousIndexBodyMod = _bodyModManager.Inventory_BodyMods()[previousIndex];
        BodyMod_Class newIndexBodyMod = _bodyModManager.Inventory_BodyMods()[newIndex];

        if (newIndexBodyMod == null)
        {
            _itemSlots[previousIndex].ClearItem();
        }
        else
        {
            _itemSlots[previousIndex].AddItemToSlot(newIndexBodyMod.Sprite(), newIndexBodyMod.Name(), newIndexBodyMod.Description());
            _itemSlots[previousIndex].SetIsEquipped(newIndexBodyMod.IsEquipped());
        }

        if (previousIndexBodyMod == null)
        {
            _itemSlots[newIndex].ClearItem();
        }
        else
        {
            _itemSlots[newIndex].AddItemToSlot(previousIndexBodyMod.Sprite(), previousIndexBodyMod.Name(), previousIndexBodyMod.Description());
            _itemSlots[newIndex].SetIsEquipped(previousIndexBodyMod.IsEquipped());
        }

        _bodyModManager.SetInventoryItemAtIndex(previousIndex, newIndexBodyMod, InventoryType.BodyMods);
        _bodyModManager.SetInventoryItemAtIndex(newIndex, previousIndexBodyMod, InventoryType.BodyMods);

    }
}
