using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class TutorialEscMenuManager : MonoBehaviour
{
    public static TutorialEscMenuManager instance;
    public TutorialPlayer localGamePlayer;

    [Header("Escape Menu UI")]
    [SerializeField] private GameObject EscMenuCanvas;
    [SerializeField] private GameObject EscMenuPanel;
    [SerializeField] private GameObject EscMenuPanelButtonHolder;
    [SerializeField] private TutorialEscMenAnimationScript escMenuAnimationScript;
    [SerializeField] private Button PauseGameButton;
    [SerializeField] private GameObject backToGameButton;
    [SerializeField] GameObject FirstButton;

    [Header("Escape Menu Options")]
    public bool isEscMenuOpen = false;
    public bool reopenCoinTossCanvas = false;

    [Header("Other UI Stuff?")]
    [SerializeField] private GameObject PowerUpSelectionObject;
    [SerializeField] private GameObject settingsMenuPanel;


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
        //
       /* Debug.Log("TutorialEscMenuManager: setting the event system stuff? Selecting \"first button\"");
        var eventSystem = EventSystem.current;
        eventSystem.SetSelectedGameObject(FirstButton, new BaseEventData(eventSystem));
        eventSystem.firstSelectedGameObject = FirstButton;

        Debug.Log("TutorialEscMenuManager: setting the event system stuff? Selecting \"PowerUpSelectionObject\"");
        //var eventSystem = EventSystem.current;
        eventSystem.SetSelectedGameObject(PowerUpSelectionObject, new BaseEventData(eventSystem));
        eventSystem.firstSelectedGameObject = PowerUpSelectionObject;*/
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void UpdateEscapeMenu()
    {
        Debug.Log("UpdateEscapeMenu. isEscMenuOpen: " + isEscMenuOpen.ToString());
        isEscMenuOpen = !isEscMenuOpen;
        if (isEscMenuOpen)
            OpenEscMenu();
        else
            CloseEscMenu();
    }
    public void OpenEscMenu()
    {
        Debug.Log("OpenEscMenu");
        GameplayerActiveControls(false);
        EscMenuCanvas.SetActive(true);
        EscMenuPanel.SetActive(true);
        EscMenuPanelButtonHolder.SetActive(true);
        escMenuAnimationScript.unroll = true;
        escMenuAnimationScript.reroll = false;
        PauseGameEscapeMenu();
    }
    public void CloseEscMenu()
    {
        Debug.Log("CloseEscMenu");
        if (isEscMenuOpen)
            isEscMenuOpen = false;


        GameplayerActiveControls(true);
        escMenuAnimationScript.ReRollScroll();
        ResumeGameEscapeMenu();
        if (settingsMenuPanel.activeInHierarchy)
        {
            ResetSettingsMenu();
        }
    }
    // Saves what control schemes are active for the gameplayer so they can be restored when the esc menu is later closed?
    public void GameplayerActiveControls(bool enable)
    {
        Debug.Log("EscMenuManager GameplayerActiveControls: " + enable.ToString());
        if (!localGamePlayer)
        {
            localGamePlayer = GameObject.FindGameObjectWithTag("TutorialPlayer").GetComponent<TutorialPlayer>();
        }
        // Go through each control set and save them to be restored later?
        if (localGamePlayer.qeSwitchingControlsOnServer)
        {
            localGamePlayer.EnableQESwitchingControls(enable);
        }
        if (localGamePlayer.kickingControlsOnServer)
        {
            localGamePlayer.ActivateKickingControls(enable);
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
            if (!enable)
                localGamePlayer.ActivatePowerUpControls(enable);
        }
        if (localGamePlayer.blockingControlsOnServer)
        {
            localGamePlayer.ActivateBlockingControls(enable);
        }
        if (localGamePlayer.attackContolsOnServer)
        {
            localGamePlayer.ActivateAttackControls(enable);
        }

        localGamePlayer.EnableMenuNavigationControls(!enable);
        if (enable)
        {
            if (localGamePlayer.powerupsControlsOnServer)
                localGamePlayer.ActivatePowerUpControls(enable);
            var eventSystem = EventSystem.current;
            eventSystem.SetSelectedGameObject(PowerUpSelectionObject, new BaseEventData(eventSystem));
            eventSystem.firstSelectedGameObject = PowerUpSelectionObject;
        }
    }
    public void PauseOrResumeGame()
    {
        Debug.Log("PauseOrResumeGame");
        //if (GameplayManager.instance.isGamePaused)
        if (localGamePlayer.isGamePaused)
            ResumeGameEscapeMenu();
        else
            PauseGameEscapeMenu();
    }
    void PauseGameEscapeMenu()
    {
        Debug.Log("PauseGameEscapeMenu");
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
        Debug.Log("ResumeGameEscapeMenu");
        try
        {
            localGamePlayer.ResumeGamePlayer();
        }
        catch (Exception e)
        {
            Debug.Log("ResumeGameEscapeMenu: Could not access local game player. Error: " + e);
        }
        //CloseEscMenu();
    }
    public void UpdatePauseGameButtonText(bool wasPaused)
    {
        Debug.Log("UpdatePauseGameButtonText: wasPaused: " + wasPaused.ToString());
        if (wasPaused)
            PauseGameButton.GetComponentInChildren<TextMeshProUGUI>().text = "Resume Game";
        else
            PauseGameButton.GetComponentInChildren<TextMeshProUGUI>().text = "Pause Game";
    }
    public void OpenSettingsMenu()
    {
        Debug.Log("OpenSettingsMenu");
        settingsMenuPanel.GetComponent<ImageAnimation>().UnScrollHalfTime();
    }
    public void BackToEscMenu()
    {
        Debug.Log("BackToEscMenu");
        settingsMenuPanel.GetComponent<ImageAnimation>().ReRollScroll();
        var eventSystem = EventSystem.current;
        eventSystem.SetSelectedGameObject(backToGameButton, new BaseEventData(eventSystem));
        eventSystem.firstSelectedGameObject = backToGameButton;
    }
    void ResetSettingsMenu()
    {
        Debug.Log("ResetSettingsMenu");
        settingsMenuPanel.GetComponent<ImageAnimation>().ResetMenu();
    }
}
