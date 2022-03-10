using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ObstacleObject : NetworkBehaviour
{
    [SerializeField] Collider2D myCollider;
    [SerializeField] bool isTripObject;
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
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("OnTriggerEnter2D for ObstacleObject: " + this.name);
        if (collision.tag == "Goblin")
        {
            if (isTripObject)
            {
                Debug.Log("ObstacleObject: collided with goblin named: " + collision.transform.name);
                uint goblinNetId = collision.transform.gameObject.GetComponent<NetworkIdentity>().netId;
                collision.transform.gameObject.GetComponent<GoblinScript>().KnockOutGoblin(false);
                //NetworkServer.Destroy(this.gameObject);
                //CmdPlayerPickUpFootball(goblinNetId);
            }
        }
    }

}
