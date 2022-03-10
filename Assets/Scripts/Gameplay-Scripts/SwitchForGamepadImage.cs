using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
//using UnityEngine.InputSystem.LowLevel;
//using UnityEngine.InputSystem.Users;

public class SwitchForGamepadImage : MonoBehaviour
{
    [SerializeField] Sprite KeyBoardSprite;
    [SerializeField] Sprite GamePadSprite;
    SpriteRenderer myRenderer;


    InputDevice m_LastUsedDevice;
    private void Awake()
    {
        //PlayerInput input = FindObjectOfType<PlayerInput>();
        myRenderer = this.GetComponent<SpriteRenderer>();
        //updateButtonImage(input.currentControlScheme);
        //InputSystem.onEvent += OnInputSystemEvent;
    }
    // Start is called before the first frame update
    void Start()
    {
        UpdateUIImage(GamepadUIManager.instance.gamepadUI);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnEnable()
    {
        //InputUser.onChange += onInputDeviceChange;
    }

    void OnDisable()
    {
        //InputUser.onChange -= onInputDeviceChange;
    }

    /*void onInputDeviceChange(InputUser user, InputUserChange change, InputDevice device)
    {
        if (change == InputUserChange.ControlSchemeChanged)
        {
            updateButtonImage(user.controlScheme.Value.name);
        }
    }*/
    // all of this was stolen from spiritworld's post here: https://forum.unity.com/threads/detect-most-recent-input-device-type.753206/
    void UpdateUIImage(bool gamepadUI)
    {
        //Debug.Log("Manufacturer description: " + schemeName);
        // assuming you have only 2 schemes: keyboard and gamepad
        //if (schemeName.Equals("GamePad"))
        if (gamepadUI)
        {
            myRenderer.sprite = GamePadSprite;
        }
        else
        {
            myRenderer.sprite = KeyBoardSprite;
        }
    }
    /*void OnInputSystemEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (m_LastUsedDevice == device)
            return;

        m_LastUsedDevice = device;
        updateButtonImage(device.displayName);
    }*/

}
