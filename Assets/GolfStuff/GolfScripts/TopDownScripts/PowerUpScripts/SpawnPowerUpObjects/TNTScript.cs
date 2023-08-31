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
    [SerializeField] [SyncVar] float _radius;

    [Header("Line Radius Stuff")]
    [SerializeField] LineRadiusDrawer _lineRadiusDrawer;

    [Header("Balls blown up")]
    public bool WaitingOnBlownUpBalls = false;
    public List<GolfBallTopDown> BallsBlownUp = new List<GolfBallTopDown>();
    public List<GolfBallTopDown> BallsBlownUpThatStopped = new List<GolfBallTopDown>();

    // Start is called before the first frame update
    void Start()
    {
        UpdateRadius(_radius);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void UpdateRadius(float newRadius)
    {
        _lineRadiusDrawer.DrawRadius(newRadius);
    }
    public bool WillTNTBlowUp(GolfBallTopDown golfBallAsking)
    {
        Debug.Log("WillTNTBlowUp: Golfball with player connection id of: " + golfBallAsking.MyPlayer.ConnectionId.ToString());
        if (PlayerConnectionID == -99)
            return false;
        if (PlayerConnectionID == golfBallAsking.MyPlayer.ConnectionId)
            return false;

        return true;
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
