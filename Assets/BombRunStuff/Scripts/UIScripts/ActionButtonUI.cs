using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ActionButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _actionName;
    [SerializeField] private Button _button;
    [SerializeField] private Outline _outline;

    [Header("Ammo Stuff")]
    [SerializeField] private TextMeshProUGUI _remainingAmmoText;

    private BaseAction _baseAction;

    public void SetBaseAction(BaseAction baseAction)
    {
        this._baseAction = baseAction;
        _actionName.text = baseAction.GetActionName().ToUpper();

        _button.onClick.AddListener(() => {
            UnitActionSystem.Instance.SetSelectedAction(baseAction);
        });
    }

    public void UpdateSelectedActionVisual()
    {
        BaseAction selectedBaseAction = UnitActionSystem.Instance.GetSelectedAction();

        if (selectedBaseAction == _baseAction)
        {
            _outline.effectColor = Color.yellow;
        }
        else
        {
            _outline.effectColor = Color.black;
        }
    }
    public void EnableOrDisableButton(bool enable)
    {
        _button.interactable = enable;
    }
    public void UpdateAmmoRemaining()
    {
        if (_baseAction == null)
            return;
        if (!_baseAction.GetRequiresAmmo())
        {
            _remainingAmmoText.enabled = false;
            return;
        }
        _remainingAmmoText.text = _baseAction.GetRemainingAmmo().ToString();
    }

}
