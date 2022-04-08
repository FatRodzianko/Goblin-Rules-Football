using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GetOutOfInputField : MonoBehaviour
{
    [SerializeField] InputField _inputField;
    [SerializeField] TextMeshProUGUI inputFieldTM;
    [SerializeField] TMP_InputField tmpInputField;
    // Use this for initialization
    void Start()
    {
        tmpInputField = this.GetComponent<TMP_InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonUp("Submit") || Input.GetButtonUp("Cancel"))
        {
            //_inputField.DeactivateInputField();
            tmpInputField.DeactivateInputField();
        }
    }
}
