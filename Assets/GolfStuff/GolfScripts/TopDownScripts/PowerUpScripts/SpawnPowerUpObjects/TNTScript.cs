using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet;
using System;

public class TNTScript : NetworkBehaviour
{
    [SerializeField] [SyncVar(OnChange = nameof(SyncPlayerConnectionID))] public int PlayerConnectionID; // connection id of the player object that spawned this TNT

    [Header("Animation")]
    [SerializeField] Animator _myAnimator;
    [SerializeField] string _explosionAnim;

    [Header("Explosion stuff")]
    [SerializeField] LayerMask _golfBallLayer;
    [SerializeField] [SyncVar] public float BlastRadius = 5f;

    [Header("Balls blown up")]
    bool _areBallsInBlastRadius = false;
    public bool WaitingOnBlownUpBalls = false;
    public List<GolfBallTopDown> BallsBlownUp = new List<GolfBallTopDown>();
    public List<GolfBallTopDown> BallsBlownUpThatStopped = new List<GolfBallTopDown>();
    [SerializeField] GolfBallTopDown _golfBallThatStartedExplosion;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void SyncPlayerConnectionID(int prev, int next, bool asServer)
    {
        if (asServer)
        {
            return;
        }

        GolfPlayerTopDown player = InstanceFinder.ClientManager.Objects.Spawned[next].GetComponent<GolfPlayerTopDown>();
        TellPlayerTheyOwnThisTNT(player);
    }
    void TellPlayerTheyOwnThisTNT(GolfPlayerTopDown player)
    {
        player.MyBall.TNTPlantedByMe = this;
    }
    [Server]
    public void BlowUpTNT(GolfBallTopDown golfBallThatInitiatedThis)
    {
        Debug.Log("BlowUpTNT: on tnt from: " + golfBallThatInitiatedThis.MyPlayer.PlayerName + "'s ball");
        Debug.Log("BlowUpTNT: PlayerConnectionID is: " + PlayerConnectionID.ToString() + " ball's connection id is: " + golfBallThatInitiatedThis.MyPlayer.ConnectionId.ToString());
        // Player that planted the TNT won't blow it up?
        if (PlayerConnectionID == -99)
        {
            
            return;
        }
        if (PlayerConnectionID != golfBallThatInitiatedThis.MyPlayer.ConnectionId)
        {
            return;
        }
            

        this._golfBallThatStartedExplosion = golfBallThatInitiatedThis;
        // get all the balls that will be launched
        GetBallsWithinBlastRadius();

        _myAnimator.Play(_explosionAnim);
        RpcStartExplosionAnimation();
    }
    [ObserversRpc]
    void RpcStartExplosionAnimation()
    {
        if (this.IsServer)
            return;

        _myAnimator.Play(_explosionAnim);
        Debug.Log("RpcStartExplosionAnimation: on client");
    }
    void GetBallsWithinBlastRadius()
    {
        RaycastHit2D[] balls = Physics2D.CircleCastAll(this.transform.position, BlastRadius, Vector2.zero, 0f, _golfBallLayer);
        if (balls.Length == 0)
            return;

        foreach (RaycastHit2D hit in balls)
        {
            try
            {
                GolfBallTopDown ball = hit.transform.GetComponent<GolfBallTopDown>();
                if (BallsBlownUp.Contains(ball))
                    continue;
                Debug.Log("GetBallsWithinBlastRadius: adding " + ball.MyPlayer.PlayerName + "'s ball to get blown up");
                BallsBlownUp.Add(ball);
            }
            catch (Exception e)
            {
                Debug.Log("GetBallsWithinBlastRadius: Could not get the golfball script? Error: " + e);
                continue;
            }
        }

        if (BallsBlownUp.Count > 0)
            _areBallsInBlastRadius = true;

    }
    public void ExplosionAnimationComplete()
    {
        if (!this.IsServer)
            return;
        Debug.Log("ExplosionAnimationComplete: telling: " + _golfBallThatStartedExplosion.MyPlayer.PlayerName + "'s ball");
        //if (_areBallsInBlastRadius)
        //    LaunchBallsInBlastRadius();
        //else
        //    AllBallsBlownUp();
        if(!WaitingOnBlownUpBalls)
            AllBallsBlownUp();


    }
    public void WillBallsLaunch()
    {
        if (!this.IsServer)
            return;
        if (!_areBallsInBlastRadius)
            return;
        LaunchBallsInBlastRadius();
    }
    [Server]
    void LaunchBallsInBlastRadius()
    {
        Debug.Log("LaunchBallsInBlastRadius: ");
        Vector3 myPos = this.transform.position;
        WaitingOnBlownUpBalls = true;
        foreach (GolfBallTopDown ball in BallsBlownUp)
        {
            float distFromTNT = Vector2.Distance(ball.transform.position, myPos);
            ball.TNTThatBlewMeUp = this;
            ball.RpcLaunchBallFromTNT(ball.Owner, distFromTNT, this);
        }
    }
    [Server]
    public void BallDoneBeingBlownUp(GolfBallTopDown ball)
    {
        Debug.Log("BallDoneBeingBlownUp: from " + ball.MyPlayer.PlayerName + "'s ball.");
        if (BallsBlownUp.Contains(ball))
            BallsBlownUp.Remove(ball);
        
        ball.TNTThatBlewMeUp = null;

        if (BallsBlownUp.Count <= 0)
            AllBallsBlownUp();
    }
    void AllBallsBlownUp()
    {
        Debug.Log("AllBallsBlownUp: ");
        WaitingOnBlownUpBalls = false;
        this._golfBallThatStartedExplosion.RpcAllBallsBlownUp(_golfBallThatStartedExplosion.Owner);
    }


}
