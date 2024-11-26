using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCurveTrajectory
{
    private const float _pixelUnit = 0.0625f;
    public static Vector3[] GetBasicCurveTrajectory(Vector3 startPostion, Vector3 endPosition)
    {
        Vector3[] trajectoryPoints = new Vector3[3];

        // Get movement direction
        Vector3 movementDirection = (endPosition - startPostion).normalized;

        // set the start and end positions
        trajectoryPoints[0] = startPostion;
        trajectoryPoints[0].z = 0f;
        trajectoryPoints[2] = endPosition;
        trajectoryPoints[2].z = 0f;

        // calculate flat XY distance of the trajectory
        float distanceTraveled = Vector2.Distance(startPostion, endPosition);

        // calculate the middle point of the arc
        float controlX = ((endPosition.x - startPostion.x) / 2f) + startPostion.x;
        float controlY = ((endPosition.y - startPostion.y) / 2f) + startPostion.y;
        // harcoded hieght of just quarter distance traveled
        float controlZ = distanceTraveled / 2f;

        trajectoryPoints[1] = new Vector3(controlX, controlY, controlZ);

        return trajectoryPoints;
    }
    public static float CalculateFlightTime(float velocity, float angle)
    {
        // https://www.omnicalculator.com/physics/projectile-motion
        float lengthOfFlight = 0f;

        //lengthOfFlight = (2 * velocity * Mathf.Sin(angle * Mathf.Deg2Rad) / Physics2D.gravity.y) * 2; // multiplying by 2 to make it a bit 
        lengthOfFlight = (2 * velocity * Mathf.Sin(angle * Mathf.Deg2Rad) / Physics2D.gravity.y);
        //Debug.Log("CalculateFlightTime: the length of the flight in seconds is: " + lengthOfFlight.ToString());

        return -lengthOfFlight;
    }
    public static float GetGrenadeHeightYValue(float zValue, float zScale)
    {
        float grenadeObjectY = 0f;
        if (zValue > _pixelUnit)
        {
            grenadeObjectY = zValue / zScale; // was previous divided by 6f...
            float mod = grenadeObjectY % _pixelUnit; // 1 pixel = 0.0625 unity units. Getting the modulus of pixelUnits and then subtracting the remainder to make sure the BallObject never moves more than a whole pixel at a time. Removes some of the jittering that was happening without doing this
            grenadeObjectY -= mod;
        }

        return grenadeObjectY;
    }
}
