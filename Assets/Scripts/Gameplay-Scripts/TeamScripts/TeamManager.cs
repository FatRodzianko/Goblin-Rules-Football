using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TeamManager : NetworkBehaviour
{
    public static TeamManager instance;
    public List<Team> teams = new List<Team>();
    // Start is called before the first frame update
    private void Awake()
    {
        MakeInstance();
        FindTeamObjects();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
    void FindTeamObjects()
    {
        GameObject[] teamObjects = GameObject.FindGameObjectsWithTag("teamObject");
        if (teamObjects.Length > 0)
        {
            foreach (GameObject teamObject in teamObjects)
            {
                Team teamObjectScript = teamObject.GetComponent<Team>();
                if (teamObjectScript != null && !teams.Contains(teamObjectScript))
                    teams.Add(teamObjectScript);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void ThrownPunch(GamePlayer player)
    {
        if (player != null)
        {
            foreach (Team team in teams)
            {
                if (team.isGrey == player.isTeamGrey && team.teamPlayers.Contains(player))
                {
                    team.punchesThrown += 1;
                }
            }
        }
    }
    public void PunchHit(GamePlayer player)
    {
        if (player != null)
        {
            foreach (Team team in teams)
            {
                if (team.isGrey == player.isTeamGrey && team.teamPlayers.Contains(player))
                {
                    team.punchesHit += 1;
                }
            }
        }
    }
    public void SlideTackle(GamePlayer player, bool slideHit)
    {
        if (player != null)
        {
            foreach (Team team in teams)
            {
                if (team.isGrey == player.isTeamGrey && team.teamPlayers.Contains(player))
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
}
