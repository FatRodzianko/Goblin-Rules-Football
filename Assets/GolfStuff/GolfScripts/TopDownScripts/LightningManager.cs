using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightningManager : MonoBehaviour
{
    [SerializeField] Light2D _lightningLight;
    [SerializeField] bool _lightningFlashing = false;
    [SerializeField] bool _lightningRoutineRunning = false;
    IEnumerator _lightningRoutine;

    public bool IsThereLightning = false;

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

    [Header("Movement Ranges")]
    [SerializeField] float _minLightningMove;
    [SerializeField] float _maxLightningMove;
    [SerializeField] float _minLightRainLightningMove = 0.05f;
    [SerializeField] float _maxLightRainLightningMove = 0.15f;
    [SerializeField] float _minMedRainLightningMove = 0.1f;
    [SerializeField] float _maxMedRainLightningMove = 0.2f;
    [SerializeField] float _minHeavyRainLightningMove = 0.15f;
    [SerializeField] float _maxHeavyRainLightningMove = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(StartLightning());
        RainManager.instance.WeatherChanged += UpdateWeather;
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

        }
        else
        {
            if (_lightningRoutineRunning)
            {
                IsThereLightning = false;
                StopCoroutine(_lightningRoutine);                
                _lightningRoutineRunning = false;
            }
        }
    }
    IEnumerator StartLightning()
    {
        _lightningRoutineRunning = true;
        while (IsThereLightning)
        {
            if (_lightningFlashing)
                yield return new WaitForSeconds(1.0f);
            else
            {
                int numberOfFlashes = UnityEngine.Random.Range(2, 6);
                
                GetThunderVolumeFromDistance(DistanceFromPlayer);
                StartCoroutine(LightningFlash(numberOfFlashes));
                float lightningCooldown = UnityEngine.Random.Range(_cooldownMin, _cooldownMax);
                Debug.Log("Delay before LightningFlash: " + lightningCooldown.ToString());
                yield return new WaitForSeconds(lightningCooldown);
            }
            
        }
        yield break;
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
            yield return new WaitForSeconds(flashDelay);
            _lightningLight.enabled = false;

            // cool down before next flash
            float lightningStrikeCooldown = UnityEngine.Random.Range(0.15f, 0.4f);
            yield return new WaitForSeconds(lightningStrikeCooldown);
            totalFlashes++;
        }
        _lightningFlashing = false;
    }
    IEnumerator ThunderClap(float dist, int numberOfFlashes)
    {
        float soundDelay = Mathf.Abs(dist) * 5f;
        Debug.Log("ThunderClap: delay will be: " + soundDelay.ToString() + " Time: " + Time.time);
        yield return new WaitForSeconds(soundDelay);
        Debug.Log("ThunderClap: Thunder will play now. Time: " + Time.time);
        // Play thunder sound
    }
    public void CheckIfLightningStartsThisTurn()
    {
        Debug.Log("CheckIfLightningStartsThisTurn: Checking for lightning at new turn. Is there lightning now? " + IsThereLightning.ToString());
        if (!IsThereLightning)
        {
            if (WillLightningStart())
            {
                Debug.Log("CheckIfLightningStartsThisTurn: Lightning will start this turn");
                IsThereLightning = true;
                DistanceFromPlayer = GetInitialLightningDistanceFromPlayer();
                _lightIntensity = GetLightningBrightnessFromDistance(DistanceFromPlayer);
                if (_lightningRoutineRunning)
                {
                    StopCoroutine(_lightningRoutine);
                    _lightningRoutineRunning = false;
                }
                //_lightningRoutine = StartLightning();
                //StartCoroutine(_lightningRoutine);
                StartLightningStrike();
            }
        }
        else
        {
            if (WillLightningStop())
            {
                Debug.Log("CheckIfLightningStartsThisTurn: Lightning will STOP this turn");
                IsThereLightning = false;
                if (_lightningRoutineRunning)
                {
                    StopCoroutine(_lightningRoutine);
                    _lightningRoutineRunning = false;
                }
                return;
            }

            // Update lightning distance for each turn
            DistanceFromPlayer = UpdateLightningDistanceFromPlayer(DistanceFromPlayer);
            // If the distance is far enough away, end the lightning
            if (DistanceFromPlayer < -_maxDistanceFromPlayer)
            {
                Debug.Log("CheckIfLightningStartsThisTurn: Lightning far enough away to end. Distance: " + DistanceFromPlayer.ToString());
                IsThereLightning = false;
                StopCoroutine(_lightningRoutine);
                _lightningRoutineRunning = false;
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
        return _minLightIntensity + (intensityRange * distPercent);
    }
    void GetThunderVolumeFromDistance(float dist)
    {
        dist = Mathf.Abs(dist);
    }
    public void LightningForHit()
    {
        if (!IsThereLightning)
            return;
        //StartCoroutine(RandomWaitForLightningAfterHit());
        // check if the player should be struck by lightning
        if (WillPlayerBeStruckByLightning())
        {
            Debug.Log("WillPlayerBeStruckByLightning: Yes!!!");
            DistanceFromPlayer = 0f;
            _lightIntensity = GetLightningBrightnessFromDistance(DistanceFromPlayer);
        }
        StartLightningStrike();

    }
    IEnumerator RandomWaitForLightningAfterHit()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.25f, 1.25f));
        StartLightningStrike();
    }
    void StartLightningStrike()
    {
        if (!IsThereLightning)
            return;
        int numberOfFlashes = UnityEngine.Random.Range(2, 6);

        GetThunderVolumeFromDistance(DistanceFromPlayer);
        StartCoroutine(LightningFlash(numberOfFlashes));
        StartCoroutine(ThunderClap(DistanceFromPlayer, numberOfFlashes));
    }
    bool WillPlayerBeStruckByLightning()
    {
        return UnityEngine.Random.Range(0f, 1.0f) > DistanceFromPlayer;
    }
}
