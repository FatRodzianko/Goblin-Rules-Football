using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamepadUIManager : MonoBehaviour
{
    public static GamepadUIManager instance;
    public bool gamepadUI = false;
    // Start is called before the first frame update
    private void Awake()
    {
        MakeInstance();
        DontDestroyOnLoad(gameObject);
    }
    void MakeInstance()
    {
        Debug.Log("GamepadUIManager MakeInstance.");
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void UseGamepadChecked(bool isChecked)
    {
        Debug.Log("GamepadUIManager UseGamepadChecked: " + isChecked.ToString());
    }
}
