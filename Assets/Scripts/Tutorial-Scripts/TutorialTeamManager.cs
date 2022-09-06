using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTeamManager : MonoBehaviour
{
    public static TutorialTeamManager instance;
    public List<TutorialTeam> teams = new List<TutorialTeam>();
    public TutorialTeam greenTeam;
    public TutorialTeam greyTeam;
    // Start is called before the first frame update
    private void Awake()
    {
        MakeInstance();
        FindTeamObjects();
    }
    private void Start()
    {
        if (greyTeam == null || greenTeam == null || teams.Count <= 0)
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
                TutorialTeam teamObjectScript = teamObject.GetComponent<TutorialTeam>();
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
}
