#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.SceneManagement;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.Tugboat;

// Testing only. Steam allows one instance of the game per account, so a 3v3 can't be tested with the Steam transport:
// six players need six accounts on six machines. This starts the same Football session over Tugboat instead, so any
// number of instances on one machine can play together. It joins the normal LobbyScene, so team and goblin selection,
// the match settings and everything after them work exactly as they do over Steam.
//
// The whole file is compiled out of a release build, and nothing else references it. The one thing it leaves behind in
// shipped code is NetworkManagerGRF.isDevSession, which stays false.
//
// Use: launch two instances for a 1v1 or six for a 3v3, press a Host button in one and Join in the others, then play
// as normal. The overlay only appears on the title screen.
//
// Scripted test runs: these command-line options let a script start instances that act on their own, for cases that
// happen too quickly to do by hand.
//   -devhost 1v1|3v3          host as soon as the title screen is up
//   -devjoin [address]        join a dev host (127.0.0.1 if no address is given)
//   -devleavemove [seconds]   press Leave on the moving-lobby screen this long after it appears (at once if no time)
// An instance started with any of them runs in the background, since a script's instances are never all focused, and
// hosts on loopback only, so a build in a new folder doesn't raise a firewall prompt.
public class DevDirectConnect : MonoBehaviour
{
    const string AddressKey = "DevDirectConnect.Address";

    string _address = "127.0.0.1";
    string _status = "";
    NetworkManager _networkManager;

