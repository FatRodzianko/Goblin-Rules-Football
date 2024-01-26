using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using PathCreation;

public class PipeMiniGolfScript : MonoBehaviour
{
    [Header("Pipe Components")]
    //[SerializeField] public Collider2D PipeCollider;
    [SerializeField] public CircleCollider2D PipeCollider;
    [SerializeField] public bool IsEntryPipe;
    [SerializeField] public ScriptableObstacle myScriptableObject;
    [SerializeField] public PipeMiniGolfScript MyExitPipe;
    [SerializeField] float _myCircleRadius;
    [SerializeField] float _mySpriteRadius;

    [Header("Exit Pipe Stuff")]
    [SerializeField] public Vector3 ExitPipePosition;
    [SerializeField] public Vector3 ExitPipeDirection;
    [SerializeField] public GameObject ExitPointReferenceObject;
    [SerializeField] public Vector3 ExitPipeExitPoint =  Vector3.zero;
    [SerializeField] public bool ExitPipeIsOffset = false;

    [Header("Pipe Path Stuff")]
    //[SerializeField] public PathCreator MyPath;
    [SerializeField] public Transform WayPointHolder;
    [SerializeField] Vector3[] _wayPoints;
    [SerializeField] float _pathLength;


    [Header("Ball Entry State")]
    [SerializeField] float _ballSpeedOnEntry = 1.0f;
    [SerializeField] Vector2 _ballMovementDirectionOnEntry = Vector2.zero;

    [Header("Ball Exit State")]
    [SerializeField] float _ballExitSpeed = 0f;
    [SerializeField] float _differenceInStartAndEndSpeed;
    [SerializeField] float _ballAcceleration = 0.75f;
    [SerializeField] float _timeInTube = 0f;

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

