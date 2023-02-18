using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torndao : MonoBehaviour
{
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] BoxCollider2D _myCollider;
    [SerializeField] public float HeightInUnityUnits = 3f;
    [SerializeField] public int TornadoStrength = 1;
    public bool IsMoving = false;
    [SerializeField] float _speed = 5f;
    [SerializeField] Vector2 _movementDir = Vector2.zero;
    [SerializeField] float _distanceToMove = 0f;
    private void Awake()
    {
        if (!_myCollider)
            _myCollider = this.GetComponent<BoxCollider2D>();
        SetTornadoStrength();
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
        if (collision.tag == "golfBall")
        {
            GolfBallTopDown golfBallScript = collision.GetComponent<GolfBallTopDown>();
            if (golfBallScript.IsInHole)
                return;

            float ballZ = golfBallScript.transform.position.z;
            float ballHeightInUnityUnits = golfBallScript.GetBallHeightYValue(ballZ);

            golfBallScript.HitByTornado(HeightInUnityUnits, ballHeightInUnityUnits,TornadoStrength);
        }
    }
    void SetTornadoStrength()
    {
        int maxStrengthFromRainLevel = 1;
        int minStrengthFromWindPower = 1;
        if (RainManager.instance.RainState.ToLower().Contains("med"))
            maxStrengthFromRainLevel = 3;
        else if (RainManager.instance.RainState.ToLower().Contains("heavy"))
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
        AdjustScaleOfTornado(TornadoStrength);
        AdjustHeightOfTornado(TornadoStrength);
    }
    void AdjustScaleOfTornado(int scaleToSet)
    {
        this.transform.localScale = new Vector3(scaleToSet, scaleToSet, 1f);
    }
    void AdjustHeightOfTornado(int scaleFactor)
    {
        HeightInUnityUnits *= scaleFactor;
    }
    public void MoveTornadoForNewTurn()
    {
        GolfPlayerTopDown furthestPlayer = GetFurthestPlayer();

        if (furthestPlayer == null)
            return;

        _movementDir = (furthestPlayer.MyBall.transform.position - this.transform.position).normalized;
        _distanceToMove = DistanceToMoveTornadoThisTurn();

        IsMoving = true;
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
    float DistanceToMoveTornadoThisTurn()
    {
        float dist = 0f;

        float min = 5f * (this.TornadoStrength / 2f);
        float max = 10f * (this.TornadoStrength / 2f);

        dist = UnityEngine.Random.Range(min, max);

        return dist;
    }
}
