using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Turns the title screen's music on and off, and keeps its own label in step with whatever the
/// music is actually doing - including music that was already playing when the scene loaded, which
/// is what happens on the way back from a match.
/// </summary>
[RequireComponent(typeof(Button))]
public class MusicToggleButton : MonoBehaviour
{
    [Tooltip("The icon. It shows the action the button performs, matching the old title screen: the off icon while music is playing.")]
    [SerializeField] private Image _icon;
    [SerializeField] private Sprite _turnOnSprite;
    [SerializeField] private Sprite _turnOffSprite;

    [Tooltip("Optional. The icon alone is enough.")]
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private string _turnOnText = "TURN MUSIC ON";
    [SerializeField] private string _turnOffText = "TURN MUSIC OFF";

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(Toggle);
    }

    private void Start()
    {
        if (MusicManager.instance != null)
            MusicManager.instance.MusicTurnedOn += OnMusicTurnedOn;
        Refresh();
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(Toggle);
        if (MusicManager.instance != null)
            MusicManager.instance.MusicTurnedOn -= OnMusicTurnedOn;
    }

    public void Toggle()
    {
        if (MusicManager.instance == null)
            return;
        if (MusicManager.instance.IsMusicPlaying())
            MusicManager.instance.TurnMusicOff();
        else
            MusicManager.instance.TurnMusicOn();
        Refresh();
    }

    private void OnMusicTurnedOn(bool isPlaying)
    {
        SetLabel(isPlaying);
    }

    private void Refresh()
    {
        SetLabel(MusicManager.instance != null && MusicManager.instance.IsMusicPlaying());
    }

    private void SetLabel(bool isPlaying)
    {
        if (_label != null)
            _label.text = isPlaying ? _turnOffText : _turnOnText;
        if (_icon != null)
        {
            Sprite sprite = isPlaying ? _turnOffSprite : _turnOnSprite;
            if (sprite != null)
                _icon.sprite = sprite;
        }
    }
}
