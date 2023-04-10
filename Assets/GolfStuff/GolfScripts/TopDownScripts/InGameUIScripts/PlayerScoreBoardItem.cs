using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScoreBoardItem : MonoBehaviour
{
    [SerializeField] public string PlayerName;
    [SerializeField] public int PlayerConnectionId;
    [SerializeField] public int PlayerScore;
    [SerializeField] private GolfPlayerScore _playerScoreScript;
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
        this._playerScoreScript = playerScore;
    }
}
