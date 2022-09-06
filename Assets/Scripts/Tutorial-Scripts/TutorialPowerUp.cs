using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPowerUp : MonoBehaviour
{
    [Header("Power Up Stats")]
    [SerializeField] public Sprite mySprite;
    [SerializeField] public string powerUpAbility;
    [SerializeField] public bool isBlueShell = false;
    [SerializeField] public int remainingUses = 1;
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
    public TutorialPlayer myPlayerOwner;
    public TutorialPlayer localPlayerOwner;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (this.gameObject.activeInHierarchy)
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

    private void OnDestroy()
    {
        try
        {
            if (myPlayerOwner.myPowerUps.Contains(this))
            {
                myPlayerOwner.myPowerUps.Remove(this);
                myPlayerOwner.RemoveUsedPowerUps();
            }
        }
        catch (Exception e)
        {
            Debug.Log("PowerUp.cs: Object destroyed. Could not remove from local player owner. " + e);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "goblin-body")
        {
            Debug.Log("PowerUp: Collided with goblin: " + collision.transform.parent.name);
            TutorialGoblinScript goblin = collision.transform.parent.GetComponent<TutorialGoblinScript>();
            TutorialPlayer playerOwner = GameObject.FindGameObjectWithTag("TutorialPlayer").GetComponent<TutorialPlayer>();
            if (goblin.isGoblinGrey)
                return;
            Debug.Log("PowerUp: The player owner of the goblin is: " + playerOwner.name);

            if (playerOwner.myPowerUps.Count < 4)
            {
                Debug.Log("PowerUp: " + playerOwner.name + " has space to pick up powerup " + this.name);

                //
                //
                // Add functionality to tutorial manager to cover this?
                //PowerUpManager.instance.RemovePowerUpFromLists(powerUpNetId);
                //PowerUpManager.instance.AddPowerupOwnedByPlayer(powerUpNetId, playerOwnerNetID);
                //
                //
                //
                this.GetComponent<BoxCollider2D>().enabled = false;
                this.gameObject.SetActive(false);
                this.gameObject.transform.position = new Vector3(100f, 100f, 0f);
                playerOwner.PowerUpPickedUp(this);
                myPlayerOwner = playerOwner;
                DisablePowerUpInHierarchy();
            }
            else
            {
                Debug.Log("PowerUp: " + playerOwner.name + " DOES NOT have enough space to pick up powerup " + this.name);
            }
        }
    }

    void DisablePowerUpInHierarchy()
    {
        if (this.gameObject.activeInHierarchy)
            this.gameObject.SetActive(false);
    }


    public bool UsePowerUp()
    {
        Debug.Log("TutorialPowerUp.cs: UsePowerUp:");
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
        else if (this.powerUpAbility == "bottleBlueShell")
        {
            wasPowerUpUsed = BottleBlueShell();
            return wasPowerUpUsed;
        }
        return wasPowerUpUsed;
    }
    bool HealNormal()
    {
        if (myPlayerOwner != null && myPlayerOwner.selectGoblin != null)
        {
            /*if (!myPlayerOwner.selectGoblin.isGoblinKnockedOut)
            {
                Debug.Log("PowerUp HealNormal: Healing player " + myPlayerOwner.name + "'s goblin " + myPlayerOwner.selectGoblin.name + " to full health");
                myPlayerOwner.selectGoblin.UpdateGoblinHealth(myPlayerOwner.selectGoblin.health, myPlayerOwner.selectGoblin.MaxHealth);
                myPlayerOwner.selectGoblin.RpcPlayPowerUpParticle("healNormal");
                return true;
            }
            else
            {
                return false;
            }*/
            //myPlayerOwner.selectGoblin.StartHealNormal();
            //myPlayerOwner.selectGoblin.RpcPlayPowerUpParticle("healNormal");
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
    
    bool AttackNormal()
    {
        if (myPlayerOwner != null && myPlayerOwner.selectGoblin != null)
        {
            if (!myPlayerOwner.selectGoblin.attackNormal)
            {
                Debug.Log("PowerUp AttackNormal: Player " + myPlayerOwner.name + "'s goblin " + myPlayerOwner.selectGoblin.name + " will have increased attack");
                //myPlayerOwner.selectGoblin.StartAttackNormal();
                //myPlayerOwner.selectGoblin.RpcPlayPowerUpParticle("attackNormal");
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
    
    bool DefenseNormal()
    {
        if (myPlayerOwner != null && myPlayerOwner.selectGoblin != null)
        {
            if (!myPlayerOwner.selectGoblin.defenseNormal)
            {
                Debug.Log("PowerUp DefenseNormal: Player " + myPlayerOwner.name + "'s goblin " + myPlayerOwner.selectGoblin.name + " will have increased defense");
                myPlayerOwner.selectGoblin.StartDefenseNormal();
                myPlayerOwner.selectGoblin.RpcPlayPowerUpParticle("defenseNormal");
                TutorialTeam playerTeam;
                if (myPlayerOwner.isTeamGrey)
                    playerTeam = TutorialTeamManager.instance.greyTeam;
                else
                    playerTeam = TutorialTeamManager.instance.greenTeam;

                if (playerTeam)
                {
                    foreach (TutorialGoblinScript goblin in playerTeam.goblins)
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
    
    bool SpeedNormal()
    {
        if (myPlayerOwner != null && myPlayerOwner.selectGoblin != null)
        {
            if (!myPlayerOwner.selectGoblin.speedNormal)
            {
                Debug.Log("PowerUp SpeedNormal: Player " + myPlayerOwner.name + "'s goblin " + myPlayerOwner.selectGoblin.name + " will have increased speed");
                //myPlayerOwner.selectGoblin.StartSpeedNormal();
                //myPlayerOwner.selectGoblin.RpcPlayPowerUpParticle("speedNormal");
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
    
    bool LightningBlueShell()
    {
        if (myPlayerOwner != null)
        {

            Team teamToStrike;

            // new thing always strike opposing team
            if (myPlayerOwner.isTeamGrey)
                teamToStrike = TeamManager.instance.greenTeam;
            else
                teamToStrike = TeamManager.instance.greyTeam;

            foreach (GoblinScript goblin in teamToStrike.goblins)
            {
                GameObject lightningStrikeObject = Instantiate(powerUpAnimationPrefab, goblin.transform);
            }

            return true;
        }
        else
            return false;

    }
    
    bool BananaNormal()
    {
        if (myPlayerOwner != null)
        {
            if (myPlayerOwner.selectGoblin.isGoblinKnockedOut)
                return false;
            GameObject bananaObject = Instantiate(thrownObjectPrefab);
            //bananaObject.transform.position = myPlayerOwner.transform.position;
            bananaObject.transform.position = myPlayerOwner.selectGoblin.transform.position;
            //NetworkServer.Spawn(bananaObject);

            PowerUpThrownObject bananaObjectScript = bananaObject.GetComponent<PowerUpThrownObject>();
            //bananaObjectScript.myPlayerOwner = myPlayerOwner;
            bananaObjectScript.isThrown = false;
            bananaObjectScript.throwSpeed = 2.5f;
            bananaObjectScript.isDroppedObject = true;
            bananaObjectScript.dropTime = Time.time;
            //bananaObjectScript.DropBehind(myPlayerOwner);

            return true;
        }
        else
            return false;
    }
    
    bool BottleNormal()
    {
        if (myPlayerOwner != null)
        {
            if (myPlayerOwner.selectGoblin.isGoblinKnockedOut)
                return false;
            GameObject bottleObject = Instantiate(thrownObjectPrefab);
            //bottleObject.transform.position = myPlayerOwner.selectGoblin.transform.position;            
            int directionModifier = 1;
            if (myPlayerOwner.selectGoblin.GetComponent<SpriteRenderer>().flipX)
                directionModifier *= -1;
            Vector3 startingPosition = myPlayerOwner.selectGoblin.transform.position;
            startingPosition.x += (1f * directionModifier);
            startingPosition.y += 0.5f;

            bottleObject.transform.position = startingPosition;
            //NetworkServer.Spawn(bottleObject);

            PowerUpThrownObject bottleObjectScript = bottleObject.GetComponent<PowerUpThrownObject>();
            //bottleObjectScript.myPlayerOwner = myPlayerOwner;
            bottleObjectScript.isThrown = false;
            bottleObjectScript.throwSpeed = 1.5f;
            //bananaObjectScript.isDroppedObject = true;
            //bottleObjectScript.ThrowForward(myPlayerOwner, startingPosition);

            return true;
        }
        else
            return false;
    }
    
    bool InvincibilityBlueShell()
    {
        if (myPlayerOwner != null)
        {
            if (myPlayerOwner.selectGoblin.invinvibilityBlueShell)
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
            //myPlayerOwner.selectGoblin.StartInvinvibilityBlueShell();
            //myPlayerOwner.selectGoblin.RpcPlayPowerUpParticle("invincibilityBlueShell");

            return true;
        }
        else
            return false;
    }
    
    bool GlueNormal()
    {
        if (myPlayerOwner != null)
        {
            if (myPlayerOwner.selectGoblin.isGoblinKnockedOut)
                return false;
            GameObject glueObject = Instantiate(thrownObjectPrefab);
            //glueObject.transform.position = myPlayerOwner.transform.position;
            glueObject.transform.position = myPlayerOwner.selectGoblin.transform.position;
            //NetworkServer.Spawn(glueObject);

            PowerUpThrownObject glueObjectScript = glueObject.GetComponent<PowerUpThrownObject>();
            //glueObjectScript.myPlayerOwner = myPlayerOwner;
            glueObjectScript.isThrown = false;
            glueObjectScript.throwSpeed = 2.5f;
            glueObjectScript.isDroppedObject = true;
            glueObjectScript.dropTime = Time.time;
            //glueObjectScript.DropBehind(myPlayerOwner);

            return true;
        }
        else
            return false;
    }
    
    bool StaminaNormal()
    {
        if (myPlayerOwner != null && myPlayerOwner.selectGoblin != null)
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
    
    bool BottleBlueShell()
    {
        if (myPlayerOwner != null)
        {
            if (myPlayerOwner.selectGoblin.isGoblinKnockedOut)
                return false;

            GameObject bottleObject = Instantiate(thrownObjectPrefab);
            //NetworkServer.Spawn(bottleObject);

            BottleBlueShellSpawner bottleObjectScript = bottleObject.GetComponent<BottleBlueShellSpawner>();
            //bottleObjectScript.myPlayerOwner = this.myPlayerOwner;
            //bottleObjectScript.StartSpawningBottleRockets(this.myPlayerOwner);

            /*Team teamToTarget;
            if (myPlayerOwner.isTeamGrey)
                teamToTarget = TeamManager.instance.greenTeam;
            else
                teamToTarget = TeamManager.instance.greyTeam;*/

            /*foreach (GoblinScript goblin in teamToTarget.goblins)
            {
                GameObject bottleObject = Instantiate(thrownObjectPrefab);
                //bottleObject.transform.position = myPlayerOwner.selectGoblin.transform.position;            
                int directionModifier = 1;
                if (myPlayerOwner.selectGoblin.GetComponent<SpriteRenderer>().flipX)
                    directionModifier *= -1;

                Vector3 startingPosition = myPlayerOwner.selectGoblin.transform.position;
                startingPosition.x += (1.25f * directionModifier);
                startingPosition.y += 0.5f;

                bottleObject.transform.localRotation = Quaternion.Euler(0, 0f, (90f * directionModifier));

                bottleObject.transform.position = startingPosition;
                NetworkServer.Spawn(bottleObject);

                BottleBlueShellScript bottleObjectScript = bottleObject.GetComponent<BottleBlueShellScript>();
                bottleObjectScript.myTarget = goblin.gameObject;
                bottleObjectScript.myTargetNetId = goblin.GetComponent<NetworkIdentity>().netId;

            }*/
            //IEnumerator bottleBlueShellRoutine = BottleBlueShellRoutine(teamToTarget);
            //StartCoroutine(bottleBlueShellRoutine);

            return true;
        }
        else
            return false;
    }

    public void HandleRemainingUses(int oldValue, int newValue)
    {
        remainingUses = newValue;
        
        if (myPlayerOwner)
        {
            myPlayerOwner.UpdatePowerUpRemainingUses();
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
