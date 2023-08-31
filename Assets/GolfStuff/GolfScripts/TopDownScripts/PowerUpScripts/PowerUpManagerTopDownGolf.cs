using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object.Synchronizing;
using FishNet.Object;
using FishNet.Connection;
using FishNet;
using System;

public class PowerUpManagerTopDownGolf : NetworkBehaviour
{
    public static PowerUpManagerTopDownGolf instance;

    [Header("Prefabs")]
    [SerializeField] GameObject _powerUpBallonPrefab;
    [SerializeField] GameObject _powerUpObject;

    [Header("Balloons")]
    [SerializeField] List<BalloonPowerUp> _activeBallonPowerUps = new List<BalloonPowerUp>(); // only maintained by the server?
    [SerializeField] [SyncObject] public readonly SyncList<int> BalloonNetIds = new SyncList<int>();

    [Header("Misc")]
    [SerializeField] Vector3 _spawnPoint; // 183.469 -79.61863

    [Header("Ballon Characteristics")]
    [SerializeField] List<string> _possibleBalloonHeights = new List<string>();

    [Header("PowerUp Types")]
    [SerializeField] List<PowerUpTypeMapping> PowerUpTypeToSpriteMapping = new List<PowerUpTypeMapping>();

    [Header("Player PowerUps")]
    Dictionary<int, PowerUpTopDown> _playerOwnedPowerUps = new Dictionary<int, PowerUpTopDown>();
    List<int> _playerNetIdsWhoUsedPowerUps = new List<int>();

    [Header("Objects SpawnedByPowerUps")]
    [SerializeField] GameObject _rockPowerUpPrefab;
    [SerializeField] GameObject _tntPowerUpPrefab;
    [SerializeField] List<GameObject> _spawnedObjectsFromPowerUps = new List<GameObject>();

    [Serializable]
    public struct PowerUpTypeMapping
    {
        public string powerUpType;
        public Sprite powerUpSprite;
        public string powerUpText;
    }

