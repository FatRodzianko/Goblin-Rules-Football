using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Object;
using FishNet.Object.Synchronizing;

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
    [Server]
    public void DisableColliderForKickAfter(bool enable)
    {
        myCollider.enabled = enable;
    }
    [Server]
    public void KickAfterWaitToEnableObstacleColliders()
    {
        IEnumerator waitToEnableObstacleColliders = WaitToEnableObstacleColliders();
        StartCoroutine(waitToEnableObstacleColliders);
    }
    [Server]
    IEnumerator WaitToEnableObstacleColliders()
    {
        yield return new WaitForSeconds(0.666f);
        DisableColliderForKickAfter(true);
    }
}
