using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraViewHole : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera _vCam;
    [SerializeField] CinemachinePixelPerfect _pixelPerfect;
    [SerializeField] CameraFollowScript _cameraFollowScript;
    
    public bool IsCameraZoomedOut = false;
    [SerializeField] Vector3 _zoomedOutPos;

    private void Awake()
    {
        if (!_vCam)
            _vCam = this.GetComponent<CinemachineVirtualCamera>();
        if (!_pixelPerfect)
            _pixelPerfect = this.GetComponent<CinemachinePixelPerfect>();
        if (!_cameraFollowScript)
            _cameraFollowScript = this.GetComponent<CameraFollowScript>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ZoomOutCamera()
    {
        Debug.Log("ZoomOutCamera: IsCameraZoomedOut: " + IsCameraZoomedOut.ToString());
        if (IsCameraZoomedOut)
        {
            _vCam.m_Lens.OrthographicSize /= 9f;
            _pixelPerfect.enabled = true;
            _cameraFollowScript.enabled = true;
            
            
        }
        else
        {
            _vCam.m_Lens.OrthographicSize *= 9f;
            _pixelPerfect.enabled = false;
            _cameraFollowScript.enabled = false;
            _vCam.transform.position = new Vector3(_zoomedOutPos.x, _zoomedOutPos.y, _vCam.transform.position.z);
        }
        IsCameraZoomedOut = !IsCameraZoomedOut;
    }
    public void SetZoomedOutPosition(Vector3 newPos)
    {
        _zoomedOutPos = newPos;
    }
}
