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
[Serializable]
public class ItemSlotLocationByBodyPart
{
    public BodyPart BodyPart;
    public List<Transform> Transforms;

    public ItemSlotLocationByBodyPart(BodyPart bodyPart, List<Transform> transform)
    {
        BodyPart = bodyPart;
        Transforms = transform;
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
    [SerializeField] private List<InventoryItemSlot> _inventoryItemSlots = new List<InventoryItemSlot>();
    //private Dictionary<int, BodyMod_Class> _bodyModByItemSlot = new Dictionary<int, BodyMod_Class>();
    //private Dictionary<int, BombRun_Item_Class> _bodyModByItemSlot = new Dictionary<int, BombRun_Item_Class>();
    //[SerializeField] private BodyMod_Class[] _bodyModArray;
    //[SerializeField] private List<BodyModByItemSlot> _bodyModByItemSlotClass = new List<BodyModByItemSlot>();
    
    [SerializeField] private int _numberOfSlots;

    [Header("Equipped Item Slots")]
    [SerializeField] private Transform _equipedItemSlotPrefab;
    private Dictionary<BodyPart, List<InventoryItemSlot>> _equippedItemSlotsByBodyPart = new Dictionary<BodyPart, List<InventoryItemSlot>>();
    [SerializeField] private List<ItemSlotLocationByBodyPart> _equippedItemSlotLocationsByBodyPart = new List<ItemSlotLocationByBodyPart>();

    [Header("Selected Item")]
    [SerializeField] private int _selectedItemIndex = 0;
    [SerializeField] private InventoryItemSlot _selectedItemSlot;

    [Header("Item Description UI")]
    [SerializeField] private Image _itemDescription_Image;
    [SerializeField] private TextMeshProUGUI _itemDescription_NameText;
    [SerializeField] private TextMeshProUGUI _itemDescription_DescriptionText;

    [Header("Inventory Type")]
    [SerializeField] private InventoryType _currentInventoryType;

    [Header("Inventory Type UI")]
    [SerializeField] private Transform _inventoryTypeButtonHolder;
    [SerializeField] private Transform _inventoryTypeButtonPrefab;

    //Events
    public static event EventHandler OnInventoryMenuOpened;
    public static event EventHandler OnInventoryMenuClosed;

    public static event EventHandler<InventoryType> OnCurrentInventoryTypeChanged;


    // Start is called before the first frame update
    void Start()
    {
        CloseInventory();
        //CreateItemSlots();
        InventoryItemSlot.OnAnyItemSlotSingleLeftClickedOn += InventoryItemSlot_OnAnyItemSlotLeftClickedOn;
        InventoryItemSlot.OnAnyItemSlotSingleRightClickedOn += InventoryItemSlot_OnAnyItemSlotSingleRightClickedOn;
        InventoryItemSlot.OnAnyItemIsSelected += InventoryItemSlot_OnAnyItemIsSelected;
        InventoryItemSlot.OnAnyItemMousedOver += InventoryItemSlot_OnAnyItemMousedOver;
        InventoryItemSlot.OnAnyItemMouseExit += InventoryItemSlot_OnAnyItemMouseExit;

        InventoryTypeButton.OnInventoryTypeButtonClicked += InventoryTypeButton_OnInventoryTypeButtonClicked;

        UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged;


        SetCurrentInventoryType(InventoryType.BodyMods);
    }

    

    private void OnDisable()
    {
        InventoryItemSlot.OnAnyItemSlotSingleLeftClickedOn -= InventoryItemSlot_OnAnyItemSlotLeftClickedOn;
        InventoryItemSlot.OnAnyItemSlotSingleRightClickedOn -= InventoryItemSlot_OnAnyItemSlotSingleRightClickedOn;
        InventoryItemSlot.OnAnyItemIsSelected -= InventoryItemSlot_OnAnyItemIsSelected;
        InventoryItemSlot.OnAnyItemMousedOver -= InventoryItemSlot_OnAnyItemMousedOver;
        InventoryItemSlot.OnAnyItemMouseExit -= InventoryItemSlot_OnAnyItemMouseExit;

        InventoryTypeButton.OnInventoryTypeButtonClicked -= InventoryTypeButton_OnInventoryTypeButtonClicked;

        UnitActionSystem.Instance.OnSelectedUnitChanged -= UnitActionSystem_OnSelectedUnitChanged;
    }

    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (_menuOpen)
            {
                if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift))
                {
                    Debug.Log("BodyModInventoryUIManager: Shift + I detected. Changing inventory type...");
                    if (_currentInventoryType == InventoryType.BodyMods)
                        SetCurrentInventoryType(InventoryType.BodyModComponents);
                    else
                        SetCurrentInventoryType(InventoryType.BodyMods);

                    //SetSelectedItemIndex(0);
                    OpenInventory();
                    return;
                }
                CloseInventory();
            }
            else
            {
                SetCurrentInventoryType(InventoryType.BodyMods);
                OpenInventory();
            }
            
        }
    }
    private void InventoryTypeButton_OnInventoryTypeButtonClicked(object sender, InventoryType inventoryType)
    {
        if (inventoryType == InventoryType.None)
            return;

        if (_currentInventoryType == inventoryType)
            return;

        SetCurrentInventoryType(inventoryType);
        OpenInventory();
    }
    private void SetCurrentInventoryType(InventoryType inventoryType)
    {
        if (_currentInventoryType == inventoryType)
            return;

        this._currentInventoryType = inventoryType;
        OnCurrentInventoryTypeChanged?.Invoke(this, _currentInventoryType);
        Debug.Log("SetCurrentInventoryType: OnCurrentInventoryTypeChanged?.Invoke(this, " + _currentInventoryType + ");");
    }
    public InventoryType GetInventoryType()
    {
        return _currentInventoryType;
    }
    private void CloseInventory()
    {
        _bodyModInventoryUIHolder.SetActive(false);
        _menuOpen = false;
        //_selectedItemIndex = 0;
        //SetSelectedItemIndex(0);
        ResetSelectedItem();

        try
        {
            _bodyModManager.OnInventoryItemsUpdated -= BodyModManager_OnInventoryItemsUpdated;
        }
        catch (Exception e)
        {
            Debug.Log("CloseInventory: Could not unsubscribe from _bodyModManager.OnInventoryItemsUpdated. Error: " + e);
        }

        _bodyModManager = null;
        OnInventoryMenuClosed?.Invoke(this, EventArgs.Empty);
    }
    private void OpenInventory()
    {
        SetSelectedItemIndex(0);

        BombRunUnit unit = UnitActionSystem.Instance.GetSelectedUnit();
        if (unit == null)
            return;

        _bodyModInventoryUIHolder.SetActive(true);
        _menuOpen = true;

        DestroyInventoryItemSlots();
        DestroyEquippedItemSlots();
        //ResetBodyModByItemSlot();
        SetBodyModManager(unit.BodyModManager());
        ClearItemDescriptionDetails();
        //CreateItemSlots(_bodyModManager.MaxInventoryCount());
        CreateInventoryItemSlots(_bodyModManager.InventorySlotCount(_currentInventoryType));
        CreateEquippedItemSlots(_bodyModManager);
        GetInventoryItems(_bodyModManager);
        GetEquippedInventoryItems(_bodyModManager);

        OnInventoryMenuOpened?.Invoke(this, EventArgs.Empty);
    }
    private void DestroyInventoryItemSlots()
    {
        for (int i = 0; i < _inventoryItemSlots.Count; i++)
        {
            Destroy(_inventoryItemSlots[i].gameObject);
        }
        _inventoryItemSlots.Clear();
    }
    private void DestroyEquippedItemSlots()
    {
        foreach (KeyValuePair<BodyPart, List<InventoryItemSlot>> equippedItemSlots in _equippedItemSlotsByBodyPart)
        {
            for (int i = 0; i < equippedItemSlots.Value.Count; i++)
            {
                Destroy(equippedItemSlots.Value[i].gameObject);
            }
            equippedItemSlots.Value.Clear();
        }
        _equippedItemSlotsByBodyPart.Clear();
    }
    //private void ResetBodyModByItemSlot()
    //{
    //    _bodyModByItemSlot.Clear();
    //    //_bodyModByItemSlotClass.Clear();
    //}
    private void SetBodyModManager(BombRunUnitBodyModManager bodyModManager)
    {
        this._bodyModManager = bodyModManager;
        if (_bodyModManager != null)
        {
            _bodyModManager.OnInventoryItemsUpdated += BodyModManager_OnInventoryItemsUpdated;
        }
    }

    private void BodyModManager_OnInventoryItemsUpdated(object sender, EventArgs e)
    {
        Debug.Log("BodyModManager_OnInventoryItemsUpdated: ");

        if (_bodyModManager == null)
            return;

        //DestroyInventoryItemSlots();
        //DestroyEquippedItemSlots();

        //CreateInventoryItemSlots(_bodyModManager.InventorySlotCount(_currentInventoryType));
        //CreateEquippedItemSlots(_bodyModManager);

        GetInventoryItems(_bodyModManager);
        GetEquippedInventoryItems(_bodyModManager);
    }

    private void CreateInventoryItemSlots(int numberOfSlots)
    {
        //Array.Resize(ref _bodyModArray, numberOfSlots);
        for (int i = 0; i < numberOfSlots; i++)
        {
            Transform newItemSlot = Instantiate(_itemSlotPrefab, _itemSlotsHolder);
            InventoryItemSlot itemSlot = newItemSlot.GetComponent<InventoryItemSlot>();
            itemSlot.SetSlotIndex(i);
            _inventoryItemSlots.Add(itemSlot);
            itemSlot.ClearItem();

        }
    }
    private void CreateEquippedItemSlots(BombRunUnitBodyModManager bodyModManager)
    {
        foreach (ItemSlotLocationByBodyPart itemSlotLocation in _equippedItemSlotLocationsByBodyPart)
        {
            if (bodyModManager.GetMaxEquippedSlotsForBodyPart(itemSlotLocation.BodyPart) < 1)
                continue;


            Debug.Log("CreateEquippedItemSlots: Creating " + bodyModManager.GetMaxEquippedSlotsForBodyPart(itemSlotLocation.BodyPart) + " slots for " + itemSlotLocation.BodyPart);
            for (int i = 0; i < bodyModManager.GetMaxEquippedSlotsForBodyPart(itemSlotLocation.BodyPart); i++)
            {
                Transform newItemSlot = Instantiate(_equipedItemSlotPrefab, GetEquippedItemSlotLocation(itemSlotLocation.BodyPart, i));
                InventoryItemSlot itemSlot = newItemSlot.GetComponent<InventoryItemSlot>();
                itemSlot.SetSlotIndex(i);

                if (!_equippedItemSlotsByBodyPart.ContainsKey(itemSlotLocation.BodyPart))
                {
                    _equippedItemSlotsByBodyPart.Add(itemSlotLocation.BodyPart, new List<InventoryItemSlot>());
                }

                _equippedItemSlotsByBodyPart[itemSlotLocation.BodyPart].Add(itemSlot);
                itemSlot.ClearItem();
            }
        }
    }
    private Transform GetEquippedItemSlotLocation(BodyPart bodyPart, int index)
    {
        //Debug.Log("GetEquippedItemSlotLocation: " + bodyPart + " : " + index);
        if (_equippedItemSlotLocationsByBodyPart.First(x => x.BodyPart == bodyPart) != null)
        {
            ItemSlotLocationByBodyPart itemSlotLocationByBodyPart = _equippedItemSlotLocationsByBodyPart.First(x => x.BodyPart == bodyPart);
            int numberOfSlotLocations = itemSlotLocationByBodyPart.Transforms.Count();

            // if there is only one slot location, return first value regardless of what the index is
            if (numberOfSlotLocations == 1)
            {
                return itemSlotLocationByBodyPart.Transforms[0];
            }

            if (index < numberOfSlotLocations)
            {
                return itemSlotLocationByBodyPart.Transforms[index];
            }

            int originalIndex = index;
            for (int i = 0; i < (originalIndex / itemSlotLocationByBodyPart.Transforms.Count()); i++)
            {
                index -= itemSlotLocationByBodyPart.Transforms.Count();
            }

            if (index < numberOfSlotLocations)
            {
                return itemSlotLocationByBodyPart.Transforms[index];
            }
        }

        return null;
    }
    private void GetInventoryItems(BombRunUnitBodyModManager bodyModManager)
    {

        for (int i = 0; i < _inventoryItemSlots.Count; i++)
        {
            //if (unit.BodyModManager().Inventory_BodyMods()[i] == null)
            if (bodyModManager.GetInventoryByType(_currentInventoryType)[i] == null)
            {
                _inventoryItemSlots[i].ClearItem();
                Debug.Log("GetInventoryItems: item NOT FOUND at index: " + i + " . Clearing...");
            }
            else
            {
                //BodyMod_Class bodyMod = unit.BodyModManager().Inventory_BodyMods()[i];
                BombRun_Item_Class item = bodyModManager.GetInventoryByType(_currentInventoryType)[i];
                //_inventoryItemSlots[i].AddItemToSlot(item.Sprite(), item.Name(), item.Description(), item, item.StackSize());
                _inventoryItemSlots[i].AddItemToSlot(item);
                //_inventoryItemSlots[i].SetIsEquipped(item.IsEquipped());
                Debug.Log("GetInventoryItems: item found at index: " + i);
            }
        }
    }
    private void GetEquippedInventoryItems(BombRunUnitBodyModManager bodyModManager)
    {
        foreach (KeyValuePair<BodyPart, List<InventoryItemSlot>> _equippedItemSlots in _equippedItemSlotsByBodyPart)
        {
            foreach (InventoryItemSlot inventoryItemSlot in _equippedItemSlots.Value)
            {
                inventoryItemSlot.AddItemToSlot(bodyModManager.GetEquippedItemAtIndex(_equippedItemSlots.Key, inventoryItemSlot.SlotIndex()));

                //BombRun_Item_Class item = _bodyModManager.GetEquippedItemAtIndex(_equippedItemSlots.Key, inventoryItemSlot.SlotIndex());
                //if (item == null)
                //{
                //    inventoryItemSlot.ClearItem();
                //}
                //else
                //{
                //    //inventoryItemSlot.AddItemToSlot(item.Sprite(), item.Name(), item.Description(), item, item.StackSize());
                //    inventoryItemSlot.AddItemToSlot(item);
                //}
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
    private void InventoryItemSlot_OnAnyItemSlotLeftClickedOn(object sender, int index)
    {
        if (!_menuOpen)
            return;
        if (index < 0)
            return;
        if (index >= _inventoryItemSlots.Count)
            return;

        if (_selectedItemIndex == index)
        {
            if (_inventoryItemSlots[index].IsSelected())
            {
                if (_bodyModManager.GetInventoryItemAtIndex(index, _currentInventoryType) != null)
                {
                    _bodyModManager.EquipInventoryItem(_currentInventoryType, index);
                }
                else
                {
                    Debug.Log("InventoryItemSlot_OnAnyItemSlotLeftClickedOn: Could not find selected item in inventory. Index: " + index + " Inventory type: " + _currentInventoryType);
                }
            }
            _inventoryItemSlots[_selectedItemIndex].SetIsSelected(!_inventoryItemSlots[_selectedItemIndex].IsSelected());
            CheckIfItemDescriptionShouldReset(_selectedItemIndex);
            return;
        }

        // make sure the inventorySlot at the selected index is currently selected
        // if it isn't selected, treat as "stale" and just select the new inventory slot
        if (!_inventoryItemSlots[_selectedItemIndex].IsSelected())
        {
            _inventoryItemSlots[index].SetIsSelected(true);
            SetSelectedItemIndex(index);
            return;
        }

        // Check to see if player is trying to move an inventory item to a new slot
        // Check if _selectedItemIndex has an item in it
        if (_inventoryItemSlots[_selectedItemIndex].HasItem())
        {
            // Check if new index is empty, move the old selected item to the new inventory slot?
            if (!_inventoryItemSlots[index].HasItem())
            {
                //SwapInventoryItemsAtIndexes(_selectedItemIndex, index);
                SwapInventoryItemsAtIndexes_BodyModManager(_selectedItemIndex, index);
                _inventoryItemSlots[_selectedItemIndex].SetIsSelected(false);
                _inventoryItemSlots[index].SetIsSelected(false);
                //_selectedItemIndex = 0;
                //SetSelectedItemIndex(0);
                //ResetSelectedItem();
                CheckIfItemDescriptionShouldReset(index);
                return;
            }
        }

        // All other checks failed so de-select current slot and select new slot
        _inventoryItemSlots[_selectedItemIndex].SetIsSelected(false);
        _inventoryItemSlots[index].SetIsSelected(true);

        SetSelectedItemIndex(index);
    }
    private void InventoryItemSlot_OnAnyItemSlotSingleRightClickedOn(object sender, int index)
    {
        if (index != _selectedItemIndex)
        {
            if (_inventoryItemSlots[_selectedItemIndex].IsSelected())
            {
                _inventoryItemSlots[_selectedItemIndex].SetIsSelected(false);
            }
            CheckIfItemDescriptionShouldReset(index);
        }
        else
        {
            if (_inventoryItemSlots[index].IsSelected())
            {
                if (_inventoryItemSlots[index].IsEquipped())
                {
                    _bodyModManager.UnEquipItemAtIndex(index, _currentInventoryType);
                }
                else
                {
                    _bodyModManager.DropItemAtIndex(index, _currentInventoryType);
                }
            }
            _inventoryItemSlots[_selectedItemIndex].SetIsSelected(false);
            CheckIfItemDescriptionShouldReset(index);
        }
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
        if (index > _inventoryItemSlots.Count)
        {
            return;
        }

        InventoryItemSlot inventorySlot = sender as InventoryItemSlot;

        if (inventorySlot.HasItem())
        {
            SetItemDescriptionDetails(inventorySlot.Sprite(), inventorySlot.Name(), inventorySlot.Description());
        }
        else
        {
            if (_inventoryItemSlots[_selectedItemIndex].IsSelected())
            {
                if (_inventoryItemSlots[_selectedItemIndex].HasItem())
                {
                    SetItemDescriptionDetails(_inventoryItemSlots[_selectedItemIndex].Sprite(), _inventoryItemSlots[_selectedItemIndex].Name(), _inventoryItemSlots[_selectedItemIndex].Description());
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

        //if (_inventoryItemSlots[index].HasItem())
        //{
        //    SetItemDescriptionDetails(_inventoryItemSlots[index].Sprite(), _inventoryItemSlots[index].Name(), _inventoryItemSlots[index].Description());
        //}
        //else
        //{
        //    if (_inventoryItemSlots[_selectedItemIndex].IsSelected())
        //    {
        //        if (_inventoryItemSlots[_selectedItemIndex].HasItem())
        //        {
        //            SetItemDescriptionDetails(_inventoryItemSlots[_selectedItemIndex].Sprite(), _inventoryItemSlots[_selectedItemIndex].Name(), _inventoryItemSlots[_selectedItemIndex].Description());
        //        }
        //        else
        //        {
        //            ClearItemDescriptionDetails();
        //        }
        //    }
        //    else
        //    {
        //        ClearItemDescriptionDetails();
        //    }
        //}

    }
    private void InventoryItemSlot_OnAnyItemMouseExit(object sender, int index)
    {
        if (_inventoryItemSlots[_selectedItemIndex].IsSelected() && _inventoryItemSlots[_selectedItemIndex].HasItem())
        {
            SetItemDescriptionDetails(_inventoryItemSlots[_selectedItemIndex].Sprite(), _inventoryItemSlots[_selectedItemIndex].Name(), _inventoryItemSlots[_selectedItemIndex].Description());
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
        if (_inventoryItemSlots[index].HasItem())
        {
            SetItemDescriptionDetails(_inventoryItemSlots[index].Sprite(), _inventoryItemSlots[index].Name(), _inventoryItemSlots[index].Description());
        }
        else
        {
            ResetSelectedItem();
        }
    }
    //private void SwapInventoryItemsAtIndexes(int previousIndex, int newIndex)
    //{
    //    //BodyModByItemSlot previousIndexBodyModItemSlot = _bodyModByItemSlotClass?.First(x => x.SlotIndex == previousIndex);
    //    //BodyModByItemSlot newIndexBodyModItemSlot = _bodyModByItemSlotClass?.First(x => x.SlotIndex == newIndex);

    //    //BodyMod_Class previousIndexBodyMod = previousIndexBodyModItemSlot?.BodyMod;
    //    //BodyMod_Class newIndexBodyMod = newIndexBodyModItemSlot?.BodyMod;

    //    // dictionary?
    //    //BodyMod_Class previousIndexBodyMod = _bodyModByItemSlot[previousIndex];
    //    //BodyMod_Class newIndexBodyMod = _bodyModByItemSlot[newIndex];
    //    BombRun_Item_Class previousIndexBodyMod = _bodyModByItemSlot[previousIndex];
    //    BombRun_Item_Class newIndexBodyMod = _bodyModByItemSlot[newIndex];

    //    // array?
    //    //BodyMod_Class previousIndexBodyMod = _bodyModArray[previousIndex];
    //    //BodyMod_Class newIndexBodyMod = _bodyModArray[newIndex];

    //    // swap the item slot contents?
    //    if (newIndexBodyMod == null)
    //    {
    //        _itemSlots[previousIndex].ClearItem();
    //    }
    //    else
    //    {
    //        _itemSlots[previousIndex].AddItemToSlot(newIndexBodyMod.Sprite(), newIndexBodyMod.Name(), newIndexBodyMod.Description());
    //        _itemSlots[previousIndex].SetIsEquipped((newIndexBodyMod as BodyMod_Class).IsEquipped());
    //    }

    //    if (previousIndexBodyMod == null)
    //    {
    //        _itemSlots[newIndex].ClearItem();
    //    }
    //    else
    //    {
    //        _itemSlots[newIndex].AddItemToSlot(previousIndexBodyMod.Sprite(), previousIndexBodyMod.Name(), previousIndexBodyMod.Description());
    //        _itemSlots[newIndex].SetIsEquipped((previousIndexBodyMod as BodyMod_Class).IsEquipped());
    //    }

    //    // list of classes
    //    //previousIndexBodyModItemSlot.BodyMod = newIndexBodyMod;
    //    //newIndexBodyModItemSlot.BodyMod = previousIndexBodyMod;

    //    // dictionary
    //    _bodyModByItemSlot[previousIndex] = newIndexBodyMod;
    //    _bodyModByItemSlot[newIndex] = previousIndexBodyMod;

    //    // array
    //    //_bodyModArray[previousIndex] = newIndexBodyMod;
    //    //_bodyModArray[newIndex] = previousIndexBodyMod;
    //}
    private void SwapInventoryItemsAtIndexes_BodyModManager(int previousIndex, int newIndex)
    {
        if (_bodyModManager == null)
            return;

        //BodyMod_Class previousIndexBodyMod = _bodyModManager.Inventory_BodyMods()[previousIndex];
        //BodyMod_Class newIndexBodyMod = _bodyModManager.Inventory_BodyMods()[newIndex];

        //BombRun_Item_Class previousIndexBodyMod = _bodyModManager.Inventory_BodyMods()[previousIndex];
        //BombRun_Item_Class newIndexBodyMod = _bodyModManager.Inventory_BodyMods()[newIndex];

        BombRun_Item_Class previousIndexBodyMod = _bodyModManager.GetInventoryItemAtIndex(previousIndex, _currentInventoryType);
        BombRun_Item_Class newIndexBodyMod = _bodyModManager.GetInventoryItemAtIndex(newIndex, _currentInventoryType);

        if (newIndexBodyMod == null)
        {
            _inventoryItemSlots[previousIndex].ClearItem();
        }
        else
        {
            //_inventoryItemSlots[previousIndex].AddItemToSlot(newIndexBodyMod.Sprite(), newIndexBodyMod.Name(), newIndexBodyMod.Description(), newIndexBodyMod, newIndexBodyMod.StackSize());
            _inventoryItemSlots[previousIndex].AddItemToSlot(newIndexBodyMod);
            _inventoryItemSlots[previousIndex].SetIsEquipped(newIndexBodyMod.IsEquipped());
        }

        if (previousIndexBodyMod == null)
        {
            _inventoryItemSlots[newIndex].ClearItem();
        }
        else
        {
            //_inventoryItemSlots[newIndex].AddItemToSlot(previousIndexBodyMod.Sprite(), previousIndexBodyMod.Name(), previousIndexBodyMod.Description(), previousIndexBodyMod, previousIndexBodyMod.StackSize());
            _inventoryItemSlots[newIndex].AddItemToSlot(previousIndexBodyMod);
            _inventoryItemSlots[newIndex].SetIsEquipped(previousIndexBodyMod.IsEquipped());
        }

        _bodyModManager.SetInventoryItemAtIndex(previousIndex, newIndexBodyMod, _currentInventoryType);
        _bodyModManager.SetInventoryItemAtIndex(newIndex, previousIndexBodyMod, _currentInventoryType);

    }
    private void UnitActionSystem_OnSelectedUnitChanged(object sender, BombRunUnit unit)
    {
        if (!_menuOpen)
            return;

        if (unit == null)
            return;

        OpenInventory();
    }
    
}
