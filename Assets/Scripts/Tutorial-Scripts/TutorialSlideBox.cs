using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSlideBox : MonoBehaviour
{
    TutorialGoblinScript myParentScript;
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
        if (collision.tag == "Goblin")
        {
            Debug.Log("OnTriggerEnter2D: SlideBox collided with Goblin from: " + collision.transform.name);
            myParentScript.SlideBoxCollision(collision.transform.GetComponent<TutorialGoblinScript>());
        }
    }
}
