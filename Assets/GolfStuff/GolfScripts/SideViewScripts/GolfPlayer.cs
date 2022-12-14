using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfPlayer : MonoBehaviour
{
    [SerializeField] GolfBall myBall;
    [SerializeField] Projection _projection;
    [SerializeField] private GameObject golfBallPrefab;
    [SerializeField] DrawTrajectory drawTrajectory;

    public Vector2 prevAngle;
    public float prevPower;
    public float prevSpin;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            myBall.HitBall(myBall.angle, myBall.power, myBall.spin);
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            myBall.ResetPosition(true);
        }
        if (!myBall.isHit && (myBall.angle != prevAngle || myBall.power != prevPower || myBall.spin != prevSpin))
        {
            prevAngle = myBall.angle;
            prevPower = myBall.power;
            prevSpin = myBall.spin;
            _projection.SimulateTrajectory(myBall, myBall.transform.position, (myBall.angle * myBall.power));
        }       
    }
    private void FixedUpdate()
    {
        /*if (!myBall.isHit)
        {
            //Vector3 forceV = (new Vector3(myBall.angle.x, myBall.angle.y, myBall.angle.z)) * myBall.power;
            drawTrajectory.UpdateTrajectory((myBall.angle.normalized * myBall.power), myBall.rb, myBall.transform.position);
            //drawTrajectory.UpdateTrajectory(forceV, myBall.rb, myBall.transform.position);
        }*/
        
    }
}
