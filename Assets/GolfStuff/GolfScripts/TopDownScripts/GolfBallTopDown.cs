using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfBallTopDown : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] CircleCollider2D myCollider;
    public float MyColliderRadius;

    [Header("Player Info")]
    public GolfPlayerTopDown myPlayer;

    [Header("Ball Sprite Info")]
    [SerializeField] SpriteRenderer ballObjectRenderer;
    [SerializeField] BallSpriteCollision _ballSpriteCollision;
    [SerializeField] Sprite groundSprite;
    [SerializeField] Sprite medSprite;
    [SerializeField] Sprite highSprite;
    [SerializeField] Sprite higherSprite;
    [SerializeField] Sprite highestSprite;
    [SerializeField] Sprite highesterSprite;
    [SerializeField] Sprite highestererSprite;

    [Header("Shadow")]
    [SerializeField] GameObject myShadow;
    [SerializeField] public GameObject myBallObject;
    public float pixelUnit = 0.0625f;

    [Header("Trail")]
    [SerializeField] TrailRenderer trail;


    public bool isHit = false;

    [Header("Hit Ball Info")]
    public Vector3[] hitBallPonts = new Vector3[3];
    public float hitBallCount = 0f;
    public float hitBallModifer = 0.5f; // changes based on the max height of the hit/arc?

    [Header("Trajectory Info")]
    public float maxHeight;
    public float flightTime;
    public float timeInAir = 0f;
    public int heightDirection = 0; // if ball is moving up in its trajectory, set to 1. If down, set to -1. If neither (on the ground) set to 0;
    public Vector2 movementDirection = Vector2.zero; // Current direction the ball is moving in X,Y. Can change if ball has to bounce off something?
    public Vector3 fallDirection = Vector3.zero; // Direction of the ball as it is falling/flying. Used to determine what direction to bounce the ball if it collides with something? After a hit, should be same as hitDirection. AFter a collision, can change to be different from hitDirection
    public float initialVelocity;
    public float launchAngle;
    public float launchTopSpin;
    public float launchDistance;
    public float speedMetersPerSecond;
    public float launchLeftOrRightSpin;

    [Header("Bounces")]
    public bool hasBouncedYet = false;
    public bool isBouncing = false;
    public int numberOfBouncesCompleted = 0;
    public int numberOfBouncesToDo = 0;
    public string bounceContactGroundMaterial;
    [SerializeField] LayerMask groundMask;
    public float minBounceHeight = 0.16f;
    public float defaultBounceHeightModifier = 0.4f;
    public float spinBounceDistanceModifier = 0.2f;
    public Vector2 originalHitDirection = Vector2.zero;
    public float twoBounceDistancesAgo;

    [Header("Rolling")]
    public bool isRolling = false;
    public bool HitApexOfHill = false;

    [Header("Ball State")]
    public bool IsInHole = false;


    [Header("Ground Material Info")]
    [SerializeField] public float greenRollSpeedModifier = 0.5f;
    [SerializeField] public float fairwayRollSpeedModifier = 1.0f;
    [SerializeField] public float roughRollSpeedModifier = 5f;
    [SerializeField] public float deepRoughRollSpeedModifier = 15f;
    public Vector2 groundSlopeDirection = Vector2.zero;
    public float slopeSpeedModifier = 0f;
    [SerializeField] LayerMask _golfHoleLayerMask;
    [SerializeField] Vector3Int _currentTileCell;

    // Start is called before the first frame update
    void Start()
    {
        isHit = false;
        MyColliderRadius = myCollider.radius;
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void FixedUpdate()
    {
        if (isHit || isBouncing)
        {
            MoveBallOnTrajectory();
            UpdateBallSpriteForHeight();
            if (!trail.enabled)
                trail.enabled = true;
        }
        else if (isRolling)
        {
            //Debug.Log("IsRolling: Roll direction before slope: " + movementDirection.ToString());
            movementDirection = GetRollDirection(movementDirection, groundSlopeDirection);
            //Debug.Log("IsRolling: Roll direction AFTER slope: " + movementDirection.ToString());
            //movementDirection = CalculateWindShiftForPutts(movementDirection);
            //Debug.Log("IsRolling: Roll direction AFTER WIND: " + movementDirection.ToString());
            RollBall(speedMetersPerSecond, movementDirection);

            if (!trail.enabled)
                trail.enabled = true;

            isRolling = WillBallRoll();
            if (!isRolling)
            {
                ResetBallAndPlayerAfterBallStoppedRolling();
            }
                
        }
        else
        {
            if (trail.enabled)
                trail.enabled = false;
        }
    }
    void MoveBallOnTrajectory()
    {
        // Move ball along its trajectory?
        if (hitBallCount < 1.0f)
        {
            hitBallCount += hitBallModifer * Time.fixedDeltaTime;
            timeInAir += Time.fixedDeltaTime;
            Vector3 m1 = Vector3.Lerp(hitBallPonts[0], hitBallPonts[1], hitBallCount);
            Vector3 m2 = Vector3.Lerp(hitBallPonts[1], hitBallPonts[2], hitBallCount);
            Vector3 newPos = Vector3.Lerp(m1, m2, hitBallCount);
            Vector3 oldPos = this.transform.position;

            fallDirection = newPos - oldPos;
            speedMetersPerSecond = CalculateCurrentSpeed(oldPos, newPos);
            //Debug.Log("Hit/Bouncing: speedMetersPerSecond: " + speedMetersPerSecond.ToString());

            this.transform.position = newPos;
            //rb.MovePosition(Vector3.Lerp(m1, m2, hitBallCount));
            if (hitBallCount >= 1.0f)
            {
                rb.MovePosition(hitBallPonts[2]);
                ResetBallInfo(true);
            }
            //Debug.Log("Height: " + this.transform.position.z.ToString());

        }
        else if (hitBallCount >= 1.0f)
        {
            rb.MovePosition(hitBallPonts[2]);
            ResetBallInfo(true);
        }
    }
    void UpdateBallSpriteForHeight()
    {
        float height = this.transform.position.z;
        float radius = pixelUnit * 2f;
        if (height < 1.0f)
            ballObjectRenderer.sprite = groundSprite;
        else if (height < 5f)
        {
            ballObjectRenderer.sprite = medSprite;
            radius = pixelUnit * 3f;
        }
        else if (height < 10f)
        {
            ballObjectRenderer.sprite = highSprite;
            radius = pixelUnit * 4f;
        }
        else if (height < 18f)
        {
            ballObjectRenderer.sprite = higherSprite;
            radius = pixelUnit * 5f;
        }
        else if (height < 27f)
        {
            ballObjectRenderer.sprite = highestSprite;
            radius = pixelUnit * 6f;
        }
        else if (height < 36f)
        {
            ballObjectRenderer.sprite = highesterSprite;
            radius = pixelUnit * 7f;
        }
        else
        {
            ballObjectRenderer.sprite = highestererSprite;
            radius = pixelUnit * 8f;
        }

        UpdateShadowPosition(height);
        UpdateTrailLine(ballObjectRenderer.sprite);
        // Update the sprite collider's radius for accurate sprite collision detection?
        _ballSpriteCollision.UpdateColliderRadius(radius);
    }
    public void UpdateShadowPosition(float ballZValue)
    {
        // Move the ball sprite up relative to the shadow object to give the illusion the ball is off the ground while still having the shadow sprite follow the trajectory path
        Vector3 ballObjectPos = myBallObject.transform.localPosition;
        /*float ballObjectY = 0f;
        if (ballZValue > pixelUnit)
        {
            ballObjectY = ballZValue / 6f;
            float mod = ballObjectY % pixelUnit; // 1 pixel = 0.0625 unity units. Getting the modulus of pixelUnits and then subtracting the remainder to make sure the BallObject never moves more than a whole pixel at a time. Removes some of the jittering that was happening without doing this
            ballObjectY -= mod;
            if (ballObjectY > 5.25f)
                ballObjectY = 5.25f;
        }
        ballObjectPos.y = ballObjectY;*/
        ballObjectPos.y = GetBallHeightYValue(ballZValue);
        myBallObject.transform.localPosition = ballObjectPos;
        //ps.transform.localPosition = ballObjectPos;
    }
    public float GetBallHeightYValue(float ballZValue)
    {
        float ballObjectY = 0f;
        if (ballZValue > pixelUnit)
        {
            ballObjectY = ballZValue / 4f; // was previous divided by 6f...
            float mod = ballObjectY % pixelUnit; // 1 pixel = 0.0625 unity units. Getting the modulus of pixelUnits and then subtracting the remainder to make sure the BallObject never moves more than a whole pixel at a time. Removes some of the jittering that was happening without doing this
            ballObjectY -= mod;
            /*if (ballObjectY > 5.25f)
                ballObjectY = 5.25f;*/
            if (ballObjectY > 12f)
                ballObjectY = 12f;
        }

        return ballObjectY;
    }
    void UpdateTrailLine(Sprite currentSprite)
    {
        //trail.startWidth = pixelUnit * 2f;
        if (currentSprite == highestererSprite)
            trail.startWidth = pixelUnit * 7f;
        else if (currentSprite == highesterSprite)
            trail.startWidth = pixelUnit * 6f;
        else if (currentSprite == highestSprite)
            trail.startWidth = pixelUnit * 5f;
        else if (currentSprite == higherSprite)
            trail.startWidth = pixelUnit * 4f;
        else if (currentSprite == highSprite)
            trail.startWidth = pixelUnit * 3f;
        else if (currentSprite == medSprite)
            trail.startWidth = pixelUnit * 2f;
        else
            trail.startWidth = pixelUnit * 1f;


        trail.endWidth = pixelUnit;
    }
    float CalculateCurrentSpeed(Vector2 start, Vector2 end)
    {
        float currentSpeed = 0f;
        // Get the distance travelled in last fixed update, then divid by fixed update to get meters/second?
        float dist = Vector2.Distance(start, end);
        currentSpeed = dist / Time.deltaTime;
        Debug.Log("CalculateCurrentSpeed: current speed: " + currentSpeed.ToString());
        return currentSpeed;
    }
    public void ResetBallInfo(bool checkForBounces)
    {
        isHit = false;
        hitBallCount = 0f;
        timeInAir = 0f;
        myShadow.transform.localPosition = Vector3.zero;
        ballObjectRenderer.sprite = groundSprite;
        if (checkForBounces)
        {
            // first, check if the ball hits directly into the hole
            if (DidBallLandInHole())
            {
                BallInHole();
            }
            else
            {
                GetBounces(launchDistance, launchAngle, launchTopSpin, movementDirection, maxHeight, launchLeftOrRightSpin);
            }
        }
    }
    public void ResetPosition()
    {
        isHit = false;
        isRolling = false;
        isBouncing = false;
        IsInHole = false;
        hitBallCount = 0f;
        timeInAir = 0f;
        this.transform.position = new Vector3(0f, 0f, 0f);
        myShadow.GetComponent<SpriteRenderer>().enabled = true;
        myBallObject.GetComponent<SpriteRenderer>().enabled = true;
    }
    public void HitBall(float hitDistance, float hitAngle, float hitTopSpin, Vector2 hitDirection, float hitLeftOrRightSpin = 0f, bool provideHeight = false, float hieghtProvided = 0f)
    {
        //CalculateHitTrajectory(hitDistance, hitAngle, hitTopSpin, hitDirection, Vector2.zero, 0f);
        if (provideHeight)
        {
            hitBallPonts = CalculateHitTrajectory(hitDistance, hitAngle, hitTopSpin, hitLeftOrRightSpin, hitDirection, WindManager.instance.WindDirection, WindManager.instance.WindPower,true, hieghtProvided, true);
        }
        else
        {
            hitBallPonts = CalculateHitTrajectory(hitDistance, hitAngle, hitTopSpin, hitLeftOrRightSpin, hitDirection, WindManager.instance.WindDirection, WindManager.instance.WindPower, false, 0f, true);
        }
        
        maxHeight = hitBallPonts[1].z / 2;
        initialVelocity = CalculateInitialVelocity(hitAngle, maxHeight);
        flightTime = CalculateFlightTime(initialVelocity, hitAngle);
        if (provideHeight)
            flightTime /= 2f;
        hitBallModifer = 1 / flightTime;

        movementDirection = hitDirection.normalized;
        launchAngle = hitAngle;
        isHit = true;
        //ps.Play();
    }
    public Vector3[] CalculateHitTrajectory(float hitDistance, float hitAngle, float hitTopSpin, float hitLeftOrRightSpin, Vector3 hitDirection, Vector2 windDirection, float windPower, bool isHeightIncluded = false, float heightToUse = 0f, bool calculateRainEffect = false)
    {
        Debug.Log("CalculateHitTrajectory: hitDistance: " + hitDistance.ToString() + " hitAngle: " + hitAngle.ToString());
        Vector3[] trajectoryPoints = new Vector3[3];
        //Debug.Log("CalculateHitTrajectory: hitAngle: " + hitAngle.ToString());
        // save the trajectory parameters
        movementDirection = hitDirection.normalized;
        launchAngle = hitAngle;
        launchTopSpin = hitTopSpin;
        launchDistance = hitDistance;
        launchLeftOrRightSpin = hitLeftOrRightSpin;

        Vector3 startPos = this.transform.position;
        trajectoryPoints[0] = startPos;
        // Calculate where the ball will land based on the hitDirection and hitDistance traveled by the ball
        // If there is rain, reduce the distance hit by a rain modifer
        if (calculateRainEffect)
        {
            if (RainManager.instance.IsRaining)
            {
                launchDistance *= RainManager.instance.RainHitModifier;
            }
        }
        // End point unmodified by wind
        Vector3 endPos = startPos + hitDirection.normalized * launchDistance;
        // Set the z value for the end to be 0. This is mainly to make sure bounces off obstacles don't have a messed up end z value
        endPos.z = 0f;
        // Will need to caclulate the effect of wind on end point. Wind's effect will be greater when ball is hit higher and lesser when ball is hit lower?


        // Save the end point
        trajectoryPoints[2] = endPos;

        // Calculate the "middle" point of the arc (also called the control point?)
        float controlX = ((endPos.x - startPos.x) / 2f) + startPos.x;
        float controlY = ((endPos.y - startPos.y) / 2f) + startPos.y;
        //float controlX = ((endPos.x - startPos.x) / 1.2f) + startPos.x; // dividing by 1.2 instead of 2 so the "control point" is skewed toward the end of the arc. Simulates a golf ball hit more?
        //float controlY = ((endPos.y - startPos.y) / 1.2f) + startPos.y;
        // The Z value of the control point will be the max height of the hit?
        float controlZ = heightToUse;
        

        // If the height to use wasn't provided, like for initial hits, then calculate it using some trig stuff
        if (!isHeightIncluded)
        {
            // The z value will be the "height" of the arc. It will be affected by the launch angle of the hit (hitAngle). Use some trig to calculate where it will be based on known values...
            var hitAngleRad = launchAngle * Mathf.Deg2Rad;
            var oppositeHitAngleRad = (90f - launchAngle) * Mathf.Deg2Rad;
            var rightAngle = 90f * Mathf.Deg2Rad;
            // https://math.stackexchange.com/questions/2532397/calculate-height-of-triangle-given-angle-and-base
            controlZ = ((launchDistance / 2) * Mathf.Sin(rightAngle) * Mathf.Sin(hitAngleRad)) / Mathf.Sin(oppositeHitAngleRad);
        }
        // Save the maximum height of the flight path
        //maxHeight = controlZ;
        controlZ *= 2; // double the controlZ value because the height of the flight path is always controlZ / 2
        trajectoryPoints[1] = new Vector3(controlX, controlY, controlZ);

        // Adjust the trajectory based on side spin. The trajectory should "curve" if there is side spin
        if (launchLeftOrRightSpin != 0)
        {
            Debug.Log("CalculateHitTrajectory: hitLeftOrRightSpin is not 0. It is: " + launchLeftOrRightSpin.ToString());
            Vector2 sideSpinShift = ShiftTrajectoryForSideSpin(launchLeftOrRightSpin,trajectoryPoints[0], trajectoryPoints[1], movementDirection, trajectoryPoints[1].z);
            Debug.Log("CalculateHitTrajectory: hitLeftOrRightSpin is not 0. the shift will be: " + sideSpinShift.ToString() + " from the original point of: " + trajectoryPoints[1].ToString());
            trajectoryPoints[1].x = sideSpinShift.x;
            trajectoryPoints[1].y = sideSpinShift.y;
        }

        // Wind?>??
        if (windDirection != Vector2.zero && windPower > 0)
        {
            Debug.Log("CalculateHitTrajectory: Wind dir/power is non-zero. Taking wind into accout. Wind dir: " + windDirection.ToString() + " wind power: " + windPower.ToString());
            trajectoryPoints[1] = CalculateWindShift(trajectoryPoints[1], windDirection, windPower, trajectoryPoints[1].z / 2f, true);
            trajectoryPoints[2] = CalculateWindShift(trajectoryPoints[2], windDirection, windPower, trajectoryPoints[1].z / 2f, false);
        }

        return trajectoryPoints;
    }
    public Vector3[] CalculatePutterTrajectoryPoints(float hitDistance, Vector3 hitDirection)
    {
        Vector3[] trajectoryPoints = new Vector3[2];

        Vector3 startPos = this.transform.position;
        trajectoryPoints[0] = startPos;
        trajectoryPoints[1] = hitDistance * hitDirection + startPos;

        return trajectoryPoints;
    }
    Vector3 CalculateWindShift(Vector3 trajectoryPoint, Vector2 windDirection, float windPower, float maxHeight, bool isMidPoint)
    {
        if (windDirection == Vector2.zero || windPower == 0)
            return trajectoryPoint;
        Vector3 windShift = windDirection.normalized * windPower * (maxHeight / 25);
        if (isMidPoint)
        {
            windShift /= 2f;
        }
        Debug.Log("CalculateWindShift: Original trajectory point: " + trajectoryPoint.ToString() + " wind shift amount: " + windShift.ToString() + " based on a wind direction of: " + windDirection.ToString() + " and wind power of: " + windPower.ToString() + " and a max height of: " + maxHeight.ToString());
        windShift += trajectoryPoint;
        return windShift;
    }

    /*void CalculateHitTrajectory(float hitDistance, float hitAngle, float hitTopSpin, Vector3 hitDirection, Vector2 windDirection, float windPower, bool isHeightIncluded = false, float heightToUse = 0f)
    {
        Debug.Log("CalculateHitTrajectory: hitAngle: " + hitAngle.ToString());
        // save the trajectory parameters
        movementDirection = hitDirection.normalized;
        launchAngle = hitAngle;
        launchTopSpin = hitTopSpin;
        launchDistance = hitDistance;

        Vector3 startPos = this.transform.position;
        hitBallPonts[0] = startPos;
        // Calculate where the ball will land based on the hitDirection and hitDistance traveled by the ball
        // End point unmodified by wind
        Vector3 endPos = startPos + hitDirection.normalized * hitDistance;
        // Will need to caclulate the effect of wind on end point. Wind's effect will be greater when ball is hit higher and lesser when ball is hit lower?

        // Save the end point
        hitBallPonts[2] = endPos;

        // Calculate the "middle" point of the arc (also called the control point?)
        float controlX = ((endPos.x - startPos.x) / 2f) + startPos.x;
        float controlY = ((endPos.y - startPos.y) / 2f) + startPos.y;
        //float controlX = ((endPos.x - startPos.x) / 1.2f) + startPos.x; // dividing by 1.2 instead of 2 so the "control point" is skewed toward the end of the arc. Simulates a golf ball hit more?
        //float controlY = ((endPos.y - startPos.y) / 1.2f) + startPos.y;

        float controlZ = heightToUse;
        // If the height to use wasn't provided, like for initial hits, then calculate it using some trig stuff
        if (!isHeightIncluded)
        {
            // The z value will be the "height" of the arc. It will be affected by the launch angle of the hit (hitAngle). Use some trig to calculate where it will be based on known values...
            var hitAngleRad = hitAngle * Mathf.Deg2Rad;
            var oppositeHitAngleRad = (90f - hitAngle) * Mathf.Deg2Rad;
            var rightAngle = 90f * Mathf.Deg2Rad;
            // https://math.stackexchange.com/questions/2532397/calculate-height-of-triangle-given-angle-and-base
            controlZ = ((hitDistance / 2) * Mathf.Sin(rightAngle) * Mathf.Sin(hitAngleRad)) / Mathf.Sin(oppositeHitAngleRad);
        }
        // Save the maximum height of the flight path
        maxHeight = controlZ;
        controlZ *= 2; // double the controlZ value because the height of the flight path is always controlZ / 2
        hitBallPonts[1] = new Vector3(controlX, controlY, controlZ);



        // Get the initial velocity given the angle of the launch and max height? Used to calculate the flight time
        //float initialVelocity = CalculateInitialVelocity(hitAngle, controlZ / 2);
        float initialVelocity = CalculateInitialVelocity(hitAngle, maxHeight);

        // Get the time of the flight to determine the hitBallModifer so the game moves the ball at the appropriate speed along the flight trajectory?
        flightTime = CalculateFlightTime(initialVelocity, hitAngle);
        hitBallModifer = 1 / flightTime;
    }*/


    public float CalculateInitialVelocity(float angle, float height)
    {
        // https://www.youtube.com/watch?v=Y8OMCrQ0eUg
        Debug.Log("CalculateInitialVelocity: Initial angle: " + angle.ToString() + " max height: " + height.ToString()) ;
        float initialVelocity = 0f;

        // Calculate the velocity upward
        //float metersPerSecondUpward = Mathf.Sqrt(0-2*Physics2D.gravity.y * height);
        float metersPerSecondUpward = Mathf.Sqrt(0 - 2 * (-9.8f) * height);
        Debug.Log("CalculateInitialVelocity: meters per second upward is: " + metersPerSecondUpward.ToString());

        if (angle == 90f)
            return metersPerSecondUpward;

        // Given the velocity upward, calculate the velocity at the given angle
        // Use the upward velocity as the "opposite" side of the right triangle
        // the angled velocity will be the hypotenuse 
        // hypotenuse = opposite/Sin(angle)
        float angle2rad = angle * Mathf.Deg2Rad;
        initialVelocity = metersPerSecondUpward / Mathf.Sin(angle2rad);
        Debug.Log("CalculateInitialVelocity: meters per second at the launch angle is: " + initialVelocity.ToString());
        return initialVelocity;
    }
    public float CalculateFlightTime(float velocity, float angle)
    {
        // https://www.omnicalculator.com/physics/projectile-motion
        float lengthOfFlight = 0f;

        //lengthOfFlight = (2 * velocity * Mathf.Sin(angle * Mathf.Deg2Rad) / Physics2D.gravity.y) * 2; // multiplying by 2 to make it a bit 
        lengthOfFlight = (2 * velocity * Mathf.Sin(angle * Mathf.Deg2Rad) / Physics2D.gravity.y); 
        Debug.Log("CalculateFlightTime: the length of the flight in seconds is: " + lengthOfFlight.ToString());

        return -lengthOfFlight;
    }
    float CalculateMaxHeightOfFlight()
    {
        float maxHeightOfFlight = 0f;
        return maxHeightOfFlight;
    }
    void GetBounces(float hitDistance, float hitAngle, float hitTopSpin, Vector3 hitDirection, float hitHeight, float leftOrRightSpin = 0f)
    {
        // Sanity check to reset bouncing before calculating new bouncing stuff
        ResetBouncingInfo(false);

        if (!hasBouncedYet)
        {
            originalHitDirection = hitDirection;
        }

        // Check if there is side spin. IF so, change the original direction to account for the side spin
        if (leftOrRightSpin != 0)
        {
            // Get the rotation from the left or right spin
            float rotationAngle = 45f * (leftOrRightSpin / myPlayer.TopSpinPositiveModifer);
            Quaternion rotation = Quaternion.AngleAxis(rotationAngle, Vector3.forward);
            Debug.Log("GetBounces: Left or right spin is not zero. Rotate the following degrees: " + rotationAngle.ToString());
            originalHitDirection = (rotation * originalHitDirection).normalized;
            movementDirection = originalHitDirection;
            hitDirection = originalHitDirection;
            Debug.Log("GetBounces: Left or right spin is not zero. New direction after applying spin: " + originalHitDirection.ToString());
            // Reset left or right spin after first bounce. Don't want to affect other bounces?
            launchLeftOrRightSpin = 0f;
        }
            

        // Get the ground material the ball will be bouncing off of. Will effect number of bounces and stuff
        bounceContactGroundMaterial = GetGroundMaterial();

        // Check if the ball will keep bouncing or not
        if (KeepBouncing(hitHeight, bounceContactGroundMaterial))
        {
            // Get the new bounce height based on previous height
            float bounceHeight = GetBounceHeight(hitHeight, bounceContactGroundMaterial);

            // Adjust the spin of the ball
            float bounceTopSpin = hitTopSpin;
            if (hasBouncedYet)
            {
                bounceTopSpin = AdjustSpinForNextBounce(bounceTopSpin);
            }

            // Get the bounce distance based on previous distance and spin
            float bounceDistance = GetBounceDistance(hitDistance, bounceTopSpin, bounceContactGroundMaterial, bounceHeight, hitHeight, hitDirection);

            // Change direction of the bounce if the distance is negative / bouncing backwards
            //Vector2 bounceDirection = (hitDirection + (Vector3)BounceOffSlope()).normalized;
            //Vector2 bounceDirection = (hitDirection + (Vector3)BounceOffSlope());
            Vector2 bounceDirection = hitDirection;
            // Adjust the distance of the bounce based on the slope of the ground the ball is bouncing off of
            //bounceDistance = AdjustBounceDistanceFromSlope(bounceDistance, bounceHeight, hitDirection, groundSlopeDirection, slopeSpeedModifier);

            //Debug.Log("GetBounces: hit direction was: " + hitDirection.ToString() + " and the new bounce direction after checking for ground slope will be: " + bounceDirection.ToString());
            if (bounceDistance <= 0)
            {   
                bounceDistance = Mathf.Abs(bounceDistance);
                if (originalHitDirection == bounceDirection)
                {
                    bounceDirection = -bounceDirection;
                }
            }
            Debug.Log("GetBounces: hit direction was: " + hitDirection.ToString() + " and the new bounce direction after checking for spin is: " + bounceDirection.ToString());
            // Factor in the slope of ground for bounces???
            bounceDirection += BounceOffSlope();
            Debug.Log("GetBounces: hit direction was: " + hitDirection.ToString() + " and the new bounce direction after checking for ground slope will be: " + bounceDirection.ToString() + " Bounce direction using reflecion is: " + Vector3.Reflect(hitDirection,Vector3.forward).ToString());


            // Get the new hit angle based on the new bounce distance. If spin is increasing or decreasing the bounce distance, the angle needs to change?
            float bounceAngle = GetBounceAngle(bounceDistance, bounceHeight);

            // Calculate the trajectory of the bounce
            //CalculateHitTrajectory(bounceDistance, bounceAngle, bounceTopSpin, bounceDirection, Vector2.zero, 0f, true, bounceHeight);
            hitBallPonts = CalculateHitTrajectory(bounceDistance, bounceAngle, bounceTopSpin, 0f, bounceDirection , WindManager.instance.WindDirection, WindManager.instance.WindPower, true, bounceHeight);
            maxHeight = hitBallPonts[1].z / 2;
            initialVelocity = CalculateInitialVelocity(hitAngle, maxHeight);
            flightTime = CalculateFlightTime(initialVelocity, hitAngle);
            hitBallModifer = 1 / flightTime;

            // Start bouncing???
            isBouncing = true;
            hasBouncedYet = true;
            twoBounceDistancesAgo = hitDistance;

            // Adjust the spin for next time
        }
        else
        {
            ResetBouncingInfo(true);

            // Check if ball should keep rolling based on the speed of the ball and ground material the ball is on?

            isRolling = WillBallRoll();
            if (!isRolling)
            {
                ResetBallAndPlayerAfterBallStoppedRolling();
            }
            //speedMetersPerSecond /= 2f;

            return;
        }
    }
    void ResetBouncingInfo(bool resetHasBouncedYet)
    {
        numberOfBouncesToDo = 0;
        numberOfBouncesCompleted = 0;
        isBouncing = false;
        if (resetHasBouncedYet)
            hasBouncedYet = false;
    }
    string GetGroundMaterial()
    {
        string material = "";

        //RaycastHit2D[] ground = Physics2D.CircleCastAll(this.transform.position, this.GetComponent<CircleCollider2D>().radius / 2, Vector2.zero, 0f, groundMask);
        RaycastHit2D[] ground = Physics2D.CircleCastAll(this.transform.position, pixelUnit, Vector2.zero, 0f, groundMask);
        if (ground.Length > 0)
        {
            for (int i = 0; i < ground.Length; i++)
            {
                GroundTopDown groundScript = ground[i].collider.GetComponent<GroundTopDown>();
                GetTileSlopeInformation(groundScript);
                string groundMaterial = groundScript.groundType;

                // Always have rough or trap override 
                if (groundMaterial == "rough" || groundMaterial.Contains("trap"))
                {
                    material = groundMaterial;
                    //GetTileSlopeInformation(groundScript);
                    break;
                }
                // Check to see if the ball is overlapping any types of ground. If it is on the edge of a green ground and non-green ground, keep the non-green ground
                if (!string.IsNullOrWhiteSpace(material))
                {
                    if (groundMaterial == "green" && !material.Equals("green"))
                    {
                        continue;
                    }
                    else
                    {
                        material = groundMaterial;

                        // Check to see if the ball has moved onto a new tile. If so, check if that new tile has any slope information
                        //GetTileSlopeInformation(groundScript);
                        //groundSlopeDirection = groundScript.slopeDirection;
                        //slopeSpeedModifier = groundScript.slopeSpeedIncrease;
                    }
                }
                else
                {
                    material = groundMaterial;
                    // Check to see if the ball has moved onto a new tile. If so, check if that new tile has any slope information
                    //GetTileSlopeInformation(groundScript);
                    //groundSlopeDirection = groundScript.slopeDirection;
                    //slopeSpeedModifier = groundScript.slopeSpeedIncrease;
                }
            }
        }
        else
            material = "fairway"; // in case of failure, default to fairway?

        return material;
    }
    bool KeepBouncing(float previousHeight, string groundMaterial)
    {
        bool keepBouncing = false;

        // never bounce out of a trap? always set number of bounces to 0
        if(groundMaterial.Contains("trap"))
            return keepBouncing;


        if (GetBounceHeight(previousHeight, groundMaterial) > minBounceHeight)
            keepBouncing = true;
        else
            keepBouncing = false;

        return keepBouncing;
    }
    float GetBounceHeight(float previousHeight, string groundMaterial)
    {
        float bounceHeight = 0f;

        // never bounce out of a trap? always set number of bounces to 0
        if (groundMaterial.Contains("trap"))
            return bounceHeight;

        float bounceHeightModifier = defaultBounceHeightModifier;

        if (groundMaterial.Equals("green"))
            bounceHeightModifier += 0.1f;
        else if (groundMaterial.Equals("rough"))
            bounceHeightModifier -= 0.2f;
        else if (groundMaterial.Equals("deep rough"))
            bounceHeightModifier -= 0.5f;

        if (bounceHeightModifier < 0.1f)
            bounceHeightModifier = 0.1f;

        bounceHeight = previousHeight * bounceHeightModifier;

        if (RainManager.instance.IsRaining)
        {
            bounceHeight *= RainManager.instance.RainBounceModifier;
        }

        return bounceHeight;
    }

    float GetBounceDistance(float previousDistance, float spin, string groundMaterial, float bounceHeight, float oldHeight, Vector2 ballDir)
    {
        float bounceDistance = 0f;

        if (groundMaterial.Contains("trap"))
            return bounceDistance = 0f;

        float bounceHeightModifer = 0.6f * (bounceHeight / oldHeight);
        Debug.Log("GetBounceDistance: bounceHeightModifer is: " + bounceHeightModifer.ToString());

        bounceDistance = previousDistance * bounceHeightModifer;
        float bounceDistanceBackSpinAddition = 0f;

        float spinToUse = spin;
        if (hasBouncedYet && spinToUse <= -2.5f)
        {
            /*spinToUse = (-spin + 1) / 2;
            if (spinToUse > 5)
                spinToUse = 5f;*/
            spinToUse = -spin;
            bounceDistanceBackSpinAddition = twoBounceDistancesAgo * 0.1f * bounceHeightModifer;
        }
        bounceDistance += bounceDistanceBackSpinAddition;

        //Debug.Log("GetBounceDistance: spingToUse is: " + spingToUse.ToString());

        float bounceDistanceModifier = (bounceDistance * spinBounceDistanceModifier * spinToUse);
        //float bounceDistanceModifier = (bounceDistance * spinBounceDistanceModifier * spin);

        if (bounceDistanceModifier > bounceDistance * 1.5f)
            bounceDistanceModifier = bounceDistance * 1.5f;

        Debug.Log("GetBounceDistance: bounceDistanceModifier is: " + bounceHeightModifer.ToString());

        bounceDistance += bounceDistanceModifier;

        if (groundSlopeDirection != Vector2.zero && slopeSpeedModifier != 0f)
        {
            
            Vector2 bounceVector = ballDir * bounceDistance;
            Vector2 slopeVector = groundSlopeDirection * slopeSpeedModifier * 7.5f * (Mathf.Abs(bounceDistance) + 0.1f); // adding 0.1 to prevent it from being zero?
            Vector2 combinedVectors = bounceVector + slopeVector;
            Debug.Log("GetBounceDistance: Checking for bounce distance modifier due to slope of ground. Bounce vector is: " + bounceVector.ToString() + " Current bounce distance: " + bounceDistance.ToString() + " magnitude of bounce vector is: " + bounceVector.magnitude.ToString() + " slope vector is: " + slopeVector.ToString() + " with a magnitude of: " + slopeVector.magnitude.ToString() + " magnitude of combined vectors is: " + combinedVectors.magnitude.ToString()); ;
            int distNegOrPos = 1;
            if (bounceDistance < 0)
                distNegOrPos = -1;
            bounceDistance = combinedVectors.magnitude * distNegOrPos;
            Debug.Log("GetBounceDistance: Checking for bounce distance modifier due to slope of ground. New bounce distance is: " + bounceDistance.ToString()); ;


        }

        Debug.Log("GetBounceDistance: bounce distance is: " + bounceDistance.ToString());
        return bounceDistance;
    }
    float GetBounceAngle(float distance, float height)
    {
        float bounceAngle = 0f;
        if (distance == 0)
            return bounceAngle = 90f;

        float adjacenetLength = Mathf.Abs(distance / 2);

        bounceAngle = Mathf.Atan(height / adjacenetLength);
        bounceAngle = bounceAngle * Mathf.Rad2Deg;

        Debug.Log("GetBounceAngle: returning angle of: " + bounceAngle.ToString());
        return bounceAngle;
    }
    float AdjustSpinForNextBounce(float spin)
    {
        float newSpin = spin;

        if (newSpin == 0)
            return newSpin;

        /*if (newSpin < -5f)
        {
            newSpin = (-newSpin + 1) / 2;
            if (newSpin > 5f)
                newSpin = 5f;
        }*/

        newSpin *= 0.5f;

        return newSpin;
    }
    public bool WillBallRoll()
    {
        bool willBallRoll = false;

        // Check what material is beneath the ball
        bounceContactGroundMaterial = GetGroundMaterial();

        if (bounceContactGroundMaterial.Contains("trap") || bounceContactGroundMaterial.Equals("deep rough"))
        {
            Debug.Log("WillBallRoll: stopping due to ground material of: " + bounceContactGroundMaterial);
            willBallRoll = false;
            return willBallRoll;
        }

        // Need to update this so if a ball rolls back down a hill, it doesn't stop rolling when the speed goes from 0 at its apex to then increase again when rolls down, this doesn't stop it from rolling down?
        // probably need to do something like, get current direction of the ball and the direction of the slope. If the angle between the two is greater than 180, continue to roll down?
        if (speedMetersPerSecond > 0.05f)
            willBallRoll = true;
        else
        {
            if (groundSlopeDirection == Vector2.zero)
            {
                Debug.Log("WillBallRoll: Ball is not on a hill. Stopping.");
                speedMetersPerSecond = 0f;
            }
            else
            {
                if (!HitApexOfHill)
                {
                    Debug.Log("WillBallRoll: Ball IS on a hill. Start rolling back down?.");
                    HitApexOfHill = true;
                    willBallRoll = true;
                    speedMetersPerSecond = 0.1f;
                    movementDirection = groundSlopeDirection;
                }
                else
                {
                    Debug.Log("WillBallRoll: Ball IS on a hill, BUT already hit apex of hill. should stop now from friction?");
                    willBallRoll = false;
                    speedMetersPerSecond = 0f;
                }
                
            }
            
        }
            

        // from when I was trying to use forces instead of my bullshit
        /*if (Mathf.Abs(rb.velocity.magnitude) < 0.05f)
        {
            Debug.Log("WillBallRoll: setting velocity to 0");
            willBallRoll = false;
            //rb.velocity = Vector2.zero;
        }*/

        return willBallRoll;
    }
    void RollBall(float rollSpeed, Vector2 rollDirection)
    {
        //original method
        /*this.rb.MovePosition(rb.position + rollDirection * rollSpeed * Time.deltaTime);
        speedMetersPerSecond -= GetGroundRollSpeedModifier(bounceContactGroundMaterial) * Time.deltaTime;*/

        /*
        //float actualSpeed = CalculateCurrentSpeed(rb.position, (rb.position + ((rollDirection * rollSpeed) + (groundSlopeDirection * slopeSpeedModifier * 10f)) * Time.deltaTime));
        //Vector2 newPos = rb.position + ((rollDirection * rollSpeed) + (groundSlopeDirection * slopeSpeedModifier)) * Time.deltaTime;
        //speedMetersPerSecond = CalculateCurrentSpeed(rb.position, newPos) - (GetGroundRollSpeedModifier(bounceContactGroundMaterial) * Time.deltaTime);
        //this.rb.MovePosition(rb.position + ((rollDirection * rollSpeed) + (groundSlopeDirection * slopeSpeedModifier * 10f)) * Time.deltaTime);
        //this.rb.MovePosition(newPos);
        */

        //rb.AddForce(-rollDirection * GetGroundRollSpeedModifier(bounceContactGroundMaterial));

        //Debug.Log("RollBall:  the new speed will be: " + speedMetersPerSecond.ToString() + " but the new speed should actually be: " + actualSpeed.ToString());
        //Debug.Log("Rolling: speedMetersPerSecond: " + speedMetersPerSecond.ToString());

        Vector2 rollVector = rollDirection * rollSpeed;
        Vector2 slopeVector = groundSlopeDirection * slopeSpeedModifier;
        Vector3 currentPos = this.transform.position;
        //Vector3 nextPos = rb.position + rollDirection * rollSpeed * Time.deltaTime;
        Vector3 nextPos = rb.position + ((rollDirection * rollSpeed) + (groundSlopeDirection * slopeSpeedModifier * 0.5f)) * Time.deltaTime;
        Debug.Log("RollBall: current position is: " + currentPos.ToString("0.00000000") + " and the next position will be: " + nextPos.ToString("0.00000000"));
        //this.rb.MovePosition(rb.position + rollDirection * rollSpeed * Time.deltaTime);
        //this.rb.MovePosition(rb.position + rollVector + slopeVector);
        //speedMetersPerSecond -= GetGroundRollSpeedModifier(bounceContactGroundMaterial) * Time.deltaTime;
        float realSpeed = CalculateCurrentSpeed(currentPos, nextPos) - (GetGroundRollSpeedModifier(bounceContactGroundMaterial) * Time.deltaTime);
        Debug.Log("RollBall: current position is: " + currentPos.ToString("0.00000000") + " and the next position will be: " + nextPos.ToString("0.00000000") + " the speed per seconds WILL BE: " + realSpeed.ToString("0.00000000") + " and the speed previously WAS: " + speedMetersPerSecond.ToString("0.00000000"));
        this.rb.MovePosition(nextPos);
        speedMetersPerSecond = realSpeed;
    }
    float GetGroundRollSpeedModifier(string groundMaterial)
    {
        float groundRollSpeedModifier = greenRollSpeedModifier;

        if (groundMaterial.Equals("fairway"))
            groundRollSpeedModifier = fairwayRollSpeedModifier;
        else if (groundMaterial.Equals("rough"))
            groundRollSpeedModifier = roughRollSpeedModifier;
        else if (groundMaterial.Equals("deep rough"))
            groundRollSpeedModifier = deepRoughRollSpeedModifier;

        return groundRollSpeedModifier;
    }
    public void PuttBall(Vector2 directionToPutt, float distanceToPutt)
    {
        if (distanceToPutt > 20f)
            distanceToPutt = 20f;
        movementDirection = directionToPutt.normalized;
        //movementDirection = CalculateWindShiftForPutts(movementDirection);
        launchDistance = distanceToPutt;
        if (RainManager.instance.IsRaining)
        {
            distanceToPutt *= RainManager.instance.RainHitModifier;
        }
        speedMetersPerSecond = GetPuttSpeed(distanceToPutt);

        // from when I was trying to figure out using forces instead of my bullshit
        //rb.AddForce(movementDirection * speedMetersPerSecond, ForceMode2D.Impulse);

        isRolling = WillBallRoll();
        if (!isRolling)
        {
            ResetBallAndPlayerAfterBallStoppedRolling();
        }
        

    }
    public float GetPuttSpeed(float distanceToPutt)
    {
        float puttSpeed = 0f;

        // https://www.toppr.com/guides/physics-formulas/deceleration-formula/
        // Given a final velocity of 0, the deceleration rate from the ground type, and the max distance of the putt, calculate the initial speed in meters per second needed to travel that distance
        puttSpeed = Mathf.Sqrt(Mathf.Abs(0f - (2f * greenRollSpeedModifier * distanceToPutt)));

        Debug.Log("GetPuttSpeed: speed for a distance of " + distanceToPutt.ToString() + " meters would be: " + puttSpeed.ToString());

        return puttSpeed;
    }
    Vector2 GetRollDirection(Vector2 rollDirection, Vector2 slopeDirection)
    {

        Vector2 newDir = (rollDirection * speedMetersPerSecond * Time.fixedDeltaTime) + (slopeDirection * slopeSpeedModifier * Time.fixedDeltaTime);
        //Vector2 newDir = (rollDirection * speedMetersPerSecond * Time.fixedDeltaTime) + (slopeDirection * slopeSpeedModifier);
        Debug.Log("GetRollDirection: initial roll direction: " + rollDirection.ToString() + " slope direction: " + slopeDirection.ToString() + " new direction: " + newDir.ToString() + " new direction normalized: " + newDir.normalized.ToString());
        return newDir.normalized;
        //return newDir;
    }
    public void BallRolledIntoHole(HoleTopDown holeRolledInto)
    {
        if (this.speedMetersPerSecond < 3.0f)
        {
            Debug.Log("BallRolledIntoHole: current speed of the ball: " + this.speedMetersPerSecond.ToString() + " which is slow enough to fall into hole");
            //BallInHole();
            IsInHole = true;
            float timeDelay = TimeBeforeSinkInHole(speedMetersPerSecond, holeRolledInto);
            StartCoroutine(DelayBeforeFallInHole(timeDelay));
        }
        else
        {
            Debug.Log("BallRolledIntoHole: current speed of the ball: " + this.speedMetersPerSecond.ToString() + " which is TOO FAST. Ball will bounce into the hole");
            BounceOutOfHole(holeRolledInto);
        }
    }
    void BallInHole()
    {
        Debug.Log("BallInHole: Wow!");
        myShadow.GetComponent<SpriteRenderer>().enabled = false;
        myBallObject.GetComponent<SpriteRenderer>().enabled = false;
        this.IsInHole = true;
        ResetBallMovementBools();
        
    }
    float TimeBeforeSinkInHole(float movementSpeed, HoleTopDown holeRolledInto)
    {
        float dist = this.myCollider.radius + holeRolledInto.MyCircleRadius;
        float timeBeforeBounce = dist / movementSpeed;
        return timeBeforeBounce;
    }
    IEnumerator DelayBeforeFallInHole(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        //ResetBallMovementBools();
        //this.HitBall(speedMetersPerSecond / 2, 50f, 0f, movementDirection, 0f);
        BallInHole();
    }
    void ResetBallMovementBools()
    {
        if (isRolling)
            isRolling = false;
        if (isHit)
            isHit = false;
        ResetBouncingInfo(true);
    }
    void ResetBallAndPlayerAfterBallStoppedRolling()
    {
        //myPlayer.EnableOrDisableLineObjects(true);
        myPlayer.ResetPreviousHitValues();
    }
    Vector2 GetPerpendicular(Vector2 dir, float leftOrRight)
    {
        Vector2 perp = Vector2.Perpendicular(dir);
        if (leftOrRight > 0)
            perp *= -1f;
        return perp;
    }
    Quaternion GetAngleForSideSpin(float sideSpinValue, float sideSpinModifier)
    {
        float rotAngle = 45f * (sideSpinValue / sideSpinModifier);
        Quaternion rot = Quaternion.AngleAxis(rotAngle, Vector3.forward);
        return rot;
    }
    Vector2 ShiftTrajectoryForSideSpin(float spinValue,Vector2 startPoint, Vector2 midPoint, Vector2 dir, float heightValue)
    {
        Vector2 shift = Vector2.zero;

        if (spinValue == 0f)
            return shift;
        // old way
        //Vector2 perp = GetPerpendicular(dir, spinValue);
        //shift = midPoint + (perp * (heightValue) * (Mathf.Abs(spinValue) / myPlayer.TopSpinPositiveModifer));

        // get the length of the adjacent side
        float lengthOfAdjacent = Vector2.Distance(midPoint, startPoint);
        // get the angle between adjacent side and hypo
        float rotAngle = 45f * (spinValue / myPlayer.TopSpinPositiveModifer);
        // flip the angle because this is the curved trajectory??? something like that hard to explain sorry
        rotAngle *= -1f;
        // Get the direction of the hypo
        Quaternion rot = Quaternion.AngleAxis(rotAngle, Vector3.forward);
        Vector2 hypoDir = (rot * dir).normalized;
        // convert angle to radians
        rotAngle *= Mathf.Deg2Rad;
        float lengthOfHypo = lengthOfAdjacent / (Mathf.Cos(rotAngle));
        // Calculate where the hypo ends based on the length of the hypo, its direction and its starting point
        shift = (hypoDir * lengthOfHypo) + startPoint;
        

        return shift;
    }
    public void UpdateGroundMaterial()
    {
        this.bounceContactGroundMaterial = GetGroundMaterial();
    }
    bool DidBallLandInHole()
    {
        bool landInHole = false;

        RaycastHit2D[] hit2Ds = Physics2D.CircleCastAll(this.transform.position, this.GetComponent<CircleCollider2D>().radius / 2, Vector2.zero, 0f, _golfHoleLayerMask);
        if (hit2Ds.Length > 0)
            landInHole = true;
        Debug.Log("DidBallLandInHole: " + landInHole.ToString() + " at position: " + this.transform.position.ToString());
        // maybe play a "boom shackalacka sound clip (recorded from myself saying it and modified in audacity) if it is a dunk shot? Would need to pass "HasBouncedYet" or whatever I called that variable so I can check that?
        return landInHole;
    }
    void BounceOutOfHole(HoleTopDown holeRolledInto)
    {
        Debug.Log("BounceOutOfHole");
        float timeBeforeBounce = TimeBeforeBounce(speedMetersPerSecond, holeRolledInto);
        StartCoroutine(DelayBeforeBounceOutOfHole(timeBeforeBounce));
    }
    float TimeBeforeBounce(float movementSpeed, HoleTopDown holeRolledInto)
    {
        float dist = this.myCollider.radius + holeRolledInto.MyCircleRadius + holeRolledInto.SpriteRadius;
        float timeBeforeBounce = dist / movementSpeed;
        return timeBeforeBounce;
    }
    IEnumerator DelayBeforeBounceOutOfHole(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        ResetBallMovementBools();
        this.HitBall(speedMetersPerSecond / 2, 50f, 0f, movementDirection, 0f);
    }
    //public void HitEnvironmentObstacle(float obstalceUnityUnits, float ballUnityUnits, bool isHoleFlag, Vector3 collisionPoint, Vector3 centerOfObstacle, Vector3 extentOfObstacle)
    public void HitEnvironmentObstacle(float obstalceUnityUnits, float ballUnityUnits, bool isHoleFlag, Vector2 collisionPoint, Vector2 ballPos, bool softBounce = false, float bounceModifier = 1.0f)
    {
        //if (ballUnityUnits < obstalceUnityUnits)
        if(DoesBallHitObject(obstalceUnityUnits,ballUnityUnits))
        {
            Debug.Log("HitEnvironmentObstacle: ball is not high enough to clear environmnet obstalce. Ball height: " + ballUnityUnits.ToString() + " enviornment obstacle height: " + obstalceUnityUnits.ToString() + " bounce modifier: " + bounceModifier.ToString());
            /*if (isHoleFlag)
            {
                Debug.Log("HitEnvironmentObstacle: the obstacle was a hole flag. Checking if the ball meets conditions to 'bounce' off the flag and into the hole.");
                if (WillBallBounceIntoHole(collisionObject))
                {
                    Debug.Log("Ball Will bounce into the hole!");
                    BounceOffFlagIntoHole(collisionObject.GetComponent<HoleTopDown>());
                }
                else
                    BounceOffTheObstacle();
                BounceOffTheObstacle();
            }*/
            //else if (isRolling)
            if (isRolling)
            {
                //BounceOffTheObstacleRolling(collisionPoint, centerOfObstacle, extentOfObstacle);
                //BounceOffTheObstacleRolling(collisionPoint, ballPos, softBounce);
                BounceOffTheObstacleRolling(collisionPoint, ballPos, softBounce, bounceModifier);
            }
            else
            {
                //BounceOffTheObstacle(softBounce);
                BounceOffTheObstacle(softBounce, bounceModifier);
            }
            
            //hitBallPonts = CalculateHitTrajectory(10f, launchAngle, launchTopSpin, launchLeftOrRightSpin, -movementDirection, Vector2.zero, 0f);
            //hitBallCount = 0f;
            //isHit = false;
            //HitBall(10f, launchAngle, launchTopSpin, -movementDirection, launchLeftOrRightSpin);
        }
        else
        {
            Debug.Log("HitEnvironmentObstacle: ball CLEARS the obstacle and will keep flying. Ball height: " + ballUnityUnits.ToString() + " enviornment obstacle height: " + obstalceUnityUnits.ToString());
        }
    }
    public bool DoesBallHitObject(float obstalceUnityUnits, float ballUnityUnits)
    {
        bool doesBallHitObject = false;
        if (ballUnityUnits < obstalceUnityUnits)
        {
            doesBallHitObject = true;
        }
        else
        {
            doesBallHitObject = false;
        }
        return doesBallHitObject;
    }
    void BounceOffTheObstacle(bool softBounce, float bounceModifier = 1.0f)
    {
        if (hitBallCount < 0.5f)
        {
            Debug.Log("BounceOffTheObstacle: ball is flying upward still");

        }
        else
        {
            Debug.Log("BounceOffTheObstacle: ball is flying downward now");
        }

        float softBounceModifier = GetSoftBounceModifier(softBounce);

        float currentHeight = this.transform.position.z;
        //float midPointHeight = GetObstacleBounceHeight(currentHeight, hitBallCount, softBounceModifier);
        float midPointHeight = GetObstacleBounceHeight(currentHeight, hitBallCount, bounceModifier);
        //float obstacleBounceDistance = GetObstacleBounceDistance(launchDistance, hitBallCount, softBounceModifier);
        float obstacleBounceDistance = GetObstacleBounceDistance(launchDistance, hitBallCount, bounceModifier);

        // reset before hitting the ball again?
        hitBallCount = 0f;
        isHit = false;
        //hitBallPonts = CalculateHitTrajectory(obstacleBounceDistance, launchAngle, launchTopSpin, launchLeftOrRightSpin, -movementDirection, Vector2.zero, 0f, true, midPointHeight);
        //maxHeight = hitBallPonts[1].z / 2;
        //initialVelocity = CalculateInitialVelocity(launchAngle, maxHeight);
        //flightTime = CalculateFlightTime(initialVelocity, launchAngle);
        //hitBallModifer = 1 / flightTime;
        //isHit = true;
        //movementDirection = GetBounceDirection(movementDirection, collisionPoint, ballPos);
        HitBall(obstacleBounceDistance, launchAngle, launchTopSpin, -movementDirection, launchLeftOrRightSpin, true, midPointHeight);
        //HitBall(obstacleBounceDistance, launchAngle, launchTopSpin, movementDirection, launchLeftOrRightSpin, true, midPointHeight);

    }
    float GetObstacleBounceHeight(float ballHeight, float hitCount, float softBounceModifier = 1.0f)
    {
        float newHeight = 0f;

        float distFromMid = 0.5f - hitBallCount;

        if (hitCount >= 0.5f)
        {
            //newHeight = ballHeight / 2;
            newHeight = ballHeight * (1 - Mathf.Abs(distFromMid));
        }
        else
        {
            //newHeight = ballHeight
            newHeight = ballHeight + (((ballHeight * distFromMid) / 2) * softBounceModifier);
        }

        return newHeight;
    }
    float GetObstacleBounceDistance(float oldDist, float hitCount, float softBounceModifier = 1.0f)
    {
        float newDist = oldDist / 4f * softBounceModifier;
        newDist *= (1f - hitCount);
        Debug.Log("GetObstacleBounceDistance: new hitDistance is: " + newDist.ToString() + " based on previous distance of: " + oldDist.ToString() + " after traveling " + hitCount.ToString() + " of its path");
        return newDist;
    }
    //void BounceOffTheObstacleRolling(Vector3 ballPos, Vector3 centerOfObstacle, Vector3 extentOfObstacle)
    void BounceOffTheObstacleRolling(Vector2 collisionPoint, Vector2 ballPos, bool softBounce, float bounceModifier = 1.0f)
    {
        Debug.Log("BounceOffTheObstacleRolling: ballPos: " + ballPos.x.ToString() + "," + ballPos.y.ToString() + " and a collision point of: " + collisionPoint.x.ToString() + "," + collisionPoint.y.ToString());
        Vector2 oldDir = movementDirection;
        /*float leftMostPoint = centerOfObstacle.x - extentOfObstacle.x;
        float rightMostPoint = centerOfObstacle.x + extentOfObstacle.x;
        float topMostPoint = centerOfObstacle.y + extentOfObstacle.y;
        float bottomMostPoint = centerOfObstacle.y - extentOfObstacle.y;
        
        
        if (ballPos.x < leftMostPoint)
        {
            Debug.Log("BounceOffTheObstacleRolling: ball is to the LEFT of the obstacle beyond leftmost extent. Ball position: " + ballPos.x.ToString() + " left most point: " + leftMostPoint);
            if (movementDirection.x > 0)
            {
                Debug.Log("BounceOffTheObstacleRolling: Collision to LEFT of obstacle's left most point while moving right. Flip X direction of ball. Movement direction: " + movementDirection.ToString());
                movementDirection.x *= -1f;
            }
        }
        else if (ballPos.x > rightMostPoint)
        {
            Debug.Log("BounceOffTheObstacleRolling: ball is to the RIGHT of the obstacle beyond right most extent. Ball position: " + ballPos.x.ToString() + " right most point: " + rightMostPoint);
            if (movementDirection.x < 0)
            {
                Debug.Log("BounceOffTheObstacleRolling: Collision to RIGHT of obstacle's right most point while moving right. Flip X direction of ball. Movement direction: " + movementDirection.ToString());
                movementDirection.x *= -1f;
            }
        }

        if (ballPos.y < bottomMostPoint)
        {
            Debug.Log("BounceOffTheObstacleRolling: ball is BELOW of the obstacle beyond its bottom most extent. Ball position: " + ballPos.y.ToString() + " bottom most point: " + bottomMostPoint);
            if (movementDirection.y > 0)
            {
                Debug.Log("BounceOffTheObstacleRolling: Collision BELOW of obstacle's top most point while moving UP. Flip y direction of ball. Movement direction: " + movementDirection.ToString());
                movementDirection.y *= -1f;
            }
        }
        else if (ballPos.y > topMostPoint)
        {
            Debug.Log("BounceOffTheObstacleRolling: ball is ABOVE of the obstacle beyond its top most extent. Ball position: " + ballPos.y.ToString() + " top most point: " + topMostPoint);
            if (movementDirection.y < 0)
            {
                Debug.Log("BounceOffTheObstacleRolling: Collision ABOVE of obstacle's top most point while moving DOWN. Flip y direction of ball. Movement direction: " + movementDirection.ToString());
                movementDirection.y *= -1f;
            }
        }
        */
        float softBounceModifier = GetSoftBounceModifier(softBounce);
        //speedMetersPerSecond *= (0.8f * softBounceModifier);
        speedMetersPerSecond *= (0.8f * bounceModifier);

        // Instead of all this, do the following?
        // Get the direction of the collision point to the center of the collider. That will be the perpendicular angle of the ball to the collider
        // Then, get the angle of the balls movement direction relative to the perpendicular angle/dir https://docs.unity3d.com/ScriptReference/Vector3.Angle.html
        // Then, get the "opposite" angle on the other side of the perpendicular
        //Vector3 newDir = Vector2.Reflect(oldDir, (centerOfObstacle - ballPos).normalized);
        Vector3 newDir = GetBounceDirection(movementDirection, collisionPoint, ballPos);
        // instead of calculating the "normal" above as the direction from the ball to the center of the collider, it should be from the ball's center point to the "closet point" returned by the collider? or maybe not? idk, seems to be working well enough? Might require everything to be a circle collider though?
        Debug.Log("BounceOffTheObstacleRolling: calculated movement direction: " + movementDirection.ToString() + " from a normal direction (perpendicular to the collision) of: " + (ballPos - collisionPoint).normalized.ToString("0.000000") + " using Vector2.reflect: " + newDir.ToString());
        //movementDirection = GetBounceDirection(movementDirection,collisionPoint,ballPos);
        movementDirection = newDir;
    }
    Vector2 GetBounceDirection(Vector2 movementDirection, Vector3 collisionPoint, Vector3 ballPos)
    { 
        return Vector2.Reflect(movementDirection.normalized, (ballPos - collisionPoint).normalized);
    }
    Vector2 CalculateWindShiftForPutts(Vector2 oldDir)
    {
        if (WindManager.instance.WindDirection == Vector2.zero || WindManager.instance.WindPower == 0)
            return oldDir;
        Vector2 newDir = oldDir;

        //Vector2 windDirModifier = WindManager.instance.WindDirection * WindManager.instance.WindPower * slopeSpeedModifier * Time.fixedDeltaTime * 0.05f;
        //Vector2 windDirModifier = WindManager.instance.WindDirection * WindManager.instance.WindPower * Time.fixedDeltaTime * 0.025f;
        Vector2 windDirModifier = WindManager.instance.WindDirection * WindManager.instance.WindPower * 0.025f;
        newDir = (newDir + windDirModifier).normalized;

        return newDir;
    }
    float GetSoftBounceModifier(bool softBounce)
    {
        if (softBounce)
            return 0.4f;
        else
            return 1.0f;
    }
    void GetTileSlopeInformation(GroundTopDown groundScript, bool forceTileLookup = false)
    {
        // Get the current tile the ball is over. If the ball is now on a new tile, get new slope information
        Vector3Int newTilePos = groundScript.GetTilePosition(this.transform.position);
        if (newTilePos == _currentTileCell && !forceTileLookup)
            return;
        Debug.Log("GetTileSlopeInformation: Ball on new tile. Old tile: " + _currentTileCell.ToString() + " new tile: " + newTilePos.ToString());
        _currentTileCell = newTilePos;
        //Tuple<Vector2, float> newSlopeValues = groundScript.GetSlopeDirection(newTilePos);
        Tuple<Vector2, float> newSlopeValues = DirectionTileManager.instance.GetSlopeDirection(newTilePos);
        groundSlopeDirection = newSlopeValues.Item1;
        slopeSpeedModifier = newSlopeValues.Item2;
        Debug.Log("GetTileSlopeInformation: New slope direction: " + groundSlopeDirection.ToString() + " new slope Speed Modifier: " + slopeSpeedModifier.ToString());
    }
    Vector2 BounceOffSlope()
    {
        Vector2 bounceOffSlope = Vector2.zero;

        if (groundSlopeDirection == Vector2.zero || slopeSpeedModifier == 0f)
            return bounceOffSlope;

        bounceOffSlope = groundSlopeDirection * slopeSpeedModifier * 10f; // was originally multipied by 7.5f before I lowered the slope modifiers on the DirectionTileManager

        Debug.Log("BounceOffSlope: new bounce direction modifier off a slope with the following modifier: " + bounceOffSlope.ToString() + " and normalized is: " + bounceOffSlope.normalized.ToString());

        return bounceOffSlope;
    }
}

 