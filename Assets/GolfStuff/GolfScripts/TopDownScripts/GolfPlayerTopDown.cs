using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Managing;
using FishNet.Object.Synchronizing;
using FishNet;
//using Mirror;

public class GolfPlayerTopDown : NetworkBehaviour
{
    [Header("Player Info")]
    [SyncVar(OnChange = nameof(SyncPlayerName))] public string PlayerName;
    public GolfPlayerScore PlayerScore;
    [SerializeField] Canvas _playerCanvas;
    [SerializeField] PlayerUIMessage _playerUIMessage;
    [SyncVar(OnChange = nameof(SyncIsGameLeader))] public bool IsGameLeader = false;
    [SyncVar] public int OwnerNetId;
    [SyncVar(OnChange = nameof(SyncConnectionId))] public int ConnectionId;


    [Header("Golf Ball Stuff")]
    [SerializeField] GameObject _golfBallPrefab;
    [SerializeField] [SyncVar(OnChange = nameof(SyncMyBallNetId))] int _myBallNetId;
    [SerializeField] public GolfBallTopDown MyBall;
    [SyncVar] public float DistanceToHole;
    [SyncVar] public int NumberOfBallsSpawnedForClient = 0;


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
    [SerializeField] Collider2D _landingTargetCollider;
    public Vector3[] trajectoryPoints = new Vector3[3];
    public Vector2 previousHitDirection;
    public float previousHitDistance;
    public float previousHitAngle;
    public float previousHitTopSpin;
    public float previousHitLeftOrRightSpin;
    Vector3 _previousLandingTargetPosition; // used for tracking the landing sprite position for players who don't own this object
    Vector3 _newLandingTargetPosition;

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
    [SyncVar(OnChange = nameof(SyncIsPlayersTurn))] public bool IsPlayersTurn = false;
    public bool DirectionAndDistanceChosen = false;
    [SyncVar] public bool HasPlayerTeedOff = false;
    public bool PromptedForLightning = false;
    public bool PlayerStruckByLightning = false;

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
    [SerializeField] SpriteRenderer _puttDistanceTextImage;
    [SerializeField] Sprite _shortPuttTextImage;
    [SerializeField] Sprite _longPuttTextImage;

    [Header("Wind UI")]
    [SerializeField] GameObject _windUIHolder;

    [Header("Camera")]
    public Camera myCamera;
    [SerializeField] CinemachineVirtualCamera _vCam;
    [SerializeField] CameraViewHole _cameraViewHole;
    public CameraFollowScript cameraFollowScript;
    [SerializeField] PolygonCollider2D _cameraBoundingBox;
    [SerializeField] LayerMask _cameraBoundingBoxMask;

    [Header("Player Sprite")]
    [SerializeField] GolferAnimator _golfAnimator;

    [Header("Start Game UI Stuff")]
    [SerializeField] GameObject _startGameButton;

    [Header("Await Tasks Booleans on server")]
    bool _tellPlayerGroundTheyLandedOn = false;
    bool _tellPlayerHoleEnded = false;

