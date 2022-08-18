using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TeamManager : NetworkBehaviour
{
    public static TeamManager instance;
    public List<Team> teams = new List<Team>();
    public Team greenTeam;
    public Team greyTeam;
    public float blockedKickTime;
    public float kickAfterAttemptTime;
    // Start is called before the first frame update
    private void Awake()
    {
        MakeInstance();
        FindTeamObjects();
    }
    private void Start()
    {
        if(greyTeam == null || greenTeam == null || teams.Count <= 0)
            FindTeamObjects();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
    void FindTeamObjects()
    {
        Debug.Log("FindTeamObjects");
        GameObject[] teamObjects = GameObject.FindGameObjectsWithTag("teamObject");
        if (teamObjects.Length > 0)
        {
            foreach (GameObject teamObject in teamObjects)
            {
                Team teamObjectScript = teamObject.GetComponent<Team>();
                if (teamObjectScript != null && !teams.Contains(teamObjectScript))
                    teams.Add(teamObjectScript);
                if (teamObjectScript.isGrey)
                    greyTeam = teamObjectScript;
                else
                    greenTeam = teamObjectScript;
            }
        }
    }
    public void GetLocalTeamObjects()
    {
        if (greyTeam == null || greenTeam == null || teams.Count <= 0)
            FindTeamObjects();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void ThrownPunch(GoblinScript goblin)
    {
        /*if (player != null)
        {
            foreach (Team team in teams)
            {
                if (team.isGrey == player.isTeamGrey && team.teamPlayers.Contains(player))
                {
                    team.punchesThrown += 1;
                }
            }
        }*/
        if (goblin != null)
        {
            foreach (Team team in teams)
            {
                if (team.isGrey == goblin.isGoblinGrey && team.goblins.Contains(goblin))
                {
                    team.punchesThrown += 1;
                }
            }
        }

    }
    public void PunchHit(GoblinScript goblin)
    {
        /*if (player != null)
        {
            foreach (Team team in teams)
            {
                if (team.isGrey == player.isTeamGrey && team.teamPlayers.Contains(player))
                {
                    team.punchesHit += 1;
                }
            }
        }*/
        if (goblin != null)
        {
            foreach (Team team in teams)
            {
                if (team.isGrey == goblin.isGoblinGrey && team.goblins.Contains(goblin))
                {
                    team.punchesHit += 1;
                }
            }
        }
    }
    public void SlideTackle(GoblinScript goblin, bool slideHit)
    {
        if (goblin != null)
        {
            foreach (Team team in teams)
            {
                if (team.isGrey == goblin.isGoblinGrey && team.goblins.Contains(goblin))
                {
                    if (slideHit)
                        team.slideTacklesHit += 1;
                    else
                        team.slideTackles += 1;
                }
            }
        }
    }
    public void KickDownfield(GamePlayer player)
    {
        if (player != null)
        {
            foreach (Team team in teams)
            {
                if (team.isGrey == player.isTeamGrey && team.teamPlayers.Contains(player))
                {
                    team.kicksDownfield += 1;
                }
            }
        }
    }
    public void PassThrown(GamePlayer player)
    {
        if (player != null)
        {
            foreach (Team team in teams)
            {
                if (team.isGrey == player.isTeamGrey && team.teamPlayers.Contains(player))
                {
                    team.passesThrown += 1;
                }
            }
        }
    }
    public void PowerUpCollectedOrUsed(GamePlayer player, bool wasUsed)
    {
        if (player != null)
        {
            foreach (Team team in teams)
            {
                if (team.isGrey == player.isTeamGrey && team.teamPlayers.Contains(player))
                {
                    if (wasUsed)
                        team.powerUpsUsed += 1;
                    else
                        team.powerUpsCollected += 1;
                }
            }
        }
    }
    public void TouchdownScored(bool isGrey)
    {
        foreach (Team team in teams)
        {
            if (team.isGrey == isGrey)
            {
                team.touchdownsScored += 1;
            }
        }
    }
    public void KickAfterAttempts(bool isGrey, bool wasGood)
    {
        Debug.Log("KickAfterAttempts: was team grey? " + isGrey.ToString() + " was the kick good? " + wasGood.ToString() + " at time " + Time.time.ToString());
        if (Time.time <= (kickAfterAttemptTime + 0.25f))
            return;
        else
            kickAfterAttemptTime = Time.time;
        foreach (Team team in teams)
        {   
            if (team.isGrey == isGrey)
            {
                team.kickAfterAttempts += 1;
                if (wasGood)
                    team.kickAfterAttemptsMade += 1;
            }
        }
    }
    public void BlockedKick(bool isGrey)
    {
        if (Time.time <= (blockedKickTime + 0.25f))
            return;
        else
            blockedKickTime = Time.time;
        foreach (Team team in teams)
        {
            if (team.isGrey == isGrey)
            {
                team.kicksBlocked += 1;
            }
        }
    }
    public void KnockedOutOrTripped(bool isGrey, bool knockedOut)
    {
        foreach (Team team in teams)
        {
            if (team.isGrey == isGrey)
            {
                if (knockedOut)
                    team.timesKnockedOut += 1;
                else
                    team.timesTripped += 1;
            }
        }
    }
    public void FumbleBall(bool isGrey)
    {
        foreach (Team team in teams)
        {
            if (team.isGrey == isGrey)
            {
                team.fumbles += 1;
            }
        }
    }
    public void YeehawCowboy(GamePlayer player)
    {
        if (player != null)
        {
            foreach (Team team in teams)
            {
                if (team.isGrey == player.isTeamGrey && team.teamPlayers.Contains(player))
                {
                    team.cowboysYeehawed += 1;
                    team.SendYeehawsToTeammates();
                }
            }
        }
    }
}
