using UnityEngine;

/// <summary>
/// How one kind of lamp flickers. Shared by every light of that kind, so a whole bank is retuned by
/// editing one asset.
///
/// This is character, not appearance: the sprite belongs on the light's own renderer, which is what
/// lets a profile be reused across lamps that look nothing alike. Size is only a shorthand for how
/// hard a lamp struggles - a big flood is slow and reluctant, a small wall lamp buzzes and holds.
/// </summary>
[CreateAssetMenu(fileName = "LightFlickerProfile", menuName = "TitleScreen/Light Flicker Profile")]
public class LightFlickerProfile : ScriptableObject
{
    [Header("Strike-up: an old lamp fighting to come on")]
    [Tooltip("Seconds before this lamp's first catch. Power reaches a bank at once, but each fitting's starter takes its own moment to close, so keep this small - it should still read as one switch being thrown.")]
    [SerializeField] private Vector2 _firstCatchDelay = new Vector2(0f, 0.3f);

    [Tooltip("How many times it catches and dies before it holds, inclusive.")]
    [SerializeField] private Vector2Int _strikeAttempts = new Vector2Int(4, 8);

    [Tooltip("Seconds it holds light: on the first attempt, then on the last. It grows.")]
    [SerializeField] private Vector2 _strikeOnSeconds = new Vector2(0.04f, 0.3f);

    [Tooltip("Seconds it stays dark: after the first attempt, then after the last. It shrinks.")]
    [SerializeField] private Vector2 _strikeOffSeconds = new Vector2(0.5f, 0.05f);

    [Tooltip("Random variation on every interval, as a fraction of it. This is what keeps lamps that started together from staying in step.")]
    [Range(0f, 0.9f)]
    [SerializeField] private float _jitter = 0.35f;

    [Header("Idle twitch: a brief stutter that settles back")]
    [SerializeField] private Vector2Int _twitchBlinks = new Vector2Int(2, 5);
    [SerializeField] private Vector2 _twitchSeconds = new Vector2(0.03f, 0.12f);

    public Vector2 FirstCatchDelay { get { return _firstCatchDelay; } }

    public Vector2Int StrikeAttempts { get { return _strikeAttempts; } }

    public Vector2 StrikeOnSeconds { get { return _strikeOnSeconds; } }

    public Vector2 StrikeOffSeconds { get { return _strikeOffSeconds; } }

    public float Jitter { get { return _jitter; } }

    public Vector2Int TwitchBlinks { get { return _twitchBlinks; } }

    public Vector2 TwitchSeconds { get { return _twitchSeconds; } }
}