    private void Awake()
    {
        
        
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //if (_moveBall)
        //{
        //    //_distanceTraveled += _moveBallSpeed * Time.deltaTime;
        //    float percentTraveled = _distanceTraveled / MyPath.path.length;
        //    _distanceTraveled += (_ballSpeedOnEntry + (_differenceInStartAndEndSpeed * percentTraveled)) * Time.deltaTime;
        //    _ballToMove.transform.position = MyPath.path.GetPointAtDistance(_distanceTraveled, EndOfPathInstruction.Stop);
        //}
    }
    public void SetExitPipe(PipeMiniGolfScript exitPipe)
    {
        if (!MyExitPipe && exitPipe)
            MyExitPipe = exitPipe;

        ExitPipePosition = exitPipe.transform.position;
        ExitPipeExitPoint = exitPipe.transform.GetChild(0).position;
        ExitPipeDirection = exitPipe.ExitPipeDirection;
        //_distanceTraveled = Vector2.Distance(this.transform.position, ExitPipePosition);
        //_ballDistanceToTravel = Vector2.Distance(this.transform.position, ExitPipePosition);
    }
    public void SetPipePathWayPoints(Vector3[] newWayPoints)
    {
        _ballDistanceToTravel = 0f;
        this._wayPoints = newWayPoints;
        for (int i = 0; i < newWayPoints.Length; i++)
        {
            //if (i == 0)
            //    continue;

            // get the distance between points and add t hem up
            if (i == 0)
            {
                _ballDistanceToTravel = Vector2.Distance(newWayPoints[i], this.transform.position);
            }
            else
                _ballDistanceToTravel += Vector2.Distance(newWayPoints[i], newWayPoints[i - 1]);

            // don't spawn a waypoint for the final point, since it will just be the pipe exit point
            if (i == newWayPoints.Length - 1)
                continue;

            GameObject newObject = new GameObject("waypoint");
            newObject.transform.position = newWayPoints[i];
            newObject.transform.parent = WayPointHolder;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!this.IsEntryPipe)
            return;
        if (collision.tag != "golfBall")
            return;
        if (this._moveBall)
            return;

        Debug.Log("PipeMiniGolfScript: OnTriggerEnter2D: This object's name is: " + this.name + " collisions name is: " + collision.transform.name) ;

        GolfBallTopDown golfBallScript = collision.GetComponent<GolfBallTopDown>();

        if (!golfBallScript.IsOwner)
            return;

        //golfBallScript.ResetBallInfo(false);
        //golfBallScript.EnableOrDisableBallSpriteRenderer(false);
        //golfBallScript.SetBallInTube(true);

        // save the ball's current speed and direction
        this._ballSpeedOnEntry = golfBallScript.speedMetersPerSecond;

        if (this._ballSpeedOnEntry > 2f)
        {
            Debug.Log("PipeMiniGolfScript: OnTriggerEnter2D: ball is moving too fast to go down hole. Bounce ball out of hole? Ball speed: " + this._ballSpeedOnEntry.ToString());
            //golfBallScript.BounceOutOfHole((0.0625f * 1.5f), (0.0625f * 3f));
            golfBallScript.BounceOutOfHole(this.PipeCollider.radius, this._mySpriteRadius);
            return;
        }
        this._ballToMove = golfBallScript;
        this._ballMovementDirectionOnEntry = golfBallScript.movementDirection;

        StartCoroutine(DelayBeforeMoveBall(golfBallScript,golfBallScript.TimeBeforeSinkInHole(golfBallScript.speedMetersPerSecond, _myCircleRadius)));
    }
    IEnumerator DelayBeforeMoveBall(GolfBallTopDown golfBallScript, float delayTime)
    {
        
        Debug.Log("DelayBeforeMoveBall: delay time: " + delayTime.ToString());

        //set ball movement so it moves toward the center of hole?
        golfBallScript.ChangeBallDirectionToCenterOfHole((this.transform.position - golfBallScript.transform.position).normalized);

        // Set the ball as in the tube
        golfBallScript.SetBallInTube(true);


        yield return new WaitForSeconds(delayTime);

        golfBallScript.ResetBallInfo(false);
        golfBallScript.EnableOrDisableBallSpriteRenderer(false);
        

        _ballExitSpeed = GetExitFinalVelocity(_ballSpeedOnEntry, this._ballAcceleration, this._ballDistanceToTravel);
        _differenceInStartAndEndSpeed = _ballExitSpeed - _ballSpeedOnEntry;
        _timeInTube = GetTimeToTravelInTube(_ballSpeedOnEntry, _ballExitSpeed, _ballAcceleration);

        //golfBallScript.transform.DOMove(ExitPipeExitPoint, _timeInTube).SetEase(Ease.InCubic).OnComplete(() => BallExitPipe(golfBallScript));
        golfBallScript.transform.DOPath(_wayPoints,_timeInTube,PathType.Linear,PathMode.TopDown2D).SetEase(Ease.InCubic).OnComplete(() => BallExitPipe(golfBallScript));
        _distanceTraveled = 0f;
        //_moveBall = true;
    }
    void BallExitPipe(GolfBallTopDown golfBallScript)
    {
        Debug.Log("BallExitPipe: " + this._ballSpeedOnEntry.ToString());
        golfBallScript.EnableOrDisableBallSpriteRenderer(true);
        golfBallScript.SetBallInTube(false);
        //golfBallScript.BallExitMiniGolfPipe(this.ExitPipeDirection, GetExitSpeed(this._ballSpeedOnEntry));
        golfBallScript.BallExitMiniGolfPipe(this.ExitPipeDirection, _ballExitSpeed);
        _moveBall = false;
    }
    float GetExitSpeed(float entrySpeed)
    {
        float exitSpeed = entrySpeed;

        exitSpeed += (0.1f * _ballDistanceToTravel);

        return exitSpeed;
    }
    float GetExitFinalVelocity(float initialVelocity, float acceleration, float distanceTraveled)
    {
        float finalVelocity = Mathf.Sqrt((Mathf.Pow(initialVelocity, 2)) + 2 * (acceleration * distanceTraveled));
        //Debug.Log("GetExitFinalVelocity: initial velocity: " + initialVelocity.ToString() + " acceleration: " + acceleration.ToString() + " distance traveled: " + distanceTraveled.ToString() + " final velocity: " + finalVelocity.ToString() + " path length: " + this.MyPath.path.length.ToString()) ;
        Debug.Log("GetExitFinalVelocity: initial velocity: " + initialVelocity.ToString() + " acceleration: " + acceleration.ToString() + " distance traveled: " + distanceTraveled.ToString() + " final velocity: " + finalVelocity.ToString() + " path length: " + _pathLength.ToString());
        return finalVelocity;
    }
    float GetTimeToTravelInTube(float initialVelocity, float finalVelocity, float acceleration)
    {
        float timeToTravel = (finalVelocity - initialVelocity) / acceleration;
        Debug.Log("GetTimeToTravelInTube: initial velocity: " + initialVelocity.ToString() + " final velocity: " + finalVelocity.ToString() + " acceleration: " + acceleration.ToString() + " time to travel: " + timeToTravel.ToString());
        return timeToTravel;
    }
}
