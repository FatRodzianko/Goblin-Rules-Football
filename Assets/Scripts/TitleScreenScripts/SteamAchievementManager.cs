using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class SteamAchievementManager : MonoBehaviour
{
    public static SteamAchievementManager instance;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        MakeInstance();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        //SteamUserStats.ResetAllStats(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void TutorialCompleted()
    {
        Debug.Log("SteamAchievementManager: TutorialCompleted");
        bool isAchieved = false;
        SteamUserStats.GetAchievement("GRF_TUTORIAL", out isAchieved);
        if (!isAchieved)
        {
            SteamUserStats.SetAchievement("GRF_TUTORIAL");
        }
        SteamUserStats.StoreStats();
    }
    public void WinSinglePlayer()
    {
        Debug.Log("SteamAchievementManager: WinSinglePlayer");
        bool winSingle = false;
        SteamUserStats.GetAchievement("GRF_WIN_SINGLE", out winSingle);
        if (!winSingle)
            SteamUserStats.SetAchievement("GRF_WIN_SINGLE");
    }
    public void WinningPlayer()
    {
        Debug.Log("SteamAchievementManager: WinningPlayer");

        int wins = 0;
        SteamUserStats.GetStat("total_wins", out wins);
        if (wins < 1000)
        {
            wins++;
            SteamUserStats.SetStat("total_wins", wins);
            if (wins == 1000)
            {
                SteamUserStats.SetAchievement("GRF_WIN_THOUSAND");
            }
            else if (wins >= 100)
            {
                SteamUserStats.SetAchievement("GRF_WIN_HUNDRED");
            }
            else if (wins >= 10)
            {
                SteamUserStats.SetAchievement("GRF_WIN_TEN");
            }
        }
        SteamUserStats.StoreStats();
    }
    public void LoseSinglePlayer()
    {
        Debug.Log("SteamAchievementManager: LoseSinglePlayer");
        bool loseSingle = false;
        SteamUserStats.GetAchievement("GRF_LOSE_SINGLE", out loseSingle);
        if (!loseSingle)
            SteamUserStats.SetAchievement("GRF_LOSE_SINGLE");
    }
    public void LosingPlayer()
    {
        Debug.Log("SteamAchievementManager: LosingPlayer");
        

        int losses = 0;
        SteamUserStats.GetStat("total_losses", out losses);
        if (losses < 1000)
        {
            losses++;
            SteamUserStats.SetStat("total_losses", losses);
            if (losses == 1000)
            {
                SteamUserStats.SetAchievement("GRF_LOSE_THOUSAND");
            }
            else if (losses >= 100)
            {
                SteamUserStats.SetAchievement("GRF_LOSE_HUNDRED");
            }
            else if (losses >= 10)
            {
                SteamUserStats.SetAchievement("GRF_LOSE_TEN");
            }
        }
        SteamUserStats.StoreStats();
    }
    public void TouchdownScored()
    {
        Debug.Log("SteamAchievementManager: TouchdownScored");
        bool firstTouchdown = false;
        SteamUserStats.GetAchievement("GRF_FIRST_TOUCHDOWN", out firstTouchdown);
        if (!firstTouchdown)
            SteamUserStats.SetAchievement("GRF_FIRST_TOUCHDOWN");

        int touchdowns = 0;
        SteamUserStats.GetStat("touchdowns_scored", out touchdowns);
        if (touchdowns < 1000)
        {
            touchdowns++;
            SteamUserStats.SetStat("touchdowns_scored", touchdowns);
            if (touchdowns == 1000)
            {
                SteamUserStats.SetAchievement("GRF_TOUCHDOWN_THOUSAND");
            }
            else if (touchdowns >= 100)
            {
                SteamUserStats.SetAchievement("GRF_TOUCHDOWN_HUNDRED");
            }
            else if (touchdowns >= 10)
            {
                SteamUserStats.SetAchievement("GRF_TOUCHDOWN_TEN");
            }
        }


        SteamUserStats.StoreStats();
    }
    public void KickAfterAttemptMade()
    {
        Debug.Log("SteamAchievementManager: KickAfterAttemptMade");
        bool firstKick = false;
        SteamUserStats.GetAchievement("GRF_MADE_KICK", out firstKick);
        if (!firstKick)
            SteamUserStats.SetAchievement("GRF_MADE_KICK");

        int kicksMade = 0;
        SteamUserStats.GetStat("kicks_made", out kicksMade);
        if (kicksMade < 1000)
        {
            kicksMade++;
            SteamUserStats.SetStat("kicks_made", kicksMade);
            if (kicksMade == 1000)
            {
                SteamUserStats.SetAchievement("GRF_MADE_KICK_THOUSAND");
            }
            else if (kicksMade >= 100)
            {
                SteamUserStats.SetAchievement("GRF_MADE_KICK_HUNDRED");
            }
            else if (kicksMade >= 10)
            {
                SteamUserStats.SetAchievement("GRF_MADE_KICK_TEN");
            }
        }


        SteamUserStats.StoreStats();
    }
    public void YeehawGiven()
    {
        Debug.Log("SteamAchievementManager: YeehawGiven");
        bool yeehawGiven = false;
        SteamUserStats.GetAchievement("GRF_YEEHAW", out yeehawGiven);
        if (!yeehawGiven)
            SteamUserStats.SetAchievement("GRF_YEEHAW");

        int yeehaws = 0;
        SteamUserStats.GetStat("cowboys_yeehawed", out yeehaws);
        if (yeehaws < 1000)
        {
            yeehaws++;
            SteamUserStats.SetStat("cowboys_yeehawed", yeehaws);
            if (yeehaws == 1000)
            {
                SteamUserStats.SetAchievement("GRF_YEEHAW_THOUSAND");
            }
            else if (yeehaws >= 100)
            {
                SteamUserStats.SetAchievement("GRF_YEEHAW_HUNDRED");
            }
        }
        SteamUserStats.StoreStats();
    }
    public void Play1v1()
    {
        Debug.Log("SteamAchievementManager: Play1v1");
        bool did1v1 = false;
        SteamUserStats.GetAchievement("GRF_PLAY_1v1", out did1v1);
        if (!did1v1)
            SteamUserStats.SetAchievement("GRF_PLAY_1v1");

    }
    public void Play3v3()
    {
        Debug.Log("SteamAchievementManager: Play3v3");
        bool did3v3 = false;
        SteamUserStats.GetAchievement("GRF_PLAY_3v3", out did3v3);
        if (!did3v3)
            SteamUserStats.SetAchievement("GRF_PLAY_3v3");

    }
    public void MercyRuleLose()
    {
        Debug.Log("SteamAchievementManager: MercyRuleLose");
        bool mercyLose = false;
        SteamUserStats.GetAchievement("GRF_MERCYRULE_LOSE", out mercyLose);
        if (!mercyLose)
            SteamUserStats.SetAchievement("GRF_MERCYRULE_LOSE");

    }
    public void MercyRuleWin()
    {
        Debug.Log("SteamAchievementManager: MercyRuleWin");
        bool mercyWin = false;
        SteamUserStats.GetAchievement("GRF_MERCYRULE_WIN", out mercyWin);
        if (!mercyWin)
            SteamUserStats.SetAchievement("GRF_MERCYRULE_WIN");

    }
    public void StubbedToe()
    {
        Debug.Log("SteamAchievementManager: StubbedToe");
        bool stubbedToe = false;
        SteamUserStats.GetAchievement("GRF_STUBBED_TOE", out stubbedToe);
        if (!stubbedToe)
            SteamUserStats.SetAchievement("GRF_STUBBED_TOE");

    }
    public void BlockedKick()
    {
        Debug.Log("SteamAchievementManager: BlockedKick");
        bool blockedKick = false;
        SteamUserStats.GetAchievement("GRF_BLOCKED_KICK", out blockedKick);
        if (!blockedKick)
            SteamUserStats.SetAchievement("GRF_BLOCKED_KICK");
    }
}
