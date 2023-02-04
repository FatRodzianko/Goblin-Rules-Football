using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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

    [Header("Lighting Related To Rain")]
    [SerializeField] Light2D _globalLight;

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

            _globalLight.intensity = 1.0f;
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

            _globalLight.intensity = 0.9f;

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

            _globalLight.intensity = 0.8f;
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
            
            _globalLight.intensity = 0.7f;
        }
        else
        {
            IsRaining = false;
            _rainGround.Stop();
            _rainBackgroundGround.Stop();
            RainBounceModifier = _noRainBounceModifier;
            RainHitModifier = _noRainHitModifier;

            _globalLight.intensity = 1.0f;
        }

        RainState = newState;
    }
    void WeatherChangedFunction(string newEffect)
    {
        //Debug.Log("WeatherChangedFunction: the new weather effect is: " + newEffect);
        SetRainState(newEffect);
    }
    public void SetInitialWeatherForHole()
    {
        // similar to wind stuff, this will probably be controlled by player settings in the full game. Right now, just randomly selecting it
        float weatherChance = UnityEngine.Random.Range(0f, 1.0f);

        if (weatherChance < 0.7f)
            RainState = "clear";
        else if (weatherChance < 0.85f)
            RainState = "light rain";
        else if (weatherChance < 0.95f)
            RainState = "med rain";
        else
            RainState = "heavy rain";

        Debug.Log("SetInitialWeatherForHole: Setting the rain state to: " + RainState + " from weather chance of: " + weatherChance.ToString());
    }
    public void UpdateWeatherForNewTurn()
    {
        // For these update functions for wind/rain, will probably have a game setting for players that's something like "Can Wind/Rain Change? Yes/No" to either allow this or not
        if (_rainState == "clear")
        {
            if (UnityEngine.Random.Range(0f, 1f) < 0.9f)
                return;
        }
        else
        {
            if (UnityEngine.Random.Range(0f, 1f) < 0.7f)
                return;
        }

        // Generate a random value of either -1 or 1 to determine if weather should get better or worse
        int negOrPos = UnityEngine.Random.Range(0, 2) * 2 - 1;
        if (negOrPos > 0)
        {
            Debug.Log("UpdateWeatherForNewTurn: Weather will get BETTER based on value of: " + negOrPos.ToString());
            if (_rainState == "clear") // can't get better if its already clear
                return;
            else if (_rainState == "light rain")
            {
                RainState = "clear";
                return; // returning after changing the rainstate in case the Update function is somehow called part way through and changes _rainState to what RainState was updated to during the middle of this, cascading downward or something
            }
            else if (_rainState == "med rain")
            {
                RainState = "light rain";
                return;
            }
            else if (_rainState == "heavy rain")
            {
                RainState = "med rain";
                return;
            }
                
        }
        else
        {
            Debug.Log("UpdateWeatherForNewTurn: Weather will get WORSE based on value of: " + negOrPos.ToString());
            if (_rainState == "heavy rain") // can't get worse if its heavy rain
                return;
            else if (_rainState == "light rain")
            {
                RainState = "med rain";
                return; // returning after changing the rainstate in case the Update function is somehow called part way through and changes _rainState to what RainState was updated to during the middle of this, cascading downward or something
            }
            else if (_rainState == "med rain")
            {
                RainState = "heavy rain";
                return;
            }
            else if (_rainState == "clear")
            {
                RainState = "light rain";
                return;
            }
        }

    }
}
