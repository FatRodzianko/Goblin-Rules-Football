using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchBoxScript : MonoBehaviour
{
    public float nextPunchTime = 0f;
    public float nextPunchRate = 0.25f;
    public Collider2D lastHurBox;
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
        if (collision.tag == "hurtbox")
        {

            /*if (Time.time > nextPunchTime)
            {
                if (collision.transform.parent.GetComponent<GoblinScript>().isGoblinKnockedOut)
                {
                    Debug.Log("OnTriggerEnter2D: PunchBoxScript.cs: Was the goblin: " + collision.transform.parent.name + " knocked out? " + collision.transform.parent.GetComponent<GoblinScript>().isGoblinKnockedOut.ToString());
                    return;
                }
                else
                {
                    Debug.Log("OnTriggerEnter2D: PunchBoxScript.cs: Was the goblin: " + collision.transform.parent.name + " knocked out? " + collision.transform.parent.GetComponent<GoblinScript>().isGoblinKnockedOut.ToString());
                    nextPunchTime = Time.time + nextPunchRate;
                }
                    
                
            }*/
          
            /*if (lastHurBox != null)
            {
                if (lastHurBox == collision || Time.time < nextPunchTime || collision.transform.parent.GetComponent<GoblinScript>().isGoblinKnockedOut || collision.transform.parent.GetComponent<GoblinScript>().health == 0)
                    return;
                else
                {
                    lastHurBox = collision;
                    nextPunchTime = Time.time + nextPunchRate;
                }
            }
            else
            {
                lastHurBox = collision;
            }*/
        }
    }
}
