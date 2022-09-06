using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchDownTrigger : MonoBehaviour
{
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
        if (collision.tag == "Goblin")
        { 
            TutorialGoblinScript goblin = collision.transform.GetComponent<TutorialGoblinScript>();
            if (goblin.isOwnedByTutorialPlayer && goblin.doesCharacterHaveBall)
            {
                TutorialManager.instance.PlayerIsNearEndzone();
            }
        }
    }
}
