using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsManager : MonoBehaviour
{
    public bool isStatsOpen = false;
    bool didGetStatsYet = false;
    [SerializeField] TextMeshProUGUI statsButtonText;
    [SerializeField] GameObject ScrollObject;

    [Header("Green Text Objects")]
    [SerializeField] TextMeshProUGUI greenYeehaws;
    [SerializeField] TextMeshProUGUI greenPunchesThrown;
    [SerializeField] TextMeshProUGUI greenPunchesHit;
    [SerializeField] TextMeshProUGUI greenSlideTackles;
    [SerializeField] TextMeshProUGUI greenSlideTacklesHit;
    [SerializeField] TextMeshProUGUI greenTouchdowns;
    [SerializeField] TextMeshProUGUI greenKickAfter;
    [SerializeField] TextMeshProUGUI greenKickAfterPercentage;
    [SerializeField] TextMeshProUGUI greenFumbles;
    [SerializeField] TextMeshProUGUI greenPassess;
    [SerializeField] TextMeshProUGUI greenPowerUps;
    [SerializeField] TextMeshProUGUI greenPowerUpsUsed;
    [SerializeField] TextMeshProUGUI greenBlockedKicks;
    [SerializeField] TextMeshProUGUI greenKicksDownfield;
    [SerializeField] TextMeshProUGUI greenKnockedOut;
    [SerializeField] TextMeshProUGUI greenTripped;

    [Header("Grey Text Objects")]
    [SerializeField] TextMeshProUGUI greyYeehaws;
    [SerializeField] TextMeshProUGUI greyPunchesThrown;
    [SerializeField] TextMeshProUGUI greyPunchesHit;
    [SerializeField] TextMeshProUGUI greySlideTackles;
    [SerializeField] TextMeshProUGUI greySlideTacklesHit;
    [SerializeField] TextMeshProUGUI greyTouchdowns;
    [SerializeField] TextMeshProUGUI greyKickAfter;
    [SerializeField] TextMeshProUGUI greyKickAfterPercentage;
    [SerializeField] TextMeshProUGUI greyFumbles;
    [SerializeField] TextMeshProUGUI greyPassess;
    [SerializeField] TextMeshProUGUI greyPowerUps;
    [SerializeField] TextMeshProUGUI greyPowerUpsUsed;
    [SerializeField] TextMeshProUGUI greyBlockedKicks;
    [SerializeField] TextMeshProUGUI greyKicksDownfield;
    [SerializeField] TextMeshProUGUI greyKnockedOut;
    [SerializeField] TextMeshProUGUI greyTripped;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StatsButtonPressed()
    {
        if (isStatsOpen)
        {
            statsButtonText.text = "Show Stats";
            ScrollObject.GetComponent<ImageAnimation>().ReRollScroll();
            isStatsOpen = !isStatsOpen;
        }
        else
        {
            statsButtonText.text = "Hide Stats";
            if(!didGetStatsYet)
                GetStatsFromTeamObjects();
            ScrollObject.GetComponent<ImageAnimation>().UnScrollHalfTime();
            isStatsOpen = !isStatsOpen;
        }
    }
    void GetStatsFromTeamObjects()
    {
        GameObject[] teamObjects = GameObject.FindGameObjectsWithTag("teamObject");
        if (teamObjects.Length > 0)
        {
            foreach (GameObject teamObject in teamObjects)
            {
                Team team = teamObject.GetComponent<Team>();
                if (team.isGrey)
                    SetGreyStats(team);
                else
                    SetGreenStats(team);
            }
            didGetStatsYet = true;
        }
    }
    void SetGreenStats(Team team)
    {
        greenYeehaws.text = team.cowboysYeehawed.ToString();
        greenPunchesThrown.text = team.punchesThrown.ToString();
        if (team.punchesThrown > 0)
        {
            float punchesHit = (float)((float)team.punchesHit / (float)team.punchesThrown) * 100;
            greenPunchesHit.text = punchesHit.ToString("0.##") + "%";
        }
        else
            greenPunchesHit.text = "0%";

        greenSlideTackles.text = team.slideTackles.ToString();
        if (team.slideTackles > 0)
        {
            float slideTacklesHit = (float)((float)team.slideTacklesHit / (float)team.slideTackles) * 100;
            greenSlideTacklesHit.text = slideTacklesHit.ToString("0.##") + "%";
        }
        else
            greenSlideTacklesHit.text = "0%";
        greenTouchdowns.text = team.touchdownsScored.ToString();
        greenKickAfter.text = team.kickAfterAttempts.ToString();
        if (team.kickAfterAttempts > 0)
        { 
            float kickAftersMade = (float)((float)team.kickAfterAttemptsMade / (float)team.kickAfterAttempts) * 100;
            greenKickAfterPercentage.text = kickAftersMade.ToString("0.##") + "%";
        }
        else
            greenKickAfterPercentage.text = "0%";
        greenFumbles.text = team.fumbles.ToString();
        greenPassess.text = team.passesThrown.ToString();
        greenPowerUps.text = team.powerUpsCollected.ToString();
        greenPowerUpsUsed.text = team.powerUpsUsed.ToString();
        greenBlockedKicks.text = team.kicksBlocked.ToString();
        greenKicksDownfield.text = team.kicksDownfield.ToString();
        greenKnockedOut.text = team.timesKnockedOut.ToString();
        greenTripped.text = team.timesTripped.ToString();
    }
    void SetGreyStats(Team team)
    {
        greyYeehaws.text = team.cowboysYeehawed.ToString();
        greyPunchesThrown.text = team.punchesThrown.ToString();
        if (team.punchesThrown > 0)
        {
            float punchesHit = (float)((float)team.punchesHit / (float)team.punchesThrown) * 100;
            greyPunchesHit.text = punchesHit.ToString("0.##") + "%";
        }
        else
            greyPunchesHit.text = "0%";

        greySlideTackles.text = team.slideTackles.ToString();
        if (team.slideTackles > 0)
        {
            float slideTacklesHit = (float)((float)team.slideTacklesHit / (float)team.slideTackles) * 100;
            greySlideTacklesHit.text = slideTacklesHit.ToString("0.##") + "%";
        }
        else
            greySlideTacklesHit.text = "0%";
        greyTouchdowns.text = team.touchdownsScored.ToString();
        greyKickAfter.text = team.kickAfterAttempts.ToString();
        if (team.kickAfterAttempts > 0)
        {
            float kickAftersMade = (float)((float)team.kickAfterAttemptsMade / (float)team.kickAfterAttempts) * 100;
            greyKickAfterPercentage.text = kickAftersMade.ToString("0.##") + "%";
        }
        else
            greyKickAfterPercentage.text = "0%";
        greyFumbles.text = team.fumbles.ToString();
        greyPassess.text = team.passesThrown.ToString();
        greyPowerUps.text = team.powerUpsCollected.ToString();
        greyPowerUpsUsed.text = team.powerUpsUsed.ToString();
        greyBlockedKicks.text = team.kicksBlocked.ToString();
        greyKicksDownfield.text = team.kicksDownfield.ToString();
        greyKnockedOut.text = team.timesKnockedOut.ToString();
        greyTripped.text = team.timesTripped.ToString();
    }
}
