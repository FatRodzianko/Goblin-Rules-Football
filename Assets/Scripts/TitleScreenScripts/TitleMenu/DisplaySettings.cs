using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Owns the game's resolution and window mode.
///
/// The rules here exist because the old handling could write a 0x0 mode into the registry and leave
/// players unable to start the game:
///  - with nothing stored, nothing is applied, so a first launch keeps the player's own default;
///  - a mode is only applied if the display actually reports it and it clears a hard floor, so a
///    bad value can never reach Screen.SetResolution;
///  - a stored mode the current display cannot do is replaced, and the replacement is written back,
///    so the same bad value cannot trap the player twice;
///  - a change is applied first and only stored once the player confirms it, so a mode they cannot
///    see reverts on its own;
///  - holding Shift while the game starts clears the stored mode and boots windowed.
///
/// Only borderless fullscreen and windowed are offered. Exclusive fullscreen is the mode that can
/// leave a display showing nothing at all, and it buys this game nothing.
/// </summary>
public class DisplaySettings : MonoBehaviour
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
    private const int SafeModeWidth = 1280;
    private const int SafeModeHeight = 720;
    private const float SafeModeWindowSeconds = 2f;

    [Tooltip("Seconds the player has to confirm a new display mode before it reverts.")]
    [SerializeField] private int _confirmSeconds = 15;

    private readonly List<Resolution> _modes = new List<Resolution>();
    private Resolution _pending;
    private bool _pendingFullscreen;
    private Resolution _previous;
    private bool _previousFullscreen;
    private Coroutine _confirmRoutine;

    /// <summary>Raised each second while a change is waiting to be confirmed, with the seconds left.</summary>
    public event System.Action<int> ConfirmCountdown;

    /// <summary>Raised when a change stops waiting, whether it was kept or reverted.</summary>
    public event System.Action ConfirmEnded;

    /// <summary>Every mode this display reports that is big enough to play on, smallest first.</summary>
    public IList<Resolution> Modes { get { return _modes; } }

    public bool IsFullscreen { get { return Screen.fullScreenMode != FullScreenMode.Windowed; } }

    public bool IsAwaitingConfirmation { get { return _confirmRoutine != null; } }

    private void Awake()
    {
        CollectModes();
        ApplyStored();
    }

    private void Start()
    {
        StartCoroutine(WatchForSafeModeKey());
    }

    /// <summary>The index into <see cref="Modes"/> that matches the window right now, or 0.</summary>
    public int CurrentModeIndex()
    {
        for (int i = 0; i < _modes.Count; i++)
        {
            if (_modes[i].width == Screen.width && _modes[i].height == Screen.height)
                return i;
        }
        return 0;
    }

    /// <summary>
    /// Applies a mode straight away and starts the confirmation countdown. Nothing is written to
    /// disk until <see cref="KeepPending"/> is called.
    /// </summary>
    public void ApplyAndAwaitConfirmation(int modeIndex, bool fullscreen)
    {
        if (modeIndex < 0 || modeIndex >= _modes.Count)
            return;

        if (_confirmRoutine != null)
            StopConfirming();

        _previous = CurrentResolution();
        _previousFullscreen = IsFullscreen;
        _pending = _modes[modeIndex];
        _pendingFullscreen = fullscreen;

        if (!Apply(_pending, fullscreen))
            return;

        // Nothing changed, so there is nothing for the player to confirm.
        if (_pending.width == _previous.width && _pending.height == _previous.height
            && fullscreen == _previousFullscreen)
        {
            Store(_pending, fullscreen);
            return;
        }

        _confirmRoutine = StartCoroutine(AwaitConfirmation());
    }

    /// <summary>Keeps the mode that is on screen and writes it to disk.</summary>
    public void KeepPending()
    {
        if (_confirmRoutine == null)
            return;
        StopConfirming();
        Store(_pending, _pendingFullscreen);
    }

    /// <summary>Puts back the mode that was in use before the last change.</summary>
    public void RevertPending()
    {
        if (_confirmRoutine == null)
            return;
        StopConfirming();
        Apply(_previous, _previousFullscreen);
    }

    /// <summary>
    /// Clears this game's display preference and Unity's own stored window state. Deliberately not
    /// PlayerPrefs.DeleteAll, which would also take the player's key bindings and golf settings.
    /// </summary>
    public void ResetToDefault()
    {
        ClearStored();
        PlayerPrefs.Save();

        Resolution fallback;
        if (TryGetFallback(out fallback))
            Apply(fallback, true);
    }

    private IEnumerator AwaitConfirmation()
    {
        for (int left = _confirmSeconds; left > 0; left--)
        {
            if (ConfirmCountdown != null)
                ConfirmCountdown(left);
            yield return new WaitForSecondsRealtime(1f);
        }
        _confirmRoutine = null;
        if (ConfirmEnded != null)
            ConfirmEnded();
        Apply(_previous, _previousFullscreen);
    }

    private void StopConfirming()
    {
        if (_confirmRoutine != null)
            StopCoroutine(_confirmRoutine);
        _confirmRoutine = null;
        if (ConfirmEnded != null)
            ConfirmEnded();
    }

    // Holding Shift as the game starts clears the stored mode and drops to a small window, so a
    // player who cannot see the game has a way back that does not involve the registry.
    private IEnumerator WatchForSafeModeKey()
    {
        float until = Time.realtimeSinceStartup + SafeModeWindowSeconds;
        while (Time.realtimeSinceStartup < until)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed))
            {
                Debug.Log("DisplaySettings: Shift held at startup, clearing the stored display mode.");
                ClearStored();
                PlayerPrefs.Save();
                Screen.SetResolution(SafeModeWidth, SafeModeHeight, FullScreenMode.Windowed);
                yield break;
            }
            yield return null;
        }
    }

    private void ApplyStored()
    {
        // A launch with nothing stored leaves the player's own default alone. Writing a mode nobody
        // chose is what produced the 0x0 window.
        if (!PlayerPrefs.HasKey(WidthKey) || !PlayerPrefs.HasKey(HeightKey))
            return;

        Resolution stored = new Resolution();
        stored.width = PlayerPrefs.GetInt(WidthKey, 0);
        stored.height = PlayerPrefs.GetInt(HeightKey, 0);
        stored.refreshRate = PlayerPrefs.GetInt(RefreshKey, 0);
        bool fullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) != 0;

        if (!IsSupported(stored.width, stored.height))
        {
            Resolution fallback;
            if (!TryGetFallback(out fallback))
            {
                Debug.LogWarning("DisplaySettings: stored mode is unusable and no replacement was found; leaving the window as it is.");
                return;
            }
            Debug.Log("DisplaySettings: stored mode " + stored.width + "x" + stored.height
                + " is not available here; falling back to " + fallback.width + "x" + fallback.height + ".");
            stored = fallback;
            fullscreen = true;
            // Write the replacement back so the bad value cannot strand the player again.
            Store(stored, fullscreen);
        }

        Apply(stored, fullscreen);
    }

    private bool Apply(Resolution mode, bool fullscreen)
    {
        if (!IsSupported(mode.width, mode.height))
        {
            Debug.LogWarning("DisplaySettings: refused to apply " + mode.width + "x" + mode.height + ".");
            return false;
        }
        FullScreenMode screenMode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        if (mode.refreshRate > 0)
            Screen.SetResolution(mode.width, mode.height, screenMode, mode.refreshRate);
        else
            Screen.SetResolution(mode.width, mode.height, screenMode);
        return true;
    }

    private void Store(Resolution mode, bool fullscreen)
    {
        PlayerPrefs.SetInt(VersionKey, SettingsVersion);
        PlayerPrefs.SetInt(WidthKey, mode.width);
        PlayerPrefs.SetInt(HeightKey, mode.height);
        PlayerPrefs.SetInt(RefreshKey, mode.refreshRate);
        PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private static void ClearStored()
    {
        PlayerPrefs.DeleteKey(VersionKey);
        PlayerPrefs.DeleteKey(WidthKey);
        PlayerPrefs.DeleteKey(HeightKey);
        PlayerPrefs.DeleteKey(RefreshKey);
        PlayerPrefs.DeleteKey(FullscreenKey);
        for (int i = 0; i < UnityScreenKeys.Length; i++)
            PlayerPrefs.DeleteKey(UnityScreenKeys[i]);
    }

    private void CollectModes()
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

    private bool IsSupported(int width, int height)
    {
        if (width < MinWidth || height < MinHeight)
            return false;
        for (int i = 0; i < _modes.Count; i++)
        {
            if (_modes[i].width == width && _modes[i].height == height)
                return true;
        }
        return false;
    }

    // Screen.currentResolution is not trustworthy on every machine, so the largest mode the display
    // reports is preferred and currentResolution is only a last resort.
    private bool TryGetFallback(out Resolution fallback)
    {
        fallback = default(Resolution);
        if (_modes.Count > 0)
        {
            fallback = _modes[_modes.Count - 1];
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

    private static Resolution CurrentResolution()
    {
        Resolution current = new Resolution();
        current.width = Screen.width;
        current.height = Screen.height;
        current.refreshRate = Screen.currentResolution.refreshRate;
        return current;
    }
}
