using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class InputManagerBombRun : MonoBehaviour
{
    public static InputManagerBombRun Instance { get; private set; }
    private BombRunControls _bombRunControls;

    [SerializeField] Vector2 _previousInput;

    private void Awake()
    {
        MakeInstance();

        _bombRunControls = new BombRunControls();
        _bombRunControls.PlayerActions.Enable();
        //_bombRunControls.PlayerActions.MoveCamera.Enable();
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
    private void Start()
    {
        _bombRunControls.PlayerActions.MoveCamera.performed += ctx => SetMoveCamerament(ctx.ReadValue<Vector2>());
        _bombRunControls.PlayerActions.MoveCamera.canceled += ctx => ResetMoveCamerament();
    }
    private void Update()
    {

    }
    public Vector2 GetMouseScreenPosition()
    {
        return Mouse.current.position.ReadValue();
    }
    public bool IsMouseButtonDownThisFrame()
    {
        return _bombRunControls.PlayerActions.Click.WasPerformedThisFrame();
    }
    public bool IsRightMouseButtonDownThisFrame()
    {
        return _bombRunControls.PlayerActions.RightClick.WasPerformedThisFrame();
    }
    public Vector2 GetCameraMoveVector()
    {
        //if (_previousInput.x == 0 && _previousInput.y == 0)
        //    return Vector2.zero;

        //Vector2 direction = Vector2.ClampMagnitude(_previousInput, 1);
        //return direction;
        return _bombRunControls.PlayerActions.MoveCamera.ReadValue<Vector2>();
    }

    private void SetMoveCamerament(Vector2 MoveCamerament) => _previousInput = MoveCamerament;


    private void ResetMoveCamerament() => _previousInput = Vector2.zero;
}
