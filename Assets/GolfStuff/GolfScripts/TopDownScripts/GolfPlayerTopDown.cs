using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfPlayerTopDown : MonoBehaviour
{
    [SerializeField] public GolfBallTopDown myBall;

    [Header("Hit Values")]
    [SerializeField] public Vector2 hitDirection = Vector2.zero;
    [SerializeField] public float hitDistance = 0f; // changes based on type of club used. Clubs will have a "max distance" and the distance hit will be a % of the max distance based on the player submitted distance
    [SerializeField] public float hitAngle = 45f; // changes based on the type of club used? Drivers lower angles. Irons middle angles. Wedges higher angles?
    [SerializeField][Range(-10f,5f)] public float hitTopSpin = 0f;
    [SerializeField] [Range(-5f, 5f)] public float hitLeftOrRightspin = 0f;
    public bool IsShanked = false;

    [Header("Trajectory Drawing Stuff")]
    [SerializeField] DrawTrajectoryTopDown drawTrajectoryTopDown;
    [SerializeField] SpriteRenderer _landingTargetSprite;
    public Vector3[] trajectoryPoints = new Vector3[3];
    public Vector2 previousHitDirection;
    public float previousHitDistance;
    public float previousHitAngle;
    public float previousHitTopSpin;
    public float previousHitLeftOrRightSpin;

    [Header("Line Renderers")]
    [SerializeField] LineRenderer trajectoryLineObject;
    [SerializeField] LineRenderer trajectoryShadowLineObject;

    [Header("Player Input")]
    public float aimLeftRight = 0f;
    public Vector2 perpendicular;
    public float turnRate = 0.5f;
    float distanceUpDown = 0f;
    [SerializeField] float _changeDistanceRate = 5f;

    [Header("Player Turn")]
    public bool IsPlayersTurn = false;

    [Header("Hit Meter Objects")]
    [SerializeField] GameObject _hitMeterObject;
    [SerializeField] GameObject _hitMeterMovingIcon;
    [SerializeField] GameObject _hitMeterPowerSubmissionIcon;
    [SerializeField] GameObject _hitMeterAccuracySubmissionIcon;
    [SerializeField] GameObject _adjustedDistanceIcon;

    [Header("Hit Meter Positions")]
    [SerializeField] float _maxDistancePosition;
    [SerializeField] float _centerAccuracyPosition;
    [SerializeField] float _furthestLeftAccuracyPosition;
    [SerializeField] float _furthestRightAccuracyPosition;
    [SerializeField] float _distancePositionLength;
    [SerializeField] float _accuracyRange;

    [Header("Hit Spin Icon and Stuff")]
    [SerializeField] SpinIcon _spinIcon;
    [SerializeField] public float TopSpinPositiveModifer = 5f;
    [SerializeField] public float TopSpinNegativeModifer = 10f;

    [Header("Hit Meter Direction and Stuff")]
    [SerializeField] float _moveSpeed;
    public bool _moveHitMeterIcon = false;
    bool _moveRight = false;
    public bool _powerSubmitted = false;
    public bool _accuracySubmitted = false;

    [Header("Hit Meter Submissions")]
    public float HitPowerSubmitted;
    public float HitAccuracySubmitted;
    public Vector2 ModifiedHitDirection = Vector2.zero;
    public Vector2 hitTopSpinSubmitted = Vector2.zero;

    [Header("Player Turn Attributes")]
    public float MaxDistanceFromClub = 100f;
    public float SpinDividerFromClub = 1f;
    public float DefaultLaunchAngleFromClub = 24f;
    public float MinDistance;
    public bool DidPlayerAdjustDistance = false;
    public float TargetDistanceXPosForPlayer = 0f;
    public float MaxTopSpinFromClub;
    public float MaxBackSpinFromClub;
    public float MaxSideSpinFromClub;
    [SerializeField] public float RoughTerrainDistModifer = 0.75f;
    //[SerializeField] public float TrapTerrainDistModifer = 0.5f;

    [Header("Club Info")]
    [SerializeField] ClubTopDown[] _myClubs;
    public ClubTopDown CurrentClub;
    [SerializeField] int _currentClubIndex;

    [Header("Club Selection UI Stuff")]
    [SerializeField] GameObject _clubSelectionHolder;
    [SerializeField] SpriteRenderer _selectedClubTextImage;
    [SerializeField] SpriteRenderer _selectedClubImage;

    [Header("Wind UI")]
    [SerializeField] GameObject _windUIHolder;

    [Header("Camera")]
    public Camera myCamera;
    public CameraFollowScript cameraFollowScript;

    // Start is called before the first frame update
    void Start()
    {
        myBall.myPlayer = this;
        myCamera = Camera.main;
        cameraFollowScript = GameObject.FindGameObjectWithTag("camera").GetComponent<CameraFollowScript>();
        //AttachUIToNewParent(myCamera.transform);
        StartGameWithDriver();
        drawTrajectoryTopDown.SetLineWidth(myBall.pixelUnit * 2f);
    }

    // Update is called once per frame
    void Update()
    {

        if (!myBall.isHit && !myBall.isBouncing && !myBall.isRolling)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //myBall.HitBall(hitDistance, hitAngle, hitTopSpin, hitDirection);
                //EnableOrDisableLineObjects(false);
                if (!IsPlayersTurn && !_moveHitMeterIcon)
                    StartPlayerTurn();
                else if (IsPlayersTurn && !_moveHitMeterIcon)
                    StartHitMeterSequence();
                else if (_moveHitMeterIcon)
                {
                    if (!_powerSubmitted)
                    {
                        SetHitPowerValue();
                    }
                    else if (_powerSubmitted && !_accuracySubmitted)
                    {
                        SetHitAccuracyValue();
                        // Later SubmitHitToBall() will be called by the animation instead of right here:
                        SubmitHitToBall();
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.LeftControl) && !IsPlayersTurn)
            {
                myBall.ResetPosition();
                EnableOrDisableLineObjects(true);
            }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                //myBall.PuttBall(hitDirection, hitDistance);
                //EnableOrDisableLineObjects(false);
                if (IsPlayersTurn && !_moveHitMeterIcon)
                {
                    ChangeCurrentClub();
                }
            }

            if (_moveHitMeterIcon)
            {
                if (_powerSubmitted && _accuracySubmitted)
                {
                    _moveHitMeterIcon = false;
                    ActivateMovingIcon(false);
                }
                else
                {
                    MoveHitMeterIcon();
                }
                
            }
            else
            {
                aimLeftRight = Input.GetAxisRaw("Horizontal");
                if (aimLeftRight != 0)
                {
                    ChangeHitDirection(aimLeftRight);
                }
                distanceUpDown = Input.GetAxisRaw("Vertical");
                if (distanceUpDown != 0)
                {
                    ChangeHitDistance(distanceUpDown);
                }
            }
            
        }

    }
    private void FixedUpdate()
    {
        if ((hitDistance != previousHitDistance || hitAngle != previousHitAngle || hitTopSpin != previousHitTopSpin || hitDirection != previousHitDirection || hitLeftOrRightspin != previousHitLeftOrRightSpin) && (!myBall.isHit && !myBall.isBouncing && !myBall.isRolling && IsPlayersTurn))
        {
            // Get the trajaectory line
            if (CurrentClub.ClubType != "putter")
                trajectoryPoints = myBall.CalculateHitTrajectory(hitDistance, hitAngle, hitTopSpin, hitLeftOrRightspin, hitDirection, Vector3.zero, 0f);
            else
                trajectoryPoints = myBall.CalculatePutterTrajectoryPoints(hitDistance, hitDirection);
            // Draw the trajectory
            drawTrajectoryTopDown.UpdateTrajectory(trajectoryPoints, myBall,CurrentClub.ClubType, hitDistance);
            UpdateCameraFollowTarget(_landingTargetSprite.gameObject);



            // Reset the "previous" data
            previousHitDirection = hitDirection;
            previousHitDistance = hitDistance;
            previousHitAngle = hitAngle;
            previousHitTopSpin = hitTopSpin;
            previousHitLeftOrRightSpin = hitLeftOrRightspin;
        }
    }
    public void EnableOrDisableLineObjects(bool enable)
    {
        trajectoryLineObject.enabled = true;
        trajectoryShadowLineObject.enabled = true;
        _landingTargetSprite.enabled = true;
    }
    public void ResetPreviousHitValues()
    {
        previousHitDistance = 0f;
        previousHitAngle = 0f;
        previousHitTopSpin = 0f;
        previousHitDirection = Vector2.zero;
    }
    void ChangeHitDirection(float direction)
    {
        //Vector2 perpendicular = Vector2.Perpendicular(hitDirection);
        perpendicular = Vector2.Perpendicular(hitDirection);
        if (direction > 0)
            perpendicular *= -1f;

        turnRate = 2.0f / (hitDistance + 0.1f);
        hitDirection += perpendicular * Time.deltaTime * turnRate;
        
        hitDirection = hitDirection.normalized;

        //turnRate = 0.5f / (hitDistance + 0.1f);

    }
    void ChangeHitDistance(float changeDirection)
    {
        hitDistance += changeDirection * Time.deltaTime * _changeDistanceRate;
        if (hitDistance > MaxDistanceFromClub)
            hitDistance = MaxDistanceFromClub;
        if (hitDistance <= MinDistance)
            hitDistance = MinDistance;

        // Adjust the location of the Adjusted Distance Icon
        UpdatePositionOfAdjustedDistanceIcon(hitDistance);
    }
    void StartPlayerTurn()
    {
        // IsPlayersTurn will be set by the game manager in a real game. This is just a place holder for now.
        this.IsPlayersTurn = true;
        SetCameraOnPlayer();
        AttachUIToNewParent(myCamera.transform);
        DeactivateIconsForNewTurn();
        UpdateBallGroundMaterial();
        GetNewClubAttributes(CurrentClub);
        GetHitStatsFromClub();
        EnableOrDisableLineObjects(true);
        //_hitMeterObject.SetActive(true);
        ActivateHitUIObjects(true);
        DidPlayerAdjustDistance = false;
        ResethitTopSpinForNewTurn();
    }
    void SetCameraOnPlayer()
    {
        // Update player so they are at the same position of the camera
        UpdateCameraFollowTarget(myBall.myBallObject);
        //cameraFollowScript.followTarget = myBall.myBallObject;
        Vector3 cameraPos = myCamera.transform.position;
        this.transform.position = new Vector3(cameraPos.x, cameraPos.y, 0f);
        //AttachPlayerToCamera(true, myCamera.transform);
    }
    void DeactivateIconsForNewTurn()
    {
        _hitMeterMovingIcon.SetActive(false);
        _hitMeterPowerSubmissionIcon.SetActive(false);
        _hitMeterAccuracySubmissionIcon.SetActive(false);
    }
    void ActivateHitUIObjects(bool enable)
    {
        _hitMeterObject.SetActive(enable);
        _spinIcon.gameObject.SetActive(enable);
        _clubSelectionHolder.SetActive(enable);
    }
    void GetHitStatsFromClub()
    {
        // code to get the max distance from different club objects here. Right now just a place holder
        hitDistance = MaxDistanceFromClub;
        hitAngle = DefaultLaunchAngleFromClub;
        //SpinDividerFromClub = 1f; // get this from the actual club object later?
        UpdatePositionOfAdjustedDistanceIcon(hitDistance);
        MinDistance = GetMinDistance(hitDistance);
        // maybe also set the icon movement speed based on the club?
    }
    float GetMinDistance(float distance)
    {
        float minDistPercentage = (_centerAccuracyPosition - (_furthestLeftAccuracyPosition - myBall.pixelUnit)) / _distancePositionLength;
        float minDistance = distance * minDistPercentage;


        Debug.Log("GetMinDistance: min distance: " + minDistance);
        return minDistance;
    }
    void UpdatePositionOfAdjustedDistanceIcon(float newDistance)
    {
        if (newDistance == MaxDistanceFromClub)
        {
            _adjustedDistanceIcon.SetActive(false);
            //DidPlayerAdjustDistance = false;
        }
        else
        {
            _adjustedDistanceIcon.SetActive(true);
            //DidPlayerAdjustDistance = true;
        }
            

        // Get the new distance as a percentage of the max distance
        float distancePercentage = newDistance / MaxDistanceFromClub;
        if (distancePercentage > 1.0f)
            distancePercentage = 1.0f;
        float adjustedDistancePosition = _maxDistancePosition + (_distancePositionLength - (_distancePositionLength * distancePercentage));

        // Update the local postion of the adjustedDistanceIcon
        //Vector2 newPos = _adjustedDistanceIcon.transform.localPosition;
        //newPos.x = adjustedDistancePosition;
        //_adjustedDistanceIcon.transform.localPosition = newPos;
        _adjustedDistanceIcon.transform.localPosition = new Vector3(adjustedDistancePosition,0f,0f);
        TargetDistanceXPosForPlayer = adjustedDistancePosition;

    }
    void StartHitMeterSequence()
    {
        // Placeholder until the actual hit meter stuff is added
        //myBall.HitBall(hitDistance, hitAngle, hitTopSpin, hitDirection);
        ResetIconPositions();
        ResetSubmissionValues();
        BeginMovingHitMeter();

        // IsPlayersTurn will be set by the game manager in a real game. This is just a place holder for now.
        this.IsPlayersTurn = false;
    }
    void ResetIconPositions()
    {
        _hitMeterMovingIcon.transform.localPosition = new Vector3(_centerAccuracyPosition, 0f, 0f);
        _hitMeterPowerSubmissionIcon.transform.localPosition = Vector3.zero;
        _hitMeterAccuracySubmissionIcon.transform.localPosition = Vector3.zero;

        //_hitMeterMovingIcon.SetActive(true);
        ActivateMovingIcon(true);
    }
    void ResetSubmissionValues()
    {
        _powerSubmitted = false;
        _accuracySubmitted = false;
        IsShanked = false;
    }
    void BeginMovingHitMeter()
    {
        _moveRight = false;
        _moveHitMeterIcon = true;
    }
    void MoveHitMeterIcon()
    {
        Vector3 newPos = _hitMeterMovingIcon.transform.localPosition;
        int moveDirection = -1;
        if (_moveRight)
            moveDirection = 1;

        newPos.x += _moveSpeed * Time.deltaTime * moveDirection;

        if (newPos.x < _maxDistancePosition)
        {
            newPos.x = _maxDistancePosition;
            _moveRight = true;
        }
        else if (newPos.x > _furthestRightAccuracyPosition)
        {
            newPos.x = _furthestRightAccuracyPosition;
            _moveHitMeterIcon = false;
            ActivateMovingIcon(false);
            if (!_accuracySubmitted)
            {
                Debug.Log("MoveHitMeterIcon: Accuracy meter off the right edge without accuracy submitted by player. SHANKED!!!");
                IsShanked = true;
                SubmitHitToBall();
            }
                
        }

        _hitMeterMovingIcon.transform.localPosition = newPos;
    }
    void SetHitPowerValue()
    {
        Debug.Log("SetHitPowerValue");
        float iconXPosition = GetMovingIconXPosition();
        // Check if player was close to their target? If they are close enough, give it to them!

        if (IsCloseEnoughToTargetPosition(TargetDistanceXPosForPlayer, iconXPosition))
            iconXPosition = TargetDistanceXPosForPlayer;

        ActivateSubmissionIcon(_hitMeterPowerSubmissionIcon, iconXPosition);
        HitPowerSubmitted = GetHitPowerFromXPosition(iconXPosition, MaxDistanceFromClub);
        _powerSubmitted = true;

    }
    float GetMovingIconXPosition()
    {
        float xPos = _hitMeterMovingIcon.transform.localPosition.x;
        /*float minXToGetMaxHit = _maxDistancePosition + (myBall.pixelUnit * 2);
        if (xPos <= minXToGetMaxHit)
        {
            Debug.Log("GetMovingIconXPosition: Got a max hit! Real x position: " + xPos.ToString()); 
            xPos = _maxDistancePosition;
        }*/

        if (xPos <= _maxDistancePosition)
            xPos = _maxDistancePosition;
        if (xPos >= _furthestRightAccuracyPosition)
            xPos = _furthestRightAccuracyPosition;
            
        Debug.Log("GetMovingIconXPosition: " + xPos.ToString());
        return xPos;
    }
    void ActivateSubmissionIcon(GameObject submissionIcon, float xPos)
    {
        Debug.Log("ActivateSubmissionIcon: for x position: " + xPos);
        submissionIcon.transform.localPosition = new Vector3(xPos, 0f, 0f);
        submissionIcon.SetActive(true);
    }
    float GetHitPowerFromXPosition(float xPos, float maxHitDistance)
    {
        float hitPowerFromXPosition = 0f;

        float distFromZero = _centerAccuracyPosition - xPos;
        float distPercentage = distFromZero / _distancePositionLength;
        hitPowerFromXPosition = maxHitDistance * distPercentage;
        Debug.Log("GetHitPowerFromXPosition: " + hitPowerFromXPosition.ToString());
        return hitPowerFromXPosition;
    }
    void SetHitAccuracyValue()
    {
        Debug.Log("SetHitAccuracyValue");
        float iconXPosition = GetMovingIconXPosition();

        if (IsCloseEnoughToTargetPosition(_centerAccuracyPosition, iconXPosition))
            iconXPosition = _centerAccuracyPosition;
        ActivateSubmissionIcon(_hitMeterAccuracySubmissionIcon, iconXPosition);

        float accuracyDistance = GetAccuracyDistance(iconXPosition, _centerAccuracyPosition);
        if (!IsShanked)
        {
            ModifiedHitDirection = ModifyHitDirectionFromAccuracy(hitDirection,accuracyDistance);
        }
        _accuracySubmitted = true;
    }
    bool IsCloseEnoughToTargetPosition(float targetPosition, float submittedPosition)
    {
        bool isCloseEnoughToTargetPosition = false;

        float diff = Mathf.Abs(targetPosition - submittedPosition);
        if (diff <= (myBall.pixelUnit * 2))
            isCloseEnoughToTargetPosition = true;

        Debug.Log("IsCloseEnoughToTargetPosition: returning: " + isCloseEnoughToTargetPosition.ToString() + " target position: " + targetPosition.ToString() + " submitted position: " + submittedPosition.ToString());
        return isCloseEnoughToTargetPosition;
    }
    float GetAccuracyDistance(float submittedPosition, float basePosition)
    {
        float accuracyDistance = submittedPosition - basePosition;

        if (accuracyDistance > _accuracyRange)
            accuracyDistance = _accuracyRange;
        if (accuracyDistance < -_accuracyRange)
        {
            Debug.Log("GetAccuracyDistance: Shanked!!! accuracy distance was: " + accuracyDistance.ToString() + " submitted position: " + submittedPosition.ToString() + " base position: " + basePosition.ToString());
            accuracyDistance = -_accuracyRange;
            IsShanked = true;
        }

        Debug.Log("GetAccuracyDistance: accuracy distance is: " + accuracyDistance.ToString());
        return accuracyDistance;
    }
    Vector2 ModifyHitDirectionFromAccuracy(Vector2 direction, float accuracyDistance)
    {
        return direction;
        Vector2 newDir = Vector2.zero;

        // If the hit is perfectly accurate, don't adjust the direction from where the player aimed
        if (accuracyDistance == 0)
            return direction;

        accuracyDistance *= -2.5f;
        // Punish innaccurate putts more than regular hits?
        if (CurrentClub.ClubType == "putter")
            accuracyDistance *= 2.5f;
		
		// https://www.youtube.com/watch?v=HH6JzH5pTGo
        var rotation = Quaternion.AngleAxis(accuracyDistance, Vector3.forward);
        newDir = (rotation * direction).normalized;
        Debug.Log("ModifyHitDirectionFromAccuracy: The new direction is: " + newDir.ToString() + " based on the original direction: " + direction.ToString() + " rotate " + accuracyDistance.ToString() + " degrees from rotation of: " + rotation.ToString());
        return newDir;
    }
    void ActivateMovingIcon(bool enable)
    {
        _hitMeterMovingIcon.SetActive(enable);
    }
    void SubmitHitToBall()
    {
        if (!IsShanked)
        {
            if (CurrentClub.ClubType == "putter")
            {
                myBall.PuttBall(ModifiedHitDirection, HitPowerSubmitted);
                //myBall.PuttBall(new Vector2 (1f,0f), 20f);
            }
            else
            {
                myBall.HitBall(HitPowerSubmitted, hitAngle, hitTopSpin, ModifiedHitDirection, hitLeftOrRightspin);
                //myBall.HitBall(100, hitAngle, hitTopSpin, new Vector2(1f,0f), hitLeftOrRightspin);
            }
        }
        else
        {
            Debug.Log("Was ball shanked? " + IsShanked.ToString());
            DoTheShank();
            //myBall.HitBall(100, hitAngle, hitTopSpin, new Vector2(1f, 0f), hitLeftOrRightspin);
        }
        ActivateHitUIObjects(false);
        AttachUIToNewParent(this.transform);
        //AttachPlayerToCamera(false, myCamera.transform);
        UpdateCameraFollowTarget(myBall.myBallObject);
        EnableOrDisableLineObjects(false);
    }
    public void UpdateHitSpinForPlayer(Vector2 newSpin)
    {
        //hitTopSpinSubmitted = newSpin.normalized;
        hitTopSpinSubmitted = newSpin;
        //Debug.Log("UpdateHitSpinForPlayer: new spin submitted: " + newSpin.ToString() + " hitTopSpinSubmitted will then be: " + hitTopSpinSubmitted.ToString());
        UpdateTopSpin(hitTopSpinSubmitted.y);
        UpdateLeftRightSpin(hitTopSpinSubmitted.x);
    }
    void UpdateTopSpin(float newTopSpin)
    {
        // Instead of using these TopSpinPositiveModifer and TopSpinNegativeModifer values, a range of possible values for top/back spin should be calculated based on the clubs Max top/back spin values
        Debug.Log("UpdateTopSpin: newTopSpin:" + newTopSpin.ToString());
        if (newTopSpin == 0f)
            hitTopSpin = 0f;

        if (newTopSpin > 0)
            hitTopSpin = MaxTopSpinFromClub * newTopSpin;
        else
            hitTopSpin = MaxBackSpinFromClub * -newTopSpin; // flipping the negative because "MaxBackSpinFromClub" will be a negative value. Maybe change that in the Club script/prefab so MaxBackSpinFromClub positive...(make it the aboslute value of the max back spin) [wait don't do that since elsewhere the negative value is used for the range of back/top spin and other calculations or whatever]

        // Old way that didn't calculate the values based on the max ranges from the club for spins
        /*else if (newTopSpin > 0)
        {
            hitTopSpin = (newTopSpin * TopSpinPositiveModifer) / SpinDividerFromClub;
        }
        else if (newTopSpin < 0)
        {
            hitTopSpin = (newTopSpin * TopSpinNegativeModifer) / SpinDividerFromClub;
        }*/

        AdjustLaunchAngleFromTopSpin(hitTopSpin);
    }
    void UpdateLeftRightSpin(float newLeftOrRightSpin)
    {
        if (newLeftOrRightSpin == 0f)
            hitLeftOrRightspin = 0f;

        hitLeftOrRightspin = MaxSideSpinFromClub * newLeftOrRightSpin;

        // Old way that didn't calculate the values based on the max ranges from the club for spins
        /*else
        {
            hitLeftOrRightspin = (newLeftOrRightSpin * TopSpinPositiveModifer) / SpinDividerFromClub;
        }*/

    }
    void AdjustLaunchAngleFromTopSpin(float spin)
    {
        if (spin == 0)
        {
            hitAngle = DefaultLaunchAngleFromClub;
            return;
        }
        else if (spin > 0)
        {
            hitAngle = DefaultLaunchAngleFromClub * (1f - ((spin * 2f) / 100f));
        }
        else
        {
            hitAngle = DefaultLaunchAngleFromClub * (1f + ((spin * -1f) / 100f));
        }
            
    }
    void ResethitTopSpinForNewTurn()
    {
        hitTopSpinSubmitted = Vector2.zero;
        _spinIcon.ResetIconPosition();
    }
    void StartGameWithDriver()
    {
        for (int i = 0; i < _myClubs.Length; i++)
        {
            if (_myClubs[i].ClubType == "driver")
            {
                CurrentClub = _myClubs[i];
                _currentClubIndex = i;
                SetSelectedClubUI(CurrentClub);
                break;
            }
        }
    }
    void ChangeCurrentClub()
    {
        // Make sure the ground material for the ball is up-to-date so you select the right clubS? May only be necessary for testing right now...
        UpdateBallGroundMaterial();
        int oldIndex = _currentClubIndex;
        _currentClubIndex++;
        if (_currentClubIndex >= _myClubs.Length)
            _currentClubIndex = 0;
        if (!CanClubBeUsedOnCurrentGround(_myClubs[_currentClubIndex]))
        {
            _currentClubIndex = GetFirstClubThatCanHitOnThisGround();
        }
        CurrentClub = _myClubs[_currentClubIndex];

        GetNewClubAttributes(CurrentClub);
        GetHitStatsFromClub();

        // Maybe remove this later so the spin isn't always reset when the club is changed, but rather the spin values are updated to accomadate the new club?
        //ResethitTopSpinForNewTurn();

        // Update the Club UI stuff
        SetSelectedClubUI(CurrentClub);

        // Update spin values to adjust for new clubs
        if (hitTopSpin > 0)
            UpdateTopSpin(hitTopSpin / _myClubs[oldIndex].MaxTopSpin);
        else
            UpdateTopSpin(hitTopSpin / _myClubs[oldIndex].MaxBackSpin);
        UpdateLeftRightSpin(hitLeftOrRightspin / _myClubs[oldIndex].MaxSideSpin);
    }
    void SetSelectedClubUI(ClubTopDown club)
    {
        _selectedClubTextImage.sprite = club.ClubTextSprite;
        _selectedClubImage.sprite = club.ClubImageSprite;
    }
    void GetNewClubAttributes(ClubTopDown club)
    {
        MaxDistanceFromClub = GetDistanceFromClub(club);
        DefaultLaunchAngleFromClub = GetLaunchAngleFromClub(club);
        SpinDividerFromClub = GetSpinDividerFromClub(club);
        MaxTopSpinFromClub = GetMaxTopSpinFromClub(club);
        MaxBackSpinFromClub = GetMaxBackSpinFromClub(club);
        MaxSideSpinFromClub = GetMaxSideSpinFromClub(club);
    }
    float GetDistanceFromClub(ClubTopDown club)
    {
        float dist = club.MaxHitDistance;

        // code here to adjust hit distance if it is in rough terrain or a trap
        if (myBall.bounceContactGroundMaterial == "rough")
        {
            dist *= club.RoughTerrainDistModifer;
        }
        else if (myBall.bounceContactGroundMaterial.Contains("trap") && club.ClubType != "wedge")
        { 
            dist *= club.TrapTerrainDistModifer;
        }

        return dist;
    }
    float GetLaunchAngleFromClub(ClubTopDown club)
    {
        float angle = club.DefaultLaunchAngle;
        return angle;
    }
    float GetSpinDividerFromClub(ClubTopDown club)
    {
        float spinDiv = club.SpinDivider;
        return spinDiv;
    }
    float GetMaxTopSpinFromClub(ClubTopDown club)
    {
        float topMax = club.MaxTopSpin;
        return topMax;
    }
    float GetMaxBackSpinFromClub(ClubTopDown club)
    {
        float backMax = club.MaxBackSpin;
        return backMax;
    }
    float GetMaxSideSpinFromClub(ClubTopDown club)
    {
        float sideMax = club.MaxSideSpin;
        return sideMax;
    }
    void UpdateBallGroundMaterial()
    {
        myBall.UpdateGroundMaterial();
    }
    bool CanClubBeUsedOnCurrentGround(ClubTopDown club)
    {
        if (myBall.bounceContactGroundMaterial == "rough")
        {
            if (club.ClubType == "putter")
                return false;
        }
        if (myBall.bounceContactGroundMaterial.Contains("trap"))
        {
            if (club.ClubType == "putter" || club.ClubType == "driver")
                return false;
        }
        return true;
    }
    int GetFirstClubThatCanHitOnThisGround()
    {
        int firstIndex = 0;

        for (int i = 0; i < _myClubs.Length; i++)
        {
            if (CanClubBeUsedOnCurrentGround(_myClubs[i]))
            {
                return i;
            }
        }

        return firstIndex;
    }
    void DoTheShank()
    {
        Debug.Log("DoTheShank");
        HitPowerSubmitted = ShankDistance(MinDistance);
        ModifiedHitDirection = ShankDirection(hitDirection);
        // If this is a putt, there is enough information to complete the shanked putt
        // if it is a normal hit, get spin info as well before hitting
        if (CurrentClub.ClubType == "putter")
        {
            Debug.Log("DoTheShank: Putting with new direction of: " + ModifiedHitDirection.ToString() + " and new power of: " + HitPowerSubmitted.ToString());
            myBall.PuttBall(ModifiedHitDirection, HitPowerSubmitted);
        }
        else
        {
            float shankAngle = ShankAngle(hitAngle);
            float shankTopSpin = ShankTopSpin(MaxBackSpinFromClub,MaxTopSpinFromClub);
            float shankSideSpin = ShankSideSpin(-MaxSideSpinFromClub, MaxSideSpinFromClub);
            Debug.Log("DoTheShank: Hitting with new direction of: " + ModifiedHitDirection.ToString() + " and new power of: " + HitPowerSubmitted.ToString() + " and new launch angle of: " + shankAngle.ToString() + " and new top spin of: " + shankTopSpin.ToString() + " and new side spin of: " + shankSideSpin.ToString());
            myBall.HitBall(HitPowerSubmitted, shankAngle, shankTopSpin, ModifiedHitDirection, shankSideSpin);
        }

    }
    float ShankDistance(float dist)
    {
        return Random.Range((dist * 0.15f), dist);
    }
    Vector2 ShankDirection(Vector2 dir)
    {
        Vector2 shankDir = dir;

        // Get the rotation from the hit direction to shank
        float rotAngle = 90f * (Random.Range(0.1f, 0.9f));
        // Multiple the rotation by 1 or -1 randomly
        int negOrPos = UnityEngine.Random.Range(0, 2) * 2 - 1;
        rotAngle *= negOrPos;
        // Convert rotation angle value to Quaternion value to be used by unity to rotate along Z axis
        Quaternion rot = Quaternion.AngleAxis(rotAngle, Vector3.forward);

        // Get new direction based on rotation
        shankDir = (rot * dir).normalized;
        Debug.Log("ShankDirection: original direction: " + dir.ToString() + " new shanked direction: " + shankDir.ToString());

        return shankDir;
    }
    float ShankAngle(float angle)
    {
        float shankAngle = angle;
        shankAngle *= (Random.Range(0.25f, 1.25f));
        return shankAngle;
    }
    float ShankTopSpin(float maxBack, float maxTop)
    {
        return Random.Range(maxBack, maxTop);
    }
    float ShankSideSpin(float minSideSpin, float maxSideSpin)
    {
        return Random.Range(minSideSpin, maxSideSpin);
    }
    void UpdateCameraFollowTarget(GameObject objectToFollow)
    {
        cameraFollowScript.followTarget = objectToFollow;
    }
    void AttachUIToNewParent(Transform newParent)
    {
        Vector3 localTransformPosition = _hitMeterObject.transform.localPosition;
        _hitMeterObject.transform.parent = newParent;
        _hitMeterObject.transform.localPosition = localTransformPosition;
        localTransformPosition = _spinIcon.transform.localPosition;
        _spinIcon.transform.parent = newParent;
        _spinIcon.transform.localPosition = localTransformPosition;
        localTransformPosition = _clubSelectionHolder.transform.localPosition;
        _clubSelectionHolder.transform.parent = newParent;
        _clubSelectionHolder.transform.localPosition = localTransformPosition;
        localTransformPosition = _windUIHolder.transform.localPosition;
        _windUIHolder.transform.parent = newParent;
        _windUIHolder.transform.localPosition = localTransformPosition;
    }
    void AttachPlayerToCamera(bool attach, Transform newParent)
    {
        //Vector3 localTransform = this.transform.localPosition;
        if (attach)
        {
            this.transform.parent = newParent;
            this.transform.localPosition = Vector3.zero;
        }
        else
        {
            this.transform.parent = null;
            this.transform.localPosition = Vector3.zero;
        }
    }
}
