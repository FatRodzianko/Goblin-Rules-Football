using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class Torndao : NetworkBehaviour
{
    [Header("My Components")]
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] BoxCollider2D _myCollider;
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] GameObject _centerObject;

    [Header("Attributes")]
    [SerializeField] public float HeightInUnityUnits = 3f;
    [SerializeField] [SyncVar(OnChange = nameof(SyncTornadoStrength))] public int TornadoStrength = -1;

    [Header("Movement")]
    public bool IsMoving = false;
    [SerializeField] float _speed = 5f;
    [SerializeField] Vector2 _movementDir = Vector2.zero;
    [SerializeField] float _distanceToMove = 0f;

    [Header("Balls Hit")]
    public bool HitBall = false;
    public List<GolfBallTopDown> BallsHit = new List<GolfBallTopDown>();
    public List<GolfBallTopDown> BallsHitThatStopped = new List<GolfBallTopDown>();

    [Header("Misc.")]
    CameraFollowScript _cameraFollowScript;

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Tornado.cs: OnStartServer");
        SetTornadoStrength();
    }
    public override void OnStartClient()
    {   
        base.OnStartClient();
        Debug.Log("Tornado.cs: OnStartClient");
        if (!_myCollider)
            _myCollider = this.GetComponent<BoxCollider2D>();
        GetCameraFollowScript();
    }
    private void Awake()
    {
        //if (!_myCollider)
        //    _myCollider = this.GetComponent<BoxCollider2D>();
        //SetTornadoStrength();
        //GetCameraFollowScript();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        if (IsMoving)
        {
            Vector2 newPos = _rb.position + _movementDir * _speed * Time.fixedDeltaTime;

            float distSinceLast = Vector2.Distance(newPos, this.transform.position);
            _rb.MovePosition(_rb.position + _movementDir * _speed * Time.fixedDeltaTime);
            _distanceToMove -= distSinceLast;
            if (_distanceToMove <= 0f)
                IsMoving = false;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!this.IsServer)
            return;
        if (collision.tag == "golfBall")
        {
            GolfBallTopDown golfBallScript = collision.GetComponent<GolfBallTopDown>();
            if (golfBallScript.IsInHole)
                return;
            if (BallsHit.Contains(golfBallScript))
                return;
            if (!golfBallScript.MyPlayer.HasPlayerTeedOff) // don't launch a player's ball if they haven't teed off yet? Seems unfair...
                return;


            float ballZ = golfBallScript.transform.position.z;
            float ballHeightInUnityUnits = golfBallScript.GetBallHeightYValue(ballZ);

            if (this.IsMoving)
            {
                HitBall = true;
                BallsHit.Add(golfBallScript);
            }
            // For multiplayer this will need to be changed so that the server tells the client to do the HitByTornado thing?
            //golfBallScript.HitByTornado(HeightInUnityUnits, ballHeightInUnityUnits,TornadoStrength, this);
            golfBallScript.RpcHitByTornado(golfBallScript.Owner, HeightInUnityUnits, ballHeightInUnityUnits, TornadoStrength, this);

        }
    }
    void SetTornadoStrength()
    {
        int maxStrengthFromRainLevel = 1;
        int minStrengthFromWindPower = 1;
        if (RainManager.instance.BaseRainState.ToLower().Contains("med")) // changed RainState to BaseRainState
            maxStrengthFromRainLevel = 3;
        else if (RainManager.instance.BaseRainState.ToLower().Contains("heavy"))
            maxStrengthFromRainLevel = 5;

        float currentWindPower = WindManager.instance.WindPower;
        if (currentWindPower < 10)
        {
            minStrengthFromWindPower = 1;
        }
        else if (currentWindPower < 15)
        {
            minStrengthFromWindPower = 2;
        }
        else
        {
            minStrengthFromWindPower = 3;
        }

        if (minStrengthFromWindPower > maxStrengthFromRainLevel)
            minStrengthFromWindPower = maxStrengthFromRainLevel;

        if (minStrengthFromWindPower == maxStrengthFromRainLevel)
        {
            TornadoStrength = minStrengthFromWindPower;
        }
        else
        {
            TornadoStrength = UnityEngine.Random.Range(minStrengthFromWindPower, maxStrengthFromRainLevel);
        }
        Debug.Log("SetTornadoStrength: Tornado strength is: " + TornadoStrength.ToString());
        RpcSetTornadoStrength(TornadoStrength);
        // removed for multiplayer. Done on the clients now?
        //AdjustScaleOfTornado(TornadoStrength);
        //AdjustHeightOfTornado(TornadoStrength);
        //AdjustCenterObject();
    }
    void AdjustScaleOfTornado(int scaleToSet)
    {
        this.transform.localScale = new Vector3(scaleToSet, scaleToSet, 1f);
    }
    void AdjustHeightOfTornado(int scaleFactor)
    {
        HeightInUnityUnits *= scaleFactor;
    }
    void AdjustCenterObject()
    {
        _centerObject.transform.localPosition = _spriteRenderer.localBounds.center;
    }
    public void MoveTornadoForNewTurn()
    {
        //GolfPlayerTopDown furthestPlayer = GetFurthestPlayer();
        GolfPlayerTopDown furthestPlayer = GetPlayerWithWorstFavor();

        if (furthestPlayer == null)
            return;

        _movementDir = (furthestPlayer.MyBall.transform.position - this.transform.position).normalized;
        _distanceToMove = DistanceToMoveTornadoThisTurn();

        //GetCameraFollowScript();
        //_cameraFollowScript.followTarget = _centerObject;
        RpcMoveTornadoForNewTurn();

        ResetHitBallStuff();
        IsMoving = true;
    }
    [ObserversRpc]
    void RpcMoveTornadoForNewTurn()
    {
        GetCameraFollowScript();
        _cameraFollowScript.followTarget = _centerObject;
    }
    GolfPlayerTopDown GetFurthestPlayer()
    {
        float distance = 0f;
        GolfPlayerTopDown playerToSpawnBy = null;
        for (int i = 0; i < GameplayManagerTopDownGolf.instance.GolfPlayers.Count; i++)
        {
            GolfPlayerTopDown player = GameplayManagerTopDownGolf.instance.GolfPlayers[i];
            if (i == 0)
            {
                distance = player.DistanceToHole;
                playerToSpawnBy = player;
                continue;
            }

            if (player.DistanceToHole > distance)
            {
                distance = player.DistanceToHole;
                playerToSpawnBy = player;
            }
        }
        return playerToSpawnBy;
    }
    GolfPlayerTopDown GetPlayerWithWorstFavor()
    {
        GolfPlayerTopDown playerToSpawnBy = null;
        int lowestFavor = 0;
        for (int i = 0; i < GameplayManagerTopDownGolf.instance.GolfPlayersServer.Count; i++)
        {
            GolfPlayerTopDown player = GameplayManagerTopDownGolf.instance.GolfPlayersServer[i];
            if (i == 0)
            {
                lowestFavor = player.FavorWeather;
                playerToSpawnBy = player;
                continue;
            }

            if (player.FavorWeather < lowestFavor)
            {
                lowestFavor = player.FavorWeather;
                playerToSpawnBy = player;
            }
            else if (player.FavorWeather == lowestFavor)
            {
                Debug.Log("GetPlayerWithWorstFavor: Two players with same favor of: " + lowestFavor.ToString() + " randomly picking between them");
                string[] headsTails = new[] { "heads","tails"};
                var rng = new System.Random();
                string result = headsTails[rng.Next(headsTails.Length)];
                if (result == "heads")
                    continue;
                else
                {
                    lowestFavor = player.FavorWeather;
                    playerToSpawnBy = player;
                }
            }
        }
        Debug.Log("GetPlayerWithWorstFavor: will have tornado follow player: " + playerToSpawnBy.PlayerName);
        return playerToSpawnBy;
    }
    float DistanceToMoveTornadoThisTurn()
    {
        float dist = 0f;

        float min = 7.5f * (this.TornadoStrength / 2f);
        float max = 15f * (this.TornadoStrength / 2f);

        dist = UnityEngine.Random.Range(min, max);

        return dist;
    }
    void GetCameraFollowScript()
    {
        if (!_cameraFollowScript)
            _cameraFollowScript = GameObject.FindGameObjectWithTag("camera").GetComponent<CameraFollowScript>();
    }
    [Server]
    public async void BallCompletedTornadoHit(GolfBallTopDown ball)
    {
        Debug.Log("BallCompletedTornadoHit: From ball: " + ball.gameObject.name);
        if (BallsHit.Contains(ball) && !BallsHitThatStopped.Contains(ball))
        {
            BallsHit.Remove(ball);
            BallsHitThatStopped.Add(ball);
        }


        if (BallsHit.Count <= 0)
        {
            foreach (GolfBallTopDown stoppedBall in BallsHitThatStopped)
            {
                //await stoppedBall.MyPlayer.TellPlayerGroundTheyLandedOn(3);
                await ball.MyPlayer.ServerTellPlayerGroundTheyLandedOn(3);
                if (stoppedBall.IsInHole)
                {
                    Debug.Log("BallCompletedTornadoHit: Ball hit into hole by tornado!");
                    if (GameplayManagerTopDownGolf.instance.AreAllPlayersInHoleOrIncapacitated())
                    {
                        GameplayManagerTopDownGolf.instance.AllPlayersInHoleOrIncapacitated(stoppedBall);
                    }
                }
            }

            HitBall = false;
        }
    }
    void ResetHitBallStuff()
    {
        HitBall = false;
        BallsHit.Clear();
        BallsHitThatStopped.Clear();
    }
    void SyncTornadoStrength(int prev, int next, bool asServer)
    {
        if (asServer)
            return;
        Debug.Log("SyncTornadoStrength: " + next.ToString());
        AdjustScaleOfTornado(next);
        AdjustHeightOfTornado(next);
        AdjustCenterObject();
    }
    [ObserversRpc(BufferLast = true)]
    void RpcSetTornadoStrength(int tornadoStrength)
    {
        if (TornadoStrength == tornadoStrength)
            return;
        Debug.Log("RpcSetTornadoStrength: new strength: " + tornadoStrength.ToString() + " old strength: " + TornadoStrength.ToString());
        AdjustScaleOfTornado(tornadoStrength);
        AdjustHeightOfTornado(tornadoStrength);
        AdjustCenterObject();
    }
}
