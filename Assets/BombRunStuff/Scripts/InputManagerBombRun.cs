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
        _bombRunControls.UI.Enable();
        _bombRunControls.UI.Navigate.Disable();
        _bombRunControls.CameraMovement.Enable();
        _bombRunControls.CameraMovement.Move.Enable();
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
        _bombRunControls.CameraMovement.Move.performed += ctx => SetMovement(ctx.ReadValue<Vector2>());
        _bombRunControls.CameraMovement.Move.canceled += ctx => ResetMovement();
    }
    private void Update()
    {
        //MoveCamera();
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
    //public Vector2 GetCameraMoveVector()
    //{
    //    if (_previousCameraMovement.x == 0 && _previousCameraMovement.y == 0)
    //        return Vector2.zero;

    //    Vector2 direction = Vector2.ClampMagnitude(_previousCameraMovement, 1);
    //    return direction;
    //}
    public Vector2 GetCameraMoveVector()
    {
        if (_previousInput.x == 0 && _previousInput.y == 0)
            return Vector2.zero;

        Vector2 direction = Vector2.ClampMagnitude(_previousInput, 1);
        return direction;
    }

    private void SetMovement(Vector2 movement) => _previousInput = movement;


    private void ResetMovement() => _previousInput = Vector2.zero;
    //void MoveCamera()
    //{
    //    if (_previousInput.x == 0 && _previousInput.y == 0)
    //        return;

    //    Vector2 direction = Vector2.ClampMagnitude(_previousInput, 1);
    //    _camera.transform.position += (Vector3)(direction * (_movementSpeed * (_camera.m_Lens.OrthographicSize / 5f)) * Time.deltaTime);
    //}
}
