using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    public List<TabButton> tabButtons;
    [SerializeField] public Sprite tabIdle;
    [SerializeField] public Sprite tabHover;
    [SerializeField] public Sprite tabActive;
    [SerializeField] Color tabIdleColor;
    [SerializeField] Color tabHoverColor;
    [SerializeField] Color tabActiveColor;
    public TabButton selectedTab;
    public List<GameObject> objectsToSwap;
    public PanelGroup panelGroup;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Subscribe(TabButton button)
    {
        if (tabButtons == null)
        {
            tabButtons = new List<TabButton>();
        }
        tabButtons.Add(button);
    }
    public void OnTabEnter(TabButton button)
    {
        ResetTabs();
        if (selectedTab == null || button != selectedTab)
        {
            //button.background.sprite = tabHover;
            button.background.color = tabHoverColor;
        }

    }
    public void OnTabExit(TabButton button)
    {
        ResetTabs();
    }
    public void OnTabSelected(TabButton button)
    {
        if (selectedTab != null)
        {
            selectedTab.Deselect();
        }
        /*if (button.name == "FindLobby" && button != selectedTab)
        {
            MainMenuManager.instance.GetListOfLobbies();
        }*/

        selectedTab = button;
        selectedTab.Select();

        ResetTabs();
        //button.background.sprite = tabActive;
        button.background.color = tabActiveColor;
        int index = button.transform.GetSiblingIndex();
        for (int i = 0; i < objectsToSwap.Count; i++)
        {
            if (i == index)
            {
                objectsToSwap[i].gameObject.SetActive(true);
            }
            else
            {
                objectsToSwap[i].gameObject.SetActive(false);
            }
        }
        if (panelGroup != null)
        {
            Debug.Log("OnTabSelected: The tab index is: " + button.transform.GetSiblingIndex().ToString());
            panelGroup.SetPageIndex(button.transform.GetSiblingIndex());
        }
    }
    public void ResetTabs()
    {
        foreach (TabButton button in tabButtons)
        {
            if (selectedTab != null && button == selectedTab)
                continue;
            //button.background.sprite = tabIdle;
            button.background.color = tabIdleColor;
        }
    }
}
