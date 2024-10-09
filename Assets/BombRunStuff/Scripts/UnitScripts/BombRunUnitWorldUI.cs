using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BombRunUnitWorldUI : MonoBehaviour
{
    [SerializeField] private BombRunUnit _unit;
    [SerializeField] private SpriteRenderer _actionSymbolSpriteRenderer;

    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
        _unit.OnActionTaken += Unit_OnActionTaken;

        ResetActionSymbolSprite();
    }
    private void OnDisable()
    {
        TurnSystem.Instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
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
}
