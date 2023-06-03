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
    [SerializeField] public string SavedHeightOfBalloon; // used in the map editor to save the height of the balloon, which is then set when the balloon is spawned
    [SerializeField] [SyncVar(OnChange = nameof(SyncHeightOfBalloon))] public string HeightOfBalloon;
    [SerializeField] public bool IsPopped = false;

    [Header("Collider Stuff")]
    [SerializeField] PolygonCollider2D _myCollider;
    [SerializeField] Vector2[] _originalColliderPoints;
    [SerializeField] Vector2[] _newColliderPoints;

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Server]
    public void SetBalloonHeight(string balloonHeight)
    {
        HeightOfBalloon = balloonHeight;
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
    public void CollisionToPopBalloon()
    {
        Debug.Log("PopBalloon: is popped alreadyt? " + IsPopped);
        if (IsPopped)
            return;
        CmdPopBalloon();
        //_balloonAnimator.PopBalloon();
        //IsPopped = true;
        PopBalloon();
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
        PopBalloon();
    }
    void PopBalloon()
    {
        if (IsPopped)
            return;
        _balloonAnimator.PopBalloon();
        IsPopped = true;
        StartCoroutine(UpdateColliderPointsDelay());
    }
    IEnumerator UpdateColliderPointsDelay()
    {
        yield return new WaitForSeconds(1.0f);
        UpdateColliderPointsAfterBalloonPop();
        _environmentObstacleTopDown.RemoveIsBalloon();
        _environmentObstacleTopDown.SetBalloonHeightValues(0f, 0.1875f);
    }
    void UpdateColliderPointsAfterBalloonPop()
    {
        Debug.Log("UpdateColliderPointsAfterBalloonPop:");
        _myCollider.SetPath(0, _newColliderPoints);
    }
}
