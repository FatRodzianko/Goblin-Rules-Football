using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Team : NetworkBehaviour
{
    [Header("Team Info")]
    [SyncVar] public bool isGrey = false;
    public List<GamePlayer> teamPlayers = new List<GamePlayer>();
    

    [Header("Team Stats")]
    [SyncVar] public int punchesThrown;
    [SyncVar] public int punchesHit;
    [SyncVar] public int slideTackles;
    [SyncVar] public int slideTacklesHit;
    [SyncVar] public int kicksDownfield;
    [SyncVar] public int passesThrown;
    [SyncVar] public int powerUpsCollected;
    [SyncVar] public int powerUpsUsed;
    [SyncVar] public int cowboysYeehawed;
    [SyncVar] public int timesKnockedOut;
    [SyncVar] public int timesTripped;
    [SyncVar] public int touchdownsScored;
    [SyncVar] public int kickAfterAttempts;
    [SyncVar] public int kickAfterAttemptsMade;
    [SyncVar] public int kicksBlocked;
    [SyncVar] public int fumbles;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [ServerCallback]
    public void AddPlayerToTeam(GamePlayer player)
    {
        if (player != null && !teamPlayers.Contains(player))
        {
            teamPlayers.Add(player);
        }
    }
}
