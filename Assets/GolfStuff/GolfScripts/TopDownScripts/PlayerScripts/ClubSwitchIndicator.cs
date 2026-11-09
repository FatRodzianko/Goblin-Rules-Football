using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The "clubs can be switched" prompt beside the club selection: an up arrow, the key bound to ChangeClub,
// and a down arrow. It flashes red when a switch is refused, which happens wherever the ground leaves only
// one legal club.
public class ClubSwitchIndicator : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private Image _upArrow;
    [SerializeField] private Image _downArrow;
    [SerializeField] private TextMeshProUGUI _keyText;

    [Header("Refused Flash")]
    [SerializeField] private Color _refusedColor = Color.red;
    [SerializeField] private float _flashDuration = 0.35f;

    private Color _upArrowColor = Color.white;
    private Color _downArrowColor = Color.white;
    private Color _keyTextColor = Color.white;
    private Coroutine _flashRoutine;

    // An arrow was clicked, +1 or -1. What a direction means is for whoever owns the clubs to decide;
    // this only reports which way was asked for.
    public event Action<int> SwitchRequested;

    private void Awake()
    {
        if (_upArrow)
            _upArrowColor = _upArrow.color;
        if (_downArrow)
            _downArrowColor = _downArrow.color;
        if (_keyText)
            _keyTextColor = _keyText.color;
    }
    private void OnDisable()
    {
        _flashRoutine = null;
        ApplyFlashColors(1f);
    }
    // Read fresh rather than cached: bindings are loaded from saved overrides at startup and can be
    // rebound from the pause menu mid-game.
    public void RefreshBinding()
    {
        if (!_keyText)
            return;

        _keyText.text = InputManagerGolf.GetBindingName(InputManagerGolf.Controls.AimingActions.ChangeClub);
    }
    public void RequestSwitch(int direction)
    {
        if (SwitchRequested != null)
            SwitchRequested(direction);
    }
    public void FlashRefused()
    {
        if (!isActiveAndEnabled)
            return;

        if (_flashRoutine != null)
            StopCoroutine(_flashRoutine);

        _flashRoutine = StartCoroutine(FlashRefusedRoutine());
    }
    IEnumerator FlashRefusedRoutine()
    {
        float elapsed = 0f;

        // Unscaled, so the flash still plays while the game is paused.
        while (elapsed < _flashDuration)
        {
            ApplyFlashColors(elapsed / _flashDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        ApplyFlashColors(1f);
        _flashRoutine = null;
    }
    void ApplyFlashColors(float backToNormal)
    {
        if (_upArrow)
            _upArrow.color = Color.Lerp(_refusedColor, _upArrowColor, backToNormal);
        if (_downArrow)
            _downArrow.color = Color.Lerp(_refusedColor, _downArrowColor, backToNormal);
        if (_keyText)
            _keyText.color = Color.Lerp(_refusedColor, _keyTextColor, backToNormal);
    }
}
