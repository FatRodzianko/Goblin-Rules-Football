
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
    private void Start()
    {
        BombRunUnitSpawner.OnSpawnLocationsFinalized += BombRunUnitSpawner_OnSpawnLocationsFinalized;
    }
    private void OnDisable()
    {
        BombRunUnitSpawner.OnSpawnLocationsFinalized -= BombRunUnitSpawner_OnSpawnLocationsFinalized;
    }

    private void BombRunUnitSpawner_OnSpawnLocationsFinalized(object sender, EventArgs e)
    {
        HideGamePromptForPlayer();
    }

    public void ShowGamePromptForPlayer(string message, float duration)
    {
        OnShowGamePromptForPlayer?.Invoke(this, new PlayerMeassgeEventArgs_BombRun(message, duration));
    }
    public void HideGamePromptForPlayer()
    {
        OnHideGamePromptForPlayer?.Invoke(this, EventArgs.Empty);
    }
    private void FlashColorOfText( int numberOfFlashes)
    {
        
    }
    private IEnumerator FlashMessageText(int numberOfFlashes)
    {
        int flashes = numberOfFlashes;
        if (flashes < 1)
            flashes = 1;

        Debug.Log("FlashMessageText: Flashes: " + flashes);
        for (int i = 0; i < flashes; i++)
        {

            yield return new WaitForSeconds(0.25f);
        }
        
    }
}
