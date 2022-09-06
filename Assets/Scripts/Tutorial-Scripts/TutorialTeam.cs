using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTeam : MonoBehaviour
{
    [Header("Team Info")]
    public bool isGrey = false;
    public List<TutorialPlayer> teamPlayers = new List<TutorialPlayer>();
    public TutorialPlayer captain;
    public List<TutorialGoblinScript> goblins = new List<TutorialGoblinScript>();

    [Header("Possession")]
    bool doesTeamHaveBall = false;
    public float possessionPoints = 0f;
    public float gainPossessionPointsRate = 2.5f;
    public bool isGainingPossesionPointsRoutineRunning = false;
    IEnumerator GainPossessionPointsRoutine;
    public bool isLosingPossesionPointsRoutineRunning = false;
    IEnumerator LosePossessionPointsRoutine;
    public bool isNoPossessionCooldownRoutineRunning = false;
    public bool didNoPossessionCooldownRoutineComplete = false;
    IEnumerator NoPossessionCooldownRoutine;
    public float possessionBonus = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void AddPlayerToTeam(TutorialPlayer player)
    {
        if (player != null && !teamPlayers.Contains(player))
        {
            teamPlayers.Add(player);
        }
    }
    public void SendYeehawsToTeammates()
    {
        foreach (TutorialPlayer player in teamPlayers)
        {
            PlayYeehawSound();
        }
    }
    void PlayYeehawSound()
    {
        Debug.Log("RpcPlayYeehawSound: placeholder until YEEHAW sound is added");
        CowboyScript cowboy = GameObject.FindGameObjectWithTag("cowboy").GetComponent<CowboyScript>();
        cowboy.ActivateYeehawText();
        SoundManager.instance.PlaySound("yeehaw", 1.0f);
    }
    void HandlePossessionPoints(float oldValue, float newValue)
    {

            possessionPoints = newValue;
            SetPossessionPointsForAllTeamPlayers(newValue);
        

            if (!this.isGrey)
                TutorialManager.instance.UpdatePossessionBar(true, newValue);
            else
                TutorialManager.instance.UpdatePossessionBar(false, newValue);
        
    }
    void SetPossessionPointsForAllTeamPlayers(float newPointsValue)
    {
        foreach (TutorialPlayer player in this.teamPlayers)
        {
            player.HandlePossessionPoints(player.possessionPoints, newPointsValue);
        }

    }
    void HandlePossessionBonus(float oldValue, float newValue)
    {
        possessionBonus = newValue;
        SetPossessionBonusForAllTeamPlayers(newValue);
        UpdatePossessionSpeedBonusForGoblinTeam(newValue);
    }
    void SetPossessionBonusForAllTeamPlayers(float newBonusValue)
    {
        foreach (TutorialPlayer player in this.teamPlayers)
        {
            player.HandlePossessionBonus(player.possessionBonus, newBonusValue);
        }
    }
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
                //possessionBonus = 1.0f;
                HandlePossessionBonus(this.possessionBonus, 1.0f);
            }
            if (possessionPoints >= 30f && possessionPoints < 50f)
            {
                gainPossessionPointsRate = 2.25f;
                //possessionBonus = 1.1f;
                HandlePossessionBonus(this.possessionBonus, 1.1f);
            }
            if (possessionPoints >= 50f && possessionPoints < 70f)
            {
                gainPossessionPointsRate = 1.5f;
                //possessionBonus = 1.2f;
                HandlePossessionBonus(this.possessionBonus, 1.2f);
            }
            if (possessionPoints >= 70f && possessionPoints < 90f)
            {
                gainPossessionPointsRate = 1.0f;
                //possessionBonus = 1.3f;
                HandlePossessionBonus(this.possessionBonus, 1.3f);
            }
            if (possessionPoints >= 90f)
            {
                gainPossessionPointsRate = 0.75f;
                //possessionBonus = 1.4f;
                HandlePossessionBonus(this.possessionBonus, 1.4f);
            }

            yield return new WaitForSeconds(1.0f);
            possessionPointTracker = possessionPoints + gainPossessionPointsRate;
            if (possessionPointTracker > 100f)
            {
                possessionPointTracker = 100f;
                isMaxValue = true;
            }
            //if (GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time")
            HandlePossessionPoints(this.possessionPoints, possessionPointTracker);
            if (isMaxValue)
            {
                isGainingPossesionPointsRoutineRunning = false;
            }
        }
        yield break;
    }
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
                //possessionBonus = 1.0f;
                HandlePossessionBonus(this.possessionBonus, 1.0f);
            }
            if (possessionPoints >= 30f && possessionPoints < 50f)
            {
                gainPossessionPointsRate = 2.25f;
                //possessionBonus = 1.1f;
                HandlePossessionBonus(this.possessionBonus, 1.1f);
            }
            if (possessionPoints >= 50f && possessionPoints < 70f)
            {
                gainPossessionPointsRate = 1.5f;
                //possessionBonus = 1.2f;
                HandlePossessionBonus(this.possessionBonus, 1.2f);
            }
            if (possessionPoints >= 70f && possessionPoints < 90f)
            {
                gainPossessionPointsRate = 1.0f;
                //possessionBonus = 1.3f;
                HandlePossessionBonus(this.possessionBonus, 1.3f);
            }
            if (possessionPoints >= 90f)
            {
                gainPossessionPointsRate = 0.75f;
                //possessionBonus = 1.4f;
                HandlePossessionBonus(this.possessionBonus, 1.4f);
            }

            yield return new WaitForSeconds(1.0f);
            possessionPointTracker = possessionPoints - (2.5f / gainPossessionPointsRate);

            if (possessionPointTracker <= 0f)
            {
                possessionPointTracker = 0f;
                isMinValue = true;
            }
            //if (GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time")
            HandlePossessionPoints(this.possessionPoints, possessionPointTracker);
            if (isMinValue)
            {
                isLosingPossesionPointsRoutineRunning = false;
            }
        }
        yield break;
    }
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
        HandlePossessionPoints(this.possessionPoints, 0f);
        possessionBonus = 1.0f;
    }
    void UpdatePossessionSpeedBonusForGoblinTeam(float newPossessionSpeedBonus)
    {
        // Calculate new speed bonus for goblins on this player's team to use
        float possessionSpeedBonus = (newPossessionSpeedBonus - 1.0f);
        if (possessionSpeedBonus > 0)
        {
            possessionSpeedBonus /= 3f;
        }
        possessionSpeedBonus += 1.0f;
        foreach (TutorialGoblinScript goblin in goblins)
        {
            goblin.possessionSpeedBonus = possessionSpeedBonus;
        }
    }
}
