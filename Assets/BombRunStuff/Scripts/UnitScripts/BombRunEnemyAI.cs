using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BombRunEnemyAI : MonoBehaviour
{
    private float timer;
    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
    }
    private void Update()
    {
        if (TurnSystem.Instance.IsPlayerTurn())
            return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            TurnSystem.Instance.NextTurn();
        }
    }
    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        timer = 3f;
    }
}
