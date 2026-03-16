using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class UnitSpawningButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Button / UI Stuff")]
    [SerializeField] private TextMeshProUGUI _unitTypeText;
    [SerializeField] private Button _button;    
    [SerializeField] private Shadow _shadow;
    [SerializeField] private Image _unitPortraitImage;
    [SerializeField] private Image _backgroundImage;


    [Header("Unit Stuff")]
    [SerializeField] private Sprite _unitPortraitSprite;
    [SerializeField] private int _index;

    [Header("Misc.")]
    [SerializeField] private bool _isSelected = false;

    //[Header("Unit Spawner")]
    //[SerializeField] private BombRunUnitSpawner _bombRunUnitSpawner;

    public static event EventHandler<int> OnPlayerClickedUnitSpawnButton;
    private static event EventHandler OnPlayerClickedButton;

    private void Start()
    {
        _button.onClick.AddListener(() =>
        {
            OnClick();
        });

        OnPlayerClickedButton += OnPlayerClickedButtonFunction;
        UnitActionSystem.Instance.OnPlayerRightClicked += UnitActionSystem_OnPlayerRightClicked;
        UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged;
    }
    private void OnDisable()
    {
        OnPlayerClickedButton -= OnPlayerClickedButtonFunction;
        UnitActionSystem.Instance.OnPlayerRightClicked -= UnitActionSystem_OnPlayerRightClicked;
        UnitActionSystem.Instance.OnSelectedUnitChanged -= UnitActionSystem_OnSelectedUnitChanged;
    }

    

    public void InitializeUIObject(Sprite portraitSprite, int index, string unitType)
    {
        _unitPortraitSprite = portraitSprite;
        _unitPortraitImage.sprite = _unitPortraitSprite;

        _index = index;

        _unitTypeText.text = unitType;
    }
    private void OnClick()
    {
        OnPlayerClickedUnitSpawnButton?.Invoke(this, _index);
        OnPlayerClickedButton?.Invoke(this, EventArgs.Empty);
    }
    private void OnPlayerClickedButtonFunction(object sender, EventArgs e)
    {
        if (sender as UnitSpawningButtonUI == this)
        {
            PlayerClickedOnThisButton();
        }
        else
        {
            PlayerClickedOnOtherButton();
        }
    }
    private void PlayerClickedOnThisButton()
    {
        this._isSelected = true;
        //this._outline.effectColor = Color.yellow;
        this._backgroundImage.color = Color.yellow;
    }
    private void PlayerClickedOnOtherButton()
    {
        this._isSelected = false;
        //this._outline.effectColor = Color.black;
        this._backgroundImage.color = Color.black;
    }
    public int GetIndex()
    {
        return _index;
    }
    private void UnitActionSystem_OnPlayerRightClicked(object sender, EventArgs e)
    {
        PlayerClickedOnOtherButton();
    }
    private void UnitActionSystem_OnSelectedUnitChanged(object sender, BombRunUnit unit)
    {
        PlayerClickedOnOtherButton();
    }
    //Detect if the Cursor starts to pass over the GameObject
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (!_isSelected)
        {
            this._backgroundImage.color = Color.white;
        }
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (!_isSelected)
        {
            this._backgroundImage.color = Color.black;
        }
    }
}
