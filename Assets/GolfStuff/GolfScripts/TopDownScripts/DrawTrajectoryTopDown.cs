using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawTrajectoryTopDown : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] GameObject _landingTargetObject;
    
    [SerializeField] [Range(3, 100)] private int _lineSegmentCount = 20;
    public List<Vector3> _linePoints = new List<Vector3>();

    [SerializeField] private LineRenderer _lineRendererShadow;
    public List<Vector3> _lineShadowPoints = new List<Vector3>();

    [Header("Trajacetory stuff")]
    public float numberOfSteps = 3;
    public float stepCountModifier = 0.5f;

    [Header("Collision Detection Stuff")]
    [SerializeField] LayerMask _environmentObstacleLayer;

    [Header("Material Stuff")]
    [SerializeField] Material _trajectoryLineMaterial;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdateTrajectory(Vector3[] trajectoryPoints, GolfBallTopDown ball, string clubType, float hitDistance)
    {

        if (trajectoryPoints.Length < 2)
            return;
        //if(clubType != "putter")
        //    Debug.Log("UpdateTrajectory: Trajectory points: " + trajectoryPoints[0].ToString() + " : " + trajectoryPoints[1].ToString() + " : " + trajectoryPoints[2].ToString() + " club type is: " + clubType + " and hit distance is: " + hitDistance.ToString());
        _linePoints.Clear();
        _lineShadowPoints.Clear();

        


        if (clubType == "putter")
        {
            _linePoints.Add(trajectoryPoints[0]);
            _lineShadowPoints.Add(trajectoryPoints[0]);
            _linePoints.Add(trajectoryPoints[1]);
            _lineShadowPoints.Add(trajectoryPoints[1]);
        }
        else
        {
            // Get the number of points for the trajectory
            //int numberOfPoints = GetNumberOfPointsForLine(hitDistance);
            _lineSegmentCount = GetNumberOfPointsForLine(hitDistance);
            //Debug.Log("UpdateTrajectory: line segment count will be: " + _lineSegmentCount.ToString());
            //_lineSegmentCount = 100;

            stepCountModifier = 1f / _lineSegmentCount;
            //Debug.Log("UpdateTrajectory: stepCountModifier: " + stepCountModifier.ToString() + " number of line segments: " + _lineSegmentCount.ToString() + " 1 / _lineSegmentCount" + (1f/ _lineSegmentCount).ToString());
            float stepCount = 0f;
            //Vector3 prevPos = Vector3.zero;
            Vector3 prevPos = trajectoryPoints[0];
            Vector3 newPos = Vector3.zero;

            for (int i = 0; i < _lineSegmentCount + 1; i++)
            {
                //Debug.Log("UpdateTrajectory: loop number: " + i.ToString());
                stepCount = stepCountModifier * i;

                Vector3 m1 = Vector3.Lerp(trajectoryPoints[0], trajectoryPoints[1], stepCount);
                Vector3 m2 = Vector3.Lerp(trajectoryPoints[1], trajectoryPoints[2], stepCount);
                newPos = Vector3.Lerp(m1, m2, stepCount);
                float newBallHeight = ball.GetBallHeightYValue(newPos.z);
                // with new position calculated, and the ball's height at that position calculated, check if the movement from the previous position to the new position would be blocked by an environment obstalce (like a tree...)
                bool willBallHitObject = false;
                RaycastHit2D[] obstaclesHit = EnvironmentObstaclesHit(prevPos, newPos, ball);
                if (obstaclesHit.Length > 0 && !(ball.MyPlayer.UsedPowerupThisTurn && ball.MyPlayer.UsedPowerUpType == "drill")) // don't make the collision checks if the player used their drill power up
                {
                    // Every object along the shadow path is in obstaclesHit. Now, check to see if any of those have a height value that is higher than the balls height. If so, add to list of objects the ball will hit?
                    List<RaycastHit2D> environmentObstaclesHigherThanBall = EnvironmentObstaclesHigherThanBall(obstaclesHit, ball, prevPos, newPos);
                    if (environmentObstaclesHigherThanBall.Count > 0)
                    {
                        Vector3 nearestCollisionPoint = NearestCollisionPoint(environmentObstaclesHigherThanBall, prevPos);
                        // Get an approxomation of the new z value for the ball height by averaging the z values of the new position and the prev position. Won't be accruate but whatever just need something really
                        //nearestCollisionPoint.z = (prevPos.z + newPos.z) / 2f;
                        // Change to get the z value as a proportion of the difference in z value between newPos and prevPos, based on distance between newPos and prevPos, and the distance between prevPos and the RaycastHit2D hitPoint
                        float newZValue = HeightOfBallAtCollisionPoint(prevPos, newPos, nearestCollisionPoint);
                        nearestCollisionPoint.z = newZValue;
                        // re-calculate the height of the ball at the new end point
                        newBallHeight = ball.GetBallHeightYValue(nearestCollisionPoint.z);



                        newPos = nearestCollisionPoint;
                        willBallHitObject = true;
                    }
                }

                // save the new position before it is modified so it can be used in obstalce collision calculations in the next iteration
                prevPos = newPos;

                // Add the position to the shadow points before the Y value is adjusted for the height of the ball
                _lineShadowPoints.Add(newPos);

                

                // something to simulate a "curve" in the arc? Uses the same method used to make the ball sprite further away from the shadow and get larger
                newPos.y += newBallHeight;

                _linePoints.Add(newPos);

                // If there was a collision with an obstacle, break out of the for loop
                if (willBallHitObject)
                    break;
                
            }
        }
        // Save the last point in the line
        Vector3 lastPoint = _linePoints[_linePoints.Count - 1];
        // Set were to draw the landing target
        _landingTargetObject.transform.position = (Vector2)lastPoint; // casting as a vector2 so the landing point is always at a z value of zero?
        // Get the direction of the last two points
        //Vector3 dir = _linePoints[_linePoints.Count - 2] - lastPont;
        //Vector3 newLastPoint = lastPont + (dir.normalized * (ball.pixelUnit * 4));
        // Set the last point in the line to the new last point
        

        Vector3 newLastPoint = GetNewLastPoint(_linePoints, ball.pixelUnit * 4f);
        _linePoints[_linePoints.Count - 1] = newLastPoint;
        newLastPoint = GetNewLastPoint(_lineShadowPoints, ball.pixelUnit * 4f);
        _lineShadowPoints[_lineShadowPoints.Count - 1] = newLastPoint;

        // Get the "tiling value" for the dashed line material
        // got the material from: https://www.youtube.com/watch?v=yEMM0sYCWoI
        _trajectoryLineMaterial.SetFloat("_Tiling", GetMaterialTilingValue(_linePoints[0], lastPoint));


        _lineRenderer.positionCount = _linePoints.Count;
        _lineRenderer.SetPositions(_linePoints.ToArray());

        _lineRendererShadow.positionCount = _lineShadowPoints.Count;
        _lineRendererShadow.SetPositions(_lineShadowPoints.ToArray());

        

        //_lineRendererShadow.SetPosition(0, trajectoryPoints[0]);
        //_lineRendererShadow.SetPosition(1, trajectoryPoints[2]);
    }
    RaycastHit2D[] EnvironmentObstaclesHit(Vector3 prevPos, Vector3 newPos, GolfBallTopDown ball)
    {
        return Physics2D.CircleCastAll(prevPos, ball.MyColliderRadius, newPos - prevPos, Vector2.Distance(prevPos, newPos), _environmentObstacleLayer);
    }
    List<RaycastHit2D> EnvironmentObstaclesHigherThanBall(RaycastHit2D[] obstaclesHit, GolfBallTopDown ball, Vector3 prevPos, Vector3 newPos)
    {
        List<RaycastHit2D> environmentObstaclesHigherThanBall = new List<RaycastHit2D>();

        for (int i = 0; i < obstaclesHit.Length; i++)
        {
            // Get the height of the ball based on the collision point of the raycasthit2d
            float ballHeightAtCollision = HeightOfBallAtCollisionPoint(prevPos, newPos, obstaclesHit[i].point);
            float ballHeightInUnityUnits = ball.GetBallHeightYValue(ballHeightAtCollision);
            // Check if the ball will hit the object or not
            EnvironmentObstacleTopDown obstacleHitScript = obstaclesHit[i].transform.GetComponent<EnvironmentObstacleTopDown>();
            Debug.Log("EnvironmentObstaclesHigherThanBall: hit object: " + obstaclesHit[i].transform.name);
            if (ball.DoesBallHitObject(obstacleHitScript.HeightInUnityUnits, ballHeightInUnityUnits, obstacleHitScript.StartHeight))
            {
                //Debug.Log("EnvironmentObstaclesHigherThanBall: Found object heigher than the ball: " + obstaclesHit[i].transform.gameObject.name);
                environmentObstaclesHigherThanBall.Add(obstaclesHit[i]);
            }
        }
        //Debug.Log("EnvironmentObstaclesHigherThanBall: returning environmentObstaclesHigherThanBall with a count of: " + environmentObstaclesHigherThanBall.Count.ToString());
        return environmentObstaclesHigherThanBall;
    }
    Vector2 NearestCollisionPoint(List<RaycastHit2D> environmentObstaclesHigherThanBall, Vector2 prevPos)
    {
        Vector2 nearestCollisionPoint = Vector2.zero;
        float nearestDist = 0f; // set to an arbitrarily high number so the first 
        for (int i = 0; i < environmentObstaclesHigherThanBall.Count; i++)
        {
            float newDist = Vector2.Distance(prevPos, environmentObstaclesHigherThanBall[i].point);
            //Debug.Log("NearestCollisionPoint: The collision point for RaycastHit2d number: " + i.ToString() + " is at " + environmentObstaclesHigherThanBall[i].point.ToString() + " and the distance from the ray origin point is: " + environmentObstaclesHigherThanBall[i].distance.ToString());
            if (i == 0)
            {
                nearestCollisionPoint = environmentObstaclesHigherThanBall[i].point;
                nearestDist = newDist;
            }
            else if (newDist < nearestDist)
            {
                
            }   
        }
        //Debug.Log("NearestCollisionPoint: returning: " + nearestCollisionPoint.ToString());
        return nearestCollisionPoint;
    }
    float HeightOfBallAtCollisionPoint(Vector3 prevPos, Vector3 newPos, Vector2 collisionPos)
    {
        float newHeight = 0f;

        float prevToNewDist = Vector2.Distance(prevPos, newPos);
        float prevToCollisionDist = Vector2.Distance(prevPos, collisionPos);
        float proportionOfDistTraveled = prevToCollisionDist / prevToNewDist;
        float diffInZ = newPos.z - prevPos.z;
        float proportionOfZtoUse = diffInZ * proportionOfDistTraveled;
        newHeight = prevPos.z + proportionOfZtoUse;

        return newHeight;
    }
    int GetNumberOfPointsForLine(float hitDistance)
    {
        int numberOfPoints = 10;
        if (hitDistance <= 20)
            return numberOfPoints;

        numberOfPoints += (int)((hitDistance - 20) / 2);
        //Debug.Log("GetNumberOfPointsForLine: The number of points for the line segment will be: " + numberOfPoints.ToString());

        return numberOfPoints;
    }
    public void SetLineWidth(float newWidth)
    {
        //Debug.Log("SetLineWidth: setting width to: " + newWidth.ToString());
        _lineRenderer.startWidth = newWidth;
        _lineRenderer.endWidth = newWidth;
        _lineRendererShadow.startWidth = newWidth;
        _lineRendererShadow.endWidth = newWidth;
    }
    Vector3 GetNewLastPoint(List<Vector3> linePoints, float offset)
    {
        //Debug.Log("GetNewLastPoint: number of line points: " + linePoints.Count.ToString());
        Vector3 lastPoint = linePoints[linePoints.Count - 1];

        Vector3 dir = linePoints[_linePoints.Count - 2] - lastPoint;
        Vector3 newLastPoint = lastPoint + (dir.normalized * offset);


        return newLastPoint;
    }
    float GetMaterialTilingValue(Vector3 startPoint, Vector3 endPoint)
    {
        return Vector2.Distance(startPoint, endPoint);
    }
}

