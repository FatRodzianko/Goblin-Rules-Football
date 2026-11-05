using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.InputSystem;

public class GetOutOfInputField : MonoBehaviour
{
    [SerializeField] InputField _inputField;
    [SerializeField] TextMeshProUGUI inputFieldTM;
    [SerializeField] TMP_InputField tmpInputField;
    // Use this for initialization
    void Start()
    {
        tmpInputField = this.GetComponent<TMP_InputField>();
        //InputManager.Controls.Player.Move.performed += ctx => SetMovement(ctx.ReadValue<Vector2>());
        //InputManager.Controls.Player.Move.canceled += ctx => ResetMovement();
        _uiNavHandler = _ => EscapeTextInputField();
        InputManager.Controls.TitleScreenUINavigation.UINav.performed += _uiNavHandler;
    }
    // Stored so OnDestroy can remove it from the shared Controls, which outlive this object.
    Action<InputAction.CallbackContext> _uiNavHandler;
    void OnDestroy()
    {
        if (_uiNavHandler != null)
            InputManager.Controls.TitleScreenUINavigation.UINav.performed -= _uiNavHandler;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonUp("Submit") || Input.GetButtonUp("Cancel"))
        {
            //_inputField.DeactivateInputField();
            try
            {
                tmpInputField.DeactivateInputField();
            }
            catch (Exception e)
            {
                Debug.Log("GetOutOfInputField.cs: could not access the tmpInputField object. Error: " + e);
            }
        }
    }
    private void FixedUpdate()
    {

    }
    void EscapeTextInputField()
    {
        Debug.Log("GetOutOfInputField.cs: EscapeTextInputField");
        try
        {
            tmpInputField.DeactivateInputField();
        }
        catch (Exception e)
        {
            Debug.Log("GetOutOfInputField.cs: could not access the tmpInputField object. Error: " + e);
        }
        
    }
    
}
