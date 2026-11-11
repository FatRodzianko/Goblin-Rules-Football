using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// The SETTINGS panel. Display work belongs to <see cref="DisplaySettings"/>; everything else is
/// simple enough to live here.
///
/// Volume moves as the slider moves so the player can hear it, but nothing is written to disk until
/// SAVE. A display change is applied on SAVE and then has to be confirmed, which is what stops a
/// player picking a mode they cannot see.
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    private const string MasterVolumeKey = "VolumePreference";
    private const string SfxVolumeKey = "GameSFXVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string GamepadUiKey = "GamepadUI";
    private const string CrtKey = "CRT";

    private const string MasterMixerParam = "volume";
    private const string SfxMixerParam = "gameSFXVolume";
    private const string MusicMixerParam = "musicVolume";

    private const float MinVolume = -35f;
    private const float MaxVolume = 5f;
    private const float DefaultMasterVolume = 0f;
    private const float DefaultSfxVolume = -21f;
    private const float DefaultMusicVolume = -21f;

    [SerializeField] private DisplaySettings _display;
    [SerializeField] private AudioMixer _mixer;

    [Tooltip("Closed by SAVE. RESET TO DEFAULT deliberately leaves the panel up, so the player can see what changed.")]
    [SerializeField] private TitleMenuController _menu;

    [Header("Rows")]
    [SerializeField] private TMP_Dropdown _resolution;
    [SerializeField] private TMP_Dropdown _windowMode;
    [SerializeField] private Slider _masterVolume;
    [SerializeField] private Slider _sfxVolume;
    [SerializeField] private Slider _musicVolume;
    [SerializeField] private Toggle _gamepadUi;
    [SerializeField] private Toggle _crtEffect;

    [Header("Keep-or-revert prompt")]
    [SerializeField] private GameObject _confirmPrompt;
    [SerializeField] private TextMeshProUGUI _confirmText;

    private void Awake()
    {
        if (_confirmPrompt != null)
            _confirmPrompt.SetActive(false);

        if (_masterVolume != null)
            _masterVolume.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (_sfxVolume != null)
            _sfxVolume.onValueChanged.AddListener(OnSfxVolumeChanged);
        if (_musicVolume != null)
            _musicVolume.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (_crtEffect != null)
            _crtEffect.onValueChanged.AddListener(OnCrtChanged);
        if (_gamepadUi != null)
            _gamepadUi.onValueChanged.AddListener(OnGamepadUiChanged);

        if (_display != null)
        {
            _display.ConfirmCountdown += OnConfirmCountdown;
            _display.ConfirmEnded += OnConfirmEnded;
        }
    }

    // Deliberately Start, not Awake. Applying the stored settings reaches RenderFeaturesManager and
    // CRTPresenter, whose statics are assigned in their own Awake, and Awake order between root
    // objects is arbitrary - so from Awake the call could land before either existed and be dropped.
    private void Start()
    {
        LoadStored();
    }

    private void OnDestroy()
    {
        if (_masterVolume != null)
            _masterVolume.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        if (_sfxVolume != null)
            _sfxVolume.onValueChanged.RemoveListener(OnSfxVolumeChanged);
        if (_musicVolume != null)
            _musicVolume.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        if (_crtEffect != null)
            _crtEffect.onValueChanged.RemoveListener(OnCrtChanged);
        if (_gamepadUi != null)
            _gamepadUi.onValueChanged.RemoveListener(OnGamepadUiChanged);
        if (_display != null)
        {
            _display.ConfirmCountdown -= OnConfirmCountdown;
            _display.ConfirmEnded -= OnConfirmEnded;
        }
    }

    /// <summary>
    /// Wired to the SETTINGS entry. This component lives on an always-active object so stored
    /// settings are applied at startup, so the rows are refreshed when the panel is opened instead.
    /// </summary>
    public void RefreshRows()
    {
        LoadStored();
        ShowDisplayRows();
    }

    /// <summary>Wired to SAVE.</summary>
    public void Save()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, SliderValue(_masterVolume, DefaultMasterVolume));
        PlayerPrefs.SetFloat(SfxVolumeKey, SliderValue(_sfxVolume, DefaultSfxVolume));
        PlayerPrefs.SetFloat(MusicVolumeKey, SliderValue(_musicVolume, DefaultMusicVolume));
        PlayerPrefs.SetInt(GamepadUiKey, IsOn(_gamepadUi) ? 1 : 0);
        PlayerPrefs.SetInt(CrtKey, IsOn(_crtEffect) ? 1 : 0);
        PlayerPrefs.Save();

        // The display mode is applied now but only stored once the player says they can see it.
        if (_display != null && _resolution != null)
            _display.ApplyAndAwaitConfirmation(_resolution.value, IsFullscreenChosen());

        if (_menu != null)
            _menu.CloseAll();
    }

    /// <summary>
    /// Wired to RESET TO DEFAULT. The panel stays open on purpose: the point of resetting is to see
    /// what everything went back to.
    /// </summary>
    public void ResetToDefault()
    {
        PlayerPrefs.DeleteKey(MasterVolumeKey);
        PlayerPrefs.DeleteKey(SfxVolumeKey);
        PlayerPrefs.DeleteKey(MusicVolumeKey);
        PlayerPrefs.DeleteKey(GamepadUiKey);
        PlayerPrefs.DeleteKey(CrtKey);
        PlayerPrefs.Save();

        if (_display != null)
            _display.ResetToDefault();

        LoadStored();
        ShowDisplayRows();
    }

    /// <summary>Wired to the KEEP button on the prompt.</summary>
    public void KeepDisplayMode()
    {
        if (_display != null)
            _display.KeepPending();
    }

    /// <summary>Wired to the UNDO button on the prompt.</summary>
    public void RevertDisplayMode()
    {
        if (_display != null)
            _display.RevertPending();
        ShowDisplayRows();
    }

    private void LoadStored()
    {
        SetSlider(_masterVolume, MasterMixerParam, MasterVolumeKey, DefaultMasterVolume);
        SetSlider(_sfxVolume, SfxMixerParam, SfxVolumeKey, DefaultSfxVolume);
        SetSlider(_musicVolume, MusicMixerParam, MusicVolumeKey, DefaultMusicVolume);

        bool gamepadUi = PlayerPrefs.GetInt(GamepadUiKey, 0) != 0;
        if (_gamepadUi != null)
            _gamepadUi.SetIsOnWithoutNotify(gamepadUi);
        ApplyGamepadUi(gamepadUi);

        bool crt = PlayerPrefs.GetInt(CrtKey, 0) != 0;
        if (_crtEffect != null)
            _crtEffect.SetIsOnWithoutNotify(crt);
        ApplyCrt(crt);
    }

    private void ShowDisplayRows()
    {
        if (_display == null)
            return;

        if (_resolution != null)
        {
            List<string> options = new List<string>();
            IList<Resolution> modes = _display.Modes;
            for (int i = 0; i < modes.Count; i++)
                options.Add(modes[i].width + " x " + modes[i].height);
            _resolution.ClearOptions();
            _resolution.AddOptions(options);
            _resolution.SetValueWithoutNotify(_display.CurrentModeIndex());
        }

        if (_windowMode != null)
        {
            List<string> options = new List<string>();
            options.Add("Fullscreen");
            options.Add("Windowed");
            _windowMode.ClearOptions();
            _windowMode.AddOptions(options);
            _windowMode.SetValueWithoutNotify(_display.IsFullscreen ? 0 : 1);
        }
    }

    private bool IsFullscreenChosen()
    {
        return _windowMode == null || _windowMode.value == 0;
    }

    private void OnConfirmCountdown(int secondsLeft)
    {
        if (_confirmPrompt != null)
            _confirmPrompt.SetActive(true);
        if (_confirmText != null)
            _confirmText.text = "KEEP THIS DISPLAY MODE?  REVERTING IN " + secondsLeft;
    }

    private void OnConfirmEnded()
    {
        if (_confirmPrompt != null)
            _confirmPrompt.SetActive(false);
        ShowDisplayRows();
    }

    private void OnMasterVolumeChanged(float value) { ApplyVolume(MasterMixerParam, value); }
    private void OnSfxVolumeChanged(float value) { ApplyVolume(SfxMixerParam, value); }
    private void OnMusicVolumeChanged(float value) { ApplyVolume(MusicMixerParam, value); }
    private void OnCrtChanged(bool value) { ApplyCrt(value); }
    private void OnGamepadUiChanged(bool value) { ApplyGamepadUi(value); }

    private void SetSlider(Slider slider, string mixerParam, string prefKey, float fallback)
    {
        float value = PlayerPrefs.GetFloat(prefKey, fallback);
        if (value < MinVolume || value > MaxVolume)
            value = fallback;
        if (slider != null)
        {
            slider.minValue = MinVolume;
            slider.maxValue = MaxVolume;
            slider.SetValueWithoutNotify(value);
        }
        ApplyVolume(mixerParam, value);
    }

    private void ApplyVolume(string mixerParam, float value)
    {
        if (_mixer == null)
            return;
        _mixer.SetFloat(mixerParam, Mathf.Clamp(value, MinVolume, MaxVolume));
    }

    // RenderFeaturesManager is the entry point, not the presenter. As well as drawing the effect it
    // switches the global volume and remembers the choice for scenes loaded later, then hands off to
    // the presenter itself when one is in the scene. Calling the presenter directly skips both.
    private static void ApplyCrt(bool enabled)
    {
        if (RenderFeaturesManager.instance != null)
            RenderFeaturesManager.instance.EnableRetroCRT(enabled);
        else if (CRTPresenter.Instance != null)
            CRTPresenter.Instance.SetEffectEnabled(enabled);
    }

    private static void ApplyGamepadUi(bool enabled)
    {
        if (GamepadUIManager.instance != null)
            GamepadUIManager.instance.SetGamepadUI(enabled);
    }

    private static bool IsOn(Toggle toggle)
    {
        return toggle != null && toggle.isOn;
    }

    private static float SliderValue(Slider slider, float fallback)
    {
        return slider != null ? slider.value : fallback;
    }
}
