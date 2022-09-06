using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHurtBox : MonoBehaviour
{
    TutorialGoblinScript myParentScript;
    public Collider2D lastPunchBox;
    public float nextPunchTime;
    // Start is called before the first frame update
    void Start()
    {
        myParentScript = this.transform.parent.GetComponent<TutorialGoblinScript>();
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
            TutorialGoblinScript punchingGoblin = collision.transform.parent.GetComponent<TutorialGoblinScript>();
            if (Time.time > punchBoxScript.nextPunchTime && !this.myParentScript.isGoblinKnockedOut && this.myParentScript.isGoblinGrey != punchingGoblin.isGoblinGrey)
            {
                //Debug.Log("OnTriggerEnter2D: Hurtbox on " + this.transform.parent.name + ":" + this.myParentScript.ownerConnectionId.ToString() + " collided with punchbox from: " + collision.transform.parent.name + ":" + punchingGoblin.ownerConnectionId.ToString() + " AFTER nextPunchTime. The punch will count!");
                punchBoxScript.nextPunchTime = Time.time + punchBoxScript.nextPunchRate;
                //myParentScript.HurtBoxCollision(collision.transform.parent.GetComponent<GoblinScript>());
                myParentScript.HurtBoxCollision(punchingGoblin);
            }
            else
            {
                //Debug.Log("OnTriggerEnter2D: Hurtbox on:" + this.transform.parent.name + ":" + this.myParentScript.ownerConnectionId.ToString() + " collided with punchbox from " + collision.transform.parent.name + ":" + punchingGoblin.ownerConnectionId.ToString() + " BEFORE nextPunchTime");
            }

        }
    }
}
