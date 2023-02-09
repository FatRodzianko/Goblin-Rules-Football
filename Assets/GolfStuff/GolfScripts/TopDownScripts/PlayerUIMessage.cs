using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUIMessage : MonoBehaviour
{
    [SerializeField] GolfPlayerTopDown _myPlayer;
    [SerializeField] GolfPlayerScore _golfPlayerScore;
    [SerializeField] TextMeshProUGUI _playerMessageText;
    [Header("Player UI Message Texts")]
    [SerializeField] const string _startTurn = "Press Space to Start Turn";
    [SerializeField] const string _water = "Water...\n+1 Stroke Penalty";
    [SerializeField] const string _outOfBounds = "Out of bounds...\n+1 Stroke Penalty";
    [SerializeField] const string _lightningOnTurn = "Lightning in area.\n\nPress Backspace to skip 1 turn with +1 penalty.\n\nPress Space to start turn now.";
    [SerializeField] const string _struckByLightning = "You've been struck by lightning!\n\n+10 stroke penalty and you're out of commission until next turn!\n\nSpace to continue...";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdatePlayerMessageText(string message)
    {
        if (message == "start turn")
        {
            _playerMessageText.text = _myPlayer.PlayerName + " " + _startTurn;
        }
        else if (message == "water")
        {
            _playerMessageText.text = _water;
        }
        else if (message == "out of bounds")
        {
            _playerMessageText.text = _outOfBounds;
        }
        else if (message == "ball in hole")
        {
            _playerMessageText.text = BallInHoleMessage();
        }
        else if (message.Contains("ground:"))
        {
            DisplayGroundType(message);
        }
        else if (message == "lightning")
        {
            LightningOnTurn();
        }
        else if (message == "hole ended")
        {
            HoleEnded();
        }
        else if (message == "game over")
        {
            GameOverMessage();
        }
        else if (message == "struck by lightning")
        {
            _playerMessageText.text = _myPlayer.PlayerName + " " + _struckByLightning;
        }
        else
        {
            _playerMessageText.text = "";
        }
    }
    string BallInHoleMessage()
    {
        string ballInHoleMessage = "Strokes: ";

        int playersScore = _golfPlayerScore.StrokesForCurrentHole;
        int holePar = GameplayManagerTopDownGolf.instance.CurrentHolePar;
        ballInHoleMessage += playersScore.ToString() + "\n";

        if(playersScore == 1)
            ballInHoleMessage += "HOLE IN ONE!!!!!!!!!";
        else if (playersScore == holePar)
            ballInHoleMessage += "That's Par! Congrats?";
        else if (playersScore == holePar + 1)
            ballInHoleMessage += "Bogey. Yikes.";
        else if (playersScore == holePar + 2)
            ballInHoleMessage += "DOUBLE Bogey...";
        else if (playersScore > holePar + 2)
            ballInHoleMessage += "...super bogey...";
        else if (playersScore == holePar - 1)
            ballInHoleMessage += "Birdie! Chirp Chirp!";
        else if (playersScore == holePar - 2)
            ballInHoleMessage += "EAGLE!";
        else if (playersScore < holePar - 2)
            ballInHoleMessage += "ALBATROSS!";

        return ballInHoleMessage;
    }
    void DisplayGroundType(string groundType)
    {
        if (groundType.Contains("water"))
            return;
        if (groundType.Contains("green"))
        {
            _playerMessageText.text = "Green";
        }
        else if (groundType.Contains("fairway"))
        {
            _playerMessageText.text = "Fairway";
        }
        else if (groundType.Contains("deep rough"))
        {
            _playerMessageText.text = "DEEP rough...";
        }
        else if (groundType.Contains("rough"))
        {
            _playerMessageText.text = "rough...";
        }
        else if (groundType.Contains("trap"))
        {
            _playerMessageText.text = "...bunker...";
        }
    }
    void HoleEnded()
    {
        _playerMessageText.text = "Hole #" + (GameplayManagerTopDownGolf.instance.CurrentHoleIndex + 1).ToString() + " Ended. Moving onto the next hole...";
    }
    void GameOverMessage()
    {
        // get the par for the course
        int parForCourse = 0;
        for (int i = 0; i < GameplayManagerTopDownGolf.instance.CurrentCourse.HolesInCourse.Length; i++)
        {
            parForCourse += GameplayManagerTopDownGolf.instance.CurrentCourse.HolesInCourse[i].HolePar;
        }
        int overUnderPar = _golfPlayerScore.TotalStrokesForCourse - parForCourse;
        _playerMessageText.text = _myPlayer.PlayerName + " score for course: " + _golfPlayerScore.TotalStrokesForCourse.ToString() + "\nPar for course: " + parForCourse.ToString() + "\nOver/Under Par: " + overUnderPar.ToString();
    }
    void LightningOnTurn()
    {
        _playerMessageText.text = _myPlayer.PlayerName + " " + _lightningOnTurn;
        _myPlayer.LightningOnTurn();
    }
}
