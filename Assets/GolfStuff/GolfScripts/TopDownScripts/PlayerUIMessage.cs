using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerUIMessage : MonoBehaviour
{
    [SerializeField] GolfPlayerTopDown _myPlayer;
    [SerializeField] GolfPlayerScore _golfPlayerScore;
    [SerializeField] TextMeshProUGUI _playerMessageText;
    [Header("Player UI Message Texts")]
    //[SerializeField] const string _startTurn = "Press Space to Start Turn";
    [SerializeField] const string _water = "Water...\n+1 Stroke Penalty";
    [SerializeField] const string _outOfBounds = "Out of bounds...\n+1 Stroke Penalty";
    //[SerializeField] const string _lightningOnTurn = "Lightning in area.\n\nPress Backspace to skip 1 turn with +1 penalty.\n\nPress Space to start turn now.";
    //[SerializeField] const string _struckByLightning = "You've been struck by lightning!\n\n+10 stroke penalty and you're out of commission until next turn!\n\nSpace to continue...";
    [SerializeField] const string _otherPlayerStruckByLightning = " has been struck by lightning!\n\nThey receive a +10 stroke penalty and are out of commission until next turn!";
    [SerializeField] const string _stormPassed = "All players have agreed to let the storm pass until there is no more lightning.";
    //[SerializeField] const string _mulliganOwner = "You have a mulligan power up. Press \"p\" if you want to use it now. Press space to continue to next turn.\n";
    [SerializeField] const string _mulliganClients = " is deciding if they want to use their mulligan...";
    [SerializeField] const string _usingMulliganOwner = "Using mulligan. Setting the ball back to where you started...";
    [SerializeField] const string _usingMulliganClients = " is using their mulligan!";
    [SerializeField] const string _playerClosestToHole = " won the challenge and will hit first!";
    [SerializeField] const string _strokeLimitOwner = "You hit the stroke limit... Try to hit the ball in fewer than ";
    [SerializeField] const string _strokeLimitClient = " hit the stroke limit!";

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
            //_playerMessageText.text = _myPlayer.PlayerName + " " + _startTurn;
            

            _playerMessageText.text = _myPlayer.PlayerName + " Press " + GetActionBindingName(InputManagerGolf.Controls.PromptPlayer.Continue) + " to Start Turn";
        }
        else if (message == "water")
        {
            _playerMessageText.text = _myPlayer.PlayerName + ":\n" + _water;
        }
        else if (message == "out of bounds")
        {
            _playerMessageText.text = _myPlayer.PlayerName + ":\n" + _outOfBounds;
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
            if (_myPlayer.IsOwner)
                _playerMessageText.text = _myPlayer.PlayerName + " " + "You've been struck by lightning!\n\n+10 stroke penalty and you're out of commission until next turn!\n\n"+ GetActionBindingName(InputManagerGolf.Controls.PromptPlayer.Continue) +" to continue...";
            else
                _playerMessageText.text = _myPlayer.PlayerName + " " + _otherPlayerStruckByLightning;
        }
        else if (message == "storm passed")
        {
            _playerMessageText.text = _stormPassed;
        }
        else if (message.StartsWith("mulligan"))
        {
            if (_myPlayer.IsOwner)
            {
                string[] mulliganMessage = message.Split(" ");
                _playerMessageText.text = "You have a mulligan power up. Press \"" + GetActionBindingName(InputManagerGolf.Controls.PowerUps.UsePowerUp) + "\" if you want to use it now. Press " + GetActionBindingName(InputManagerGolf.Controls.PromptPlayer.Continue) + " to continue to next turn.\n" + mulliganMessage[1].ToString() + " seconds remaining...";
            }

            else
                _playerMessageText.text = _myPlayer.PlayerName + _mulliganClients;
        }
        else if (message == "UsingMulligan")
        {
            if (_myPlayer.IsOwner)
            {
                _playerMessageText.text = _usingMulliganOwner;
            }

            else
                _playerMessageText.text = _myPlayer.PlayerName + _usingMulliganClients;
        }
        else if (message.StartsWith("challenege"))
        {
            string[] challenegeMessage = message.Split(":");
            if (challenegeMessage[1].Contains("hole"))
            {
                _playerMessageText.text = _myPlayer.PlayerName + " made it in the hole! (that's a distance of 0f)";
            }
            else
            {
                _playerMessageText.text = _myPlayer.PlayerName + " is " + challenegeMessage[1] + " meters away from the hole.";
            }
        }
        else if (message == "PlayerClosest")
        {
            _playerMessageText.text = _myPlayer.PlayerName + _playerClosestToHole;
        }
        else if (message == "stroke limit")
        {
            if (_myPlayer.IsOwner)
            {
                _playerMessageText.text = _strokeLimitOwner + GameplayManagerTopDownGolf.instance.StrokeLimitNumber.ToString() + " strokes.";
            }
            else
            {
                _playerMessageText.text = _myPlayer.PlayerName + _strokeLimitClient;
            }
        }
        else
        {
            _playerMessageText.text = "";
        }
    }
    string BallInHoleMessage()
    {
        string ballInHoleMessage = _myPlayer.PlayerName + ":\n" + "Strokes: ";

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
        _playerMessageText.text = _myPlayer.PlayerName + ":\n";
        if (groundType.Contains("green"))
        {
            _playerMessageText.text += "Green";
        }
        else if (groundType.Contains("fairway"))
        {
            _playerMessageText.text += "Fairway";
        }
        else if (groundType.Contains("deep rough"))
        {
            _playerMessageText.text += "DEEP rough...";
        }
        else if (groundType.Contains("rough"))
        {
            _playerMessageText.text += "rough...";
        }
        else if (groundType.Contains("trap"))
        {
            _playerMessageText.text += "...bunker...";
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
        _playerMessageText.text = _myPlayer.PlayerName + " " + "Lightning in area.\n\nPress " + GetActionBindingName(InputManagerGolf.Controls.PromptPlayer.Skip) + " to skip 1 turn with +1 penalty.\n\nPress " + GetActionBindingName(InputManagerGolf.Controls.PromptPlayer.Continue) + " to start turn now.";
        _myPlayer.LightningOnTurn();
    }
    string GetActionBindingName(InputAction action)
    {
        string bindingGroup = "Keyboard and Mouse";
        if(GamepadUIManager.instance.gamepadUI)
            bindingGroup = "GamePad";

        return action.GetBindingDisplayString(0, bindingGroup);
    }
}
