using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class UnitActionSystemUI : MonoBehaviour
{
    [SerializeField] private Transform _actionButtonPrefab;
    [SerializeField] private Transform _actionButtonContainer;
    [SerializeField] private TextMeshProUGUI _actionPointsText;

    private List<ActionButtonUI> _actionButtonUIList = new List<ActionButtonUI>();

    private void Start()
    {
        UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged;
        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged;
        UnitActionSystem.Instance.OnActionStarted += UnitActionSystem_OnActionStarted;
        UnitActionSystem.Instance.OnBusyChanged += UnitActionSystem_OnBusyChanged;

        BombRunUnit.OnAnyActionPointsChanged += BombRunUnit_OnAnyActionPointsChanged;

        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;

        BaseAction.OnAnyAmmoRemainingChanged += BaseAction_OnAnyAmmoRemainingChanged;
        BaseAction.OnAnyActionCompleted += BaseAction_OnAnyActionCompleted;

        BombRunUnitHealthSystem.OnAnyBodyPartFrozenStateChanged += BombRunUnitHealthSystem_OnAnyBodyPartFrozenStateChanged;

        CreateUnitActionButtons(UnitActionSystem.Instance.GetSelectedUnit());
        UpdateSelectedActionVisual();
        UpdateActionPoints();
    }

    

    private void OnDisable()
    {
        UnitActionSystem.Instance.OnSelectedUnitChanged -= UnitActionSystem_OnSelectedUnitChanged;
        UnitActionSystem.Instance.OnSelectedActionChanged -= UnitActionSystem_OnSelectedActionChanged;
        UnitActionSystem.Instance.OnActionStarted -= UnitActionSystem_OnActionStarted;
        UnitActionSystem.Instance.OnBusyChanged -= UnitActionSystem_OnBusyChanged;

        BombRunUnit.OnAnyActionPointsChanged -= BombRunUnit_OnAnyActionPointsChanged;

        TurnSystem.Instance.OnTurnChanged -= TurnSystem_OnTurnChanged;

        BaseAction.OnAnyAmmoRemainingChanged -= BaseAction_OnAnyAmmoRemainingChanged;
        BaseAction.OnAnyActionCompleted -= BaseAction_OnAnyActionCompleted;

        BombRunUnitHealthSystem.OnAnyBodyPartFrozenStateChanged -= BombRunUnitHealthSystem_OnAnyBodyPartFrozenStateChanged;
    }
    private void CreateUnitActionButtons(BombRunUnit selectedUnit)
    {
        // destroy the old button game objects
        foreach (Transform buttonTransform in _actionButtonContainer)
        {
            Destroy(buttonTransform.gameObject);
        }
        // clear the button list
        _actionButtonUIList.Clear();

        if (selectedUnit == null)
        {
            Debug.Log("CreateUnitActionButtons: selected unit is null?");
            return;
        }

        // instantiate new button game objects
        foreach (BaseAction baseAction in selectedUnit.GetBaseActionArray())
        {
            //if (!selectedUnit.CanSpendActionPointsToTakeAction(baseAction))
            //{
            //    continue;
            //}

            bool canUnitUseAction = selectedUnit.CanSpendActionPointsToTakeAction(baseAction);
            // Check if the player can use the action or not. If "HideWhenCantUse" is true, don't create it.
            if (baseAction.GetHideWhenCantUse())
            {
                if (!canUnitUseAction)
                {
                    continue;
                }
            }

            // if action will not be hidden, create the action buttons
            Transform actionButtonTransform = Instantiate(_actionButtonPrefab, _actionButtonContainer);
            ActionButtonUI actionButtonUI = actionButtonTransform.GetComponent<ActionButtonUI>();
            // disable/grey out button if it can't be used, but also doesn't get hidden
            actionButtonUI.EnableOrDisableButton(canUnitUseAction);
            actionButtonUI.SetBaseAction(baseAction);
            actionButtonUI.UpdateAmmoRemaining();
            actionButtonUI.SetBodyPartType(baseAction.GetActionBodyPart());

            _actionButtonUIList.Add(actionButtonUI);            
        }
    }
    private void UnitActionSystem_OnSelectedUnitChanged(object sender, BombRunUnit unit)
    {
        //CreateUnitActionButtons(unit);
        //UpdateSelectedActionVisual();
        //UpdateActionPoints();
        UpdateActionItems();
    }
    private void UnitActionSystem_OnSelectedActionChanged(object sender, EventArgs e)
    {
        UpdateSelectedActionVisual();
    }
    private void UnitActionSystem_OnActionStarted(object sender, EventArgs e)
    {
        //UpdateActionPoints();
        //CreateUnitActionButtons(UnitActionSystem.Instance.GetSelectedUnit());
        //UpdateSelectedActionVisual();

        //UpdateActionItems();
    }
    private void BaseAction_OnAnyActionCompleted(object sender, EventArgs e)
    {
        Debug.Log("UnitActionSystemUI: BaseAction_OnAnyActionCompleted");
        UpdateActionItems();
    }
    private void UpdateSelectedActionVisual()
    {
        foreach (ActionButtonUI actionButtonUI in _actionButtonUIList)
        {
            actionButtonUI.UpdateSelectedActionVisual();
        }
    }
    private void UpdateActionButtonBodyPartVisuals()
    {
        foreach (ActionButtonUI actionButtonUI in _actionButtonUIList)
        {
            actionButtonUI.UpdateBodyPartSpriteIndicator();
        }
    }
    private void UpdateActionPoints()
    {
        BombRunUnit unit = UnitActionSystem.Instance.GetSelectedUnit();

        _actionPointsText.text = "Action Points: ";
        if (unit == null)
        {
            Debug.Log("UpdateActionPoints: no selected unit?");
            
            return;
        }

        _actionPointsText.text += unit.GetActionPoints().ToString();
    }
    private void BombRunUnit_OnAnyActionPointsChanged(object sender, EventArgs e)
    {
        UpdateActionPoints();

        UpdateActionItems();
    }
    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if (!TurnSystem.Instance.IsPlayerTurn())
            return;

        UpdateActionItems();
    }
    private void UpdateActionItems()
    {
        Debug.Log("UpdateActionItems: ");
        UpdateActionPoints();
        CreateUnitActionButtons(UnitActionSystem.Instance.GetSelectedUnit());
        UpdateSelectedActionVisual();
    }
    private void BaseAction_OnAnyAmmoRemainingChanged(object sender, EventArgs e)
    {
        if (_actionButtonUIList.Count == 0)
            return;

        foreach (ActionButtonUI actionButtonUI in _actionButtonUIList)
        {
            actionButtonUI.UpdateAmmoRemaining();
        }

    }
    private void BombRunUnitHealthSystem_OnAnyBodyPartFrozenStateChanged(object sender, EventArgs e)
    {
        BombRunUnitHealthSystem healthSystem = sender as BombRunUnitHealthSystem;
        if (healthSystem.GetUnit() == UnitActionSystem.Instance.GetSelectedUnit())
        {
            UpdateActionButtonBodyPartVisuals();
        }
    }
    private void UnitActionSystem_OnBusyChanged(object sender, bool actionBusy)
    {
        _actionButtonContainer.gameObject.SetActive(!actionBusy);
    }
}

