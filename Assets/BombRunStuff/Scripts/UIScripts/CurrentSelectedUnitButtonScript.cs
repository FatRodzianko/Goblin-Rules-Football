using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CurrentSelectedUnitButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Button / UI Stuff")]
    [SerializeField] private Button _button;
    [SerializeField] private Image _currentSelectedUnitPortraitImage;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private TextMeshProUGUI _unitTypeText;


    [Header("Unit Stuff")]
    [SerializeField] private Sprite _unitPortraitSprite;
    [SerializeField] private BombRunUnit _currentSelectedUnit;

    public static event EventHandler<Vector3> OnPlayerClickedCurrentSelectedUnitButton;

    private void Start()
    {
        _button.onClick.AddListener(() =>
        {
            OnClick();
        });
        UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged;
    }

    

    private void OnClick()
    {
        Debug.Log("CurrentSelectedUnitButtonScript: OnClick");
        if (_currentSelectedUnit == null)
        {
            return;
        }

        OnPlayerClickedCurrentSelectedUnitButton?.Invoke(this, _currentSelectedUnit.transform.position);
    }

    //Detect if the Cursor starts to pass over the GameObject
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        this._backgroundImage.color = Color.white;
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        this._backgroundImage.color = Color.black;
    }
    private void UnitActionSystem_OnSelectedUnitChanged(object sender, BombRunUnit unit)
    {
        this._currentSelectedUnit = unit;
        if (_currentSelectedUnit == null)
        {
            _currentSelectedUnitPortraitImage.gameObject.SetActive(false);
            _backgroundImage.gameObject.SetActive(false);
            _unitTypeText.gameObject.SetActive(false);
            return;
        }

        _currentSelectedUnitPortraitImage.gameObject.SetActive(true);
        _backgroundImage.gameObject.SetActive(true);
        _unitTypeText.gameObject.SetActive(true);

        _currentSelectedUnitPortraitImage.sprite = unit.GetUnitPortrait();
        _unitTypeText.text = unit.GetUnitType().ToString();

    }
}
