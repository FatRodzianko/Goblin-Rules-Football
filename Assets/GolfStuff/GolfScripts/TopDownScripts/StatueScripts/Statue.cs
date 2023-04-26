using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statue : MonoBehaviour
{

    [Header("Sprite Stuff")]
    [SerializeField] SpriteRenderer _myRenderer;
    [SerializeField] Sprite _mySprite;

    [Header("Statue Info")]
    [SerializeField] public string StatueType;
    [SerializeField] public float HeightInUnityUnits;
    [SerializeField] public float RingRadius;

    [Header("Collider Stuff")]
    [SerializeField] BoxCollider2D _myCollider;
    [SerializeField] CircleCollider2D _ringRadiusCollider;

    [Header("Ring Components")]
    [SerializeField] LineRenderer _myLineRenderer;
    [SerializeField] float _lineWidth;
    [SerializeField] int _lineSegments;

    [Header("Inner Circle Components")]
    [SerializeField] GameObject _innerCircle;

    // Start is called before the first frame update
    void Start()
    {
        RingRadius = GetStartingRadius();
        _lineSegments = GetRingSegments(RingRadius);
        SetLineThickness(RingRadius);
        DrawCircle(_lineSegments, RingRadius);
        UpdateInnerCircleSize(RingRadius);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    float GetStartingRadius()
    {
        return UnityEngine.Random.Range(3f, 10f);
    }
    int GetRingSegments(float radius)
    {
        return (int)(radius * 10);
    }
    void SetLineThickness(float radius)
    {
        _lineWidth = radius * 0.04f;
        _myLineRenderer.startWidth = _lineWidth;
        _myLineRenderer.endWidth = _lineWidth;
    }
    void UpdateInnerCircleSize(float radius)
    {
        _innerCircle.transform.localScale = new Vector3(radius * 2, radius * 2, 1f);
    }
    void DrawCircle(int steps, float radius)
    {

        _myLineRenderer.positionCount = steps + 1;
        Vector3 myPos = this.transform.position;
        for (int currentStep = 0; currentStep < steps; currentStep++)
        {
            float circumferenceProgress = (float)currentStep / steps;

            float currentRadian = circumferenceProgress * 2 * Mathf.PI;
            float xScaled = Mathf.Cos(currentRadian);
            float yScaled = Mathf.Sin(currentRadian);
            float x = xScaled * radius;
            float y = yScaled * radius;

            Vector3 currentPos = new Vector3(x, y, 0);
            currentPos += myPos;


            _myLineRenderer.SetPosition(currentStep, currentPos);
        }

        _myLineRenderer.SetPosition(_myLineRenderer.positionCount - 1, _myLineRenderer.GetPosition(0));

        
    }
}
