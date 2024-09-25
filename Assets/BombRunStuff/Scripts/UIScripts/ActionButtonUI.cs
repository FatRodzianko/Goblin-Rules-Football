using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ActionButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMeshPro;
    [SerializeField] private Button _button;
    [SerializeField] private Outline _outline;

    private BaseAction _baseAction;

    public void SetBaseAction(BaseAction baseAction)
    {
        this._baseAction = baseAction;
        _textMeshPro.text = baseAction.GetActionName().ToUpper();

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
}
