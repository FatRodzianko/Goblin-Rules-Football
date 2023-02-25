using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterestingPointMarkers : MonoBehaviour
{
    [Header("Marker UI Objects")]
    [SerializeField] GameObject _holeMarker;
    [SerializeField] GameObject _tornadoMarker;

    [Header("Marker UI Position Values")]
    [SerializeField] float _maxX;
    [SerializeField] float _minX;
    [SerializeField] float _maxY;
    [SerializeField] float _minY;
    [SerializeField] float lengthToHeightRatio;
    [SerializeField] float heightToLengthRatio;
    [SerializeField] BoxCollider2D _collider;

    [Header("Objects To Track")]
    [SerializeField] GameObject _holeObject;
    [SerializeField] GameObject _tornadoObject;

    
    void Awake()
    {
        _holeMarker.SetActive(false);
        _tornadoMarker.SetActive(false);
        GetRatios();
    }
    private void Start()
    {
        WindManager.instance.TornadoChanged += UpdateForTornadoChanged;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void GetRatios()
    {
        float length = _maxX - _minX;
        float height = _maxY - _minY;
        lengthToHeightRatio = length / height;
        heightToLengthRatio = height / length;
    }
    public void UpdateMarkerPositions(Vector3 startPos)
    {
        //UpdateHoleMarkerPosition();
        UpdateMarkerPosition(_holeObject, _holeMarker, startPos);
        if (!WindManager.instance.IsThereATorndao)
        {
            _tornadoMarker.SetActive(false);
            _tornadoObject = null;
            return;
        }
        //UpdateTornadoMarkerPosition();
        UpdateMarkerPosition(_tornadoObject,_tornadoMarker, startPos);
    }
    void UpdateHoleMarkerPosition()
    {
        if (_tornadoObject == null)
            return;

        if (_holeObject.GetComponent<SpriteRenderer>().isVisible)
        {
            _holeObject.SetActive(false);
            return;
        }
    }
    void UpdateTornadoMarkerPosition()
    {
        if (_tornadoObject == null)
            return;

        if (_tornadoObject.GetComponent<SpriteRenderer>().isVisible)
        {
            _tornadoMarker.SetActive(false);
            return;
        }

        
    }
    void UpdateMarkerPosition(GameObject markerObject, GameObject markerIcon, Vector3 startPos)
    {
        if (markerObject == null || markerIcon == null)
            return;

        if (markerObject.GetComponent<SpriteRenderer>().isVisible)
        {
            markerIcon.SetActive(false);
            return;
        }

        Vector2 directionToObject = (markerObject.transform.position - startPos).normalized;
        Vector3 newPos = startPos + (Vector3)(directionToObject * 50f);

        Vector3 closestPoint = _collider.ClosestPoint(newPos);

        markerIcon.transform.position = new Vector3(closestPoint.x, closestPoint.y, markerIcon.transform.position.z);
        markerIcon.SetActive(true);
    }
    void UpdateForTornadoChanged(bool toradoExists)
    {
        if (toradoExists)
        {
            _tornadoObject = WindManager.instance.TornadoScript.gameObject;

        }
        else
        {
            _tornadoMarker.SetActive(false);
            _tornadoObject = null;
        }
    }
    public void GetHoleObjectForNewHole()
    {
        GameObject[] holes = GameObject.FindGameObjectsWithTag("golfHole");
        if (holes.Length > 0)
        {
            _holeObject = holes[0];
        }
    }

}
