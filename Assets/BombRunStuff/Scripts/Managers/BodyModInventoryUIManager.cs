using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyModInventoryUIManager : MonoBehaviour
{
    [SerializeField] GameObject _bodyModInventoryUIHolder;
    [SerializeField] bool _menuOpen;

    [Header("Item Slots")]
    [SerializeField] private Transform _itemSlotsHolder;
    [SerializeField] private Transform _itemSlotPrefab;
    [SerializeField] private List<InventoryItemSlot> _itemSlots = new List<InventoryItemSlot>();
    [SerializeField] private int _numberOfSlots;

    // Start is called before the first frame update
    void Start()
    {
        //CreateItemSlots();
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
    }
    private void OpenInventory()
    {
        BombRunUnit unit = UnitActionSystem.Instance.GetSelectedUnit();
        if (unit == null)
            return;

        _bodyModInventoryUIHolder.SetActive(true);
        _menuOpen = true;

        DestroyItemSlots();
        CreateItemSlots(unit.BodyModManager().MaxInventoryCount());
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
    private void CreateItemSlots(int numberOfSlots)
    {
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
        List<BodyMod_Class> bodyMods = unit.BodyModManager().GetAllBodyMods();

        for (int i = 0; i < _itemSlots.Count; i++)
        {
            //_itemSlots[i].ClearItem();

            if (bodyMods.Count > i)
            {
                _itemSlots[i].AddItemToSlot(bodyMods[i].Sprite(), bodyMods[i].Name(), bodyMods[i].Description());
                _itemSlots[i].SetIsEquipped(bodyMods[i].IsEquipped());
            }
        }
    }
}
