using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollToSelected : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform inventoryScrollRectTransform;
    public RectTransform oldRect;
    public RectTransform contentPanel;
    public List<GameObject> inventoryChildrenList = new List<GameObject>();
    public GameObject curSelected = null;

    private void Start()
    {
        scrollRect = this.gameObject.GetComponent<ScrollRect>();
        inventoryScrollRectTransform = scrollRect.GetComponent<RectTransform>();
        contentPanel = scrollRect.content;
        foreach (Transform child in contentPanel.transform)
        {
            inventoryChildrenList.Add(child.gameObject);
        }
    }
    private void Update()
    {
        try
        {
            curSelected = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        }
        catch (Exception)
        {
            curSelected = null;
        }
        if (curSelected != null)
        {
            if (inventoryChildrenList.Contains(curSelected))
                SnapTo(curSelected.GetComponent<RectTransform>());
            else
            {
                oldRect = null;
            }
        }
        
        
        /*try
        {
            GameObject curSelected = EventSystem.current.currentSelectedGameObject;
            SnapTo(curSelected.GetComponent<RectTransform>());
        }
        catch (Exception e)
        {
            Debug.Log("ScrollToSelected: Error: " + e);
        }*/
    }
    /*public void SnapTo(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        contentPanel.anchoredPosition =
                (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
                - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
    }*/
    public void SnapTo(RectTransform target)
    {
        int index = inventoryChildrenList.IndexOf(target.gameObject); //Inventory Children List contains all the Children of the ScrollView's Content. We are getting the index of the selected one.
        RectTransform rect = inventoryChildrenList[index].GetComponent<RectTransform>(); //We are getting the RectTransform of the selected Inventory Item.
        Vector2 v = rect.position; //We are getting the Position of the Selected Inventory Item's RectTransform.

        bool inView = RectTransformUtility.RectangleContainsScreenPoint(inventoryScrollRectTransform, v); //We are checking if the Selected Inventory Item is visible from the camera.


        float incrimentSize = rect.rect.height + contentPanel.GetComponent<VerticalLayoutGroup>().spacing; //We are getting the height of our Inventory Items

        if (!inView) //If the selected Item is not visible.
        {
            //Debug.Log("SnapTo: not in view. increment size is: " + incrimentSize.ToString());
            if (oldRect != null) //If we haven't assigned Old before we do nothing.
            {
                if (oldRect.localPosition.y < rect.localPosition.y) //If the last rect we were selecting is lower than our newly selected rect.
                {
                    contentPanel.anchoredPosition += new Vector2(0, -incrimentSize); //We move the content panel down.
                }
                else if (oldRect.localPosition.y > rect.localPosition.y) //if the last rect we were selecting is higher than our newly selected rect.
                {
                    contentPanel.anchoredPosition += new Vector2(0, incrimentSize); //We move the content panel up.
                    
                }
            }
            else
            {
                Debug.Log("SnapTo: oldRect was null?");
                //contentPanel.anchoredPosition = new Vector2 (0,v.y);
                if (rect.localPosition.y < contentPanel.anchorMin.y)
                {
                    //float distance = Vector2.Distance(rect.localPosition, contentPanel.anchorMin);
                    float distance = Vector2.Distance(rect.localPosition, contentPanel.anchoredPosition);
                    Debug.Log("SnapTo: Rect below anchor min: rect " + rect.name + "  position: " + rect.localPosition.ToString() + " transform position: " + rect.transform.position.ToString() + " anchor position: " + contentPanel.anchoredPosition.ToString() + " anchor min: " + contentPanel.anchorMin.y.ToString() + " distance: " + distance);
                    if (rect.localPosition.y == -10)
                    {
                        contentPanel.anchoredPosition = contentPanel.anchorMax;
                    }
                    else
                        contentPanel.anchoredPosition += new Vector2(0, distance);
                    Debug.Log("SnapTo: new achor position: " + contentPanel.anchoredPosition.ToString());
                }
                else if (rect.localPosition.y > contentPanel.anchorMax.y)
                {
                    
                    float distance = Vector2.Distance(rect.localPosition, contentPanel.anchorMax);
                    Debug.Log("SnapTo: Rect ABOVE anchor min: rect " + rect.name + " position: " + rect.localPosition.ToString() + " transform position: " + rect.transform.position.ToString() + " anchor position: " + contentPanel.anchoredPosition.ToString() + " anchor max: " + contentPanel.anchorMax.y.ToString() + " distance: " + distance);
                    contentPanel.anchoredPosition += new Vector2(0, -distance);
                    Debug.Log("SnapTo: new achor position: " + contentPanel.anchoredPosition.ToString());
                }
            }
        }

        oldRect = rect; //We assign our newly selected rect as the OldRect.
    }
}
