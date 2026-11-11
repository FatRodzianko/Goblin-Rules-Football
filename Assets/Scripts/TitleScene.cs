using UnityEngine.SceneManagement;

/// <summary>
/// Which scenes count as the title screen, while there are two of them.
///
/// The name of the title scene used to be written out wherever it was needed, which made the second
/// one impossible to run: code that gates on "am I at the menu" would answer no, and code that sends
/// a player back to the menu would send them to the other screen. The worst of those were the two
/// gates in <see cref="NetworkManagerGRF"/>, which disconnect anyone who connects from the wrong
/// scene - a second title screen could be hosted from but never joined.
///
/// Two questions are asked of it, and they are not the same one:
/// <see cref="Is"/> for "is this a title screen", which must accept either; and
/// <see cref="Startup"/> for "where does going back to the menu lead", which is whichever screen
/// this build actually opens with. Taking that from the build settings rather than a constant means
/// swapping which screen ships needs no code change at all.
/// </summary>
public static class TitleScene
{
    public const string Original = "TitleScreen";
    public const string Rebuilt = "TitleScreen_new";

    private static string _startup;

    /// <summary>The title screen this build opens with: whatever sits at build index 0.</summary>
    public static string Startup
    {
        get
        {
            if (_startup == null)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(0);
                string name = string.IsNullOrEmpty(path)
                    ? null
                    : System.IO.Path.GetFileNameWithoutExtension(path);

                // Index 0 is not always a title screen - in the editor it is whatever the build
                // settings happen to list first. Falling back keeps "back to the menu" meaningful.
                _startup = Is(name) ? name : Original;
            }
            return _startup;
        }
    }

    /// <summary>Whether a scene name is one of the title screens.</summary>
    public static bool Is(string sceneName)
    {
        return sceneName == Original || sceneName == Rebuilt;
    }

    /// <summary>Whether the scene the player is looking at now is a title screen.</summary>
    public static bool IsActive()
    {
        return Is(SceneManager.GetActiveScene().name);
    }
}
