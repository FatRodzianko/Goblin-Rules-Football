using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class RebindSaveLoad : MonoBehaviour
{
    public InputActionAsset actions;
    public bool isKeyboardControls;
    public const string rebindKeyboard = "rebindKeyboard";
    public const string rebindGamepad = "rebindGamepad";
    string rebindStringToUse;
    // Start is called before the first frame update
    public void Start()
    {
        LoadBindings();
    }
    public void OnEnable()
    {
        Debug.Log("RebindSaveLoad: OnEnable");
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            //actions.LoadFromJson(rebinds);
            actions.LoadBindingOverridesFromJson(rebinds);
            Debug.Log("RebindSaveLoad: LoadBindings: loaded from json string! Json string: " + rebinds);
        }
    }
    public void OnDisable()
    {
        Debug.Log("RebindSaveLoad: OnDisable");
        //var rebinds = actions.ToJson();
        var rebinds = actions.SaveBindingOverridesAsJson();
        if (!string.IsNullOrEmpty(rebinds))
        {
            PlayerPrefs.SetString("rebinds", rebinds);
            Debug.Log("RebindSaveLoad: SaveBindings: json string: " + rebinds);
        }
    }
    public void SaveBindings()
    {
        Debug.Log("RebindSaveLoad: SaveBindings");
        //var rebinds = actions.ToJson();
        var rebinds = actions.SaveBindingOverridesAsJson();
        
        if (!string.IsNullOrEmpty(rebinds))
        {
            PlayerPrefs.SetString("rebinds", rebinds);
            Debug.Log("RebindSaveLoad: SaveBindings: json string: " + rebinds);
        }
    }
    public void LoadBindings()
    {
        Debug.Log("RebindSaveLoad: LoadBindings");
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            //actions.LoadFromJson(rebinds);
            actions.LoadBindingOverridesFromJson(rebinds);
            Debug.Log("RebindSaveLoad: LoadBindings: loaded from json string!");
        }
            
    }
}
