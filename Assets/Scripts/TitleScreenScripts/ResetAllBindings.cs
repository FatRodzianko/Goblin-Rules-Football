using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ResetAllBindings : MonoBehaviour
{
    public static ResetAllBindings instance;
    [SerializeField] InputActionAsset inputActions;
    public InputActionMap playerActionMap;
    public InputActionMap qeSwitchingActionMap;
    public InputActionMap powerUpsActionMap;
    public List<InputActionMap> actionMapsToCheck = new List<InputActionMap>();

    private void Awake()
    {
        MakeInstance();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        foreach (InputActionMap map in inputActions.actionMaps)
        {
            //Debug.Log("ResetAllBindings: map: " + map.name);
            /*foreach (InputControlScheme scheme in map.controlSchemes)
            {
                //Debug.Log("ResetAllBindings: map: " + map + " control scheme: " + scheme.name);
                if (scheme.name == "GamePad")
                    Debug.Log("ResetAllBindings: " + map.name + " for gamepad");
                else if (scheme.name == "Keyboard and Mouse")
                    Debug.Log("ResetAllBindings: " + map.name + " for keyboard and mouse");
            }
            foreach (InputAction action in map.actions)
            {
                //Debug.Log("ResetAllBindings: action name: " + action.name + " action map name: " + action.actionMap.ToString());
                foreach (InputBinding binding in action.bindings)
                {
                    Debug.Log("ResetAllBindings: Action name: " + action.name + " Action map name: " + action.actionMap.ToString() + " Binding name: " + binding.ToString() + " Binding group: " + binding.groups);
                }
            }*/
            if (map.name.Equals("Player"))
            {
                if (!actionMapsToCheck.Contains(map))
                {
                    playerActionMap = map;
                    actionMapsToCheck.Add(map);
                }
            }
            else if (map.name.Equals("QESwitchGoblins"))
            {
                if (!actionMapsToCheck.Contains(map))
                {
                    qeSwitchingActionMap = map;
                    actionMapsToCheck.Add(map);
                }
            }
            else if (map.name.Equals("PowerUps"))
            {
                if (!actionMapsToCheck.Contains(map))
                {
                    powerUpsActionMap = map;
                    actionMapsToCheck.Add(map);
                }
            }   
        }
        ShowEachActionMapBinding();
    }
    void ShowEachActionMapBinding()
    {
        if (actionMapsToCheck.Count > 0)
        {
            foreach (InputActionMap map in actionMapsToCheck)
            {
                Debug.Log("ResetAllBindings: from actionMapsToCheck: map: " + map.name);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
    }
    public void ResetBindings(bool isKeyboardControls)
    {
        var rebinds = PlayerPrefs.GetString("rebinds");
        foreach (InputActionMap map in inputActions.actionMaps)
        {
            map.RemoveAllBindingOverrides();
        }
        PlayerPrefs.DeleteKey("rebinds");
    }
}

