using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitToDesktopButton : MonoBehaviour
{
    [SerializeField] Selectable singlePlayerSelectable;
    [SerializeField] Selectable multiplayerSelectable;
    [SerializeField] Selectable settingsSelectable;
    [SerializeField] Selectable hostGameSelectable;
    [SerializeField] Selectable joinGameSelectable;
    [SerializeField] Selectable controlsGameSelectable;
    [SerializeField] Selectable tutorialGameSelectable;
    [SerializeField] Selectable keyboardMouseGameSelectable;
    [SerializeField] Selectable gamepadGameSelectable;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdateUpSelectable(string selectableType)
    {
        StartCoroutine(WaitToChangeSelectable(selectableType));
    }
    IEnumerator WaitToChangeSelectable(string selectableType)
    {
        yield return new WaitForSeconds(0.1f);
        Debug.Log("UpdateUpSelectable: " + selectableType + " on " + this.gameObject.name);
        Navigation newNav = this.GetComponent<Button>().navigation;
        switch (selectableType)
        {
            case "singleplayer":

                newNav.selectOnUp = singlePlayerSelectable;
                break;
            case "multiplayer":
                newNav.selectOnUp = multiplayerSelectable;
                break;
            case "settings":
                newNav.selectOnUp = settingsSelectable;
                break;
            case "hostgame":
                newNav.selectOnUp = hostGameSelectable;
                break;
            case "joingame":
                newNav.selectOnUp = joinGameSelectable;
                break;
            case "controls":
                newNav.selectOnUp = controlsGameSelectable;
                break;
            case "tutorial":
                newNav.selectOnUp = tutorialGameSelectable;
                break;
            case "keyboardmouse":
                newNav.selectOnUp = keyboardMouseGameSelectable;
                break;
            case "gamepad":
                newNav.selectOnUp = gamepadGameSelectable;
                break;
        }
        this.GetComponent<Button>().navigation = newNav;
    }
}
