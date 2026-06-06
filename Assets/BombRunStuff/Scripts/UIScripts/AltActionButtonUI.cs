using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AltActionButtonUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button _button;
    [SerializeField] private ActionButtonUI _parentButton;
    [SerializeField] private GameObject _selectedHighlightSprite;
    [SerializeField] private GameObject _notSelectableSprite;

    [Header("Alt Action parameters")]
    [SerializeField] private BaseAction _parentAction;
    [SerializeField] private BaseAltAction _altAction;
    [SerializeField] private int _index;


    private void Start()
    {
        _button.onClick.AddListener(() => {
            ButtonClicked();
        });

    }
    private void OnDisable()
    {
        try
        {
            this._parentAction.OnAltActionIndexChanged -= BaseAction_OnAltActionIndexChanged;
        }
        catch (Exception e)
        {
            Debug.Log("AltActionButtonUI: " + this.name + " could not unsubscribe from OnAltActionIndexChanged event");
        }
        UnitActionSystem.Instance.OnSelectedActionChanged -= UnitActionSystem_OnSelectedActionChanged;
    }
    public void InitializeAltActionButton(BaseAction action, int index, ActionButtonUI parent)
    {
        this._parentAction = action;
        this._parentAction.OnAltActionIndexChanged += BaseAction_OnAltActionIndexChanged;

        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged;

        this._index = index;
        this._parentButton = parent;

        if (_parentAction.GetAltActionIndex() == this._index)
        {
            EnableSelectedHighlightSprite();
        }
        else
        {
            DisableSelectedHighlightSprite();
        }

        UpdateAltActionButtonStatus();
    }

    private void UnitActionSystem_OnSelectedActionChanged(object sender, EventArgs e)
    {
        if (this._index == 0)
            return;

        if (UnitActionSystem.Instance.GetSelectedAction() == this._parentAction)
        {
            if (!_parentAction.GetAltActionAtIndex(this._index).CanPlayerSpendActionPointsForAltAction())
            {
                _parentAction.SetAltActionIndex(0);
            }
        }
    }

    private void BaseAction_OnAltActionIndexChanged(object sender, int index)
    {
        if (index == this._index)
        {
            EnableSelectedHighlightSprite();
        }
        else
        {
            DisableSelectedHighlightSprite();
        }
    }

    private void ButtonClicked()
    {
        Debug.Log("AltActionButtonUI: Button Clicked!");

        if (UnitActionSystem.Instance.GetSelectedAction() != this._parentAction)
            return;
        
        _parentButton.AltButtonClicked();
        _parentAction.SetAltActionIndex(this._index);
    }
    private void EnableSelectedHighlightSprite()
    {
        _selectedHighlightSprite.SetActive(true);
    }
    private void DisableSelectedHighlightSprite()
    {
        _selectedHighlightSprite.SetActive(false);
    }
    private void UpdateAltActionButtonStatus()
    {
        if (_parentAction == null)
            return;

        if (this._index == 0)
        {
            EnableAltActionButton();
            return;
        }

        if (_parentAction.GetAltActionAtIndex(this._index).CanPlayerSpendActionPointsForAltAction())
        {
            EnableAltActionButton();
        }
        else
        {
            DisableAltActionButton();
        }
    }
    private void EnableAltActionButton()
    {
        Debug.Log("EnableAltActionButton: ");
        _button.interactable = true;
        _notSelectableSprite.SetActive(false);
    }
    private void DisableAltActionButton()
    {
        Debug.Log("DisableAltActionButton: ");
        _button.interactable = false;
        _notSelectableSprite.SetActive(true);
    }
}
