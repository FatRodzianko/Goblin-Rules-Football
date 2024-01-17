using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeMiniGolfScript : MonoBehaviour
{
    [Header("Pipe Components")]
    [SerializeField] public Collider2D PipeCollider;
    [SerializeField] public bool IsEntryPipe;
    [SerializeField] public ScriptableObstacle myScriptableObject;
    [SerializeField] public PipeMiniGolfScript MyExitPipe;

    [Header("Exit Pipe Stuff")]
    [SerializeField] public Vector3 ExitPipePosition;
    [SerializeField] public Vector3 ExitPipeDirection;
    [SerializeField] public GameObject ExitPointReferenceObject;
    [SerializeField] public Vector3 ExitPipeExitPoint =  Vector3.zero;
    [SerializeField] public bool ExitPipeIsOffset = false;


    [Header("Ball Entry State")]
    [SerializeField] float _ballSpeedOnEntry = 1.0f;
    [SerializeField] Vector2 _ballMovementDirectionOnEntry = Vector2.zero;

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
        
    }
    public void SetExitPipe(PipeMiniGolfScript exitPipe)
    {
        if (!MyExitPipe && exitPipe)
            MyExitPipe = exitPipe;

        ExitPipePosition = exitPipe.transform.position;
        ExitPipeExitPoint = exitPipe.transform.GetChild(0).position;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!this.IsEntryPipe)
            return;
        if (collision.tag != "golfBall")
            return;

        Debug.Log("PipeMiniGolfScript: OnTriggerEnter2D: This object's name is: " + this.name);

        GolfBallTopDown golfBallScript = collision.GetComponent<GolfBallTopDown>();

        if (!golfBallScript.IsOwner)
            return;
    }
}
