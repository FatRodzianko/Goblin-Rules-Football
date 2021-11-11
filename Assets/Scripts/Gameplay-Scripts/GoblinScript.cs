using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GoblinScript : NetworkBehaviour
{
    public float pressedTime = 0f;
    public float releasedTime = 0f;

    [Header("Player Owner Info")]
    [SyncVar] public string ownerName;
    [SyncVar] public int ownerConnectionId;
    [SyncVar] public int ownerPlayerNumber;
    [SyncVar] public uint ownerNetId;
    public GamePlayer myGamePlayer;

    [Header("Goblin Base Stats")]
    [SyncVar] public int MaxHealth;
    [SyncVar] public float MaxStamina;
    [SyncVar] public float StaminaDrain;
    [SyncVar] public float StaminaRecovery;
    [SyncVar] public float MaxSpeed;
    [SyncVar] public int MaxDamage;


    [Header("Goblin Current Stats")]
    [SyncVar(hook = nameof(UpdateGoblinHealth))] public float health;
    [SyncVar(hook = nameof(UpdateGoblinStamina))] public float stamina;
    [SyncVar] public float speed;
    [SyncVar] public float damage;
    [SyncVar] public float ballCarrySpeedModifier = 1.0f;
    [SyncVar] public float blockingSpeedModifier = 1.0f;

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

    [Header("Character Properties")]    
    public Animator animator;
    public Rigidbody2D rb;
    public Collider2D playerCollider;
    public Vector2 previousInput;
    public SpriteRenderer myRenderer;
    [SerializeField] BoxCollider2D golbinBodyCollider;
    [SyncVar] public string goblinType;
    [SerializeField] private StatusBarScript myStatusBars;


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
        //rb.bodyType = RigidbodyType2D.Kinematic;
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
                    rb.bodyType = RigidbodyType2D.Dynamic;
                }
                else
                {
                    rb.bodyType = RigidbodyType2D.Kinematic;
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
                            myGamePlayer.SwitchToEGoblin();
                        }
                        else if (this.isQGoblin)
                        {
                            myGamePlayer.SwitchToQGoblin();
                        }
                    }
                }
            }
            else
            {
                GameObject football = GameObject.FindGameObjectWithTag("football");
                football.transform.parent = null;
            }
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
    [Command]
    void CmdStartThrowing()
    {
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
                    this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer) * 1.15f;
                }
                //Update CanRecoverStamina Event here?
                if (isStaminaRecoveryRoutineRunning)
                    StopCoroutine(staminaRecoveryRoutine);
      
                staminaRecoveryRoutine = CanGoblinRecoverStamina();
                StartCoroutine(staminaRecoveryRoutine);                

            }
            else if (isFatigued)
            {
                this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer) * 0.5f;
            }
        }        
        else
        {
            if(!isFatigued)
                this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer * blockingSpeedModifier);
            else
                this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer * blockingSpeedModifier) * 0.5f;
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
                this.speed = (MaxSpeed * ballCarrySpeedModifier * slideSpeedModifer) * 0.5f;
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

                goblinReceivingDamage.health -= (goblinGivingDamage.damage * blockingModifier);
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
                }
            }
            
        }
    }
    [Server]
    public void KnockOutGoblin(bool knockedOut)
    {
        Debug.Log("KnockOutGoblin: " + knockedOut.ToString());
        if (this.health < 0f)
            this.health = 0f;
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
        yield return new WaitForSeconds(4.5f);
        isHealthRecoveryRoutineRunning = false;
        this.health = (MaxHealth * 0.25f);
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
                    animator.Play(goblinType + "-knocked-out");
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
            CmdSlideGoblin(previousInput);
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
                animator.SetBool("isSliding", newValue);
        }
    }
    [Server]
    public IEnumerator SlideGoblinRoutine()
    {
        isSlidingRoutineRunning = true;
        isSliding = true;
        speed = MaxSpeed * 1.2f;
        yield return new WaitForSeconds(0.5f);
        isSliding = false;
        slideSpeedModifer = 0.7f;
        yield return new WaitForSeconds(1.25f);
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
            HandleIsBlocking(this.isBlocking, false);

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
}
