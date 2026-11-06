using UnityEngine;

// Drop-in play: the map maker hands the hole being edited to a one player golf session so it can be
// tested. This outlives the scene change in both directions, so it is static rather than a
// component: the map maker scene is gone while the hole is being played, and the golf session needs
// to know where to go back to and what to put back when it ends.
public static class MapMakerDropInSession
{
    public const string MapMakerSceneName = "Golf-MapMaker";
    public const string TitleSceneName = "TitleScreen";

    public static bool IsActive { get; private set; }

    // The hole as it stood when drop-in started, unsaved edits included. Restored on the way back.
    public static HoleData HoleBeingTested { get; private set; }

    // The id the temporary one hole course was registered under, so it can be removed again
    public static string TempCourseId { get; private set; }

    // The same course as an object. A normal session learns its course from an RPC sent when the
    // gameplay manager starts, which has not arrived yet when a drop-in starts the hole, so the
    // drop-in hands the course over instead of waiting for it to be looked up.
    public static ScriptableCourse CourseToPlay { get; private set; }

    // The course and hole that were open in the map maker, so the same ones are open again
    // afterwards and the restored tiles have somewhere to be saved to.
    public static string CourseIdBeingEdited { get; private set; }
    public static int HoleNumberBeingEdited { get; private set; }

    // Where a golf session should send the player when it ends. A normal session goes to the title
    // screen; a drop-in goes back to the map maker.
    public static string ReturnScene
    {
        get { return IsActive ? MapMakerSceneName : TitleSceneName; }
    }

    public static void Begin(HoleData holeBeingTested, string tempCourseId, ScriptableCourse courseToPlay, string courseIdBeingEdited, int holeNumberBeingEdited)
    {
        IsActive = true;
        HoleBeingTested = holeBeingTested;
        TempCourseId = tempCourseId;
        CourseToPlay = courseToPlay;
        CourseIdBeingEdited = courseIdBeingEdited;
        HoleNumberBeingEdited = holeNumberBeingEdited;
        Debug.Log("MapMakerDropInSession: Begin: testing a hole under temporary course id: " + tempCourseId);
    }

    // Called by the map maker once it has put the hole back, so a later normal golf session is not
    // mistaken for a drop-in.
    public static void End()
    {
        Debug.Log("MapMakerDropInSession: End");
        IsActive = false;
        HoleBeingTested = null;
        TempCourseId = null;
        CourseToPlay = null;
        CourseIdBeingEdited = null;
        HoleNumberBeingEdited = 0;
    }
}
