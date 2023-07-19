using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;

public class WindManager : NetworkBehaviour
{
    public static WindManager instance;

    [SyncVar] public Vector2 WindDirection = Vector2.zero;
    private Vector2 _windDirection = Vector2.zero;
    [SerializeField] [SyncVar] public int WindPower = 0; // WindPower for the player's turn that uses player FavorWeather to set its value relative to BaseWindPower. Negative favor = higher wind power. Positive favor = lower wind power
    [SerializeField] private int _windPower = 0;
    [SerializeField] [SyncVar] public int BaseWindPower = 0; // this is the base wind power that server tracks
    private int _baseWindPower = 0;
    [SerializeField] public int InitialWindPower = 0; // this is the initial wind power. used when modifying the "Base" wind power from player favor. Average player favor modifies the base wind power relative to the initial wind power. negative average favor = Base Wind Power increases from initial window. Oppositve for positive average favor


    // followed event instructions from here https://answers.unity.com/questions/1206632/trigger-event-on-variable-change.html
    public delegate void WindDirectionChanged(Vector2 dir);
    public event WindDirectionChanged DirectionChanged;

    public delegate void WindPowerChanged(int power);
    public event WindPowerChanged PowerChanged;

    public delegate void BaseWindPowerChanged(int power);
    public event BaseWindPowerChanged BasePowerChanged;

    public string WindSeverity; // none, low, med, high, highest

    [Header("Tornado Stuff")]
    [SerializeField] GameObject _tornadoPrefab;
    [SerializeField] GameObject _tornadoObject;
    [SyncVar] public bool IsThereATorndao = false;
    private bool _isThereATornado = false;
    public delegate void IsTornadoChanged(bool tornado);
    public event IsTornadoChanged TornadoChanged;
    public Torndao TornadoScript;
    [SerializeField] float _minSpawnDist = 25f;
    [SerializeField] float _maxSpawnDist = 50f;
    [SerializeField] bool _spawnedThisHoleAlready = false;
    [SerializeField] GolfPlayerTopDown _targetPlayer = null;

    [Header("Wind Power Up Stuff")]
    [SerializeField] List<GolfPlayerTopDown> _playersWhoDidntUsePowerUp = new List<GolfPlayerTopDown>();

    private void Awake()
    {
        MakeInstance();

        DirectionChanged = DirectionChangedFunction;
        PowerChanged = PowerChangedFunction;
        BasePowerChanged = BasePowerChangedFunction;
        TornadoChanged = TornadoChangedFunction;
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
        //RainManager.instance.WeatherChanged += UpdateRainForTornado;
        RainManager.instance.BaseWeatherChanged += UpdateRainForTornado;
    }

