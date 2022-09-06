using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEndzone : MonoBehaviour
{
    public bool isGrey;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("OnTriggerEnter2D for " + this.name);

        if (collision.tag == "touchdown-hitbox")
        {
            Debug.Log(this.name + ": collision with touchdown-hitbox");
            TutorialGoblinScript goblin = collision.transform.parent.GetComponent<TutorialGoblinScript>();
            if (goblin.doesCharacterHaveBall && isGrey != goblin.isGoblinGrey)
            {
                Debug.Log(this.name + ": Goblin with ball touched down in opposite endzone. This should be a touchdown. Goblin: " + goblin.name);

                //
                // Redo in TutorialManager
                //GameplayManager.instance.TouchDownScored(goblin.isGoblinGrey, goblin.GetComponent<NetworkIdentity>().netId, goblin.transform.position.y);
                //GameplayManager.instance.ActivateGameTimer(false);
                //
                //
                TutorialManager.instance.PlayerScoredTouchdown();
            }
        }
    }
}
