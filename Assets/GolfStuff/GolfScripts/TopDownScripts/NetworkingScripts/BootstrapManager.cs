using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Managing;
using Steamworks;

public class BootstrapManager : MonoBehaviour
{
    [SerializeField] NetworkManager _networkManager;
    [SerializeField] private FishySteamworks.FishySteamworks _fishySteamWorks;

    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;


    // Start is called before the first frame update
    void Start()
    {
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    { 

    }
    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    { 

    }
    private void OnLobbyEntered(LobbyEnter_t callback)
    { 

    }
}
