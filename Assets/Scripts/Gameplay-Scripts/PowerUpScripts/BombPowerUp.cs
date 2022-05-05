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

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
}
