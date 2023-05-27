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
    public void SpawnBallonsForNewHole()
    {
        Debug.Log("SpawnBallonsForNewHole");
        for (int i = 0; i < 3; i++)
        {
            float randomX = UnityEngine.Random.Range(-10f, 10f);
            float randomy = UnityEngine.Random.Range(-10f, 10f);
            Vector3 spawnPos = new Vector3(_spawnPoint.x + randomX, _spawnPoint.y + randomy, _spawnPoint.z);
            SpawnBallon(spawnPos);
        }
    }
    [Server]
    void SpawnBallon(Vector3 pos)
    {
        Debug.Log("SpawnBallon: " + pos.ToString());
        GameObject balloon = Instantiate(_powerUpBallonPrefab);
        InstanceFinder.ServerManager.Spawn(balloon);
        BalloonPowerUp balloonScript = balloon.GetComponent<BalloonPowerUp>();
        _activeBallonPowerUps.Add(balloonScript);
        if (!BalloonNetIds.Contains(balloonScript.ObjectId))
            BalloonNetIds.Add(balloonScript.ObjectId);
        balloonScript.RpcUpdateBalloonPosition(pos);
    }
}
