using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum GameState_BombRun
{
    None,
    InitializeWorld,
    SetSpawnLocation,
    Gameplay,
    GameOver
}
public class GameplayManager_BombRun : MonoBehaviour
{
    public static GameplayManager_BombRun Instance { get; private set; }
    [SerializeField] private GameState_BombRun _gameState;

    // static events
    public static event EventHandler<GameState_BombRun> OnGameStateChanged;

    private void Awake()
    {
        MakeInstance();

        InitializeLevelManager_BombRun.OnInitializationBegin += InitializeLevelManager_BombRun_OnInitializationBegin;
    }   

    void MakeInstance()
    {
        if (Instance != null)
        {
            Debug.Log("MakeInstance: more than one GameplayManager_BombRun. Destroying...");
            Destroy(this);
            return;
        }
        Instance = this;
    }
    public GameState_BombRun GameState()
    {
        return _gameState;
    }
    private void InitializeLevelManager_BombRun_OnInitializationBegin(object sender, EventArgs e)
    {
        _gameState = GameState_BombRun.InitializeWorld;
        OnGameStateChanged?.Invoke(this, _gameState);
    }
}
