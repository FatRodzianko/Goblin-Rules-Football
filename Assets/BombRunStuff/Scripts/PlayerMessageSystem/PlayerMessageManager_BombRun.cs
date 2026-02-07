
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerMeassgeEventArgs_BombRun: EventArgs
{
    public string MessageText { get; }
    public float Duration { get; }

    public PlayerMeassgeEventArgs_BombRun(string messageText, float duration)
    {
        MessageText = messageText;
        Duration = duration;
    }
}
public class PlayerMessageManager_BombRun : MonoBehaviour
{
    public static PlayerMessageManager_BombRun Instance { get; private set; }

    // static events
    public static event EventHandler<PlayerMeassgeEventArgs_BombRun> OnShowGamePromptForPlayer;
    public static event EventHandler OnHideGamePromptForPlayer;

    private void Awake()
    {
        MakeInstance();
    }
    void MakeInstance()
    {
        if (Instance != null)
        {
            Debug.Log("MakeInstance: more than one PlayerMessageManager_BombRun. Destroying...");
            Destroy(this);
            return;
        }
        Instance = this;
    }
    public void PromptPlayerToChooseUnitSpawnLocations()
    {
        ShowGamePromptForPlayer("Choose Spawn Location For Your Units", 0f);
    }
    private void ShowGamePromptForPlayer(string message, float duration)
    {
        OnShowGamePromptForPlayer?.Invoke(this, new PlayerMeassgeEventArgs_BombRun(message, duration));
    }
    private void HideGamePromptForPlayer()
    {
        OnHideGamePromptForPlayer?.Invoke(this, EventArgs.Empty);
    }
}
