using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using UnityEngine.SceneManagement;

public class NetworkManagerGRF : NetworkManager
{
    [SerializeField] public int minPlayers = 2;
    [SerializeField] private LobbyPlayer lobbyPlayerPrefab;
    [SerializeField] private GamePlayer gamePlayerPrefab;
    

    [Header("Waiting for players to load scene stuff")]
    [SerializeField] private GameObject WaitingForPlayersCanvas;
    public GameObject waitingForPlayersObject;
    public List<int> playersFinishedLoading = new List<int>();
    public bool areAllPlayersLoaded = false;
    public Dictionary<int, int> numberOfGoblinsLoaded = new Dictionary<int, int>();
    public bool areAllGoblinsLoaded = false;
    public List<int> playersWithFootballSpawned = new List<int>();
    public bool areFootballsSpawned = false;

    [Header("Game Info")]
    public bool is1v1 = false;
    public bool isSinglePlayer = false;

    [Header("Pause/Resume Game")]
    public bool isGamePaused = false;
    public GamePlayer playerWhoPaused;
    public uint playerWhoPausedNetId;
    public float lastPauseTimeStamp;


    public List<GamePlayer> GamePlayers { get; } = new List<GamePlayer>();
    public List<LobbyPlayer> LobbyPlayers { get; } = new List<LobbyPlayer>();


