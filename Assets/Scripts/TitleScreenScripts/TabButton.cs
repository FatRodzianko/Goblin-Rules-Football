using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

[RequireComponent(typeof(Image))]
public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    [SerializeField] public TabGroup tabGroup;
    public Image background;

    public UnityEvent onTabSelected;
    public UnityEvent onTabDeselected;

    [Header("UI Navigation Stuff")]
    [SerializeField] Selectable selectOnDownComponent;
    [SerializeField] string selectableType;

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelected(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tabGroup.OnTabEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tabGroup.OnTabExit(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        background = GetComponent<Image>();
        tabGroup.Subscribe(this);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Select()
    {
        if (onTabSelected != null)
        {
            onTabSelected.Invoke();
        }
    }
    public void Deselect()
    {
        if (onTabDeselected != null)
        {
            onTabDeselected.Invoke();
        }
    }
    public void ClickOnTab()
    {
        tabGroup.OnTabSelected(this);
        TabOpenNavigation();
    }
    public void TabOpenNavigation()
    {
        try
        {
            Navigation newNav = this.GetComponent<Button>().navigation;
            newNav.selectOnDown = selectOnDownComponent;
            this.GetComponent<Button>().navigation = newNav;
        }
        catch (Exception e)
        {
            Debug.Log("TabOpenNavigation: Failed to set new navigation? Error: " + e);
        }
        UpdateQuitButtonNavigation();
    }
    public void TabClosedNavigation()
    {
        Navigation newNav = this.GetComponent<Button>().navigation;
        newNav.selectOnDown = null;
        this.GetComponent<Button>().navigation = newNav;
    }
    void UpdateQuitButtonNavigation()
    {
        GameObject.FindGameObjectWithTag("QuitButtonTitleScreen").GetComponent<QuitToDesktopButton>().UpdateUpSelectable(selectableType);
    }

}
