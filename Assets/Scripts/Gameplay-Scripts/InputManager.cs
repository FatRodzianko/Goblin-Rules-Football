using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // Dictionary to keep track of action map names and if they are disable or not
    // if an action map name has a value greater than 0, it is disabled
    private static readonly IDictionary<string, int> mapStates = new Dictionary<string, int>();
    public InputActionAsset actions;

    private static Controls controls;
    public static Controls Controls
    {
        get
        {
            if (controls != null) { return controls; }
            return controls = new Controls();
        }
    }
    private void Awake()
    {
        if (controls != null)
            return;
        controls = new Controls();
        
    }

    // Start is called before the first frame update
    void Start()
    {
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            //actions.LoadFromJson(rebinds);
            //actions.LoadBindingOverridesFromJson(rebinds);
            controls.asset.LoadBindingOverridesFromJson(rebinds);
            Debug.Log("RebindSaveLoad: LoadBindings: loaded from json string!");
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnEnable()
    {
        Controls.Enable();
    }
    private void OnDisable()
    {
        Controls.Disable();
    }
    private void OnDestroy()
    {
        controls = null;
    }
    // Add a "disable" to an action map by making its int value +1
    public static void Add(string mapName)
    {
        mapStates.TryGetValue(mapName, out int value);
        mapStates[mapName] = value + 1;

        UpdateMapState(mapName);
    }
    // Remove a "disable" to an action map by making its int value -1
    public static void Remove(string mapName)
    {
        mapStates.TryGetValue(mapName, out int value);
        mapStates[mapName] = Mathf.Max(value - 1, 0);

        UpdateMapState(mapName);
    }
    // Update the stat of the action map. If the int value is greater than 0, disable the action map
    // If it is not greater than 0, enable the action map
    private static void UpdateMapState(string mapName)
    {
        int value = mapStates[mapName];
        if (value > 0)
        {
            Controls.asset.FindActionMap(mapName).Disable();
            return;
        }
        Controls.asset.FindActionMap(mapName).Enable();
    }
}
