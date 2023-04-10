using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class PlayerScoreBoard : MonoBehaviour
{
    [SerializeField] GameObject _playerScoreBoardItemPrefab;

    [Header("Player Score Items")]
    public List<PlayerScoreBoardItem> PlayerScoreItems = new List<PlayerScoreBoardItem>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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

        GameObject newPlayerScoreBordItem = Instantiate(_playerScoreBoardItemPrefab, this.transform);
        PlayerScoreBoardItem newPlayerScoreBordItemScript = newPlayerScoreBordItem.GetComponent<PlayerScoreBoardItem>();
        newPlayerScoreBordItemScript.PlayerName = player.PlayerName;
        newPlayerScoreBordItemScript.PlayerConnectionId = player.ConnectionId;
        newPlayerScoreBordItemScript.GetPlayerScoreScript(playerScore);
        this.PlayerScoreItems.Add(newPlayerScoreBordItemScript);
    }
}
