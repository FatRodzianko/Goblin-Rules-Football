using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SettingsManager : MonoBehaviour
{

    private const string resolutionWidthPlayerPrefKey = "ResolutionWidth";
    private const string resolutionHeightPlayerPrefKey = "ResolutionHeight";
    private const string volumePlayerPrefKey = "VolumePreference";
    private const string fullScreenPlayerPrefKey = "FullScreen";
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
    int currentScreenWidth;
    int currentScreenHeight;
    
    Resolution[] resolutions;
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
    public void SetFullscreen(bool isFullScreen)
    {
        Debug.Log("SetFullscreen: " + isFullScreen.ToString());
        fillScreenToggle.isOn = isFullScreen;
        //Screen.fullScreenMode = FullScreenMode.Windowed;
        //Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        Screen.fullScreen = isFullScreen;
        //string test = Screen.fullScreenMode
        //ullScreenMode fullScreen = Screen.FullScreenMode;
        //Screen.fullScreenMode = FullScreenMode.Windowed;
    }
    void GetResolutions()
    {
        //resolutions = Screen.resolutions;
        //Debug.Log("GetResolutions: Current screen resolution is: " + Screen.currentResolution.ToString());
        List<Resolution> resolutionList = new List<Resolution>();
        double aspectRatio = ((double)16 / (double)9);

        double maxHeight = 0;

        foreach (Resolution resolution in Screen.resolutions)
        {
            double resolutionAspectRation = (double)resolution.width / (double)resolution.height;
            if ((resolutionAspectRation == aspectRatio) && (resolution.refreshRate == Screen.currentResolution.refreshRate))
            {
                resolutionList.Add(resolution);
            }
            //Debug.Log("GetResolutions: Found resolution of " + resolution.ToString());
            if (resolution.height > maxHeight)
                maxHeight = resolution.height;

        }
        Debug.Log("GetResolutions: The max screen resolution height is: " + maxHeight.ToString());
        /*if (resolutionList.Count > 0)
        {
            resolutions = resolutionList.ToArray();
        }
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        if (resolutions.Length > 0)
        {
            int currentResolutionIndex = 0;
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + "x" + resolutions[i].height;
                options.Add(option);


                if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
                {
                    currentResolutionIndex = i;
                }
            }
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            ResolutionDropDown(currentResolutionIndex);
        }*/
        resolutionList.Clear();
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        string option = "";
        int index = 0;
        int currentResolutionIndex = 0;
        Resolution newRes = new Resolution();
        if (maxHeight >= 720)
        {
            option = "1280x720";
            options.Add(option);
            
            newRes.width = 1280;
            newRes.height = 720;
            newRes.refreshRate = Screen.currentResolution.refreshRate;
            resolutionList.Add(newRes);

            if (Screen.width == 1280 && Screen.height == 720)
            {
                currentResolutionIndex = index;
            }
            index++;
        }
        if (maxHeight >= 900)
        {
            option = "1600x900";
            options.Add(option);

            newRes.width = 1600;
            newRes.height = 900;
            newRes.refreshRate = Screen.currentResolution.refreshRate;
            resolutionList.Add(newRes);

            if (Screen.width == 1600 && Screen.height == 900)
            {
                currentResolutionIndex = index;
            }
            index++;
        }
        if (maxHeight >= 1080)
        {
            option = "1920x1080";
            options.Add(option);

            newRes.width = 1920;
            newRes.height = 1080;
            newRes.refreshRate = Screen.currentResolution.refreshRate;
            resolutionList.Add(newRes);

            if (Screen.width == 1920 && Screen.height == 1080)
            {
                currentResolutionIndex = index;
            }
            index++;
        }
        if (maxHeight >= 1440)
        {
            option = "2560x1440";
            options.Add(option);

            newRes.width = 2560;
            newRes.height = 1440;
            newRes.refreshRate = Screen.currentResolution.refreshRate;
            resolutionList.Add(newRes);

            if (Screen.width == 2560 && Screen.height == 1440)
            {
                currentResolutionIndex = index;
            }
            index++;
        }
        if (resolutionList.Count > 0)
        {
            resolutions = resolutionList.ToArray();
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        ResolutionDropDown(currentResolutionIndex);

        //LoadSettings();        
    }
    public void ResolutionDropDown(int index)
    {
        currentScreenWidth = resolutions[index].width;
        currentScreenHeight = resolutions[index].height;

        Debug.Log("ResolutionDropDown " + currentScreenWidth.ToString() + " " + currentScreenHeight.ToString());
    }
    public void SetResolution(int width, int height, bool fullScreen)
    {
        Debug.Log("SetResolution: " + width.ToString() + "x" + height.ToString() + " " + fullScreen.ToString());
        if (width > 0 && height > 0 && (width / height) == (1920 / 1080))
        {
            if(fullScreen)
                Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
            else
                Screen.SetResolution(width, height, false);
        }   
        else
            Screen.SetResolution(1920, 1080, false);
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

        int fullScreenBool = Convert.ToInt32(fillScreenToggle.isOn);
        if (fullScreenBool == 1 || fullScreenBool == 0)
            PlayerPrefs.SetInt(fullScreenPlayerPrefKey, fullScreenBool);
        else
            PlayerPrefs.SetInt(fullScreenPlayerPrefKey, 0);
        int gamepadUIBool = Convert.ToInt32(gamepadUIToggle.isOn);
        if (gamepadUIBool == 1 || gamepadUIBool == 0)
            PlayerPrefs.SetInt(gamepadUIPlayerPrefKey, gamepadUIBool);
        else
            PlayerPrefs.SetInt(gamepadUIPlayerPrefKey, 0);
        if (currentScreenHeight > 0 && currentScreenWidth > 0 && (currentScreenWidth / currentScreenHeight) == (1920 / 1080))
        {
            PlayerPrefs.SetInt(resolutionWidthPlayerPrefKey, currentScreenWidth);
            PlayerPrefs.SetInt(resolutionHeightPlayerPrefKey, currentScreenHeight);
        }
        else
        {
            PlayerPrefs.SetInt(resolutionWidthPlayerPrefKey, 1920);
            PlayerPrefs.SetInt(resolutionHeightPlayerPrefKey, 1080);
        }

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
        if (PlayerPrefs.HasKey(fullScreenPlayerPrefKey))
        {
            int fullScreenBool = PlayerPrefs.GetInt(fullScreenPlayerPrefKey);
            if (fullScreenBool == 1 || fullScreenBool == 0)
                SetFullscreen(Convert.ToBoolean(PlayerPrefs.GetInt(fullScreenPlayerPrefKey)));
            else
                SetFullscreen(false);
        }
        else
        {
            SetFullscreen(false);
        }
        if (PlayerPrefs.HasKey(resolutionWidthPlayerPrefKey) && PlayerPrefs.HasKey(resolutionHeightPlayerPrefKey) && PlayerPrefs.HasKey(fullScreenPlayerPrefKey))
        {
            int width = PlayerPrefs.GetInt(resolutionWidthPlayerPrefKey);
            int height = PlayerPrefs.GetInt(resolutionHeightPlayerPrefKey);
            int fullScreenBool = PlayerPrefs.GetInt(fullScreenPlayerPrefKey);
            if ((width / height) == (1920 / 1080) && width > 0 && height > 0 && (fullScreenBool == 1 || fullScreenBool == 0))
            {
                SetResolution(PlayerPrefs.GetInt(resolutionWidthPlayerPrefKey), PlayerPrefs.GetInt(resolutionHeightPlayerPrefKey), Convert.ToBoolean(PlayerPrefs.GetInt(fullScreenPlayerPrefKey)));
            }
            else
            {
                SetResolution(1920, 1080, false);
            }

        }
        else
        {
            SetResolution(1920, 1080, false);
        }
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
}
