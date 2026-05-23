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

    [Header("Body Part Stuff")]
    [SerializeField] private ActionButtonBodyPartSpriteHolderScript _actionButtonBodyPartSpriteHolderScript;

    [Header("Ammo Stuff")]
    [SerializeField] private TextMeshProUGUI _remainingAmmoText;

    [Header("Action Noise")]
    [SerializeField] private ScriptableNoiseUIMapping _scriptableNoiseUIMapping;
    [SerializeField] private Image _actionNoiseIconImage;

    [Header("Alt Action Stuff")]
    [SerializeField] private Transform _altActionButtonsHolder;
    [SerializeField] private Transform _altActionButtonPrefab;
    [SerializeField] private List<AltActionButtonUI> _altActionButtons = new List<AltActionButtonUI>();

    private BaseAction _baseAction;

    public void SetBaseAction(BaseAction baseAction)
    {
        this._baseAction = baseAction;
        _actionName.text = baseAction.GetActionName().ToUpper();

        _button.onClick.AddListener(() => {
            UnitActionSystem.Instance.SetSelectedAction(baseAction);
        });

        UpdateAltActions();
        UpdateActionNoiseIcon();
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
    public void SetBodyPartType(BodyPart bodyPart)
    {
        _actionButtonBodyPartSpriteHolderScript.SetBodyPartType(bodyPart);
    }
    public void UpdateBodyPartSpriteIndicator()
    {
        _actionButtonBodyPartSpriteHolderScript.UpdateBodyPartImage();
    }
    private void UpdateActionNoiseIcon()
    {
        if (!_baseAction.MakesNoise())
        {
            _actionNoiseIconImage.sprite = _scriptableNoiseUIMapping.GetSpriteFromNoiseVolume(0);
            return;
        }

        _actionNoiseIconImage.sprite = _scriptableNoiseUIMapping.GetSpriteFromNoiseVolume(_baseAction.NoiseDistance());
    }
    private void UpdateAltActions()
    {
        if (!_baseAction.GetHasAltAction())
        {
            _altActionButtonsHolder.gameObject.SetActive(false);
            return;
        }

        _altActionButtonsHolder.gameObject.SetActive(true);
        DestroyAltActionButtons();
        SpawnAltActionButtons();
    }
    private void DestroyAltActionButtons()
    {
        if (_altActionButtons.Count > 0)
        {
            foreach (AltActionButtonUI button in _altActionButtons)
            {
                Destroy(button);
            }
        }
        _altActionButtons.Clear();
    }
    private void SpawnAltActionButtons()
    {
        int numberOfAltActions = _baseAction.GetNumberOfAltActions();
        for (int i = 0; i < numberOfAltActions + 1; i++)
        {
            Transform altActionButton = Instantiate(_altActionButtonPrefab, _altActionButtonsHolder.transform);
            AltActionButtonUI altActionButtonUI = altActionButton.GetComponent<AltActionButtonUI>();
            altActionButtonUI.InitializeAltActionButton(this._baseAction, i, this);
        }
    }
    public void AltButtonClicked()
    {
        UnitActionSystem.Instance.SetSelectedAction(this._baseAction);
    }
}
