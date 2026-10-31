using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class SplitGroundEvent : NetworkBehaviour
{
    [Header("SFX Stuff")]
    [SerializeField] public string sfxClipName;
    [SerializeField] GoblinSoundManager myAudioManager;
    public bool playZombieSound = false;
    public bool isZombieSoundPlaying = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Client(Logging = LoggingType.Off)]
    private void FixedUpdate()
    {
        if (this.IsOnScreen() && playZombieSound)
        {
            if (!isZombieSoundPlaying)
            {
                myAudioManager.PlaySound("zombie-sounds", 1.0f);
                isZombieSoundPlaying = true;
            }
            
        }
        else
        {
            myAudioManager.StopSound("zombie-sounds");
            isZombieSoundPlaying = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsServerInitialized)
        {
            Debug.Log("OnTriggerEnter2D for SplitGroundEvent: " + this.name);
            if (collision.tag == "Goblin")
            {
                Debug.Log("SplitGroundEvent: collided with goblin named: " + collision.transform.name);
                int goblinNetId = collision.transform.gameObject.GetComponent<NetworkObject>().ObjectId;

                GoblinScript goblinScript = collision.transform.gameObject.GetComponent<GoblinScript>();
                if (goblinScript.canCollide && !goblinScript.isGoblinKnockedOut)
                    goblinScript.KnockOutGoblin(true);

            }
        }
        if (IsClientInitialized)
        {
            if (collision.tag == "Goblin")
            {
                //this.PlaySFXClip();
                int goblinNetId = collision.transform.gameObject.GetComponent<NetworkObject>().ObjectId;

                GoblinScript goblinScript = collision.transform.gameObject.GetComponent<GoblinScript>();
                if (goblinScript.canCollide)
                    goblinScript.CollisionWithObstacleObject(true);
            }
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
    [Server]
    public void DestroyMe()
    {
        if (IsServerInitialized)
            InstanceFinder.ServerManager.Despawn(this.gameObject);
    }
    // Animation event: it fires on every machine that plays the clip, so it skips quietly where it doesn't apply.
    [Client(Logging = LoggingType.Off)]
    public void PlayGroundSplitOpenSound()
    {
        Debug.Log("PlayGroundSplitOpenSound");
        if (this.IsOnScreen())
            myAudioManager.PlaySound("ground-split-open", 1.0f);
    }
    // Animation event: it fires on every machine that plays the clip, so it skips quietly where it doesn't apply.
    [Client(Logging = LoggingType.Off)]
    public void SetPlayZombieSoundTrue()
    {
        playZombieSound = true;
    }
    // Animation event: it fires on every machine that plays the clip, so it skips quietly where it doesn't apply.
    [Client(Logging = LoggingType.Off)]
    public void SetPlayZombieSoundFalse()
    {
        playZombieSound = false;
    }
}
