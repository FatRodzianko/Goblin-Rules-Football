using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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
    [ClientCallback]
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
        if (isServer)
        {
            Debug.Log("OnTriggerEnter2D for SplitGroundEvent: " + this.name);
            if (collision.tag == "Goblin")
            {
                Debug.Log("SplitGroundEvent: collided with goblin named: " + collision.transform.name);
                uint goblinNetId = collision.transform.gameObject.GetComponent<NetworkIdentity>().netId;

                GoblinScript goblinScript = collision.transform.gameObject.GetComponent<GoblinScript>();
                if (goblinScript.canCollide && !goblinScript.isGoblinKnockedOut)
                    goblinScript.KnockOutGoblin(true);

            }
        }
        if (isClient)
        {
            if (collision.tag == "Goblin")
            {
                //this.PlaySFXClip();
                uint goblinNetId = collision.transform.gameObject.GetComponent<NetworkIdentity>().netId;

                GoblinScript goblinScript = collision.transform.gameObject.GetComponent<GoblinScript>();
                if (goblinScript.canCollide)
                    goblinScript.CollisionWithObstacleObject(true);
            }
        }

    }
    [ClientCallback]
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
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(this.transform.position);
        if (screenPoint.x < 0 || screenPoint.x > 1)
        {
            onscreen = false;
        }
        else
            onscreen = true;
        return onscreen;
    }
    [ServerCallback]
    public void DestroyMe()
    {
        if (isServer)
            NetworkServer.Destroy(this.gameObject);
    }
    [ClientCallback]
    public void PlayGroundSplitOpenSound()
    {
        Debug.Log("PlayGroundSplitOpenSound");
        if (this.IsOnScreen())
            myAudioManager.PlaySound("ground-split-open", 1.0f);
    }
    [ClientCallback]
    public void SetPlayZombieSoundTrue()
    {
        playZombieSound = true;
    }
    [ClientCallback]
    public void SetPlayZombieSoundFalse()
    {
        playZombieSound = false;
    }
}
