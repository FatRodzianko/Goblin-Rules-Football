using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    

    [Header("Tilemap Manager References")]
    [SerializeField] TileMapManager _tileMapManager;

    [Header("Player Info")]
    [SerializeField] public List<GolfPlayerTopDown> GolfPlayers = new List<GolfPlayerTopDown>();
    public List<GolfPlayerTopDown> GolfPlayersInTeeOffOrder = new List<GolfPlayerTopDown>();
    [SerializeField] int _numberOfPlayersTeedOff = 0;
    public GolfPlayerTopDown CurrentPlayer;

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
        // Set the new tee off location
        UpdateTeeOffPositionForNewHole(CurrentHoleInCourse.TeeOffLocation);
        // update the par value for the hole
        UpdateParForNewHole(CurrentHoleInCourse.HolePar);
        // Sort the players by lowest score to start the hole
        OrderListOfPlayers();
        // Move the first player to the tee off location
        MovePlayerToTeeOffLocation(GolfPlayersInTeeOffOrder[0]);
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
    void MovePlayerToTeeOffLocation(GolfPlayerTopDown player)
    {
        player.transform.position = TeeOffPosition;
        player.MyBall.transform.position = TeeOffPosition;
    }
    public void PlayerTeedOff()
    {
        _numberOfPlayersTeedOff++;
    }
}
