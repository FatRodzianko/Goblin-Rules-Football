using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class BombRunCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _cinemachineVirtualCamera;

    private CinemachineTransposer cinemachineTransposer;

    private void Start()
    {
        CurrentSelectedUnitButtonScript.OnPlayerClickedCurrentSelectedUnitButton += CurrentSelectedUnitButtonScript_OnPlayerClickedCurrentSelectedUnitButton;
    }

    

    private void Update()
    {
        HandleCameraMovement();
    }
    private void HandleCameraMovement()
    {
        Vector2 inputMoveDir = InputManagerBombRun.Instance.GetCameraMoveVector();

        float moveSpeed = 25f;

        Vector3 moveVector = inputMoveDir.normalized;
        _cinemachineVirtualCamera.transform.position += moveVector * moveSpeed * Time.deltaTime;

    }
    private void CurrentSelectedUnitButtonScript_OnPlayerClickedCurrentSelectedUnitButton(object sender, Vector3 unitPosition)
    {
        _cinemachineVirtualCamera.transform.position = new Vector3(unitPosition.x, unitPosition.y, _cinemachineVirtualCamera.transform.position.z);
    }
}
