using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Drives the title screen's nested menus. Choosing an entry keeps it and every entry above it
/// highlighted, opens its panel, and closes whatever its siblings had open.
///
/// The levels are not declared anywhere. An entry's depth is read from the hierarchy: it is one
/// deeper than the open entry whose panel contains it. That keeps the nesting in the scene, where
/// it can be seen, instead of in a list that has to be kept in step with it.
/// </summary>
public class TitleMenuController : MonoBehaviour
{
    [Tooltip("Selected when the screen opens, so a gamepad has somewhere to start.")]
    [SerializeField] private TitleMenuEntry _firstSelected;

    private readonly List<TitleMenuEntry> _entries = new List<TitleMenuEntry>();

    // The chain of entries currently open, outermost first.
    private readonly List<TitleMenuEntry> _openPath = new List<TitleMenuEntry>();

    // Something that owns the Escape key for a moment, such as a rebind waiting for a key press.
    private int _backSuppressions;

    /// <summary>How many levels are open. Zero means only the first level is showing.</summary>
    public int OpenDepth { get { return _openPath.Count; } }

    private void Awake()
    {
        // A match pauses by setting timeScale to zero, and coming back to the title screen has to
        // undo that or every coroutine and animation here stays frozen.
        if (Time.timeScale != 1f)
            Time.timeScale = 1f;

        GetComponentsInChildren(true, _entries);
        for (int i = 0; i < _entries.Count; i++)
            _entries[i].Chosen += OnEntryChosen;
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i] != null)
                _entries[i].Chosen -= OnEntryChosen;
        }
    }

    private void OnEnable()
    {
        InputAction back = BackAction();
        if (back != null)
        {
            back.performed += OnBackPerformed;
            InputManager.Controls.EscMenu.Enable();
        }
        if (_firstSelected != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject);
    }

    private void OnDisable()
    {
        InputAction back = BackAction();
        if (back != null)
        {
            back.performed -= OnBackPerformed;
            InputManager.Controls.EscMenu.Disable();
        }
    }

    /// <summary>
    /// Holds off the Escape key while something else needs it, such as a rebind listening for a key
    /// press, which cancels on Escape itself. Counted, so overlapping holds behave.
    /// </summary>
    public void SuppressBack(bool suppress)
    {
        _backSuppressions += suppress ? 1 : -1;
        if (_backSuppressions < 0)
            _backSuppressions = 0;
    }

    // Escape and the gamepad's Start button, as the EscMenu action already binds them.
    private static InputAction BackAction()
    {
        Controls controls = InputManager.Controls;
        return controls != null ? controls.EscMenu.EscMenu : null;
    }

    private void OnBackPerformed(InputAction.CallbackContext context)
    {
        if (_backSuppressions > 0)
            return;
        Back();
    }

    /// <summary>Closes the deepest open level. Does nothing when only the first level is showing.</summary>
    public void Back()
    {
        if (_openPath.Count > 0)
            CloseFrom(_openPath.Count - 1);
    }

    /// <summary>Closes every open level.</summary>
    public void CloseAll()
    {
        CloseFrom(0);
    }

    private void OnEntryChosen(TitleMenuEntry entry)
    {
        int depth = DepthOf(entry);

        bool wasAlreadyOpen = depth < _openPath.Count && _openPath[depth] == entry;

        // A tab is a choice between siblings, not a switch: choosing the one already showing has to
        // leave it showing, or the panel it belongs to would be left with nothing in it.
        if (wasAlreadyOpen && entry.StaysOpen)
            return;

        // Choosing the entry that is already open at this depth closes it again.
        CloseFrom(depth);
        if (wasAlreadyOpen)
            return;

        if (!entry.IsLeaf)
        {
            entry.SetOpen(true);
            _openPath.Add(entry);
        }

        entry.RunAction();

        // A panel comes up with its first tab already picked, so the player never sees an empty one.
        if (entry.DefaultChild != null)
            OnEntryChosen(entry.DefaultChild);
    }

    // An entry sitting inside an open entry's panel is one level deeper than that entry. Anything
    // not inside an open panel belongs to the first level.
    private int DepthOf(TitleMenuEntry entry)
    {
        for (int i = _openPath.Count - 1; i >= 0; i--)
        {
            GameObject opened = _openPath[i].Opens;
            if (opened != null && entry.transform.IsChildOf(opened.transform))
                return i + 1;
        }
        return 0;
    }

    private void CloseFrom(int depth)
    {
        if (depth >= _openPath.Count)
            return;
        for (int i = _openPath.Count - 1; i >= depth; i--)
            _openPath[i].SetOpen(false);
        _openPath.RemoveRange(depth, _openPath.Count - depth);
    }
}
