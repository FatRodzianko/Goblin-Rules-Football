using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawTrajectory : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] [Range(3,30)] private int _lineSegmentCount = 20;
    public List<Vector3> _linePoints = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdateTrajectory(Vector2 forceVector, Rigidbody2D rigidBody, Vector2 startingPos)
    {
        
        Vector2 velocity = (forceVector / rigidBody.mass) * Time.fixedDeltaTime;
        Debug.Log("UpdateTrajectory: Velocity is: " + velocity.ToString());;

        float flightDuration = (2 * velocity.y) / Physics2D.gravity.y;

        float stepTime = flightDuration / _lineSegmentCount;

        _linePoints.Clear();

        for (int i = 0; i < _lineSegmentCount; i++)
        {
            float stepTimePassed = stepTime * i;

            Vector2 movementVector = new Vector2(velocity.x * stepTimePassed, velocity.y * stepTimePassed - 0.5f * Physics2D.gravity.y * stepTimePassed * stepTimePassed);

            _linePoints.Add(-movementVector + startingPos);
        }

        _lineRenderer.positionCount = _linePoints.Count;
        _lineRenderer.SetPositions(_linePoints.ToArray());
        Debug.Log("UpdateTrajectory: line points count: " + _linePoints.Count.ToString());
    }
}
