using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUIMessage : MonoBehaviour
{
    [SerializeField] GolfPlayerScore _golfPlayerScore;
    [SerializeField] TextMeshProUGUI _playerMessageText;
    [Header("Player UI Message Texts")]
    [SerializeField] const string _startTurn = "Press Space to Start Turn";
    [SerializeField] const string _water = "Water...\n+1 Stroke Penalty";
    [SerializeField] const string _outOfBounds = "Out of bounds...\n+1 Stroke Penalty";

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
            _playerMessageText.text = _startTurn;
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
}
