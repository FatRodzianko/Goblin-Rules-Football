using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class RainManager : NetworkBehaviour
{
    public static RainManager instance;

    [Header("Rain state")]
    public bool IsRaining = false;
    [SyncVar] public string RainState; // player's rain state. Based on the BaseRainState value and the player's weather favor
    public List<string> RainStates = new List<string> { "clear", "light rain", "med rain", "heavy rain" };
    [SerializeField] private string _rainState;
    [SyncVar] public string BaseRainState; // rain state that player's favor modifies. ex: base state is light rain, but player has +3 good weather favor, so there's a chance they have clear weather for their turn. Next player has -5 favor, so they get med rain for their turn

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

    [Header("Weather SFX")]
    [SerializeField] string[] _clearSounds;
    [SerializeField] string _clearSoundForHole;
    [SerializeField] bool _clearSoundPlaying = false;
    [SerializeField] string[] _rainSounds;
    [SerializeField] string _rainSoundForHole;
    [SerializeField] bool _rainSoundPlaying = false;

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
        //GetRainSoundForHole();
        //SetRainState(RainState);
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        SetRainState(RainState);
        BaseRainState = "clear";
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        GetRainSoundForHole();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    public void GetRainSoundForHole()
    {
        var rng = new System.Random();
        if(!string.IsNullOrWhiteSpace(_rainSoundForHole))
            SoundManager.instance.StopSound(_rainSoundForHole);
        if (!string.IsNullOrWhiteSpace(_clearSoundForHole))
            SoundManager.instance.StopSound(_clearSoundForHole);
        _rainSoundForHole = _rainSounds[rng.Next(_rainSounds.Length)];
        _clearSoundForHole = _clearSounds[rng.Next(_clearSounds.Length)];

        PlaySoundBasedOnRainState(RainState);

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

            PlaySoundBasedOnRainState(newState);

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

            PlaySoundBasedOnRainState(newState);

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

            PlaySoundBasedOnRainState(newState);

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

            PlaySoundBasedOnRainState(newState);

            _globalLight.intensity = 0.7f;
        }
        else
        {
            IsRaining = false;
            _rainGround.Stop();
            _rainBackgroundGround.Stop();
            RainBounceModifier = _noRainBounceModifier;
            RainHitModifier = _noRainHitModifier;

            SoundManager.instance.StopSound(_clearSoundForHole);
            var rng = new System.Random();
            _clearSoundForHole = _clearSounds[rng.Next(_clearSounds.Length)];
            SoundManager.instance.StopSound(_rainSoundForHole);
            SoundManager.instance.PlaySound(_clearSoundForHole, 1.0f);

            _globalLight.intensity = 1.0f;
        }

        if(this.IsServer)
            RainState = newState;
    }
    void WeatherChangedFunction(string newEffect)
    {
        Debug.Log("WeatherChangedFunction: the new weather effect is: " + newEffect);
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
    [Server]
    public void UpdateWeatherForNewTurn(GolfPlayerTopDown currentPlayer)
    {
        // Get the BaseRainState for the game before calculating what the rain state is for the player
        SetBaseRainState();

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
    [Server]
    void SetBaseRainState()
    {
        Debug.Log("SetBaseRainState");
        string baseState = BaseRainState;

        // Calculate the average weather favor of all players
        float averagePlayerFavor = GetAveragePlayerFavor();
        float weatherChangeChanceFloor = 0f;
        float weatherChangeChanceTotal = 0f;

        float changeChance = UnityEngine.Random.Range(0f, 1f);

        if (averagePlayerFavor > 0)
        {
            Debug.Log("SetBaseRainState: Average player favor is positive. BaseRainState MAY get BETTER.");
            if (averagePlayerFavor >= 10)
            {
                Debug.Log("SetBaseRainState: Average player favor is PERFECT. Rain state will be set to clear no matter what!");
                BaseRainState = "clear";
                return;
            }

            // Based on the current BaseRainState, set the weather chance floor that will be used to see if the weather changes?
            if (BaseRainState == "heavy rain")
                weatherChangeChanceFloor = 0.5f;
            else if (BaseRainState == "med rain")
                weatherChangeChanceFloor = 0.7f;
            else if (BaseRainState == "light rain")
                weatherChangeChanceFloor = 0.85f;

            weatherChangeChanceTotal = weatherChangeChanceFloor - (averagePlayerFavor / 10);
            Debug.Log("SetBaseRainState: change chance is: " + changeChance.ToString() + " weatherChangeChanceTotal is: " + weatherChangeChanceTotal.ToString());
            if (changeChance <= 0.01f || changeChance >= 0.99f)
            {
                Debug.Log("SetBaseRainState: The change chaces was either almost exactly zero or almost exactly 1. Making it WORSE because players have positive favor. Owned!");
                MakeRainWorse();
            }            
            else if (changeChance > weatherChangeChanceTotal)
            {
                MakeRainBetter();
            }

        }
        else if (averagePlayerFavor < 0)
        {
            Debug.Log("SetBaseRainState: Average player favor is negative. BaseRainState MAY get WORSE.");
            if (averagePlayerFavor <= -10)
            {
                Debug.Log("SetBaseRainState: Average player favor is THE WORST IT CAN GET. Rain state will be set to HEAVY RAIN no matter what!");
                BaseRainState = "heavy rain";
                return;
            }

            // Based on the current BaseRainState, set the weather chance floor that will be used to see if the weather changes?
            if (BaseRainState == "clear")
                weatherChangeChanceFloor = 0.85f;
            else if (BaseRainState == "light rain")
                weatherChangeChanceFloor = 0.8f;
            else if (BaseRainState == "med rain")
                weatherChangeChanceFloor = 0.85f;

            weatherChangeChanceTotal = weatherChangeChanceFloor - (Mathf.Abs(averagePlayerFavor) / 10);
            Debug.Log("SetBaseRainState: change chance is: " + changeChance.ToString() + " weatherChangeChanceTotal is: " + weatherChangeChanceTotal.ToString());
            if (changeChance <= 0.01f || changeChance >= 0.99f)
            {
                Debug.Log("SetBaseRainState: The change chaces was either almost exactly zero or almost exactly 1. Making it BETTER because players have negative favor. Lucky!!");
                MakeRainBetter();
            }            
            else if (changeChance > weatherChangeChanceTotal)
            {
                MakeRainWorse();
            }

        }
        else
        {
            Debug.Log("SetBaseRainState: Average player favor is 0. BaseRainState SHOULD stay the same (random chance to get better or worsE?)");
            if (changeChance >= 0.95f)
            {
                Debug.Log("SetBaseRainState: change chance was " + changeChance.ToString() + " unfortunately weather will get worse!");
                MakeRainWorse();
            }
            else if (changeChance <= 0.05f)
            {
                Debug.Log("SetBaseRainState: change chance was " + changeChance.ToString() + " fortuantely weather will get better!!");
                MakeRainBetter();
            }
        }

    }
    [Server]
    void SetPlayerRainState()
    { 

    }
    float GetAveragePlayerFavor()
    {
        float averageFavor = 0f;

        if (GameplayManagerTopDownGolf.instance.GolfPlayersServer.Count <= 0)
            return averageFavor;

        float totalFavor = 0f;
        foreach (GolfPlayerTopDown player in GameplayManagerTopDownGolf.instance.GolfPlayersServer)
        {
            totalFavor += player.FavorWeather;
        }
        averageFavor = totalFavor / GameplayManagerTopDownGolf.instance.GolfPlayersServer.Count;
        Debug.Log("GetAveragePlayerFavor: Average player favor is: " + averageFavor.ToString() + " based on total favor of: " + totalFavor.ToString() + " and " + GameplayManagerTopDownGolf.instance.GolfPlayersServer.Count.ToString() + " number of players.");

        return averageFavor;
    }
    void MakeRainBetter()
    {
        Debug.Log("MakeRainBetter");
        string baseState = BaseRainState;
        if (baseState == "clear") // can't get better if its already clear
            return;
        else if (baseState == "light rain")
        {
            BaseRainState = "clear";
            return; // returning after changing the BaseRainState in case the Update function is somehow called part way through and changes baseState to what BaseRainState was updated to during the middle of this, cascading downward or something
        }
        else if (baseState == "med rain")
        {
            BaseRainState = "light rain";
            return;
        }
        else if (baseState == "heavy rain")
        {
            BaseRainState = "med rain";
            return;
        }
    }
    void MakeRainWorse()
    {
        Debug.Log("MakeRainWorse");
        string baseState = BaseRainState;
        if (baseState == "heavy rain") // can't get worse if its heavy rain
            return;
        else if (baseState == "light rain")
        {
            BaseRainState = "med rain";
            return; // returning after changing the BaseRainState in case the Update function is somehow called part way through and changes baseState to what BaseRainState was updated to during the middle of this, cascading downward or something
        }
        else if (baseState == "med rain")
        {
            BaseRainState = "heavy rain";
            return;
        }
        else if (baseState == "clear")
        {
            BaseRainState = "light rain";
            return;
        }
    }
    void PlaySoundBasedOnRainState(string state)
    {
        if (!RainStates.Contains(state))
            return;
        if (state == "clear")
        {

            SoundManager.instance.StopSound(_clearSoundForHole);
            var rng = new System.Random();
            _clearSoundForHole = _clearSounds[rng.Next(_clearSounds.Length)];
            SoundManager.instance.StopSound(_rainSoundForHole);
            SoundManager.instance.PlaySound(_clearSoundForHole, 1.0f);

        }
        else if (state == "light rain")
        {

            SoundManager.instance.StopSound(_clearSoundForHole);
            SoundManager.instance.PlaySound(_rainSoundForHole, 0.75f);


        }
        else if (state == "med rain")
        {
            SoundManager.instance.StopSound(_clearSoundForHole);
            SoundManager.instance.PlaySound(_rainSoundForHole, 0.95f);

        }
        else if (state == "heavy rain")
        {


            SoundManager.instance.StopSound(_clearSoundForHole);
            SoundManager.instance.PlaySound(_rainSoundForHole, 1.15f);

        }
        else
        {
            SoundManager.instance.StopSound(_clearSoundForHole);
            var rng = new System.Random();
            _clearSoundForHole = _clearSounds[rng.Next(_clearSounds.Length)];
            SoundManager.instance.StopSound(_rainSoundForHole);
            SoundManager.instance.PlaySound(_clearSoundForHole, 1.0f);

        }
    }
    void PlayClearSound()
    { 

    }
}
