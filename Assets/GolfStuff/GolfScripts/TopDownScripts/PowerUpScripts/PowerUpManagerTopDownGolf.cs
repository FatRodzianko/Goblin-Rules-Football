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

        PowerUpTopDown powerUpObjectScript = powerUpObject.GetComponent<PowerUpTopDown>();
        powerUpObjectScript.SetOwnerID(playerOwner.ObjectId);
        powerUpObjectScript.SetPowerUpType(powerUpType);


        this.AddNewPlayerOwnedPowerUp(playerOwner.ObjectId, powerUpObjectScript);


        Debug.Log("SpawnPowerUpObjectForPlayer: Spawned powerup of type: " + powerUpType + " for player: " + playerOwner.PlayerName + ":" + playerOwner.ObjectId);
    }
    [Server]
    void AddNewPlayerOwnedPowerUp(int playerNetId, PowerUpTopDown newPowerUp)
    {
        if (!_playerOwnedPowerUps.ContainsKey(playerNetId))
        {
            _playerOwnedPowerUps.Add(playerNetId, newPowerUp);
        }
        else
        {
            // Remove power up from player if they already had one
            RemovePowerUpObjectFromPlayer(playerNetId, newPowerUp);
        }
    }
    [Server]
    void RemovePowerUpObjectFromPlayer(int playerNetId, PowerUpTopDown powerUp)
    {
        PowerUpTopDown powerUpToRemove = _playerOwnedPowerUps[playerNetId];
        _playerOwnedPowerUps.Remove(playerNetId);
        // call to player script to remove the power up from them?

        // For now, destroy. Maybe in the future have it randomly give the old power up to a random player that doesn't have a power up? And if all have power ups, then destroy it?
        InstanceFinder.ServerManager.Despawn(powerUp.gameObject);
    }
    [Server]
    void DestroyPowerUpObject(GameObject powerUpToDestroy)
    {
        InstanceFinder.ServerManager.Despawn(powerUpToDestroy);
    }
}
