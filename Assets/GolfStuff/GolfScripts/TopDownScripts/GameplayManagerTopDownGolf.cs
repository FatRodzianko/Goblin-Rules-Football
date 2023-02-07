using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using System.Globalization;
using System.Threading.Tasks;

public class GameplayManagerTopDownGolf : MonoBehaviour
{
    public static GameplayManagerTopDownGolf instance;

    [Header("Course Information")]
    [SerializeField] public ScriptableCourse CurrentCourse;
    [SerializeField] public ScriptableHole CurrentHoleInCourse;
    [SerializeField] public int CurrentHoleIndex;

    [Header("Current Hole Information")]
    public Vector3 TeeOffPosition;
    public string CurrentHoleName;
    public int CurrentHolePar;
    public List<Vector3> HolePositions;
    public Vector3 TeeOffAimPoint;
    [SerializeField] float _lastNewHoleTime = 0f;


    [Header("Tilemap Manager References")]
    [SerializeField] TileMapManager _tileMapManager;

    [Header("Player Info")]
    [SerializeField] public List<GolfPlayerTopDown> GolfPlayers = new List<GolfPlayerTopDown>();
    public List<GolfPlayerTopDown> GolfPlayersInTeeOffOrder = new List<GolfPlayerTopDown>();
    [SerializeField] int _numberOfPlayersTeedOff = 0;
    [SerializeField] int _teeOffOrder = 0;
    [SerializeField] int _numberOfPlayersInHole = 0;
    [SerializeField] float _lastBallInHoleTime = 0f;
    [SerializeField] bool _haveAllPlayersTeedOff = false;
    public GolfPlayerTopDown CurrentPlayer;

    [Header("Weather Effects")]
    [SerializeField] LightningManager _lightningManager;

    [Header("Camera and UI Stuff?")]
    [SerializeField] CameraViewHole _cameraViewHole;
    [SerializeField] PolygonCollider2D _cameraBoundingBox;
    [SerializeField] TextMeshProUGUI _holeNumberText;
    [SerializeField] TextMeshProUGUI _parText;
    [SerializeField] TextMeshProUGUI _playerNameText;
    [SerializeField] TextMeshProUGUI _numberOfStrokesText;
    [SerializeField] TextMeshProUGUI _terrainTypeText;
    TextInfo titleCase = new CultureInfo("en-US", false).TextInfo;
    [SerializeField] Canvas _loadingHoleCanvas;
    [SerializeField] Canvas _holeInfoCanvas;

    [Header("Skip For Lightning Info")]
    [SerializeField] public bool PlayerHasSkippedTurn = false;
    [SerializeField] public List<GolfPlayerTopDown> TurnOrderForLightningSkips = new List<GolfPlayerTopDown>();
    [SerializeField] public int TurnsSinceSkip = 0;

