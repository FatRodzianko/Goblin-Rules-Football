using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideBox : MonoBehaviour
{
    GoblinScript myParentScript;
    // Start is called before the first frame update
    void Start()
    {
        myParentScript = this.transform.parent.GetComponent<GoblinScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Goblin")
        {
            Debug.Log("OnTriggerEnter2D: SlideBox collided with Goblin from: " + collision.transform.name);
            // The contact is claimed by the machine that owns the slider - what that player saw - and the server
            // verifies it before the trip (Attack Fairness Plan.md). Other machines see the same overlap and stay
            // quiet; SlideBoxCollision remains the server-side applier.
            if (myParentScript.IsOwner)
                myParentScript.ReportSlideHit(collision.transform.GetComponent<GoblinScript>());
        }
    }
}
