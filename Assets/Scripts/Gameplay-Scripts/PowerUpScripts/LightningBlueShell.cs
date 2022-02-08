using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class LightningBlueShell : NetworkBehaviour
{
    [SyncVar(hook = nameof(HandleGoblinToStrikeNetId))] public uint goblinToStrikeNetId;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        try
        {
            GameObject goblinToStrikeObject = NetworkIdentity.spawned[goblinToStrikeNetId].gameObject;
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
        if (isServer)
        {
            Debug.Log("LightningBlueShell.cs Strike Goblin: Will try to strike goblin with net id of " + goblinToStrikeNetId.ToString());
            try
            {
                GoblinScript goblinToStrike = NetworkIdentity.spawned[goblinToStrikeNetId].GetComponent<GoblinScript>();
                goblinToStrike.KnockOutGoblin(true);
            }
            catch (Exception e)
            {
                Debug.Log("LightningBlueShell.cs Strike Goblin: failed to strike goblin: " + e);
            }
        }       
    }
    public void DestroyAnimationObject()
    {
        if (isServer)
        {
            NetworkServer.Destroy(this.gameObject);
        }        
    }
    public void HandleGoblinToStrikeNetId(uint oldValue, uint newValue)
    {
        if (isServer)
            goblinToStrikeNetId = newValue;
        if (isClient)
        {
            try
            {
                GameObject goblinToStrikeObject = NetworkIdentity.spawned[goblinToStrikeNetId].gameObject;
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
}
