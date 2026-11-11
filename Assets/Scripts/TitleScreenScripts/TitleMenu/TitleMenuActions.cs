using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The title menu's leaf actions - the entries that leave the screen rather than opening a panel.
/// Wired to a <see cref="TitleMenuEntry"/>'s Activated event in the inspector.
/// </summary>
public class TitleMenuActions : MonoBehaviour
{
    [SerializeField] private string _tutorialSceneName = "Tutorial";
    [SerializeField] private string _mapMakerSceneName = "Golf-MapMaker";

    public void StartTutorial()
    {
        NetworkManagerGRF game = NetworkManagerGRF.singleton;
        if (game != null)
            game.ConfigureTutorialSession();
        SceneManager.LoadScene(_tutorialSceneName);
    }

    public void OpenGolfMapMaker()
    {
        SceneManager.LoadScene(_mapMakerSceneName);
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }
}
