using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class UnitActionSystem : MonoBehaviour
{
    public static UnitActionSystem Instance { get; private set; }


    [SerializeField] private BombRunUnit _selectedUnit;
    [SerializeField] private LayerMask _unitLayerMask;
    [SerializeField] private BaseAction _selectedAction;

    private bool _isBusy;

    // Events
    public event EventHandler<BombRunUnit> OnSelectedUnitChanged;
    public event EventHandler OnSelectedActionChanged;
    public event EventHandler<bool> OnBusyChanged;

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
        if (_isBusy)
            return;

        //if (InputManagerBombRun.Instance.IsMouseButtonDownThisFrame())
        //{
        //    //if (TryHandleUnitSelection())
        //    //    return;

        //    if (TryHandleSelectGridPosition())
        //        return;


        //    GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPositon(MouseWorld.GetPosition());
        //    if (_selectedUnit.GetMoveAction().IsValidActionGridPosition(mouseGridPosition))
        //    {
        //        _selectedUnit.GetMoveAction().Move(mouseGridPosition, ClearBusy);
        //        SetBusy();
        //    }
        //    //_selectedUnit.GetMoveAction().Move(MouseWorld.GetPosition());
        //}

        //if (InputManagerBombRun.Instance.IsRightMouseButtonDownThisFrame())
        //{
        //    _selectedUnit.GetSpinAction().Spin(ClearBusy);
        //    SetBusy();
        //}

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (TryHandleSelectGridPosition())
            return;
        HandleSelectedAction();
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
    private bool TryHandleSelectGridPosition()
    {
        if (!InputManagerBombRun.Instance.IsMouseButtonDownThisFrame())
            return false;

        GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPositon(MouseWorld.GetPosition());
        //Debug.Log("TryHandleSelectGridPosition: " + mouseGridPosition);
        if (!LevelGrid.Instance.IsValidGridPosition(mouseGridPosition))
            return false;

        List<BombRunUnit> units = LevelGrid.Instance.GetUnitListAtGridPosition(mouseGridPosition);

        if (units.Count > 0)
        {
            // don't re-select the unit if it is already selected
            if (_selectedUnit == units[0])
            {
                return false;
            }

            // later will need to check if multiple units are on a grid position. If so, expand those units to allow player to select individual units?
            SetSelectedUnit(units[0]);
            return true;
        }
        else
        {
            return false;
        }
    }
    private void HandleSelectedAction()
    {
        if (!InputManagerBombRun.Instance.IsMouseButtonDownThisFrame())
            return;

        GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPositon(MouseWorld.GetPosition());
        if (_selectedAction.IsValidActionGridPosition(mouseGridPosition))
        {
            SetBusy();
            _selectedAction.TakeAction(mouseGridPosition, ClearBusy);
        }
        

        // alternative for take actions to do a switch and call each actions individual method for its action
        //switch (_selectedAction)
        //{
        //    case MoveAction moveAction:
        //        if (moveAction.IsValidActionGridPosition(mouseGridPosition))
        //        {
        //            moveAction.Move(mouseGridPosition, ClearBusy);
        //            SetBusy();
        //        }
        //        break;
        //    case SpinAction spinAction:
        //        spinAction.Spin(ClearBusy);
        //        SetBusy();
        //        break;
        //}
        
    }
    private void SetSelectedUnit(BombRunUnit unit)
    {
        this._selectedUnit = unit;
        SetSelectedAction(unit.GetMoveAction());

        OnSelectedUnitChanged?.Invoke(this, _selectedUnit);
        Debug.Log("SetSelectedUnit: " + unit);
    }
    public void SetSelectedAction(BaseAction baseAction)
    {
        _selectedAction = baseAction;
        OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
        Debug.Log("SetSelectedAction: " + baseAction.GetActionName());
    }
    public BombRunUnit GetSelectedUnit()
    {
        return _selectedUnit;
    }
    public BaseAction GetSelectedAction()
    {
        return _selectedAction;
    }
    private void SetBusy()
    {
        _isBusy = true;
        OnBusyChanged?.Invoke(this, _isBusy);
    }
    private void ClearBusy()
    {
        _isBusy = false;
        OnBusyChanged?.Invoke(this, _isBusy);
    }
}
