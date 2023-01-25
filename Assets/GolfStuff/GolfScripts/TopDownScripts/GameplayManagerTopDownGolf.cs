using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using System.Globalization;

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
    

    [Header("Tilemap Manager References")]
    [SerializeField] TileMapManager _tileMapManager;

    [Header("Player Info")]
    [SerializeField] public List<GolfPlayerTopDown> GolfPlayers = new List<GolfPlayerTopDown>();
    public List<GolfPlayerTopDown> GolfPlayersInTeeOffOrder = new List<GolfPlayerTopDown>();
    [SerializeField] int _numberOfPlayersTeedOff = 0;
    [SerializeField] int _numberOfPlayersInHole = 0;
    [SerializeField] float _lastBallInHoleTime = 0f;
    bool _haveAllPlayersTeedOff = false;
    public GolfPlayerTopDown CurrentPlayer;

    [Header("Camera and UI Stuff?")]
    [SerializeField] PolygonCollider2D _cameraBoundingBox;
    [SerializeField] TextMeshProUGUI _holeNumberText;
    [SerializeField] TextMeshProUGUI _parText;
    [SerializeField] TextMeshProUGUI _playerNameText;
    [SerializeField] TextMeshProUGUI _numberOfStrokesText;
    [SerializeField] TextMeshProUGUI _terrainTypeText;
    TextInfo titleCase = new CultureInfo("en-US", false).TextInfo;

    private void Awake()
    {
        MakeInstance();
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
        // Set the Hole Positions for the new hole
        HolePositions = CurrentHoleInCourse.HolePositions;
        // Set the Course aim points for players to use
        TeeOffAimPoint = CurrentHoleInCourse.TeeOffAimPoint;
        // Set the new tee off location
        UpdateTeeOffPositionForNewHole(CurrentHoleInCourse.TeeOffLocation);
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
        UpdateUIForCurrentPlayer(CurrentPlayer);
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
    }
    void LoadNewHole(int index)
    {
        // make sure there is a hole to load? Shouldn't happen unless game is over...
        if (index > CurrentCourse.HolesInCourse.Length)
            return;

        // First, clear the tilemap of any pre-existing hole tiles
        _tileMapManager.ClearMap();
        // load the next map in the course
        CurrentHoleIndex = index;
        CurrentHoleInCourse = CurrentCourse.HolesInCourse[CurrentHoleIndex];
        _tileMapManager.LoadMap(CurrentHoleInCourse);
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
            GolfPlayersInTeeOffOrder = GolfPlayers;
        else
        {
            GolfPlayersInTeeOffOrder = GolfPlayers.OrderByDescending(x => x.PlayerScore).ToList();
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
    }
    public void PlayerTeedOff()
    {
        _numberOfPlayersTeedOff++;
    }
    public void StartCurrentPlayersTurn(GolfPlayerTopDown requestingPlayer)
    {
        if (requestingPlayer != CurrentPlayer)
            return;
        CurrentPlayer.StartPlayerTurn();
    }
    void SetCameraOnPlayer(GolfPlayerTopDown player)
    {
        player.SetCameraOnPlayer();
    }
    public async void StartNextPlayersTurn(GolfBallTopDown ball)
    {
        Debug.Log("GameplayManager: StartNextPlayersTurn: executing...");

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
        if (!ball.IsInHole)
        {  
            Debug.Log("StartNextPlayersTurn: Calling TellPlayerGroundTheyLandedOn at time: " + Time.time.ToString());
            await ball.MyPlayer.TellPlayerGroundTheyLandedOn(3);
            Debug.Log("StartNextPlayersTurn: Returning from TellPlayerGroundTheyLandedOn at time: " + Time.time.ToString());
        }
        
        // Set the new wind for the next turn
        WindManager.instance.UpdateWindForNewTurn();
        WindManager.instance.UpdateWindDirectionForNewTurn();
        // Set new weather for the next turn
        RainManager.instance.UpdateWeatherForNewTurn();
        // Check if any players still have to hit a ball
        if (_numberOfPlayersInHole >= GolfPlayers.Count)
        {
            // should put transition to load next hole here. just returning for now
            Debug.Log("GameplayManager: StartNextPlayersTurn: All players have made it into the hole. No more remaining players! Loading next hole?");
            return;
        }
        // Find the next player based on tee off position, or by furthest player from hole if all players teed off
        CurrentPlayer = SelectNextPlayer();
        // Prompt player to start their turn
        CurrentPlayer.PlayerUIMessage("start turn");
        CurrentPlayer.EnablePlayerCanvas(true);
        UpdateUIForCurrentPlayer(CurrentPlayer);
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
                return GolfPlayersInTeeOffOrder[_numberOfPlayersTeedOff];
            }
        }

        return FindPlayerFurthestFromHole();
    }
    bool HaveAllPlayersTeedOff()
    {
        if (_numberOfPlayersTeedOff < GolfPlayersInTeeOffOrder.Count)
            return false;
        else
        {
            _haveAllPlayersTeedOff = true;
            return true;
        }
    }
    GolfPlayerTopDown FindPlayerFurthestFromHole()
    {
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
                currentClosestPlayer = player;
        }
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
    void UpdateUIForCurrentPlayer(GolfPlayerTopDown player)
    {
        if (!player)
            return;
        _playerNameText.text = "Player: " + player.PlayerName;
        _numberOfStrokesText.text = "Strokes: " + player.PlayerScore.StrokesForCurrentHole.ToString();
        _terrainTypeText.text = titleCase.ToTitleCase(player.GetTerrainTypeFromBall());
    }
}
