using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CourseMarkerType
{
    None,
    TeeOffLocation,
    AimPoint
}
[CreateAssetMenu(fileName = "MapMaker", menuName = "MapMakerObjects/Create Course Marker (map maker)")]
public class MapMakerCourseMarker : MapMakerGroundTileBase
{
    [SerializeField] CourseMarkerType _courseMarkerType;
    public CourseMarkerType CourseMarkerType
    {
        get
        {
            return _courseMarkerType;
        }
    }
}
