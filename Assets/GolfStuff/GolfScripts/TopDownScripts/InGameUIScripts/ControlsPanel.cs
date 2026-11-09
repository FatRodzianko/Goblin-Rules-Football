using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// The list of controls on the hole info canvas. It is shown until the player dismisses it, and the
// small button left behind brings it back.
//
// The key names are read from the actions each time the list is shown rather than written into the
// text, because bindings are loaded from saved overrides at startup and can be rebound mid-game, so
// the asset is never what is actually bound.
public class ControlsPanel : MonoBehaviour
{
    // Per machine, like the rebinds themselves and the ball colour. Drop-in keeps its own answer,
    // so a player who has dismissed the list in a normal round still gets it the first time they
    // drop into a hole and meet the extra keys.
    private const string ShowPanelKey = "golf-show-controls";
    private const string ShowPanelDropInKey = "golf-show-controls-drop-in";

    static string ShownKey
    {
        get { return MapMakerDropInSession.IsActive ? ShowPanelDropInKey : ShowPanelKey; }
    }

    [SerializeField] private GameObject _panel;
    [SerializeField] private GameObject _showButton;
    [SerializeField] private TextMeshProUGUI _controlsText;
    [SerializeField] private Button _hideButtonControl;
    [SerializeField] private Button _showButtonControl;

    private void Awake()
    {
        if (_hideButtonControl)
            _hideButtonControl.onClick.AddListener(Hide);
        if (_showButtonControl)
            _showButtonControl.onClick.AddListener(Show);

        // This canvas is up from the moment the scene loads, and both it and the lobby are on screen
        // together while FishNet moves between them. Nothing here appears until a turn has started.
        HideEverything();
        GameplayManagerTopDownGolf.TurnStarted += HandleTurnStarted;
    }
    private void OnDestroy()
    {
        if (_hideButtonControl)
            _hideButtonControl.onClick.RemoveListener(Hide);
        if (_showButtonControl)
            _showButtonControl.onClick.RemoveListener(Show);

        GameplayManagerTopDownGolf.TurnStarted -= HandleTurnStarted;
    }
    // Re-applied on every turn rather than only the first, which also refreshes the key names if the
    // player rebinds something mid-round.
    void HandleTurnStarted(GolfPlayerTopDown player, bool isTeeOff)
    {
        ApplyShown(PlayerPrefs.GetInt(ShownKey, 1) == 1);
    }
    void HideEverything()
    {
        if (_panel)
            _panel.SetActive(false);
        if (_showButton)
            _showButton.SetActive(false);
    }
    public void Show()
    {
        RememberAndApply(true);
    }
    public void Hide()
    {
        RememberAndApply(false);
    }
    void RememberAndApply(bool shown)
    {
        PlayerPrefs.SetInt(ShownKey, shown ? 1 : 0);
        PlayerPrefs.Save();
        ApplyShown(shown);
    }
    void ApplyShown(bool shown)
    {
        if (shown)
            RefreshControlsText();

        if (_panel)
            _panel.SetActive(shown);
        if (_showButton)
            _showButton.SetActive(!shown);
    }
    void RefreshControlsText()
    {
        if (!_controlsText)
            return;

        GolfControls controls = InputManagerGolf.Controls;
        if (controls == null)
            return;

        StringBuilder text = new StringBuilder();

        AppendControl(text, "Start turn", controls.PromptPlayer.Continue);
        AppendControl(text, "Aim", controls.AimingActions.AimPosition);
        AppendControl(text, "Confirm aim", controls.AimingActions.SubmitAim);
        AppendControl(text, "Change club", controls.AimingActions.ChangeClub);
        AppendControl(text, "Short putt", controls.AimingActions.ShortPutt);
        AppendControl(text, "Zoom out", controls.AimingActions.ZoomOut);
        text.AppendLine();

        AppendControl(text, "Spin", controls.AimingActions.SpinPosition);
        AppendControl(text, "Reset spin", controls.AimingActions.ResetSpin);
        text.AppendLine();

        AppendControl(text, "Start swing", controls.HittingActions.StartHit);
        AppendControl(text, "Power, then accuracy", controls.HittingActions.SubmitHit);
        AppendControl(text, "Cancel swing", controls.HittingActions.CancelHit);
        text.AppendLine();

        AppendControl(text, "Scoreboard", controls.Misc.ViewScoreBoard);
        AppendControl(text, "Menu", controls.Misc.EscMenu);

        // Only bound while testing a hole straight from the map maker, so they are only worth listing
        // then. The same flag decides whether the player subscribes to them at all.
        if (MapMakerDropInSession.IsActive)
        {
            text.AppendLine();
            AppendControl(text, "Reset to tee", controls.DropInControls.ResetToTee);
            AppendControl(text, "Undo shot", controls.DropInControls.UndoShot);
            AppendControl(text, "Ball placement", controls.DropInControls.ToggleBallPlacement);
        }

        _controlsText.text = text.ToString();
    }
    static void AppendControl(StringBuilder text, string label, InputAction action)
    {
        string binding = InputManagerGolf.GetBindingName(action);
        if (string.IsNullOrEmpty(binding))
            return;

        text.AppendLine(label + ": " + binding);
    }
}
