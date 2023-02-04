using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherIconHolder : MonoBehaviour
{
    [Header("Sprite Renderers")]
    [SerializeField] SpriteRenderer _weatherIcon;
    [SerializeField] SpriteRenderer _weatherText;

    [Header("Weather Icons")]
    [SerializeField] Sprite _clearSprite;
    [SerializeField] Sprite _lightRainSprite;
    [SerializeField] Sprite _medRainSprite;
    [SerializeField] Sprite _heavyRainSprite;

    [Header("Weather Text Sprites")]
    [SerializeField] Sprite _clearSpriteText;
    [SerializeField] Sprite _lightRainSpriteText;
    [SerializeField] Sprite _medRainSpriteText;
    [SerializeField] Sprite _heavyRainSpriteText;


    // Start is called before the first frame update
    void Start()
    {
        RainManager.instance.WeatherChanged += UpdateWeatherIcons;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void UpdateWeatherIcons(string newWeatherEffect)
    {
        //Debug.Log("UpdateWeatherIcons: updating to new state: " + newWeatherEffect);
        if (string.IsNullOrWhiteSpace(newWeatherEffect))
            return;

        newWeatherEffect = newWeatherEffect.ToLower();

        if (newWeatherEffect == "light rain")
        {
            _weatherIcon.sprite = _lightRainSprite;
            _weatherText.sprite = _lightRainSpriteText;

        }
        else if (newWeatherEffect == "med rain")
        {
            _weatherIcon.sprite = _medRainSprite;
            _weatherText.sprite = _medRainSpriteText;
        }
        else if (newWeatherEffect == "heavy rain")
        {
            _weatherIcon.sprite = _heavyRainSprite;
            _weatherText.sprite = _heavyRainSpriteText;
        }
        else
        {
            _weatherIcon.sprite = _clearSprite;
            _weatherText.sprite = _clearSpriteText;
        }
    }
}
