using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialGoblinScript : MonoBehaviour
{
    public enum State
    {
        ChaseFootball,
        ChaseBallCarrier,
        TeamHasBall,
        AttackNearbyGoblin,
        RunTowardEndzone,
        BlockKick,
        RunTowardPlayerGoblin,
        PunchPlayerGoblinForTutorial,
        None,
    }
    public TutorialPlayer myGamePlayer;
    public TutorialFootball gameFootball;

    [SerializeField] GameObject PowerUpParticleSystemPrefab;

    [Header("Character Properties")]
    public Animator animator;
    public Rigidbody2D rb;
    public Collider2D playerCollider;
    public Vector2 previousInput;
    public SpriteRenderer myRenderer;
    [SerializeField] BoxCollider2D golbinBodyCollider;
    public string goblinType;
    [SerializeField] private StatusBarScript myStatusBars;
    [SerializeField] private GameObject touchdownHitbox;
    [SerializeField] private GameObject divingHitbox;
    [SerializeField] private SpriteRenderer myShadow;
    [SerializeField] private SpriteRenderer mySelectedCircle;
    [SerializeField] private ParticleSystem sprintParticleSystem;

    [Header("Goblin Base Stats")]
    public int MaxHealth;
    public float MaxStamina;
    public float StaminaDrain;
    public float StaminaRecovery;
    public float MaxSpeed;
    public int MaxDamage;
    public bool canCollide = true;
    public string soundType;

    [Header("Goblin Current Stats")]
    public float health;
    public float stamina;
    public float speed;
    public float damage;
    public float ballCarrySpeedModifier = 1.0f;
    public float blockingSpeedModifier = 1.0f;
    public float defenseModifier;
    public float speedModifierFromPowerUps = 1.0f;
    public float slowDownObstacleModifier = 1.0f;
    public float possessionSpeedBonus = 1.0f;

    [Header("Character Game State Stuff")]
    public bool isOwnedByTutorialPlayer = false;
    public bool isGoblinGrey = false;
    public bool doesCharacterHaveBall;
    public bool isThrowing = false;
    public bool isRunning = false;
    public bool isSprinting = false;
    public bool shiftHeldDown = false;
    public bool isSprintingOnServer = false;
    public bool canRecoveryStamina = true;
    public bool isFatigued = false;
    public bool isGoblinKnockedOut = false;

    [Header("Can the Goblin Pass stuff")]
    //public Football gameFootball;
    public bool canGoblinReceivePass = false;

    [Header("Character selection stuff")]
    public bool isCharacterSelected = false;
    public bool isQGoblin = false;
    public bool isEGoblin = false;
    public bool isQGoblinLocally3v3 = false;
    public bool isEGoblinLocally3v3 = false;
    public bool canGoblinMove = true;
    public bool isGoblinPunching = false;
    public bool isPunching = false;
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

    [Header("Kicking Info")]
    public bool isKicking = false;
    [SerializeField] GameObject KickPowerBarHolder;
    [SerializeField] GameObject KickPowerBarFillerImage;
    [SerializeField] GameObject KickoffAimArrow;
    public bool powerBarActive = false;
    public float GoblinMaxKickDistance = 40f;
    public float GoblinMinKickDistance = 10f;
    public float GoblinPowerBarSpeed = 1f;
    public float GoblinKickPower = 0f;
    public float currentPowerBarScale = 0f;
    public int powerBarDirection = 1;
    public float kickoffAngle = 0f;
    public float GoblinKickoffAngle = 0f;
    float kickoffAngleSpeed = 30f;
    public bool aimArrowButtonHeldDown = false;
    public bool aimArrowUp = false;

    [Header("Kick After Accuracy Bar Stuff")]
    [SerializeField] GameObject kickAfterAccuracyBar;
    [SerializeField] GameObject kickAfterGuageLine;
    [SerializeField] GameObject kickAfterMarkerLeft;
    [SerializeField] GameObject kickAfterMarkerRight;
    public float accuracyValueSubmitted;
    public float powerValueSubmitted;
    public bool isGoblinDoingKickAfterAttempt = false;
    bool isAccuracySubmittedYet = false;
    bool isPowerSubmittedYet = false;
    IEnumerator kickAfterMoveAccuracyGuageLineRoutine;
    float currentAccuracyGaugeXPosition = -1f;
    int currentAccuracyGaugeDirection = 1;
    public float AccuracyBarSpeed = 0f;

    [Header("Kick After Repositioning Stuff")]
    public bool isKickAfterGoblin = false;
    public bool isKickAfterPositionSet = false;
    public float angleOfKickAttempt = 0f;
    public float kickAfterAccuracyDifficulty = 0f;
    //[SyncVar] public float kickAfterAccuracyBar1 = 0f;
    //[SyncVar] public float kickAfterAccuracyBar2 = 0f;
    public bool repositioningButtonHeldDown = false;
    public bool repositioningToLeft = false;
    Vector2 greenGoalPost = new Vector2(50.3f, -1.5f);
    Vector2 greyGoalPost = new Vector2(-50.3f, -1.5f);
    public Vector2 kickAfterFinalPosition = Vector2.zero;
    public bool hasGoblinBeenRepositionedForKickAfter = false;

    [Header("Dive Info")]
    public bool isDiving = false;
    public bool DivingRoutineRunning = false;
    IEnumerator DivingRoutine;

    

    [Header("Recovery Enumerator Stuff?")]
    public bool isStaminaRecoveryRoutineRunning = false;
    public IEnumerator staminaRecoveryRoutine;
    public bool isHealthRecoveryRoutineRunning = false;
    public IEnumerator healthRecoveryRoutine;
    public bool canGoblinRegainHealth = false;
    public bool isRegainHealthRoutineRunning = false;
    public IEnumerator regainHealthRoutine;
    public bool isTrippedTimerRunning = false;
    public IEnumerator trippedTimerRoutine;

    [Header("SFX Stuff")]
    public bool onWaterSlowDown;
    public bool onBrushSlowDown;
    public bool onGlueSlowDown;
    public bool firstFootStep = false;
    [SerializeField] GoblinSoundManager mySoundManager;

    [Header("Sprite Effects?")]
    [SerializeField] private SpriteFlash spriteFlash;
    [SerializeField] private GameObject fatigueSweatDrop;
    [SerializeField] private GameObject wasPunchedBandAid;

    [Header("Slide Info")]
    public bool isSliding = false;
    public Vector2 slideDirection = Vector2.zero;
    public bool isSlidingRoutineRunning = false;
    IEnumerator isSlidingRoutine;
    public float slideSpeedModifer = 1.0f;

    [Header("Blocking Info")]
    public bool isBlocking = false;

    [Header("Is Goblin Throwing Stuff")]
    public bool throwingRoutine = false;
    IEnumerator throwing;

    [Header("wasPunchedSpeedModifier Info")]
    public bool isWasPunchedRoutineRunning = false;
    IEnumerator isWasPunched;
    public float wasPunchedSpeedModifier = 1.0f;

    [Header("PowerUp Effects")]
    public bool attackNormal;
    public bool defenseNormal;
    public bool speedNormal;
    public bool invinvibilityBlueShell;

    [Header("HurtBox/HitBox Stuff")]
    [SerializeField] private GameObject punchBoxCollider;
    [SerializeField] private GameObject hurtBoxCollider;
    [SerializeField] private GameObject slideBoxCollider;

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
    public TutorialGoblinScript goblinTarget;
    public Vector2 preDirection = Vector2.zero;
    public Vector2 postDirection = Vector2.zero;
    public float adjacentGoblinTime = 0f;
    public float diveRate = 2.5f;
    public float nextDiveTime = 0f;
    public TutorialGoblinAIPathFinding myGoblinAIPathFindingScript;

    [Header("Punch Player Goblin")]
    public bool punchPlayerGoblin = false;
    public int numberOfPunches = 0;

    [Header("Chase Kicked Ball")]
    public bool chaseKickedBall = false;

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
    // Start is called before the first frame update
    void Start()
    {
        if (this.speed == 0)
            this.speed = this.MaxSpeed;
        if (this.stamina == 0)
            this.stamina = this.MaxStamina;
        if (this.health == 0)
            UpdateGoblinHealth(this.health, this.MaxHealth);
        if (this.damage == 0)
            this.damage = this.MaxDamage;
    }

    // Update is called once per frame
    void Update()
    {
        GetCanGoblinReceivePass();
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
        if (repositioningButtonHeldDown && TutorialManager.instance.gamePhase == "kick-after-attempt" && !isKickAfterPositionSet)
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
        if (isGoblinDoingKickAfterAttempt && !isAccuracySubmittedYet && !isPowerSubmittedYet && TutorialManager.instance.gamePhase == "kick-after-attempt")
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
    private void FixedUpdate()
    {
        if (canGoblinRegainHealth && health < MaxHealth)
            CmdRegainHealth();
        Move();
        UpdateStaminaBar();
    }
    public void EnableMovementControls()
    {
        InputManager.Controls.Player.Move.performed += ctx => SetMovement(ctx.ReadValue<Vector2>());
        InputManager.Controls.Player.Move.canceled += ctx => ResetMovement();
    }

    private void SetMovement(Vector2 movement)
    {
        previousInput = movement;
        //Debug.Log("SetMovement: " + movement.ToString());
    }

    private void ResetMovement() => previousInput = Vector2.zero;
    public void EnableSprintControls()
    {
        InputManager.Controls.Player.Sprint.performed += _ => IsPlayerSprinting(true);
        InputManager.Controls.Player.Sprint.canceled += _ => IsPlayerSprinting(false);
    }
    public void IsPlayerSprinting(bool isPlayerSprinting)
    {
        /*if (isBlocking)
            isPlayerSprinting = false;*/

        //Debug.Log("IsPlayerSprinting: " + isPlayerSprinting.ToString() + " for goblin: " + this.name);
        if (this.isCharacterSelected)
        {
            isSprinting = isPlayerSprinting;
            //this.speed = this.MaxSpeed * 1.2f;
        }
        else
        {
            isSprinting = false;
            //this.speed = this.MaxSpeed * 1.0f;
        }
            
    }
    private void Move()
    {
        if (isFatigued)
            RecoverStamina();
        else if ((!isSprinting) && !isFatigued && stamina < MaxStamina)
            RecoverStamina();

        if (isCharacterSelected && isOwnedByTutorialPlayer && !isGoblinKnockedOut && !isSliding && !isDiving)
        {
            isRunning = false;
            if (previousInput.x != 0 || previousInput.y != 0)
                isRunning = true;

            CheckIfGoblinIsSprinting();

            if (isSprinting && !isFatigued)
            {
                DrainStamina();
            }

            Vector2 direction = Vector2.ClampMagnitude(previousInput, 1);
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

            if (previousInput.x > 0)
            {
                //Vector3 newScale = new Vector3(1f, 1f, 1f);
                //transform.localScale = newScale;
                myRenderer.flipX = false;
                FlipRenderer(false);
            }
            else if (previousInput.x < 0)
            {
                //Vector3 newScale = new Vector3(-1f, 1f, 1f);
                //transform.localScale = newScale;
                myRenderer.flipX = true;
                FlipRenderer(true);
            }
            animator.SetBool("isRunning", isRunning);
        }
        else if (isOwnedByTutorialPlayer && (isSliding || isDiving) && slideDirection != Vector2.zero)
        {
            Vector2 direction = Vector2.ClampMagnitude(slideDirection, 1);
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        }
        else if (!isOwnedByTutorialPlayer)
        {
            if (punchPlayerGoblin)
            {
                this.state = State.PunchPlayerGoblinForTutorial;
            }
            else if (chaseKickedBall)
            {
                this.state = State.ChaseFootball;
            }
            else
                this.state = State.None;
            switch (state)
            {
                default:
                case State.None:
                    isRunning = false;
                    animator.SetBool("isRunning", isRunning);
                    break;
                case State.ChaseFootball:
                    MoveTowardFootball();
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
                case State.PunchPlayerGoblinForTutorial:
                    IsNearPlayerGoblin();
                    break;
            }
        }
    }
    void CheckIfGoblinIsSprinting()
    {
        //Debug.Log("CheckIfGoblinIsSprinting: isSprining: " + isSprinting.ToString());
        if (isSprinting)
        {
            if (!this.isFatigued)
            {
                if (this.stamina > 0f)
                {
                    this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer * blockingSpeedModifier * wasPunchedSpeedModifier * speedModifierFromPowerUps * slowDownObstacleModifier * possessionSpeedBonus) * 1.2f;
                }
                if (isStaminaRecoveryRoutineRunning)
                    StopCoroutine(staminaRecoveryRoutine);

                staminaRecoveryRoutine = CanGoblinRecoverStamina();
                StartCoroutine(staminaRecoveryRoutine);
            }
            else
            {
                this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer * blockingSpeedModifier * wasPunchedSpeedModifier * speedModifierFromPowerUps * slowDownObstacleModifier * possessionSpeedBonus) * 0.5f;
            }
        }
        else
        {
            if (!isFatigued)
                this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer * blockingSpeedModifier * wasPunchedSpeedModifier * speedModifierFromPowerUps * slowDownObstacleModifier * possessionSpeedBonus);
            else
                this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer * blockingSpeedModifier * wasPunchedSpeedModifier * speedModifierFromPowerUps * slowDownObstacleModifier * possessionSpeedBonus) * 0.5f;
        }
    }
    public IEnumerator CanGoblinRecoverStamina()
    {
        isStaminaRecoveryRoutineRunning = true;
        canRecoveryStamina = false;
        yield return new WaitForSeconds(1.00f);
        canRecoveryStamina = true;
        isStaminaRecoveryRoutineRunning = false;
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
        if (this.isCharacterSelected)
            volume = 0.5f;
        else
            volume = 0.25f;

        string footstepType = GetFootstepSFXType(firstFootStep);

        SoundManager.instance.PlaySound(footstepType, volume);

        try
        {
            if (this.isSprinting && !this.isFatigued)
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
        else if (onWaterSlowDown)
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
    void RecoverStamina()
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
                    //isFatigued = false;
                    this.HandleIsFatigued(this.isFatigued, false);
                }
            }
        }
    }
    void DrainStamina()
    {
        if (isRunning)
        {
            if (stamina > 0f)
            {
                stamina -= (Time.fixedDeltaTime * (StaminaDrain));
            }
            else
            {
                stamina = 0f;
                //isFatigued = true;
                //isFatigued = true;
                this.HandleIsFatigued(this.isFatigued, true);
                CheckTutorialForFatigueEvent();
                this.speed = (MaxSpeed) * 0.5f;
            }
        }
    }
    void CheckTutorialForFatigueEvent()
    {
        if (TutorialManager.instance.index5Tracker)
            return;
        TutorialManager.instance.PlayerSprintedToFatigue();
    }
    void UpdateStaminaBar()
    {
        float newStamina = stamina;
        if (newStamina < 0f)
            newStamina = 0f;
        myStatusBars.StaminaBarUpdate(newStamina / MaxStamina);
    }
    public void HandleIsFatigued(bool oldValue, bool newValue)
    {
        isFatigued = newValue;
        myStatusBars.ChangeStaminaBarColor(newValue);
        /*fatigueSweatDrop.SetActive(newValue);
        if(newValue)
            spriteFlash.Flash(Color.yellow);*/
        FatigueIndicators(newValue);

        if (newValue)
        {

        }
        else
        {
            IsPlayerSprinting(false);
        }

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
        //else if (this.isFatigued || this.isSlidingRoutineRunning || this.DivingRoutineRunning)
        else if (this.isFatigued)
        {
            fatigueSweatDrop.SetActive(true);
        }
        else
        {
            fatigueSweatDrop.SetActive(false);
        }

    }
    public void FlipRenderer(bool flip)
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
        if (mySelectedCircle.flipX != flip)
            mySelectedCircle.flipX = flip;

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
    public void HandleCharacterSelected(bool oldValue, bool newValue)
    {
        isCharacterSelected = newValue;

        if (newValue)
        {
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
        if (!this.doesCharacterHaveBall && youMarkerObject != null)
            youMarkerObject.SetActive(newValue);

    }
    public void SpawnQEandYouMarkers(bool playerTeam)
    {
        if (playerTeam)
        {
            ballMarkerObject = Instantiate(ballMarkerPrefab);
            youMarkerObject = Instantiate(youMarkerPrefab);
            mySelectedCircle.color = Color.white;
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

        if (playerTeam)
        {
            youMarkerObject.transform.SetParent(this.transform);
            youMarkerObject.transform.localPosition = markerPosition;
            youMarkerObject.SetActive(false);

            myGamePlayer = GameObject.FindGameObjectWithTag("TutorialPlayer").GetComponent<TutorialPlayer>();
        }
    }
    public void SetEGoblin(bool isE)
    {
        Debug.Log("SetEGoblin " + isE.ToString() + " " + this.name);

        this.HandleIsEGoblin(this.isEGoblin, isE);
        /*if (isBlocking)
            CmdSetBlocking(false);*/

    }
    public void HandleIsEGoblin(bool oldValue, bool newValue)
    {
        isEGoblin = newValue;

        if (!isOwnedByTutorialPlayer)
        {
            eMarker.SetActive(false);
            return;
        }
        eMarker.SetActive(newValue);
    }
    public void SetQGoblin(bool isQ)
    {
        Debug.Log("SetQGoblin " + isQ.ToString() + " " + this.name);

        this.HandleIsQGoblin(this.isQGoblin, isQ);
        /*if (isBlocking)
            CmdSetBlocking(false);*/

    }
    public void HandleIsQGoblin(bool oldValue, bool newValue)
    {
        isQGoblin = newValue;

        if (!isOwnedByTutorialPlayer)
        {
            qMarker.SetActive(false);
            return;
        }
        qMarker.SetActive(newValue);
    }
    public void UnSelectThisCharacter()
    {
        Debug.Log("UnSelectThisCharacter " + this.name);
        if (isOwnedByTutorialPlayer)
            this.HandleCharacterSelected(this.isCharacterSelected, false);
    }
    public void SelectThisCharacter()
    {
        Debug.Log("SelectThisCharacter " + this.name);
        if (isOwnedByTutorialPlayer)
            this.HandleCharacterSelected(this.isCharacterSelected, true);
    }
    public IEnumerator CantMove()
    {
        Debug.Log("Starting CantMove for " + this.name);
        canGoblinMove = false;
        yield return new WaitForSeconds(0.25f);
        canGoblinMove = true;
    }
    public void ThrowBall(TutorialGoblinScript goblinToThrowTo)
    {
        Debug.Log("ThrowBall: Throwing the ball to: " + goblinToThrowTo.gameObject.name);
        TutorialFootball footballScript = GameObject.FindGameObjectWithTag("football").GetComponent<TutorialFootball>();
        if (!isKicking && !isDiving && !this.isGoblinKnockedOut && this.doesCharacterHaveBall)
        {
            if (isOwnedByTutorialPlayer)
            {
                golbinBodyCollider.enabled = false;
                StartThrowing();
            }

            footballScript.ThrowFootball(this, goblinToThrowTo);
        }

    }
    public void GoblinPickUpFootball()
    {
        if (!isThrowing)
        {
            GameObject.FindGameObjectWithTag("football").GetComponent<TutorialFootball>().PlayerPickUpFootball(this);
        }
        TutorialManager.instance.PlayerPickedUpBall();
    }
    public void HandleHasBall(bool oldValue, bool newValue)
    {
        doesCharacterHaveBall = newValue;

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(goblinType + "-punch"))
        {
            animator.enabled = false;
            animator.enabled = true;
        }
        animator.SetBool("withFootball", newValue);
        CheckIfTeamStillHasBall();
        if (isOwnedByTutorialPlayer)
        {
            if (newValue && youMarkerObject != null)
                youMarkerObject.SetActive(false);
            else if (!newValue && this.isCharacterSelected && youMarkerObject != null)
                youMarkerObject.SetActive(true);

        }
        else
        {
            if (newValue)
                TutorialManager.instance.OpponentPickedUpBall(this);
            else
                TutorialManager.instance.PlayerCausedFumble();
        }
        if (newValue)
        {
            GameObject football = GameObject.FindGameObjectWithTag("football");
            football.transform.SetParent(this.transform);
            football.transform.localPosition = new Vector3(0f, 0f, 0f);
            if (!this.isCharacterSelected)
            {
                Debug.Log("HandleHasBall: Player that is not selected has the ball: " + this.name);
                if (isOwnedByTutorialPlayer)
                {
                    if (this.isEGoblin)
                    {
                        myGamePlayer.SwitchToEGoblin();
                    }
                    else if (this.isQGoblin)
                    {
                        myGamePlayer.SwitchToQGoblin();
                    }
                }
            }
            if(isOwnedByTutorialPlayer)
                TutorialManager.instance.PlayerPickedUpBallAfterSlideTackle();
        }
        else
        {
            GameObject football = GameObject.FindGameObjectWithTag("football");
            football.transform.parent = null;
            if (isOwnedByTutorialPlayer && powerBarActive)
                ResetPowerBar();
        }

        ballMarkerObject.SetActive(newValue);
        mySelectedCircle.gameObject.SetActive(newValue);

    }
    void GetCanGoblinReceivePass()
    {
        if (myGamePlayer.doesTeamHaveBall && !doesCharacterHaveBall)
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
    }
    public void HandleCanGoblinReceivePass(bool oldValue, bool newValue)
    {
        canGoblinReceivePass = newValue;

        if (isOwnedByTutorialPlayer)
        {
            qMarker.GetComponent<TutorialQEMarkerScript>().UpdateSpriteForPassing(newValue);
            eMarker.GetComponent<TutorialQEMarkerScript>().UpdateSpriteForPassing(newValue);
        }
    }
    public void GetSpawnedFootball()
    {
        if (gameFootball == null)
        {
            gameFootball = GameObject.FindGameObjectWithTag("football").GetComponent<TutorialFootball>();
        }
    }
    void StartThrowing()
    {
        this.HandleIsThrowing(isThrowing, true);
    }
    public void HandleIsThrowing(bool oldValue, bool newValue)
    {
        isThrowing = newValue;

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
    public IEnumerator DisableColliderForThrow()
    {
        golbinBodyCollider.enabled = false;
        throwingRoutine = true;
        yield return new WaitForSeconds(0.2f);
        golbinBodyCollider.enabled = true;
        throwingRoutine = false;
        if (isOwnedByTutorialPlayer)
            StopThrowing();
    }
    void StopThrowing()
    {
        this.HandleIsThrowing(isThrowing, false);
    }
    void CheckIfTeamStillHasBall()
    {
        if (!isOwnedByTutorialPlayer && this.doesCharacterHaveBall)
        {
            GameObject.FindGameObjectWithTag("TutorialPlayer").GetComponent<TutorialPlayer>().doesTeamHaveBall = false;
            return;
        }
        else if (isOwnedByTutorialPlayer)
        {
            if (this.doesCharacterHaveBall)
            {
                Debug.Log("CmdCheckIfTeamStillHasBall: Goblin has ball. Goblin's team still has ball. Setting doesTeamHaveBall to true for player: ");
                myGamePlayer.doesTeamHaveBall = true;
            }
            else
            {
                bool anyGoblinHaveBall = false;
                foreach (TutorialGoblinScript goblin in TutorialManager.instance.greenGoblins)
                {
                    if (goblin.doesCharacterHaveBall)
                    {
                        anyGoblinHaveBall = true;
                        break;
                    }
                }
                Debug.Log("CmdCheckIfTeamStillHasBall: Do any goblins on " + myGamePlayer.name + " team have the ball? " + anyGoblinHaveBall.ToString());
                myGamePlayer.doesTeamHaveBall = anyGoblinHaveBall;
            }
        }
    }
    public void EnableGoblinMovement(bool allowMovement)
    {
        Debug.Log("EnableGoblinMovement for " + this.name + " allowMovement: " + allowMovement.ToString());
        if (allowMovement)
        {
            InputManager.Controls.Player.Move.Enable();
            InputManager.Controls.Player.Sprint.Enable();
        }
        else
        {
            InputManager.Controls.Player.Move.Disable();
            InputManager.Controls.Player.Sprint.Disable();
        }
    }
    public void HandleIsGoblinGrey(bool oldValue, bool newValue)
    {
        isGoblinGrey = newValue;
        if (newValue)
            animator.SetBool("isGrey", newValue);
        if (newValue && isOwnedByTutorialPlayer)
        {
            FlipKickoffAimArrow();
        }
        
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
    public void AIMoveTowardDirection(Vector3 directionToMoveTo, Vector3 targetPosition)
    {
        if (this.isGoblinKnockedOut)
            return;
        //Vector2 direction = Vector2.ClampMagnitude(directionToMoveTo, 1);
        Vector2 direction = this.GetComponent<TutorialGoblinAIPathFinding>().AIMoveTowardDirection(Vector2.ClampMagnitude(directionToMoveTo, 1), targetPosition);

        // Set whether the goblin is moving. If the "direction" to the football is 0, then they shouldn't be moving?
        isRunning = false;
        if (direction.x != 0 || direction.y != 0)
            isRunning = true;
        // Set the goblin's "Running On Server" value so that can be tracked by the server?
        //SetIsRunningOnServer(isRunning);

        //Set the "isSprinting" on the server and make sure that all the speed modifiers get set correctly?
        IsPlayerSprinting(isSprinting);

        // If the goblin is sprinting, make sure to drain their stamina
        if (isSprinting && !isFatigued)
        {
            DrainStamina();
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
        /*if (teammateCollision)
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
        }*/

        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

        // check the direction the goblin is moving. If they are moving left, make sprite face left. If right, sprite face right
        if (direction.x > 0)
        {
            myRenderer.flipX = false;
            FlipRenderer(false);
        }
        else if (direction.x < 0)
        {
            myRenderer.flipX = true;
            FlipRenderer(true);
        }
        animator.SetBool("isRunning", isRunning);
    }
    public void PunchPlayerGoblin(TutorialGoblinScript playerGoblin, int numberOfPunchesToThrow)
    {
        goblinTarget = playerGoblin;
        numberOfPunches = numberOfPunchesToThrow;
        punchPlayerGoblin = true;
    }
    void IsNearPlayerGoblin()
    {
        if (!punchPlayerGoblin)
            return;
        Vector3 targetPosition = goblinTarget.transform.position;
        Vector3 myPosition = this.transform.position;
        // Check distance to player goblin. If not close enough, keep moving toward goblin. Else, punch the goblin
        Vector3 diretionToBallCarrier = (targetPosition - myPosition).normalized;
        float distanceToTarget = Vector2.Distance(myPosition, targetPosition);
        if (distanceToTarget > minDistanceFromTarget)
        {
            myGoblinAIPathFindingScript.goblinTarget = goblinTarget;
            myGoblinAIPathFindingScript.isTargetingAGoblin = true;
            AIMoveTowardDirection(diretionToBallCarrier, targetPosition);
        }
        else
        {
            isRunning = false;
            animator.SetBool("isRunning", isRunning);
            if (WillGoblinPunchTarget(distanceToTarget, myPosition.y, targetPosition.y) && numberOfPunches > 0)
            {
                StartPunchingThePlayerGoblin(targetPosition, myPosition);
                numberOfPunches--;
            }
            else if (numberOfPunches <= 0)
            {
                TutorialManager.instance.DonePunchingPlayerGoblin();
                this.punchPlayerGoblin = false;
            }
        }
    }
    bool WillGoblinPunchTarget(float distanceToTarget, float myYPosition, float targetYPosition)
    {
        bool willPunch = false;

        if (distanceToTarget < this.punchRange && Time.time > this.nextPunchTime && Mathf.Abs(myYPosition - targetYPosition) <= punchYMax)
            willPunch = true;

        return willPunch;
    }
    void StartPunchingThePlayerGoblin(Vector3 target, Vector3 myPosition)
    {
        if (target.x < myPosition.x && !this.myRenderer.flipX)
            FlipRenderer(true);
        else if (target.x > myPosition.x && this.myRenderer.flipX)
            FlipRenderer(false);

        // Do the punch
        this.Attack();
        nextPunchTime = Time.time + punchRate;
    }
    public void Attack()
    {
        Debug.Log("Attack: from goblin " + this.name);
        if (!doesCharacterHaveBall && !isGoblinKnockedOut && !isSliding && !isDiving)
        {
            if (isRunning)
            {
                animator.Play(goblinType + "-punch-running");
            }
            else
            {
                animator.Play(goblinType + "-punch");
            }

        }
    }
    public void StartBlocking()
    {
        if (isOwnedByTutorialPlayer)
        {
            SetBlocking(true);
            // stop player from sprinting when blocking
            if (isSprinting)
                IsPlayerSprinting(false);
            if (isKicking)
                StopKicking();
        }

    }
    public void StopBlocking()
    {
        if (isOwnedByTutorialPlayer)
        {
            SetBlocking(false);
            if (Input.GetKey(KeyCode.LeftShift))
                IsPlayerSprinting(true);
        }

    }
    void SetBlocking(bool isGoblinBlocking)
    {
        Debug.Log("SetBlocking: Set blocking to " + isGoblinBlocking.ToString() + " for goblin " + this.name);
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
        isBlocking = newValue;

        if (isOwnedByTutorialPlayer)
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
    void StopKicking()
    {
        HandleIsKicking(this.isKicking, false);
    }
    public void HandleIsKicking(bool oldValue, bool newValue)
    {
        isKicking = newValue;

        Debug.Log("HandleIsKicking: isClient");
        if (isOwnedByTutorialPlayer)
        {
            Debug.Log("HandleIsKicking: hasAuthority");
            if (newValue)
            {
                Debug.Log("HandleIsKicking: start the kick animation");
                animator.Play(goblinType + "-kick-football");
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
    }
    void ResetPowerBar()
    {
        KickPowerBarHolder.SetActive(false);
        Vector3 resetScale = new Vector3(0f, 1f, 1f);
        KickPowerBarFillerImage.transform.localScale = resetScale;
        powerBarActive = false;
        currentPowerBarScale = 0f;
        powerBarDirection = 1;
        //if(hasAuthority)
        //CmdResetKickPower();
    }
    public void KickFootballGoblin(float kickPower, float kickAngle)
    {
        if (isOwnedByTutorialPlayer && !isPunching && !isDiving && !isSliding && !isGoblinKnockedOut && doesCharacterHaveBall && !isThrowing)
        {
            KickFootball(kickPower, kickAngle);
        }

    }
    void KickFootball(float kickPower, float kickAngle)
    {
        if (!isPunching && !isDiving && !isSliding && !isGoblinKnockedOut && doesCharacterHaveBall && !isThrowing && kickPower <= 1f && kickPower >= 0f)
        {
            if (TutorialManager.instance.gamePhase == "kick-after-attempt")
            {
                powerValueSubmitted = kickPower;
                //GameplayManager.instance.StopTimeoutKickAfterRoutine();
            }
            HandleIsKicking(this.isKicking, true);
            GoblinKickPower = kickPower;
            GoblinKickoffAngle = kickAngle;
        }
    }
    public void KickTheFootball()
    {
        Debug.Log("KickTheFootball");
        if (isOwnedByTutorialPlayer && !isPunching && !isDiving && !isSliding && !isGoblinKnockedOut && doesCharacterHaveBall && !isThrowing)
        {
            TellFootballToBeKicked();
        }
        KickedFootballSFX();
    }
    void TellFootballToBeKicked()
    {
        HandleHasBall(doesCharacterHaveBall, false);
        TutorialFootball footballScript = GameObject.FindGameObjectWithTag("football").GetComponent<TutorialFootball>();
        Vector3 newLocalPosition = footballScript.transform.localPosition;
        if (myRenderer.flipX)
        {
            newLocalPosition.x -= 1.0f;
        }
        else
            newLocalPosition.x += 1.0f;
        footballScript.transform.localPosition = newLocalPosition;

        if (TutorialManager.instance.gamePhase == "kick-after-attempt")
        {
            footballScript.KickAfterAttemptKick(isGoblinGrey, powerValueSubmitted, angleOfKickAttempt, GoblinMaxKickDistance, GoblinMinKickDistance, kickAfterAccuracyDifficulty, accuracyValueSubmitted, kickAfterFinalPosition);
            return;
        }

        footballScript.KickFootballDownField(isGoblinGrey, GoblinKickPower, GoblinKickoffAngle, GoblinMaxKickDistance, GoblinMinKickDistance);
    }
    void KickedFootballSFX()
    {
        if (this.isGoblinOnScreen())
            SoundManager.instance.PlaySound("kicked-ball", 1.0f);
    }
    
    public void HurtBoxCollision(TutorialGoblinScript punchingGoblin)
    {


        if (this.isGoblinGrey != punchingGoblin.isGoblinGrey)
            DealDamageToGoblins(this, punchingGoblin);
       
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
                if (this.isOwnedByTutorialPlayer && this.isCharacterSelected)
                    this.mySoundManager.PlaySound(this.soundType + "-hit", 1.0f);
                else
                    this.mySoundManager.PlaySound(this.soundType + "-hit", 0.8f);
            }
        }
    }
    void DealDamageToGoblins(TutorialGoblinScript goblinReceivingDamage, TutorialGoblinScript goblinGivingDamage)
    {
        Debug.Log("DealDamageToGoblins: " + goblinGivingDamage.name + " is taking damage from " + goblinGivingDamage.name);
        if (!goblinReceivingDamage.isGoblinKnockedOut)
        {
            // If A Goblin is hit while diving, immediately knock them out.
            if (goblinReceivingDamage.isDiving)
            {
                //goblinReceivingDamage.health = 0f;
                goblinReceivingDamage.UpdateGoblinHealth(goblinReceivingDamage.health, 0f);
                goblinReceivingDamage.KnockOutGoblin(true);

            }
            else
            {
                float blockingModifier;
                if (goblinReceivingDamage.isBlocking)
                    blockingModifier = 0.5f;
                else
                    blockingModifier = 1.0f;


                float damageDealt = goblinGivingDamage.damage * blockingModifier;
                Debug.Log("DealDamageToGoblins: Goblin " + goblinReceivingDamage.name + " will receive the following amount of damage: " + damageDealt.ToString());
                //goblinReceivingDamage.health -= (goblinGivingDamage.damage * (blockingModifier));
                goblinReceivingDamage.UpdateGoblinHealth(goblinReceivingDamage.health, (goblinReceivingDamage.health - damageDealt));
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
                    if (!this.isBlocking)
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
            //TeamManager.instance.PunchHit(goblinGivingDamage);
            goblinReceivingDamage.PunchedFlashSprite();
        }
    }
    public void KnockOutGoblin(bool knockedOut)
    {
        Debug.Log("KnockOutGoblin: " + knockedOut.ToString());
        if (this.health < 0f)
            UpdateGoblinHealth(this.health, 0f);


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
            TutorialFootball footballScript = GameObject.FindGameObjectWithTag("football").GetComponent<TutorialFootball>();
            if(!this.isOwnedByTutorialPlayer && !footballScript.opponentFumbledYet)
                footballScript.FumbleFootball(true);
            else
                footballScript.FumbleFootball(false);

            if (!isOwnedByTutorialPlayer)
                TutorialManager.instance.OpponentFumbledFromSlide();

        }
        //Code here for ending kick-after-attempt?
        if (TutorialManager.instance.gamePhase == "kick-after-attempt" && this.isKickAfterGoblin && !gameFootball.isKicked)
        {
            //GameplayManager.instance.KickAfterAttemptWasBlocked();
            //RpcKickBlockedStopKickAfterAttempt(this.connectionToClient);
        }
    }
    public void HandleIsGoblinKnockedOut(bool oldValue, bool newValue)
    {
        isGoblinKnockedOut = newValue;

        golbinBodyCollider.enabled = !newValue;
        myStatusBars.gameObject.SetActive(!newValue);

        if (isBlocking)
            SetBlocking(false);

        // code for knocked out animation?
        if (newValue)
        {
            animator.Play(goblinType + "-knocked-out");

            //Make sure that all the isPunching, isDiving things are false so it doesn't fuck with controls
            if (isPunching)
                Punching(false);
            if (isDiving)
                StopDiving();
            if (isThrowing)
                StopThrowing();
            if (isSliding)
            {
                StopSliding();
            }
            if (isKicking)
                StopKicking();
        }

        animator.SetBool("isKnockedOut", newValue);

        
        if (newValue && isGoblinOnScreen())
        {
            if (isOwnedByTutorialPlayer && this.isCharacterSelected)
                mySoundManager.PlaySound("goblin-knocked-out", 0.75f);
            else
                mySoundManager.PlaySound("goblin-knocked-out", 0.4f);
        }
        else
        {
            mySoundManager.StopSound("goblin-knocked-out");
        }
    }
    void Punching(bool punching)
    {
        //isPunching = punching;
        HandleIsPunching(isPunching, punching);
        /*if (this.isCharacterSelected && punching)
            TeamManager.instance.ThrownPunch(this.serverGamePlayer);*/
        //TeamManager.instance.ThrownPunch(this);
    }
    public void HandleIsPunching(bool oldValue, bool newValue)
    {
        isPunching = newValue;

        if (isOwnedByTutorialPlayer)
        {
            if (newValue && isBlocking)
                SetBlocking(false);
            if (!newValue)
            {
                ToggleGoblinBodyCollider();
            }
        }
    }
    public void ToggleGoblinBodyCollider()
    {
        golbinBodyCollider.gameObject.SetActive(false);
        golbinBodyCollider.gameObject.SetActive(true);
    }
    void StopDiving()
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
    public void HandleIsDiving(bool oldValue, bool newValue)
    {
        isDiving = newValue;


        if (isOwnedByTutorialPlayer && newValue)
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
        if (isOwnedByTutorialPlayer && !newValue)
        {
            if (!golbinBodyCollider.gameObject.activeInHierarchy)
                golbinBodyCollider.gameObject.SetActive(true);
        }
    }
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
    public void HandleIsDivingRoutineRunning(bool oldValue, bool newValue)
    {
        this.DivingRoutineRunning = newValue;
        this.FatigueIndicators(newValue);
    }
    void StopSliding()
    {
        //isSliding = false;
        HandleIsSliding(this.isSliding, false);
        slideSpeedModifer = 1.0f;
        slideDirection = Vector2.zero;
        if (isSlidingRoutineRunning)
            StopCoroutine(isSlidingRoutine);
        this.HandleIsSlidingRoutineRunning(this.isSlidingRoutineRunning, false);
    }
    public void HandleIsSlidingRoutineRunning(bool oldValue, bool newValue)
    {
        this.isSlidingRoutineRunning = newValue;
        this.FatigueIndicators(newValue);
    }
    public IEnumerator RegainHealth()
    {
        isRegainHealthRoutineRunning = true;
        canGoblinRegainHealth = false;
        yield return new WaitForSeconds(2.0f);
        isRegainHealthRoutineRunning = false;
        canGoblinRegainHealth = true;
    }
    public void HandleWasPunchedRoutineRunning(bool oldValue, bool newValue)
    {
        isWasPunchedRoutineRunning = newValue;
        wasPunchedBandAid.SetActive(newValue);
    }
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
    public void PunchedFlashSprite()
    {
        Debug.Log("RpcPunchedFlashSprite: for goblin " + this.name);
        spriteFlash.Flash(Color.white);
    }
    public IEnumerator KnockedOutTimer()
    {
        isHealthRecoveryRoutineRunning = true;
        yield return new WaitForSeconds(3.5f);
        isHealthRecoveryRoutineRunning = false;
        //this.health = (MaxHealth * 0.66f);
        UpdateGoblinHealth(this.health, (MaxHealth * 0.66f));
        //isGoblinKnockedOut = false;
        HandleIsGoblinKnockedOut(isGoblinKnockedOut, false);

        if (isRegainHealthRoutineRunning)
            StopCoroutine(regainHealthRoutine);

        regainHealthRoutine = RegainHealth();
        StartCoroutine(regainHealthRoutine);
    }
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
    public void UpdateGoblinHealth(float oldValue, float newValue)
    {
        health = newValue;

        float newHealth = newValue;
        if (newHealth < 0f)
            newHealth = 0f;

        myStatusBars.HealthBarUpdate(newHealth / MaxHealth);
    }
    public void StartPunching()
    {
        Punching(true);
    }
    public void StopPunching()
    {
        Punching(false);
    }
    void CmdRegainHealth()
    {
        if (canGoblinRegainHealth && health < MaxHealth)
        {
            //health += (2.0f ) * Time.fixedDeltaTime;
            float newHealth = this.health + ((2.0f) * Time.fixedDeltaTime);
            if (newHealth >= MaxHealth)
            {
                newHealth = MaxHealth;
                canGoblinRegainHealth = false;
            }
            this.UpdateGoblinHealth(this.health, newHealth);
        }
    }
    public void StartChasingBall()
    {
        chaseKickedBall = true;
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
            //this.WillAIGoblinSprint(true);
        }
        if (distToFootball > 0.2f)
        {
            AIMoveTowardDirection(directionToFootball, positionOfFootball);
        }
    }
    public void SlideGoblin()
    {
        if (isOwnedByTutorialPlayer)
        {
            if (isBlocking)
                SetBlocking(false);
            CmdSlideGoblin(previousInput);
        }
    }
    void CmdSlideGoblin(Vector2 directionToSlide)
    {
        if (isRunning && directionToSlide != Vector2.zero && !doesCharacterHaveBall && !isSliding && !isFatigued && !isPunching && !isGoblinKnockedOut)
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
    public IEnumerator SlideGoblinRoutine()
    {
        this.HandleIsSlidingRoutineRunning(this.isSlidingRoutineRunning, true);
        //isSliding = true;
        HandleIsSliding(this.isSliding, true);
        yield return new WaitForSeconds(0.25f);
        speed = MaxSpeed * 1.2f;
        Debug.Log("SlideGoblinRoutine: Set speed to 1.2x for goblin " + this.name);
        yield return new WaitForSeconds(0.25f);
        //isSliding = false;
        HandleIsSliding(this.isSliding, false);
        slideSpeedModifer = 0.7f;
        yield return new WaitForSeconds(2.25f);
        slideSpeedModifer = 1.0f;
        yield return new WaitForSeconds(0.75f);
        slideDirection = Vector2.zero;
        this.HandleIsSlidingRoutineRunning(this.isSlidingRoutineRunning, false);
    }
    public void HandleIsSliding(bool oldValue, bool newValue)
    {
        isSliding = newValue;

        animator.SetBool("isSliding", newValue);
        if (!newValue)
        {
            if (!golbinBodyCollider.gameObject.activeInHierarchy)
                golbinBodyCollider.gameObject.SetActive(true);
        }
        if (this.isGoblinOnScreen() && newValue)
            SoundManager.instance.PlaySound("slide", 0.75f);
        else
            SoundManager.instance.StopSound("slide");
    }
    public void SlideBoxCollision(TutorialGoblinScript slidingGoblin)
    {
        /*if (this.ownerConnectionId != slidingGoblin.ownerConnectionId)
            slidingGoblin.TripGoblin();
        if (this.isCharacterSelected)
            TeamManager.instance.SlideTackle(this.serverGamePlayer, true);*/
        if (this.isGoblinGrey != slidingGoblin.isGoblinGrey)
        {
            slidingGoblin.TripGoblin();
            /*if (this.isCharacterSelected)
                TeamManager.instance.SlideTackle(this, true);*/
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
                //if (!this.defenseNormal)
                    this.KnockOutGoblin(false);
            }
        }
    }
    public void StartDefenseNormal()
    {
        IEnumerator defenseNormalRoutine = DefenseNormalRoutine();
        StartCoroutine(defenseNormalRoutine);
    }
    IEnumerator DefenseNormalRoutine()
    {
        defenseModifier = 0.35f;
        defenseNormal = true;
        yield return new WaitForSeconds(3.0f);
        defenseModifier = 1.0f;
        defenseNormal = false;
    }
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
    public void SlowDownObstacleEffect(bool stillColliding)
    {
        if (stillColliding)
            slowDownObstacleModifier = 0.5f;
        else
            slowDownObstacleModifier = 1.0f;
    }
    public void DiveGoblin()
    {
        if (isOwnedByTutorialPlayer)
        {
            if (isRunning)
            {
                if (!isGoblinKnockedOut && !isSliding && !isDiving && isOwnedByTutorialPlayer && !isPunching)
                {
                    StartDiving(previousInput);
                    if (isBlocking)
                        SetBlocking(false);
                }
            }
        }
    }
    void StartDiving(Vector2 directionToDive)
    {
        Debug.Log("CmdStartDiving: isRunningOnServer: " + this.isRunning.ToString() + " Direction to move: " + directionToDive.ToString());
        if (isRunning && directionToDive != Vector2.zero && !isSliding && !isFatigued && !isDiving && !DivingRoutineRunning && !isSlidingRoutineRunning && !isPunching && !this.isKicking)
        {
            Debug.Log("CmdStartDiving: Goblin will slide in direction of: " + directionToDive.ToString());
            slideDirection = directionToDive;
            HandleIsDiving(isDiving, true);
            speed = MaxSpeed * 1.2f;
            Debug.Log("CmdStartDiving: Set speed to 1.2x for goblin " + this.name);
        }

    }
    public void CheckIfGoblinNeedsToFlipForKickAfter(bool isPlayerKicking)
    {
        if (this.isGoblinGrey && !myRenderer.flipX)
        {
            this.FlipRenderer(true);
        }
        else if (!this.isGoblinGrey && myRenderer.flipX)
        {
            this.FlipRenderer(false);
        }
    }
    public void UpdateHasGoblinRepositionedForKickAfter()
    {
        if (isOwnedByTutorialPlayer)
            CmdUpdateHasGoblinRepositionedForKickAfter();
    }
    void CmdUpdateHasGoblinRepositionedForKickAfter()
    {
        this.hasGoblinBeenRepositionedForKickAfter = true;
    }
    public void ActivateKickAfterAccuracyBar(bool activate)
    {

        kickAfterAccuracyBar.SetActive(activate);
        kickAfterGuageLine.SetActive(false);

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
    public void KickAfterRepositioning(bool moveLeft)
    {
        repositioningButtonHeldDown = true;
        repositioningToLeft = moveLeft;
    }
    public void EndKickAfterRepositioning()
    {
        repositioningButtonHeldDown = false;
    }
    public void SubmitKickAfterPositionToServer()
    {
        if (isOwnedByTutorialPlayer)
            CmdSubmitKickAfterPositionToServer(transform.position);

    }
    void CmdSubmitKickAfterPositionToServer(Vector3 kickAfterPosition)
    {
        if (Vector2.Distance(kickAfterPosition, this.transform.position) > 3.0f)
            return;

        kickAfterFinalPosition = kickAfterPosition;
        isKickAfterPositionSet = true;
        TutorialManager.instance.DisableKickAfterPositioningControls();

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
    }
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
    public void HandleKickAfterAccuracyDifficultyUpdate(float oldValue, float newValue)
    {
        kickAfterAccuracyDifficulty = newValue;

        if (isOwnedByTutorialPlayer)
        {
            Vector3 newPosition = new Vector3(newValue, 0f, 0f);
            kickAfterMarkerRight.transform.localPosition = newPosition;
            newPosition.x *= -1;
            kickAfterMarkerLeft.transform.localPosition = newPosition;
        }
    }
    public void SubmitKickAfterKicking()
    {
        if (!isAccuracySubmittedYet)
        {
            isAccuracySubmittedYet = true;
            if (isOwnedByTutorialPlayer)
                CmdSubmitKickAfterAccuracyValue(kickAfterGuageLine.transform.localPosition.x);
            TutorialManager.instance.PlayerSubmittedKickAfterAccuracy();
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
    void CmdSubmitKickAfterAccuracyValue(float accuracyValue)
    {
        Debug.Log("CmdSubmitKickAfterAccuracyValue: " + accuracyValue);
        accuracyValueSubmitted = accuracyValue;
    }
    public void UpdateGamePadUIMarkersForGoblins(bool enableGamePadUI)
    {
        Debug.Log("UpdateGamePadUIMarkersForGoblins: to: " + enableGamePadUI.ToString() + " for goblin: " + this.name);
        qMarker.GetComponent<TutorialQEMarkerScript>().UpdateForGamepadUI(enableGamePadUI);
        eMarker.GetComponent<TutorialQEMarkerScript>().UpdateForGamepadUI(enableGamePadUI);
    }
}
