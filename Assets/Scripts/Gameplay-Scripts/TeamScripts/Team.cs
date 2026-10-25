using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class Team : NetworkBehaviour
{
    [Header("Team Info")]
    public bool isGrey = false;
    public List<GamePlayer> teamPlayers = new List<GamePlayer>();
    public GamePlayer captain;
    public List<GoblinScript> goblins = new List<GoblinScript>();
    public readonly SyncList<int> goblinNetIds = new SyncList<int>();
    public readonly SyncList<int> teammateNetIds = new SyncList<int>();
    public bool sentMercyRuleToTeamServer = false;
    public bool sentMercyRuleToTeamClient = false;


    [Header("Team Stats")]
    private readonly SyncVar<int> _syncPunchesThrown = new SyncVar<int>();
    public int punchesThrown { get => _syncPunchesThrown.Value; set => _syncPunchesThrown.Value = value; } //
    private readonly SyncVar<int> _syncPunchesHit = new SyncVar<int>();
    public int punchesHit { get => _syncPunchesHit.Value; set => _syncPunchesHit.Value = value; } //
    private readonly SyncVar<int> _syncSlideTackles = new SyncVar<int>();
    public int slideTackles { get => _syncSlideTackles.Value; set => _syncSlideTackles.Value = value; } //
    private readonly SyncVar<int> _syncSlideTacklesHit = new SyncVar<int>();
    public int slideTacklesHit { get => _syncSlideTacklesHit.Value; set => _syncSlideTacklesHit.Value = value; } //
    private readonly SyncVar<int> _syncKicksDownfield = new SyncVar<int>();
    public int kicksDownfield { get => _syncKicksDownfield.Value; set => _syncKicksDownfield.Value = value; } //
    private readonly SyncVar<int> _syncPassesThrown = new SyncVar<int>();
    public int passesThrown { get => _syncPassesThrown.Value; set => _syncPassesThrown.Value = value; } //
    private readonly SyncVar<int> _syncPowerUpsCollected = new SyncVar<int>();
    public int powerUpsCollected { get => _syncPowerUpsCollected.Value; set => _syncPowerUpsCollected.Value = value; } //
    private readonly SyncVar<int> _syncPowerUpsUsed = new SyncVar<int>();
    public int powerUpsUsed { get => _syncPowerUpsUsed.Value; set => _syncPowerUpsUsed.Value = value; } //
    private readonly SyncVar<int> _syncCowboysYeehawed = new SyncVar<int>();
    public int cowboysYeehawed { get => _syncCowboysYeehawed.Value; set => _syncCowboysYeehawed.Value = value; } //
    private readonly SyncVar<int> _syncTimesKnockedOut = new SyncVar<int>();
    public int timesKnockedOut { get => _syncTimesKnockedOut.Value; set => _syncTimesKnockedOut.Value = value; } //
    private readonly SyncVar<int> _syncTimesTripped = new SyncVar<int>();
    public int timesTripped { get => _syncTimesTripped.Value; set => _syncTimesTripped.Value = value; }
    private readonly SyncVar<int> _syncTouchdownsScored = new SyncVar<int>();
    public int touchdownsScored { get => _syncTouchdownsScored.Value; set => _syncTouchdownsScored.Value = value; } //
    private readonly SyncVar<int> _syncKickAfterAttempts = new SyncVar<int>();
    public int kickAfterAttempts { get => _syncKickAfterAttempts.Value; set => _syncKickAfterAttempts.Value = value; } //
    private readonly SyncVar<int> _syncKickAfterAttemptsMade = new SyncVar<int>();
    public int kickAfterAttemptsMade { get => _syncKickAfterAttemptsMade.Value; set => _syncKickAfterAttemptsMade.Value = value; } //
    private readonly SyncVar<int> _syncKicksBlocked = new SyncVar<int>();
    public int kicksBlocked { get => _syncKicksBlocked.Value; set => _syncKicksBlocked.Value = value; } //
    private readonly SyncVar<int> _syncFumbles = new SyncVar<int>();
    public int fumbles { get => _syncFumbles.Value; set => _syncFumbles.Value = value; } //

    [Header("Possession")]
    private readonly SyncVar<bool> _syncDoesTeamHaveBall = new SyncVar<bool>();
    private bool doesTeamHaveBall { get => _syncDoesTeamHaveBall.Value; set => _syncDoesTeamHaveBall.Value = value; }
    private readonly SyncVar<float> _syncPossessionPoints = new SyncVar<float>();
    public float possessionPoints { get => _syncPossessionPoints.Value; set => _syncPossessionPoints.Value = value; }
    private readonly SyncVar<float> _syncGainPossessionPointsRate = new SyncVar<float>(2.5f);
    public float gainPossessionPointsRate { get => _syncGainPossessionPointsRate.Value; set => _syncGainPossessionPointsRate.Value = value; }
    public bool isGainingPossesionPointsRoutineRunning = false;
    IEnumerator GainPossessionPointsRoutine;
    public bool isLosingPossesionPointsRoutineRunning = false;
    IEnumerator LosePossessionPointsRoutine;
    public bool isNoPossessionCooldownRoutineRunning = false;
    public bool didNoPossessionCooldownRoutineComplete = false;
    IEnumerator NoPossessionCooldownRoutine;
    private readonly SyncVar<float> _syncPossessionBonus = new SyncVar<float>(1.0f);
    public float possessionBonus { get => _syncPossessionBonus.Value; set => _syncPossessionBonus.Value = value; }

    // Start is called before the first frame update
    private void Awake()
    {
        _syncPossessionPoints.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandlePossessionPoints(prev, next); };
        _syncPossessionBonus.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandlePossessionBonus(prev, next); };
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Server]
    public void AddPlayerToTeam(GamePlayer player)
    {
        if (player != null && !teamPlayers.Contains(player))
        {
            teamPlayers.Add(player);
            teammateNetIds.Add(player.GetComponent<NetworkObject>().ObjectId);
        }
    }
    // Called from GamePlayer.OnStopServer, so a player who left the game doesn't stay on the team. The loops over
    // teamPlayers read the player (Owner, possessionPoints), which throws once that object is destroyed.
    // Returns true if the player was on this team, so the caller knows which team to check for being abandoned.
    [Server]
    public bool RemovePlayerFromTeam(GamePlayer player)
    {
        if (player == null || !teamPlayers.Contains(player))
            return false;
        teamPlayers.Remove(player);
        int playerObjectId = player.ObjectId;
        if (teammateNetIds.Contains(playerObjectId))
            teammateNetIds.Remove(playerObjectId);
        if (captain == player)
        {
            captain = null;
            PromoteNewCaptain();
        }
        return true;
    }
    // In a 3v3 the captain is whoever picked the Grenadier (GamePlayer.CmdGetTeamName), and GameplayManager reads
    // team.captain for the kicking, receiving and blocking player at every kickoff and kick-after. If the captain
    // leaves mid-match, hand the job to a teammate who is still here. Nobody left means the team can't kick at all,
    // which GamePlayer.OnStopServer handles by ending the match.
    [Server]
    public void PromoteNewCaptain()
    {
        if (captain != null)
            return;
        foreach (GamePlayer player in teamPlayers)
        {
            if (player == null)
                continue;
            captain = player;
            Debug.Log("PromoteNewCaptain: " + player.PlayerName + " is now the captain of the " + (isGrey ? "grey" : "green") + " team.");
            return;
        }
    }
    [Server]
    public void SendYeehawsToTeammates()
    {
        foreach (GamePlayer player in teamPlayers)
        {
            RpcPlayYeehawSound(player.Owner);
        }
    }
    [TargetRpc]
    void RpcPlayYeehawSound(NetworkConnection target)
    {
        Debug.Log("RpcPlayYeehawSound: placeholder until YEEHAW sound is added");
        CowboyScript cowboy = GameObject.FindGameObjectWithTag("cowboy").GetComponent<CowboyScript>();
        cowboy.ActivateYeehawText();
        SoundManager.instance.PlaySound("yeehaw", 1.0f);
    }
    void HandlePossessionPoints(float oldValue, float newValue)
    {
        if (IsServerInitialized)
        {
            SetPossessionPointsForAllTeamPlayers(newValue);
        }
        if (IsClientInitialized)
        {
            if (!this.isGrey)
                GameplayManager.instance.UpdatePossessionBar(true, newValue);
            else
                GameplayManager.instance.UpdatePossessionBar(false, newValue);
        }
    }
    void SetPossessionPointsForAllTeamPlayers(float newPointsValue)
    {
        foreach (GamePlayer player in this.teamPlayers)
        {
            player.possessionPoints = newPointsValue;
        }

    }
    void HandlePossessionBonus(float oldValue, float newValue)
    {
        if (IsServerInitialized)
        {
            SetPossessionBonusForAllTeamPlayers(newValue);
            UpdatePossessionSpeedBonusForGoblinTeam(newValue);
        }
        if (IsClientInitialized)
        {
            
        }
    }
    void SetPossessionBonusForAllTeamPlayers(float newBonusValue)
    {
        foreach (GamePlayer player in this.teamPlayers)
        {
            player.possessionBonus = newBonusValue;
        }
    }
    [Server]
    public void UpdatePlayerPossessionTracker(bool hasPosession)
    {
        Debug.Log("UpdatePlayerPossessionTracker: Does " + this.name + " have possession of the ball: " + hasPosession.ToString());
        if (hasPosession)
        {
            if (!isGainingPossesionPointsRoutineRunning)
            {
                Debug.Log("UpdatePlayerPossessionTracker: Starting GainPossessionPointsRoutine");
                GainPossessionPointsRoutine = GainPossessionPoints();
                StartCoroutine(GainPossessionPointsRoutine);
            }
            if (isLosingPossesionPointsRoutineRunning)
            {
                Debug.Log("UpdatePlayerPossessionTracker: STOPPING LosePossessionPointsRoutine");
                StopCoroutine(LosePossessionPointsRoutine);
                isLosingPossesionPointsRoutineRunning = false;
            }
            if (isNoPossessionCooldownRoutineRunning)
            {
                Debug.Log("UpdatePlayerPossessionTracker: STOPPING NoPossessionCooldownRoutine");
                StopCoroutine(NoPossessionCooldownRoutine);
                isNoPossessionCooldownRoutineRunning = false;
                didNoPossessionCooldownRoutineComplete = false;
            }
        }
        else
        {
            if (isGainingPossesionPointsRoutineRunning)
            {
                Debug.Log("UpdatePlayerPossessionTracker: STOPPING GainPossessionPointsRoutine");
                StopCoroutine(GainPossessionPointsRoutine);
                isGainingPossesionPointsRoutineRunning = false;
            }
            if (!isNoPossessionCooldownRoutineRunning && !didNoPossessionCooldownRoutineComplete)
            {
                Debug.Log("UpdatePlayerPossessionTracker: Starting NoPossessionCooldown");
                NoPossessionCooldownRoutine = NoPossessionCooldown();
                StartCoroutine(NoPossessionCooldownRoutine);
            }
        }
    }
    [Server]
    IEnumerator GainPossessionPoints()
    {
        isGainingPossesionPointsRoutineRunning = true;
        didNoPossessionCooldownRoutineComplete = false;
        float possessionPointTracker;
        bool isMaxValue = false;
        while (isGainingPossesionPointsRoutineRunning)
        {
            // Set the possession rate. Starts fast, gets slower as it goes on?
            if (possessionPoints < 30f)
            {
                gainPossessionPointsRate = 3.0f;
                possessionBonus = 1.0f;
            }
            if (possessionPoints >= 30f && possessionPoints < 50f)
            {
                gainPossessionPointsRate = 2.25f;
                possessionBonus = 1.1f;
            }
            if (possessionPoints >= 50f && possessionPoints < 70f)
            {
                gainPossessionPointsRate = 1.5f;
                possessionBonus = 1.2f;
            }
            if (possessionPoints >= 70f && possessionPoints < 90f)
            {
                gainPossessionPointsRate = 1.0f;
                possessionBonus = 1.3f;
            }
            if (possessionPoints >= 90f)
            {
                gainPossessionPointsRate = 0.75f;
                possessionBonus = 1.4f;
            }

            yield return new WaitForSeconds(1.0f);
            possessionPointTracker = possessionPoints + gainPossessionPointsRate;
            if (possessionPointTracker > 100f)
            {
                possessionPointTracker = 100f;
                isMaxValue = true;
            }
            if (GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time")
                possessionPoints = possessionPointTracker;
            if (isMaxValue)
            {
                isGainingPossesionPointsRoutineRunning = false;
            }
        }
        yield break;
    }
    [Server]
    IEnumerator NoPossessionCooldown()
    {
        didNoPossessionCooldownRoutineComplete = false;
        isNoPossessionCooldownRoutineRunning = true;
        yield return new WaitForSeconds(2.0f);
        didNoPossessionCooldownRoutineComplete = true;
        isNoPossessionCooldownRoutineRunning = false;

        if (!isLosingPossesionPointsRoutineRunning)
        {
            LosePossessionPointsRoutine = LosePossessionPoints();
            StartCoroutine(LosePossessionPointsRoutine);
        }
    }
    [Server]
    IEnumerator LosePossessionPoints()
    {
        isLosingPossesionPointsRoutineRunning = true;
        float possessionPointTracker;
        bool isMinValue = false;
        while (isLosingPossesionPointsRoutineRunning)
        {
            // Set the possession rate. Starts fast, gets slower as it goes on?
            if (possessionPoints < 30f)
            {
                gainPossessionPointsRate = 3.0f;
                possessionBonus = 1.0f;
            }
            if (possessionPoints >= 30f && possessionPoints < 50f)
            {
                gainPossessionPointsRate = 2.25f;
                possessionBonus = 1.1f;
            }
            if (possessionPoints >= 50f && possessionPoints < 70f)
            {
                gainPossessionPointsRate = 1.5f;
                possessionBonus = 1.2f;
            }
            if (possessionPoints >= 70f && possessionPoints < 90f)
            {
                gainPossessionPointsRate = 1.0f;
                possessionBonus = 1.3f;
            }
            if (possessionPoints >= 90f)
            {
                gainPossessionPointsRate = 0.75f;
                possessionBonus = 1.4f;
            }

            yield return new WaitForSeconds(1.0f);
            possessionPointTracker = possessionPoints - (2.5f / gainPossessionPointsRate);

            if (possessionPointTracker <= 0f)
            {
                possessionPointTracker = 0f;
                isMinValue = true;
            }
            if (GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time")
                possessionPoints = possessionPointTracker;
            if (isMinValue)
            {
                isLosingPossesionPointsRoutineRunning = false;
            }
        }
        yield break;
    }
    [Server]
    public void StopAllPossessionRoutines()
    {
        Debug.Log("StopAllPossessionRoutines for player: " + this.name);
        if (isGainingPossesionPointsRoutineRunning)
        {
            Debug.Log("StopAllPossessionRoutines: STOPPING GainPossessionPointsRoutine");
            StopCoroutine(GainPossessionPointsRoutine);
            isGainingPossesionPointsRoutineRunning = false;
        }
        if (isLosingPossesionPointsRoutineRunning)
        {
            Debug.Log("StopAllPossessionRoutines: STOPPING LosePossessionPointsRoutine");
            StopCoroutine(LosePossessionPointsRoutine);
            isLosingPossesionPointsRoutineRunning = false;

        }
        if (isNoPossessionCooldownRoutineRunning)
        {
            Debug.Log("StopAllPossessionRoutines: STOPPING NoPossessionCooldownRoutine");
            StopCoroutine(NoPossessionCooldownRoutine);
            isNoPossessionCooldownRoutineRunning = false;
        }
        didNoPossessionCooldownRoutineComplete = false;
        possessionPoints = 0f;
        possessionBonus = 1.0f;
    }
    [Server]
    void UpdatePossessionSpeedBonusForGoblinTeam(float newPossessionSpeedBonus)
    {
        // Calculate new speed bonus for goblins on this player's team to use
        float possessionSpeedBonus = (newPossessionSpeedBonus - 1.0f);
        if (possessionSpeedBonus > 0)
        {
            possessionSpeedBonus /= 3f;
        }
        possessionSpeedBonus += 1.0f;
        foreach (GoblinScript goblin in goblins)
        {
            goblin.possessionSpeedBonus = possessionSpeedBonus;
        }
    }
    [Server]
    public void TellTeamMembersMercyRuleEndedGame(bool wonGame)
    {
        Debug.Log("TellTeamMembersMercyRuleEndedGame: for team: " + this.name + " did team win the mercy rule? " + wonGame);
        if (sentMercyRuleToTeamServer)
            return;
        foreach (GamePlayer player in teamPlayers)
        {
            if (GameplayManager.instance.isSinglePlayer)
            {
                if (player.PlayerName == "AIPlayer")
                    continue;
            }
            RpcMercyRuleAchievement(player.Owner, wonGame);
        }
        sentMercyRuleToTeamServer = true;
    }
    [TargetRpc]
    void RpcMercyRuleAchievement(NetworkConnection target, bool wonGame)
    {
        Debug.Log("RpcMercyRuleAchievement: did player win by mercy rule? " + wonGame.ToString());
        if (sentMercyRuleToTeamClient)
            return;
        if (wonGame)
        {
            SteamAchievementManager.instance.MercyRuleWin();
        }
        else
        {
            SteamAchievementManager.instance.MercyRuleLose();
        }
        sentMercyRuleToTeamClient = true;
    }
}
