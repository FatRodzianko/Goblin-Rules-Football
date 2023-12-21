using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Available Course List", menuName = "Available Course List")]
public class AvailableCourses : ScriptableObject
{
    // https://discussions.unity.com/t/make-a-public-variable-with-a-private-setter-appear-in-inspector/132173
    [SerializeField] public List<ScriptableCourse> Courses { get { return _availableCourses; }  private set { _availableCourses = value; } }
    [SerializeField] private List<ScriptableCourse> _availableCourses = new List<ScriptableCourse>();

}
