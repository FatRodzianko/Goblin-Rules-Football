using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SettingsManager : MonoBehaviour
{

    // The resolution and window mode are not stored here: they belong to DisplayPreferences, which
    // every menu shares. This only shows them and hands the player's choice over on Save.
    private const string volumePlayerPrefKey = "VolumePreference";
    private const string gamepadUIPlayerPrefKey = "GamepadUI";
    private const string crtScreenEffectPrefKey = "CRT";
    private const string _gameSFXVolumePrefKEy = "GameSFXVolume";
    private const string _musicVolumePrefKEy = "MusicVolume";


    public AudioMixer audioMixer;
    public AudioMixerGroup _gameSFXMixer;
    public AudioMixerGroup _musicMixer;
    public Dropdown resolutionDropdown;
    
    public Toggle fillScreenToggle;
    public Toggle gamepadUIToggle;
    private int _chosenModeIndex;

    public Toggle crtScreenEffectToggle;

    [Header("Volume Slider Stuff")]
    public Slider volumeSlider;
    [SerializeField] float currentVolume;
    [SerializeField] Slider _gameSFXSlider;
    [SerializeField] float _gameSFXVolume;
    [SerializeField] float _defaultGameSFXVolume = -21f;
    [SerializeField] Slider _musicSlider;
    [SerializeField] float _musicVolume;
    [SerializeField] float _defaultMusicVolume = -21f;

    private void Start()
    {
        // A scene can carry this without its menu (the map maker does), which leaves nothing to show.
        if (resolutionDropdown == null)
            return;
        GetResolutions();
        LoadSettings();
    }
    private void Update()
    {

    }

    public void SetVolume(float volume, string audioGroup)
    {
        Debug.Log("SetVolume: " + volume.ToString() + " for audio group: " + audioGroup);
        if (String.IsNullOrEmpty(audioGroup))
            audioGroup = "volume";
        if (volume >= -35 && volume <= 5)
        {
            audioMixer.SetFloat(audioGroup, volume);
        }   
        else
        {
            volume = 0f;
            audioMixer.SetFloat(audioGroup, volume);
        }
        //currentVolume = volume;
        //volumeSlider.value = volume;
        //if (volumeSlider.gameObject.activeInHierarchy)
        //    SoundManager.instance.PlaySound("bottle-break", 1.0f);
        SetVolumeSliderPosition(volume, audioGroup);
    }
    void SetVolumeSliderPosition(float newVolume, string audioGroup)
    {
        if (audioGroup == "volume")
        {
            currentVolume = newVolume;
            volumeSlider.value = newVolume;
            if (volumeSlider.gameObject.activeInHierarchy)
                SoundManager.instance.PlaySound("bottle-break", 1.0f);
        }
        else if (audioGroup == "gameSFXVolume")
        {
            _gameSFXVolume = newVolume;
            _gameSFXSlider.value = newVolume;
            if (_gameSFXSlider.gameObject.activeInHierarchy)
                SoundManager.instance.PlaySound("bottle-break", 1.0f);
        }
        else if (audioGroup == "musicVolume")
        {
            _musicVolume = newVolume;
            _musicSlider.value = newVolume;
        }
    }
    // Lists the modes the display reports and selects the one on screen. Nothing is applied here:
    // the window already shows the player's mode, and a scene load must never change it.
    void GetResolutions()
    {
        DisplayPreferences.RefreshModes();
        IList<Resolution> modes = DisplayPreferences.Modes;
        List<string> options = new List<string>();
        for (int i = 0; i < modes.Count; i++)
            options.Add(modes[i].width + " x " + modes[i].height);
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        ShowCurrentDisplayMode();
    }
    public void ResolutionDropDown(int index)
    {
        _chosenModeIndex = index;
    }
    private void ShowCurrentDisplayMode()
    {
        _chosenModeIndex = DisplayPreferences.CurrentModeIndex();
        if (resolutionDropdown != null)
            resolutionDropdown.SetValueWithoutNotify(_chosenModeIndex);
        if (fillScreenToggle != null)
            fillScreenToggle.SetIsOnWithoutNotify(DisplayPreferences.IsFullscreen);
    }
    private void SaveDisplayMode()
    {
        Resolution mode;
        if (!DisplayPreferences.TryGetMode(_chosenModeIndex, out mode))
            return;
        bool fullscreen = fillScreenToggle == null || fillScreenToggle.isOn;
        DisplayPreferences.ApplyAndStore(mode, fullscreen);
    }
    public void SetCRTScreenEffect(bool isCRTEffectOn)
    {
        Debug.Log("SetCRTScreenEffect: " + isCRTEffectOn.ToString());
        crtScreenEffectToggle.isOn = isCRTEffectOn;

        try
        {
            RenderFeaturesManager.instance.EnableRetroCRT(isCRTEffectOn);
        }
        catch (Exception e)
        {
            Debug.Log("SetCRTScreenEffect: Could not access RenderFeaturesManager. Error: " + e);
        }

    }
    public void SaveSettings()
    {
        Debug.Log("Saving Settings");
        // Audio values
        PlayerPrefs.SetFloat(volumePlayerPrefKey, currentVolume);
        PlayerPrefs.SetFloat(_gameSFXVolumePrefKEy, _gameSFXVolume);
        PlayerPrefs.SetFloat(_musicVolumePrefKEy, _musicVolume);

        int gamepadUIBool = Convert.ToInt32(gamepadUIToggle.isOn);
        if (gamepadUIBool == 1 || gamepadUIBool == 0)
            PlayerPrefs.SetInt(gamepadUIPlayerPrefKey, gamepadUIBool);
        else
            PlayerPrefs.SetInt(gamepadUIPlayerPrefKey, 0);

        SaveDisplayMode();

        int crtBool = Convert.ToInt32(crtScreenEffectToggle.isOn);
        if (crtBool == 1 || crtBool == 0)
            PlayerPrefs.SetInt(crtScreenEffectPrefKey, crtBool);
        else
            PlayerPrefs.SetInt(crtScreenEffectPrefKey, 0);

        LoadSettings();
    }
    public void LoadSettings()
    {
        Debug.Log("Loading Settings");
        if (PlayerPrefs.HasKey(volumePlayerPrefKey))
        {
            //SetVolume(PlayerPrefs.GetFloat(volumePlayerPrefKey));
            float setVolume = PlayerPrefs.GetFloat(volumePlayerPrefKey);
            if (setVolume >= -35 && setVolume <= 5)
                audioMixer.SetFloat("volume", setVolume);
            else
                setVolume = 0;
            currentVolume = setVolume;
            volumeSlider.value = setVolume;
        }
        else
        {
            //SetVolume(0);
            float setVolume = 0f;
            audioMixer.SetFloat("volume", setVolume);
            currentVolume = setVolume;
            volumeSlider.value = setVolume;

        }
        if (PlayerPrefs.HasKey(_gameSFXVolumePrefKEy))
        {
            //SetVolume(PlayerPrefs.GetFloat(_gameSFXVolumePrefKEy));
            float setVolume = PlayerPrefs.GetFloat(_gameSFXVolumePrefKEy);
            if (setVolume >= -35 && setVolume <= 5)
                audioMixer.SetFloat("gameSFXVolume", setVolume);
            else
                setVolume = _defaultGameSFXVolume;
            _gameSFXVolume = setVolume;
            _gameSFXSlider.value = setVolume;
        }
        else
        {
            //SetVolume(0);
            float setVolume = _defaultGameSFXVolume;
            audioMixer.SetFloat("gameSFXVolume", setVolume);
            _gameSFXVolume = setVolume;
            _gameSFXSlider.value = setVolume;

        }
        if (PlayerPrefs.HasKey(_musicVolumePrefKEy))
        {
            //SetVolume(PlayerPrefs.GetFloat(_musicVolumePrefKEy));
            float setVolume = PlayerPrefs.GetFloat(_musicVolumePrefKEy);
            if (setVolume >= -35 && setVolume <= 5)
                audioMixer.SetFloat("musicVolume", setVolume);
            else
                setVolume = _defaultMusicVolume;
            _musicVolume = setVolume;
            _musicSlider.value = setVolume;
        }
        else
        {
            //SetVolume(0);
            float setVolume = _defaultMusicVolume;
            audioMixer.SetFloat("musicVolume", setVolume);
            _musicVolume = setVolume;
            _musicSlider.value = setVolume;

        }
        ShowCurrentDisplayMode();
        if (PlayerPrefs.HasKey(gamepadUIPlayerPrefKey))
        {
            int gamepadUIBool = PlayerPrefs.GetInt(gamepadUIPlayerPrefKey);
            if (gamepadUIBool == 1 || gamepadUIBool == 0)
            {
                GamepadUIManager.instance.gamepadUI = Convert.ToBoolean(PlayerPrefs.GetInt(gamepadUIPlayerPrefKey));
                gamepadUIToggle.isOn = Convert.ToBoolean(gamepadUIBool);
            }
            else
            {
                GamepadUIManager.instance.gamepadUI = false;
                gamepadUIToggle.isOn = false;
            }
                
        }
        else
        {
            GamepadUIManager.instance.gamepadUI = false;
            gamepadUIToggle.isOn = false;
        }
        if (PlayerPrefs.HasKey(crtScreenEffectPrefKey))
        {
            int crtBool = PlayerPrefs.GetInt(crtScreenEffectPrefKey);
            if (crtBool == 1 || crtBool == 0)
                SetCRTScreenEffect(Convert.ToBoolean(PlayerPrefs.GetInt(crtScreenEffectPrefKey)));
            else
                SetCRTScreenEffect(false);
        }
        else
        {
            SetCRTScreenEffect(false);
        }
    }
    // Resets only what this menu shows. Never PlayerPrefs.DeleteAll, which would also take the
    // player's key bindings, name and golf settings.
    public void ResetToDefault()
    {
        PlayerPrefs.DeleteKey(volumePlayerPrefKey);
        PlayerPrefs.DeleteKey(_gameSFXVolumePrefKEy);
        PlayerPrefs.DeleteKey(_musicVolumePrefKEy);
        PlayerPrefs.DeleteKey(gamepadUIPlayerPrefKey);
        PlayerPrefs.DeleteKey(crtScreenEffectPrefKey);
        PlayerPrefs.Save();
        DisplayPreferences.ResetToDefault();
        LoadSettings();
    }
}
