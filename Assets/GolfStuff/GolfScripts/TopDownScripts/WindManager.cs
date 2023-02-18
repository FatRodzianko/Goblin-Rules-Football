using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindManager : MonoBehaviour
{
    public static WindManager instance;

    public Vector2 WindDirection = Vector2.zero;
    private Vector2 _windDirection = Vector2.zero;
    [SerializeField] public int WindPower = 0;
    [SerializeField] private int _windPower = 0;


    // followed event instructions from here https://answers.unity.com/questions/1206632/trigger-event-on-variable-change.html
    public delegate void WindDirectionChanged(Vector2 dir);
    public event WindDirectionChanged DirectionChanged;

    public delegate void WindPowerChanged(int power);
    public event WindPowerChanged PowerChanged;

    public string WindSeverity; // none, low, med, high, highest

    [Header("Tornado Stuff")]
    [SerializeField] GameObject _tornadoPrefab;
    [SerializeField] GameObject _tornadoObject;
    public bool IsThereATorndao = false;
    public Torndao TornadoScript;
    [SerializeField] float _minSpawnDist = 10f;
    [SerializeField] float _maxSpawnDist = 50f;

    private void Awake()
    {
        MakeInstance();

        DirectionChanged = DirectionChangedFunction;
        PowerChanged = PowerChangedFunction;
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        RainManager.instance.WeatherChanged += UpdateRainForTornado;
    }

    // Update is called once per frame
    void Update()
    {
        if (WindPower != _windPower && PowerChanged != null)
        {
            _windPower = WindPower;
            PowerChanged(_windPower);
        }
        if (WindDirection != _windDirection && DirectionChanged != null)
        {
            _windDirection = WindDirection;
            DirectionChanged(_windDirection);
        }
    }
    void DirectionChangedFunction(Vector2 dir)
    {
        Debug.Log("DirectionChangedFunction: " + dir.ToString());
    }
    void PowerChangedFunction(int power)
    {
        Debug.Log("PowerChangedFunction: " + power.ToString());
        // if the power drops to 0, destroy and tornado objects
        if (power <= 0f)
            DestroyTornadoObjects();
    }
    public void UpdateWindForNewTurn()
    {
        WindPower = GetNewWindPower(_windPower);
    }
    public void SetInitialWindForNewHole()
    {
        // This will eventually pull from player set settings for low/medium/severe wind conditions but for now just make it random

        // Get how severe the wind will be
        WindSeverity = GetWindSeverity();
        //float newWind = GetInitialWindSpeedFromSeverity(WindSeverity);
        WindPower = GetInitialWindSpeedFromSeverity(WindSeverity);
        //WindPower = GetNewWindPower(newWind);
    }
    public void SetInitialWindDirection()
    {
        // set the x value of the wind
        int negOrPos = UnityEngine.Random.Range(0, 2) * 2 - 1;
        int x = (int)UnityEngine.Random.Range(0, 2) * negOrPos;
        // reset negOrPos
        negOrPos = UnityEngine.Random.Range(0, 2) * 2 - 1;
        // set the y value of the wind
        int y = (int)UnityEngine.Random.Range(0, 2) * negOrPos;
        if (x == 0 && y == 0)
        {
            Debug.Log("SetInitialWindDirection: both x and y are 0. Randomly setting one to 1 or -1. " + x.ToString() +":" + y.ToString());
            if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
                x = negOrPos;
            else
                y = negOrPos;
            Debug.Log("SetInitialWindDirection: both x and y were 0. Randomly set one to 1 or -1. " + x.ToString() + ":" + y.ToString());
        }
        Debug.Log("SetInitialWindDirection: setting new wind direction of " + x.ToString() + "," + y.ToString());
        WindDirection = new Vector2(x, y);
    }
    string GetWindSeverity()
    {
        float windSeverity = UnityEngine.Random.Range(0f, 1f);
        Debug.Log("GetWindSeverity: The wind severity chance value is: " + windSeverity.ToString());
        if (windSeverity < 0.05)
            return "none";
        else if (windSeverity < 0.6f)
            return "low";
        else if (windSeverity < 0.85f)
            return "med";
        else if (windSeverity < 0.95f)
            return "high";
        else
            return "highest";
        
    }
    int GetInitialWindSpeedFromSeverity(string severity)
    {
        if (severity == "none")
            return 0;
        else if (severity == "low")
            return UnityEngine.Random.Range(1, 5);
        else if (severity == "med")
            return UnityEngine.Random.Range(5, 10);
        else if (severity == "high")
            return UnityEngine.Random.Range(10, 18);
        else
            return UnityEngine.Random.Range(18, 25);
    }
    int GetNewWindPower(float currentWindPower)
    {

        float range = currentWindPower * 0.25f;
        if (range < 1)
            range = 1;
        float minRange = currentWindPower - range;
        if (minRange < 0)
            minRange = 0;
        float maxRange = currentWindPower + range;
        if (maxRange > 25)
            maxRange = 25;

        return (int)UnityEngine.Random.Range(minRange, maxRange);
    }
    public void UpdateWindDirectionForNewTurn()
    {
        // For these update functions for wind/rain, will probably have a game setting for players that's something like "Can Wind/Rain Change? Yes/No" to either allow this or not
        Debug.Log("UpdateWindDirectionForNewTurn");
        if (UnityEngine.Random.Range(0f, 1f) < 0.75f)
            return;

        Debug.Log("UpdateWindDirectionForNewTurn: Running!");
        Vector2 newWindDir = _windDirection;

        // decide whether to change the x or y coordinate
        bool changeX = false;
        if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
            changeX = true;

        // decide to subtract or add 1 to the coordinate
        int negOrPos = UnityEngine.Random.Range(0, 2) * 2 - 1;

        // get the current coordinate value
        int coordinateValue = 0;
        if (changeX)
            coordinateValue = (int)_windDirection.x;
        else
            coordinateValue = (int)_windDirection.y;

        // add the negOrPos value to the coordinate
        coordinateValue += negOrPos;

        Debug.Log("UpdateWindDirectionForNewTurn: coordinate value: " + coordinateValue + " and change x? " + changeX.ToString());

        // check if the coordinate value has changed past 1 or -1. IF so, set the OPPOSITE coordinate to 0
        if (coordinateValue > 1 || coordinateValue < -1)
        {
            Debug.Log("UpdateWindDirectionForNewTurn: roll over past 1 or -1 with value of: " + coordinateValue.ToString() + " change x? " + changeX.ToString());
            if (changeX)
                newWindDir.y = 0;
            else
                newWindDir.x = 0;
        }
        else if (coordinateValue == 0)
        {
            int otherCoord = 0;
            if (changeX)
                otherCoord = (int)_windDirection.y;
            else
                otherCoord = (int)_windDirection.x;
            if (otherCoord == 0)
            {
                Debug.Log("UpdateWindDirectionForNewTurn: Other Coord equals 0.");
            }
            else
            {
                Debug.Log("UpdateWindDirectionForNewTurn: Setting to coordinate value of " + coordinateValue.ToString() + " change x?" + changeX.ToString());
                if (changeX)
                    newWindDir.x = coordinateValue;
                else
                    newWindDir.y = coordinateValue;
            }
        }
        else
        {
            Debug.Log("UpdateWindDirectionForNewTurn: Setting to coordinate value of " + coordinateValue.ToString() + " change x?" + changeX.ToString());
            if (changeX)
                newWindDir.x = coordinateValue;
            else
                newWindDir.y = coordinateValue;
        }
        WindDirection = newWindDir;
    }
    void UpdateRainForTornado(string newRainState)
    {
        // If the weather is clear, destroy any tornado objects 
        if (newRainState == "clear")
        {
            DestroyTornadoObjects();
        }
    }
    void DestroyTornadoObjects()
    {
        IsThereATorndao = false;
        if (_tornadoObject)
        {
            GameObject destroyObject = _tornadoObject;
            Destroy(destroyObject);
            _tornadoObject = null;
        }
        if (TornadoScript)
        {
            TornadoScript = null;
        }
    }
    public void CheckIfTornadoWillSpawn()
    {
        if (IsThereATorndao)
            return;
        if (this.WindPower <= 0f)
        {
            DestroyTornadoObjects();
            return;
        }
        if (RainManager.instance.RainState == "clear")
        {
            DestroyTornadoObjects();
            return;
        }
        if (GameplayManagerTopDownGolf.instance.GolfPlayers.Count == 0)
            return;
        Debug.Log("CheckIfTornadoWillSpawn: Will Spawn a torndao this turn.");

        Vector3 spawnPos = GetTornadoSpawnPosition();

        _tornadoObject = Instantiate(_tornadoPrefab, spawnPos, Quaternion.identity);
        TornadoScript = _tornadoObject.GetComponent<Torndao>();

        IsThereATorndao = true;
    }
    Vector3 GetTornadoSpawnPosition()
    {
        Vector3 spawnPos = Vector3.zero;

        // For now, select player furthest from the hole?
        float distance = 0f;
        GolfPlayerTopDown playerToSpawnBy = GetFurthestPlayer();

        if (playerToSpawnBy == null)
            return spawnPos;

        Vector3 startPos = playerToSpawnBy.MyBall.transform.position;

        spawnPos = GetRandomPositionFromGivenPosition(startPos);

        // Check to make sure the spawn position is in bounds
        PolygonCollider2D cameraBoundingBox = GameObject.FindGameObjectWithTag("CameraBoundingBox").GetComponent<PolygonCollider2D>();
        bool isSpawnInbounds = cameraBoundingBox.OverlapPoint(spawnPos);
        if (!isSpawnInbounds)
        {
            while (!isSpawnInbounds)
            {
                spawnPos = GetRandomPositionFromGivenPosition(startPos);
                isSpawnInbounds = cameraBoundingBox.OverlapPoint(spawnPos);
            }
        }

        return spawnPos;
    }
    Vector3 GetRandomPositionFromGivenPosition(Vector3 startPos)
    {
        Vector3 randomPos = Vector3.zero;

        float randX = UnityEngine.Random.Range(-1f, 1f);
        float randY = UnityEngine.Random.Range(-1f, 1f);
        Vector3 randDir = new Vector2(randX, randY).normalized;

        float randDist = UnityEngine.Random.Range(_minSpawnDist, _maxSpawnDist);

        randomPos = startPos + (randDist * randDir);

        return randomPos;
    }
    public void MoveTornadoForNewTurn()
    {
        if (!IsThereATorndao)
            return;
        if (!TornadoScript)
            return;

        TornadoScript.MoveTornadoForNewTurn();
        
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

}