    private void Awake()
    {
        //if (!MyBall)
        //    SpawnPlayerBall();
        /*if(!myCamera)
            myCamera = Camera.main;
        if (!_vCam)
            _vCam = GameObject.FindGameObjectWithTag("camera").GetComponent<CinemachineVirtualCamera>();
        if (!cameraFollowScript)
            cameraFollowScript = _vCam.GetComponent<CameraFollowScript>();        
        if (!_cameraViewHole)
            _cameraViewHole = _vCam.GetComponent<CameraViewHole>();*/
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("OnStartServer: On GolfPlayerTopDown: is this player object (" + this.name + ") the host from base.IsHost? " + this.IsHost.ToString() + " and from base.Owner.IsHost? " + base.Owner.IsHost.ToString() + " and an owned client id of: " + base.Owner.ClientId + ":" + OwnerId);
        //this.IsGameLeader = base.Owner.IsHost;
        //this.ConnectionId = this.Owner.ClientId;
        GameplayManagerTopDownGolf.instance.AddGolfPlayer(this);
        this.ConnectionId = this.ObjectId;
        this.SpawnBallOnServer();
    }
    public override void OnStopServer()
    {
        base.OnStopServer();
        if (base.IsServer)
        {
            GameplayManagerTopDownGolf.instance.RemoveGolfPlayer(this);
            //GameplayManagerTopDownGolf.instance.RemoveOwnerFromHost();
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("OnStartClient: for " + this.name);
        if (base.IsOwner)
        {
            gameObject.name = "LocalGolfPlayerTopDown";
            gameObject.tag = "LocalGamePlayer";
            GameplayManagerTopDownGolf.instance.SetLocalGolfPlayer(this);
        }
        else
        {
            gameObject.tag = "GamePlayer";
        }
        Debug.Log("OnStartClient: for playeR: " + this.PlayerName + " with object id of: " + this.ObjectId);
        if (!myCamera)
            myCamera = Camera.main;
        if (!_vCam)
            _vCam = GameObject.FindGameObjectWithTag("camera").GetComponent<CinemachineVirtualCamera>();
        if (!cameraFollowScript)
            cameraFollowScript = _vCam.GetComponent<CameraFollowScript>();
        if (!_cameraViewHole)
            _cameraViewHole = _vCam.GetComponent<CameraViewHole>();

    }
    void SyncIsGameLeader(bool prev, bool next, bool asServer)
    {
        if (asServer)
        {
            Debug.Log("SyncIsGameLeader: as server");
            //this.IsGameLeader = next;
            //if (next)
                //GameplayManagerTopDownGolf.instance.AssignOwnerToHost(base.Owner);
        }   
        else
        {
            Debug.Log("SyncIsGameLeader: as client");
            if (!base.IsOwner)
            {
                EnablePlayerCanvas(false);
                _startGameButton.SetActive(false);
                return;
            }   
            if (next)
            {
                //EnablePlayerCanvas(true);
                //_startGameButton.SetActive(true);
            }
            else
            {
                EnablePlayerCanvas(false);
                _startGameButton.SetActive(false);
            }
        }
    }
    void SyncConnectionId(int prev, int next, bool asServer)
    {
        if (asServer)
        {
            //this.ConnectionId = next;
        }   
        else
        {
            if (!base.IsOwner)
                return;
        }
    }
    void SyncPlayerName(string prev, string next, bool asServer)
    {
        if (asServer)
        {
            //PlayerName = next;
        }   
        else
        { 

        }
    }
    // Start is called before the first frame update
    void Start()
    {
        //SetPlayerOnBall();
        
        
        //AttachUIToNewParent(myCamera.transform);
        StartGameWithDriver();
        // do this after getting the players ball
        //drawTrajectoryTopDown.SetLineWidth(MyBall.pixelUnit * 2f);
        try
        {
            GetCameraBoundingBox();
        }
        catch (Exception e)
        {
            Debug.Log("GolfPlayerTopDown.cs: could not get camera bounding box. Error: " + e);
        }
    }
    //void SpawnPlayerBall()
    //{
    //    GameObject newBall = Instantiate(_golfBallPrefab);
    //    MyBall = newBall.GetComponent<GolfBallTopDown>();
    //    MyBall.MyPlayer = this;
    //}
    [Server]
    void SpawnBallOnServer()
    {
        Debug.Log("SpawnBallOnServer::");
        GameObject newBall = Instantiate(_golfBallPrefab);
        GolfBallTopDown newBallScript = newBall.GetComponent<GolfBallTopDown>();
        InstanceFinder.ServerManager.Spawn(newBall, this.Owner);
        this._myBallNetId = newBallScript.ObjectId;
    }
    void SyncMyBallNetId(int prev, int next, bool asServer)
    {
        if (asServer) // maybe get rid of this to make sure the server always set the MyBall parameter. Otherwise, if there was a server but no host who is a server and a client, the server would never get MyBall Set?
            return;
        if (!MyBall)
        {
            Debug.Log("SyncMyBallNetId: for playeR: " + this.PlayerName + " ball id is: " + next.ToString());
            //if (IsServer)
            //{
            //    MyBall = InstanceFinder.ServerManager.Objects.Spawned[next].GetComponent<GolfBallTopDown>();
            //}
            //else
            //{
            //    MyBall = InstanceFinder.ClientManager.Objects.Spawned[next].GetComponent<GolfBallTopDown>();
            //}
            MyBall = InstanceFinder.ClientManager.Objects.Spawned[next].GetComponent<GolfBallTopDown>();
            MyBall.MyPlayer = this;
            if (this.IsOwner)
            {
                MyBall.transform.position = this.transform.position;
                CmdBallSpawnedForPlayer();
                drawTrajectoryTopDown.SetLineWidth(MyBall.pixelUnit * 2f);
            }
            else
            {
                Debug.Log("SyncMyBallNetId: player spawned for someone who isn't the owner of this playeR? " + this.IsOwner.ToString() + " will try and find local game player to track number of balls spawned");
                try
                {
                    GolfPlayerTopDown localPlayer = GameObject.FindGameObjectWithTag("LocalGamePlayer").GetComponent<GolfPlayerTopDown>();
                    localPlayer.CmdBallSpawnedForPlayer();
                }
                catch (Exception e)
                {
                    Debug.Log("SyncMyBallNetId: error finding local game player. Error: " + e);
                }
            }   
        }   
    }
    [ServerRpc]
    public void CmdBallSpawnedForPlayer()
    {
        NumberOfBallsSpawnedForClient++;
        GameplayManagerTopDownGolf.instance.CheckIfAllBallsAreSpawned();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Player pressed space");
        }
        if (!base.IsOwner)
            return;
        if (!MyBall)
            return;
        if (MyBall.isHit || MyBall.isBouncing || MyBall.isRolling)
            return;
        if (!IsPlayersTurn)
        {
            if (GameplayManagerTopDownGolf.instance.CurrentPlayer == this && Input.GetKeyDown(KeyCode.Space) && Time.time >= (GameplayManagerTopDownGolf.instance.TimeSinceLastTurnStart + 0.15f))
            {
                Debug.Log("GolfPlayerTopDown: Player: " + this.PlayerName + " will start their turn after pressing space! Time: " + Time.time);
                this.EnablePlayerCanvas(false);
                // Begin old way of starting turn
                //GameplayManagerTopDownGolf.instance.StartCurrentPlayersTurn(this);
                // End old way of starting turn
                CmdStartCurrentPlayersTurnOnServer();
            }
            else if (PromptedForLightning && GameplayManagerTopDownGolf.instance.CurrentPlayer == this && Input.GetKeyDown(KeyCode.Backspace) && Time.time >= (GameplayManagerTopDownGolf.instance.TimeSinceLastSkip + 0.15f))
            {
                Debug.Log("GolfPlayerTopDown: Player: " + this.PlayerName + " is skipping their turn due to lightning. At time of: " + Time.time.ToString() + " and last skip was: " + GameplayManagerTopDownGolf.instance.TimeSinceLastSkip.ToString());
                this.EnablePlayerCanvas(false);
                PlayerScore.StrokePenalty(1);
                GameplayManagerTopDownGolf.instance.StartNextPlayersTurn(MyBall, true);
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //MyBall.HitBall(hitDistance, hitAngle, hitTopSpin, hitDirection);
            //EnableOrDisableLineObjects(false);
            //if (!IsPlayersTurn && !_moveHitMeterIcon)
            //    StartPlayerTurn();
            Debug.Log("Player (the owner) pressed space");
            if (this.PlayerStruckByLightning)
            {
                Debug.Log("Player pressed space: this.PlayerStruckByLightning");
                _golfAnimator.ResetGolfAnimator();
                EnablePlayerSprite(false);
                this.EnablePlayerCanvas(false);
                //this.IsPlayersTurn = false;
                CmdEndPlayersTurn();

                previousHitDistance = 0;

                ActivateHitUIObjects(false);
                CmdTellPlayersToActivateHitUIObjects(false);
                AttachUIToNewParent(this.transform);
                CmdTellPlayersToAttachUIToSelf();
                //AttachPlayerToCamera(false, myCamera.transform);
                //UpdateCameraFollowTarget(MyBall.MyBallObject);
                EnableOrDisableLineObjects(false);
                CmdTellPlayersToEnableOrDisableLineObjects(false);
                this.PlayerStruckByLightning = false;
                Debug.Log("GolfPlayerTopDown: Player: " + this.PlayerName + " has acknowledged they were struck by lightning! Moving on to next turn by calling: GameplayManagerTopDownGolf.instance.PlayerWasStruckByLightning(this). Time: " + Time.time);
                GameplayManagerTopDownGolf.instance.PlayerWasStruckByLightning(this);
                return;
            }
            if (!DirectionAndDistanceChosen && !_moveHitMeterIcon)
            {
                Debug.Log("Player pressed space: !DirectionAndDistanceChosen && !_moveHitMeterIcon");
                PlayerChooseDirectionAndDistance(true);
                if (_cameraViewHole.IsCameraZoomedOut)
                    _cameraViewHole.ZoomOutCamera();
                UpdateCameraFollowTarget(MyBall.MyBallObject);
                CmdSetCameraOnMyBallForOtherPlayers();
            }
            else if (DirectionAndDistanceChosen && !_moveHitMeterIcon)
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
                    _golfAnimator.StartSwing();
                    //SubmitHitToBall();
                }
            }
        }
        //if (Input.GetKeyDown(KeyCode.LeftControl) && !IsPlayersTurn)
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            //MyBall.ResetPosition();
            //EnableOrDisableLineObjects(true);
            Debug.Log("LEft Control Pressed");
            if (DirectionAndDistanceChosen && !_moveHitMeterIcon)
            {
                PlayerChooseDirectionAndDistance(false);
                UpdateCameraFollowTarget(_landingTargetSprite.gameObject);
            }
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            //MyBall.PuttBall(hitDirection, hitDistance);
            //EnableOrDisableLineObjects(false);
            if (!_moveHitMeterIcon)
            {
                ChangeCurrentClub();
                if (DirectionAndDistanceChosen)
                {
                    PlayerChooseDirectionAndDistance(false);
                    UpdateCameraFollowTarget(_landingTargetSprite.gameObject);
                }
            }
        }
        if (!_moveHitMeterIcon && CurrentClub.ClubType == "putter" && Input.GetKeyDown(KeyCode.LeftShift))
        {
            SetPutterDistance(CurrentClub, true);
            if (DirectionAndDistanceChosen)
            {
                PlayerChooseDirectionAndDistance(false);
                UpdateCameraFollowTarget(_landingTargetSprite.gameObject);
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
        else if (!DirectionAndDistanceChosen)
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
        if (Input.GetKeyDown(KeyCode.BackQuote) && !_powerSubmitted && !_accuracySubmitted)
        {
            Debug.Log("GolfPlayer: BackQuote key pressed. _moveHitMeterIcon: " + _moveHitMeterIcon.ToString());
            _cameraViewHole.ZoomOutCamera();
        }
            
        

    }
    private void FixedUpdate()
    {
        if (!MyBall)
            return;
        if ((hitDistance != previousHitDistance || hitAngle != previousHitAngle || hitTopSpin != previousHitTopSpin || hitDirection != previousHitDirection || hitLeftOrRightspin != previousHitLeftOrRightSpin) && (!MyBall.isHit && !MyBall.isBouncing && !MyBall.isRolling && IsPlayersTurn))
        {
            // Get the trajaectory line
            if (CurrentClub.ClubType != "putter")
                trajectoryPoints = MyBall.CalculateHitTrajectory(hitDistance, hitAngle, hitTopSpin, hitLeftOrRightspin, hitDirection, Vector3.zero, 0f);
            else
                trajectoryPoints = MyBall.CalculatePutterTrajectoryPoints(hitDistance, hitDirection);
            // Draw the trajectory
            drawTrajectoryTopDown.UpdateTrajectory(trajectoryPoints, MyBall,CurrentClub.ClubType, hitDistance);
            UpdateCameraFollowTarget(_landingTargetSprite.gameObject);



            // Reset the "previous" data
            previousHitDirection = hitDirection;
            previousHitDistance = hitDistance;
            previousHitAngle = hitAngle;
            previousHitTopSpin = hitTopSpin;
            previousHitLeftOrRightSpin = hitLeftOrRightspin;

            // Update the sprite direction
            _golfAnimator.UpdateSpriteDirection(hitDirection.normalized);

            // Update for the other game players?
            if(base.IsOwner)
                CmdUpdateHitValuesForOtherPlayers(hitDistance, hitAngle, hitTopSpin, hitDirection, hitLeftOrRightspin);
        }
    }
    [ServerRpc]
    void CmdUpdateHitValuesForOtherPlayers(float newDist, float newAngle, float newTopSpin, Vector2 newHitDir, float newLeftOrRightSpin)
    {
        RpcUpdateHitValuesForOtherPlayers(newDist, newAngle, newTopSpin, newHitDir, newLeftOrRightSpin);
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcUpdateHitValuesForOtherPlayers(float newDist, float newAngle, float newTopSpin, Vector2 newHitDir, float newLeftOrRightSpin)
    {
        if (hitDistance != newDist)
            hitDistance = newDist;
        if (hitAngle != newAngle)
            hitAngle = newAngle;
        if (hitTopSpin != newTopSpin)
            hitTopSpin = newTopSpin;
        if (hitDirection != newHitDir)
            hitDirection = newHitDir;
        if (hitLeftOrRightspin != newLeftOrRightSpin)
            hitLeftOrRightspin = newLeftOrRightSpin;
    }
    [ServerRpc]
    void CmdTellPlayersToEnableOrDisableLineObjects(bool enable)
    {
        RpcEnableOrDisableLineObjectsForOtherPlayers(enable);
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcEnableOrDisableLineObjectsForOtherPlayers(bool enable)
    {
        EnableOrDisableLineObjects(enable);
    }
    public void EnableOrDisableLineObjects(bool enable)
    {
        Debug.Log("EnableOrDisableLineObjects: " + enable.ToString());
        trajectoryLineObject.enabled = enable;
        trajectoryShadowLineObject.enabled = enable;
        _landingTargetSprite.enabled = enable;
    }
    public void ResetPreviousHitValues()
    {
        //Debug.Log("ResetPreviousHitValues");
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

        turnRate = 5.5f / (hitDistance + 0.1f);
        Vector2 newDir = hitDirection + perpendicular * Time.deltaTime * turnRate;
        Vector3 newTargetPos = MyBall.transform.position + (Vector3)(newDir.normalized * hitDistance);

        // check if the new target is within the bounds of the camera bounding box
        GetCameraBoundingBox();
        if (_cameraBoundingBox.OverlapPoint(newTargetPos))
        {
            //Debug.Log("ChangeHitDirection: new point is colliding with the camera bounding box at point: " + newTargetPos.ToString("0.00000"));
            hitDirection = newDir.normalized;
        }
        else
        {
            //Debug.Log("ChangeHitDirection: new point is NOT COLLIDING the camera bounding box at point: " + newTargetPos.ToString("0.00000"));            
        }

        // old way?
        //hitDirection += perpendicular * Time.deltaTime * turnRate;

        //hitDirection = hitDirection.normalized;

        //turnRate = 0.5f / (hitDistance + 0.1f);

    }
    void ChangeHitDistance(float changeDirection)
    {
        
        float newDist = hitDistance + (changeDirection * Time.deltaTime * _changeDistanceRate);
        // check if the new distance will be out of bounds
        Vector3 newTargetPos = MyBall.transform.position + (Vector3)(hitDirection.normalized * newDist);

        if (_cameraBoundingBox.OverlapPoint(newTargetPos))
        {
            //Debug.Log("ChangeHitDistance: new point is colliding with the camera bounding box at point: " + newTargetPos.ToString("0.00000"));
            hitDistance = newDist;
        }
        else
        {
            //Debug.Log("ChangeHitDistance: new point is NOT COLLIDING the camera bounding box at point: " + newTargetPos.ToString("0.00000"));
            return;
        }
        // old way
        /*hitDistance += changeDirection * Time.deltaTime * _changeDistanceRate;*/
        if (hitDistance > MaxDistanceFromClub)
            hitDistance = MaxDistanceFromClub;
        if (hitDistance <= MinDistance)
            hitDistance = MinDistance;


        // Adjust the location of the Adjusted Distance Icon
        UpdatePositionOfAdjustedDistanceIcon(hitDistance);
    }
    [Server]
    public void ServerSetIsPlayersTurn(bool isItTheirTurn)
    {
        this.IsPlayersTurn = isItTheirTurn;
    }
    void SyncIsPlayersTurn(bool prev, bool next, bool asServer)
    {
        if (asServer)
            return;
        if (next)
        {
            Debug.Log("SyncIsPlayersTurn: Is it " + this.PlayerName + "'s turn? " + next.ToString());
            StartPlayerTurn();
        }
        else
        { 

        }
    }
    public void StartPlayerTurn()
    {
        // IsPlayersTurn will be set by the game manager in a real game. This is just a place holder for now.
        //this.IsPlayersTurn = true; // old way from singleplayer mode
        SetCameraOnPlayer();
        SetPlayerOnBall();
        AttachUIToNewParent(myCamera.transform);
        DeactivateIconsForNewTurn();
        ResetSubmissionValues();
        UpdateBallGroundMaterial();

        // Find the closest hole to the player
        Vector3 ballPos = MyBall.transform.position;
        GameObject closestHole = FindClosestHole(ballPos);
        Vector3 aimTarget = closestHole.transform.position;
        // Check if the player has teed off yet
        if (!HasPlayerTeedOff)
        {
            if (Vector2.Distance(ballPos, GameplayManagerTopDownGolf.instance.TeeOffPosition) > 25f)
            {
                //HasPlayerTeedOff = true;
                CmdPlayerTeedOff(true);
            }   
            else
            {
                aimTarget = GameplayManagerTopDownGolf.instance.TeeOffAimPoint;
            }
                
        }
        // Get the distance to the hole
        float distToHole = GetDistanceToHole(closestHole, ballPos);
        float distToAimPoint = Vector2.Distance(aimTarget, ballPos);
        hitDirection = SetInitialDirection(ballPos, aimTarget);
        // Find most appropriate club to start with based on distance to hole
        //_currentClubIndex = FindAppropriateClubToStart(distToHole);
        _currentClubIndex = FindAppropriateClubToStart(distToAimPoint);
        CurrentClub = _myClubs[_currentClubIndex];
        // Update the Club UI stuff
        SetSelectedClubUI(CurrentClub);
        if (this.IsOwner)
            CmdTellClientsSelectedClub(_currentClubIndex);
        // update the club stats?
        GetNewClubAttributes(CurrentClub);

        GetHitStatsFromClub();
        SetPutterDistance(CurrentClub, false);
        EnableOrDisableLineObjects(true);
        //_hitMeterObject.SetActive(true);
        ActivateHitUIObjects(true);
        // Set the initial direction of the hit to be in the direction of the hole
        //hitDirection = SetInitialDirection(ballPos, closestHole.transform.position);
        

        // Check to see if the current club's max distance is greater than distance to the hole. If so, adjust the player's shot so that their initial hit distance is right at the hole
        DidPlayerAdjustDistance = false;
        if (MaxDistanceFromClub > distToAimPoint && distToAimPoint >= distToHole)
        {
            hitDistance = distToHole;
            UpdatePositionOfAdjustedDistanceIcon(distToHole);
        }
        ResethitTopSpinForNewTurn();
        PlayerChooseDirectionAndDistance(false);
        UpdateCameraFollowTarget(_landingTargetSprite.gameObject);
    }
    [ObserversRpc]
    public void RpcSetCameraOnPlayer()
    {
        Debug.Log("RpcSetCameraOnPlayer: For player: " + this.PlayerName);
        this.SetCameraOnPlayer();
    }
    public void SetCameraOnPlayer()
    {
        // Update player so they are at the same position of the camera
        UpdateCameraFollowTarget(MyBall.MyBallObject);
        //cameraFollowScript.followTarget = MyBall.MyBallObject;
        //Vector3 cameraPos = myCamera.transform.position;
        //this.transform.position = new Vector3(cameraPos.x, cameraPos.y, 0f);
        //AttachPlayerToCamera(true, myCamera.transform);
    }
    public void SetPlayerOnBall()
    {
        this.transform.position = MyBall.transform.position;
    }
    void DeactivateIconsForNewTurn()
    {
        _hitMeterMovingIcon.SetActive(false);
        _hitMeterPowerSubmissionIcon.SetActive(false);
        _hitMeterAccuracySubmissionIcon.SetActive(false);
    }
    [ServerRpc]
    void CmdTellPlayersToActivateHitUIObjects(bool enable)
    {
        RpcTellPlayersToActivateHitUIObjects(enable);
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcTellPlayersToActivateHitUIObjects(bool enable)
    {
        ActivateHitUIObjects(enable);
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
        // old way
        //hitDistance = MaxDistanceFromClub;

        float newDist = MaxDistanceFromClub;
        Vector3 ballPos = MyBall.transform.position;
        Vector3 newTargetPos = ballPos + (Vector3)(hitDirection.normalized * newDist);
        GetCameraBoundingBox();
        if (_cameraBoundingBox.OverlapPoint(newTargetPos))
        {
            Debug.Log("GetHitStatsFromClub: new point is colliding with the camera bounding box at point: " + newTargetPos.ToString("0.00000") + ". No change to hit distance.");
            hitDistance = newDist;
        }
        else
        {

            //Vector3 inBoundsPos = GetNearestPointInBounds(ballPos, newTargetPos, hitDirection);
            //hitDistance = Vector2.Distance(inBoundsPos, ballPos);
            hitDistance = GetInBoundsDistance(newDist, hitDirection, ballPos);
            Debug.Log("GetHitStatsFromClub: new point is NOT COLLIDING the camera bounding box at point: " + newTargetPos.ToString("0.00000") + ". CHANGING hit distance to: " + hitDistance + " from original distance: " + newDist.ToString()) ;
        }

        hitAngle = DefaultLaunchAngleFromClub;
        //SpinDividerFromClub = 1f; // get this from the actual club object later?
        UpdatePositionOfAdjustedDistanceIcon(hitDistance);
        MinDistance = GetMinDistance(hitDistance);
        // maybe also set the icon movement speed based on the club?
    }
    float GetMinDistance(float distance)
    {
        float minDistPercentage = (_centerAccuracyPosition - (_furthestLeftAccuracyPosition - MyBall.pixelUnit)) / _distancePositionLength;
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
        //MyBall.HitBall(hitDistance, hitAngle, hitTopSpin, hitDirection);
        ResetIconPositions();
        ResetSubmissionValues();
        BeginMovingHitMeter();

        // Make sure the camera isn't zoomed out?
        if (_cameraViewHole.IsCameraZoomedOut)
            _cameraViewHole.ZoomOutCamera();

        // IsPlayersTurn will be set by the game manager in a real game. This is just a place holder for now.
        //this.IsPlayersTurn = false;
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
                //SubmitHitToBall();
                _golfAnimator.StartSwing();
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
        /*float minXToGetMaxHit = _maxDistancePosition + (MyBall.pixelUnit * 2);
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
        if (diff <= (MyBall.pixelUnit * 2))
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
        //return direction;
        Vector2 newDir = Vector2.zero;

        // If the hit is perfectly accurate, don't adjust the direction from where the player aimed
        if (accuracyDistance == 0)
            return direction;

        accuracyDistance *= -2.5f;
        // Punish innaccurate putts more than regular hits?
        if (CurrentClub.ClubType == "putter")
            accuracyDistance *= 2.5f;
        // Also punish driver hits?
        if (CurrentClub.ClubType == "driver")
            accuracyDistance *= 1.5f;
		
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
    public void SubmitHitToBall()
    {
        Debug.Log("SubmitHitToBall: For player: " + this.PlayerName);
        // This will all need to be updated when the network animator is used to make sure only the owner makes certain calls, but that the clients still play sounds and stuff?
        bool playerTeeOffSound = false;
        if (!this.HasPlayerTeedOff)
        {
            //GameplayManagerTopDownGolf.instance.PlayerTeedOff(this); // this needs to be updated on the server
            CmdTellServerPlayerTeedOff();
            CmdPlayerTeedOff(true);
            //this.HasPlayerTeedOff = true;

            playerTeeOffSound = true;

        }
            
        this.PlayerScore.PlayerHitBall();
        //GameplayManagerTopDownGolf.instance.ResetCurrentPlayer();
        CmdResetCurrentPlayer();
        //GameplayManagerTopDownGolf.instance.LightningAfterPlayerHit();

        if (!IsShanked)
        {
            if (CurrentClub.ClubType == "putter")
            {
                MyBall.PuttBall(ModifiedHitDirection, HitPowerSubmitted);
                //MyBall.PuttBall(new Vector2 (1f,0f), 10f);                
                SoundManager.instance.PlaySound("golfball-hit", 0.8f);
            }
            else
            {
                MyBall.HitBall(HitPowerSubmitted, hitAngle, hitTopSpin, ModifiedHitDirection, hitLeftOrRightspin);
                //MyBall.HitBall(CurrentClub.MaxHitDistance, hitAngle, hitTopSpin, ModifiedHitDirection, hitLeftOrRightspin);
                if(playerTeeOffSound)
                    SoundManager.instance.PlaySound("golfball-teeoff", 1f);
                else
                    SoundManager.instance.PlaySound("golfball-hit", 1f);
            }
        }
        else
        {
            Debug.Log("Was ball shanked? " + IsShanked.ToString());
            DoTheShank();
            SoundManager.instance.PlaySound("golfball-shank", 1f);
            //MyBall.HitBall(100, hitAngle, hitTopSpin, new Vector2(1f, 0f), hitLeftOrRightspin);
        }
        //this.IsPlayersTurn = false;
        CmdEndPlayersTurn();
        ActivateHitUIObjects(false);
        CmdTellPlayersToActivateHitUIObjects(false);
        AttachUIToNewParent(this.transform);
        CmdTellPlayersToAttachUIToSelf();
        //AttachPlayerToCamera(false, myCamera.transform);
        UpdateCameraFollowTarget(MyBall.MyBallObject);
        EnableOrDisableLineObjects(false);
        CmdTellPlayersToEnableOrDisableLineObjects(false);
    }
    public void UpdateHitSpinForPlayer(Vector2 newSpin)
    {
        if (!base.IsOwner)
            return;
        //hitTopSpinSubmitted = newSpin.normalized;
        hitTopSpinSubmitted = newSpin;
        //Debug.Log("UpdateHitSpinForPlayer: new spin submitted: " + newSpin.ToString() + " hitTopSpinSubmitted will then be: " + hitTopSpinSubmitted.ToString());
        UpdateTopSpin(hitTopSpinSubmitted.y);
        UpdateLeftRightSpin(hitTopSpinSubmitted.x);
        if(DirectionAndDistanceChosen)
            PlayerChooseDirectionAndDistance(false);
    }
    void UpdateTopSpin(float newTopSpin)
    {
        // Instead of using these TopSpinPositiveModifer and TopSpinNegativeModifer values, a range of possible values for top/back spin should be calculated based on the clubs Max top/back spin values
        //Debug.Log("UpdateTopSpin: newTopSpin:" + newTopSpin.ToString());
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
            //hitAngle = DefaultLaunchAngleFromClub * (1f - ((spin * 2f) / 100f));
            hitAngle = DefaultLaunchAngleFromClub * (1f - ((spin * 6f) / 100f));
        }
        else
        {
            //hitAngle = DefaultLaunchAngleFromClub * (1f + ((spin * -1f) / 100f));
            hitAngle = DefaultLaunchAngleFromClub * (1f + ((spin * -3f) / 100f));
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
                if (this.IsOwner)
                    CmdTellClientsSelectedClub(_currentClubIndex);
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
        // Play the change club sound if the clubs will be different
        if (CurrentClub != _myClubs[_currentClubIndex])
        {
            SoundManager.instance.PlaySound("golf-change-club", 1f);
        }
        CurrentClub = _myClubs[_currentClubIndex];

        GetNewClubAttributes(CurrentClub);
        GetHitStatsFromClub();

        // Maybe remove this later so the spin isn't always reset when the club is changed, but rather the spin values are updated to accomadate the new club?
        //ResethitTopSpinForNewTurn();

        // Update the Club UI stuff
        SetSelectedClubUI(CurrentClub);
        SetPutterDistance(CurrentClub, false);
        // Update spin values to adjust for new clubs
        if (hitTopSpin > 0)
            UpdateTopSpin(hitTopSpin / _myClubs[oldIndex].MaxTopSpin);
        else
            UpdateTopSpin(hitTopSpin / _myClubs[oldIndex].MaxBackSpin);
        UpdateLeftRightSpin(hitLeftOrRightspin / _myClubs[oldIndex].MaxSideSpin);

        if (this.IsOwner)
            CmdTellClientsSelectedClub(_currentClubIndex);
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
        if (MyBall.bounceContactGroundMaterial == "rough")
        {
            dist *= club.RoughTerrainDistModifer;
        }
        if (MyBall.bounceContactGroundMaterial.Contains("deep rough"))
        {
            dist *= club.DeepRoughTerrainDistModifer;
        }
        else if (MyBall.bounceContactGroundMaterial.Contains("trap") && club.ClubType != "wedge")
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
        MyBall.UpdateGroundMaterial();
    }
    bool CanClubBeUsedOnCurrentGround(ClubTopDown club)
    {
        if (MyBall.bounceContactGroundMaterial.Contains("rough"))
        {
            if (club.ClubType == "putter")
                return false;
        }
        if (MyBall.bounceContactGroundMaterial.Contains("trap"))
        {
            /*if (club.ClubType == "putter" || club.ClubType == "driver")
                return false;*/
            if (club.ClubType != "wedge")
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
            MyBall.PuttBall(ModifiedHitDirection, HitPowerSubmitted);
        }
        else
        {
            float shankAngle = ShankAngle(hitAngle);
            float shankTopSpin = ShankTopSpin(MaxBackSpinFromClub,MaxTopSpinFromClub);
            float shankSideSpin = ShankSideSpin(-MaxSideSpinFromClub, MaxSideSpinFromClub);
            Debug.Log("DoTheShank: Hitting with new direction of: " + ModifiedHitDirection.ToString() + " and new power of: " + HitPowerSubmitted.ToString() + " and new launch angle of: " + shankAngle.ToString() + " and new top spin of: " + shankTopSpin.ToString() + " and new side spin of: " + shankSideSpin.ToString());
            MyBall.HitBall(HitPowerSubmitted, shankAngle, shankTopSpin, ModifiedHitDirection, shankSideSpin);
        }

    }
    float ShankDistance(float dist)
    {
        return UnityEngine.Random.Range((dist * 0.15f), dist);
    }
    Vector2 ShankDirection(Vector2 dir)
    {
        Vector2 shankDir = dir;

        // Get the rotation from the hit direction to shank
        float rotAngle = 90f * (UnityEngine.Random.Range(0.1f, 0.9f));
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
        shankAngle *= (UnityEngine.Random.Range(0.25f, 1.25f));
        return shankAngle;
    }
    float ShankTopSpin(float maxBack, float maxTop)
    {
        return UnityEngine.Random.Range(maxBack, maxTop);
    }
    float ShankSideSpin(float minSideSpin, float maxSideSpin)
    {
        return UnityEngine.Random.Range(minSideSpin, maxSideSpin);
    }
    void UpdateCameraFollowTarget(GameObject objectToFollow)
    {
        cameraFollowScript.followTarget = objectToFollow;
    }
    [ServerRpc]
    void CmdTellPlayersToAttachUIToSelf()
    {
        RpcAttachUIToSelf();
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcAttachUIToSelf()
    {
        AttachUIToNewParent(this.transform);
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
    void SetPutterDistance(ClubTopDown club, bool fromShift)
    {
        if (club.ClubType != "putter")
        {
            _puttDistanceTextImage.enabled = false;
            return;
        }

        if (!fromShift)
        {
            _puttDistanceTextImage.sprite = _longPuttTextImage;
        }
        else
        {
            if (_puttDistanceTextImage.sprite == _longPuttTextImage)
            {
                _puttDistanceTextImage.sprite = _shortPuttTextImage;
                MaxDistanceFromClub = GetDistanceFromClub(club) / 4f;
                hitDistance = MaxDistanceFromClub;
                UpdatePositionOfAdjustedDistanceIcon(hitDistance);
                MinDistance = GetMinDistance(hitDistance);
                
            }
            else
            {
                _puttDistanceTextImage.sprite = _longPuttTextImage;
                GetNewClubAttributes(club);
                hitDistance = MaxDistanceFromClub;
                UpdatePositionOfAdjustedDistanceIcon(hitDistance);
                MinDistance = GetMinDistance(hitDistance);
            }
        }
        _puttDistanceTextImage.enabled = true;
    }
    Vector2 SetInitialDirection(Vector3 ballPos, Vector3 aimPoint)
    {
        // Find the flag hole objects
        /*GameObject[] flagHoles = GameObject.FindGameObjectsWithTag("golfHole");

        if (flagHoles.Length > 0)
        {
            int closestHoleIndex = 0;
            float closestDist = 0f;
            for (int i = 0; i < flagHoles.Length; i++)
            {
                float holeDist = Vector2.Distance(flagHoles[i].transform.position, ballPos);
                if (i == 0)
                {
                    closestDist = holeDist;
                    closestHoleIndex = i;
                    continue;
                }
                if (holeDist < closestDist)
                {
                    closestDist = holeDist;
                    closestHoleIndex = i;
                }
            }

            Vector2 newDir = (flagHoles[closestHoleIndex].transform.position - ballPos).normalized;
            hitDirection = newDir;
        }*/
        Vector2 newDir = (aimPoint - ballPos).normalized;
        return newDir;
        
    }
    void PlayerChooseDirectionAndDistance(bool enable)
    {
        Debug.Log("PlayerChooseDirectionAndDistance: setting to: " + enable.ToString());
        DirectionAndDistanceChosen = enable;
    }
    // Search through available clubs and select the one that has a max hit distance closest to the hole
    int FindAppropriateClubToStart(float distToHole)
    {
        // Get distance to the hole
        //float distanceToHole = GetDistanceToHole();
        UpdateBallGroundMaterial();
        float bestDifferenceFromHoleDistance = 0f;
        int bestClubIndex = 0;
        bool firstCheckDone = false;        

        for (int i = 0; i < _myClubs.Length; i++)
        {
            if (CanClubBeUsedOnCurrentGround(_myClubs[i]))
            {
                float clubDist = _myClubs[i].MaxHitDistance;
                // Make checks based on how terrain modifies hit distance
                if (MyBall.bounceContactGroundMaterial == "rough")
                {
                    clubDist *= _myClubs[i].RoughTerrainDistModifer;
                }
                if (MyBall.bounceContactGroundMaterial.Contains("deep rough"))
                {
                    clubDist *= _myClubs[i].DeepRoughTerrainDistModifer;
                }
                else if (MyBall.bounceContactGroundMaterial.Contains("trap") && _myClubs[i].ClubType != "wedge")
                {
                    clubDist *= _myClubs[i].TrapTerrainDistModifer;
                }

                float distDifference = Mathf.Abs(clubDist - distToHole);

                // only default to the putter if the player is on the green
                if (MyBall.bounceContactGroundMaterial != "green" && _myClubs[i].ClubType == "putter")
                    continue;

                if (!firstCheckDone)
                {
                    bestDifferenceFromHoleDistance = distDifference;
                    bestClubIndex = i;
                    firstCheckDone = true;
                    continue;
                }
                
                if (distDifference < bestDifferenceFromHoleDistance)
                {
                    bestDifferenceFromHoleDistance = distDifference;
                    bestClubIndex = i;
                }
                else if (distDifference == bestDifferenceFromHoleDistance && clubDist > distToHole) // default to the club that has a distance greater than distance to hole if the difference in distances is the same?
                {
                    bestDifferenceFromHoleDistance = distDifference;
                    bestClubIndex = i;
                }
            }
        }
        Debug.Log("FindAppropriateClubToStart: Returning club index of: " + bestClubIndex.ToString() + " which maps to: " + _myClubs[bestClubIndex].ClubName);
        return bestClubIndex;
    }
    // Get distance from the ball to the hole
    float GetDistanceToHole(GameObject closestHole, Vector3 ballPos)
    {
        float dist = Vector2.Distance(closestHole.transform.position, ballPos);
        return dist;
    }
    GameObject FindClosestHole(Vector3 ballPos)
    {
        // Find the flag hole objects
        GameObject[] flagHoles = GameObject.FindGameObjectsWithTag("golfHole");
        GameObject closestHole = null;
        if (flagHoles.Length > 0)
        {
            int closestHoleIndex = 0;
            float closestDist = 0f;
            for (int i = 0; i < flagHoles.Length; i++)
            {
                float holeDist = Vector2.Distance(flagHoles[i].transform.position, ballPos);
                if (i == 0)
                {
                    closestDist = holeDist;
                    closestHoleIndex = i;
                    closestHole = flagHoles[i];
                    continue;
                }
                if (holeDist < closestDist)
                {
                    closestDist = holeDist;
                    closestHoleIndex = i;
                    closestHole = flagHoles[i];
                }
            }

            //Vector2 newDir = (flagHoles[closestHoleIndex].transform.position - ballPos).normalized;
            //hitDirection = newDir;
        }
        return closestHole;
    }
    [ObserversRpc]
    public void RpcEnablePlayerCanvas(bool enable)
    {
        if (!this.IsOwner)
        {
            EnablePlayerCanvas(false);
            return;
        }
        EnablePlayerCanvas(enable);
    }


    public void EnablePlayerCanvas(bool enable)
    {
        Debug.Log("EnablePlayerCanvas: " + enable + " for player: " + this.PlayerName);
        _playerCanvas.gameObject.SetActive(enable);
    }
    [TargetRpc]
    public void RpcPlayerUIMessage(NetworkConnection conn, string message)
    {
        if (!this.IsOwner)
            return;
        PlayerUIMessage(message);
    }
    public void PlayerUIMessage(string message)
    {
        if (message == "lightning")
            PromptedForLightning = true;
        else if (message == "start turn")
            PromptedForLightning = false;
        _playerUIMessage.UpdatePlayerMessageText(message);
        if (this.IsOwner)
            CmdTellClientsUpdatePlayerMessageText(message);
    }
    [ServerRpc]
    void CmdTellClientsUpdatePlayerMessageText(string newMessage)
    {
        if (newMessage == "lightning" || newMessage == "start turn")
            return;
        RpcTellClientsUpdatePlayerMessageText(newMessage);
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcTellClientsUpdatePlayerMessageText(string newMessage)
    {
        this.EnablePlayerCanvas(true);
        _playerUIMessage.UpdatePlayerMessageText(newMessage);
    }
    [Server]
    public async Task ServerTellPlayerGroundTheyLandedOn(float duration)
    {
        _tellPlayerGroundTheyLandedOn = true;
        float end = Time.time + (duration * 2);
        RpcTellPlayerGroundTheyLandedOn(duration);
        Debug.Log("ServerTellPlayerGroundTheyLandedOn: Start time is: " + Time.time.ToString());
        while (_tellPlayerGroundTheyLandedOn)
        {
            //Debug.Log("ServerTellPlayerGroundTheyLandedOn: Task.Yield time is: " + Time.time.ToString());
            await Task.Yield();
            if (Time.time >= end)
                _tellPlayerGroundTheyLandedOn = false;
        }
        Debug.Log("ServerTellPlayerGroundTheyLandedOn: End time is: " + Time.time.ToString());
    }
    [ObserversRpc]
    public void RpcTellPlayerGroundTheyLandedOn(float duration)
    {
        StartTellPlayerGroundTheyLandedOn(duration);
    }
    async void StartTellPlayerGroundTheyLandedOn(float duration)
    {
        await TellPlayerGroundTheyLandedOn(duration);
    }
    public async Task TellPlayerGroundTheyLandedOn(float duration)
    {
        Debug.Log("TellPlayerGroundTheyLandedOn: on game player: " + this.PlayerName);
        if (!this.IsOwner)
        {
            return;
        }

        float end = Time.time + duration;
        GetCameraBoundingBox();
        bool isInBounds = true;
        if (_cameraBoundingBox)
        {
            if (!_cameraBoundingBox.OverlapPoint(MyBall.transform.position))
            {
                Debug.Log("TellPlayerGroundTheyLandedOn: Ball is NOT in bounds. Moving the ball for the ball");
                isInBounds = false;
                MyBall.OutOfBounds();
                //ball.OutOfBounds();
                //await ball.MyPlayer.TellPlayerBallIsOutOfBounds(3);
                //return;
            }
        }
        if (isInBounds && !(MyBall.IsInHole || MyBall.LocalIsInHole))
            MyBall.TellPlayerGroundTypeTheyLandedOn();
        else if (isInBounds && (MyBall.IsInHole || MyBall.LocalIsInHole))
        {
            Debug.Log("TellPlayerGroundTheyLandedOn: on game player: " + this.PlayerName + " ball is in hole!");
            MyBall.TellPlayerBallIsInHole();
            end += 1f;
        }
        while (Time.time < end)
        {
            await Task.Yield();
        }
        this.EnablePlayerCanvas(false);
        if (this.IsOwner)
            CmdTellServerPlayerGroundTheyLandedOnCompleted();
    }
    [ServerRpc]
    void CmdTellServerPlayerGroundTheyLandedOnCompleted()
    {
        _tellPlayerGroundTheyLandedOn = false;
        RpcTellServerPlayerGroundTheyLandedOnCompleted();
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcTellServerPlayerGroundTheyLandedOnCompleted()
    {
        this.EnablePlayerCanvas(false);
    }
    public async Task TellPlayerBallIsOutOfBounds(float duration)
    {
        Debug.Log("TellPlayerBallIsOutOfBounds: on game player");
        float end = Time.time + duration;
        MyBall.OutOfBounds();
        while (Time.time < end)
        {
            await Task.Yield();
        }
        this.EnablePlayerCanvas(false);
    }
    [Server]
    public async Task ServerTellPlayerHoleEnded(float duration)
    {
        _tellPlayerHoleEnded = true;
        float end = Time.time + (duration * 2);
        //RpcTellPlayerGroundTheyLandedOn(duration);
        RpcTellPlayerHoleEnded(this.Owner, duration);
        Debug.Log("ServerTellPlayerHoleEnded: Start time is: " + Time.time.ToString());
        while (_tellPlayerHoleEnded)
        {
            //Debug.Log("ServerTellPlayerGroundTheyLandedOn: Task.Yield time is: " + Time.time.ToString());
            await Task.Yield();
            if (Time.time >= end)
                _tellPlayerHoleEnded = false;
        }
        Debug.Log("ServerTellPlayerHoleEnded: End time is: " + Time.time.ToString());
    }
    [TargetRpc]
    void RpcTellPlayerHoleEnded(NetworkConnection conn, float duration)
    {
        StartTellPlayerHoleEnded(duration);
    }
    async void StartTellPlayerHoleEnded(float duration)
    {
        await TellPlayerHoleEnded(duration);
    }
    public async Task TellPlayerHoleEnded(float duration)
    {
        Debug.Log("TellPlayerHoleEnded: on game player: " + this.PlayerName);
        float end = Time.time + duration;
        this.PlayerUIMessage("hole ended");
        this.EnablePlayerCanvas(true);
        while (Time.time < end)
        {
            await Task.Yield();
        }
        this.EnablePlayerCanvas(false);
        if (this.IsOwner)
            CmdTellServerPlayerHoleEndedCompleted();
    }
    [ServerRpc]
    void CmdTellServerPlayerHoleEndedCompleted()
    {
        _tellPlayerHoleEnded = false;
    }
    public async Task TellPlayerGameIsOver(float duration)
    {
        Debug.Log("TellPlayerGameIsOver: on game player: " + this.PlayerName);
        float end = Time.time + duration;
        this.PlayerUIMessage("game over");
        this.EnablePlayerCanvas(true);
        while (Time.time < end)
        {
            await Task.Yield();
        }
        this.EnablePlayerCanvas(false);
    }
    public void GetCameraBoundingBox()
    {
        if (!_cameraBoundingBox)
            _cameraBoundingBox = GameObject.FindGameObjectWithTag("CameraBoundingBox").GetComponent<PolygonCollider2D>();
    }
    [TargetRpc]
    public void RpcGetCameraBoundingBox(NetworkConnection conn)
    {
        if (!this.IsOwner)
            return;
        GetCameraBoundingBox();
    }
    public Vector3 GetNearestPointInBounds(Vector3 startPos, Vector3 endPos, Vector2 dir)
    {
        float distToPlayer = Vector2.Distance(endPos, startPos);
        RaycastHit2D hit = Physics2D.Raycast(endPos, -dir, distToPlayer, _cameraBoundingBoxMask);
        if (hit)
        {
            Vector3 newPos = hit.point;
            newPos += (Vector3)(-dir * MyBall.MyColliderRadius * 2f);
            return newPos;
        }
        else
            return endPos;
    }
    float GetInBoundsDistance(float attemptedDist, Vector2 dir, Vector3 startPos)
    {
        float inBoundsDistance = 0f;

        Vector3 newTargetPos = startPos + (Vector3)(dir.normalized * attemptedDist);
        if (_cameraBoundingBox.OverlapPoint(newTargetPos))
        {
            Debug.Log("GetInBoundsDistance: new point is colliding with the camera bounding box at point: " + newTargetPos.ToString("0.00000"));
            return attemptedDist;
        }
        else
        {
            Debug.Log("GetInBoundsDistance: new point is NOT COLLIDING the camera bounding box at point: " + newTargetPos.ToString("0.00000") + ". Calculating inbounds distance...");

            Vector3 inboundsPos = GetNearestPointInBounds(startPos, newTargetPos, dir);
            inBoundsDistance = Vector2.Distance(inboundsPos, startPos);

            return inBoundsDistance;
        }
    }
    public string GetTerrainTypeFromBall()
    {
        UpdateBallGroundMaterial();
        string ground = MyBall.bounceContactGroundMaterial;
        if (ground.Contains("deep rough"))
            ground = "deep rough";
        return ground;
    }
    [TargetRpc]
    public void RpcResetForNewHole(NetworkConnection conn, int holeIndex)
    {
        if (!this.IsOwner)
            return;
        ResetForNewHole(holeIndex);
    }
    public void ResetForNewHole(int holeIndex)
    {
        // Reset player score for the current hole
        PlayerScore.SaveScoreAtEndOfHole(holeIndex);
        //PlayerScore.ResetScoreForNewHole();
        PlayerScore.CmdResetScoreForNewHole();
        // Reset whether the player has teed off
        //this.HasPlayerTeedOff = false;
        CmdPlayerTeedOff(false);
        // reset the ball being in the hole
        //MyBall.IsInHole = false;
        MyBall.LocalIsInHole = false;
        MyBall.CmdSetBallInHoleOnServer(false);
        
        PlayerStruckByLightning = false;
    }
    public void LightningOnTurn()
    {
        PromptedForLightning = true;
    }
    public void SetDistanceToHoleForPlayer()
    {
        Vector3 ballPos = MyBall.transform.position;
        CmdSetDistanceToHoleForPlayer(ballPos);
        //GameObject closestHole = FindClosestHole(ballPos);
        //DistanceToHole = GetDistanceToHole(closestHole, ballPos);
    }
    [Server]
    public void ServerSetDistanceToHoleForPlayer()
    {
        Vector3 ballPos = MyBall.transform.position;
        GameObject closestHole = FindClosestHole(ballPos);
        DistanceToHole = GetDistanceToHole(closestHole, ballPos);
    }
    [ServerRpc]
    void CmdSetDistanceToHoleForPlayer(Vector3 ballPos)
    {
        GameObject closestHole = FindClosestHole(ballPos);
        DistanceToHole = GetDistanceToHole(closestHole, ballPos);
    }
    [ObserversRpc]
    public void RpcEnablePlayerSprite(bool enable)
    {
        EnablePlayerSprite(enable);
    }
    public void EnablePlayerSprite(bool enable)
    {
        Debug.Log("EnablePlayerSprite: " + enable.ToString());
        _golfAnimator.EnablePlayerSprite(enable);
    }
    public void StruckByLightning()
    {
        PlayerStruckByLightning = true;
        _golfAnimator.PlayerStruckByLightning();
        //PlayerUIMessage("struck by lightning");
        //EnablePlayerCanvas(true);
        PlayerScore.StrokePenalty(10);
    }
    public void StruckByLightningOver()
    {
        _golfAnimator.StartDeathFromLightning();
        //PlayerUIMessage("struck by lightning");
        //EnablePlayerCanvas(true);
        //EnablePlayerSprite(false);
    }
    public void LightningFlashForPlayerStruck(bool enableStruckSprite)
    {
        _golfAnimator.ChangeToStruckByLightningSprite(enableStruckSprite);
    }
    [TargetRpc]
    public void MovePlayerToPosition(NetworkConnection conn, Vector2 newPos, bool moveBall = false)
    {
        if (!this.IsOwner)
            return;
        Debug.Log("MovePlayerToPosition: new position is: " + newPos.ToString() + " and move the ball too? " + moveBall.ToString());
        this.transform.position = newPos;
        if(moveBall && MyBall)
            this.MyBall.transform.position = newPos;
    }
    [ServerRpc]
    void CmdStartCurrentPlayersTurnOnServer()
    {
        GameplayManagerTopDownGolf.instance.StartCurrentPlayersTurn(this);
    }
    [ServerRpc]
    void CmdSetCameraOnMyBallForOtherPlayers()
    {
        RpcSetCameraOnMyBallForOtherPlayers();
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcSetCameraOnMyBallForOtherPlayers()
    {
        UpdateCameraFollowTarget(MyBall.MyBallObject);
    }
    [ObserversRpc]
    public void RpcResetBallSpriteForNewHole()
    {
        if (!MyBall)
            return;
        MyBall.ResetBallSpriteForNewHole();
    }
    [ServerRpc]
    void CmdEndPlayersTurn()
    {
        this.IsPlayersTurn = false;
    }
    [ServerRpc]
    void CmdPlayerTeedOff(bool teedOff)
    {
        this.HasPlayerTeedOff = teedOff;
    }
    [ServerRpc]
    void CmdTellServerPlayerTeedOff()
    {
        GameplayManagerTopDownGolf.instance.PlayerTeedOff(this);
    }
    [ServerRpc]
    void CmdResetCurrentPlayer()
    {
        GameplayManagerTopDownGolf.instance.ResetCurrentPlayer();
    }
    [ServerRpc]
    void CmdTellClientsSelectedClub(int newClubIndex)
    {
        RpcTellClientsSelectedClub(newClubIndex);
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcTellClientsSelectedClub(int newClubIndex)
    {
        if (this.IsOwner)
            return;
        _currentClubIndex = newClubIndex;
        CurrentClub = _myClubs[_currentClubIndex];
        SetSelectedClubUI(CurrentClub);
    }
}
