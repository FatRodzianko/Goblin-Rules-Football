using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Loads and saves key bindings.
///
/// It writes the same PlayerPrefs keys the old title screen used, so a player's bindings survive the
/// move from one title screen to the other and the two stay in step while both exist.
///
/// Rebinding edits the project's action asset, while the game plays through the generated wrapper's
/// own copy of it, so a save pushes the overrides into that copy too. Without this a rebind would
/// only take effect the next time the game was started.
/// </summary>
public class BindingStore : MonoBehaviour
{
    public const string FootballKey = "rebinds";
    public const string GolfKey = "golf-rebinds";

    [SerializeField] private InputActionAsset _footballActions;
    [SerializeField] private InputActionAsset _golfActions;

    public InputActionAsset FootballActions { get { return _footballActions; } }

    public InputActionAsset GolfActions { get { return _golfActions; } }

    private void Awake()
    {
        Load();
    }

    public void Load()
    {
        LoadInto(_footballActions, FootballKey);
        LoadInto(_golfActions, GolfKey);
    }

    public void Save()
    {
        SaveFrom(_footballActions, FootballKey);
        SaveFrom(_golfActions, GolfKey);
        PlayerPrefs.Save();
        PushToRunningGame();
    }

    /// <summary>Puts every binding in the asset back to how it ships, and forgets the saved overrides.</summary>
    public void ResetFootball()
    {
        ResetAsset(_footballActions, FootballKey);
        PushToRunningGame();
    }

    public void ResetGolf()
    {
        ResetAsset(_golfActions, GolfKey);
        PushToRunningGame();
    }

    private static void LoadInto(InputActionAsset asset, string key)
    {
        if (asset == null)
            return;
        string json = PlayerPrefs.GetString(key, string.Empty);
        if (string.IsNullOrEmpty(json))
            return;
        asset.LoadBindingOverridesFromJson(json);
    }

    private static void SaveFrom(InputActionAsset asset, string key)
    {
        if (asset == null)
            return;
        string json = asset.SaveBindingOverridesAsJson();
        if (string.IsNullOrEmpty(json))
            PlayerPrefs.DeleteKey(key);
        else
            PlayerPrefs.SetString(key, json);
    }

    private static void ResetAsset(InputActionAsset asset, string key)
    {
        if (asset != null)
            asset.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
    }

    // The gameplay wrappers hold their own copies of these assets, so they have to be told.
    private void PushToRunningGame()
    {
        ApplyTo(InputManager.Controls != null ? InputManager.Controls.asset : null, _footballActions);
        ApplyTo(InputManagerGolf.Controls != null ? InputManagerGolf.Controls.asset : null, _golfActions);
    }

    private static void ApplyTo(InputActionAsset target, InputActionAsset source)
    {
        if (target == null || source == null || target == source)
            return;
        string json = source.SaveBindingOverridesAsJson();
        target.RemoveAllBindingOverrides();
        if (!string.IsNullOrEmpty(json))
            target.LoadBindingOverridesFromJson(json);
    }
}
