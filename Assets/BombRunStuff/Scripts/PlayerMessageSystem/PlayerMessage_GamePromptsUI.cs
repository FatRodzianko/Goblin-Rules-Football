using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerMessage_GamePromptsUI : MonoBehaviour
{
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private TextMeshProUGUI _text;
    private void Awake()
    {
        HideGamePromptUI();


        PlayerMessageManager_BombRun.OnShowGamePromptForPlayer += PlayerMessageManager_BombRun_OnShowGamePromptForPlayer;
        PlayerMessageManager_BombRun.OnHideGamePromptForPlayer += PlayerMessageManager_BombRun_OnHideGamePromptForPlayer;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            HideGamePromptUI();
        }
    }

    private void OnDisable()
    {
        PlayerMessageManager_BombRun.OnShowGamePromptForPlayer -= PlayerMessageManager_BombRun_OnShowGamePromptForPlayer;
        PlayerMessageManager_BombRun.OnHideGamePromptForPlayer -= PlayerMessageManager_BombRun_OnHideGamePromptForPlayer;
    }

    private void PlayerMessageManager_BombRun_OnShowGamePromptForPlayer(object sender, PlayerMeassgeEventArgs_BombRun playerMessageArgs)
    {
        _backgroundImage.gameObject.SetActive(true);
        _text.text = playerMessageArgs.MessageText;
        _text.gameObject.SetActive(true);
    }
    private void PlayerMessageManager_BombRun_OnHideGamePromptForPlayer(object sender, EventArgs e)
    {
        HideGamePromptUI();    }
    private void HideGamePromptUI()
    {
        _backgroundImage.gameObject.SetActive(false);
        _text.text = "";
        _text.gameObject.SetActive(false);
    }
}
