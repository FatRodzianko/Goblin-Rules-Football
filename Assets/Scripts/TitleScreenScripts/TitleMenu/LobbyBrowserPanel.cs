using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;

/// <summary>
/// The JOIN GAME list. Lobbies arrive from <see cref="SteamLobby"/>'s events rather than through a
/// call by name, so this panel and the old title screen's list can both exist.
///
/// Sorting and the name filter are applied to the lobbies already held, so neither costs a round
/// trip to Steam, and the chosen column and direction survive a refresh.
/// </summary>
public class LobbyBrowserPanel : MonoBehaviour
{
    public enum SortColumn { Name, Mode, PlayerCount }

    /// <summary>One lobby as the list sees it. Read once, when Steam reports the lobby's data.</summary>
    public struct LobbyEntry
    {
        public CSteamID Id;
        public string Name;
        public string Mode;
        public string CourseName;
        public int Players;
        public int MaxPlayers;
    }

    [SerializeField] private RectTransform _rowParent;
    [Tooltip("An inactive row in the scene, copied for each lobby.")]
    [SerializeField] private LobbyBrowserRow _rowTemplate;
    [SerializeField] private TMP_InputField _searchField;
    [SerializeField] private TextMeshProUGUI _emptyMessage;

    [Header("Sort direction arrows, shown on the header that is sorting")]
    [SerializeField] private GameObject _nameAscending;
    [SerializeField] private GameObject _nameDescending;
    [SerializeField] private GameObject _modeAscending;
    [SerializeField] private GameObject _modeDescending;
    [SerializeField] private GameObject _countAscending;
    [SerializeField] private GameObject _countDescending;

    private readonly List<LobbyEntry> _lobbies = new List<LobbyEntry>();
    private readonly List<LobbyBrowserRow> _rows = new List<LobbyBrowserRow>();
    private SortColumn _sortBy = SortColumn.Name;
    private bool _ascending = true;

    private void Awake()
    {
        if (_rowTemplate != null)
            _rowTemplate.gameObject.SetActive(false);
        if (_searchField != null)
            _searchField.onValueChanged.AddListener(OnSearchChanged);
        ShowSortArrows();
    }

    private void OnEnable()
    {
        if (SteamLobby.instance != null)
        {
            SteamLobby.instance.LobbyListCleared += OnLobbyListCleared;
            SteamLobby.instance.LobbyInfoReceived += OnLobbyInfoReceived;
        }
        RequestLobbies();
    }

    private void OnDisable()
    {
        if (SteamLobby.instance != null)
        {
            SteamLobby.instance.LobbyListCleared -= OnLobbyListCleared;
            SteamLobby.instance.LobbyInfoReceived -= OnLobbyInfoReceived;
        }
    }

    private void OnDestroy()
    {
        if (_searchField != null)
            _searchField.onValueChanged.RemoveListener(OnSearchChanged);
    }

    /// <summary>
    /// Wired to SEARCH. Asks Steam for a fresh list and keeps the typed name filter, which is
    /// applied as the lobbies arrive. Typing already filters on its own; this also re-queries.
    /// </summary>
    public void Search()
    {
        RequestLobbies();
    }

    /// <summary>
    /// Wired to REFRESH. Clears the name filter and asks Steam again, so the whole list comes back.
    /// That is the difference between the two buttons.
    /// </summary>
    public void RefreshAll()
    {
        if (_searchField != null)
            _searchField.SetTextWithoutNotify(string.Empty);
        RequestLobbies();
    }

    private void RequestLobbies()
    {
        if (SteamLobby.instance == null)
        {
            Debug.Log("LobbyBrowserPanel: no SteamLobby in the scene, so no lobbies to list.");
            Rebuild();
            return;
        }
        SteamLobby.instance.GetListOfLobbies();
    }

    public void SortByName() { ToggleSort(SortColumn.Name); }
    public void SortByMode() { ToggleSort(SortColumn.Mode); }
    public void SortByPlayerCount() { ToggleSort(SortColumn.PlayerCount); }

    /// <summary>Joins a lobby a row was clicked on, unless it has since started or filled up.</summary>
    public void JoinLobby(CSteamID lobbyId)
    {
        if (SteamLobby.instance == null)
            return;
        if (SteamMatchmaking.GetLobbyData(lobbyId, "GameStatus") != "Lobby")
        {
            Debug.Log("LobbyBrowserPanel: lobby " + lobbyId + " is no longer open; dropping it from the list.");
            RemoveLobby(lobbyId);
            return;
        }
        if (NetworkManagerGRF.singleton != null)
            NetworkManagerGRF.singleton.ResetForNewSession();
        // SteamLobby starts the right game from the lobby's GameMode once it has joined.
        SteamLobby.instance.JoinLobby(lobbyId);
    }

