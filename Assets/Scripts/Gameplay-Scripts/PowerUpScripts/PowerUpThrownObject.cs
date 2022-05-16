using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PowerUpThrownObject : NetworkBehaviour
{
    [Header("Sprite Stuff")]
    [SerializeField] SpriteRenderer mySpriteRenderer;
    [SerializeField] Animator myAnimator;
    [SerializeField] Sprite onGroundSprite;
    [SerializeField] Sprite collisionSprite; // change this to an animation thing? Like a bottle hits something, then there is an animation of glass breaking that plays?
    [SerializeField] string playerCollisionAnimationName;
    [SerializeField] string groundCollisionAnimationName;

    [Header("Player Info")]
    public GamePlayer myPlayerOwner;

    [Header("Throw/drop info")]
    [SyncVar] public bool isDroppedObject;
    [SyncVar] public bool slowObject;
    Vector3[] throwPoints = new Vector3[3];
    [SyncVar(hook = nameof(HandleIsThrown))] public bool isThrown = false;
    public float throwCount = 0.0f;
    public float throwSpeed;
    public float dropTime = 0f;

    [Header("SFX Types")]
    public bool isGlue;
    public bool isWater;
    public bool isBrush;

    [Header("SFX Stuff")]
    [SerializeField] public string sfxClipName;
    [SerializeField] bool playSoundOnDestroy;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [ServerCallback]
    private void FixedUpdate()
    {
        if (isThrown)
        {
            if (throwCount < 1.0f)
            {
                throwCount += throwSpeed * Time.fixedDeltaTime;
                Vector3 m1 = Vector3.Lerp(throwPoints[0], throwPoints[1], throwCount);
                Vector3 m2 = Vector3.Lerp(throwPoints[1], throwPoints[2], throwCount);
                this.transform.position = Vector3.Lerp(m1, m2, throwCount);
                if (throwCount >= 1.0f)
                {
                    isThrown = false;
                    //this.GetComponent<BoxCollider2D>().enabled = false;
                }
            }
            else
            {
                isThrown = false;

                //this.GetComponent<BoxCollider2D>().enabled = false;
            }
        }
    }
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("OnTriggerEnter2D for PowerUpThrownObject: " + this.name);
        if (slowObject)
            return;
        if (collision.tag == "Goblin" && isDroppedObject)
        {
            // This is just to make sure that the goblin who threw the banana isn't immediately knocked out by their own banana
            if ((dropTime + 0.2f) > Time.time && collision.GetComponent<GoblinScript>().serverGamePlayer == myPlayerOwner)
                return;

            Debug.Log("PowerUpThrownObject: collided with goblin named: " + collision.transform.name);
            uint goblinNetId = collision.transform.gameObject.GetComponent<NetworkIdentity>().netId;

            GoblinScript goblinScript = collision.transform.gameObject.GetComponent<GoblinScript>();
            if (goblinScript.canCollide)
            {
                goblinScript.KnockOutGoblin(false);
                NetworkServer.Destroy(this.gameObject);
            }


            /*if (!string.IsNullOrWhiteSpace(sfxClipName))
                this.RpcPlaySFXClip();*/

            //collision.transform.gameObject.GetComponent<GoblinScript>().KnockOutGoblin(false);
            //NetworkServer.Destroy(this.gameObject);

                //CmdPlayerPickUpFootball(goblinNetId);
        }
        if (collision.tag == "goblin-body" && !isDroppedObject)
        {
            Debug.Log("PowerUpThrownObject: collided with goblin named: " + collision.transform.parent.name);
            uint goblinNetId = collision.transform.parent.gameObject.GetComponent<NetworkIdentity>().netId;
            collision.transform.parent.gameObject.GetComponent<GoblinScript>().KnockOutGoblin(false);
            isThrown = false;
            this.GetComponent<BoxCollider2D>().enabled = false;
            myAnimator.Play(playerCollisionAnimationName);
        }
    }
    [ServerCallback]
    private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log("OnTriggerStay2D for ObstacleObject: " + this.name);
        if (!slowObject)
            return;
        if (collision.tag == "Goblin")
        {
            if (slowObject && !isThrown)
            {
                Debug.Log("ObstacleObject: still colliding with goblin named: " + collision.transform.name);
                collision.transform.gameObject.GetComponent<GoblinScript>().SlowDownObstacleEffect(true);
                collision.transform.gameObject.GetComponent<GoblinScript>().onGlueSlowDown = this.isGlue;
                collision.transform.gameObject.GetComponent<GoblinScript>().onWaterSlowDown = this.isWater;
                collision.transform.gameObject.GetComponent<GoblinScript>().onBrushSlowDown = this.isBrush;
            }
        }
    }
    [ServerCallback]
    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("OnTriggerStay2D for ObstacleObject: " + this.name);
        if (!slowObject)
            return;
        if (collision.tag == "Goblin")
        {
            if (slowObject && !isThrown)
            {
                Debug.Log("ObstacleObject: still colliding with goblin named: " + collision.transform.name);
                collision.transform.gameObject.GetComponent<GoblinScript>().SlowDownObstacleEffect(false);
                collision.transform.gameObject.GetComponent<GoblinScript>().onGlueSlowDown = false;
                collision.transform.gameObject.GetComponent<GoblinScript>().onWaterSlowDown = false;
                collision.transform.gameObject.GetComponent<GoblinScript>().onBrushSlowDown = false;
                NetworkServer.Destroy(this.gameObject);
            }
        }
    }
    public void DropBehind(GamePlayer playerOwner)
    {
        Debug.Log("DropBehind");
        if (!myPlayerOwner)
            myPlayerOwner = playerOwner;
        //Get direction the player throwing the object is facing
        int directionToDrop = 1;
        if (myPlayerOwner.serverSelectGoblin.GetComponent<SpriteRenderer>().flipX)
        {
            Debug.Log("DropBehind: flip x is true");
            directionToDrop = -1;
            RpcFlipXOfSpriteRenderer(true);
        }

        //Calculate directory of object to throw
        Vector3 startingPosition = myPlayerOwner.serverSelectGoblin.transform.position;
        throwPoints[0] = startingPosition; // startung position
        Vector3 controlPoint = startingPosition;
        controlPoint.y += 0.5f;
        controlPoint.x -= (1f * directionToDrop);
        if (controlPoint.x > GameplayManager.instance.maxX)
            controlPoint.x = GameplayManager.instance.maxX;
        else if (controlPoint.x < GameplayManager.instance.minX)
            controlPoint.x = GameplayManager.instance.minX;
        throwPoints[1] = controlPoint; // control point aka the mid point of throwing arc
        controlPoint.x -= (1f * directionToDrop);
        controlPoint.y -= 1.5f;
        if (controlPoint.y > GameplayManager.instance.maxY)
            controlPoint.y = GameplayManager.instance.maxY;
        if (controlPoint.x > GameplayManager.instance.maxX)
            controlPoint.x = GameplayManager.instance.maxX;
        else if (controlPoint.x < GameplayManager.instance.minX)
            controlPoint.x = GameplayManager.instance.minX;
        throwPoints[2] = controlPoint; // destination point
        throwCount = 0.0f;
        isThrown = true;
    }
    public void ThrowForward(GamePlayer playerOwner, Vector3 startingPosition)
    {
        Debug.Log("ThrowForward");
        if (!myPlayerOwner)
            myPlayerOwner = playerOwner;
        //Get direction the player throwing the object is facing
        GoblinScript selectedGoblin = myPlayerOwner.serverSelectGoblin;
        int directionToDrop = 1;
        if (selectedGoblin.GetComponent<SpriteRenderer>().flipX)
        {
            Debug.Log("ThrowForward: flip x is true");
            directionToDrop = -1;
            RpcFlipXOfSpriteRenderer(true);
        }
        float playerMovementModifier = 0;
        if (selectedGoblin.isRunningOnServer)
        {
            playerMovementModifier = selectedGoblin.speed / 2.5f;
        }
        
        //Calculate throw trajectory
        /*Vector3 startingPosition = myPlayerOwner.serverSelectGoblin.transform.position;
        startingPosition.y += 0.5f;
        startingPosition.x += (1 * directionToDrop);*/
        throwPoints[0] = startingPosition; // startung position
        Vector3 controlPoint = startingPosition;
        controlPoint.x += ((8f + playerMovementModifier) * directionToDrop);
        controlPoint.y += 0.2f;
        throwPoints[1] = controlPoint; // control point aka the mid point of throwing arc
        Vector3 endPosition = startingPosition;
        endPosition.y -= 1.7f;
        endPosition.x += ((11f + playerMovementModifier) * directionToDrop);
        throwPoints[2] = endPosition; // destination point

        throwCount = 0.0f;
        isThrown = true;
        RpcEnableSpriteRenderer(true);
    }
    void HandleIsThrown(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            isThrown = newValue;                
        }            
        if (isClient)
        {
            if (!newValue)
            {
                if (isDroppedObject)
                    mySpriteRenderer.sprite = onGroundSprite;
                else
                {
                    myAnimator.Play(groundCollisionAnimationName);
                    this.GetComponent<BoxCollider2D>().enabled = false;
                }
            }
                
        }
    }
    [ClientRpc]
    void RpcFlipXOfSpriteRenderer(bool flip)
    {
        mySpriteRenderer.flipX = flip;
    }
    public void DestroyObjectOnServer()
    {
        if (isServer)
        {
            NetworkServer.Destroy(this.gameObject);
        }
    }
    [ClientRpc]
    void RpcEnableSpriteRenderer(bool activate)
    {
        mySpriteRenderer.enabled = activate;
    }
    /*[ClientCallback]
    public void PlaySFXClip()
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(this.transform.position);
        if (screenPoint.x < 0 || screenPoint.x > 1)
            return;
        SoundManager.instance.PlaySound(sfxClipName, 0.75f);
    }*/
    public bool IsOnScreen()
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
    [ClientCallback]
    void PlaySFXClip()
    {
        Debug.Log("PowerUpThrownObject: RpcPlaySFXClip for " + this.name);
        if (this.IsOnScreen() && !string.IsNullOrWhiteSpace(sfxClipName))
        {
            SoundManager.instance.PlaySound(sfxClipName, 0.75f);
        }
    }
    private void OnDestroy()
    {
        if (isClient && playSoundOnDestroy)
        {
            this.PlaySFXClip();
        }
    }
}
