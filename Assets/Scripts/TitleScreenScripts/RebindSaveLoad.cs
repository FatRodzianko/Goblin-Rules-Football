using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class RebindSaveLoad : MonoBehaviour
{
    public InputActionAsset actions;
    public InputActionAsset GolfActions;
    public bool isKeyboardControls;
    public const string rebindKeyboard = "rebindKeyboard";
    public const string rebindGamepad = "rebindGamepad";
    string rebindStringToUse;
    // Start is called before the first frame update
    public void Start()
    {
        LoadBindings();
        LoadGolfBindings();
    }
    public void OnEnable()
    {
        Debug.Log("RebindSaveLoad: OnEnable");
        try
        {
            var rebinds = PlayerPrefs.GetString("rebinds");
            if (!string.IsNullOrEmpty(rebinds))
            {
                //actions.LoadFromJson(rebinds);
                actions.LoadBindingOverridesFromJson(rebinds);
                Debug.Log("RebindSaveLoad: LoadBindings: loaded from json string! Json string: " + rebinds);
            }
        }
        catch (Exception e)
        {
            Debug.Log("RebindSaveLoad: OnEnable: error " + e);
        }
        try
        {
            LoadGolfBindings();
        }
        catch (Exception e)
        {
            Debug.Log("RebindSaveLoad: OnEnable: Golf Actions: error " + e);
        }

    }
    public void OnDisable()
    {
        Debug.Log("RebindSaveLoad: OnDisable");
        //var rebinds = actions.ToJson();
        try
        {
            var rebinds = actions.SaveBindingOverridesAsJson();
            if (!string.IsNullOrEmpty(rebinds))
            {
                PlayerPrefs.SetString("rebinds", rebinds);
                Debug.Log("RebindSaveLoad: SaveBindings: json string: " + rebinds);
            }
        }
        catch (Exception e)
        {
            Debug.Log("RebindSaveLoad: OnDisable: error " + e);
        }
        try
        {
            SaveGolfBindings();
        }
        catch (Exception e)
        {
            Debug.Log("RebindSaveLoad: OnDisable: Golf Actions: error " + e);
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
    public void SaveGolfBindings()
    {
        Debug.Log("RebindSaveLoad: SaveGolfBindings");
        //var rebinds = actions.ToJson();
        var rebinds = GolfActions.SaveBindingOverridesAsJson();

        if (!string.IsNullOrEmpty(rebinds))
        {
            PlayerPrefs.SetString("golf-rebinds", rebinds);
            Debug.Log("RebindSaveLoad: SaveGolfBindings: json string: " + rebinds);
        }
        else
        {
            PlayerPrefs.DeleteKey("golf-rebinds");
            Debug.Log("RebindSaveLoad: SaveGolfBindings as an empty string?: json string: " + rebinds);
        }
    }
    public void LoadGolfBindings()
    {
        Debug.Log("RebindSaveLoad: LoadGolfBindings");
        var rebinds = PlayerPrefs.GetString("golf-rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            //actions.LoadFromJson(rebinds);
            GolfActions.LoadBindingOverridesFromJson(rebinds);
            Debug.Log("RebindSaveLoad: LoadGolfBindings: loaded from json string!");
        }

    }
}
