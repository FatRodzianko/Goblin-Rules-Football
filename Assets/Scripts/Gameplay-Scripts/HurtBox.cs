using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour
{
    GoblinScript myParentScript;
    Collider2D lastPunchBox;
    float nextPunchTime;
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
        if (collision.tag == "punchbox")
        {
            if (lastPunchBox != null)
            {
                if (lastPunchBox == collision && Time.time < nextPunchTime)
                    return;
                else
                {
                    lastPunchBox = collision;
                    nextPunchTime = Time.time + 0.15f;
                }
            }
            Debug.Log("OnTriggerEnter2D: Hurtbox collided with punchbox from: " + collision.transform.parent.name);
            myParentScript.HurtBoxCollision(collision.transform.parent.GetComponent<GoblinScript>());
        }
        //Can't collided with these due to layer physics stuff
        /*
        if (collision.tag == "goblin-body")
        {
            Debug.Log("HutrBox: collision with goblin body of: " + collision.transform.parent.name);
        }
        if (collision.tag == "Goblin")
        {
            Debug.Log("HutrBox: collision with Goblin (not goblin body) of: " + collision.name);
        }
        if (collision.tag == "hurtbox")
        {
            Debug.Log("HutrBox: collision with hurtbox of: " + collision.transform.parent.name);
        }
        */
    }
}
