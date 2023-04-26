using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatueCollision : MonoBehaviour
{
    [SerializeField] Statue _myStatue;
    [Header("Bounce Sounds")]
    [SerializeField] string _softBounceSoundType;
    [SerializeField] string _hardBounceSoundType;

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

        GolfBallTopDown golfBallScript = collision.GetComponent<GolfBallTopDown>();

        // Get the height of the ball:
        float ballZ = golfBallScript.transform.position.z;
        float ballHeightInUnityUnits = golfBallScript.GetBallHeightYValue(ballZ);

        // Check to see if the ball will hit the statue or not:
        if (!golfBallScript.DoesBallHitObject(_myStatue.HeightInUnityUnits, ballHeightInUnityUnits))
        {
            Debug.Log("StatueCollision.cs: Ball DOES NOT hit statue. Ball height: " + ballHeightInUnityUnits.ToString() + " and statue height: " + _myStatue.HeightInUnityUnits.ToString());
            return;
        }
        Debug.Log("StatueCollision.cs: Ball DOES hit statue. Ball height: " + ballHeightInUnityUnits.ToString() + " and statue height: " + _myStatue.HeightInUnityUnits.ToString());
    }
}
