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
        RunTowardEndzone,
        BlockKick,
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
    public string soundType;


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
    [SyncVar] public float possessionSpeedBonus = 1.0f;


    [Header("Character selection stuff")]
    [SyncVar(hook = nameof(HandleCharacterSelected))] public bool isCharacterSelected = false;
    [SyncVar(hook = nameof(HandleIsQGoblin))] public bool isQGoblin = false;
    [SyncVar(hook = nameof(HandleIsEGoblin))] public bool isEGoblin = false;
    public bool isQGoblinLocally3v3 = false;
    public bool isEGoblinLocally3v3 = false;
    public bool canGoblinMove = true;
    public bool isGoblinPunching = false;
    [SyncVar(hook = nameof(HandleIsPunching))] public bool isPunching = false;
    bool hasGoblinGottenBallBefore = false;
    
    
    [SerializeField] private GameObject eMarkerPrefab;
    [SerializeField] private GameObject qMarkerPrefab;
    [SerializeField] private GameObject eMarker;
    [SerializeField] private GameObject qMarker;
    [SerializeField] private GameObject ballMarkerPrefab;
    [SerializeField] private GameObject ballMarkerOpponentPrefab;
    private GameObject ballMarkerObject;
    [SerializeField] private GameObject youMarkerPrefab;
    private GameObject youMarkerObject;

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
    [SerializeField] private SpriteRenderer myShadow;
    [SerializeField] private SpriteRenderer mySelectedCircle;
    [SerializeField] private ParticleSystem sprintParticleSystem;


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
    [SyncVar(hook = nameof(HandleIsSlidingRoutineRunning))] public bool isSlidingRoutineRunning = false;
    IEnumerator isSlidingRoutine;
    public float slideSpeedModifer = 1.0f;

    [Header("Dive Info")]
    [SyncVar(hook = nameof(HandleIsDiving))] public bool isDiving = false;
    [SyncVar(hook = nameof(HandleIsDivingRoutineRunning))] public bool DivingRoutineRunning = false;
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
    public bool repositioningButtonHeldDown = false;
    public bool repositioningToLeft = false;
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
    [SyncVar(hook = nameof(HandleWasPunchedRoutineRunning))] public bool isWasPunchedRoutineRunning = false;
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
    public bool invinvibilityBlueShell;

    [Header("Sprite Effects?")]
    [SerializeField] private SpriteFlash spriteFlash;
    [SerializeField] private GameObject fatigueSweatDrop;
    [SerializeField] private GameObject wasPunchedBandAid;

    [Header("SFX Stuff")]
    [SyncVar] public bool onWaterSlowDown;
    [SyncVar] public bool onBrushSlowDown;
    [SyncVar] public bool onGlueSlowDown;
    public bool firstFootStep = false;
    [SerializeField] GoblinSoundManager mySoundManager;

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
    public GoblinAIPathFinding myGoblinAIPathFindingScript;

    [Header("AI Sprinting Parameters")]
    public bool didGoblinCompleteSprint = false;
    public bool willGoblinSprintToFatigue = false;
    public bool canGoblinSprint = true;
    public float minStaminaToSprintTo = 1.0f;

    [Header("AI Stuck Stuff")]
    public bool isStuck = false;
    public float stuckCheckTime = 0f;
    public float stuckCheckRate = 0.5f;
    public Vector3 lastCheckedPosition;
    public Vector2 stuckDirectionChange = Vector2.zero;

    [Header("AI Teammate Collision")]
    public bool teammateCollision = false;
    public Vector2 teammateCollisionDirectionModifer = Vector2.zero;

    [Header("Dive Over Obstacle Checks")]
    public float diveCheckTime = 0f;
    public float diveCheckRate = 0.25f;


    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        GamePlayer localPlayer = GameObject.FindGameObjectWithTag("LocalGamePlayer").GetComponent<GamePlayer>();
        if (localPlayer && !localPlayer.isSinglePlayer)
        {
            myGamePlayer = localPlayer;
            myGamePlayer.AddToGoblinTeam(this);
            if (GameplayManager.instance.is1v1)
            {
                if (!myGamePlayer.IsGameLeader)
                    CmdMakeGoblinGrey();
            }
            else
            {
                if (myGamePlayer.isTeamGrey)
                    CmdMakeGoblinGrey();
            }


            //GoblinStartingPosition(myGamePlayer.IsGameLeader);
            if (transform.position.x > 0f)
                CmdFlipRenderer(true);
        }
        else if (localPlayer && localPlayer.isSinglePlayer)
        {
            GamePlayer aiPlayer = null;
            try
            {
                aiPlayer = GameObject.FindGameObjectWithTag("GamePlayer").GetComponent<GamePlayer>();
            }
            catch (Exception e)
            {
                Debug.Log("OnStartAuthority for " + this.name + " could not find AIPlayer object. " + e);
            }
            if (this.ownerNetId == localPlayer.GetComponent<NetworkIdentity>().netId)
            {
                myGamePlayer = localPlayer;
                myGamePlayer.AddToGoblinTeam(this);
            }
            else
            {
                myGamePlayer = aiPlayer;
                myGamePlayer.AddToGoblinTeam(this);
            }
            if (myGamePlayer.isTeamGrey)
                CmdMakeGoblinGrey();
            if (transform.position.x > 0f)
                CmdFlipRenderer(true);
        }

        myGoblinAIPathFindingScript = this.GetComponent<GoblinAIPathFinding>();

        if (this.myGamePlayer.isSinglePlayer && !this.myGamePlayer.isLocalPlayer)
        {
            this.AIResetSprintingParameters();
            //youMarkerObject.SetActive(false);
            return;
        }
            

        InputManager.Controls.Player.Move.performed += ctx => SetMovement(ctx.ReadValue<Vector2>());
        InputManager.Controls.Player.Move.canceled += ctx => ResetMovement();

        InputManager.Controls.Player.Sprint.performed += _ => IsPlayerSprinting(true);
        InputManager.Controls.Player.Sprint.canceled += _ => IsPlayerSprinting(false);
        EnableGoblinMovement(false);

        
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
        {
            if (!GameplayManager.instance.isSinglePlayer)
            {
                ballMarkerObject = Instantiate(ballMarkerPrefab);
                youMarkerObject = Instantiate(youMarkerPrefab);
                mySelectedCircle.color = Color.white;
            }
            else
            {
                if (this.ownerConnectionId != -1)
                {
                    ballMarkerObject = Instantiate(ballMarkerPrefab);
                    youMarkerObject = Instantiate(youMarkerPrefab);
                    mySelectedCircle.color = Color.white;
                }
                else
                {
                    youMarkerObject = new GameObject("empty-you-marker");
                    ballMarkerObject = Instantiate(ballMarkerOpponentPrefab);
                    mySelectedCircle.color = Color.yellow;
                }
            }
        }
        else
        {
            ballMarkerObject = Instantiate(ballMarkerOpponentPrefab);
            mySelectedCircle.color = Color.yellow;
        }
            

        ballMarkerObject.transform.SetParent(this.transform);
        Vector3 markerPosition = myStatusBars.transform.localPosition;
        markerPosition.y += 0.75f;
        ballMarkerObject.transform.localPosition = markerPosition;
        ballMarkerObject.SetActive(false);
        if (hasAuthority)
        {
            youMarkerObject.transform.SetParent(this.transform);
            youMarkerObject.transform.localPosition = markerPosition;
            youMarkerObject.SetActive(false);
        }
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
                    Debug.Log("HandleCharacterSelected: Following new selected character " + this.name + " is this goblin grey? " + this.isGoblinGrey.ToString());
                    if (GameplayManager.instance.isSinglePlayer)
                    { 
                        if(myGamePlayer.isLocalPlayer)
                            myGamePlayer.FollowSelectedGoblin(this.transform);
                    }
                    else
                        myGamePlayer.FollowSelectedGoblin(this.transform);
                    try
                    {
                        if (this.myGamePlayer.is1v1 || this.myGamePlayer.isSinglePlayer)
                        {
                            this.isSprinting = false;
                            this.AIResetSprintingParameters();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("HandleCharacterSelected: could not access myGamePlayer. Error: " + e);
                    }
                    
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
                if(!this.doesCharacterHaveBall && youMarkerObject != null && this.myGamePlayer.isLocalPlayer)
                    youMarkerObject.SetActive(newValue);

            }
        }
    }
    public void SetQGoblin(bool isQ)
    {
        Debug.Log("SetQGoblin " + isQ.ToString());
        if (hasAuthority)
        {
            if(GameplayManager.instance.is1v1 || GameplayManager.instance.isSinglePlayer)
                CmdSetQGoblin(isQ);
            else
                SetQGoblinLocally3v3(isQ);
            if (isBlocking)
                CmdSetBlocking(false);
        }
            
    }
    [Command]
    void CmdSetQGoblin(bool isQ)
    {
        Debug.Log("CmdSetQGoblin " + isQ.ToString() + " "  + this.name + " is grey? " + this.isGoblinGrey.ToString());
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
                    if (GameplayManager.instance.isSinglePlayer && !this.myGamePlayer.isLocalPlayer)
                    {
                        qMarker.SetActive(false);
                        return;
                    }
                    qMarker.SetActive(newValue);
                }
            }
        }
    }
    public void SetQGoblinLocally3v3(bool enable)
    {
        Debug.Log("SetQGoblinLocally3v3: " + enable.ToString() + " for goblin " + this.name);
        qMarker.SetActive(enable);
        if (!enable)
        {
            qMarker.GetComponent<QEMarkerScript>().DeactivateGoblinMarkerOnCamera();
        }
        /*if (!this.doesCharacterHaveBall)
            qMarker.SetActive(true);*/
        if (this.doesCharacterHaveBall)
            qMarker.SetActive(false);
        if (!this.isQGoblinLocally3v3)
            this.isQGoblinLocally3v3 = true;
    }
    public void SetEGoblin(bool isE)
    {
        Debug.Log("SetEGoblin " + isE.ToString() + " " + this.name);
        if (hasAuthority)
        {
            if (GameplayManager.instance.is1v1 || GameplayManager.instance.isSinglePlayer)
                CmdSetEGoblin(isE);
            else
                SetEGoblinLocally3v3(isE);
            if (isBlocking)
                CmdSetBlocking(false);
        }
            
    }
    [Command]
    void CmdSetEGoblin(bool isE)
    {
        Debug.Log("CmdSetEGoblin " + isE.ToString() + " is grey? " + this.isGoblinGrey.ToString());
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
                if (GameplayManager.instance.isSinglePlayer && !this.myGamePlayer.isLocalPlayer)
                {
                    eMarker.SetActive(false);
                    return;
                }
                eMarker.SetActive(newValue);
            }
        }
    }
    // Only set E goblin locally for 3v3 games since it will be different for each player on the team. No need (or ability to) sync the Q/E goblins between players on same team.
    public void SetEGoblinLocally3v3(bool enable)
    {
        Debug.Log("SetEGoblinLocally3v3: " + enable.ToString() + " for goblin " + this.name);
        eMarker.SetActive(enable);
        if (!enable)
        {
            eMarker.GetComponent<QEMarkerScript>().DeactivateGoblinMarkerOnCamera();
        }
        /*if(!this.doesCharacterHaveBall)
            eMarker.SetActive(true);*/
        if (this.doesCharacterHaveBall)
            qMarker.SetActive(false);
        if (!this.isEGoblinLocally3v3)
            this.isEGoblinLocally3v3 = true;
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
        if (hasAuthority && isCharacterSelected && canGoblinMove && !isGoblinKnockedOut && !isSliding && !isDiving && !(GameplayManager.instance.isSinglePlayer && !this.myGamePlayer.isLocalPlayer) && GameplayManager.instance.gamePhase != "kickoff")
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
                    /*bool newTargetNeeded = false;
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
                        bool targetBallCarrier = AIShouldGoblinTargetBallCarrier();
                        if (targetBallCarrier)
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
                            }
                        }
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
                    }*/
                    AIGetGoblinTarget();
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
                    MoveTowardFootball();
                    break;
                case State.ChaseBallCarrier:
                    MoveTowrdBallCarrier();
                    break;
                case State.TeamHasBall:
                    GetOpenForPass();
                    break;
                case State.AttackNearbyGoblin:
                    MoveTowardGoblinTarget();
                    break;
            }
        }
        // Check for the AI character for what to do with their goblin that they have selected?
        else if (hasAuthority && this.isCharacterSelected && !this.myGamePlayer.isLocalPlayer && this.myGamePlayer.isSinglePlayer && canGoblinMove && !isGoblinKnockedOut && !isSliding && !isDiving && (GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time"))
        {
            if (this.doesCharacterHaveBall)
                state = State.RunTowardEndzone;
            else if (gameFootball.isHeld && !this.doesCharacterHaveBall)
            {
                AIGetGoblinTarget();
            }   
            else
                state = State.ChaseFootball;

            switch (state)
            {
                default:
                case State.ChaseFootball:
                    MoveTowardFootball();
                    break;
                case State.ChaseBallCarrier:
                    MoveTowrdBallCarrier();
                    break;
                case State.TeamHasBall:
                    GetOpenForPass();
                    break;
                case State.AttackNearbyGoblin:
                    MoveTowardGoblinTarget();
                    break;
                case State.RunTowardEndzone:
                    myGamePlayer.myAiPlayer.RunTowardEndZone();
                    break;
            }

        }

    }
    public void FlipRenderer(bool flip)
    { 
        if(hasAuthority)
        {
            if (this.myRenderer.flipX != flip)
                CmdFlipRenderer(flip);
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
        if (myShadow.flipX != flip)
            myShadow.flipX = flip;
        // Remove quotes if removing the animated selection circle?
        /*if (mySelectedCircle.flipX != flip)
            mySelectedCircle.flipX = flip;*/

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

        FlipFatigueSweatIndicator();
        /*Vector3 newPosition = fatigueSweatDrop.transform.localPosition;
        newPosition.x *= -1;
        fatigueSweatDrop.transform.localPosition = newPosition;*/

        try
        {
            sprintParticleSystem.transform.localScale = new Vector3(newScaleX, 1f, 1f);
        }
        catch (Exception e)
        {
            Debug.Log("PlaySprintDust: Could not play particle effect. Error: " + e);
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
                if(newValue && youMarkerObject != null)
                    youMarkerObject.SetActive(false);
                else if(!newValue && this.isCharacterSelected && youMarkerObject != null && myGamePlayer.isLocalPlayer)
                    youMarkerObject.SetActive(true);
                try
                {
                    if (this.myGamePlayer.isSinglePlayer && !this.myGamePlayer.isLocalPlayer)
                    {
                        if (!newValue)
                        {
                            this.isSprinting = false;
                            this.AIResetSprintingParameters();
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("HandleHasBall: Could not get myGamePlayer variable. Error: " + e);
                }

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
                if (!hasGoblinGottenBallBefore)
                {
                    if (GameplayManager.instance.is1v1 || GameplayManager.instance.isSinglePlayer)
                        hasGoblinGottenBallBefore = true;
                    else if (hasAuthority)
                        hasGoblinGottenBallBefore = true;
                    else
                    {
                        // If you're here, it is a 3v3 game and this is a goblin not controlled by the local player. Check if this goblin is on the same team as the local player. If yes, set ball marker stuff to white. If not, markers should already be yellow
                        // remove this if it's too confusing?
                        try
                        {
                            GamePlayer localPlayer = GameObject.FindGameObjectWithTag("LocalGamePlayer").GetComponent<GamePlayer>();
                            if (localPlayer.isTeamGrey == this.isGoblinGrey)
                            {
                                Debug.Log("HandleHasBall: hasGoblinGottenBallBefore is false and goblin is on same team as local player. Goblin: " + this.name + ":" + this.ownerConnectionId.ToString() + " local player number: " + localPlayer.playerNumber.ToString());
                                if (ballMarkerObject)
                                {
                                    GameObject oldMarker = ballMarkerObject;
                                    Destroy(oldMarker);
                                    ballMarkerObject = null;
                                }
                                ballMarkerObject = Instantiate(ballMarkerPrefab);
                                ballMarkerObject.transform.SetParent(this.transform);
                                Vector3 markerPosition = myStatusBars.transform.localPosition;
                                markerPosition.y += 0.75f;
                                ballMarkerObject.transform.localPosition = markerPosition;
                                ballMarkerObject.SetActive(false);
                                mySelectedCircle.color = Color.white;
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log("HandleHasBall: failed to get if goblin is on same team as local player for 3v3 game. Error: " + e);
                        }
                        hasGoblinGottenBallBefore = true;
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
            if (!GameplayManager.instance.is1v1)
            {
                if (isEGoblinLocally3v3)
                    SetEGoblinLocally3v3(!newValue);
                if (isQGoblinLocally3v3)
                    SetQGoblinLocally3v3(!newValue);
            }
            
            ballMarkerObject.SetActive(newValue);
            mySelectedCircle.gameObject.SetActive(newValue);
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
            if (!GameplayManager.instance.is1v1)
            {
                if (goblinOwnerScript.isTeamGrey)
                {
                    foreach (GamePlayer player in TeamManager.instance.greyTeam.teamPlayers)
                    {
                        player.doesTeamHaveBall = true;
                    }
                }
                else
                {
                    foreach (GamePlayer player in TeamManager.instance.greenTeam.teamPlayers)
                    {
                        player.doesTeamHaveBall = true;
                    }
                }
            }
        }
        else
        {
            bool anyGoblinHaveBall = false;
            List<uint> goblinNetIds = new List<uint>();
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
            if (!GameplayManager.instance.is1v1)
            {
                if (goblinOwnerScript.isTeamGrey)
                {
                    foreach (GamePlayer player in TeamManager.instance.greyTeam.teamPlayers)
                    {
                        player.doesTeamHaveBall = anyGoblinHaveBall;
                    }
                }
                else
                {
                    foreach (GamePlayer player in TeamManager.instance.greenTeam.teamPlayers)
                    {
                        player.doesTeamHaveBall = anyGoblinHaveBall;
                    }
                }
            }
        }
    }
    public void ThrowBall(GoblinScript goblinToThrowTo)
    {
        Debug.Log("ThrowBall: Throwing the ball to: " + goblinToThrowTo.gameObject.name);
        Football footballScript = GameObject.FindGameObjectWithTag("football").GetComponent<Football>();
        if (!isKicking && !isDiving && !this.isGoblinKnockedOut && this.doesCharacterHaveBall)
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
                
            Debug.Log("IsPlayerSprinting: " + isPlayerSprinting.ToString() + " for goblin: " + this.name + " owned by player " + this.ownerName + ":" + this.ownerConnectionId.ToString());
            if (this.isCharacterSelected)
                isSprinting = isPlayerSprinting;
            else
                isSprinting = false;
        }
            
    }
    [Command]
    void CmdIsPlayerSprinting(bool isPlayerSprinting)
    {
        //Debug.Log("CmdIsPlayerSprinting: " + isPlayerSprinting.ToString());

        isSprintingOnServer = isPlayerSprinting;
        // Get speed modifer for the possession bonus from this goblin's player owner. Possession bonus is divided by 3. So, of possession bonus is 20% (aka 1.2f) then it should make them 6.7% faster (1.067f)
        /*float possessionSpeedBonus = (this.serverGamePlayer.possessionBonus - 1.0f);
        if (possessionSpeedBonus > 0)
        {
            possessionSpeedBonus /= 3f;            
        }
        possessionSpeedBonus += 1.0f;*/

        //Debug.Log("CmdIsPlayerSprinting: possessionSpeedBonus is: " + possessionSpeedBonus.ToString() + " for goblin: " + this.name + " owned by " + this.serverGamePlayer.PlayerName);

        if (isPlayerSprinting)
        {
            if (!isFatigued)
            {
                if (stamina > 0f)
                {
                    this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer * blockingSpeedModifier * wasPunchedSpeedModifier * speedModifierFromPowerUps * slowDownObstacleModifier * possessionSpeedBonus) * 1.15f;
                }
                //Update CanRecoverStamina Event here?
                if (isStaminaRecoveryRoutineRunning)
                    StopCoroutine(staminaRecoveryRoutine);
      
                staminaRecoveryRoutine = CanGoblinRecoverStamina();
                StartCoroutine(staminaRecoveryRoutine);                

            }
            else if (isFatigued)
            {
                this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer * blockingSpeedModifier * wasPunchedSpeedModifier * speedModifierFromPowerUps * slowDownObstacleModifier * possessionSpeedBonus) * 0.5f;
            }
        }        
        else
        {
            if(!isFatigued)
                this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer * blockingSpeedModifier * wasPunchedSpeedModifier * speedModifierFromPowerUps * slowDownObstacleModifier * possessionSpeedBonus);
            else
                this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer * blockingSpeedModifier * wasPunchedSpeedModifier * speedModifierFromPowerUps * slowDownObstacleModifier * possessionSpeedBonus) * 0.5f;
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
        if (isRunningOnServer && !this.invinvibilityBlueShell)
        {
            if (stamina > 0f)
            {
                stamina -= (Time.fixedDeltaTime * (StaminaDrain / this.serverGamePlayer.possessionBonus));
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
            /*fatigueSweatDrop.SetActive(newValue);
            if(newValue)
                spriteFlash.Flash(Color.yellow);*/
            FatigueIndicators(newValue);
            if (hasAuthority)
            {
                if (this.myGamePlayer.isSinglePlayer && !myGamePlayer.isLocalPlayer)
                {
                    if (newValue)
                    {
                        this.canGoblinSprint = false;
                    }
                    else
                    {
                        //this.canGoblinSprint = true;
                        this.AIResetSprintingParameters();
                    }
                }
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
                stamina += (Time.fixedDeltaTime * StaminaRecovery * this.serverGamePlayer.possessionBonus);
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
        if (doesCharacterHaveBall && !this.invinvibilityBlueShell)
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
            /*if(this.ownerConnectionId != punchingGoblin.ownerConnectionId)
                DealDamageToGoblins(this, punchingGoblin);*/
            if (this.isGoblinGrey != punchingGoblin.isGoblinGrey)
                DealDamageToGoblins(this, punchingGoblin);
        }
        if (isClient)
        {
            if (this.isGoblinGrey != punchingGoblin.isGoblinGrey)
            {
                // play sound based on punching goblin?
                if (punchingGoblin.isGoblinOnScreen())
                {
                    SoundManager.instance.PlaySound("hit-by-" + punchingGoblin.soundType, 1.0f);
                    if (punchingGoblin.soundType == "berserker")
                        SoundManager.instance.StopSound("berserker-swing");
                }
                if (this.isGoblinOnScreen())
                {
                    if(this.hasAuthority && this.isCharacterSelected)
                        this.mySoundManager.PlaySound(this.soundType + "-hit", 1.0f);
                    else
                        this.mySoundManager.PlaySound(this.soundType + "-hit", 0.8f);
                }
            }
        }
            
    }
    [Server]
    void DealDamageToGoblins(GoblinScript goblinReceivingDamage, GoblinScript goblinGivingDamage)
    {
        Debug.Log("DealDamageToGoblins: " + goblinGivingDamage.name + " is taking damage from " + goblinGivingDamage.name);
        if (goblinReceivingDamage.invinvibilityBlueShell)
            return;
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
                goblinReceivingDamage.health -= (goblinGivingDamage.damage * (blockingModifier / goblinReceivingDamage.serverGamePlayer.possessionBonus) * defenseModifier);
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

                    // Slow down goblin after they are punched, ONLY IF they are NOT blocking?
                    if (!this.isBlocking && !this.defenseNormal)
                    {
                        if (isWasPunchedRoutineRunning)
                            StopCoroutine(isWasPunched);
                        isWasPunched = WasPunchedRoutine();
                        StartCoroutine(isWasPunched);
                    }
                    
                }
            }
            /*if (goblinGivingDamage.isCharacterSelected)
            {
                TeamManager.instance.PunchHit(goblinGivingDamage.serverGamePlayer);
            }*/
            TeamManager.instance.PunchHit(goblinGivingDamage);
            goblinReceivingDamage.RpcPunchedFlashSprite();
        }
    }
    [Server]
    public void KnockOutGoblin(bool knockedOut)
    {
        Debug.Log("KnockOutGoblin: " + knockedOut.ToString());
        if (this.invinvibilityBlueShell)
            return;
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
            if (GameplayManager.instance.gamePhase != "kick-after-attempt")
                TeamManager.instance.FumbleBall(this.serverGamePlayer.isTeamGrey);
        }
        //Code here for ending kick-after-attempt?
        if (GameplayManager.instance.gamePhase == "kick-after-attempt" && this.isKickAfterGoblin && !gameFootball.isKicked)
        {
            GameplayManager.instance.KickAfterAttemptWasBlocked();
            RpcKickBlockedStopKickAfterAttempt(this.connectionToClient);
        }
        else
        {
            TeamManager.instance.KnockedOutOrTripped(this.serverGamePlayer.isTeamGrey, knockedOut);
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
        yield return new WaitForSeconds(3.5f);
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
            health += (2.0f * this.serverGamePlayer.possessionBonus) * Time.fixedDeltaTime;
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
                        /*isSliding = false;
                        slideSpeedModifer = 1.0f;
                        slideDirection = Vector2.zero;
                        StopCoroutine(isSlidingRoutine);
                        isSlidingRoutineRunning = false;*/
                        CmdStopSliding();
                    }
                    if (isKicking)
                        CmdStopKicking();
                }
                    
                animator.SetBool("isKnockedOut", newValue);

            }
            if (newValue && isGoblinOnScreen())
            {
                if(hasAuthority && this.isCharacterSelected)
                    mySoundManager.PlaySound("goblin-knocked-out", 0.75f);
                else
                    mySoundManager.PlaySound("goblin-knocked-out", 0.4f);
            }
            else
            {
                mySoundManager.StopSound("goblin-knocked-out");
            }
        }
    }
    [Command]
    void CmdStopSliding()
    {
        isSliding = false;
        slideSpeedModifer = 1.0f;
        slideDirection = Vector2.zero;
        if(isSlidingRoutineRunning)
            StopCoroutine(isSlidingRoutine);
        this.HandleIsSlidingRoutineRunning(this.isSlidingRoutineRunning, false);
    }
    public void GoblinPickUpFootball()
    {
        if (hasAuthority && !isThrowing)
            CmdGoblinPickUpFootball();
    }
    [Command]
    void CmdGoblinPickUpFootball()
    {
        Debug.Log("CmdGoblinPickUpFootball: for goblin: " + this.name + " owned by player " + this.serverGamePlayer.PlayerName);
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
            try
            {
                if (hasAuthority)
                {
                    if (this.myGamePlayer.isSinglePlayer && !this.myGamePlayer.isLocalPlayer)
                    {
                        if (newValue >= this.MaxStamina)
                            this.AIResetSprintingParameters();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("UpdateGoblinStamina: Could not get myGamePlayer variable. Error: " + e);
            }

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
        if (isRunningOnServer && directionToSlide != Vector2.zero && !doesCharacterHaveBall && !isSliding && !isFatigued && !isPunching && !isGoblinKnockedOut)
        {
            Debug.Log("CmdSlideGoblin: Goblin will slide in direction of: " + directionToSlide.ToString());
            slideDirection = directionToSlide;
            //isSliding = true;
            if (!isSlidingRoutineRunning && !DivingRoutineRunning)
            {
                isSlidingRoutine = SlideGoblinRoutine();
                StartCoroutine(isSlidingRoutine);
                if (this.isCharacterSelected)
                    TeamManager.instance.SlideTackle(this, false);
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
            if(this.isGoblinOnScreen() && newValue)
                SoundManager.instance.PlaySound("slide", 0.75f);
            else
                SoundManager.instance.StopSound("slide");
        }
    }
    [Server]
    public IEnumerator SlideGoblinRoutine()
    {
        this.HandleIsSlidingRoutineRunning(this.isSlidingRoutineRunning, true);
        isSliding = true;
        yield return new WaitForSeconds(0.25f);
        speed = MaxSpeed * 1.2f;
        Debug.Log("SlideGoblinRoutine: Set speed to 1.2x for goblin " + this.name + " owned by player: " + this.ownerConnectionId.ToString());
        yield return new WaitForSeconds(0.25f);
        isSliding = false;
        slideSpeedModifer = 0.7f;
        yield return new WaitForSeconds(2.25f);
        slideSpeedModifer = 1.0f;
        yield return new WaitForSeconds(0.75f);
        slideDirection = Vector2.zero;
        this.HandleIsSlidingRoutineRunning(this.isSlidingRoutineRunning, false);
    }
    public void SlideBoxCollision(GoblinScript slidingGoblin)
    {
        if (isServer)
        {
            /*if (this.ownerConnectionId != slidingGoblin.ownerConnectionId)
                slidingGoblin.TripGoblin();
            if (this.isCharacterSelected)
                TeamManager.instance.SlideTackle(this.serverGamePlayer, true);*/
            if (this.isGoblinGrey != slidingGoblin.isGoblinGrey)
            {
                slidingGoblin.TripGoblin();
                if (this.isCharacterSelected)
                    TeamManager.instance.SlideTackle(this, true);
            }   
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
                if(!this.defenseNormal)
                    this.KnockOutGoblin(false);
            }
        }

    }
    [Server]
    public IEnumerator TripGoblinTimer()
    {
        isTrippedTimerRunning = true;
        yield return new WaitForSeconds(1.25f);
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
    public void DiveAIGoblinTowardEndzone(Vector3 diveDirection)
    {
        if (!isGoblinKnockedOut && !isSliding && !isDiving && hasAuthority && !isPunching)
        {
            CmdStartDiving(diveDirection);
            if (isBlocking)
                CmdSetBlocking(false);
        }
    }
    [Command]
    void CmdStartDiving(Vector2 directionToDive)
    {
        Debug.Log("CmdStartDiving: isRunningOnServer: " + this.isRunningOnServer.ToString() + " Direction to move: " + directionToDive.ToString());
        if (isRunningOnServer && directionToDive != Vector2.zero && !isSliding && !isFatigued && !isDiving && !DivingRoutineRunning && !isSlidingRoutineRunning && !isPunching && !this.isKicking)
        {
            Debug.Log("CmdStartDiving: Goblin will slide in direction of: " + directionToDive.ToString());
            slideDirection = directionToDive;
            HandleIsDiving(isDiving, true);
            speed = MaxSpeed * 1.2f;
            Debug.Log("CmdStartDiving: Set speed to 1.2x for goblin " + this.name + " owned by player: " + this.ownerConnectionId.ToString());
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
                IEnumerator divingResetSanityCheckRoutine = DivingResetSanityCheckRoutine();
                StartCoroutine(divingResetSanityCheckRoutine);
            }
            if (hasAuthority && !newValue)
            {
                if (!golbinBodyCollider.gameObject.activeInHierarchy)
                    golbinBodyCollider.gameObject.SetActive(true);
            }
        }
    }
    IEnumerator DivingResetSanityCheckRoutine()
    {
        yield return new WaitForSeconds(1.25f);
        if (this.isDiving)
            this.StopDiving();

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
        //DivingRoutineRunning = true;
        this.HandleIsDivingRoutineRunning(this.DivingRoutineRunning, true);
        slideSpeedModifer = 0.7f;
        yield return new WaitForSeconds(1.25f);
        slideSpeedModifer = 1.0f;
        yield return new WaitForSeconds(0.75f);
        slideDirection = Vector2.zero;
        //DivingRoutineRunning = false;
        this.HandleIsDivingRoutineRunning(this.DivingRoutineRunning, false);
    }
    public void StartBlocking()
    {
        if (hasAuthority)
        {
            CmdSetBlocking(true);
            // stop player from sprinting when blocking
            if (isSprinting)
                IsPlayerSprinting(false);
            if (isKicking)
                CmdStopKicking();
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
        /*if (this.isCharacterSelected && punching)
            TeamManager.instance.ThrownPunch(this.serverGamePlayer);*/
        TeamManager.instance.ThrownPunch(this);
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
                if (!newValue)
                {
                    ToggleGoblinBodyCollider();
                }
            }
        }
    }
    [TargetRpc]
    public void RpcKickOffTimeoutForceKick(NetworkConnection target)
    {
        if (hasAuthority)
        {
            //this.KickFootballGoblin(0.5f, 0f);
            this.StopKickPower();
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
            {
                powerValueSubmitted = kickPower;
                GameplayManager.instance.StopTimeoutKickAfterRoutine();
            }
            if (GameplayManager.instance.gamePhase == "kickoff")
            {
                GameplayManager.instance.StopTimeoutKickOffRoutine();
            }
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
        {
            CmdKickTheFootball();
        }
        KickedFootballSFX();
            
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
        if (GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time")
            TeamManager.instance.KickDownfield(this.serverGamePlayer);
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
            if (!GameplayManager.instance.is1v1)
            { 
                if(isQGoblinLocally3v3)
                    qMarker.GetComponent<QEMarkerScript>().UpdateSpriteForPassing(newValue);
                if(isEGoblinLocally3v3)
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
        //isWasPunchedRoutineRunning = true;
        this.HandleWasPunchedRoutineRunning(this.isWasPunchedRoutineRunning, true);
        wasPunchedSpeedModifier = 0.8f;
        yield return new WaitForSeconds(3.0f);
        wasPunchedSpeedModifier = 1.0f;
        //isWasPunchedRoutineRunning = false;
        this.HandleWasPunchedRoutineRunning(this.isWasPunchedRoutineRunning, false);
    }
    public void ToggleGoblinBodyCollider()
    {
        golbinBodyCollider.gameObject.SetActive(false);
        golbinBodyCollider.gameObject.SetActive(true);
    }
    void FlipKickoffAimArrow()
    {
        Debug.Log("FlipKickoffAimArrow: for goblin: " + this.name + " is that goblin grey? " + this.isGoblinGrey.ToString());
        Vector3 newPosition = KickoffAimArrow.transform.localPosition;
        if (newPosition.x > 0f)
        {
            newPosition.x *= -1;
            KickoffAimArrow.transform.localPosition = newPosition;
            KickoffAimArrow.GetComponent<SpriteRenderer>().flipX = true;
        }   
    }
    public void EnableKickoffAimArrow(bool activate)
    {
        if(!GameplayManager.instance.isSinglePlayer)
            KickoffAimArrow.SetActive(activate);
        else if (this.myGamePlayer.isLocalPlayer && GameplayManager.instance.isSinglePlayer)
            KickoffAimArrow.SetActive(activate);
        else if(!this.myGamePlayer.isLocalPlayer && GameplayManager.instance.isSinglePlayer)
            KickoffAimArrow.SetActive(false);
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
        Debug.Log("StartKickAfterKickAttempt: for goblin " + this.name);
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
    //[ServerCallback]
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isServer)
        {
            if (GameplayManager.instance.gamePhase == "kick-after-attempt")
            {
                if (collision.collider.tag == "Goblin")
                {
                    GoblinScript collidingGoblin = collision.collider.GetComponent<GoblinScript>();
                    if (collidingGoblin.isKickAfterGoblin && collidingGoblin.isGoblinGrey != this.isGoblinGrey && collidingGoblin.isKickAfterPositionSet && collidingGoblin.doesCharacterHaveBall)
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
        if(isClient)
        {
            if ((GameplayManager.instance.is1v1 || GameplayManager.instance.isSinglePlayer) && (GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time"))
            {
                if (collision.collider.tag == "Goblin")
                {
                    GoblinScript collidingGoblin = collision.collider.GetComponent<GoblinScript>();
                    if (collidingGoblin.isGoblinGrey == this.isGoblinGrey)
                    {
                        Debug.Log("GoblinScript: OnCollisionEnter2D: Teammates collided. This goblin: " + this.name + ":" + this.ownerConnectionId.ToString() + " collided with teammate goblin:" + collidingGoblin.name + ":" + collidingGoblin.ownerConnectionId.ToString());
                        if (!teammateCollision)
                        {
                            // Get the perpindicular direction of the teammate collision
                            Vector2 directionOfTeammate = (collidingGoblin.transform.position - this.transform.position).normalized;
                            Debug.Log("GoblinScript: OnCollisionEnter2D: Teammates collided. Grey team?: " + this.isGoblinGrey.ToString() + " Direction of teammate " + collidingGoblin.name + " is: " + directionOfTeammate.ToString() + " and the Perpendicular is: " + Vector2.Perpendicular(directionOfTeammate).normalized.ToString());
                            teammateCollisionDirectionModifer.x = directionOfTeammate.x * -1f;
                            directionOfTeammate = Vector2.Perpendicular(directionOfTeammate).normalized;
                            teammateCollisionDirectionModifer.y = directionOfTeammate.y;

                            if (UnityEngine.Random.Range(0f, 1.0f) > 0.5f)
                            {
                                teammateCollisionDirectionModifer.y = teammateCollisionDirectionModifer.y * -1f;
                            }

                            // Don't have the ball character change their direction
                            /*if (this.doesCharacterHaveBall)
                                return;*/
                            IEnumerator teammateCollisionCooldownRoutine = TeammateCollisionCooldownRoutine();
                            StartCoroutine(teammateCollisionCooldownRoutine);
                        }
                    }
                }
            }
        }
        
        
    }
    IEnumerator TeammateCollisionCooldownRoutine()
    {
        teammateCollision = true;
        yield return new WaitForSeconds(0.25f);
        teammateCollision = false;
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
        this.damage = MaxDamage * 2.0f;
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
        defenseModifier = 0.35f;
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

        if (particleType.Contains("invincibilityBlueShell"))
        { 
            mySoundManager.PlaySound("powerup-used-" + particleType, 0.8f);
            return;
        }
        if (isGoblinOnScreen())
        {
            SoundManager.instance.PlaySound("powerup-used-" + particleType, 0.8f);
        }
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
    [Server]
    public void StartInvinvibilityBlueShell()
    {
        Debug.Log("StartInvinvibilityBlueShell: for goblin: " + this.name);
        IEnumerator invinvibilityBlueShellRoutine = InvinvibilityBlueShellRoutine();
        StartCoroutine(invinvibilityBlueShellRoutine);
    }
    [ServerCallback]
    IEnumerator InvinvibilityBlueShellRoutine()
    {
        invinvibilityBlueShell = true;
        if (this.doesCharacterHaveBall)
            ballCarrySpeedModifier = 1.0f;
        this.RpcInvincibilityMultiFlash();
        yield return new WaitForSeconds(5.0f);
        invinvibilityBlueShell = false;
        if (this.doesCharacterHaveBall)
            ballCarrySpeedModifier = 0.9f;
        //SoundManager.instance.StopSound("powerup-used-invincibilityBlueShell");
        RpcStopInvincibilitySound();

    }
    [ClientRpc]
    public void RpcStopInvincibilitySound()
    {
        mySoundManager.StopSound("powerup-used-invincibilityBlueShell");
    }
    [Server]
    public void StartStaminaNormal()
    {
        this.UpdateGoblinStamina(this.stamina, this.MaxStamina);
        if (this.isFatigued)
        {
            this.HandleIsFatigued(this.isFatigued, false);
        }
    }
    [ClientRpc]
    public void RpcInvincibilityMultiFlash()
    {
        Debug.Log("RpcInvincibilityMultiFlash: for goblin " + this.name);
        spriteFlash.MultiFlash(5.0f, Color.yellow, Color.blue, Color.magenta);
    }
    void MoveTowardFootball()
    {
        // Reset the ai script for targetting goblin
        myGoblinAIPathFindingScript.goblinTarget = null;
        this.goblinTarget = null;
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
        // If goblin is far away from the ball, try to sprint toward it?
        if (distToFootball > 10f)
        {
            this.WillAIGoblinSprint(true);
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
                this.goblinTarget = NetworkIdentity.spawned[gameFootball.goblinWithBallNetId].GetComponent<GoblinScript>();
                
                //Check to see if the goblin should be sprinting to the ball carrier
                if (distanceToTarget > 3f || goblinTarget.isSprintingOnServer)
                {
                    this.WillAIGoblinSprint(false);
                }
                myGoblinAIPathFindingScript.goblinTarget = goblinTarget;
                myGoblinAIPathFindingScript.isTargetingAGoblin = true;
                AIMoveTowardDirection(diretionToBallCarrier, targetPosition);
            }                
            else
            {
                isRunning = false;
                animator.SetBool("isRunning", isRunning);
                //this.AIResetSprintingParameters();
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
    public void AIMoveTowardDirection(Vector3 directionToMoveTo, Vector3 targetPosition)
    {
        //Vector2 direction = Vector2.ClampMagnitude(directionToMoveTo, 1);
        Vector2 direction = this.GetComponent<GoblinAIPathFinding>().AIMoveTowardDirection(Vector2.ClampMagnitude(directionToMoveTo, 1), targetPosition);

        preDirection = directionToMoveTo;
        postDirection = direction;

        // Set whether the goblin is moving. If the "direction" to the football is 0, then they shouldn't be moving?
        isRunning = false;
        if (direction.x != 0 || direction.y != 0)
            isRunning = true;
        // Set the goblin's "Running On Server" value so that can be tracked by the server?
        CmdSetIsRunningOnServer(isRunning);

        //Set the "isSprinting" on the server and make sure that all the speed modifiers get set correctly?
        CmdIsPlayerSprinting(isSprinting);

        // If the goblin is sprinting, make sure to drain their stamina
        if (isSprinting && !isFatigued)
        {
            CmdDrainStamina();
        }

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

        // Check to see if the goblin has been "stuck" for the last set amount of time. If they are stuck, make the goblin move either up or down
        if (teammateCollision)
        {
            direction += teammateCollisionDirectionModifer;
            //direction = direction.normalized;
            //direction = teammateCollisionDirectionModifer;
            direction = Vector2.ClampMagnitude(direction, 1);
        }
        if (CheckIfAIGoblinIsStuck())
        {
            direction += (stuckDirectionChange * 2);
            direction = Vector2.ClampMagnitude(direction, 1);
            DiveAIGoblinTowardEndzone(direction);
        }
        else if (DiveOverObstacles(direction))
        {
            DiveAIGoblinTowardEndzone(direction);
        }

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
        // Reset any sprinting for the AI?
        if (this.isSprinting)
            this.AIResetSprintingParameters();
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
                    ballCarrierPosition.x += (UnityEngine.Random.Range(1.5f, 7.5f) * directionModifier);
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
                        ballCarrierPosition.x += (UnityEngine.Random.Range(1.5f, 7.5f) * directionModifier);
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
                ballCarrierPosition.x += (UnityEngine.Random.Range(1.5f, 7.5f) * directionModifier);
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
                ballCarrierPosition.x += (UnityEngine.Random.Range(1.5f, 7.5f) * directionModifier);
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
                /*isSliding = false;
                slideSpeedModifer = 1.0f;
                slideDirection = Vector2.zero;
                isSlidingRoutineRunning = false;
                StopCoroutine(isSlidingRoutine);*/
                CmdStopSliding();
            }
            if (isBlocking)
                CmdSetBlocking(false);
            if (isKicking)
                CmdStopKicking();
            if (isWasPunchedRoutineRunning)
                this.CmdStopWasPunchedCoroutine();

            animator.SetBool("isRunning", false);
            animator.SetBool("isSliding", false);
            animator.SetBool("isBlocking", false);
            if (!this.doesCharacterHaveBall)
                animator.SetBool("withFootball", false);
            this.CmdResetFootstepModifers();
            if (this.isSprinting)
                this.IsPlayerSprinting(false);
            try
            {
                if (this.myGamePlayer.isSinglePlayer || this.myGamePlayer.is1v1)
                {
                    this.AIResetSprintingParameters();
                    if (this.goblinTarget != null)
                        this.goblinTarget = null;
                }
            }
            catch (Exception e)
            {
                Debug.Log("RpcResetGoblinStatuses: Could not get goblin player owner object. Error: " + e);
            }

            // Reset speed for the goblin?
            CmdIsPlayerSprinting(this.isSprinting);
        }        
    }
    // If the isWasPunchedRoutineRunning coroutine is running on the server, this command will stop it and reset the goblins wasPunchedSpeedModifier.
    [Command]
    void CmdStopWasPunchedCoroutine()
    {
        if (isWasPunchedRoutineRunning)
        {
            StopCoroutine(isWasPunched);
            wasPunchedSpeedModifier = 1.0f;
            //isWasPunchedRoutineRunning = false;
            this.HandleWasPunchedRoutineRunning(this.isWasPunchedRoutineRunning, false);
        }
    }
    // Resets the footstep modifers on the server. This controls the footstep sound that is played, as well as any slowDownObstacleModifier effects that are affecting the goblin.
    [Command]
    void CmdResetFootstepModifers()
    {
        this.onGlueSlowDown = false;
        this.onWaterSlowDown = false;
        this.onBrushSlowDown = false;
        this.slowDownObstacleModifier = 1.0f;
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
                    //Check to see if the goblin should be sprinting to the ball carrier
                    if (distanceToTarget > 10f)
                    {
                        this.WillAIGoblinSprint(false);
                    }
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
        if (stillColliding && !this.invinvibilityBlueShell)
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
    [ClientRpc]
    public void RpcPunchedFlashSprite()
    {
        Debug.Log("RpcPunchedFlashSprite: for goblin " + this.name);
        spriteFlash.Flash(Color.white);
    }
    public void FatigueIndicators(bool enable)
    {
        Debug.Log("FatigueIndicators: for goblin " + this.name + " " + enable.ToString());
        if (enable && this.isFatigued)
        {  
            spriteFlash.Flash(Color.yellow);
        }
        if (enable)
        {
            fatigueSweatDrop.SetActive(enable);
        }
        else if (this.isFatigued || this.isSlidingRoutineRunning || this.DivingRoutineRunning)
        {
            fatigueSweatDrop.SetActive(true);
        }
        else
        {
            fatigueSweatDrop.SetActive(false);
        }
        
    }
    public void FlipFatigueSweatIndicator()
    {
        bool flipSweat = false;
        Vector3 newPosition = fatigueSweatDrop.transform.localPosition;
        Vector3 bandAidPosition = wasPunchedBandAid.transform.localPosition;
        if (myRenderer.flipX && newPosition.x < 0)
        {
            flipSweat = true;
        }
        if (!myRenderer.flipX && newPosition.x > 0)
        {
            flipSweat = true;
        }
        if (flipSweat)
        {
            newPosition.x *= -1;
            bandAidPosition.x *= -1;
            fatigueSweatDrop.transform.localPosition = newPosition;
            wasPunchedBandAid.transform.localPosition = bandAidPosition;
        }
    }
    [ClientCallback]
    public void BerserkerSwingSound()
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(this.transform.position);
        if (screenPoint.x < 0 || screenPoint.x > 1)
            return;
        SoundManager.instance.PlaySound("berserker-swing", 0.75f);
    }
    public bool isGoblinOnScreen()
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
    [ClientRpc]
    public void RpcHitByGoblin(string type)
    {
        Debug.Log("RpcHitByGoblin: goblin type: " + type);
        SoundManager.instance.PlaySound("hit-by-" + type, 1.0f);
        if(this.goblinType.Contains("berserker"))
            SoundManager.instance.StopSound("berserker-swing");
    }
    public void FootstepSFX1()
    {
        /*if (!isGoblinOnScreen())
            return;
        float volume = 0.7f;
        if (this.hasAuthority && this.isCharacterSelected)
            volume = 0.7f;
        else
            volume = 0.45f;

        string footstepType = GetFootstepSFXType(true);

        SoundManager.instance.PlaySound(footstepType, volume);*/
        FootstepSFXAllInOne();
    }
    public void FootstepSFX2()
    {
        /*if (!isGoblinOnScreen())
            return;
        float volume = 0.7f;
        if (this.hasAuthority && this.isCharacterSelected)
            volume = 0.7f;
        else
            volume = 0.35f;

        string footstepType = GetFootstepSFXType(false);

        SoundManager.instance.PlaySound(footstepType, volume);*/
        FootstepSFXAllInOne();
    }
    void FootstepSFXAllInOne()
    {
        if (!isGoblinOnScreen())
            return;
        //string stepNumber = "-1";
        if (firstFootStep)
        {
            //stepNumber = "-2";
            firstFootStep = false;
        }
        else
        {
            //stepNumber = "-1";
            firstFootStep = true;
        }

        float volume = 0.0f;
        if (this.hasAuthority && this.isCharacterSelected)
            volume = 0.5f;
        else
            volume = 0.25f;

        string footstepType = GetFootstepSFXType(firstFootStep);

        SoundManager.instance.PlaySound(footstepType, volume);

        try
        {
            if (this.isSprintingOnServer && !this.isFatigued)
                sprintParticleSystem.Play();
        }
        catch (Exception e)
        {
            Debug.Log("PlaySprintDust: Could not play particle effect. Error: " + e);
        }


    }
    public string GetFootstepSFXType(bool step1)
    {
        string stepType = "";

        if (onGlueSlowDown)
            stepType = "glue-footstep";
        else if(onWaterSlowDown)
            stepType = "water-footstep";
        else if (onBrushSlowDown)
            stepType = "brush-footstep";
        else
            stepType = "footstep";

        if (step1)
            stepType = stepType + "-1";
        else
            stepType = stepType + "-2";

        return stepType;
    }
    void KickedFootballSFX()
    {
        if (this.isGoblinOnScreen())
            SoundManager.instance.PlaySound("kicked-ball", 1.0f);
    }
    public void CollisionWithObstacleObject(bool knockOut)
    {
        if (hasAuthority)
        {
            this.CmdCollisionWithObstacleObject(knockOut);
        }
    }
    [Command]
    void CmdCollisionWithObstacleObject(bool knockOut)
    {
        if (!this.isGoblinKnockedOut && this.canCollide)
        {
            this.KnockOutGoblin(knockOut);
        }
    }
    public void AISubmitKickAfterKickingAccuracy(float accuracyValue, float powerValue)
    {
        Debug.Log("AISubmitKickAfterKickingAccuracy: Accuracy value: " + accuracyValue.ToString() + " power value: " + powerValue.ToString());
        //isAccuracySubmittedYet = true;
        IEnumerator delayForKickAfterAttempt = DelayForKickAfterAttempt(accuracyValue, powerValue);
        StartCoroutine(delayForKickAfterAttempt);
    }
    IEnumerator DelayForKickAfterAttempt(float accuracyValue, float powerValue)
    {
        CmdSubmitKickAfterAccuracyValue(accuracyValue);
        yield return new WaitForSeconds(0.1f);
        CmdKickFootball(powerValue, 0f);
    }
    // If a goblin is going to start to sprint, set paramaters for that sprinting "session" to abide by?
    // Will the goblin sprint until they are fatigued?
    // What percentage of their max stamina will the goblin sprint to?
    public void AIGetSprintingParameters()
    {
        Debug.Log("AIGetSprintingParameters: for goblin: " + this.name + " owned by player: " + this.ownerConnectionId.ToString());
        // Determine if the goblin will sprint to the point of fatigue
        float fatigueChance = UnityEngine.Random.Range(0f, 1.0f);
        if (fatigueChance > 0.9f)
            this.willGoblinSprintToFatigue = true;
        else
            this.willGoblinSprintToFatigue = false;
        Debug.Log("AIGetSprintingParameters: for goblin: " + this.name + " owned by player: " + this.ownerConnectionId.ToString() + " will sprint to fatigue? " + willGoblinSprintToFatigue.ToString());
        // If the goblin won't sprint to fatigue, set a minimum % of MaxStamina to sprint to?
        // If they will sprint to fatigue, set to 0f
        if (willGoblinSprintToFatigue)
            minStaminaToSprintTo = 0f;
        else
        {
            minStaminaToSprintTo = UnityEngine.Random.Range(0.05f, 0.15f);
        }
    }
    public void AIResetSprintingParameters()
    {
        Debug.Log("AIResetSprintingParameters: for goblin: " + this.name + " owned by player: " + this.ownerConnectionId.ToString());
        this.didGoblinCompleteSprint = false;
        this.willGoblinSprintToFatigue = false;
        this.minStaminaToSprintTo = 1.0f;

        IEnumerator aiCanSprintAgainDelay = AICanSprintAgainDelay();
        StartCoroutine(aiCanSprintAgainDelay);
    }
    IEnumerator AICanSprintAgainDelay()
    {
        yield return new WaitForSeconds(0.15f);
        this.canGoblinSprint = true;
    }
    // Determine if the goblin should keep sprinting based on the Sprint Parameters that were set earlier?
    public bool AIWillGoblinKeepSprinting()
    {
        Debug.Log("AIWillGoblinKeepSprinting: for goblin: " + this.name + " owned by player: " + this.ownerConnectionId.ToString());
        bool keepSprinting = false;
        if (!this.canGoblinSprint)
        {
            keepSprinting = false;
            return keepSprinting;
        }

        if (this.willGoblinSprintToFatigue && !this.isFatigued)
        {
            keepSprinting = true;
            return keepSprinting;
        }
        else if (!this.willGoblinSprintToFatigue && !this.isFatigued)
        {
            if ((this.stamina / this.MaxStamina) > minStaminaToSprintTo)
            {
                keepSprinting = true;
                return keepSprinting;
            }
            else
            {
                keepSprinting = false;
                this.canGoblinSprint = false;
                return keepSprinting;
            }
        }
        //Debug.Log("AIWillGoblinKeepSprinting: returning keepSprinting as: " + keepSprinting.ToString());
        return keepSprinting;
    }
    // Check to see how many other AI goblins are targeting the ball carrier to see if this goblin should also target the ball carrier
    // Idea is that at least 2 goblins will target the ball carrier and one other will target a random goblin that is "getting open for a pass"
    // For 1v1, it is assumed the player goblin is targeting the ball carrier, so only 1 more AI goblin needs to target the ball carrier
    bool AIShouldGoblinTargetBallCarrier()
    {
        // testing always targeting the ball carrier
        return true;
        bool targetBallCarrier = false;

        int minToTargetBallCarrier = 1;
        // For single player games. The AI will need to have at least 2 goblins targeting the ball carrier. The playter controlled teams only need 1 AI to target the ball carrier, and the player will (hopefully) target the ball carrier themselves
        if (myGamePlayer.isSinglePlayer && !myGamePlayer.isLocalPlayer)
        {
            minToTargetBallCarrier = 2;
        }

        // The skirmisher should almost always be targetting the ball carrier. 90% chance they will and return as true?
        if (this.soundType == "skirmisher")
        {
            if (UnityEngine.Random.Range(0f, 1.0f) > 0.9f)
            {
                targetBallCarrier = true;
                return targetBallCarrier;
            }
        }

        // Get the ball carrier goblin
        GoblinScript ballCarrier = NetworkIdentity.spawned[gameFootball.goblinWithBallNetId].GetComponent<GoblinScript>();

        // Go through each goblin in the player's goblinTeam and check who they are targetting (if any) and track how many are targetting the ball carrier
        int numberTargetingBallCarrier = 0;
        foreach (GoblinScript goblin in myGamePlayer.goblinTeam)
        {
            // don't count this goblin in this count
            if (goblin == this)
                continue;
            // don't count the goblin that is controlled by a player character. Their "goblinTarget" value won't be set correctly?
            // The isLocalPlayer check is to make sure they are a player controlled goblin and not an AI controlled goblin in single player
            if (goblin.isCharacterSelected && goblin.myGamePlayer.isLocalPlayer)
                continue;
            if (goblin.goblinTarget == ballCarrier)
                numberTargetingBallCarrier++;
        }

        // if minimum number of goblins are targeting the ball carrier, return false. Else, return true
        if (numberTargetingBallCarrier >= minToTargetBallCarrier)
            targetBallCarrier = false;
        else
            targetBallCarrier = true;

        return targetBallCarrier;
    }
    // Checks to see if the AI should target the ball carrier or a random other goblin. Sets goblin state to either ChaseBallCarrier or AttackNearbyGoblin
    void AIGetGoblinTarget()
    {
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
            bool targetBallCarrier = AIShouldGoblinTargetBallCarrier();
            if (targetBallCarrier)
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
                }
            }
        }
    }
    public void WillAIGoblinSprint(bool chasingBall)
    {
        if (this.goblinTarget != null || chasingBall)
        {
            if (!this.isSprinting && this.canGoblinSprint)
            {
                this.AIGetSprintingParameters();
                this.isSprinting = true;
            }
            else if (this.isSprinting && this.canGoblinSprint)
            {
                this.isSprinting = this.AIWillGoblinKeepSprinting();
            }
            else if (!this.canGoblinSprint)
            {
                this.isSprinting = false;
            }
        }
        else
        {
            this.isSprinting = false;
            this.canGoblinSprint = false;
            this.AIResetSprintingParameters();
        }
    }
    // Check to see if an AI goblin is stuck or not based on how far they have moved for the last set amount of time?
    bool CheckIfAIGoblinIsStuck()
    {
        bool isGoblinStuck = false;
        if (Time.time <= (stuckCheckTime + stuckCheckRate))
            return isGoblinStuck;
        else
            stuckCheckTime = Time.time;
        if (this.DivingRoutineRunning)
            return isGoblinStuck;
        if (this.isGoblinKnockedOut || this.isDiving || this.isSliding || this.isPunching || this.isKicking)
        {
            // Reset the "last checked position?
            if(this.isGoblinKnockedOut)
                this.lastCheckedPosition = new Vector3(100f, 100f, 0f);
            return isGoblinStuck;
        }   
        try
        {
            if ((this.state == State.ChaseBallCarrier || this.state == State.AttackNearbyGoblin))
            {
                if (this.goblinTarget != null)
                {
                    float distToTarget = Vector2.Distance(this.transform.position, this.goblinTarget.transform.position);
                    if (distToTarget <= this.slideRange || distToTarget <= this.punchRange)
                        return isGoblinStuck;
                }
            }
            else if (this.state == State.ChaseFootball)
            {
                Vector3 footballPosition = Vector3.zero;
                if (gameFootball.isKicked)
                {
                    GameObject footBallLandingSpot = GameObject.FindGameObjectWithTag("footballLandingTarget");
                    footballPosition = footBallLandingSpot.transform.position;
                }
                else
                {
                    footballPosition = gameFootball.transform.position;
                }
                float distToFootball = Vector2.Distance(this.transform.position, footballPosition);
                if (distToFootball <= 3.0f)
                    return isGoblinStuck;
            }
        }
        catch (Exception e)
        {
            Debug.Log("CheckIfAIGoblinIsStuck: Unable to check state/get distance to targeT? Error: " + e);
        }
        if (Vector2.Distance(this.transform.position, this.lastCheckedPosition) > 0.5f)
        {
            this.lastCheckedPosition = this.transform.position;
            return isGoblinStuck;
        }   
        else
        {
            Debug.Log("Goblin: " + this.name + ":" + this.ownerConnectionId.ToString() + " is stuck. Making them dive in a random direction to get free???");
            isGoblinStuck = true;
            // If the goblin appears to be stuck
            if (UnityEngine.Random.Range(0f, 1.0f) > 0.5f)
            {
                stuckDirectionChange = new Vector2(0f, 1f);
            }
            else
            {
                stuckDirectionChange = new Vector2(0f, -1f);
            }
            /*float xDirection = 1f;
            if (this.myRenderer.flipX)
                xDirection = -1f;
            Vector2 newDirection = new Vector2(xDirection, UnityEngine.Random.Range(-1f, 1.0f));
            stuckDirectionChange = newDirection.normalized;*/
            // reset the last checked position to something off the field to make sure that goblins aren't going to pass the check by randomly walking by the last checked position they dove from again?
            this.lastCheckedPosition = new Vector3(100f, 100f, 0f);
        }
        return isGoblinStuck;
    }
    bool DiveOverObstacles(Vector3 direction)
    {
        bool willDive = false;

        if (Time.time <= (diveCheckTime + diveCheckRate))
            return willDive;
        else
            diveCheckTime = Time.time;
        Debug.Log("DiveOverObstacles: running trip obstacle checks for this Goblin: " + this.name + ":" + this.ownerConnectionId.ToString());

        // make sure that the diving routine/cool down isn't running? It is a syncvar so clients should know as well?
        if (this.DivingRoutineRunning)
            return willDive;

        if (this.isGoblinKnockedOut || this.isDiving || this.isSliding || this.isPunching || this.isKicking)
            return willDive;

        // Only want to dive over obstacles if the goblin has the ball or if they are chasing the ball carrier?
        if (!this.doesCharacterHaveBall || this.state != State.ChaseBallCarrier)
            return willDive;

        // cast out in the left/right direction the goblin is moving. If a trip obstacle is hit, dive?
        // cast from the "worldCenterOfMass" since that is the goblin's feet, and the footbox collider/whatever is what collides with a trip object and causes a trip
        RaycastHit2D[] hits = Physics2D.CircleCastAll(this.rb.worldCenterOfMass, 0.25f, direction, 1.5f);
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.gameObject.tag == "tripObject")
                {

                    Debug.Log("DiveOverObstacles: trip obstacle hit! Goblin: " + this.name + ":" + this.ownerConnectionId.ToString() + " will dive over obstacle: " + hits[i].collider.name);
                    willDive = true;
                    break;
                }
            }
        }
        return willDive;
    }
    public void HandleIsDivingRoutineRunning(bool oldValue, bool newValue)
    {
        if (isServer)
            this.DivingRoutineRunning = newValue;
        if (isClient)
        {
            this.FatigueIndicators(newValue);
        }
    }
    public void HandleIsSlidingRoutineRunning(bool oldValue, bool newValue)
    {
        if (isServer)
            this.isSlidingRoutineRunning = newValue;
        if (isClient)
        {
            this.FatigueIndicators(newValue);
        }
    }
    public void UpdateGamePadUIMarkersForGoblins(bool enableGamePadUI)
    {
        Debug.Log("UpdateGamePadUIMarkersForGoblins: to: " + enableGamePadUI.ToString() + " for goblin: " + this.name + " owned by: " + this.ownerNetId.ToString());
        qMarker.GetComponent<QEMarkerScript>().UpdateForGamepadUI(enableGamePadUI);
        eMarker.GetComponent<QEMarkerScript>().UpdateForGamepadUI(enableGamePadUI);
        //qMarker.GetComponent<QEMarkerScript>().UpdateSpriteForPassing(this.canGoblinReceivePass);
        //eMarker.GetComponent<QEMarkerScript>().UpdateSpriteForPassing(this.canGoblinReceivePass);
    }
    public void HandleWasPunchedRoutineRunning(bool oldVale, bool newValue)
    {
        if (isServer)
            isWasPunchedRoutineRunning = newValue;
        if (isClient)
        {
            wasPunchedBandAid.SetActive(newValue);
        }
    }
}

