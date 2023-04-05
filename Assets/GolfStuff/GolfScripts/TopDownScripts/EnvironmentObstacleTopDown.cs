using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentObstacleTopDown : MonoBehaviour
{
    [SerializeField] public ScriptableObstacle myScriptableObject; // scriptable object for the obstacle. Used to store the prefab of the obstacle for when the tilemapmanager needs to save/load new holes
    [SerializeField] public float HeightInUnityUnits; // get the number of pixels high the sprite is, then divide by 16 (or just multiple by the pixel unit, 0.0625f)
    [SerializeField] bool _isHoleFlag = false;
    // have a unity for "hardness" of the object? Balls bounce far from hard objects, don't bounce as much off soft objects? Pass that value to golfBallScript.HitEnvironmentObstacle to determine how far to bounce off the object???
    [SerializeField] public bool SoftBounce = false;
    [SerializeField] float _minHeightForSoftBounce = 0f; // for things like trees. Make it a hard bounce if the ball hits low (as in, the ball hit the tree trunk) and make it a soft bounce if the ball hits high (hits the tree leaves)
    [SerializeField] Collider2D _myCollider;

    [Header("Bounce Modifier Stuff")]
    [SerializeField] float _bounceModifier = 1.0f;
    [SerializeField] bool _gradualBounceModifier = false;
    [SerializeField] bool _immediateBounceModifier = false;
    [SerializeField] float _minHeightForImmediateBounceModifier = 0f;
    [SerializeField] float _minBounceModifier = 1.0f;
    [SerializeField] float _maxBounceModifier = 1.0f;

    [Header("Bounce Sounds")]
    [SerializeField] ScriptableBallSounds _ballSounds;
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
        if (collision.tag == "golfBall")
        {
            GolfBallTopDown golfBallScript = collision.GetComponent<GolfBallTopDown>();

            // only calculate collisions for balls the client owns...
            if (!golfBallScript.IsOwner)
                return;

            if (_isHoleFlag && golfBallScript.isRolling)
                return;
            if (golfBallScript.IsInHole)
                return;
            if (golfBallScript.LocalIsInHole)
                return;

            float ballZ = golfBallScript.transform.position.z;
            float ballHeightInUnityUnits = golfBallScript.GetBallHeightYValue(ballZ);
            /*Debug.Log("EnvironmentObstacleTopDown: collided with a golf ball: " + collision.name + ". My height in unity units is: " + _heightInUnityUnits.ToString() + " The z value of the ball is: " + ballZ.ToString() + " and its height value is: " + ballHeightInUnityUnits.ToString());
            if (ballHeightInUnityUnits < _heightInUnityUnits)
            {
                Debug.Log("EnvironmentObstacleTopDown: Ball below height of object. Ball should bounce back.");
            }
            else
            {
                Debug.Log("EnvironmentObstacleTopDown: Ball ABOVE height of object. Ball should continue flight.");
            }*/

            bool isSoftBounce = SoftBounce;
            string soundTypeToUse = _hardBounceSoundType;
            if (SoftBounce && _minHeightForSoftBounce > 0f)
            {
                if (ballHeightInUnityUnits < _minHeightForSoftBounce)
                {
                    isSoftBounce = false;
                }

                else
                {
                    isSoftBounce = true;
                    soundTypeToUse = _softBounceSoundType;
                }
                    
            }
            float newBounceModifier = GetBounceModifier(ballHeightInUnityUnits);
            Debug.Log("EnvironmentObstacleTopDown: new bounce modifier: " + newBounceModifier);
            // Make a call to the ball here. Give it the height of the ball in Unity Units and the height of the obstalce in Unity Units. The ball will then decide if it should continue flying, or bounce back
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
                golfBallScript.HitEnvironmentObstacle(HeightInUnityUnits, ballHeightInUnityUnits, _isHoleFlag, closestPoint, ballPos, isSoftBounce, newBounceModifier, soundTypeToUse);
            }
            else
            {
                Debug.Log("EnvironmentObstacleTopDown: ball is NOT rolling."); ;
                golfBallScript.HitEnvironmentObstacle(HeightInUnityUnits, ballHeightInUnityUnits, _isHoleFlag, Vector2.zero, Vector2.zero, isSoftBounce, newBounceModifier, soundTypeToUse) ;
            }

            /*Vector3 ballPos = golfBallScript.transform.position;
            Vector2 closestPoint = this.GetComponent<Collider2D>().ClosestPoint(ballPos);
            golfBallScript.HitEnvironmentObstacle(HeightInUnityUnits, ballHeightInUnityUnits, _isHoleFlag, closestPoint, ballPos);
            Debug.Log("EnvironmentObstacleTopDown: ball is rolling. Ball position: " + ballPos.ToString("0.00000") + " closest point from collider: " + closestPoint.ToString("0.00000"));*/
            //Debug.Break();
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_isHoleFlag)
            return;
        if (collision.tag == "golfBall")
        {
            GolfBallTopDown golfBallScript = collision.GetComponent<GolfBallTopDown>();
            if (!golfBallScript.IsOwner || golfBallScript.IsInHole || golfBallScript.isRolling || golfBallScript.isHit || golfBallScript.isBouncing)
                return;
            Debug.Log("EnvironmentObstacleTopDown: OnTriggerStay2D: moving ball to prevent issues?");

            Vector2 closestExitPoint = this._myCollider.ClosestPoint(collision.transform.position);
            Vector2 directionToPlayer = ((Vector2)golfBallScript.MyPlayer.transform.position - closestExitPoint).normalized;

            Vector2 newBallPoint = closestExitPoint + (directionToPlayer * (golfBallScript.MyColliderRadius * 4f));
            Debug.Log("EnvironmentObstacleTopDown: OnTriggerStay2D: moving ball to new point of: " + newBallPoint.ToString());
            collision.transform.position = newBallPoint;

            //_myCollider.enabled = false;

        }
    }
    /*private void OnTriggerExit2D(Collider2D collision)
    {
        if (_isHoleFlag)
            return;
        if (collision.tag == "golfBall")
        {
            Debug.Log("EnvironmentObstacleTopDown: OnTriggerExit2D: ball is leaving collider. Re-enabling the collider?");

        }
    }*/
    float GetBounceModifier(float ballHeightInUnityUnits)
    {
        if (!_gradualBounceModifier && !_immediateBounceModifier)
            return _bounceModifier;

        if (ballHeightInUnityUnits > HeightInUnityUnits)
            return _bounceModifier;

        float newModifier = _bounceModifier;

        if (_immediateBounceModifier)
        {
            if (ballHeightInUnityUnits > _minHeightForImmediateBounceModifier)
                return _minBounceModifier;
            else
                return _maxBounceModifier;
        }
        if (_gradualBounceModifier)
        {
            float modifierRange = _maxBounceModifier - _minBounceModifier;
            float heightPercentrage = ballHeightInUnityUnits / HeightInUnityUnits;
            float heightPercentageModifier = (modifierRange * heightPercentrage) + _minBounceModifier;
            Debug.Log("GetBounceModifier: Gradual bounce modifier. Ball height: " + ballHeightInUnityUnits.ToString() + " modifier range: " + modifierRange.ToString() + " height percentage ball was on the obstacle: " + heightPercentrage.ToString() + " height percentage modifier: " + heightPercentageModifier.ToString() + " and new bounce modifier: " + newModifier.ToString());
            return heightPercentageModifier;
        }

        return newModifier;
    }
}