    private void ToggleSort(SortColumn column)
    {
        if (_sortBy == column)
            _ascending = !_ascending;
        else
        {
            _sortBy = column;
            _ascending = true;
        }
        ShowSortArrows();
        Rebuild();
    }

    private void OnSearchChanged(string text)
    {
        Rebuild();
    }

    private void OnLobbyListCleared()
    {
        _lobbies.Clear();
        Rebuild();
    }

    private void OnLobbyInfoReceived(CSteamID lobbyId)
    {
        // Only this game's lobbies, and only ones still waiting in their lobby screen.
        if (SteamMatchmaking.GetLobbyData(lobbyId, "GameName") != "GRF")
            return;
        if (SteamMatchmaking.GetLobbyData(lobbyId, "GameStatus") != "Lobby")
            return;

        LobbyEntry entry;
        entry.Id = lobbyId;
        entry.Name = SteamMatchmaking.GetLobbyData(lobbyId, "name") ?? string.Empty;
        entry.Mode = SteamMatchmaking.GetLobbyData(lobbyId, "GameMode") ?? string.Empty;
        entry.CourseName = SteamMatchmaking.GetLobbyData(lobbyId, "CourseName") ?? string.Empty;
        entry.Players = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
        entry.MaxPlayers = SteamMatchmaking.GetLobbyMemberLimit(lobbyId);

        RemoveLobby(lobbyId);
        _lobbies.Add(entry);
        Rebuild();
    }

    private void RemoveLobby(CSteamID lobbyId)
    {
        for (int i = _lobbies.Count - 1; i >= 0; i--)
        {
            if (_lobbies[i].Id == lobbyId)
                _lobbies.RemoveAt(i);
        }
    }

    private void Rebuild()
    {
        List<LobbyEntry> shown = Filtered();
        Sort(shown);

        // Rows are pooled rather than destroyed and remade. Destroy only takes effect at the end of
        // the frame, and Steam delivers a whole list's worth of lobby data within one frame, so
        // recreating would stack every refresh's rows on top of the last.
        if (_rowTemplate != null && _rowParent != null)
        {
            while (_rows.Count < shown.Count)
                _rows.Add(Instantiate(_rowTemplate, _rowParent));

            for (int i = 0; i < _rows.Count; i++)
            {
                bool used = i < shown.Count;
                _rows[i].gameObject.SetActive(used);
                if (used)
                    _rows[i].Bind(this, shown[i]);
            }
        }

        if (_emptyMessage != null)
            _emptyMessage.gameObject.SetActive(shown.Count == 0);
    }

    private List<LobbyEntry> Filtered()
    {
        string search = _searchField != null ? _searchField.text : string.Empty;
        List<LobbyEntry> shown = new List<LobbyEntry>();
        bool filtering = !string.IsNullOrWhiteSpace(search);
        string needle = filtering ? search.Trim().ToLowerInvariant() : string.Empty;

        for (int i = 0; i < _lobbies.Count; i++)
        {
            // The search box filters on the lobby's name only.
            if (filtering && _lobbies[i].Name.ToLowerInvariant().IndexOf(needle) < 0)
                continue;
            shown.Add(_lobbies[i]);
        }
        return shown;
    }

    private void Sort(List<LobbyEntry> entries)
    {
        SortColumn by = _sortBy;
        int direction = _ascending ? 1 : -1;
        entries.Sort(delegate (LobbyEntry a, LobbyEntry b)
        {
            int result;
            if (by == SortColumn.PlayerCount)
                result = a.Players.CompareTo(b.Players);
            else if (by == SortColumn.Mode)
                result = string.Compare(a.Mode, b.Mode, System.StringComparison.OrdinalIgnoreCase);
            else
                result = string.Compare(a.Name, b.Name, System.StringComparison.OrdinalIgnoreCase);

            // Name breaks every tie, so the order never shuffles between identical rows.
            if (result == 0)
                return string.Compare(a.Name, b.Name, System.StringComparison.OrdinalIgnoreCase);
            return result * direction;
        });
    }

    private void ShowSortArrows()
    {
        SetActive(_nameAscending, _sortBy == SortColumn.Name && _ascending);
        SetActive(_nameDescending, _sortBy == SortColumn.Name && !_ascending);
        SetActive(_modeAscending, _sortBy == SortColumn.Mode && _ascending);
        SetActive(_modeDescending, _sortBy == SortColumn.Mode && !_ascending);
        SetActive(_countAscending, _sortBy == SortColumn.PlayerCount && _ascending);
        SetActive(_countDescending, _sortBy == SortColumn.PlayerCount && !_ascending);
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }
}
