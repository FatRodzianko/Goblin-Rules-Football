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

    // Start is called before the first frame update
    void Start()
    {
        _camera = GameObject.FindGameObjectWithTag("camera").GetComponent<CinemachineVirtualCamera>();

        _builder = MapMakerBuilder.GetInstance();

        _playerInput = _builder.PlayerInput;
        _playerInput.MapMaker.MoveCamera.performed += ctx => SetMovement(ctx.ReadValue<Vector2>());
        _playerInput.MapMaker.MoveCamera.canceled += ctx => ResetMovement();

    }
    private void OnDisable()
    {
        _playerInput.MapMaker.MoveCamera.performed -= ctx => SetMovement(ctx.ReadValue<Vector2>());
        _playerInput.MapMaker.MoveCamera.canceled -= ctx => ResetMovement();
    }
    // Update is called once per frame
    void Update()
    {
        MoveCamera();
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
}
