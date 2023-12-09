using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MiniGolfWalls : MonoBehaviour
{
    [SerializeField] TilemapCollider2D _myCollider;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "golfBall")
        {
            Vector2 collisionPoint = collision.contacts[0].point;
            Vector2 collisionNormal = collision.contacts[0].normal;

            GolfBallTopDown golfBallScript = collision.transform.GetComponent<GolfBallTopDown>();

            if (!golfBallScript.isRolling)
                return;
            if (golfBallScript.BouncedOffObstacle)
                return;
            
            Vector3 ballPos = golfBallScript.transform.position;

            Debug.Log("MiniGolfWalls: OnCollisionEnter2D: ball position: " + ballPos.ToString("0.000000") + " contact point: " + collisionPoint.ToString("0.000000") + " collision normal: " + collisionNormal.ToString("0.000000"));

            //golfBallScript.HitEnvironmentObstacle(0.1f, 0f, false, collisionPoint, ballPos);
            golfBallScript.BallHitMiniGolfWall(collisionNormal, ballPos);
        }
    }
}
