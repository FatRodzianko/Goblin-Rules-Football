using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;
using UnityEngine.SceneManagement;
public class Football : NetworkBehaviour
{
    [Header("State of the Ball")]
    [SyncVar(hook = nameof(HandleIsHeld))] public bool isHeld;
    [SyncVar] public bool isOnGround;
    [SyncVar(hook = nameof(HandleIsThrown))] public bool isThrown;
    [SyncVar] public bool isFalling;
    public GoblinScript goblinWithBall;
    [SyncVar(hook = nameof(HandleGoblinWithBallNetId))] public uint goblinWithBallNetId;
    public Team teamWithBallLast;

    [Header("Ball Components")]
    [SerializeField] private SpriteRenderer myRenderer;
    [SerializeField] private BoxCollider2D myCollider;
    [SerializeField] private Rigidbody2D myRigidBody;
    [SerializeField] public Animator animator;
    [SerializeField] private GameObject ballMarkerPrefab;
    private GameObject ballMarkerObject;

    [Header("Throwing and Kicking Stuff?")]
    [SyncVar] public Vector2 _directionToThrow = Vector2.zero;
    [SerializeField] [SyncVar] public float speedOfThrow; //  = 15 f;
    [SerializeField] float maxThrowDistance; // 10. 0f
    [SyncVar] public float fallSpeed = 3f;
    [SyncVar] public float distanceFell;
    [SyncVar] public Vector2 startingPositionOfThrow;
    [SyncVar] public float distanceTraveled;

    [Header("Fumble Football Stuff")]
    Vector3[] FumblePoints = new Vector3[3];
    [SyncVar(hook = nameof(HandleIsFumbled))] public bool isFumbled = false;
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
    [SyncVar(hook = nameof(HandleIsBouncing))] public bool isBouncing;
    [SyncVar] public int numberOfBounces;
    float bounceProgressCount = 0.0f;
    bool bounceForward = false;
    Vector3[] BouncingBallPoints = new Vector3[3];

    [Header("Kick After Attempt")]
    public bool isKickAfterAttemptGood;
    bool hasKickAfterAttemptBeenSubmittedToGameManager;

    [SerializeField] public CinemachineVirtualCamera myCamera;
    public GamePlayer localPlayer;
    public CameraMarker myCameraMarker;

    [Header("Field Max Parameters")]
    [SerializeField] float ThrownMaxY; // 7. 02f // 8. 5f
    [SerializeField] float ThrownMinY; // -6. 7f // -11. 07f
    [SerializeField] float ThrownMaxX; // 44. 4f // 59. 44f
    [SerializeField] float ThrownMinX; // -44. 5f // -59. 5f
    [SerializeField] float GroundedMaxY; // 5. 6f // 7. 1f
    [SerializeField] float GroundedMinY; // -6. 5f // -11. 0f
    [SerializeField] public Vector3 GoalPostForGrey; // GoalPostForGrey; // -50.3, -1.5
    [SerializeField] public Vector3 GoalPostForGreen; // GoalPostForGreen; // 50.3 , -1.5
    [SerializeField] float KickFootballMaxY; // 5. 2f // 6. 7f
    [SerializeField] float KickFootballMinY; // -6. 0f // -10. 5f

    private NetworkManagerGRF game;
    private NetworkManagerGRF Game
    {
        get
        {
            if (game != null)
            {
                return game;
            }
            return game = NetworkManagerGRF.singleton as NetworkManagerGRF;
        }
    }

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

        footballShadowObject = Instantiate(footballShadowPrefab);
        footballShadowObject.SetActive(false);
        footballShadowObject.transform.localPosition = Vector3.zero;

