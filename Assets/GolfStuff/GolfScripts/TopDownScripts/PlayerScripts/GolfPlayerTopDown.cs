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
using TMPro;
using UnityEngine.UI;
using System.Threading;
using UnityEngine.InputSystem;
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
    [SerializeField] TextMeshPro _playerNameText;

    [Header("Favor of the Gods")]
    [SerializeField] [SyncVar(OnChange = nameof(SyncFavorWeather))] public int FavorWeather;
    [SerializeField] [SyncVar] public int FavorWind;
    [SerializeField] [SyncVar] public float AccuracyFavorModifier = 1.0f;
    [SerializeField] [SyncVar] public float DistanceFavorModifier = 1.0f;


    [Header("Golf Ball Stuff")]
    [SerializeField] GameObject _golfBallPrefab;
    [SerializeField] [SyncVar(OnChange = nameof(SyncMyBallNetId))] int _myBallNetId;
    [SerializeField] public GolfBallTopDown MyBall;
    [SyncVar] public float DistanceToHole;
    [SyncVar] public int NumberOfBallsSpawnedForClient = 0;
    [SyncVar] public Color BallColor;


    [Header("Hit Values")]
    [SerializeField] public Vector2 hitDirection = Vector2.zero;
    [SerializeField] public float hitDistance = 0f; // changes based on type of club used. Clubs will have a "max distance" and the distance hit will be a % of the max distance based on the player submitted distance
    [SerializeField] public float hitAngle = 45f; // changes based on the type of club used? Drivers lower angles. Irons middle angles. Wedges higher angles?
    [SerializeField] [Range(-10f, 5f)] public float hitTopSpin = 0f;
    [SerializeField] [Range(-5f, 5f)] public float hitLeftOrRightspin = 0f;
    public bool IsShanked = false;
    [SyncVar] public bool IsShankedSynced = false;
    Vector2 _previousAimMovement = Vector2.zero;
    [SerializeField] Vector2 _normalizedDirection = new Vector2(1f, 1f).normalized;

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
    float _zoomOutTurnModifier = 1.0f;
    float distanceUpDown = 0f;
    [SerializeField] float _changeDistanceRate = 5f;

    [Header("Player Turn")]
    [SyncVar(OnChange = nameof(SyncIsPlayersTurn))] public bool IsPlayersTurn = false;
    public bool DirectionAndDistanceChosen = false;
    [SyncVar] public bool HasPlayerTeedOff = false;
    public bool PromptedForLightning = false;
    public bool PlayerStruckByLightning = false;
    [SerializeField] int _currentAimPointIndex = 0;


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
    public bool HitSequenceStarted = false;

    [Header("Hit Meter Submissions")]
    public float HitPowerSubmitted;
    public float HitAccuracySubmitted;
    public Vector2 ModifiedHitDirection = Vector2.zero;
    public Vector2 hitTopSpinSubmitted = Vector2.zero;
    bool _perfectPowerSubmission = false;
    bool _perfectAccuracySubmission = false;

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

    [Header("Power Ups")]
    [SyncVar] public bool HasPowerUp = false;
    [SyncVar] public string PlayerPowerUpType = null;
    string _playerPowerUpText = null;
    PowerUpTopDown _myPowerUp;
    [SyncVar(OnChange = nameof(SyncUsedPowerUpType))] public string UsedPowerUpType;
    [SyncVar(OnChange = nameof(SyncUsedPowerupThisTurn))] public bool UsedPowerupThisTurn = false;

    [Header("Power Up Effects")]
    [SyncVar(OnChange = nameof(SyncPowerUpDistanceModifier))] public float PowerUpDistanceModifier = 1.0f;
    [SyncVar] public float PowerUpAccuracyModifier = 1.0f;
    Vector2 _rocketMove = Vector2.zero;
    float _rocketBoost = 10f;
    [SerializeField] bool _canUseRocketPower = false;


    [Header("Mulligan Stuff")]
    [SyncVar(OnChange = nameof(SyncPlayerMulligan))] public bool PlayerMulligan = false;
    public bool PromptedForMulligan = false;
    [SerializeField] Vector3 _ballStartPosition = Vector3.zero;
    IEnumerator _mulliganRoutine;
    [SerializeField] bool _isMulliganRoutineRunning = false;

    [Header("Statue and Weather Effects")]
    [SyncVar] public bool BrokeGoodWeatherStatue = false;
    [SyncVar] public bool BrokeBadWeatherStatue = false;

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
    bool _tellPlayerGameIsOver = false;
    bool _tellPlayerHowFarTheyAreFromHoleForChallenege = false;
    bool _messageSentToPlayer = false;

    [Header("Sound References")]
    [SerializeField] ScriptableBallSounds _ballSounds;

    [Header("controls")]
    [SerializeField] public bool PromptPlayerControls = false;
    [SerializeField] bool _aimingActionsEnabled = false;

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
            GameplayManagerTopDownGolf.instance.CheckIfPlayerWhoLeftHadBallInHole(this);
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
            Debug.Log("ScriptableBallSound string for tee off: " + _ballSounds.HitTeeOff);
            SubscribeToControls();
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

        _normalizedDirection = _normalizedDirection.normalized;

    }
    public override void OnStopClient()
    {
        base.OnStopClient();
        if (this.IsOwner)
        {
            UnsubscribeToControls();
        }
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
            PlayerScoreBoard.instance.AddPlayerToScoreBoard(this, this.PlayerScore);
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
            _playerNameText.text = next;
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
            MyBall.UpdateBallColor(this.BallColor);
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
            Debug.Log("Player pressed space at time: " + Time.time.ToString());
        }
        if (!base.IsOwner)
        {
            // move the hit meter icons for other players
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

            return;
        }
        if (!MyBall)
            return;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            //PlayerScoreBoard.instance.OpenScoreBoard();
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            //PlayerScoreBoard.instance.CloseScoreBoard();
        }
        if (MyBall.isHit || MyBall.isBouncing || MyBall.isRolling)
            return;
        if (!IsPlayersTurn)
        {
            //if (GameplayManagerTopDownGolf.instance.CurrentPlayer == this && Input.GetKeyDown(KeyCode.Space) && Time.time >= (GameplayManagerTopDownGolf.instance.TimeSinceLastTurnStart + 0.15f))
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (GameplayManagerTopDownGolf.instance.CurrentPlayer == this && !this.PlayerMulligan)
                {
                    //Debug.Log("GolfPlayerTopDown: Player: " + this.PlayerName + " will start their turn after pressing space! Time: " + Time.time);
                    //this.EnablePlayerCanvas(false);
                    //// Begin old way of starting turn
                    ////GameplayManagerTopDownGolf.instance.StartCurrentPlayersTurn(this);
                    //// End old way of starting turn
                    //CmdStartCurrentPlayersTurnOnServer();
                }
                else if (this.PlayerMulligan && !this.UsedPowerupThisTurn)
                {
                    //SkipMulligan();
                }
            }
            //if (GameplayManagerTopDownGolf.instance.CurrentPlayer == this && Input.GetKeyDown(KeyCode.Space))
            //{
            //    Debug.Log("GolfPlayerTopDown: Player: " + this.PlayerName + " will start their turn after pressing space! Time: " + Time.time);
            //    this.EnablePlayerCanvas(false);
            //    // Begin old way of starting turn
            //    //GameplayManagerTopDownGolf.instance.StartCurrentPlayersTurn(this);
            //    // End old way of starting turn
            //    CmdStartCurrentPlayersTurnOnServer();
            //}
            //else if (PromptedForLightning && GameplayManagerTopDownGolf.instance.CurrentPlayer == this && Input.GetKeyDown(KeyCode.Backspace) && Time.time >= (GameplayManagerTopDownGolf.instance.TimeSinceLastSkip + 0.15f))
            else if (PromptedForLightning && GameplayManagerTopDownGolf.instance.CurrentPlayer == this && Input.GetKeyDown(KeyCode.Backspace))
            {
                //Debug.Log("GolfPlayerTopDown: Player: " + this.PlayerName + " is skipping their turn due to lightning. At time of: " + Time.time.ToString() + " and last skip was: " + GameplayManagerTopDownGolf.instance.TimeSinceLastSkip.ToString());
                //this.EnablePlayerCanvas(false);
                //PlayerScore.StrokePenalty(1);
                ////GameplayManagerTopDownGolf.instance.StartNextPlayersTurn(MyBall, true);
                //MyBall.CmdTellServerToStartNexPlayersTurn(true);
            }
            else if (Input.GetKeyDown(KeyCode.P) && this.PlayerMulligan)
            {
                //UseMulligan();
            }
            return;
        }

        // actions player can do while it is their "turn" aka after their turn as started and before they hit the ball
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //MyBall.HitBall(hitDistance, hitAngle, hitTopSpin, hitDirection);
            //EnableOrDisableLineObjects(false);
            //if (!IsPlayersTurn && !_moveHitMeterIcon)
            //    StartPlayerTurn();
            Debug.Log("Player (the owner) pressed space");
            if (this.PlayerStruckByLightning)
            {
                //Debug.Log("Player pressed space: this.PlayerStruckByLightning");
                //_golfAnimator.ResetGolfAnimator();
                //EnablePlayerSprite(false);
                //CmdDisableSpriteForLightningStrike();
                //this.EnablePlayerCanvas(false);
                //CmdEnablePlayerCanvasForOtherClients(false);
                ////this.IsPlayersTurn = false;
                //CmdEndPlayersTurn();

                //previousHitDistance = 0;

                //ActivateHitUIObjects(false);
                //CmdTellPlayersToActivateHitUIObjects(false);
                //AttachUIToNewParent(this.transform);
                //CmdTellPlayersToAttachUIToSelf();
                ////AttachPlayerToCamera(false, myCamera.transform);
                ////UpdateCameraFollowTarget(MyBall.MyBallObject);
                //EnableOrDisableLineObjects(false);
                //CmdTellPlayersToEnableOrDisableLineObjects(false);
                //this.PlayerStruckByLightning = false;
                //Debug.Log("GolfPlayerTopDown: Player: " + this.PlayerName + " has acknowledged they were struck by lightning! Moving on to next turn by calling: GameplayManagerTopDownGolf.instance.PlayerWasStruckByLightning(this). Time: " + Time.time);
                ////GameplayManagerTopDownGolf.instance.PlayerWasStruckByLightning(this);
                //CmdTellServerPlayerWasStruckByLightning();
                return;
            }
            if (!DirectionAndDistanceChosen && !_moveHitMeterIcon)
            {
                //Debug.Log("Player pressed space: !DirectionAndDistanceChosen && !_moveHitMeterIcon");
                //PlayerChooseDirectionAndDistance(true);
                ////EnableAimPositionControls(false);
                //EnableAimingActions(false);
                //if (_cameraViewHole.IsCameraZoomedOut)
                //    _cameraViewHole.ZoomOutCamera();
                //UpdateCameraFollowTarget(MyBall.MyBallObject);
                //CmdSetCameraOnMyBallForOtherPlayers();
                //GameplayManagerTopDownGolf.instance.PowerUpHideInstructions();
                Debug.Log(this.PlayerName + " pressed space with !DirectionAndDistanceChosen && !_moveHitMeterIcon");
                return;
            }
            else if (DirectionAndDistanceChosen && !_moveHitMeterIcon && !_golfAnimator.IsSwinging)
            {
                //StartHitMeterSequence();
            }
                
            else if (_moveHitMeterIcon)
            {
                //if (!_powerSubmitted)
                //{
                //    SetHitPowerValue();
                //}
                //else if (_powerSubmitted && !_accuracySubmitted)
                //{
                //    SetHitAccuracyValue();
                //    // change this to the player sending a command to the server to see if they swing normally or do the "struck by lightning" swing instead
                //    //_golfAnimator.StartSwing();
                //    GetPermissionToStartHitFromServer();
                //}
            }
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            //Debug.Log("Player pressed \"p\"");
            //if (this.IsPlayersTurn && !DirectionAndDistanceChosen && !_moveHitMeterIcon && !(MyBall.isHit || MyBall.isBouncing || MyBall.isRolling) && !this.PlayerMulligan)
            //{
            //    //Debug.Log("Player pressed \"p\": allowed to use powerup because: !DirectionAndDistanceChosen && !_moveHitMeterIcon && !(MyBall.isHit || MyBall.isBouncing || MyBall.isRolling)");
            //    this.UsePowerUp();
            //}
            //else if (this.PlayerMulligan)
            //{
            //    UseMulligan();
            //}

        }
        //if (Input.GetKeyDown(KeyCode.LeftControl) && !IsPlayersTurn)
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            ////MyBall.ResetPosition();
            ////EnableOrDisableLineObjects(true);
            //Debug.Log("LEft Control Pressed");
            //if (DirectionAndDistanceChosen && !_moveHitMeterIcon)
            //{
            //    PlayerChooseDirectionAndDistance(false);
            //    //EnableAimPositionControls(true);
            //    EnableAimingActions(true);
            //    UpdateCameraFollowTarget(_landingTargetSprite.gameObject);
            //    GameplayManagerTopDownGolf.instance.PowerUpShowInstructions(this);
            //}
        }
        //if (Input.GetKeyDown(KeyCode.Tab))
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //MyBall.PuttBall(hitDirection, hitDistance);
            //EnableOrDisableLineObjects(false);
            //if (!_moveHitMeterIcon)
            //{
            //    ChangeCurrentClub();
            //    if (DirectionAndDistanceChosen)
            //    {
            //        PlayerChooseDirectionAndDistance(false);
            //        UpdateCameraFollowTarget(_landingTargetSprite.gameObject);
            //    }
            //}
        }
        if (!_moveHitMeterIcon && CurrentClub.ClubType == "putter" && Input.GetKeyDown(KeyCode.LeftShift))
        {
            //SetPutterDistance(CurrentClub, true);
            //if (DirectionAndDistanceChosen)
            //{
            //    PlayerChooseDirectionAndDistance(false);
            //    UpdateCameraFollowTarget(_landingTargetSprite.gameObject);
            //}
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
                //ChangeHitDirection(aimLeftRight);
            }
            distanceUpDown = Input.GetAxisRaw("Vertical");
            if (distanceUpDown != 0)
            {
                //ChangeHitDistance(distanceUpDown);
            }
            if (_cameraViewHole.IsCameraZoomedOut)
            {
                //if (Input.GetMouseButtonDown(2))
                //{
                //    Vector3 mouseScreenPos = Input.mousePosition;
                //    //Debug.Log("Player clicked on screen position: " + mouseScreenPos.ToString());
                //    if (mouseScreenPos.x > 0 && mouseScreenPos.y > 0 && mouseScreenPos.y <= Screen.height && mouseScreenPos.x <= Screen.width)
                //    {
                //        //Debug.Log("Player clicked on screen position INSIDE of window");
                //        Vector3 mouseWorldPos = myCamera.ScreenToWorldPoint(mouseScreenPos);
                //        Vector2 mouseDir = (mouseWorldPos - MyBall.transform.position).normalized;
                //        //Debug.Log("Player clicked on screen/world position: " + mouseScreenPos.ToString() + "/" + mouseWorldPos.ToString() + ". Direction to mouse: " + mouseDir.ToString() + " compared to player's current aim position: " + hitDirection.ToString());
                //        if (this.IsOwner)
                //            ChangeDirectionFromMouseClick(mouseDir);
                //        //hitDirection = mouseDir;
                //    }
                //    else
                //        Debug.Log("Player clicked on screen position OUTSIDE of window");

                //}
            }
        }
        //if (Input.GetKeyDown(KeyCode.BackQuote) && !_powerSubmitted && !_accuracySubmitted)
        //{
        //    Debug.Log("GolfPlayer: BackQuote key pressed. _moveHitMeterIcon: " + _moveHitMeterIcon.ToString());
        //    _cameraViewHole.ZoomOutCamera();
        //}



    }
    private void FixedUpdate()
    {
        if (!MyBall)
            return;
        if (this.IsOwner)
        {
            if (!this.DirectionAndDistanceChosen)
            {
                //UpdateAimMovement();
                ChangeHitDirection(_previousAimMovement.x);
                ChangeHitDistance(_previousAimMovement.y);
            }
        }
        if ((hitDistance != previousHitDistance || hitAngle != previousHitAngle || hitTopSpin != previousHitTopSpin || hitDirection != previousHitDirection || hitLeftOrRightspin != previousHitLeftOrRightSpin) && (!MyBall.isHit && !MyBall.isBouncing && !MyBall.isRolling && IsPlayersTurn))
        {
            // Get the trajaectory line
            if (CurrentClub.ClubType != "putter")
                trajectoryPoints = MyBall.CalculateHitTrajectory(hitDistance, hitAngle, hitTopSpin, hitLeftOrRightspin, hitDirection, Vector3.zero, 0f);
            else
                trajectoryPoints = MyBall.CalculatePutterTrajectoryPoints(hitDistance, hitDirection);
            // Draw the trajectory
            drawTrajectoryTopDown.UpdateTrajectory(trajectoryPoints, MyBall, CurrentClub.ClubType, hitDistance);
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
            if (base.IsOwner)
                CmdUpdateHitValuesForOtherPlayers(hitDistance, hitAngle, hitTopSpin, hitDirection, hitLeftOrRightspin);
        }
        // allow player to move the ball when they use the rocket power up?
        if (this.IsOwner && this.MyBall.isHit)
        {
            if (!this.UsedPowerupThisTurn || this.UsedPowerUpType != "rocket")
                return;
            if (!this._canUseRocketPower)
            {
                MyBall.SetPlayerUsingRocket(false);
                return;
            }
            //_rocketMove.x = Input.GetAxisRaw("Horizontal");
            //_rocketMove.y = Input.GetAxisRaw("Vertical");
            if (_rocketMove == Vector2.zero)
            {
                MyBall.SetPlayerUsingRocket(false);
                return;
            }
            this.MyBall.MoveBallWithRocketPowerUp(_rocketMove.normalized, _rocketBoost);
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
        if (direction == 0)
            return;
        //Vector2 perpendicular = Vector2.Perpendicular(hitDirection);
        perpendicular = Vector2.Perpendicular(hitDirection);
        if (direction > 0)
            perpendicular *= -1f;

        turnRate = 5.5f / (hitDistance + 0.1f);
        Vector2 newDir = hitDirection + perpendicular * Time.deltaTime * turnRate * _zoomOutTurnModifier;
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
        //UpdateSpriteDirectionFromHitDirection(hitDirection);

        // old way?
        //hitDirection += perpendicular * Time.deltaTime * turnRate;

        //hitDirection = hitDirection.normalized;

        //turnRate = 0.5f / (hitDistance + 0.1f);

    }
    void ChangeHitDistance(float changeDirection)
    {
        if (changeDirection == 0)
            return;
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
    void ChangeDirectionFromMouseClick(Vector2 newDir)
    {
        Vector3 newTargetPos = MyBall.transform.position + (Vector3)(newDir.normalized * hitDistance);
        GetCameraBoundingBox();
        if (_cameraBoundingBox.OverlapPoint(newTargetPos))
        {
            //Debug.Log("ChangeDirectionFromMouseClick: new point is colliding with the camera bounding box at point: " + newTargetPos.ToString("0.00000"));
            hitDirection = newDir.normalized;
        }
        else
        {
            //Debug.Log("ChangeDirectionFromMouseClick: new point is NOT COLLIDING the camera bounding box at point: " + newTargetPos.ToString("0.00000") + ". Will try and adjust distance to stay in bounds?");            
        }
    }
    void UpdateSpriteDirectionFromHitDirection(Vector2 newDir)
    {
        Debug.Log("UpdateSpriteDirectionFromHitDirection: " + newDir.ToString());
        if (newDir.y >= _normalizedDirection.y)
        {
            this._golfAnimator.SetGolferDirection("up");
        }
        else if (newDir.y <= -_normalizedDirection.y)
        {
            this._golfAnimator.SetGolferDirection("down");
        }
        else
        {
            this._golfAnimator.SetGolferDirection("sideways");
        }
    }
    void UpdateAimMovement()
    {
        if (this.DirectionAndDistanceChosen)
        {
            _previousAimMovement = Vector2.zero;
            return;
        }
        if (_previousAimMovement == Vector2.zero)
            return;

        Vector2 direction = Vector2.ClampMagnitude(_previousAimMovement, 1);
        Vector3 previousAimPont = this.hitDirection * this.hitDistance;
        Vector3 newAimPosition = previousAimPont + (Vector3)(direction * _changeDistanceRate * Time.fixedDeltaTime);

        Debug.Log("UpdateAimMovement: Original position: " + previousAimPont.ToString() + " new aim direction: " + direction.ToString() + " new position: " + newAimPosition.ToString());
        // calculate distance of new position
        float distance = Vector2.Distance(Vector2.zero, newAimPosition); // since the "newAimPosition" is a localPosition, can ue Vector2.zero instead of call to transform.localPosition for the ball as well?
        if (distance > MaxDistanceFromClub)
            return;

        Debug.Log("UpdateAimMovement: set landing sprite to position of: " + _landingTargetSprite.transform.localPosition.ToString());
        this.hitDistance = distance;
        this.hitDirection = newAimPosition.normalized;
        UpdatePositionOfAdjustedDistanceIcon(this.hitDistance);
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
            if (this.IsOwner)
            {
                GameplayManagerTopDownGolf.instance.PowerUpShowInstructions(this);
                CanPlayerUsePowerUp();
            }
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
        HitSequenceStarted = false;
        UpdateBallGroundMaterial();

        // Find the closest hole to the player
        Vector3 ballPos = MyBall.transform.position;
        // Save the ball's starting position for if the player uses a mulligan
        this._ballStartPosition = ballPos;

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
                SetCurrentAimPoint(0);
            }

        }
        else
        { 
            if(GameplayManagerTopDownGolf.instance.CourseAimPoints.Count > 0)
                SetCurrentAimPoint(GameplayManagerTopDownGolf.instance.CourseAimPoints.Count - 1);
        }
        // Get the distance to the hole
        float distToHole = GetDistanceToHole(closestHole, ballPos);
        float distToAimPoint = Vector2.Distance(aimTarget, ballPos);
        hitDirection = SetInitialDirection(ballPos, aimTarget);
        // Find most appropriate club to start with based on distance to hole
        //_currentClubIndex = FindAppropriateClubToStart(distToHole);
        _currentClubIndex = FindAppropriateClubToStart(distToAimPoint);
        CurrentClub = _myClubs[_currentClubIndex];
        GameplayManagerTopDownGolf.instance.UpdateTerrainPenaltyText(IsThereATerrainFavorPenalty(MyBall.bounceContactGroundMaterial, CurrentClub.ClubType));


        // Update the Club UI stuff
        SetSelectedClubUI(CurrentClub);
        if (this.IsOwner)
        {
            //MyBall.UpdateBallZHeightLocally(0);
            CmdTellClientsSelectedClub(_currentClubIndex);
        }
            
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
        //EnableAimPositionControls(true);
        EnableAimingActions(true);
        EnableRocketControls(false);
        UpdateCameraFollowTarget(_landingTargetSprite.gameObject);

        PlayerScoreBoard.instance.UpdatePlayerScoreBoardItemPlayerFavor(this);
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
        Debug.Log("SetPlayerOnBall: ");
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

        if (!this.HasPlayerTeedOff)
        {
            Debug.Log("GetHitStatsFromClub: player has NOT teed off yet!");
            // Get direction to the aim point
            //bool isAimingAtAimPoint = false;
            //Vector2 dirToAimPoint = (GameplayManagerTopDownGolf.instance.TeeOffAimPoint - ballPos).normalized;
            //if (Vector2.Angle(dirToAimPoint, hitDirection) < 15f)
            //{
            //    Debug.Log("GetHitStatsFromClub: Player aim direction is LESS than 15 degrees away from aim point");
            //    isAimingAtAimPoint = true;
            //}
            //else
            //{
            //    Debug.Log("GetHitStatsFromClub: Player aim direction is MORE than 15 degrees away from aim point");
            //}

            if (IsPlayerAimingAtAimPoint(ballPos, GameplayManagerTopDownGolf.instance.TeeOffAimPoint))
            {
                float distanceToAimPoint = Vector2.Distance(MyBall.transform.position, GameplayManagerTopDownGolf.instance.TeeOffAimPoint);
                if (MaxDistanceFromClub > distanceToAimPoint)
                {
                    newDist = distanceToAimPoint;
                }
            }
            // Get the distance to the tee off point. If the max distance from club is greater than distance to tee off aim point, set the hit distance to distance to tee off aim point
            //float distanceToAimPoint = Vector2.Distance(MyBall.transform.position, GameplayManagerTopDownGolf.instance.TeeOffAimPoint);
            //if (MaxDistanceFromClub > distanceToAimPoint && isAimingAtAimPoint)
            //{
            //    newDist = distanceToAimPoint;
            //}
        }
        else
        {
            Debug.Log("GetHitStatsFromClub: player HAS teed off already!");
            Vector2 dirToHole = (FindClosestHole(ballPos).transform.position - ballPos).normalized;

            if (IsPlayerAimingAtAimPoint(ballPos, FindClosestHole(ballPos).transform.position))
            {
                if (MaxDistanceFromClub > this.DistanceToHole)
                {
                    newDist = this.DistanceToHole;
                }
            }
        }

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
            Debug.Log("GetHitStatsFromClub: new point is NOT COLLIDING the camera bounding box at point: " + newTargetPos.ToString("0.00000") + ". CHANGING hit distance to: " + hitDistance + " from original distance: " + newDist.ToString());
        }

        hitAngle = DefaultLaunchAngleFromClub;
        //SpinDividerFromClub = 1f; // get this from the actual club object later?
        UpdatePositionOfAdjustedDistanceIcon(hitDistance);
        MinDistance = GetMinDistance(hitDistance);
        // maybe also set the icon movement speed based on the club?
    }
    bool IsPlayerAimingAtAimPoint(Vector3 ballPos, Vector3 aimPos)
    {
        Vector2 dirToAimPoint = (aimPos - ballPos).normalized;
        if (Vector2.Angle(dirToAimPoint, hitDirection) < 15f)
        {
            Debug.Log("IsPlayerAimingAtAimPoint: Player aim direction is LESS than 15 degrees away from aim point");
            return true;
        }
        else
        {
            Debug.Log("IsPlayerAimingAtAimPoint: Player aim direction is MORE than 15 degrees away from aim point");
            return false;
        }
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
        _adjustedDistanceIcon.transform.localPosition = new Vector3(adjustedDistancePosition, 0f, 0f);
        TargetDistanceXPosForPlayer = adjustedDistancePosition;

    }
    void StartHitMeterSequence()
    {
        Debug.Log("StartHitMeterSequence: ");
        // Placeholder until the actual hit meter stuff is added
        //MyBall.HitBall(hitDistance, hitAngle, hitTopSpin, hitDirection);
        ResetIconPositions();
        ResetSubmissionValues();
        ResetPerfectSubmissions();
        BeginMovingHitMeter();

        // Make sure the camera isn't zoomed out?
        if (_cameraViewHole.IsCameraZoomedOut)
        {
            _cameraViewHole.ZoomOutCamera();
            EnableZoomOutAim(_cameraViewHole.IsCameraZoomedOut);
        }
            

        // IsPlayersTurn will be set by the game manager in a real game. This is just a place holder for now.
        //this.IsPlayersTurn = false;
        if (IsOwner)
            CmdStartHitMeterSequence();
    }
    [ServerRpc]
    void CmdStartHitMeterSequence()
    {
        RpcStartHitMeterSequence();
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcStartHitMeterSequence()
    {
        ResetIconPositions();
        ResetSubmissionValues();
        BeginMovingHitMeter();

        // Make sure the camera isn't zoomed out?
        if (_cameraViewHole.IsCameraZoomedOut)
        {
            _cameraViewHole.ZoomOutCamera();
            EnableZoomOutAim(_cameraViewHole.IsCameraZoomedOut);
        }
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
        else if (_moveRight && !_powerSubmitted && newPos.x > _centerAccuracyPosition)
        {
            Debug.Log("MoveHitMeterIcon: Icon bounced back and is past origin point without player submitting a power value. Shank?");
            _moveHitMeterIcon = false;
            ActivateMovingIcon(false);
            SetShank();
            GetPermissionToStartHitFromServer();
        }
        else if (newPos.x > _furthestRightAccuracyPosition)
        {
            newPos.x = _furthestRightAccuracyPosition;
            _moveHitMeterIcon = false;
            ActivateMovingIcon(false);

            if (!_accuracySubmitted && this.IsOwner)
            {
                Debug.Log("MoveHitMeterIcon: Accuracy meter off the right edge without accuracy submitted by player. SHANKED!!!");
                //IsShanked = true;
                SetShank();
                // change this to the player sending a command to the server to see if they swing normally or do the "struck by lightning" swing instead
                //_golfAnimator.StartSwing();
                GetPermissionToStartHitFromServer();
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
        {
            iconXPosition = TargetDistanceXPosForPlayer;
            PerfectPowerSubmission();
        }

        ActivateSubmissionIcon(_hitMeterPowerSubmissionIcon, iconXPosition);
        HitPowerSubmitted = GetHitPowerFromXPosition(iconXPosition, MaxDistanceFromClub);
        _powerSubmitted = true;
        if (IsOwner)
            CmdSetHitPowerValue(iconXPosition);

    }
    [ServerRpc]
    void CmdSetHitPowerValue(float iconPosition)
    {
        RpcSetHitPowerValue(iconPosition);
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcSetHitPowerValue(float iconPosition)
    {
        ActivateSubmissionIcon(_hitMeterPowerSubmissionIcon, iconPosition);
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
        {
            iconXPosition = _centerAccuracyPosition;
            PerfectAccuracySubmission();
        }

        ActivateSubmissionIcon(_hitMeterAccuracySubmissionIcon, iconXPosition);

        float accuracyDistance = GetAccuracyDistance(iconXPosition, _centerAccuracyPosition);
        if (!IsShanked)
        {
            ModifiedHitDirection = ModifyHitDirectionFromAccuracy(hitDirection, accuracyDistance);
        }
        _accuracySubmitted = true;
        if (IsOwner)
            CmdSetHitAccuracyValue(iconXPosition);
    }
    [ServerRpc]
    void CmdSetHitAccuracyValue(float iconPosition)
    {
        RpcSetHitAccuracyValue(iconPosition);
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcSetHitAccuracyValue(float iconPosition)
    {
        ActivateSubmissionIcon(_hitMeterAccuracySubmissionIcon, iconPosition);
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
            //IsShanked = true;
            SetShank();
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
        if (this.UsedPowerupThisTurn && this.UsedPowerUpType == "accuracy")
        {
            Debug.Log("ModifyHitDirectionFromAccuracy: Player used accuracy power up. Return perfect accuracy by returning the original direction?");
            return direction;
        }


        accuracyDistance *= -2.5f;
        // Punish innaccurate putts more than regular hits?
        if (CurrentClub.ClubType == "putter")
            accuracyDistance *= 2.5f;
        // Also punish driver hits?
        if (CurrentClub.ClubType == "driver")
        { 
            if(this.HasPlayerTeedOff)
                accuracyDistance *= 2.5f;
            else
                accuracyDistance *= 1.5f;
        }
            

        // Adjust the accuracy distance based on player's weather favor
        float newAccuracyDistance = accuracyDistance * this.AccuracyFavorModifier * this.PowerUpAccuracyModifier;
        Debug.Log("ModifyHitDirectionFromAccuracy: " + this.PlayerName + "'s original accuracy distance is: " + accuracyDistance.ToString() + " but their new accuracy distance will be: " + newAccuracyDistance.ToString() + " based on AccuracyFavorModifier of: " + AccuracyFavorModifier.ToString() + " and power up modifier of: " + this.PowerUpAccuracyModifier.ToString());

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
        if (!this.IsOwner)
            return;
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
        else
        {
            CheckTerrainFavorPenalty(MyBall.bounceContactGroundMaterial, CurrentClub.ClubType);
        }

        this.PlayerScore.PlayerHitBall();
        //GameplayManagerTopDownGolf.instance.ResetCurrentPlayer();
        CmdResetCurrentPlayer();
        //GameplayManagerTopDownGolf.instance.LightningAfterPlayerHit();

        // save info for what sound to play to later be sent to the clients
        string soundName = null;
        float soundVolume = 1.0f;

        if (!IsShanked)
        {
            if (CurrentClub.ClubType == "putter")
            {
                MyBall.PuttBall(ModifiedHitDirection, HitPowerSubmitted);
                //MyBall.PuttBall(new Vector2 (1f,0f), 10f);                
                //SoundManager.instance.PlaySound("golfball-hit", 0.8f);
                soundName = _ballSounds.HitOffGround;
                soundVolume = 0.8f;
            }
            else
            {
                MyBall.HitBall(HitPowerSubmitted, hitAngle, hitTopSpin, ModifiedHitDirection, hitLeftOrRightspin);
                //MyBall.HitBall(CurrentClub.MaxHitDistance, hitAngle, hitTopSpin, ModifiedHitDirection, hitLeftOrRightspin);
                if (playerTeeOffSound)
                {
                    //SoundManager.instance.PlaySound("golfball-teeoff", 1f);
                    soundName = _ballSounds.HitTeeOff;
                }
                else
                {
                    SoundManager.instance.PlaySound("golfball-hit", 1f);
                    soundName = _ballSounds.HitOffGround;
                }

            }
            if (this._canUseRocketPower)
                EnableRocketControls(true);
            else
                EnableRocketControls(false);
        }
        else
        {
            Debug.Log("Was ball shanked? " + IsShanked.ToString());
            DoTheShank();
            SoundManager.instance.PlaySound("golfball-shank", 1f);
            soundName = _ballSounds.HitShank;
            //MyBall.HitBall(100, hitAngle, hitTopSpin, new Vector2(1f, 0f), hitLeftOrRightspin);
        }
        if (!string.IsNullOrEmpty(soundName))
        {
            SoundManager.instance.PlaySound(soundName, soundVolume);
            CmdPlaySoundForClients(soundName, soundVolume);
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

        //SpawnPowerUpObjects();
        CmdRemoveUsedPowerUps();

    }
    [ServerRpc]
    void CmdPlaySoundForClients(string soundName, float soundVolume)
    {
        RpcPlaySoundForClients(soundName, soundVolume);
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcPlaySoundForClients(string soundName, float soundVolume)
    {
        if (string.IsNullOrEmpty(soundName))
            return;
        SoundManager.instance.PlaySound(soundName, soundVolume);
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
        if (DirectionAndDistanceChosen)
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

        GameplayManagerTopDownGolf.instance.UpdateTerrainPenaltyText(IsThereATerrainFavorPenalty(MyBall.bounceContactGroundMaterial, CurrentClub.ClubType));

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

        // Modify the max distance of the club based on the player's distance favor modifer, but not for putters
        if (club.ClubType != "putter")
        {
            //dist *= this.DistanceFavorModifier;
            //float newDist = dist * this.DistanceFavorModifier;
            float newDist = dist * this.DistanceFavorModifier * this.PowerUpDistanceModifier;
            Debug.Log("GetDistanceFromClub: New hit distance of: " + newDist.ToString() + " compared to original distance of: " + dist.ToString() + ". Player " + this.PlayerName + " has DistanceFavorModifier of: " + DistanceFavorModifier.ToString() + " based on favor of: " + this.FavorWeather.ToString());
            dist = newDist;
        }

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
        if (GameplayManagerTopDownGolf.instance.IsTeeOffChallenge)
        {
            if (club.ClubType != GameplayManagerTopDownGolf.instance.TeeOffChallengeClubType)
                return false;
        }
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

        if (GameplayManagerTopDownGolf.instance.IsTeeOffChallenge)
        {
            return GetClubIndexForTeeOffChallenege(GameplayManagerTopDownGolf.instance.TeeOffChallengeClubType);
        }

        for (int i = 0; i < _myClubs.Length; i++)
        {
            if (CanClubBeUsedOnCurrentGround(_myClubs[i]))
            {
                return i;
            }
        }

        return firstIndex;
    }
    void SetShank()
    {
        Debug.Log("SetShank: ");
        this.IsShanked = true;
        EnableSubmitHit(false);
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
            float shankTopSpin = ShankTopSpin(MaxBackSpinFromClub, MaxTopSpinFromClub);
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
        //cameraFollowScript.UpdateCameraFollow(objectToFollow.transform);
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

        if (GameplayManagerTopDownGolf.instance.IsTeeOffChallenge)
        {
            return GetClubIndexForTeeOffChallenege(GameplayManagerTopDownGolf.instance.TeeOffChallengeClubType);
        }


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
    int GetClubIndexForTeeOffChallenege(string clubType)
    {
        Debug.Log("GetClubIndexForTeeOffChallenege: for type: " + clubType);
        for (int i = 0; i < _myClubs.Length; i++)
        {
            if (_myClubs[i].ClubType == clubType)
                return i;
        }
        return 0;
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
        //GameObject[] flagHoles = GameObject.FindGameObjectsWithTag("golfHole");
        GameObject[] flagHoles = GameplayManagerTopDownGolf.instance.HoleHoleObjects.ToArray();
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
    [ServerRpc]
    void CmdEnablePlayerCanvasForOtherClients(bool enable)
    {
        RpcEnablePlayerCanvasForOtherClients(enable);
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcEnablePlayerCanvasForOtherClients(bool enable)
    {
        EnablePlayerCanvas(enable);
    }
    [ObserversRpc]
    public void RpcEnablePlayerCanvasForNewTurn(bool enable)
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
        try
        {
            _playerCanvas.gameObject.SetActive(enable);
        }
        catch (Exception e)
        {
            Debug.Log("EnablePlayerCanvas: error: " + e);
        }
    }
    [TargetRpc]
    public void RpcPlayerUIMessage(NetworkConnection conn, string message)
    {
        //Debug.Log("RpcPlayerUIMessage: for player: " + this.PlayerName + " message: " + message);
        if (!this.IsOwner)
            return;
        this.EnablePlayerCanvas(true);
        PlayerUIMessage(message);
    }
    public void PlayerUIMessage(string message)
    {
        //Debug.Log("PlayerUIMessage: for player: " + this.PlayerName + " message: " + message);
        if (message == "lightning")
            PromptedForLightning = true;
        else if (message == "start turn")
            PromptedForLightning = false;
        else if (message == "mulligan")
            PromptedForMulligan = true;
        _playerUIMessage.UpdatePlayerMessageText(message);
        if (this.IsOwner && (!message.StartsWith("mulligan")))
            CmdTellClientsUpdatePlayerMessageText(message);
    }
    [ServerRpc]
    void CmdTellClientsUpdatePlayerMessageText(string newMessage)
    {
        //Debug.Log("CmdTellClientsUpdatePlayerMessageText: for player: " + this.PlayerName + " message: " + newMessage);
        if (newMessage == "lightning" || newMessage == "start turn")
            return;
        RpcTellClientsUpdatePlayerMessageText(newMessage);
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcTellClientsUpdatePlayerMessageText(string newMessage)
    {
        //Debug.Log("RpcTellClientsUpdatePlayerMessageText: with message: " + newMessage);
        this.EnablePlayerCanvas(true);
        _playerUIMessage.UpdatePlayerMessageText(newMessage);
    }
    [Server]
    public async Task ServerAskPlayerIfTheyWantToMulligan(int duration)
    {

        Debug.Log("ServerAskPlayerIfTheyWantToMulligan: for player: " + this.PlayerName + " started at time: " + Time.time.ToString());
        this.PlayerMulligan = true;
        this.RpcPromptPlayerForMulligan(duration);
        //StartCoroutine(MulliganPromptCountDown(duration));
        while (PlayerMulligan)
        {
            await Task.Yield();
        }
        //this.HasPowerUp = false;
        //this.PlayerPowerUpType = "";
        Debug.Log("ServerAskPlayerIfTheyWantToMulligan: for player: " + this.PlayerName + " ended at time: " + Time.time.ToString());
        //GameplayManagerTopDownGolf.instance.StartNextPlayersTurn(this.MyBall);

    }
    IEnumerator MulliganPromptCountDown(int timeRemaining)
    {
        Debug.Log("MulliganPromptCountDown: starting...");
        _isMulliganRoutineRunning = true;
        EnableUsePowerUp(true);
        EnableSkipMulligan(true);
        while (timeRemaining > 0 && PlayerMulligan)
        {
            //RpcMulliganCountdown(this.Owner, timeRemaining);
            PlayerUIMessage("mulligan " + timeRemaining.ToString());
            yield return new WaitForSeconds(1.0f);
            timeRemaining--;

        }
        _isMulliganRoutineRunning = false;
        EnableUsePowerUp(false);
        EnableSkipMulligan(false);
        this.SkipMulligan();
        yield break;
    }
    [ObserversRpc]
    void RpcPromptPlayerForMulligan(int duration)
    {
        PlayerUIMessage("mulligan " + duration.ToString());
        EnablePlayerCanvas(true);

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
        {
            MyBall.TellPlayerGroundTypeTheyLandedOn();
            //MyBall.CheckIfInStatueRingRadius();
        }
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
        RpcTellServerPlayerHoleEndedCompleted();
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcTellServerPlayerHoleEndedCompleted()
    {
        this.EnablePlayerCanvas(false);
    }
    [Server]
    public async Task ServerTellPlayerGameIsOver(float duration)
    {
        _tellPlayerGameIsOver = true;
        float end = Time.time + (duration * 2);
        //RpcTellPlayerGroundTheyLandedOn(duration);
        RpcTellPlayerGameIsOver(this.Owner, duration);
        Debug.Log("ServerTellPlayerGameIsOver: Start time is: " + Time.time.ToString());
        while (_tellPlayerGameIsOver)
        {
            //Debug.Log("ServerTellPlayerGroundTheyLandedOn: Task.Yield time is: " + Time.time.ToString());
            await Task.Yield();
            if (Time.time >= end)
                _tellPlayerGameIsOver = false;
        }
        Debug.Log("ServerTellPlayerGameIsOver: End time is: " + Time.time.ToString());
    }
    [TargetRpc]
    void RpcTellPlayerGameIsOver(NetworkConnection conn, float duration)
    {
        StartTellPlayerGameIsOver(duration);
    }
    public async void StartTellPlayerGameIsOver(float duration)
    {
        await TellPlayerGameIsOver(duration);
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
        if (this.IsOwner)
            CmdTellServerPlayerGameIsOverCompleted();
    }
    [ServerRpc]
    void CmdTellServerPlayerGameIsOverCompleted()
    {
        _tellPlayerGameIsOver = false;
        RpcTellServerPlayerGameIsOverCompleted();
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcTellServerPlayerGameIsOverCompleted()
    {
        this.EnablePlayerCanvas(false);
    }
    public void GetCameraBoundingBox(bool forceUpdate = false)
    {
        if (!_cameraBoundingBox)
            _cameraBoundingBox = GameObject.FindGameObjectWithTag("CameraBoundingBox").GetComponent<PolygonCollider2D>();
        if (forceUpdate)
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
        if (_cameraViewHole.IsCameraZoomedOut)
        {
            _cameraViewHole.ZoomOutCamera();
            EnableZoomOutAim(_cameraViewHole.IsCameraZoomedOut);
        }
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
    [ServerRpc]
    void CmdDisableSpriteForLightningStrike()
    {
        RpcEnablePlayerSprite(false);
    }
    public void EnablePlayerSprite(bool enable)
    {
        Debug.Log("EnablePlayerSprite: " + enable.ToString() + " for player: " + this.PlayerName);
        _playerNameText.gameObject.SetActive(enable);
        _golfAnimator.EnablePlayerSprite(enable);
    }
    public void StruckByLightning()
    {
        if (!this.IsOwner)
        {
            //_golfAnimator.PlayerStruckByLightningForClients();
            return;
        }

        PlayerStruckByLightning = true;
        _golfAnimator.PlayerStruckByLightning();
        //PlayerUIMessage("struck by lightning");
        //EnablePlayerCanvas(true);
        PlayerScore.StrokePenalty(10);
        EnableAcknowledgePlayerLightningStrike(true);
    }
    public void StruckByLightningOver()
    {
        if (!this.IsOwner)
            return;
        _golfAnimator.StartDeathFromLightning();
        //PlayerUIMessage("struck by lightning");
        //EnablePlayerCanvas(true);
        //EnablePlayerSprite(false);
    }
    public void LightningFlashForPlayerStruck(bool enableStruckSprite)
    {
        if (!this.IsOwner)
            return;
        Debug.Log("LightningFlashForPlayerStruck: " + enableStruckSprite.ToString() + " for player: " + this.PlayerName);
        _golfAnimator.ChangeToStruckByLightningSprite(enableStruckSprite);
    }
    [TargetRpc]
    public void MovePlayerToPosition(NetworkConnection conn, Vector2 newPos, bool moveBall = false)
    {
        if (!this.IsOwner)
            return;
        Debug.Log("MovePlayerToPosition: new position is: " + newPos.ToString() + " and move the ball too? " + moveBall.ToString());
        this.transform.position = newPos;
        if (moveBall && MyBall)
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
    [ServerRpc]
    void CmdTellServerPlayerWasStruckByLightning()
    {
        GameplayManagerTopDownGolf.instance.PlayerWasStruckByLightning(this);
    }
    public void StatueEffectFromBallLanding(string effect)
    {
        if (!this.IsOwner)
            return;
        CmdStatueEffectFromBallLanding(effect);
    }
    [ServerRpc]
    void CmdStatueEffectFromBallLanding(string effect)
    {
        int newWeatherFavor = FavorWeather;
        int newWindFavor = FavorWind;

        if (effect == "good")
        {
            newWeatherFavor++;
        }
        else if (effect == "bad")
        {
            newWeatherFavor--;
        }
        else if (effect == "wind") // probably need to think of a way to make player be able to improve wind favor?
        {
            newWindFavor--;
        }

        if (newWeatherFavor != FavorWeather)
            FavorWeather = CapWeatherFavor(newWeatherFavor);
        if (newWindFavor != FavorWind)
            FavorWind = CapWeatherFavor(newWindFavor);

    }
    public void BrokenStatuePenalty(string statueType)
    {
        CmdBrokenStatuePenalty(statueType);
    }
    [ServerRpc]
    void CmdBrokenStatuePenalty(string statueType)
    {
        int newFavor = FavorWeather;
        if (statueType == "good-weather")
            newFavor += 5;
        else if (statueType == "bad-weather")
            newFavor -= 5;

        this.SetPlayerBrokenStatueEffect(statueType);
        GameplayManagerTopDownGolf.instance.BrokenStatueWeatherEffects(statueType, this);

        FavorWeather = CapWeatherFavor(newFavor);
    }
    [Server]
    void SetPlayerBrokenStatueEffect(string statueType)
    {
        if (statueType == "good-weather")
        {
            this.BrokeGoodWeatherStatue = true;
            if (this.BrokeBadWeatherStatue)
            {
                ResetBrokenBadStatueEffect();
            }
        }
        else if (statueType == "bad-weather")
        {
            this.BrokeBadWeatherStatue = true;
            if (this.BrokeGoodWeatherStatue)
            {
                ResetBrokenGoodStatueEffect();
            }
        }
    }
    [Server]
    public void ResetBrokenGoodStatueEffect()
    {
        this.BrokeGoodWeatherStatue = false;
    }
    [Server]
    public void ResetBrokenBadStatueEffect()
    {
        this.BrokeBadWeatherStatue = false;
    }
    void CheckTerrainFavorPenalty(string terrainType, string clubType)
    {
        Debug.Log("CheckTerrainFavorPenalty: club: " + clubType + " on terrain: " + terrainType);
        CmdCheckTerrainFavorPenalty(terrainType, clubType);
    }
    [ServerRpc]
    void CmdCheckTerrainFavorPenalty(string terrainType, string clubType)
    {
        Debug.Log("CmdCheckTerrainFavorPenalty: on server: club: " + clubType + " on terrain: " + terrainType);
        if (IsThereATerrainFavorPenalty(terrainType, clubType))
        {
            this.FavorWeather = CapWeatherFavor(FavorWeather - 1);
        }
    }
    bool IsThereATerrainFavorPenalty(string terrainType, string clubType)
    {
        if (!this.HasPlayerTeedOff)
            return false;
        if (terrainType == "green" && clubType != "putter")
        {
            Debug.Log("IsThereATerrainFavorPenalty: not putter on green");
            return true;
        }
        if (terrainType == "fairway" && clubType == "driver")
        {
            Debug.Log("IsThereATerrainFavorPenalty: driver on fairway");
            return true;
        }

        return false;
    }
    [Server]
    int CapWeatherFavor(int newFavor)
    {
        if (newFavor > 10)
            newFavor = 10;
        if (newFavor < -10)
            newFavor = -10;

        return newFavor;
    }
    [Server]
    public void ResetFavorWeatherForNewGame()
    {
        this.FavorWeather = 0;
    }
    void SyncFavorWeather(int prev, int next, bool asServer)
    {
        if (asServer)
        {
            Debug.Log("SyncFavorWeather: as server? " + asServer.ToString());
            SetAccuracyFavorModifier(next);
            SetDistanceFavorModifier(next);
            return;
        }
        PlayerScoreBoard.instance.UpdatePlayerScoreBoardItemPlayerFavor(this);
    }
    [Server]
    void SetAccuracyFavorModifier(int newPlayerFavor)
    {
        float newFavorModifier = (float)newPlayerFavor / 100f;
        if (newPlayerFavor > 0)
            newFavorModifier *= 2f;
        this.AccuracyFavorModifier = 1.0f - newFavorModifier;
        Debug.Log("SetAccuracyFavorModifier: new accuracy modifier for player " + this.PlayerName + " will be: " + AccuracyFavorModifier.ToString() + " based on their weather favor of: " + newPlayerFavor.ToString());
    }
    [Server]
    void SetDistanceFavorModifier(int newPlayerFavor)
    {
        float newFavorModifier = (float)newPlayerFavor / 50f;
        if (newFavorModifier > 0)
            newFavorModifier /= 2f;

        this.DistanceFavorModifier = 1.0f + newFavorModifier;
        Debug.Log("SetDistanceFavorModifier: new distance modifier for player " + this.PlayerName + " will be: " + DistanceFavorModifier.ToString() + " based on their weather favor of: " + newPlayerFavor.ToString());
    }
    [Server]
    public void NewPowerUpForPlayer(string newPowerUpType, string newPowerUpText)
    {
        Debug.Log("NewPowerUpForPlayer: " + this.PlayerName + " " + newPowerUpType);
        this.HasPowerUp = true;
        this.PlayerPowerUpType = newPowerUpType;
        this.RpcPlayerGotNewPowerUp(newPowerUpType, newPowerUpText);
    }
    [ObserversRpc(BufferLast = true)]
    void RpcPlayerGotNewPowerUp(string newPowerUpType, string newPowerUpText)
    {
        Debug.Log("RpcPlayerGotNewPowerUp: new power up for player of type: " + newPowerUpType);
        this._playerPowerUpText = newPowerUpText;
        GameplayManagerTopDownGolf.instance.PlayerGotNewPowerUp(newPowerUpType, newPowerUpText);
        //StartCoroutine(NewPowerUpMessage(newPowerUpText));
    }
    //IEnumerator NewPowerUpMessage(string newPowerUpText)
    //{
    //    bool wasPowerUpCanvasEnabled = _powerUpCanvas.gameObject.activeInHierarchy;
    //    _powerUpMessageText.text = "New Power Up! " + newPowerUpText;
    //    _powerUpCanvas.gameObject.SetActive(true);
    //    yield return new WaitForSeconds(5.0f);
    //    _powerUpMessageText.text = "";
    //    if (!wasPowerUpCanvasEnabled)
    //        _powerUpCanvas.gameObject.SetActive(false);
    //}
    //public void UpdatePlayerPowerUpSprite(Sprite newPowerUpSprite)
    //{
    //    this._powerUpImage.texture = newPowerUpSprite.texture;
    //}
    public void NewPowerUpObject(PowerUpTopDown newPowerUp, bool removePowerUp = false)
    {
        if (removePowerUp && this._myPowerUp == newPowerUp)
        {
            this._myPowerUp = null;
            return;
        }
        this._myPowerUp = newPowerUp;
    }
    void UsePowerUp()
    {
        if (!this.IsOwner)
            return;
        if (!this.HasPowerUp)
            return;
        if (this.UsedPowerupThisTurn)
        {
            // future functionality to cancel power up usage? Or maybe just have it so if you use it, it's gone forever. Sorry.
            return;
        }
        Debug.Log("UsePowerUp: for player: " + this.PlayerName + " and their power up of type: " + this.PlayerPowerUpType);
        CmdUsePowerUp();
    }
    [ServerRpc]
    void CmdUsePowerUp()
    {
        if (!this.HasPowerUp)
            return;
        if (this.UsedPowerupThisTurn)
            return;
        Debug.Log("CmdUsePowerUp: for player: " + this.PlayerName + " and their power up of type: " + this.PlayerPowerUpType);
        PowerUpManagerTopDownGolf.instance.PlayerIsUsingPowerUp(this.ObjectId);
    }
    void SyncUsedPowerUpType(string prev, string next, bool asServer)
    {
        if (asServer)
        {
            return;
        }
        if (!this.IsOwner)
        {
            return;
            
        }
        if (next == "higher")
        {
            this.previousHitDistance = 0f;
        }
        Debug.Log("SyncUsedPowerUpType: " + next + " on player: " + this.PlayerName);
    }
    void SyncUsedPowerupThisTurn(bool prev, bool next, bool asServer)
    {
        if (asServer)
            return;
        GameplayManagerTopDownGolf.instance.SetInstructionsText(next);
    }
    [Server]
    public void SetUsedPowerUpType(string usedType)
    {
        Debug.Log("SetUsedPowerUpType: " + usedType + " on player: " + this.PlayerName);
        this.UsedPowerUpType = usedType;
        this.UsedPowerupThisTurn = true;
        PlayerUsedPowerUp(usedType);
        GetPowerUpEffect(usedType);
    }
    [Server]
    public void PlayerUsedPowerUp(string usedPowerUpType)
    {
        Debug.Log("PlayerUsedPowerUp: " + this.PlayerName + " " + usedPowerUpType);
        this.HasPowerUp = false;
        this.PlayerPowerUpType = null;
        // replace with RPC that just says the player used the power up? Should alert all clients?
        //this.RpcPlayerGotNewPowerUp(newPowerUpType, newPowerUpText);
    }
    [Server]
    void GetPowerUpEffect(string usedPowerUpType)
    {
        if (string.IsNullOrEmpty(usedPowerUpType))
            return;
        Debug.Log("GetPowerUpEffect: " + usedPowerUpType);
        switch (usedPowerUpType)
        {
            case "power":
                PowerPowerUpEffect();
                break;
            case "accuracy":
                AccuracyPowerUpEffect();
                break;
            case "rain":
                RainPowerUpEffect();
                break;
            case "wind":
                WindPowerUpEffect();
                break;
            case "rocket":
                RpcRocketPowerUpEffect(this.Owner, true);
                break;
        }
    }
    [Server]
    void PowerPowerUpEffect()
    {
        this.PowerUpDistanceModifier = 1.5f;
    }
    void SyncPowerUpDistanceModifier(float prev, float next, bool asServer)
    {
        if (asServer)
            return;
        if (!this.IsOwner)
            return;
        if (next != 1.0f)
        {
            Debug.Log("SyncPowerUpDistanceModifier: Player " + this.PlayerName + "'s PowerUpDistanceModifier is not 1.0f. Forcing a recalculation of trajectory to increase max distance?");
            GetNewClubAttributes(CurrentClub);
            GetHitStatsFromClub();
        }
    }
    [Server]
    void AccuracyPowerUpEffect()
    {
        this.PowerUpAccuracyModifier = 0.5f;
    }
    [Server]
    void RainPowerUpEffect()
    {
        RainManager.instance.RainPowerUpUsed(this);
    }
    [Server]
    void WindPowerUpEffect()
    {
        WindManager.instance.WindPowerUpUsed(this);
    }
    [TargetRpc]
    void RpcRocketPowerUpEffect(NetworkConnection conn, bool enable)
    {
        this._canUseRocketPower = enable;
        if (!enable)
            EnableRocketControls(false);
    }
    public void SetCanUseRocket(bool enable)
    {
        this._canUseRocketPower = enable;
        if (!enable)
            EnableRocketControls(false);
    }
    [ServerRpc]
    void CmdRemoveUsedPowerUps()
    {
        PowerUpManagerTopDownGolf.instance.RemoveUsedPowerupsFromPlayer(this.ObjectId);
    }
    [Server]
    public void ResetPlayersUsedPowerUpEffects()
    {
        if (this.UsedPowerUpType == "rocket")
            RpcRocketPowerUpEffect(this.Owner, false);
        this.UsedPowerUpType = "";
        this.UsedPowerupThisTurn = false;
        this.PowerUpAccuracyModifier = 1.0f;
        this.PowerUpDistanceModifier = 1.0f;
    }
    void SyncPlayerMulligan(bool prev, bool next, bool asServer)
    {
        if (asServer)
            return;
        if (this.IsOwner)
        {
            if (next)
            {
                //StartCoroutine(MulliganPromptCountDown(duration));
                _mulliganRoutine = MulliganPromptCountDown(10);
                StartCoroutine(_mulliganRoutine);
            }
        }
    }
    void UseMulligan()
    {
        if (!this.IsOwner)
            return;
        if (!this.PlayerMulligan)
            return;
        Debug.Log("UseMulligan: for player: " + this.PlayerName);
        if (_isMulliganRoutineRunning)
        {
            StopCoroutine(_mulliganRoutine);
            _isMulliganRoutineRunning = false;
        }
        PlayerUIMessage("UsingMulligan");
        this.MyBall.transform.position = _ballStartPosition;
        SetDistanceToHoleForPlayer();
        CmdUseMulligan();
    }
    [ServerRpc]
    void CmdUseMulligan()
    {
        if (!this.PlayerMulligan)
            return;
        StartCoroutine(DelayForUsedMulligan(5));
        PowerUpManagerTopDownGolf.instance.PlayerIsUsingPowerUp(this.ObjectId);
        this.PlayerScore.RemoveStroke();
        
        if (this.HasPlayerTeedOff && this.PlayerScore.StrokesForCurrentHole == 0)
        {
            Debug.Log("CmdUseMulligan: Player is mulliganing their tee off hit. Reseting HasPlayerTeedOff to false");
            this.HasPlayerTeedOff = false;
        }
    }
    IEnumerator DelayForUsedMulligan(int delay)
    {
        yield return new WaitForSeconds(delay);
        GameplayManagerTopDownGolf.instance.PlayerUsedMulligan(this);
        this.ServerSetIsPlayersTurn(true);
        this.RpcPlayerUIMessage(this.Owner, "");
    }
    void SkipMulligan()
    {
        if (!this.IsOwner)
            return;
        if (!this.PlayerMulligan)
            return;
        if (this.UsedPowerupThisTurn)
            return;
        Debug.Log("SkipMulligan: for player: " + this.PlayerName);
        if (_isMulliganRoutineRunning)
        {
            StopCoroutine(_mulliganRoutine);
            _isMulliganRoutineRunning = false;
        }
        CmdSkipMulligan();
    }
    [ServerRpc]
    void CmdSkipMulligan()
    {
        if (!this.PlayerMulligan)
            return;
        if (this.UsedPowerupThisTurn)
            return;
        Debug.Log("CmdSkipMulligan: for player: " + this.PlayerName);
        this.PlayerMulligan = false;
    }
    [Server]
    public void RemovePlayerMulligan()
    {
        this.PlayerMulligan = false;
    }
    public void SpawnRockFromPowerUp()
    {
        if (!this.UsedPowerupThisTurn)
            return;
        if (this.UsedPowerUpType != "rock")
            return;
        
        CmdSpawnRockFromPowerUp(this._ballStartPosition);
        
    }
    public void SpawnTNTFromPowerUp()
    {
        CmdSpawnTNTFromPowerUp(MyBall.transform.position);
    }
    [ServerRpc]
    void CmdSpawnRockFromPowerUp(Vector3 rockPosition)
    {
        PowerUpManagerTopDownGolf.instance.SpawnRockFromPowerUp(rockPosition);
    }
    [ServerRpc]
    void CmdSpawnTNTFromPowerUp(Vector3 ballPos)
    {
        PowerUpManagerTopDownGolf.instance.SpawnTNTFromPowerUp(ballPos, this);
    }
    void GetPermissionToStartHitFromServer()
    {
        if (!this.IsOwner)
            return;
        CmdCanPlayerHitBall();
    }
    [ServerRpc]
    void CmdCanPlayerHitBall()
    {
        // Check to see if the player will be struck by lightning or not. If they will be, tell player to play the "struck by lightning animation. If not, normal animation
        bool willPlayerBeStruck = GameplayManagerTopDownGolf.instance.WillPlayerBeStruckByLightning(this);
        Debug.Log("CmdCanPlayerHitBall: Will the player be struck by lightning? " + willPlayerBeStruck.ToString());
        if (willPlayerBeStruck)
        {
            //this.RpcTellPlayerTheyWereStruckByLightning(this.Owner);
            this.SetUpLightningStrikeForPlayer();
            return;
        }

        this.RpcTellPlayerToHitBallNormally(this.Owner);
    }
    [Server]
    void SetUpLightningStrikeForPlayer()
    {
        this.RpcTellPlayerTheyWereStruckByLightning(this.Owner);
    }
    [TargetRpc]
    void RpcTellPlayerToHitBallNormally(NetworkConnection conn)
    {
        Debug.Log("RpcTellPlayerToHitBallNormally: " + this.PlayerName);
        _golfAnimator.StartSwing();
    }
    [TargetRpc]
    void RpcTellPlayerTheyWereStruckByLightning(NetworkConnection conn)
    {
        Debug.Log("RpcTellPlayerTheyWereStruckByLightning: " + this.PlayerName);
        // code for the lightning strike animation
        //_golfAnimator.PlayerStruckByLightning();
        this.StruckByLightning();
    }
    void PerfectPowerSubmission()
    {
        if (!this.IsOwner)
            return;
        CmdPerfectPowerSubmission();
        SoundManager.instance.PlaySound("player-perfect-power-submission", 1f);
        _perfectPowerSubmission = true;
    }
    [ServerRpc]
    void CmdPerfectPowerSubmission()
    {
        Debug.Log("CmdPerfectPowerSubmission: Perfect power submission from player: " + this.PlayerName);
        this.FavorWeather = CapWeatherFavor(FavorWeather + 1);
        this.RpcPerfectPowerSubmission();
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcPerfectPowerSubmission()
    {
        SoundManager.instance.PlaySound("player-perfect-power-submission", 1f);
    }
    void PerfectAccuracySubmission()
    {
        if (!this.IsOwner)
            return;
        CmdPerfectAccuracySubmission();
        SoundManager.instance.PlaySound("player-perfect-accuracy-submission", 1f);
        _perfectAccuracySubmission = true;

        if (_perfectPowerSubmission)
            StartCoroutine(BothSubmissionPerfect());
    }
    [ServerRpc]
    void CmdPerfectAccuracySubmission()
    {
        Debug.Log("CmdPerfectAccuracySubmission: Perfect Accuracy submission from player: " + this.PlayerName);
        this.FavorWeather = CapWeatherFavor(FavorWeather + 1);
        this.RpcPerfectAccuracySubmission();
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcPerfectAccuracySubmission()
    {
        SoundManager.instance.PlaySound("player-perfect-accuracy-submission", 1f);
    }
    IEnumerator BothSubmissionPerfect()
    {
        yield return new WaitForSeconds(SoundManager.instance.GetClipLength("player-perfect-accuracy-submission"));
        SoundManager.instance.PlaySound("player-both-perfect-submissions", 1f);
        CmdBothSubmissionPerfect();
    }
    [Server]
    void CmdBothSubmissionPerfect()
    {
        RpcBothSubmissionPerfect();
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcBothSubmissionPerfect()
    {
        SoundManager.instance.PlaySound("player-both-perfect-submissions", 1f);
    }
    void ResetPerfectSubmissions()
    {
        _perfectPowerSubmission = false;
        _perfectAccuracySubmission = false;
    }
    public void BallThroughHoop()
    {
        if (!this.IsOwner)
            return;

        CmdBallThroughHoop();
    }
    [ServerRpc]
    void CmdBallThroughHoop()
    {
        this.PlayerScore.RemoveStroke();
    }
    public bool IsBallInBounds()
    {
        bool isInBounds = true;
        if (_cameraBoundingBox)
        {
            if (!_cameraBoundingBox.OverlapPoint(MyBall.transform.position))
            {
                Debug.Log("IsBallInBounds: Ball is NOT in bounds. Moving the ball for the ball");
                isInBounds = false;
                //MyBall.OutOfBounds();
                //ball.OutOfBounds();
                //await ball.MyPlayer.TellPlayerBallIsOutOfBounds(3);
                //return;
            }
        }
        return isInBounds;
    }
    void PrevOrNextAimPoint(bool next)
    {
        Debug.Log("PrevOrNextAimPoint: Current index: " + this._currentAimPointIndex.ToString() + " next aim point? " + next.ToString() + " how many course aimpoints?: " + GameplayManagerTopDownGolf.instance.CourseAimPoints.Count.ToString());
        if (GameplayManagerTopDownGolf.instance.CourseAimPoints.Count <= 0)
            return;
        if (next)
            this._currentAimPointIndex++;
        else
            this._currentAimPointIndex--;

        if (this._currentAimPointIndex >= GameplayManagerTopDownGolf.instance.CourseAimPoints.Count)
        {
            this._currentAimPointIndex = 0;
        }
        else if (this._currentAimPointIndex < 0)
        {
            this._currentAimPointIndex = GameplayManagerTopDownGolf.instance.CourseAimPoints.Count - 1;
        }
        ChangeAimToNewIndex(this._currentAimPointIndex);
    }
    void ChangeAimToNewIndex(int newIndex)
    {
        Debug.Log("ChangeAimToNewIndex: " + newIndex.ToString());
        if (GameplayManagerTopDownGolf.instance.CourseAimPoints.Count <= 0)
        {
            return;
        }

        if (newIndex >= GameplayManagerTopDownGolf.instance.CourseAimPoints.Count)
        {
            newIndex = GameplayManagerTopDownGolf.instance.CourseAimPoints.Count - 1;
        }
        else if (newIndex < 0)
        {
            newIndex = 0;
        }

        //if (this._currentAimPointIndex != newIndex)
        //    SetCurrentAimPoint(newIndex);
        SetCurrentAimPoint(newIndex);
        Vector3 currentBallPosition = this.MyBall.transform.position;

        // Get direction to Aim Index
        Vector2 directionToAimIndex = (GameplayManagerTopDownGolf.instance.CourseAimPoints[newIndex] - currentBallPosition).normalized;
        float inboundsDistance = GetDistanceToStayInBounds(currentBallPosition, directionToAimIndex, this.hitDistance);

        // Get distance to Aim Index
        float distanceToAimIndex = Vector2.Distance(GameplayManagerTopDownGolf.instance.CourseAimPoints[newIndex], currentBallPosition);
        if (distanceToAimIndex < this.hitDistance)
        {
            this.hitDistance = distanceToAimIndex;
        }
        else if (distanceToAimIndex > this.hitDistance)
        {
            if (distanceToAimIndex > MaxDistanceFromClub)
                this.hitDistance = MaxDistanceFromClub;
            else
                this.hitDistance = distanceToAimIndex;
        }
        //this.hitDistance = inboundsDistance;
        this.hitDirection = directionToAimIndex;
        UpdatePositionOfAdjustedDistanceIcon(hitDistance);
    }
    float GetDistanceToStayInBounds(Vector3 currentPosition, Vector2 direction, float distance)
    {
        Debug.Log("GetDistanceToStayInBounds: currentPosition: " + currentPosition.ToString() + " direction: " + direction.ToString() + " is in bounds! distance: " + distance.ToString());
        if (!_cameraBoundingBox)
            GetCameraBoundingBox();
        Vector3 newAimPosition = currentPosition + (Vector3)(direction * distance);
        if (_cameraBoundingBox.OverlapPoint(newAimPosition))
        {
            Debug.Log("GetDistanceToStayInBounds: New aim position of: " + newAimPosition.ToString() + " is in bounds! returning distance of: " + distance.ToString());
            return distance;
        }
        Vector3 inboundsPos =  GetNearestPointInBounds(currentPosition, newAimPosition, direction);
        float newDistance = Vector2.Distance(currentPosition, inboundsPos);
        Debug.Log("GetDistanceToStayInBounds: New aim position of: " + newAimPosition.ToString() + " is NOT bounds! returning distance of: " + newDistance.ToString());
        return newDistance;

    }
    public void GivePlayerParFavorPenalty(int parScoreDifference)
    {
        if (!this.IsOwner)
            return;
        CmdGivePlayerParFavorPenalty(parScoreDifference);
    }
    [ServerRpc]
    void CmdGivePlayerParFavorPenalty(int parScoreDifference)
    {
        if (!GameplayManagerTopDownGolf.instance.ParFavorPenalty)
            return;

        this.FavorWeather = CapWeatherFavor(this.FavorWeather + parScoreDifference);
    }
    #region Tee Off Challenge
    [Server]
    public async Task ServerTellPlayerHowFarTheyAreFromHoleForChallenege(float duration, float distance)
    {
        _tellPlayerHowFarTheyAreFromHoleForChallenege = true;
        float end = Time.time + (duration * 2);
        RpcTellPlayerHowFarTheyAreFromHoleForChallenege(duration, distance);
        Debug.Log("ServerTellPlayerHowFarTheyAreFromHoleForChallenege: Start time is: " + Time.time.ToString());
        while (_tellPlayerHowFarTheyAreFromHoleForChallenege)
        {
            //Debug.Log("ServerTellPlayerGroundTheyLandedOn: Task.Yield time is: " + Time.time.ToString());
            await Task.Yield();
            if (Time.time >= end)
                _tellPlayerHowFarTheyAreFromHoleForChallenege = false;
        }
        Debug.Log("ServerTellPlayerHowFarTheyAreFromHoleForChallenege: End time is: " + Time.time.ToString());
    }
    [ObserversRpc]
    public void RpcTellPlayerHowFarTheyAreFromHoleForChallenege(float duration, float distance)
    {
        StartTellPlayerHowFarTheyAreFromHoleForChallenege(duration, distance);
    }
    async void StartTellPlayerHowFarTheyAreFromHoleForChallenege(float duration, float distance)
    {
        await TellPlayerHowFarTheyAreFromHoleForChallenege(duration, distance);
    }
    public async Task TellPlayerHowFarTheyAreFromHoleForChallenege(float duration, float distance)
    {
        Debug.Log("TellPlayerHowFarTheyAreFromHoleForChallenege: on game player: " + this.PlayerName);
        if (!this.IsOwner)
        {
            return;
        }

        float end = Time.time + duration;
        if (MyBall.IsInHole || MyBall.LocalIsInHole)
        {
            Debug.Log("TellPlayerHowFarTheyAreFromHoleForChallenege: on game player: " + this.PlayerName + " ball is in hole!");
            this.PlayerUIMessage("challenege:hole");
            this.EnablePlayerCanvas(true);
            end += 1f;
        }
        else
        {
            this.PlayerUIMessage("challenege:" + distance.ToString("0.00"));
            this.EnablePlayerCanvas(true);
        }
        while (Time.time < end)
        {
            await Task.Yield();
        }

        this.EnablePlayerCanvas(false);
        if (this.IsOwner)
            CmdTellPlayerHowFarTheyAreFromHoleForChallenegeCompleted();
    }
    [ServerRpc]
    void CmdTellPlayerHowFarTheyAreFromHoleForChallenegeCompleted()
    {
        _tellPlayerHowFarTheyAreFromHoleForChallenege = false;
        RpcTellPlayerHowFarTheyAreFromHoleForChallenegeCompleted();
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcTellPlayerHowFarTheyAreFromHoleForChallenegeCompleted()
    {
        this.EnablePlayerCanvas(false);
    }
    [Server]
    public void RewardPlayerForWinningChallenege()
    {
        this.FavorWeather = CapWeatherFavor(1);
    }
    #endregion
    #region Server Messages To Players
    [Server]
    public async Task ServerSendMessagetoPlayer(float duration, string messageType, CancellationToken token, float distance = 0f)
    {
        _messageSentToPlayer = true;
        float end = Time.time + (duration * 2);

        RpcSelectMessageToSend(this.Owner, duration, messageType, distance);

        Debug.Log("ServerTellPlayerHowFarTheyAreFromHoleForChallenege: Start time is: " + Time.time.ToString());
        while (_messageSentToPlayer)
        {
            //Debug.Log("ServerTellPlayerGroundTheyLandedOn: Task.Yield time is: " + Time.time.ToString());

            // check if the cancellation token was cancelled
            if (token.IsCancellationRequested)
                return;
            await Task.Yield();
            if (Time.time >= end)
                _messageSentToPlayer = false;
        }
        Debug.Log("ServerSendMessagetoPlayer: End time is: " + Time.time.ToString());
    }
    [TargetRpc]
    void RpcSelectMessageToSend(NetworkConnection conn, float duration, string messageType, float distance = 0f)
    {
        StartTellPlayerMessageFromServer(duration, messageType, distance);
    }
    async void StartTellPlayerMessageFromServer(float duration, string messageType, float distance = 0f)
    {
        float end = Time.time + duration;

        // select message function to call based on message type
        switch (messageType)
        {
            default:
            case "PlayerClosest":
                TellPlayerWhoWasClosestOnChallenege();
                break;
            case "DistanceFromHoleOnChallenege":
                TellPlayerHowFarTheyWereFromHole(distance);
                break;
            case "stroke limit":
                TellPlayerTheyHitTheStrokeLimit();
                break;
        }

        while (Time.time < end)
        {
            await Task.Yield();
        }
        this.EnablePlayerCanvas(false);
        CmdServerSendMessagetoPlayerCompleted();
    }
    void TellPlayerWhoWasClosestOnChallenege()
    {
        this.PlayerUIMessage("PlayerClosest");
        this.EnablePlayerCanvas(true);
    }
    void TellPlayerHowFarTheyWereFromHole(float distance)
    {
        if (MyBall.IsInHole || MyBall.LocalIsInHole)
        {
            Debug.Log("TellPlayerHowFarTheyWereFromHole: on game player: " + this.PlayerName + " ball is in hole!");
            this.PlayerUIMessage("challenege:hole");
            this.EnablePlayerCanvas(true);
        }
        else
        {
            this.PlayerUIMessage("challenege:" + distance.ToString("0.00"));
            this.EnablePlayerCanvas(true);
        }
    }
    void TellPlayerTheyHitTheStrokeLimit()
    {
        this.PlayerUIMessage("stroke limit");
        this.EnablePlayerCanvas(true);
    }
    [ServerRpc]
    void CmdServerSendMessagetoPlayerCompleted()
    {
        _messageSentToPlayer = false;
        RpcServerSendMessagetoPlayerCompleted();
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcServerSendMessagetoPlayerCompleted()
    {
        this.EnablePlayerCanvas(false);
    }
    #endregion
    #region Ball In Tube
    public void LaunchBallOutOfTube(bool enteredPrimaryTube)
    {
        Vector3 ballPos = MyBall.transform.position;
        GameObject nearestHole = FindClosestHole(ballPos);
        Vector3 holePos = nearestHole.transform.position;
        Vector2 dir = (holePos - ballPos).normalized;
        float dist = 7.5f;

        if (enteredPrimaryTube)
        {
            dist = Vector2.Distance(holePos, ballPos) * 0.5f;
        }
        else
        {
            dir = -dir;
        }
        
        MyBall.LaunchBallOutOfTube(dist, dir);
    }
    #endregion
    #region Aim Point Stuff
    void SetCurrentAimPoint(int aimPointIndex)
    {
        this._currentAimPointIndex = aimPointIndex;
        // code to update the UI for which aimpoint is selected?
        if (this.IsOwner)
        {
            GameplayManagerTopDownGolf.instance.UpdateCurrentAimPointUI(aimPointIndex);
            this.CmdSetCurrentAimPoint(aimPointIndex);
        }
    }
    [ServerRpc]
    void CmdSetCurrentAimPoint(int aimPointIndex)
    {
        this.RpcSetCurrentAimPoint(aimPointIndex);
    }
    [ObserversRpc(ExcludeOwner = true)]
    void RpcSetCurrentAimPoint(int aimPointIndex)
    {
        GameplayManagerTopDownGolf.instance.UpdateCurrentAimPointUI(aimPointIndex);
    }
    #endregion
    #region Controls / InputManager stuff
    void SubscribeToControls()
    {
        Debug.Log("SubscribeToControls: For: " + this.PlayerName);
        // Prompt Player Controls
        //InputManagerGolf.Controls.PromptPlayer.Continue.performed += _ => PromptPlayerContinue();
        //InputManagerGolf.Controls.PromptPlayer.Skip.performed += _ => PromptPlayerSkip();
        EnableViewScoreBoard(true);
        EnableZoomOut(true);
        EnableEscapeMenu(true);
    }
    void UnsubscribeToControls()
    {
        EnableViewScoreBoard(false);
        EnableZoomOut(false);
        EnableEscapeMenu(false);
    }
    #region Start Player Turn
    [TargetRpc]
    public void RpcEnablePromptPlayerToStartTurn(NetworkConnection conn, bool enable)
    {
        Debug.Log("RpcEnablePromptPlayerToStartTurn: player: " + this.PlayerName + ":" + enable.ToString());

        EnablePromptPlayerToStartTurn(enable);
        if (_cameraViewHole.IsCameraZoomedOut)
        {
            _cameraViewHole.ZoomOutCamera();
            EnableZoomOutAim(_cameraViewHole.IsCameraZoomedOut);
        }
    }
    void EnablePromptPlayerToStartTurn(bool enable)
    {
        if (!this.IsOwner)
            return;
        Debug.Log("EnablePromptPlayerToStartTurn: player: " + this.PlayerName + ":" + enable.ToString());
        if (enable)
        {
            InputManagerGolf.Controls.PromptPlayer.Continue.Enable();
            InputManagerGolf.Controls.PromptPlayer.Continue.performed += PlayerStartTurn;
        }
        else
        {
            InputManagerGolf.Controls.PromptPlayer.Continue.Disable();
            InputManagerGolf.Controls.PromptPlayer.Continue.performed -= PlayerStartTurn;
        }
    }
    void PlayerStartTurn(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (GameplayManagerTopDownGolf.instance.CurrentPlayer != this)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
            
        Debug.Log("PlayerStartTurn: Player: " + this.PlayerName + " will start their turn after pressing space! Time: " + Time.time);
        this.EnablePlayerCanvas(false);
        // Begin old way of starting turn
        //GameplayManagerTopDownGolf.instance.StartCurrentPlayersTurn(this);
        // End old way of starting turn
        CmdStartCurrentPlayersTurnOnServer();

        // disable prompt controls?
        EnablePromptPlayerToStartTurn(false);
        EnablePromptPlayerSkipForLightning(false);
    }
    void EnableAcknowledgePlayerLightningStrike(bool enable)
    {
        if (!this.IsOwner)
            return;

        Debug.Log("EnableAcknowledgePlayerLightningStrike: player: " + this.PlayerName + ":" + enable.ToString());
        if (enable)
        {
            InputManagerGolf.Controls.PromptPlayer.Continue.Enable();
            InputManagerGolf.Controls.PromptPlayer.Continue.performed += AcknowledgePlayerLightningStrike;
        }
        else 
        {
            InputManagerGolf.Controls.PromptPlayer.Continue.Disable();
            InputManagerGolf.Controls.PromptPlayer.Continue.performed -= AcknowledgePlayerLightningStrike;
        }
    }
    void AcknowledgePlayerLightningStrike(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (!this.PlayerStruckByLightning)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        Debug.Log("AcknowledgePlayerLightningStrike: PlayerStruckByLightning: " + this.PlayerStruckByLightning.ToString());
        _golfAnimator.ResetGolfAnimator();
        EnablePlayerSprite(false);
        CmdDisableSpriteForLightningStrike();
        this.EnablePlayerCanvas(false);
        CmdEnablePlayerCanvasForOtherClients(false);
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
        Debug.Log("GolfPlayerTopDown: AcknowledgePlayerLightningStrike: Player: " + this.PlayerName + " has acknowledged they were struck by lightning! Moving on to next turn by calling: GameplayManagerTopDownGolf.instance.PlayerWasStruckByLightning(this). Time: " + Time.time);
        //GameplayManagerTopDownGolf.instance.PlayerWasStruckByLightning(this);
        CmdTellServerPlayerWasStruckByLightning();
        EnableAcknowledgePlayerLightningStrike(false);
        return;
    }
    #endregion
    #region Skip For Lightning
    [TargetRpc]
    public void RpcEnablePromptPlayerSkipForLightning(NetworkConnection conn, bool enable)
    {
        Debug.Log("RpcEnablePromptPlayerSkipForLightning: player: " + this.PlayerName + ":" + enable.ToString());
        EnablePromptPlayerSkipForLightning(enable);
    }
    void EnablePromptPlayerSkipForLightning(bool enable)
    {
        if (!this.IsOwner)
            return;
        Debug.Log("EnablePromptPlayerSkipForLightning: player: " + this.PlayerName + ":" + enable.ToString());
        if (enable)
        {
            InputManagerGolf.Controls.PromptPlayer.Skip.Enable();
            InputManagerGolf.Controls.PromptPlayer.Skip.performed += PlayerSkipForLightning;
        }
        else
        {
            InputManagerGolf.Controls.PromptPlayer.Skip.Disable();
            InputManagerGolf.Controls.PromptPlayer.Skip.performed -= PlayerSkipForLightning;
        }
    }
    void PlayerSkipForLightning(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (GameplayManagerTopDownGolf.instance.CurrentPlayer != this)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        Debug.Log("PlayerSkipForLightning: Player: " + this.PlayerName + " is skipping their turn due to lightning. At time of: " + Time.time.ToString() + " and last skip was: " + GameplayManagerTopDownGolf.instance.TimeSinceLastSkip.ToString());
        this.EnablePlayerCanvas(false);
        PlayerScore.StrokePenalty(1);
        //GameplayManagerTopDownGolf.instance.StartNextPlayersTurn(MyBall, true);
        MyBall.CmdTellServerToStartNexPlayersTurn(true);

        // disable prompt controls?
        EnablePromptPlayerToStartTurn(false);
        EnablePromptPlayerSkipForLightning(false);

    }
    #endregion
    #region Aiming Actions
    void EnableAimingActions(bool enable)
    {
        if (!this.IsOwner)
            return;
        Debug.Log("EnableAimingActions: player: " + this.PlayerName + ":" + enable.ToString());
        _aimingActionsEnabled = enable;
        if (enable)
        {
            // disable UI controls?
            //if(!GolfEscMenuManager.instance.IsMenuOpen)
            //    InputManagerGolf.Controls.UI.Disable();

            //InputManagerGolf.Controls.AimingActions.Enable();
            //AimPosition
            InputManagerGolf.Controls.AimingActions.AimPosition.Enable();
            InputManagerGolf.Controls.AimingActions.AimPosition.performed += ChangeAimPosition;
            InputManagerGolf.Controls.AimingActions.AimPosition.canceled += ResetAimMovement;
            //SpinPosition
            InputManagerGolf.Controls.AimingActions.SpinPosition.Enable();
            InputManagerGolf.Controls.AimingActions.SpinPosition.performed += ChangeSpinMovement;
            InputManagerGolf.Controls.AimingActions.SpinPosition.canceled += ResetSpinMovement;
            //ResetSpin
            InputManagerGolf.Controls.AimingActions.ResetSpin.Enable();
            InputManagerGolf.Controls.AimingActions.ResetSpin.performed += ResetSpinPressed;
            //ChangeClub
            InputManagerGolf.Controls.AimingActions.ChangeClub.Enable();
            InputManagerGolf.Controls.AimingActions.ChangeClub.performed += ChangeClubButtonPressed;
            //ZoomOut
            //InputManagerGolf.Controls.AimingActions.ZoomOut.Enable();
            //InputManagerGolf.Controls.AimingActions.ZoomOut.performed += ZoomOutPressed;
            //ZoomOutAim ? maybe only enable/disable that when toggling between the zoom outs?
            //ShortPutt
            InputManagerGolf.Controls.AimingActions.ShortPutt.Enable();
            InputManagerGolf.Controls.AimingActions.ShortPutt.performed += ShortPuttButtonPressed;
            //SubmitAim
            InputManagerGolf.Controls.AimingActions.SubmitAim.Enable();
            InputManagerGolf.Controls.AimingActions.SubmitAim.performed += SubmitAim;

            // Aim Point controls
            InputManagerGolf.Controls.AimingActions.PreviousAimPoint.Enable();
            InputManagerGolf.Controls.AimingActions.PreviousAimPoint.performed += PreviousAimPointPressed;
            InputManagerGolf.Controls.AimingActions.NextAimPoint.Enable();
            InputManagerGolf.Controls.AimingActions.NextAimPoint.performed += NextAimPointPressed;
            // aim point shortcuts
            InputManagerGolf.Controls.AimingActions.AimPoint1.Enable();
            InputManagerGolf.Controls.AimingActions.AimPoint1.performed += AimPointShortCutPressed;
            InputManagerGolf.Controls.AimingActions.AimPoint2.Enable();
            InputManagerGolf.Controls.AimingActions.AimPoint2.performed += AimPointShortCutPressed;
            InputManagerGolf.Controls.AimingActions.AimPoint3.Enable();
            InputManagerGolf.Controls.AimingActions.AimPoint3.performed += AimPointShortCutPressed;
            InputManagerGolf.Controls.AimingActions.AimPoint4.Enable();
            InputManagerGolf.Controls.AimingActions.AimPoint4.performed += AimPointShortCutPressed;
            InputManagerGolf.Controls.AimingActions.AimPoint5.Enable();
            InputManagerGolf.Controls.AimingActions.AimPoint5.performed += AimPointShortCutPressed;
        }
        else
        {
            //InputManagerGolf.Controls.AimingActions.Disable();
            //AimPosition
            InputManagerGolf.Controls.AimingActions.AimPosition.Disable();
            InputManagerGolf.Controls.AimingActions.AimPosition.performed -= ChangeAimPosition;
            InputManagerGolf.Controls.AimingActions.AimPosition.canceled -= ResetAimMovement;
            _previousAimMovement = Vector2.zero;
            //SpinPosition
            InputManagerGolf.Controls.AimingActions.SpinPosition.Disable();
            InputManagerGolf.Controls.AimingActions.SpinPosition.performed -= ChangeSpinMovement;
            InputManagerGolf.Controls.AimingActions.SpinPosition.canceled -= ResetSpinMovement;
            _spinIcon.ResetSpinDirection(); // make sure the sping icon doesn't keep moving?
            //ResetSpin
            InputManagerGolf.Controls.AimingActions.ResetSpin.Disable();
            InputManagerGolf.Controls.AimingActions.ResetSpin.performed -= ResetSpinPressed;
            //ChangeClub
            InputManagerGolf.Controls.AimingActions.ChangeClub.Disable();
            InputManagerGolf.Controls.AimingActions.ChangeClub.performed -= ChangeClubButtonPressed;
            //ZoomOut
            //InputManagerGolf.Controls.AimingActions.ZoomOut.Disable();
            //InputManagerGolf.Controls.AimingActions.ZoomOut.performed -= ZoomOutPressed;
            //ZoomOutAim ? maybe only enable/disable that when toggling between the zoom outs?
            //ShortPutt
            InputManagerGolf.Controls.AimingActions.ShortPutt.Disable();
            InputManagerGolf.Controls.AimingActions.ShortPutt.performed -= ShortPuttButtonPressed;
            //SubmitAim
            InputManagerGolf.Controls.AimingActions.SubmitAim.Disable();
            InputManagerGolf.Controls.AimingActions.SubmitAim.performed -= SubmitAim;
            // Aim Point controls
            InputManagerGolf.Controls.AimingActions.PreviousAimPoint.Disable();
            InputManagerGolf.Controls.AimingActions.PreviousAimPoint.performed -= PreviousAimPointPressed;
            InputManagerGolf.Controls.AimingActions.NextAimPoint.Disable();
            InputManagerGolf.Controls.AimingActions.NextAimPoint.performed -= NextAimPointPressed;
            // aim point shortcuts
            InputManagerGolf.Controls.AimingActions.AimPoint1.Disable();
            InputManagerGolf.Controls.AimingActions.AimPoint1.performed -= AimPointShortCutPressed;
            InputManagerGolf.Controls.AimingActions.AimPoint2.Disable();
            InputManagerGolf.Controls.AimingActions.AimPoint2.performed -= AimPointShortCutPressed;
            InputManagerGolf.Controls.AimingActions.AimPoint3.Disable();
            InputManagerGolf.Controls.AimingActions.AimPoint3.performed -= AimPointShortCutPressed;
            InputManagerGolf.Controls.AimingActions.AimPoint4.Disable();
            InputManagerGolf.Controls.AimingActions.AimPoint4.performed -= AimPointShortCutPressed;
            InputManagerGolf.Controls.AimingActions.AimPoint5.Disable();
            InputManagerGolf.Controls.AimingActions.AimPoint5.performed -= AimPointShortCutPressed;
        }
    }
    void ChangeAimPosition(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (GameplayManagerTopDownGolf.instance.CurrentPlayer != this)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("ChangeAimPosition: EscMenuOpen. Do not take action!");
            return;
        }
        Debug.Log("ChangeAimPosition: ");
        _previousAimMovement = context.ReadValue<Vector2>();
        
        //Debug.Log("ChangeAimPosition: Player: " + this.PlayerName + " and move aim in direction of: " + _previousAimMovement.ToString());

    }
    void ResetAimMovement(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        _previousAimMovement = Vector2.zero;
    }
    void SubmitAim(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        Debug.Log("Player pressed space: DirectionAndDistanceChosen: " + DirectionAndDistanceChosen .ToString() + " && _moveHitMeterIcon: " + _moveHitMeterIcon.ToString());
        if (DirectionAndDistanceChosen)
            return;
        if (_moveHitMeterIcon)
            return;
        
        PlayerChooseDirectionAndDistance(true);
        //EnableAimPositionControls(false);
        // disable aiming controls
        EnableAimingActions(false);

        if (_cameraViewHole.IsCameraZoomedOut)
        {
            _cameraViewHole.ZoomOutCamera();
            EnableZoomOutAim(_cameraViewHole.IsCameraZoomedOut);
        }
        UpdateCameraFollowTarget(MyBall.MyBallObject);
        CmdSetCameraOnMyBallForOtherPlayers();
        GameplayManagerTopDownGolf.instance.PowerUpHideInstructions();

        // enable the CancelHit and StartHit controls
        EnableCancelHit(true);
        EnableStartHit(true);
        EnableUsePowerUp(false);

    }
    void ChangeClubButtonPressed(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        if (DirectionAndDistanceChosen)
            return;
        if (_moveHitMeterIcon)
            return;
        Debug.Log("ChangeClubButtonPressed: ");

        ChangeCurrentClub();
    }
    void ZoomOutPressed(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        Debug.Log("ZoomOutPressed: ");

        if (DirectionAndDistanceChosen && this.IsPlayersTurn)
            return;

        _cameraViewHole.ZoomOutCamera();
        EnableZoomOutAim(_cameraViewHole.IsCameraZoomedOut);
    }
    public void ResetEnableZoomOutAim(bool enable)
    {
        EnableZoomOutAim(enable);
    }
    void EnableZoomOutAim(bool enable)
    {
        if (!this.IsOwner)
            return;
        Debug.Log("EnableZoomOutAim: " + enable.ToString());
        if (enable)
        {
            InputManagerGolf.Controls.AimingActions.ZoomOutAim.Enable();
            InputManagerGolf.Controls.AimingActions.ZoomOutAim.performed += ZoomOutAim;
            this._zoomOutTurnModifier = 5f;
        }
        else
        {
            InputManagerGolf.Controls.AimingActions.ZoomOutAim.Disable();
            InputManagerGolf.Controls.AimingActions.ZoomOutAim.performed -= ZoomOutAim;
            this._zoomOutTurnModifier = 1f;
        }
    }
    void ZoomOutAim(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (!_cameraViewHole.IsCameraZoomedOut)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        Debug.Log("ZoomOutAim: ");

        Vector3 mouseScreenPos = Input.mousePosition;
        //Debug.Log("Player clicked on screen position: " + mouseScreenPos.ToString());
        if (mouseScreenPos.x > 0 && mouseScreenPos.y > 0 && mouseScreenPos.y <= Screen.height && mouseScreenPos.x <= Screen.width)
        {
            //Debug.Log("Player clicked on screen position INSIDE of window");
            Vector3 mouseWorldPos = myCamera.ScreenToWorldPoint(mouseScreenPos);
            Vector2 mouseDir = (mouseWorldPos - MyBall.transform.position).normalized;
            //Debug.Log("Player clicked on screen/world position: " + mouseScreenPos.ToString() + "/" + mouseWorldPos.ToString() + ". Direction to mouse: " + mouseDir.ToString() + " compared to player's current aim position: " + hitDirection.ToString());
            if (this.IsOwner)
                ChangeDirectionFromMouseClick(mouseDir);
            //hitDirection = mouseDir;
        }
        else
            Debug.Log("Player clicked on screen position OUTSIDE of window");
    }
    void ResetSpinPressed(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (DirectionAndDistanceChosen)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        Debug.Log("ResetSpinPressed: ");
        _spinIcon.ResetIconPosition();
    }
    void ChangeSpinMovement(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (DirectionAndDistanceChosen)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        //Debug.Log("ChangeSpinMovement: ");
        //Debug.Log("ChangeSpinMovement: Player: " + this.PlayerName + " and move aim in direction of: " + context.ReadValue<Vector2>().ToString());
        _spinIcon.UpdateSpinDirection(context.ReadValue<Vector2>());
    }
    void ResetSpinMovement(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        //Debug.Log("ResetSpinMovement: ");
        _spinIcon.ResetSpinDirection();
    }
    void ShortPuttButtonPressed(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (DirectionAndDistanceChosen)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        Debug.Log("ShortPuttButtonPressed: ");

        SetPutterDistance(CurrentClub, true);
    }
    void PreviousAimPointPressed(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (DirectionAndDistanceChosen)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        Debug.Log("PreviousAimPointPressed: ");
        PrevOrNextAimPoint(false);
    }
    void NextAimPointPressed(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (DirectionAndDistanceChosen)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        Debug.Log("NextAimPointPressed: ");
        PrevOrNextAimPoint(true);
    }
    void AimPointShortCutPressed(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (DirectionAndDistanceChosen)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        Debug.Log("AimPointShortCutPressed: " + context.action.name);
        int indexValue = Int32.Parse(context.action.name[context.action.name.Length - 1].ToString());
        indexValue--;
        //PrevOrNextAimPoint(true);
        ChangeAimToNewIndex(indexValue);
    }
    #endregion
    #region Hitting Actions
    void EnableCancelHit(bool enable)
    {
        if (!this.IsOwner)
            return;

        Debug.Log("EnableCancelHit: " + enable.ToString());
        if (enable)
        {
            InputManagerGolf.Controls.HittingActions.CancelHit.Enable();
            InputManagerGolf.Controls.HittingActions.CancelHit.performed += CancelHit;
        }
        else
        {
            InputManagerGolf.Controls.HittingActions.CancelHit.Disable();
            InputManagerGolf.Controls.HittingActions.CancelHit.performed -= CancelHit;
        }
    }
    void CancelHit(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        Debug.Log("CancelHit: ");
        if (_moveHitMeterIcon)
            return;
        if (!DirectionAndDistanceChosen)
            return;

        PlayerChooseDirectionAndDistance(false);
        //EnableAimPositionControls(true);
        EnableAimingActions(true);
        EnableRocketControls(false);
        UpdateCameraFollowTarget(_landingTargetSprite.gameObject);
        GameplayManagerTopDownGolf.instance.PowerUpShowInstructions(this);
        
        // Disable CancelHit and ESPECIALLY StartHit controls
        EnableCancelHit(false);
        EnableStartHit(false);

        EnableAimingActions(true);
        CanPlayerUsePowerUp();
    }
    void EnableStartHit(bool enable)
    {
        if (!this.IsOwner)
            return;
        Debug.Log("EnableStartHit: " + enable.ToString());
        if (enable)
        {
            InputManagerGolf.Controls.HittingActions.StartHit.Enable();
            InputManagerGolf.Controls.HittingActions.StartHit.performed += StartHitButtonPerformed;
        }
        else
        {
            InputManagerGolf.Controls.HittingActions.StartHit.Disable();
            InputManagerGolf.Controls.HittingActions.StartHit.performed -= StartHitButtonPerformed;
        }
    }
    void StartHitButtonPerformed(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        Debug.Log("StartHitButtonPerformed: ");
        //DirectionAndDistanceChosen && !_moveHitMeterIcon && !_golfAnimator.IsSwinging
        if (!DirectionAndDistanceChosen)
            return;
        if (_moveHitMeterIcon)
            return;
        if (_golfAnimator.IsSwinging)
            return;

        //Disable CancelHit and StartHit controls
        EnableStartHit(false);
        EnableCancelHit(false);


        StartHitMeterSequence();
        EnableSubmitHit(true);
    }
    void EnableSubmitHit(bool enable)
    {
        if (!this.IsOwner)
            return;
        Debug.Log("EnableSubmitHit: " + enable.ToString());
        if (enable)
        {
            InputManagerGolf.Controls.HittingActions.SubmitHit.Enable();
            InputManagerGolf.Controls.HittingActions.SubmitHit.performed += SubmitHitPower;
        }
        else
        {
            InputManagerGolf.Controls.HittingActions.SubmitHit.Disable();
            InputManagerGolf.Controls.HittingActions.SubmitHit.performed -= SubmitHitPower;
            InputManagerGolf.Controls.HittingActions.SubmitHit.performed -= SubmitHitAccuracy;
        }
    }
    void SubmitHitPower(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        if (!_moveHitMeterIcon)
            return;
        if (_powerSubmitted)
            return;
        if (_accuracySubmitted)
            return;

        Debug.Log("SubmitHitPower: ");
        SetHitPowerValue();
        InputManagerGolf.Controls.HittingActions.SubmitHit.performed -= SubmitHitPower;
        InputManagerGolf.Controls.HittingActions.SubmitHit.performed += SubmitHitAccuracy;
    }
    void SubmitHitAccuracy(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        if (!_moveHitMeterIcon)
            return;
        if (!_powerSubmitted)
            return;
        if (_accuracySubmitted)
            return;

        Debug.Log("SubmitHitAccuracy: ");
        SetHitAccuracyValue();
        GetPermissionToStartHitFromServer();
        EnableSubmitHit(false);
    }
    #endregion
    #region Power Ups
    void CanPlayerUsePowerUp()
    {
        if (!this.IsOwner)
            return;
        if (this.HasPowerUp && !this.UsedPowerupThisTurn && this.PlayerPowerUpType != "mulligan")
            EnableUsePowerUp(true);
        else
            EnableUsePowerUp(false);
    }
    void EnableUsePowerUp(bool enable)
    {
        if (!this.IsOwner)
            return;
        Debug.Log("EnableUsePower: player: " + this.PlayerName + ":" + enable.ToString());
        if (enable)
        {
            InputManagerGolf.Controls.PowerUps.UsePowerUp.Enable();
            InputManagerGolf.Controls.PowerUps.UsePowerUp.performed += UsePowerUpButtonPressed;
        }
        else
        {
            InputManagerGolf.Controls.PowerUps.UsePowerUp.Disable();
            InputManagerGolf.Controls.PowerUps.UsePowerUp.performed -= UsePowerUpButtonPressed;
        }
    }
    void UsePowerUpButtonPressed(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        if (!this.HasPowerUp)
            return;
        if (this.UsedPowerupThisTurn)
            return;

        if (this.PlayerMulligan)
        {
            Debug.Log("UsePowerUpButtonPressed: player mulligan? " + this.PlayerMulligan.ToString());
            UseMulligan();
            EnableUsePowerUp(false);
        }
        else
        {
            if (!this.IsPlayersTurn)
                return;
            if (DirectionAndDistanceChosen)
                return;
            if ((MyBall.isHit || MyBall.isBouncing || MyBall.isRolling))
                return;

            Debug.Log("UsePowerUpButtonPressed: player mulligan? " + this.PlayerMulligan.ToString());

            this.UsePowerUp();
            EnableUsePowerUp(false);
        }
        
    }
    void EnableSkipMulligan(bool enable)
    {
        if (!this.IsOwner)
            return;
        Debug.Log("EnableSkipMulligan: " + enable.ToString());
        if (enable)
        {
            InputManagerGolf.Controls.PromptPlayer.Continue.Enable();
            InputManagerGolf.Controls.PromptPlayer.Continue.performed += SkipMulliganPressed;
        }
        else
        {
            InputManagerGolf.Controls.PromptPlayer.Continue.Disable();
            InputManagerGolf.Controls.PromptPlayer.Continue.performed -= SkipMulliganPressed;
        }
    }
    void SkipMulliganPressed(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        if (!this.HasPowerUp)
            return;
        if (this.UsedPowerupThisTurn)
            return;
        if (this.PlayerPowerUpType != "mulligan")
            return;


        Debug.Log("SkipMulliganPressed: ");
        this.SkipMulligan();
        EnableSkipMulligan(false);
    }
    #endregion
    #region Misc Actions
    void EnableViewScoreBoard(bool enable)
    {
        if (!this.IsOwner)
            return;
        Debug.Log("EnableViewScores: " + enable.ToString());
        if (enable)
        {
            InputManagerGolf.Controls.Misc.ViewScoreBoard.Enable();
            InputManagerGolf.Controls.Misc.ViewScoreBoard.performed += OpenScoreBoard;
            InputManagerGolf.Controls.Misc.ViewScoreBoard.canceled += CloseScoreBoard;
        }
        else
        {
            InputManagerGolf.Controls.Misc.ViewScoreBoard.Disable();
            InputManagerGolf.Controls.Misc.ViewScoreBoard.performed -= OpenScoreBoard;
            InputManagerGolf.Controls.Misc.ViewScoreBoard.canceled -= CloseScoreBoard;
        }
    }
    void OpenScoreBoard(InputAction.CallbackContext context)
    {
        PlayerScoreBoard.instance.OpenScoreBoard();
    }
    void CloseScoreBoard(InputAction.CallbackContext context)
    {
        PlayerScoreBoard.instance.CloseScoreBoard();
    }
    void EnableZoomOut(bool enable)
    {
        if (!this.IsOwner)
            return;
        Debug.Log("EnableZoomOut: " + enable.ToString());
        if (enable)
        {
            InputManagerGolf.Controls.AimingActions.ZoomOut.Enable();
            InputManagerGolf.Controls.AimingActions.ZoomOut.performed += ZoomOutPressed;
        }
        else
        {
            InputManagerGolf.Controls.AimingActions.ZoomOut.Disable();
            InputManagerGolf.Controls.AimingActions.ZoomOut.performed -= ZoomOutPressed;
        }
    }
    void EnableEscapeMenu(bool enable)
    {
        if (!this.IsOwner)
            return;
        Debug.Log("EnableEscapeMenu: " + enable.ToString());
        if (enable)
        {
            InputManagerGolf.Controls.Misc.EscMenu.Enable();
            InputManagerGolf.Controls.Misc.EscMenu.performed += OpenEscMenu;
;
        }
        else
        {
            InputManagerGolf.Controls.Misc.EscMenu.Disable();
            InputManagerGolf.Controls.Misc.EscMenu.performed -= OpenEscMenu;
        }
    }
    void OpenEscMenu(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        Debug.Log("OpenEscMenu: ");
        GolfEscMenuManager.instance.OpenOrCloseEscMenu();
    }
    #endregion
    #region Rocket Controls
    void EnableRocketControls(bool enable)
    {
        if (!this.IsOwner)
            return;
        Debug.Log("EnableRocketControls: " + enable.ToString());
        if (enable)
        {
            InputManagerGolf.Controls.AimingActions.AimPosition.Enable();
            InputManagerGolf.Controls.AimingActions.AimPosition.performed += ChangeRocketMove;
            InputManagerGolf.Controls.AimingActions.AimPosition.canceled += ResetRocketMove;
        }
        else
        {
            if(!_aimingActionsEnabled)
                InputManagerGolf.Controls.AimingActions.AimPosition.Disable();
            InputManagerGolf.Controls.AimingActions.AimPosition.performed -= ChangeRocketMove;
            InputManagerGolf.Controls.AimingActions.AimPosition.canceled -= ResetRocketMove;
            _rocketMove = Vector2.zero;
        }
    }
    void ChangeRocketMove(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        if (GolfEscMenuManager.instance.IsMenuOpen)
        {
            Debug.Log("EscMenuOpen. Do not take action!");
            return;
        }
        if (!this.UsedPowerupThisTurn || this.UsedPowerUpType != "rocket")
        {
            return;
        }   
        if (!this._canUseRocketPower)
        {
            return;
        }

        _rocketMove = context.ReadValue<Vector2>();

        //Debug.Log("ChangeAimPosition: Player: " + this.PlayerName + " and move aim in direction of: " + _previousAimMovement.ToString());

    }
    void ResetRocketMove(InputAction.CallbackContext context)
    {
        if (!this.IsOwner)
            return;
        _rocketMove = Vector2.zero;
    }
    #endregion
    #endregion
}