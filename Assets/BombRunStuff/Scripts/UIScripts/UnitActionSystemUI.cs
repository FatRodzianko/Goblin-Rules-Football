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

        CreateUnitActionButtons(UnitActionSystem.Instance.GetSelectedUnit());
        UpdateSelectedActionVisual();
        UpdateActionPoints();
    }
    private void OnDisable()
    {
        UnitActionSystem.Instance.OnSelectedUnitChanged -= UnitActionSystem_OnSelectedUnitChanged;
        UnitActionSystem.Instance.OnSelectedActionChanged -= UnitActionSystem_OnSelectedActionChanged;
        UnitActionSystem.Instance.OnActionStarted -= UnitActionSystem_OnActionStarted;
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

        // instantiate new button game objects
        foreach (BaseAction baseAction in selectedUnit.GetBaseActionArray())
        {
            Transform actionButtonTransform = Instantiate(_actionButtonPrefab, _actionButtonContainer);
            ActionButtonUI actionButtonUI = actionButtonTransform.GetComponent<ActionButtonUI>();
            actionButtonUI.SetBaseAction(baseAction);
            _actionButtonUIList.Add(actionButtonUI);
        }
    }
    private void UnitActionSystem_OnSelectedUnitChanged(object sender, BombRunUnit unit)
    {
        CreateUnitActionButtons(unit);
        UpdateSelectedActionVisual();
        UpdateActionPoints();
    }
    private void UnitActionSystem_OnSelectedActionChanged(object sender, EventArgs e)
    {
        UpdateSelectedActionVisual();
    }
    private void UnitActionSystem_OnActionStarted(object sender, EventArgs e)
    {
        UpdateActionPoints();
    }
    private void UpdateSelectedActionVisual()
    {
        foreach (ActionButtonUI actionButtonUI in _actionButtonUIList)
        {
            actionButtonUI.UpdateSelectedActionVisual();
        }
    }
    private void UpdateActionPoints()
    {
        BombRunUnit unit = UnitActionSystem.Instance.GetSelectedUnit();
        _actionPointsText.text = "Action Points: " + unit.GetActionPoints().ToString();
    }
}