    private void Awake()
    {
        MakeInstance();
        _loadingHoleCanvas.gameObject.SetActive(false);
        if (!_tileMapManager)
            _tileMapManager = GameObject.FindGameObjectWithTag("TileMapManager").GetComponent<TileMapManager>();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
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
        // Sort the players by lowest score to start the hole
        OrderListOfPlayers();
        // Set the current player
        SetCurrentPlayer(GolfPlayersInTeeOffOrder[0]);
        // Move the first player to the tee off location
        MovePlayerToTeeOffLocation(CurrentPlayer);
        // Set the camera on the current player
        SetCameraOnPlayer(CurrentPlayer);
        // Prompt player to start their turn
        CurrentPlayer.PlayerUIMessage("start turn");
        CurrentPlayer.EnablePlayerCanvas(true);
        UpdateUIForCurrentPlayer(CurrentPlayer,true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
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
        CurrentHoleIndex = index;
        CurrentHoleInCourse = CurrentCourse.HolesInCourse[CurrentHoleIndex];
        //_tileMapManager.LoadMap(CurrentHoleInCourse);
        Debug.Log("GameplayManagertopDownGolf: LoadNewHole: Starting task to load hole at index: " + CurrentHoleInCourse.ToString() + ". Time: " + Time.time);
        _holeInfoCanvas.gameObject.SetActive(false);
        _loadingHoleCanvas.gameObject.SetActive(true);
        await _tileMapManager.LoadMapAsTask(CurrentHoleInCourse);
        Debug.Log("GameplayManagertopDownGolf: LoadNewHole: Task to load hole at index: " + CurrentHoleInCourse.ToString() + " completed. Time: " + Time.time);
        _loadingHoleCanvas.gameObject.SetActive(false);
        _holeInfoCanvas.gameObject.SetActive(true);
        _holeNumberText.text = CurrentHoleInCourse.CourseName + " #" + (CurrentHoleIndex + 1).ToString();
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
            GolfPlayersInTeeOffOrder.AddRange(GolfPlayers);
        }   
        else
        {
            GolfPlayersInTeeOffOrder.Clear();
            GolfPlayersInTeeOffOrder.AddRange(GolfPlayers.OrderByDescending(x => x.PlayerScore.TotalStrokesForCourse).ToList());
        }
    }
    void SetCurrentPlayer(GolfPlayerTopDown player)
    {
        // this check probably isn't needed unless I have some sort of event tied to the current player changing and don't want to trigger it multiple times?
        //if (CurrentPlayer == player)
        //    return;
        CurrentPlayer = player;
    }
    void MovePlayerToTeeOffLocation(GolfPlayerTopDown player)
    {
        player.transform.position = TeeOffPosition;
        player.MyBall.transform.position = TeeOffPosition;
        player.MyBall.ResetBallSpriteForNewHole();
        
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
    public void StartCurrentPlayersTurn(GolfPlayerTopDown requestingPlayer)
    {
        if (requestingPlayer != CurrentPlayer)
            return;
        UpdateUIForCurrentPlayer(requestingPlayer);
        CurrentPlayer.StartPlayerTurn();
    }
    void SetCameraOnPlayer(GolfPlayerTopDown player)
    {
        player.SetCameraOnPlayer();
    }
    public async void StartNextPlayersTurn(GolfBallTopDown ball, bool playerSkippingForLightning = false)
    {
        Debug.Log("GameplayManager: StartNextPlayersTurn: executing... playerSkippingForLightning: " + playerSkippingForLightning.ToString());

        // Check if the ball is out-of-bounds. If so, move ball back in bounds before selecting next player
        /*GetCameraBoundingBox();
        if (_cameraBoundingBox)
        {
            if (!_cameraBoundingBox.OverlapPoint(ball.transform.position))
            {
                Debug.Log("GameplayManager: StartNextPlayersTurn: Ball is NOT in bounds. Moving the ball for the ball");
                //ball.OutOfBounds();
                await ball.MyPlayer.TellPlayerBallIsOutOfBounds(3);
                //return;
            }
        }*/
        // Check if the ball is in water. If so, move ball out of water before selecting next player
        /*if (ball.bounceContactGroundMaterial.Contains("water"))
        {
            Debug.Log("GameplayManager: StartNextPlayersTurn: ball landed in water");
            ball.BallEndedInWater();
            return;

        }*/
        if (ball.IsInHole)
        {
            Debug.Log("GameplayManager: StartNextPlayersTurn: the ball is in the hole! Congrats to player: " + ball.MyPlayer.PlayerName);
            // This only needs to happen here for the single player right now, I think?
            UpdateUIForCurrentPlayer(ball.MyPlayer);
            //return;
        }
        
        //TellPlayerGroundBallIsOn(ball);
        if (!ball.IsInHole && !playerSkippingForLightning)
        {  
            Debug.Log("StartNextPlayersTurn: Calling TellPlayerGroundTheyLandedOn at time: " + Time.time.ToString());
            await ball.MyPlayer.TellPlayerGroundTheyLandedOn(3);
            Debug.Log("StartNextPlayersTurn: Returning from TellPlayerGroundTheyLandedOn at time: " + Time.time.ToString());
        }
        
        
        // Check if any players still have to hit a ball
        if (_numberOfPlayersInHole >= GolfPlayers.Count)
        {

            if ((CurrentHoleIndex + 1) < CurrentCourse.HolesInCourse.Length)
            {
                Debug.Log("GameplayManager: StartNextPlayersTurn: All players have made it into the hole. No more remaining players! Loading next hole?");
                await ball.MyPlayer.TellPlayerHoleEnded(3);
                NextHole();
            }
            else
            {
                Debug.Log("GameplayManager: StartNextPlayersTurn: All players have made it into the hole. No more remaining players! And this was the last hole! Ending the game...");
                EndGame();
            }
            return;
        }
        // Set the weather for the next turn
        SetWeatherForNextTurn();
        // Find the next player based on tee off position, or by furthest player from hole if all players teed off
        if (playerSkippingForLightning)
        {
            Debug.Log("GameplayManagerTopDownGolf: StartNextPlayersTurn: Player: " + ball.MyPlayer.PlayerName + " skipping for next turn due to lightning.");
            // Skip the current player's turn. Find the next player up based on tee off order, or distance to hole. If no other players are left, then this player stays as current player
            if (_numberOfPlayersInHole == GolfPlayers.Count - 1)
            {
                // If the only player who hasn't made it in the hole is this player, they stay as the current player
                Debug.Log("StartNextPlayersTurn: Skip For Lighning next player will be: Player: " + ball.MyPlayer.PlayerName + " because they are the only player not in the hole.");
                CurrentPlayer = ball.MyPlayer;
            }
            else
            {
                // If players still have not teed off, remove this player from GolfPlayersInTeeOffOrder, then add them back at the end of the list
                /*if (!_haveAllPlayersTeedOff)
                {
                    if (!HaveAllPlayersTeedOff())
                    {
                        MovePlayerToBackOfTeeOffOrder(ball.MyPlayer);
                        CurrentPlayer = GolfPlayersInTeeOffOrder[_numberOfPlayersTeedOff];
                    }
                }
                else
                { 
                    // If all players have teed off, find the next furthest away player

                    // Once a player has skipped a turn, create an ordered list of players by their distance to the hole. Add the player who just skipped to the end of the list. Everyone takes their turn based on that list until all players have had a turn to either hit or skip turn. Once all players either choose skip/hit and the list makes it back to the first player who skipped, then reset a "player has skipped turn" value and calculate next player normally based on just distance
                    // all of the above is just to make sure that if a player skips their turn, and then the next player goes, it doesn't just go back to the player who skipped (who will still be the furthest away from the hole), penalizing them continuously for their turn taken place near a lightning storm
                }*/

                if (PlayerHasSkippedTurn)
                {
                    /*Debug.Log("StartNextPlayersTurn: Player: " + ball.MyPlayer.PlayerName + " has skipped for lightning. They are not the first to do so.");
                    if (TurnOrderForLightningSkips.Contains(ball.MyPlayer))
                        TurnOrderForLightningSkips.Remove(ball.MyPlayer);
                    if (TurnOrderForLightningSkips.Count > 0)
                    {
                        CurrentPlayer = TurnOrderForLightningSkips[0];
                        Debug.Log("StartNextPlayersTurn: " + TurnOrderForLightningSkips.Count.ToString() + " players remaining in TurnOrderForLightningSkips. Setting player: " + CurrentPlayer.PlayerName + " as the next player");
                    }
                    else
                    {
                        ResetLightningSkipInfo();
                        CurrentPlayer = SelectNextPlayer();
                        Debug.Log("StartNextPlayersTurn: TurnOrderForLightningSkips should be zero. It is: " + TurnOrderForLightningSkips.Count.ToString() + ". Resetting info and setting current player as normally would. Current Player is: " + CurrentPlayer.PlayerName);
                    }*/
                    GetNextPlayerFromLightningSkipList(ball.MyPlayer);

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
                    TurnOrderForLightningSkips.Add(ball.MyPlayer);

                    CurrentPlayer = TurnOrderForLightningSkips[0];
                }
            }
        }
        else if (PlayerHasSkippedTurn)
        {
            Debug.Log("StartNextPlayersTurn: Player did not skip for lightning but PlayerHasSkippedTurn was true before their turn.");
            GetNextPlayerFromLightningSkipList(ball.MyPlayer);
        }
        else
        {
            CurrentPlayer = SelectNextPlayer();
        }
        // Move the player too the tee off location if they haven't teed off yet
        if(!CurrentPlayer.HasPlayerTeedOff)
            MovePlayerToTeeOffLocation(CurrentPlayer);
        // Prompt player to start their turn
        PromptPlayerForNextTurn();
    }
    /*async void TellPlayerGroundBallIsOn(GolfBallTopDown ball)
    {
        await ball.MyPlayer.TellPlayerGroundTheyLandedOn(3);
    }*/
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
    void GetCameraBoundingBox()
    {
        if (!_cameraBoundingBox)
            _cameraBoundingBox = GameObject.FindGameObjectWithTag("CameraBoundingBox").GetComponent<PolygonCollider2D>();
        if (_cameraBoundingBox)
            _cameraViewHole.GetLinePointsForOutOfBoundsBorder(_cameraBoundingBox);
    }
    void TellPlayersToGetCameraBoundingBox()
    {
        foreach (GolfPlayerTopDown player in GolfPlayers)
        {
            player.GetCameraBoundingBox();
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
    }
    void UpdateZoomedOutPos(Vector3 newPos)
    {
        if (!_cameraViewHole)
            _cameraViewHole = GameObject.FindGameObjectWithTag("camera").GetComponent<CameraViewHole>();
        _cameraViewHole.SetZoomedOutPosition(newPos);

    }
    void NextHole()
    {
        Debug.Log("Next hole: " + Time.time);
        if (Time.time < _lastNewHoleTime + 0.25f)
            return;
        _lastNewHoleTime = Time.time;
        CurrentHoleIndex++;
        _numberOfPlayersInHole = 0;
        // Save each player's score and reset their "current" score of the new hole
        foreach (GolfPlayerTopDown player in GolfPlayers)
        {   
            player.ResetForNewHole(CurrentHoleIndex);
        }
        // Reset the number of players that have teed off
        ResetNumberOfPlayersWhoHaveTeedOff();
        // Load a new hole
        LoadNewHole(CurrentHoleIndex);
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
        // Set the weather for the next turn
        SetWeatherForNextTurn();
        // Sort the players by lowest score to start the hole
        OrderListOfPlayers();
        // Set the current player
        SetCurrentPlayer(GolfPlayersInTeeOffOrder[0]);
        // Move the first player to the tee off location
        MovePlayerToTeeOffLocation(CurrentPlayer);
        // Set the camera on the current player
        SetCameraOnPlayer(CurrentPlayer);
        // Prompt player to start their turn
        PromptPlayerForNextTurn();
    }
    void SetWeatherForNextTurn()
    {
        // Set the new wind for the next turn
        WindManager.instance.UpdateWindForNewTurn();
        WindManager.instance.UpdateWindDirectionForNewTurn();
        // Set new weather for the next turn
        RainManager.instance.UpdateWeatherForNewTurn();
        _lightningManager.CheckIfLightningStartsThisTurn();
    }
    void PromptPlayerForNextTurn()
    {
        if (_lightningManager.IsThereLightning)
        {
            CurrentPlayer.PlayerUIMessage("lightning");
        }
        else
        {
            CurrentPlayer.PlayerUIMessage("start turn");
        }
        CurrentPlayer.EnablePlayerCanvas(true);
        UpdateUIForCurrentPlayer(CurrentPlayer);
    }
    async void EndGame()
    {
        var tasks = new Task[GolfPlayers.Count];
        Debug.Log("EndGame: Before calling TellPlayerGameIsOver time is: " + Time.time.ToString());
        // Save each player's score and reset their "current" score of the new hole
        for (int i = 0; i < GolfPlayers.Count; i++)
        {
            GolfPlayerTopDown player = GolfPlayers[i];
            player.ResetForNewHole(CurrentHoleIndex + 1);
            tasks[i] = player.TellPlayerGameIsOver(5);
        }

        await Task.WhenAll(tasks);
        Debug.Log("EndGame: AFTER calling TellPlayerGameIsOver time is: " + Time.time.ToString());

    }
    public void LightningForPlayerHit()
    {
        _lightningManager.LightningForHit();
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
            player.SetDistanceToHoleForPlayer();
            playersByDistance.Add(player);
        }

        playersByDistance = playersByDistance.OrderByDescending(x => x.DistanceToHole).ToList();
        return playersByDistance;
    }
    void GetNextPlayerFromLightningSkipList(GolfPlayerTopDown playerToRemove)
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
}
