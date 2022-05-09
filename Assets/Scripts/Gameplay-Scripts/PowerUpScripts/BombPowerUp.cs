using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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
    [ServerCallback]
    public void StartBombCountDown()
    {   
        if (!isCountdownRunning)
        {
            Debug.Log("StartBombCountDown");
            countDownRoutine = CountdownRoutine();
            StartCoroutine(countDownRoutine);
        }
    }
    [ServerCallback]
    IEnumerator CountdownRoutine()
    {
        isCountdownRunning = true;
        RpcStartTimerSound();
        yield return new WaitForSeconds(3.0f);
        myAnimator.SetBool("explode", true);
    }
    [ServerCallback]
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
                    hitCollider.GetComponent<GoblinScript>().KnockOutGoblin(true);
                }
            }
        }

    }
    [ServerCallback]
    public void DestroyMe()
    {
        NetworkServer.Destroy(this.gameObject);
    }
    [ClientCallback]
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
    void RpcStartTimerSound()
    {
        PlayTimerSound();
    }
    [ClientCallback]
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
    [ClientCallback]
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
