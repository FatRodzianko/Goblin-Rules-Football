using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Managing;
using FishNet.Object.Synchronizing;
using FishNet;

public class StatueSpawner : NetworkBehaviour
{
    public static StatueSpawner instance;

    [Header("Statue Prefabs")]
    [SerializeField] GameObject _badStatue;
    [SerializeField] GameObject _goodStatue;
    // Start is called before the first frame update
    void Awake()
    {
        MakeInstance();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    public void SpawnStatue(Vector3 spawnPos, string statueType)
    {
        if (string.IsNullOrEmpty(statueType))
            return;

        GameObject prefabToSpawn = GetStatuePrefab(statueType);
        if (prefabToSpawn == null)
            return;

        GameObject newStatue = Instantiate(prefabToSpawn);
        InstanceFinder.ServerManager.Spawn(newStatue);
        //newStatue.transform.position = spawnPos;
        newStatue.GetComponent<Statue>().RpcUpdatePosition(spawnPos);
    }
    GameObject GetStatuePrefab(string statueType)
    {
        if (statueType == "bad")
            return _badStatue;
        else if (statueType == "good")
            return _goodStatue;
        else
            return null;
    }
}
