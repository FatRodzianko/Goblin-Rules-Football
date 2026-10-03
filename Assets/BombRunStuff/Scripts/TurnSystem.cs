using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TurnSystem : MonoBehaviour
{
    public static TurnSystem Instance { get; private set; }
    [SerializeField] private int _turnNumber = 1;
    [SerializeField] private bool _isPlayerTurn = true;

    // events
    public event EventHandler OnTurnChanged;
    

    private void Awake()
    {
        MakeInstance();
    }
    void MakeInstance()
    {
        if (Instance != null)
        {
            Debug.Log("MakeInstance: more than one TurnSystem. Destroying...");
            Destroy(this);
            return;
        }
        Instance = this;
    }
    public void NextTurn()
    {
        if (UnitActionSystem.Instance.GetIsBusy())
            return;

        _turnNumber++;
        _isPlayerTurn = !_isPlayerTurn;
        OnTurnChanged?.Invoke(this, EventArgs.Empty);
    }
    public int GetTurnNumber()
    {
        return _turnNumber;
    }
    public bool IsPlayerTurn()
    {
        return _isPlayerTurn;
    }
}
