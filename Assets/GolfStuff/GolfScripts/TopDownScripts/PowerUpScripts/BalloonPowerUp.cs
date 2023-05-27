using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Managing;
using FishNet.Object.Synchronizing;
using FishNet;

public class BalloonPowerUp : NetworkBehaviour
{
    [Header("Sprite Stuff")]
    [SerializeField] SpriteRenderer _myRenderer;
    [SerializeField] BalloonAnimator _balloonAnimator;

    [Header("Balloon Info")]
    [SerializeField] [SyncVar(OnChange = nameof(SyncHeightOfBalloon))] public string HeightOfBalloon;
    [SerializeField] public bool IsPopped = false;

    [Header("Collider Stuff")]
    [SerializeField] Collider2D _myCollider;

    [Header("Height Values")]
    [SerializeField] float _lowStartHeight;
    [SerializeField] float _lowTopHeight;
    [SerializeField] float _medStartHeight;
    [SerializeField] float _medTopHeight;
    [SerializeField] float _highStartHeight;
    [SerializeField] float _highTopHeight;

    [Header("Misc.")]
    [SerializeField] List<string> _possibleHieghts = new List<string>();
    [SerializeField] EnvironmentObstacleTopDown _environmentObstacleTopDown;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        SetBalloonHeight();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Server]
    public void SetBalloonHeight()
    {
        var random = new System.Random();
        int index = random.Next(_possibleHieghts.Count);
        HeightOfBalloon = _possibleHieghts[index];
        Debug.Log("SetBalloonHeight: " + HeightOfBalloon);
    }
    void SyncHeightOfBalloon(string prev, string next, bool asServer)
    {
        if (asServer)
            return;
        if (string.IsNullOrWhiteSpace(next))
            return;
        Debug.Log("SyncHeightOfBalloon: " + next);
        _balloonAnimator.SetHeightOfBallon(next);
        _balloonAnimator.SetIsIdle(true);
        SetObstacleHeightValues(next);


    }
    void SetObstacleHeightValues(string height)
    {
        if (height == "high")
            _environmentObstacleTopDown.SetBalloonHeightValues(_highStartHeight, _highTopHeight);
        if (height == "med")
            _environmentObstacleTopDown.SetBalloonHeightValues(_medStartHeight, _medTopHeight);
        if (height == "low")
            _environmentObstacleTopDown.SetBalloonHeightValues(_lowStartHeight, _lowTopHeight);
    }
    [ObserversRpc(BufferLast = true)]
    public void RpcUpdateBalloonPosition(Vector3 newPos)
    {
        Debug.Log("RpcUpdateBalloonPosition: " + newPos.ToString());
        this.transform.position = newPos;
    }
    public void PopBalloon()
    {
        Debug.Log("PopBalloon: is popped alreadyt? " + IsPopped);
        if (IsPopped)
            return;
        CmdPopBalloon();
        _balloonAnimator.PopBalloon();
        IsPopped = true;
    }
    [ServerRpc(RequireOwnership = false)]
    void CmdPopBalloon()
    {
        RpcBreakStatueAnimation();
    }
    [ObserversRpc(BufferLast = true)]
    void RpcBreakStatueAnimation()
    {
        if (IsPopped)
            return;
        _balloonAnimator.PopBalloon();
        IsPopped = true;
    }
}
