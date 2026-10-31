using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Component.Animating;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class BottleBlueShellScript : NetworkBehaviour
{
    [Header("Target Info")]
    public GameObject myTarget;
    private readonly SyncVar<int> _syncMyTargetNetId = new SyncVar<int>(ObjectLookup.NoObject);
    public int myTargetNetId { get => _syncMyTargetNetId.Value; set => _syncMyTargetNetId.Value = value; }

    [Header("Animation Stuff")]
    [SerializeField] SpriteRenderer mySpriteRenderer;
    [SerializeField] Animator myAnimator;
    NetworkAnimator networkAnimator;
    [SerializeField] string playerCollisionAnimationName;

    [Header("Other stuff???")]
    public float speed;
    [SerializeField] Rigidbody2D rb;
    bool hasHitTargetYet = false;

    [Header("SFX Stuff")]
    [SerializeField] public string sfxCollisionName;
    [SerializeField] public string sfxFlyingName;

    // Start is called before the first frame update
    void Start()
    {
        networkAnimator = GetComponent<NetworkAnimator>();
        Debug.Log("Start function for BottleBlueShellScript on: " + this.name);
    }

    // Update is called once per frame
    void Update()
    {

    }
    [Server(Logging = LoggingType.Off)]
    private void FixedUpdate()
    {
        if (!myTarget)
            return;
        if (!(GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time"))
        {
            InstanceFinder.ServerManager.Despawn(this.gameObject);
        }
        if (Vector2.Distance(this.transform.position, myTarget.transform.position) > 0.15f && !this.hasHitTargetYet)
        {
            Vector2 directionToFly = (myTarget.transform.position - this.transform.position).normalized;
            // Get the direction the bottle is flying and set the rotation of the bottle accordingly
            if (Mathf.Abs(directionToFly.x) >= 0.707f)
            {
                if (directionToFly.x > 0)
                {
                    this.transform.localRotation = Quaternion.Euler(0, 0f, 90f);
                }
                else
                {
                    this.transform.localRotation = Quaternion.Euler(0, 0f, -90f);
                }
            }
            else
            {
                if (directionToFly.y > 0)
                {
                    this.transform.localRotation = Quaternion.Euler(0, 0f, 180f);
                }
                else
                {
                    this.transform.localRotation = Quaternion.Euler(0, 0f, 0);
                }
            }
            rb.MovePosition(rb.position + directionToFly * speed * Time.fixedDeltaTime);
        }
    }
    [Server(Logging = LoggingType.Off)]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("OnTriggerEnter2D for BottleBlueShellScript: " + this.name);
        if (!(GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time"))
        {
            InstanceFinder.ServerManager.Despawn(this.gameObject);
        }

        //if ((collision.tag == "goblin-body" || collision.name == "diving-hitbox") && collision.transform.parent.gameObject == myTarget)
        if ((collision.tag == "hurtbox") && collision.transform.parent.gameObject == myTarget)
        {
            Debug.Log("BottleBlueShellScript: collided with goblin named: " + collision.transform.parent.name);
            hasHitTargetYet = true;
            int goblinNetId = collision.transform.parent.gameObject.GetComponent<NetworkObject>().ObjectId;
            GoblinScript goblinHit = collision.transform.parent.gameObject.GetComponent<GoblinScript>();
            if(!goblinHit.isGoblinKnockedOut)
                goblinHit.KnockOutGoblin(true);
            this.GetComponent<BoxCollider2D>().enabled = false;
            this.transform.Rotate(0f, 0f, -90f, Space.Self);
            // Through the NetworkAnimator, so clients see it: FishNet only sends state changes made through it.
            networkAnimator.Play(playerCollisionAnimationName, 0, float.NegativeInfinity);
        }
    }
    public void PlayHitClip()
    {
        this.PlaySFXClip(sfxCollisionName);
    }
    [Client]
    void PlaySFXClip(string clipName)
    {
        Debug.Log("PowerUpThrownObject: RpcPlaySFXClip for " + this.name);
        if (this.IsOnScreen() && !string.IsNullOrWhiteSpace(clipName))
        {
            SoundManager.instance.PlaySound(clipName, 0.75f);
        }
    }
    public bool IsOnScreen()
    {
        bool onscreen = false;
        // No camera while a scene is unloading, and despawn paths still ask; nothing is on screen then.
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            return false;
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(this.transform.position);
        if (screenPoint.x < 0 || screenPoint.x > 1)
        {
            onscreen = false;
        }
        else
            onscreen = true;
        return onscreen;
    }
    public void DestroyObjectOnServer()
    {
        if (IsServerInitialized)
        {
            InstanceFinder.ServerManager.Despawn(this.gameObject);
        }
    }
}
