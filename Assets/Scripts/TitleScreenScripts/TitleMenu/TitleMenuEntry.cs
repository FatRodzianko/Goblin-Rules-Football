using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// One selectable item in the title menu: the button, whatever it opens, the highlight shown while
/// it is open, and the bottom-bar content that belongs to it.
///
/// An entry that opens something is a branch; one that opens nothing is a leaf and runs its action
/// instead. The entry itself only reports that it was chosen - <see cref="TitleMenuController"/>
/// decides what that means for the rest of the menu.
/// </summary>
[RequireComponent(typeof(Button))]
public class TitleMenuEntry : MonoBehaviour
{
    [Tooltip("Opened while this entry is chosen. Leave empty for a leaf, which runs Activated instead.")]
    [SerializeField] private GameObject _opens;

    [Tooltip("Shown while this entry is open. Usually the highlight border drawn over the button.")]
    [SerializeField] private GameObject _highlight;

    [Tooltip("Shown in the bottom bar while this entry is open, such as CREATE GAME or SAVE.")]
    [SerializeField] private GameObject _bottomBarContent;

    [Tooltip("Chosen automatically once this entry opens, so a panel comes up with a tab already picked.")]
    [SerializeField] private TitleMenuEntry _defaultChild;

    [Tooltip("Runs when this entry is chosen, after whatever it opens is shown.")]
    [SerializeField] private UnityEvent _activated;

    private Button _button;

    /// <summary>Raised when the player chooses this entry.</summary>
    public event Action<TitleMenuEntry> Chosen;

    public GameObject Opens { get { return _opens; } }

    public TitleMenuEntry DefaultChild { get { return _defaultChild; } }

    public bool IsLeaf { get { return _opens == null; } }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClicked);

        // A level that has just been opened starts with nothing chosen inside it. This also runs the
        // first time an entry on a deeper level is activated, which is when its own Awake happens.
        SetOpen(false);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnClicked);
    }

    private void OnClicked()
    {
        if (Chosen != null)
            Chosen(this);
    }

    /// <summary>Shows or hides this entry's panel, highlight and bottom-bar content together.</summary>
    public void SetOpen(bool isOpen)
    {
        if (_opens != null)
            _opens.SetActive(isOpen);
        if (_highlight != null)
            _highlight.SetActive(isOpen);
        if (_bottomBarContent != null)
            _bottomBarContent.SetActive(isOpen);
    }

    /// <summary>Runs this entry's action, if it has one.</summary>
    public void RunAction()
    {
        if (_activated != null)
            _activated.Invoke();
    }
}
