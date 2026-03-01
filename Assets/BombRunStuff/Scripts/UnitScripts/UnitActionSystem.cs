using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.EventSystems;

public class UnitActionSystem : MonoBehaviour
{
    public static UnitActionSystem Instance { get; private set; }
    public static EventHandler OnPlayerClickInvalidPosition;


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
    public event EventHandler<GridPosition> OnSpawnLocationSelected;

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
        // Change what happens on mouse click based on current game state? Or only have this work on the "Gameplay" game state?
        switch (GameplayManager_BombRun.Instance.GameState())
        {
            case GameState_BombRun.None:
                return;
            case GameState_BombRun.InitializeWorld:
                //return;
                break;
        }

        if (_isBusy)
            return;
        if (!TurnSystem.Instance.IsPlayerTurn())
        {
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (TryHandleSelectGridPosition())
        {
            return;
        }
        if (TryHandleRightClickPressed())
        {
            return;
        }

        // moved to the TryHandleSelectGridPosition function for when TryHandleSelectGridPosition_Gameplay return false;
        //HandleSelectedAction();
        //HandleCancelSelectedAction();
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

        switch (GameplayManager_BombRun.Instance.GameState())
        {
            case GameState_BombRun.None:
                return false;
            case GameState_BombRun.InitializeWorld:
                return false;
            case GameState_BombRun.SetSpawnLocation:
                if (TryHandleSelectGridPosition_SetSpawnLocation(mouseGridPosition))
                {
                    return true;
                }
                else
                {
                    OnPlayerClickInvalidPosition?.Invoke(this, EventArgs.Empty);
                    return false;
                } 
            case GameState_BombRun.Gameplay:
                if (TryHandleSelectGridPosition_Gameplay(mouseGridPosition))
                {
                    return true;
                }
                else
                {
                    HandleSelectedAction();
                    //HandleCancelSelectedAction();
                }
                break;

        }

        return false;
        //List<BombRunUnit> units = LevelGrid.Instance.GetUnitListAtGridPosition(mouseGridPosition);

        //if (units.Count > 0)
        //{
        //    if (_selectedUnit == null)
        //    {
        //        if (units.Any(x => !x.IsEnemy()))
        //        {
        //            SetSelectedUnit(units.First(x => !x.IsEnemy()));
        //            return true;
        //        }
        //    }
        //    // don't re-select the unit if it is already selected
        //    if (_selectedUnit == units[0])
        //    {
        //        return false;
        //    }

        //    if (units.Any(x => x.IsEnemy() != _selectedUnit.IsEnemy()))
        //    {
        //        Debug.Log("TryHandleSelectGridPosition: Clicked on Enemy Unit");
        //        return false;
        //    }


        //    // later will need to check if multiple units are on a grid position. If so, expand those units to allow player to select individual units?
        //    Debug.Log("TryHandleSelectGridPosition: Clicked on Friendly Unit");
        //    if (_selectedAction != null)
        //    {
        //        if (_selectedAction.CanTargetFriendlyUnits())
        //        {
        //            if (_selectedAction.IsValidActionGridPosition(mouseGridPosition))
        //            {
        //                return false;
        //            }
                    
        //        }
        //    }
            
        //    SetSelectedUnit(units[0]);
        //    return true;
        //}
        //else
        //{
        //    return false;
        //}
    }
    private bool TryHandleSelectGridPosition_SetSpawnLocation(GridPosition mouseGridPosition)
    {
        if (!LevelGrid.Instance.IsPositionAValidSpawnPosition(mouseGridPosition))
        {
            return false;
        }
        if (LevelGrid.Instance.HasWallOnGridPosition(mouseGridPosition))
        {
            return false;
        }
        if (LevelGrid.Instance.HasAnyObstacleOnGridPosition(mouseGridPosition))
        {
            return false;
        }

        if (LevelGrid.Instance.HasAnyUnitOnGridPosition(mouseGridPosition))
        {
            List<BombRunUnit> units = LevelGrid.Instance.GetUnitListAtGridPosition(mouseGridPosition);
            if (units.Count > 0)
            {
                Debug.Log("TryHandleSelectGridPosition_SetSpawnLocation: Units at position: " + mouseGridPosition + "? Yes");
                if (_selectedUnit == null)
                {
                    if (units.Any(x => !x.IsEnemy()))
                    {
                        SetSelectedUnit(units.First(x => !x.IsEnemy()));
                        return true;
                    }
                }
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
            }
            SetSelectedUnit(units[0]);
            return true;
        }
        else
        {
            // if there is no unit on this grid position, check:
            // if the player already has a selected unit. If not spawn unit at that position
            if (_selectedUnit == null)
            {
                Debug.Log("TryHandleSelectGridPosition_SetSpawnLocation: No unit selected. Spawn unit at: " + mouseGridPosition);
                OnSpawnLocationSelected?.Invoke(this, mouseGridPosition);
                return true;
                
            }
            // if player had a unit selected, move selected unit to this space
            else
            {
                _selectedUnit.MoveUnitPosition(mouseGridPosition);
                SetSelectedUnit(null);
                return true;
            }
        }
        return false;
    }
    private bool TryHandleSelectGridPosition_Gameplay(GridPosition mouseGridPosition)
    {
        List<BombRunUnit> units = LevelGrid.Instance.GetUnitListAtGridPosition(mouseGridPosition);

        if (units.Count > 0)
        {
            if (_selectedUnit == null)
            {
                if (units.Any(x => !x.IsEnemy()))
                {
                    SetSelectedUnit(units.First(x => !x.IsEnemy()));
                    return true;
                }
            }
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
            if (_selectedAction != null)
            {
                if (_selectedAction.CanTargetFriendlyUnits())
                {
                    if (_selectedAction.IsValidActionGridPosition(mouseGridPosition))
                    {
                        return false;
                    }

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
    private bool TryHandleRightClickPressed()
    {
        if (InputManagerBombRun.Instance.IsMouseButtonDownThisFrame())
        {
            return false;
        }

        if (!InputManagerBombRun.Instance.IsRightMouseButtonDownThisFrame())
        {
            return false;
        }

        switch (GameplayManager_BombRun.Instance.GameState())
        {
            case GameState_BombRun.None:
                return false;
            case GameState_BombRun.InitializeWorld:
                return false;
            case GameState_BombRun.SetSpawnLocation:
                if (TryHandleRightClickPressed_SetSpawnLocation())
                {
                    return true;
                }
                break;
            case GameState_BombRun.Gameplay:
                if (TryHandleRightClickPressed_Gameplay())
                {
                    return true;
                }
                break;
        }

        return false;
    }
    private void HandleSelectedAction()
    {
        if (!InputManagerBombRun.Instance.IsMouseButtonDownThisFrame())
            return;

        if (_selectedAction == null)
            return;

        GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPositon(MouseWorld.GetPosition());

        if (_selectedAction == null)
        {
            return;
        }
        if (_selectedUnit.GetActionPoints() < _selectedAction.GetActionPointsCost())
        {
            return;
        }
        if (ActionGridVisualManager.Instance.CalculatingVisualGrid())
        {
            return;
        }
        if (!_selectedAction.IsValidActionGridPosition(mouseGridPosition))
        {
            // first, check if grid position has been seen by player. If yes, standard "invalid action"
            // If not, check if action "CanTakeActionInFogOfWar." If not, standard invalid action
            // If action can CanTakeActionInFogOfWar, then get new valid grid position for the action from the action
            if (!_selectedAction.CanTakeActionInFogOfWar())
            {
                OnPlayerClickInvalidPosition?.Invoke(this, EventArgs.Empty);
                return;
            }
            else
            {
                if (!LevelGrid.Instance.IsValidGridPosition(mouseGridPosition))
                {
                    OnPlayerClickInvalidPosition?.Invoke(this, EventArgs.Empty);
                    return;
                }
                if (LevelGrid.Instance.GetSeenByPlayer(mouseGridPosition))
                {
                    OnPlayerClickInvalidPosition?.Invoke(this, EventArgs.Empty);
                    return;                    
                }
                Debug.Log("HandleSelectedAction: GetNearestValidGridPosition: OLD mouse grid position: " + mouseGridPosition);
                mouseGridPosition = _selectedAction.GetNearestValidGridPosition(mouseGridPosition);
                Debug.Log("HandleSelectedAction: GetNearestValidGridPosition: NEW mouse grid position: " + mouseGridPosition);
            }

        }
        if (!_selectedUnit.TrySpendActionPointsToTakeAction(_selectedAction, mouseGridPosition))
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
    private bool TryHandleRightClickPressed_SetSpawnLocation()
    {
        if (_selectedUnit != null)
        {
            SetSelectedUnit(null);
            return true;
        }
        return false;
    }
    private bool TryHandleRightClickPressed_Gameplay()
    {
        if (HandleCancelSelectedAction())
            return true;
        return false;
    }
    private bool HandleCancelSelectedAction()
    {
        if (_selectedAction == null)
            return false;
        if (InputManagerBombRun.Instance.IsMouseButtonDownThisFrame())
            return false;
        if (!InputManagerBombRun.Instance.IsRightMouseButtonDownThisFrame())
            return false;

        // cancel the selected action by right clicking?
        SetSelectedAction(null);
        return true;

    }
    private void SetSelectedUnit(BombRunUnit unit)
    {
        this._selectedUnit = unit;
        if (_selectedUnit != null && GameplayManager_BombRun.Instance.GameState() == GameState_BombRun.Gameplay)
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
