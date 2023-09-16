using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using System.Globalization;
using System.Threading.Tasks;
using FishNet.Object.Synchronizing;
using FishNet.Object;
using FishNet.Connection;
using FishNet;
using UnityEngine.UI;
using System.Threading;

public class GameplayManagerTopDownGolf : NetworkBehaviour
{
    public static GameplayManagerTopDownGolf instance;

    [Header("Course Information")]
    [SerializeField] public ScriptableCourse CurrentCourse;
    [SerializeField] public ScriptableHole CurrentHoleInCourse;
    [SerializeField] [SyncVar] public int CurrentHoleIndex;
    [SerializeField] public ScriptableCourse CourseToPlay;

    [Header("TeeOffChallenge Info:")]
    [SerializeField] public ScriptableCourse TeeOffChallenges;
    [SerializeField] public ScriptableHole SelectedChallenge;
    [SerializeField] List<GolfPlayerTopDown> _playerOrderFromChallenege = new List<GolfPlayerTopDown>();
    [SerializeField] [SyncVar] public string TeeOffChallengeClubType;

    [Header("Current Hole Information")]
    public Vector3 TeeOffPosition;
    public string CurrentHoleName;
    public int CurrentHolePar;
    public List<Vector3> HolePositions;
    public List<GameObject> HoleHoleObjects = new List<GameObject>();
    public Vector3 TeeOffAimPoint;
    [SerializeField] float _lastNewHoleTime = 0f;
    public int CurrentTurnNumber = 0;
    public int LastTurnWithWeatherChange = 0;


    [Header("Tilemap Manager References")]
    [SerializeField] TileMapManager _tileMapManager;

    [Header("Local Player")]
    [SerializeField] public GolfPlayerTopDown LocalGolfPlayer;

    [Header("Player Info")]
    [SerializeField] [SyncObject] public readonly SyncList<NetworkPlayer> NetworkPlayersServer = new SyncList<NetworkPlayer>();
    [SerializeField] public List<GolfPlayerTopDown> GolfPlayers = new List<GolfPlayerTopDown>();
    [SerializeField] [SyncObject] public readonly SyncList<GolfPlayerTopDown> GolfPlayersServer = new SyncList<GolfPlayerTopDown>();
    public List<GolfPlayerTopDown> GolfPlayersInTeeOffOrder = new List<GolfPlayerTopDown>();
    public List<GolfPlayerTopDown> GolfPlayersOutOfCommission = new List<GolfPlayerTopDown>();
    [SerializeField] int _numberOfPlayersTeedOff = 0;
    [SerializeField] int _teeOffOrder = 0;
    [SerializeField] int _numberOfPlayersInHole = 0;
    [SerializeField] float _lastBallInHoleTime = 0f;
    [SerializeField] bool _haveAllPlayersTeedOff = false;
    public GolfPlayerTopDown CurrentPlayer;
    [SyncVar(OnChange = nameof(SyncCurrentPlayerNetId))] public int CurrentPlayerNetId;

    [Header("Player Score Board")]
    [SerializeField] PlayerScoreBoard _playerScoreBoard;

    [Header("Weather Effects")]
    [SerializeField] LightningManager _lightningManager;
    [SerializeField] [SyncVar] public float AveragePlayerWeatherFavor = 0;

    [Header("Camera and UI Stuff?")]
    [SerializeField] CameraViewHole _cameraViewHole;
    [SerializeField] PolygonCollider2D _cameraBoundingBox;
    [SerializeField] TextMeshProUGUI _holeNumberText;
    [SerializeField] TextMeshProUGUI _parText;
    [SerializeField] TextMeshProUGUI _playerNameText;
    [SerializeField] TextMeshProUGUI _numberOfStrokesText;
    [SerializeField] TextMeshProUGUI _terrainTypeText;
    [SerializeField] TextMeshProUGUI _terrainPenaltyText;
    TextInfo titleCase = new CultureInfo("en-US", false).TextInfo;
    [SerializeField] Canvas _loadingHoleCanvas;
    [SerializeField] Canvas _holeInfoCanvas;
    //[SerializeField] GameObject _currentplayerui;
    //[SerializeField] TextMeshProUGUI _currentplayeruiText;

    [Header("Power Up UI Stuff")]
    [SerializeField] TextMeshProUGUI _powerUpMessageText;
    [SerializeField] GameObject _powerUpDisplayHolder;
    [SerializeField] RawImage _powerUpImage;
    [SerializeField] TextMeshProUGUI _powerUpInstructionsText;
    [SerializeField] const string _instructionsPressP = "Press \"p\" to use";


    [Header("Skip For Lightning Info")]
    [SerializeField] public bool PlayerHasSkippedTurn = false;
    [SerializeField] public List<GolfPlayerTopDown> TurnOrderForLightningSkips = new List<GolfPlayerTopDown>();
    [SerializeField] public int TurnsSinceSkip = 0;
    [SerializeField] public int SkipsInARow = 0;
    [SerializeField] bool _skipLightningCheck = false;
    [SyncVar] public float TimeSinceLastSkip = 0f;
    [SyncVar] public float TimeSinceLastTurnStart = 0f;
    [SerializeField] bool _tellingPlayersStormWillPass = false;

    [Header("Statue Stuff")]
    [SerializeField] List<GameObject> StatesOnServer = new List<GameObject>();

    [Header("Game Phase")] // this should probably be changed to an enum or something at some point? Or switch to a "finite state machine?"
    [SyncVar] public bool IsTeeOffChallenge = false;

    [Header("Tasks")]
    [SerializeField] CancellationTokenSource _cancellationTokenSource;