    // Start is called before the first frame update
    void Awake()
    {
        MakeInstance();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    // Spawn in random offset from spawn point for now. Will later have powerup spawn points be set in the scriptable hole
    [Server]
    public void SpawnBallonsForNewHole(List<SavedBalloonPowerUp> savedBalloonPowerUps)
    {
        Debug.Log("SpawnBallonsForNewHole: " + savedBalloonPowerUps.Count.ToString());

        if (savedBalloonPowerUps.Count <= 0)
            return;

        foreach (SavedBalloonPowerUp saved in savedBalloonPowerUps)
        {
            string balloonHeight = saved.BalloonHeight;

            // If the balloon didn't have a saved height, pick a height at random
            if (string.IsNullOrEmpty(balloonHeight))
            {
                Debug.Log("SpawnBallonsForNewHole: ballon did not have a set height. Picking a random height.");
                balloonHeight = GetRandomBalloonHeight();
                Debug.Log("SpawnBallonsForNewHole: ballon did not have a set height. Randomly picked height will be: " + balloonHeight);
            }

            SpawnBallon(saved.BalloonPosition, balloonHeight);
        }
    }
    [Server]
    void SpawnBallon(Vector3 pos, string balloonHeight)
    {
        Debug.Log("SpawnBallon: " + pos.ToString() + " with a height of: " + balloonHeight);
        GameObject balloon = Instantiate(_powerUpBallonPrefab);
        InstanceFinder.ServerManager.Spawn(balloon);
        BalloonPowerUp balloonScript = balloon.GetComponent<BalloonPowerUp>();
        _activeBallonPowerUps.Add(balloonScript);
        if (!BalloonNetIds.Contains(balloonScript.ObjectId))
            BalloonNetIds.Add(balloonScript.ObjectId);
        balloonScript.SetBalloonHeight(balloonHeight);
        balloonScript.RpcUpdateBalloonPosition(pos);
        balloonScript.SetBalloonPowerUpType(GetPowerUpType());
    }
    [Server]
    string GetRandomBalloonHeight()
    {
        var random = new System.Random();
        int index = random.Next(_possibleBalloonHeights.Count);
        return _possibleBalloonHeights[index];
    }
    [Server]
    string GetPowerUpType()
    {
        var random = new System.Random();
        int index = random.Next(PowerUpTypeToSpriteMapping.Count);
        return PowerUpTypeToSpriteMapping[index].powerUpType;
    }
    public Sprite GetPowerUpSprite(string type)
    {
        Sprite spriteToReturn = PowerUpTypeToSpriteMapping.Find(x => x.powerUpType == type).powerUpSprite;
        return spriteToReturn;
    }
    public string GetPowerUpText(string type)
    { 
        string typeToReturn = PowerUpTypeToSpriteMapping.Find(x => x.powerUpType == type).powerUpText;
        return typeToReturn;
    }
    [Server]
    public void DespawnBalloonsForNewHole()
    {
        Debug.Log("DespawnBalloonsForNewHole: destroying this many balloons: " + _activeBallonPowerUps.Count.ToString());
        if (_activeBallonPowerUps.Count <= 0)
            return;
        foreach (BalloonPowerUp balloon in _activeBallonPowerUps)
        {
            GameObject objectToDestroy = balloon.gameObject;
            InstanceFinder.ServerManager.Despawn(objectToDestroy);
        }
        _activeBallonPowerUps.Clear();
        BalloonNetIds.Clear();
    }
    [Server]
    public void SpawnPowerUpObjectForPlayer(int ballNetId, string powerUpType)
    {
        Debug.Log("SpawnPowerUpObjectForPlayer: PowerUpManagerTopDownGolf");

        GolfPlayerTopDown playerOwner = InstanceFinder.ServerManager.Objects.Spawned[ballNetId].GetComponent<GolfBallTopDown>().MyPlayer;

        GameObject powerUpObject = Instantiate(_powerUpObject);
        InstanceFinder.ServerManager.Spawn(powerUpObject, playerOwner.Owner);

        // Set up the power up's parameters/configuration
        PowerUpTopDown powerUpObjectScript = powerUpObject.GetComponent<PowerUpTopDown>();
        powerUpObjectScript.SetOwnerID(playerOwner.ObjectId);
        powerUpObjectScript.SetPowerUpType(powerUpType);
        string powerUpText = GetPowerUpText(powerUpType);
        powerUpObjectScript.SetPowerUpText(powerUpText);
        //powerUpObjectScript.RpcSetPowerUpSpriteInPlayerUI(playerOwner.ObjectId, powerUpType);

        // Set the player power up parameters
        playerOwner.NewPowerUpForPlayer(powerUpType, powerUpText);

        // Track the powerup to the player
        this.AddNewPlayerOwnedPowerUp(playerOwner.ObjectId, powerUpObjectScript);


        Debug.Log("SpawnPowerUpObjectForPlayer: Spawned powerup of type: " + powerUpType + " for player: " + playerOwner.PlayerName + ":" + playerOwner.ObjectId);
    }
    [Server]
    void AddNewPlayerOwnedPowerUp(int playerNetId, PowerUpTopDown newPowerUp)
    {
        if (!_playerOwnedPowerUps.ContainsKey(playerNetId))
        {
            Debug.Log("AddNewPlayerOwnedPowerUp: adding powerup for player with id: " + playerNetId.ToString() + "powerup type is: " + newPowerUp.PowerUpType + ":" + newPowerUp.ObjectId.ToString());
            _playerOwnedPowerUps.Add(playerNetId, newPowerUp);
        }
        else
        {
            Debug.Log("AddNewPlayerOwnedPowerUp: Player already has a powerup. Removing the old power up.");
            // Remove power up from player if they already had one
            RemovePowerUpObjectFromPlayer(playerNetId, newPowerUp);
            // Give the player the new powerup
            _playerOwnedPowerUps.Add(playerNetId, newPowerUp);
            Debug.Log("AddNewPlayerOwnedPowerUp: adding powerup for player with id: " + playerNetId.ToString() + "powerup type is: " + newPowerUp.PowerUpType + ":" + newPowerUp.ObjectId.ToString());
        }
    }
    [Server]
    void RemovePowerUpObjectFromPlayer(int playerNetId, PowerUpTopDown powerUp)
    {
        PowerUpTopDown powerUpToRemove = _playerOwnedPowerUps[playerNetId];
        Debug.Log("RemovePowerUpObjectFromPlayer: destroying powerup for player: " + playerNetId.ToString() + " and destroying powerup: " + powerUpToRemove.PowerUpType + ":" + powerUpToRemove.ObjectId.ToString());
        _playerOwnedPowerUps.Remove(playerNetId);
        // call to player script to remove the power up from them?

        // For now, destroy. Maybe in the future have it randomly give the old power up to a random player that doesn't have a power up? And if all have power ups, then destroy it?
        InstanceFinder.ServerManager.Despawn(powerUpToRemove.gameObject);
    }
    [Server]
    public void PlayerIsUsingPowerUp(int playerNetId)
    {
        if (!_playerOwnedPowerUps.ContainsKey(playerNetId))
        {
            Debug.Log("PlayerIsUsingPowerUp: Player net id of: " + playerNetId.ToString() + " not found. aborting...");
            return;
        }
        PowerUpTopDown powerUpToUse = _playerOwnedPowerUps[playerNetId];
        if (powerUpToUse.HasBeenUsed)
        {
            Debug.Log("PlayerIsUsingPowerUp: power up has already been used. Aborting... " + powerUpToUse.PowerUpType + ":" + powerUpToUse.HasBeenUsed.ToString());
            return;
        }
        GolfPlayerTopDown playerWhoUsed = InstanceFinder.ServerManager.Objects.Spawned[playerNetId].GetComponent<GolfPlayerTopDown>();
        _playerNetIdsWhoUsedPowerUps.Add(playerWhoUsed.ObjectId);
        playerWhoUsed.SetUsedPowerUpType(powerUpToUse.PowerUpType);
        powerUpToUse.HasBeenUsed = true;
    }
    [Server]
    public void RemoveUsedPowerUps()
    {
        if (_playerNetIdsWhoUsedPowerUps.Count <= 0)
            return;
        foreach (int playerId in _playerNetIdsWhoUsedPowerUps)
        {
            PowerUpTopDown powerup = _playerOwnedPowerUps[playerId];
            RemovePowerUpObjectFromPlayer(playerId, powerup);
        }
    }
    [Server]
    public void RemoveUsedPowerupsFromPlayer(int playerNetId)
    {
        Debug.Log("RemoveUsedPowerupsFromPlayer: for player with net id: " + playerNetId.ToString());
        if (!_playerOwnedPowerUps.ContainsKey(playerNetId))
        {
            return;
        }
        if(_playerOwnedPowerUps[playerNetId].GetComponent<PowerUpTopDown>().HasBeenUsed)
            RemovePowerUpObjectFromPlayer(playerNetId, _playerOwnedPowerUps[playerNetId]);
    }
    [Server]
    public void SpawnRockFromPowerUp(Vector3 rockPosition)
    {
        Debug.Log("SpawnRockFromPowerUp: " + rockPosition.ToString());
        Vector3 spawnPosition = GetValidSpawnPosition(rockPosition);
        SpawnObjectFromPowerUp(_rockPowerUpPrefab, spawnPosition);
    }
    [Server]
    public void SpawnTNTFromPowerUp(Vector3 tntPosition, GolfPlayerTopDown playerThatSpawnedTNT)
    {
        Vector3 spawnPosition = GetValidSpawnPosition(tntPosition);

        GameObject spawnedObject = Instantiate(_tntPowerUpPrefab, spawnPosition, Quaternion.identity);
        TNTScript spawnedObjectTNTScript = spawnedObject.GetComponent<TNTScript>();

        spawnedObjectTNTScript.PlayerConnectionID = playerThatSpawnedTNT.ConnectionId;

        InstanceFinder.ServerManager.Spawn(spawnedObject);
        this._spawnedObjectsFromPowerUps.Add(spawnedObject);

    }
    Vector3 GetValidSpawnPosition(Vector3 originalPosition)
    {
        bool isTooCloseToHoleOrTeeOff = IsPositionTooCloseToHoleOrTeeOffPositions(originalPosition);

        if (!isTooCloseToHoleOrTeeOff)
            return originalPosition;

        Vector3 newPos = originalPosition;

        while (isTooCloseToHoleOrTeeOff)
        {
            Vector3 tooCloseObjectPos = GetNearestObjectToAvoid(originalPosition);
            Vector2 moveDirection = (originalPosition - tooCloseObjectPos).normalized;

            newPos = tooCloseObjectPos + (Vector3)(moveDirection * 1.21f);
            isTooCloseToHoleOrTeeOff = IsPositionTooCloseToHoleOrTeeOffPositions(newPos);
        }

        return newPos;
    }
    bool IsPositionTooCloseToHoleOrTeeOffPositions(Vector3 position)
    {
        bool tooClose = false;

        foreach (Vector3 holePosition in GameplayManagerTopDownGolf.instance.HolePositions)
        {
            if (Vector2.Distance(position, holePosition) <= 1.2f)
            {
                tooClose = true;
                Debug.Log("IsPositionTooCloseToHoleOrTeeOffPositions: " + tooClose.ToString() + " from hole");
                return tooClose;
            }
        }

        if (!tooClose)
        {
            if (Vector2.Distance(GameplayManagerTopDownGolf.instance.TeeOffPosition, position) <= 1.2f)
            {
                tooClose = true;
                Debug.Log("IsPositionTooCloseToHoleOrTeeOffPositions: " + tooClose.ToString() + " from tee off position");
                return tooClose;
            }
        }
        Debug.Log("IsPositionTooCloseToHoleOrTeeOffPositions: " + tooClose.ToString());
        return tooClose;
    }
    Vector3 GetNearestObjectToAvoid(Vector3 position)
    {
        foreach (Vector3 holePosition in GameplayManagerTopDownGolf.instance.HolePositions)
        {
            if (Vector2.Distance(position, holePosition) <= 1.2f)
            {
                Debug.Log("GetNearestObjectToAvoid: return hole position");
                return holePosition;
            }
        }

        if (Vector2.Distance(GameplayManagerTopDownGolf.instance.TeeOffPosition, position) <= 1.2f)
        {
            Debug.Log("GetNearestObjectToAvoid: return tee off position");
            return GameplayManagerTopDownGolf.instance.TeeOffPosition;
        }

        return position;
    }
    void SpawnObjectFromPowerUp(GameObject prefabToSpawn, Vector3 spawnPos)
    {
        GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        InstanceFinder.ServerManager.Spawn(spawnedObject);
        this._spawnedObjectsFromPowerUps.Add(spawnedObject);
    }
    public void DespawnObjectsFromPowerUpsForNewTurn()
    {
        if(this._spawnedObjectsFromPowerUps.Count == 0)
            return;

        foreach (GameObject spawnedObject in _spawnedObjectsFromPowerUps)
        {
            GameObject objectToDestroy = spawnedObject.gameObject;
            InstanceFinder.ServerManager.Despawn(objectToDestroy);
        }

        this._spawnedObjectsFromPowerUps.Clear();
    }
    [Server]
    void DestroyPowerUpObject(GameObject powerUpToDestroy)
    {
        InstanceFinder.ServerManager.Despawn(powerUpToDestroy);
    }
}
