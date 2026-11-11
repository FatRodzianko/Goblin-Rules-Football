using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One lobby in the JOIN GAME list. Built from a template row in the scene, so the look stays
/// editable without touching code.
///
/// The whole row is the button. That needs saying on the row itself, which is what the action cell
/// on the right is for: without it the list reads as a table and nothing suggests a row can be
/// clicked at all. It doubles as the reason a row cannot be used, so a full lobby says so in place
/// of inviting a click that would be refused.
/// </summary>
[RequireComponent(typeof(Button))]
public class LobbyBrowserRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _modeText;
    [SerializeField] private TextMeshProUGUI _playerCountText;

    [Tooltip("The cell on the right that says what clicking the row does.")]
    [SerializeField] private TextMeshProUGUI _actionText;

    [SerializeField] private string _joinLabel = "JOIN";
    [SerializeField] private string _fullLabel = "FULL";

    private LobbyBrowserPanel _owner;
    private CSteamID _lobbyId;
    private Button _button;

    public void Bind(LobbyBrowserPanel owner, LobbyBrowserPanel.LobbyEntry entry)
    {
        _owner = owner;
        _lobbyId = entry.Id;

        if (_nameText != null)
            _nameText.text = entry.Name;
        if (_modeText != null)
            _modeText.text = entry.CourseName.Length > 0 ? entry.Mode + ": " + entry.CourseName : entry.Mode;
        if (_playerCountText != null)
            _playerCountText.text = entry.Players + "/" + entry.MaxPlayers;

        // A lobby with no room is still worth listing - it says who is playing - but it must not
        // look like something to click.
        bool hasRoom = entry.MaxPlayers <= 0 || entry.Players < entry.MaxPlayers;
        if (_actionText != null)
            _actionText.text = hasRoom ? _joinLabel : _fullLabel;
        if (_button != null)
            _button.interactable = hasRoom;
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (_owner != null)
            _owner.JoinLobby(_lobbyId);
    }
}
