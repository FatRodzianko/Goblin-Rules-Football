using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfPlayerScore : MonoBehaviour
{
    public int StrokesForCurrentHole = 0;
    public int TotalStrokesForCourse = 0;
    public Dictionary<int, int> HoleWithScores = new Dictionary<int, int>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ResetScoreForNewHole()
    {
        StrokesForCurrentHole = 0;
    }
    public void PlayerHitBall()
    {
        StrokesForCurrentHole++;
        TotalStrokesForCourse++;
    }
}
