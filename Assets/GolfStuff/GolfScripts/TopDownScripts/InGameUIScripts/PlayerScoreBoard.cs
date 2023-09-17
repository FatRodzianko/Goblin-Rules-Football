using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class PlayerScoreBoard : MonoBehaviour
{
    public static PlayerScoreBoard instance;

    [SerializeField] ScriptableCourse _currentCourse;
    public bool IsScoreBoardOpen = false;

    [Header("UI Components")]
    [SerializeField] GameObject _scoreBoardCanvas;
    [SerializeField] GameObject _scoreBoardOutlinePanel;
    [SerializeField] PlayerScoreBoardItem _holeInfoPanel;
    [SerializeField] PlayerScoreBoardItem _parInfoPanel;

    [Header("Player Score Items")]
    [SerializeField] GameObject _scoreBoardItemPrefab;
    public List<PlayerScoreBoardItem> PlayerScoreItems = new List<PlayerScoreBoardItem>();

    private void Awake()
    {
        MakeInstance();
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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        
    }
    public void CreateHoleInfoAndParInfoPanels(ScriptableCourse course)
    {
        _currentCourse = course;

        GameObject holeInfoPanelObject = Instantiate(_scoreBoardItemPrefab, _scoreBoardOutlinePanel.transform);
        _holeInfoPanel = holeInfoPanelObject.GetComponent<PlayerScoreBoardItem>();
        _holeInfoPanel.CreateHoleInfoPanel(course);

        GameObject parInfoPanelObject = Instantiate(_scoreBoardItemPrefab, _scoreBoardOutlinePanel.transform);
        _parInfoPanel = parInfoPanelObject.GetComponent<PlayerScoreBoardItem>();
        _parInfoPanel.CreateParInfoPanel(course);
    }
    public void AddPlayerToScoreBoard(GolfPlayerTopDown player, GolfPlayerScore playerScore)
    {
        if (!player)
            return;
        if (!playerScore)
            return;

        // Don't add a player to the scoreboard if they are already in the list!
        if (PlayerScoreItems.Any(x => x.PlayerConnectionId == player.ConnectionId))
            return;

        GameObject newPlayerScoreBordItem = Instantiate(_scoreBoardItemPrefab, _scoreBoardOutlinePanel.transform);
        PlayerScoreBoardItem newPlayerScoreBordItemScript = newPlayerScoreBordItem.GetComponent<PlayerScoreBoardItem>();
        newPlayerScoreBordItemScript.PlayerName = player.PlayerName;
        newPlayerScoreBordItemScript.PlayerColor = player.BallColor;
        newPlayerScoreBordItemScript.PlayerConnectionId = player.ConnectionId;
        newPlayerScoreBordItemScript.GetPlayerScoreScript(playerScore);
        newPlayerScoreBordItemScript.InitializePlayerItem(_currentCourse.HolesInCourse.Length);
        this.PlayerScoreItems.Add(newPlayerScoreBordItemScript);
    }
    void SortScoreBoardList()
    {
        PlayerScoreItems = PlayerScoreItems.OrderBy(x => x.PlayerScoreScript.TotalStrokesForCourse).ToList();
    }
    public void UpdatePlayerScoreBoardItemForNewStroke(GolfPlayerTopDown player, int strokes)
    {
        if (!player)
            return;
        if (strokes == 0)
            return;

        PlayerScoreBoardItem itemToUpdate = PlayerScoreItems.FirstOrDefault(x => x.PlayerConnectionId == player.ConnectionId);
        //int holeIndex = GameplayManagerTopDownGolf.instance.CurrentHoleIndex + 1;
        itemToUpdate.UpdateStrokesForCurrentHole(GameplayManagerTopDownGolf.instance.CurrentHoleIndex, strokes);
        itemToUpdate.UpdatePlayerFavor(player.FavorWeather);
        //itemToUpdate.PlayerScoresPerHole[holeIndex] = strokes;
    }
    public void UpdatePlayerScoreBoardItemScoreForCourse(GolfPlayerTopDown player, int strokes)
    {
        if (!player)
            return;
        if (strokes == 0)
            return;

        PlayerScoreBoardItem itemToUpdate = PlayerScoreItems.FirstOrDefault(x => x.PlayerConnectionId == player.ConnectionId);
        itemToUpdate.UpdateTotalStrokesForCourse(strokes);
        itemToUpdate.UpdatePlayerFavor(player.FavorWeather);
        //itemToUpdate.PlayerScoreForCourse = strokes;

        // Sort the list after a new score for the course is received?
        SortScoreBoardList();
    }
    public void UpdatePlayerScoreBoardItemPlayerFavor(GolfPlayerTopDown player)
    {
        Debug.Log("UpdatePlayerScoreBoardItemPlayerFavor: for player: " + player.PlayerName);
        PlayerScoreBoardItem itemToUpdate = PlayerScoreItems.FirstOrDefault(x => x.PlayerConnectionId == player.ConnectionId);
        itemToUpdate.UpdatePlayerFavor(player.FavorWeather);
    }
    public void UpdateCourseScore(GolfPlayerTopDown player, Dictionary<int, int> courseScoresPerHole)
    {

    }
    public void OpenScoreBoard()
    {
        SortScoreBoardList();
        OrderObjectsInHierarchy();
        _scoreBoardCanvas.SetActive(true);
        IsScoreBoardOpen = true;
    }
    public void CloseScoreBoard()
    {
        _scoreBoardCanvas.SetActive(false);
        IsScoreBoardOpen = false;
    }
    void OrderObjectsInHierarchy()
    {
        _holeInfoPanel.transform.SetAsFirstSibling();
        _parInfoPanel.transform.SetSiblingIndex(1);
        for (int i = 0; i < PlayerScoreItems.Count; i++)
        {
            PlayerScoreItems[i].transform.SetSiblingIndex(i + 2);
        }
    }
}