    // Start is called before the first frame update
    public override void OnStartServer()
    {
        spawnPrefabs = Resources.LoadAll<GameObject>("Prefabs").ToList();
    }
    public override void OnStartClient()
    {
        Debug.Log("Starting client...");
        List<GameObject> spawnablePrefabs = Resources.LoadAll<GameObject>("Prefabs").ToList();
        Debug.Log("Spawnable Prefab count: " + spawnablePrefabs.Count());

        foreach (GameObject prefab in spawnablePrefabs)
        {
            NetworkClient.RegisterPrefab(prefab);
            Debug.Log("Registering prefab: " + prefab);
        }
    }
    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("Client connected.");
        base.OnClientConnect(conn);
    }
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        Debug.Log("Client disconnected.");
        base.OnClientDisconnect(conn);
    }
    public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log("Connecting to server...");
        if (numPlayers >= maxConnections) // prevents players joining if the game is full
        {
            Debug.Log("Too many players. Disconnecting user.");
            conn.Disconnect();
            return;
        }
        if (SceneManager.GetActiveScene().name == "TitleScreen" || SceneManager.GetActiveScene().name == "LobbyScene") // prevents players from joining a game that has already started. When the game starts, the scene will no longer be the "TitleScreen"
        {
            Debug.Log("Player loaded from scene: " + SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.Log("Player did not load from correct scene. Disconnecting user. Player loaded from scene: " + SceneManager.GetActiveScene().name);
            conn.Disconnect();
            return;
        }
        Debug.Log("Server Connected");
    }
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        Debug.Log("Checking if player is in correct scene. Player's scene name is: " + SceneManager.GetActiveScene().name.ToString() + ". Correct scene name is: TitleScreen");
        if (SceneManager.GetActiveScene().name == "TitleScreen" || SceneManager.GetActiveScene().name == "LobbyScene")
        {
            bool isGameLeader = LobbyPlayers.Count == 0; // isLeader is true if the player count is 0, aka when you are the first player to be added to a server/room

            LobbyPlayer lobbyPlayerInstance = Instantiate(lobbyPlayerPrefab);

            lobbyPlayerInstance.IsGameLeader = isGameLeader;
            lobbyPlayerInstance.ConnectionId = conn.connectionId;
            lobbyPlayerInstance.playerNumber = LobbyPlayers.Count + 1;
            //lobbyPlayerInstance.playerSteamId = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.instance.current_lobbyID, LobbyPlayers.Count);
            lobbyPlayerInstance.is1v1 = this.is1v1;
            lobbyPlayerInstance.isSinglePlayer = this.isSinglePlayer;

            NetworkServer.AddPlayerForConnection(conn, lobbyPlayerInstance.gameObject);
            Debug.Log("Player added. Player name: " + lobbyPlayerInstance.PlayerName + ". Player connection id: " + lobbyPlayerInstance.ConnectionId.ToString());
        }
    }
    /*public override void OnServerAddPlayer(NetworkConnection conn)
    {
        Debug.Log("Checking if player is in correct scene. Player's scene name is: " + SceneManager.GetActiveScene().name.ToString() + ". Correct scene name is: TitleScreen");
        bool isGameLeader = GamePlayers.Count == 0; // isLeader is true if the player count is 0, aka when you are the first player to be added to a server/room

        GamePlayer gamePlayerInstance = Instantiate(gamePlayerPrefab);

        gamePlayerInstance.IsGameLeader = isGameLeader;
        gamePlayerInstance.ConnectionId = conn.connectionId;
        gamePlayerInstance.playerNumber = GamePlayers.Count + 1;

        NetworkServer.AddPlayerForConnection(conn, gamePlayerInstance.gameObject);
        Debug.Log("Player added. Player name: " + gamePlayerInstance.PlayerName + ". Player connection id: " + gamePlayerInstance.ConnectionId.ToString());
        
    }*/
    public void StartGame()
    {
        if (CanStartGame() && SceneManager.GetActiveScene().name == "LobbyScene")
        {
            //ServerChangeScene("Gameplay");
            ServerChangeScene("Gameplay-768-432");

        }
    }
    private bool CanStartGame()
    {
        Debug.Log("NetworkManager: CanStartGame");
        if (numPlayers < minPlayers)
        {
            Debug.Log("CanStartGame: Not enough players to start the game. total players: " + numPlayers.ToString() + " need this many players: " + minPlayers.ToString());
            return false;
        }
        foreach (LobbyPlayer player in LobbyPlayers)
        {
            if (!player.isPlayerReady)
                return false;
        }
        return true;
    }
    public override void ServerChangeScene(string newSceneName)
    {
        Debug.Log("ServerChangeScene: Changing to the following scene: " + newSceneName);
        //Changing from the menu to the scene
        //if ((SceneManager.GetActiveScene().name == "TitleScreen" || SceneManager.GetActiveScene().name == "LobbyScene") && newSceneName == "Gameplay")
        if ((SceneManager.GetActiveScene().name == "TitleScreen" || SceneManager.GetActiveScene().name == "LobbyScene") && newSceneName == "Gameplay-768-432")
        {
            Debug.Log("Changing scene to: " + newSceneName);
            for (int i = LobbyPlayers.Count - 1; i >= 0; i--)
            {
                var conn = LobbyPlayers[i].connectionToClient;
                var gamePlayerInstance = Instantiate(gamePlayerPrefab);

                gamePlayerInstance.SetPlayerName(LobbyPlayers[i].PlayerName);
                gamePlayerInstance.SetConnectionId(LobbyPlayers[i].ConnectionId);
                gamePlayerInstance.SetPlayerNumber(LobbyPlayers[i].playerNumber);
                gamePlayerInstance.IsGameLeader = LobbyPlayers[i].IsGameLeader;
                //gamePlayerInstance.playerSteamId = LobbyPlayers[i].playerSteamId;
                gamePlayerInstance.is1v1 = LobbyPlayers[i].is1v1;
                gamePlayerInstance.isSinglePlayer = LobbyPlayers[i].isSinglePlayer;
                if (!this.is1v1 && !this.isSinglePlayer)
                {
                    gamePlayerInstance.isTeamGrey = LobbyPlayers[i].isTeamGrey;
                    gamePlayerInstance.goblinType = LobbyPlayers[i].goblinType;
                }
                else if (this.isSinglePlayer)
                {
                    gamePlayerInstance.isTeamGrey = LobbyPlayers[i].isTeamGrey;
                }
                

                NetworkServer.Destroy(conn.identity.gameObject);
                NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject, true);
                Debug.Log("Spawned new GamePlayer: " + gamePlayerInstance.PlayerName);
            }
        }
        base.ServerChangeScene(newSceneName);
    }
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity != null)
        {
            LobbyPlayer player = conn.identity.GetComponent<LobbyPlayer>();
            LobbyPlayers.Remove(player);
        }
        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        LobbyPlayers.Clear();
        GamePlayers.Clear();
    }
    public void HostShutDownServer()
    {
        GameObject NetworkManagerObject = GameObject.Find("NetworkManager");
        //Destroy(this.GetComponent<SteamManager>());
        Destroy(NetworkManagerObject);
        Shutdown();
        SceneManager.LoadScene("TitleScreen");

        Start();

    }
    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);
        Debug.Log("OnClientSceneChanged for scene: " + SceneManager.GetActiveScene().name + " client with connection id: " + conn.ToString());
        if (SceneManager.GetActiveScene().name == "Gameplay")
        {

           /* try
            {
                Destroy(waitingForPlayersObject);
            }
            catch
            {
                Debug.Log("Could not destroy WaitingForPlayersCanvas");
            }*/
            
        }

        
    }
    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        Debug.Log("OnClientChangeScene. Old scene: " + SceneManager.GetActiveScene().name + " new scene: " + newSceneName);
        //if (newSceneName == "Gameplay")
        if (newSceneName == "Gameplay-768-432")
        {
            waitingForPlayersObject = Instantiate(WaitingForPlayersCanvas);
            DontDestroyOnLoad(waitingForPlayersObject);
        }
            
        base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
    }
    [Server]
    public void ReportPlayerFinishedLoading(int playerNumber)
    {
        Debug.Log("ReportPlayerFinishedLoading for player " + playerNumber.ToString());
        if (!playersFinishedLoading.Contains(playerNumber))
        {
            playersFinishedLoading.Add(playerNumber);
        }

        // Check if all players have finished loading
        foreach (GamePlayer player in GamePlayers)
        {
            if (!playersFinishedLoading.Contains(player.playerNumber))
            {
                areAllPlayersLoaded = false;
                break;
            }               
            else
                areAllPlayersLoaded = true;

        }
        Debug.Log("ReportPlayerFinishedLoading: Are all players loaded?: " + areAllPlayersLoaded.ToString());
        CheckIfAllGameplayStuffIsLoaded();
    }
    [Server]
    void CheckIfAllGameplayStuffIsLoaded()
    {
        if (areAllPlayersLoaded && areAllGoblinsLoaded && areFootballsSpawned)
        {
            Debug.Log("CheckIfAllGameplayStuffIsLoaded: true");
            foreach (GamePlayer player in GamePlayers)
                player.DoneLoadingGameplayStuff();
        }
        else
        {
            Debug.Log("CheckIfAllGameplayStuffIsLoaded: false");
        }
    }
    [Server]
    public void ReportFootballSpawnedForPlayer(int playerNumber)
    {
        Debug.Log("ReportFootballSpawnedForPlayer for player " + playerNumber.ToString());
        if (!playersWithFootballSpawned.Contains(playerNumber))
        {
            playersWithFootballSpawned.Add(playerNumber);
        }

        // Check if all players have finished loading
        foreach (GamePlayer player in GamePlayers)
        {
            if (!playersWithFootballSpawned.Contains(player.playerNumber))
            {
                areFootballsSpawned = false;
                break;
            }
            else
                areFootballsSpawned = true;

        }
        Debug.Log("ReportFootballSpawnedForPlayer: Do all players have their football spawned?: " + areFootballsSpawned.ToString());
        CheckIfAllGameplayStuffIsLoaded();
    }
    [Server]
    public void ReportGoblinSpawnedForPlayer(int playerNumber)
    {
        if (numberOfGoblinsLoaded.ContainsKey(playerNumber))
        {
            int goblinNumber = numberOfGoblinsLoaded[playerNumber] + 1;
            numberOfGoblinsLoaded[playerNumber] = goblinNumber;
        }
        else
        {
            numberOfGoblinsLoaded.Add(playerNumber, 1);
            Debug.Log("ReportGoblinSpawnedForPlayer: Player number: " + playerNumber + " has spawned " + numberOfGoblinsLoaded[playerNumber] + " goblins");
        }

        // Check if each player has loaded all three goblins
        if (numberOfGoblinsLoaded.Count != GamePlayers.Count  && !this.isSinglePlayer)
            return;
        else
        {
            foreach (GamePlayer player in GamePlayers)
            {
                if (this.isSinglePlayer && player.playerNumber == 2)
                    continue;
                if (!numberOfGoblinsLoaded.ContainsKey(player.playerNumber))
                {
                    areAllGoblinsLoaded = false;
                    break;
                }
                else if (numberOfGoblinsLoaded[player.playerNumber] != 6)
                {
                    areAllGoblinsLoaded = false;
                    break;
                }
                else
                {
                    areAllGoblinsLoaded = true;
                }

            }
        }
        Debug.Log("ReportGoblinSpawnedForPlayer: Do all players have all goblins spawned?: " + areAllGoblinsLoaded.ToString());
        CheckIfAllGameplayStuffIsLoaded();
    }
    public void DestroyWaitingForPlayersCanvas()
    {
        try
        {
            Destroy(waitingForPlayersObject);
        }
        catch
        {
            Debug.Log("Could not destroy WaitingForPlayersCanvas");
        }
    }
    public void PauseGame()
    {
        Time.timeScale = 0f;
    }
    public void ResumeGame()
    {
        Time.timeScale = 1.0f;
    }

}
