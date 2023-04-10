using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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


    // Start is called before the first frame update
    void Start()
    {
        if (!_myPlayer)
            _myPlayer = this.GetComponent<NetworkPlayer>();
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
    void UpdateColorInfo()
    {
        _finalColor = new Color(_red, _green, _blue);
        UpdateBallImageColor();
        _myPlayer.UpdateBallColorValue(_finalColor);
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
