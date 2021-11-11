using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour
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
        if (collision.tag == "punchbox")
        {
            Debug.Log("OnTriggerEnter2D: Hurtbox collided with punchbox from: " + collision.transform.parent.name);
            myParentScript.HurtBoxCollision(collision.transform.parent.GetComponent<GoblinScript>());
        }
    }
}
