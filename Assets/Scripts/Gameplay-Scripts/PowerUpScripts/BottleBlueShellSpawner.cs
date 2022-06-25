using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BottleBlueShellSpawner : NetworkBehaviour
{
    [Header("Player Info")]
    public GamePlayer myPlayerOwner;

    [Header("Prefab stuff?")]
    [SerializeField] GameObject thrownObjectPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartSpawningBottleRockets(GamePlayer owner)
    {
        Debug.Log("StartSpawningBottleRockets");
        if (!myPlayerOwner)
            myPlayerOwner = owner;

        Team teamToTarget;
        if (myPlayerOwner.isTeamGrey)
            teamToTarget = TeamManager.instance.greenTeam;
        else
            teamToTarget = TeamManager.instance.greyTeam;

        IEnumerator bottleBlueShellRoutine = BottleBlueShellRoutine(teamToTarget);
        StartCoroutine(bottleBlueShellRoutine);
    }
    IEnumerator BottleBlueShellRoutine(Team teamToTarget)
    {
        foreach (GoblinScript goblin in teamToTarget.goblins)
        {
            if (!(GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time"))
                break;
            Debug.Log("BottleBlueShellRoutine: Spawning new bottle");
            GameObject bottleObject = Instantiate(thrownObjectPrefab);
            //bottleObject.transform.position = myPlayerOwner.serverSelectGoblin.transform.position;            
            int directionModifier = 1;
            if (myPlayerOwner.serverSelectGoblin.GetComponent<SpriteRenderer>().flipX)
                directionModifier *= -1;

            Vector3 startingPosition = myPlayerOwner.serverSelectGoblin.transform.position;
            startingPosition.x += (1.25f * directionModifier);
            startingPosition.y += 0.5f;

            bottleObject.transform.localRotation = Quaternion.Euler(0, 0f, (90f * directionModifier));

            bottleObject.transform.position = startingPosition;
            NetworkServer.Spawn(bottleObject);

            BottleBlueShellScript bottleObjectScript = bottleObject.GetComponent<BottleBlueShellScript>();
            bottleObjectScript.myTarget = goblin.gameObject;
            bottleObjectScript.myTargetNetId = goblin.GetComponent<NetworkIdentity>().netId;

            yield return new WaitForSeconds(0.5f);
        }
        NetworkServer.Destroy(this.gameObject);
        yield break;
    }

}
