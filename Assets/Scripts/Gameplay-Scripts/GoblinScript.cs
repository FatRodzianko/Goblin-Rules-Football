using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Component.Animating;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Object;
using FishNet.Object.Synchronizing;
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
        SpyBallCarrier,
    }

    public float pressedTime = 0f;
    public float releasedTime = 0f;

    [SerializeField] GameObject PowerUpParticleSystemPrefab;


    [Header("Player Owner Info")]
    private readonly SyncVar<string> _syncOwnerName = new SyncVar<string>();
    public string ownerName { get => _syncOwnerName.Value; set => _syncOwnerName.Value = value; }
    private readonly SyncVar<int> _syncOwnerConnectionId = new SyncVar<int>();
    public int ownerConnectionId { get => _syncOwnerConnectionId.Value; set => _syncOwnerConnectionId.Value = value; }
    private readonly SyncVar<int> _syncOwnerPlayerNumber = new SyncVar<int>();
    public int ownerPlayerNumber { get => _syncOwnerPlayerNumber.Value; set => _syncOwnerPlayerNumber.Value = value; }
    private readonly SyncVar<int> _syncOwnerNetId = new SyncVar<int>(ObjectLookup.NoObject);
    public int ownerNetId { get => _syncOwnerNetId.Value; set => _syncOwnerNetId.Value = value; }
    public GamePlayer myGamePlayer;
    public GamePlayer serverGamePlayer;
    float touchdownScoredtime = 0f;
    float blockedKickTime = 0f;
    

    [Header("Goblin Base Stats")]
    public int MaxHealth;
    public float MaxStamina;
    public float StaminaDrain;
    public float StaminaRecovery;
    public float MaxSpeed;
    public int MaxDamage;
    private readonly SyncVar<bool> _syncCanCollide = new SyncVar<bool>(true);
    public bool canCollide { get => _syncCanCollide.Value; set => _syncCanCollide.Value = value; }
    public string soundType;


    [Header("Goblin Current Stats")]
    private readonly SyncVar<float> _syncHealth = new SyncVar<float>();
    public float health { get => _syncHealth.Value; set => _syncHealth.Value = value; }
    private readonly SyncVar<float> _syncStamina = new SyncVar<float>();
    public float stamina { get => _syncStamina.Value; set => _syncStamina.Value = value; }
    private readonly SyncVar<float> _syncSpeed = new SyncVar<float>();
    public float speed { get => _syncSpeed.Value; set => _syncSpeed.Value = value; }
    private readonly SyncVar<float> _syncDamage = new SyncVar<float>();
    public float damage { get => _syncDamage.Value; set => _syncDamage.Value = value; }
    private readonly SyncVar<float> _syncBallCarrySpeedModifier = new SyncVar<float>(1.0f);
    public float ballCarrySpeedModifier { get => _syncBallCarrySpeedModifier.Value; set => _syncBallCarrySpeedModifier.Value = value; }
    private readonly SyncVar<float> _syncBlockingSpeedModifier = new SyncVar<float>(1.0f);
    public float blockingSpeedModifier { get => _syncBlockingSpeedModifier.Value; set => _syncBlockingSpeedModifier.Value = value; }
    private readonly SyncVar<float> _syncDefenseModifier = new SyncVar<float>();
    public float defenseModifier { get => _syncDefenseModifier.Value; set => _syncDefenseModifier.Value = value; }
    private readonly SyncVar<float> _syncSpeedModifierFromPowerUps = new SyncVar<float>(1.0f);
    public float speedModifierFromPowerUps { get => _syncSpeedModifierFromPowerUps.Value; set => _syncSpeedModifierFromPowerUps.Value = value; }
    private readonly SyncVar<float> _syncSlowDownObstacleModifier = new SyncVar<float>(1.0f);
    public float slowDownObstacleModifier { get => _syncSlowDownObstacleModifier.Value; set => _syncSlowDownObstacleModifier.Value = value; }
    private readonly SyncVar<float> _syncPossessionSpeedBonus = new SyncVar<float>(1.0f);
    public float possessionSpeedBonus { get => _syncPossessionSpeedBonus.Value; set => _syncPossessionSpeedBonus.Value = value; }
    private float _sprintSpeedModifier = 1.15f;


    [Header("Character selection stuff")]
    private readonly SyncVar<bool> _syncIsCharacterSelected = new SyncVar<bool>();
    public bool isCharacterSelected { get => _syncIsCharacterSelected.Value; set => _syncIsCharacterSelected.Value = value; }
    private readonly SyncVar<bool> _syncIsQGoblin = new SyncVar<bool>();
    public bool isQGoblin { get => _syncIsQGoblin.Value; set => _syncIsQGoblin.Value = value; }
    private readonly SyncVar<bool> _syncIsEGoblin = new SyncVar<bool>();
    public bool isEGoblin { get => _syncIsEGoblin.Value; set => _syncIsEGoblin.Value = value; }
    public bool isQGoblinLocally3v3 = false;
    public bool isEGoblinLocally3v3 = false;
    public bool canGoblinMove = true;
    public bool isGoblinPunching = false;
    private readonly SyncVar<bool> _syncIsPunching = new SyncVar<bool>();
    public bool isPunching { get => _syncIsPunching.Value; set => _syncIsPunching.Value = value; }
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
    NetworkAnimator networkAnimator;
    public Rigidbody2D rb;
    public Collider2D playerCollider;
    public Vector2 previousInput;
    public SpriteRenderer myRenderer;
    [SerializeField] BoxCollider2D golbinBodyCollider;
    private readonly SyncVar<string> _syncGoblinType = new SyncVar<string>();
    public string goblinType { get => _syncGoblinType.Value; set => _syncGoblinType.Value = value; }
    [SerializeField] private StatusBarScript myStatusBars;
    [SerializeField] private GameObject touchdownHitbox;
    [SerializeField] private GameObject divingHitbox;
    [SerializeField] private SpriteRenderer myShadow;
    [SerializeField] private SpriteRenderer mySelectedCircle;
    [SerializeField] private ParticleSystem sprintParticleSystem;
    [SerializeField] private Material spriteDefault;
    [SerializeField] private Material spriteOutlineWhite;
    [SerializeField] private Material spriteOutlineYellow;
    Material spriteOutline;
    [SerializeField] string greenGoblinLayer;
    [SerializeField] string greyGoblinLayer;


    [Header("Character Game State Stuff")]
    private readonly SyncVar<bool> _syncIsGoblinGrey = new SyncVar<bool>();
    public bool isGoblinGrey { get => _syncIsGoblinGrey.Value; set => _syncIsGoblinGrey.Value = value; }
    private readonly SyncVar<bool> _syncDoesCharacterHaveBall = new SyncVar<bool>();
    public bool doesCharacterHaveBall { get => _syncDoesCharacterHaveBall.Value; set => _syncDoesCharacterHaveBall.Value = value; }
    private readonly SyncVar<bool> _syncIsThrowing = new SyncVar<bool>();
    public bool isThrowing { get => _syncIsThrowing.Value; set => _syncIsThrowing.Value = value; }
    private readonly SyncVar<bool> _syncIsRunningOnServer = new SyncVar<bool>();
    public bool isRunningOnServer { get => _syncIsRunningOnServer.Value; set => _syncIsRunningOnServer.Value = value; }
    private Vector2 runningDirectionOnServer = Vector2.zero;
    public bool isRunning = false;
    public bool isSprinting = false;
    public bool shiftHeldDown = false;
    private readonly SyncVar<bool> _syncIsSprintingOnServer = new SyncVar<bool>();
    public bool isSprintingOnServer { get => _syncIsSprintingOnServer.Value; set => _syncIsSprintingOnServer.Value = value; }
    private readonly SyncVar<bool> _syncCanRecoveryStamina = new SyncVar<bool>(true);
    public bool canRecoveryStamina { get => _syncCanRecoveryStamina.Value; set => _syncCanRecoveryStamina.Value = value; }
    private readonly SyncVar<bool> _syncIsFatigued = new SyncVar<bool>();
    public bool isFatigued { get => _syncIsFatigued.Value; set => _syncIsFatigued.Value = value; }
    private readonly SyncVar<bool> _syncIsGoblinKnockedOut = new SyncVar<bool>();
    public bool isGoblinKnockedOut { get => _syncIsGoblinKnockedOut.Value; set => _syncIsGoblinKnockedOut.Value = value; }
    
    [Header("Slide Info")]
    private readonly SyncVar<bool> _syncIsSliding = new SyncVar<bool>();
    public bool isSliding { get => _syncIsSliding.Value; set => _syncIsSliding.Value = value; }
    private readonly SyncVar<Vector2> _syncSlideDirection = new SyncVar<Vector2>(Vector2.zero);
    public Vector2 slideDirection { get => _syncSlideDirection.Value; set => _syncSlideDirection.Value = value; }
    private readonly SyncVar<bool> _syncIsSlidingRoutineRunning = new SyncVar<bool>();
    public bool isSlidingRoutineRunning { get => _syncIsSlidingRoutineRunning.Value; set => _syncIsSlidingRoutineRunning.Value = value; }
    IEnumerator isSlidingRoutine;
    public float slideSpeedModifer = 1.0f;

    [Header("Dive Info")]
    private readonly SyncVar<bool> _syncIsDiving = new SyncVar<bool>();
    public bool isDiving { get => _syncIsDiving.Value; set => _syncIsDiving.Value = value; }
    private readonly SyncVar<bool> _syncDivingRoutineRunning = new SyncVar<bool>();
    public bool DivingRoutineRunning { get => _syncDivingRoutineRunning.Value; set => _syncDivingRoutineRunning.Value = value; }
    IEnumerator DivingRoutine;

    [Header("Is Goblin Throwing Stuff")]
    public bool throwingRoutine = false;
    IEnumerator throwing;

    [Header("Blocking Info")]
    private readonly SyncVar<bool> _syncIsBlocking = new SyncVar<bool>();
    public bool isBlocking { get => _syncIsBlocking.Value; set => _syncIsBlocking.Value = value; }

    [Header("Kicking Info")]
    private readonly SyncVar<bool> _syncIsKicking = new SyncVar<bool>();
    public bool isKicking { get => _syncIsKicking.Value; set => _syncIsKicking.Value = value; }
    [SerializeField] GameObject KickPowerBarHolder;
    [SerializeField] GameObject KickPowerBarFillerImage;
    [SerializeField] GameObject KickoffAimArrow;
    public bool powerBarActive = false;
    public float GoblinMaxKickDistance = 40f;
    public float GoblinMinKickDistance = 10f;
    public float GoblinPowerBarSpeed = 1f;
    private readonly SyncVar<float> _syncGoblinKickPower = new SyncVar<float>();
    public float GoblinKickPower { get => _syncGoblinKickPower.Value; set => _syncGoblinKickPower.Value = value; }
    
    public float currentPowerBarScale = 0f;
    public int powerBarDirection = 1;
    public float kickoffAngle = 0f;
    private readonly SyncVar<float> _syncGoblinKickoffAngle = new SyncVar<float>();
    public float GoblinKickoffAngle { get => _syncGoblinKickoffAngle.Value; set => _syncGoblinKickoffAngle.Value = value; }
    float kickoffAngleSpeed = 30f;
    public bool aimArrowButtonHeldDown = false;
    public bool aimArrowUp = false;

    [Header("Kick After Repositioning Stuff")]
    private readonly SyncVar<bool> _syncIsKickAfterGoblin = new SyncVar<bool>();
    public bool isKickAfterGoblin { get => _syncIsKickAfterGoblin.Value; set => _syncIsKickAfterGoblin.Value = value; }
    private readonly SyncVar<bool> _syncIsKickAfterPositionSet = new SyncVar<bool>();
    public bool isKickAfterPositionSet { get => _syncIsKickAfterPositionSet.Value; set => _syncIsKickAfterPositionSet.Value = value; }
    private readonly SyncVar<float> _syncAngleOfKickAttempt = new SyncVar<float>();
    public float angleOfKickAttempt { get => _syncAngleOfKickAttempt.Value; set => _syncAngleOfKickAttempt.Value = value; }
    private readonly SyncVar<float> _syncKickAfterAccuracyDifficulty = new SyncVar<float>();
    public float kickAfterAccuracyDifficulty { get => _syncKickAfterAccuracyDifficulty.Value; set => _syncKickAfterAccuracyDifficulty.Value = value; }
    //[SyncVar] public float kickAfterAccuracyBar1 = 0f;
    //[SyncVar] public float kickAfterAccuracyBar2 = 0f;
    public bool repositioningButtonHeldDown = false;
    public bool repositioningToLeft = false;
    Vector2 greenGoalPost = new Vector2(50.3f, -1.5f);
    Vector2 greyGoalPost = new Vector2(-50.3f, -1.5f);
    private readonly SyncVar<Vector2> _syncKickAfterFinalPosition = new SyncVar<Vector2>(Vector2.zero);
    public Vector2 kickAfterFinalPosition { get => _syncKickAfterFinalPosition.Value; set => _syncKickAfterFinalPosition.Value = value; }
    public bool hasGoblinBeenRepositionedForKickAfter = false;

    [Header("Kick After Accuracy Bar Stuff")]
    [SerializeField] GameObject kickAfterAccuracyBar;
    [SerializeField] GameObject kickAfterGuageLine;
    [SerializeField] GameObject kickAfterMarkerLeft;
    [SerializeField] GameObject kickAfterMarkerRight;
    private readonly SyncVar<float> _syncAccuracyValueSubmitted = new SyncVar<float>();
    public float accuracyValueSubmitted { get => _syncAccuracyValueSubmitted.Value; set => _syncAccuracyValueSubmitted.Value = value; }
    private readonly SyncVar<float> _syncPowerValueSubmitted = new SyncVar<float>();
    public float powerValueSubmitted { get => _syncPowerValueSubmitted.Value; set => _syncPowerValueSubmitted.Value = value; }
    public bool isGoblinDoingKickAfterAttempt = false;
    bool isAccuracySubmittedYet = false;
    bool isPowerSubmittedYet = false;
    IEnumerator kickAfterMoveAccuracyGuageLineRoutine;
    float currentAccuracyGaugeXPosition = -1f;
    int currentAccuracyGaugeDirection = 1;
    public float AccuracyBarSpeed = 0f;

    [Header("wasPunchedSpeedModifier Info")]
    private readonly SyncVar<bool> _syncIsWasPunchedRoutineRunning = new SyncVar<bool>();
    public bool isWasPunchedRoutineRunning { get => _syncIsWasPunchedRoutineRunning.Value; set => _syncIsWasPunchedRoutineRunning.Value = value; }
    IEnumerator isWasPunched;
    public float wasPunchedSpeedModifier = 1.0f;

    [Header("Can the Goblin Pass stuff")]
    public Football gameFootball;
    private readonly SyncVar<bool> _syncCanGoblinReceivePass = new SyncVar<bool>();
    public bool canGoblinReceivePass { get => _syncCanGoblinReceivePass.Value; set => _syncCanGoblinReceivePass.Value = value; }


    [Header("Recovery Enumerator Stuff?")]
    private readonly SyncVar<bool> _syncIsStaminaRecoveryRoutineRunning = new SyncVar<bool>();
    public bool isStaminaRecoveryRoutineRunning { get => _syncIsStaminaRecoveryRoutineRunning.Value; set => _syncIsStaminaRecoveryRoutineRunning.Value = value; }
    public IEnumerator staminaRecoveryRoutine;
    private readonly SyncVar<bool> _syncIsHealthRecoveryRoutineRunning = new SyncVar<bool>();
    public bool isHealthRecoveryRoutineRunning { get => _syncIsHealthRecoveryRoutineRunning.Value; set => _syncIsHealthRecoveryRoutineRunning.Value = value; }
    public IEnumerator healthRecoveryRoutine;
    private readonly SyncVar<bool> _syncCanGoblinRegainHealth = new SyncVar<bool>();
    public bool canGoblinRegainHealth { get => _syncCanGoblinRegainHealth.Value; set => _syncCanGoblinRegainHealth.Value = value; }
    private readonly SyncVar<bool> _syncIsRegainHealthRoutineRunning = new SyncVar<bool>();
    public bool isRegainHealthRoutineRunning { get => _syncIsRegainHealthRoutineRunning.Value; set => _syncIsRegainHealthRoutineRunning.Value = value; }
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
    private readonly SyncVar<bool> _syncOnWaterSlowDown = new SyncVar<bool>();
    public bool onWaterSlowDown { get => _syncOnWaterSlowDown.Value; set => _syncOnWaterSlowDown.Value = value; }
    private readonly SyncVar<bool> _syncOnBrushSlowDown = new SyncVar<bool>();
    public bool onBrushSlowDown { get => _syncOnBrushSlowDown.Value; set => _syncOnBrushSlowDown.Value = value; }
    private readonly SyncVar<bool> _syncOnGlueSlowDown = new SyncVar<bool>();
    public bool onGlueSlowDown { get => _syncOnGlueSlowDown.Value; set => _syncOnGlueSlowDown.Value = value; }
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
    // How far goal-side of the ball carrier a spying grenadier holds station, and how close a teammate has to be to
    // the carrier before the spy trusts them to make the tackle. Tuning numbers, not serialized, so changing them
    // here changes them for every goblin.
    const float spyStandoffDistance = 8f;
    const float teammateContestingBallDistance = 10f;
    // A carrier closer than this has the spy cutting them off instead of holding station, and closer still has it
    // making the tackle like the chaser would. The red zone is how near the carrier may get to the endzone they
    // score in before everyone chases, spy included.
    const float spyInterceptDistance = 10f;
    const float spyTackleDistance = 4f;
    const float redZoneDistance = 15f;
    // Where the carrier was on the spy's previous physics tick, for estimating their direction and speed.
    Vector3 lastSpiedCarrierPosition = Vector3.zero;
    // The spy's last logged decision, so the log shows changes rather than every physics tick.
    string lastSpyDecision = "";
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


    // This was Mirror's OnStartAuthority. OnStartClient calls it first for the owner, since Mirror ran it before OnStartClient.
    void StartAsOwner()
    {
        // A goblin belongs to the player whose CmdSpawnPlayerCharacters made it, which the server records in ownerNetId.
        // This used to take "the local player" from a tag, which is only right while a machine owns one player's
        // goblins: with bots filling a 3v3 the host owns several players' goblins at once and all six landed on the
        // human's team. The single player branch this replaces already resolved by ownerNetId, just with a tag lookup
        // for its one AI, so the id is known to be here by now.
        if (!ObjectLookup.TryGet(ownerNetId, out myGamePlayer) || myGamePlayer == null)
        {
            // Last resort if the id hasn't arrived: the local player is right in every case except a host with bots.
            GameObject localPlayerObject = GameObject.FindGameObjectWithTag("LocalGamePlayer");
            if (localPlayerObject != null)
                myGamePlayer = localPlayerObject.GetComponent<GamePlayer>();
        }
        if (myGamePlayer == null)
        {
            Debug.Log("StartAsOwner: " + this.name + " could not find its GamePlayer from ownerNetId " + ownerNetId.ToString());
            return;
        }
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
        if (transform.position.x > 0f)
            CmdFlipRenderer(true);

        myGoblinAIPathFindingScript = this.GetComponent<GoblinAIPathFinding>();

        if (this.myGamePlayer.isAIPlayer)
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
    public override void OnStopServer()
    {
        base.OnStopServer();
        DropFootballForDespawn();
    }
    // The carried football is parented to the goblin holding it (see ParentFootballOnServer). FishNet despawns a
    // disconnecting player's goblins, and despawning an object takes its child NetworkObjects with it, so a player who
    // quit while carrying the ball used to take the football with them. OnStopServer runs before the object is destroyed
    // and before its children despawn, so the ball can still be handed back. Dropped as a fumble, the way a knockout does
    // it, so either team can pick it up.
    // Not [Server]: this object is already deinitializing here, so that check would refuse to run the method.
    void DropFootballForDespawn()
    {
        // False while the server is shutting down, when every object is despawning and there is no ball to drop.
        if (!IsServerStarted)
            return;
        GameObject football = GameObject.FindGameObjectWithTag("football");
        if (football == null || football.transform.parent != this.transform)
            return;
        NetworkObject footballNetworkObject = football.GetComponent<NetworkObject>();
        if (footballNetworkObject == null || !footballNetworkObject.IsSpawned)
            return;

        Debug.Log("DropFootballForDespawn: " + this.name + " was carrying the football as it despawned. Dropping the ball.");
        footballNetworkObject.UnsetParent();
        football.GetComponent<Football>().FumbleFootball();
        if (TeamManager.instance != null && this.serverGamePlayer != null && GameplayManager.instance != null && GameplayManager.instance.gamePhase != "kick-after-attempt")
            TeamManager.instance.FumbleBall(this.serverGamePlayer.isTeamGrey);
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
        if (IsOwner)
            StartAsOwner();

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

        if (IsOwner)
        {
            if (!GameplayManager.instance.isSinglePlayer)
            {
                ballMarkerObject = Instantiate(ballMarkerPrefab);
                youMarkerObject = Instantiate(youMarkerPrefab);
                mySelectedCircle.color = Color.white;
                spriteOutline = spriteOutlineWhite;
            }
            else
            {
                if (this.ownerConnectionId != -1)
                {
                    ballMarkerObject = Instantiate(ballMarkerPrefab);
                    youMarkerObject = Instantiate(youMarkerPrefab);
                    mySelectedCircle.color = Color.white;
                    spriteOutline = spriteOutlineWhite;
                }
                else
                {
                    youMarkerObject = new GameObject("empty-you-marker");
                    ballMarkerObject = Instantiate(ballMarkerOpponentPrefab);
                    mySelectedCircle.color = Color.yellow;
                    spriteOutline = spriteOutlineYellow;
                }
            }
        }
        else
        {
            ballMarkerObject = Instantiate(ballMarkerOpponentPrefab);
            mySelectedCircle.color = Color.yellow;
            spriteOutline = spriteOutlineYellow;
        }
            

        ballMarkerObject.transform.SetParent(this.transform);
        Vector3 markerPosition = myStatusBars.transform.localPosition;
        markerPosition.y += 0.75f;
        ballMarkerObject.transform.localPosition = markerPosition;
        ballMarkerObject.SetActive(false);
        if (IsOwner)
        {
            youMarkerObject.transform.SetParent(this.transform);
            youMarkerObject.transform.localPosition = markerPosition;
            youMarkerObject.SetActive(false);
        }
    }
    // Start is called before the first frame update
    private void Awake()
    {
        networkAnimator = GetComponent<NetworkAnimator>();
        _syncHealth.OnChange +=(prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) UpdateGoblinHealth(prev, next); };
        _syncStamina.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) UpdateGoblinStamina(prev, next); };
        _syncIsCharacterSelected.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleCharacterSelected(prev, next); };
        _syncIsQGoblin.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleIsQGoblin(prev, next); };
        _syncIsEGoblin.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleIsEGoblin(prev, next); };
        _syncIsPunching.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleIsPunching(prev, next); };
        _syncIsGoblinGrey.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleIsGoblinGrey(prev, next); };
        _syncDoesCharacterHaveBall.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleHasBall(prev, next); };
        _syncIsThrowing.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleIsThrowing(prev, next); };
        _syncIsFatigued.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleIsFatigued(prev, next); };
        _syncIsGoblinKnockedOut.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleIsGoblinKnockedOut(prev, next); };
        _syncIsSliding.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleIsSliding(prev, next); };
        _syncIsSlidingRoutineRunning.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleIsSlidingRoutineRunning(prev, next); };
        _syncIsDiving.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleIsDiving(prev, next); };
        _syncDivingRoutineRunning.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleIsDivingRoutineRunning(prev, next); };
        _syncIsBlocking.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleIsBlocking(prev, next); };
        _syncIsKicking.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleIsKicking(prev, next); };
        _syncKickAfterAccuracyDifficulty.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleKickAfterAccuracyDifficultyUpdate(prev, next); };
        _syncIsWasPunchedRoutineRunning.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleWasPunchedRoutineRunning(prev, next); };
        _syncCanGoblinReceivePass.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleCanGoblinReceivePass(prev, next); };
        
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
        myRenderer.material = spriteDefault;
    }
    public void SelectThisCharacter()
    {
        Debug.Log("SelectThisCharacter " + this.name);
        if (IsOwner)
            CmdSelectThisCharacter();
    }

    [ServerRpc]
    void CmdSelectThisCharacter()
    {
        isCharacterSelected = true;
    }
    public void UnSelectThisCharacter()
    {
        Debug.Log("UnSelectThisCharacter " + this.name);
        if (IsOwner)
            CmdUnSelectThisCharacter();
    }

    [ServerRpc]
    void CmdUnSelectThisCharacter()
    {
        isCharacterSelected = false;
    }
    public void HandleCharacterSelected(bool oldValue, bool newValue)
    {
        if (IsClientInitialized)
        {
            if (IsOwner)
            {
                if (newValue)
                {
                    //rb.bodyType = RigidbodyType2D.Dynamic;
                    Debug.Log("HandleCharacterSelected: Following new selected character " + this.name + " is this goblin grey? " + this.isGoblinGrey.ToString());
                    // Don't move the camera onto a goblin an AI is playing. This asked "single player and not my
                    // player", which was the same thing while the AI only existed in single player.
                    if (!myGamePlayer.isAIPlayer)
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
        //Debug.Log("SetQGoblin " + isQ.ToString());
        if (IsOwner)
        {
            if(GameplayManager.instance.is1v1 || GameplayManager.instance.isSinglePlayer)
                CmdSetQGoblin(isQ);
            else
                SetQGoblinLocally3v3(isQ);
            if (isBlocking)
                CmdSetBlocking(false);
        }
            
    }
    [ServerRpc]
    void CmdSetQGoblin(bool isQ)
    {
        Debug.Log("CmdSetQGoblin " + isQ.ToString() + " "  + this.name + " is grey? " + this.isGoblinGrey.ToString());
        isQGoblin = isQ;
    }
    public void HandleIsQGoblin(bool oldValue, bool newValue)
    {
        if (IsClientInitialized)
        {
            if (IsOwner)
            {
                if (IsOwner)
                {
                    if (this.myGamePlayer.isAIPlayer)
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
        if (IsOwner)
        {
            if (GameplayManager.instance.is1v1 || GameplayManager.instance.isSinglePlayer)
                CmdSetEGoblin(isE);
            else
                SetEGoblinLocally3v3(isE);
            if (isBlocking)
                CmdSetBlocking(false);
        }
            
    }
    [ServerRpc]
    void CmdSetEGoblin(bool isE)
    {
        Debug.Log("CmdSetEGoblin " + isE.ToString() + " is grey? " + this.isGoblinGrey.ToString());
        isEGoblin = isE;
    }

    public void HandleIsEGoblin(bool oldValue, bool newValue)
    {
        if (IsClientInitialized)
        {
            if (IsOwner)
            {
                if (this.myGamePlayer.isAIPlayer)
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
        if (IsServerInitialized)
        {
            // Check if goblin can be passed too?
            if (serverGamePlayer.doesTeamHaveBall && !doesCharacterHaveBall)
            {
                float diffFromFootball = gameFootball.transform.position.x - this.transform.position.x;
                if (isGoblinGrey)
                {
                    if (diffFromFootball <= 0)
                    {
                        canGoblinReceivePass = true;
                    }
                    else
                    {
                        canGoblinReceivePass = false;
                    }
                }
                else
                {
                    if (diffFromFootball >= 0)
                    {
                        canGoblinReceivePass = true;
                    }
                    else
                    {
                        canGoblinReceivePass = false;
                    }
                }
            }
            else
            {
                canGoblinReceivePass = true;
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
        if (IsClientInitialized)
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
    [Client(Logging = LoggingType.Off)]
    private void FixedUpdate()
    {
        if (canGoblinRegainHealth && health < MaxHealth && IsOwner)
            CmdRegainHealth();
        Move();
    }
    // Input callbacks: their subscriptions outlive this goblin, so they stay quiet after it despawns.
    [Client(Logging = LoggingType.Off)]
    private void SetMovement(Vector2 movement) => previousInput = movement;

    [Client(Logging = LoggingType.Off)]
    private void ResetMovement() => previousInput = Vector2.zero;

    [Client]
    private void Move()
    {
        if (IsOwner)
        {
            if (isFatigued)
                CmdRecoverStamina();
            else if ((!isSprinting) && !isFatigued && stamina < MaxStamina)
                CmdRecoverStamina();

            /*if (isGoblinKnockedOut)
                CmdRecoverHealth();*/
        }
        //if (hasAuthority && isCharacterSelected && canGoblinMove && !isGoblinPunching && !isGoblinKnockedOut && !isSliding && !isDiving)
        if (IsOwner && isCharacterSelected && canGoblinMove && !isGoblinKnockedOut && !isSliding && !isDiving && !this.myGamePlayer.isAIPlayer && GameplayManager.instance.gamePhase != "kickoff")
        {
            CmdIsPlayerSprinting(isSprinting);
            isRunning = false;
            if (previousInput.x != 0 || previousInput.y != 0)
                isRunning = true;
            CmdSetIsRunningOnServer(isRunning, previousInput);

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
        else if (IsOwner && (isSliding || isDiving) && slideDirection != Vector2.zero)
        {
            Vector2 direction = Vector2.ClampMagnitude(slideDirection, 1);
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        }
        // gameFootball can be gone for a frame or two when a game is ending or a player has just left, and the goblin
        // chasing code below reads it every physics frame.
        if (IsOwner && !isCharacterSelected && gameFootball != null && canGoblinMove && !isGoblinKnockedOut && !isSliding && !isDiving && (GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time"))
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
                    MoveTowardFootball();
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
                case State.SpyBallCarrier:
                    MoveTowardSpyPosition();
                    break;
            }
        }
        // Check for the AI character for what to do with their goblin that they have selected?
        else if (IsOwner && this.isCharacterSelected && this.myGamePlayer.isAIPlayer && gameFootball != null && canGoblinMove && !isGoblinKnockedOut && !isSliding && !isDiving && (GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time"))
        {
            if (this.doesCharacterHaveBall)
                state = State.RunTowardEndzone;
            // A teammate holding the ball is not something to run at. The branch above, for an AI's unselected
            // goblins, asks this first; this one never did. While one player owned all three goblins its selected
            // goblin was usually the carrier itself, so it never showed. In a 3v3 a bot owns one goblin and the
            // carrier is always another player's, so every bot charged its own ball carrier.
            else if (myGamePlayer.doesTeamHaveBall)
            {
                state = State.TeamHasBall;
                goblinTarget = null;
            }
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
                case State.SpyBallCarrier:
                    MoveTowardSpyPosition();
                    break;
                case State.RunTowardEndzone:
                    myGamePlayer.myAiPlayer.RunTowardEndZone();
                    break;
            }

        }

    }
    public void FlipRenderer(bool flip)
    { 
        if(IsOwner)
        {
            if (this.myRenderer.flipX != flip)
                CmdFlipRenderer(flip);
        }
    }
    [ServerRpc]
    void CmdFlipRenderer(bool flip)
    {
        if(myRenderer.flipX != flip)
            myRenderer.flipX = flip;
        RpcFlipRenderer(flip);
    }
    [ObserversRpc]
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
        //Debug.Log("OnTriggerEnter2D for Goblin");
        if (collision.tag == "football")
        {
            //Debug.Log("Goblin collided with football");
        }
        if (collision.tag == "punchbox")
        {
            //Debug.Log(this.name + " was punched by: " + collision.transform.parent.name);
            /*if (isServer)
            {
                //this is to disable the body collider to make sure damage isn't done twice in a row too quickly.
                DealDamageToGoblins(this, collision.transform.parent.GetComponent<GoblinScript>());
            }*/
                
        }
    }
    public void SetGoblinHasBall(bool doesHaveBall)
    {
        if (IsOwner)
            CmdSetGoblinHasBall(doesHaveBall);
    }
    [ServerRpc]
    void CmdSetGoblinHasBall(bool doesHaveBall)
    {
        doesCharacterHaveBall = doesHaveBall;
    }
    public void HandleHasBall(bool oldValue, bool newValue)
    {
        if (IsServerInitialized)
        {
            ParentFootballOnServer(newValue);
        }
        if (IsClientInitialized)
        {
            if (IsOwner)
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
                    if (this.myGamePlayer.isAIPlayer)
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
                if (!this.isCharacterSelected)
                {
                    //Debug.Log("HandleHasBall: Player that is not selected has the ball: " + this.name);
                    // On the host this runs twice: from the server's direct call and from the SyncVar hook. Mirror's host
                    // ran the selection Cmds at once, so the second run saw this goblin selected. FishNet runs them a tick
                    // later, so also check the player's own selection, or the second run switches back and throws the ball.
                    if (IsOwner && myGamePlayer.selectGoblin != this)
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
                    else if (IsOwner)
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
                                //Debug.Log("HandleHasBall: hasGoblinGottenBallBefore is false and goblin is on same team as local player. Goblin: " + this.name + ":" + this.ownerConnectionId.ToString() + " local player number: " + localPlayer.playerNumber.ToString());
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
                                spriteOutline = spriteOutlineWhite;
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log("HandleHasBall: failed to get if goblin is on same team as local player for 3v3 game. Error: " + e);
                        }
                        hasGoblinGottenBallBefore = true;
                    }
                }
                // Set the sprite outline material
                myRenderer.material = spriteOutline;
            }
            else
            {
                if (IsOwner && powerBarActive)
                    ResetPowerBar();
                // Set the sprite outline material
                myRenderer.material = spriteDefault;
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
            // Checks for setting the sprite outline material
            /*if (newValue)
            {
                if (hasAuthority)
                {
                    if (myGamePlayer.isSinglePlayer)
                    { 
                        if(myGamePlayer.isLocalPlayer)
                            myRenderer.material = spriteOutlineWhite;
                        else
                            myRenderer.material = spriteOutlineYellow;
                    }
                    else
                        myRenderer.material = spriteOutlineWhite;
                }
                else
                {
                    myRenderer.material = spriteOutlineYellow;
                }
            }
            else
            {
                myRenderer.material = spriteDefault;
            }*/
        }
    }
    // The football used to be parented here on every machine. FishNet sends SyncVars every 0.1 s but the football's
    // position every tick, so clients placed a dropped ball's world position under the goblin that still held it.
    // Now only the server parents it, through FishNet, and the football's NetworkTransform syncs the parent with the position.
    void ParentFootballOnServer(bool hasBall)
    {
        GameObject football = GameObject.FindGameObjectWithTag("football");
        NetworkObject footballNetworkObject = football.GetComponent<NetworkObject>();
        if (hasBall)
        {
            if (football.transform.parent != this.transform)
                footballNetworkObject.SetParent(this);
            football.transform.localPosition = new Vector3(0f, 0f, 0f);
        }
        else if (football.transform.parent != null)
        {
            footballNetworkObject.UnsetParent();
        }
    }
    [ServerRpc]
    void CmdCheckIfTeamStillHasBall()
    {
        // A goblin's own ServerRpc can land a tick after its player has disconnected, and FishNet has already despawned
        // everything that player owned by then. Nothing is left to update, so drop it.
        if (!ObjectLookup.TryGet(ownerNetId, out GamePlayer goblinOwnerScript))
            return;
        if (this.doesCharacterHaveBall)
        {
            //Debug.Log("CmdCheckIfTeamStillHasBall: Goblin has ball. Goblin's team still has ball. Setting doesTeamHaveBall to true for player: " + goblinOwnerScript.PlayerName);
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
            Team ballCheckTeamRoster = TeamOf(goblinOwnerScript.isTeamGrey);
            if (ballCheckTeamRoster == null)
                return;
            foreach (int goblinNetId in ballCheckTeamRoster.goblinNetIds)
            {
                // A player who leaves has their goblins despawned, but their ids stay in this list, so skip the ones
                // that are no longer spawned rather than throwing on the server. This was two null references in the
                // six-instance 3v3, at the moments players left.
                if (!ObjectLookup.TryGet(goblinNetId, out GoblinScript goblinNetIdScript))
                    continue;
                if (goblinNetIdScript.doesCharacterHaveBall)
                {
                    anyGoblinHaveBall = true;
                    break;
                }
            }
            //Debug.Log("CmdCheckIfTeamStillHasBall: Do any goblins on " + goblinOwnerScript.PlayerName + " team have the ball? " + anyGoblinHaveBall.ToString());
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
            if (IsOwner)
            {
                /*IEnumerator throwing = DisableColliderForThrow();
                //DisableColliderForThrow();
                StartCoroutine(throwing);*/
                golbinBodyCollider.enabled = false;
                CmdStartThrowing();
            }

            footballScript.CmdThrowFootball(this.gameObject.GetComponent<NetworkObject>().ObjectId, goblinToThrowTo.gameObject.GetComponent<NetworkObject>().ObjectId);
        }
        
    }
    [ServerRpc]
    void CmdStartThrowing()
    {
        if(!isKicking && !isDiving)
            this.isThrowing = true;
    }
    public void HandleIsThrowing(bool oldValue, bool newValue)
    {
        if (IsClientInitialized)
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
        if (IsOwner)
            CmdStopThrowing();
    }
    [ServerRpc]
    void CmdStopThrowing()
    {
        this.isThrowing = false;
    }
    [ServerRpc]
    void CmdMakeGoblinGrey()
    {
        isGoblinGrey = true;
    }
    public void HandleIsGoblinGrey(bool oldValue, bool newValue)
    {
        if (IsServerInitialized)
        {
            //Debug.Log("HandleIsGoblinGrey: current goblin layeR: " + this.gameObject.layer);
            /*if (newValue)
                this.gameObject.layer = greyGoblinLayer.value;
            else
                this.gameObject.layer = greenGoblinLayer.value;*/
            //Debug.Log("HandleIsGoblinGrey: current goblin layeR: " + this.gameObject.layer + " green layer: " + greenGoblinLayer.value + " grey layeR: " + greyGoblinLayer.value);
        }   
        if (IsClientInitialized)
        {
            Debug.Log("HandleIsGoblinGrey: current goblin layeR: " + this.gameObject.layer);
            if (newValue)
            {
                animator.SetBool("isGrey", newValue);
                int layerInt = LayerMask.NameToLayer(greyGoblinLayer);
                Debug.Log("HandleIsGoblinGrey: changing goblin layer to: " + layerInt);
                this.gameObject.layer = layerInt;
            }
            else
            {
                int layerInt = LayerMask.NameToLayer(greenGoblinLayer);

                this.gameObject.layer = layerInt;
            }
            Debug.Log("HandleIsGoblinGrey: current goblin layeR: " + this.gameObject.layer + " green layer: " + greenGoblinLayer.ToString() + " grey layeR: " + greyGoblinLayer.ToString());
            if (newValue && IsOwner)
            {
                FlipKickoffAimArrow();
            }
        }
    }
    void IsPlayerSprinting(bool isPlayerSprinting)
    {
        if (IsOwner)
        {

            /*CmdIsPlayerSprinting(isPlayerSprinting);
            isSprinting = isPlayerSprinting;
            if (isPlayerSprinting)
                pressedTime = Time.time;
            else
                releasedTime = Time.time;*/
            if (isBlocking)
                isPlayerSprinting = false;
                
            //Debug.Log("IsPlayerSprinting: " + isPlayerSprinting.ToString() + " for goblin: " + this.name + " owned by player " + this.ownerName + ":" + this.ownerConnectionId.ToString());
            if (this.isCharacterSelected)
                isSprinting = isPlayerSprinting;
            else
                isSprinting = false;
        }
            
    }
    [ServerRpc]
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
                    //this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer * blockingSpeedModifier * wasPunchedSpeedModifier * speedModifierFromPowerUps * slowDownObstacleModifier * possessionSpeedBonus) * 1.15f;
                    this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer * blockingSpeedModifier * wasPunchedSpeedModifier * speedModifierFromPowerUps * slowDownObstacleModifier * possessionSpeedBonus) * _sprintSpeedModifier;
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
    public float GetSprintSpeedModifier()
    {
        return _sprintSpeedModifier;
    }
    [ServerRpc]
    void CmdSetIsRunningOnServer(bool isPlayerRunning, Vector2 runningDirection)
    {
        isRunningOnServer = isPlayerRunning;
        runningDirectionOnServer = runningDirection;
    }
    [Server]
    public Vector2 GetRunningDirectionOnServer()
    {
        return runningDirectionOnServer;
    }
    [ServerRpc]
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
                isFatigued = true;
                this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer * blockingSpeedModifier * wasPunchedSpeedModifier * speedModifierFromPowerUps * slowDownObstacleModifier) * 0.5f;
            }
        }
        
    }
    public void HandleIsFatigued(bool oldValue, bool newValue)
    {
        if (IsClientInitialized)
        {
            myStatusBars.ChangeStaminaBarColor(newValue);
            /*fatigueSweatDrop.SetActive(newValue);
            if(newValue)
                spriteFlash.Flash(Color.yellow);*/
            FatigueIndicators(newValue);
            if (IsOwner)
            {
                if (this.myGamePlayer.isAIPlayer)
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
    [ServerRpc]
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
                    isFatigued = false;
                }
            }
        }        
    }
    [ServerRpc]
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
        if(!this.isRunning)
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
                PlayNetworkedState(goblinType + "-punch-running");
            }
            else
            {
                PlayNetworkedState(goblinType + "-punch");
            }

        }
    }
    // FishNet's NetworkAnimator only sends state changes made through it (Mirror's picked up animator.Play). The server's
    // copy has to play these states too: the punch, dive and touchdown hitboxes are driven by the animations.
    // NegativeInfinity keeps animator.Play's behavior of not restarting a state that's already playing.
    void PlayNetworkedState(string stateName)
    {
        networkAnimator.Play(stateName, 0, float.NegativeInfinity);
    }
    public void HurtBoxCollision(GoblinScript punchingGoblin)
    {

        if (IsServerInitialized)
        {
            /*if(this.ownerConnectionId != punchingGoblin.ownerConnectionId)
                DealDamageToGoblins(this, punchingGoblin);*/
            if (this.isGoblinGrey != punchingGoblin.isGoblinGrey)
                DealDamageToGoblins(this, punchingGoblin);
        }
        if (IsClientInitialized)
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
                    if(this.IsOwner && this.isCharacterSelected)
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
        //Debug.Log("KnockOutGoblin: " + knockedOut.ToString());
        if (this.invinvibilityBlueShell)
            return;
        if (GameplayManager.instance.gamePhase == "kickoff")
            return;
        if (this.health < 0f)
            this.health = 0f;



        isGoblinKnockedOut = true;
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

        //Code here for ending kick-after-attempt?
        if (GameplayManager.instance.gamePhase == "kick-after-attempt" && this.isKickAfterGoblin && !gameFootball.isKicked && doesCharacterHaveBall)
        {
            GameplayManager.instance.KickAfterAttemptWasBlocked();
            RpcKickBlockedStopKickAfterAttempt(this.Owner);
        }
        else
        {
            TeamManager.instance.KnockedOutOrTripped(this.serverGamePlayer.isTeamGrey, knockedOut);
        }

        if (doesCharacterHaveBall)
        {
            //Debug.Log("KnockOutGoblin: Goblin with football was knocked out. Goblin will need to fumble the football.");
            doesCharacterHaveBall = false;
            Football footballScript = GameObject.FindGameObjectWithTag("football").GetComponent<Football>();
            footballScript.FumbleFootball();
            if (GameplayManager.instance.gamePhase != "kick-after-attempt")
                TeamManager.instance.FumbleBall(this.serverGamePlayer.isTeamGrey);
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
        isGoblinKnockedOut = false;

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
    [ServerRpc]
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
        if (IsClientInitialized)
        {
            golbinBodyCollider.enabled = !newValue;
            myStatusBars.gameObject.SetActive(!newValue);
            if (IsOwner)
            {
                if (isBlocking)
                    CmdSetBlocking(false);                

                // code for knocked out animation?
                if (newValue)
                {
                    // CrossFade rather than Play: NetworkAnimator.Play sends whatever state the animator is in at the next
                    // tick. The bomb and lightning knock goblins out from animation events, after this frame's animator
                    // update, so a host goblin sent its old state (running) to clients. CrossFade sends this state itself.
                    networkAnimator.CrossFade(goblinType + "-knocked-out", 0f, 0, 0f);

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
                if(IsOwner && this.isCharacterSelected)
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
    [ServerRpc]
    void CmdStopSliding()
    {
        isSliding = false;
        slideSpeedModifer = 1.0f;
        slideDirection = Vector2.zero;
        if(isSlidingRoutineRunning)
            StopCoroutine(isSlidingRoutine);
        this.isSlidingRoutineRunning = false;
    }
    public void GoblinPickUpFootball()
    {
        if (IsOwner && !isThrowing)
            CmdGoblinPickUpFootball();
    }
    [ServerRpc]
    void CmdGoblinPickUpFootball()
    {
        //Debug.Log("CmdGoblinPickUpFootball: for goblin: " + this.name + " owned by player " + this.serverGamePlayer.PlayerName);
        GameObject.FindGameObjectWithTag("football").GetComponent<Football>().PlayerPickUpFootball(this.GetComponent<NetworkObject>().ObjectId);
    }
    public void UpdateGoblinHealth(float oldValue, float newValue)
    {
        if (IsServerInitialized)
            health = newValue;
        if (IsClientInitialized)
        {
            float newHealth = newValue;
            if (newHealth < 0f)
                newHealth = 0f;

            myStatusBars.HealthBarUpdate(newHealth / MaxHealth);

        }
    }
    public void UpdateGoblinStamina(float oldValue, float newValue)
    {
        if (IsServerInitialized)
            stamina = newValue;
        if (IsClientInitialized)
        {
            float newStamina = newValue;
            if (newStamina < 0f)
                newStamina = 0f;
            try
            {
                if (IsOwner)
                {
                    if (this.myGamePlayer.isAIPlayer)
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
        if (IsOwner)
        {
            if (isBlocking)
                CmdSetBlocking(false);
            CmdSlideGoblin(previousInput);
        }
            
    }
    [ServerRpc]
    void CmdSlideGoblin(Vector2 directionToSlide)
    {
        if (isRunningOnServer && directionToSlide != Vector2.zero && !doesCharacterHaveBall && !isSliding && !isFatigued && !isPunching && !isGoblinKnockedOut)
        {
            //Debug.Log("CmdSlideGoblin: Goblin will slide in direction of: " + directionToSlide.ToString());
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
        if (IsClientInitialized)
        {
            if (IsOwner)
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
        this.isSlidingRoutineRunning = true;
        isSliding = true;
        yield return new WaitForSeconds(0.25f);
        speed = MaxSpeed * 1.2f;
        //Debug.Log("SlideGoblinRoutine: Set speed to 1.2x for goblin " + this.name + " owned by player: " + this.ownerConnectionId.ToString());
        yield return new WaitForSeconds(0.25f);
        isSliding = false;
        slideSpeedModifer = 0.7f;
        yield return new WaitForSeconds(2.25f);
        slideSpeedModifer = 1.0f;
        yield return new WaitForSeconds(0.75f);
        slideDirection = Vector2.zero;
        this.isSlidingRoutineRunning = false;
    }
    public void SlideBoxCollision(GoblinScript slidingGoblin)
    {
        if (IsServerInitialized)
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
                //GoblinScript collidingGoblin = collision.collider.GetComponent<GoblinScript>();
                //Debug.Log("SlideBoxCollision: Is the sliding goblin " + slidingGoblin.name + " the kickafter goblin? " + slidingGoblin.isKickAfterGoblin + " has ball? " + slidingGoblin.doesCharacterHaveBall);
                if (GameplayManager.instance.gamePhase == "kick-after-attempt" && slidingGoblin.isKickAfterGoblin && slidingGoblin.isGoblinGrey != this.isGoblinGrey && slidingGoblin.isKickAfterPositionSet && !gameFootball.isKicked)
                {   
                    if (this.serverGamePlayer)
                    {
                        serverGamePlayer.RpcBlockedKickAchievement(serverGamePlayer.Owner);
                    }
                }
            }   
        }
    }
    public void TripGoblin()
    {
        if (!isGoblinKnockedOut)
        {
            //Debug.Log("TripGoblin: Tripping goblin: " + this.name);
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

        isGoblinKnockedOut = false;

        if (isRegainHealthRoutineRunning)
            StopCoroutine(regainHealthRoutine);

        regainHealthRoutine = RegainHealth();
        StartCoroutine(regainHealthRoutine);
    }
    public void DiveGoblin()
    {
        if (IsOwner)
        {
            if (isRunning)
            {
                if (!isGoblinKnockedOut && !isSliding && !isDiving && IsOwner && !isPunching)
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
        if (!isGoblinKnockedOut && !isSliding && !isDiving && IsOwner && !isPunching)
        {
            CmdStartDiving(diveDirection);
            if (isBlocking)
                CmdSetBlocking(false);
        }
    }
    [ServerRpc]
    void CmdStartDiving(Vector2 directionToDive)
    {
        //Debug.Log("CmdStartDiving: isRunningOnServer: " + this.isRunningOnServer.ToString() + " Direction to move: " + directionToDive.ToString());
        if (isRunningOnServer && directionToDive != Vector2.zero && !isSliding && !isFatigued && !isDiving && !DivingRoutineRunning && !isSlidingRoutineRunning && !isPunching && !this.isKicking)
        {
            //Debug.Log("CmdStartDiving: Goblin will slide in direction of: " + directionToDive.ToString());
            slideDirection = directionToDive;
            isDiving = true;
            speed = MaxSpeed * 1.2f;
            //Debug.Log("CmdStartDiving: Set speed to 1.2x for goblin " + this.name + " owned by player: " + this.ownerConnectionId.ToString());
        }
            
    }
    public void HandleIsDiving(bool oldValue, bool newValue)
    {
        if (IsClientInitialized)
        {

            if (IsOwner && newValue)
            {
                
                if (doesCharacterHaveBall)
                {
                    PlayNetworkedState(goblinType + "-dive-with-football");
                }
                else
                {
                    PlayNetworkedState(goblinType + "-dive");
                }
                IEnumerator divingResetSanityCheckRoutine = DivingResetSanityCheckRoutine();
                StartCoroutine(divingResetSanityCheckRoutine);
            }
            if (IsOwner && !newValue)
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
        if (IsOwner)
        {
            CmdStopDiving();
        }
    }
    [ServerRpc]
    void CmdStopDiving()
    {
        if (isDiving)
        {
            isDiving = false;
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
        this.DivingRoutineRunning = true;
        slideSpeedModifer = 0.7f;
        yield return new WaitForSeconds(1.25f);
        slideSpeedModifer = 1.0f;
        yield return new WaitForSeconds(0.75f);
        slideDirection = Vector2.zero;
        //DivingRoutineRunning = false;
        this.DivingRoutineRunning = false;
    }
    public void StartBlocking()
    {
        if (IsOwner)
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
        if (IsOwner)
        {
            CmdSetBlocking(false);
            if(Input.GetKey(KeyCode.LeftShift))
                IsPlayerSprinting(true);
        }
            
    }
    [ServerRpc]
    void CmdSetBlocking(bool isGoblinBlocking)
    {
        //Debug.Log("CmdSetBlocking: Set blocking to " + isGoblinBlocking.ToString() + " for goblin " + this.name);
        //isBlocking = isGoblinBlocking;
        if (!isSliding && !isDiving && !isGoblinKnockedOut)
        {
            if (isGoblinBlocking)
                blockingSpeedModifier = 0.8f;
            else
            {
                blockingSpeedModifier = 1.0f;
            }

            isBlocking = isGoblinBlocking;
        }
        else
        {
            blockingSpeedModifier = 1.0f;
            isBlocking = false;
        }
    }
    public void HandleIsBlocking(bool oldValue, bool newValue)
    {
        if (IsClientInitialized)
        {
            if (IsOwner)
            {
                animator.SetBool("isBlocking", newValue);
                if (newValue)
                {
                    if (doesCharacterHaveBall)
                    {
                        PlayNetworkedState(goblinType + "-block-with-football");
                    }
                    else
                    {
                        PlayNetworkedState(goblinType + "-block");
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
        if (IsOwner)
            CmdPunching(true);
    }
    public void StopPunching()
    {
        if (IsOwner)
            CmdPunching(false);
    }
    [ServerRpc]
    void CmdPunching(bool punching)
    {
        //isPunching = punching;
        isPunching = punching;
        /*if (this.isCharacterSelected && punching)
            TeamManager.instance.ThrownPunch(this.serverGamePlayer);*/
        TeamManager.instance.ThrownPunch(this);
    }
    public void HandleIsPunching(bool oldValue, bool newValue)
    {
        if (IsClientInitialized)
        {
            if (IsOwner)
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
        if (IsOwner)
        {
            //this.KickFootballGoblin(0.5f, 0f);
            this.StopKickPower();
        }
    }
    public void KickFootballGoblin(float kickPower, float kickAngle)
    {
        if (IsOwner && !isPunching && !isDiving && !isSliding && !isGoblinKnockedOut && doesCharacterHaveBall && !isThrowing)
        {
            CmdKickFootball(kickPower, kickAngle);
        }
            
    }
    [ServerRpc]
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
            isKicking = true;
            GoblinKickPower = kickPower;
            GoblinKickoffAngle = kickAngle;
        }
    }
    public void HandleIsKicking(bool oldValue, bool newValue)
    {
        if (IsClientInitialized)
        {
            //Debug.Log("HandleIsKicking: isClient");
            if (IsOwner)
            {
                //Debug.Log("HandleIsKicking: hasAuthority");
                if (newValue)
                {
                    //Debug.Log("HandleIsKicking: start the kick animation");
                    PlayNetworkedState(goblinType + "-kick-football");
                }
                    
            }
            
        }
    }
    public void KickTheFootball()
    {
        //Debug.Log("KickTheFootball");
        if (IsOwner && !isPunching && !isDiving && !isSliding && !isGoblinKnockedOut && doesCharacterHaveBall && !isThrowing)
        {
            CmdKickTheFootball();
        }
        KickedFootballSFX();
            
    }
    [ServerRpc]
    void CmdKickTheFootball()
    {
        Debug.Log("CmdKickTheFootball()");

        if (this.isGoblinKnockedOut)
        {
            //Debug.Log("CmdKickTheFootball: this goblin is knocked out on the server. Don't kick? Returning..." );
            return;
        }
        doesCharacterHaveBall = false;
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
        //Debug.Log("StopKicking");
        if (IsOwner)
            CmdStopKicking();
    }
    [ServerRpc]
    void CmdStopKicking()
    {
        isKicking = false;
    }
    public void HandleCanGoblinReceivePass(bool oldValue, bool newValue)
    {
        if (IsClientInitialized)
        {
            if (IsOwner)
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
        this.isWasPunchedRoutineRunning = true;
        wasPunchedSpeedModifier = 0.8f;
        yield return new WaitForSeconds(3.0f);
        wasPunchedSpeedModifier = 1.0f;
        //isWasPunchedRoutineRunning = false;
        this.isWasPunchedRoutineRunning = false;
    }
    public void ToggleGoblinBodyCollider()
    {
        golbinBodyCollider.gameObject.SetActive(false);
        golbinBodyCollider.gameObject.SetActive(true);
    }
    void FlipKickoffAimArrow()
    {
        //Debug.Log("FlipKickoffAimArrow: for goblin: " + this.name + " is that goblin grey? " + this.isGoblinGrey.ToString());
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
        // An AI never aims, so its goblin never shows the arrow. The three cases this replaced came to the same
        // thing while the AI only existed in single player.
        KickoffAimArrow.SetActive(activate && !this.myGamePlayer.isAIPlayer);
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
        kickAfterAccuracyDifficulty = newDifficulty;
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
        if (IsClientInitialized)
        {
            if (IsOwner)
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
        if (IsOwner)
            CmdSubmitKickAfterPositionToServer(transform.position);

    }
    [ServerRpc]
    void CmdSubmitKickAfterPositionToServer(Vector3 kickAfterPosition)
    {
        if (Vector2.Distance(kickAfterPosition, this.transform.position) > 3.0f)
            return;

        kickAfterFinalPosition = kickAfterPosition;
        isKickAfterPositionSet = true;
        GameplayManager.instance.DisableKickAfterPositioningControls();
        GameplayManager.instance.StartKickAfterTimer();
        this.RpcKickAfterPositionFromServer(this.Owner, kickAfterPosition);
        
    }
    [TargetRpc]
    void RpcKickAfterPositionFromServer(NetworkConnection target, Vector2 newPosition)
    {
        if (IsOwner)
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
            if (IsOwner)
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
    [ServerRpc]
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
        if (IsServerInitialized)
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
                        if (this.serverGamePlayer)
                        {
                            serverGamePlayer.RpcBlockedKickAchievement(serverGamePlayer.Owner);
                        }
                    }
                    /*if (this.isKickAfterGoblin)
                    {
                        Debug.Log("GoblinScript: OnCollisionEnter2D: THIS GOBLIN is the kick after goblin! And another goblin ran into it! This goblin: " + this.name + " colliding Goblin: " + collidingGoblin.name);
                        KickAfterGoblinWasRunInto();
                    }*/
                }
            }
        }
        if(IsClientInitialized)
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
        Debug.Log("KickAfterGoblinWasRunInto()");
        this.KnockOutGoblin(false);
        RpcKickBlockedStopKickAfterAttempt(this.Owner);
    }
    [Server]
    public void StartAttackNormal()
    {
        IEnumerator attackNormalRoutine = AttackNormalRoutine();
        StartCoroutine(attackNormalRoutine);
    }
    [Server]
    IEnumerator AttackNormalRoutine()
    {
        this.damage = MaxDamage * 2.0f;
        attackNormal = true;
        yield return new WaitForSeconds(3.0f);
        this.damage = MaxDamage;
        attackNormal = false;
    }
    [Server]
    public void StartDefenseNormal()
    {
        IEnumerator defenseNormalRoutine = DefenseNormalRoutine();
        StartCoroutine(defenseNormalRoutine);
    }
    [Server]
    IEnumerator DefenseNormalRoutine()
    {
        defenseModifier = 0.35f;
        defenseNormal = true;
        yield return new WaitForSeconds(3.0f);
        defenseModifier = 1.0f;
        defenseNormal = false;
    }
    [Server]
    public void StartSpeedNormal()
    {
        IEnumerator speedNormalRoutine = SpeedNormalRoutine();
        StartCoroutine(speedNormalRoutine);
    }
    [Server]
    IEnumerator SpeedNormalRoutine()
    {
        speedModifierFromPowerUps = 1.2f;
        speedNormal = true;
        yield return new WaitForSeconds(3.0f);
        speedModifierFromPowerUps = 1.0f;
        speedNormal = false;
    }
    [ObserversRpc]
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
            isGoblinKnockedOut = false;
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
    [Server]
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
    [ObserversRpc]
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
            this.isFatigued = false;
        }
    }
    [ObserversRpc]
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
            if (footBallLandingSpot != null)
            {
                Debug.Log("MoveTowardFootball: footBallLandingSpot found!");
                positionOfFootball = footBallLandingSpot.transform.position;
            }
            else
            {
                Debug.Log("MoveTowardFootball: could not find the football landing spot idk how.");
                positionOfFootball = gameFootball.transform.position;
            }
            

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
                this.goblinTarget = ObjectLookup.Get(gameFootball.goblinWithBallNetId).GetComponent<GoblinScript>();
                
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
        CmdSetIsRunningOnServer(isRunning, direction);

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
        // The carrier was read as myGamePlayer.selectGoblin, which is the carrier only while one player owns all
        // three goblins. A 3v3 bot owns one, so that was this goblin's own position and it tried to get behind
        // itself. The football knows who is actually carrying.
        GoblinScript ballCarrier = gameFootball.goblinWithBall;
        Vector3 ballCarrierPosition = ballCarrier != null ? ballCarrier.transform.position : myGamePlayer.selectGoblin.transform.position;
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
    [ObserversRpc]
    public void RpcResetGoblinStatuses()
    {
        if (IsOwner)
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
    [ServerRpc]
    void CmdStopWasPunchedCoroutine()
    {
        if (isWasPunchedRoutineRunning)
        {
            StopCoroutine(isWasPunched);
            wasPunchedSpeedModifier = 1.0f;
            //isWasPunchedRoutineRunning = false;
            this.isWasPunchedRoutineRunning = false;
        }
    }
    // Resets the footstep modifers on the server. This controls the footstep sound that is played, as well as any slowDownObstacleModifier effects that are affecting the goblin.
    [ServerRpc]
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
        Team myTeamRoster = TeamOf(this.isGoblinGrey);
        if (myTeamRoster == null)
            return false;
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
                    // Take this opposing goblin only if nobody else on the team is already on it. Two bugs lived here.
                    // The list was myGamePlayer.myGoblins, which in a 3v3 holds only this goblin, so the loop always
                    // skipped itself, set nothing and returned false - the berserker could never brawl. And it took the
                    // target as soon as ONE teammate was not on it rather than when none were, which the comment it
                    // replaced describes correctly.
                    bool alreadyTargetedByTeammate = false;
                    foreach (int goblinNetId in myTeamRoster.goblinNetIds)
                    {
                        if (!ObjectLookup.TryGet(goblinNetId, out GoblinScript goblinOnMyTeam))
                            continue;
                        if (goblinOnMyTeam == null || goblinOnMyTeam == this)
                            continue;
                        // A goblin a person is driving does not keep goblinTarget up to date. isCharacterSelected on
                        // its own is no longer that test: in a 3v3 every goblin is its owner's selected one.
                        if (goblinOnMyTeam.isCharacterSelected && goblinOnMyTeam.myGamePlayer != null && !goblinOnMyTeam.myGamePlayer.isAIPlayer)
                            continue;
                        if (goblinOnMyTeam.goblinTarget == goblinBodyColliderScript)
                        {
                            alreadyTargetedByTeammate = true;
                            break;
                        }
                    }
                    if (!alreadyTargetedByTeammate)
                    {
                        this.goblinTarget = goblinBodyColliderScript;
                        goblinNearBy = true;
                        break;
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
        if (IsOwner)
            CmdUpdateHasGoblinRepositionedForKickAfter();
    }
    [ServerRpc]
    void CmdUpdateHasGoblinRepositionedForKickAfter()
    {
        this.hasGoblinBeenRepositionedForKickAfter = true;
        GameplayManager.instance.AreAllGoblinsRepositionedForKickAfter();
    }
    [Server]
    public void SlowDownObstacleEffect(bool stillColliding)
    {
        if (stillColliding && !this.invinvibilityBlueShell)
            slowDownObstacleModifier = 0.5f;
        else
            slowDownObstacleModifier = 1.0f;
    }
    [Server]
    public void KickAfterWaitToEnableObstacleColliders()
    {
        canCollide = false;
        IEnumerator waitToEnableObstacleColliders = WaitToEnableObstacleColliders();
        StartCoroutine(waitToEnableObstacleColliders);
    }
    [Server]
    IEnumerator WaitToEnableObstacleColliders()
    {
        yield return new WaitForSeconds(0.666f);
        canCollide = true;
    }
    [ObserversRpc]
    public void RpcPunchedFlashSprite()
    {
        Debug.Log("RpcPunchedFlashSprite: for goblin " + this.name);
        spriteFlash.Flash(Color.white);
    }
    public void FatigueIndicators(bool enable)
    {
        //Debug.Log("FatigueIndicators: for goblin " + this.name + " " + enable.ToString());
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
    // Animation event: it fires on every machine that plays the clip, so it skips quietly where it doesn't apply.
    [Client(Logging = LoggingType.Off)]
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
    [ObserversRpc]
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
        if (this.IsOwner && this.isCharacterSelected)
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
        if (IsOwner)
        {
            this.CmdCollisionWithObstacleObject(knockOut);
        }
    }
    [ServerRpc]
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
        //Debug.Log("AIResetSprintingParameters: for goblin: " + this.name + " owned by player: " + this.ownerConnectionId.ToString());
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
        //Debug.Log("AIWillGoblinKeepSprinting: for goblin: " + this.name + " owned by player: " + this.ownerConnectionId.ToString());
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
        if (myGamePlayer.isAIPlayer)
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
        GoblinScript ballCarrier = ObjectLookup.Get(gameFootball.goblinWithBallNetId).GetComponent<GoblinScript>();

        // Go through each goblin in the player's myGoblins and check who they are targetting (if any) and track how many are targetting the ball carrier
        int numberTargetingBallCarrier = 0;
        foreach (GoblinScript goblin in myGamePlayer.myGoblins)
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
    // The goblin's role decides what it does while the other team has the ball. This used to ask
    // AIShouldGoblinTargetBallCarrier, which opens with "return true", so every goblin always chased the carrier and
    // AttackNearbyGoblin could never be reached. That method is left in place but is no longer called.
    void AIGetGoblinTarget()
    {
        // A grey goblin's goblinType carries a team suffix: CmdSpawnPlayerCharacters sets "grenadier" and then
        // appends "-grey". An exact match here only ever fired for the green team, so every grey bot fell through
        // to the berserker branch in the first run with roles - attacking whoever was nearby instead of spying or
        // always-chasing. The rest of the codebase asks Contains for the same reason.
        if (this.goblinType.Contains("grenadier"))
        {
            AIGrenadierSpyOrChase();
            return;
        }
        if (this.goblinType.Contains("skirmisher"))
        {
            goblinTarget = null;
            state = State.ChaseBallCarrier;
            return;
        }
        // The berserker brawls. It keeps a target for as long as that target is worth keeping, and falls back to the
        // carrier when there is nobody to hit, so the defence is never all standing off the ball.
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
        if (newTargetNeeded)
        {
            goblinTarget = null;
            if (FindNearByGoblinToTarget())
                state = State.AttackNearbyGoblin;
            else
                state = State.ChaseBallCarrier;
        }
        else
        {
            state = State.AttackNearbyGoblin;
        }
    }
    // The team a goblin belongs to. Pair it with Team.goblinNetIds: the server adds each goblin to that SyncList as it
    // joins, so it is the roster, correct on every machine for people and bots alike. GamePlayer.goblinTeamNetIds used
    // to be asked instead, but that is a per-player copy of the same thing, built only for players that ran
    // GetGoblinTeammatesFor3v3 - which was the person alone, so a bot's copy held just its own goblin.
    // Resolve ids with ObjectLookup and skip the misses: the roster is not pruned when a player leaves.
    static Team TeamOf(bool isGrey)
    {
        if (TeamManager.instance == null)
            return null;
        return isGrey ? TeamManager.instance.greyTeam : TeamManager.instance.greenTeam;
    }
    // +1 for a grey goblin, -1 for a green one: the way its own endzone lies, the one the other team scores in. The
    // same convention GetOpenForPass uses to put a receiver behind its own ball carrier.
    float DirectionOfMyEndzone()
    {
        return this.isGoblinGrey ? 1f : -1f;
    }
    // The grenadier plays a spy. It holds off the ball only while a teammate is contesting it and while it is still
    // between the carrier and the endzone it defends; otherwise it goes for the tackle itself.
    void AIGrenadierSpyOrChase()
    {
        goblinTarget = null;
        GoblinScript ballCarrier = gameFootball != null ? gameFootball.goblinWithBall : null;
        if (ballCarrier == null)
        {
            LogSpyDecision("chase - no carrier resolved");
            state = State.ChaseBallCarrier;
            return;
        }
        // Has the carrier got past this goblin, toward the endzone it is defending? Then the spy is beaten and chases.
        float carrierLeadTowardMyEndzone = (ballCarrier.transform.position.x - this.transform.position.x) * DirectionOfMyEndzone();
        if (carrierLeadTowardMyEndzone > 0f)
        {
            LogSpyDecision("chase - carrier is past me");
            state = State.ChaseBallCarrier;
            return;
        }
        // The carrier is near the endzone they score in: nothing is left to spy for, so everyone chases.
        float carrierDistanceToMyEndzone = fieldMaxX - (ballCarrier.transform.position.x * DirectionOfMyEndzone());
        if (carrierDistanceToMyEndzone <= redZoneDistance)
        {
            LogSpyDecision("chase - carrier in the red zone");
            state = State.ChaseBallCarrier;
            return;
        }
        // Nobody else is contesting the ball, so the spy takes it on itself.
        if (!IsTeammateNearBallCarrier(ballCarrier))
        {
            LogSpyDecision("chase - no teammate near the carrier");
            state = State.ChaseBallCarrier;
            return;
        }
        LogSpyDecision("spy");
        state = State.SpyBallCarrier;
    }
    void LogSpyDecision(string decision)
    {
        if (decision == lastSpyDecision)
            return;
        lastSpyDecision = decision;
        Debug.Log("AIGrenadierSpyOrChase: " + this.name + " (" + (this.isGoblinGrey ? "grey" : "green") + ") at x " + this.transform.position.x.ToString("F1") + ": " + decision);
    }
    // Is anyone else on this goblin's team contesting the ball?
    bool IsTeammateNearBallCarrier(GoblinScript ballCarrier)
    {
        Team myTeamRoster = TeamOf(this.isGoblinGrey);
        if (myTeamRoster == null)
            return false;
        foreach (int goblinNetId in myTeamRoster.goblinNetIds)
        {
            if (!ObjectLookup.TryGet(goblinNetId, out GoblinScript teammate))
                continue;
            if (teammate == null || teammate == this || teammate.isGoblinKnockedOut)
                continue;
            if (Vector3.Distance(teammate.transform.position, ballCarrier.transform.position) <= teammateContestingBallDistance)
                return true;
        }
        return false;
    }
    // Hold station goal-side of the carrier, tracking them up and down the field, rather than committing to a tackle.
    // AIGrenadierSpyOrChase has already decided a teammate is on the ball and that the carrier has not got past.
    void MoveTowardSpyPosition()
    {
        GoblinScript ballCarrier = gameFootball != null ? gameFootball.goblinWithBall : null;
        if (ballCarrier == null)
        {
            state = State.ChaseBallCarrier;
            return;
        }
        myGoblinAIPathFindingScript.goblinTarget = null;
        myGoblinAIPathFindingScript.isTargetingAGoblin = false;
        this.goblinTarget = null;

        Vector3 carrierPosition = ballCarrier.transform.position;
        Vector3 myPosition = this.transform.position;
        float distanceToCarrier = Vector3.Distance(myPosition, carrierPosition);

        // How the carrier is moving, from how far they travelled since the spy's last physics tick. A jump too big
        // to be running - stale history, or a reposition - counts as standing still for one tick.
        Vector3 carrierVelocity = (carrierPosition - lastSpiedCarrierPosition) / Time.fixedDeltaTime;
        if (carrierVelocity.magnitude > MaxSpeed * 3f)
            carrierVelocity = Vector3.zero;
        lastSpiedCarrierPosition = carrierPosition;

        if (distanceToCarrier <= spyTackleDistance)
        {
            // Close enough to make the play: do what the chaser does, slide and punch included.
            MoveTowrdBallCarrier();
            return;
        }

        Vector3 spyPosition;
        if (distanceToCarrier <= spyInterceptDistance)
        {
            // The carrier is bearing down on the spy: cut them off. Aim at where they will be by the time this
            // goblin can get there, from their current direction and speed, instead of where they are now.
            float timeToReach = distanceToCarrier / Mathf.Max(MaxSpeed, 0.1f);
            spyPosition = carrierPosition + (carrierVelocity * timeToReach);
        }
        else
        {
            // Hold station goal-side of the carrier, tracking them up and down the field.
            spyPosition = carrierPosition;
            spyPosition.x = carrierPosition.x + (DirectionOfMyEndzone() * spyStandoffDistance);
        }
        spyPosition.x = Mathf.Clamp(spyPosition.x, -fieldMaxX, fieldMaxX);
        spyPosition.y = Mathf.Clamp(spyPosition.y, -fieldMaxY, fieldMaxY);

        float distanceToSpyPosition = Vector3.Distance(myPosition, spyPosition);
        if (distanceToSpyPosition <= 0.5f)
        {
            // Already in position. AIMoveTowardDirection is what normally clears isRunning, so simply not moving
            // would leave the goblin playing its run animation on the spot.
            if (isRunning)
            {
                isRunning = false;
                CmdSetIsRunningOnServer(false, Vector2.zero);
            }
            return;
        }
        // Sprint like the chaser does (MoveTowrdBallCarrier), and match a sprinting carrier. The first run only
        // sprinted when 10 units off station, so a sprinting carrier dragged the station away faster than the spy
        // walked, overtook it, and "carrier is past me" turned every spy into a chaser within moments.
        if (distanceToSpyPosition > 3f || ballCarrier.isSprintingOnServer)
            WillAIGoblinSprint(true);
        AIMoveTowardDirection((spyPosition - myPosition).normalized, spyPosition);
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
                    if(footBallLandingSpot != null)
                        footballPosition = footBallLandingSpot.transform.position;
                    else
                        footballPosition = gameFootball.transform.position;
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
        if (IsClientInitialized)
        {
            this.FatigueIndicators(newValue);
        }
    }
    public void HandleIsSlidingRoutineRunning(bool oldValue, bool newValue)
    {
        if (IsClientInitialized)
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
        if (IsClientInitialized)
        {
            wasPunchedBandAid.SetActive(newValue);
        }
    }
    public void CheckForStubbedToeAchievement()
    {
        Debug.Log("CheckForStubbedToeAchievement: for goblin: " + this.name + " is grey? " + this.isGoblinGrey.ToString());
        if (IsOwner)
        {
            // Don't give the achievement for a goblin that the player isn't controlling
            if (!this.isCharacterSelected)
                return;
            // Never award this for a goblin an AI played.
            if (myGamePlayer.isAIPlayer)
                return;

            SteamAchievementManager.instance.StubbedToe();
        }
    }
    public void TouchdownScoredAchievement()
    {
        Debug.Log("TouchdownScoredAchievement");
        if (serverGamePlayer)
        {
            // Was a check against the name "AIPlayer", which only held while there was exactly one bot.
            if (serverGamePlayer.isAIPlayer)
                return;
            RpcTouchdownScoredAchievement(serverGamePlayer.Owner);
        }
    }
    [TargetRpc]
    void RpcTouchdownScoredAchievement(NetworkConnection target)
    {
        Debug.Log("RpcTouchdownScoredAchievement: goblin scored touchdown: " + this.name);
        if (IsOwner)
        {
            if (myGamePlayer.isAIPlayer)
                return;
            if (Time.time >= (touchdownScoredtime + 1f))
            {
                SteamAchievementManager.instance.TouchdownScored();
                touchdownScoredtime = Time.time;
            }
            
        }
    }
}

