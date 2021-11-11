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
            myParentScript.SlideBoxCollision(collision.transform.GetComponent<GoblinScript>());
        }
    }
}
