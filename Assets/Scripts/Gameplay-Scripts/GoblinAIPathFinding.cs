using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinAIPathFinding : MonoBehaviour
{
    Rigidbody2D myRigidBody;
    Vector3 targetPosition;
    Transform targetTransform;

    //Avoid direction stuff?
    [SerializeField] LayerMask goblinLayerMask;
    [SerializeField] LayerMask obstacleLayerMask;
    public float avoidDirectionMultiplier = 1f;
    public float avoidObjectMultiplier = 20f;
    public float avoidForceProximityMultiplier = 100;
    public float avoidObjectDirectionFallOffSpeed = 20f;
    private Vector2 lastPos = Vector2.zero;
    private Vector2 avoidObjectDirection = Vector2.zero;
    public float avoidObjectsScanDistance = 2f;
    Vector2 currentDirection;

    public GoblinScript goblinTarget;
    public bool isTargetingAGoblin = false;


    // Start is called before the first frame update
    void Start()
    {
        //seeker = GetComponent<Seeker>();
        myRigidBody = this.transform.GetComponent<Rigidbody2D>();

        //seeker.StartPath(myRigidBody.position, target.position, OnPathComplete);
        //InvokeRepeating("UpdatePath", 0f, 0.5f);

    }

    // Update is called once per frame
    void Update()
    {
        

    }
    private void FixedUpdate()
    {
        /*if (path == null)
            return;
        if (currentWayPoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }
        
        if (!reachedEndOfPath)
        {
            //Vector2 myPosition = this.transform.position;
            Vector2 direction = ((Vector2)path.vectorPath[currentWayPoint] - myRigidBody.position).normalized;
            Vector2 newPosition = myRigidBody.worldCenterOfMass + direction * speed * Time.fixedDeltaTime;
            Vector2 newPositon2 = myRigidBody.position + direction * speed * Time.fixedDeltaTime;
            Debug.Log("NewPosition: " + newPosition.ToString() + " NewPosition2: " + newPositon2.ToString() + " WorldCenterOfMass: " + myRigidBody.worldCenterOfMass.ToString() + " Direction: " + direction.ToString() + " RigidBody.Position: " + myRigidBody.position + " Speed* Time.fixedDeltaTime: " + (speed * Time.fixedDeltaTime).ToString());
            //myRigidBody.MovePosition(newPosition);
            //myRigidBody.AddForce(direction * speed * Time.fixedDeltaTime);
            Debug.Log("Position after MovePosition is called: " + myRigidBody.position.ToString());
            //myRigidBody.MovePosition(myRigidBody.position + direction * speed * Time.fixedDeltaTime);
            //Debug.Log("Moving position to: " + myRigidBody.position.ToString() + " " + myRigidBody.position.ToString());

            float distance = Vector2.Distance(myRigidBody.position, path.vectorPath[currentWayPoint]);

            if (distance < nextWayPointDistance)
            {
                currentWayPoint++;
            }
        }*/
        /*Vector3 myPosition = this.transform.position;
        Vector3 targetPosition = target.transform.position;
        if (Vector3.Distance(myPosition, targetPosition) > 1.5f)
        {
            Vector3 directionToMove = (targetPosition - myPosition).normalized;
            AIMoveTowardDirection(directionToMove);
        }*/
            
    }


    public Vector2 AIMoveTowardDirection(Vector3 directionToMoveTo, Vector3 target)
    {
        Vector2 direction = Vector2.ClampMagnitude(directionToMoveTo, 1);
        //targetTransform = target;
        targetPosition = target;


        // Set whether the goblin is moving. If the "direction" to the football is 0, then they shouldn't be moving?
        Vector2 moveDirection = Vector2.zero;
        moveDirection = directionToMoveTo;
        // Check for any goblins and whatever to avoid within 3.5f radius?
        moveDirection += GetAvoidDirection(3.5f);
        //moveDirection = moveDirection.normalized;
        //moveDirection = Vector2.ClampMagnitude(moveDirection, 1);

        //check for walls to avoid?
        var avoidObjectDirectionT = ScanForObjectsToAvoid().normalized * avoidObjectMultiplier;
        if (avoidObjectDirectionT.magnitude > 0)
            avoidObjectDirection = avoidObjectDirectionT;
        avoidObjectDirection = Vector2.Lerp(avoidObjectDirection, Vector2.zero, Time.deltaTime * avoidObjectDirectionFallOffSpeed);
        if (avoidObjectDirection.magnitude < 0.01f)
        {
            avoidObjectDirection = Vector2.zero;
        }
        moveDirection += avoidObjectDirection;
        moveDirection = moveDirection.normalized;
        moveDirection = Vector2.ClampMagnitude(moveDirection, 1);

        //Debug.Log("DirectorToMoveTo: " + direction.ToString() + " AvoidDirection: " + moveDirection.ToString());

        // Move the goblin toward the football
        //myRigidBody.MovePosition(myRigidBody.position + direction * speed * Time.fixedDeltaTime);
        //myRigidBody.MovePosition(myRigidBody.position + moveDirection * speed * Time.fixedDeltaTime);
        currentDirection = moveDirection;

        return moveDirection;

    }
    // stolen from https://chrishammond.ca/2017/10/10/boids-2d-unity-approach/
    private Vector2 GetAvoidDirection(float avoidRadius)
    {
        Vector2 oppositeDirection = Vector2.zero;
        //Vector3 colliderPosition = this.transform.position;
        //colliderPosition.y -= 1.25f;
        //colliderPosition.x -= 0.05f;
        Vector3 colliderPosition = myRigidBody.worldCenterOfMass;
        //use for loop instead of foreach, for loops are more optimized in Unity for memory management at high frame rate.
        //Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, 3.5f, goblinLayerMask);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(colliderPosition, 3.5f, goblinLayerMask);
        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                Vector3 positionOfColliderThatWasHit = colliders[i].transform.position;
                if (positionOfColliderThatWasHit == targetPosition)
                    continue;
                if (colliders[i].transform == this.transform)
                    continue;

                //Check to see if the goblin is targeting another goblin. If so, make sure they do not avoid the direction of their target goblin, as that is who the goblin is supposed to be running at
                if (isTargetingAGoblin)
                { 
                    GoblinScript collidersScript = colliders[i].GetComponent<GoblinScript>();
                    if (collidersScript == goblinTarget)
                        continue;
                }
                float distance = Vector2.Distance(colliderPosition, positionOfColliderThatWasHit);
                if (distance <= avoidRadius)
                {
                    Vector2 diff = colliderPosition - positionOfColliderThatWasHit;
                    diff.Normalize();
                    diff = diff / distance;
                    oppositeDirection += diff;
                }
            }
        }
        
        return oppositeDirection;
    }
    public Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
    public Vector2 DegreeToVector2(float degree)
    {
        return RadianToVector2(degree * Mathf.Deg2Rad);
    }
    // stolen from https://chrishammond.ca/2017/10/10/boids-2d-unity-approach/
    Vector2 ScanForObjectsToAvoid()
    {
        Vector2 avgDirection = Vector2.zero;
        Vector3 longestOpenPathDirection = Vector3.zero;
        Vector3 longestClosedPathDirection = Vector3.zero;
        bool hitSomething = false;
        float lastHitDistance = 0;
        // What to check distances from the goblin's foot collider because that is what is running into stuff. Set the raycast point with the offsets from the goblin's foot collider.
        //Vector3 colliderPosition = this.transform.position;
        //colliderPosition.y -= 1.25f;
        //colliderPosition.x -= 0.05f;
        Vector3 colliderPosition = myRigidBody.worldCenterOfMass;

        //get angle of current heading to do some trig
        float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;

        //we're doing 3 raycasts
        for (int t = 0; t < 3; t++)
        {
            RaycastHit2D raycastHit2DResult;                                                                                                                                    // Gizmos.DrawLine(this.transform.position + new Vector3(0, -avoidRadius / 2f, 0), this.transform.position + (new Vector3(0, -avoidRadius / 2f, 0) + new Vector3(facingDirection.x, facingDirection.y, 0) * avoidRadius));
            Vector2 currentRayDir = Vector2.zero;
            if (t == 0)
            {
                //raycast forward
                currentRayDir = currentDirection * (avoidObjectsScanDistance + 0.5f);
                //raycastHit2DResult = Physics2D.Raycast(this.transform.position, currentRayDir, avoidObjectsScanDistance + 0.5f, 1 << LayerMask.NameToLayer("sidelines"));
                raycastHit2DResult = Physics2D.Raycast(colliderPosition, currentRayDir, avoidObjectsScanDistance + 0.5f, obstacleLayerMask);
                //raycastHit2DResult = Physics2D.Raycast(colliderPosition, currentRayDir, avoidObjectsScanDistance + 0.5f, 1 << LayerMask.NameToLayer("pathFindingObstacles"));
                //Debug.DrawRay(this.transform.position, currentDirection * (avoidObjectsScanDistance + 0.5f), Color.blue, 0.1f);
                Debug.DrawRay(colliderPosition, currentDirection * (avoidObjectsScanDistance + 0.5f), Color.blue, 0.1f);
                /*if (raycastHit2DResult.collider != null)
                    Debug.Log("ScanForObjectsToAvoid: Hit straight ahead");*/
            }
            else if (t == 1)
            {
                //upward relative angle to direction of movement to be used for raycast.
                Vector2 dir = DegreeToVector2(75 + angle);
                dir = (currentDirection + dir).normalized * (avoidObjectsScanDistance + 0.2f);
                currentRayDir = dir;
                //raycastHit2DResult = Physics2D.Raycast(this.transform.position, dir, avoidObjectsScanDistance, 1 << LayerMask.NameToLayer("sidelines"));
                raycastHit2DResult = Physics2D.Raycast(colliderPosition, dir, avoidObjectsScanDistance + 0.2f, obstacleLayerMask);
                //raycastHit2DResult = Physics2D.Raycast(colliderPosition, dir, avoidObjectsScanDistance, 1 << LayerMask.NameToLayer("pathFindingObstacles"));
                //Debug.DrawRay(this.transform.position, dir, Color.red, 0.1f);
                Debug.DrawRay(colliderPosition, dir, Color.red, 0.1f);
                /*if (raycastHit2DResult.collider != null)
                    Debug.Log("ScanForObjectsToAvoid: Hit above");*/
            }
            else
            {
                //downward relative angle to direction of movement to be used for raycast.
                Vector2 dir = DegreeToVector2(-75 + angle);
                dir = (currentDirection + dir).normalized * (avoidObjectsScanDistance + 0.2f);
                currentRayDir = dir;
                //raycastHit2DResult = Physics2D.Raycast(this.transform.position, dir, avoidObjectsScanDistance + 1.0f, 1 << LayerMask.NameToLayer("sidelines"));
                raycastHit2DResult = Physics2D.Raycast(colliderPosition, dir, avoidObjectsScanDistance + 0.2f, obstacleLayerMask);
                //raycastHit2DResult = Physics2D.Raycast(colliderPosition, dir, avoidObjectsScanDistance, 1 << LayerMask.NameToLayer("pathFindingObstacles"));
                //Debug.DrawRay(this.transform.position, dir, Color.green, 0.1f);
                Debug.DrawRay(colliderPosition, dir, Color.green, 0.1f);
                /*if (raycastHit2DResult.collider != null)
                    Debug.Log("ScanForObjectsToAvoid: Hit below");*/
            }
            //check if we hit anything and if mutliple raycasts hit something, we want to find the one furthest away.
            if (raycastHit2DResult.collider != null)
            {
                hitSomething = true;
                //float dist = Vector2.Distance(this.transform.position, raycastHit2DResult.point);
                float dist = Vector2.Distance(colliderPosition, raycastHit2DResult.point);
                //if this point was further than other hit points, we might potentially head in this direction
                if (dist > lastHitDistance)
                {
                    lastHitDistance = dist;
                    longestClosedPathDirection = currentRayDir;
                }
            }
            //if the raycast didn't hit anything then that is currently longest open path
            else
            {
                longestOpenPathDirection = currentRayDir;
            }
        }
        //if no raycasts hit anything, we should return nothing and keep heading in current direction.
        if (!hitSomething)
        {
            return Vector2.zero;
        }
        //if all three directional raycasts hit something, we'll head towards direction with furthest distance between self and hit point.
        else if (longestOpenPathDirection == Vector3.zero)
        {
            return longestClosedPathDirection;
        }
        //if one of the raycasts were open but others hit something, we'll head in direction of ray that didn't hit anything.
        else
        {
            return longestOpenPathDirection;
        }
    }
}
