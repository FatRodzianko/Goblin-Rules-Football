using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class ZoomInOutManager : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera _camera;
    [SerializeField] MapMakerBuilder _builder;
    [SerializeField] MapMakerGolfControls _playerInput;
    [SerializeField] CinemachinePixelPerfect _pixelPerfect;
    [SerializeField] float _defaultOrthoSize;
    [SerializeField] float _maxOrthSize = 225f;
    [SerializeField] float _scrollRate = 5f;
    // Start is called before the first frame update
    void Start()
    {
        _camera = GameObject.FindGameObjectWithTag("camera").GetComponent<CinemachineVirtualCamera>();
        _pixelPerfect = _camera.GetComponent<CinemachinePixelPerfect>();
        _defaultOrthoSize = _camera.m_Lens.OrthographicSize;
        _builder = MapMakerBuilder.GetInstance();
        _playerInput = _builder.PlayerInput;
        _playerInput.MapMaker.ZoomInOut.performed += ZoomInOut;
        _playerInput.MapMaker.ResetZoom.performed += ResetZoom;
    }
    private void OnDisable()
    {
        _playerInput.MapMaker.ZoomInOut.performed -= ZoomInOut;
        _playerInput.MapMaker.ResetZoom.performed -= ResetZoom;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    private void ZoomInOut(InputAction.CallbackContext ctx)
    {
        Vector2 scrollInput = (Vector2)ctx.ReadValueAsObject();
        Debug.Log("ZoomInOut: scrollInput: + " + scrollInput.ToString());

        float currentOrtho = _camera.m_Lens.OrthographicSize;
        float newOrth = currentOrtho + (_scrollRate * (-scrollInput.y / (Mathf.Abs(scrollInput.y))));

        if (newOrth > _defaultOrthoSize)
        {
            _pixelPerfect.enabled = false;
            if (newOrth > _maxOrthSize)
                newOrth = _maxOrthSize;
        }   
        else
        {
            newOrth = _defaultOrthoSize;
            _pixelPerfect.enabled = true;
        }

        _camera.m_Lens.OrthographicSize = newOrth;
    }
    private void ResetZoom(InputAction.CallbackContext ctx)
    {
        _camera.m_Lens.OrthographicSize = _defaultOrthoSize;
        _pixelPerfect.enabled = true;
    }
}
