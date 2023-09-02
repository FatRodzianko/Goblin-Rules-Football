using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet;

public class TNTScript : NetworkBehaviour
{
    [SerializeField] [SyncVar] public int PlayerConnectionID; // connection id of the player object that spawned this TNT

    [Header("Balls blown up")]
    public bool WaitingOnBlownUpBalls = false;
    public List<GolfBallTopDown> BallsBlownUp = new List<GolfBallTopDown>();
    public List<GolfBallTopDown> BallsBlownUpThatStopped = new List<GolfBallTopDown>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BlowUpTNT(GolfBallTopDown golfBallThatInitiatedThis)
    {
        // Player that planted the TNT won't blow it up?
        if (PlayerConnectionID == -99)
            return;
        if (PlayerConnectionID == golfBallThatInitiatedThis.MyPlayer.ConnectionId)
            return;
    }
}
