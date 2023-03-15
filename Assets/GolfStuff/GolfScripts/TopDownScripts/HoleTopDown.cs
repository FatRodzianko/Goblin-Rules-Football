using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleTopDown : MonoBehaviour
{
    [SerializeField] CircleCollider2D _myCircleCollider;
    public float MyCircleRadius;
    public float SpriteRadius;
    // Start is called before the first frame update
    void Start()
    {
        MyCircleRadius = _myCircleCollider.radius;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "golfBall")
        {
            Debug.Log("HoleTopDown: collided with a golf ball: " + collision.name);
            GolfBallTopDown golfBallScript = collision.GetComponent<GolfBallTopDown>();

            if (!golfBallScript.IsOwner)
                return;

            if (golfBallScript.isRolling)
                golfBallScript.BallRolledIntoHole(this);

            // checks for isBouncong/isHit to check balls height. If low enough, count it as in as though it was a "dunk" shot
            // also check height to see if ball will hit the flag and bounce off
        }
    }
}