    private void Awake()
    {

        MakeInstance();

        _cancellationTokenSource = new CancellationTokenSource();

        QualitySettings.vSyncCount = 1;

        _loadingHoleCanvas.gameObject.SetActive(false);
        if (!_tileMapManager)
            _tileMapManager = GameObject.FindGameObjectWithTag("TileMapManager").GetComponent<TileMapManager>();
        GolfPlayersServer.OnChange += GolfPlayersServer_OnChange;
    }
    private void OnDestroy()
    {
        // Cancel the token when the object is destroyed
        _cancellationTokenSource.Cancel();
    }
    private void GolfPlayersServer_OnChange(SyncListOperation op, int index, GolfPlayerTopDown oldItem, GolfPlayerTopDown newItem, bool asServer)
    {
        //throw new System.NotImplementedException();
        if (asServer)
            return;
        if (op != SyncListOperation.Add)
            return;
        Debug.Log("GolfPlayersServer_OnChange: op: " + op.ToString() + " index: " + index.ToString() + " as server: " + asServer.ToString() + " Length of GolfPlayersServer: " + GolfPlayersServer.Count.ToString() + " new player name: " + newItem.PlayerName);
    }

    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        // set the ownership to the server?
        Debug.Log("OnStartServer: On GameplayManager: is this player object (" + this.name + ") the host from base.IsHost? " + this.IsHost.ToString() + " and from base.Owner.IsHost? " + base.Owner.IsHost.ToString() + " and an owned client id of: " + base.Owner.ClientId + ":" + OwnerId);
        ClearSyncLists();
    }
    public override void OnStopServer()
    {
        base.OnStopServer();
        ClearSyncLists();
    }
    void ClearSyncLists()
    {
        NetworkPlayersServer.Clear();
        GolfPlayersServer.Clear();
        //GolfPlayersInTeeOffOrderServer.Clear();
        //GolfPlayersOutOfCommissionServer.Clear();
    }
    // Start is called before the first frame update
    void Start()
    {
        /* commented out for now while getting multiplayer set up. Will probably to something different from start anyway to like onstartserver
        // how to start the game for now
        // Reset each player's score for the current hole to 0
        ResetPlayerScoresForNewHole();
        // Reset the number of players that have teed off
        ResetNumberOfPlayersWhoHaveTeedOff();
        // Load a new hole
        LoadNewHole(0);
        // Get the CameraBoundBox and tell players to get it as well?
        GetCameraBoundingBox();
        TellPlayersToGetCameraBoundingBox();
        // Set the Hole Positions for the new hole
        HolePositions = CurrentHoleInCourse.HolePositions;
        // Set the Course aim points for players to use
        TeeOffAimPoint = CurrentHoleInCourse.TeeOffAimPoint;
        // Set the new tee off location
        UpdateTeeOffPositionForNewHole(CurrentHoleInCourse.TeeOffLocation);
        // Set the Camera Zoomed Out position
        UpdateZoomedOutPos(CurrentHoleInCourse.ZoomedOutPos);
        // update the par value for the hole
        UpdateParForNewHole(CurrentHoleInCourse.HolePar);
        // Set the Initial Wind for the hole
        WindManager.instance.SetInitialWindForNewHole();
        WindManager.instance.SetInitialWindDirection();
        // Set initial weather for the hole
        RainManager.instance.SetInitialWeatherForHole();
        //_lightningManager.CheckIfLightningStartsThisTurn();
        // Sort the players by lowest score to start the hole
        OrderListOfPlayers();
        // Set the current player
        SetCurrentPlayer(GolfPlayersInTeeOffOrder[0]);
        // Move the first player to the tee off location
        MoveAllPlayersNearTeeOffLocation();
        MovePlayerToTeeOffLocation(CurrentPlayer);
        // Diable sprite of players that are not the current player
        EnableAndDisablePlayerSpritesForNewTurn(CurrentPlayer);
        // Set the camera on the current player
        SetCameraOnPlayer(CurrentPlayer);
        // Prompt player to start their turn
        CurrentPlayer.PlayerUIMessage("start turn");
        CurrentPlayer.EnablePlayerCanvas(true);
        UpdateUIForCurrentPlayer(CurrentPlayer,true);
        */

        // This is for now with a hardcoded current course. Will need to update for when host can choose course that is then synced to other players
        PlayerScoreBoard.instance.CreateHoleInfoAndParInfoPanels(CurrentCourse);
    }

    // Update is called once per frame
    void Update()
    {

    }
    [Server]
    void ResetPlayerScoresForNewHole()
    {
        foreach (GolfPlayerTopDown player in GolfPlayers)
        {
            player.PlayerScore.ResetScoreForNewHole();
        }
    }
    void ResetNumberOfPlayersWhoHaveTeedOff()
    {
        _numberOfPlayersTeedOff = 0;
        _haveAllPlayersTeedOff = false;
        GolfPlayersInTeeOffOrder.Clear();
    }
    async void LoadNewHole(int index)
    {
        // make sure there is a hole to load? Shouldn't happen unless game is over...
        if (index > CurrentCourse.HolesInCourse.Length)
            return;

        // First, clear the tilemap of any pre-existing hole tiles
        Debug.Log("GameplayManagerTopDownGolf: ClearMap: start time: " + Time.time.ToString());
        //_tileMapManager.ClearMap(CurrentCourse.HolesInCourse[CurrentHoleIndex]);
        // load the next map in the course
        //CurrentHoleIndex = index;
        //CurrentHoleInCourse = CurrentCourse.HolesInCourse[CurrentHoleIndex];
        //_tileMapManager.LoadMap(CurrentHoleInCourse);

        // Old, from singleplayer version
        Debug.Log("GameplayManagertopDownGolf: LoadNewHole: Starting task to load hole at index: " + index.ToString() + ". Time: " + Time.time);
        _holeInfoCanvas.gameObject.SetActive(false);
        _loadingHoleCanvas.gameObject.SetActive(true);
        //await _tileMapManager.LoadMapAsTask(CurrentHoleInCourse);
        HoleHoleObjects.Clear();
        await _tileMapManager.LoadMapAsTask(CurrentCourse.HolesInCourse[index]);
        Debug.Log("GameplayManagertopDownGolf: LoadNewHole: Task to load hole at index: " + index.ToString() + " completed. Time: " + Time.time);
        _loadingHoleCanvas.gameObject.SetActive(false);
        _holeInfoCanvas.gameObject.SetActive(true);
        _holeNumberText.text = CurrentHoleInCourse.CourseName + " #" + (CurrentHoleIndex + 1).ToString();
        // End of old, singleplayer version
    }
    [Server]
    void LoadNewHoleServer(int index)
    {
        Debug.Log("LoadNewHoleServer: loading index: " + index.ToString() + ". Total number of courses: " + CurrentCourse.HolesInCourse.Length.ToString());
        CurrentHoleIndex = index;
        CurrentHoleInCourse = CurrentCourse.HolesInCourse[CurrentHoleIndex];
        // tell all clients to load the next hole and to then update all the client side stuff
        RpcLoadNewHoleOnClient(CurrentHoleIndex, this.IsTeeOffChallenge);

        // Spawn the statues on the server
        //StartSpawnStatues(CurrentHoleInCourse);
        //StatueSpawner.instance.SpawnStatuesOnServer(CurrentHoleInCourse.Statues);
    }
    [Server]
    void StartSpawnStatues(ScriptableHole hole)
    {
        //if (hole.GoodStatuePositions.Length > 0)
        //{
        //    SpawnStatues(hole.GoodStatuePositions, "good");
        //}
        //if (hole.BadStatuePositions.Length > 0)
        //{
        //    SpawnStatues(hole.BadStatuePositions, "bad");
        //}
    }
    [Server]
    void SpawnStatues(Vector3[] spawnPositions, string statueType)
    {
        for (int i = 0; i < spawnPositions.Length; i++)
        {
            StatueSpawner.instance.SpawnStatue(spawnPositions[i], statueType);
        }
    }
    [ObserversRpc]
    void RpcLoadNewHoleOnClient(int index, bool teeOffChallenge = false)
    {
        Debug.Log("RpcLoadNewHoleOnClient: Starting task to load hole at index: " + index.ToString() + ". Time: " + Time.time);
        if (teeOffChallenge)
        {
            CurrentCourse = TeeOffChallenges;
        }
        else
        {
            CurrentCourse = CourseToPlay;
        }

        CurrentHoleInCourse = CurrentCourse.HolesInCourse[index];
        if (PlayerScoreBoard.instance.IsScoreBoardOpen)
            PlayerScoreBoard.instance.CloseScoreBoard();
        LoadNewHole(index);
        GetCameraBoundingBox(CurrentHoleInCourse.PolygonPoints, true);
        TellPlayersToGetCameraBoundingBox();
        //// Set the Hole Positions for the new hole
        HolePositions = CurrentHoleInCourse.HolePositions;
        //// Set the Course aim points for players to use
        TeeOffAimPoint = CurrentHoleInCourse.TeeOffAimPoint;
        //// Set the new tee off location
        UpdateTeeOffPositionForNewHole(CurrentHoleInCourse.TeeOffLocation);
        //// Set the Camera Zoomed Out position
        UpdateZoomedOutPos(CurrentHoleInCourse.ZoomedOutPos, CurrentHoleInCourse.CameraZoomValue);
        //// update the par value for the hole
        UpdateParForNewHole(CurrentHoleInCourse.HolePar);
        if (IsServer)
        {
            StatueSpawner.instance.SpawnStatuesOnServer(CurrentHoleInCourse.Statues);
            PowerUpManagerTopDownGolf.instance.SpawnBallonsForNewHole(CurrentHoleInCourse.BalloonPowerUps);
        }
    }
    public void UpdateTeeOffPositionForNewHole(Vector3 newPosition)
    {
        Debug.Log("UpdateTeeOffPositionForNewHole: " + newPosition.ToString());
        TeeOffPosition = newPosition;
    }
    public void UpdateParForNewHole(int newPar)
    {
        Debug.Log("UpdateParForNewHole: " + newPar.ToString());
        CurrentHolePar = newPar;
        _parText.text = "Par " + CurrentHolePar.ToString();
    }
    void OrderListOfPlayers()
    {

        if (CurrentHoleIndex == 0)
        {
            GolfPlayersInTeeOffOrder.Clear();
            if (_playerOrderFromChallenege.Count > 0)
            {
                GolfPlayersInTeeOffOrder.AddRange(_playerOrderFromChallenege);
            }
            else
            {
                GolfPlayersInTeeOffOrder.AddRange(GolfPlayers);
            }
        }
        else
        {
            GolfPlayersInTeeOffOrder.Clear();
            GolfPlayersInTeeOffOrder.AddRange(GolfPlayers.OrderByDescending(x => x.PlayerScore.TotalStrokesForCourse).ToList());
        }
        SetGolfPlayersInTeeOffOrderForClients(GolfPlayersInTeeOffOrder);
    }
    [ObserversRpc]
    void SetGolfPlayersInTeeOffOrderForClients(List<GolfPlayerTopDown> playerList)
    {
        if (IsLocalPlayerHost())
            return;
        Debug.Log("SetGolfPlayersInTeeOffOrderForClients");
        GolfPlayersInTeeOffOrder.Clear();
        GolfPlayersInTeeOffOrder.AddRange(playerList);
    }
    void SetCurrentPlayer(GolfPlayerTopDown player)
    {
        // this check probably isn't needed unless I have some sort of event tied to the current player changing and don't want to trigger it multiple times?
        //if (CurrentPlayer == player)
        //    return;
        CurrentPlayer = player;
        CurrentPlayerNetId = player.ObjectId;
    }
    void MovePlayerToTeeOffLocation(GolfPlayerTopDown player)
    {
        Debug.Log("MovePlayerToTeeOffLocation: " + player.PlayerName);
        //player.transform.position = TeeOffPosition;
        //player.MyBall.transform.position = TeeOffPosition;
        //player.MyBall.ResetBallSpriteForNewHole();
        player.MovePlayerToPosition(player.Owner, TeeOffPosition, true);
        player.RpcResetBallSpriteForNewHole();

    }
    [Server]
    void MoveAllPlayersNearTeeOffLocation()
    {
        Debug.Log("MoveAllPlayersNearTeeOffLocation: ");
        foreach (GolfPlayerTopDown player in GolfPlayers)
        {
            //Vector3 offset = new Vector3(5f, UnityEngine.Random.Range(-5f, 5f), 0f);
            //player.transform.position = TeeOffPosition - offset;
            //player.MyBall.transform.position = TeeOffPosition - offset;
            //player.MyBall.ResetBallSpriteForNewHole();
            if (player == CurrentPlayer)
            {
                player.MovePlayerToPosition(player.Owner, TeeOffPosition);
                player.RpcResetBallSpriteForNewHole();
            }
            else
            {
                Vector3 offset = new Vector3(UnityEngine.Random.Range(3f, 6f), UnityEngine.Random.Range(-5f, 5f), 0f);
                Vector3 newPos = TeeOffPosition - offset;
                player.MovePlayerToPosition(player.Owner, newPos);
                player.RpcResetBallSpriteForNewHole();
            }
        }
    }
    public void PlayerTeedOff(GolfPlayerTopDown submittingPlayer)
    {
        Debug.Log("PlayerTeedOff: Count of all players: " + GolfPlayers.Count.ToString() + " count of Players in tee off order: " + GolfPlayersInTeeOffOrder.Count.ToString());
        _numberOfPlayersTeedOff++;
        GolfPlayersInTeeOffOrder.Remove(submittingPlayer);
        Debug.Log("PlayerTeedOff: After doing GolfPlayersInTeeOffOrder.Remove(submittingPlayer): Count of all players: " + GolfPlayers.Count.ToString() + " count of Players in tee off order: " + GolfPlayersInTeeOffOrder.Count.ToString());
        _haveAllPlayersTeedOff = HaveAllPlayersTeedOff();
        Debug.Log("PlayerTeedOff: After doing _haveAllPlayersTeedOff = HaveAllPlayersTeedOff(): Count of all players: " + GolfPlayers.Count.ToString() + " count of Players in tee off order: " + GolfPlayersInTeeOffOrder.Count.ToString());

    }
    [Server]
    public void StartCurrentPlayersTurn(GolfPlayerTopDown requestingPlayer)
    {
        // to make sure the new player who was struck by lightning is pressing space?
        if (Time.time < (TimeSinceLastTurnStart + 0.15f))
            return;
        else
            TimeSinceLastTurnStart = Time.time;


        if (requestingPlayer != CurrentPlayer)
            return;

        //UpdateUIForCurrentPlayer(requestingPlayer);
        RpcUpdateUIForCurrentPlayer(requestingPlayer);
        //CurrentPlayer.StartPlayerTurn();
        CurrentPlayer.ServerSetIsPlayersTurn(true);

        // Check if the player will be struck by lightning this turn

    }
    void SetCameraOnPlayer(GolfPlayerTopDown player)
    {
        if (!player)
            return;
        //player.SetCameraOnPlayer();
        Debug.Log("GameplayManager: SetCameraOnPlayer: For player: " + player.PlayerName);
        player.RpcSetCameraOnPlayer();
    }
    public async void StartNextPlayersTurn(GolfBallTopDown ball, bool playerSkippingForLightning = false, bool playerWasStruckByLightning = false)
    {
        Debug.Log("GameplayManager: StartNextPlayersTurn: executing... from player: " + ball.MyPlayer.PlayerName + " playerSkippingForLightning: " + playerSkippingForLightning.ToString());
        if (this.IsTeeOffChallenge)
        {
            NextPlayerForChallenege(ball);
            return;
        }
        if (playerSkippingForLightning && Time.time < TimeSinceLastSkip + 0.15f)
        {
            Debug.Log("GameplayManager: StartNextPlayersTurn: playerSkippingForLightning: " + playerSkippingForLightning.ToString() + " current time is: " + Time.time + " and last skip was: " + TimeSinceLastSkip.ToString() + " SKIPPING this StartNextPlayersTurn call...");
            return;
        }
        else if (playerSkippingForLightning && Time.time >= TimeSinceLastSkip + 0.15f)
        {
            Debug.Log("GameplayManager: StartNextPlayersTurn: playerSkippingForLightning: " + playerSkippingForLightning.ToString() + " current time is: " + Time.time + " and last skip was: " + TimeSinceLastSkip.ToString() + " will continue with this StartNextPlayersTurn call...");
            TimeSinceLastSkip = Time.time;
        }

        // Reset the player's used power up stuff after their turn? Should be done before calculating the next player and before any returns from this function that would prevent it from happening?
        ResetPlayersUsedPowerUpEffects(ball.MyPlayer);

        if (ball.IsInHole)
        {
            Debug.Log("GameplayManager: StartNextPlayersTurn: the ball is in the hole! Congrats to player: " + ball.MyPlayer.PlayerName);
            // This only needs to happen here for the single player right now, I think?
            //UpdateUIForCurrentPlayer(ball.MyPlayer);
            RpcUpdateUIForCurrentPlayer(CurrentPlayer, false);
            //return;
        }

        //if (!ball.IsInHole && !playerSkippingForLightning && !playerWasStruckByLightning)
        if (!playerSkippingForLightning && !playerWasStruckByLightning)
        {
            Debug.Log("StartNextPlayersTurn: Calling TellPlayerGroundTheyLandedOn at time: " + Time.time.ToString());
            // check if the player has a mulligan power up here. If they do, prompt them for it?
            if (ball.MyPlayer.HasPowerUp && ball.MyPlayer.PlayerPowerUpType == "mulligan" && !ball.IsInHole)
            {
                Debug.Log("StartNextPlayersTurn: Player has a mulligan power up they need to be prompted about?");
                // may want to use / lookup a "TaskCompletionSource" to do this? https://stackoverflow.com/questions/15122936/write-an-async-method-that-will-await-a-bool
                await ball.MyPlayer.ServerAskPlayerIfTheyWantToMulligan(10);
                Debug.Log("StartNextPlayersTurn: done checking for mulligan. Moving on...");
            }

            //await ball.MyPlayer.TellPlayerGroundTheyLandedOn(3);
            await ball.MyPlayer.ServerTellPlayerGroundTheyLandedOn(3);
            Debug.Log("StartNextPlayersTurn: Returning from TellPlayerGroundTheyLandedOn at time: " + Time.time.ToString());
        }



        // Check if any players still have to hit a ball
        if (_numberOfPlayersInHole >= GolfPlayers.Count)
        {
            AllPlayersInHoleOrIncapacitated(ball);
            return;
        }

        // Set the weather for the next turn
        //await SetWeatherForNextTurn();
        // Find the next player based on tee off position, or by furthest player from hole if all players teed off
        if (playerSkippingForLightning)
        {
            Debug.Log("GameplayManagerTopDownGolf: StartNextPlayersTurn: Player: " + ball.MyPlayer.PlayerName + " skipping for next turn due to lightning.");
            SkipsInARow++; // track number of times players have skipped lightning in a row
            // Skip the current player's turn. Find the next player up based on tee off order, or distance to hole. If no other players are left, then this player stays as current player
            if (_numberOfPlayersInHole == GolfPlayers.Count - 1)
            {
                // If the only player who hasn't made it in the hole is this player, they stay as the current player
                Debug.Log("StartNextPlayersTurn: Skip For Lighning next player will be: Player: " + ball.MyPlayer.PlayerName + " because they are the only player not in the hole.");
                CurrentPlayer = ball.MyPlayer;
            }
            else
            {

                if (PlayerHasSkippedTurn)
                {
                    GetNextPlayerFromLightningSkipList(ball.MyPlayer, playerSkippingForLightning);
                }
                else // For the first player skip for lightning, create the TurnOrderForLightningSkips list that will be used to track turn order after the skip
                {
                    Debug.Log("StartNextPlayersTurn: First player to skip turn for lightning. Player: " + ball.MyPlayer.PlayerName);
                    // Reset Lightning Skip player list just in case
                    ResetLightningSkipInfo();
                    PlayerHasSkippedTurn = true;
                    // Check to see if there are any players who haven't teed off yet. If there are, add them to the TurnOrderForLightningSkips list
                    if (!_haveAllPlayersTeedOff)
                    {
                        TurnOrderForLightningSkips.AddRange(GolfPlayersInTeeOffOrder);
                    }
                    // if the skip isn't from the first player to tee off, or all players have teed off, then create a list of users based on their distance from the hole to set turn order for TurnOrderForLightningSkips
                    if (TurnOrderForLightningSkips.Count != GolfPlayers.Count)
                    {
                        TurnOrderForLightningSkips.AddRange(SortPlayersByDistanceToHole());
                    }

                    // Make sure that the current user is at the end of the TurnOrderForLightningSkips list by removing them and then adding them back to the list
                    if (TurnOrderForLightningSkips.Contains(ball.MyPlayer))
                        TurnOrderForLightningSkips.Remove(ball.MyPlayer);
                    //TurnOrderForLightningSkips.Add(ball.MyPlayer);

                    CurrentPlayer = TurnOrderForLightningSkips[0];
                }
            }

            // new for multiplayeR? Needed to set the current player network id value
            SetCurrentPlayer(CurrentPlayer);
        }
        else if (!playerSkippingForLightning && PlayerHasSkippedTurn)
        {
            Debug.Log("StartNextPlayersTurn: Player did not skip for lightning but PlayerHasSkippedTurn was true before their turn.");
            SkipsInARow = 0;
            GetNextPlayerFromLightningSkipList(ball.MyPlayer, playerSkippingForLightning);
            SetCurrentPlayer(CurrentPlayer);
        }
        else
        {
            SkipsInARow = 0; // reset skips in a row if player did not skip for lightning?
            CurrentPlayer = SelectNextPlayer();
            SetCurrentPlayer(CurrentPlayer);
        }
        // Move the player too the tee off location if they haven't teed off yet. Otherwise, move them to their ball object
        if (!CurrentPlayer.HasPlayerTeedOff)
        {
            MovePlayerToTeeOffLocation(CurrentPlayer);
        }
        else
        {
            Debug.Log("StartNextPlayersTurn: move current player to their ball's position");
            //CurrentPlayer.SetPlayerOnBall();
            CurrentPlayer.MovePlayerToPosition(CurrentPlayer.Owner, CurrentPlayer.MyBall.transform.position);
        }
        if (SkipsInARow >= (GolfPlayers.Count - _numberOfPlayersInHole))
        {
            await StormPassesForSkips(5f);
            _skipLightningCheck = true;
        }
        // removed used powerups
        //PowerUpManagerTopDownGolf.instance.RemoveUsedPowerUps();
        // Set the weather for the next turn
        await SetWeatherForNextTurn();
        // Diable sprite of players that are not the current player
        EnableAndDisablePlayerSpritesForNewTurn(CurrentPlayer);
        // Move the camera to the player
        SetCameraOnPlayer(CurrentPlayer);
        // Prompt player to start their turn
        PromptPlayerForNextTurn();

        // Reset the player's used power up stuff before their turn starts. This is kind of a "just in case" type deal?
        ResetPlayersUsedPowerUpEffects(CurrentPlayer);
    }
    GolfPlayerTopDown SelectNextPlayer()
    {
        if (!_haveAllPlayersTeedOff)
        {
            if (!HaveAllPlayersTeedOff())
            {
                //return GolfPlayersInTeeOffOrder[_numberOfPlayersTeedOff];
                Debug.Log("SelectNextPlayer: Returning from tee order");
                return GolfPlayersInTeeOffOrder[0];
            }
        }

        return FindPlayerFurthestFromHole();
    }
    bool HaveAllPlayersTeedOff()
    {
        if (_numberOfPlayersTeedOff < GolfPlayers.Count)
            return false;
        else
        {
            _haveAllPlayersTeedOff = true;
            return true;
        }
    }
    GolfPlayerTopDown FindPlayerFurthestFromHole()
    {
        Debug.Log("FindPlayerFurthestFromHole: executing...");
        float furthestDistance = 0f;
        GolfPlayerTopDown currentClosestPlayer = CurrentPlayer;
        foreach (GolfPlayerTopDown player in GolfPlayers)
        {
            // skip players whose ball is in the hole
            if (player.MyBall.IsInHole)
                continue;
            float playerDistanceToHole = 0f;
            Vector3 playerBallPosition = player.MyBall.transform.position;
            // loop through all the different holes. Use the hole the player is closest to as their distance value
            for (int i = 0; i < this.HolePositions.Count; i++)
            {
                if (i == 0)
                {
                    playerDistanceToHole = Vector2.Distance(playerBallPosition, this.HolePositions[i]);
                }
                else
                {
                    float thisDistance = Vector2.Distance(playerBallPosition, this.HolePositions[i]);
                    if (thisDistance < playerDistanceToHole)
                        playerDistanceToHole = thisDistance;
                }
            }

            if (playerDistanceToHole > furthestDistance)
            {
                currentClosestPlayer = player;
                furthestDistance = playerDistanceToHole;
            }
            Debug.Log("FindPlayerFurthestFromHole: Checking for player: " + player.PlayerName + " player distance to hole is: " + playerDistanceToHole.ToString() + " furthest distance so far is: " + furthestDistance.ToString());
        }
        Debug.Log("FindPlayerFurthestFromHole: Returning player: " + currentClosestPlayer.PlayerName);
        return currentClosestPlayer;
    }
    public void ResetCurrentPlayer()
    {
        CurrentPlayer = null;
    }
    void GetCameraBoundingBox(Vector2[] polygonPoints, bool forceUpdate = false)
    {
        Debug.Log("GameplayManager: GetCameraBoundingBox");
        if (!_cameraBoundingBox)
            _cameraBoundingBox = GameObject.FindGameObjectWithTag("CameraBoundingBox").GetComponent<PolygonCollider2D>();
        if (forceUpdate)
            _cameraBoundingBox = GameObject.FindGameObjectWithTag("CameraBoundingBox").GetComponent<PolygonCollider2D>();
        if (_cameraBoundingBox)
            _cameraViewHole.GetLinePointsForOutOfBoundsBorder(polygonPoints);
    }
    [Client]
    void TellPlayersToGetCameraBoundingBox()
    {
        foreach (GolfPlayerTopDown player in GolfPlayers)
        {
            player.GetCameraBoundingBox(true);
            //player.RpcGetCameraBoundingBox(player.Owner);
        }
    }
    bool AllPlayersBallsInHole()
    {
        bool allPlayersBallsInHole = false;


        return allPlayersBallsInHole;
    }
    public void PlayersBallInHole()
    {
        if (Time.time < (_lastBallInHoleTime + 0.5f))
            return;
        _numberOfPlayersInHole++;
        _lastBallInHoleTime = Time.time;
    }
    [ObserversRpc]
    void RpcUpdateUIForCurrentPlayer(GolfPlayerTopDown player, bool forTeeOff = false)
    {
        UpdateUIForCurrentPlayer(player, forTeeOff);
        UpdatePowerUpUIForCurrentPlayer(player);
    }
    void UpdateUIForCurrentPlayer(GolfPlayerTopDown player, bool forTeeOff = false)
    {
        if (!player)
            return;
        _playerNameText.text = "Player: " + player.PlayerName;
        _numberOfStrokesText.text = "Strokes: " + player.PlayerScore.StrokesForCurrentHole.ToString();
        if (forTeeOff)
            _terrainTypeText.text = "";
        else
            _terrainTypeText.text = titleCase.ToTitleCase(player.GetTerrainTypeFromBall());
        if (player != LocalGolfPlayer)
            UpdateTerrainPenaltyText(false);
    }
    public void UpdateTerrainPenaltyText(bool enable)
    {
        _terrainPenaltyText.gameObject.SetActive(enable);
    }
    void UpdatePowerUpUIForCurrentPlayer(GolfPlayerTopDown player)
    {
        if (!player)
            return;
        _powerUpInstructionsText.gameObject.SetActive(false);

        if (!player.HasPowerUp)
        {
            DeactivatePowerUpUI();
            return;
        }

        _powerUpMessageText.gameObject.SetActive(false);
        _powerUpMessageText.text = "";

        SetPowerUpSpriteByType(player.PlayerPowerUpType);
        _powerUpDisplayHolder.SetActive(true);
        PowerUpHideInstructions();
        //PowerUpShowInstructions(player);
    }
    void UpdateZoomedOutPos(Vector3 newPos, float newZoomValue)
    {
        if (!_cameraViewHole)
            _cameraViewHole = GameObject.FindGameObjectWithTag("camera").GetComponent<CameraViewHole>();
        _cameraViewHole.SetZoomedOutPosition(newPos);
        _cameraViewHole.SetCameraZoomValue(newZoomValue);
    }
    async void NextHole()
    {
        Debug.Log("Next hole: " + Time.time);
        if (Time.time < _lastNewHoleTime + 0.25f)
            return;
        _lastNewHoleTime = Time.time;
        CurrentHoleIndex++;
        CurrentTurnNumber = 0;
        LastTurnWithWeatherChange = 0;
        _numberOfPlayersInHole = 0;
        AddPlayersOutOfCommissionBack();
        // Save each player's score and reset their "current" score of the new hole
        foreach (GolfPlayerTopDown player in GolfPlayers)
        {
            //player.ResetForNewHole(CurrentHoleIndex);
            player.RpcResetForNewHole(player.Owner, CurrentHoleIndex);
        }
        // Reset the number of players that have teed off
        ResetNumberOfPlayersWhoHaveTeedOff();

        // Remove all statues on the server
        RemoveStatuesForNewHole();

        // Remove all power up balloons on the server
        PowerUpManagerTopDownGolf.instance.DespawnBalloonsForNewHole();
        PowerUpManagerTopDownGolf.instance.DespawnObjectsFromPowerUpsForNewTurn();

        // Load a new hole
        //LoadNewHole(CurrentHoleIndex);
        LoadNewHoleServer(CurrentHoleIndex);
        //// Get the CameraBoundBox and tell players to get it as well?
        //GetCameraBoundingBox();
        //TellPlayersToGetCameraBoundingBox();
        // Set the Hole Positions for the new hole
        HolePositions = CurrentHoleInCourse.HolePositions;
        // Set the Course aim points for players to use
        TeeOffAimPoint = CurrentHoleInCourse.TeeOffAimPoint;
        // Set the new tee off location
        UpdateTeeOffPositionForNewHole(CurrentHoleInCourse.TeeOffLocation);
        // Set the Camera Zoomed Out position
        UpdateZoomedOutPos(CurrentHoleInCourse.ZoomedOutPos, CurrentHoleInCourse.CameraZoomValue);
        // update the par value for the hole
        UpdateParForNewHole(CurrentHoleInCourse.HolePar);
        // Update the "rain sound" for the next hole
        RainManager.instance.GetRainSoundForHole();
        // If there was a tornado on the previous hole, destroy it. They players are in a different location now so it doesn't make sense for the tornado to stay in the same "spot" it was on the previous hole 

        //skipping for now while testing other things in multiplayer
        //WindManager.instance.DestroyTornadoForNextHole();

        // Sort the players by lowest score to start the hole
        OrderListOfPlayers();
        // Set the current player
        SetCurrentPlayer(GolfPlayersInTeeOffOrder[0]);
        // Move the first player to the tee off location        
        MovePlayerToTeeOffLocation(CurrentPlayer);
        // Set the weather for the next turn
        MoveAllPlayersNearTeeOffLocation(); // move the players first so the tornado knows where to go?
        await SetWeatherForNextTurn(true);
        // Sort the players by lowest score to start the hole
        //OrderListOfPlayers();
        // Set the current player
        //SetCurrentPlayer(GolfPlayersInTeeOffOrder[0]);
        // Move the first player to the tee off location        
        //MovePlayerToTeeOffLocation(CurrentPlayer);

        // Diable sprite of players that are not the current player
        EnableAndDisablePlayerSpritesForNewTurn(CurrentPlayer);
        // Set the camera on the current player
        SetCameraOnPlayer(CurrentPlayer);
        // Prompt player to start their turn
        PromptPlayerForNextTurn();

        // Reset the player's used power up stuff before their turn starts. This is kind of a "just in case" type deal?
        ResetPlayersUsedPowerUpEffects(CurrentPlayer);
    }
    async Task SetWeatherForNextTurn(bool newHole = false)
    {

        Debug.Log("SetWeatherForNextTurn");
        // Get the average weather favor of all players?
        AveragePlayerWeatherFavor = GetAveragePlayerFavor();
        // Set the new wind for the next turn
        WindManager.instance.UpdateWindForNewTurn(this.CurrentPlayer);
        WindManager.instance.UpdateWindDirectionForNewTurn();
        // Set new weather for the next turn
        RainManager.instance.UpdateWeatherForNewTurn(this.CurrentPlayer);

        // skipping for now while testing other things for multiplayer
        WindManager.instance.CheckIfTornadoWillSpawn(newHole);
        ////WindManager.instance.MoveTornadoForNewTurn();

        // skipping for now while testing other things for multiplayer
        //Debug.Log("SetWeatherForNextTurn: move tornado start at: " + Time.time);
        await WindManager.instance.MoveTornadoTask();
        //Debug.Log("SetWeatherForNextTurn: move tornado end at: " + Time.time);

        // skipping for now while testing other things for multiplayer
        // make sure lightning occurs after the tornado tracking?
        _lightningManager.CheckIfLightningStartsThisTurn(_skipLightningCheck, newHole);
        _skipLightningCheck = false;
        if (!_lightningManager.IsThereLightning)
        {
            SkipsInARow = 0;
        }


    }
    void PromptPlayerForNextTurn()
    {
        Debug.Log("PromptPlayerForNextTurn: For player: " + CurrentPlayer.PlayerName);
        CurrentPlayer.RpcEnablePromptPlayerToStartTurn(CurrentPlayer.Owner, true);
        if (_lightningManager.IsThereLightning)
        {
            //CurrentPlayer.PlayerUIMessage("lightning");
            CurrentPlayer.RpcPlayerUIMessage(CurrentPlayer.Owner, "lightning");
            CurrentPlayer.RpcEnablePromptPlayerSkipForLightning(CurrentPlayer.Owner, true);
        }
        else
        {
            //CurrentPlayer.PlayerUIMessage("start turn");
            CurrentPlayer.RpcPlayerUIMessage(CurrentPlayer.Owner, "start turn");
            
            CurrentPlayer.RpcEnablePromptPlayerSkipForLightning(CurrentPlayer.Owner, false);
        }
        //CurrentPlayer.EnablePlayerCanvas(true);
        CurrentPlayer.RpcEnablePlayerCanvasForNewTurn(true);
        //UpdateUIForCurrentPlayer(CurrentPlayer);
        RpcUpdateUIForCurrentPlayer(CurrentPlayer, true);
    }
    async void EndGame()
    {
        AddPlayersOutOfCommissionBack();

        // The below commented out section tells all players at once that the game has ended, causing the UI from each player to overlap. This will be more useful for networked multiplayer, but my "local multiplayer" should instead do it sequentially like below
        /*var tasks = new Task[GolfPlayers.Count];
        Debug.Log("EndGame: Before calling TellPlayerGameIsOver time is: " + Time.time.ToString());
        // Save each player's score and reset their "current" score of the new hole
        for (int i = 0; i < GolfPlayers.Count; i++)
        {
            GolfPlayerTopDown player = GolfPlayers[i];
            player.ResetForNewHole(CurrentHoleIndex + 1);
            tasks[i] = player.TellPlayerGameIsOver(5);
        }

        await Task.WhenAll(tasks);*/
        // doing each "TellPlayerGameIsOver" for "local multiplayer"
        Debug.Log("EndGame: Before calling TellPlayerGameIsOver time is: " + Time.time.ToString());
        for (int i = 0; i < GolfPlayers.Count; i++)
        {
            GolfPlayerTopDown player = GolfPlayers[i];
            player.RpcResetForNewHole(player.Owner, CurrentHoleIndex + 1);
        }
        // Doing this a second time for the "await" thing to tell each player's score. Can probably skip once the scoreboard works? Instead, just show the scoreboard after everyone has had their score updated?
        // Would probably still need to do an await type thing to make sure all the scores are synced before bringing up the score board? Similar to how the "tell player type of ground landed on" stuff works where there is an await on the server that then waits for a response from client to confirm they got the new score
        for (int i = 0; i < GolfPlayers.Count; i++)
        {
            GolfPlayerTopDown player = GolfPlayers[i];
            await player.ServerTellPlayerGameIsOver(5);
        }

        Debug.Log("EndGame: AFTER calling TellPlayerGameIsOver time is: " + Time.time.ToString());

    }
    [Server]
    public void LightningForPlayerHit(GolfPlayerTopDown player)
    {
        _lightningManager.LightningForHit(player);
    }
    public void MovePlayerToBackOfTeeOffOrder(GolfPlayerTopDown player)
    {
        GolfPlayersInTeeOffOrder.Remove(player);
        GolfPlayersInTeeOffOrder.Add(player);
    }
    void ResetLightningSkipInfo()
    {
        PlayerHasSkippedTurn = false;
        TurnOrderForLightningSkips.Clear();
        TurnsSinceSkip = 0;
    }
    List<GolfPlayerTopDown> SortPlayersByDistanceToHole()
    {
        List<GolfPlayerTopDown> playersByDistance = new List<GolfPlayerTopDown>();

        foreach (GolfPlayerTopDown player in GolfPlayers)
        {
            if (player.MyBall.IsInHole)
                continue;
            if (!player.HasPlayerTeedOff)
                continue;
            //player.SetDistanceToHoleForPlayer();
            player.ServerSetDistanceToHoleForPlayer();
            playersByDistance.Add(player);
        }

        playersByDistance = playersByDistance.OrderByDescending(x => x.DistanceToHole).ToList();
        return playersByDistance;
    }
    void GetNextPlayerFromLightningSkipList(GolfPlayerTopDown playerToRemove, bool didThisPlayerSkip)
    {
        Debug.Log("GetNextPlayerFromLightningSkipList: Player: " + playerToRemove.PlayerName + " has skipped for lightning. They are not the first to do so.");
        if (TurnOrderForLightningSkips.Contains(playerToRemove))
            TurnOrderForLightningSkips.Remove(playerToRemove);
        if (TurnOrderForLightningSkips.Count > 0)
        {
            CurrentPlayer = TurnOrderForLightningSkips[0];
            Debug.Log("GetNextPlayerFromLightningSkipList: " + TurnOrderForLightningSkips.Count.ToString() + " players remaining in TurnOrderForLightningSkips. Setting player: " + CurrentPlayer.PlayerName + " as the next player");
        }
        else
        {
            ResetLightningSkipInfo();
            CurrentPlayer = SelectNextPlayer();
            Debug.Log("GetNextPlayerFromLightningSkipList: TurnOrderForLightningSkips should be zero. It is: " + TurnOrderForLightningSkips.Count.ToString() + ". Resetting info and setting current player as normally would. Current Player is: " + CurrentPlayer.PlayerName);
        }
    }
    void EnableAndDisablePlayerSpritesForNewTurn(GolfPlayerTopDown playerToEnable)
    {
        foreach (GolfPlayerTopDown player in GolfPlayers)
        {
            /*if (player == playerToEnable)
                player.EnablePlayerSprite(true);
            else
                player.EnablePlayerSprite(false);*/
            if (player == playerToEnable)
                player.RpcEnablePlayerSprite(true);
            else
                player.RpcEnablePlayerSprite(false);
        }
        if (GolfPlayersOutOfCommission.Count > 0)
        {
            foreach (GolfPlayerTopDown player in GolfPlayersOutOfCommission)
            {
                player.RpcEnablePlayerSprite(false);
            }
        }
    }
    public void PlayerWasStruckByLightning(GolfPlayerTopDown player)
    {
        Debug.Log("GameplayManager: PlayerWasStruckByLightning: " + player.PlayerName + " was struck by lightning.");
        // this is just for testing with "local multiplayer" to make sure when the player that was struck by lightning presses space, it doesn't also get read as the next player pressing space to start their turn
        TimeSinceLastTurnStart = Time.time;
        PlayerOutOfCommission(player);
        if (player.PlayerMulligan)
        {
            Debug.Log("PlayerWasStruckByLightning: Player " + player.PlayerName + " was struck by lightning while using their mulligan? " + player.PlayerMulligan.ToString() + " resetting their PlayerMulligan value...");
            player.RemovePlayerMulligan();
            return;
        }
        StartNextPlayersTurn(player.MyBall, false, true);
    }
    void PlayerOutOfCommission(GolfPlayerTopDown player)
    {
        if (GolfPlayers.Contains(player))
            GolfPlayers.Remove(player);
        if (GolfPlayersInTeeOffOrder.Contains(player))
            GolfPlayersInTeeOffOrder.Remove(player);
        if (TurnOrderForLightningSkips.Contains(player))
        {
            TurnOrderForLightningSkips.Remove(player);
            if (TurnOrderForLightningSkips.Count == 0)
                ResetLightningSkipInfo();
        }
        if (!GolfPlayersOutOfCommission.Contains(player))
            GolfPlayersOutOfCommission.Add(player);
    }
    void AddPlayersOutOfCommissionBack()
    {
        if (GolfPlayersOutOfCommission.Count > 0)
        {
            GolfPlayers.AddRange(GolfPlayersOutOfCommission);
            GolfPlayersOutOfCommission.Clear();
        }
    }
    public async void AllPlayersInHoleOrIncapacitated(GolfBallTopDown ball)
    {
        if (PlayerHasSkippedTurn)
            ResetLightningSkipInfo();

        if ((CurrentHoleIndex + 1) < CurrentCourse.HolesInCourse.Length)
        {
            Debug.Log("AllPlayersInHoleOrIncapacitated: All players have made it into the hole. No more remaining players! Loading next hole?");
            await ball.MyPlayer.ServerTellPlayerHoleEnded(3);
            NextHole();
        }
        else
        {
            Debug.Log("AllPlayersInHoleOrIncapacitated: All players have made it into the hole. No more remaining players! And this was the last hole! Ending the game...");
            EndGame();
        }
    }
    public bool AreAllPlayersInHoleOrIncapacitated()
    {
        if (_numberOfPlayersInHole >= GolfPlayers.Count)
            return true;
        else
            return false;
    }
    [Server]
    public void AddGolfPlayer(GolfPlayerTopDown player)
    {
        if (!GolfPlayersServer.Contains(player))
            GolfPlayersServer.Add(player);
        if (!GolfPlayers.Contains(player))
            GolfPlayers.Add(player);
    }
    [Server]
    public void RemoveGolfPlayer(GolfPlayerTopDown player)
    {
        //Debug.Log("RemoveGolfPlayer: removing player with connection id of: " + player.ConnectionId);
        if (GolfPlayersServer.Contains(player))
            GolfPlayersServer.Remove(player);
        if (GolfPlayers.Contains(player))
            GolfPlayers.Remove(player);
    }
    [Server]
    public void HostStartGame(NetworkConnection conn)
    {
        if (!conn.IsHost)
            return;
        Debug.Log("HostStartGame: Starting the game.");
        //GolfPlayers.Clear();
        //for (int i = 0; i < GolfPlayersServer.Count; i++)
        //{
        //    GolfPlayers.Add(GolfPlayersServer[i]);
        //}
        RpcSetGolfPlayersLocally(GolfPlayers);
        _playerOrderFromChallenege.Clear();
        // check for tee off challenge
        if (GolfPlayers.Count <= 1)
        {
            StartFirstHole();
        }
        else
        {
            StartTeeOffChallenge();
        }

        //StartTeeOffChallenge();
    }
    [Server]
    void StartFirstHole()
    {
        ResetPlayerScoresForNewHole();
        //// Reset the number of players that have teed off
        ResetNumberOfPlayersWhoHaveTeedOff();
        //// Load a new hole
        //LoadNewHole(0);
        //LoadNewHoleServer(0);
        if (this.IsTeeOffChallenge)
            LoadNewHoleServer(CurrentHoleIndex);
        else
            LoadNewHoleServer(0);

        // Client side checks for things that can just be sent to each client to do
        //// Get the CameraBoundBox and tell players to get it as well?
        //GetCameraBoundingBox();
        //TellPlayersToGetCameraBoundingBox();
        //// Set the Hole Positions for the new hole

        HolePositions = CurrentHoleInCourse.HolePositions;
        //// Set the Course aim points for players to use
        TeeOffAimPoint = CurrentHoleInCourse.TeeOffAimPoint;
        //// Set the new tee off location
        UpdateTeeOffPositionForNewHole(CurrentHoleInCourse.TeeOffLocation);
        //// Set the Camera Zoomed Out position
        UpdateZoomedOutPos(CurrentHoleInCourse.ZoomedOutPos, CurrentHoleInCourse.CameraZoomValue);
        //// update the par value for the hole
        UpdateParForNewHole(CurrentHoleInCourse.HolePar);

        // Server side stuff to set weather conditions?
        //// Set the Initial Wind for the hole

        if (this.IsTeeOffChallenge)
        {
            WindManager.instance.SetRandomWindPowerForChallenegeShot();
        }
        else
        {
            WindManager.instance.SetInitialWindForNewHole();            
        }
        WindManager.instance.SetInitialWindDirection();
        //// Set initial weather for the hole
        RainManager.instance.SetInitialWeatherForHole();

        ////_lightningManager.CheckIfLightningStartsThisTurn();
        //// Sort the players by lowest score to start the hole
        OrderListOfPlayers();
        //// Set the current player
        SetCurrentPlayer(GolfPlayersInTeeOffOrder[0]);
        //// Move the first player to the tee off location
        MoveAllPlayersNearTeeOffLocation();

        //// Diable sprite of players that are not the current player
        EnableAndDisablePlayerSpritesForNewTurn(CurrentPlayer);

        // move the player and their ball if this is after the tee off challenege?
        //if (_wasThereATeeOffChallenege)
        //{
        //    MovePlayerToTeeOffLocation(CurrentPlayer);
        //    PromptPlayerForNextTurn();
        //}
    }

    [ObserversRpc(ExcludeOwner = true)]
    void RpcSetGolfPlayersLocally(List<GolfPlayerTopDown> players)
    {
        if (IsLocalPlayerHost())
            return;
        Debug.Log("RpcSetGolfPlayersLocally");
        GolfPlayers.Clear();
        for (int i = 0; i < players.Count; i++)
        {
            GolfPlayers.Add(players[i]);
        }
    }
    [Server]
    public void AddNetworkPlayer(NetworkPlayer player)
    {
        if (NetworkPlayersServer.Contains(player))
            return;

        NetworkPlayersServer.Add(player);
        Debug.Log("AddNetworkPlayer: Player added with connection id of: " + player.OwnerId + " count of NetworkPlayerServers: " + NetworkPlayersServer.Count.ToString());
    }
    [Server]
    public void RemoveNetworkPlayer(NetworkPlayer player)
    {
        if (NetworkPlayersServer.Contains(player))
            NetworkPlayersServer.Remove(player);

        Debug.Log("RemoveNetworkPlayer: Player removed with connection id of: " + player.OwnerId + " count of NetworkPlayerServers: " + NetworkPlayersServer.Count.ToString());
    }
    public void SetLocalGolfPlayer(GolfPlayerTopDown player)
    {
        if (player.tag.ToLower().Contains("local"))
            LocalGolfPlayer = player;
    }
    void SyncCurrentPlayerNetId(int prev, int next, bool asServer)
    {
        if (IsLocalPlayerHost())
        {
            //_currentplayerui.SetActive(true);
            //_currentplayeruiText.text = CurrentPlayer.PlayerName + ":" + next.ToString();
            return;
        }

        Debug.Log("SyncCurrentPlayerNetId: " + next.ToString());
        CurrentPlayer = InstanceFinder.ClientManager.Objects.Spawned[next].GetComponent<GolfPlayerTopDown>();
        //_currentplayerui.SetActive(true);
        //_currentplayeruiText.text = CurrentPlayer.PlayerName + ":" + next.ToString();
    }
    bool IsLocalPlayerHost()
    {
        if (!LocalGolfPlayer || LocalGolfPlayer.IsHost)
            return true;

        return false;
    }
    [Server]
    public void CheckIfAllBallsAreSpawned()
    {
        bool allSpawned = false;
        foreach (GolfPlayerTopDown player in GolfPlayers)
        {
            if (player.NumberOfBallsSpawnedForClient != GolfPlayers.Count)
            {
                allSpawned = false;
                break;
            }
            allSpawned = true;
        }
        if (allSpawned)
        {
            SetCameraOnPlayer(CurrentPlayer);
            CurrentPlayer.RpcPlayerUIMessage(CurrentPlayer.Owner, "start turn");
            CurrentPlayer.RpcEnablePromptPlayerToStartTurn(CurrentPlayer.Owner, true);
            CurrentPlayer.RpcEnablePromptPlayerSkipForLightning(CurrentPlayer.Owner, false);
            CurrentPlayer.RpcEnablePlayerCanvasForNewTurn(true);
            RpcUpdateUIForCurrentPlayer(CurrentPlayer, true);
        }
    }
    async Task StormPassesForSkips(float duration)
    {
        Debug.Log("StormPassesForSkips: Number of skippers: " + SkipsInARow.ToString() + " And total number of players: " + GolfPlayersServer.Count.ToString());
        _lightningManager.EndStorm();
        float endTime = Time.time + duration;

        // Send the message to the player
        CurrentPlayer.RpcPlayerUIMessage(CurrentPlayer.Owner, "storm passed");

        while (Time.time < endTime)
        {
            await Task.Yield();
        }

        // End the message?
        CurrentPlayer.RpcEnablePlayerCanvasForNewTurn(false);

        SkipsInARow = 0;
    }
    public void SaveStatues(List<GameObject> statuesSpawned)
    {
        StatesOnServer.Clear();
        if (statuesSpawned.Count <= 0)
            return;
        StatesOnServer.AddRange(statuesSpawned);
    }
    void RemoveStatuesForNewHole()
    {
        if (StatesOnServer.Count <= 0)
            return;

        foreach (GameObject statue in StatesOnServer)
        {
            GameObject objectToDestroy = statue;
            InstanceFinder.ServerManager.Despawn(objectToDestroy);
        }
    }
    public float GetAveragePlayerFavor()
    {
        float averageFavor = 0f;

        if (this.GolfPlayersServer.Count <= 0)
            return averageFavor;

        float totalFavor = 0f;
        foreach (GolfPlayerTopDown player in this.GolfPlayersServer)
        {
            totalFavor += player.FavorWeather;
        }
        averageFavor = totalFavor / this.GolfPlayersServer.Count;
        Debug.Log("GameplayManagerTopDownGolf.cs: GetAveragePlayerFavor: Average player favor is: " + averageFavor.ToString() + " based on total favor of: " + totalFavor.ToString() + " and " + this.GolfPlayersServer.Count.ToString() + " number of players.");

        return averageFavor;
    }
    public void PlayerGotNewPowerUp(string newPowerUpType, string newPowerUpText)
    {
        StartCoroutine(NewPowerUpMessage(newPowerUpText));
        SetPowerUpSpriteByType(newPowerUpType);
        _powerUpDisplayHolder.SetActive(true);
    }
    IEnumerator NewPowerUpMessage(string newPowerUpText)
    {
        _powerUpMessageText.text = "New Power Up! " + newPowerUpText;
        _powerUpMessageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(5.0f);
        _powerUpMessageText.gameObject.SetActive(false);
        _powerUpMessageText.text = "";
    }
    void SetPowerUpSpriteByType(string type)
    {
        Debug.Log("SetPowerUpSpriteByType: " + type);
        _powerUpImage.texture = PowerUpManagerTopDownGolf.instance.GetPowerUpSprite(type).texture;
        _powerUpImage.enabled = true;
    }
    void DeactivatePowerUpUI()
    {
        _powerUpMessageText.text = "";
        _powerUpMessageText.gameObject.SetActive(false);
        _powerUpDisplayHolder.SetActive(false);
        _powerUpImage.enabled = false;
    }
    public void PowerUpShowInstructions(GolfPlayerTopDown player)
    {
        if (!player.HasPowerUp && !player.UsedPowerupThisTurn)
        {
            _powerUpInstructionsText.gameObject.SetActive(false);
            return;
        }

        if (player.PlayerPowerUpType != "mulligan")
        {
            _powerUpInstructionsText.gameObject.SetActive(true);
            SetInstructionsText(player.UsedPowerupThisTurn);
        }
        else
        {
            _powerUpInstructionsText.gameObject.SetActive(false);
        }
    }
    public void PowerUpHideInstructions()
    {
        _powerUpInstructionsText.gameObject.SetActive(false);
    }
    void ResetPlayersUsedPowerUpEffects(GolfPlayerTopDown player)
    {
        Debug.Log("GameplayManagerTopDownGolf.cs: ResetPlayersUsedPowerUpEffects: " + player.PlayerName);
        player.ResetPlayersUsedPowerUpEffects();
    }
    public void PlayerUsedMulligan(GolfPlayerTopDown mulliganPlayer)
    {
        SetCurrentPlayer(mulliganPlayer);
        StartCurrentPlayersTurn(mulliganPlayer);
    }
    public void SetInstructionsText(bool usedPowerUp)
    {
        if (usedPowerUp)
            _powerUpInstructionsText.text = "Used!";
        else
            _powerUpInstructionsText.text = _instructionsPressP;
    }
    public bool WillPlayerBeStruckByLightning(GolfPlayerTopDown player)
    {
        return _lightningManager.WillPlayerBeStruckByLightning(player);
    }
    public LightningManager GetLightningManager()
    {
        return _lightningManager;
    }
    public void BrokenStatueWeatherEffects(string statueType, GolfPlayerTopDown playerThatBrokeStatue)
    {
        Debug.Log("BrokenStatueWeatherEffects: " + statueType + " for player: " + playerThatBrokeStatue.PlayerName);
        if (statueType == "bad-weather")
            BrokenBadStatueWeatherEffects(playerThatBrokeStatue);
        else
            BrokenGoodStatueWeatherEffects(playerThatBrokeStatue);
    }
    void BrokenBadStatueWeatherEffects(GolfPlayerTopDown playerThatBrokeStatue)
    {
        // tornado stuff for bad statue
        WindManager.instance.PlayerBrokeBadStatue(playerThatBrokeStatue);
    }
    void BrokenGoodStatueWeatherEffects(GolfPlayerTopDown playerThatBrokeStatue)
    {
        _lightningManager.StopLightningForBrokenGoodWeatherStatue();
        WindManager.instance.PlayerBrokeGoodStatue();
    }
    void ClearPlayerWeatherFavorForFirstHole()
    {
        foreach (GolfPlayerTopDown player in GolfPlayers)
        {
            player.ResetFavorWeatherForNewGame();
        }
    }
    #region Tee Off Challenge
    void StartTeeOffChallenge()
    {
        this.IsTeeOffChallenge = true;
        //SelectedChallenge = GetChallenge(TeeOffChallenges);
        _playerOrderFromChallenege.Clear();
        CurrentCourse = TeeOffChallenges;
        CurrentHoleIndex = GetChallenegeIndex(TeeOffChallenges);
        this.TeeOffChallengeClubType = CurrentCourse.HolesInCourse[CurrentHoleIndex].ClubToUse;
        StartFirstHole();
    }
    ScriptableHole GetChallenge(ScriptableCourse challenges)
    {
        System.Random random = new System.Random();
        int index = random.Next(0, challenges.HolesInCourse.Length);
        return challenges.HolesInCourse[index];
    }
    int GetChallenegeIndex(ScriptableCourse challenges)
    {
        System.Random random = new System.Random();
        int index = random.Next(0, challenges.HolesInCourse.Length);
        return index;
    }
    public async void NextPlayerForChallenege(GolfBallTopDown ball)
    {
        Debug.Log("NextPlayerForChallenege: from player: " + ball.MyPlayer.name + " and their distance to the hole: " + ball.MyPlayer.DistanceToHole.ToString());

        // Prompt player about their distance to the hole
        //await ball.MyPlayer.ServerTellPlayerHowFarTheyAreFromHoleForChallenege(5, ball.MyPlayer.DistanceToHole);

        // Get new cancellation token for tasks?
        CancellationToken token = _cancellationTokenSource.Token;

        await ball.MyPlayer.ServerSendMessagetoPlayer(5f, "DistanceFromHoleOnChallenege", token, ball.MyPlayer.DistanceToHole);

        // check to see if there are any players remaining to take their challenege shot. If they have all taken their shot, then calculate player order and move on to the game...
        if (GolfPlayersInTeeOffOrder.Count <= 0)
        {
            Debug.Log("NextPlayerForChallenege: All players have teed off. Will calculate who is furthest away from the hole beep boop.");
            _playerOrderFromChallenege.AddRange(OrderPlayersFromTeeOffChallenge());
            // Tell players who will hit first
            await _playerOrderFromChallenege[0].ServerSendMessagetoPlayer(5f, "PlayerClosest", token);
            //DebugOutPlayerOrder();
            ClearPlayerWeatherFavorForFirstHole();
            ResetPlayerScoresForNewHole();
            RewardTeeOffChallenegeWinnersWithFavor();
            this.IsTeeOffChallenge = false;
            SwitchToCourse();
            return;
        }

        // remember to change weather for each player
        WindManager.instance.SetInitialWindDirection();
        WindManager.instance.SetRandomWindPowerForChallenegeShot();

        CurrentPlayer = SelectNextPlayer();
        SetCurrentPlayer(CurrentPlayer);
        MovePlayerToTeeOffLocation(CurrentPlayer);
        EnableAndDisablePlayerSpritesForNewTurn(CurrentPlayer);
        SetCameraOnPlayer(CurrentPlayer);
        PromptPlayerForNextTurn();
    }
    List <GolfPlayerTopDown> OrderPlayersFromTeeOffChallenge()
    {
        return GolfPlayers.OrderBy(x => x.DistanceToHole).ThenBy(x => x.FavorWeather).ToList();
    }
    void DebugOutPlayerOrder()
    {
        for (int i = 0; i < _playerOrderFromChallenege.Count; i++)
        {
            Debug.Log("DebugOutPlayerOrder: Player " + _playerOrderFromChallenege[i].PlayerName + " in order: " + i.ToString() + " had a distance from hole of: " + _playerOrderFromChallenege[i].DistanceToHole.ToString());
        }
    }
    void RewardTeeOffChallenegeWinnersWithFavor()
    {
        float closestDistance = _playerOrderFromChallenege[0].DistanceToHole;
        for (int i = 0; i < _playerOrderFromChallenege.Count; i++)
        {
            if (i == 0)
            {
                _playerOrderFromChallenege[i].RewardPlayerForWinningChallenege();
                continue;
            }

            if(_playerOrderFromChallenege[i].DistanceToHole <= closestDistance)
                _playerOrderFromChallenege[i].RewardPlayerForWinningChallenege();
        }

    }
    void SwitchToCourse()
    {
        CurrentCourse = CourseToPlay;
        CurrentHoleIndex = -1;
        WindManager.instance.SetInitialWindForNewHole();
        NextHole();
    }
    #endregion
}
