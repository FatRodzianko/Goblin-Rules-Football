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
    public void InitializeAltActionButton(BaseAction action, int index, ActionButtonUI parent)
    {
        this._parentAction = action;
        this._index = index;
        this._parentButton = parent;
    }
    private void ButtonClicked()
    {
        Debug.Log("AltActionButtonUI: Button Clicked!");
        _selectedHighlightSprite.SetActive(!_selectedHighlightSprite.activeInHierarchy);
        _parentButton.AltButtonClicked();
    }
}
