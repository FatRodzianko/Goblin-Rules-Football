using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BombRunCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _cinemachineVirtualCamera;

    private CinemachineTransposer cinemachineTransposer;


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
}
