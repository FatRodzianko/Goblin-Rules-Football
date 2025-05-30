using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

[Serializable]
public class BodyPartToSpriteObjectMapping
{
    public BodyPart BodyPart;
    public SpriteRenderer Sprite;
}
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
    [SerializeField] private ScriptableBodyPartSpriteMapping _scriptableBodyPartSpriteMapping;
    [SerializeField] private List<BodyPartToSpriteObjectMapping> _bodyPartToSpriteObjectMapping = new List<BodyPartToSpriteObjectMapping>();
    [SerializeField] private Transform _bodyPartSpriteHolder;

    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
        _unit.OnActionTaken += Unit_OnActionTaken;
        BombRunUnit.OnAnyActionPointsChanged += BombRunUnit_OnAnyActionPointsChanged;

        if (_healthSystem == null)
            _healthSystem = _unit.GetUnitHealthSystem();

        _healthSystem.OnTakeDamage += HealthSystem_OnTakeDamage;
        _healthSystem.OnBodyPartFrozenStateChanged += HealthSystem_OnBodyPartFrozenStateChanged;

        ResetActionSymbolSprite();
        UpdateActionPointsText();
        UpdateHealthBar();
    }

    

    private void OnDisable()
    {
        TurnSystem.Instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
        BombRunUnit.OnAnyActionPointsChanged -= BombRunUnit_OnAnyActionPointsChanged;
        _healthSystem.OnTakeDamage -= HealthSystem_OnTakeDamage;
        _healthSystem.OnBodyPartFrozenStateChanged -= HealthSystem_OnBodyPartFrozenStateChanged;
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
    private void HealthSystem_OnBodyPartFrozenStateChanged(object sender, BodyPart bodyPart)
    {
        UpdateBodyPartSprites(bodyPart);
    }
    public void UpdateBodyPartSprites(BodyPart bodyPartToUpdate)
    {
        foreach (BodyPartToSpriteObjectMapping bodyPartToSpriteObjectMapping in _bodyPartToSpriteObjectMapping)
        {
            ScriptableBodyPartSprites scriptableBodyPartSprites = _scriptableBodyPartSpriteMapping.GetBodyPartSpriteMappingForBodyPart(bodyPartToSpriteObjectMapping.BodyPart).Sprites;
            if (bodyPartToSpriteObjectMapping.BodyPart == bodyPartToUpdate)
            {
                BodyPartFrozenState state = _healthSystem.GetBodyPartFrozenState(bodyPartToUpdate);
                bodyPartToSpriteObjectMapping.Sprite.sprite = scriptableBodyPartSprites.GetSpriteForState(state);
            }
        }
    }

}
