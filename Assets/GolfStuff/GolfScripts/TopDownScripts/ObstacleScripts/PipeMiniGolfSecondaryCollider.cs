using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeMiniGolfSecondaryCollider : MonoBehaviour
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
        if (collision.tag != "golfBall")
            return;
        Debug.Log("PipeMiniGolfSecondaryCollider: OnTriggerEnter2D: This object's name is: " + this.name + " collisions name is: " + collision.transform.name);

        GolfBallTopDown golfBallScript = collision.GetComponent<GolfBallTopDown>();

        if (!golfBallScript.IsOwner)
            return;

        golfBallScript.BallOnPipeEntryHole = true;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag != "golfBall")
            return;
        Debug.Log("PipeMiniGolfSecondaryCollider: OnTriggerExit2D: This object's name is: " + this.name + " collisions name is: " + collision.transform.name);

        GolfBallTopDown golfBallScript = collision.GetComponent<GolfBallTopDown>();

        if (!golfBallScript.IsOwner)
            return;
        golfBallScript.BallOnPipeEntryHole = false;
    }
}
