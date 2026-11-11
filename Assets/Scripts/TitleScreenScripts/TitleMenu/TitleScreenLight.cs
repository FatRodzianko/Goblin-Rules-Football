using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One light on the title screen, with two very different flickers.
///
/// Striking up is an old lamp fighting to catch: it flashes, dies, sits dark a long moment, catches
/// again for a little longer, and the dark gaps shrink until it holds. The on and off times are
/// ramped in opposite directions across the attempt, which is what makes it read as struggling
/// rather than strobing - a single random interval for both would look like a disco.
///
/// An idle twitch is the opposite: a short, even stutter that settles back to where it was.
///
/// How hard this particular lamp struggles comes from its <see cref="LightFlickerProfile"/>, so all
/// the flood lights share one character and all the wall lamps share another.
/// <see cref="FlickerStarted"/> fires at each first visible catch, which is where a sound belongs -
/// one listener covers every light.
/// </summary>
public class TitleScreenLight : MonoBehaviour
{
    [Tooltip("How this lamp flickers. Flood lights, wall lamps and so on each have their own profile asset.")]
    [SerializeField] private LightFlickerProfile _profile;

    [Tooltip("Lights start dark so the screen can bring them up. Tick this for one that is already on.")]
    [SerializeField] private bool _litAtStart;

    /// <summary>
    /// Raised by every light the moment its flicker starts. Subscribe once from a sound or effect
    /// component; unsubscribe in OnDisable, as this is static.
    /// </summary>
    public static event Action<TitleScreenLight> FlickerStarted;

    /// <summary>
    /// Raised every time this one light goes on or off, each blink of a flicker included. It is for
    /// anything that has to light up with the lamp but is not a renderer the lamp can switch for
    /// itself - a shader property, say. Subscribers apply the current <see cref="IsLit"/> once on
    /// subscribing, since component start order decides whether they were listening for the first.
    /// </summary>
    public event Action<bool> LitChanged;

    private static LightFlickerProfile _defaultProfile;

    private SpriteRenderer _sprite;
    private Image _image;
    private SpriteMask _mask;
    private Coroutine _routine;

    public bool IsLit { get; private set; }

    public bool IsFlickering { get { return _routine != null; } }

    /// <summary>The profile in use, falling back to built-in defaults if none was assigned.</summary>
    public LightFlickerProfile Profile
    {
        get
        {
            if (_profile != null)
                return _profile;
            if (_defaultProfile == null)
                _defaultProfile = ScriptableObject.CreateInstance<LightFlickerProfile>();
            return _defaultProfile;
        }
    }

    private void Awake()
    {
        // Either kind of renderer, so a light can live in the world or on a canvas.
        _sprite = GetComponent<SpriteRenderer>();
        _image = GetComponent<Image>();
        _mask = GetComponent<SpriteMask>();
        if (_profile == null)
            Debug.LogWarning("TitleScreenLight on " + name + " has no flicker profile; using defaults.", this);
        SetLit(_litAtStart);
    }

    private void OnDisable()
    {
        // Unity has already stopped the coroutine; drop the handle so the light is not stuck "busy".
        _routine = null;
    }

    /// <summary>Struggles up to lit, like a cold lamp receiving power.</summary>
    public void FlickerOn()
    {
        Begin(RunStrike(true));
    }

    /// <summary>Gutters down to dark: the same shape run backwards.</summary>
    public void FlickerOff()
    {
        Begin(RunStrike(false));
    }

    /// <summary>
    /// A twitch that settles back to the state it was already in. This is what an idle flicker
    /// wants: a light that is on should still be on afterwards.
    /// </summary>
    public void Flicker()
    {
        Begin(RunTwitch(IsLit));
    }

    /// <summary>Sets the light with no flicker at all.</summary>
    public void SetLit(bool lit)
    {
        IsLit = lit;
        if (_sprite != null)
            _sprite.enabled = lit;
        if (_image != null)
            _image.enabled = lit;

        // The mask is what cuts the night filter open, so it has to go out with the lamp. Left on,
        // an unlit fixture would still be pouring light onto the scene.
        if (_mask != null)
            _mask.enabled = lit;

        if (LitChanged != null)
            LitChanged(lit);
    }

    private void Begin(IEnumerator routine)
    {
        // A light already mid-flicker keeps the flicker it has, rather than restarting and
        // stuttering twice as long.
        if (!isActiveAndEnabled || _routine != null)
            return;
        _routine = StartCoroutine(routine);
    }

    private IEnumerator RunStrike(bool endLit)
    {
        LightFlickerProfile profile = Profile;

        // The starters in a bank do not close in unison. Without this every lamp catches on the
        // same frame, which reads as a deliberate strobe rather than a row of separate fittings.
        float wait = UnityEngine.Random.Range(profile.FirstCatchDelay.x, profile.FirstCatchDelay.y);
        if (wait > 0f)
            yield return new WaitForSeconds(wait);

        // Raised at the first visible catch, not at the request, so a sound lands with the light.
        if (FlickerStarted != null)
            FlickerStarted(this);

        int attempts = UnityEngine.Random.Range(profile.StrikeAttempts.x, profile.StrikeAttempts.y + 1);
        for (int i = 0; i < attempts; i++)
        {
            // Across the attempt the light holds a little longer each time and the dark gaps get
            // shorter. A lamp going out runs the same ramp backwards.
            float t = attempts == 1 ? 1f : (float)i / (attempts - 1);
            if (!endLit)
                t = 1f - t;

            SetLit(true);
            yield return new WaitForSeconds(Jittered(Mathf.Lerp(profile.StrikeOnSeconds.x, profile.StrikeOnSeconds.y, t), profile.Jitter));
            SetLit(false);
            yield return new WaitForSeconds(Jittered(Mathf.Lerp(profile.StrikeOffSeconds.x, profile.StrikeOffSeconds.y, t), profile.Jitter));
        }

        // Set the end state rather than trusting the attempt count to be odd or even.
        SetLit(endLit);
        _routine = null;
    }

    private IEnumerator RunTwitch(bool endLit)
    {
        LightFlickerProfile profile = Profile;

        if (FlickerStarted != null)
            FlickerStarted(this);

        int blinks = UnityEngine.Random.Range(profile.TwitchBlinks.x, profile.TwitchBlinks.y + 1);
        for (int i = 0; i < blinks; i++)
        {
            SetLit(!IsLit);
            yield return new WaitForSeconds(UnityEngine.Random.Range(profile.TwitchSeconds.x, profile.TwitchSeconds.y));
        }

        SetLit(endLit);
        _routine = null;
    }

    private static float Jittered(float seconds, float jitter)
    {
        float spread = seconds * jitter;
        return Mathf.Max(0.01f, seconds + UnityEngine.Random.Range(-spread, spread));
    }
}
