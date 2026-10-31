using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The AI takeover's decision flow (AI Takeover Plan.md): part A replaces a mid-match leaver with a bot, and part B
// is the play-on offer when a team's last person leaves. A plain MonoBehaviour on purpose: the per-player messages
// ride GamePlayer's RPCs, the way every other per-player message in the game does, so nothing here is networked.
// Server-side only - every entry point is reached from a server path (GamePlayer.OnStopServer, an owner ServerRpc,
// or GameplayManager.HandleTeamAbandoned) - and the prompt's UI lives in TakeoverOfferMenu.
public class TakeoverManager : MonoBehaviour
{
    public static TakeoverManager instance;

    // The surviving humans who have not yet answered the play-on offer. The choice is individual: each client
    // decides only for itself, the host's leave ends the match for everyone, and the game stays paused until this
    // list is empty. Silence counts as leave when the countdown runs out.
    List<GamePlayer> awaitingAnswer = new List<GamePlayer>();
    bool offerPending = false;
    bool abandonedTeamIsGrey = false;
    // Set while an accepted offer's bots are still spawning, so a captain-missing kickoff check in that window does
    // not re-open the offer or forfeit a team that is about to be filled.
    bool fillInProgress = false;

    GameplayManager Gameplay { get { return GameplayManager.instance; } }
    NetworkManagerGRF Game { get { return NetworkManagerGRF.singleton; } }

    public bool IsOfferOrFillInProgress { get { return offerPending || fillInProgress; } }

    // Where each leaver's goblin was when it despawned, by team and goblin type, so the bot that takes over can start
    // there rather than back at its kickoff spot. Recorded by GoblinScript.OnStopServer; a takeover bot's spawn uses
    // it once (CmdSpawnPlayerCharacters). For the play-on fill the leaver is long gone by the time anyone answers.
    Dictionary<string, Vector3> leaverGoblinPositions = new Dictionary<string, Vector3>();
    // Nearby spots to try when the leaver's own spot is taken, nearest first.
    static readonly Vector2[] replacementSpotOffsets = {
        Vector2.zero, new Vector2(0f, 2f), new Vector2(0f, -2f), new Vector2(2f, 0f), new Vector2(-2f, 0f),
        new Vector2(2f, 2f), new Vector2(-2f, 2f), new Vector2(2f, -2f), new Vector2(-2f, -2f) };
    const float replacementSpotRadius = 0.75f;
    // The leaver's ball is respawned loose where they fell; a bot appearing on top of it would hand it straight back.
    const float replacementSpotBallClearance = 2f;
    const float replacementSpotFieldMargin = 1f;
    Collider2D[] replacementSpotHits = new Collider2D[16];

