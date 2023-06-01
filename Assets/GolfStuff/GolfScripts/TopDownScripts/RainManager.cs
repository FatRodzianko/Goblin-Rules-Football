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
    private string _baseRainState;

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

    [Header("BaseRainState Values")]
    [SerializeField] float _clearStateModifer = 1;
    [SerializeField] float _lightRainStateModifer = -1;
    [SerializeField] float _medRainStateModifer = -2;
    [SerializeField] float _heavyRainStateModifer = -3;

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

    public delegate void BaseWeatherEffectChange(string newEffect);
    public event BaseWeatherEffectChange BaseWeatherChanged;

    private void Awake()
    {
        MakeInstance();
        
        WeatherChanged = WeatherChangedFunction;
        BaseWeatherChanged = BaseChangedFunction;
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
        _baseRainState = "";
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
            //Debug.Log("RainManager: rain states no longer match. RainState: " + RainState.ToString() + " _rainState: " + _rainState.ToString());
            _rainState = RainState;
            WeatherChanged(_rainState);
        }
        if (BaseRainState != _baseRainState && BaseWeatherChanged != null)
        {
            _baseRainState = BaseRainState;
            BaseWeatherChanged(_baseRainState);
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
    void BaseChangedFunction(string newEffect)
    {
        Debug.Log("BaseChangedFunction: the new weather effect is: " + newEffect);
    }
    public void SetInitialWeatherForHole()
    {
        // similar to wind stuff, this will probably be controlled by player settings in the full game. Right now, just randomly selecting it
        float weatherChance = UnityEngine.Random.Range(0f, 1.0f);

        // previous way before weather favor
        //if (weatherChance < 0.7f)
        //    RainState = "clear";
        //else if (weatherChance < 0.85f)
        //    RainState = "light rain";
        //else if (weatherChance < 0.95f)
        //    RainState = "med rain";
        //else
        //    RainState = "heavy rain";

        // Begin new way with weather favor. Always start the game with "clear" weather. No one wants to golf on a rainy day? Maybe later change so it's like wind where the base state is relative to the "initial" weather state that's set randomly?
        RainState = "clear";
        BaseRainState = RainState; // make sure the base rain state matches the initial rain state?
        
        Debug.Log("SetInitialWeatherForHole: Setting the rain state to: " + RainState + " from weather chance of: " + weatherChance.ToString());
    }
    [Server]
    public void UpdateWeatherForNewTurn(GolfPlayerTopDown currentPlayer)
    {
        // Get the BaseRainState for the game before calculating what the rain state is for the player
        SetBaseRainState();
        SetPlayerRainState(currentPlayer);

        return;

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
        //float averagePlayerFavor = GetAveragePlayerFavor();
        float averagePlayerFavor = GameplayManagerTopDownGolf.instance.AveragePlayerWeatherFavor;
        // Get the modifier value for the current BaseRainState
        float rainStateModifier = GetRainStateModifier();
        float combinedFavorAndModifier = averagePlayerFavor + rainStateModifier;


        if (averagePlayerFavor >= _lightRainStateModifer)
        {
            BaseRainState = "clear";
            Debug.Log("SetBaseRainState: BaseState will be: " + BaseRainState + " averagePlayerFavor: " + averagePlayerFavor.ToString());
        }
        else if (averagePlayerFavor >= _medRainStateModifer)
        {
            BaseRainState = "light rain";
            Debug.Log("SetBaseRainState: BaseState will be: " + BaseRainState + " averagePlayerFavor: " + averagePlayerFavor.ToString());
        }
        else if (averagePlayerFavor >= _heavyRainStateModifer)
        {
            BaseRainState = "med rain";
            Debug.Log("SetBaseRainState: BaseState will be: " + BaseRainState + " averagePlayerFavor: " + averagePlayerFavor.ToString());
        }
        else
        {
            BaseRainState = "heavy rain";
            Debug.Log("SetBaseRainState: BaseState will be: " + BaseRainState + " averagePlayerFavor: " + averagePlayerFavor.ToString());
        }

        return;

        //// Set the BaseRainState based on the combinedFavorAndModifier
        //if (combinedFavorAndModifier > 0f)
        //{
        //    BaseRainState = "clear";
        //    Debug.Log("SetBaseRainState: BaseState will be: " + BaseRainState + "rainStateModifier: " + rainStateModifier.ToString() + " averagePlayerFavor: " + averagePlayerFavor.ToString() + " combinedFavorAndModifier: " + combinedFavorAndModifier.ToString());
        //}   
        //else if (combinedFavorAndModifier > (_medRainStateModifer -2))
        //{
        //    BaseRainState = "light rain";
        //    Debug.Log("SetBaseRainState: BaseState will be: " + BaseRainState + "rainStateModifier: " + rainStateModifier.ToString() + " averagePlayerFavor: " + averagePlayerFavor.ToString() + " combinedFavorAndModifier: " + combinedFavorAndModifier.ToString());
        //}
        //else if (combinedFavorAndModifier > (_heavyRainStateModifer -3))
        //{
        //    BaseRainState = "med rain";
        //    Debug.Log("SetBaseRainState: BaseState will be: " + BaseRainState + "rainStateModifier: " + rainStateModifier.ToString() + " averagePlayerFavor: " + averagePlayerFavor.ToString() + " combinedFavorAndModifier: " + combinedFavorAndModifier.ToString());
        //}
        //else
        //{
        //    BaseRainState = "heavy rain";
        //    Debug.Log("SetBaseRainState: BaseState will be: " + BaseRainState + "rainStateModifier: " + rainStateModifier.ToString() + " averagePlayerFavor: " + averagePlayerFavor.ToString() + " combinedFavorAndModifier: " + combinedFavorAndModifier.ToString());
        //}

    }
    [Server]
    void SetPlayerRainState(GolfPlayerTopDown currentPlayer)
    {
        Debug.Log("SetPlayerRainState: for player: " + currentPlayer.PlayerName + " who has a player favor of: " + currentPlayer.FavorWeather.ToString());
        float rainStateModifier = GetRainStateModifier();
        float playerFavor = currentPlayer.FavorWeather;

        // if player favor is at the extremes, automatically set it to highest/lowest rain values
        if (playerFavor >= 10)
        {
            RainState = "clear";
            return;
        }
        else if (playerFavor <= -10)
        {
            RainState = "heavy rain";
            return;
        }

        if (playerFavor > 0) // if the player has positive favor, add their favor to the base state modifier. If it crosses another state modifier above the current state, move their weather up?
        {
            if (BaseRainState == "clear") // can't go higher than clear weather, no need to calculate further
            {
                RainState = "clear";
                Debug.Log("SetPlayerRainState: Player RainState will be: " + RainState + " because player has positive favor and base state is clear");
                return;
            }

            float combinedFavorAndModifier = playerFavor + rainStateModifier;
            if (combinedFavorAndModifier >= _clearStateModifer)
            {
                RainState = "clear";
                Debug.Log("SetPlayerRainState: Player RainState will be: " + RainState + " player favor is: " + playerFavor.ToString() + " combined favor is: " + combinedFavorAndModifier.ToString() + " and the rain state modifier is: " + rainStateModifier.ToString());
            }
            else if (combinedFavorAndModifier >= _lightRainStateModifer)
            {
                RainState = "light rain";
                Debug.Log("SetPlayerRainState: Player RainState will be: " + RainState + " player favor is: " + playerFavor.ToString() + " combined favor is: " + combinedFavorAndModifier.ToString() + " and the rain state modifier is: " + rainStateModifier.ToString());
            }
            else if (combinedFavorAndModifier >= _medRainStateModifer)
            {
                RainState = "med rain";
                Debug.Log("SetPlayerRainState: Player RainState will be: " + RainState + " player favor is: " + playerFavor.ToString() + " combined favor is: " + combinedFavorAndModifier.ToString() + " and the rain state modifier is: " + rainStateModifier.ToString());
            }
            else if (combinedFavorAndModifier < _medRainStateModifer)
            {
                RainState = "heavy rain";
                Debug.Log("SetPlayerRainState: Player RainState will be: " + RainState + " player favor is: " + playerFavor.ToString() + " combined favor is: " + combinedFavorAndModifier.ToString() + " and the rain state modifier is: " + rainStateModifier.ToString());
            }
            else // fail condition?
            {
                RainState = BaseRainState;
                Debug.Log("SetPlayerRainState: Player RainState will be: " + RainState + " player favor is: " + playerFavor.ToString() + " combined favor is: " + combinedFavorAndModifier.ToString() + " and the rain state modifier is: " + rainStateModifier.ToString() + " hit fail condition?");
            }

        }
        else if (playerFavor < 0) // if player has negative favor, calculate distance from the base rain state modifer. If it is a certain distance away, move the player down one rain state?
        {
            if (BaseRainState == "heavy rain") // can't go lower than heavy rain, no need to calculate further
            {
                RainState = "heavy rain";
                Debug.Log("SetPlayerRainState: Player RainState will be: " + RainState + " because player has negative favor and base state is heavy rain");
                return;
            }

            if (BaseRainState == "clear" && playerFavor < -1)
            {
                RainState = "light rain";
                Debug.Log("SetPlayerRainState: Player RainState will be: " + RainState + " because player has negative favor and base state is clear");
            }
            if (playerFavor < (rainStateModifier - 5))
            {
                MakePlayerRainStateWorseTwice();
            }
            else if (playerFavor < (rainStateModifier - 2))
            {
                MakePlayerRainStateWorse();
            }
            else
            {
                RainState = BaseRainState;
            }

        }
        else // if player has 0 favor, the rain state stays the same as base rain state
        {
            RainState = BaseRainState;
        }

        // old way
        //float rainStateModifier = GetRainStateModifier();
        //float combinedFavorAndModifier = currentPlayer.FavorWeather + rainStateModifier;
        //if (combinedFavorAndModifier > 0f)
        //{
        //    RainState = "clear";
        //    Debug.Log("SetPlayerRainState: Player RainState will be: " + RainState + "rainStateModifier: " + rainStateModifier.ToString() + " averagePlayerFavor: " + currentPlayer.FavorWeather.ToString() + " combinedFavorAndModifier: " + combinedFavorAndModifier.ToString());
        //}
        //else if (combinedFavorAndModifier > (_medRainStateModifer - 2))
        //{
        //    RainState = "light rain";
        //    Debug.Log("SetPlayerRainState: Player RainState will be: " + RainState + "rainStateModifier: " + rainStateModifier.ToString() + " averagePlayerFavor: " + currentPlayer.FavorWeather.ToString() + " combinedFavorAndModifier: " + combinedFavorAndModifier.ToString());
        //}
        //else if (combinedFavorAndModifier > (_heavyRainStateModifer - 3))
        //{
        //    RainState = "med rain";
        //    Debug.Log("SetPlayerRainState: Player RainState will be: " + RainState + "rainStateModifier: " + rainStateModifier.ToString() + " averagePlayerFavor: " + currentPlayer.FavorWeather.ToString() + " combinedFavorAndModifier: " + combinedFavorAndModifier.ToString());
        //}
        //else
        //{
        //    RainState = "heavy rain";
        //    Debug.Log("SetPlayerRainState: Player RainState will be: " + RainState + "rainStateModifier: " + rainStateModifier.ToString() + " averagePlayerFavor: " + currentPlayer.FavorWeather.ToString() + " combinedFavorAndModifier: " + combinedFavorAndModifier.ToString());
        //}
    }
    float GetRainStateModifier()
    {
        float modifier = 1f;
        if (BaseRainState == "clear")
            modifier = _clearStateModifer;
        else if (BaseRainState == "light rain")
            modifier = _lightRainStateModifer;
        else if (BaseRainState == "med rain")
            modifier = _medRainStateModifer;
        else if (BaseRainState == "heavy rain")
            modifier = _heavyRainStateModifer;

        return modifier;
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
    void MakePlayerRainStateWorse()
    {
        string baseState = BaseRainState;
        if (baseState == "heavy rain") // can't get worse if its heavy rain
            return;
        else if (baseState == "light rain")
        {
            RainState = "med rain";
            return; // returning after changing the BaseRainState in case the Update function is somehow called part way through and changes baseState to what BaseRainState was updated to during the middle of this, cascading downward or something
        }
        else if (baseState == "med rain")
        {
            RainState = "heavy rain";
            return;
        }
        else if (baseState == "clear")
        {
            RainState = "light rain";
            return;
        }
    }
    void MakePlayerRainStateWorseTwice()
    {
        string baseState = BaseRainState;
        if (baseState == "heavy rain") // can't get worse if its heavy rain
            return;
        else if (baseState == "light rain" || baseState == "med rain")
        {
            RainState = "heavy rain";
            return; // returning after changing the BaseRainState in case the Update function is somehow called part way through and changes baseState to what BaseRainState was updated to during the middle of this, cascading downward or something
        }
        else if (baseState == "clear")
        {
            RainState = "med rain";
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