    // Update is called once per frame
    void Update()
    {
        if (WindPower != _windPower && PowerChanged != null)
        {
            _windPower = WindPower;
            PowerChanged(_windPower);
        }
        if (BaseWindPower != _baseWindPower && BasePowerChanged != null)
        {
            _baseWindPower = BaseWindPower;
            BasePowerChanged(_baseWindPower);
        }
        if (WindDirection != _windDirection && DirectionChanged != null)
        {
            _windDirection = WindDirection;
            DirectionChanged(_windDirection);
        }
        if (IsThereATorndao != _isThereATornado && TornadoChanged != null)
        {
            _isThereATornado = IsThereATorndao;
            TornadoChanged(_isThereATornado);
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
        
        // this was done before there was weather favor and so on to change weather on a per player basis. now Tornadoes are only destroyed if the BaseWindPower is 0
        //if (power <= 0f && this.IsServer)
        //    DestroyTornadoObjects();
    }
    void BasePowerChangedFunction(int power)
    {
        Debug.Log("BasePowerChangedFunction: " + power.ToString());
        // if the power drops to 0, destroy and tornado objects
        if (power <= 0f && this.IsServer)
            DestroyTornadoObjects();
    }
    void TornadoChangedFunction(bool tornado)
    {
        Debug.Log("TornadoChangedFunction: Is there a tornado?" + tornado.ToString());
    }
    public void UpdateWindForNewTurn(GolfPlayerTopDown currentPlayer)
    {
        // old way before weather favor
        //WindPower = GetNewWindPower(_windPower);

        // begin new way with weather favor
        SetBaseWindPower();
        SetPlayerWindPower(currentPlayer);
    }
    public void SetInitialWindForNewHole()
    {
        // This will eventually pull from player set settings for low/medium/severe wind conditions but for now just make it random

        // Get how severe the wind will be
        WindSeverity = GetWindSeverity();
        //float newWind = GetInitialWindSpeedFromSeverity(WindSeverity);
        //WindPower = GetInitialWindSpeedFromSeverity(WindSeverity);
        InitialWindPower = GetInitialWindSpeedFromSeverity(WindSeverity); // this is the initial wind power (think of it as the wind for that "day"). Base wind power will be modified from that based on average player favor
        BaseWindPower = InitialWindPower; // set the initial value for the BaseWindPower?
        WindPower = InitialWindPower;
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
        else
            return "low";
        //else if (windSeverity < 0.95f)
        //    return "low";
        //else
        //    return "med";
        //else if (windSeverity < 0.85f)
        //    return "med";
        //else if (windSeverity < 0.95f)
        //    return "high";
        //else
        //    return "highest";
        
    }
    int GetInitialWindSpeedFromSeverity(string severity)
    {
        if (severity == "none")
            return 0;
        else
            return UnityEngine.Random.Range(1, 6);

        //else if (severity == "low")
        //    return UnityEngine.Random.Range(1, 5);
        //else if (severity == "med")
        //    return UnityEngine.Random.Range(5, 10);
        //else if (severity == "high")
        //    return UnityEngine.Random.Range(10, 18);
        //else
        //    return UnityEngine.Random.Range(18, 25);
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
    [Server]
    void SetBaseWindPower()
    {
        Debug.Log("SetBaseWindPower: ");
        float averagePlayerFavor = GameplayManagerTopDownGolf.instance.AveragePlayerWeatherFavor;
        if (averagePlayerFavor >= -1 && averagePlayerFavor <= 1)
        {
            BaseWindPower = InitialWindPower;
            return;
        }

        //float windModifierValue = GetWindModifierPercentFromSeverity(WindSeverity);
        //int windModifier = Mathf.RoundToInt(averagePlayerFavor / 2 * windModifierValue);
        int windModifier = Mathf.RoundToInt(averagePlayerFavor / 2);

        //BaseWindPower = InitialWindPower - windModifier;
        BaseWindPower = Mathf.Clamp((InitialWindPower - windModifier), 0, 26);
        Debug.Log("SetBaseWindPower: New BaseWindPower will be: " + BaseWindPower.ToString() + " based on windModifier of: " + windModifier.ToString() + " and average player weather favor of: " + averagePlayerFavor.ToString());
    }
    [Server]
    void SetPlayerWindPower(GolfPlayerTopDown currentPlayer)
    {
        Debug.Log("SetPlayerWindPower: ");
        int playerFavor = currentPlayer.FavorWeather;

        Debug.Log("SetPlayerWindPower: Rounded player favor: " + Mathf.RoundToInt(playerFavor * 0.75f).ToString());
        int newWindPower = BaseWindPower - Mathf.RoundToInt(playerFavor * 0.75f);
        newWindPower = ModifyWindByPowerUp(currentPlayer, newWindPower);
        
        if (newWindPower < 0)
            newWindPower = 0;
        if (newWindPower > 25)
            newWindPower = 25;

        Debug.Log("SetPlayerWindPower: New Player wind power will be " + newWindPower.ToString() + " based on BaseWindPower of: " + BaseWindPower.ToString() + " and current player favor of: " + playerFavor.ToString());
        WindPower = newWindPower;
    }
    float GetWindModifierPercentFromSeverity(string severity)
    {
        if (severity == "none")
            return 1f;
        else if (severity == "low")
            return 1f;
        else if (severity == "med")
            return 1.5f;
        else if (severity == "high")
            return 1.75f;
        else
            return 2f;
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
        if (newRainState == "clear" && this.IsServer)
        {
            DestroyTornadoObjects();
        }
    }
    [Server]
    void DestroyTornadoObjects()
    {
        Debug.Log("DestroyTornadoObjects");
        IsThereATorndao = false;
        if (_tornadoObject && TornadoScript)
        {
            GameObject destroyObject = _tornadoObject;
            //InstanceFinder.ServerManager.Despawn(destroyObject);
            try
            {
                TornadoScript.Despawn();
            }
            catch (Exception e)
            {
                Debug.Log("DestroyTornadoObjects: Could not DESPAWN tornado object. Error: " + e);
            }
            try
            {
                Destroy(destroyObject);
            }
            catch (Exception e)
            {
                Debug.Log("DestroyTornadoObjects: Could not destroy tornado object. Error: " + e);
            }
            _tornadoObject = null;
        }
        if (TornadoScript)
        {
            TornadoScript = null;
        }
    }
    [Server]
    public void CheckIfTornadoWillSpawn(bool newHole = false)
    {
        if (newHole)
        {
            if (IsThereATorndao)
            {
                DestroyTornadoObjects();
            }
            _spawnedThisHoleAlready = false;
            return;
        }
        if (IsThereATorndao)
            return;

        if (_spawnedThisHoleAlready)
            return;

        if (this.BaseWindPower <= 0f)
        {
            DestroyTornadoObjects();
            return;
        }
        //if (RainManager.instance.RainState == "clear") // Changed RainState to BaseRainState through this function
        if (RainManager.instance.BaseRainState == "clear")
        {
            DestroyTornadoObjects();
            return;
        }
        if (GameplayManagerTopDownGolf.instance.GolfPlayers.Count == 0)
            return;

        // update this so it will only spawn if a player hits a treshhold, like one player has to be below -5f?
        float tornadoLikelihood = 0.2f;
        //float tornadoLikelihood = 1.0f; // for testing only
        if (RainManager.instance.BaseRainState == "med rain")
            tornadoLikelihood += 0.1f;
        else if (RainManager.instance.BaseRainState == "heavy rain")
            tornadoLikelihood += 0.25f;

        if (UnityEngine.Random.Range(0f, 1f) > tornadoLikelihood && !IsThereATorndao)
        {
            Debug.Log("CheckIfTornadoWillSpawn: Will NOT Spawn a torndao this turn.");
            IsThereATorndao = false;
            return;
        }


        SpawnTornado();
    }
    void SpawnTornado()
    {
        Debug.Log("CheckIfTornadoWillSpawn: Will Spawn a torndao this turn.");

        GolfPlayerTopDown playerToSpawnBy = GetFurthestPlayer();

        Vector3 spawnPos = GetTornadoSpawnPosition(playerToSpawnBy);

        _tornadoObject = Instantiate(_tornadoPrefab, spawnPos, Quaternion.identity);
        TornadoScript = _tornadoObject.GetComponent<Torndao>();

        TornadoScript.GetPlayerTarget(playerToSpawnBy);

        InstanceFinder.ServerManager.Spawn(_tornadoObject);
        Debug.Log("CheckIfTornadoWillSpawn: spawned a tornado with netid of: " + TornadoScript.ObjectId);

        IsThereATorndao = true;
        _spawnedThisHoleAlready = true;
    }
    Vector3 GetTornadoSpawnPosition(GolfPlayerTopDown playerToSpawnBy)
    {
        Vector3 spawnPos = Vector3.zero;

        // For now, select player furthest from the hole?
        float distance = 0f;

        //GolfPlayerTopDown playerToSpawnBy = GetFurthestPlayer();

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
    public async Task MoveTornadoTask()
    {

        if (!IsThereATorndao)
            return;
        if (!TornadoScript)
            return;

        if (this.BaseWindPower <= 0f || RainManager.instance.BaseRainState == "clear")
        {
            DestroyTornadoObjects();
            return;
        }

        

        TornadoScript.MoveTornadoForNewTurn();

        while (this.BaseWindPower > 0 && RainManager.instance.BaseRainState != "clear" && (TornadoScript.IsMoving || TornadoScript.HitBall))
        {
            await Task.Yield();
        }
        if (TornadoScript.HitPlayerTarget)
        {
            Debug.Log("MoveTornadoTask: Tornado hit their target last turn. Destroying the tornado");
            DestroyTornadoObjects();
        }
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
        // was originally using GolfPlayersServer here, but that means a tornado could target a player that had been struck by lightning. Making sure not to target them any more?
        for (int i = 0; i < GameplayManagerTopDownGolf.instance.GolfPlayers.Count; i++)
        {
            GolfPlayerTopDown player = GameplayManagerTopDownGolf.instance.GolfPlayers[i];
            if (player.MyBall.IsInHole)
                continue;
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
                string[] headsTails = new[] { "heads", "tails" };
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

    [Server]
    public void WindPowerUpUsed(GolfPlayerTopDown powerUpPlayer)
    {
        WindPower = 0;
        _playersWhoDidntUsePowerUp.Clear();
        foreach (GolfPlayerTopDown player in GameplayManagerTopDownGolf.instance.GolfPlayersServer)
        {
            if (player.ObjectId != powerUpPlayer.ObjectId)
                _playersWhoDidntUsePowerUp.Add(player);
        }

    }
    [Server]
    int ModifyWindByPowerUp(GolfPlayerTopDown player, int currentWindPower)
    {
        if (_playersWhoDidntUsePowerUp.Count <= 0)
            return currentWindPower;
        if (!_playersWhoDidntUsePowerUp.Contains(player))
            return currentWindPower;

        Debug.Log("ModifyRainStateByPowerUp: for player: " + player.PlayerName);
        _playersWhoDidntUsePowerUp.Remove(player);
        if (player.FavorWeather >= 10)
        {
            return currentWindPower;
        }

        if (currentWindPower < 6)
            return currentWindPower + 2;
        else if (currentWindPower < 11)
            return currentWindPower + 3;
        else if (currentWindPower < 21)
            return currentWindPower + 4;
        else
            return currentWindPower + 5;

    }
    public void DestroyTornadoForNextHole()
    {
        DestroyTornadoObjects();
    }

}
