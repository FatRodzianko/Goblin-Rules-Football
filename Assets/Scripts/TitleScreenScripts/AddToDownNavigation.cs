using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddToDownNavigation : MonoBehaviour
{
    [SerializeField] Selectable updateDownNavigation;
    [SerializeField] Selectable downSelectable;
    // Start is called before the first frame update
    void Start()
    {
        if (updateDownNavigation.gameObject.activeInHierarchy)
        {
            Navigation newNav = updateDownNavigation.GetComponent<Button>().navigation;
            newNav.selectOnDown = downSelectable;
            updateDownNavigation.GetComponent<Button>().navigation = newNav;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
