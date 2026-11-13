using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The game's one display setting: the resolution and whether the window is fullscreen.
///
/// Every menu that offers display options reads and writes it here, and nothing else calls
/// Screen.SetResolution. The stored mode is applied once, as the game starts. The window then keeps
/// it through every scene load until the player changes it, so no scene ever applies a mode of its own.
///
/// The rules exist because applying an unchecked mode could write 0x0 into the registry and leave
/// players unable to start the game:
///  - with nothing stored, nothing is applied, so a first launch keeps the player's own default;
///  - a mode is only applied if the display actually reports it and it clears a hard floor, so a
///    bad value can never reach Screen.SetResolution;
///  - a stored mode the current display cannot do is replaced, and the replacement is written back,
///    so the same bad value cannot trap the player twice.
///
/// Only borderless fullscreen and windowed are offered. Exclusive fullscreen is the mode that can
/// leave a display showing nothing at all, and it buys this game nothing.
/// </summary>
public static class DisplayPreferences
{
    public const string WidthKey = "Display.Width";
    public const string HeightKey = "Display.Height";
    public const string RefreshKey = "Display.RefreshRate";
    public const string FullscreenKey = "Display.Fullscreen";
    public const string VersionKey = "Display.Version";

    // Unity's own window state lives in the same place as PlayerPrefs. Clearing these is what a
    // player used to have to do by hand in the registry.
    private static readonly string[] UnityScreenKeys =
    {
        "Screenmanager Resolution Width",
        "Screenmanager Resolution Height",
        "Screenmanager Fullscreen mode",
        "Screenmanager Is Fullscreen deprecated",
        "Screenmanager Resolution Use Native",
        "Screenmanager Window Position X",
        "Screenmanager Window Position Y",
        "UnitySelectMonitor"
    };

    private const int SettingsVersion = 1;
    private const int MinWidth = 640;
    private const int MinHeight = 360;

    // Screen.SetResolution only takes effect a frame or more later, so a menu that redraws straight after
    // a change would read the window it is leaving, and its next save would send that back. Until the
    // window reports the requested mode, the request is what counts as on screen. The time limit lets the
    // window win again if the request never lands, or if the player changes it some other way (Alt+Enter).
    private const float RequestSettleSeconds = 2f;

    private static readonly List<Resolution> _modes = new List<Resolution>();
    private static bool _hasRequest;
    private static Resolution _requested;
    private static bool _requestedFullscreen;
    private static float _requestedAt;

    /// <summary>Every mode this display reports that is big enough to play on, smallest first.</summary>
    public static IList<Resolution> Modes
    {
        get
        {
            if (_modes.Count == 0)
                RefreshModes();
            return _modes;
        }
    }

