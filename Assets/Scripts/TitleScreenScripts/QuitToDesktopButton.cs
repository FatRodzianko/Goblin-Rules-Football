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
        }
        this.GetComponent<Button>().navigation = newNav;
    }
}
