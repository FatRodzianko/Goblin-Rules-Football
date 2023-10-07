using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TubeScript : MonoBehaviour
{
    [SerializeField] public Collider2D HoleCollider;
    [SerializeField] public bool IsPrimaryTube;
    [SerializeField] public TubeScript CompanionTube;

    [Header("Ball Movement Stuff")]
    [SerializeField] bool _moveBall = false;
    [SerializeField] GolfBallTopDown _ballToMove;
    [SerializeField] float _moveBallSpeed = 1.0f;
    [SerializeField] Vector3 _ballStartingPoint = Vector3.zero;
    [SerializeField] Vector3 _ballDestination = Vector3.zero;
    [SerializeField] Vector2 _ballMovementDirection = Vector2.zero;
    [SerializeField] float _ballDistanceToTravel = 0f;
    [SerializeField] float _timeSinceMovementStarted = 0f;
    [SerializeField] float _maxTravelTime = 4.5f;
    float _movePercentage = 0f;
    float _distanceTraveled = 0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!_moveBall)
            return;

        _timeSinceMovementStarted += Time.fixedDeltaTime;

        if (_timeSinceMovementStarted >= _maxTravelTime)
        {
            BallDoneMoving();
            return;
        }

        _movePercentage = _timeSinceMovementStarted /_maxTravelTime;
        _distanceTraveled = _ballDistanceToTravel * _movePercentage;

        Vector3 newPoint = _ballStartingPoint + (Vector3)(_ballMovementDirection * _distanceTraveled);
        Debug.Log("TubeScript: newPoint is: " + newPoint.ToString() + " based on starting point: " + _ballStartingPoint.ToString() + " movement direction: " + _ballMovementDirection.ToString() + " distanced traveled: " + _distanceTraveled.ToString() + " out of: " + _ballDistanceToTravel.ToString());
        _ballToMove.transform.position = newPoint;

    }
    public void BallLandedInTubeHole(GolfBallTopDown golfBallScript)
    {
        if (!CompanionTube)
            return;
        if (_moveBall)
            return;
        Debug.Log("BallLandedInTubeHole: will send ball to companion tube at: " + CompanionTube.transform.position.ToString() + " and ball's current position is: " + golfBallScript.transform.position) ;

        if (!golfBallScript.IsOwner)
            return;

        // save ball position
        _ballToMove = golfBallScript;
        _ballStartingPoint = golfBallScript.transform.position;

        // Stop the ball from moving and turn off its sprite renderer
        golfBallScript.ResetBallInfo(false);
        golfBallScript.EnableOrDisableBallSpriteRenderer(false);
        golfBallScript.SetBallInTube(true);

        //Get Movement stuff?

        //_ballDestination = (CompanionTube.transform.position + new Vector3(0f, 0.25f, golfBallScript.transform.position.z)); // set the ball's destination to be the position of the middle of the tube sprite with the correct Z value for the ball height
        _ballDestination = (CompanionTube.transform.position + new Vector3(0f, 0.25f, golfBallScript.transform.position.z)); // set the ball's destination to be the position of the middle of the tube sprite with the correct Z value for the ball height
        _ballMovementDirection = (_ballDestination - _ballStartingPoint).normalized;
        _ballDistanceToTravel = Vector2.Distance(_ballStartingPoint, _ballDestination);
        Debug.Log("BallLandedInTubeHole: Starting point: " + _ballStartingPoint.ToString() + " destination pont: " + _ballDestination.ToString() + " direction: " + _ballMovementDirection.ToString() + " distance to travel: " + _ballDistanceToTravel);
        //Debug.Break();
        _timeSinceMovementStarted = 0f;
        //_moveBall = true;
        StartCoroutine(DelayBeforeMoveBall());
    }
    IEnumerator DelayBeforeMoveBall()
    {
        yield return new WaitForSeconds(1f);
        _moveBall = true;
    }
    void BallDoneMoving()
    {
        _moveBall = false;
        _ballToMove.transform.position = _ballDestination;
        // launch ball out of the tube?
        _ballToMove.MyPlayer.LaunchBallOutOfTube(this.IsPrimaryTube);
    }

}
