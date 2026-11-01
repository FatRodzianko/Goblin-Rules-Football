using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FishNet.Managing;

// Keeps a Football lobby going when its host leaves before the match starts. Every client remembers the lobby while it
// is in it (the host's settings, who is in it, each team's bots, and its own team and goblin). When the host goes, one of
// the remaining players starts hosting with those settings and everyone else reconnects to them and gets their seat back.
// Sits on the NetworkManager prefab next to NetworkManagerGRF and SteamLobby.
// Steam lobbies: Steam hands the lobby's ownership to a remaining member when the host leaves, so every client agrees
// on the new host without asking anyone. The new owner hosts and then writes its id to the lobby's HostAddress; the
// others wait for that and connect to it.
// Dev (Tugboat) sessions have no Steam lobby: the next player in join order takes over on the same address and port,
// which works for instances on one machine.
public class LobbyHostMigration : MonoBehaviour
{
    public static LobbyHostMigration instance;

    private const float ReconnectTimeoutSeconds = 20f;
    private const float ReconnectRetrySeconds = 1f;
    // Gives the old host's socket a moment to close before the new host listens on the same port.
    private const float NewHostStartDelaySeconds = 0.5f;

    private NetworkManager _networkManager;

    // What this client last saw of the lobby. Only refreshed while connected: FishNet despawns the lobby's objects before
    // it reports the connection lost, so a refresh during that teardown would remember a half-empty lobby.
    private bool _hasSeat = false;
    private int _myPlayerNumber;
    private bool _seatIsGrey;
    private string _seatGoblinType = "";
    // The other people in the lobby, by player number, with their names for the moving-lobby screen.
    private readonly Dictionary<int, string> _otherPlayers = new Dictionary<int, string>();
    private int _greyBots;
    private int _greenBots;

    private bool _isMigrating = false;
    private Coroutine _migrationRoutine;
    // Kept for the next lobby this player enters, which is the moved one.
    private bool _hasSeatToRestore = false;
    private int _greyBotsToRestore;
    private int _greenBotsToRestore;
    // The host that left, so a Steam client doesn't reconnect to it while Steam still names it.
    private string _departedHostAddress = "";
    // A new Steam host tells the lobby where to connect once its own player is in, so nobody joins before it.
    private bool _announceAsHost = false;

    // What the player sees while the lobby moves (CanvasMovingLobby).
    [SerializeField] private MovingLobbyScreen movingLobbyScreenPrefab;
    private MovingLobbyScreen _screen;
    private Coroutine _closeScreenRoutine;
    private const float FailureMessageSeconds = 3f;

