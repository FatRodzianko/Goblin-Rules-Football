using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Golf Course", menuName = "Golf Course")]
public class ScriptableCourse : ScriptableObject
{
    public string CourseName;
    public ScriptableHole[] HolesInCourse;
}
