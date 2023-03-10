using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class GolfPlayerScore : NetworkBehaviour
{
    [SyncVar] public int StrokesForCurrentHole = 0;
    [SyncVar] public int TotalStrokesForCourse = 0;
    public Dictionary<int, int> HoleWithScores = new Dictionary<int, int>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Server]
    public void ResetScoreForNewHole()
    {
        StrokesForCurrentHole = 0;
    }
    public void PlayerHitBall()
    {
        //StrokesForCurrentHole++;
        //TotalStrokesForCourse++;
        if (!this.IsOwner)
            return;
        CmdPlayerHitBall();
    }
    [ServerRpc]
    void CmdPlayerHitBall()
    {
        StrokesForCurrentHole++;
        TotalStrokesForCourse++;
    }
    public void StrokePenalty(int penalty)
    {
        StrokesForCurrentHole += penalty;
        TotalStrokesForCourse += penalty;
    }
    public void SaveScoreAtEndOfHole(int holeIndex)
    {
        HoleWithScores[holeIndex] = StrokesForCurrentHole;

        foreach (KeyValuePair<int, int> entry in HoleWithScores)
        {
            Debug.Log(transform.GetComponent<GolfPlayerTopDown>().PlayerName + " Score for hole #" + entry.Key.ToString() + " is: " + entry.Value.ToString());
        }
    }
}
