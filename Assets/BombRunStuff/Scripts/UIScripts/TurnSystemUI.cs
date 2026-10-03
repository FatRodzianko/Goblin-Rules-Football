using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TurnSystemUI : MonoBehaviour
{
    

    [SerializeField] private Button _endTurnButton;
    [SerializeField] private TextMeshProUGUI _turnNumberText;
    [SerializeField] private GameObject _enemyTurnVisualGameObject;
    [SerializeField] private GameObject _currentSelectedUnitHolder;

    [SerializeField] private bool _gamePlay = false;

    private void Start()
    {
        _endTurnButton.onClick.AddListener(() =>
        {
            TurnSystem.Instance.NextTurn();
        });

        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;

        UpdateTurnText();
        UpdateEnemyTurnVisual();
        UpdateEndTurnButtonVisibility();

        GameplayManager_BombRun.OnGameStateChanged += GameplayManager_BombRun_OnGameStateChanged;

        BodyModInventoryUIManager.OnInventoryMenuOpened += BodyModInventoryUIManager_OnInventoryMenuOpened;
        BodyModInventoryUIManager.OnInventoryMenuClosed += BodyModInventoryUIManager_OnInventoryMenuClosed;
    }
    private void OnDisable()
    {
        TurnSystem.Instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
        GameplayManager_BombRun.OnGameStateChanged -= GameplayManager_BombRun_OnGameStateChanged;

        BodyModInventoryUIManager.OnInventoryMenuOpened -= BodyModInventoryUIManager_OnInventoryMenuOpened;
        BodyModInventoryUIManager.OnInventoryMenuClosed -= BodyModInventoryUIManager_OnInventoryMenuClosed;
    }

    

    private void GameplayManager_BombRun_OnGameStateChanged(object sender, GameState_BombRun gameState)
    {
        switch (gameState)
        {
            case GameState_BombRun.Gameplay:
                _gamePlay = true;
                ShowTurnUI();
                break;
            default:
                _gamePlay = false;
                HideTurnUI();
                break;
        }
    }
    private void HideTurnUI(bool forInventoryMenu = false)
    {
        _turnNumberText.gameObject.SetActive(false);
        _endTurnButton.gameObject.SetActive(false);
        _enemyTurnVisualGameObject.SetActive(false);

        if (!forInventoryMenu)
        {
            _currentSelectedUnitHolder.SetActive(false);
        }
        
    }
    private void ShowTurnUI()
    {
        if (!_gamePlay)
            return;

        _turnNumberText.gameObject.SetActive(true);
        _endTurnButton.gameObject.SetActive(true);
        _enemyTurnVisualGameObject.SetActive(true);
        _currentSelectedUnitHolder.SetActive(true);

        UpdateTurnText();
        UpdateEnemyTurnVisual();
        UpdateEndTurnButtonVisibility();
    }
    private void UpdateTurnText()
    {
        _turnNumberText.text = "TURN " + TurnSystem.Instance.GetTurnNumber();
    }
    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        UpdateTurnText();
        UpdateEnemyTurnVisual();
        UpdateEndTurnButtonVisibility();
    }
    private void UpdateEnemyTurnVisual()
    {
        _enemyTurnVisualGameObject.SetActive(!TurnSystem.Instance.IsPlayerTurn());
    }
    private void UpdateEndTurnButtonVisibility()
    {
        _endTurnButton.gameObject.SetActive(TurnSystem.Instance.IsPlayerTurn());
    }
    private void BodyModInventoryUIManager_OnInventoryMenuOpened(object sender, EventArgs e)
    {
        HideTurnUI(true);
    }

    private void BodyModInventoryUIManager_OnInventoryMenuClosed(object sender, EventArgs e)
    {
        ShowTurnUI();
    }
}
