using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Managing;
using FishNet.Object.Synchronizing;
using FishNet;


public class PowerUpTopDown : NetworkBehaviour
{
    [Header("Owner Info")]
    [SyncVar(OnChange = nameof(SyncOwnerID))] public int OwnerID;

    [Header("PowerUp Attributes")]
    [SyncVar(OnChange = nameof(SyncPowerUpType))] public string PowerUpType;
    [SyncVar] string _powerUpText;
    Sprite _powerUpSprite;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetOwnerID(int newId)
    {
        OwnerID = newId;
    }
    void SyncOwnerID(int prev, int next, bool asServer)
    {
        if (asServer)
            return;

        Transform owner = InstanceFinder.ClientManager.Objects.Spawned[next].transform;
        this.transform.parent = owner;
    }
    [Server]
    public void SetPowerUpType(string newType)
    {
        PowerUpType = newType;
    }
    void SyncPowerUpType(string prev, string next, bool asServer)
    {
        if (asServer)
            return;
        _powerUpSprite = PowerUpManagerTopDownGolf.instance.GetPowerUpSprite(next);

    }
}
