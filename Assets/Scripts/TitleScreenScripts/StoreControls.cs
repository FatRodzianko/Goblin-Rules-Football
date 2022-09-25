using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class StoreControls : MonoBehaviour
{
    // Start is called before the first frame update
    public InputActionAsset PlayerControls;
    public InputActionMap playerActionMap;
    public InputActionMap qeSwitchGoblinsActionMap;
    public InputActionMap powerUpsActionMap;
    public InputAction actionToLookAt;

    void Start()
    {
        //playerActionMap = PlayerControls.actionMaps.
        /*foreach (InputActionMap map in PlayerControls.actionMaps)
        {
            Debug.Log("StoreControls: action map in PlayerControls.actionMaps is: " + map.ToString() + " ");
            map.controlSchemes
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

