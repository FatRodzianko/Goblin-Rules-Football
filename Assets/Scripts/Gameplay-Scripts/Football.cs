using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;
public class Football : NetworkBehaviour
{
    [Header("State of the Ball")]
    [SyncVar(hook = nameof(HandleIsHeld))] public bool isHeld;
    [SyncVar] public bool isOnGround;
    [SyncVar(hook = nameof(HandleIsThrown))] public bool isThrown;
    [SyncVar] public bool isFalling;
    public GoblinScript goblinWithBall;

    [Header("Ball Components")]
    [SerializeField] private SpriteRenderer myRenderer;
    [SerializeField] private BoxCollider2D myCollider;
    [SerializeField] private Rigidbody2D myRigidBody;
    [SerializeField] public Animator animator;

    [Header("Throwing and Kicking Stuff?")]
    [SyncVar] public Vector2 directionToThrow = Vector2.zero;
    [SyncVar] public float speedOfThrow = 15f;
    [SyncVar] public float fallSpeed = 3f;
    [SyncVar] public float distanceFell;
    [SyncVar] public Vector2 startingPositionOfThrow;
    [SyncVar] public float distanceTraveled;

    [Header("Fumble Football Stuff")]
    Vector3[] FumblePoints = new Vector3[3];
    [SyncVar] public bool isFumbled = false;
    public float fumbleCount = 0.0f;

    [Header("Kicked Football")]
    Vector3[] KickedBallPoints = new Vector3[3];
    [SyncVar(hook = nameof(HandleIsKicked))] public bool isKicked = false;
    public float kickCount = 0.0f;
    public float kickCountModifier = 0.5f;
    public float minKickDist = 10f;
    public float maxKickDist = 40f;
    public float xDistanceOfKick = 0.0f;
    [SerializeField] GameObject footballShadowPrefab;
    GameObject footballShadowObject;
    [SerializeField] GameObject footballLandingTargetPrefab;
    GameObject footballLandingTargetObject;
    public float localKickCount = 0.0f;
    public float localKickCountModifier;
    Vector3 localShadowStartPosition = Vector3.zero;
    Vector3 localShadowEndPosition = Vector3.zero;

    [Header("Football Bounces")]
    [SyncVar] public bool isBouncing;
    [SyncVar] public int numberOfBounces;
    float bounceProgressCount = 0.0f;
    bool bounceForward = false;
    Vector3[] BouncingBallPoints = new Vector3[3];

    [SerializeField] public CinemachineVirtualCamera myCamera;
    public GamePlayer localPlayer;
    CameraMarker myCameraMarker;

