﻿using System.Collections;
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

    [SerializeField] public CinemachineVirtualCamera myCamera;
    public GamePlayer localPlayer;

    public override void OnStartClient()
    {
        base.OnStartClient();
        myRenderer.enabled = true;
        myCollider.enabled = true;

        myCamera = GameObject.FindGameObjectWithTag("camera").GetComponent<CinemachineVirtualCamera>();
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
        if (!isHeld)
        {
            GameObject goblinToCheck = NetworkIdentity.spawned[goblinNetId].gameObject;
            GoblinScript goblinToCheckScript = goblinToCheck.GetComponent<GoblinScript>();            
            
            if (!goblinToCheckScript.isGoblinKnockedOut && !goblinToCheckScript.isSliding && !goblinToCheckScript.isDiving && !goblinToCheckScript.isBlocking && !goblinToCheckScript.isPunching)
            {
                float distanceToBall = Vector3.Distance(goblinToCheck.transform.position, this.transform.position);
                Debug.Log("CmdPlayerPickUpFootball: distance to ball is: " + distanceToBall.ToString() + " from goblin with netid of " + goblinNetId.ToString());
                if (distanceToBall < 2.5f)
                {
                    Debug.Log("CmdPlayerPickUpFootball: Goblin is close enough to pick up the ball");
                    //GoblinScript goblinToCheckScript = goblinToCheck.GetComponent<GoblinScript>();

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
            isHeld = newValue;
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

        if (throwingGoblinScript.doesCharacterHaveBall && !goblinToThrowToScript.doesCharacterHaveBall)
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
}
