using System.IO;
using UnityEngine;

// Limits and checks for course files, which arrive from the Steam Workshop and therefore come from
// other people. None of this guards against a file running code - JsonUtility cannot construct
// types the game did not ask for, and tiles are resolved by looking a GUID up against the game's
// own assets - but a file can still ask the game to do something ruinous with the values it holds.
public static class CustomCourseLimits
{
    // The largest course measured here spans a few hundred cells. A tile far outside that makes a
    // Unity tilemap allocate storage covering the whole span, which hangs or exhausts memory.
    public const int MaxTileCellDistance = 10000;

    // The largest real course here is 2.0 MB and 23,783 tiles across four holes, at 86 bytes a tile.
    public const long MaxCourseFileBytes = 32L * 1024L * 1024L;
    public const int MaxTilesPerTileMap = 200000;
    public const int MaxHolesPerCourse = 50;

    // Long enough for any real name, short enough not to break the UI it is drawn into
    public const int MaxCourseNameLength = 80;

    public static bool IsTilePositionInRange(Vector3Int position)
    {
        return position.x >= -MaxTileCellDistance && position.x <= MaxTileCellDistance
            && position.y >= -MaxTileCellDistance && position.y <= MaxTileCellDistance;
    }

    // A course file carries the name of the file it was saved as. Writing is only ever meant to land
    // in the save folder, so refuse anything carrying a directory part or a traversal.
    public static bool IsSafeSaveFileName(string filename)
    {
        if (string.IsNullOrEmpty(filename))
            return false;

        if (filename.Contains("..") || filename.Contains("/") || filename.Contains("\\") || filename.Contains(":"))
            return false;

        return filename.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
    }

    // Course names out of a file are drawn with TextMeshPro, which parses rich text tags, so a name
    // can otherwise resize or recolour the UI around it, or be long enough to push it apart.
    public static string SanitizeCourseName(string courseName)
    {
        if (string.IsNullOrEmpty(courseName))
            return "";

        string cleaned = courseName.Replace('<', ' ').Replace('>', ' ').Trim();
        if (cleaned.Length > MaxCourseNameLength)
            cleaned = cleaned.Substring(0, MaxCourseNameLength);

        return cleaned;
    }
}