    public override void OnStartClient()
    {
        base.OnStartClient();
        myRenderer.enabled = true;
        myCollider.enabled = true;

        myCamera = GameObject.FindGameObjectWithTag("camera").GetComponent<CinemachineVirtualCamera>();
        /*TrackFootballScript footballTracker = Camera.main.GetComponent<TrackFootballScript>();
        if (!footballTracker.myFootball)
        {
            footballTracker.myFootball = this.gameObject;
            footballTracker.myFootballScript = this;
            footballTracker.doesCameraHaveFootball = true;
        }*/

        myCameraMarker = Camera.main.GetComponent < CameraMarker > ();

        footballShadowObject = Instantiate(footballShadowPrefab, this.transform);
        footballShadowObject.SetActive(false);
        footballShadowObject.transform.localPosition = Vector3.zero;

        /*footballLandingTargetObject = Instantiate(footballLandingTargetPrefab, this.transform);
        footballLandingTargetObject.SetActive(false);
        footballLandingTargetObject.transform.localPosition = Vector3.zero;*/


    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void nothing()
    {
        /*if (isClient)
        {
            if (isThrown)
            {
                animator.SetBool("isThrown", isThrown);
                if (Mathf.Abs(directionToThrow.y) < 0.55f)
                    animator.SetBool("isSideways", true);
                else
                    animator.SetBool("isSideways", false);
            }
                
        }*/
    }
    private void Update()
    {
        /*Vector3 screenPoint = Camera.main.WorldToViewportPoint(this.transform.position);
        if (screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
        {
            Debug.Log("Football is on screen.");
        }
        else
            Debug.Log("Football is OFF screen.");*/

        Vector3 screenPoint = Camera.main.WorldToViewportPoint(this.transform.position);
        if (screenPoint.x < 0)
        {
            myCameraMarker.ActivateFootballMarker(true, isHeld,this.transform.position.y);
        }
        else if (screenPoint.x > 1)
        {
            myCameraMarker.ActivateFootballMarker(false, isHeld, this.transform.position.y);
        }
        else
        {
            myCameraMarker.DeActivateFootballMarker();
        }

        if (isClient)
        {
            if ((isKicked || isBouncing) && !isHeld)
            {
                if (localKickCount < 1.0f)
                {
                    localKickCount += localKickCountModifier * Time.deltaTime;
                    Vector3 localM1 = Vector3.Lerp(localShadowStartPosition, localShadowEndPosition, localKickCount);
                    footballShadowObject.transform.position = localM1;
                    if (localKickCount >= 1.0f)
                    {
                        footballShadowObject.SetActive(false);
                    }
                }
                else
                {
                    footballShadowObject.SetActive(false);
                }
            }
        }
    }
    private void FixedUpdate()
    {
        if (isServer)
        {
            if (isThrown)
            {
                if (transform.position.y < 7.02f && transform.position.y > -6.7f)
                {
                    myRigidBody.MovePosition(myRigidBody.position + directionToThrow * speedOfThrow * Time.fixedDeltaTime);
                    //transform.position += (directionToThrow * speedOfThrow * Time.fixedDeltaTime);
                    animator.SetBool("isThrown", isThrown);

                    if (Mathf.Abs(directionToThrow.y) < 0.55f)
                        animator.SetBool("isSideways", true);
                    else
                        animator.SetBool("isSideways", false);
                    //gameObject.transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.Abs(Vector2.Angle(new Vector2(1f, 0f), directionToThrow)+90f));
                    distanceTraveled = Vector2.Distance(transform.position, startingPositionOfThrow);
                    if (distanceTraveled > 10.0f)
                    {
                        isThrown = false;
                        animator.SetBool("isThrown", isThrown);
                        isFalling = true;
                        animator.SetBool("isFalling", isFalling);
                        //animator.SetBool("isSideways", false);
                        IEnumerator fallDown = FallDown();
                        StartCoroutine(fallDown);
                    }
                }
                else
                {
                    directionToThrow = Vector2.zero;
                    isThrown = false;
                    animator.SetBool("isThrown", isThrown);
                    if (transform.position.y >= 6.5f)
                        transform.position = new Vector2(transform.position.x, 5.6f);
                    else
                        transform.position = new Vector2(transform.position.x, -6.5f);
                }

            }
            if (isFalling && !isHeld)
            {
                if (transform.position.y < 7.02f && transform.position.y > -6.7f)
                {
                    Vector2 directionToFall = directionToThrow;
                    directionToFall.y -= (fallSpeed * Time.fixedDeltaTime);
                    myRigidBody.MovePosition(myRigidBody.position + directionToFall * speedOfThrow * Time.fixedDeltaTime);
                }
                else
                {
                    directionToThrow = Vector2.zero;
                    isThrown = false;
                    animator.SetBool("isThrown", isThrown);
                    isFalling = false;
                    animator.SetBool("isFalling", isFalling);
                    if (transform.position.y >= 6.5f)
                        transform.position = new Vector2(transform.position.x, 5.6f);
                    else
                        transform.position = new Vector2(transform.position.x, -6.5f);
                }

            }
            if (!isThrown && !isHeld && !isFalling)
            { 
                if(transform.position.y < -6.5f)
                    transform.position = new Vector2(transform.position.x, -6.5f);
                else if(transform.position.y > 5.6f)
                    transform.position = new Vector2(transform.position.x, 5.6f);
            }
            if (isFumbled && !isHeld)
            {
                if (fumbleCount < 1.0f)
                {
                    fumbleCount += 2.0f * Time.fixedDeltaTime;
                    Vector3 m1 = Vector3.Lerp(FumblePoints[0], FumblePoints[1], fumbleCount);
                    Vector3 m2 = Vector3.Lerp(FumblePoints[1], FumblePoints[2], fumbleCount);
                    this.transform.position = Vector3.Lerp(m1, m2, fumbleCount);
                    if (fumbleCount >= 1.0f)
                    {
                        isFumbled = false;
                        animator.SetBool("isFumbled", false);
                    }
                }
                else
                {
                    isFumbled = false;
                    animator.SetBool("isFumbled", false);
                }
                    

            }
            if (isKicked && !isHeld)
            {
                if (kickCount < 1.0f)
                {
                    kickCount += kickCountModifier * Time.fixedDeltaTime;
                    Vector3 m1 = Vector3.Lerp(KickedBallPoints[0], KickedBallPoints[1], kickCount);
                    Vector3 m2 = Vector3.Lerp(KickedBallPoints[1], KickedBallPoints[2], kickCount);
                    this.transform.position = Vector3.Lerp(m1, m2, kickCount);
                    if (kickCount >= 1.0f)
                    {
                        //isKicked = false;
                        HandleIsKicked(this.isKicked, false);
                        //animator.SetBool("isFumbled", false);
                        RpcActivateFootballShadow(false);
                    }
                }
                else
                {
                    //isKicked = false;
                    HandleIsKicked(this.isKicked, false);
                    //animator.SetBool("isFumbled", false);
                    RpcActivateFootballShadow(false);
                }
            }
            if (isBouncing && !isHeld)
            {
                if (bounceProgressCount < 1.0f)
                {
                    bounceProgressCount += 2.0f * Time.fixedDeltaTime;
                    Vector3 m1 = Vector3.Lerp(BouncingBallPoints[0], BouncingBallPoints[1], bounceProgressCount);
                    Vector3 m2 = Vector3.Lerp(BouncingBallPoints[1], BouncingBallPoints[2], bounceProgressCount);
                    this.transform.position = Vector3.Lerp(m1, m2, bounceProgressCount);
                    if (bounceProgressCount >= 1.0f)
                    {
                        BounceFootball(false);
                    }
                }
                else
                {
                    BounceFootball(false);
                }


            }

        }
        
    }
    public IEnumerator FallDown()
    {
        
        yield return new WaitForSeconds(0.2f);
        isFalling = false;
        animator.SetBool("isFalling", isFalling);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("OnTriggerEnter2D for football");
        if (collision.tag == "goblin-body")
        {
            Debug.Log("Football: collided with goblin-body of goblin named: " + collision.transform.parent.name);
            uint goblinNetId = collision.transform.parent.gameObject.GetComponent<NetworkIdentity>().netId;
            collision.transform.parent.gameObject.GetComponent<GoblinScript>().GoblinPickUpFootball();
            //CmdPlayerPickUpFootball(goblinNetId);
        }
    }
    /*[Command(requiresAuthority = false)]
    void CmdPlayerPickUpFootball(uint goblinNetId)
    {
        Debug.Log("CmdPlayerPickUpFootball");
        if (!isHeld)
        {
            GameObject goblinToCheck = NetworkIdentity.spawned[goblinNetId].gameObject;
            float distanceToBall = Vector3.Distance(goblinToCheck.transform.position, this.transform.position);
            Debug.Log("CmdPlayerPickUpFootball: distance to ball is: " + distanceToBall.ToString() + " from goblin with netid of " + goblinNetId.ToString());
            if (distanceToBall < 2.5f)
            {
                Debug.Log("CmdPlayerPickUpFootball: Goblin is close enough to pick up the ball");
                GoblinScript goblinToCheckScript = goblinToCheck.GetComponent<GoblinScript>();

                goblinToCheckScript.HandleHasBall(goblinToCheckScript.doesCharacterHaveBall, true);
                this.HandleIsHeld(this.isHeld, true);
                RpcBallIsHeld(true);
                
            }
            if (isThrown)
            {
                isThrown = false;                
                
                directionToThrow = Vector2.zero;
                animator.SetBool("isThrown", false);
                animator.SetBool("isSideways", false);
                
            }
            if (isFumbled)
            {
                isFumbled = false;
                animator.SetBool("isFumbled", false);
            }
                
        }
    }*/
    [Server]
    public void PlayerPickUpFootball(uint goblinNetId)
    {
        Debug.Log("PlayerPickUpFootball");
        if (!isHeld && !isKicked) // replace "isKicked" with something that makes it so the football can only be caught near the beginning and end of the kicking arc
        {
            GameObject goblinToCheck = NetworkIdentity.spawned[goblinNetId].gameObject;
            GoblinScript goblinToCheckScript = goblinToCheck.GetComponent<GoblinScript>();            
            
            if (!goblinToCheckScript.isGoblinKnockedOut && !goblinToCheckScript.isSliding && !goblinToCheckScript.isDiving && !goblinToCheckScript.isBlocking && !goblinToCheckScript.isPunching && !goblinToCheckScript.isKicking)
            {
                float distanceToBall = Vector3.Distance(goblinToCheck.transform.position, this.transform.position);
                Debug.Log("CmdPlayerPickUpFootball: distance to ball is: " + distanceToBall.ToString() + " from goblin with netid of " + goblinNetId.ToString());
                if (distanceToBall < 2.5f)
                {
                    Debug.Log("CmdPlayerPickUpFootball: Goblin is close enough to pick up the ball");
                    //GoblinScript goblinToCheckScript = goblinToCheck.GetComponent<GoblinScript>();

                    goblinToCheckScript.HandleHasBall(goblinToCheckScript.doesCharacterHaveBall, true);
                    this.HandleIsHeld(this.isHeld, true);
                    goblinWithBall = goblinToCheckScript;
                    RpcBallIsHeld(true);

                }
                if (isThrown)
                {
                    isThrown = false;

                    directionToThrow = Vector2.zero;
                    animator.SetBool("isThrown", false);
                    animator.SetBool("isSideways", false);

                }
                if (isFumbled)
                {
                    isFumbled = false;
                    animator.SetBool("isFumbled", false);
                }
                if (isKicked)
                {
                    //isKicked = false;
                    HandleIsKicked(this.isKicked, false);
                    animator.SetBool("isFumbled", false);
                }
                if (isBouncing)
                {
                    //isKicked = false;
                    isBouncing = false;
                    animator.SetBool("isFumbled", false);
                }
            }
            

        }
    }
    [ClientRpc]
    void RpcBallIsHeld(bool isBallHeld)
    {
        myRenderer.enabled = !isBallHeld;
        myCollider.enabled = !isBallHeld;
    }
    public void HandleIsHeld(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            isHeld = newValue;
            if (!newValue)
                goblinWithBall = null;
        }            
        if (isClient)
        {
            myRenderer.enabled = !newValue;
            myCollider.enabled = !newValue;
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdThrowFootball(uint throwingGoblinNetId, uint goblinToThrowToNetId)
    {
        GoblinScript throwingGoblinScript = NetworkIdentity.spawned[throwingGoblinNetId].GetComponent<GoblinScript>();
        GoblinScript goblinToThrowToScript = NetworkIdentity.spawned[goblinToThrowToNetId].GetComponent<GoblinScript>();

        if (throwingGoblinScript.doesCharacterHaveBall && !goblinToThrowToScript.doesCharacterHaveBall && !throwingGoblinScript.isKicking && !throwingGoblinScript.isDiving)
        {
            throwingGoblinScript.HandleHasBall(throwingGoblinScript.doesCharacterHaveBall, false);
            this.HandleIsHeld(isHeld, false);
            RpcBallIsHeld(false);

            Vector3 direction = (goblinToThrowToScript.transform.position - throwingGoblinScript.transform.position).normalized;
            startingPositionOfThrow = this.transform.position;
            Debug.Log("CmdThrowFootball: The direction of the throw is: " + direction.ToString());
            directionToThrow = direction;
            //float angletoThrow = Vector2.Angle(new Vector2(1f, 0f), direction);
            //Debug.Log("CmdThrowFootball: angle of the throw is " + angletoThrow.ToString());
            // this.transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, angletoThrow);
            if (transform.position.y >= 7.02f)
                transform.position = new Vector2(transform.position.x, 7.01f);


            //this.GetComponent<Rigidbody2D>().AddForce()
            this.isThrown = true;
            /*animator.SetBool("isThrown", isThrown);

            if (Mathf.Abs(directionToThrow.y) < 0.55f)
                animator.SetBool("isSideways", true);
            else
                animator.SetBool("isSideways", false);*/
        }
    }
    [Server]
    public void FumbleFootball()
    {
        Debug.Log("FumbleFootball");
        this.HandleIsHeld(isHeld, false);

        int directionModifier = Random.Range(0, 2) * 2 - 1;

        //Set fumble control points
        Vector3 footballPosition = transform.position;
        /*if (footballPosition.y > 5.6f)
        {
            footballPosition.y = 5.6f;
            this.transform.position = footballPosition;
        }*/

        FumblePoints[0] = footballPosition; // starting point
        Vector3 controlPoint = footballPosition;
        controlPoint.y += 1.25f;
        controlPoint.x -= (2.25f * directionModifier);
        FumblePoints[1] = controlPoint; // control point
        controlPoint.x -= (2.25f * directionModifier);
        controlPoint.y -= 1.25f;
        if (controlPoint.y > 5.6f)
            controlPoint.y = 5.6f;
        FumblePoints[2] = controlPoint; // destination point
        fumbleCount = 0.0f;
        isFumbled = true;
        animator.SetBool("isFumbled", true);

    }
    [Server]
    public void KickFootballDownField(bool isKickingGoblinGrey)
    {
        Debug.Log("KickFootballDownField with isKickingGoblinGrey: " + isKickingGoblinGrey.ToString());
        this.HandleIsHeld(isHeld, false);

        int directionToKickModifier = 1;
        if (isKickingGoblinGrey)
            directionToKickModifier = -1;

            
        // Starting position of the kick
        Vector3 footballPosition = transform.position;
        KickedBallPoints[0] = footballPosition;

        // Get destination of ball / where the ball lands
        float destinationY = Random.Range(-6.0f, 5.2f);
        float differenceInY = destinationY - footballPosition.y;
        float distanceTraveled = Random.Range(minKickDist, maxKickDist);
        xDistanceOfKick = distanceTraveled;
        float destinationX = footballPosition.x + (distanceTraveled * directionToKickModifier);
        KickedBallPoints[2] = new Vector3(destinationX, destinationY, footballPosition.z); // destination point

        // get direction of kick for bounching later
        if (destinationX > footballPosition.x)
            bounceForward = true;
        else
            bounceForward = false;

        // Get the "control point" or the height of the ball
        float controlX = ((destinationX - footballPosition.x ) / 2) + footballPosition.x;
        //float controlY = (distanceTraveled + (differenceInY)) / 2;
        float controlY = distanceTraveled + footballPosition.y;
        KickedBallPoints[1] = new Vector3(controlX, controlY, footballPosition.z);// control point

        kickCount = 0.0f;
        //kickCountModifier = 0.8f;

        if (distanceTraveled < 15f)
            kickCountModifier = 1.25f;
        else if (distanceTraveled < 20f)
            kickCountModifier = 1.05f;
        else if (distanceTraveled < 25f)
            kickCountModifier = 0.85f;
        else if (distanceTraveled < 30f)
            kickCountModifier = 0.725f;
        else if (distanceTraveled < 35f)
            kickCountModifier = 0.65f;
        else if (distanceTraveled < 40f)
            kickCountModifier = 0.6f;
        else
            kickCountModifier = 0.6f;


        //isKicked = true;
        HandleIsKicked(this.isKicked, true);
        animator.SetBool("isFumbled", true);
        RpcActivateFootballShadow(true);
        RpcShadowPosition(KickedBallPoints[0], KickedBallPoints[2], kickCountModifier);
    }
    void HandleIsThrown(bool oldValue, bool newValue)
    {
        if (isServer)
            isThrown = newValue;
        if (isClient)
        {
            if (!newValue)
            {
                if (myCamera.Follow == this.transform)
                {
                    localPlayer.FollowSelectedGoblin(localPlayer.selectGoblin.transform);
                }
            }
        }
    }
    /*private void OnBecameInvisible()
    {
        Debug.Log("Football is no longer visibile");

    }
    private void OnBecameVisible()
    {
        Debug.Log("Football IS NOW visibile");
    }*/
    void HandleIsKicked(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            isKicked = newValue;
            if (!newValue && !isHeld)
                BounceFootball(true);
        }        
        if (isClient)
        {
            if (!newValue)
            {
                myCollider.enabled = false;
                myCollider.enabled = true;
                if (footballLandingTargetObject)
                {
                    footballLandingTargetObject.SetActive(false);
                    GameObject destroyObject = footballLandingTargetObject;
                    Destroy(destroyObject);
                    footballLandingTargetObject = null;
                }
                if (footballShadowObject.activeInHierarchy)
                    footballShadowObject.SetActive(false);
            }
        }
    }
    [ClientRpc]
    void RpcActivateFootballShadow(bool shadow)
    {
        footballShadowObject.SetActive(shadow);
        if (!shadow)
        {
            
        }

        
    }
    [ClientRpc]
    void RpcShadowPosition(Vector3 startPosition, Vector3 endPosition, float kickCountModifier)
    {
        //footballShadowObject.transform.position = newRpcShadowPosition;
        localKickCount = 0.0f;
        localKickCountModifier = kickCountModifier;
        localShadowStartPosition = startPosition;
        localShadowEndPosition = endPosition;

        if (footballLandingTargetObject == null)
        {
            footballLandingTargetObject = Instantiate(footballLandingTargetPrefab);
            footballLandingTargetObject.transform.position = endPosition;
            footballLandingTargetObject.SetActive(true);
        }
        
    }
    [Server]
    void BounceFootball(bool firstBounce)
    {
        if (firstBounce)
        {
            numberOfBounces = 0;
            isBouncing = true;
        }
        else
        {
            numberOfBounces++;
        }

        bool anotherBounce = false;
        if (numberOfBounces == 0)
            anotherBounce = true;
        else
        {
            float rangeMin = (numberOfBounces * 15);
            if (Random.Range(rangeMin, 50) < xDistanceOfKick)
                anotherBounce = true;
            else
                anotherBounce = false;
        }


        if (anotherBounce)
        {
            int directionToBounce = 0;
            if (bounceForward)
                directionToBounce = 1;
            else
                directionToBounce = -1;

            BouncingBallPoints[0] = this.transform.position;
            Vector3 bounceControlPoint = BouncingBallPoints[0];
            float controlYDist = Random.Range(1.0f, 1.5f);
            bounceControlPoint.y += controlYDist;
            float controlXDist = (Random.Range(1.0f, 2.25f)) * directionToBounce;
            bounceControlPoint.x += controlXDist;
            BouncingBallPoints[1] = bounceControlPoint;

            BouncingBallPoints[2] = BouncingBallPoints[1];
            BouncingBallPoints[2].x += controlXDist;
            BouncingBallPoints[2].y = BouncingBallPoints[0].y;

            bounceProgressCount = 0.0f;
            animator.SetBool("isFumbled", true);
        }
        else
        {
            isBouncing = false;
            animator.SetBool("isFumbled", false);
        }
            
    }
}
