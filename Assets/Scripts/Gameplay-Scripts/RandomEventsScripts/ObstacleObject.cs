using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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
    [ServerCallback]
    public void DisableColliderDuringPhase(bool enable)
    {
        myCollider.enabled = enable;
    }
    //[ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isServer)
        {
            Debug.Log("OnTriggerEnter2D for ObstacleObject: " + this.name);
            if (collision.tag == "Goblin")
            {
                if (isTripObject)
                {
                    Debug.Log("ObstacleObject: collided with goblin named: " + collision.transform.name);
                    uint goblinNetId = collision.transform.gameObject.GetComponent<NetworkIdentity>().netId;

                    GoblinScript goblinScript = collision.transform.gameObject.GetComponent<GoblinScript>();
                    if (goblinScript.canCollide)
                        goblinScript.KnockOutGoblin(false);

                    //collision.transform.gameObject.GetComponent<GoblinScript>().KnockOutGoblin(false);
                    //NetworkServer.Destroy(this.gameObject);
                    //CmdPlayerPickUpFootball(goblinNetId);
                    /*if (!string.IsNullOrWhiteSpace(sfxClipName))
                        this.RpcPlaySFXClip();*/
                }
            }
        }
        if (isClient)
        {
            if (collision.tag == "Goblin")
            {
                if (isTripObject)
                {
                    this.PlaySFXClip();
                }
            }
        }
        
    }
    [ServerCallback]
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
    [ClientRpc]
    void RpcPlaySFXClip()
    {
        if (this.IsOnScreen() && !string.IsNullOrWhiteSpace(sfxClipName))
        {
            SoundManager.instance.PlaySound(sfxClipName, 0.75f);
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

}
