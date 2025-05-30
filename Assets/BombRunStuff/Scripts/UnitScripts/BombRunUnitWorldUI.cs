using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class BombRunUnitWorldUI : MonoBehaviour
{
    [SerializeField] private BombRunUnit _unit;
    [SerializeField] private SpriteRenderer _actionSymbolSpriteRenderer;

    [Header("ACtion Points")]
    [SerializeField] private TextMeshPro _actionPointsText;

    [Header("Health Bar (old)")]
    [SerializeField] private Image _healthBarImage;
    [SerializeField] private BombRunUnitHealthSystem _healthSystem;

    [Header("Body Part Sprites")]
    [SerializeField] private Transform _bodyPartSpriteHolder;
    [SerializeField] private SpriteRenderer _legsSprite;
    [SerializeField] private SpriteRenderer _armsSprite;
    [SerializeField] private SpriteRenderer _headSprite;

    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
        _unit.OnActionTaken += Unit_OnActionTaken;
        BombRunUnit.OnAnyActionPointsChanged += BombRunUnit_OnAnyActionPointsChanged;

        if (_healthSystem == null)
            _healthSystem = _unit.GetUnitHealthSystem();

        _healthSystem.OnTakeDamage += HealthSystem_OnTakeDamage;

        ResetActionSymbolSprite();
        UpdateActionPointsText();
        UpdateHealthBar();
    }

    

    private void OnDisable()
    {
        TurnSystem.Instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
        BombRunUnit.OnAnyActionPointsChanged -= BombRunUnit_OnAnyActionPointsChanged;
        _healthSystem.OnTakeDamage -= HealthSystem_OnTakeDamage;
    }
    private void ResetActionSymbolSprite()
    {
        _actionSymbolSpriteRenderer.sprite = null;
    }
    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if ((_unit.IsEnemy() && !TurnSystem.Instance.IsPlayerTurn()) || // if the unit IS an enemy and it IS NOT the player's turn aka, the unit is an enemy and it is the enemy turn
            (!_unit.IsEnemy() && TurnSystem.Instance.IsPlayerTurn())) // if the unit IS NOT an enemy and it IS the player's turn, aka the unit is the player's and it is the player's turn
        {
            ResetActionSymbolSprite();
        }
    }
    private void Unit_OnActionTaken(object sender, BaseAction action)
    {
        Sprite actionSymbolSprite = action.GetActionSymbolSprite();
        if (actionSymbolSprite == null)
            return;
        _actionSymbolSpriteRenderer.sprite = actionSymbolSprite;
    }
    private void BombRunUnit_OnAnyActionPointsChanged(object sender, EventArgs e)
    {
        BombRunUnit unit = sender as BombRunUnit;
        if (unit != _unit)
            return;

        UpdateActionPointsText();
    }
    private void UpdateActionPointsText()
    {
        _actionPointsText.text = _unit.GetActionPoints().ToString();
    }
    private void HealthSystem_OnTakeDamage(object sender, EventArgs e)
    {
        UpdateHealthBar();
    }
    private void UpdateHealthBar()
    {
        _healthBarImage.fillAmount = _healthSystem.GetHealthPercentRemaining();
    }

}
