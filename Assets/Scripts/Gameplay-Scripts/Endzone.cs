using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Endzone : NetworkBehaviour
{
    [SerializeField] private BoxCollider2D endzoneCollider;
    public bool isGrey;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("OnTriggerEnter2D for " + this.name);
        if (collision.tag == "goblin-body")
        {
            /*//Debug.Log(this.name + ": collided with goblin-body of goblin named: " + collision.transform.parent.name);
            GoblinScript goblin = collision.transform.parent.GetComponent<GoblinScript>();
            if (goblin.isDiving && goblin.doesCharacterHaveBall)
                Debug.Log(this.name + ": Goblin is diving in the endzone for a touchdown " + collision.transform.parent.name);
            else
            {
                Debug.Log(this.name + ": is in the endzone but NO touchdown " + collision.transform.parent.name);
            }*/
            
        }
        if (collision.tag == "touchdown-hitbox")
        {
            Debug.Log(this.name + ": collision with touchdown-hitbox");
            GoblinScript goblin = collision.transform.parent.GetComponent<GoblinScript>();
            if (goblin.doesCharacterHaveBall && isGrey != goblin.isGoblinGrey)
            {
                Debug.Log(this.name + ": Goblin with ball touched down in opposite endzone. This should be a touchdown. Goblin: " + goblin.name);
                GameplayManager.instance.TouchDownScored(goblin.isGoblinGrey, goblin.GetComponent<NetworkIdentity>().netId, goblin.transform.position.y);
                GameplayManager.instance.ActivateGameTimer(false);
            }
        }
    }
}
