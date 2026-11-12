using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class MoveCameraManager : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera _camera;
    [SerializeField] MapMakerBuilder _builder;
    [SerializeField] MapMakerGolfControls _playerInput;
    [SerializeField] Vector2 _previousInput;
    [SerializeField] float _movementSpeed = 5f;

    bool _draggingCamera = false;
    Vector2 _previousDragPointer = Vector2.zero;

    // Kept so OnDisable can remove the same delegate it added. A lambda built at the -= is a different object
    // and never matches the one that was subscribed.
    Action<InputAction.CallbackContext> _moveCameraPerformed;
    Action<InputAction.CallbackContext> _moveCameraCanceled;

    // Start is called before the first frame update
    void Start()
    {
        _camera = GameObject.FindGameObjectWithTag("camera").GetComponent<CinemachineVirtualCamera>();

        _builder = MapMakerBuilder.GetInstance();

        _playerInput = _builder.PlayerInput;
        _moveCameraPerformed = ctx => SetMovement(ctx.ReadValue<Vector2>());
        _moveCameraCanceled = ctx => ResetMovement();
        _playerInput.MapMaker.MoveCamera.performed += _moveCameraPerformed;
        _playerInput.MapMaker.MoveCamera.canceled += _moveCameraCanceled;

        _playerInput.MapMaker.MiddleMouseMove.started += StartDraggingCamera;
        _playerInput.MapMaker.MiddleMouseMove.canceled += StopDraggingCamera;
    }
    private void OnDisable()
    {
        // Start does the subscribing, so this runs with nothing to remove if the object is disabled before it.
        if (_playerInput == null)
            return;
        if (_moveCameraPerformed != null)
            _playerInput.MapMaker.MoveCamera.performed -= _moveCameraPerformed;
        if (_moveCameraCanceled != null)
            _playerInput.MapMaker.MoveCamera.canceled -= _moveCameraCanceled;

        _playerInput.MapMaker.MiddleMouseMove.started -= StartDraggingCamera;
        _playerInput.MapMaker.MiddleMouseMove.canceled -= StopDraggingCamera;
    }
    // Update is called once per frame
    void Update()
    {
        MoveCamera();
        DragCamera();
    }
    private void SetMovement(Vector2 movement) => _previousInput = movement;


    private void ResetMovement() => _previousInput = Vector2.zero;
    void MoveCamera()
    {
        if (_previousInput.x == 0 && _previousInput.y == 0)
            return;

        Vector2 direction = Vector2.ClampMagnitude(_previousInput, 1);
        _camera.transform.position += (Vector3)(direction * (_movementSpeed * (_camera.m_Lens.OrthographicSize / 5f)) * Time.deltaTime);
    }
    // Holding the middle mouse button drags the map under the cursor, the way a scene view does.
    void StartDraggingCamera(InputAction.CallbackContext context)
    {
        Vector2 pointer = PointerPosition();

        // Grabbing the tile palette or a panel should not drag the map out from under it
        if (MapMakerBuilder.IsPointerOverUI(pointer))
            return;

        _draggingCamera = true;
        _previousDragPointer = pointer;
    }
    void StopDraggingCamera(InputAction.CallbackContext context)
    {
        _draggingCamera = false;
    }
    void DragCamera()
    {
        if (!_draggingCamera)
            return;

        Vector2 pointer = PointerPosition();
        Vector2 pixelsMoved = pointer - _previousDragPointer;
        _previousDragPointer = pointer;

        if (pixelsMoved == Vector2.zero)
            return;

        // The screen's height covers twice the orthographic size, so this turns a drag in pixels
        // into the world distance underneath it. It stays correct at any zoom, with no sensitivity
        // to tune, and reads the lens rather than the rendered camera, which lags the virtual one
        // while Cinemachine blends.
        float worldUnitsPerPixel = (_camera.m_Lens.OrthographicSize * 2f) / Screen.height;

        // Whatever was grabbed stays under the cursor, so the camera travels against the drag.
        _camera.transform.position -= (Vector3)(pixelsMoved * worldUnitsPerPixel);
    }
    Vector2 PointerPosition()
    {
        return _playerInput.MapMaker.MousePosition.ReadValue<Vector2>();
    }
}
