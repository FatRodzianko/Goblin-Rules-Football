using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour
{
    GoblinScript myParentScript;
    public Collider2D lastPunchBox;
    public float nextPunchTime;
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
            else
            {
                lastPunchBox = collision;
            }
            PunchBoxScript punchBoxScript = collision.gameObject.GetComponent<PunchBoxScript>();
            GoblinScript punchingGoblin = collision.transform.parent.GetComponent<GoblinScript>();
            if (Time.time > punchBoxScript.nextPunchTime && !this.myParentScript.isGoblinKnockedOut && this.myParentScript.isGoblinGrey != punchingGoblin.isGoblinGrey)
            {
                Debug.Log("OnTriggerEnter2D: Hurtbox on " + this.transform.parent.name + ":" + this.myParentScript.ownerConnectionId.ToString() +  " collided with punchbox from: " + collision.transform.parent.name + ":" + punchingGoblin.ownerConnectionId.ToString() + " AFTER nextPunchTime. The punch will count!");
                punchBoxScript.nextPunchTime = Time.time + punchBoxScript.nextPunchRate;
                //myParentScript.HurtBoxCollision(collision.transform.parent.GetComponent<GoblinScript>());
                myParentScript.HurtBoxCollision(punchingGoblin);
            }
            else
            {
                Debug.Log("OnTriggerEnter2D: Hurtbox on:" + this.transform.parent.name + ":" + this.myParentScript.ownerConnectionId.ToString() + " collided with punchbox from " + collision.transform.parent.name + ":" + punchingGoblin.ownerConnectionId.ToString() + " BEFORE nextPunchTime");
            }
            
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
