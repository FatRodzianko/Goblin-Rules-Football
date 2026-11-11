using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// One tab of the CONTROLS panel: a game and a control scheme, such as Football on the keyboard.
///
/// Rows are generated from the action asset rather than authored one by one, so adding an action
/// needs no scene work and a row can never drift from the binding it claims to show.
/// </summary>
public class RebindingPanel : MonoBehaviour
{
    [SerializeField] private BindingStore _store;

    [Tooltip("Which asset to read: the football one, or golf's.")]
    [SerializeField] private bool _isGolf;

    [Tooltip("Control scheme as named in the .inputactions asset.")]
    [SerializeField] private string _controlScheme = "Keyboard and Mouse";

    [Tooltip("Only these maps are listed, and only these count when looking for a clash. They are the maps that are live at the same time.")]
    [SerializeField] private string[] _mapNames;

    [Tooltip("What a press has to come from, e.g. <Keyboard> and <Mouse>.")]
    [SerializeField] private string[] _deviceLayouts;

    [Tooltip("Controls the player must not be able to rebind onto.")]
    [SerializeField] private string[] _excludedControls;

    [SerializeField] private RectTransform _rowParent;
    [SerializeField] private RebindRow _rowTemplate;
    [SerializeField] private TextMeshProUGUI _message;

    [Tooltip("Shown when nothing has just happened.")]
    [SerializeField] private string _hint = "CLICK A BINDING TO CHANGE IT";

    private readonly List<RebindRow> _rows = new List<RebindRow>();
    private readonly List<InputActionMap> _scope = new List<InputActionMap>();
    private InputActionRebindingExtensions.RebindingOperation _operation;
    private RebindRow _listeningRow;
    private TitleMenuController _menu;
    private bool _holdingEscape;

    private static readonly Color SwapHighlight = new Color32(0xFF, 0xC0, 0x40, 0xFF);

    private void Awake()
    {
        if (_rowTemplate != null)
            _rowTemplate.gameObject.SetActive(false);
        _menu = GetComponentInParent<TitleMenuController>();
    }

    private void OnEnable()
    {
        BuildRows();
    }

    private void OnDisable()
    {
        CancelRebind();
        CleanUpOperation();
    }

    private InputActionAsset Asset
    {
        get
        {
            if (_store == null)
                return null;
            return _isGolf ? _store.GolfActions : _store.FootballActions;
        }
    }

    /// <summary>Starts listening for the control the player wants on this row.</summary>
    public void BeginRebind(RebindRow row)
    {
        if (row == null || row.Action == null)
            return;
        CancelRebind();
        SetMessage(_hint);
        _listeningRow = row;
        row.SetListening(true);

        int index = row.BindingIndex;
        bool wholeComposite = row.IsComposite;
        if (wholeComposite)
        {
            int firstPart = index + 1;
            if (firstPart >= row.Action.bindings.Count || !row.Action.bindings[firstPart].isPartOfComposite)
            {
                CancelRebind();
                return;
            }
            index = firstPart;
        }
        Listen(row.Action, index, wholeComposite);
    }

    /// <summary>Puts this row's binding back to the one the game ships with.</summary>
    public void ResetRow(RebindRow row)
    {
        if (row == null || row.Action == null)
            return;
        InputAction action = row.Action;
        InputActionMap map = action.actionMap;
        bool wasEnabled = map.enabled;
        if (wasEnabled)
            map.Disable();

        if (row.IsComposite)
        {
            for (int i = row.BindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++)
                action.RemoveBindingOverride(i);
        }
        else
        {
            action.RemoveBindingOverride(row.BindingIndex);
        }

        if (wasEnabled)
            map.Enable();

        RefreshRows();
        if (_store != null)
            _store.Save();
    }

    /// <summary>Wired to RESET TO DEFAULT. Puts every binding in this game's asset back.</summary>
    public void ResetAll()
    {
        if (_store == null)
            return;
        if (_isGolf)
            _store.ResetGolf();
        else
            _store.ResetFootball();
        RefreshRows();
        SetMessage("All bindings reset.");
    }

    /// <summary>Wired to SAVE.</summary>
    public void Save()
    {
        if (_store == null)
            return;
        _store.Save();
        SetMessage("Bindings saved.");
    }

