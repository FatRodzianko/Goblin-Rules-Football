using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManagerBombRun : MonoBehaviour
{
    public static InputManagerBombRun Instance { get; private set; }
    private BombRunControls _bombRunControls;

    private void Awake()
    {
        MakeInstance();

        _bombRunControls = new BombRunControls();
        _bombRunControls.UI.Enable();
    }
    void MakeInstance()
    {
        if (Instance != null)
        {
            Debug.Log("MakeInstance: more than one InputManagerBombRun. Destroying...");
            Destroy(this);
            return;
        }
        Instance = this;
    }
    public Vector2 GetMouseScreenPosition()
    {
        return Mouse.current.position.ReadValue();
    }
    public bool IsMouseButtonDownThisFrame()
    {
        return _bombRunControls.UI.Click.WasPerformedThisFrame();
    }
    public bool IsRightMouseButtonDownThisFrame()
    {
        return _bombRunControls.UI.RightClick.WasPerformedThisFrame();
    }
}
