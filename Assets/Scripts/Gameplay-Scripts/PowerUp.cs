using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;

public class PowerUp : NetworkBehaviour
{
    int lifeTimeCount = 0;
    IEnumerator lifeTimeCounterRoutine;

    [Header("Power Up Stats")]
    [SerializeField] public Sprite mySprite;
    [SerializeField] public string powerUpAbility;

    public GamePlayer myPlayerOwner;
    public GamePlayer localPlayerOwner;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        lifeTimeCounterRoutine = LifeTimeCounter();
        StartCoroutine(lifeTimeCounterRoutine);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    [ServerCallback]
    IEnumerator LifeTimeCounter()
    {
        while (lifeTimeCount < 15)
        {
            yield return new WaitForSeconds(1.0f);
            lifeTimeCount++;
        }
        PowerUpManager.instance.DestroyPowerUp(this.GetComponent<NetworkIdentity>().netId);
        yield break;
    }
    private void OnDestroy()
    {
        if(isServer)
            StopCoroutine(lifeTimeCounterRoutine);
        if (isClient && localPlayerOwner != null)
        {
            try
            {
                if (localPlayerOwner.myPowerUps.Contains(this))
                {
                    localPlayerOwner.myPowerUps.Remove(this);
                    localPlayerOwner.RemoveUsedPowerUps();
                }
            }
            catch (Exception e)
            {
                Debug.Log("PowerUp.cs: Object destroyed. Could not remove from local player owner. " + e);
            }
                         
        }
    }
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "goblin-body")
        {
            Debug.Log("PowerUp: Collided with goblin: " + collision.transform.parent.name);
            GoblinScript goblin = collision.transform.parent.GetComponent<GoblinScript>();
            GamePlayer playerOwner = NetworkIdentity.spawned[goblin.ownerNetId].GetComponent<GamePlayer>();

            Debug.Log("PowerUp: The player owner of the goblin is: " + playerOwner.PlayerName);

            if (playerOwner.serverPowerUpUints.Count < 4)
            {
                Debug.Log("PowerUp: " + playerOwner.PlayerName + " has space to pick up powerup " + this.name);
                uint powerUpNetId = this.GetComponent<NetworkIdentity>().netId;
                uint playerOwnerNetID = playerOwner.GetComponent<NetworkIdentity>().netId;
                playerOwner.serverPowerUpUints.Add(powerUpNetId);
                PowerUpManager.instance.RemovePowerUpFromLists(powerUpNetId);
                PowerUpManager.instance.AddPowerupOwnedByPlayer(powerUpNetId, playerOwnerNetID);
                this.GetComponent<BoxCollider2D>().enabled = false;
                this.gameObject.SetActive(false);
                this.gameObject.transform.position = new Vector3(100f, 100f, 0f);
                playerOwner.RpcPowerUpPickedUp(playerOwner.connectionToClient, powerUpNetId);
                RpcSetLocalPlayerOwner(playerOwnerNetID);
                myPlayerOwner = playerOwner;
                RpcDisablePowerUpInHierarchy();
            }
            else
            {
                Debug.Log("PowerUp: " + playerOwner.PlayerName + " DOES NOT have enough space to pick up powerup " + this.name);
            }
        }
    }
    [ClientRpc]
    void RpcDisablePowerUpInHierarchy()
    { 
        if(this.gameObject.activeInHierarchy)
            this.gameObject.SetActive(false);
    }
    [ClientRpc]
    void RpcSetLocalPlayerOwner(uint playerOwner)
    {
        GamePlayer newPlayerOwner = NetworkIdentity.spawned[playerOwner].GetComponent<GamePlayer>();
        if (newPlayerOwner != null)
            localPlayerOwner = newPlayerOwner;

    }
    [ServerCallback]
    public void UsePowerUp()
    {
        if (this.powerUpAbility == "healNormal")
        {
            HealNormal();
        }
    }
    [ServerCallback]
    void HealNormal()
    {
        if (myPlayerOwner != null && myPlayerOwner.serverSelectGoblin != null)
        {
            Debug.Log("PowerUp HealNormal: Healing player " + myPlayerOwner.PlayerName + "'s goblin " + myPlayerOwner.serverSelectGoblin.name + " to full health");
            myPlayerOwner.serverSelectGoblin.UpdateGoblinHealth(myPlayerOwner.serverSelectGoblin.health, myPlayerOwner.serverSelectGoblin.MaxHealth);
        }
    }
}
