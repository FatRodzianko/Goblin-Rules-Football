using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnitActionSystem : MonoBehaviour
{
    public static UnitActionSystem Instance { get; private set; }


    [SerializeField] private BombRunUnit _selectedUnit;
    [SerializeField] private LayerMask _unitLayerMask;

    // Events
    public event EventHandler<BombRunUnit> OnSelectedUnitChanged;

    private void Awake()
    {
        MakeInstance();
    }
   
    void MakeInstance()
    {
        if (Instance != null)
        {
            Debug.Log("MakeInstance: more than one UnitActionSystem. Destroying...");
            Destroy(this);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        SetSelectedUnit(_selectedUnit);
    }
    private void Update()
    {
        
        if (InputManagerBombRun.Instance.IsMouseButtonDownThisFrame())
        {
            if (TryHandleUnitSelection())
                return;

            GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPositon(MouseWorld.GetPosition());
            if (_selectedUnit.GetMoveAction().IsValidActionGridPosition(mouseGridPosition))
            {
                _selectedUnit.GetMoveAction().Move(mouseGridPosition);
            }
            //_selectedUnit.GetMoveAction().Move(MouseWorld.GetPosition());
        }
    }
    private bool TryHandleUnitSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(InputManagerBombRun.Instance.GetMouseScreenPosition());
        RaycastHit2D raycastHit = Physics2D.Raycast(ray.origin, ray.direction, float.MaxValue, _unitLayerMask);
        if (raycastHit)
        {
            if (raycastHit.transform.TryGetComponent<BombRunUnit>(out BombRunUnit unit))
            {
                //_selectedUnit = unit;
                SetSelectedUnit(unit);
                return true;
            }
        }
        return false;
    }
    private void SetSelectedUnit(BombRunUnit unit)
    {
        this._selectedUnit = unit;

        OnSelectedUnitChanged?.Invoke(this, _selectedUnit);
        Debug.Log("SetSelectedUnit: " + unit);
    }
    public BombRunUnit GetSelectedUnit()
    {
        return _selectedUnit;
    }
}
