using System.Globalization;
using TMPro;
using UnityEngine;

// Fills the hole info panel for whichever player's turn is starting. GameplayManagerTopDownGolf.TurnStarted
// says who that is and everything else is read off that player, so this never has to know whether the
// player is the local one or a remote one.
public class HoleInfoDisplay : MonoBehaviour
{
    [Header("Current Player")]
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _playerPositionText;
    [SerializeField] private TextMeshProUGUI _numberOfStrokesText;
    [SerializeField] private TextMeshProUGUI _distanceToHoleText;
    [SerializeField] private TextMeshProUGUI _terrainTypeText;
    [SerializeField] private PlayerAvatarDisplay _playerAvatar;

    private readonly TextInfo _titleCase = new CultureInfo("en-US", false).TextInfo;

    // Subscribed in Awake rather than OnEnable because the canvas this sits on is switched off while a
    // hole loads, and a turn can start in that window. Text written to an inactive object still shows
    // once it comes back.
    private void Awake()
    {
        GameplayManagerTopDownGolf.TurnStarted += HandleTurnStarted;
    }
    private void OnDestroy()
    {
        GameplayManagerTopDownGolf.TurnStarted -= HandleTurnStarted;
    }
    private void HandleTurnStarted(GolfPlayerTopDown player, bool isTeeOff)
    {
        if (!player)
            return;

        if (_playerNameText)
            _playerNameText.text = player.PlayerName;
        if (_numberOfStrokesText && player.PlayerScore)
            _numberOfStrokesText.text = "Strokes: " + player.PlayerScore.StrokesForCurrentHole.ToString();
        if (_terrainTypeText)
            _terrainTypeText.text = isTeeOff ? string.Empty : _titleCase.ToTitleCase(player.GetTerrainTypeFromBall());
        if (_playerPositionText)
            _playerPositionText.text = PositionTextFor(player);
        if (_distanceToHoleText)
            _distanceToHoleText.text = DistanceTextFor(player);
        if (_playerAvatar)
            _playerAvatar.ShowAvatarFor(SteamIdFor(player));
    }
    string PositionTextFor(GolfPlayerTopDown player)
    {
        if (PlayerScoreBoard.instance == null)
            return string.Empty;

        int position = PlayerScoreBoard.instance.GetPositionFor(player);
        if (position <= 0)
            return string.Empty;

        return Ordinal(position);
    }
    string DistanceTextFor(GolfPlayerTopDown player)
    {
        float distance = player.GetDistanceFromBallToHole();
        if (distance < 0f)
            return string.Empty;

        return "Distance: " + Mathf.RoundToInt(distance).ToString() + "m";
    }
    ulong SteamIdFor(GolfPlayerTopDown player)
    {
        if (GameplayManagerTopDownGolf.instance == null)
            return 0;

        return GameplayManagerTopDownGolf.instance.GetSteamIdFor(player);
    }
    static string Ordinal(int position)
    {
        // 11th, 12th and 13th break the pattern the last digit otherwise follows.
        int lastTwoDigits = position % 100;
        if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
            return position.ToString() + "th";

        switch (position % 10)
        {
            case 1:
                return position.ToString() + "st";
            case 2:
                return position.ToString() + "nd";
            case 3:
                return position.ToString() + "rd";
            default:
                return position.ToString() + "th";
        }
    }
}
