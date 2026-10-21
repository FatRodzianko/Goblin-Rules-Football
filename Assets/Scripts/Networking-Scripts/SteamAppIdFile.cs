using System;
using System.IO;
using UnityEngine;

// Steam needs steam_appid.txt next to the executable when the game is started outside of the Steam client.
// FishySteamworks 2.x wrote it before SteamManager initialized Steam; 4.x no longer does, so write it here,
// before the first scene loads.
public static class SteamAppIdFile
{
    const string FileName = "steam_appid.txt";
    const string AppId = "2071510";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void WriteBeforeSteamStarts()
    {
        try
        {
            if (File.Exists(FileName) && File.ReadAllText(FileName).Trim() == AppId)
                return;
            File.WriteAllText(FileName, AppId);
            Debug.Log("SteamAppIdFile: wrote " + FileName + " with app id " + AppId);
        }
        catch (Exception e)
        {
            Debug.LogWarning("SteamAppIdFile: could not write " + FileName + ". Error: " + e.Message);
        }
    }
}