        /*footballLandingTargetObject = Instantiate(footballLandingTargetPrefab, this.transform);
        footballLandingTargetObject.SetActive(false);
        footballLandingTargetObject.transform.localPosition = Vector3.zero;*/
        try
        {
            GamePlayer localPlayer = GameObject.FindGameObjectWithTag("LocalGamePlayer").GetComponent<GamePlayer>();
            localPlayer.ReportPlayerSpawnedFootball();
        }
        catch
        {
            Debug.Log("Football.cs: Could not find local game player object");
        }
        ballMarkerObject = Instantiate(ballMarkerPrefab, this.transform);
        ballMarkerObject.SetActive(false);
        ballMarkerObject.transform.localPosition = new Vector3(0.08f, 1f, 0f);

    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        PowerUpManager.instance.GetFootballObject(this);
        RandomEventManager.instance.GetFootballObject(this);
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
            try
            {
                myCameraMarker.DeActivateFootballMarker();
            }
            catch
            {
                Debug.Log("Football.cs: couldn't run camera thing? myCameraMarker.DeActivateFootballMarker();");
            }
            
        }

        /*if (isClient)
        {
            if (isKicked && !isHeld)
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
        }*/
    }
    private void FixedUpdate()
    {
        if (isServer)
        {
            if (isThrown)
            {
                if (transform.position.y < ThrownMaxY && transform.position.y > ThrownMinY && transform.position.x > ThrownMinX && transform.position.x < ThrownMaxX)
                {
                    myRigidBody.MovePosition(myRigidBody.position + _directionToThrow * speedOfThrow * Time.fixedDeltaTime);
                    //transform.position += (directionToThrow * speedOfThrow * Time.fixedDeltaTime);
                    animator.SetBool("isThrown", isThrown);

                    if (Mathf.Abs(_directionToThrow.y) < 0.55f)
                        animator.SetBool("isSideways", true);
                    else
                        animator.SetBool("isSideways", false);
                    //gameObject.transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.Abs(Vector2.Angle(new Vector2(1f, 0f), directionToThrow)+90f));
                    distanceTraveled = Vector2.Distance(transform.position, startingPositionOfThrow);
                    if (distanceTraveled > maxThrowDistance)
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
                    _directionToThrow = Vector2.zero;
                    isThrown = false;
                    animator.SetBool("isThrown", isThrown);
                    if (transform.position.y >= (GroundedMaxY - 0.52f))
                        transform.position = new Vector2(transform.position.x, GroundedMaxY);
                    else if (transform.position.y <= GroundedMinY)
                        transform.position = new Vector2(transform.position.x, GroundedMinY);

                    if (transform.position.x < ThrownMinX)
                        transform.position = new Vector2(ThrownMinX, transform.position.y);
                    else if (transform.position.x > ThrownMaxX)
                        transform.position = new Vector2(ThrownMaxX, transform.position.y);
                }

            }
            if (isFalling && !isHeld)
            {
                if (transform.position.y < ThrownMaxY && transform.position.y > ThrownMinY && transform.position.x > ThrownMinX && transform.position.x < ThrownMaxX)
                {
                    Vector2 directionToFall = _directionToThrow;
                    directionToFall.y -= (fallSpeed * Time.fixedDeltaTime);
                    myRigidBody.MovePosition(myRigidBody.position + directionToFall * speedOfThrow * Time.fixedDeltaTime);
                }
                else
                {
                    _directionToThrow = Vector2.zero;
                    isThrown = false;
                    animator.SetBool("isThrown", isThrown);
                    isFalling = false;
                    animator.SetBool("isFalling", isFalling);
                    if (transform.position.y >= (GroundedMaxY - 0.52f))
                        transform.position = new Vector2(transform.position.x, GroundedMaxY);
                    else if(transform.position.y <= GroundedMinY)
                        transform.position = new Vector2(transform.position.x, GroundedMinY);

                    if (transform.position.x < ThrownMinX)
                        transform.position = new Vector2(ThrownMinX, transform.position.y);
                    else if (transform.position.x > ThrownMaxX)
                        transform.position = new Vector2(ThrownMaxX, transform.position.y);
                }

            }
            if (!isThrown && !isHeld && !isFalling && GameplayManager.instance.gamePhase != "kick-after-attempt")
            { 
                if(transform.position.y < GroundedMinY)
                    transform.position = new Vector2(transform.position.x, GroundedMinY);
                else if(transform.position.y > GroundedMaxY)
                    transform.position = new Vector2(transform.position.x, GroundedMaxY);

                if (transform.position.x < ThrownMinX)
                    transform.position = new Vector2(ThrownMinX, transform.position.y);
                else if (transform.position.x > ThrownMaxX)
                    transform.position = new Vector2(ThrownMaxX, transform.position.y);
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
                        RpcActivateFootballShadow(false, this.transform.position);
                        // Send gameplay manager a notification if the kick came up short
                        if (GameplayManager.instance.gamePhase == "kick-after-attempt" && !hasKickAfterAttemptBeenSubmittedToGameManager)
                        {
                            float xPosition = transform.position.x;
                            if ((xPosition < 0 && xPosition > GoalPostForGrey.x) || (xPosition > 0 && xPosition < GoalPostForGreen.x))
                            {
                                Debug.Log("Football.cs: Football has crossed the goalpost. x position of: " + xPosition.ToString());
                                GameplayManager.instance.KickAfterWasKickGoodOrBad(isKickAfterAttemptGood);
                                hasKickAfterAttemptBeenSubmittedToGameManager = true;
                            }
                        }
                    }
                    // If this is a kick after attempt, check if the kick has crossed the goal posts
                    if (GameplayManager.instance.gamePhase == "kick-after-attempt" && !hasKickAfterAttemptBeenSubmittedToGameManager)
                    {
                        float xPosition = transform.position.x;
                        if ((xPosition < 0 && xPosition < GoalPostForGrey.x) || (xPosition > 0 && xPosition > GoalPostForGreen.x))
                        {
                            Debug.Log("Football.cs: Football has crossed the goalpost. x position of: " + xPosition.ToString());
                            GameplayManager.instance.KickAfterWasKickGoodOrBad(isKickAfterAttemptGood);
                            hasKickAfterAttemptBeenSubmittedToGameManager = true;
                        }
                    }
                }
                else
                {
                    //isKicked = false;
                    HandleIsKicked(this.isKicked, false);
                    //animator.SetBool("isFumbled", false);
                    RpcActivateFootballShadow(false, this.transform.position);
                    // Send gameplay manager a notification if the kick came up short
                    if (GameplayManager.instance.gamePhase == "kick-after-attempt" && !hasKickAfterAttemptBeenSubmittedToGameManager)
                    {
                        float xPosition = transform.position.x;
                        if ((xPosition < 0 && xPosition > GoalPostForGrey.x) || (xPosition > 0 && xPosition < GoalPostForGreen.x))
                        {
                            Debug.Log("Football.cs: Football has crossed the goalpost. x position of: " + xPosition.ToString());
                            GameplayManager.instance.KickAfterWasKickGoodOrBad(isKickAfterAttemptGood);
                            hasKickAfterAttemptBeenSubmittedToGameManager = true;
                        }
                    }
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
            if (GameplayManager.instance.gamePhase == "xtra-time" && !isHeld && !isThrown && !isFumbled && !isFalling && !isKicked)
            {
                // have this start a counter for like 0.5f to allow for players to "catch" kicked balls before the game ends?
                //GameplayManager.instance.HandleGamePhase(GameplayManager.instance.gamePhase, "gameover");
                IEnumerator countdownToEndGame = CountdownToEndGame();
                StartCoroutine(countdownToEndGame);
            }

        }
        if (isClient)
        {
            if (isKicked && !isHeld)
            {
                if (localKickCount < 1.0f)
                {
                    localKickCount += localKickCountModifier * Time.fixedDeltaTime;
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
        if (isServer && collision.tag == "tripObject")
        {
            if (!isThrown && !isKicked && !isHeld && !isFumbled && (GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time"))
            {
                Debug.Log("Football.cs: Football collided with a trip object: " + collision.gameObject.name);
                if (isBouncing)
                    numberOfBounces--;
                else
                    FumbleFootball();
            }
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
        Debug.Log("PlayerPickUpFootball: isHeld: " + this.isHeld.ToString() + " isKicked: " + this.isKicked.ToString());
        if (!isHeld && !isKicked) // replace "isKicked" with something that makes it so the football can only be caught near the beginning and end of the kicking arc
        {
            GameObject goblinToCheck = NetworkIdentity.spawned[goblinNetId].gameObject;
            GoblinScript goblinToCheckScript = goblinToCheck.GetComponent<GoblinScript>();            
            
            if (!goblinToCheckScript.isGoblinKnockedOut && !goblinToCheckScript.isSliding && !goblinToCheckScript.isDiving && !goblinToCheckScript.isBlocking && !goblinToCheckScript.isPunching && !goblinToCheckScript.isKicking)
            {
                float distanceToBall = Vector3.Distance(goblinToCheck.transform.position, this.transform.position);
                /*if (GameplayManager.instance.gamePhase == "kickoff" && goblinToCheckScript.isGoblinGrey && goblinToCheckScript.name.Contains("grenadier"))
                {
                    Debug.Log("PlayerPickUpFootball: Grey team kicking football?");
                    distanceToBall = 0f;
                }*/
                if (GameplayManager.instance.gamePhase == "kickoff" && goblinToCheckScript.ownerConnectionId == GameplayManager.instance.kickingPlayer.ConnectionId && goblinToCheckScript.name.Contains("grenadier"))
                {
                    Debug.Log("PlayerPickUpFootball: kicking team is kicking football?");
                    distanceToBall = 0f;
                }
                if ((GameplayManager.instance.gamePhase == "touchdown-transition" || GameplayManager.instance.gamePhase == "kick-after-attempt") && goblinToCheckScript.isKickAfterGoblin)
                {
                    Debug.Log("PlayerPickUpFootball: Need to return football to scoring goblin for kick after?");
                    distanceToBall = 0f;
                }
                Debug.Log("PlayerPickUpFootball: distance to ball is: " + distanceToBall.ToString() + " from goblin with netid of " + goblinNetId.ToString());
                if (distanceToBall < 4.1f)
                {
                    Debug.Log("PlayerPickUpFootball: Goblin is close enough to pick up the ball");
                    //GoblinScript goblinToCheckScript = goblinToCheck.GetComponent<GoblinScript>();

                    goblinToCheckScript.HandleHasBall(goblinToCheckScript.doesCharacterHaveBall, true);
                    this.HandleIsHeld(this.isHeld, true);
                    goblinWithBall = goblinToCheckScript;
                    //goblinWithBallNetId = goblinToCheckScript.GetComponent<NetworkIdentity>().netId;
                    this.HandleGoblinWithBallNetId(goblinWithBallNetId, goblinToCheckScript.GetComponent<NetworkIdentity>().netId);
                    RpcBallIsHeld(true);

                    if (teamWithBallLast == null)
                    {
                        if (goblinToCheckScript.isGoblinGrey)
                            teamWithBallLast = TeamManager.instance.greyTeam;
                        else
                            teamWithBallLast = TeamManager.instance.greenTeam;
                        RpcChangePossessionSFX(goblinToCheckScript.isGoblinGrey);
                    }
                    if (teamWithBallLast.isGrey != goblinToCheckScript.isGoblinGrey)
                    {
                        Debug.Log("PlayerPickUpFootball: team that previously had the ball was grey? " + teamWithBallLast.isGrey.ToString() + " and the goblin picking up the ball is grey? " + goblinToCheckScript.isGoblinGrey.ToString());
                        if (goblinToCheckScript.isGoblinGrey)
                            teamWithBallLast = TeamManager.instance.greyTeam;
                        else
                            teamWithBallLast = TeamManager.instance.greenTeam;
                        RpcChangePossessionSFX(goblinToCheckScript.isGoblinGrey);
                    }
                }
                if (isThrown)
                {
                    isThrown = false;

                    _directionToThrow = Vector2.zero;
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
            {
                goblinWithBall = null;
                goblinWithBallNetId = 0;
                //GameplayManager.instance.RpcNoTeamWithFootball();
                /*foreach (GamePlayer player in Game.GamePlayers)
                {
                    player.UpdatePlayerPossessionTracker(false);
                }*/
                TeamManager.instance.greenTeam.UpdatePlayerPossessionTracker(false);
                TeamManager.instance.greyTeam.UpdatePlayerPossessionTracker(false);
            }
            if (newValue)
            {
                // If the ball is held, make sure that animation states aren't "stuck" in old state?
                if (animator.GetBool("isThrown"))
                    animator.SetBool("isThrown", false);
                if(animator.GetBool("isSideways"))
                    animator.SetBool("isSideways", false);
                if (animator.GetBool("isFalling"))
                    animator.SetBool("isFalling", false);
                if (animator.GetBool("isFumbled"))
                    animator.SetBool("isFumbled", false);
            }
        }            
        if (isClient)
        {
            myRenderer.enabled = !newValue;
            myCollider.enabled = !newValue;
            ballMarkerObject.SetActive(!newValue);
            if (!newValue)
            {
                GameplayManager.instance.NoTeamWithFootball();
            }   
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdThrowFootball(uint throwingGoblinNetId, uint goblinToThrowToNetId)
    {
        GoblinScript throwingGoblinScript = NetworkIdentity.spawned[throwingGoblinNetId].GetComponent<GoblinScript>();
        GoblinScript goblinToThrowToScript = NetworkIdentity.spawned[goblinToThrowToNetId].GetComponent<GoblinScript>();
        if (!goblinToThrowToScript.canGoblinReceivePass)
            return;

        if (throwingGoblinScript.doesCharacterHaveBall && !goblinToThrowToScript.doesCharacterHaveBall && !throwingGoblinScript.isKicking && !throwingGoblinScript.isDiving && !throwingGoblinScript.isGoblinKnockedOut)
        {
            throwingGoblinScript.HandleHasBall(throwingGoblinScript.doesCharacterHaveBall, false);
            this.HandleIsHeld(isHeld, false);
            RpcBallIsHeld(false);

            //Vector3 direction = (goblinToThrowToScript.transform.position - throwingGoblinScript.transform.position).normalized;
            Vector3 direction = GetDirectionOfThrow(throwingGoblinScript, goblinToThrowToScript);
            startingPositionOfThrow = this.transform.position;
            Debug.Log("CmdThrowFootball: The direction of the throw is: " + direction.ToString());
            _directionToThrow = direction;
            //float angletoThrow = Vector2.Angle(new Vector2(1f, 0f), direction);
            //Debug.Log("CmdThrowFootball: angle of the throw is " + angletoThrow.ToString());
            // this.transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, angletoThrow);
            if (transform.position.y >= ThrownMaxY)
                transform.position = new Vector2(transform.position.x, (ThrownMaxY - 0.1f));


            //this.GetComponent<Rigidbody2D>().AddForce()
            this.isThrown = true;
            /*animator.SetBool("isThrown", isThrown);

            if (Mathf.Abs(directionToThrow.y) < 0.55f)
                animator.SetBool("isSideways", true);
            else
                animator.SetBool("isSideways", false);*/
            TeamManager.instance.PassThrown(throwingGoblinScript.serverGamePlayer);
        }
    }
    [Server]
    private Vector2 GetDirectionOfThrow(GoblinScript throwingGoblin, GoblinScript receivingGoblin)
    {
        Vector3 throwingGoblinPosition = throwingGoblin.transform.position;
        Vector3 receivingGoblinPosition = receivingGoblin.transform.position;
        Vector2 throwDirection = (receivingGoblinPosition - throwingGoblinPosition).normalized;
        // If the receiving goblin is not moving, just throw toward their current position using direction calculated above
        if (!receivingGoblin.isRunningOnServer)
        {
            Debug.Log("GetDirectionOfThrow: receiving goblin is not moving. Throwing toward their current position of " + receivingGoblin.ToString());
            return throwDirection;
        }
        Vector2 receivingGoblinMovementDirection = receivingGoblin.GetRunningDirectionOnServer();
        float receivingGobinSpeed = receivingGoblin.speed;

        // Get a rough estimate of the max distance the receiving goblin will travel during a throw based on how long the ball will travel while thrown to the receiving goblin's starting position
        float distanceBetweenGoblins = Vector2.Distance(throwingGoblinPosition, receivingGoblinPosition);
        float timeForBallToGetToReceivingGoblin = GetTimeForThrownBallToTravel(distanceBetweenGoblins);
        Debug.Log("GetDirectionOfThrow: The throw ball will take: " + timeForBallToGetToReceivingGoblin.ToString() + " seconds to travel a distance of: " + distanceBetweenGoblins.ToString() + ". Start position: " + throwingGoblinPosition.ToString() + " end position: " + receivingGoblinPosition.ToString());
        float maxDistanceReceivingGoblinWillTravel = DistanceReceivingGoblinWillTravel(timeForBallToGetToReceivingGoblin, receivingGobinSpeed);

        // With the max distance traveled for the receiving goblin calculated, get their "end position" if they traveled that far?
        Vector3 receivingGoblinEndPosition = ReceivingGoblinEndPosition(receivingGoblinPosition, receivingGoblinMovementDirection, maxDistanceReceivingGoblinWillTravel);
        Debug.Log("GetDirectionOfThrow: Max distance receiving goblin will travel is: " + maxDistanceReceivingGoblinWillTravel.ToString() + " start position: " + receivingGoblinPosition.ToString() + " End position: " + receivingGoblinEndPosition.ToString() + " Receiving Goblin speed: " + receivingGobinSpeed.ToString() + " and direction: " + receivingGoblinMovementDirection.ToString());


        return throwDirection;
    }
    private float GetTimeForThrownBallToTravel(float distance)
    {
        if (distance > maxThrowDistance)
        {
            distance = maxThrowDistance;
        }

        return distance / speedOfThrow;
    }
    private float DistanceReceivingGoblinWillTravel(float time, float speed)
    {
        return time * speed;
    }
    private Vector3 ReceivingGoblinEndPosition(Vector3 startPosition, Vector2 direction, float distance)
    {
        return startPosition + (Vector3)(direction * distance);
    }
    [Server]
    public void FumbleFootball()
    {
        Debug.Log("FumbleFootball");
        this.HandleIsHeld(isHeld, false);

        int directionModifier = Random.Range(0, 2) * 2 - 1;

        //Set fumble control points
        Vector3 footballPosition = transform.position;
        /*if (footballPosition.y > GroundedMaxY)
        {
            footballPosition.y = GroundedMaxY;
            this.transform.position = footballPosition;
        }*/

        FumblePoints[0] = footballPosition; // starting point
        Vector3 controlPoint = footballPosition;
        controlPoint.y += 1.25f;
        controlPoint.x -= (2.25f * directionModifier);
        if (controlPoint.x > ThrownMaxX)
            controlPoint.x = ThrownMaxX;
        else if (controlPoint.x < ThrownMinX)
            controlPoint.x = ThrownMinX;
        FumblePoints[1] = controlPoint; // control point
        controlPoint.x -= (2.25f * directionModifier);
        controlPoint.y -= 1.25f;
        if (controlPoint.y > GroundedMaxY)
            controlPoint.y = GroundedMaxY;
        if (controlPoint.x > ThrownMaxX)
            controlPoint.x = ThrownMaxX;
        else if (controlPoint.x < ThrownMinX)
            controlPoint.x = ThrownMinX;
        FumblePoints[2] = controlPoint; // destination point
        fumbleCount = 0.0f;
        isFumbled = true;
        animator.SetBool("isFumbled", true);

    }
    /*[Server]
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
        float destinationY = Random.Range(KickFootballMinY, KickFootballMaxY);
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
        RpcActivateFootballShadow(true, KickedBallPoints[0]);
        RpcShadowPosition(KickedBallPoints[0], KickedBallPoints[2], kickCountModifier);
    }*/
    [Server]
    public void KickAfterAttemptKick(bool isKickingGoblinGrey, float kickPower, float kickAngle, float maxDistance, float minDistance, float accuracyDifficulty, float accuracySubmitted, Vector3 goblinPosition)
    {
        Debug.Log("KickAfterAttemptKick: for kick after attempt");
        this.HandleIsHeld(isHeld, false);

        isKickAfterAttemptGood = true;
        hasKickAfterAttemptBeenSubmittedToGameManager = false;

        Vector3 goalPostPosition = Vector3.zero;
        float inaccurateKickYModifer = 1.0f;
        if (isKickingGoblinGrey)
            goalPostPosition = GoalPostForGrey;
        else
            goalPostPosition = GoalPostForGreen;
        float distanceToGoalPost = Vector2.Distance(goalPostPosition, goblinPosition);
        float distanceTraveled = ((maxDistance - minDistance) * kickPower) + minDistance;
        if (distanceTraveled >= distanceToGoalPost)
        {
            Debug.Log("KickAfterAttemptKick: Kick distance is GOOD! Distance needed: " + distanceToGoalPost.ToString() + " distance traveled: " + distanceTraveled.ToString());
        }            
        else
        {
            Debug.Log("KickAfterAttemptKick: Kick distance is BAD! The kick is not long enough! Distance needed: " + distanceToGoalPost.ToString() + " distance traveled: " + distanceTraveled.ToString());
            isKickAfterAttemptGood = false;
        }
        if (Mathf.Abs(accuracySubmitted) <= accuracyDifficulty)
        {
            Debug.Log("KickAfterAttemptKick: Kick accuracy is GOOD! Accuracy difficulty: " + accuracyDifficulty.ToString() + " accuracy submitted: " + accuracySubmitted.ToString());
        }
        else
        {
            Debug.Log("KickAfterAttemptKick: Kick accuracy is BAD! Kick is wide left/right. Accuracy difficulty: " + accuracyDifficulty.ToString() + " accuracy submitted: " + accuracySubmitted.ToString());
            isKickAfterAttemptGood = false;
            if (accuracySubmitted < 0)
            {
                if (isKickingGoblinGrey)
                {
                    if (goblinPosition.y >= goalPostPosition.y)
                        inaccurateKickYModifer = 2.1f;
                    else
                        inaccurateKickYModifer = 0.4f;
                }
                else
                {
                    if (goblinPosition.y >= goalPostPosition.y)
                        inaccurateKickYModifer = 0.4f;
                    else
                        inaccurateKickYModifer = 2.1f;
                }
                
            }
            else
            {
                if (isKickingGoblinGrey)
                {
                    if (goblinPosition.y >= goalPostPosition.y)
                        inaccurateKickYModifer = 0.4f;
                    else
                        inaccurateKickYModifer = 2.1f;
                }
                else
                {
                    if (goblinPosition.y >= goalPostPosition.y)
                        inaccurateKickYModifer = 2.1f;
                    else
                        inaccurateKickYModifer = 0.4f;
                }                
            }
        }

        // look at this for calculating x,y position of endpoint when you know distance and angle:
        //https://answers.unity.com/questions/759542/get-coordinate-with-angle-and-distance.html


        int directionToKickModifier = 1;
        if (isKickingGoblinGrey)
            directionToKickModifier = -1;

        int yModifier = 1;
        if (goblinPosition.y >= goalPostPosition.y)
            yModifier = -1;
        Debug.Log("KickAfterAttemptKick: Calculating kick angle for kickoff using a kick angle of " + kickAngle.ToString() + " and a distance of " + distanceTraveled.ToString());
        //var degrees = kickAngle * directionToKickModifier;
        var degrees = (kickAngle * inaccurateKickYModifer);
        var radians = degrees * Mathf.Deg2Rad;
        var x = Mathf.Cos(radians);
        var y = Mathf.Sin(radians);
        x = x * distanceTraveled * directionToKickModifier;
        y = y * distanceTraveled * yModifier;
        Vector2 newPosition = new Vector2(x, y);
        Vector2 goblinPosition2 = goblinPosition;
        Vector2 newPosition2 = goblinPosition2 + newPosition;
        Debug.Log("KickAfterAttemptKick: NEW POSITION for kick should be: " + newPosition.ToString() + " and NEW POSITION 2 is: " + newPosition2.ToString());


        /*float slope = (goalPostPosition.y - goblinPosition.y) / (goalPostPosition.x / goblinPosition.x);
        float c = 1 / (Mathf.Sqrt(1 + Mathf.Pow(slope, 2)));
        float s = slope / (Mathf.Sqrt(1 + Mathf.Pow(slope, 2)));
        Vector2 csPoint = new Vector2(c, s);
        Vector2 footballPosition2 = goblinPosition;
        Vector2 newEndPoint1 = footballPosition2 + (distanceTraveled * csPoint);
        Vector2 newEndPoint2 = footballPosition2 - (distanceTraveled * csPoint);
        Debug.Log("KickFootballDownField: NEW ENDPOINTS for kick after attempt. + endpoint: " + newEndPoint1.ToString() + " - endpoint " + newEndPoint2.ToString() + " c value: " + c.ToString() + " s value: " + s.ToString() + "slope was: " + slope.ToString());*/

        //KickFootballDownField(isKickingGoblinGrey, kickPower, kickAngle, maxDistance, minDistance);

        //Setup the kick control points for animation
        KickedBallPoints[0] = goblinPosition; //start point
        KickedBallPoints[2] = newPosition2; // destination point
        float controlX = ((newPosition2.x - goblinPosition.x) / 2) + goblinPosition.x;
        //float controlY = (distanceTraveled + (differenceInY)) / 2;
        float controlY = distanceTraveled + goblinPosition.y;
        KickedBallPoints[1] = new Vector3(controlX, controlY, goblinPosition.z);// control point


        if (newPosition2.x > goblinPosition.x)
            bounceForward = true;
        else
            bounceForward = false;

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
        RpcActivateFootballShadow(true, KickedBallPoints[0]);
        RpcShadowPosition(KickedBallPoints[0], KickedBallPoints[2], kickCountModifier);

        //do stuff depending on if kick was good or bad
        Debug.Log("KickAfterAttemptKick: Was the kick good??? " + isKickAfterAttemptGood.ToString() + "!!!");

    }
    [Server]
    public void KickFootballDownField(bool isKickingGoblinGrey, float kickPower, float kickAngle, float maxDistance, float minDistance)
    {
        Debug.Log("KickFootballDownField with isKickingGoblinGrey: " + isKickingGoblinGrey.ToString() + " kick poweR: " + kickPower.ToString() + " max distance: " + maxDistance.ToString() + " min distance: " + minDistance.ToString());
        this.HandleIsHeld(isHeld, false);

        int directionToKickModifier = 1;
        if (isKickingGoblinGrey)
            directionToKickModifier = -1;


        // Starting position of the kick
        Vector3 footballPosition = transform.position;
        KickedBallPoints[0] = footballPosition;

        // Get destination of ball / where the ball lands
        float destinationY = Random.Range(KickFootballMinY, KickFootballMaxY);
        float differenceInY = destinationY - footballPosition.y;
        float distanceTraveled = ((maxDistance - minDistance) * kickPower) + minDistance;
        xDistanceOfKick = distanceTraveled;
        float destinationX = footballPosition.x + (distanceTraveled * directionToKickModifier);

        if (GameplayManager.instance.gamePhase == "kickoff")
        {
            // Reset the "last team with possession value for the sound stuff?
            this.teamWithBallLast = null;
            Debug.Log("KickFootballDownField: Calculating kick angle for kickoff using a kick angle of " + kickAngle.ToString() + " and a distance of " + distanceTraveled.ToString());
            //Vector3 kickoffPos = new Vector3();
            var degrees = kickAngle * directionToKickModifier;
            var radians = degrees * Mathf.Deg2Rad;
            var y = Mathf.Sin(radians);
            y = y * distanceTraveled;
            if(SceneManager.GetActiveScene().name.Contains("768-432"))
                footballPosition.y = -1.5f;
            y += footballPosition.y;

            if (y > KickFootballMaxY || y < KickFootballMinY)
            {
                Debug.Log("KickFootballDownField: ball was kicked out of bounds. Checking where it should intersect.");
                float slope = (y - footballPosition.y) / (destinationX - footballPosition.x);

                // This is to make sure the kick off isn't adjusted "up" on the higher resolution scene?
                //y += footballPosition.y;

                if (y > KickFootballMaxY)
                    y = KickFootballMaxY;
                if (y < KickFootballMinY)
                    y = KickFootballMinY;
                float newX = ((y - footballPosition.y) / slope) - footballPosition.x;
                
                if ((newX * directionToKickModifier) < 10f)
                    newX = (10.1f * directionToKickModifier);

                Debug.Log("KickFootballDownField: Ball out of bounds. New x position will be " + newX.ToString());
                destinationX = newX;
            }
            // This is to make sure the kick off isn't adjusted "up" on the higher resolution scene?
            //destinationY = footballPosition.y + y;
            destinationY = y;
            Debug.Log("KickFootballDownField: y value for kickoff is: " + destinationY.ToString());
            differenceInY = destinationY - footballPosition.y;
        }

        /*if (GameplayManager.instance.gamePhase == "kick-after-attempt")
        {
            Debug.Log("KickFootballDownField: for kick after attempt");
            Vector3 goalPostPosition = Vector3.zero;
            if (isKickingGoblinGrey)
                goalPostPosition = new Vector2GoalPostForGrey;
            else
                goalPostPosition = new Vector2GoalPostForGreen;

            float slope = (goalPostPosition.y - footballPosition.y) / (goalPostPosition.x / footballPosition.x);
            float c = 1 / (Mathf.Sqrt(1 + Mathf.Pow(slope,2)));
            float s = slope / (Mathf.Sqrt(1 + Mathf.Pow(slope, 2)));
            Vector2 csPoint = new Vector2(c, s);
            Vector2 footballPosition2 = footballPosition;
            Vector2 newEndPoint1 = footballPosition2 + (distanceTraveled * csPoint);
            Vector2 newEndPoint2 = footballPosition2 - (distanceTraveled * csPoint);
            Debug.Log("KickFootballDownField: NEW ENDPOINTS for kick after attempt. + endpoint: " + newEndPoint1.ToString() + " - endpoint " + newEndPoint2.ToString() + " c value: " + c.ToString() + " s value: " + s.ToString() + "slope was: " + slope.ToString());

        }*/

        if(destinationX > ThrownMaxX)
            destinationX = ThrownMaxX;
        else if (destinationX < ThrownMinX)
            destinationX = ThrownMinX;

        KickedBallPoints[2] = new Vector3(destinationX, destinationY, footballPosition.z); // destination point

        // get direction of kick for bounching later
        if (destinationX > footballPosition.x)
            bounceForward = true;
        else
            bounceForward = false;

        // Get the "control point" or the height of the ball
        float controlX = ((destinationX - footballPosition.x) / 2) + footballPosition.x;
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
        RpcActivateFootballShadow(true, KickedBallPoints[0]);
        RpcShadowPosition(KickedBallPoints[0], KickedBallPoints[2], kickCountModifier);
    }
    [Server]
    public void KickFootballForKickoff(bool isKickingGoblinGrey, float kickPower, float kickAngle, float maxDistance, float minDistance)
    { 

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
            if (newValue && GameplayManager.instance.gamePhase == "kickoff")
            {
                GameplayManager.instance.HandleGamePhase(GameplayManager.instance.gamePhase, "gameplay");
                GameplayManager.instance.ActivateGameTimer(true);
            }
            if (newValue && GameplayManager.instance.gamePhase == "kick-after-attempt")
            {
                GameplayManager.instance.DisableKickAfterAttemptControls();
            }
            if (!newValue && GameplayManager.instance.gamePhase == "kick-after-attempt")
            {
                GameplayManager.instance.TransitionFromKickAfterAttemptToKickOff();
            }
                
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
            else if (newValue)
            {
                if (GameplayManager.instance.gamePhase == "kick-after-attempt")
                {
                    GamePlayer localPlayer = GameObject.FindGameObjectWithTag("LocalGamePlayer").GetComponent<GamePlayer>();
                    localPlayer.FollowSelectedGoblin(footballShadowObject.transform);
                }
            }
        }
    }
    [ClientRpc]
    void RpcActivateFootballShadow(bool shadow, Vector3 shadowStartPosition)
    {
        footballShadowObject.transform.position = shadowStartPosition;
        footballShadowObject.SetActive(shadow);

        /*if (GameplayManager.instance.gamePhase == "kick-after-attempt")
        {
            GamePlayer localPlayer = GameObject.FindGameObjectWithTag("LocalGamePlayer").GetComponent<GamePlayer>();
            localPlayer.FollowSelectedGoblin(footballShadowObject.transform);
        }*/

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
        // Tell clients to playing the bouncing ball sound?
        this.RpcBounceFootballSFX();

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
        if (numberOfBounces <= 0)
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

            if (GameplayManager.instance.gamePhase != "kick-after-attempt")
            {
                if (bounceControlPoint.x > ThrownMaxX)
                {
                    bounceControlPoint.x = ThrownMaxX;
                    controlXDist = 0f;
                }
                else if (bounceControlPoint.x < ThrownMinX)
                {
                    bounceControlPoint.x = ThrownMinX;
                    controlXDist = 0f;
                }
            }
            
                

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
    [Server]
    public void MoveFootballToKickoffGoblin(uint goblinNetId)
    {
        GoblinScript goblin = NetworkIdentity.spawned[goblinNetId].GetComponent<GoblinScript>();
        if(GameplayManager.instance.gamePhase == "kickoff")
            this.transform.position = new Vector3(0f,-2f,0f);
        else
            this.transform.position = goblin.transform.position;
        if (this.isHeld)
        {
            goblinWithBall.HandleHasBall(goblinWithBall.doesCharacterHaveBall, false);
            HandleIsHeld(this.isHeld, false);
        }
    }
    void HandleGoblinWithBallNetId(uint oldValue, uint newValue)
    {
        if (isServer)
        {
            goblinWithBallNetId = newValue;
            if ((GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time") && this.isHeld && newValue != 0)
            {
                try
                {
                    GoblinScript goblinScript = NetworkIdentity.spawned[goblinWithBallNetId].GetComponent<GoblinScript>();
                    /*foreach (GamePlayer player in Game.GamePlayers)
                    {
                        if (player.ConnectionId == goblinScript.serverGamePlayer.ConnectionId)
                        {
                            player.UpdatePlayerPossessionTracker(true);
                        }
                        else
                        {
                            player.UpdatePlayerPossessionTracker(false);
                        }
                    }*/
                    if (goblinScript.isGoblinGrey)
                    {
                        TeamManager.instance.greyTeam.UpdatePlayerPossessionTracker(true);
                        TeamManager.instance.greenTeam.UpdatePlayerPossessionTracker(false);
                    }
                    else
                    {
                        TeamManager.instance.greenTeam.UpdatePlayerPossessionTracker(true);
                        TeamManager.instance.greyTeam.UpdatePlayerPossessionTracker(false);
                    }
                }
                catch
                {
                    Debug.Log("HandleIsHeld: Could not update player possession tracker with the goblin with ball.");
                }

            }
        }
        if (isClient)
        {
            if ((GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time") && this.isHeld && newValue != 0)
            {
                try
                {
                    GameplayManager.instance.UpdatePossessionOfFootballtoTeam(NetworkIdentity.spawned[goblinWithBallNetId].GetComponent<GoblinScript>().isGoblinGrey);
                }
                catch
                {
                    Debug.Log("HandleIsHeld: Could not update GameplayManager with the goblin with ball.");
                }

            }
            /*else if (!newValue)
            {
                GameplayManager.instance.RpcNoTeamWithFootball();
            }*/
        }
    }
    void HandleIsFumbled(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            isFumbled = newValue;
            if (!newValue)
            {
                myCollider.enabled = false;
                myCollider.enabled = true;
            }
        }
        if (isClient)
        { 

        }
    }
    void HandleIsBouncing(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            isBouncing = newValue;
            if (!newValue)
            {
                myCollider.enabled = false;
                myCollider.enabled = true;
            }
        }
        if (isClient)
        {

        }
    }
    [ServerCallback]
    IEnumerator CountdownToEndGame()
    {
        yield return new WaitForSeconds(0.25f);
        if (!this.isHeld)
        {
            if (!GameplayManager.instance.firstHalfCompleted)
            {
                GameplayManager.instance.HandleGamePhase(GameplayManager.instance.gamePhase, "halftime");
            }
            else
            {
                GameplayManager.instance.HandleGamePhase(GameplayManager.instance.gamePhase, "gameover");
            }            
        }
    }
    [ClientRpc]
    void RpcChangePossessionSFX(bool doesGreyTeamHaveBallNow)
    {
        Debug.Log("Football.cs RpcChangePossessionSFX: is team with ball grey? " + doesGreyTeamHaveBallNow.ToString());
        if(GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time")
            GameplayManager.instance.PlayPossessionChangedSFX(doesGreyTeamHaveBallNow);
    }
    [ClientRpc]
    void RpcBounceFootballSFX()
    {
        this.BounceFootballSFX();
    }
    [ClientCallback]
    void BounceFootballSFX()
    {
        if (this.IsFootballOnScreen())
            SoundManager.instance.PlaySound("bounce-ball", 0.8f);
    }
    public bool IsFootballOnScreen()
    {
        bool onscreen = false;
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(this.transform.position);
        if (screenPoint.x < 0 || screenPoint.x > 1)
        {
            onscreen = false;
        }
        else
            onscreen = true;
        return onscreen;
    }
}
