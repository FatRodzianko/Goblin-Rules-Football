using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object.Synchronizing;
using FishNet.Object;
using FishNet.Connection;
using FishNet;

public class PowerUpManagerTopDownGolf : NetworkBehaviour
{
    public static PowerUpManagerTopDownGolf instance;

    [Header("Prefabs")]
    [SerializeField] GameObject _powerUpBallonPrefab;

    [Header("Balloons")]
    [SerializeField] List<BalloonPowerUp> _activeBallonPowerUps = new List<BalloonPowerUp>(); // only maintained by the server?
    [SerializeField] [SyncObject] public readonly SyncList<int> BalloonNetIds = new SyncList<int>();

    [Header("Misc")]
    [SerializeField] Vector3 _spawnPoint; // 183.469 -79.61863

    [Header("Ballon Characteristics")]
    [SerializeField] List<string> _possibleBalloonHeights = new List<string>();

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
    }
    [Server]
    string GetRandomBalloonHeight()
    {
        var random = new System.Random();
        int index = random.Next(_possibleBalloonHeights.Count);
        return _possibleBalloonHeights[index];
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
}
