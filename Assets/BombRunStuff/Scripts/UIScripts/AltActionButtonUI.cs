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
    }
    public void InitializeAltActionButton(BaseAction action, int index, ActionButtonUI parent)
    {
        this._parentAction = action;
        this._parentAction.OnAltActionIndexChanged += BaseAction_OnAltActionIndexChanged;
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
}
