using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainManager : MonoBehaviour
{
    public static RainManager instance;

    [Header("Rain state")]
    public bool IsRaining = false;
    public string RainState; // "No Rain" "light rain" "Med Rain" "Heavy Rain"
    public List<string> RainStates = new List<string> { "clear", "light rain", "med rain", "heavy rain" };
    [SerializeField] private string _rainState;

    [Header("Rain Rates")]
    [SerializeField] int _rainGroundLow;
    [SerializeField] int _rainGroundMed;
    [SerializeField] int _rainGroundHeavy;
    [SerializeField] int _rainBackGroundLow;
    [SerializeField] int _rainBackGroundMed;
    [SerializeField] int _rainBackGroundHeavy;

    [Header("Particle Systems")]
    [SerializeField] ParticleSystem _rainGround;
    [SerializeField] ParticleSystem _rainBackgroundGround;

    [Header("Rain Bounce Modifiers")]
    [SerializeField] public float RainBounceModifier = 1.0f;
    [SerializeField] float _noRainBounceModifier = 1.0f;
    [SerializeField] float _lightRainBounceModifier = 0.8f;
    [SerializeField] float _medRainBounceModifier = 0.65f;
    [SerializeField] float _heavyRainBounceModifier = 0.25f;

    [Header("Rain Hit Modifiers")]
    [SerializeField] public float RainHitModifier = 1.0f;
    [SerializeField] float _noRainHitModifier = 1.0f;
    [SerializeField] float _lightRainHitModifier = 0.975f;
    [SerializeField] float _medRainHitModifier = 0.935f;
    [SerializeField] float _heavyRainHitModifier = 0.865f;

    public delegate void WeatherEffectChange(string newEffect);
    public event WeatherEffectChange WeatherChanged;

    private void Awake()
    {
        MakeInstance();
        WeatherChanged = WeatherChangedFunction;
    }
    // Start is called before the first frame update
    void Start()
    {
        SetRainState(RainState);
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        if (RainState != _rainState && WeatherChanged != null)
        {
            Debug.Log("RainManager: rain states no longer match. RainState: " + RainState.ToString() + " _rainState: " + _rainState.ToString());
            _rainState = RainState;
            WeatherChanged(_rainState);
        }
    }
    public void SetRainState(string newState)
    {
        
        if (string.IsNullOrWhiteSpace(newState))
            return;

        newState = newState.ToLower();

        if (!RainStates.Contains(newState))
            return;
        if (newState == "clear")
        {
            IsRaining = false;
            _rainGround.Stop();
            _rainBackgroundGround.Stop();
            RainBounceModifier = _noRainBounceModifier;
            RainHitModifier = _noRainHitModifier;
        }
        else if (newState == "light rain")
        {
            IsRaining = true;

            var groundEmitter = _rainGround.emission;
            groundEmitter.rateOverTime = _rainGroundLow;

            groundEmitter = _rainBackgroundGround.emission;
            groundEmitter.rateOverTime = _rainBackGroundLow;

            if (!_rainGround.isPlaying)
                _rainGround.Play();
            if (!_rainBackgroundGround.isPlaying)
                _rainBackgroundGround.Play();
            RainBounceModifier = _lightRainBounceModifier;
            RainHitModifier = _lightRainHitModifier;

        }
        else if (newState == "med rain")
        {
            IsRaining = true;

            var groundEmitter = _rainGround.emission;
            groundEmitter.rateOverTime = _rainGroundMed;

            groundEmitter = _rainBackgroundGround.emission;
            groundEmitter.rateOverTime = _rainBackGroundMed;

            if (!_rainGround.isPlaying)
                _rainGround.Play();
            if (!_rainBackgroundGround.isPlaying)
                _rainBackgroundGround.Play();

            RainBounceModifier = _medRainBounceModifier;
            RainHitModifier = _medRainHitModifier;
        }
        else if (newState == "heavy rain")
        {
            IsRaining = true;

            var groundEmitter = _rainGround.emission;
            groundEmitter.rateOverTime = _rainGroundHeavy;

            groundEmitter = _rainBackgroundGround.emission;
            groundEmitter.rateOverTime = _rainBackGroundHeavy;

            if (!_rainGround.isPlaying)
                _rainGround.Play();
            if (!_rainBackgroundGround.isPlaying)
                _rainBackgroundGround.Play();

            RainBounceModifier = _heavyRainBounceModifier;
            RainHitModifier = _heavyRainHitModifier;
        }
        else
        {
            IsRaining = false;
            _rainGround.Stop();
            _rainBackgroundGround.Stop();
            RainBounceModifier = _noRainBounceModifier;
            RainHitModifier = _noRainHitModifier;
        }

        RainState = newState;
    }
    void WeatherChangedFunction(string newEffect)
    {
        Debug.Log("WeatherChangedFunction: the new weather effect is: " + newEffect);
        SetRainState(newEffect);
    }
}
