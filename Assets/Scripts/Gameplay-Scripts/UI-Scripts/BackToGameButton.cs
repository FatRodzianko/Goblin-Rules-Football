using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackToGameButton : MonoBehaviour
{
    [SerializeField] Button pauseGameButton;
    [SerializeField] Button exitGameButton;
    [SerializeField] Button settingsButton;

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            /*Navigation newNav = new Navigation();
            newNav.mode = Navigation.Mode.Explicit;
            newNav.selectOnUp = exitGameButton;*/
            Navigation newNav = this.GetComponent<Button>().navigation;
            if (GameplayManager.instance.isSinglePlayer)
            {
                Debug.Log("BackToGameButton: changing navigation for singleplayer ui");
                newNav.selectOnDown = settingsButton;
            }
            else
            {
                Debug.Log("BackToGameButton: changing navigation for NOT singleplayer ui");
                newNav.selectOnDown = pauseGameButton;
            }
            this.GetComponent<Button>().navigation = newNav;
        }
        catch (Exception e)
        {
            Debug.Log("BackToGameButton: could not access GameplayManager and/or change the selectOnDown button. Error: " + e);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
