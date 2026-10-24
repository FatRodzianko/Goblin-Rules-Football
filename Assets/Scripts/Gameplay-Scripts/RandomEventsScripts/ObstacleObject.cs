using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class ObstacleObject : NetworkBehaviour
{
    [SerializeField] Collider2D myCollider;
    [SerializeField] bool isTripObject;
    public bool isSlowObject;

    [Header("SFX Types")]
    public bool isGlue;
    public bool isWater;
    public bool isBrush;

    [Header("SFX Stuff")]
    [SerializeField] public string sfxClipName;

    // Start is called before the first frame update
    void Start()
    {
        myCollider = this.GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Server]
    public void DisableColliderDuringPhase(bool enable)
    {
        myCollider.enabled = enable;
    }
    //[ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsServerInitialized)
        {
            Debug.Log("OnTriggerEnter2D for ObstacleObject: " + this.name);
            if (collision.tag == "Goblin")
            {
                if (isTripObject)
                {
                    Debug.Log("ObstacleObject: collided with goblin named: " + collision.transform.name);
                    int goblinNetId = collision.transform.gameObject.GetComponent<NetworkObject>().ObjectId;

                    GoblinScript goblinScript = collision.transform.gameObject.GetComponent<GoblinScript>();
                    if (goblinScript.canCollide && !goblinScript.isGoblinKnockedOut)
                        goblinScript.KnockOutGoblin(false);

                    //collision.transform.gameObject.GetComponent<GoblinScript>().KnockOutGoblin(false);
                    //NetworkServer.Destroy(this.gameObject);
                    //CmdPlayerPickUpFootball(goblinNetId);
                    /*if (!string.IsNullOrWhiteSpace(sfxClipName))
                        this.RpcPlaySFXClip();*/
                }
            }
        }
        if (IsClientInitialized)
        {
            if (collision.tag == "Goblin")
            {
                if (isTripObject)
                {
                    this.PlaySFXClip();
                    int goblinNetId = collision.transform.gameObject.GetComponent<NetworkObject>().ObjectId;

                    GoblinScript goblinScript = collision.transform.gameObject.GetComponent<GoblinScript>();
                    if (goblinScript.canCollide)
                        goblinScript.CollisionWithObstacleObject(false);
                    goblinScript.CheckForStubbedToeAchievement();
                }
            }
        }
        
    }
    [Server(Logging = LoggingType.Off)]
    private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log("OnTriggerStay2D for ObstacleObject: " + this.name);
        if (collision.tag == "Goblin")
        {
            if (!isTripObject)
            {
                Debug.Log("ObstacleObject: still colliding with goblin named: " + collision.transform.name);
                collision.transform.gameObject.GetComponent<GoblinScript>().SlowDownObstacleEffect(true);
                if (isSlowObject)
                {
                    collision.transform.gameObject.GetComponent<GoblinScript>().onWaterSlowDown = this.isWater;
                    collision.transform.gameObject.GetComponent<GoblinScript>().onBrushSlowDown = this.isBrush;
                    collision.transform.gameObject.GetComponent<GoblinScript>().onGlueSlowDown = this.isGlue;
                }

            }
        }
    }
    // Server only, like OnTriggerStay2D: the slowdown and footstep values are server-owned SyncVars that clients receive.
    [Server(Logging = LoggingType.Off)]
    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("OnTriggerStay2D for ObstacleObject: " + this.name);
        if (collision.tag == "Goblin")
        {
            if (!isTripObject)
            {
                Debug.Log("ObstacleObject: still colliding with goblin named: " + collision.transform.name);
                collision.transform.gameObject.GetComponent<GoblinScript>().SlowDownObstacleEffect(false);
                if (isSlowObject)
                {
                    collision.transform.gameObject.GetComponent<GoblinScript>().onWaterSlowDown = false;
                    collision.transform.gameObject.GetComponent<GoblinScript>().onBrushSlowDown = false;
                    collision.transform.gameObject.GetComponent<GoblinScript>().onGlueSlowDown = false;
                }   
            }
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
    [ObserversRpc]
    void RpcPlaySFXClip()
    {
        if (this.IsOnScreen() && !string.IsNullOrWhiteSpace(sfxClipName))
        {
            SoundManager.instance.PlaySound(sfxClipName, 0.75f);
        }
    }
    [Client]
    void PlaySFXClip()
    {
        if (GameplayManager.instance.gamePhase == "touchdown-transition")
            return;
        if (this.IsOnScreen() && !string.IsNullOrWhiteSpace(sfxClipName))
        {
            SoundManager.instance.PlaySound(sfxClipName, 1.0f);
        }

    }

}
