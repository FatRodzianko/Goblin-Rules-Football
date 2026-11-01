using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// The screen a player sees while their lobby moves to a new host: what is happening, and a way out. LobbyHostMigration
// creates it from its prefab, keeps it across the title screen and the lobby loading, and destroys it when the player
// is back in the lobby or the move is over. It covers the title screen the move passes through on the way.
// The Leave button's On Click is wired in the prefab to LeavePressed.
public class MovingLobbyScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI MessageText;
    [SerializeField] Button LeaveButton;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void Start()
    {
        SelectLeaveButton();
    }
    // The title screen selects its own Start button when it loads, which would leave a gamepad on a button this
    // screen covers.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SelectLeaveButton();
    }
    void SelectLeaveButton()
    {
        if (LeaveButton != null && LeaveButton.gameObject.activeInHierarchy && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(LeaveButton.gameObject);
    }
    public void SetMessage(string message)
    {
        if (MessageText != null)
            MessageText.text = message;
    }
    public void ShowLeaveButton(bool show)
    {
        if (LeaveButton == null)
            return;
        LeaveButton.gameObject.SetActive(show);
        if (show)
            SelectLeaveButton();
    }
    public void LeavePressed()
    {
        if (LobbyHostMigration.instance != null)
            LobbyHostMigration.instance.LeaveMovingLobby();
    }
}
