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

    public AudioMixer audioMixer;
    public Dropdown resolutionDropdown;
    public Slider volumeSlider;
    public Toggle fillScreenToggle;
    int currentScreenWidth;
    int currentScreenHeight;
    float currentVolume;
    Resolution[] resolutions;

    private void Start()
    {
        GetResolutions();
        LoadSettings();
    }
    private void Update()
    {

    }

    public void SetVolume(float volume)
    {
        Debug.Log("SetVolume: " + volume.ToString());
        if (volume >= -35 && volume <= 5)
            audioMixer.SetFloat("volume", volume);
        else
        {
            volume = 0f;
            audioMixer.SetFloat("volume", volume);
        }
        currentVolume = volume;
        volumeSlider.value = volume;
        if (volumeSlider.gameObject.activeInHierarchy)
            SoundManager.instance.PlaySound("bottle-break", 1.0f);
    }
    public void SetFullscreen(bool isFullScreen)
    {
        Debug.Log("SetFullscreen: " + isFullScreen.ToString());
        fillScreenToggle.isOn = isFullScreen;
        Screen.fullScreen = isFullScreen;
        //string test = Screen.fullScreenMode
        //ullScreenMode fullScreen = Screen.FullScreenMode;
        //Screen.fullScreenMode = FullScreenMode.Windowed;
    }
    void GetResolutions()
    {
        //resolutions = Screen.resolutions;
        List<Resolution> resolutionList = new List<Resolution>();
        double aspectRatio = ((double)16 / (double)9);
        foreach (Resolution resolution in Screen.resolutions)
        {
            double resolutionAspectRation = (double)resolution.width / (double)resolution.height;
            if ((resolutionAspectRation == aspectRatio) && (resolution.refreshRate == Screen.currentResolution.refreshRate))
            {
                resolutionList.Add(resolution);
            }

        }
        if (resolutionList.Count > 0)
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
        }
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
            Screen.SetResolution(width, height, fullScreen);
        else
            Screen.SetResolution(1920, 1080, false);
    }
    public void SaveSettings()
    {
        Debug.Log("Saving Settings");
        PlayerPrefs.SetFloat(volumePlayerPrefKey, currentVolume);
        int fullScreenBool = Convert.ToInt32(fillScreenToggle.isOn);
        if (fullScreenBool == 1 || fullScreenBool == 0)
            PlayerPrefs.SetInt(fullScreenPlayerPrefKey, fullScreenBool);
        else
            PlayerPrefs.SetInt(fullScreenPlayerPrefKey, 0);
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
    }
}
