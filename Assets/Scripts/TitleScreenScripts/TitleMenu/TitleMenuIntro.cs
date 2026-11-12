using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Throws the main menu buttons up from below the screen when the game is opened, so the menu
/// arrives with the power rather than being there from the start.
///
/// Like the lights, this belongs to opening the game rather than to the scene: coming back to the
/// menu from a match finds the buttons already in place. The flag is static, so it outlives the
/// scene but not the process.
///
/// Nothing can be clicked until the last button has landed. A button is held by switching its
/// Button component off rather than by clearing <c>interactable</c>, which would grey it out while
/// it flew.
/// </summary>
public class TitleMenuIntro : MonoBehaviour
{
    [Tooltip("The buttons to throw, in the order they should arrive. The first one sets the timing.")]
    [SerializeField] private RectTransform[] _buttons;

    [Tooltip("Takes the arrival time from the lights, so the first button lands as the sparks fly. Leave empty to use the time below instead.")]
    [SerializeField] private TitleScreenLights _arriveWith;

    [Tooltip("Seconds from the scene opening to the first button landing, when no lights are assigned.")]
    [SerializeField] private float _firstArrivesAt = 1.5f;

    [Tooltip("How long one button is in the air. Short is what makes it snap.")]
    [SerializeField] private float _flyTime = 0.26f;

    [Tooltip("Seconds between one button setting off and the next.")]
    [SerializeField] private float _stagger = 0.07f;

    [Tooltip("How far below the bottom of the screen the buttons wait, on top of their own height.")]
    [SerializeField] private float _startBelowScreen = 40f;

    [Tooltip("Flight from start to finish. Overshooting past 1 near the end is what gives it the snap.")]
    [SerializeField] private AnimationCurve _ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // Opening the game is something that happens once, not every time the menu is loaded.
    private static bool _played;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnBoot()
    {
        // Statics survive entering play mode when domain reload is off, which would mean the intro
        // played once and never again for the rest of the editor session.
        _played = false;
    }

    /// <summary>Whether the buttons have already flown in this time the game was started.</summary>
    public static bool HasPlayed { get { return _played; } }

    /// <summary>Whether the buttons are in the air right now.</summary>
    public bool IsPlaying { get; private set; }

    private void Start()
    {
        if (_played || _buttons == null || _buttons.Length == 0)
            return;
        _played = true;
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        IsPlaying = true;

        float[] landAt = new float[_buttons.Length];
        for (int i = 0; i < _buttons.Length; i++)
            landAt[i] = _buttons[i] != null ? _buttons[i].anchoredPosition.y : 0f;

        float waitBelow = StartHeight();
        for (int i = 0; i < _buttons.Length; i++)
            MoveTo(i, waitBelow);
        SetButtonsLive(false);

        // The first button lands on the beat; everything before that is the wait for it to set off.
        float lead = ArrivalTime() - _flyTime;
        if (lead > 0f)
            yield return new WaitForSeconds(lead);

        float span = _stagger * (_buttons.Length - 1) + _flyTime;
        float elapsed = 0f;
        while (elapsed < span)
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                float t = Mathf.Clamp01((elapsed - _stagger * i) / _flyTime);
                // Unclamped, so a curve that overshoots past its mark actually overshoots.
                MoveTo(i, Mathf.LerpUnclamped(waitBelow, landAt[i], _ease.Evaluate(t)));
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < _buttons.Length; i++)
            MoveTo(i, landAt[i]);
        SetButtonsLive(true);
        IsPlaying = false;
    }

    private float ArrivalTime()
    {
        return _arriveWith != null ? _arriveWith.OpeningDelay : _firstArrivesAt;
    }

    // Far enough down that the tallest button is clear of the bottom edge before it sets off.
    private float StartHeight()
    {
        float height = 0f;
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            height = ((RectTransform)canvas.transform).rect.height;

        float tallest = 0f;
        for (int i = 0; i < _buttons.Length; i++)
            if (_buttons[i] != null)
                tallest = Mathf.Max(tallest, _buttons[i].rect.height);

        return -(height + tallest + _startBelowScreen);
    }

    private void MoveTo(int index, float y)
    {
        RectTransform rt = _buttons[index];
        if (rt == null)
            return;
        Vector2 p = rt.anchoredPosition;
        p.y = y;
        rt.anchoredPosition = p;
    }

    private void SetButtonsLive(bool live)
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
            if (_buttons[i] == null)
                continue;
            var button = _buttons[i].GetComponent<Button>();
            if (button != null)
                button.enabled = live;
        }

        // A gamepad would otherwise be sitting on a button that is still in the air.
        if (EventSystem.current == null)
            return;
        if (!live)
            EventSystem.current.SetSelectedGameObject(null);
        else if (_buttons[0] != null)
            EventSystem.current.SetSelectedGameObject(_buttons[0].gameObject);
    }
}
