using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BallColorPicker : MonoBehaviour
{
    [SerializeField] NetworkPlayer _myPlayer;

    [Header("Color Picker Stuff")]
    [SerializeField] Color _finalColor;
    [SerializeField] float _red = 1;
    [SerializeField] float _green = 1;
    [SerializeField] float _blue = 1;

    [Header("UI Components")]
    [SerializeField] Slider _redSlider;
    [SerializeField] Slider _greenSlider;
    [SerializeField] Slider _blueSlider;
    [SerializeField] Image _ballImage;

    [Header("Player Prefs Stuff")]
    private const string _ballColorRed = "BallColorRed";
    private const string _ballColorGreen = "BallColorGreen";
    private const string _ballColorBlue = "BallColorBlue";


    // Start is called before the first frame update
    void Start()
    {
        if (!_myPlayer)
            _myPlayer = this.GetComponent<NetworkPlayer>();
        //GetPlayerPrefValues();

        _finalColor = new Color(_red, _green, _blue);

        UpdateSliderPositions();
        UpdateBallImageColor();

        _redSlider.onValueChanged.AddListener(delegate { RedSliderChange(); });
        _greenSlider.onValueChanged.AddListener(delegate { GreenSliderChange(); });
        _blueSlider.onValueChanged.AddListener(delegate { BlueSliderChange(); });

    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void GetPlayerPrefValues()
    {   
        try
        {
            if (PlayerPrefs.HasKey(_ballColorRed))
                _red = PlayerPrefs.GetFloat(_ballColorRed);
            if (PlayerPrefs.HasKey(_ballColorGreen))
                _green = PlayerPrefs.GetFloat(_ballColorGreen);
            if (PlayerPrefs.HasKey(_ballColorBlue))
                _blue = PlayerPrefs.GetFloat(_ballColorBlue);

            _finalColor = new Color(_red, _green, _blue);
            UpdateSliderPositions();
            UpdateBallImageColor();
            UpdateColorInfo();
        }
        catch (Exception e)
        {
            Debug.Log("GetPlayerPrefValues: could not get player pref values for ball call. Error: " + e);
        }
    }
    void UpdatePlayerPrefValues()
    {
        try
        {
            PlayerPrefs.SetFloat(_ballColorRed, _red);
            PlayerPrefs.SetFloat(_ballColorGreen, _green);
            PlayerPrefs.SetFloat(_ballColorBlue, _blue);
        }
        catch (Exception e)
        {
            Debug.Log("UpdatePlayerPrefValues: could not update player pref values for ball call. Error: " + e);
        }
        
    }
    void UpdateColorInfo()
    {
        Debug.Log("UpdateColorInfo: ");
        _finalColor = new Color(_red, _green, _blue);
        UpdateBallImageColor();
        _myPlayer.UpdateBallColorValue(_finalColor);
        UpdatePlayerPrefValues();
    }
    void UpdateSliderPositions()
    {
        _redSlider.value = _red;
        _greenSlider.value = _green;
        _blueSlider.value = _blue;
    }
    void UpdateBallImageColor()
    {
        _ballImage.color = _finalColor;
    }
    void RedSliderChange()
    {
        _red = _redSlider.value;
        UpdateColorInfo();
    }
    void GreenSliderChange()
    {
        _green = _greenSlider.value;
        UpdateColorInfo();
    }
    void BlueSliderChange()
    {
        _blue = _blueSlider.value;
        UpdateColorInfo();
    }
}
