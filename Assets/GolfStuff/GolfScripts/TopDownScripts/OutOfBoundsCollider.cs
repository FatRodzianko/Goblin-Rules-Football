using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBoundsCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "GolfLandingTarget")
        {
            Debug.Log("OutOfBoundsCollider: GolfLandingTarget has exited the camera bounding box. ");
        }
        Debug.Log("OutOfBoundsCollider: Something  has exited the camera bounding box. Something name: " + collision.gameObject.name + " something's collision tag: " + collision.tag);
    }

}
