using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class LightningManager : NetworkBehaviour
{
    [SerializeField] Light2D _lightningLight;
    [SerializeField] bool _lightningFlashing = false;
    IEnumerator _lightningRoutine;

    [SyncVar(OnChange = nameof(SyncIsThereLightning))] public bool IsThereLightning = false;

    [Header("Lightning Cooldown Times")]
    [SerializeField] float _lightRainMin = 20f;
    [SerializeField] float _lightRainMax = 45f;
    [SerializeField] float _medRainMin = 10f;
    [SerializeField] float _medRainMax = 30f;
    [SerializeField] float _heavyRainMin = 5f;
    [SerializeField] float _heavyRainMax = 15f;
    [SerializeField] float _cooldownMin;
    [SerializeField] float _cooldownMax;

    [Header("Lightning Odds and Stuff")]
    [SerializeField] bool _lightningStormThisTurnAlready = false;
    [SerializeField] float _startLightningOdds;
    [SerializeField] float _lightRainOddsToStart = 0.9f;
    [SerializeField] float _medRainOddsToStart = 0.75f;
    [SerializeField] float _heavyRainOddsToStart = 0.5f;
    [SerializeField] float _stopLightningOdds;
    [SerializeField] float _lightRainOddsToStop = 0.7f;
    [SerializeField] float _medRainOddsToStop = 0.85f;
    [SerializeField] float _heavyRainOddsToStop = 0.98f;

    [Header("Lightning/Storm Distance and stuff")]
    public float DistanceFromPlayer; // in miles? 5 second sound delay for thunder for each mile away from player
    [SerializeField] float _lightIntensity;
    [SerializeField] float _maxDistanceFromPlayer = 1.5f;
    [SerializeField] float _minLightIntensity = 0.25f;
    [SerializeField] float _maxLightIntensity = 0.75f;
    public float LightIntensityMax = 1.5f;

    [Header("Movement Ranges")]
    [SerializeField] float _minLightningMove;
    [SerializeField] float _maxLightningMove;
    [SerializeField] float _minLightRainLightningMove = 0.05f;
    [SerializeField] float _maxLightRainLightningMove = 0.15f;
    [SerializeField] float _minMedRainLightningMove = 0.1f;
    [SerializeField] float _maxMedRainLightningMove = 0.2f;
    [SerializeField] float _minHeavyRainLightningMove = 0.15f;
    [SerializeField] float _maxHeavyRainLightningMove = 0.3f;

    [Header("Player Struck By Lightning Stuff")]
    [SerializeField] bool _wasPlayerStruck = false;
    [SerializeField] int _playerThatWasStruckNetId = -99;
    [SerializeField] GolfPlayerTopDown _playerThatWasStruck;

    [Header("Thunder Sound Clip Names")]
    [SerializeField] string _thunderPlayerStruck;
    [SerializeField] string _thunderNear;
    [SerializeField] string _thunderMed;
    [SerializeField] string _thunderFarAway;
    [SerializeField] GameObject _thunderSubtitle;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(StartLightning());
        //RainManager.instance.WeatherChanged += UpdateWeather;
        RainManager.instance.BaseWeatherChanged += UpdateWeather;
        _thunderSubtitle.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void UpdateWeather(string newWeather)
    {
        bool isRain = true;
        if (newWeather.Equals("light rain"))
        {
            _cooldownMin = _lightRainMin;
            _cooldownMax = _lightRainMax;
            _startLightningOdds = _lightRainOddsToStart;
            _stopLightningOdds = _lightRainOddsToStop;
            _minLightningMove = _minLightRainLightningMove;
            _maxLightningMove = _maxLightRainLightningMove;
        }
        else if (newWeather.Equals("med rain"))
        {
            _cooldownMin = _medRainMin;
            _cooldownMax = _medRainMax;
            _startLightningOdds = _medRainOddsToStart;
            _stopLightningOdds = _medRainOddsToStop;
            _minLightningMove = _minMedRainLightningMove;
            _maxLightningMove = _maxMedRainLightningMove;

        }
        else if (newWeather.Equals("heavy rain"))
        {
            _cooldownMin = _heavyRainMin;
            _cooldownMax = _heavyRainMax;
            _startLightningOdds = _heavyRainOddsToStart;
            _stopLightningOdds = _heavyRainOddsToStop;
            _minLightningMove = _minHeavyRainLightningMove;
            _maxLightningMove = _maxHeavyRainLightningMove;
        }
        else
        {
            isRain = false;
            _startLightningOdds = 1.0f;
            _stopLightningOdds = 0f;
            _minLightningMove = 0f;
            _maxLightningMove = 0f;
        }
            

        // If the weath has rain, start the lightning routine. If no rain, end the lightning routine
        if (isRain)
        {
            Debug.Log("LightningManager: UpdateWeather: is there rain?: " + isRain.ToString());
        }
        else
        {
            Debug.Log("LightningManager: UpdateWeather: is there rain?: " + isRain.ToString());
            StopLightningRoutineStuff();
            IsThereLightning = false;

        }
    }
    IEnumerator LightningFlash(int numberOfFlashes)
    {
        Debug.Log("LightningFlash: " + numberOfFlashes.ToString());
        int totalFlashes = 0;
        _lightningFlashing = true;
        while (totalFlashes < numberOfFlashes)
        {
            // lightning flash
            float flashDelay = UnityEngine.Random.Range(0.15f, 0.25f);
            _lightningLight.intensity = _lightIntensity;
            _lightningLight.enabled = true;
            if (_wasPlayerStruck)
            {
                Debug.Log("LightningFlash: _wasPlayerStruck: " + _wasPlayerStruck.ToString() + " lightning flash on for player: " + _playerThatWasStruck.PlayerName);
                _playerThatWasStruck.LightningFlashForPlayerStruck(true);
            }
            yield return new WaitForSeconds(flashDelay);
            if (_wasPlayerStruck)
            {
                Debug.Log("LightningFlash:  _wasPlayerStruck: " + _wasPlayerStruck.ToString() + " lightning flash off for player: " + _playerThatWasStruck.PlayerName);
                _playerThatWasStruck.LightningFlashForPlayerStruck(false);
                if (totalFlashes == numberOfFlashes - 1)
                {
                    Debug.Log("LightningFlash:  _wasPlayerStruck: " + _wasPlayerStruck.ToString() + " lightning flashes over for player: " + _playerThatWasStruck.PlayerName);
                    _playerThatWasStruck.StruckByLightningOver();
                }
            }
            _lightningLight.enabled = false;

            // cool down before next flash
            float lightningStrikeCooldown = UnityEngine.Random.Range(0.15f, 0.4f);
            yield return new WaitForSeconds(lightningStrikeCooldown);
            totalFlashes++;
        }
        _lightningFlashing = false;
        StopLightningRoutineStuff();
    }
    IEnumerator ThunderClap(float dist, int numberOfFlashes)
    {
        float soundDelay = Mathf.Abs(dist) * 5f + 0.01f;
        Debug.Log("ThunderClap: delay will be: " + soundDelay.ToString() + " Time: " + Time.time);
        string thunderClip = "";
        if (_wasPlayerStruck)
        {
            thunderClip = _thunderPlayerStruck;
            soundDelay = 0f;
        }
        else if (dist <= 0.5f)
        {
            thunderClip = _thunderNear;
        }
        else if (dist <= 1.0f)
        {
            thunderClip = _thunderMed;
        }
        else
            thunderClip = _thunderFarAway;
        yield return new WaitForSeconds(soundDelay);
        Debug.Log("ThunderClap: Thunder will play now. Time: " + Time.time);
        // Play thunder sound
        if (!string.IsNullOrWhiteSpace(thunderClip))
        {
            SoundManager.instance.PlaySound(thunderClip, 1f);
            _thunderSubtitle.SetActive(true);
            yield return new WaitForSeconds(2f);
        }
        _thunderSubtitle.SetActive(false);
    }
    void ThunderSubtitle()
    { 

    }
    public void CheckIfLightningStartsThisTurn(bool skipLightningCheck = false, bool forNewhole = false)
    {

        Debug.Log("CheckIfLightningStartsThisTurn: Checking for lightning at new turn. Is there lightning now? " + IsThereLightning.ToString());
        // if this check is for a new hole, reset the _lightningStormThisTurnAlready value so lightning storms can start again on the new hole
        if (forNewhole)
            _lightningStormThisTurnAlready = false;

        if (skipLightningCheck)
            return;
        if (!IsThereLightning)
        {
            // only one lightning storm per turn/hole
            if (_lightningStormThisTurnAlready)
                return;
            if (WillLightningStart())
            {
                Debug.Log("CheckIfLightningStartsThisTurn: Lightning will start this turn");
                IsThereLightning = true;
                DistanceFromPlayer = GetInitialLightningDistanceFromPlayer();
                _lightIntensity = GetLightningBrightnessFromDistance(DistanceFromPlayer);
                StopLightningRoutineStuff();
                //_lightningRoutine = StartLightning();
                //StartCoroutine(_lightningRoutine);
                StartLightningStrike();
                _lightningStormThisTurnAlready = true;
            }
        }
        else
        {
            if (WillLightningStop())
            {
                Debug.Log("CheckIfLightningStartsThisTurn: Lightning will STOP this turn");
                IsThereLightning = false;
                StopLightningRoutineStuff();
                return;
            }

            // Update lightning distance for each turn
            DistanceFromPlayer = UpdateLightningDistanceFromPlayer(DistanceFromPlayer);
            // If the distance is far enough away, end the lightning
            if (DistanceFromPlayer < -_maxDistanceFromPlayer)
            {
                Debug.Log("CheckIfLightningStartsThisTurn: Lightning far enough away to end. Distance: " + DistanceFromPlayer.ToString());
                IsThereLightning = false;
                StopLightningRoutineStuff();
                return;
            }
            _lightIntensity = GetLightningBrightnessFromDistance(DistanceFromPlayer);
            // Start a lightning flash at the beginning of a player's turn so they know the distance of the lightning right away from the sound/brightness of the flash. Then, start a coroutine that runs during their turn. When the turn ends (or when the hit is made?) stop that routine to be resumed during the next turn?
            StartLightningStrike();
        }
    }
    bool WillLightningStart()
    {

        float chance = UnityEngine.Random.Range(0f, 1f);
        if (chance > _startLightningOdds)
            return true;
        else
            return false;
    }
    bool WillLightningStop()
    {
        if (RainManager.instance.BaseRainState == "clear")
            return true;

        float chance = UnityEngine.Random.Range(0f, 1f);
        Debug.Log("WillLightningStop: Chance is: " + chance.ToString() + " against stop odds of: " + _stopLightningOdds.ToString());
        if (chance > _stopLightningOdds)
            return true;
        else
            return false;
    }
    float GetInitialLightningDistanceFromPlayer()
    {
        return UnityEngine.Random.Range(_startLightningOdds * _maxDistanceFromPlayer, _maxDistanceFromPlayer);
    }
    float UpdateLightningDistanceFromPlayer(float currentDist)
    {
        return currentDist - UnityEngine.Random.Range(_minLightningMove, _maxLightningMove);
    }
    float GetLightningBrightnessFromDistance(float dist)
    {
        float distPercent = (_maxDistanceFromPlayer - Mathf.Abs(DistanceFromPlayer)) / _maxDistanceFromPlayer;
        float intensityRange = _maxLightIntensity - _minLightIntensity;
        return Mathf.Clamp((_minLightIntensity + (intensityRange * distPercent)), 0f, 1.0f);
    }
    void GetThunderVolumeFromDistance(float dist)
    {
        dist = Mathf.Abs(dist);
    }
    [Server]
    public void LightningForHit(GolfPlayerTopDown player)
    {
        if (!IsThereLightning)
            return;
        //StartCoroutine(RandomWaitForLightningAfterHit());
        // check if the player should be struck by lightning
        if (WillPlayerBeStruckByLightning(player))
        {
            Debug.Log("WillPlayerBeStruckByLightning: Yes!!!");
            //DistanceFromPlayer = 0f;
            _lightIntensity = 1.5f;
            _playerThatWasStruck = player;
            _playerThatWasStruckNetId = player.ObjectId;
            //player.StruckByLightning(); // this should be called on the clients
            _wasPlayerStruck = true;
        }
        else
        {
            Debug.Log("WillPlayerBeStruckByLightning: no...");
            _wasPlayerStruck = false;
            _playerThatWasStruck = null;
            _playerThatWasStruckNetId = -99;
        }
        StartLightningStrike(true);

    }
    IEnumerator RandomWaitForLightningAfterHit()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.25f, 1.25f));
        StartLightningStrike();
    }
    void StartLightningStrike(bool forHit = false)
    {
        Debug.Log("StartLightningStrike: for a player hit? " + forHit.ToString());
        if (!IsThereLightning)
            return;
        int numberOfFlashes = UnityEngine.Random.Range(3, 6);

        //GetThunderVolumeFromDistance(DistanceFromPlayer);
        //StartCoroutine(LightningFlash(numberOfFlashes));
        //StartCoroutine(ThunderClap(DistanceFromPlayer, numberOfFlashes));
        RpcStartLightningStrike(DistanceFromPlayer, _lightIntensity, numberOfFlashes, _wasPlayerStruck, _playerThatWasStruckNetId);
    }
    [ObserversRpc]
    void RpcStartLightningStrike(float distFromPlay, float lightIntensity, int numberOfFlashes, bool wasPlayerStruck, int playerThatWasStruckNetId = -99)
    {
        if (!this.IsServer)
        {
            _lightIntensity = lightIntensity;
            _wasPlayerStruck = wasPlayerStruck;
            if (_wasPlayerStruck && playerThatWasStruckNetId != -99)
            {
                _playerThatWasStruckNetId = playerThatWasStruckNetId;
                _playerThatWasStruck = InstanceFinder.ClientManager.Objects.Spawned[_playerThatWasStruckNetId].GetComponent<GolfPlayerTopDown>();
            }
        }

        Debug.Log("RpcStartLightningStrike: Was player struck by lightning? " + _wasPlayerStruck.ToString() + " with net id of: " + playerThatWasStruckNetId.ToString());

        if (_wasPlayerStruck)
        {
            _playerThatWasStruck.StruckByLightning();
        }

        GetThunderVolumeFromDistance(distFromPlay);
        StartCoroutine(LightningFlash(numberOfFlashes));
        StartCoroutine(ThunderClap(distFromPlay, numberOfFlashes));
    }
    public bool WillPlayerBeStruckByLightning(GolfPlayerTopDown player)
    {
        // Can't be struck by lightning if there is no lightning storm. May need to change with the move toward individualized weather? Or "IsThereLightning" will be based on the "base" weather which will be determined by the average favor of the player group?
        if (!this.IsThereLightning)
            return false;
        // If the weather is clear for the player, then they won't be struck by lightning
        Debug.Log("WillPlayerBeStruckByLightning: The base weather is: " + RainManager.instance.RainState);
        if (RainManager.instance.RainState == "clear")
        {
            Debug.Log("WillPlayerBeStruckByLightning: The player's weather is: " + RainManager.instance.RainState + " and you can't be struck by lightning if it is clear out!!");
            return false;
        }
        // If player has perfect +10 favor, no lightning strike. If player has perfect -10 favor, always strike them
        if (player.FavorWeather >= 10)
        {
            Debug.Log("WillPlayerBeStruckByLightning: Player has max favor of: " + player.FavorWeather.ToString() + " so lightning CANNOT strike them down.");
            return false;
        }
        if (player.FavorWeather <= -10)
        {
            Debug.Log("WillPlayerBeStruckByLightning: Player has minimum favor of: " + player.FavorWeather.ToString() + " so lightning WILL ALWAYS strike them down.");
            return true;
        }

        // If the distance from the player is (almost) exactly zero, the player gets struck unless they have a perfect 10 favor
        if (Mathf.Abs(DistanceFromPlayer) <= 0.001f)
        {
            Debug.Log("WillPlayerBeStruckByLightning: Distance from player is almost exactly zero (" + DistanceFromPlayer.ToString("0.0000") + ") AND player has a favor of: " + player.FavorWeather.ToString() + " so they WILL be struck by lightning!!!");
            return true;
        }
        //If the distance from the player is (almost) exactly 1.5f, the player will NOT be struck unless they have max negative favor
        if (Mathf.Abs(DistanceFromPlayer) >= 1.499f)
        {
            Debug.Log("WillPlayerBeStruckByLightning: Distance from player is almost exactly 1.5f (" + DistanceFromPlayer.ToString("0.0000") + ") AND player has a favor of: " + player.FavorWeather.ToString() + " so they WILL NOT be struck by lightning!!!");
            return false;
        }

        // Get the weather favor modifer for the player. The further negative they are, the more likely to be struck by lightning. The more positive, the less likely to be struck?
        float favorModifier = GetFavorModifierForPlayer(player);
        float lightningChance = UnityEngine.Random.Range(0f, 1.0f);
        Debug.Log("WillPlayerBeStruckByLightning: DistanceFromPlayer is: " + DistanceFromPlayer.ToString() + " and their favor modifier is: " + favorModifier.ToString() + " and the lightning chance will be: " + lightningChance.ToString());
        return lightningChance > (Mathf.Abs(DistanceFromPlayer) + favorModifier);
        // Still need to make an actual system for when lightning should or should not strike a player
        //return UnityEngine.Random.Range(0f, 1.0f) > Mathf.Abs(DistanceFromPlayer);
    }
    float GetFavorModifierForPlayer(GolfPlayerTopDown player)
    {
        float favorModifier = player.FavorWeather / 9f;
        return favorModifier;
    }
    void StopLightningRoutineStuff()
    {
        if (_lightningFlashing)
        {
            try
            {
                StopCoroutine(_lightningRoutine);
            }
            catch (Exception e)
            {
                Debug.Log("StopLightningRoutineStuff: Could stop coroutine. Error: " + e);
            }
            _lightningFlashing = false;
        }
        if (_lightningFlashing)
            _lightningFlashing = false;
        if (_wasPlayerStruck)
            _wasPlayerStruck = false;
        if (_playerThatWasStruck)
            _playerThatWasStruck = null;

        if (IsServer)
            RpcLightningRoutineStuff();
    }
    [ObserversRpc]
    void RpcLightningRoutineStuff()
    {
        if (IsServer)
            return;
        StopLightningRoutineStuff();
    }
    void SyncIsThereLightning(bool prev, bool next, bool asServer)
    {
        if (asServer)
            return;
        if (next)
        {

        }
        else
        {
            StopLightningRoutineStuff();
        }
    }
    //[Server]
    //public void DetermineIfPlayerWillBeStruckByLightningThisTurn(GolfPlayerTopDown player)
    //{
    //    Debug.Log("DetermineIfPlayerWillBeStruckByLightningThisTurn");
    //    if (WillPlayerBeStruckByLightning(player))
    //    {
    //        _wasPlayerStruck = true;
    //        _playerThatWasStruckNetId = player.ObjectId;
    //        _playerThatWasStruck = player;
    //    }
    //    else
    //    {
    //        _wasPlayerStruck = false;
    //        _playerThatWasStruckNetId = -99;
    //        _playerThatWasStruck = null;
    //        return;
    //    }
    //}
    [Server]
    public void EndStorm()
    {
        if (IsThereLightning)
        {
            StopLightningRoutineStuff();
            IsThereLightning = false;
        }
    }
    [Server]
    public void WeatherTurnedClear(string weatherState)
    {
        Debug.Log("WeatherTurnedClear: ");
        UpdateWeather(weatherState);
        //StopLightningRoutineStuff();
        //IsThereLightning = false;
    }
    public void TurnOnLightForLightningStrike(float lightIntensity)
    {
        _lightIntensity = lightIntensity;
        _lightningLight.enabled = true;
    }
    public void TurnOffLightForLightningStrike()
    {
        _lightningLight.enabled = false;
    }
    public void PlayThunderClip(bool playerStruck, float distance)
    {
        if (playerStruck)
        {
            SoundManager.instance.PlaySound(_thunderPlayerStruck, 1f);
            return;
        }

        StartCoroutine(ThunderClap(distance, 0));
    }
    [Server]
    public void StopLightningForBrokenGoodWeatherStatue()
    {
        IsThereLightning = false;
        StopLightningRoutineStuff();
    }
}
