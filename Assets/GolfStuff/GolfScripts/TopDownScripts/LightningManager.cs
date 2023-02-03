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
        }
        else if (newWeather.Equals("med rain"))
        {
            _cooldownMin = _medRainMin;
            _cooldownMax = _medRainMax;
            _startLightningOdds = _medRainOddsToStart;
            _stopLightningOdds = _medRainOddsToStop;
        }
        else if (newWeather.Equals("heavy rain"))
        {
            _cooldownMin = _heavyRainMin;
            _cooldownMax = _heavyRainMax;
            _startLightningOdds = _heavyRainOddsToStart;
            _stopLightningOdds = _heavyRainOddsToStop;
        }
        else
        {
            isRain = false;
            _startLightningOdds = 1.0f;
            _stopLightningOdds = 0f;
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
                UpdateLightningDistanceFromPlayer();
                GetLightningBrightnessFromDistance();
                GetThunderVolumeFromDistance();
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
    IEnumerator ThunderClap(float soundDelay, int numberOfFlashes)
    {
        yield return new WaitForSeconds(soundDelay);
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
                GetInitialLightningDistanceFromPlayer();
                GetLightningBrightnessFromDistance();
                GetThunderVolumeFromDistance();
                _lightningRoutine = StartLightning();
                StartCoroutine(_lightningRoutine);
            }
        }
        else
        {
            if (WillLightningStop())
            {
                Debug.Log("CheckIfLightningStartsThisTurn: Lightning will STOP this turn");
                IsThereLightning = false;
                StopCoroutine(_lightningRoutine);
                _lightningRoutineRunning = false;
            }
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
        if (chance > _stopLightningOdds)
            return true;
        else
            return false;
    }
    void GetInitialLightningDistanceFromPlayer()
    { 

    }
    void UpdateLightningDistanceFromPlayer()
    { 

    }
    void GetLightningBrightnessFromDistance()
    { 

    }
    void GetThunderVolumeFromDistance()
    { 

    }
}