    void Awake()
    {
        instance = this;
    }
    // Part A: decide what a player leaving a team means. Called from the leaver's OnStopServer, after
    // RemovePlayerFromTeam, so the leaver is readable but already off the team. A leaver whose team still has a
    // person is replaced by a bot; a team with no people left goes to GameplayManager.HandleTeamAbandoned, which
    // forfeits or starts the play-on offer.
    public void HandlePlayerLeftTeam(GamePlayer leaver, Team team)
    {
        if (Gameplay == null || Gameplay.gamePhase == "gameover")
            return;
        // An answerer who closes the game instead of choosing counts as having left: take them out of the offer's
        // list, or the game would stay paused waiting on a player who no longer exists.
        if (offerPending && awaitingAnswer.Contains(leaver))
        {
            awaitingAnswer.Remove(leaver);
            CheckAnswersComplete();
        }
        // A bot despawning is not a person leaving; keep the old empty-team check and nothing else.
        if (leaver.isAIPlayer)
        {
            if (team.teamPlayers.Count == 0)
                Gameplay.HandleTeamAbandoned(team.isGrey);
            return;
        }
        // A set piece runs through named players, and its timeout resolves through that same object - a despawned
        // scorer made TimeoutKickAfterRoutine throw and the phase stuck. A leaver who was taking the kick-after
        // forfeits the attempt: the blocked path gets from here to the next kickoff on team references alone, and
        // its guard makes a second call safe. This runs before the abandonment check on purpose: an accepted
        // play-on offer must resume into a live phase, not a kick-after stuck on a goblin that no longer exists.
        if (leaver == Gameplay.scoringPlayer && (Gameplay.gamePhase == "kick-after-attempt" || Gameplay.gamePhase == "touchdown-transition"))
            Gameplay.KickAfterAttemptWasBlocked();
        bool teamStillHasAHuman = false;
        foreach (GamePlayer teammate in team.teamPlayers)
        {
            if (teammate != null && !teammate.isAIPlayer)
            {
                teamStillHasAHuman = true;
                break;
            }
        }
        if (!teamStillHasAHuman)
        {
            // Empty, or bots only. teamPlayers.Count == 0 stopped being the test for an abandoned team the moment
            // bots became teamPlayers; without this a team whose last person left would silently play on as bots.
            Gameplay.HandleTeamAbandoned(team.isGrey);
            return;
        }
        if (Gameplay.is1v1 || Gameplay.isSinglePlayer)
            return;
        ReplaceLeaverWithBot(leaver, team);
    }
    void ReplaceLeaverWithBot(GamePlayer leaver, Team team)
    {
        string leaverName = leaver.PlayerName;
        string leaverGoblinType = leaver.goblinType;
        Debug.Log("ReplaceLeaverWithBot: " + leaverName + " left the " + (team.isGrey ? "grey" : "green") + " team; a bot takes over their " + leaverGoblinType + ".");
        GamePlayer bot = Gameplay.SpawnAIPlayer(team.isGrey, Game.TakePlayerNumber(), "BOT", leaverGoblinType);
        bot.isTakeoverBot = true;
        string announcement = leaverName + " has left and has been replaced by a BOT";
        Gameplay.SendErrorMessageToTeam(true, announcement);
        Gameplay.SendErrorMessageToTeam(false, announcement);
        StartCoroutine(RefreshGoblinTeammatesWhenBotReady(bot, team.isGrey));
    }
    static string LeaverGoblinKey(bool isTeamGrey, string goblinType)
    {
        return (isTeamGrey ? "grey-" : "green-") + goblinType.ToLowerInvariant();
    }
    public void RecordLeaverGoblinPosition(bool isTeamGrey, string goblinType, Vector3 position)
    {
        leaverGoblinPositions[LeaverGoblinKey(isTeamGrey, goblinType)] = position;
    }
    // A takeover bot's goblin starts where the leaver's goblin was, but only in open play - every other phase places
    // the goblins itself (kickoff and kick-after positions, the coin toss), so the starting spot is right there. The
    // spot has to be on the field and clear of obstacles (a trip obstacle or split ground knocks a goblin down on
    // entry), other goblins, power-ups and the loose ball. False means use the kickoff spot.
    public bool TryTakeReplacementPosition(bool isTeamGrey, string goblinType, out Vector3 position)
    {
        position = Vector3.zero;
        string key = LeaverGoblinKey(isTeamGrey, goblinType);
        Vector3 leaverPosition;
        if (!leaverGoblinPositions.TryGetValue(key, out leaverPosition))
            return false;
        leaverGoblinPositions.Remove(key);
        if (Gameplay == null || (Gameplay.gamePhase != "gameplay" && Gameplay.gamePhase != "xtra-time"))
            return false;
        GameObject football = GameObject.FindGameObjectWithTag("football");
        foreach (Vector2 offset in replacementSpotOffsets)
        {
            Vector2 spot = (Vector2)leaverPosition + offset;
            spot.x = Mathf.Clamp(spot.x, Gameplay.minX + replacementSpotFieldMargin, Gameplay.maxX - replacementSpotFieldMargin);
            spot.y = Mathf.Clamp(spot.y, Gameplay.minY + replacementSpotFieldMargin, Gameplay.maxY - replacementSpotFieldMargin);
            if (football != null && Vector2.Distance(spot, football.transform.position) < replacementSpotBallClearance)
                continue;
            if (!IsReplacementSpotClear(spot))
                continue;
            position = new Vector3(spot.x, spot.y, 0f);
            Debug.Log("TryTakeReplacementPosition: the " + key + " bot starts at " + position.ToString() + " (the leaver was at " + leaverPosition.ToString() + ").");
            return true;
        }
        Debug.Log("TryTakeReplacementPosition: no clear spot near " + leaverPosition.ToString() + "; the " + key + " bot starts at its kickoff spot.");
        return false;
    }
    bool IsReplacementSpotClear(Vector2 spot)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        int hitCount = Physics2D.OverlapCircle(spot, replacementSpotRadius, filter, replacementSpotHits);
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = replacementSpotHits[i];
            if (hit.GetComponentInParent<ObstacleObject>() != null || hit.GetComponentInParent<SplitGroundEvent>() != null
                || hit.GetComponentInParent<GoblinScript>() != null || hit.GetComponentInParent<PowerUp>() != null)
                return false;
        }
        return true;
    }
    // The replacement's goblin arrives over several ticks: the bot's OnStartClient runs InitializeAIPlayer, whose
    // ServerRpc spawns the goblin, which then adds itself to the Team roster. Only once the goblin exists, and has
    // had a moment to replicate, is the team told to remap its Q and E, and an orphaned kickoff handed over.
    IEnumerator RefreshGoblinTeammatesWhenBotReady(GamePlayer bot, bool isTeamGrey)
    {
        float waited = 0f;
        while (bot != null && bot.myGoblins.Count == 0 && waited < 10f)
        {
            waited += 0.25f;
            yield return new WaitForSeconds(0.25f);
        }
        yield return new WaitForSeconds(0.5f);
        fillInProgress = false;
        // If the leaver was the kicking player, the kickoff is orphaned: nobody holds the ball, and the timeout
        // force-kick reads kickingPlayer.serverSelectGoblin on the departed object and throws. Point the role at
        // the replacement and re-run its kickoff reposition, which places the goblin, hands it the ball and starts
        // the AI kick the normal way. A 1v1 takeover bot goes through the 1v1 reposition instead, whose AI branch
        // is the one single player has always used.
        if (Gameplay != null && Gameplay.gamePhase == "kickoff" && Gameplay.kickingPlayer == null && Gameplay.kickingTeam != null && Gameplay.kickingTeam.isGrey == isTeamGrey && bot != null)
        {
            Debug.Log("RefreshGoblinTeammatesWhenBotReady: the kicking player left mid-kickoff; the replacement bot takes the kickoff.");
            Gameplay.kickingPlayer = bot;
            if (Gameplay.is1v1)
                bot.RpcRepositionTeamForKickOff(true);
            else
                bot.RpcRepositionTeamForKickOff3v3(bot.Owner, true, true);
        }
        // Each human machine refreshes its own Q/E mapping, and the bots it owns, through its player's TargetRpc:
        // a survivor's slot can hold the leaver's destroyed goblin, which compares == null and refills.
        foreach (GamePlayer player in Game.GamePlayers)
        {
            if (player != null && !player.isAIPlayer)
                player.RpcRefreshGoblinTeammates(player.Owner, isTeamGrey);
        }
    }
    // Part B: the play-on offer. GameplayManager.HandleTeamAbandoned starts it; the countdown rides GameplayManager's
    // SyncVar to every client, where it shows on the prompt itself - the shared timeout timer sits under
    // CanvasGameplay, which is inactive until kickoff, so an offer during the coin toss could not use it.
    public void StartTakeoverOffer(bool teamIsGrey)
    {
        if (offerPending)
            return;
        offerPending = true;
        abandonedTeamIsGrey = teamIsGrey;
        Debug.Log("StartTakeoverOffer: the " + (teamIsGrey ? "grey" : "green") + " team has no people left. Asking the other team whether to play on against AI.");
        SetOfferPauseForHumans(true);
        Gameplay.isGamePaused = true;
        awaitingAnswer.Clear();
        foreach (GamePlayer player in Game.GamePlayers)
        {
            if (player != null && !player.isAIPlayer && player.isTeamGrey != teamIsGrey)
            {
                awaitingAnswer.Add(player);
                player.RpcShowTakeoverOffer(player.Owner, true);
            }
        }
        StartCoroutine(OfferCountdownRoutine());
    }
    void SetOfferPauseForHumans(bool pause)
    {
        foreach (GamePlayer player in Game.GamePlayers)
        {
            if (player != null && !player.isAIPlayer)
                player.RpcSetTimeScaleForTakeoverOffer(player.Owner, pause);
        }
    }
    IEnumerator OfferCountdownRoutine()
    {
        int secondsLeft = 30;
        Gameplay.SetTakeoverCountdownDisplay(secondsLeft);
        while (secondsLeft > 0 && offerPending)
        {
            // Real time: the offer pauses the game, and a scaled WaitForSeconds would never elapse at timeScale 0.
            yield return new WaitForSecondsRealtime(1f);
            secondsLeft--;
            Gameplay.SetTakeoverCountdownDisplay(secondsLeft);
        }
        if (!offerPending)
            yield break;
        // Silence counts as leave: an undecided client is sent to the main menu - part A then fills its seat with
        // a bot - and an undecided host ends the match the way its own leave would.
        bool hostTimedOut = false;
        foreach (GamePlayer undecided in new List<GamePlayer>(awaitingAnswer))
        {
            if (undecided == null || undecided.Owner == null)
                continue;
            if (undecided.Owner.IsLocalClient)
            {
                hostTimedOut = true;
                continue;
            }
            undecided.RpcShowTakeoverOffer(undecided.Owner, false);
            undecided.RpcLeaveToMainMenu(undecided.Owner);
        }
        awaitingAnswer.Clear();
        if (hostTimedOut)
            ResolveAsForfeit();
        else
            CheckAnswersComplete();
    }
    // Called by GamePlayer.CmdAnswerTakeoverOffer: an owner ServerRpc, so whose answer it is comes free.
    public void HandleTakeoverAnswer(GamePlayer answeringPlayer, bool playOn)
    {
        if (!offerPending || answeringPlayer == null || !awaitingAnswer.Contains(answeringPlayer))
            return;
        Debug.Log("HandleTakeoverAnswer: " + answeringPlayer.PlayerName + " chose " + (playOn ? "to play on." : "to leave."));
        awaitingAnswer.Remove(answeringPlayer);
        answeringPlayer.RpcShowTakeoverOffer(answeringPlayer.Owner, false);
        if (!playOn)
        {
            // The host's leave ends the match for everyone, through the same forfeit the opponent's abandonment
            // earned; a client's leave affects only that client, whose seat part A backfills with a bot.
            if (answeringPlayer.Owner != null && answeringPlayer.Owner.IsLocalClient)
            {
                ResolveAsForfeit();
                return;
            }
            answeringPlayer.RpcLeaveToMainMenu(answeringPlayer.Owner);
        }
        CheckAnswersComplete();
    }
    void CheckAnswersComplete()
    {
        if (!offerPending)
            return;
        // A player who closed the game instead of answering despawns; a destroyed entry compares == null.
        for (int i = awaitingAnswer.Count - 1; i >= 0; i--)
        {
            if (awaitingAnswer[i] == null)
                awaitingAnswer.RemoveAt(i);
        }
        if (awaitingAnswer.Count > 0)
            return;
        offerPending = false;
        Debug.Log("CheckAnswersComplete: every remaining player has answered. Playing on against AI.");
        SetOfferPauseForHumans(false);
        Gameplay.isGamePaused = false;
        FillAbandonedTeamWithBots(abandonedTeamIsGrey);
    }
    void ResolveAsForfeit()
    {
        if (!offerPending)
            return;
        offerPending = false;
        awaitingAnswer.Clear();
        Debug.Log("ResolveAsForfeit: the host left or timed out; the match ends on the forfeit.");
        // (no shared-timer cleanup: the countdown lives on the prompt, which RpcShowTakeoverOffer(false) hides)
        foreach (GamePlayer player in Game.GamePlayers)
        {
            if (player != null && !player.isAIPlayer)
                player.RpcShowTakeoverOffer(player.Owner, false);
        }
        SetOfferPauseForHumans(false);
        Gameplay.isGamePaused = false;
        Gameplay.ForfeitAbandonedTeam(abandonedTeamIsGrey);
    }
    void FillAbandonedTeamWithBots(bool isTeamGrey)
    {
        fillInProgress = true;
        Debug.Log("FillAbandonedTeamWithBots: bots take over the " + (isTeamGrey ? "grey" : "green") + " team.");
        GamePlayer kickoffCandidate;
        if (Gameplay.is1v1)
        {
            // The 1v1 shape is single player's: one AI player whose CmdSpawnPlayerCharacters spawns all three goblins.
            kickoffCandidate = Gameplay.SpawnAIPlayer(isTeamGrey, Game.TakePlayerNumber(), "BOT", "");
        }
        else
        {
            // An abandoned team is one with no HUMANS left - bots it was already playing with are alive and keep
            // their goblins, so only the missing types are spawned.
            List<string> missingTypes = new List<string> { "Grenadier", "Berserker", "Skirmisher" };
            GamePlayer grenadierOwner = null;
            Team abandonedTeam = isTeamGrey ? TeamManager.instance.greyTeam : TeamManager.instance.greenTeam;
            if (abandonedTeam != null)
            {
                foreach (GamePlayer teammate in abandonedTeam.teamPlayers)
                {
                    if (teammate == null)
                        continue;
                    missingTypes.Remove(teammate.goblinType);
                    if (teammate.goblinType == "Grenadier")
                        grenadierOwner = teammate;
                }
            }
            foreach (string missingType in missingTypes)
            {
                GamePlayer newBot = Gameplay.SpawnAIPlayer(isTeamGrey, Game.TakePlayerNumber(), "BOT", missingType);
                newBot.isTakeoverBot = true;
                if (missingType == "Grenadier")
                    grenadierOwner = newBot;
            }
            // The kickoff candidate is whoever owns the Grenadier - a surviving bot or a new one. An existing bot
            // already has its goblin, so the ready-coroutine's wait passes straight through.
            kickoffCandidate = grenadierOwner;
        }
        string announcement = "BOTs have taken over the " + (isTeamGrey ? "Grey" : "Green") + " team";
        Gameplay.SendErrorMessageToTeam(true, announcement);
        Gameplay.SendErrorMessageToTeam(false, announcement);
        StartCoroutine(RefreshGoblinTeammatesWhenBotReady(kickoffCandidate, isTeamGrey));
    }
}
