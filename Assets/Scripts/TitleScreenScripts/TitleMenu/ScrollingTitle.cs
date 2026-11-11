using TMPro;
using UnityEngine;

/// <summary>
/// Runs the game's name in from the right of the title panel and settles it centred, like a
/// scoreboard ticker.
///
/// This object's own rect is the window the text is clipped to - it needs a RectMask2D - so the
/// text can sit outside it without spilling across the screen.
/// </summary>
public class ScrollingTitle : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;

    [Tooltip("Canvas units per second. The canvas is 768 wide, so 160 crosses the panel in under three seconds.")]
    [SerializeField] private float _scrollSpeed = 160f;

    [Tooltip("Keep scrolling and wrap around, instead of stopping centred.")]
    [SerializeField] private bool _loop;

    [Tooltip("Hidden until Play is called, so the panel is blank while the lights are still out.")]
    [SerializeField] private bool _hiddenUntilPlayed = true;

    private RectTransform _window;
    private RectTransform _textRect;
    private float _targetX;
    private float _width;
    private bool _scrolling;

    public bool IsScrolling { get { return _scrolling; } }

    private void Awake()
    {
        _window = (RectTransform)transform;
        if (_text != null)
        {
            _textRect = _text.rectTransform;
            if (_hiddenUntilPlayed)
                _text.enabled = false;
        }
    }

    private void Update()
    {
        if (!_scrolling || _textRect == null)
            return;

        Vector2 position = _textRect.anchoredPosition;
        position.x -= _scrollSpeed * Time.deltaTime;

        if (_loop)
        {
            // Off the left edge entirely, so bring it back round to the right.
            if (position.x < -_width * 0.5f)
                position.x = _window.rect.width + _width * 0.5f;
        }
        else if (position.x <= _targetX)
        {
            position.x = _targetX;
            _scrolling = false;
        }

        _textRect.anchoredPosition = position;
    }

    /// <summary>Starts the text moving in from the right.</summary>
    public void Play()
    {
        if (_text == null)
            return;

        Prepare();
        _textRect.anchoredPosition = new Vector2(_window.rect.width + _width * 0.5f, 0f);
        _text.enabled = true;
        _scrolling = true;
    }

    /// <summary>Shows the title centred with no scroll, for when the opening is skipped.</summary>
    public void ShowNow()
    {
        if (_text == null)
            return;
        Prepare();
        _textRect.anchoredPosition = new Vector2(_targetX, 0f);
        _text.enabled = true;
        _scrolling = false;
    }

    public void Hide()
    {
        _scrolling = false;
        if (_text != null)
            _text.enabled = false;
    }

    // The text has to be a free-floating rect of its own width to be moved across the window, so it
    // is detached from whatever anchoring it was authored with.
    private void Prepare()
    {
        // Resolved here rather than trusting Awake, because Play is wired to a UnityEvent: it can
        // reach a component whose Awake has not run, or one whose private fields were cleared by a
        // domain reload mid-play.
        if (_window == null)
            _window = (RectTransform)transform;
        if (_textRect == null)
            _textRect = _text.rectTransform;

        _width = _text.GetPreferredValues(_text.text, 100000f, 0f).x;
        _textRect.anchorMin = new Vector2(0f, 0.5f);
        _textRect.anchorMax = new Vector2(0f, 0.5f);
        _textRect.pivot = new Vector2(0.5f, 0.5f);
        _textRect.sizeDelta = new Vector2(_width, _window.rect.height);
        _text.alignment = TextAlignmentOptions.Center;
        _text.enableWordWrapping = false;
        _targetX = _window.rect.width * 0.5f;
    }
}
