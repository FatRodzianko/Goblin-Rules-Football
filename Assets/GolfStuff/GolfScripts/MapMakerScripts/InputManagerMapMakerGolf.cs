using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManagerMapMakerGolf : MonoBehaviour
{
    private static readonly IDictionary<string, int> mapStates = new Dictionary<string, int>();
    public InputActionAsset actions;

    private static MapMakerGolfControls controls;
    public static MapMakerGolfControls Controls
    {
        get
        {
            if (controls != null) { return controls; }
            return controls = new MapMakerGolfControls();
        }
    }
    private void Awake()
    {
        if (controls != null)
            return;
        controls = new MapMakerGolfControls();

    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("InputManagerMapMakerGolf: Start");
        var rebinds = PlayerPrefs.GetString("mapmaker-golf-rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            //actions.LoadFromJson(rebinds);
            //actions.LoadBindingOverridesFromJson(rebinds);
            controls.asset.LoadBindingOverridesFromJson(rebinds);
            Debug.Log("InputManagerMapMakerGolf: LoadBindings: loaded from json string!");
        }
        else
        {
            Debug.Log("InputManagerMapMakerGolf: RemoveAllBindingOverrides!");
            controls.asset.RemoveAllBindingOverrides();
        }
    }

    // Update is called once per frame
    void Update()
    {

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
