using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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
        InputManager.Controls.TitleScreenUINavigation.UINav.performed += _ => EscapeTextInputField();
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
