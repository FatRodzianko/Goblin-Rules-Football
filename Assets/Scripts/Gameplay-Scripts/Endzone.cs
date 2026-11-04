using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class Endzone : NetworkBehaviour
{
    [SerializeField] private BoxCollider2D endzoneCollider;
    public bool isGrey;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // Attack fairness (Attack Fairness Plan.md): the trigger fires on every machine, and the machine that owns the
    // carrier claims the crossing it saw; the server verifies in GoblinScript.CmdReportTouchdown before scoring.
    // This used to be [Server]-only: the server's older view of a client's carrier could score a clear dive short.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag != "touchdown-hitbox" || GameplayManager.instance == null)
            return;
        if (GameplayManager.instance.gamePhase != "gameplay" && GameplayManager.instance.gamePhase != "xtra-time")
            return;
        Debug.Log(this.name + ": collision with touchdown-hitbox");
        GoblinScript goblin = collision.transform.parent.GetComponent<GoblinScript>();
        if (goblin.doesCharacterHaveBall && isGrey != goblin.isGoblinGrey && goblin.IsOwner)
            goblin.ReportTouchdown(this);
    }
    // Whether a position is inside this endzone's collider grown by the given slack: the room the position sync
    // allows between the carrier's machine, where the crossing was seen, and the server, plus the touchdown
    // hitbox's own reach past the goblin's pivot.
    public bool IsNearEndzone(Vector3 position, float slack)
    {
        Bounds bounds = endzoneCollider.bounds;
        bounds.Expand(slack * 2f);
        return bounds.Contains(new Vector3(position.x, position.y, bounds.center.z));
    }
}
