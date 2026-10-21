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
    private readonly SyncVar<int> _syncOwnerID = new SyncVar<int>();
    public int OwnerID { get => _syncOwnerID.Value; set => _syncOwnerID.Value = value; }
    GolfPlayerTopDown _myOwner;

    [Header("PowerUp Attributes")]
    private readonly SyncVar<string> _syncPowerUpType = new SyncVar<string>();
    public string PowerUpType { get => _syncPowerUpType.Value; set => _syncPowerUpType.Value = value; }
    private readonly SyncVar<string> _powerUpText = new SyncVar<string>();
    Sprite _powerUpSprite;
    private readonly SyncVar<bool> _syncHasBeenUsed = new SyncVar<bool>();
    public bool HasBeenUsed { get => _syncHasBeenUsed.Value; set => _syncHasBeenUsed.Value = value; }

    private void Awake()
    {
        _syncOwnerID.OnChange += SyncOwnerID;
        _syncPowerUpType.OnChange += SyncPowerUpType;
    }
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
        _myOwner = owner.GetComponent<GolfPlayerTopDown>();
        _myOwner.NewPowerUpObject(this, false);
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
    [Server]
    public void SetPowerUpText(string newText)
    {
        this._powerUpText.Value = newText;
    }
    private void OnDestroy()
    {
        if (!_myOwner)
            return;
        _myOwner.NewPowerUpObject(this, true);
    }
    //[ObserversRpc(BufferLast = true)]
    //public void RpcSetPowerUpSpriteInPlayerUI(int ownerId, string type)
    //{
    //    Debug.Log("RpcSetPowerUpSpriteInPlayerUI: for palyer with id: " + ownerId.ToString() + " for power up type: " + type);
    //    GolfPlayerTopDown playerOwner = InstanceFinder.ClientManager.Objects.Spawned[ownerId].GetComponent<GolfPlayerTopDown>();
    //    _powerUpSprite = PowerUpManagerTopDownGolf.instance.GetPowerUpSprite(type);
    //    UpdatePowerUpSpriteOnPlayer(playerOwner, _powerUpSprite);
    //}
    //void UpdatePowerUpSpriteOnPlayer(GolfPlayerTopDown playerOwner, Sprite sprite)
    //{
    //    playerOwner.UpdatePlayerPowerUpSprite(sprite);
    //}
}
