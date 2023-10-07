using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatueCollision : MonoBehaviour
{
    [SerializeField] public ScriptableObstacle myScriptableObject;
    [SerializeField] Statue _myStatue;
    [SerializeField] float _bounceModifier = 1f;

    [Header("Bounce Sounds")]
    [SerializeField] string _softBounceSoundType;
    [SerializeField] string _hardBounceSoundType;

    // Start is called before the first frame update
    void Start()
    {
        _softBounceSoundType = myScriptableObject.SoftBounceSoundType;
        _hardBounceSoundType = myScriptableObject.HardBounceSoundType;
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

        // only calculate collisions for balls the client owns...
        if (!golfBallScript.IsOwner)
            return;

        // Get the height of the ball:
        //float ballZ = golfBallScript.transform.position.z;
        float ballZ = golfBallScript.transform.position.z;
        float ballHeightInUnityUnits = golfBallScript.GetBallHeightYValue(ballZ);

        // Check to see if the ball will hit the statue or not:
        if (!golfBallScript.DoesBallHitObject(_myStatue.HeightInUnityUnits, ballHeightInUnityUnits))
        {
            Debug.Log("StatueCollision.cs: Ball DOES NOT hit statue. Ball height: " + ballHeightInUnityUnits.ToString() + " and statue height: " + _myStatue.HeightInUnityUnits.ToString());
            return;
        }
        Debug.Log("StatueCollision.cs: Ball DOES hit statue. Ball height: " + ballHeightInUnityUnits.ToString() + " and statue height: " + _myStatue.HeightInUnityUnits.ToString());

        // Check to see how fast the ball is going on collision. If it is going fast enough, the statue will break?
        
        if (golfBallScript.speedMetersPerSecond < 5f || _myStatue.IsBroken) // was 12.5f before. set to only 1 for testing breaking the statue
        {
            Debug.Log("StatueCollision.cs: Ball had a speed of: " + golfBallScript.speedMetersPerSecond + " at the time of collision. Ball will BOUNCE off the statue");
            if (golfBallScript.isRolling)
            {
                Vector3 centerOfCollider = this.GetComponent<Collider2D>().bounds.center;
                Vector3 ballPos = golfBallScript.transform.position;
                Vector2 closestPoint = this.GetComponent<Collider2D>().ClosestPoint(ballPos);
                Vector3 collisionDir = (centerOfCollider - ballPos).normalized;
                Vector3 collisionPoint = ballPos + (collisionDir * golfBallScript.MyColliderRadius) - (Vector3)(golfBallScript.movementDirection.normalized * golfBallScript.speedMetersPerSecond * Time.deltaTime);
                Debug.Log("EnvironmentObstacleTopDown: ball is rolling. Ball position: " + ballPos.ToString("0.00000") + " centerOfCollider: " + centerOfCollider.ToString() + " and the collision point: " + collisionPoint.ToString() + " closest point from collider: " + closestPoint.ToString("0.00000")); ;
                //golfBallScript.HitEnvironmentObstacle(HeightInUnityUnits, ballHeightInUnityUnits, _isHoleFlag, collisionPoint, centerOfCollider, this.GetComponent<Collider2D>().bounds.extents);
                //golfBallScript.HitEnvironmentObstacle(HeightInUnityUnits, ballHeightInUnityUnits, _isHoleFlag, closestPoint, centerOfCollider, this.GetComponent<Collider2D>().bounds.extents);
                golfBallScript.HitEnvironmentObstacle(_myStatue.HeightInUnityUnits, ballHeightInUnityUnits, false, closestPoint, ballPos, false, _bounceModifier, _hardBounceSoundType);
            }
            else
            {
                golfBallScript.HitEnvironmentObstacle(_myStatue.HeightInUnityUnits, ballHeightInUnityUnits, false, Vector2.zero, Vector2.zero, false, _bounceModifier, _hardBounceSoundType);
            }
            // Don't continue execution after the ball bounces?
            return;
        }
        Debug.Log("StatueCollision.cs: Ball had a speed of: " + golfBallScript.speedMetersPerSecond + " at the time of collision. Ball will BREAK the statue");
        BreakStatue();
        golfBallScript.BrokenStatuePenalty(_myStatue.StatueType);
    }
    void BreakStatue()
    {
        //Destroy(this.gameObject);
        this.transform.GetComponent<Statue>().BreakStatueAnimation();
    }
}
