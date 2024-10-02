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

    private void Start()
    {
        _endTurnButton.onClick.AddListener(() =>
        {
            TurnSystem.Instance.NextTurn();
        });

        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;

        UpdateTurnText();
    }
    private void OnDisable()
    {
        TurnSystem.Instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
    }
    private void UpdateTurnText()
    {
        _turnNumberText.text = "TURN " + TurnSystem.Instance.GetTurnNumber();
    }
    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        UpdateTurnText();
    }
}
