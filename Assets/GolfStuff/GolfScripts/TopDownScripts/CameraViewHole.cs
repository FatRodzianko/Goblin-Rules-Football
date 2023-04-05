using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraViewHole : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera _vCam;
    [SerializeField] CinemachinePixelPerfect _pixelPerfect;
    [SerializeField] CameraFollowScript _cameraFollowScript;
    [SerializeField] LineRenderer _outOfBoundsBorderLine;
    
    public bool IsCameraZoomedOut = false;
    [SerializeField] Vector3 _zoomedOutPos;
    [SerializeField] float _cameraZoomValue = 9f;

    private void Awake()
    {
        if (!_vCam)
            _vCam = this.GetComponent<CinemachineVirtualCamera>();
        if (!_pixelPerfect)
            _pixelPerfect = this.GetComponent<CinemachinePixelPerfect>();
        if (!_cameraFollowScript)
            _cameraFollowScript = this.GetComponent<CameraFollowScript>();
        _outOfBoundsBorderLine.enabled = false;
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
            //_vCam.m_Lens.OrthographicSize /= 9f;
            _vCam.m_Lens.OrthographicSize /= _cameraZoomValue;
            _pixelPerfect.enabled = true;
            _cameraFollowScript.enabled = true;
            //_outOfBoundsBorderLine.enabled = false;
            
        }
        else
        {
            //_vCam.m_Lens.OrthographicSize *= 9f;
            _vCam.m_Lens.OrthographicSize *= _cameraZoomValue;
            _pixelPerfect.enabled = false;
            _cameraFollowScript.enabled = false;
            _outOfBoundsBorderLine.enabled = true;
            _vCam.transform.position = new Vector3(_zoomedOutPos.x, _zoomedOutPos.y, _vCam.transform.position.z);
        }
        IsCameraZoomedOut = !IsCameraZoomedOut;
    }
    public void SetZoomedOutPosition(Vector3 newPos)
    {
        _zoomedOutPos = newPos;
    }
    public void SetCameraZoomValue(float newZoomValue)
    {
        _cameraZoomValue = newZoomValue;
    }
    //public void GetLinePointsForOutOfBoundsBorder(PolygonCollider2D border)
    public void GetLinePointsForOutOfBoundsBorder(Vector2[] polygonPoints)
    {
        Debug.Log("GetLinePointsForOutOfBoundsBorder");
        //https://gamedev.stackexchange.com/questions/197313/show-colliders-in-a-build-game-in-unity
        //var points = border.GetPath(0); // dumb assumption for demo -- only one path
        var points = polygonPoints;

        //Vector3[] positions = new Vector3[points.Length + 1];
        Vector3[] positions = new Vector3[points.Length + 1];
        for (int i = 0; i < points.Length; i++)
        {
            //positions[i] = transform.TransformPoint(points[i]);
            positions[i] = (Vector2)points[i];
        }
        positions[points.Length] = (Vector2)points[0];
        _outOfBoundsBorderLine.positionCount = points.Length + 1;
        _outOfBoundsBorderLine.SetPositions(positions);
        _outOfBoundsBorderLine.enabled = true;
    }
}
