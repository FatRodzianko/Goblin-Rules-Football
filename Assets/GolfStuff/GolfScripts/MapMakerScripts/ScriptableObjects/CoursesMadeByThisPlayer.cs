using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapMaker", menuName = "MapMaker Misc/Courses Made By This Player Container")]
public class CoursesMadeByThisPlayer : ScriptableObject
{
    [SerializeField] List<string> _courseIDsMadeByThisPlayer = new List<string>();

    public List<string> CourseIDsMadeByThisPlayer
    {
        get
        {
            return _courseIDsMadeByThisPlayer;
        }
    }
    public void AddCourse(string newCourseId)
    {
        if (_courseIDsMadeByThisPlayer.Contains(newCourseId))
            return;

        _courseIDsMadeByThisPlayer.Add(newCourseId);
    }
}
