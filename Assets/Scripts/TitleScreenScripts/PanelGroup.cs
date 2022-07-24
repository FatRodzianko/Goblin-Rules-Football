using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelGroup : MonoBehaviour
{
    [SerializeField] public GameObject[] panels;
    [SerializeField] public TabGroup tabGroup;
    public int panelIndex;
    // Start is called before the first frame update
    private void Awake()
    {
        HideAllPanels();
    }
    void HideAllPanels()
    {
        Debug.Log("HideAllPanels");
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].gameObject.SetActive(false);
        }
    }
    void ShowCurrentPanel()
    {
        Debug.Log("ShowCurrentPanel: The panelIndex is: " + panelIndex.ToString());
        for (int i = 0; i < panels.Length; i++)
        {
            if (i == panelIndex)
            {
                panels[i].gameObject.SetActive(true);
            }
            else
            {
                panels[i].gameObject.SetActive(false);
            }
        }
    }
    public void SetPageIndex(int index)
    {
        Debug.Log("SetPageIndex called with index: " + index.ToString());
        panelIndex = index;
        ShowCurrentPanel();
    }
}
