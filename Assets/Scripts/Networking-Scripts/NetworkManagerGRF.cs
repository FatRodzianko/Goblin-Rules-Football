using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using UnityEngine.SceneManagement;

public class NetworkManagerGRF : NetworkManager
{
    [SerializeField] private GamePlayer gamePlayerPrefab;
    public List<GamePlayer> GamePlayers { get; } = new List<GamePlayer>();

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

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        Debug.Log("Checking if player is in correct scene. Player's scene name is: " + SceneManager.GetActiveScene().name.ToString() + ". Correct scene name is: TitleScreen");
        bool isGameLeader = GamePlayers.Count == 0; // isLeader is true if the player count is 0, aka when you are the first player to be added to a server/room

        GamePlayer gamePlayerInstance = Instantiate(gamePlayerPrefab);

        gamePlayerInstance.IsGameLeader = isGameLeader;
        gamePlayerInstance.ConnectionId = conn.connectionId;
        gamePlayerInstance.playerNumber = GamePlayers.Count + 1;

        NetworkServer.AddPlayerForConnection(conn, gamePlayerInstance.gameObject);
        Debug.Log("Player added. Player name: " + gamePlayerInstance.PlayerName + ". Player connection id: " + gamePlayerInstance.ConnectionId.ToString());
        
    }
}
