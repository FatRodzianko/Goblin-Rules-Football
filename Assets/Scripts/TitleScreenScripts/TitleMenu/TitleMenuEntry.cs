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

    [Header("Tab")]
    [Tooltip("A tab cannot turn itself off: choosing the one already showing leaves it showing.")]
    [SerializeField] private bool _staysOpen;

    [Tooltip("The panel inside this entry's border. Set it to recolour the border and the inside together, which is what makes a tab read as the chosen one.")]
    [SerializeField] private Graphic _interior;

    [SerializeField] private Color _borderWhenOpen = Color.white;
    [SerializeField] private Color _borderWhenClosed = Color.white;
    [SerializeField] private Color _interiorWhenOpen = Color.white;
    [SerializeField] private Color _interiorWhenClosed = Color.white;

    private Button _button;
    private Graphic _border;

    /// <summary>Raised when the player chooses this entry.</summary>
    public event Action<TitleMenuEntry> Chosen;

    public GameObject Opens { get { return _opens; } }

    public TitleMenuEntry DefaultChild { get { return _defaultChild; } }

    public bool IsLeaf { get { return _opens == null; } }

    /// <summary>Whether choosing this entry while it is already open should leave it open.</summary>
    public bool StaysOpen { get { return _staysOpen; } }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _border = GetComponent<Graphic>();
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

        // A tab turns inside out when it is the chosen one: dark inside, light edge.
        if (_interior != null)
        {
            _interior.color = isOpen ? _interiorWhenOpen : _interiorWhenClosed;
            if (_border != null)
                _border.color = isOpen ? _borderWhenOpen : _borderWhenClosed;
        }
    }

    /// <summary>Runs this entry's action, if it has one.</summary>
    public void RunAction()
    {
        if (_activated != null)
            _activated.Invoke();
    }
}
