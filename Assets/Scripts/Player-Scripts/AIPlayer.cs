using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class AIPlayer : NetworkBehaviour
{
    [Header("GamePlayer Stuff")]
    [SerializeField] GamePlayer myPlayerScript;
    private Football gameFootball;

    [Header("Kickoff Parameters")]
    [SerializeField] float minKickAngle;
    [SerializeField] float maxKickAngle;
    [SerializeField] float minKickPower;
    [SerializeField] float maxKickPower;

    [Header("Endzone info")]
    [SerializeField] Vector3 greenEndzone;
    [SerializeField] Vector3 greyEndzone;
    public Vector3 myEndzone;
    public Vector3 myGoalPost = Vector3.zero;
    [SerializeField] float greenEndzoneXValue;
    [SerializeField] float greyEndzoneXValue;
    public float myEndzoneXValue;
    public Vector3 positionToRunToward;
    Vector3 selectedGoblinPosition;

    [Header("Goblin Direction Stuff?")]
    [SerializeField] LayerMask goblinPlayerLayerMask;
    [SerializeField] LayerMask goblinBodyLayerMask;
    [SerializeField] float fieldYMax;
    [SerializeField] float fieldYMin;

    [Header("Surrounding Goblin Stuff")]
    public Collider2D[] surroundingGoblinColliders = new Collider2D[6];
    public int surroundingGoblinCollidersLength = 0;
    public GameObject[] enemyGoblins = new GameObject[3];
    public int enemyGoblinsLength = 0;
    public GameObject[] teammateGoblins = new GameObject[3];
    public int teammateGoblinsLength = 0;
    public GameObject[] closeEnemyGoblins = new GameObject[3];
    public int closeEnemyGoblinsLength = 0;
    public GoblinScript goblinToPassTo;

    [Header("Action state bools?")]
    public bool areEnemyGoblinsNearby = false;
    public bool willKickDownField = false;
    public bool willPass = false;
    public float passCheckLastTime = 0f;
    public float passCheckRate = 0.15f;
    public float kickCheckLastTime = 0f;
    public float kickCheckRate = 0.15f;

    [Header("Kick After - Blocking")]
    public bool blockKick = false;

    [Header("Kick After - Kicking")]
    public bool kickKickAfter = false;
    bool gotMaxAcceptableDistance = false;
    public GoblinScript kickAfterGoblin;
    public float minAccuracyDifficultySetting = 0.85f;
    public float minKickAfterDistance = 12.5f;
    public float maxAcceptableDistance = 0f;
    public bool finalPositionReached = false;
    [SerializeField] LayerMask obstacleLayerMask;

    [Header("PowerUp Usage/Tracking stuff")]
    IEnumerator powerUpRoutine;
    public bool isPowerUpRoutineRunning = false;
    IEnumerator powerUpCoolDown;
    public bool isPowerUpCoolDownRoutineRunning = false;
    public bool hasDefensePowerUp = false;
    public int defensePowerUpIndex = 0;
    public bool hasAttackPowerUp = false;
    public int attackPowerUpIndex = 0;
    public bool hasSpeedPowerUp = false;
    public int speedPowerUpIndex = 0;
    public bool hasStaminaPowerUp = false;
    public int staminaPowerUpIndex = 0;
    public bool hasHealthPowerUp = false;
    public int healthPowerUpIndex = 0;
    public bool hasBlueShellEnemy = false;
    public int blueShellEnemyIndex = 0;
    public bool hasBlueShellSelf = false;
    public int blueShellSelfIndex = 0;
    public bool hasBottlePowerUp = false;
    public int bottlePowerUpIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            myPlayerScript = this.gameObject.GetComponent<GamePlayer>();
            this.GetMyEndzone();
        }
        catch (Exception e)
        {
            Debug.Log("AIPlayer: Error getting GamePlayer script from object: " + e);
        }
        if (!gameFootball)
        {
            gameFootball = GameObject.FindGameObjectWithTag("football").GetComponent<Football>();
            this.GetMyGoalPost();
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        if (GameplayManager.instance.gamePhase == "kick-after-attempt")
        {
            if (blockKick)
                BlockKickAfterAttempt();
            else if (kickKickAfter && !finalPositionReached)
            {
                if (!gotMaxAcceptableDistance)
                    GetMaxAcceptableDistance();
                else
                    KickAfterPositioning();
            }
        }
    }
    void GetMyEndzone()
    {
        if (myPlayerScript.isTeamGrey)
        {
            myEndzone = greenEndzone;
            myEndzoneXValue = greenEndzoneXValue;
        }
        else
        {
            myEndzone = greyEndzone;
            myEndzoneXValue = greyEndzoneXValue;
        }
    }
    void GetMyGoalPost()
    {
        if (myPlayerScript.isTeamGrey)
        {
            myGoalPost = gameFootball.GoalPostForGrey;
        }
        else
        {
            myGoalPost = gameFootball.GoalPostForGreen;
        }
    }
    public void KickOffSequence()
    {
        Debug.Log("KickOffSequence for AI Player");
        float kickAngle = SetKickOffAngle();
        float kickPower = SetKickOffPower();
        myPlayerScript.selectGoblin.KickFootballGoblin(kickPower, kickAngle);
    }
    public float SetKickOffAngle()
    {
        float angle = 0f;

        angle = UnityEngine.Random.Range(minKickAngle, maxKickAngle);

        return angle;
    }
    public float SetKickOffPower()
    {
        float power = 0f;

        power = UnityEngine.Random.Range(minKickPower, maxKickPower);

        return power;
    }
    public void RunTowardEndZone()
    {
        myPlayerScript.selectGoblin.myGoblinAIPathFindingScript.goblinTarget = null;
        myPlayerScript.selectGoblin.myGoblinAIPathFindingScript.isTargetingAGoblin = false;
        selectedGoblinPosition = myPlayerScript.selectGoblin.transform.position;

        //Vector3 directionOfEndzone = (myEndzone - selectedGoblinPosition).normalized;

        // Get "average direction" of enemy goblins that are nearby. Move in opposite direction of them. Adjust where your positionToRunToward's Y value will be. If average direction is below, move up. If average direction is above, move down. If average direction is basically the same above/below, stay where you are?
        //positionToRunToward = myEndzone;


        /*Vector2 avoidDirection = AverageDirectionOfEnemyGoblins(selectedGoblinPosition, myPlayerScript.selectGoblin.isGoblinGrey);
        positionToRunToward.y += avoidDirection.y;
        if (positionToRunToward.y > fieldYMax)
            positionToRunToward.y = fieldYMax;
        else if (positionToRunToward.y < fieldYMin)
            positionToRunToward.y = fieldYMin;*/

        /* Steps to take to determine where to run to in endzone, whether to pass, whether to kick downfield
         * * Get list of nearby goblins
         * * * break the list of goblins into teammates and opponents
         * * If enemy goblins are CLOSE nearby
         * * * if far downfield from endzone, kick ball
         * * * if NOT far downfield and there are nearby teammates, check for pass
         * * * if neither, or "random" decision not to kick/pass, continue to next checks
         * * If no close by goblins OR no kick/pass
         * * * check average direction of enemy goblins, adjust target to run to based on average direction
         * * * * Try to run in opposite direction of enemy goblins. If enemies on top half of field, run further toward bottom and vice versa
         */

        // Get Surrounding Goblins. Divide into enemy/teammates
        GetSurroundingGoblins(selectedGoblinPosition, myPlayerScript.selectGoblin.isGoblinGrey);

        // Check if enemy goblins are close to the selected goblin
        if (areEnemyGoblinsNearby)
        {
            // Check if the ball should be clicked downfield
            willKickDownField = KickBallDownfield(selectedGoblinPosition);
            // Check if the ball should be passed
            if (!willKickDownField)
                willPass = PassBallToTeammate(selectedGoblinPosition, myPlayerScript.selectGoblin.isGoblinGrey);
            else
                willPass = false;
        }
        else
        {
            willKickDownField = false;
            willPass = false;
        }
        if (willKickDownField)
        {
            float kickPower = UnityEngine.Random.Range(0.5f, 1.0f);
            myPlayerScript.selectGoblin.KickFootballGoblin(kickPower, 0f);
            return;
        }
        if (willPass)
        {
            if (goblinToPassTo.isEGoblin)
                myPlayerScript.SwitchToEGoblin(false, Time.time);
            else if (goblinToPassTo.isQGoblin)
                myPlayerScript.SwitchToQGoblin(false, Time.time);

        }

        WillGoblinSprint();
        

        // Set position to run toward. Continue on straight line unless there are enemy goblins nearby?
        
        positionToRunToward.y = selectedGoblinPosition.y;
        positionToRunToward.x = myEndzoneXValue;

        // If the ball carrier did not kick or pass the ball, check for nearby goblins and adjust where the ball carrier will run to
        if (!willKickDownField && !willPass)
        {
            Vector2 avoidDirection = AdjustEndzonePositionBasedOnEnemyGoblins(selectedGoblinPosition);
            //positionToRunToward.y += (avoidDirection.y * 10f);
            positionToRunToward.y += (avoidDirection.y * 2.5f);
            if (positionToRunToward.y > fieldYMax)
                positionToRunToward.y = fieldYMax;
            else if (positionToRunToward.y < fieldYMin)
                positionToRunToward.y = fieldYMin;
        }
        Vector2 directionOfEndzone = (positionToRunToward - selectedGoblinPosition).normalized;

        // Check if the goblin has run past their endzone line yet or not?
        bool isGoblinPastEndzoneLine = false;
        if (Mathf.Abs(selectedGoblinPosition.x) < Mathf.Abs(myEndzoneXValue))
            isGoblinPastEndzoneLine = false;
        else
            isGoblinPastEndzoneLine = true;
        //if (Vector2.Distance(positionToRunToward, selectedGoblinPosition) > 1.0f)
        // If the ball carrier is NOT past the endzone line, or near it, keep running toward endzone line. Otherwise, make checks for when to dive into the endzone?
        if (Mathf.Abs(myEndzoneXValue - selectedGoblinPosition.x) > 1.8f && (!isGoblinPastEndzoneLine))
        {
            myPlayerScript.selectGoblin.AIMoveTowardDirection(directionOfEndzone, positionToRunToward);
        }
        else
        {
            //Debug.Log("RunTowardEndZone: Goblin near endzone. dive into endzone?");
            // If enemy goblins are nearby, dive as soon as possible into the endzone
            if (areEnemyGoblinsNearby && !isGoblinPastEndzoneLine)
                DiveForEndzone(directionOfEndzone);
            else if (areEnemyGoblinsNearby && isGoblinPastEndzoneLine)
            {
                // IF goblin is past their endzone, and there is a goblin nearby, dive in avoid direction?
                DiveForEndzone(AdjustEndzonePositionBasedOnEnemyGoblins(selectedGoblinPosition));
            }
            else
            {
                // If there are no enemies nearby, run toward center of endzone for best kick after attempt results?
                positionToRunToward = myEndzone;
                directionOfEndzone = (positionToRunToward - selectedGoblinPosition).normalized;
                // Check if goblin is near center of endzone now. If yes, dive. If not, keep running?
                if (Vector2.Distance(positionToRunToward, selectedGoblinPosition) <= 1.8f)
                {
                    DiveForEndzone(directionOfEndzone);
                }
                else
                {
                    myPlayerScript.selectGoblin.AIMoveTowardDirection(directionOfEndzone, positionToRunToward);
                }

            }
        }
    }
    void DiveForEndzone(Vector3 diveDirection)
    {
        myPlayerScript.selectGoblin.AIMoveTowardDirection(diveDirection, myEndzone);
        myPlayerScript.selectGoblin.DiveAIGoblinTowardEndzone(diveDirection);
    }
    private Vector2 GoblinsInFrontOfBallCarrirer(Vector3 goblinPosition)
    {
        Vector2 goblinsInFrontDirection = Vector2.zero;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(goblinPosition, 10f, goblinPlayerLayerMask);

        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                Vector3 positionOfColliderThatWasHit = colliders[i].transform.position;

                if (colliders[i].transform.position == goblinPosition)
                    continue;

                float distance = Vector2.Distance(goblinPosition, positionOfColliderThatWasHit);
                if (distance <= 10f)
                {
                    Vector2 diff = goblinPosition - positionOfColliderThatWasHit;
                    diff.Normalize();
                    diff = diff / distance;
                    goblinsInFrontDirection += diff;
                }
            }
        }

        return goblinsInFrontDirection;
    }
    private Vector2 AverageDirectionOfEnemyGoblins(Vector3 goblinPosition, bool isGoblinGrey)
    {
        Vector2 averageDirection = Vector2.zero;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(goblinPosition, 10f, goblinPlayerLayerMask);
        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                GoblinScript colliderScript = colliders[i].GetComponent<GoblinScript>();
                if (colliderScript.isGoblinGrey == isGoblinGrey)
                    continue;
                if (colliders[i].transform.position == goblinPosition)
                    continue;

                Vector3 positionOfColliderThatWasHit = colliders[i].transform.position;                

                float distance = Vector2.Distance(goblinPosition, positionOfColliderThatWasHit);
                if (distance <= 10f)
                {
                    Vector2 diff = goblinPosition - positionOfColliderThatWasHit;
                    diff.Normalize();
                    //diff = -diff.normalized;
                    diff = diff / distance;
                    averageDirection += diff;
                }
            }
        }
        Debug.Log("AverageDirectionOfEnemyGoblins: returning the following average direction: " + averageDirection.ToString());
        return averageDirection;
    }
    private Vector2 AdjustEndzonePositionBasedOnEnemyGoblins(Vector3 goblinPosition)
    {
        Vector2 avoidDirection = Vector2.zero;

        for (int i = 0; i < enemyGoblinsLength; i++)
        {
            Vector3 enemyPosition = enemyGoblins[i].gameObject.transform.position;
            float distance = Vector2.Distance(goblinPosition, enemyPosition);
            Vector2 diff = goblinPosition - enemyPosition;
            diff.Normalize();
            diff = diff / distance;
            avoidDirection += diff;
        }

        return avoidDirection;
    }
    void GetSurroundingGoblins(Vector3 goblinPosition, bool isGrey)
    {
        // reset lengths of all nearby goblin arrays
        surroundingGoblinCollidersLength = 0;
        enemyGoblinsLength = 0;
        teammateGoblinsLength = 0;
        closeEnemyGoblinsLength = 0;

        // reset the nearby goblins bool
        areEnemyGoblinsNearby = false;

        // Get all surrounding goblins
        Collider2D[] colliders = Physics2D.OverlapCircleAll(goblinPosition, 15f, goblinPlayerLayerMask);
        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                GoblinScript nearbyGoblin = colliders[i].GetComponent<GoblinScript>();
                if (nearbyGoblin.isGoblinGrey == isGrey)
                {
                    if (nearbyGoblin.gameObject != myPlayerScript.selectGoblin.gameObject && !nearbyGoblin.isGoblinKnockedOut)
                    {
                        teammateGoblins[teammateGoblinsLength] = nearbyGoblin.gameObject;
                        teammateGoblinsLength++;
                    }
                }
                else if (nearbyGoblin.isGoblinGrey != isGrey && !nearbyGoblin.isGoblinKnockedOut)
                {
                    enemyGoblins[enemyGoblinsLength] = nearbyGoblin.gameObject;
                    enemyGoblinsLength++;
                    if (Vector2.Distance(goblinPosition, nearbyGoblin.transform.position) <= 3.5f)
                    { 
                        closeEnemyGoblins[closeEnemyGoblinsLength] = nearbyGoblin.gameObject;
                        closeEnemyGoblinsLength++;
                        areEnemyGoblinsNearby = true;
                    }
                }
            }
        }
    }
    bool KickBallDownfield(Vector3 goblinPosition)
    {
        bool kickDownfield = false;

        // Only check to pass the ball eery 0.3 seconds instead of every fixed update (0.02 seconds!)
        /*if (Time.time <= (kickCheckLastTime + kickCheckRate))
            return kickDownfield;
        else
            passCheckLastTime = Time.time;*/


        // If the goblin is invinvible, do not kick!
        if (myPlayerScript.selectGoblin.invinvibilityBlueShell || this.hasBlueShellSelf)
            return false;
        // If the game is in xtra-time or near the end of the game, check if AI team is winning. If they are winning, kick the ball to end the game. If they are not winning, do not kick ball. If tied, ignore and continue with regular checks
        if (GameplayManager.instance.gamePhase == "xtra-time" || GameplayManager.instance.timeLeftInGame < 2)
        {
            int scoreDifference = GameplayManager.instance.greenScore - GameplayManager.instance.greyScore;

            if (myPlayerScript.isTeamGrey)
            {
                if (scoreDifference > 0)
                    return false;
                else if (scoreDifference < 0)
                {
                    if(GameplayManager.instance.firstHalfCompleted || Vector2.Distance(myPlayerScript.selectGoblin.transform.position, myGoalPost) > 35f)
                        return true;
                }
            }
            else
            {
                if (scoreDifference < 0)
                    return false;
                else if (scoreDifference > 0)
                {
                    if (GameplayManager.instance.firstHalfCompleted || Vector2.Distance(myPlayerScript.selectGoblin.transform.position, myGoalPost) > 35f)
                        return true;
                }
                    
            }
        }
        float distanceFuzziness = UnityEngine.Random.Range(-7.5f, 7.5f);

        //if (Vector2.Distance(goblinPosition, myEndzone) < (70f + distanceFuzziness))
        if (Mathf.Abs(myEndzoneXValue - goblinPosition.x) < (70f + distanceFuzziness))
            return kickDownfield;

        // Check for goblins in front of the ball carrier. If there are none in front of the goblin, no need to kick downfield?
        // By "in front" I mean between the ball carrier and the endzone they want to score in. It does not matter what direction the goblin is facing. the direction to cast is based on the team the goblin is on.
        Vector2 directionToCast = new Vector2(1f, 0f);
        if (myPlayerScript.selectGoblin.isGoblinGrey)
            directionToCast = new Vector2(-1f, 0f);
        bool anyEnemiesInFrontOfMe = false;
        Vector3 colliderPosition = myPlayerScript.selectGoblin.transform.position;
        for (int t = 0; t < 3; t++)
        {
            RaycastHit2D[] enemyHits;
            Vector2 currentRayDir = Vector2.zero;
            if (t == 0)
            {
                //raycast forward
                currentRayDir = directionToCast;
                enemyHits = Physics2D.CircleCastAll(colliderPosition, 0.25f, currentRayDir, 4f, goblinBodyLayerMask);
                Debug.DrawRay(colliderPosition, (currentRayDir * 4), Color.yellow, 0.1f);
            }
            else if (t == 1)
            {
                //upward angle
                Vector2 dir = DegreeToVector2(75);
                dir = (directionToCast + dir).normalized;
                currentRayDir = dir;
                enemyHits = Physics2D.CircleCastAll(colliderPosition, 0.25f, currentRayDir, 4f, goblinBodyLayerMask);
                Debug.DrawRay(colliderPosition, (currentRayDir * 4), Color.red, 0.1f);
            }
            else
            {
                //downward relative angle to direction of movement to be used for raycast.
                Vector2 dir = DegreeToVector2(-75);
                dir = (directionToCast + dir).normalized;
                currentRayDir = dir;
                enemyHits = Physics2D.CircleCastAll(colliderPosition, 0.25f, currentRayDir, 4f, goblinBodyLayerMask);
                Debug.DrawRay(colliderPosition, (currentRayDir * 4), Color.green, 0.1f);
            }
            //check if we hit anything and if mutliple raycasts hit something, we want to find the one furthest away.
            if (enemyHits.Length > 0)
            {
                for (int x = 0; x < enemyHits.Length; x++)
                {
                    GoblinScript goblinHit = enemyHits[x].collider.transform.parent.GetComponent<GoblinScript>();
                    if (goblinHit == myPlayerScript.selectGoblin)
                        continue;
                    else if (goblinHit.isGoblinGrey != myPlayerScript.selectGoblin.isGoblinGrey)
                    {
                        Debug.Log("PassBallToTeammate: Goblin in front of me. Found on t value of: " + t.ToString());
                        anyEnemiesInFrontOfMe = true;
                        break;
                    }
                }
                if (anyEnemiesInFrontOfMe)
                    break;

            }
        }

        if (!anyEnemiesInFrontOfMe)
            return willKickDownField;


        float kickLikelihood = UnityEngine.Random.Range(0f, 1.0f);
        float likelihoodWeight = 0.2f;
        if (closeEnemyGoblinsLength == 2)
            likelihoodWeight = 0.6f;
        else if (closeEnemyGoblinsLength == 3)
            likelihoodWeight = 0.8f;

        if (kickLikelihood < likelihoodWeight)
            kickDownfield = true;
        else
            kickDownfield = false;

        return kickDownfield;
    }
    bool PassBallToTeammate(Vector3 goblinPosition, bool isGrey)
    {
        bool passBall = false;

        // Only check to pass the ball eery 0.3 seconds instead of every fixed update (0.02 seconds!)
        if (Time.time <= (passCheckLastTime + passCheckRate))
            return passBall;
        else
            passCheckLastTime = Time.time;

        // If the goblin is invinvible, do not pass!
        if (myPlayerScript.selectGoblin.invinvibilityBlueShell || this.hasBlueShellSelf)
            return false;

        // if there were no close teammates, return false
        if (teammateGoblinsLength < 1)
        {
            passBall = false;
            return passBall;
        }


        // Cast a circle out in front of goblin. If there are no enemies directly in front, don't pass?
        Vector2 directionToCast = new Vector2(1f, 0f);
        if (myPlayerScript.selectGoblin.myRenderer.flipX)
            directionToCast = new Vector2(-1f, 0f);

        // See how close the enemies are. Don't pass if the enemies aren't close enough?
        bool areEnemiesReallyClose = false;
        for (int i = 0; i < closeEnemyGoblinsLength; i++)
        {
            float distance = Vector2.Distance(goblinPosition, closeEnemyGoblins[i].transform.position);
            if (distance >= 2.5f)
                continue;
            else
            {
                areEnemiesReallyClose = true;
                //Debug.Log("PassBallToTeammate: areEnemiesReallyClose is true!");
                break;
            }   
        }
        //if (!anyEnemiesInFrontOfMe && !areEnemiesReallyClose)
        if (!areEnemiesReallyClose)
        {
            //Debug.Log("PassBallToTeammate: areEnemiesReallyClose and anyEnemiesInFrontOfMe are false. no need to pass.");
            passBall = false;
            return passBall;
        }

        // Even if there are nearby teammates, random chance they will not pass and return false. Weighted by how many goblins are close by?
        float passChance = UnityEngine.Random.Range(0f, 1.0f);
        float likelihoodWeight = 0.1f;
        if (closeEnemyGoblinsLength == 2)
            likelihoodWeight = 0.25f;
        else if (closeEnemyGoblinsLength == 3)
            likelihoodWeight = 0.6f;
        /*if (!anyEnemiesInFrontOfMe)
            likelihoodWeight *= 0.5f;*/


        if (passChance > likelihoodWeight)
        {
            passBall = false;
            return passBall;
        }

        // Check nearby teammates. If there is a line-of-sight to them with no enemy goblins between, pass to them
        if (teammateGoblinsLength > 0)
        {
            for (int i = 0; i < teammateGoblinsLength; i++)
            {
                Vector3 teammatePosition = teammateGoblins[i].transform.position;
                // Check to make sure teammate is behind the goblin so they can receive a pass. If not, skip that teammate
                if (isGrey)
                {
                    if (teammatePosition.x < goblinPosition.x)
                    {
                        continue;
                    }
                }
                else
                {
                    if (teammatePosition.x > goblinPosition.x)
                    {
                        continue;
                    }
                }

                // Check distance to teammate. If it is too great, continue to next teammate
                float distanceToTeammate = Vector2.Distance(goblinPosition, teammatePosition);
                if (distanceToTeammate > 15f)
                {
                    continue;
                }

                // Check for line of sight between teammate and ball carrier
                // Change this to a Physics2D.CircleCast to make the raycast "thicker" instead of just a single point along a line
                Vector3 directionToTeammate = (teammatePosition - goblinPosition).normalized;
                //RaycastHit2D[] hits = Physics2D.RaycastAll(goblinPosition, directionToTeammate, distanceToTeammate, goblinBodyLayerMask);
                RaycastHit2D[] hits = Physics2D.CircleCastAll(goblinPosition, 0.25f, directionToTeammate, distanceToTeammate, goblinBodyLayerMask);
                bool enemyBetween = false;
                if (hits.Length > 0)
                {
                    //Debug.Log("PassBallToTeammate: someone in between goblin. This many goblins in direction? " + hits.Length.ToString());
                    for (int j = 0; j < hits.Length; j++)
                    {
                        GoblinScript goblin = hits[j].collider.transform.parent.GetComponent<GoblinScript>();
                        //Debug.Log("PassBallToTeammate: is the goblin in between grey? " + goblin.isGoblinGrey.ToString() + " and their name? " + goblin.name + " : " + goblin.ownerConnectionId.ToString());
                        if (goblin.isGoblinGrey != isGrey && !goblin.isGoblinKnockedOut)
                        {
                            // Opposing goblin found between ball carrier and this goblin. New position will be needed
                            //Debug.Log("GetOpenForPass: found goblin between " + this.name + " and the ball carrier. It is " + goblin.name);
                            //Debug.Log("PassBallToTeammate: enemy detected between");
                            enemyBetween = true;
                            break;
                        }
                    }
                }
                // If there is an enemy between teammate and ball carrier, check next teammate. Else, pass to that teammate
                if (enemyBetween)
                {
                    continue;
                }
                else
                {
                    passBall = true;
                    goblinToPassTo = teammateGoblins[i].GetComponent<GoblinScript>();
                    break;
                }
            }
        }
        
        return passBall;
    }
    void BlockKickAfterAttempt()
    {
        // stop trying to block kick after football was kicked
        if (!gameFootball.isHeld)
            return;
        GoblinScript goblinWithBall = null;
        Vector2 directionOfGoblinWithBall = Vector2.zero;
        Vector3 goblinWithBallPosition = Vector3.zero;
        Vector3 myGoblinPosition = Vector3.zero;
        try
        {
            goblinWithBall = gameFootball.goblinWithBall;
            if (goblinWithBall != null)
                goblinWithBallPosition = goblinWithBall.transform.position;
        }
        catch (Exception e)
        {
            Debug.Log("BlockKickAfterAttempt: could not get goblinWithBall from the game football. Error: " + e);
            return;
        }
        foreach (GoblinScript goblin in myPlayerScript.goblinTeam)
        {
            if (goblin.soundType != "skirmisher")
                continue;
            myGoblinPosition = goblin.transform.position;
            directionOfGoblinWithBall = (goblinWithBallPosition - myGoblinPosition).normalized;
            goblin.WillAIGoblinSprint(true);
            if(!goblin.isGoblinKnockedOut)
                goblin.AIMoveTowardDirection(directionOfGoblinWithBall, goblinWithBallPosition);
        }
    }
    void KickAfterPositioning()
    {
        if (myGoalPost == Vector3.zero)
        {
            if (!gameFootball)
            {
                gameFootball = GameObject.FindGameObjectWithTag("football").GetComponent<Football>();
            }
            this.GetMyGoalPost();
        }
        float kickAfterDistance = Vector2.Distance(kickAfterGoblin.transform.position, myGoalPost);

        // Cast out a ray behind kick after goblin to detect if a trip obstacle is behind them. If it is, stop moving?
        bool tripObject = false;
        Vector2 castDirection = new Vector2(-1.0f, 0f);
        if(myPlayerScript.isTeamGrey)
            castDirection = new Vector2(1.0f, 0f);
        RaycastHit2D[] hits = Physics2D.CircleCastAll(kickAfterGoblin.rb.worldCenterOfMass, 0.25f, castDirection, 1f);
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.gameObject.tag == "tripObject" && (Mathf.Abs(hits[i].collider.transform.position.x) < Mathf.Abs(kickAfterGoblin.transform.position.x)))
                {

                    Debug.Log("KickAfterPositioning: trip obstacle hit! stop moving the goblin. Trip obecjt: " + hits[i].collider.name);
                    tripObject = true;
                    break;
                }
            }
        }

        if ((kickAfterGoblin.kickAfterAccuracyDifficulty > minAccuracyDifficultySetting && kickAfterDistance > minKickAfterDistance) || kickAfterDistance > maxAcceptableDistance || tripObject)
        {
            // Attempt the kick after
            kickAfterGoblin.EndKickAfterRepositioning();
            finalPositionReached = true;
            kickAfterGoblin.SubmitKickAfterPositionToServer();
        }
        else
        {
            kickAfterGoblin.KickAfterRepositioning(!kickAfterGoblin.isGoblinGrey);
        }
    }
    // Get the maximum acceptable distance for the kick after attempt. Calculated based on the kicking goblin's max kick distance value that's set by the server.
    void GetMaxAcceptableDistance()
    {
        Debug.Log("GetMaxAcceptableDistance: for goblin: " + kickAfterGoblin.name + ":" + kickAfterGoblin.ownerConnectionId.ToString());
        float randomValue = UnityEngine.Random.Range(0.85f, 0.95f);
        maxAcceptableDistance = kickAfterGoblin.GoblinMaxKickDistance * randomValue;
        gotMaxAcceptableDistance = true;
        Debug.Log("GetMaxAcceptableDistance: for goblin: " + kickAfterGoblin.name + ":" + kickAfterGoblin.ownerConnectionId.ToString() + " max acceptable distance is: " + maxAcceptableDistance + " and the kicking goblin's max distance is: " + kickAfterGoblin.GoblinMaxKickDistance.ToString());
    }
    public void KickAfterAttempt()
    {
        IEnumerator kickAfterAttemptDelay = KickAfterAttemptDelay();
        StartCoroutine(kickAfterAttemptDelay);
    }
    IEnumerator KickAfterAttemptDelay()
    {
        float delayAmount = (0.6f / kickAfterGoblin.kickAfterAccuracyDifficulty) + UnityEngine.Random.Range(0f,0.25f);
        float maxDelay = UnityEngine.Random.Range(2.75f, 3.5f);
        if (delayAmount > maxDelay)
            delayAmount = maxDelay;
        Debug.Log("KickAfterAttemptDelay: delay amount: " + delayAmount);
        yield return new WaitForSeconds(delayAmount);
        float accuracyValue = UnityEngine.Random.Range(0f, 1.0f);
        float powerValue = KickAfterPower();
        kickAfterGoblin.AISubmitKickAfterKickingAccuracy(accuracyValue, powerValue);
        this.kickKickAfter = false;
        this.gotMaxAcceptableDistance = false;
        this.finalPositionReached = false;
    }
    float KickAfterPower()
    {
        float kickAfterPower = 0f;

        float minPowerNeeded = Vector2.Distance(kickAfterGoblin.transform.position, myEndzone) / kickAfterGoblin.GoblinMaxKickDistance;
        kickAfterPower = UnityEngine.Random.Range((minPowerNeeded - 0.2f), (minPowerNeeded + 0.15f));

        if (kickAfterPower > 1.0f)
            kickAfterPower = 1.0f;

        return kickAfterPower;
    }
    // Determine whether goblin should be sprinting or not
    public void WillGoblinSprint()
    {
        // Checks for when the ball carrier should sprint
        if (!willKickDownField && !willPass && enemyGoblinsLength > 0)
        {
            if (!myPlayerScript.selectGoblin.isSprinting && myPlayerScript.selectGoblin.canGoblinSprint)
            {
                myPlayerScript.selectGoblin.AIGetSprintingParameters();
                myPlayerScript.selectGoblin.isSprinting = true;
            }
            else if (myPlayerScript.selectGoblin.isSprinting && myPlayerScript.selectGoblin.canGoblinSprint)
            {
                myPlayerScript.selectGoblin.isSprinting = myPlayerScript.selectGoblin.AIWillGoblinKeepSprinting();
            }
            else if (!myPlayerScript.selectGoblin.canGoblinSprint)
            {
                myPlayerScript.selectGoblin.isSprinting = false;
            }
        }
        else
        {
            myPlayerScript.selectGoblin.isSprinting = false;
            myPlayerScript.selectGoblin.canGoblinSprint = false;
            myPlayerScript.selectGoblin.AIResetSprintingParameters();
        }
    }
    public void ActivateAIPowerUpRoutine(bool activate)
    {
        Debug.Log("ActivateAIPowerUpRoutine: Activating? " + activate.ToString());
        if (activate)
        {
            Debug.Log("ActivateGameTimer: " + activate.ToString());
            powerUpRoutine = PowerUpUsageRoutine();
            StartCoroutine(powerUpRoutine);
        }
        else
        {
            Debug.Log("ActivateGameTimer: " + activate.ToString());
            isPowerUpRoutineRunning = false;
            StopCoroutine(powerUpRoutine);
        }
    }
    IEnumerator PowerUpUsageRoutine()
    {
        isPowerUpRoutineRunning = true;
        while (isPowerUpRoutineRunning)
        {
            yield return new WaitForSeconds(0.5f);
            if (myPlayerScript.myPowerUps.Count > 0 && !isPowerUpCoolDownRoutineRunning)
            {
                // Get the types of powerups that the player has
                GetTypesOfPowerUps();

                // Check if AI player has the ball
                if (myPlayerScript.selectGoblin.doesCharacterHaveBall)
                {
                    // BlueShell Lightning should be applicable in almost all scenarios? just use it if available
                    if (hasBlueShellEnemy && areEnemyGoblinsNearby && !isPowerUpCoolDownRoutineRunning)
                    {
                        UsePowerUp(blueShellEnemyIndex);
                    }
                    // Check for blue shell invincibility
                    else if (hasBlueShellSelf && areEnemyGoblinsNearby && !isPowerUpCoolDownRoutineRunning)
                        UsePowerUp(blueShellSelfIndex);
                    // Prioritize the speed powerup first
                    else if (hasSpeedPowerUp && !isPowerUpCoolDownRoutineRunning)
                        UsePowerUp(speedPowerUpIndex);
                    // Check for the stamina powerup next
                    else if (hasStaminaPowerUp && !isPowerUpCoolDownRoutineRunning && ((myPlayerScript.selectGoblin.stamina / myPlayerScript.selectGoblin.MaxStamina) <= 0.15f))
                        UsePowerUp(staminaPowerUpIndex);
                    else if (hasHealthPowerUp && !isPowerUpCoolDownRoutineRunning && ((myPlayerScript.selectGoblin.health / myPlayerScript.selectGoblin.MaxHealth) < 0.4f))
                        ShouldHealthPowerUpBeUsed();
                    // Check for defense powerups last
                    else if (hasDefensePowerUp && !isPowerUpCoolDownRoutineRunning && areEnemyGoblinsNearby)
                        UsePowerUp(defensePowerUpIndex);
                }
                else if (!myPlayerScript.selectGoblin.doesCharacterHaveBall && gameFootball.isHeld)
                {
                    // Check if opponent goblins are nearby. If yes, use attack powerup?
                    if (hasBlueShellEnemy && !isPowerUpCoolDownRoutineRunning)
                    {
                        UsePowerUp(blueShellEnemyIndex);
                    }
                    else if (hasSpeedPowerUp && !isPowerUpCoolDownRoutineRunning)
                        UsePowerUp(speedPowerUpIndex);
                    else if (hasAttackPowerUp && !isPowerUpCoolDownRoutineRunning)
                    {
                        if (CheckForNearbyGoblinsToAttack() && !isPowerUpCoolDownRoutineRunning)
                            UsePowerUp(attackPowerUpIndex);
                    }
                    // Check if an enemy goblin is directly in front of any goblins. If yes, use bottle powerup
                    else if (hasBottlePowerUp && !isPowerUpCoolDownRoutineRunning)
                    {
                        if (CheckForLineOfSightForBottlePowerUp() && !isPowerUpCoolDownRoutineRunning)
                            UsePowerUp(bottlePowerUpIndex);
                    }

                }
                // If a powerup still hasn't been used during this check, make additional checks for average goblin health/stamina/whatever and use appropriate powerup?
                else if(!isPowerUpCoolDownRoutineRunning)
                {
                    // Make checks like: Is average health/stamina of goblins below 50%?

                    // Check health first:
                    if (hasBlueShellEnemy && !isPowerUpCoolDownRoutineRunning)
                    {
                        UsePowerUp(blueShellEnemyIndex);
                    }
                    else if (hasHealthPowerUp && !isPowerUpCoolDownRoutineRunning)
                        ShouldHealthPowerUpBeUsed();
                    else if (hasStaminaPowerUp && !isPowerUpCoolDownRoutineRunning)
                        ShouldStaminaPowerUpBeUsed();

                }
            }
        }
        yield break;
    }
    void GetTypesOfPowerUps()
    {
        // reset list of powerup types?
        hasDefensePowerUp = false;
        defensePowerUpIndex = 0;
        hasAttackPowerUp = false;
        attackPowerUpIndex = 0;
        hasSpeedPowerUp = false;
        speedPowerUpIndex = 0;
        hasStaminaPowerUp = false;
        staminaPowerUpIndex = 0;
        hasHealthPowerUp = false;
        healthPowerUpIndex = 0;
        hasBlueShellEnemy = false;
        blueShellEnemyIndex = 0;
        hasBlueShellSelf = false;
        blueShellSelfIndex = 0;
        hasBottlePowerUp = false;
        bottlePowerUpIndex = 0;

        int index = 0;

        foreach (PowerUp powerup in myPlayerScript.myPowerUps)
        {
            if (powerup.aiPowerUpType == "attack")
            {
                hasAttackPowerUp = true;
                attackPowerUpIndex = index;
            }
            else if (powerup.aiPowerUpType == "defense")
            {
                hasDefensePowerUp = true;
                defensePowerUpIndex = index;
            }
            else if (powerup.powerUpAbility == "speedNormal")
            {
                hasSpeedPowerUp = true;
                speedPowerUpIndex = index;
            }
            else if (powerup.powerUpAbility == "staminaNormal")
            {
                hasStaminaPowerUp = true;
                staminaPowerUpIndex = index;
            }
            else if (powerup.powerUpAbility == "healNormal")
            {
                hasHealthPowerUp = true;
                healthPowerUpIndex = index;
            }
            else if (powerup.aiPowerUpType == "blueShellSelf")
            {
                hasBlueShellSelf = true;
                blueShellSelfIndex = index;
            }
            else if (powerup.aiPowerUpType == "blueShellEnemy")
            {
                hasBlueShellEnemy = true;
                blueShellEnemyIndex = index;
            }
            else if (powerup.powerUpAbility == "bottleNormal")
            {
                hasBottlePowerUp = true;
                bottlePowerUpIndex = index;
            }

            index++;
        }
    }
    /*void UseDefensePowerUps()
    {
        Debug.Log("AIPlayer: UseDefensePowerUps with index of : " + speedPowerUpIndex.ToString());
        isPowerUpCoolDownRoutineRunning = true;
        powerUpCoolDown = PowerUpCoolDownRoutine();
        StartCoroutine(powerUpCoolDown);
        myPlayerScript.UsePowerUp(defensePowerUpIndex);
    }
    void UseSpeedPowerUp()
    {
        Debug.Log("AIPlayer: UseSpeedPowerUp with index of : " + speedPowerUpIndex.ToString());
        isPowerUpCoolDownRoutineRunning = true;
        powerUpCoolDown = PowerUpCoolDownRoutine();
        StartCoroutine(powerUpCoolDown);
        myPlayerScript.UsePowerUp(speedPowerUpIndex);
    }
    void UseStaminaPowerUp()
    {
        Debug.Log("AIPlayer: UseStaminaPowerUp with index of : " + speedPowerUpIndex.ToString());
        isPowerUpCoolDownRoutineRunning = true;
        powerUpCoolDown = PowerUpCoolDownRoutine();
        StartCoroutine(powerUpCoolDown);
        myPlayerScript.UsePowerUp(staminaPowerUpIndex);
    }*/
    void UsePowerUp(int powerUpIndex)
    {
        Debug.Log("AIPlayer UsePowerUp with index: " + powerUpIndex.ToString());
        isPowerUpCoolDownRoutineRunning = true;
        // Check if the powerup has multiple uses
        if (myPlayerScript.myPowerUps[powerUpIndex].multipleUses)
        {
            powerUpCoolDown = PowerUpMultipleUsesCoolDownRoutine(powerUpIndex);
            StartCoroutine(powerUpCoolDown);
        }
        else
        {
            powerUpCoolDown = PowerUpCoolDownRoutine(powerUpIndex);
            StartCoroutine(powerUpCoolDown);
        }
    }
    IEnumerator PowerUpCoolDownRoutine(int powerUpIndex)
    {
        isPowerUpCoolDownRoutineRunning = true;
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 0.5f));
        myPlayerScript.UsePowerUp(powerUpIndex);
        yield return new WaitForSeconds(0.5f);
        isPowerUpCoolDownRoutineRunning = false;
    }
    IEnumerator PowerUpMultipleUsesCoolDownRoutine(int powerUpIndex)
    {
        isPowerUpCoolDownRoutineRunning = true;
        int numberOfUses = myPlayerScript.myPowerUps[powerUpIndex].remainingUses;
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 0.5f));
        for (int i = 0; i < numberOfUses; i++)
        {
            myPlayerScript.UsePowerUp(powerUpIndex);
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.25f, 0.5f));
        }
        yield return new WaitForSeconds(0.5f);
        isPowerUpCoolDownRoutineRunning = false;
    }
    void ShouldHealthPowerUpBeUsed()
    {
        // Go through each member of goblin team. See if anyone is below minimum health threshold as well as seeing if the team average is below a threshold
        bool goblinBelowMin = false;
        float minHealthThreshold = 0.2f;
        float combinedHealthPercentage = 0f;
        foreach (GoblinScript goblin in myPlayerScript.goblinTeam)
        {
            float goblinHealthPercent = goblin.health / goblin.MaxHealth;
            if (goblin.isGoblinKnockedOut)
            {
                goblinBelowMin = true;
                break;
            }
            if (goblinHealthPercent <= minHealthThreshold)
            {
                goblinBelowMin = true;
                break;
            }
            combinedHealthPercentage += goblinHealthPercent;
        }
        if (goblinBelowMin || (combinedHealthPercentage / 3) < 0.5f)
        {
            UsePowerUp(healthPowerUpIndex);
        }
    }
    void ShouldStaminaPowerUpBeUsed()
    {
        // Go through each member of goblin team. See if anyone is below minimum Stamina threshold as well as seeing if the team average is below a threshold
        bool goblinBelowMin = false;
        float minStaminaThreshold = 0.15f;
        float combinedStaminaPercentage = 0f;
        foreach (GoblinScript goblin in myPlayerScript.goblinTeam)
        {
            float goblinStaminaPercent = goblin.stamina / goblin.MaxStamina;
            if (goblinStaminaPercent <= minStaminaThreshold)
            {
                goblinBelowMin = true;
                break;
            }
            combinedStaminaPercentage += goblinStaminaPercent;
        }
        if (goblinBelowMin || (combinedStaminaPercentage / 3) < 0.5f)
        {
            UsePowerUp(staminaPowerUpIndex);
        }
    }
    // Loop through each goblin on the goblin team. If any goblins are near an enemy goblin, return true so that the attack powerup is used
    bool CheckForNearbyGoblinsToAttack()
    {
        bool goblinsNearby = false;

        foreach (GoblinScript goblin in myPlayerScript.goblinTeam)
        {
            GetSurroundingGoblins(goblin.transform.position, goblin.isGoblinGrey);
            if (areEnemyGoblinsNearby)
            {
                goblinsNearby = true;
                break;
            }
        }

        return goblinsNearby;
    }
    // Loop through each goblin on team. Do a raycast to see if an enemy is directly ahead of them. If yes, switch to that goblin (if necessary) and use the bottle powerup
    // Add check to make sure a teammate isn't in the line of sight so the AI doesn't knock out teammate?
    bool CheckForLineOfSightForBottlePowerUp()
    {
        bool useBottle = false;
        bool wasTeammateHit = false;
        float distToTeammate = 0f;
        float distToEnemy = 0f;
        foreach (GoblinScript goblin in myPlayerScript.goblinTeam)
        {
            Vector2 directionToCast = new Vector2(1f, 0f);
            if(goblin.myRenderer.flipX)
                directionToCast = new Vector2(-1f, 0f);

            // Reset the usebottle and wasteammatehit variables for each goblin before doing the circlecast
            useBottle = false;
            wasTeammateHit = false;
            distToEnemy = 100f;
            distToTeammate = 100f;

            // Send out a ray cast and see if an enemy goblin is directly in front of any teammates
            RaycastHit2D[] hits = Physics2D.CircleCastAll(goblin.transform.position, 0.5f, directionToCast, 6f, goblinBodyLayerMask);
            if (hits.Length > 0)
            {
                for (int j = 0; j < hits.Length; j++)
                {
                    GoblinScript goblinHit = hits[j].collider.transform.parent.GetComponent<GoblinScript>();
                    if (goblin == goblinHit)
                        continue;
                    else if (goblin.isGoblinGrey != goblinHit.isGoblinGrey)
                    {
                        // The bottle is thrown from the selected goblin. So, if the goblin that has an enemy in front of them is not selected, switch to them
                        if (goblin.isEGoblin)
                        {
                            myPlayerScript.SwitchToEGoblin(false, Time.time);
                        }
                        else if (goblin.isQGoblin)
                        {
                            myPlayerScript.SwitchToQGoblin(false, Time.time);
                        }
                        useBottle = true;
                        //break;
                        // save the closest enemy
                        if(distToEnemy > Vector2.Distance(goblin.transform.position, goblinHit.transform.position))
                            distToEnemy = Vector2.Distance(goblin.transform.position, goblinHit.transform.position);
                    }
                    else if (goblin.isGoblinGrey == goblinHit.isGoblinGrey)
                    {
                        // If a teammmate is in front of the bottle thrower, don't throw
                        Debug.Log("CheckForLineOfSightForBottlePowerUp: teammate is in the way of bottle throw!");
                        wasTeammateHit = true;
                        // Save distance to closest teammate
                        if(distToTeammate > Vector2.Distance(goblin.transform.position, goblinHit.transform.position))
                            distToTeammate = Vector2.Distance(goblin.transform.position, goblinHit.transform.position);
                        //break;
                    }
                }
            }
            // If an enemy is hit and a teammate was not, stop looping through goblin teammates and throw bottle from goblin?
            if (useBottle && !wasTeammateHit)
                break;
            else if (useBottle && wasTeammateHit && distToTeammate > distToEnemy)
            {
                Debug.Log("CheckForLineOfSightForBottlePowerUp: teammate was hit but distance to teammate is: " + distToTeammate.ToString() + " and distance to enemy is: " + distToEnemy.ToString());
                break;
            }
            else
                useBottle = false;
        }
        return useBottle;
    }
    public Vector2 DegreeToVector2(float degree)
    {
        return RadianToVector2(degree * Mathf.Deg2Rad);
    }
    public Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
    public void StopBlockingGoblinFromRunning()
    {
        foreach (GoblinScript goblin in myPlayerScript.goblinTeam)
        {
            if (goblin.soundType != "skirmisher")
                continue;
            goblin.RpcResetGoblinStatuses();
        }
    }
}