    string _commandLineHostMode = null;
    bool _commandLineJoin = false;
    bool _isCommandLineRun = false;
    float _leaveMoveDelay = -1f;
    float _leaveMoveTime = -1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        GameObject host = new GameObject("DevDirectConnect");
        host.AddComponent<DevDirectConnect>();
        DontDestroyOnLoad(host);
    }

    void Awake()
    {
        _address = PlayerPrefs.GetString(AddressKey, "127.0.0.1");
        ReadCommandLine();
    }

    void ReadCommandLine()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 1; i < args.Length; i++)
        {
            string value = i + 1 < args.Length && !args[i + 1].StartsWith("-") ? args[i + 1] : null;
            if (args[i] == "-devhost")
                _commandLineHostMode = value ?? "1v1";
            else if (args[i] == "-devjoin")
            {
                _commandLineJoin = true;
                _address = value ?? "127.0.0.1";
            }
            else if (args[i] == "-devleavemove")
            {
                float seconds;
                _leaveMoveDelay = value != null && float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out seconds) ? seconds : 0f;
            }
        }
        _isCommandLineRun = _commandLineHostMode != null || _commandLineJoin || _leaveMoveDelay >= 0f;
        if (_isCommandLineRun)
            Application.runInBackground = true;
    }

    void Update()
    {
        if ((_commandLineHostMode != null || _commandLineJoin) && SceneManager.GetActiveScene().name == "TitleScreen"
            && TitleScreenManager.instance != null && NetworkManagerGRF.singleton != null)
        {
            if (_commandLineHostMode != null)
                StartDevHost(_commandLineHostMode != "3v3");
            else
                StartDevClient();
            _commandLineHostMode = null;
            _commandLineJoin = false;
        }

        if (_leaveMoveDelay >= 0f && LobbyHostMigration.instance != null)
        {
            if (!LobbyHostMigration.instance.IsMigrating)
                _leaveMoveTime = -1f;
            else if (_leaveMoveTime < 0f)
                _leaveMoveTime = Time.realtimeSinceStartup + _leaveMoveDelay;
            else if (Time.realtimeSinceStartup >= _leaveMoveTime)
                PressMovingLobbyLeave();
        }
    }

    // Clicks the screen's own button, so the prefab's On Click wiring is part of the test.
    void PressMovingLobbyLeave()
    {
        _leaveMoveDelay = -1f;
        MovingLobbyScreen screen = FindObjectOfType<MovingLobbyScreen>();
        UnityEngine.UI.Button leave = screen != null ? screen.GetComponentInChildren<UnityEngine.UI.Button>() : null;
        if (leave == null)
        {
            Debug.Log("DevDirectConnect: no Leave button on the moving-lobby screen.");
            return;
        }
        Debug.Log("DevDirectConnect: pressing Leave on the moving-lobby screen.");
        leave.onClick.Invoke();
    }

    void OnGUI()
    {
        // Out of the way during a match. The title screen is the only place a session can be started from anyway.
        if (SceneManager.GetActiveScene().name != "TitleScreen")
            return;

        GUILayout.BeginArea(new Rect(10f, 10f, 230f, 180f), GUI.skin.box);
        GUILayout.Label("Local testing (no Steam)");

        if (GUILayout.Button("Host 1v1"))
            StartDevHost(true);


        if (GUILayout.Button("Host 3v3"))
            StartDevHost(false);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Host:", GUILayout.Width(38f));
        _address = GUILayout.TextField(_address);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Join over Tugboat"))
            StartDevClient();

        if (!string.IsNullOrEmpty(_status))
            GUILayout.Label(_status);
        GUILayout.EndArea();
    }

    void StartDevHost(bool is1v1)
    {
        if (!Ready())
            return;
        NetworkManagerGRF.singleton.isDevSession = true;

        // Over Steam the mode and the match settings are set in TitleScreenManager.CreateNewLobby, just before it starts
        // the host, and nowhere else. Skipping that leaves the prefab's values: 3v3 with minPlayers 2, so the lobby
        // offers Start Game at two players and then hangs waiting for six, and halves of zero seconds.
        NetworkManagerGRF.singleton.is1v1 = is1v1;
        NetworkManagerGRF.singleton.isSinglePlayer = false;
        NetworkManagerGRF.singleton.maxConnections = is1v1 ? 2 : 6;
        NetworkManagerGRF.singleton.minPlayers = is1v1 ? 2 : 6;

        // The real settings panel, so a dev session uses whatever is typed there and the same fallbacks as a Steam lobby
        // (60 second halves, mercy rule at 21) when it has never been opened.
        if (TitleScreenManager.instance != null)
            TitleScreenManager.instance.SetGameSettings(false);

        NetworkManagerGRF.singleton.StartHost();
        _status = NetworkManagerGRF.singleton.IsFootballSession ? "Hosting " + (is1v1 ? "1v1." : "3v3.") : "Could not start the server.";
        Debug.Log("DevDirectConnect: " + _status);
    }

    void StartDevClient()
    {
        if (!Ready())
            return;
        PlayerPrefs.SetString(AddressKey, _address);
        NetworkManagerGRF.singleton.isDevSession = true;
        NetworkManagerGRF.singleton.networkAddress = _address;
        NetworkManagerGRF.singleton.StartClient();
        _status = NetworkManagerGRF.singleton.IsFootballSession ? "Connecting to " + _address + "." : "Could not start the client.";
        Debug.Log("DevDirectConnect: " + _status);
    }

    // Points Multipass at Tugboat. The server opens every transport it holds, so this only decides what the local client
    // connects with; SteamLobby.UseSteamTransport puts it back for a Steam session.
    bool Ready()
    {
        if (NetworkManagerGRF.singleton == null)
        {
            _status = "No NetworkManagerGRF yet.";
            return false;
        }
        if (_networkManager == null)
            _networkManager = InstanceFinder.NetworkManager;
        if (_networkManager == null)
        {
            _status = "No NetworkManager yet.";
            return false;
        }
        if (_networkManager.ServerManager.Started || _networkManager.ClientManager.Started)
        {
            _status = "Already in a session.";
            return false;
        }
        Multipass multipass = _networkManager.TransportManager.Transport as Multipass;
        if (multipass == null)
        {
            _status = "Transport isn't Multipass.";
            return false;
        }
        multipass.SetClientTransport<Tugboat>();
        // Single player binds Tugboat to loopback; clear that so a dev host is reachable from other machines.
        Tugboat tugboat = multipass.GetTransport<Tugboat>();
        if (tugboat != null)
        {
            tugboat.SetServerBindAddress(_isCommandLineRun ? "127.0.0.1" : string.Empty, FishNet.Transporting.IPAddressType.IPv4);
            tugboat.SetClientAddress("localhost");
        }
        _status = "";
        return true;
    }
}
#endif
