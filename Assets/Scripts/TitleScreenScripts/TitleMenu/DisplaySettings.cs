using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The title screen's display options: changing the mode with a confirmation step, and the
/// Shift-at-startup way back from a mode the player cannot see. The setting itself, and every rule
/// about what may be applied, belongs to <see cref="DisplayPreferences"/>.
///
///  - a change is applied first and only stored once the player confirms it, so a mode they cannot
///    see reverts on its own;
///  - holding Shift while the game starts clears the stored mode and boots windowed.
/// </summary>
public class DisplaySettings : MonoBehaviour
{
    private const int SafeModeWidth = 1280;
    private const int SafeModeHeight = 720;
    private const float SafeModeWindowSeconds = 2f;

    [Tooltip("Seconds the player has to confirm a new display mode before it reverts.")]
    [SerializeField] private int _confirmSeconds = 15;

    // Shift only means "boot in safe mode" while the game is starting, not on every return to the menu.
    private static bool _safeModeWindowUsed;

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
    public IList<Resolution> Modes { get { return DisplayPreferences.Modes; } }

    public bool IsFullscreen { get { return DisplayPreferences.IsFullscreen; } }

    public bool IsAwaitingConfirmation { get { return _confirmRoutine != null; } }

    private void Awake()
    {
        DisplayPreferences.RefreshModes();
    }

    private void Start()
    {
        if (!_safeModeWindowUsed)
        {
            _safeModeWindowUsed = true;
            StartCoroutine(WatchForSafeModeKey());
        }
    }

    /// <summary>The index into <see cref="Modes"/> that matches the window right now, or 0.</summary>
    public int CurrentModeIndex()
    {
        return DisplayPreferences.CurrentModeIndex();
    }

    /// <summary>
    /// Applies a mode straight away and starts the confirmation countdown. Nothing is written to
    /// disk until <see cref="KeepPending"/> is called.
    /// </summary>
    public void ApplyAndAwaitConfirmation(int modeIndex, bool fullscreen)
    {
        Resolution mode;
        if (!DisplayPreferences.TryGetMode(modeIndex, out mode))
            return;

        if (_confirmRoutine != null)
            StopConfirming();

        _previous = DisplayPreferences.CurrentResolution();
        _previousFullscreen = IsFullscreen;
        _pending = mode;
        _pendingFullscreen = fullscreen;

        if (!DisplayPreferences.Apply(_pending, fullscreen))
            return;

        // Nothing changed, so there is nothing for the player to confirm.
        if (_pending.width == _previous.width && _pending.height == _previous.height
            && fullscreen == _previousFullscreen)
        {
            DisplayPreferences.Store(_pending, fullscreen);
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
        DisplayPreferences.Store(_pending, _pendingFullscreen);
    }

    /// <summary>Puts back the mode that was in use before the last change.</summary>
    public void RevertPending()
    {
        if (_confirmRoutine == null)
            return;
        StopConfirming();
        DisplayPreferences.Apply(_previous, _previousFullscreen);
    }

    /// <summary>Clears the stored display mode and goes back to the display's own default.</summary>
    public void ResetToDefault()
    {
        DisplayPreferences.ResetToDefault();
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
        DisplayPreferences.Apply(_previous, _previousFullscreen);
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
                DisplayPreferences.ClearStored();
                PlayerPrefs.Save();
                Screen.SetResolution(SafeModeWidth, SafeModeHeight, FullScreenMode.Windowed);
                yield break;
            }
            yield return null;
        }
    }
}
