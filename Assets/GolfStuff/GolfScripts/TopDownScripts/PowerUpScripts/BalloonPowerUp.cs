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

    [Header("PowerUp Info")]
    [SyncVar(OnChange = nameof(SyncPowerUpType))] public string PowerUpType;
    [SerializeField] Sprite _powerUpSprite;
    [SerializeField] string _powerUpText;

    [Header("PowerUp Icon")]
    [SerializeField] SpriteRenderer _iconRenderer;
    bool _raiseIcon = false;
    float _transparencyRate = 1f;
    float _moveUpRate = 1.5f;
    [SerializeField] Vector3 _iconInitialPosition;
    [SerializeField] float _iconMaxHeight = 3.0f;

    [Header("Misc.")]
    [SerializeField] List<string> _possibleHieghts = new List<string>();
    [SerializeField] EnvironmentObstacleTopDown _environmentObstacleTopDown;
    GolfBallTopDown _ballThatPoppedMe;

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
        if (_raiseIcon)
        {
            IconGoesUp();
            IconBecomesVisible();

        }
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
    public void CollisionToPopBalloon(GolfBallTopDown ball)
    {
        Debug.Log("PopBalloon: is popped alreadyt? " + IsPopped);
        if (IsPopped)
            return;
        CmdPopBalloon(ball.ObjectId);
        //_balloonAnimator.PopBalloon();
        //IsPopped = true;
        PopBalloon();
        _ballThatPoppedMe = ball;
    }
    [ServerRpc(RequireOwnership = false)]
    void CmdPopBalloon(int ballNetId)
    {
        RpcBreakStatueAnimation();
        SpawnPowerUpObjectForPlayer(ballNetId);
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
    [Server]
    void SpawnPowerUpObjectForPlayer(int ballNetId)
    {
        Debug.Log("SpawnPowerUpObjectForPlayer: ball that popped this balloon: " + ballNetId.ToString());
        PowerUpManagerTopDownGolf.instance.SpawnPowerUpObjectForPlayer(ballNetId, PowerUpType);
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
    [Server]
    public void SetBalloonPowerUpType(string newType)
    {
        // default to setting the power up type to "power" if for some reason it is null?
        if (string.IsNullOrEmpty(newType))
        {
            PowerUpType = "power";
            return;
        }
        PowerUpType = newType;
        Debug.Log("SetBalloonPowerUpType: " + PowerUpType);
    }
    void SyncPowerUpType(string prev, string next, bool asServer)
    {
        if (asServer)
            return;
        _powerUpSprite = PowerUpManagerTopDownGolf.instance.GetPowerUpSprite(next);
        
    }
    public void StartShowIcon()
    {
        _iconRenderer.sprite = _powerUpSprite;
        _raiseIcon = true;
    }
    void IconGoesUp()
    {
        _iconInitialPosition.y += (_moveUpRate * Time.deltaTime);
        _iconRenderer.transform.localPosition = _iconInitialPosition;
        if (_iconInitialPosition.y >= _iconMaxHeight)
        {
            _raiseIcon = false;
            StartCoroutine(HidePowerUpIconCountdown());
        }
    }
    void IconBecomesVisible()
    {
        if (_iconRenderer.color.a >= 1)
            return;

        Color newColor = _iconRenderer.color;
        newColor.a += (_transparencyRate * Time.deltaTime);
        _iconRenderer.color = newColor;
    }
    IEnumerator HidePowerUpIconCountdown()
    {
        yield return new WaitForSeconds(2.0f);
        _iconRenderer.enabled = false;
    }
}