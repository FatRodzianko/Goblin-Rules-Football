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
    [SerializeField] [SyncVar(hook = nameof(HandleRemainingUses))] public int remainingUses = 1;
    [SerializeField] public bool multipleUses = false;
    [SerializeField] public string aiPowerUpType;

    [Header("Power Up Objects")]
    [SerializeField] GameObject PowerUpImageObject;
    [SerializeField] GameObject PowerUpBorderObject;
    int directionToMoveUpAndDown = 1;
    Vector3 newPosition = Vector3.zero;
    public float bounceSpeed = 0.25f;
    [SerializeField] GameObject powerUpAnimationPrefab;

    [Header("Thrown Objects")]
    [SerializeField] GameObject thrownObjectPrefab;

    [Header("Player Owner Info")]
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
                if (this.isBlueShell)
                {
                    Debug.Log("PowerUp: player is trying to pick up blueshell powerup. Is the player on team grey? " + goblin.isGoblinGrey.ToString());
                    if (!CanPlayerPickUpBlueShell(goblin.isGoblinGrey))
                        return;
                }
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
        // Only allow for powerups to be used during gameplay and xtra-time phases
        if (GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time")
        {
            Debug.Log("UsePowerUp: player used powerup during gameplay or xtra-time phase. current phase: " + GameplayManager.instance.gamePhase);
            //return false;
        }
        else
            return false;

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
        else if (this.powerUpAbility == "bananaNormal")
        {
            wasPowerUpUsed = BananaNormal();
            return wasPowerUpUsed;
        }
        else if (this.powerUpAbility == "bottleNormal")
        {
            wasPowerUpUsed = BottleNormal();
            return wasPowerUpUsed;
        }
        else if (this.powerUpAbility == "invincibilityBlueShell")
        {
            wasPowerUpUsed = InvincibilityBlueShell();
            return wasPowerUpUsed;
        }
        else if (this.powerUpAbility == "glueNormal")
        {
            wasPowerUpUsed = GlueNormal();
            return wasPowerUpUsed;
        }
        else if (this.powerUpAbility == "staminaNormal")
        {
            wasPowerUpUsed = StaminaNormal();
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
            //myPlayerOwner.serverSelectGoblin.StartHealNormal();
            //myPlayerOwner.serverSelectGoblin.RpcPlayPowerUpParticle("healNormal");
            Team playerTeam;
            if (myPlayerOwner.isTeamGrey)
                playerTeam = TeamManager.instance.greyTeam;
            else
                playerTeam = TeamManager.instance.greenTeam;

            if (playerTeam)
            {
                foreach (GoblinScript goblin in playerTeam.goblins)
                {
                    goblin.StartHealNormal();
                    goblin.RpcPlayPowerUpParticle("healNormal");
                }
            }
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
                //myPlayerOwner.serverSelectGoblin.StartAttackNormal();
                //myPlayerOwner.serverSelectGoblin.RpcPlayPowerUpParticle("attackNormal");
                Team playerTeam;
                if (myPlayerOwner.isTeamGrey)
                    playerTeam = TeamManager.instance.greyTeam;
                else
                    playerTeam = TeamManager.instance.greenTeam;

                if (playerTeam)
                {
                    foreach (GoblinScript goblin in playerTeam.goblins)
                    {
                        goblin.StartAttackNormal();
                        goblin.RpcPlayPowerUpParticle("attackNormal");
                    }
                }
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
                //myPlayerOwner.serverSelectGoblin.StartDefenseNormal();
                //myPlayerOwner.serverSelectGoblin.RpcPlayPowerUpParticle("defenseNormal");
                Team playerTeam;
                if (myPlayerOwner.isTeamGrey)
                    playerTeam = TeamManager.instance.greyTeam;
                else
                    playerTeam = TeamManager.instance.greenTeam;

                if (playerTeam)
                {
                    foreach (GoblinScript goblin in playerTeam.goblins)
                    {
                        goblin.StartDefenseNormal();
                        goblin.RpcPlayPowerUpParticle("defenseNormal");
                    }
                }
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
                //myPlayerOwner.serverSelectGoblin.StartSpeedNormal();
                //myPlayerOwner.serverSelectGoblin.RpcPlayPowerUpParticle("speedNormal");
                Team playerTeam;
                if (myPlayerOwner.isTeamGrey)
                    playerTeam = TeamManager.instance.greyTeam;
                else
                    playerTeam = TeamManager.instance.greenTeam;

                if (playerTeam)
                {
                    foreach (GoblinScript goblin in playerTeam.goblins)
                    {
                        goblin.StartSpeedNormal();
                        goblin.RpcPlayPowerUpParticle("speedNormal");
                    }
                }
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
            /*GamePlayer opposingPlayer = null;
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
            //lightningStrikeObject.transform.localPosition = newLocalPosition;*/
            /*GoblinScript goblinToStrike;
            uint goblinToStrikeNetId;
            Football football = GameObject.FindGameObjectWithTag("football").GetComponent<Football>();
            if (football.isHeld && football.goblinWithBallNetId != 0)
            {
                goblinToStrike = NetworkIdentity.spawned[football.goblinWithBallNetId].GetComponent<GoblinScript>();
                goblinToStrikeNetId = football.goblinWithBallNetId;
            }
            else
            {
                Team opposingTeam;
                if (myPlayerOwner.isTeamGrey)
                    opposingTeam = TeamManager.instance.greenTeam;
                else
                    opposingTeam = TeamManager.instance.greyTeam;

            }*/
            Team teamToStrike;
            // old thing where it always stricks the ball carrying team? seems not good?
            /*Football football = GameObject.FindGameObjectWithTag("football").GetComponent<Football>();
            if (football.isHeld && football.goblinWithBallNetId != 0)
            {
                GoblinScript goblinWithBall = NetworkIdentity.spawned[football.goblinWithBallNetId].GetComponent<GoblinScript>();
                if (goblinWithBall.isGoblinGrey)
                    teamToStrike = TeamManager.instance.greyTeam;
                else
                    teamToStrike = TeamManager.instance.greenTeam;
            }
            else
            { 
                if(myPlayerOwner.isTeamGrey)
                    teamToStrike = TeamManager.instance.greenTeam;
                else
                    teamToStrike = TeamManager.instance.greyTeam;
            }*/

            // new thing always strike opposing team
            if (myPlayerOwner.isTeamGrey)
                teamToStrike = TeamManager.instance.greenTeam;
            else
                teamToStrike = TeamManager.instance.greyTeam;

            foreach (GoblinScript goblin in teamToStrike.goblins)
            {
                GameObject lightningStrikeObject = Instantiate(powerUpAnimationPrefab, goblin.transform);
                lightningStrikeObject.GetComponent<LightningBlueShell>().goblinToStrikeNetId = goblin.GetComponent<NetworkIdentity>().netId;
                NetworkServer.Spawn(lightningStrikeObject);
            }

            return true;
        }
        else
            return false;

    }
    [ServerCallback]
    bool BananaNormal()
    {
        if (myPlayerOwner != null)
        {
            if (myPlayerOwner.serverSelectGoblin.isGoblinKnockedOut)
                return false;
            GameObject bananaObject = Instantiate(thrownObjectPrefab);
            //bananaObject.transform.position = myPlayerOwner.transform.position;
            bananaObject.transform.position = myPlayerOwner.serverSelectGoblin.transform.position;
            NetworkServer.Spawn(bananaObject);

            PowerUpThrownObject  bananaObjectScript = bananaObject.GetComponent<PowerUpThrownObject>();
            bananaObjectScript.myPlayerOwner = myPlayerOwner;
            bananaObjectScript.isThrown = false;
            bananaObjectScript.throwSpeed = 2.5f;
            bananaObjectScript.isDroppedObject = true;
            bananaObjectScript.dropTime = Time.time;
            bananaObjectScript.DropBehind(myPlayerOwner);

            return true;
        }
        else
            return false;
    }
    [ServerCallback]
    bool BottleNormal()
    {
        if (myPlayerOwner != null)
        {
            if (myPlayerOwner.serverSelectGoblin.isGoblinKnockedOut)
                return false;
            GameObject bottleObject = Instantiate(thrownObjectPrefab);
            //bottleObject.transform.position = myPlayerOwner.serverSelectGoblin.transform.position;            
            int directionModifier = 1;
            if (myPlayerOwner.serverSelectGoblin.GetComponent<SpriteRenderer>().flipX)
                directionModifier *= -1;
            Vector3 startingPosition = myPlayerOwner.serverSelectGoblin.transform.position;
            startingPosition.x += (1f * directionModifier);
            startingPosition.y += 0.5f;
            
            bottleObject.transform.position = startingPosition;
            NetworkServer.Spawn(bottleObject);

            PowerUpThrownObject bottleObjectScript = bottleObject.GetComponent<PowerUpThrownObject>();
            bottleObjectScript.myPlayerOwner = myPlayerOwner;
            bottleObjectScript.isThrown = false;
            bottleObjectScript.throwSpeed = 1.5f;
            //bananaObjectScript.isDroppedObject = true;
            bottleObjectScript.ThrowForward(myPlayerOwner, startingPosition);

            return true;
        }
        else
            return false;
    }
    [ServerCallback]
    bool InvincibilityBlueShell()
    {
        if (myPlayerOwner != null)
        {
            if (myPlayerOwner.serverSelectGoblin.invinvibilityBlueShell)
                return false;

            // First, heal all goblins on your team
            Team playerTeam;
            if (myPlayerOwner.isTeamGrey)
                playerTeam = TeamManager.instance.greyTeam;
            else
                playerTeam = TeamManager.instance.greenTeam;

            if (playerTeam)
            {
                foreach (GoblinScript goblin in playerTeam.goblins)
                {
                    goblin.StartHealNormal();
                    //goblin.RpcPlayPowerUpParticle("healNormal");
                }
            }

            // Set the selected goblin to invincible
            myPlayerOwner.serverSelectGoblin.StartInvinvibilityBlueShell();
            myPlayerOwner.serverSelectGoblin.RpcPlayPowerUpParticle("invincibilityBlueShell");

            return true;
        }
        else
            return false;
    }
    [ServerCallback]
    bool GlueNormal()
    {
        if (myPlayerOwner != null)
        {
            if (myPlayerOwner.serverSelectGoblin.isGoblinKnockedOut)
                return false;
            GameObject glueObject = Instantiate(thrownObjectPrefab);
            //glueObject.transform.position = myPlayerOwner.transform.position;
            glueObject.transform.position = myPlayerOwner.serverSelectGoblin.transform.position;
            NetworkServer.Spawn(glueObject);

            PowerUpThrownObject glueObjectScript = glueObject.GetComponent<PowerUpThrownObject>();
            glueObjectScript.myPlayerOwner = myPlayerOwner;
            glueObjectScript.isThrown = false;
            glueObjectScript.throwSpeed = 2.5f;
            glueObjectScript.isDroppedObject = true;
            glueObjectScript.dropTime = Time.time;
            glueObjectScript.DropBehind(myPlayerOwner);

            return true;
        }
        else
            return false;
    }
    [ServerCallback]
    bool StaminaNormal()
    {
        if (myPlayerOwner != null && myPlayerOwner.serverSelectGoblin != null)
        {
            Team playerTeam;
            if (myPlayerOwner.isTeamGrey)
                playerTeam = TeamManager.instance.greyTeam;
            else
                playerTeam = TeamManager.instance.greenTeam;

            if (playerTeam)
            {
                foreach (GoblinScript goblin in playerTeam.goblins)
                {
                    goblin.StartStaminaNormal();
                    goblin.RpcPlayPowerUpParticle("staminaNormal");
                }
            }
            return true;
        }
        else
            return false;
    }
    public void HandleRemainingUses(int oldValue, int newValue)
    { 
        if(isServer)
        {
            remainingUses = newValue;
        }
        if (isClient)
        {
            if (localPlayerOwner)
            {
                localPlayerOwner.UpdatePowerUpRemainingUses();
            }
        }
    }
    bool CanPlayerPickUpBlueShell(bool isGoblinGrey)
    {
        bool canPickUp = false;

        if (GameplayManager.instance.greyScore > GameplayManager.instance.greenScore)
        {
            if (isGoblinGrey)
                canPickUp = false;
            else
                canPickUp = true;
        }
        else if (GameplayManager.instance.greyScore < GameplayManager.instance.greenScore)
        {
            if (isGoblinGrey)
                canPickUp = true;
            else
                canPickUp = false;
        }
        else if (GameplayManager.instance.greyScore == GameplayManager.instance.greenScore)
        {
            canPickUp = true;
        }

        return canPickUp;
    }
}
