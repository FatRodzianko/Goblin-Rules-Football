using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// One binding on the CONTROLS panel: what the action is called, what it is bound to now, a button
/// to change it and a button to put it back. Built from a template row by
/// <see cref="RebindingPanel"/>, which owns the rebinding itself.
/// </summary>
public class RebindRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _actionLabel;
    [SerializeField] private TextMeshProUGUI _bindingLabel;
    [SerializeField] private Button _rebindButton;
    [SerializeField] private Button _resetButton;
    [SerializeField] private Image _background;

    private RebindingPanel _owner;
    private InputAction _action;
    private int _bindingIndex;
    private bool _isComposite;
    private Color _restingColour;
    private float _highlightUntil;

    public InputAction Action { get { return _action; } }

    public int BindingIndex { get { return _bindingIndex; } }

    public bool IsComposite { get { return _isComposite; } }

    private void Awake()
    {
        if (_background != null)
            _restingColour = _background.color;
        if (_rebindButton != null)
            _rebindButton.onClick.AddListener(OnRebindClicked);
        if (_resetButton != null)
            _resetButton.onClick.AddListener(OnResetClicked);
    }

    private void OnDestroy()
    {
        if (_rebindButton != null)
            _rebindButton.onClick.RemoveListener(OnRebindClicked);
        if (_resetButton != null)
            _resetButton.onClick.RemoveListener(OnResetClicked);
    }

    private void Update()
    {
        if (_highlightUntil <= 0f || _background == null)
            return;
        if (Time.unscaledTime >= _highlightUntil)
        {
            _highlightUntil = 0f;
            _background.color = _restingColour;
        }
    }

    public void Bind(RebindingPanel owner, InputAction action, int bindingIndex, bool isComposite, string actionName)
    {
        _owner = owner;
        _action = action;
        _bindingIndex = bindingIndex;
        _isComposite = isComposite;
        if (_actionLabel != null)
            _actionLabel.text = actionName;
        Refresh();
    }

    public void Refresh()
    {
        if (_bindingLabel == null || _action == null)
            return;
        string display = _action.GetBindingDisplayString(_bindingIndex, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
        _bindingLabel.text = string.IsNullOrWhiteSpace(display) ? "UNBOUND" : display;
    }

    /// <summary>Marks the row that a rebind just moved, so a swap is never silent.</summary>
    public void Highlight(Color colour, float seconds)
    {
        if (_background == null)
            return;
        _background.color = colour;
        _highlightUntil = Time.unscaledTime + seconds;
    }

    /// <summary>Shows that this row is waiting for the player to press something.</summary>
    public void SetListening(bool listening)
    {
        if (_bindingLabel != null && listening)
            _bindingLabel.text = "PRESS A KEY...";
    }

    private void OnRebindClicked()
    {
        if (_owner != null)
            _owner.BeginRebind(this);
    }

    private void OnResetClicked()
    {
        if (_owner != null)
            _owner.ResetRow(this);
    }
}