    public bool IsMigrating => _isMigrating;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
        _networkManager = GetComponent<NetworkManager>();
    }
    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
    // Called whenever the lobby changes on this client.
    public void RememberLobby()
    {
        if (_isMigrating || !_networkManager.ClientManager.Started || SceneManager.GetActiveScene().name != "LobbyScene")
            return;
        NetworkManagerGRF game = NetworkManagerGRF.singleton;
        if (game == null)
            return;
        LobbyPlayer me = null;
        _otherPlayers.Clear();
        _greyBots = 0;
        _greenBots = 0;
        foreach (LobbyPlayer player in game.LobbyPlayers)
        {
            if (player == null)
                continue;
            if (player.isBot)
            {
                if (player.isTeamGrey)
                    _greyBots++;
                else
                    _greenBots++;
            }
            else if (player.IsOwner)
                me = player;
            // The leader is the host who would be leaving, so it is never anyone's successor.
            else if (!player.IsGameLeader)
                _otherPlayers[player.playerNumber] = player.PlayerName;
        }
        _hasSeat = me != null;
        if (me == null)
            return;
        _myPlayerNumber = me.playerNumber;
        _seatIsGrey = me.isTeamGrey;
        _seatGoblinType = me.goblinType;
    }
    // A lobby this player is leaving for good: the title screen, a match starting, or a failed move.
    public void Forget()
    {
        _hasSeat = false;
        _otherPlayers.Clear();
        _isMigrating = false;
        CloseScreen();
        _hasSeatToRestore = false;
        _greyBotsToRestore = 0;
        _greenBotsToRestore = 0;
        _departedHostAddress = "";
        _announceAsHost = false;
        if (_migrationRoutine != null)
        {
            StopCoroutine(_migrationRoutine);
            _migrationRoutine = null;
        }
    }
    // NetworkManagerGRF asks this when its client stops. True means a move is under way, so the session isn't over.
    public bool HandleClientStopped()
    {
        // A reconnect attempt that failed while the new host was still starting. The routine keeps retrying.
        if (_isMigrating)
            return true;
        if (!CanMoveLobby())
            return false;

        _isMigrating = true;
        _hasSeatToRestore = true;
        // Leave the old lobby the way a finished session does, so the moved one loads fresh, exactly as a normal host or
        // join from the title screen. FishNet keeps a scene that is already loaded, so the old LobbyScene, its UI still
        // holding the players that just despawned, would otherwise carry on as the new lobby.
        SceneManager.LoadScene("TitleScreen");
        ShowScreen(MovingMessage(""));
        if (!NetworkManagerGRF.singleton.isDevSession)
        {
            _departedHostAddress = NetworkManagerGRF.singleton.networkAddress;
            Debug.Log("LobbyHostMigration: the host left the lobby. Waiting for Steam to pick the new host.");
            _migrationRoutine = StartCoroutine(SteamMoveRoutine());
            return true;
        }
        bool isNewHost = IsSuccessor();
        Debug.Log("LobbyHostMigration: the host left the lobby. " + (isNewHost ? "This player takes over as host." : "Waiting for the next player to take over."));
        if (isNewHost)
        {
            SetScreenMessage(NewHostMessage());
            _migrationRoutine = StartCoroutine(BecomeHostRoutine());
        }
        else
        {
            SetScreenMessage(MovingMessage(SuccessorName()));
            _migrationRoutine = StartCoroutine(ReconnectRoutine());
        }
        return true;
    }
    // The Leave button on the moving-lobby screen: this player gives up on the move and goes to the title screen.
    public void LeaveMovingLobby()
    {
        if (!_isMigrating)
        {
            CloseScreen();
            return;
        }
        Debug.Log("LobbyHostMigration: this player left the lobby while it was moving.");
        Forget();
        NetworkManagerGRF game = NetworkManagerGRF.singleton;
        if (_networkManager.ServerManager.Started)
            game.StopHost();
        else if (_networkManager.ClientManager.Started)
            game.StopClient();
        else
            game.EndFootballSession();
    }
    bool CanMoveLobby()
    {
        NetworkManagerGRF game = NetworkManagerGRF.singleton;
        if (game == null || !game.IsFootballSession || game.isLeavingOnPurpose)
            return false;
        // A dev session, or a Steam lobby this player is still in. Single player has neither, and no one to move to.
        bool isInSteamLobby = game.HasSteamLobby && SteamLobby.instance != null && SteamLobby.instance.CurrentLobbyID != 0;
        if (!game.isDevSession && !isInSteamLobby)
            return false;
        // The host's own leave never moves its lobby: the host is the one going.
        if (_networkManager.ServerManager.Started)
            return false;
        // Only a lobby this player had a seat in, before Start Game. A player turned away while joining has no seat.
        if (!_hasSeat || !game.hasReceivedMatchSettings || SceneManager.GetActiveScene().name != "LobbyScene")
            return false;
        return true;
    }
    // The next player in join order takes over. Every client works this out from the same lobby, so they agree.
    bool IsSuccessor()
    {
        foreach (int playerNumber in _otherPlayers.Keys)
        {
            if (playerNumber < _myPlayerNumber)
                return false;
        }
        return true;
    }
    // The dev session's successor when it isn't this player: the remaining person who joined first.
    string SuccessorName()
    {
        int lowest = int.MaxValue;
        string name = "";
        foreach (KeyValuePair<int, string> player in _otherPlayers)
        {
            if (player.Key < lowest)
            {
                lowest = player.Key;
                name = player.Value;
            }
        }
        return name;
    }
    IEnumerator BecomeHostRoutine()
    {
        yield return new WaitForSecondsRealtime(NewHostStartDelaySeconds);
        _migrationRoutine = null;
        StartHostingMovedLobby();
    }
    void StartHostingMovedLobby()
    {
        _greyBotsToRestore = _greyBots;
        _greenBotsToRestore = _greenBots;
        NetworkManagerGRF game = NetworkManagerGRF.singleton;
        game.ApplyMatchSettings(game.receivedMatchSettings);
        // If the server can't start, StartHost ends the session, which sends this player to the title screen.
        game.StartHost();
    }
    // Steam has the final say on who hosts: whoever owns the Steam lobby. Checked every second, since the owner can
    // change again if the new host leaves too. Steam keeps naming the old host until it has noticed it is gone.
    IEnumerator SteamMoveRoutine()
    {
        float giveUpTime = Time.realtimeSinceStartup + ReconnectTimeoutSeconds;
        while (Time.realtimeSinceStartup < giveUpTime)
        {
            yield return new WaitForSecondsRealtime(ReconnectRetrySeconds);
            if (_networkManager.ClientManager.Started)
                continue;
            SteamLobby lobby = SteamLobby.instance;
            if (lobby == null || lobby.CurrentLobbyID == 0)
                break;
            if (lobby.IsThisPlayerLobbyOwner())
            {
                Debug.Log("LobbyHostMigration: Steam made this player the lobby's owner. Taking over as host.");
                SetScreenMessage(NewHostMessage());
                _migrationRoutine = null;
                _announceAsHost = true;
                StartHostingMovedLobby();
                yield break;
            }
            string ownerAddress;
            string ownerName;
            if (lobby.TryGetLobbyOwner(out ownerAddress, out ownerName) && ownerAddress != _departedHostAddress)
                SetScreenMessage(MovingMessage(ownerName));
            string hostAddress;
            if (lobby.TryGetLobbyHost(out hostAddress) && hostAddress != _departedHostAddress)
                lobby.JoinFootballHost(hostAddress);
        }
        _migrationRoutine = null;
        GiveUp();
    }
    IEnumerator ReconnectRoutine()
    {
        float giveUpTime = Time.realtimeSinceStartup + ReconnectTimeoutSeconds;
        while (Time.realtimeSinceStartup < giveUpTime)
        {
            yield return new WaitForSecondsRealtime(ReconnectRetrySeconds);
            if (_networkManager.ClientManager.Started)
                continue;
            // Same address and port: in a dev session the new host is on this machine.
            NetworkManagerGRF.singleton.StartClient();
        }
        _migrationRoutine = null;
        GiveUp();
    }
    void GiveUp()
    {
        Debug.Log("LobbyHostMigration: no new host after " + ReconnectTimeoutSeconds + " seconds. Leaving the lobby.");
        Forget();
        // A connection still being set up ends the session when it stops; otherwise there is nothing left to stop.
        if (_networkManager.ClientManager.Started)
            NetworkManagerGRF.singleton.StopClient();
        else
            NetworkManagerGRF.singleton.EndFootballSession();
        // Shown over the title screen for a moment, so the player knows why they are back there. Ending the session
        // calls Forget again, which leaves a timed message alone.
        ShowScreen("The lobby couldn't be moved" + System.Environment.NewLine + "to a new host.");
        if (_screen != null)
            _screen.ShowLeaveButton(false);
        _closeScreenRoutine = StartCoroutine(CloseScreenAfterRoutine(FailureMessageSeconds));
    }
    static string MovingMessage(string newHostName)
    {
        string moving = string.IsNullOrEmpty(newHostName) ? "Moving the lobby..." : "Moving the lobby to " + newHostName + "...";
        return "The host left." + System.Environment.NewLine + moving;
    }
    static string NewHostMessage()
    {
        return "The host left." + System.Environment.NewLine + "You are the new host. Setting up the lobby...";
    }
    void ShowScreen(string message)
    {
        if (_closeScreenRoutine != null)
        {
            StopCoroutine(_closeScreenRoutine);
            _closeScreenRoutine = null;
        }
        if (_screen == null && movingLobbyScreenPrefab != null)
            _screen = Instantiate(movingLobbyScreenPrefab);
        if (_screen == null)
            return;
        _screen.SetMessage(message);
        _screen.ShowLeaveButton(true);
    }
    void SetScreenMessage(string message)
    {
        if (_screen != null)
            _screen.SetMessage(message);
    }
    void CloseScreen()
    {
        // A timed message closes itself.
        if (_closeScreenRoutine != null)
            return;
        if (_screen != null)
            Destroy(_screen.gameObject);
        _screen = null;
    }
    IEnumerator CloseScreenAfterRoutine(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        _closeScreenRoutine = null;
        CloseScreen();
    }
    // This player's own LobbyPlayer has started in the moved lobby: the move is done for them.
    // Returns the seat they had, if any, for LobbyPlayer to ask the server for.
    public bool TakeSeatToRestore(out bool isGrey, out string goblinType)
    {
        isGrey = _seatIsGrey;
        goblinType = _seatGoblinType;
        bool hadSeat = _hasSeatToRestore;
        _hasSeatToRestore = false;
        if (_isMigrating)
        {
            _isMigrating = false;
            if (_migrationRoutine != null)
            {
                StopCoroutine(_migrationRoutine);
                _migrationRoutine = null;
            }
            CloseScreen();
            Debug.Log("LobbyHostMigration: back in the lobby.");
        }
        return hadSeat;
    }
    // The new host's own LobbyPlayer is in the lobby, so bots have an owner. Puts back the bots each team had, then, on
    // Steam, tells the lobby to connect here. Called for every host's own player; it does nothing outside a move.
    public void FinishTakingOver()
    {
        NetworkManagerGRF game = NetworkManagerGRF.singleton;
        for (int i = 0; i < _greyBotsToRestore; i++)
            game.AddBot(true);
        for (int i = 0; i < _greenBotsToRestore; i++)
            game.AddBot(false);
        _greyBotsToRestore = 0;
        _greenBotsToRestore = 0;
        if (_announceAsHost)
        {
            _announceAsHost = false;
            if (SteamLobby.instance != null)
                SteamLobby.instance.AnnounceThisPlayerAsHost();
        }
    }
}
