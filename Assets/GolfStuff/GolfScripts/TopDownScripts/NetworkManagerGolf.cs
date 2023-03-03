using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
//using FishNet.Connection;
//using FishNet.Managing;
//using FishNet.Object;
using System.Linq;
using UnityEngine.SceneManagement;


public class NetworkManagerGolf : NetworkManager
{
    [Header("Player Prefabs and stuff")]
    [SerializeField] private GolfPlayerTopDown _golfPlayerPrefab;

    public List<GolfPlayerTopDown> GolfPlayers { get; } = new List<GolfPlayerTopDown>();


    // Start is called before the first frame update
    public override void OnStartServer()
    {
        base.OnStartServer();
        //spawnPrefabs = Resources.LoadAll<GameObject>("Prefabs").ToList();
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("Starting client...");
        List<GameObject> spawnablePrefabs = Resources.LoadAll<GameObject>("Prefabs").ToList();
        Debug.Log("Spawnable Prefab count: " + spawnablePrefabs.Count());

        foreach (GameObject prefab in spawnablePrefabs)
        {
            //NetworkClient.RegisterPrefab(prefab);
            Debug.Log("Registering prefab: " + prefab);
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        Debug.Log("Checking if player is in correct scene. Player's scene name is: " + SceneManager.GetActiveScene().name.ToString() + ". Correct scene name is: TitleScreen");
        if (SceneManager.GetActiveScene().name == "Golf-prototype-topdown" || SceneManager.GetActiveScene().name == "LobbyScene")
        {
            bool isGameLeader = GolfPlayers.Count == 0; // isLeader is true if the player count is 0, aka when you are the first player to be added to a server/room

            GolfPlayerTopDown golfPlayerInstance = Instantiate(_golfPlayerPrefab);

            golfPlayerInstance.IsGameLeader = isGameLeader;
            golfPlayerInstance.ConnectionId = conn.connectionId;


            NetworkServer.AddPlayerForConnection(conn, golfPlayerInstance.gameObject);
            Debug.Log("Player added. Player name: " + golfPlayerInstance.PlayerName + ". Player connection id: " + golfPlayerInstance.ConnectionId.ToString());
        }
    }
}
