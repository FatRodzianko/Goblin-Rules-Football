using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerScoreBoardItem : MonoBehaviour
{
    [SerializeField] public string PlayerName;
    [SerializeField] public int PlayerConnectionId;
    [SerializeField] public int PlayerScoreForCourse;
    [SerializeField] public Color PlayerColor = Color.white;
    [SerializeField] public Dictionary<int, int> PlayerScoresPerHole = new Dictionary<int, int>();
    [SerializeField] public GolfPlayerScore PlayerScoreScript;

    [Header("UI Components")]
    [SerializeField] TextMeshProUGUI _name;
    [SerializeField] TextMeshProUGUI _totalScore;
    [SerializeField] GameObject _scoreItemHolderPanel;
    [SerializeField] GameObject _scoreTextPrefab;
    [SerializeField] List<TextMeshProUGUI> _scoreItems = new List<TextMeshProUGUI>();

    // Start is called before the first frame update
    void Start()
    {
        // Create an event subscription to update PlayerScore any time the player's score changes?
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GetPlayerScoreScript(GolfPlayerScore playerScore)
    {
        this.PlayerScoreScript = playerScore;
    }
    public void CreateHoleInfoPanel(ScriptableCourse course)
    {
        _name.text = "Hole";
        _totalScore.text = "Total";

        for (int i = 0; i < course.HolesInCourse.Length; i++)
        {
            GameObject newScoreTextItem = Instantiate(_scoreTextPrefab, _scoreItemHolderPanel.transform);
            int holeNumber = i + 1;
            newScoreTextItem.GetComponent<TextMeshProUGUI>().text = holeNumber.ToString();
        }
    }
    public void CreateParInfoPanel(ScriptableCourse course)
    {
        _name.text = "Par";
        int par = 0;
        for (int i = 0; i < course.HolesInCourse.Length; i++)
        {
            int parText = course.HolesInCourse[i].HolePar;
            par += parText;

            GameObject newScoreTextItem = Instantiate(_scoreTextPrefab, _scoreItemHolderPanel.transform);
            newScoreTextItem.GetComponent<TextMeshProUGUI>().text = parText.ToString();
        }

        _totalScore.text = par.ToString();
    }
    public void InitializePlayerItem(int numberOfHoles)
    {
        _name.text = PlayerName;
        _name.color = PlayerColor;

        for (int i = 0; i < numberOfHoles; i++)
        {
            GameObject newScoreTextItem = Instantiate(_scoreTextPrefab, _scoreItemHolderPanel.transform);
            TextMeshProUGUI newScoreTextItemText = newScoreTextItem.GetComponent<TextMeshProUGUI>();
            newScoreTextItemText.text = 0.ToString();
            _scoreItems.Add(newScoreTextItemText);
        }
        _totalScore.text = "0";
    }
    public void UpdateStrokesForCurrentHole(int holeIndex, int newStrokes)
    {
        PlayerScoresPerHole[holeIndex] = newStrokes;
        _scoreItems[holeIndex].text = newStrokes.ToString();
    }
    public void UpdateTotalStrokesForCourse(int newStrokes)
    {
        PlayerScoreForCourse = newStrokes;
        _totalScore.text = newStrokes.ToString();
    }
}
