using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRadiusDrawer : MonoBehaviour
{
    [SerializeField] LineRenderer _lineRenderer;

    [Header("Inner Circle Components")]
    [SerializeField] GameObject _innerCircle;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void DrawRadius(float radius)
    {
        // set up the line stuff
        int steps = GetRingSegments(radius);
        SetLineThickness(radius);

        // update the inner circle
        UpdateInnerCircleSize(radius);
        Vector3 innerCircleOffset = _innerCircle.transform.localPosition;

        // start to draw the line
        _lineRenderer.positionCount = steps + 1;
        Vector3 myPos = this.transform.localPosition;
        for (int currentStep = 0; currentStep < steps; currentStep++)
        {
            float circumferenceProgress = (float)currentStep / steps;

            float currentRadian = circumferenceProgress * 2 * Mathf.PI;
            float xScaled = Mathf.Cos(currentRadian);
            float yScaled = Mathf.Sin(currentRadian);
            float x = xScaled * radius;
            float y = yScaled * radius;

            Vector3 currentPos = new Vector3(x, y, 0);
            //currentPos += myPos;


            _lineRenderer.SetPosition(currentStep, (currentPos + innerCircleOffset));
        }

        _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, _lineRenderer.GetPosition(0));
    }
    int GetRingSegments(float radius)
    {
        return (int)(radius * 10);
    }
    void SetLineThickness(float radius)
    {
        float lineWidth = radius * 0.03f;
        _lineRenderer.startWidth = lineWidth;
        _lineRenderer.endWidth = lineWidth;
    }
    void UpdateInnerCircleSize(float radius)
    {
        _innerCircle.transform.localScale = new Vector3(radius * 2, radius * 2, 1f);
    }
}
