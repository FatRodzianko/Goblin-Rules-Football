using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class RebindKeyScript : MonoBehaviour
{
    [SerializeField] InputActionReference keyAction = null;
    [SerializeField] private TMP_Text bindingDisplayNameText = null;
    [SerializeField] bool isKeyboardMouse = false;

    private const string KeyboardMouseRebindsKey = "KeyboardMouseRebindsKey";
    private const string GamepadMouseRebindsKey = "GamepadRebindsKey";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