    private void Listen(InputAction action, int bindingIndex, bool wholeComposite)
    {
        string previousPath = action.bindings[bindingIndex].effectivePath;

        var operation = action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .WithTimeout(6f);

        if (_deviceLayouts != null)
        {
            for (int i = 0; i < _deviceLayouts.Length; i++)
            {
                if (!string.IsNullOrEmpty(_deviceLayouts[i]))
                    operation = operation.WithControlsHavingToMatchPath(_deviceLayouts[i]);
            }
        }
        if (_excludedControls != null)
        {
            for (int i = 0; i < _excludedControls.Length; i++)
            {
                if (!string.IsNullOrEmpty(_excludedControls[i]))
                    operation = operation.WithControlsExcluding(_excludedControls[i]);
            }
        }

        operation.OnCancel(op =>
        {
            CleanUpOperation();
            RefreshRows();
        });

        operation.OnComplete(op =>
        {
            string displaced = BindingConflicts.SwapWithConflict(_scope, action, bindingIndex, previousPath);
            CleanUpOperation();
            RefreshRows();

            if (displaced != null)
            {
                SetMessage(ShortName(displaced) + " swapped to " + Readable(previousPath));
                HighlightRowFor(displaced);
            }

            if (_store != null)
                _store.Save();

            // A composite is rebound one part at a time.
            if (wholeComposite)
            {
                int next = bindingIndex + 1;
                if (next < action.bindings.Count && action.bindings[next].isPartOfComposite)
                    Listen(action, next, true);
            }
        });

        _operation = operation;
        // Escape cancels the rebind, so the menu must not also treat it as "go back".
        if (_menu != null && !_holdingEscape)
        {
            _menu.SuppressBack(true);
            _holdingEscape = true;
        }
        operation.Start();
    }

    private void CancelRebind()
    {
        if (_operation != null)
        {
            _operation.Cancel();
            CleanUpOperation();
        }
    }

    private void CleanUpOperation()
    {
        if (_operation != null)
        {
            _operation.Dispose();
            _operation = null;
        }
        _listeningRow = null;
        if (_menu != null && _holdingEscape)
        {
            _menu.SuppressBack(false);
            _holdingEscape = false;
        }
    }

    private void BuildRows()
    {
        InputActionAsset asset = Asset;
        if (asset == null || _rowTemplate == null || _rowParent == null)
            return;

        _scope.Clear();
        if (_mapNames != null)
        {
            for (int i = 0; i < _mapNames.Length; i++)
            {
                InputActionMap map = asset.FindActionMap(_mapNames[i], false);
                if (map != null)
                    _scope.Add(map);
                else
                    Debug.LogWarning("RebindingPanel: no action map called " + _mapNames[i] + ".");
            }
        }

        int used = 0;
        for (int m = 0; m < _scope.Count; m++)
        {
            InputActionMap map = _scope[m];
            for (int a = 0; a < map.actions.Count; a++)
            {
                InputAction action = map.actions[a];
                for (int b = 0; b < action.bindings.Count; b++)
                {
                    InputBinding binding = action.bindings[b];
                    if (binding.isPartOfComposite)
                        continue;
                    if (!binding.isComposite && !BelongsToScheme(binding))
                        continue;
                    if (binding.isComposite && !CompositeBelongsToScheme(action, b))
                        continue;

                    RebindRow row = RowAt(used);
                    row.gameObject.SetActive(true);
                    row.Bind(this, action, b, binding.isComposite, action.name);
                    used++;
                }
            }
        }

        for (int i = used; i < _rows.Count; i++)
            _rows[i].gameObject.SetActive(false);
    }

    private RebindRow RowAt(int index)
    {
        while (_rows.Count <= index)
            _rows.Add(Instantiate(_rowTemplate, _rowParent));
        return _rows[index];
    }

    private void RefreshRows()
    {
        for (int i = 0; i < _rows.Count; i++)
        {
            if (_rows[i] != null && _rows[i].gameObject.activeSelf)
                _rows[i].Refresh();
        }
    }

    private void HighlightRowFor(string actionName)
    {
        for (int i = 0; i < _rows.Count; i++)
        {
            RebindRow row = _rows[i];
            if (row == null || !row.gameObject.activeSelf || row.Action == null)
                continue;
            if (ShortName(actionName) == row.Action.name)
                row.Highlight(SwapHighlight, 2f);
        }
    }

    private bool BelongsToScheme(InputBinding binding)
    {
        if (string.IsNullOrEmpty(_controlScheme))
            return true;
        if (string.IsNullOrEmpty(binding.groups))
            return false;
        string[] groups = binding.groups.Split(InputBinding.Separator);
        for (int i = 0; i < groups.Length; i++)
        {
            if (string.Equals(groups[i], _controlScheme, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private bool CompositeBelongsToScheme(InputAction action, int compositeIndex)
    {
        for (int i = compositeIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++)
        {
            if (BelongsToScheme(action.bindings[i]))
                return true;
        }
        return false;
    }

    private void SetMessage(string text)
    {
        if (_message != null)
            _message.text = text;
    }

    // An action reads back as "Map/Action"; only the action part is shown.
    private static string ShortName(string actionName)
    {
        if (string.IsNullOrEmpty(actionName))
            return string.Empty;
        int slash = actionName.LastIndexOf('/');
        return slash >= 0 && slash + 1 < actionName.Length ? actionName.Substring(slash + 1) : actionName;
    }

    private static string Readable(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "nothing";
        return InputControlPath.ToHumanReadableString(path, InputControlPath.HumanReadableStringOptions.OmitDevice);
    }
}
