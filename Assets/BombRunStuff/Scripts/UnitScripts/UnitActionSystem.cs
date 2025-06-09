using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.EventSystems;

public class UnitActionSystem : MonoBehaviour
{
    public static UnitActionSystem Instance { get; private set; }


    [SerializeField] private BombRunUnit _selectedUnit;
    [SerializeField] private LayerMask _unitLayerMask;
    [SerializeField] private BaseAction _selectedAction;

    private bool _isBusy;

    [Header("Sub Action Stuff")]
    [SerializeField] private bool _waitingOnSubAction = false;

    // Events
    public event EventHandler<BombRunUnit> OnSelectedUnitChanged;
    public event EventHandler OnSelectedActionChanged;
    public event EventHandler<bool> OnBusyChanged;
    public event EventHandler OnActionStarted;

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

        BombRunUnit.OnAnyUnitDied += BombRunUnit_OnAnyUnitDied;
        BaseSubAction.OnAnySubActionCancelled += BaseSubAction_OnAnySubActionCancelled;
    }

    

    private void OnDisable()
    {
        BombRunUnit.OnAnyUnitDied -= BombRunUnit_OnAnyUnitDied;
        BaseSubAction.OnAnySubActionCancelled -= BaseSubAction_OnAnySubActionCancelled;
    }
    private void Update()
    {
        if (_isBusy)
            return;
        if (!TurnSystem.Instance.IsPlayerTurn())
        {
            return;
        }

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

            if (units.Any(x => x.IsEnemy() != _selectedUnit.IsEnemy()))
            {
                Debug.Log("TryHandleSelectGridPosition: Clicked on Enemy Unit");
                return false;
            }


            // later will need to check if multiple units are on a grid position. If so, expand those units to allow player to select individual units?
            Debug.Log("TryHandleSelectGridPosition: Clicked on Friendly Unit");
            if (_selectedAction.CanTargetFriendlyUnits())
            {
                if (_selectedAction.IsValidActionGridPosition(mouseGridPosition))
                {
                    return false;
                }
            }
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

        if (_selectedAction == null)
        {
            return;
        }
        if (_selectedUnit.GetActionPoints() <= 0)
        {
            return;
        }
        if (!_selectedAction.IsValidActionGridPosition(mouseGridPosition))
        {
            return;
        }
        if (!_selectedUnit.TrySpendActionPointsToTakeAction(_selectedAction))
        {
            return;
        }

        SetBusy();
        if (_selectedAction.GetHasSubAction())
        {
            Debug.Log("HandleSelectedAction: " + _selectedAction.GetActionName() + " has a sub action. Taking sub action...");
            _selectedAction.GetSubAction().TakeSubAction(mouseGridPosition, ClearBusy);
        }
        else
        {
            Debug.Log("HandleSelectedAction: " + _selectedAction.GetActionName() + " does not have a sub action.");
            _selectedAction.TakeAction(mouseGridPosition, ClearBusy);
        }


        

        OnActionStarted?.Invoke(this, EventArgs.Empty);

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
        if (_selectedUnit != null)
        {
            SetSelectedAction(unit.GetAction<MoveAction>());
        }
        OnSelectedUnitChanged?.Invoke(this, _selectedUnit);
        Debug.Log("SetSelectedUnit: " + unit);        
    }
    public void SetSelectedAction(BaseAction baseAction)
    {

        _selectedAction = baseAction;
        OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
    }
    public BombRunUnit GetSelectedUnit()
    {
        return _selectedUnit;
    }
    public BaseAction GetSelectedAction()
    {
        return _selectedAction;
    }
    public bool GetIsBusy()
    {
        return _isBusy;
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
    private void BombRunUnit_OnAnyUnitDied(object sender, EventArgs e)
    {
        BombRunUnit unit = sender as BombRunUnit;

        if (_selectedUnit == unit)
        {
            // change selected unit
            List<BombRunUnit> friendlyUnits = BombRunUnitManager.Instance.GetFriendlyUnitList();

            // make sure the dead unit is no longer in the list
            if (friendlyUnits.Contains(unit))
                friendlyUnits.Remove(unit);

            // check if the play has any units remain
            // if yes, set selected unit to the first unit in the list
            if (friendlyUnits.Count > 0)
            {
                SetSelectedUnit(friendlyUnits[0]);
            }
            else
            {
                // game over thing?
                SetSelectedUnit(null);
                SetSelectedAction(null);
            }

        }
    }
    private void BaseSubAction_OnAnySubActionCancelled(object sender, EventArgs e)
    {
        //SetSelectedAction(null);
    }
}
