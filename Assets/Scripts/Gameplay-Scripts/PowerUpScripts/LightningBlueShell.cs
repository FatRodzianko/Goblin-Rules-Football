using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;

public class LightningBlueShell : NetworkBehaviour
{
    private readonly SyncVar<int> _syncGoblinToStrikeNetId = new SyncVar<int>(ObjectLookup.NoObject);
    public int goblinToStrikeNetId { get => _syncGoblinToStrikeNetId.Value; set => _syncGoblinToStrikeNetId.Value = value; }
    // Start is called before the first frame update
    private void Awake()
    {
        _syncGoblinToStrikeNetId.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleGoblinToStrikeNetId(prev, next); };
    }
    void Start()
    {
        
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        try
        {
            GameObject goblinToStrikeObject = ObjectLookup.Get(goblinToStrikeNetId).gameObject;
            this.transform.parent = goblinToStrikeObject.transform;
            this.transform.localPosition = new Vector3(0.33f, 1f, 0f);
        }
        catch (Exception e)
        {
            Debug.Log("LightnightBlueShell.cs: could not set goblin to strike as parent: " + e);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StrikeGoblin()
    {
        if (IsServerInitialized)
        {
            Debug.Log("LightningBlueShell.cs Strike Goblin: Will try to strike goblin with net id of " + goblinToStrikeNetId.ToString());
            try
            {
                GoblinScript goblinToStrike = ObjectLookup.Get(goblinToStrikeNetId).GetComponent<GoblinScript>();
                goblinToStrike.KnockOutGoblin(true);
            }
            catch (Exception e)
            {
                Debug.Log("LightningBlueShell.cs Strike Goblin: failed to strike goblin: " + e);
            }
        }
        if (IsClientInitialized)
            PlayThunderSound();
    }
    public void DestroyAnimationObject()
    {
        if (IsServerInitialized)
        {
            InstanceFinder.ServerManager.Despawn(this.gameObject);
        }        
    }
    public void HandleGoblinToStrikeNetId(int oldValue, int newValue)
    {
        if (IsClientInitialized)
        {
            try
            {
                GameObject goblinToStrikeObject = ObjectLookup.Get(goblinToStrikeNetId).gameObject;
                this.transform.parent = goblinToStrikeObject.transform;
                this.transform.localPosition = new Vector3(0.33f, 1f, 0f);
                //this.gameObject.SetActive(true);
            }
            catch (Exception e)
            {
                Debug.Log("LightnightBlueShell.cs: could not set goblin to strike as parent: " + e);
            }
        }
    }
    [Client]
    public void PlayThunderSound()
    {
        if (IsOnScreen())
        {
            SoundManager.instance.PlaySound("powerup-used-ilightningBlueShell", 0.8f);
        }
    }
    public bool IsOnScreen()
    {
        bool onscreen = false;
        // No camera while a scene is unloading, and despawn paths still ask; nothing is on screen then.
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            return false;
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(this.transform.position);
        if (screenPoint.x < 0 || screenPoint.x > 1)
        {
            onscreen = false;
        }
        else
            onscreen = true;
        return onscreen;
    }
}
