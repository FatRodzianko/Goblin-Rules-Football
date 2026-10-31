using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class BombPowerUp : NetworkBehaviour
{
    public bool isCountdownRunning;
    IEnumerator countDownRoutine;
    [SerializeField] Animator myAnimator;
    [SerializeField] LayerMask goblinLayer;
    bool isTimerSFXPlaying = false;
    bool isExploded = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        PlayTimerSound();
    }
    // Animation event: it fires on every machine that plays the clip, so it skips quietly where it doesn't apply.
    [Server(Logging = LoggingType.Off)]
    public void StartBombCountDown()
    {   
        if (!isCountdownRunning)
        {
            Debug.Log("StartBombCountDown");
            countDownRoutine = CountdownRoutine();
            StartCoroutine(countDownRoutine);
        }
    }
    [Server]
    IEnumerator CountdownRoutine()
    {
        isCountdownRunning = true;
        RpcStartTimerSound();
        yield return new WaitForSeconds(0.6f);
        myAnimator.SetBool("explode", true);
    }
    // Animation event: it fires on every machine that plays the clip, so it skips quietly where it doesn't apply.
    [Server(Logging = LoggingType.Off)]
    public void ExplosionKnockOutGoblins()
    {
        Debug.Log("ExplosionKnockOutGoblins");
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(this.transform.position, 3.0f, goblinLayer);
        if (hitColliders.Length > 0)
        {
            foreach (Collider2D hitCollider in hitColliders)
            { 
                if (hitCollider.tag == "Goblin")
                {
                    Debug.Log("ExplosionKnockOutGoblins: Will knock out goblin: " + hitCollider.gameObject.name);
                    GoblinScript hitGoblin = hitCollider.GetComponent<GoblinScript>();
                    if(!hitGoblin.isDiving)
                        hitGoblin.KnockOutGoblin(true);
                }
            }
        }

    }
    // Animation event: it fires on every machine that plays the clip, so it skips quietly where it doesn't apply.
    [Server(Logging = LoggingType.Off)]
    public void DestroyMe()
    {
        InstanceFinder.ServerManager.Despawn(this.gameObject);
    }
    // Animation event: it fires on every machine that plays the clip, so it skips quietly where it doesn't apply.
    [Client(Logging = LoggingType.Off)]
    public void SFXBombExplode()
    {
        if (IsOnScreen())
        {
            isExploded = true;
            StopTimerSound();
            SoundManager.instance.PlaySound("bomb-explode", 1f);
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
    [ObserversRpc]
    void RpcStartTimerSound()
    {
        PlayTimerSound();
    }
    [Client]
    void PlayTimerSound()
    {
        if (IsOnScreen())
        {
            if (!isTimerSFXPlaying && !isExploded)
            {
                SoundManager.instance.PlaySound("bomb-timer", 0.75f);
                isTimerSFXPlaying = true;
            }
        }
        else
        {
            if (isTimerSFXPlaying)
                StopTimerSound();
        }
    }
    [Client]
    void StopTimerSound()
    {
        SoundManager.instance.StopSound("bomb-timer");
        //SoundManager.instance.StopPlaying("bomb-timer");
        isTimerSFXPlaying = false;
    }
    private void OnDestroy()
    {
        SoundManager.instance.StopSound("bomb-timer");
    }
}
