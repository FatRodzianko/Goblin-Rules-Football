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
    public Dictionary<int, int> LocalHoleWithScores = new Dictionary<int, int>();
    [SyncObject] public readonly SyncDictionary<int, int> ServerHoleWithScores = new SyncDictionary<int,int>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [ServerRpc]
    public void CmdResetScoreForNewHole()
    {
        ResetScoreForNewHole();
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
        //StrokesForCurrentHole += penalty;
        //TotalStrokesForCourse += penalty;
        if (!this.IsOwner)
            return;
        CmdStrokePenalty(penalty);
    }
    [ServerRpc]
    void CmdStrokePenalty(int penalty)
    {
        StrokesForCurrentHole += penalty;
        TotalStrokesForCourse += penalty;
    }
    public void SaveScoreAtEndOfHole(int holeIndex)
    {
        LocalHoleWithScores[holeIndex] = StrokesForCurrentHole;

        foreach (KeyValuePair<int, int> entry in LocalHoleWithScores)
        {
            Debug.Log(transform.GetComponent<GolfPlayerTopDown>().PlayerName + " Score for hole #" + entry.Key.ToString() + " is: " + entry.Value.ToString());
        }
        CmdSaveScoreAtEndOfHoleOnServer(holeIndex);
    }
    [ServerRpc]
    void CmdSaveScoreAtEndOfHoleOnServer(int holeIndex)
    {
        ServerHoleWithScores[holeIndex] = StrokesForCurrentHole;
    }
}
