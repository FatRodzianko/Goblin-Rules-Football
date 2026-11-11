using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Football's rows on the Single Player / Host Game panel, and the one place a football session is
/// started from. Single player and hosting share these rows, so a rule only has to be written once.
/// </summary>
public class FootballSetupPanel : GameSetupPanel
{
    private const int DefaultSecondsPerHalf = 60;
    private const int MinSecondsPerHalf = 30;
    private const int MaxSecondsPerHalf = 300;
    private const int MinMercyRulePoints = 21;
    private const int MaxLobbyNameLength = 15;
    private const string DefaultLobbyName = "Player's Lobby";

    [Header("Rows shown only when hosting")]
    [SerializeField] private GameObject[] _hostOnlyRows;
    [SerializeField] private TMP_InputField _lobbyName;
    [SerializeField] private Toggle _friendsOnly;
    [SerializeField] private Toggle _threeVsThree;

    [Header("Rows shown either way")]
    [SerializeField] private TMP_InputField _secondsPerHalf;
    [SerializeField] private Toggle _powerUps;
    [SerializeField] private Toggle _randomEvents;
    [SerializeField] private Toggle _spawnObstacles;
    [SerializeField] private Toggle _mercyRule;
    [SerializeField] private TMP_InputField _mercyRulePoints;

    public override void SetHosting(bool isHosting)
    {
        if (_hostOnlyRows == null)
            return;
        for (int i = 0; i < _hostOnlyRows.Length; i++)
        {
            if (_hostOnlyRows[i] != null)
                _hostOnlyRows[i].SetActive(isHosting);
        }
    }

    public override void CreateGame(bool isHosting)
    {
        NetworkManagerGRF game = NetworkManagerGRF.singleton;
        if (game == null)
        {
            Debug.LogWarning("FootballSetupPanel: no NetworkManagerGRF in the scene.");
            return;
        }

        game.ResetForNewSession();

        // The mode fields have to move together, so they only ever come from these setters.
        if (isHosting)
            game.ConfigureFootballLobby(_threeVsThree == null || !_threeVsThree.isOn);
        else
            game.ConfigureSinglePlayerSession();

        ApplyMatchSettings(game, isHosting);

        if (isHosting)
        {
            if (SteamLobby.instance == null)
            {
                Debug.LogWarning("FootballSetupPanel: no SteamLobby in the scene.");
                return;
            }
            ELobbyType lobbyType = (_friendsOnly != null && _friendsOnly.isOn)
                ? ELobbyType.k_ELobbyTypeFriendsOnly
                : ELobbyType.k_ELobbyTypePublic;
            // An empty name gets SteamLobby's default, "<Steam name>'s lobby".
            SteamLobby.instance.CreateFootballLobby(lobbyType, ReadLobbyName());
            return;
        }

        // Single player has no remote peers, so it hosts on loopback Tugboat rather than
        // FishySteamworks, which cannot start when Steam is closed.
        if (!game.UseLoopbackTransport())
            Debug.Log("FootballSetupPanel: could not point the transport at loopback; using the current one.");
        game.StartHost();
    }

    private void ApplyMatchSettings(NetworkManagerGRF game, bool isHosting)
    {
        if (isHosting)
            game.lobbyName = ReadLobbyName();

        game.secondsPerHalf = ReadClampedInt(_secondsPerHalf, DefaultSecondsPerHalf, MinSecondsPerHalf, MaxSecondsPerHalf);
        game.mercyRulePointDifferential = ReadClampedInt(_mercyRulePoints, MinMercyRulePoints, MinMercyRulePoints, int.MaxValue);
        game.powerUpsEnabled = IsOn(_powerUps);
        game.randomEventsEnabled = IsOn(_randomEvents);
        game.spawnObstaclesEnabled = IsOn(_spawnObstacles);
        game.mercyRuleEnabled = IsOn(_mercyRule);
    }

    private string ReadLobbyName()
    {
        if (_lobbyName == null || string.IsNullOrWhiteSpace(_lobbyName.text))
            return DefaultLobbyName;
        string name = _lobbyName.text.Trim();
        return name.Length > MaxLobbyNameLength ? name.Substring(0, MaxLobbyNameLength) : name;
    }

    private static bool IsOn(Toggle toggle)
    {
        return toggle != null && toggle.isOn;
    }

    // A blank box means "use the default"; anything else is clamped rather than refused, so a
    // player cannot start a match with a half of two seconds.
    private static int ReadClampedInt(TMP_InputField field, int fallback, int min, int max)
    {
        if (field == null || string.IsNullOrWhiteSpace(field.text))
            return fallback;
        int value;
        if (!int.TryParse(field.text, out value))
            return fallback;
        return Mathf.Clamp(value, min, max);
    }
}
