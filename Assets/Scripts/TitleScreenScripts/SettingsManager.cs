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

    public AudioMixer audioMixer;
    public Dropdown resolutionDropdown;
    public Slider volumeSlider;
    public Toggle fillScreenToggle;
    public Toggle gamepadUIToggle;
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
    public void SaveSettings()
    {
        Debug.Log("Saving Settings");
        PlayerPrefs.SetFloat(volumePlayerPrefKey, currentVolume);
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
    }
}
