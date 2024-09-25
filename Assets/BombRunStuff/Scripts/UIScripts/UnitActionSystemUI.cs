using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UnitActionSystemUI : MonoBehaviour
{
    [SerializeField] private Transform _actionButtonPrefab;
    [SerializeField] private Transform _actionButtonContainer;

    private List<ActionButtonUI> _actionButtonUIList = new List<ActionButtonUI>();

    private void Start()
    {
        UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged;
        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged;

        CreateUnitActionButtons(UnitActionSystem.Instance.GetSelectedUnit());
        UpdateSelectedActionVisual();
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
    }
    private void UnitActionSystem_OnSelectedActionChanged(object sender, EventArgs e)
    {
        UpdateSelectedActionVisual();
    }
    private void UpdateSelectedActionVisual()
    {
        foreach (ActionButtonUI actionButtonUI in _actionButtonUIList)
        {
            actionButtonUI.UpdateSelectedActionVisual();
        }
    }

}

