using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TripObject : NetworkBehaviour
{
    Collider2D myCollider;
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
    public void DisableColliderForKickAfter(bool enable)
    {
        myCollider.enabled = enable;
    }
    [ServerCallback]
    public void KickAfterWaitToEnableObstacleColliders()
    {
        IEnumerator waitToEnableObstacleColliders = WaitToEnableObstacleColliders();
        StartCoroutine(waitToEnableObstacleColliders);
    }
    [ServerCallback]
    IEnumerator WaitToEnableObstacleColliders()
    {
        yield return new WaitForSeconds(0.666f);
        DisableColliderForKickAfter(true);
    }
}
