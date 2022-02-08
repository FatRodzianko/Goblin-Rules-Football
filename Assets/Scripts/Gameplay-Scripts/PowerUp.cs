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
    [SerializeField] public bool isBlueShell = false;

    [Header("Power Up Objects")]
    [SerializeField] GameObject PowerUpImageObject;
    [SerializeField] GameObject PowerUpBorderObject;
    int directionToMoveUpAndDown = 1;
    Vector3 newPosition = Vector3.zero;
    public float bounceSpeed = 0.25f;
    [SerializeField] GameObject powerUpAnimationPrefab;

    public GamePlayer myPlayerOwner;
    public GamePlayer localPlayerOwner;
    private NetworkManagerGRF game;
    private NetworkManagerGRF Game
    {
        get
        {
            if (game != null)
            {
                return game;
            }
            return game = NetworkManagerGRF.singleton as NetworkManagerGRF;
        }
    }
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
    public override void OnStartClient()
    {
        base.OnStartClient();
        //newPosition = PowerUpImageObject.transform.localPosition;
    }
    // Update is called once per frame
    void Update()
    {
        if (isClient && this.gameObject.activeInHierarchy)
        {
            newPosition.y += Time.deltaTime * directionToMoveUpAndDown * bounceSpeed;
            if (newPosition.y > 0.1)
            {
                newPosition.y = 0.1f;
                directionToMoveUpAndDown = -1;
            }
            if (newPosition.y < -0.1)
            {
                newPosition.y = -0.1f;
                directionToMoveUpAndDown = 1;
            }
            PowerUpImageObject.transform.localPosition = newPosition;
            PowerUpBorderObject.transform.localPosition = newPosition;

        }
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
    public bool UsePowerUp()
    {
        bool wasPowerUpUsed = false;
        if (this.powerUpAbility == "healNormal")
        {
            wasPowerUpUsed = HealNormal();
            return wasPowerUpUsed;
        }
        else if (this.powerUpAbility == "attackNormal")
        {
            wasPowerUpUsed = AttackNormal();
            return wasPowerUpUsed;
        }
        else if (this.powerUpAbility == "defenseNormal")
        {
            wasPowerUpUsed = DefenseNormal();
            return wasPowerUpUsed;
        }
        else if (this.powerUpAbility == "speedNormal")
        {
            wasPowerUpUsed = SpeedNormal();
            return wasPowerUpUsed;
        }
        else if (this.powerUpAbility == "lightningBlueShell")
        {
            wasPowerUpUsed = LightningBlueShell();
            return wasPowerUpUsed;
        }
        return wasPowerUpUsed;
    }
    [ServerCallback]
    bool HealNormal()
    {
        if (myPlayerOwner != null && myPlayerOwner.serverSelectGoblin != null)
        {
            /*if (!myPlayerOwner.serverSelectGoblin.isGoblinKnockedOut)
            {
                Debug.Log("PowerUp HealNormal: Healing player " + myPlayerOwner.PlayerName + "'s goblin " + myPlayerOwner.serverSelectGoblin.name + " to full health");
                myPlayerOwner.serverSelectGoblin.UpdateGoblinHealth(myPlayerOwner.serverSelectGoblin.health, myPlayerOwner.serverSelectGoblin.MaxHealth);
                myPlayerOwner.serverSelectGoblin.RpcPlayPowerUpParticle("healNormal");
                return true;
            }
            else
            {
                return false;
            }*/
            myPlayerOwner.serverSelectGoblin.StartHealNormal();
            myPlayerOwner.serverSelectGoblin.RpcPlayPowerUpParticle("healNormal");
            return true;
        }
        else
            return false;
    }
    [ServerCallback]
    bool AttackNormal()
    {
        if (myPlayerOwner != null && myPlayerOwner.serverSelectGoblin != null)
        {
            if (!myPlayerOwner.serverSelectGoblin.attackNormal)
            {
                Debug.Log("PowerUp AttackNormal: Player " + myPlayerOwner.PlayerName + "'s goblin " + myPlayerOwner.serverSelectGoblin.name + " will have increased attack");
                myPlayerOwner.serverSelectGoblin.StartAttackNormal();
                myPlayerOwner.serverSelectGoblin.RpcPlayPowerUpParticle("attackNormal");
                return true;
            }
            else
                return false;
            
        }
        else
            return false;
    }
    [ServerCallback]
    bool DefenseNormal()
    {
        if (myPlayerOwner != null && myPlayerOwner.serverSelectGoblin != null)
        {
            if (!myPlayerOwner.serverSelectGoblin.defenseNormal)
            {
                Debug.Log("PowerUp DefenseNormal: Player " + myPlayerOwner.PlayerName + "'s goblin " + myPlayerOwner.serverSelectGoblin.name + " will have increased defense");
                myPlayerOwner.serverSelectGoblin.StartDefenseNormal();
                myPlayerOwner.serverSelectGoblin.RpcPlayPowerUpParticle("defenseNormal");
                return true;
            }
            else
                return false;
        }
        else
            return false;
    }
    [ServerCallback]
    bool SpeedNormal()
    {
        if (myPlayerOwner != null && myPlayerOwner.serverSelectGoblin != null)
        {
            if (!myPlayerOwner.serverSelectGoblin.speedNormal)
            {
                Debug.Log("PowerUp SpeedNormal: Player " + myPlayerOwner.PlayerName + "'s goblin " + myPlayerOwner.serverSelectGoblin.name + " will have increased speed");
                myPlayerOwner.serverSelectGoblin.StartSpeedNormal();
                myPlayerOwner.serverSelectGoblin.RpcPlayPowerUpParticle("speedNormal");
                return true;
            }
            else
                return false;
        }
        else
            return false;

    }
    [ServerCallback]
    bool LightningBlueShell()
    {
        if (myPlayerOwner != null)
        {
            GamePlayer opposingPlayer = null;
            foreach (GamePlayer player in Game.GamePlayers)
            {
                if (player.ConnectionId != myPlayerOwner.ConnectionId)
                {
                    opposingPlayer = player;
                    break;
                }
            }
            //opposingPlayer.serverSelectGoblin.KnockOutGoblin(true);
            GoblinScript goblinToStrike = opposingPlayer.serverSelectGoblin;
            GameObject lightningStrikeObject = Instantiate(powerUpAnimationPrefab, opposingPlayer.serverSelectGoblin.transform);

            lightningStrikeObject.GetComponent<LightningBlueShell>().goblinToStrikeNetId = goblinToStrike.GetComponent<NetworkIdentity>().netId;
            //lightningStrikeObject.GetComponent<LightningBlueShell>().HandleGoblinToStrikeNetId(lightningStrikeObject.GetComponent<LightningBlueShell>().goblinToStrikeNetId, goblinToStrike.GetComponent<NetworkIdentity>().netId);
            NetworkServer.Spawn(lightningStrikeObject);

            //Vector3 newLocalPosition = new Vector3(0.33f, 1f, 0f);
            //lightningStrikeObject.transform.localPosition = newLocalPosition;

            return true;
        }
        else
            return false;

    }
}
