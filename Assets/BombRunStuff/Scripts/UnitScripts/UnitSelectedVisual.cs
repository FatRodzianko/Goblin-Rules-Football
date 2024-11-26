using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectedVisual : MonoBehaviour
{
    [Header("Unit Info")]
    [SerializeField] private BombRunUnit _unit;

    [Header("Renderers")]
    [SerializeField] private SpriteRenderer _unitSpriteRenderer;
    [SerializeField] private SpriteRenderer _shadowSpriteRenderer;

    [Header("Materials")]
    [SerializeField] private Material _notSelectedMaterial;
    [SerializeField] private Material _selectedMaterial;

    [Header("Sprites")]
    [SerializeField] private Sprite _notSelectedShadowSprite;
    [SerializeField] private Sprite _selectedShadowSprite;

    private void Start()
    {
        UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChange;
        CheckIfSelectedUnit();
    }
    private void OnDisable()
    {
        UnitActionSystem.Instance.OnSelectedUnitChanged -= UnitActionSystem_OnSelectedUnitChange;
    }
    private void UnitActionSystem_OnSelectedUnitChange(object sender, BombRunUnit unit)
    {
        Debug.Log("UnitActionSystem_OnSelectedUnitChange: " + _unit + " : " + unit);
        UpdateSelectedUnitVisuals(unit);
        
    }
    private void CheckIfSelectedUnit()
    {
        UpdateSelectedUnitVisuals(UnitActionSystem.Instance.GetSelectedUnit());
    }
    private void UpdateSelectedUnitVisuals(BombRunUnit selectedUnit)
    {
        if (this._unit == selectedUnit)
        {
            _shadowSpriteRenderer.sprite = _selectedShadowSprite;
            _unitSpriteRenderer.material = _selectedMaterial;
        }
        else
        {
            _shadowSpriteRenderer.sprite = _notSelectedShadowSprite;
            _unitSpriteRenderer.material = _notSelectedMaterial;
        }
    }
}
