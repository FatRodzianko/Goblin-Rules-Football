using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Decides when the title screen's lights flicker. Sits on the object the lights live under and
/// finds them itself, so adding or removing a light needs no wiring.
///
/// The screen opens dark, then the banks come up one after another the way stadium circuits do,
/// each lamp struggling on its own. After that one light twitches at random now and then, so the
/// screen is never completely still. The lights own the flicker itself.
/// </summary>
public class TitleScreenLights : MonoBehaviour
{
    [Tooltip("Where the lights are. Left empty, this object's own children are used.")]
    [SerializeField] private Transform _lightsRoot;

    [Tooltip("Bring the lights up when the screen opens.")]
    [SerializeField] private bool _flickerOnAtStart = true;

    [Tooltip("Seconds of darkness before the first bank catches, so the screen is not already lit when it appears.")]
    [SerializeField] private float _openingDelay = 1.5f;

    [Tooltip("Seconds between one bank of lights starting and the next. Zero brings them all up together.")]
    [SerializeField] private float _bankStagger = 0.5f;

    [Tooltip("Arcing wiring, played just before the first bank catches. Optional.")]
    [SerializeField] private SparkBurst _openingSparks;

    [Tooltip("Seconds the sparks get to themselves before the lights start.")]
    [SerializeField] private float _sparksLeadTime = 0.6f;

    [Tooltip("Fires as the first bank starts to catch. Wire the title scroll here, and a sound later.")]
    [SerializeField] private UnityEvent _openingStrikeBegan;

    [Tooltip("Fires instead, on a later visit, when the screen comes up already lit. Wire whatever the strike would have switched on.")]
    [SerializeField] private UnityEvent _openingSkipped;

    [Tooltip("Seconds between idle flickers. One light twitches each time.")]
    [SerializeField] private Vector2 _idleIntervalRange = new Vector2(30f, 120f);

    private readonly List<TitleScreenLight> _lights = new List<TitleScreenLight>();
    private readonly List<List<TitleScreenLight>> _banks = new List<List<TitleScreenLight>>();
    private bool _opened;
    private float _nextIdleFlicker;

    // The power coming on is something that happens to the stadium once, not every time the player
    // walks back into the menu. Static, so it outlives the scene but not the process.
    private static bool _powerIsOn;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnBoot()
    {
        // Statics survive entering play mode when domain reload is turned off, which would mean the
        // opening played once and never again for the rest of the editor session.
        _powerIsOn = false;
    }

    /// <summary>Whether the opening has already run this time the game was started.</summary>
    public static bool PowerIsOn { get { return _powerIsOn; } }

    /// <summary>Seconds of darkness before the sparks fly and the first bank catches.</summary>
    public float OpeningDelay { get { return _openingDelay; } }

    public int LightCount { get { return _lights.Count; } }

    public int BankCount { get { return _banks.Count; } }

    private void Awake()
    {
        Transform root = _lightsRoot != null ? _lightsRoot : transform;
        root.GetComponentsInChildren(true, _lights);

        // Each immediate child is a bank: the flood lights, the wall lights, and so on. A light
        // sitting directly under the root is a bank of one.
        foreach (Transform child in root)
        {
            List<TitleScreenLight> bank = new List<TitleScreenLight>();
            child.GetComponentsInChildren(true, bank);
            if (bank.Count > 0)
                _banks.Add(bank);
        }
    }

    private void Start()
    {
        // Every light set itself dark in its own Awake. Nothing lights up until the opening runs.
        ForceAllDark();

        if (_flickerOnAtStart && !_powerIsOn)
        {
            _powerIsOn = true;
            StartCoroutine(RunOpening());
        }
        else
        {
            // Coming back from a match: the stadium has been lit all along.
            ForceAllLit();
            if (_openingSkipped != null)
                _openingSkipped.Invoke();
            _opened = true;
            ScheduleNextIdleFlicker();
        }
    }

    private void Update()
    {
        if (!_opened || _lights.Count == 0 || Time.time < _nextIdleFlicker)
            return;
        ScheduleNextIdleFlicker();
        FlickerRandomLight();
    }

    /// <summary>Stutters every light up, all at once.</summary>
    public void FlickerAllOn()
    {
        for (int i = 0; i < _lights.Count; i++)
            _lights[i].FlickerOn();
    }

    /// <summary>Stutters every light down, all at once.</summary>
    public void FlickerAllOff()
    {
        for (int i = 0; i < _lights.Count; i++)
            _lights[i].FlickerOff();
    }

    /// <summary>Puts every light out with no flicker. The state the screen opens from.</summary>
    public void ForceAllDark()
    {
        for (int i = 0; i < _lights.Count; i++)
            _lights[i].SetLit(false);
    }

    /// <summary>Lights everything with no flicker, for a screen the player has already seen come on.</summary>
    public void ForceAllLit()
    {
        for (int i = 0; i < _lights.Count; i++)
            _lights[i].SetLit(true);
    }

    /// <summary>Twitches one light, which settles back to the state it was in.</summary>
    public void FlickerRandomLight()
    {
        TitleScreenLight light = PickIdleLight();
        if (light != null)
            light.Flicker();
    }

    private IEnumerator RunOpening()
    {
        yield return new WaitForSeconds(_openingDelay);

        // The wiring arcs first, then the power takes and the banks start catching.
        if (_openingSparks != null)
        {
            _openingSparks.Play();
            if (_sparksLeadTime > 0f)
                yield return new WaitForSeconds(_sparksLeadTime);
        }

        if (_openingStrikeBegan != null)
            _openingStrikeBegan.Invoke();

        for (int i = 0; i < _banks.Count; i++)
        {
            List<TitleScreenLight> bank = _banks[i];
            for (int j = 0; j < bank.Count; j++)
                bank[j].FlickerOn();

            if (_bankStagger > 0f && i < _banks.Count - 1)
                yield return new WaitForSeconds(_bankStagger);
        }

        _opened = true;
        ScheduleNextIdleFlicker();
    }

    // A few tries rather than a full shuffle: with one light flickering at a time, a busy pick is
    // rare, and skipping a turn costs nothing.
    private TitleScreenLight PickIdleLight()
    {
        const int attempts = 5;
        for (int i = 0; i < attempts; i++)
        {
            TitleScreenLight candidate = _lights[Random.Range(0, _lights.Count)];
            if (candidate != null && !candidate.IsFlickering)
                return candidate;
        }
        return null;
    }

    private void ScheduleNextIdleFlicker()
    {
        _nextIdleFlicker = Time.time + Random.Range(_idleIntervalRange.x, _idleIntervalRange.y);
    }
}
