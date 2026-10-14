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
    private BombRunUnitStatManager _statManager;

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

    [Header("Unit Stats UI")]
    [SerializeField] private TextMeshProUGUI _unitStatsText;
    [SerializeField] private TextMeshProUGUI _unitNoiseText;

    //Events
    public static event EventHandler OnInventoryMenuOpened;
    public static event EventHandler OnInventoryMenuClosed;

    public static event EventHandler<InventoryType> OnCurrentInventoryTypeChanged;
    public static event EventHandler<InventoryItemSlot> OnInventoryItemSlotSelected;


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
        //BodyModInventoryUIManager.OnInventoryItemSlotSelected += BodyModInventoryUIManager_OnInventoryItemSlotSelected;

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
        //BodyModInventoryUIManager.OnInventoryItemSlotSelected -= BodyModInventoryUIManager_OnInventoryItemSlotSelected;

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
            _statManager.OnAnyStatChanged -= StatManager_OnAnyStatChanged;
        }
        catch (Exception e)
        {
            Debug.Log("CloseInventory: Could not unsubscribe from _bodyModManager.OnInventoryItemsUpdated. Error: " + e);
        }

        //_bodyModManager = null;
        SetBodyModManager(null);
        SetStatManager(null);

        OnInventoryMenuClosed?.Invoke(this, EventArgs.Empty);
    }

    

    private void OpenInventory()
    {
        SetSelectedItemIndex(0);
        SetSelectedItemSlot(null);

        BombRunUnit unit = UnitActionSystem.Instance.GetSelectedUnit();
        if (unit == null)
            return;

        _bodyModInventoryUIHolder.SetActive(true);
        _menuOpen = true;

        DestroyInventoryItemSlots();
        DestroyEquippedItemSlots();
        //ResetBodyModByItemSlot();

        SetBodyModManager(unit.BodyModManager());
        SetStatManager(unit.StatManager());

        ClearItemDescriptionDetails();
        //CreateItemSlots(_bodyModManager.MaxInventoryCount());
        CreateInventoryItemSlots(_bodyModManager.InventorySlotCount(_currentInventoryType));
        CreateEquippedItemSlots(_bodyModManager);
        GetInventoryItems(_bodyModManager);
        GetEquippedInventoryItems(_bodyModManager);

        UpdateUnitStats(_statManager);
        UpdateUnitNoiseStats(_bodyModManager);

        OnInventoryMenuOpened?.Invoke(this, EventArgs.Empty);
    }
    private void DestroyInventoryItemSlots()
    {
        Debug.Log("DestroyInventoryItemSlots");
        for (int i = 0; i < _inventoryItemSlots.Count; i++)
        {
            Destroy(_inventoryItemSlots[i].gameObject);
        }
        _inventoryItemSlots.Clear();
    }
    private void DestroyEquippedItemSlots()
    {
        Debug.Log("DestroyEquippedItemSlots");
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
    private void SetBodyModManager(BombRunUnitBodyModManager bodyModManager)
    {
        this._bodyModManager = bodyModManager;
        if (_bodyModManager != null)
        {
            _bodyModManager.OnInventoryItemsUpdated += BodyModManager_OnInventoryItemsUpdated;
        }
    }
    private void SetStatManager(BombRunUnitStatManager statManager)
    {
        this._statManager = statManager;
        if (_statManager != null)
        {
            _statManager.OnAnyStatChanged += StatManager_OnAnyStatChanged;
        }
    }

    

    private void BodyModManager_OnInventoryItemsUpdated(object sender, EventArgs e)
    {
        Debug.Log("BodyModManager_OnInventoryItemsUpdated: ");

        if (_bodyModManager == null)
        {
            Debug.Log("BodyModManager_OnInventoryItemsUpdated: _bodyModManager == null");
            return;
        }
           

        DestroyInventoryItemSlots();
        DestroyEquippedItemSlots();

        CreateInventoryItemSlots(_bodyModManager.InventorySlotCount(_currentInventoryType));
        CreateEquippedItemSlots(_bodyModManager);

        GetInventoryItems(_bodyModManager);
        GetEquippedInventoryItems(_bodyModManager);

        UpdateUnitStats(_statManager);
        UpdateUnitNoiseStats(_bodyModManager);
    }
    private void StatManager_OnAnyStatChanged(object sender, EventArgs e)
    {
        UpdateUnitStats(_statManager);
    }
    private void CreateInventoryItemSlots(int numberOfSlots)
    {
        Debug.Log("CreateInventoryItemSlots: " + numberOfSlots) ;
        //Array.Resize(ref _bodyModArray, numberOfSlots);
        for (int i = 0; i < numberOfSlots; i++)
        {
            Transform newItemSlot = Instantiate(_itemSlotPrefab, _itemSlotsHolder);
            InventoryItemSlot itemSlot = newItemSlot.GetComponent<InventoryItemSlot>();
            itemSlot.SetSlotIndex(i);
            _inventoryItemSlots.Add(itemSlot);
            //itemSlot.ClearItem();

        }
    }
    private void CreateEquippedItemSlots(BombRunUnitBodyModManager bodyModManager)
    {
        Debug.Log("CreateEquippedItemSlots");
        foreach (ItemSlotLocationByBodyPart itemSlotLocation in _equippedItemSlotLocationsByBodyPart)
        {
            if (bodyModManager.GetMaxEquippedSlotsForBodyPart(itemSlotLocation.BodyPart) < 1)
                continue;


            //Debug.Log("CreateEquippedItemSlots: Creating " + bodyModManager.GetMaxEquippedSlotsForBodyPart(itemSlotLocation.BodyPart) + " slots for " + itemSlotLocation.BodyPart);
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
                //itemSlot.ClearItem();
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
            BombRun_Item_Class item = bodyModManager.GetInventoryByType(_currentInventoryType)[i];
            _inventoryItemSlots[i].AddItemToSlot(item);
        }
    }
    private void GetEquippedInventoryItems(BombRunUnitBodyModManager bodyModManager)
    {
        Debug.Log("GetEquippedInventoryItems");
        foreach (KeyValuePair<BodyPart, List<InventoryItemSlot>> _equippedItemSlots in _equippedItemSlotsByBodyPart)
        {
            foreach (InventoryItemSlot inventoryItemSlot in _equippedItemSlots.Value)
            {
                inventoryItemSlot.AddItemToSlot(bodyModManager.GetEquippedItemAtIndex(_equippedItemSlots.Key, inventoryItemSlot.SlotIndex()), true, _equippedItemSlots.Key);
            }
        }
    }
    private void UpdateUnitStats(BombRunUnitStatManager statManager)
    {
        _unitStatsText.text = "Stats:\n\n";
        if (ReferenceEquals(statManager, null))
        {            
            return;
        }

        foreach (StatType statType in statManager.GetAllStatTypes())
        {
            float statValue = statManager.GetStatValueByType(statType);
            string statName = statManager.StatTypeToDisplayName(statType);

            _unitStatsText.text += statName + ":\t" + statValue + "\n";
        }

    }
    private void UpdateUnitNoiseStats(BombRunUnitBodyModManager bodyModManager)
    {
        _unitNoiseText.text = "Noise:\n\n";
        if (ReferenceEquals(bodyModManager, null))
        {
            return;
        }
        foreach (BodyPart bodyPart in bodyModManager.GetAllBodyPartsWithInventories())
        {
            float additiveNoiseValue = bodyModManager.GetAdditiveNoiseModifierFromBodyMods(bodyPart);
            float multiplyNoiseValue = bodyModManager.GetMultiplyingNoiseModifierFromBodyMods(bodyPart);

            string plusOrMinus = "+";
            if (additiveNoiseValue < 0)
                plusOrMinus = "";

            _unitNoiseText.text += bodyPart + "\t" + plusOrMinus + additiveNoiseValue + "\t" + "x" + multiplyNoiseValue + "\n";
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
    private void SetSelectedItemSlot(InventoryItemSlot inventorySlot)
    {
        if (inventorySlot != null)
        {
            if (!inventorySlot.HasItem())
                inventorySlot = null;
        }
        this._selectedItemSlot = inventorySlot;
        OnInventoryItemSlotSelected?.Invoke(this, this._selectedItemSlot);
    }
    private void SetIsSelectedForIventoryItemSlot(InventoryItemSlot inventorySlot, bool isSelected)
    {
        if (ReferenceEquals(inventorySlot, null))
            return;

        inventorySlot.SetIsSelected(isSelected);
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

        InventoryItemSlot clickedOnSlot = sender as InventoryItemSlot;
        if (clickedOnSlot == null)
            return;

        if (_selectedItemSlot == clickedOnSlot)
        {
            Debug.Log("InventoryItemSlot_OnAnyItemSlotLeftClickedOn: _selectedItemSlot == clickedOnSlot");
            if (clickedOnSlot.IsSelected())
            {
                if (!clickedOnSlot.IsEquipped())
                {
                    if (_bodyModManager.GetInventoryItemAtIndex(clickedOnSlot.SlotIndex(), _currentInventoryType) != null)
                    {
                        _bodyModManager.EquipInventoryItem(_currentInventoryType, clickedOnSlot.SlotIndex());
                    }
                }
                else
                {
                    if (_bodyModManager.GetEquippedItemAtIndex(clickedOnSlot.GetBodyPart(), clickedOnSlot.SlotIndex()) != null)
                    {
                        _bodyModManager.UnEquipItemAtIndex(clickedOnSlot.GetBodyPart(), clickedOnSlot.SlotIndex());
                    }
                }
            }

            if (_selectedItemSlot != null)
                _selectedItemSlot.SetIsSelected(!_selectedItemSlot.IsSelected());

            if (_selectedItemSlot.IsSelected())
                SetSelectedItemSlot(_selectedItemSlot);
            else
                SetSelectedItemSlot(null);

            CheckIfItemDescriptionShouldReset(_selectedItemIndex);
            return;
        }

        // make sure the inventorySlot at the selected index is currently selected
        // if it isn't selected, treat as "stale" and just select the new inventory slot
        //if (!_inventoryItemSlots[_selectedItemIndex].IsSelected())
        if (ReferenceEquals(_selectedItemSlot, null))
        {
            Debug.Log("InventoryItemSlot_OnAnyItemSlotLeftClickedOn: ReferenceEquals(_selectedItemSlot, null)");

            SetSelectedItemIndex(index);
            //clickedOnSlot.SetIsSelected(true);
            SetSelectedItemSlot(clickedOnSlot);
            return;
        }
        if (!_selectedItemSlot.IsSelected())
        {
            Debug.Log("InventoryItemSlot_OnAnyItemSlotLeftClickedOn: !_selectedItemSlot.IsSelected()");
            SetSelectedItemIndex(index);
            SetSelectedItemSlot(clickedOnSlot);
            return;
        }

        // If neither new or selected slot have items, just selected the new one
        if (!_selectedItemSlot.HasItem())
        {
            Debug.Log("!_selectedItemSlot.HasItem()");
            SetSelectedItemIndex(index);
            SetSelectedItemSlot(clickedOnSlot);
            return;
        }

        // if either slot has an item, swap them
        // If _selectedItemSlot is inventory, and clickedOnSlot is inventory, swap the items (whether one is null or not)
        // If _selectedItemSlot is inventory, and clickedOnSlot is equipped slot, check if item at _selectedItemSlot can be equipped there
        // // if clickedOnSlot has an item equipped, swap the equipped item for the inventory item (if equipped inventory types match)

        if (_selectedItemSlot.HasItem() || clickedOnSlot.HasItem())
        {
            SwapInventoryItemsAtIndexes_BodyModManager(_selectedItemSlot, clickedOnSlot);
            SetSelectedItemSlot(null);

            CheckIfItemDescriptionShouldReset(clickedOnSlot);
            return;
        }

        SetSelectedItemSlot(clickedOnSlot);
        SetSelectedItemIndex(index);
    }
    private void InventoryItemSlot_OnAnyItemSlotSingleRightClickedOn(object sender, int index)
    {
        if (!_menuOpen)
            return;
        if (index < 0)
            return;

        InventoryItemSlot clickedOnSlot = sender as InventoryItemSlot;
        if (clickedOnSlot == null)
            return;


        if (clickedOnSlot != _selectedItemSlot)
        {
            SetSelectedItemSlot(null);
            CheckIfItemDescriptionShouldReset(clickedOnSlot);
        }
        else
        {
            if (_selectedItemSlot != null)
            {
                if (_selectedItemSlot.IsSelected() && clickedOnSlot.HasItem())
                {
                    if (clickedOnSlot.IsEquippedSlot())
                    {
                        _bodyModManager.UnEquipItemAtIndex(clickedOnSlot.GetBodyPart(), clickedOnSlot.SlotIndex());
                    }
                    else 
                    {
                        _bodyModManager.DropItemAtIndex(clickedOnSlot.SlotIndex(), _currentInventoryType);
                    }
                }
                SetSelectedItemSlot(null);
                CheckIfItemDescriptionShouldReset(clickedOnSlot);
            }
            
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

    }
    private void InventoryItemSlot_OnAnyItemMouseExit(object sender, int index)
    {
        if (_selectedItemSlot == null)
        {
            ClearItemDescriptionDetails();
            return;
        }

        if (_selectedItemSlot.IsSelected() && _selectedItemSlot.HasItem())
        {
            SetItemDescriptionDetails(_selectedItemSlot.Sprite(), _selectedItemSlot.Name(), _selectedItemSlot.Description());
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
    private void CheckIfItemDescriptionShouldReset(InventoryItemSlot inventorySlot)
    {
        if (ReferenceEquals(inventorySlot, null))
        {
            ResetSelectedItem();

        }            
        else
        {
            if (inventorySlot.HasItem())
            {
                SetItemDescriptionDetails(inventorySlot.Sprite(), inventorySlot.Name(), inventorySlot.Description());
            }
        }
    }
    private void SwapInventoryItemsAtIndexes_BodyModManager(InventoryItemSlot previouslySelectedSlot, InventoryItemSlot newlySelectedSlot)
    {
        Debug.Log("SwapInventoryItemsAtIndexes_BodyModManager: ");
        if (_bodyModManager == null)
            return;

        if (ReferenceEquals(previouslySelectedSlot, null) || ReferenceEquals(newlySelectedSlot, null))
        {
            return;
        }
        Debug.Log("SwapInventoryItemsAtIndexes_BodyModManager: previouslySelectedSlot: " + previouslySelectedSlot.SlotIndex() + " newlySelectedSlot: " + newlySelectedSlot.SlotIndex());


        //previouslySelectedSlot is inventory, newlySelectedSlot is inventory:
        // swap the inventory items(_bodyModManager.SwapInventoryItemsAtIndex)
        if (!previouslySelectedSlot.IsEquippedSlot() && !newlySelectedSlot.IsEquippedSlot())
        {
            Debug.Log("SwapInventoryItemsAtIndexes_BodyModManager: swapping inventory items at indices: " + previouslySelectedSlot.SlotIndex() + ":" + newlySelectedSlot.SlotIndex());
            _bodyModManager.SwapInventoryItemsAtIndex(_currentInventoryType, previouslySelectedSlot.SlotIndex(), newlySelectedSlot.SlotIndex());
            return;
        }

        // previouslySelectedSlot is inventory, newlySelectedSlot is equipped slot:
        // If the new slot is an equipment slot, try to equip that item at at slot
        if (!previouslySelectedSlot.IsEquippedSlot() && newlySelectedSlot.IsEquippedSlot())
        {
            // check if previouslySelectedSlot's body part matches newlySelectedSlot body part
            if (newlySelectedSlot.GetBodyPart() != _bodyModManager.GetInventoryItemAtIndex(previouslySelectedSlot.SlotIndex(), _currentInventoryType).BodyPart())
            {
                Debug.Log("SwapInventoryItemsAtIndexes_BodyModManager: Body parts do not match. Equipped Slot: " + newlySelectedSlot.GetBodyPart() + " item body part: " + _bodyModManager.GetInventoryItemAtIndex(previouslySelectedSlot.SlotIndex(), _currentInventoryType).BodyPart());
                return;
            }

            // If newlySelectedSlot does not have item:
            // // equip previouslySelectedSlot to newlySelectedSlot(_bodyModManager.EquipInventoryItem)
            if (!newlySelectedSlot.HasItem())
            {
                _bodyModManager.EquipInventoryItemAtEquippedIndex(_currentInventoryType, previouslySelectedSlot.SlotIndex(), newlySelectedSlot.SlotIndex());
                return;
            }
            // If newlySelectedSlot DOES have item:
            // // swap previouslySelectedSlot item into newlySelectedSlot's equipped slot, then newlySelectedSlot into previouslySelectedSlot's inventory slot(SwapInventoryItemAtIndexForEquippedItemAtIndex)
            else
            {
                _bodyModManager.SwapInventoryItemAtIndexForEquippedItemAtIndex(_currentInventoryType, previouslySelectedSlot.SlotIndex(), newlySelectedSlot.GetBodyPart(), newlySelectedSlot.SlotIndex());
                return;
            }
        }

        // previouslySelectedSlot is equipped slot, newlySelectedSlot is inventory:	
        if (previouslySelectedSlot.IsEquippedSlot() && !newlySelectedSlot.IsEquippedSlot())
        {
            // If newlySelectedSlot does have item:
            // // swap the two previouslySelectedSlot equipped item into inventory, and newlySelectedSlot inventory item into equipped slot
            // // // SwapInventoryItemAtIndexForEquippedItemAtIndex checks that the body parts match
            if (newlySelectedSlot.HasItem())
            {
                _bodyModManager.SwapInventoryItemAtIndexForEquippedItemAtIndex(_currentInventoryType, newlySelectedSlot.SlotIndex(), previouslySelectedSlot.GetBodyPart(), previouslySelectedSlot.SlotIndex());
                return;
            }
            // if newlySelectedSlot does NOT have item:
            // // unequip previouslySelectedSlot and add to inventory at newlySelectedSlot index
            else
            {
                _bodyModManager.UnEquipItemAtIndex(previouslySelectedSlot.GetBodyPart(), previouslySelectedSlot.SlotIndex(), _currentInventoryType, newlySelectedSlot.SlotIndex());
                return;
            }
        }
        // previouslySelectedSlot is equipped slot, newlySelectedSlot is equipped slot:
        if (previouslySelectedSlot.IsEquippedSlot() && newlySelectedSlot.IsEquippedSlot())
        {
            Debug.Log("SwapInventoryItemsAtIndexes_BodyModManager: previouslySelectedSlot.IsEquippedSlot() && newlySelectedSlot.IsEquippedSlot()");
            // check if the body parts match
            if (previouslySelectedSlot.GetBodyPart() != newlySelectedSlot.GetBodyPart())
            {
                return;
            }
            _bodyModManager.SwapEquippedItemsAtIndices(previouslySelectedSlot.GetBodyPart(), previouslySelectedSlot.SlotIndex(), newlySelectedSlot.SlotIndex());
            return;

        }
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
