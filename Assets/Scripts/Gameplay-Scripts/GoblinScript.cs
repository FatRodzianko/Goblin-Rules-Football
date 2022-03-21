using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class GoblinScript : NetworkBehaviour
{
    public enum State
    {
        ChaseFootball,
        ChaseBallCarrier,
        TeamHasBall,
        AttackNearbyGoblin,
    }

    public float pressedTime = 0f;
    public float releasedTime = 0f;

    [SerializeField] GameObject PowerUpParticleSystemPrefab;


    [Header("Player Owner Info")]
    [SyncVar] public string ownerName;
    [SyncVar] public int ownerConnectionId;
    [SyncVar] public int ownerPlayerNumber;
    [SyncVar] public uint ownerNetId;
    public GamePlayer myGamePlayer;
    public GamePlayer serverGamePlayer;
    

    [Header("Goblin Base Stats")]
    [SyncVar] public int MaxHealth;
    [SyncVar] public float MaxStamina;
    [SyncVar] public float StaminaDrain;
    [SyncVar] public float StaminaRecovery;
    [SyncVar] public float MaxSpeed;
    [SyncVar] public int MaxDamage;
    [SyncVar] public bool canCollide = true;


    [Header("Goblin Current Stats")]
    [SyncVar(hook = nameof(UpdateGoblinHealth))] public float health;
    [SyncVar(hook = nameof(UpdateGoblinStamina))] public float stamina;
    [SyncVar] public float speed;
    [SyncVar] public float damage;
    [SyncVar] public float ballCarrySpeedModifier = 1.0f;
    [SyncVar] public float blockingSpeedModifier = 1.0f;
    [SyncVar] public float defenseModifier;
    [SyncVar] public float speedModifierFromPowerUps = 1.0f;
    [SyncVar] public float slowDownObstacleModifier = 1.0f;


    [Header("Character selection stuff")]
    [SyncVar(hook = nameof(HandleCharacterSelected))] public bool isCharacterSelected = false;
    [SyncVar(hook = nameof(HandleIsQGoblin))] public bool isQGoblin = false;
    [SyncVar(hook = nameof(HandleIsEGoblin))] public bool isEGoblin = false;
    public bool canGoblinMove = true;
    public bool isGoblinPunching = false;
    [SyncVar(hook = nameof(HandleIsPunching))] public bool isPunching = false;
    
    
    [SerializeField] private GameObject eMarkerPrefab;
    [SerializeField] private GameObject qMarkerPrefab;
    private GameObject eMarker;
    private GameObject qMarker;
    [SerializeField] private GameObject ballMarkerPrefab;
    [SerializeField] private GameObject ballMarkerOpponentPrefab;
    private GameObject ballMarkerObject;

    [Header("Character Properties")]    
    public Animator animator;
    public Rigidbody2D rb;
    public Collider2D playerCollider;
    public Vector2 previousInput;
    public SpriteRenderer myRenderer;
    [SerializeField] BoxCollider2D golbinBodyCollider;
    [SyncVar] public string goblinType;
    [SerializeField] private StatusBarScript myStatusBars;
    [SerializeField] private GameObject touchdownHitbox;
    [SerializeField] private GameObject divingHitbox;


    [Header("Character Game State Stuff")]
    [SyncVar(hook = nameof(HandleIsGoblinGrey))] public bool isGoblinGrey = false;
    [SyncVar(hook = nameof(HandleHasBall))] public bool doesCharacterHaveBall;
    [SyncVar(hook = nameof(HandleIsThrowing))] public bool isThrowing = false;
    [SyncVar] public bool isRunningOnServer = false;
    public bool isRunning = false;
    public bool isSprinting = false;
    public bool shiftHeldDown = false;
    [SyncVar] public bool isSprintingOnServer = false;
    [SyncVar] public bool canRecoveryStamina = true;
    [SyncVar(hook = nameof(HandleIsFatigued))] public bool isFatigued = false;
    [SyncVar(hook = nameof(HandleIsGoblinKnockedOut))] public bool isGoblinKnockedOut = false;
    
    [Header("Slide Info")]
    [SyncVar(hook = nameof(HandleIsSliding))] public bool isSliding = false;
    [SyncVar] public Vector2 slideDirection = Vector2.zero;
    public bool isSlidingRoutineRunning = false;
    IEnumerator isSlidingRoutine;
    public float slideSpeedModifer = 1.0f;

    [Header("Dive Info")]
    [SyncVar(hook = nameof(HandleIsDiving))] public bool isDiving = false;
    public bool DivingRoutineRunning = false;
    IEnumerator DivingRoutine;

    [Header("Is Goblin Throwing Stuff")]
    public bool throwingRoutine = false;
    IEnumerator throwing;

    [Header("Blocking Info")]
    [SyncVar(hook = nameof(HandleIsBlocking))] public bool isBlocking = false;

    [Header("Kicking Info")]
    [SyncVar(hook = nameof(HandleIsKicking))] public bool isKicking = false;
    [SerializeField] GameObject KickPowerBarHolder;
    [SerializeField] GameObject KickPowerBarFillerImage;
    [SerializeField] GameObject KickoffAimArrow;
    public bool powerBarActive = false;
    [SyncVar] public float GoblinMaxKickDistance = 40f;
    [SyncVar] public float GoblinMinKickDistance = 10f;
    [SyncVar] public float GoblinPowerBarSpeed = 1f;
    [SyncVar] public float GoblinKickPower = 0f;
    
    public float currentPowerBarScale = 0f;
    public int powerBarDirection = 1;
    public float kickoffAngle = 0f;
    [SyncVar] public float GoblinKickoffAngle = 0f;
    float kickoffAngleSpeed = 30f;
    public bool aimArrowButtonHeldDown = false;
    public bool aimArrowUp = false;

    [Header("Kick After Repositioning Stuff")]
    [SyncVar] public bool isKickAfterGoblin = false;
    [SyncVar] public bool isKickAfterPositionSet = false;
    [SyncVar] public float angleOfKickAttempt = 0f;
    [SyncVar(hook =nameof(HandleKickAfterAccuracyDifficultyUpdate))] public float kickAfterAccuracyDifficulty = 0f;
    //[SyncVar] public float kickAfterAccuracyBar1 = 0f;
    //[SyncVar] public float kickAfterAccuracyBar2 = 0f;
    bool repositioningButtonHeldDown = false;
    bool repositioningToLeft = false;
    Vector2 greenGoalPost = new Vector2(50.3f, -1.5f);
    Vector2 greyGoalPost = new Vector2(-50.3f, -1.5f);
    [SyncVar] public Vector2 kickAfterFinalPosition = Vector2.zero;
    public bool hasGoblinBeenRepositionedForKickAfter = false;

    [Header("Kick After Accuracy Bar Stuff")]
    [SerializeField] GameObject kickAfterAccuracyBar;
    [SerializeField] GameObject kickAfterGuageLine;
    [SerializeField] GameObject kickAfterMarkerLeft;
    [SerializeField] GameObject kickAfterMarkerRight;
    [SyncVar] public float accuracyValueSubmitted;
    [SyncVar] public float powerValueSubmitted;
    public bool isGoblinDoingKickAfterAttempt = false;
    bool isAccuracySubmittedYet = false;
    bool isPowerSubmittedYet = false;
    IEnumerator kickAfterMoveAccuracyGuageLineRoutine;
    float currentAccuracyGaugeXPosition = -1f;
    int currentAccuracyGaugeDirection = 1;
    public float AccuracyBarSpeed = 0f;

    [Header("wasPunchedSpeedModifier Info")]
    public bool isWasPunchedRoutineRunning = false;
    IEnumerator isWasPunched;
    public float wasPunchedSpeedModifier = 1.0f;

    [Header("Can the Goblin Pass stuff")]
    public Football gameFootball;
    [SyncVar(hook = nameof(HandleCanGoblinReceivePass))] public bool canGoblinReceivePass = false;


    [Header("Recovery Enumerator Stuff?")]
    [SyncVar] public bool isStaminaRecoveryRoutineRunning = false;
    public IEnumerator staminaRecoveryRoutine;
    [SyncVar] public bool isHealthRecoveryRoutineRunning = false;
    public IEnumerator healthRecoveryRoutine;
    [SyncVar] public bool canGoblinRegainHealth = false;
    [SyncVar] public bool isRegainHealthRoutineRunning = false;
    public IEnumerator regainHealthRoutine;
    public bool isTrippedTimerRunning = false;
    public IEnumerator trippedTimerRoutine;

    [Header("HurtBox/HitBox Stuff")]
    [SerializeField] private GameObject punchBoxCollider;
    [SerializeField] private GameObject hurtBoxCollider;
    [SerializeField] private GameObject slideBoxCollider;

    [Header("PowerUp Effects")]
    public bool attackNormal;
    public bool defenseNormal;
    public bool speedNormal;

    [Header("AI Stuff")]
    public State state;
    public float punchRange = 2.5f;
    public float punchRate = 1.0f;
    public float nextPunchTime = 0f;
    public float slideRange = 3.5f;
    public float slideRate = 2.5f;
    public float nextSlideTime = 0f;
    public float minDistanceFromTarget = 1.7f;
    public float punchYMax = 1.15f;
    public Vector3 positionToRunTo = Vector3.zero;
    [SerializeField] float fieldMaxY; // = 6.25f;
    [SerializeField] float fieldMinY; // = -5f;
    //float fieldMaxY = 5.99f;
    //float fieldMinY = -4.25f;
    [SerializeField] float fieldMaxX; // = 41f;
    [SerializeField] float fieldMinX; // = -41;
    [SerializeField] LayerMask goblinLayerMask;
    public GoblinScript goblinTarget;
    public Vector2 preDirection = Vector2.zero;
    public Vector2 postDirection = Vector2.zero;
    public float adjacentGoblinTime = 0f;
    public float diveRate = 2.5f;
    public float nextDiveTime = 0f;
    GoblinAIPathFinding myGoblinAIPathFindingScript;


    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        GamePlayer localPlayer = GameObject.FindGameObjectWithTag("LocalGamePlayer").GetComponent<GamePlayer>();
        if (localPlayer)
        {
            myGamePlayer = localPlayer;
            myGamePlayer.AddToGoblinTeam(this);
            if (!myGamePlayer.IsGameLeader)
                CmdMakeGoblinGrey();

            //GoblinStartingPosition(myGamePlayer.IsGameLeader);
            if (transform.position.x > 0f)
                CmdFlipRenderer(true);
        }
        InputManager.Controls.Player.Move.performed += ctx => SetMovement(ctx.ReadValue<Vector2>());
        InputManager.Controls.Player.Move.canceled += ctx => ResetMovement();

        InputManager.Controls.Player.Sprint.performed += _ => IsPlayerSprinting(true);
        InputManager.Controls.Player.Sprint.canceled += _ => IsPlayerSprinting(false);
        EnableGoblinMovement(false);

        myGoblinAIPathFindingScript = this.GetComponent<GoblinAIPathFinding>();
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        if (!gameFootball)
        {
            gameFootball = GameObject.FindGameObjectWithTag("football").GetComponent<Football>();
        }
    }

    public void EnableGoblinMovement(bool allowMovement)
    {
        if (allowMovement)
        {
            InputManager.Controls.Player.Move.Enable();
            InputManager.Controls.Player.Sprint.Enable();

           /*nputManager.Controls.Player.Move.performed += ctx => SetMovement(ctx.ReadValue<Vector2>());
            InputManager.Controls.Player.Move.canceled += ctx => ResetMovement();

            InputManager.Controls.Player.Sprint.performed += _ => IsPlayerSprinting(true);
            InputManager.Controls.Player.Sprint.canceled += _ => IsPlayerSprinting(false);*/
        }
        else
        {
            InputManager.Controls.Player.Move.Disable();
            InputManager.Controls.Player.Sprint.Disable();
        }
    }
    void GoblinStartingPosition(bool isPlayerGameLeader)
    {
        Vector3 startingPosition = Vector3.zero;
        if (isPlayerGameLeader)
        {
            startingPosition.x = -9f;
        }
        else
            startingPosition.x = 9f;

        if (this.name.Contains("grenadier"))
            startingPosition.y = 4.45f;
        else if (this.name.Contains("berserker"))
            startingPosition.y = 0f;
        else if (this.name.Contains("skirmisher"))
            startingPosition.y = -4.45f;

        this.transform.position = startingPosition;
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        try
        {
            GamePlayer localPlayer = GameObject.FindGameObjectWithTag("LocalGamePlayer").GetComponent<GamePlayer>();
            localPlayer.ReportPlayerGoblinSpawned();
            if (!gameFootball)
            {
                gameFootball = GameObject.FindGameObjectWithTag("football").GetComponent<Football>();
            }
        }
        catch
        {
            Debug.Log("GoblinScript.cs: Could not find local game player object");
        }
        //rb.bodyType = RigidbodyType2D.Kinematic;

        if (hasAuthority)
            ballMarkerObject = Instantiate(ballMarkerPrefab);
        else
            ballMarkerObject = Instantiate(ballMarkerOpponentPrefab);
        ballMarkerObject.transform.SetParent(this.transform);
        Vector3 markerPosition = myStatusBars.transform.localPosition;
        markerPosition.y += 0.75f;
        ballMarkerObject.transform.localPosition = markerPosition;
        ballMarkerObject.SetActive(false);
    }
    // Start is called before the first frame update
    private void Awake()
    {
        
        qMarker = Instantiate(qMarkerPrefab);
        qMarker.transform.SetParent(this.transform);
        //qMarker.transform.localPosition = new Vector3(0f, 2f, 0f);
        Vector3 markerPosition = myStatusBars.transform.localPosition;
        markerPosition.y += 0.75f;
        qMarker.transform.localPosition = markerPosition;
        qMarker.SetActive(false);
        eMarker = Instantiate(eMarkerPrefab);
        eMarker.transform.SetParent(this.transform);
        //eMarker.transform.localPosition = new Vector3(0f, 2f, 0f);
        eMarker.transform.localPosition = markerPosition;
        eMarker.SetActive(false);

        

    }
    public void SelectThisCharacter()
    {
        Debug.Log("SelectThisCharacter " + this.name);
        if (hasAuthority)
            CmdSelectThisCharacter();
    }

    [Command]
    void CmdSelectThisCharacter()
    {
        isCharacterSelected = true;
    }
    public void UnSelectThisCharacter()
    {
        Debug.Log("UnSelectThisCharacter " + this.name);
        if (hasAuthority)
            CmdUnSelectThisCharacter();
    }

    [Command]
    void CmdUnSelectThisCharacter()
    {
        isCharacterSelected = false;
    }
    public void HandleCharacterSelected(bool oldValue, bool newValue)
    {
        if (isServer)
            isCharacterSelected = newValue;
        if (isClient)
        {
            if (hasAuthority)
            {
                if (newValue)
                {
                    //rb.bodyType = RigidbodyType2D.Dynamic;
                    myGamePlayer.FollowSelectedGoblin(this.transform);
                }
                else
                {
                    //rb.bodyType = RigidbodyType2D.Kinematic;
                    if (isRunning)
                    {
                        isRunning = false;
                        animator.SetBool("isRunning", isRunning);
                    }
                }
            }
        }
    }
    public void SetQGoblin(bool isQ)
    {
        Debug.Log("SetQGoblin " + isQ.ToString());
        if (hasAuthority)
        {
            CmdSetQGoblin(isQ);
            if (isBlocking)
                CmdSetBlocking(false);
        }
            
    }
    [Command]
    void CmdSetQGoblin(bool isQ)
    {
        Debug.Log("CmdSetQGoblin " + isQ.ToString() + " "  + this.name);
        HandleIsQGoblin(isQGoblin, isQ);
    }
    public void HandleIsQGoblin(bool oldValue, bool newValue)
    {
        if (isServer)
            isQGoblin = newValue;
        if (isClient)
        {
            if (hasAuthority)
            {
                if (hasAuthority)
                {
                    qMarker.SetActive(newValue);
                }
            }
        }
    }
    public void SetEGoblin(bool isE)
    {
        Debug.Log("SetEGoblin " + isE.ToString() + " " + this.name);
        if (hasAuthority)
        {
            CmdSetEGoblin(isE);
            if (isBlocking)
                CmdSetBlocking(false);
        }
            
    }
    [Command]
    void CmdSetEGoblin(bool isE)
    {
        Debug.Log("CmdSetEGoblin " + isE.ToString());
        HandleIsEGoblin(isEGoblin, isE);
    }

    public void HandleIsEGoblin(bool oldValue, bool newValue)
    {
        if (isServer)
            isEGoblin = newValue;
        if (isClient)
        {
            if (hasAuthority)
            {
                eMarker.SetActive(newValue);
            }
        }
    }
    private void Update()
    {
        if (isServer)
        {
            // Check if goblin can be passed too?
            if (serverGamePlayer.doesTeamHaveBall && !doesCharacterHaveBall)
            {
                float diffFromFootball = gameFootball.transform.position.x - this.transform.position.x;
                if (isGoblinGrey)
                {
                    if (diffFromFootball <= 0)
                    {
                        HandleCanGoblinReceivePass(this.canGoblinReceivePass, true);
                    }
                    else
                    {
                        HandleCanGoblinReceivePass(this.canGoblinReceivePass, false);
                    }
                }
                else
                {
                    if (diffFromFootball >= 0)
                    {
                        HandleCanGoblinReceivePass(this.canGoblinReceivePass, true);
                    }
                    else
                    {
                        HandleCanGoblinReceivePass(this.canGoblinReceivePass, false);
                    }
                }
            }
            else
            {
                HandleCanGoblinReceivePass(this.canGoblinReceivePass, true);
            }
            if (this.isKickAfterGoblin && !this.isKickAfterPositionSet && GameplayManager.instance.gamePhase == "kick-after-attempt")
            {
                // Calculate accuracy stuff for kick after as player moves goblin
                if (this.isGoblinGrey)
                {
                    Vector2 v2 = transform.position;
                    angleOfKickAttempt = Mathf.Abs(Mathf.Atan2((v2.y - greyGoalPost.y), (v2.x - greyGoalPost.x)) * Mathf.Rad2Deg);

                }
                else
                {
                    Vector2 v2 = transform.position;
                    /*Vector2 v1 = greenGoalPost;
                    v2 = v1 - v2;*/
                    angleOfKickAttempt = Mathf.Abs(Mathf.Atan2((greenGoalPost.y - v2.y), (greenGoalPost.x - v2.x)) * Mathf.Rad2Deg);
                    //angleOfKickAttempt = Vector2.Angle(this.transform.position, greenGoalPost);
                }
                CalculateKickAfterAccuracyDifficulty(angleOfKickAttempt);
            }
        }
        if (isClient)
        {
            // Update the kick powerbar stuff
            if (powerBarActive && doesCharacterHaveBall)
            {
                currentPowerBarScale += Time.deltaTime * GoblinPowerBarSpeed * powerBarDirection;
                if (currentPowerBarScale > 1f)
                {
                    currentPowerBarScale = 1f;
                    powerBarDirection = -1;
                }
                else if (currentPowerBarScale < 0f)
                {
                    currentPowerBarScale = 0f;
                    powerBarDirection = 1;
                }
                    
                KickPowerBarFillerImage.transform.localScale = new Vector3(currentPowerBarScale, 1f, 1f);
            }
            if (aimArrowButtonHeldDown && doesCharacterHaveBall && GameplayManager.instance.gamePhase == "kickoff")
            {
                int angleMultiplier = 1;
                if (aimArrowUp)
                    angleMultiplier = 1;
                else
                    angleMultiplier = -1;
                if (isGoblinGrey)
                    angleMultiplier *= -1;

                kickoffAngle += (Time.deltaTime * kickoffAngleSpeed) * angleMultiplier;
                if (kickoffAngle > 45f)
                    kickoffAngle = 45f;
                if(kickoffAngle < -45f)
                    kickoffAngle = -45f;

                //KickoffAimArrow.transform.Rotate(0f, 0f, kickoffAngle, Space.Self);
                KickoffAimArrow.transform.localRotation = Quaternion.Euler(0f, 0f, kickoffAngle);
            }
            if (repositioningButtonHeldDown && GameplayManager.instance.gamePhase == "kick-after-attempt" && !isKickAfterPositionSet)
            {
                int directionModifier = 1;
                if (repositioningToLeft)
                    directionModifier = -1;
                Vector3 newPosition = this.transform.position;
                newPosition.x += speed * Time.deltaTime * directionModifier;

                /*if (this.isGoblinGrey && newPosition.x < -30f)
                    newPosition.x = -30f;
                else if (!this.isGoblinGrey && newPosition.x > 30f)
                    newPosition.x = 30f;*/
                if (this.isGoblinGrey && newPosition.x < -40f)
                    newPosition.x = -40f;
                else if (!this.isGoblinGrey && newPosition.x > 40f)
                    newPosition.x = 40f;

                if (this.isGoblinGrey)
                {
                    if (Vector2.Distance(newPosition, greyGoalPost) > GoblinMaxKickDistance)
                        return;
                }
                else
                {
                    if (Vector2.Distance(newPosition, greenGoalPost) > GoblinMaxKickDistance)
                        return;
                }
                
                this.transform.position = newPosition;
            }
            if(isGoblinDoingKickAfterAttempt && !isAccuracySubmittedYet && !isPowerSubmittedYet && GameplayManager.instance.gamePhase == "kick-after-attempt")
            {
                currentAccuracyGaugeXPosition += Time.deltaTime * (GoblinPowerBarSpeed * 2.0f) * currentAccuracyGaugeDirection;
                if (currentAccuracyGaugeXPosition > 1f)
                {
                    currentAccuracyGaugeXPosition = 1f;
                    currentAccuracyGaugeDirection = -1;
                }
                else if (currentAccuracyGaugeXPosition < -1f)
                {
                    currentAccuracyGaugeXPosition = -1f;
                    currentAccuracyGaugeDirection = 1;
                }
                kickAfterGuageLine.transform.localPosition = new Vector3(currentAccuracyGaugeXPosition, 0f, 0f);
            }
        }
    }
    [ClientCallback]
    private void FixedUpdate()
    {
        if (canGoblinRegainHealth && health < MaxHealth && hasAuthority)
            CmdRegainHealth();
        Move();
    }
    [Client]
    private void SetMovement(Vector2 movement) => previousInput = movement;

    [Client]
    private void ResetMovement() => previousInput = Vector2.zero;

    [Client]
    private void Move()
    {
        if (hasAuthority)
        {
            if (isFatigued)
                CmdRecoverStamina();
            else if ((!isSprinting) && !isFatigued && stamina < MaxStamina)
                CmdRecoverStamina();

            /*if (isGoblinKnockedOut)
                CmdRecoverHealth();*/
        }
        //if (hasAuthority && isCharacterSelected && canGoblinMove && !isGoblinPunching && !isGoblinKnockedOut && !isSliding && !isDiving)
        if (hasAuthority && isCharacterSelected && canGoblinMove && !isGoblinKnockedOut && !isSliding && !isDiving)
        {
            CmdIsPlayerSprinting(isSprinting);
            isRunning = false;
            if (previousInput.x != 0 || previousInput.y != 0)
                isRunning = true;
            CmdSetIsRunningOnServer(isRunning);

            if (isSprinting && !isFatigued)
            {
                CmdDrainStamina();
            }

            //isRunning = false;
            Vector2 direction = Vector2.ClampMagnitude(previousInput, 1);
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
            //rb.velocity = new Vector2(direction.x*speed, direction.y*speed);
            /*if (previousInput.x != 0 || previousInput.y != 0)
                isRunning = true;*/

            if (previousInput.x > 0)
            {
                //Vector3 newScale = new Vector3(1f, 1f, 1f);
                //transform.localScale = newScale;
                myRenderer.flipX = false;
                CmdFlipRenderer(false);
            }
            else if (previousInput.x < 0)
            {
                //Vector3 newScale = new Vector3(-1f, 1f, 1f);
                //transform.localScale = newScale;
                myRenderer.flipX = true;
                CmdFlipRenderer(true);
            }
            //transform.position += direction * speed * Time.deltaTime;

            /*newPosition.y += direction.y * speed * Time.deltaTime;
            newPosition.x += direction.x * speed * Time.deltaTime;
            transform.position = newPosition;*/

            animator.SetBool("isRunning", isRunning);
        }
        else if (hasAuthority && (isSliding || isDiving) && slideDirection != Vector2.zero)
        {
            Vector2 direction = Vector2.ClampMagnitude(slideDirection, 1);
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        }
        if (hasAuthority && !isCharacterSelected && canGoblinMove && !isGoblinKnockedOut && !isSliding && !isDiving && (GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time"))
        {
            // Important thing here is "&& !isCharacterSelected"
            // This will be what the goblin does when they are not selected. AI code will go here?
            // Check if there is a nearby goblin. If so, target them?
            //bool isOpposingGoblinNearBy = FindNearByGoblinToTarget();
            
            if (myGamePlayer.doesTeamHaveBall)
            {
                // AI behaviour when your team has the ball - stay near the player but "behind" them to receive a pass
                state = State.TeamHasBall;
                goblinTarget = null;
            }
            else
            {
                // AI behavior when team does not have the ball - find the ball object and run toward it
                if (gameFootball.isHeld)
                {
                    // if the football is held, the opposing team has the ball. Track that goblin down and punch them?
                    //animator.SetBool("isRunning", false);
                    //MoveTowrdBallCarrier();
                    // Check to see if a new target is needed. If not keep current target
                    bool newTargetNeeded = false;
                    if (goblinTarget != null)
                    {
                        if (goblinTarget.isGoblinKnockedOut)
                            newTargetNeeded = true;
                        if (Vector3.Distance(this.transform.position, goblinTarget.transform.position) > 10f)
                            newTargetNeeded = true;
                    }
                    else
                    {
                        newTargetNeeded = true;
                    }
                    // If a new target is needed, check to see if any goblins are nearby (<10f units away). If none are, then target the ball carrier
                    if (newTargetNeeded)
                    {
                        goblinTarget = null;
                        //Prioritize chasing goblin with ball instead of random goblin
                        if (UnityEngine.Random.Range(0f, 1f) < 0.6f)
                        {
                            state = State.ChaseBallCarrier;
                        }
                        else
                        {
                            bool isOpposingGoblinNearBy = FindNearByGoblinToTarget();
                            if (isOpposingGoblinNearBy)
                            {
                                state = State.AttackNearbyGoblin;
                            }
                            else
                            {
                                state = State.ChaseBallCarrier;
                                //goblinTarget = null;
                            }
                        }                        
                    }
                }
                else
                {
                    //MoveTowardFootball();
                    state = State.ChaseFootball;
                    goblinTarget = null;
                }
            }
            switch (state)
            {
                default:
                case State.ChaseFootball:
                    //MoveTowardFootball();
                    break;
                case State.ChaseBallCarrier:
                    //MoveTowrdBallCarrier();
                    break;
                case State.TeamHasBall:
                    //GetOpenForPass();
                    break;
                case State.AttackNearbyGoblin:
                    //MoveTowardGoblinTarget();
                    break;
            }
        }

    }
    [Command]
    void CmdFlipRenderer(bool flip)
    {
        if(myRenderer.flipX != flip)
            myRenderer.flipX = flip;
        RpcFlipRenderer(flip);
    }
    [ClientRpc]
    void RpcFlipRenderer(bool flip)
    {
        float newScaleX = 0f;
        if (flip)
            newScaleX = -1f;
        else
            newScaleX = 1f;

        if (myRenderer.flipX != flip)
            myRenderer.flipX = flip;
        //Flip colliders for hurt/hitboxes so they match the flipped sprite
        Vector3 newLocalScale = punchBoxCollider.transform.localScale;
        newLocalScale.x = newScaleX;
        punchBoxCollider.transform.localScale = newLocalScale;

        newLocalScale = hurtBoxCollider.transform.localScale;
        newLocalScale.x = newScaleX;
        hurtBoxCollider.transform.localScale = newLocalScale;

        newLocalScale = slideBoxCollider.transform.localScale;
        newLocalScale.x = newScaleX;
        slideBoxCollider.transform.localScale = newLocalScale;

        if (flip)
        {
            Vector3 newScale = new Vector3(-1f, 1f, 1f);
            touchdownHitbox.transform.localScale = newScale;
            divingHitbox.transform.localScale = newScale;
        }
        else
        {
            Vector3 newScale = new Vector3(1f, 1f, 1f);
            touchdownHitbox.transform.localScale = newScale;
            divingHitbox.transform.localScale = newScale;
        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("OnTriggerEnter2D for Goblin");
        if (collision.tag == "football")
        {
            Debug.Log("Goblin collided with football");
        }
        if (collision.tag == "punchbox")
        {
            Debug.Log(this.name + " was punched by: " + collision.transform.parent.name);
            /*if (isServer)
            {
                //this is to disable the body collider to make sure damage isn't done twice in a row too quickly.
                DealDamageToGoblins(this, collision.transform.parent.GetComponent<GoblinScript>());
            }*/
                
        }
    }
    public void SetGoblinHasBall(bool doesHaveBall)
    {
        if (hasAuthority)
            CmdSetGoblinHasBall(doesHaveBall);
    }
    [Command]
    void CmdSetGoblinHasBall(bool doesHaveBall)
    {
        HandleHasBall(this.doesCharacterHaveBall, doesHaveBall);
    }
    public void HandleHasBall(bool oldValue, bool newValue)
    {
        if (isServer)
            doesCharacterHaveBall = newValue;
        if (isClient)
        {
            if (hasAuthority)
            {
                //add check for if goblin is in punch animation. if so, stop the animation?
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName(goblinType + "-punch"))
                {
                    animator.enabled = false;
                    animator.enabled = true;
                }
                animator.SetBool("withFootball", newValue);
                CmdCheckIfTeamStillHasBall();
                CmdSetBallCarrySpeedModifier();
            }
            if (newValue)
            {
                GameObject football = GameObject.FindGameObjectWithTag("football");
                football.transform.SetParent(this.transform);
                football.transform.localPosition = new Vector3(0f, 0f, 0f);
                if (!this.isCharacterSelected)
                {
                    Debug.Log("HandleHasBall: Player that is not selected has the ball: " + this.name);
                    if (hasAuthority)
                    {
                        if (this.isEGoblin)
                        {
                            myGamePlayer.SwitchToEGoblin(false,Time.time);
                        }
                        else if (this.isQGoblin)
                        {
                            myGamePlayer.SwitchToQGoblin(false, Time.time);
                        }
                    }
                }
                
            }
            else
            {
                GameObject football = GameObject.FindGameObjectWithTag("football");
                football.transform.parent = null;
                if (hasAuthority && powerBarActive)
                    ResetPowerBar();
            }
            ballMarkerObject.SetActive(newValue);
        }
    }
    [Command]
    void CmdCheckIfTeamStillHasBall()
    {
        GamePlayer goblinOwnerScript = NetworkIdentity.spawned[ownerNetId].gameObject.GetComponent<GamePlayer>();
        if (this.doesCharacterHaveBall)
        {
            Debug.Log("CmdCheckIfTeamStillHasBall: Goblin has ball. Goblin's team still has ball. Setting doesTeamHaveBall to true for player: " + goblinOwnerScript.PlayerName);
            goblinOwnerScript.doesTeamHaveBall = true;
        }
        else
        {
            bool anyGoblinHaveBall = false;
            foreach (uint goblinNetId in goblinOwnerScript.goblinTeamNetIds)
            {
                GoblinScript goblinNetIdScript = NetworkIdentity.spawned[goblinNetId].gameObject.GetComponent<GoblinScript>();
                if (goblinNetIdScript.doesCharacterHaveBall)
                {
                    anyGoblinHaveBall = true;
                    break;
                }
            }
            Debug.Log("CmdCheckIfTeamStillHasBall: Do any goblins on " + goblinOwnerScript.PlayerName + " team have the ball? " + anyGoblinHaveBall.ToString());
            goblinOwnerScript.doesTeamHaveBall = anyGoblinHaveBall;
        }
    }
    public void ThrowBall(GoblinScript goblinToThrowTo)
    {
        Debug.Log("ThrowBall: Throwing the ball to: " + goblinToThrowTo.gameObject.name);
        Football footballScript = GameObject.FindGameObjectWithTag("football").GetComponent<Football>();
        if (!isKicking && !isDiving)
        {
            if (hasAuthority)
            {
                /*IEnumerator throwing = DisableColliderForThrow();
                //DisableColliderForThrow();
                StartCoroutine(throwing);*/
                golbinBodyCollider.enabled = false;
                CmdStartThrowing();
            }

            footballScript.CmdThrowFootball(this.gameObject.GetComponent<NetworkIdentity>().netId, goblinToThrowTo.gameObject.GetComponent<NetworkIdentity>().netId);
        }
        
    }
    [Command]
    void CmdStartThrowing()
    {
        if(!isKicking && !isDiving)
            this.HandleIsThrowing(isThrowing, true);
    }
    public void HandleIsThrowing(bool oldValue, bool newValue)
    {
        if (isServer)
            isThrowing = newValue;
        if (isClient)
        {
            if (newValue)
            {
                if (throwingRoutine)
                    StopCoroutine(throwing);
                throwing = DisableColliderForThrow();
                //DisableColliderForThrow();
                StartCoroutine(throwing);
            }
            else
            {
                
            }
        }
    }
    public IEnumerator DisableColliderForThrow()
    {
        golbinBodyCollider.enabled = false;
        throwingRoutine = true;
        yield return new WaitForSeconds(0.2f);
        golbinBodyCollider.enabled = true;
        throwingRoutine = false;
        if (hasAuthority)
            CmdStopThrowing();
    }
    [Command]
    void CmdStopThrowing()
    {
        this.HandleIsThrowing(isThrowing, false);
    }
    [Command]
    void CmdMakeGoblinGrey()
    {
        HandleIsGoblinGrey(isGoblinGrey, true);
    }
    public void HandleIsGoblinGrey(bool oldValue, bool newValue)
    {
        if (isServer)
            isGoblinGrey = newValue;
        if (isClient)
        {
            if (newValue)
                animator.SetBool("isGrey", newValue);
            if (newValue && hasAuthority)
            {
                FlipKickoffAimArrow();
            }
        }
    }
    void IsPlayerSprinting(bool isPlayerSprinting)
    {
        if (hasAuthority)
        {

            /*CmdIsPlayerSprinting(isPlayerSprinting);
            isSprinting = isPlayerSprinting;
            if (isPlayerSprinting)
                pressedTime = Time.time;
            else
                releasedTime = Time.time;*/
            if (isBlocking)
                isPlayerSprinting = false;
                
            Debug.Log("IsPlayerSprinting: " + isPlayerSprinting.ToString());
            isSprinting = isPlayerSprinting;
        }
            
    }
    [Command]
    void CmdIsPlayerSprinting(bool isPlayerSprinting)
    {
        //Debug.Log("CmdIsPlayerSprinting: " + isPlayerSprinting.ToString());

        isSprintingOnServer = isPlayerSprinting;

        if (isPlayerSprinting)
        {
            if (!isFatigued)
            {
                if (stamina > 0f)
                {
                    this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer * blockingSpeedModifier * wasPunchedSpeedModifier * speedModifierFromPowerUps * slowDownObstacleModifier) * 1.15f;
                }
                //Update CanRecoverStamina Event here?
                if (isStaminaRecoveryRoutineRunning)
                    StopCoroutine(staminaRecoveryRoutine);
      
                staminaRecoveryRoutine = CanGoblinRecoverStamina();
                StartCoroutine(staminaRecoveryRoutine);                

            }
            else if (isFatigued)
            {
                this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer * blockingSpeedModifier * wasPunchedSpeedModifier * speedModifierFromPowerUps * slowDownObstacleModifier) * 0.5f;
            }
        }        
        else
        {
            if(!isFatigued)
                this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer * blockingSpeedModifier * wasPunchedSpeedModifier * speedModifierFromPowerUps * slowDownObstacleModifier);
            else
                this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer * blockingSpeedModifier * wasPunchedSpeedModifier * speedModifierFromPowerUps * slowDownObstacleModifier) * 0.5f;
        }
            
    }
    [Command]
    void CmdSetIsRunningOnServer(bool isPlayerRunning)
    {
        isRunningOnServer = isPlayerRunning;
    }
    [Command]
    void CmdDrainStamina()
    {
        if (isRunningOnServer)
        {
            if (stamina > 0f)
            {
                stamina -= (Time.fixedDeltaTime * StaminaDrain);
            }
            else
            {
                stamina = 0f;
                //isFatigued = true;
                HandleIsFatigued(isFatigued, true);
                this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer * blockingSpeedModifier * wasPunchedSpeedModifier * speedModifierFromPowerUps * slowDownObstacleModifier) * 0.5f;
            }
        }
        
    }
    public void HandleIsFatigued(bool oldValue, bool newValue)
    {
        if (isServer)
            isFatigued = newValue;
        if (isClient)
        {
            myStatusBars.ChangeStaminaBarColor(newValue);
            if (hasAuthority)
            {
                if (newValue)
                {
                    
                }
                else
                {
                    CmdIsPlayerSprinting(false);
                }

            }            
        }
    }
    [Command]
    void CmdRecoverStamina()
    {
        bool isGoblinAllowedToRecover = false;
        if (isFatigued)
            isGoblinAllowedToRecover = true;
        else if (!isFatigued && canRecoveryStamina)
            isGoblinAllowedToRecover = true;

        if (isGoblinAllowedToRecover)
        {
            if (stamina <= MaxStamina)
            {
                stamina += (Time.fixedDeltaTime * StaminaRecovery);
                if (stamina > MaxStamina)
                {
                    stamina = MaxStamina;
                    HandleIsFatigued(isFatigued, false);
                }
            }
        }        
    }
    [Command]
    void CmdSetBallCarrySpeedModifier()
    {
        if (doesCharacterHaveBall)
            ballCarrySpeedModifier = 0.9f;
        else
            ballCarrySpeedModifier = 1.0f;
    }
    public IEnumerator CantMove()
    {
        Debug.Log("Starting CantMove for " + this.name) ;
        canGoblinMove = false;
        yield return new WaitForSeconds(0.25f);
        canGoblinMove = true;
    }
    public void Attack()
    {
        Debug.Log("Attack: from goblin " + this.name);
        if (!doesCharacterHaveBall && !isGoblinKnockedOut &&!isSliding && !isDiving)
        {
            if (isRunningOnServer)
            {
                animator.Play(goblinType + "-punch-running");
            }
            else
            {
                animator.Play(goblinType + "-punch");
            }
            
        }
    }
    public void HurtBoxCollision(GoblinScript punchingGoblin)
    {
        if (isServer)
        {
            if(this.ownerConnectionId != punchingGoblin.ownerConnectionId)
                DealDamageToGoblins(this, punchingGoblin);
        }
            
    }
    [Server]
    void DealDamageToGoblins(GoblinScript goblinReceivingDamage, GoblinScript goblinGivingDamage)
    {
        Debug.Log("DealDamageToGoblins: " + goblinGivingDamage.name + " is taking damage from " + goblinGivingDamage.name);
        if (!goblinReceivingDamage.isGoblinKnockedOut)
        {
            // If A Goblin is hit while diving, immediately knock them out.
            if (goblinReceivingDamage.isDiving)
            {
                goblinReceivingDamage.health = 0f;
                goblinReceivingDamage.KnockOutGoblin(true);

            }
            else
            {
                float blockingModifier;
                if (goblinReceivingDamage.isBlocking)
                    blockingModifier = 0.5f;
                else
                    blockingModifier = 1.0f;


                float damageDealt = goblinGivingDamage.damage * blockingModifier * defenseModifier;
                Debug.Log("DealDamageToGoblins: Goblin " + goblinReceivingDamage.name + " will receive the following amount of damage: " + damageDealt.ToString());
                goblinReceivingDamage.health -= (goblinGivingDamage.damage * blockingModifier * defenseModifier);
                if (goblinReceivingDamage.health <= 0f)
                {
                    Debug.Log("DealDamageToGoblins: Goblin " + goblinReceivingDamage.name + "'s health is now below 0. Knocking them out.");
                    goblinReceivingDamage.KnockOutGoblin(true);
                }
                else
                {
                    if (isRegainHealthRoutineRunning)
                        StopCoroutine(regainHealthRoutine);

                    regainHealthRoutine = RegainHealth();
                    StartCoroutine(regainHealthRoutine);

                    // Slow down goblin after they are punched
                    if (isWasPunchedRoutineRunning)
                        StopCoroutine(isWasPunched);
                    isWasPunched = WasPunchedRoutine();
                    StartCoroutine(isWasPunched);
                }
            }
            
        }
    }
    [Server]
    public void KnockOutGoblin(bool knockedOut)
    {
        Debug.Log("KnockOutGoblin: " + knockedOut.ToString());
        if (GameplayManager.instance.gamePhase == "kickoff")
            return;
        if (this.health < 0f)
            this.health = 0f;
            

        //Make sure that all the isPunching, isDiving things are false so it doesn't fuck with controls
        /*if (isPunching)
            HandleIsPunching(this.isPunching, false);
        if (isDiving)
        {
            HandleIsDiving(isDiving, false);
            if (!DivingRoutineRunning)
            {
                DivingRoutine = DiveGoblinRoutine();
                StartCoroutine(DivingRoutine);
            }
        }            
        if(isKicking)
            HandleIsKicking(this.isKicking, false);
        if(isThrowing)
            this.HandleIsThrowing(isThrowing, false);
        if (isSliding)
        {
            isSliding = false;
            slideSpeedModifer = 1.0f;
        }
        if (isSlidingRoutineRunning)
        {
            slideDirection = Vector2.zero;
            isSlidingRoutineRunning = false;
        }*/


        HandleIsGoblinKnockedOut(isGoblinKnockedOut, true);
        //isGoblinKnockedOut = true;

        if (isRegainHealthRoutineRunning)
            StopCoroutine(regainHealthRoutine);
        canGoblinRegainHealth = false;

        if (isHealthRecoveryRoutineRunning)
            StopCoroutine(healthRecoveryRoutine);

        if (isTrippedTimerRunning)
            StopCoroutine(trippedTimerRoutine);

        if (knockedOut)
        {
            healthRecoveryRoutine = KnockedOutTimer();
            StartCoroutine(healthRecoveryRoutine);
        }
        else
        {
            trippedTimerRoutine = TripGoblinTimer();
            StartCoroutine(trippedTimerRoutine);
        }
        
        if (doesCharacterHaveBall)
        {
            Debug.Log("KnockOutGoblin: Goblin with football was knocked out. Goblin will need to fumble the football.");
            HandleHasBall(doesCharacterHaveBall, false);
            Football footballScript = GameObject.FindGameObjectWithTag("football").GetComponent<Football>();
            footballScript.FumbleFootball();
        }
        //Code here for ending kick-after-attempt?
        if (GameplayManager.instance.gamePhase == "kick-after-attempt" && this.isKickAfterGoblin)
        {
            GameplayManager.instance.KickAfterAttemptWasBlocked();
            RpcKickBlockedStopKickAfterAttempt(this.connectionToClient);
        }
    }
    [Server]
    public IEnumerator CanGoblinRecoverStamina()
    {
        isStaminaRecoveryRoutineRunning = true;
        canRecoveryStamina = false;
        yield return new WaitForSeconds(1.00f);
        canRecoveryStamina = true;
        isStaminaRecoveryRoutineRunning = false;
    }
    [Server]
    public IEnumerator KnockedOutTimer()
    {
        isHealthRecoveryRoutineRunning = true;
        yield return new WaitForSeconds(4.0f);
        isHealthRecoveryRoutineRunning = false;
        this.health = (MaxHealth * 0.66f);
        //isGoblinKnockedOut = false;
        HandleIsGoblinKnockedOut(isGoblinKnockedOut, false);

        if (isRegainHealthRoutineRunning)
            StopCoroutine(regainHealthRoutine);

        regainHealthRoutine = RegainHealth();
        StartCoroutine(regainHealthRoutine);
    }
    [Server]
    public IEnumerator RegainHealth()
    {
        isRegainHealthRoutineRunning = true;
        canGoblinRegainHealth = false;
        yield return new WaitForSeconds(2.0f);
        isRegainHealthRoutineRunning = false;
        canGoblinRegainHealth = true;

    }
    [Command]
    void CmdRegainHealth()
    {
        if (canGoblinRegainHealth && health < MaxHealth)
        {
            health += 1.0f * Time.fixedDeltaTime;
            if (health >= MaxHealth)
            {
                health = MaxHealth;
                canGoblinRegainHealth = false;
            }
        }
    }
    public void HandleIsGoblinKnockedOut(bool oldValue, bool newValue)
    {
        if (isServer)
            isGoblinKnockedOut = newValue;
        if (isClient)
        {
            golbinBodyCollider.enabled = !newValue;
            myStatusBars.gameObject.SetActive(!newValue);
            if (hasAuthority)
            {
                if (isBlocking)
                    CmdSetBlocking(false);                

                // code for knocked out animation?
                if (newValue)
                {
                    animator.Play(goblinType + "-knocked-out");

                    //Make sure that all the isPunching, isDiving things are false so it doesn't fuck with controls
                    if (isPunching)
                        CmdPunching(false);
                    if (isDiving)
                        CmdStopDiving();
                    if (isThrowing)
                        CmdStopThrowing();
                    if (isSliding)
                    {
                        isSliding = false;
                        slideSpeedModifer = 1.0f;
                        slideDirection = Vector2.zero;
                        isSlidingRoutineRunning = false;
                        StopCoroutine(isSlidingRoutine);
                    }
                    if (isKicking)
                        CmdStopKicking();
                }
                    
                animator.SetBool("isKnockedOut", newValue);

            }
        }
    }
    public void GoblinPickUpFootball()
    {
        if (hasAuthority && !isThrowing)
            CmdGoblinPickUpFootball();
    }
    [Command]
    void CmdGoblinPickUpFootball()
    {
        GameObject.FindGameObjectWithTag("football").GetComponent<Football>().PlayerPickUpFootball(this.GetComponent<NetworkIdentity>().netId);
    }
    public void UpdateGoblinHealth(float oldValue, float newValue)
    {
        if (isServer)
            health = newValue;
        if (isClient)
        {
            float newHealth = newValue;
            if (newHealth < 0f)
                newHealth = 0f;

            myStatusBars.HealthBarUpdate(newHealth / MaxHealth);

        }
    }
    public void UpdateGoblinStamina(float oldValue, float newValue)
    {
        if (isServer)
            stamina = newValue;
        if (isClient)
        {
            float newStamina = newValue;
            if (newStamina < 0f)
                newStamina = 0f;

            myStatusBars.StaminaBarUpdate(newStamina / MaxStamina);

        }
    }
    public void SlideGoblin()
    {
        if (hasAuthority)
        {
            if (isBlocking)
                CmdSetBlocking(false);
            CmdSlideGoblin(previousInput);
        }
            
    }
    [Command]
    void CmdSlideGoblin(Vector2 directionToSlide)
    {
        if (isRunningOnServer && directionToSlide != Vector2.zero && !doesCharacterHaveBall && !isSliding && !isFatigued && !isPunching)
        {
            Debug.Log("CmdSlideGoblin: Goblin will slide in direction of: " + directionToSlide.ToString());
            slideDirection = directionToSlide;
            //isSliding = true;
            if (!isSlidingRoutineRunning && !DivingRoutineRunning)
            {
                isSlidingRoutine = SlideGoblinRoutine();
                StartCoroutine(isSlidingRoutine);
            }
            
        }
    }
    public void HandleIsSliding(bool oldValue, bool newValue)
    {
        if (isServer)
            isSliding = newValue;
        if (isClient)
        {

            if (hasAuthority)
            {
                animator.SetBool("isSliding", newValue);
                if (!newValue)
                {
                    if (!golbinBodyCollider.gameObject.activeInHierarchy)
                        golbinBodyCollider.gameObject.SetActive(true);
                }
            }
        }
    }
    [Server]
    public IEnumerator SlideGoblinRoutine()
    {
        isSlidingRoutineRunning = true;
        isSliding = true;
        yield return new WaitForSeconds(0.25f);
        speed = MaxSpeed * 1.2f;
        //yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(0.25f);
        isSliding = false;
        slideSpeedModifer = 0.7f;
        //yield return new WaitForSeconds(1.25f);
        yield return new WaitForSeconds(2.25f);
        slideSpeedModifer = 1.0f;
        yield return new WaitForSeconds(0.75f);
        slideDirection = Vector2.zero;
        isSlidingRoutineRunning = false;
    }
    public void SlideBoxCollision(GoblinScript slidingGoblin)
    {
        if (isServer)
        {
            if (this.ownerConnectionId != slidingGoblin.ownerConnectionId)
                slidingGoblin.TripGoblin();
        }
    }
    public void TripGoblin()
    {
        if (!isGoblinKnockedOut)
        {
            Debug.Log("TripGoblin: Tripping goblin: " + this.name);
            this.health -= 10f;
            if (this.health <= 0f)
            {
                this.KnockOutGoblin(true);
            }
            else
            {
                this.KnockOutGoblin(false);
            }
        }

    }
    [Server]
    public IEnumerator TripGoblinTimer()
    {
        isTrippedTimerRunning = true;
        yield return new WaitForSeconds(1.0f);
        isTrippedTimerRunning = false;

        HandleIsGoblinKnockedOut(isGoblinKnockedOut, false);

        if (isRegainHealthRoutineRunning)
            StopCoroutine(regainHealthRoutine);

        regainHealthRoutine = RegainHealth();
        StartCoroutine(regainHealthRoutine);
    }
    public void DiveGoblin()
    {
        if (hasAuthority)
        {
            if (isRunning)
            {
                if (!isGoblinKnockedOut && !isSliding && !isDiving && hasAuthority && !isPunching)
                {
                    CmdStartDiving(previousInput);
                    if (isBlocking)
                        CmdSetBlocking(false);
                }
            }
        }
    }
    [Command]
    void CmdStartDiving(Vector2 directionToDive)
    {
        if (isRunningOnServer && directionToDive != Vector2.zero && !isSliding && !isFatigued && !isDiving && !DivingRoutineRunning && !isSlidingRoutineRunning && !isPunching)
        {
            Debug.Log("CmdStartDiving: Goblin will slide in direction of: " + directionToDive.ToString());
            slideDirection = directionToDive;
            HandleIsDiving(isDiving, true);
            speed = MaxSpeed * 1.2f;
        }
            
    }
    public void HandleIsDiving(bool oldValue, bool newValue)
    {
        if (isServer)
            isDiving = newValue;
        if (isClient)
        {

            if (hasAuthority && newValue)
            {
                
                if (doesCharacterHaveBall)
                {
                    animator.Play(goblinType + "-dive-with-football");
                }
                else
                {
                    animator.Play(goblinType + "-dive");
                }
            }
            if (hasAuthority && !newValue)
            {
                if (!golbinBodyCollider.gameObject.activeInHierarchy)
                    golbinBodyCollider.gameObject.SetActive(true);
            }
        }
    }
    public void StopDiving()
    {
        if (hasAuthority)
        {
            CmdStopDiving();
        }
    }
    [Command]
    void CmdStopDiving()
    {
        if (isDiving)
        {
            HandleIsDiving(isDiving, false);
            if (!DivingRoutineRunning)
            {
                DivingRoutine = DiveGoblinRoutine();
                StartCoroutine(DivingRoutine);
            }
            /*if (GameplayManager.instance.gamePhase == "xtra-time")
            {
                if (this.doesCharacterHaveBall)
                    GameplayManager.instance.DidWinningGoblinDiveInXtraTimeToEndGame(this.isGoblinGrey);
            }*/
        }
    }
    [Server]
    public IEnumerator DiveGoblinRoutine()
    {
        DivingRoutineRunning = true;
        slideSpeedModifer = 0.7f;
        yield return new WaitForSeconds(1.25f);
        slideSpeedModifer = 1.0f;
        yield return new WaitForSeconds(0.75f);
        slideDirection = Vector2.zero;
        DivingRoutineRunning = false;
    }
    public void StartBlocking()
    {
        if (hasAuthority)
        {
            CmdSetBlocking(true);
            // stop player from sprinting when blocking
            if (isSprinting)
                IsPlayerSprinting(false);
        }
            
    }
    public void StopBlocking()
    {
        if (hasAuthority)
        {
            CmdSetBlocking(false);
            if(Input.GetKey(KeyCode.LeftShift))
                IsPlayerSprinting(true);
        }
            
    }
    [Command]
    void CmdSetBlocking(bool isGoblinBlocking)
    {
        Debug.Log("CmdSetBlocking: Set blocking to " + isGoblinBlocking.ToString() + " for goblin " + this.name);
        //isBlocking = isGoblinBlocking;
        if (!isSliding && !isDiving && !isGoblinKnockedOut)
        {
            if (isGoblinBlocking)
                blockingSpeedModifier = 0.8f;
            else
                blockingSpeedModifier = 1.0f;
            HandleIsBlocking(this.isBlocking, isGoblinBlocking);
        }
        else
        {
            blockingSpeedModifier = 1.0f;
            HandleIsBlocking(this.isBlocking, false);
        }
            

    }
    public void HandleIsBlocking(bool oldValue, bool newValue)
    {
        if (isServer)
            isBlocking = newValue;
        if (isClient)
        {
            if (hasAuthority)
            {
                animator.SetBool("isBlocking", newValue);
                if (newValue)
                {
                    if (doesCharacterHaveBall)
                    {
                        animator.Play(goblinType + "-block-with-football");
                    }
                    else
                    {
                        animator.Play(goblinType + "-block");
                    }
                    
                }
                else
                {
                    golbinBodyCollider.gameObject.SetActive(false);
                    golbinBodyCollider.gameObject.SetActive(true);
                }
            }
        }
    }
    public void StartPunching()
    {
        if (hasAuthority)
            CmdPunching(true);
    }
    public void StopPunching()
    {
        if (hasAuthority)
            CmdPunching(false);
    }
    [Command]
    void CmdPunching(bool punching)
    {
        //isPunching = punching;
        HandleIsPunching(isPunching, punching);
    }
    public void HandleIsPunching(bool oldValue, bool newValue)
    {
        if (isServer)
            isPunching = newValue;
        if (isClient)
        {
            if (hasAuthority)
            {
                if (newValue && isBlocking)
                    CmdSetBlocking(false);
            }
        }
    }
    public void KickFootballGoblin(float kickPower, float kickAngle)
    {
        if (hasAuthority && !isPunching && !isDiving && !isSliding && !isGoblinKnockedOut && doesCharacterHaveBall && !isThrowing)
        {
            CmdKickFootball(kickPower, kickAngle);
        }
            
    }
    [Command]
    void CmdKickFootball(float kickPower, float kickAngle)
    {
        if (!isPunching && !isDiving && !isSliding && !isGoblinKnockedOut && doesCharacterHaveBall && !isThrowing && kickPower <= 1f && kickPower >= 0f)
        {
            if (GameplayManager.instance.gamePhase == "kick-after-attempt")
                powerValueSubmitted = kickPower;
            HandleIsKicking(this.isKicking, true);
            GoblinKickPower = kickPower;
            GoblinKickoffAngle = kickAngle;
        }
    }
    public void HandleIsKicking(bool oldValue, bool newValue)
    {
        if (isServer)
            isKicking = newValue;
        if (isClient)
        {
            Debug.Log("HandleIsKicking: isClient");
            if (hasAuthority)
            {
                Debug.Log("HandleIsKicking: hasAuthority");
                if (newValue)
                {
                    Debug.Log("HandleIsKicking: start the kick animation");
                    animator.Play(goblinType + "-kick-football");
                }
                    
            }
            
        }
    }
    public void KickTheFootball()
    {
        Debug.Log("KickTheFootball");
        if (hasAuthority && !isPunching && !isDiving && !isSliding && !isGoblinKnockedOut && doesCharacterHaveBall && !isThrowing)
            CmdKickTheFootball();
    }
    [Command]
    void CmdKickTheFootball()
    {
        HandleHasBall(doesCharacterHaveBall, false);
        Football footballScript = GameObject.FindGameObjectWithTag("football").GetComponent<Football>();
        Vector3 newLocalPosition = footballScript.transform.localPosition;
        if (myRenderer.flipX)
        {
            newLocalPosition.x -= 1.0f;
        }
        else
            newLocalPosition.x += 1.0f;
        footballScript.transform.localPosition = newLocalPosition;

        if (GameplayManager.instance.gamePhase == "kick-after-attempt")
        {
            footballScript.KickAfterAttemptKick(isGoblinGrey, powerValueSubmitted, angleOfKickAttempt, GoblinMaxKickDistance, GoblinMinKickDistance, kickAfterAccuracyDifficulty, accuracyValueSubmitted, kickAfterFinalPosition);
            return;
        }

        footballScript.KickFootballDownField(isGoblinGrey, GoblinKickPower, GoblinKickoffAngle, GoblinMaxKickDistance, GoblinMinKickDistance);
        
    }
    public void StopKicking()
    {
        Debug.Log("StopKicking");
        if (hasAuthority)
            CmdStopKicking();
    }
    [Command]
    void CmdStopKicking()
    {
        HandleIsKicking(this.isKicking, false);
    }
    public void HandleCanGoblinReceivePass(bool oldValue, bool newValue)
    {
        if (isServer)
            canGoblinReceivePass = newValue;
        if (isClient)
        {
            if (hasAuthority)
            {
                qMarker.GetComponent<QEMarkerScript>().UpdateSpriteForPassing(newValue);
                eMarker.GetComponent<QEMarkerScript>().UpdateSpriteForPassing(newValue);
            }
            
        }
    }
    public void StartKickPower()
    {
        KickPowerBarHolder.SetActive(true);
        powerBarActive = true;
    }
    public void StopKickPower()
    {
        KickPowerBarHolder.SetActive(false);
        powerBarActive = false;
        KickFootballGoblin(KickPowerBarFillerImage.transform.localScale.x, kickoffAngle);
        ResetPowerBar();
        if (GameplayManager.instance.gamePhase == "kickoff")
        {
            KickoffAimArrow.SetActive(false);
            KickoffAimArrow.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            kickoffAngle = 0f;
        }
            

    }
    void ResetPowerBar()
    {
        KickPowerBarHolder.SetActive(false);
        Vector3 resetScale = new Vector3 (0f, 1f, 1f);
        KickPowerBarFillerImage.transform.localScale = resetScale;
        powerBarActive = false;
        currentPowerBarScale = 0f;
        powerBarDirection = 1;
        //if(hasAuthority)
            //CmdResetKickPower();
    }
    /*[Command]
    void CmdResetKickPower()
    {
        GoblinKickPower = 0f;
    }*/
    [Server]
    public IEnumerator WasPunchedRoutine()
    {
        isWasPunchedRoutineRunning = true;
        wasPunchedSpeedModifier = 0.8f;
        yield return new WaitForSeconds(3.0f);
        wasPunchedSpeedModifier = 1.0f;
        isWasPunchedRoutineRunning = false;
    }
    public void ToggleGoblinBodyCollider()
    {
        golbinBodyCollider.gameObject.SetActive(false);
        golbinBodyCollider.gameObject.SetActive(true);
    }
    void FlipKickoffAimArrow()
    {
        Vector3 newPosition = KickoffAimArrow.transform.localPosition;
        newPosition.x *= -1;
        KickoffAimArrow.transform.localPosition = newPosition;
        KickoffAimArrow.GetComponent<SpriteRenderer>().flipX = true;
    }
    public void EnableKickoffAimArrow(bool activate)
    {
        KickoffAimArrow.SetActive(activate);
    }
    public void StartAimArrowDirection(bool aimUp)
    {
        aimArrowButtonHeldDown = true;
        aimArrowUp = aimUp;
    }
    public void EndAimArrowDirection()
    {
        aimArrowButtonHeldDown = false;
    }
    public void KickAfterRepositioning(bool moveLeft)
    {
        repositioningButtonHeldDown = true;
        repositioningToLeft = moveLeft;
    }
    public void EndKickAfterRepositioning()
    {
        repositioningButtonHeldDown = false;
    }
    public void CheckIfGoblinNeedsToFlipForKickAfter(bool isPlayerKicking)
    {
        if (this.isGoblinGrey && !myRenderer.flipX)
        {
            this.CmdFlipRenderer(true);
        }
        else if (!this.isGoblinGrey && myRenderer.flipX)
        {
            this.CmdFlipRenderer(false);
        }
    }
    [Server]
    void CalculateKickAfterAccuracyDifficulty(float angle)
    {
        //kickAfterAccuracyDifficulty = 1 - ((angle / 100) * 3);
        float newDifficulty = 1 - ((angle / 100) * 3);
        if (newDifficulty > 0.9f)
            newDifficulty = 0.9f;
        if (newDifficulty < 0.1f)
            newDifficulty = 0.1f;
        //HandleKickAfterAccuracyDifficultyUpdate(this.kickAfterAccuracyDifficulty, (1 - ((angle / 100) * 3)));
        HandleKickAfterAccuracyDifficultyUpdate(this.kickAfterAccuracyDifficulty, newDifficulty);
        //kickAfterAccuracyBar1 = 0.5f - (kickAfterAccuracyDifficulty / 2);
        //kickAfterAccuracyBar2 = 0.5f + (kickAfterAccuracyDifficulty / 2);
    }
    public void ActivateKickAfterAccuracyBar(bool activate)
    {
        kickAfterAccuracyBar.SetActive(activate);
        kickAfterGuageLine.SetActive(false);
    }
    void HandleKickAfterAccuracyDifficultyUpdate(float oldValue, float newValue)
    {
        if (isServer)
            kickAfterAccuracyDifficulty = newValue;
        if (isClient)
        {
            if (hasAuthority)
            {
                Vector3 newPosition = new Vector3(newValue, 0f, 0f);
                kickAfterMarkerRight.transform.localPosition = newPosition;
                newPosition.x *= -1;
                kickAfterMarkerLeft.transform.localPosition = newPosition;

            }
        }
    }
    public void SubmitKickAfterPositionToServer()
    {
        if (hasAuthority)
            CmdSubmitKickAfterPositionToServer(transform.position);

    }
    [Command]
    void CmdSubmitKickAfterPositionToServer(Vector3 kickAfterPosition)
    {
        if (Vector2.Distance(kickAfterPosition, this.transform.position) > 3.0f)
            return;

        kickAfterFinalPosition = kickAfterPosition;
        isKickAfterPositionSet = true;
        GameplayManager.instance.DisableKickAfterPositioningControls();
        GameplayManager.instance.StartKickAfterTimer();
        this.RpcKickAfterPositionFromServer(this.connectionToClient, kickAfterPosition);
    }
    [TargetRpc]
    void RpcKickAfterPositionFromServer(NetworkConnection target, Vector2 newPosition)
    {
        if (hasAuthority)
            this.transform.position = newPosition;
    }
    public void StartKickAfterKickAttempt()
    {
        isGoblinDoingKickAfterAttempt = true;
        isAccuracySubmittedYet = false;
        isPowerSubmittedYet = false;
        kickAfterGuageLine.transform.localPosition = new Vector3(-1f, 0f, 0f);
        currentAccuracyGaugeXPosition = -1f;
        kickAfterGuageLine.SetActive(true);
        //kickAfterMoveAccuracyGuageLineRoutine = KickAfterMoveAccuracyGuageLine();
        //StartCoroutine(kickAfterMoveAccuracyGuageLineRoutine);
    }
    IEnumerator KickAfterMoveAccuracyGuageLine()
    {
        int directionModifer = 1;
        Vector3 newLocalPosition = kickAfterGuageLine.transform.localPosition;
        while (!isAccuracySubmittedYet)
        {
            yield return new WaitForSeconds(0.05f);
            newLocalPosition.x += 0.1f * directionModifer;
            if (newLocalPosition.x >= 1.0f)
            {
                newLocalPosition.x = 1.0f;
                directionModifer = -1;
            }
            else if (newLocalPosition.x <= -1f)
            {
                newLocalPosition.x = -1f;
                directionModifer = 1;
            }
            kickAfterGuageLine.transform.localPosition = newLocalPosition;
        }
        yield break;
    }
    public void SubmitKickAfterKicking()
    {
        if (!isAccuracySubmittedYet)
        {
            isAccuracySubmittedYet = true;
            if (hasAuthority)
                CmdSubmitKickAfterAccuracyValue(kickAfterGuageLine.transform.localPosition.x);
            StartKickAfterPower();
            //StopCoroutine(kickAfterMoveAccuracyGuageLineRoutine);
        }
        else if (isAccuracySubmittedYet && !isPowerSubmittedYet)
        {
            StopKickPower();
        }
            
    }
    void StartKickAfterPower()
    {
        Debug.Log("StartKickAfterPower");
        kickAfterGuageLine.SetActive(false);
        kickAfterAccuracyBar.SetActive(false);
        StartKickPower();
    }
    [Command]
    void CmdSubmitKickAfterAccuracyValue(float accuracyValue)
    {
        Debug.Log("CmdSubmitKickAfterAccuracyValue: " + accuracyValue);
        accuracyValueSubmitted = accuracyValue;
    }
    [TargetRpc]
    void RpcKickBlockedStopKickAfterAttempt(NetworkConnection target)
    {
        /*if (isGoblinDoingKickAfterAttempt || isKickAfterGoblin)
        {
            
        }*/
        isGoblinDoingKickAfterAttempt = false;
        kickAfterGuageLine.SetActive(false);
        kickAfterAccuracyBar.SetActive(false);
        ResetPowerBar();

    }
    [ServerCallback]
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameplayManager.instance.gamePhase == "kick-after-attempt")
        {
            if (collision.collider.tag == "Goblin")
            {
                GoblinScript collidingGoblin = collision.collider.GetComponent<GoblinScript>();
                if (collidingGoblin.isKickAfterGoblin && collidingGoblin.isGoblinGrey != this.isGoblinGrey && collidingGoblin.isKickAfterPositionSet)
                {
                    Debug.Log("GoblinScript: OnCollisionEnter2D: The colliding goblin is the kick after goblin! Goblin: " + collidingGoblin.name);
                    collidingGoblin.KickAfterGoblinWasRunInto();
                }
                /*if (this.isKickAfterGoblin)
                {
                    Debug.Log("GoblinScript: OnCollisionEnter2D: THIS GOBLIN is the kick after goblin! And another goblin ran into it! This goblin: " + this.name + " colliding Goblin: " + collidingGoblin.name);
                    KickAfterGoblinWasRunInto();
                }*/
            }
        }
    }
    [Server]
    void KickAfterGoblinWasRunInto()
    {
        this.KnockOutGoblin(false);
        RpcKickBlockedStopKickAfterAttempt(this.connectionToClient);
    }
    [ServerCallback]
    public void StartAttackNormal()
    {
        IEnumerator attackNormalRoutine = AttackNormalRoutine();
        StartCoroutine(attackNormalRoutine);
    }
    [ServerCallback]
    IEnumerator AttackNormalRoutine()
    {
        this.damage = MaxDamage * 1.5f;
        attackNormal = true;
        yield return new WaitForSeconds(3.0f);
        this.damage = MaxDamage;
        attackNormal = false;
    }
    [ServerCallback]
    public void StartDefenseNormal()
    {
        IEnumerator defenseNormalRoutine = DefenseNormalRoutine();
        StartCoroutine(defenseNormalRoutine);
    }
    [ServerCallback]
    IEnumerator DefenseNormalRoutine()
    {
        defenseModifier = 0.5f;
        defenseNormal = true;
        yield return new WaitForSeconds(3.0f);
        defenseModifier = 1.0f;
        defenseNormal = false;
    }
    [ServerCallback]
    public void StartSpeedNormal()
    {
        IEnumerator speedNormalRoutine = SpeedNormalRoutine();
        StartCoroutine(speedNormalRoutine);
    }
    [ServerCallback]
    IEnumerator SpeedNormalRoutine()
    {
        speedModifierFromPowerUps = 1.2f;
        speedNormal = true;
        yield return new WaitForSeconds(3.0f);
        speedModifierFromPowerUps = 1.0f;
        speedNormal = false;
    }
    [ClientRpc]
    public void RpcPlayPowerUpParticle(string particleType)
    {
        GameObject newParticle = Instantiate(PowerUpParticleSystemPrefab, this.transform);
        newParticle.GetComponent<PowerUpParticleSystem>().StartParticleSystem(particleType);
    }
    [Server]
    public void StartHealNormal()
    {
        //Revive Goblin if they are knocked out
        if (this.isGoblinKnockedOut)
        {
            if (isHealthRecoveryRoutineRunning)
            {
                isHealthRecoveryRoutineRunning = false;
                StopCoroutine(healthRecoveryRoutine);
            }
            if (isTrippedTimerRunning)
            {
                isTrippedTimerRunning = false;
                StopCoroutine(trippedTimerRoutine);
            }
            
            
            //this.health = (MaxHealth * 0.25f);
            //isGoblinKnockedOut = false;
            HandleIsGoblinKnockedOut(isGoblinKnockedOut, false);
            if (isRegainHealthRoutineRunning)
                StopCoroutine(regainHealthRoutine);
            regainHealthRoutine = RegainHealth();
            StartCoroutine(regainHealthRoutine);
        }
        this.UpdateGoblinHealth(this.health, this.MaxHealth);
    }
    void MoveTowardFootball()
    {
        // Reset the ai script for targetting goblin
        myGoblinAIPathFindingScript.goblinTarget = null;
        myGoblinAIPathFindingScript.isTargetingAGoblin = false;

        float distToFootball = Vector3.Distance(this.transform.position, gameFootball.transform.position);

        // Get the direction of the football in relation to the goblin
        Vector3 directionToFootball = Vector3.zero;
        Vector3 positionOfFootball = Vector3.zero;
        if (gameFootball.isKicked)
        {
            GameObject footBallLandingSpot = GameObject.FindGameObjectWithTag("footballLandingTarget");
            positionOfFootball = footBallLandingSpot.transform.position;
            directionToFootball = (positionOfFootball - this.transform.position).normalized;
            
        }
        else
        {
            positionOfFootball = gameFootball.transform.position;
            directionToFootball = (positionOfFootball - this.transform.position).normalized;
        }

        if (distToFootball > 0.2f)
        {
            AIMoveTowardDirection(directionToFootball, positionOfFootball);
        }
            
        /*Vector2 direction = Vector2.ClampMagnitude(directionToFootball, 1);

        // Set whether the goblin is moving. If the "direction" to the football is 0, then they shouldn't be moving?
        isRunning = false;
        if (direction.x != 0 || direction.y != 0)
            isRunning = true;

        // Move the goblin toward the football
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

        // check the direction the goblin is moving. If they are moving left, make sprite face left. If right, sprite face right
        if (direction.x > 0)
        {
            myRenderer.flipX = false;
            CmdFlipRenderer(false);
        }
        else if (direction.x < 0)
        {
            myRenderer.flipX = true;
            CmdFlipRenderer(true);
        }
        animator.SetBool("isRunning", isRunning);*/
    }
    void MoveTowrdBallCarrier()
    {
        // Opposing team has the ball. Find the ball carrier and move toward them. If the AI is close enough, they should punch them
        /*GoblinScript goblinWithBall = null;
        try {
            goblinWithBall = gameFootball.transform.parent.GetComponent<GoblinScript>();
        }
        catch (Exception e)
        {
            Debug.Log("MoveTowrdBallCarrier: could not find goblin script of oglbin with the football. Reaseon: " + e);
        }
        if (goblinWithBall != null)*/

        // check how far away the goblin with the ball is. If they are within 1, punch
        //float distToBall = Vector3.Distance(this.transform.position, goblinWithBall.transform.position);
        //float distToBall = Vector3.Distance(this.transform.position, gameFootball.transform.position);

        Vector3 myPosition = this.transform.position;
        Vector3 targetPosition = gameFootball.transform.position;

        float distanceToTarget = Vector3.Distance(myPosition, targetPosition);
        
        bool slide = WillGoblinSlideTowardTarget(distanceToTarget);

        if (slide)
        {
            SlideTowardGoblinTaget(targetPosition, myPosition);
        }
        else
        {
            bool punch = WillGoblinPunchTarget(distanceToTarget, myPosition.y, targetPosition.y);
            if (punch)
            {
                PunchGoblinTarget(targetPosition, myPosition);
            }
            // Move goblin toward their target
            Vector3 diretionToBallCarrier = (targetPosition - myPosition).normalized;
            if (distanceToTarget > minDistanceFromTarget)
            {
                // Get the goblin with the football to target them with the AI script
                goblinTarget = NetworkIdentity.spawned[gameFootball.goblinWithBallNetId].GetComponent<GoblinScript>();
                myGoblinAIPathFindingScript.goblinTarget = goblinTarget;
                myGoblinAIPathFindingScript.isTargetingAGoblin = true;
                AIMoveTowardDirection(diretionToBallCarrier, targetPosition);
            }                
            else
            {
                isRunning = false;
                animator.SetBool("isRunning", isRunning);
            }
        }

        // First check if the goblin will slide. If not, then check if they will punch.
        /*if (distToBall < slideRange && Time.time > nextSlideTime && UnityEngine.Random.Range(0f, 1f) > 0.66f)
        {
            slide = true;
        }
        if (distToBall < punchRange && Time.time > nextPunchTime && !slide)
            punch = true;
        else
            punch = false;*/
        // verify you aren't punching your teammate
        /*if (goblinWithBall.isGoblinGrey == this.isGoblinGrey)
            punch = false;*/
        //Debug.Log("MoveTowrdBallCarrier: punch is " + punch.ToString() + " dist to ball is " + distToBall.ToString() + " time versus nextPunchTime " + Time.time.ToString() + " " + nextPunchTime.ToString());
        /*if (punch || slide)
        {
            Debug.Log("MoveTowrdBallCarrier: will punch now");
            // ball carrier is close enough to punch. Start punching!
            //First check to make sure you are facing the right direction to punch
            if (gameFootball.transform.position.x < this.transform.position.x && !this.myRenderer.flipX)
                CmdFlipRenderer(true);
            else if (gameFootball.transform.position.x > this.transform.position.x && this.myRenderer.flipX)
                CmdFlipRenderer(false);

            if (punch)
            {
                this.Attack();
                nextPunchTime = Time.time + punchRate;
            }
            if (slide)
            {
                Debug.Log("MoveTowrdBallCarrier: will SLIDE now");
                Vector3 directionToMoveTo = (gameFootball.transform.position - this.transform.position).normalized;
                Vector2 direction = Vector2.ClampMagnitude(directionToMoveTo, 1);
                CmdSlideGoblin(direction);
                nextSlideTime = Time.time + slideRate;
            }
        }

        if (!slide)
        {
            // Move goblin to the ball carrier
            Vector3 diretionToBallCarrier = (gameFootball.transform.position - this.transform.position).normalized;
            if (distToBall > 1.5f)
                AIMoveTowardDirection(diretionToBallCarrier);
            else
            {
                isRunning = false;
                animator.SetBool("isRunning", isRunning);
            }
        }*/
                   
    }
    void AIMoveTowardDirection(Vector3 directionToMoveTo, Vector3 targetPosition)
    {
        //Vector2 direction = Vector2.ClampMagnitude(directionToMoveTo, 1);
        Vector2 direction = this.GetComponent<GoblinAIPathFinding>().AIMoveTowardDirection(Vector2.ClampMagnitude(directionToMoveTo, 1), targetPosition);

        preDirection = directionToMoveTo;
        postDirection = direction;

        // Set whether the goblin is moving. If the "direction" to the football is 0, then they shouldn't be moving?
        isRunning = false;
        if (direction.x != 0 || direction.y != 0)
            isRunning = true;
        CmdSetIsRunningOnServer(isRunning);
        // Check if a goblin has had an "adjacent" goblin for more than 0.4 seconds and needs to dive to get unstuck?
        /*bool doesGoblinNeedtoDive = IsThereAnAdjacentGoblin();
        if (doesGoblinNeedtoDive && Time.time > nextDiveTime)
        {
            CmdStartDiving(directionToMoveTo);
            nextDiveTime = Time.time + diveRate;
            return;
        }
        else
        {
            // Move the goblin toward the football
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        }*/
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

        // check the direction the goblin is moving. If they are moving left, make sprite face left. If right, sprite face right
        if (direction.x > 0)
        {
            myRenderer.flipX = false;
            CmdFlipRenderer(false);
        }
        else if (direction.x < 0)
        {
            myRenderer.flipX = true;
            CmdFlipRenderer(true);
        }
        animator.SetBool("isRunning", isRunning);
    }
    void GetOpenForPass()
    {
        // Should not have a goblin target, so resetting that on the AI script
        myGoblinAIPathFindingScript.goblinTarget = null;
        myGoblinAIPathFindingScript.isTargetingAGoblin = false;
        //Find where teammate with ball is. Goblin will then make sure to get "behind" the ball carrier so they can receive a pass
        //myGamePlayer.selectGoblin
        int directionModifier = -1;
        if (this.isGoblinGrey)
            directionModifier = 1;

        // Check to see if the saved position is too far away from the current ball carrier. If it is, get a new position
        Vector3 ballCarrierPosition = myGamePlayer.selectGoblin.transform.position;
        Vector3 currentPosition = this.transform.position;
        bool newPositionToRunTo = false;
        bool isBehindBallCarrier = false;

        // check if the current position to run to is too far away from the ball carrier or not
        float distToBallCarrier = Vector3.Distance(positionToRunTo, ballCarrierPosition);
        if (distToBallCarrier > 10f)
            newPositionToRunTo = true;
        // Check to see if the goblin is close to their new position. If they are close enough, find a new one
        float distToRunTo = Vector3.Distance(positionToRunTo, currentPosition);
        if (distToRunTo <= 0.25f)
            newPositionToRunTo = true;        
        //check to make sure the current position the goblin is moving toward is "behind" the ball carrier so they can receive a pass. If they are infront of the ball carrier, they will need a new position to run to
        if (this.isGoblinGrey && positionToRunTo.x > ballCarrierPosition.x)
        {
            isBehindBallCarrier = true;
        }
        else if (!this.isGoblinGrey && positionToRunTo.x < ballCarrierPosition.x)
        {
            isBehindBallCarrier = true;
        }
        else
        {
            newPositionToRunTo = true;
            isBehindBallCarrier = false;
        }

        // If newPositionToRunTo is true, get the new position
        if (newPositionToRunTo)
        {
            bool goblinBetween = false;
            // First, check to see if a whole new position is needed, or to see if the goblin should just "stay the course" and keep its current Y position and just move forward
            if (distToRunTo <= 0.25f)
            {
                // This will detect if any enemy goblins are between the goblin and the ball carrier. If yes, a new position will be chosen? The goblinLayerMask is the goblin-body layer
                
                Vector3 direction = (ballCarrierPosition - currentPosition).normalized;
                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, distToBallCarrier, goblinLayerMask);
                if (hits.Length > 0)
                {
                    for (int i = 0; i < hits.Length; i++)
                    {
                        GoblinScript goblin = hits[i].collider.transform.parent.GetComponent<GoblinScript>();
                        if (goblin.isGoblinGrey != this.isGoblinGrey)
                        {
                            // Opposing goblin found between ball carrier and this goblin. New position will be needed
                            //Debug.Log("GetOpenForPass: found goblin between " + this.name + " and the ball carrier. It is " + goblin.name);
                            goblinBetween = true;
                            break;
                        }
                    }
                }
                if (goblinBetween)
                {
                    // There is a goblin between this goblin and the ball carrier. Find a new position entirely
                    //Debug.Log("GetOpenForPass: Goblin found between " + this.name + " and ball carrier");
                    //Debug.Log("GetOpenForPass: Getting new position for " + this.name + " Was there a goblin between them? " + goblinBetween.ToString());
                    ballCarrierPosition.x += (UnityEngine.Random.Range(2.5f, 7f) * directionModifier);
                    if (ballCarrierPosition.x > fieldMaxX)
                        ballCarrierPosition.x = (fieldMaxX - UnityEngine.Random.Range(0f, 4.0f));
                    else if (ballCarrierPosition.x < fieldMinX)
                        ballCarrierPosition.x = (fieldMinX + UnityEngine.Random.Range(0f, 4.0f));

                    int negOrPos = UnityEngine.Random.Range(0, 2) * 2 - 1;
                    ballCarrierPosition.y += (UnityEngine.Random.Range(0f, 4.0f) * negOrPos);
                    if (ballCarrierPosition.y > fieldMaxY)
                        ballCarrierPosition.y = (fieldMaxY - UnityEngine.Random.Range(0f, 3.0f));
                    else if (ballCarrierPosition.y < fieldMinY)
                        ballCarrierPosition.y = (fieldMinY + UnityEngine.Random.Range(0f, 3.0f));
                }
                else
                {
                    // NO goblin was found between. Just change the x position of the goblin to move forward. Y position can remain the same.
                    //Debug.Log("GetOpenForPass: !!!NO!!! Goblin found between " + this.name + " and ball carrier");
                    if (Mathf.Abs(currentPosition.x - ballCarrierPosition.x) <= 5.0f && (isBehindBallCarrier))
                    {
                        ballCarrierPosition.x = currentPosition.x;
                    }
                    else
                    {
                        ballCarrierPosition.x += (UnityEngine.Random.Range(2.5f, 7f) * directionModifier);
                        if (ballCarrierPosition.x > fieldMaxX)
                            ballCarrierPosition.x = (fieldMaxX - UnityEngine.Random.Range(0f, 4.0f));
                        else if (ballCarrierPosition.x < fieldMinX)
                            ballCarrierPosition.x = (fieldMinX + UnityEngine.Random.Range(0f, 4.0f));
                    }                    

                    // KEEP current Y position to run to
                    ballCarrierPosition.y = currentPosition.y;
                }
            }
            else
            {
                // There is a goblin between this goblin and the ball carrier. Find a new position entirely
                //Debug.Log("GetOpenForPass: Goblin found between " + this.name + " and ball carrier");
                //Debug.Log("GetOpenForPass: Getting new position for " + this.name + " Was there a goblin between them? " + goblinBetween.ToString());
                ballCarrierPosition.x += (UnityEngine.Random.Range(2.5f, 7f) * directionModifier);
                if (ballCarrierPosition.x > fieldMaxX)
                    ballCarrierPosition.x = (fieldMaxX - UnityEngine.Random.Range(0f, 4.0f));
                else if (ballCarrierPosition.x < fieldMinX)
                    ballCarrierPosition.x = (fieldMinX + UnityEngine.Random.Range(0f, 4.0f));

                int negOrPos = UnityEngine.Random.Range(0, 2) * 2 - 1;
                ballCarrierPosition.y += (UnityEngine.Random.Range(0f, 4.0f) * negOrPos);
                if (ballCarrierPosition.y > fieldMaxY)
                    ballCarrierPosition.y = (fieldMaxY - UnityEngine.Random.Range(0f, 3.0f));
                else if (ballCarrierPosition.y < fieldMinY)
                    ballCarrierPosition.y = (fieldMinY + UnityEngine.Random.Range(0f, 3.0f));
            }
            /*else
            {
                // NO goblin was found between. Just change the x position of the goblin to move forward. Y position can remain the same.
                Debug.Log("GetOpenForPass: !!!NO!!! Goblin found between " + this.name + " and ball carrier");
                ballCarrierPosition.x += (UnityEngine.Random.Range(2.5f, 7f) * directionModifier);
                if (ballCarrierPosition.x > fieldMaxX)
                    ballCarrierPosition.x = (fieldMaxX - UnityEngine.Random.Range(0f, 4.0f));
                else if (ballCarrierPosition.x < fieldMinX)
                    ballCarrierPosition.x = (fieldMinX + UnityEngine.Random.Range(0f, 4.0f));

                // KEEP current Y position to run to
                ballCarrierPosition.y = currentPosition.y;
            }*/
            positionToRunTo = ballCarrierPosition;
        }
        //Get the new position that the goblin will go to to try and be behind the player        

        //Debug.Log("GetOpenForPass: Moving goblin: " + this.name + " toward position: " + ballCarrierPosition.ToString());
        Vector3 diretionToBallCarrier = (positionToRunTo - currentPosition).normalized;
        AIMoveTowardDirection(diretionToBallCarrier, positionToRunTo);
    }
    [ClientRpc]
    public void RpcResetGoblinStatuses()
    {
        if (hasAuthority)
        {
            if (isPunching)
                CmdPunching(false);
            if (isDiving)
                CmdStopDiving();
            if (isThrowing)
                CmdStopThrowing();
            if (isSliding)
            {
                isSliding = false;
                slideSpeedModifer = 1.0f;
                slideDirection = Vector2.zero;
                isSlidingRoutineRunning = false;
                StopCoroutine(isSlidingRoutine);
            }
            if (isBlocking)
                CmdSetBlocking(false);
            if (isKicking)
                CmdStopKicking();

            animator.SetBool("isRunning", false);
            animator.SetBool("isSliding", false);
            animator.SetBool("isBlocking", false);
            if (!this.doesCharacterHaveBall)
                animator.SetBool("withFootball", false);
        }        
    }
    bool FindNearByGoblinToTarget()
    {
        // Get all goblins within 10f of this goblin
        bool goblinNearBy = false;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, 10f, goblinLayerMask);
        if (colliders.Length > 0)
        {
            foreach (Collider2D goblinBodyCollider in colliders)
            {
                // Check if the goblin is on the opposing team or not
                GoblinScript goblinBodyColliderScript = goblinBodyCollider.transform.parent.GetComponent<GoblinScript>();
                if (goblinBodyColliderScript.isGoblinGrey == this.isGoblinGrey)
                {
                    // Goblins are on the same team. Skip to next goblin
                    continue;
                }
                if (goblinBodyColliderScript.isGoblinKnockedOut)
                    continue;
                else
                {
                    // If it is an opposing goblin, check to see if any other goblin on your team is targetting that goblin. If not, target them
                    foreach (GoblinScript goblinOnMyTeam in myGamePlayer.goblinTeam)
                    {
                        // Don't check for this goblin or on the goblin the player has selected
                        if (goblinOnMyTeam == this || goblinOnMyTeam.isCharacterSelected)
                            continue;
                        if (goblinOnMyTeam.goblinTarget != goblinBodyColliderScript)
                        {
                            this.goblinTarget = goblinBodyColliderScript;
                            goblinNearBy = true;
                            break;
                        }
                    }
                }
            }
        }
        else
            goblinNearBy = false;
        return goblinNearBy;
    }
    void MoveTowardGoblinTarget()
    {
        if (goblinTarget != null)
        {
            // Check for distance to goblin target. If they are within punch/slide range, then punch or slide them until they are knocked out?
            bool didGoblinSlide = false;
            bool didGoblinPunch = false;

            Vector3 myPosition = this.transform.position;
            Vector3 targetPosition = goblinTarget.transform.position;
            float distanceToTarget = Vector3.Distance(myPosition, targetPosition);

            // Check if the goblin will slide at their target
            didGoblinSlide = WillGoblinSlideTowardTarget(distanceToTarget);

            // If didGoblinSlide is true, slide the goblin toward their target
            if (didGoblinSlide)
            {
                SlideTowardGoblinTaget(targetPosition, myPosition);
            }
            else // Goblin did not slide. Check to see if they can punch. Move goblin toward their target as well??
            {
                // Check to see if goblin is close enough to their target to punch them
                didGoblinPunch = WillGoblinPunchTarget(distanceToTarget, myPosition.y, targetPosition.y);
                if (didGoblinPunch)
                {
                    PunchGoblinTarget(targetPosition, myPosition);
                }

                // Move goblin toward their target
                Vector3 diretionToBallCarrier = (targetPosition - myPosition).normalized;
                if (distanceToTarget > minDistanceFromTarget)
                {
                    // Provide the AI script with the goblin target info
                    myGoblinAIPathFindingScript.goblinTarget = goblinTarget;
                    myGoblinAIPathFindingScript.isTargetingAGoblin = true;
                    AIMoveTowardDirection(diretionToBallCarrier, targetPosition);
                }                    
                else
                {
                    isRunning = false;
                    animator.SetBool("isRunning", isRunning);
                }
            }
            
        }
    }
    bool WillGoblinSlideTowardTarget(float distanceToTarget)
    {
        bool willSlide = false;

        if (distanceToTarget < this.slideRange && Time.time > this.nextSlideTime && UnityEngine.Random.Range(0f, 1f) > 0.66f)
        {
            willSlide = true;
        }

        return willSlide;
    }
    bool WillGoblinPunchTarget(float distanceToTarget, float myYPosition, float targetYPosition)
    {
        bool willPunch = false;

        if (distanceToTarget < this.punchRange && Time.time > this.nextPunchTime && Mathf.Abs(myYPosition - targetYPosition) <= punchYMax)
            willPunch = true;

        return willPunch;
    }
    void PunchGoblinTarget(Vector3 target, Vector3 myPosition)
    {
        // Make sure sprite is in the correct direction
        //Vector3 myPosition = this.transform.position;
        if (target.x < myPosition.x && !this.myRenderer.flipX)
            CmdFlipRenderer(true);
        else if (target.x > myPosition.x && this.myRenderer.flipX)
            CmdFlipRenderer(false);

        // Do the punch
        this.Attack();
        nextPunchTime = Time.time + punchRate;
    }
    void SlideTowardGoblinTaget(Vector3 target, Vector3 myPosition)
    {
        // Make sure sprite is in the correct direction
        //Vector3 myPosition = this.transform.position;
        if (target.x < myPosition.x && !this.myRenderer.flipX)
            CmdFlipRenderer(true);
        else if (target.x > myPosition.x && this.myRenderer.flipX)
            CmdFlipRenderer(false);

        // Do the slide 
        Vector3 directionToMoveTo = (target - myPosition).normalized;
        Vector2 direction = Vector2.ClampMagnitude(directionToMoveTo, 1);
        CmdSlideGoblin(direction);
        nextSlideTime = Time.time + slideRate;
    }
    bool IsThereAnAdjacentGoblin()
    {
        // Get all goblins within 10f of this goblin
        bool needToDive = false;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, (minDistanceFromTarget + 0.15f), goblinLayerMask);
        if (colliders.Length > 0)
        {
            bool anyGoblinsThatArentMe = false;
            foreach (Collider2D goblinBodyCollider in colliders)
            {
                if (goblinBodyCollider.transform == this.transform)
                    continue;
                else
                {
                    anyGoblinsThatArentMe = true;
                    break;
                }
            }
            if (anyGoblinsThatArentMe)
            {
                // adjacent goblin that wasn't this transform was found
                if (adjacentGoblinTime > 0f)
                {
                    if ((adjacentGoblinTime + 0.4f) > Time.time)
                    {
                        // goblin has been adjacent for 0.4 seconds. Tell goblin to dive
                        needToDive = true;
                        adjacentGoblinTime = 0f;
                    }

                }
                else
                {
                    // first time a goblin has been found nearby. Set adjacentGoblinTime and return false
                    adjacentGoblinTime = Time.time;
                    needToDive = false;
                }
            }
        }
        else
        {
            // no goblins found adjacent. Reset goblin time counter and return false;
            adjacentGoblinTime = 0f;
            needToDive = false;
        }
            
        return needToDive;
    }
    public void UpdateHasGoblinRepositionedForKickAfter()
    {
        if (hasAuthority)
            CmdUpdateHasGoblinRepositionedForKickAfter();
    }
    [Command]
    void CmdUpdateHasGoblinRepositionedForKickAfter()
    {
        this.hasGoblinBeenRepositionedForKickAfter = true;
        GameplayManager.instance.AreAllGoblinsRepositionedForKickAfter();
    }
    [ServerCallback]
    public void SlowDownObstacleEffect(bool stillColliding)
    {
        if (stillColliding)
            slowDownObstacleModifier = 0.5f;
        else
            slowDownObstacleModifier = 1.0f;
    }
    [ServerCallback]
    public void KickAfterWaitToEnableObstacleColliders()
    {
        canCollide = false;
        IEnumerator waitToEnableObstacleColliders = WaitToEnableObstacleColliders();
        StartCoroutine(waitToEnableObstacleColliders);
    }
    [ServerCallback]
    IEnumerator WaitToEnableObstacleColliders()
    {
        yield return new WaitForSeconds(0.666f);
        canCollide = true;
    }

}
