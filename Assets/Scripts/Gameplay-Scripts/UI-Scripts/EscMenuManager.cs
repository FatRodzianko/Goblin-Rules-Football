using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EscMenuManager : MonoBehaviour
{
    public static EscMenuManager instance;
    public GamePlayer localGamePlayer;

    [Header("Escape Menu UI")]
    [SerializeField] private GameObject EscMenuCanvas;
    [SerializeField] private GameObject EscMenuPanel;
    [SerializeField] private GameObject EscMenuPanelButtonHolder;
    [SerializeField] private EscMenuAnimationScript escMenuAnimationScript;
    [SerializeField] private Button PauseGameButton;

    [Header("Escape Menu Options")]
    public bool isEscMenuOpen = false;
    public bool reopenCoinTossCanvas = false;

    [Header("Other UI Stuff?")]
    [SerializeField] private GameObject CoinTossCanvas;
    [SerializeField] private GameObject PowerUpSelectionObject;

    [Header("Player Controls to Restore")]
    bool coinToss = false;
    bool kickOrReceive = false;
    bool qeSwitching = false;
    bool kicking = false;
    bool kickOffAimArrow = false;
    bool goblinMovement = false;
    bool kickAfterPositioning = false;
    bool kickAfterKicking = false;
    bool powerups = false;

    private void Awake()
    {
        MakeInstance();
    }
    void MakeInstance()
    {
        Debug.Log("EscMenuManager MakeInstance.");
        if (instance == null)
            instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        EscMenuCanvas.SetActive(false);
        EscMenuPanel.SetActive(false);
        EscMenuPanelButtonHolder.SetActive(false);
        InputManager.Controls.EscMenu.EscMenu.performed += _ => UpdateEscapeMenu();
        try
        {
            localGamePlayer = GameObject.FindGameObjectWithTag("LocalGamePlayer").GetComponent<GamePlayer>();
        }
        catch (Exception e)
        {
            Debug.Log("EscMenuManager.cs: could not find local game playeR? Error: " + e);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void UpdateEscapeMenu()
    {
        isEscMenuOpen = !isEscMenuOpen;
        if (isEscMenuOpen)
            OpenEscMenu();
        else
            CloseEscMenu();
    }
    public void OpenEscMenu()
    {
        if (CoinTossCanvas.activeInHierarchy)
        {
            reopenCoinTossCanvas = true;
            CoinTossCanvas.SetActive(false);
        }
        else
        {
            reopenCoinTossCanvas = false;
        }
        GameplayerActiveControls(false);
        EscMenuCanvas.SetActive(true);
        EscMenuPanel.SetActive(true);
        EscMenuPanelButtonHolder.SetActive(true);
        escMenuAnimationScript.unroll = true;
        escMenuAnimationScript.reroll = false;
        if (GameplayManager.instance.isSinglePlayer)
            PauseGameEscapeMenu();
    }
    public void CloseEscMenu()
    {
        if (isEscMenuOpen)
            isEscMenuOpen = false;

        GameplayerActiveControls(true);
        escMenuAnimationScript.ReRollScroll();
        if(reopenCoinTossCanvas && GameplayManager.instance.gamePhase == "cointoss")
            CoinTossCanvas.SetActive(true);
        if (GameplayManager.instance.isSinglePlayer)
            ResumeGameEscapeMenu();
    }
    // Saves what control schemes are active for the gameplayer so they can be restored when the esc menu is later closed?
    public void GameplayerActiveControls(bool enable)
    {
        Debug.Log("EscMenuManager GameplayerActiveControls: " + enable.ToString());
        if (!localGamePlayer)
        {
            localGamePlayer = GameObject.FindGameObjectWithTag("LocalGamePlayer").GetComponent<GamePlayer>();
        }
        // Go through each control set and save them to be restored later?
        if (localGamePlayer.coinTossControlsOnServer)
        {
            //this.coinToss = true;
            localGamePlayer.CoinTossControlls(enable);
        }
        if (localGamePlayer.kickOrReceiveControlsOnServer)
        {
            //this.kickOrReceive = true;
            localGamePlayer.KickOrReceiveControls(enable);
        }
        if (localGamePlayer.qeSwitchingControlsOnServer)
        {
            localGamePlayer.EnableQESwitchingControls(enable);
        }
        if (localGamePlayer.kickingControlsOnServer)
        {
            localGamePlayer.EnableKickingControls(enable);
        }
        if (localGamePlayer.kickOffAimArrowControlsOnServer)
        {
            localGamePlayer.EnableKickoffAimArrowControls(enable);
        }
        if (localGamePlayer.goblinMovementControlsOnServer)
        {
            localGamePlayer.EnableGoblinMovement(enable);
        }
        if (localGamePlayer.gameplayActionControlsOnServer)
        {
            localGamePlayer.EnableGameplayActions(enable);
        }
        if (localGamePlayer.kickAfterPositioningControlsOnServer)
        {
            localGamePlayer.EnableKickAfterPositioning(enable);
        }
        if (localGamePlayer.kickAfterKickingControlsOnServer)
        {
            localGamePlayer.EnableKickAfterKicking(enable);
        }
        if (localGamePlayer.powerupsControlsOnServer)
        {
            if(!enable)
                localGamePlayer.EnablePowerUpControls(enable);
        }

        localGamePlayer.EnableMenuNavigationControls(!enable);
        if (enable)
        {
            if (localGamePlayer.powerupsControlsOnServer)
                localGamePlayer.EnablePowerUpControls(enable);
            var eventSystem = EventSystem.current;
            eventSystem.SetSelectedGameObject(PowerUpSelectionObject, new BaseEventData(eventSystem));
            eventSystem.firstSelectedGameObject = PowerUpSelectionObject;
        }
    }
    public void PauseOrResumeGame()
    {
        //if (GameplayManager.instance.isGamePaused)
        if(localGamePlayer.isGamePaused)
            ResumeGameEscapeMenu();
        else
            PauseGameEscapeMenu();
    }
    void PauseGameEscapeMenu()
    {
        try
        {
            localGamePlayer.PauseGamePlayer();
        }
        catch (Exception e)
        {
            Debug.Log("PauseGameEscapeMenu: Could not access local game player. Error: " + e);
        }
    }
    void ResumeGameEscapeMenu()
    {
        try
        {
            localGamePlayer.ResumeGamePlayer();
        }
        catch (Exception e)
        {
            Debug.Log("ResumeGameEscapeMenu: Could not access local game player. Error: " + e);
        }
        if(!GameplayManager.instance.isSinglePlayer)
            CloseEscMenu();
    }
    public void UpdatePauseGameButtonText(bool wasPaused)
    { 
        if(wasPaused)
            PauseGameButton.GetComponentInChildren<TextMeshProUGUI>().text = "Resume Game";
        else
            PauseGameButton.GetComponentInChildren<TextMeshProUGUI>().text = "Pause Game";
    }
}
