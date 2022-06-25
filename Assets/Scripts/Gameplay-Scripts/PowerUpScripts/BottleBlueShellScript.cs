using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BottleBlueShellScript : NetworkBehaviour
{
    [Header("Target Info")]
    public GameObject myTarget;
    [SyncVar] public uint myTargetNetId;

    [Header("Animation Stuff")]
    [SerializeField] SpriteRenderer mySpriteRenderer;
    [SerializeField] Animator myAnimator;
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
        Debug.Log("Start function for BottleBlueShellScript on: " + this.name);
    }

    // Update is called once per frame
    void Update()
    {

    }
    [ServerCallback]
    private void FixedUpdate()
    {
        if (!myTarget)
            return;
        if (!(GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time"))
        {
            NetworkServer.Destroy(this.gameObject);
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
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("OnTriggerEnter2D for BottleBlueShellScript: " + this.name);
        if (!(GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time"))
        {
            NetworkServer.Destroy(this.gameObject);
        }

        //if ((collision.tag == "goblin-body" || collision.name == "diving-hitbox") && collision.transform.parent.gameObject == myTarget)
        if ((collision.tag == "hurtbox") && collision.transform.parent.gameObject == myTarget)
        {
            Debug.Log("BottleBlueShellScript: collided with goblin named: " + collision.transform.parent.name);
            hasHitTargetYet = true;
            uint goblinNetId = collision.transform.parent.gameObject.GetComponent<NetworkIdentity>().netId;
            GoblinScript goblinHit = collision.transform.parent.gameObject.GetComponent<GoblinScript>();
            if(!goblinHit.isGoblinKnockedOut)
                goblinHit.KnockOutGoblin(true);
            this.GetComponent<BoxCollider2D>().enabled = false;
            this.transform.Rotate(0f, 0f, -90f, Space.Self);
            myAnimator.Play(playerCollisionAnimationName);
        }
    }
    public void PlayHitClip()
    {
        this.PlaySFXClip(sfxCollisionName);
    }
    [ClientCallback]
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
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(this.transform.position);
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
        if (isServer)
        {
            NetworkServer.Destroy(this.gameObject);
        }
    }
}