    /// <summary>Whether the window is fullscreen, or is about to be after the last change.</summary>
    public static bool IsFullscreen
    {
        get
        {
            Resolution requested;
            bool requestedFullscreen;
            if (TryGetUnsettledRequest(out requested, out requestedFullscreen))
                return requestedFullscreen;
            return ScreenIsFullscreen();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ApplyStoredAtStartup()
    {
        RefreshModes();

        // A launch with nothing stored leaves the player's own default alone. Writing a mode nobody
        // chose is what produced the 0x0 window.
        if (!PlayerPrefs.HasKey(WidthKey) || !PlayerPrefs.HasKey(HeightKey))
            return;

        Resolution stored = new Resolution();
        stored.width = PlayerPrefs.GetInt(WidthKey, 0);
        stored.height = PlayerPrefs.GetInt(HeightKey, 0);
        stored.refreshRateRatio = new RefreshRate { numerator = (uint)Mathf.Max(0, PlayerPrefs.GetInt(RefreshKey, 0)), denominator = 1 };
        bool fullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) != 0;

        if (!IsSupported(stored.width, stored.height))
        {
            Resolution fallback;
            if (!TryGetFallback(out fallback))
            {
                Debug.LogWarning("DisplayPreferences: stored mode is unusable and no replacement was found; leaving the window as it is.");
                return;
            }
            Debug.Log("DisplayPreferences: stored mode " + stored.width + "x" + stored.height
                + " is not available here; falling back to " + fallback.width + "x" + fallback.height + ".");
            stored = fallback;
            fullscreen = true;
            // Write the replacement back so the bad value cannot strand the player again.
            Store(stored, fullscreen);
        }

        Apply(stored, fullscreen);
    }

    /// <summary>Re-reads the modes the display reports, for when a menu is about to list them.</summary>
    public static void RefreshModes()
    {
        _modes.Clear();
        Resolution[] reported = Screen.resolutions;
        for (int i = 0; i < reported.Length; i++)
        {
            if (reported[i].width < MinWidth || reported[i].height < MinHeight)
                continue;
            bool already = false;
            for (int j = 0; j < _modes.Count; j++)
            {
                if (_modes[j].width == reported[i].width && _modes[j].height == reported[i].height)
                {
                    already = true;
                    break;
                }
            }
            if (!already)
                _modes.Add(reported[i]);
        }
    }

    /// <summary>The index into <see cref="Modes"/> that matches the window right now, or 0.</summary>
    public static int CurrentModeIndex()
    {
        Resolution current = CurrentResolution();
        IList<Resolution> modes = Modes;
        for (int i = 0; i < modes.Count; i++)
        {
            if (modes[i].width == current.width && modes[i].height == current.height)
                return i;
        }
        return 0;
    }

    /// <summary>The mode at <paramref name="modeIndex"/>, if there is one.</summary>
    public static bool TryGetMode(int modeIndex, out Resolution mode)
    {
        IList<Resolution> modes = Modes;
        if (modeIndex < 0 || modeIndex >= modes.Count)
        {
            mode = default(Resolution);
            return false;
        }
        mode = modes[modeIndex];
        return true;
    }

    /// <summary>Puts a mode on screen without storing it. Refuses anything the display does not report.</summary>
    public static bool Apply(Resolution mode, bool fullscreen)
    {
        if (!IsSupported(mode.width, mode.height))
        {
            Debug.LogWarning("DisplayPreferences: refused to apply " + mode.width + "x" + mode.height + ".");
            return false;
        }
        FullScreenMode screenMode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        if (mode.refreshRateRatio.value > 0)
            Screen.SetResolution(mode.width, mode.height, screenMode, mode.refreshRateRatio);
        else
            Screen.SetResolution(mode.width, mode.height, screenMode);

        _hasRequest = true;
        _requested = mode;
        _requestedFullscreen = fullscreen;
        _requestedAt = Time.realtimeSinceStartup;
        return true;
    }

    /// <summary>Puts a mode on screen and stores it, for a menu that has no confirmation step.</summary>
    public static bool ApplyAndStore(Resolution mode, bool fullscreen)
    {
        if (!Apply(mode, fullscreen))
            return false;
        Store(mode, fullscreen);
        return true;
    }

    public static void Store(Resolution mode, bool fullscreen)
    {
        PlayerPrefs.SetInt(VersionKey, SettingsVersion);
        PlayerPrefs.SetInt(WidthKey, mode.width);
        PlayerPrefs.SetInt(HeightKey, mode.height);
        PlayerPrefs.SetInt(RefreshKey, Mathf.RoundToInt((float)mode.refreshRateRatio.value));
        PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Clears the stored mode and Unity's own stored window state. Deliberately not
    /// PlayerPrefs.DeleteAll, which would also take the player's key bindings and golf settings.
    /// </summary>
    public static void ClearStored()
    {
        PlayerPrefs.DeleteKey(VersionKey);
        PlayerPrefs.DeleteKey(WidthKey);
        PlayerPrefs.DeleteKey(HeightKey);
        PlayerPrefs.DeleteKey(RefreshKey);
        PlayerPrefs.DeleteKey(FullscreenKey);
        for (int i = 0; i < UnityScreenKeys.Length; i++)
            PlayerPrefs.DeleteKey(UnityScreenKeys[i]);
    }

    /// <summary>Clears the stored mode and goes borderless at the largest mode the display reports.</summary>
    public static void ResetToDefault()
    {
        ClearStored();
        PlayerPrefs.Save();

        Resolution fallback;
        if (TryGetFallback(out fallback))
            Apply(fallback, true);
    }

    /// <summary>The window as it is now, or as it is about to be after the last change, as a mode.</summary>
    public static Resolution CurrentResolution()
    {
        Resolution requested;
        bool requestedFullscreen;
        if (TryGetUnsettledRequest(out requested, out requestedFullscreen))
            return requested;

        Resolution current = new Resolution();
        current.width = Screen.width;
        current.height = Screen.height;
        current.refreshRateRatio = Screen.currentResolution.refreshRateRatio;
        return current;
    }

    private static bool ScreenIsFullscreen()
    {
        return Screen.fullScreenMode != FullScreenMode.Windowed;
    }

    // The last requested mode, while the window has not reported it yet.
    private static bool TryGetUnsettledRequest(out Resolution mode, out bool fullscreen)
    {
        mode = _requested;
        fullscreen = _requestedFullscreen;
        if (!_hasRequest)
            return false;

        bool arrived = Screen.width == _requested.width && Screen.height == _requested.height
            && ScreenIsFullscreen() == _requestedFullscreen;
        if (arrived || Time.realtimeSinceStartup - _requestedAt > RequestSettleSeconds)
        {
            _hasRequest = false;
            return false;
        }
        return true;
    }

    private static bool IsSupported(int width, int height)
    {
        if (width < MinWidth || height < MinHeight)
            return false;
        IList<Resolution> modes = Modes;
        for (int i = 0; i < modes.Count; i++)
        {
            if (modes[i].width == width && modes[i].height == height)
                return true;
        }
        return false;
    }

    // Screen.currentResolution is not trustworthy on every machine, so the largest mode the display
    // reports is preferred and currentResolution is only a last resort.
    private static bool TryGetFallback(out Resolution fallback)
    {
        fallback = default(Resolution);
        IList<Resolution> modes = Modes;
        if (modes.Count > 0)
        {
            fallback = modes[modes.Count - 1];
            return true;
        }
        Resolution current = Screen.currentResolution;
        if (current.width >= MinWidth && current.height >= MinHeight)
        {
            fallback = current;
            return true;
        }
        return false;
    }
}
