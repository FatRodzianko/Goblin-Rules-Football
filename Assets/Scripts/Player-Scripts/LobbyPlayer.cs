using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using Steamworks;

public class LobbyPlayer : NetworkBehaviour
{
    [Header("Player Info")]
    private readonly SyncVar<string> _syncPlayerName = new SyncVar<string>("");
    public string PlayerName { get => _syncPlayerName.Value; set => _syncPlayerName.Value = value; }
    private readonly SyncVar<int> _syncConnectionId = new SyncVar<int>();
    public int ConnectionId { get => _syncConnectionId.Value; set => _syncConnectionId.Value = value; }
    private readonly SyncVar<int> _syncPlayerNumber = new SyncVar<int>();
    public int playerNumber { get => _syncPlayerNumber.Value; set => _syncPlayerNumber.Value = value; }
    private readonly SyncVar<ulong> _syncPlayerSteamId = new SyncVar<ulong>();
    public ulong playerSteamId { get => _syncPlayerSteamId.Value; set => _syncPlayerSteamId.Value = value; }

    [Header("Game Info")]
    private readonly SyncVar<bool> _syncIsGameLeader = new SyncVar<bool>(false);
    public bool IsGameLeader { get => _syncIsGameLeader.Value; set => _syncIsGameLeader.Value = value; }
    private readonly SyncVar<bool> _syncIsPlayerReady = new SyncVar<bool>();
    public bool isPlayerReady { get => _syncIsPlayerReady.Value; set => _syncIsPlayerReady.Value = value; }
    public PlayerListItem myPlayerListItem;

    [Header("1v1 or 3v3 stuff")]
    private readonly SyncVar<bool> _syncIs1v1 = new SyncVar<bool>(false);
    public bool is1v1 { get => _syncIs1v1.Value; set => _syncIs1v1.Value = value; }
    private readonly SyncVar<bool> _syncIsSinglePlayer = new SyncVar<bool>(false);
    public bool isSinglePlayer { get => _syncIsSinglePlayer.Value; set => _syncIsSinglePlayer.Value = value; }
    private readonly SyncVar<string> _syncGoblinType = new SyncVar<string>("");
    public string goblinType { get => _syncGoblinType.Value; set => _syncGoblinType.Value = value; }
    private readonly SyncVar<bool> _syncIsTeamGrey = new SyncVar<bool>();
    public bool isTeamGrey { get => _syncIsTeamGrey.Value; set => _syncIsTeamGrey.Value = value; }
    private readonly SyncVar<bool> _syncIsGoblinSelected = new SyncVar<bool>(false);
    public bool isGoblinSelected { get => _syncIsGoblinSelected.Value; set => _syncIsGoblinSelected.Value = value; }
    private readonly SyncVar<bool> _syncWantsToSwitchTeams = new SyncVar<bool>(false);
    public bool wantsToSwitchTeams { get => _syncWantsToSwitchTeams.Value; set => _syncWantsToSwitchTeams.Value = value; }
    // A bot the host added to fill a 3v3 slot. It is an ordinary LobbyPlayer so the team lists, the lobby rows and
    // Start Game all handle it with no special cases. Set before Spawn, so it is already true in OnStartClient.
    private readonly SyncVar<bool> _syncIsBot = new SyncVar<bool>(false);
    public bool isBot { get => _syncIsBot.Value; set => _syncIsBot.Value = value; }

    [Header("DLC Installed SyncVars")]
    private readonly SyncVar<string> _syncWhatDLCsAreInstalled = new SyncVar<string>("");
    public string whatDLCsAreInstalled { get => _syncWhatDLCsAreInstalled.Value; set => _syncWhatDLCsAreInstalled.Value = value; }

    // FishNet clears the owner before OnDestroy runs, so remember whether this machine owned it.
    private bool _isOwnedLocally = false;

    private const string PlayerPrefsNameKey = "PlayerName";

    private NetworkManagerGRF game;
    private NetworkManagerGRF Game
    {
        get
        {
            if (game != null)
            {
                return game;
            }
            return game = NetworkManagerGRF.singleton as NetworkManagerGRF;
        }
    }

    private void Awake()
    {
        _syncPlayerName.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandlePlayerNameUpdate(prev, next); };
        _syncIsPlayerReady.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandlePlayerReadyStatusChange(prev, next); };
        _syncGoblinType.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleGoblinType(prev, next); };
        _syncIsTeamGrey.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleIsTeamGrey(prev, next); };
        _syncIsGoblinSelected.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleIsGoblinSelected(prev, next); };
        _syncWantsToSwitchTeams.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleWantsToSwitchTeams(prev, next); };
        _syncWhatDLCsAreInstalled.OnChange += (prev, next, asServer) => { if (HookFilter.ShouldRun(asServer)) HandleWhatDLCsAreInstalled(prev, next); };
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        //UpdateTeamListsOnLobbyManager();
    }
    [ServerRpc]
    private void CmdSetPlayerName(string PlayerNameSubmitted)
    {
        Debug.Log("CmdSetPlayerName: Setting player name to: " + PlayerNameSubmitted);
        string playerNameToSet = "";
        if (PlayerNameSubmitted.Length > 12)
            playerNameToSet = PlayerNameSubmitted.Substring(0, 12);
        else
            playerNameToSet = PlayerNameSubmitted;
        this.PlayerName = playerNameToSet;
    }
    [ServerRpc]
    void CmdSetPlayerInitialTeam()
    {
        // set the host as team grey
        if (this.IsGameLeader)
            isTeamGrey = false;
        else
        {
            // Balance by players, not by members: with the lobby filled out by bots both teams always look equal, and a
            // joining player should land on whichever side has fewer actual people. Ignore this player while counting:
            // it is still sitting on the default team, green, so counting it made green look one heavier than it is and
            // the two sides tie. That tie fell to the coin flip below, which is how a second player sometimes landed on
            // the same team as the first. Whether it counted at all depended on tick order, so the bug came and went.
            if (LobbyManager.instance.HumansOnTeam(false, this) > LobbyManager.instance.HumansOnTeam(true, this))
            {
                this.isTeamGrey = true;
            }
            else if (LobbyManager.instance.HumansOnTeam(false, this) < LobbyManager.instance.HumansOnTeam(true, this))
            {
                this.isTeamGrey = false;
            }
            else
            {
                string[] headsTails = new[]
                { "green","grey"};
                var rng = new System.Random();
                string result = headsTails[rng.Next(headsTails.Length)];
                if (result == "green")
                {
                    this.isTeamGrey = false;
                }
                else
                {
                    this.isTeamGrey = true;
                }
            }
        }
        UpdateTeamListsOnLobbyManager();
        UpdateAllPlayersOnAvailableGoblins(this.isTeamGrey);
        // The team now has one more player than it had room for, since the lobby let this one in past the bots.
        Game.EnforceTeamCapacity();
    }
    public void HandlePlayerNameUpdate(string oldValue, string newValue)
    {
        Debug.Log("Player name has been updated for: " + oldValue + " to new value: " + newValue);
        if (IsClientInitialized)
        {
            LobbyManager.instance.UpdateUI();
        }

    }
    public void UpdateTeam(bool isGrey)
    {
        Debug.Log("UpdateTeam to grey? " + isGrey.ToString() + " for player " + this.PlayerName);
        if (IsOwner)
        {
            if (isGrey != this.isTeamGrey)
            {
                
                if (this.isGoblinSelected)
                    CmdUnselectGoblin();
                CmdUpdateTeam(isGrey);
                if (this.isPlayerReady)
                    CmdChangePlayerReadyStatus();
            }
        }
            
    }
    [ServerRpc]
    void CmdUpdateTeam(bool isGrey)
    {
        Debug.Log("CmdUpdateTeam to grey? " + isGrey.ToString() + " for player " + this.PlayerName + " are they already grey? " + isGrey.ToString());
        if (this.isTeamGrey != isGrey)
        {
            if (isGrey && LobbyManager.instance.HumansOnTeam(true) >= 3)
            {
                Debug.Log("CmdUpdateTeam: " + this.PlayerName + " is trying to switch to GREY but GREY is full. Setting wantsToSwitch To True");
                this.wantsToSwitchTeams = true;
                CheckIfOtherPlayersWantToSwitch(this);
                return;
            }
            else if (!isGrey && LobbyManager.instance.HumansOnTeam(false) >= 3)
            {
                Debug.Log("CmdUpdateTeam: " + this.PlayerName + " is trying to switch to GREEN but GREEN is full. Setting wantsToSwitch To True");
                this.wantsToSwitchTeams = true;
                CheckIfOtherPlayersWantToSwitch(this);
                return;
            }
            this.isTeamGrey = isGrey;
            if (this.wantsToSwitchTeams)
                this.wantsToSwitchTeams = false;
            UpdateAllPlayersOnAvailableGoblins(isGrey);
            // The team this player just joined may now be four deep, since admission only counted humans. A bot gives
            // way. Joining the lobby does this in SpawnLobbyPlayer; switching team needs it too.
            Game.EnforceTeamCapacity();
        }
    }
    [Server]
    void CheckIfOtherPlayersWantToSwitch(LobbyPlayer playerRequestingSwitch)
    {
        Debug.Log("CheckIfOtherPlayersWantToSwitch for player: " + playerRequestingSwitch.PlayerName);
        foreach (LobbyPlayer player in Game.LobbyPlayers)
        {
            if (player.wantsToSwitchTeams && player != playerRequestingSwitch && player.isTeamGrey != playerRequestingSwitch.isTeamGrey)
            {
                Debug.Log("CheckIfOtherPlayersWantToSwitch: Found player that wants to switch: " + player.PlayerName);
                if (playerRequestingSwitch.isGoblinSelected && !string.IsNullOrWhiteSpace(playerRequestingSwitch.goblinType))
                {
                    playerRequestingSwitch.UnselectGoblinOnServer();
                }
                UpdateAllPlayersOnAvailableGoblins(playerRequestingSwitch.isTeamGrey);
                playerRequestingSwitch.isTeamGrey = !playerRequestingSwitch.isTeamGrey;
                playerRequestingSwitch.wantsToSwitchTeams = false;


                if (player.isGoblinSelected && !string.IsNullOrWhiteSpace(player.goblinType))
                {
                    player.UnselectGoblinOnServer();
                }
                UpdateAllPlayersOnAvailableGoblins(player.isTeamGrey);
                player.isTeamGrey = !player.isTeamGrey;
                player.wantsToSwitchTeams = false;
                break;
            }
        }
    }
    [Server]
    void CheckIfTeamIsNoLongerFull()
    {
        foreach (LobbyPlayer player in Game.LobbyPlayers)
        {
            if (player.wantsToSwitchTeams)
            {
                if (player.isTeamGrey)
                {
                    Debug.Log("CheckIfTeamIsNoLongerFull: player " + player.PlayerName + " is on team grey and wants to switch to team green.");
                    if (LobbyManager.instance.HumansOnTeam(false) >= 3)
                    {
                        Debug.Log("CheckIfTeamIsNoLongerFull: player " + player.PlayerName + " is on team grey and wants to switch to team green BUT green is full. checking next player...");
                        continue;
                    }
                    else
                    {
                        Debug.Log("CheckIfTeamIsNoLongerFull: player " + player.PlayerName + " is on team grey and wants to switch to team green AND green has a spot open. Switching player");
                        if (player.isGoblinSelected && !string.IsNullOrWhiteSpace(player.goblinType))
                        {
                            player.UnselectGoblinOnServer();
                        }
                        UpdateAllPlayersOnAvailableGoblins(player.isTeamGrey);
                        player.isTeamGrey = !player.isTeamGrey;
                        player.wantsToSwitchTeams = false;
                        Game.EnforceTeamCapacity();
                    }
                }
                else
                {
                    Debug.Log("CheckIfTeamIsNoLongerFull: player " + player.PlayerName + " is on team green and wants to switch to team grey.");
                    if (LobbyManager.instance.HumansOnTeam(true) >= 3)
                    {
                        Debug.Log("CheckIfTeamIsNoLongerFull: player " + player.PlayerName + " is on team green and wants to switch to team grey BUT green is full. checking next player...");
                        continue;
                    }
                    else
                    {
                        Debug.Log("CheckIfTeamIsNoLongerFull: player " + player.PlayerName + " is on team green and wants to switch to team grey AND grey has a spot open. Switching player");
                        if (player.isGoblinSelected && !string.IsNullOrWhiteSpace(player.goblinType))
                        {
                            player.UnselectGoblinOnServer();
                        }
                        UpdateAllPlayersOnAvailableGoblins(player.isTeamGrey);
                        player.isTeamGrey = !player.isTeamGrey;
                        player.wantsToSwitchTeams = false;
                        Game.EnforceTeamCapacity();
                    }
                }
            }
        }
        UpdateTeamListsOnLobbyManager();
    }
    public void StartUpdateTeamListsOnLobbyManager()
    {
        if (IsOwner)
            CmdStartUpdateTeamListsOnLobbyManager();
    }
    [ServerRpc]
    void CmdStartUpdateTeamListsOnLobbyManager()
    {
        UpdateTeamListsOnLobbyManager();
    }
    [Server]
    void UpdateTeamListsOnLobbyManager()
    {
        Debug.Log("UpdateTeamListsOnLobbyManager: for this many players: " + Game.LobbyPlayers.Count.ToString());
        foreach (LobbyPlayer player in Game.LobbyPlayers)
        {
            if (player.isTeamGrey)
            {
                if (!LobbyManager.instance.GreyTeamMembers.Contains(player))
                    LobbyManager.instance.GreyTeamMembers.Add(player);
                if (LobbyManager.instance.GreenTeamMembers.Contains(player))
                    LobbyManager.instance.GreenTeamMembers.Remove(player);
            }
            else
            {
                if (!LobbyManager.instance.GreenTeamMembers.Contains(player))
                    LobbyManager.instance.GreenTeamMembers.Add(player);
                if (LobbyManager.instance.GreyTeamMembers.Contains(player))
                    LobbyManager.instance.GreyTeamMembers.Remove(player);
            }
        }
        /*if(LobbyManager.instance.GreenTeamMembers.Count >= 3 || LobbyManager.instance.GreyTeamMembers.Count >= 3)
            CheckIfTeamIsNoLongerFull();*/
    }
    
    void HandleIsTeamGrey(bool oldValue, bool newValue)
    {
        if (IsServerInitialized)
        {
            if (!this.isSinglePlayer)
            {
                UpdateTeamListsOnLobbyManager();
                UpdateAllPlayersOnAvailableGoblins(newValue);
            }
        }   
        if (IsClientInitialized)
        {
            if (!this.isSinglePlayer)
            {
                LobbyManager.instance.UpdateUI();
            }
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        // This was OnStartAuthority with Mirror, which ran right before OnStartClient. FindLocalLobbyPlayer has to run first
        // so LobbyManager knows if this is a 1v1 lobby before UpdateUI makes this player's list item.
        if (IsOwner && !isBot)
        {
            _isOwnedLocally = true;
            // Local instances in a dev session all share one Steam account, so Steam would give them the same name and the
            // lobby list would be unreadable. The process id tells them apart.
            if (NetworkManagerGRF.singleton != null && NetworkManagerGRF.singleton.isDevSession)
                CmdSetPlayerName("Dev-" + (System.Diagnostics.Process.GetCurrentProcess().Id % 10000).ToString());
            else
                CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());
            //CmdSetPlayerName(PlayerPrefs.GetString(PlayerPrefsNameKey));
            gameObject.name = "LocalLobbyPlayer";
            LobbyManager.instance.FindLocalLobbyPlayer();
            LobbyManager.instance.UpdateLobbyName();
            CmdSetPlayerInitialTeam();
            CheckIfDLCsAreInstalled();
        }

        Game.LobbyPlayers.Add(this);
        LobbyManager.instance.UpdateLobbyName();
        LobbyManager.instance.UpdateUI();
        StartUpdateTeamListsOnLobbyManager();
    }
    public void ChangeReadyStatus()
    {
        Debug.Log("Executing ChangeReadyStatus for player: " + this.PlayerName);
        if (IsOwner)
            CmdChangePlayerReadyStatus();
    }
    [ServerRpc]
    void CmdChangePlayerReadyStatus()
    {
        Debug.Log("Executing CmdChangePlayerReadyStatus on the server for player: " + this.PlayerName);
        this.isPlayerReady = !this.isPlayerReady;
    }
    void HandlePlayerReadyStatusChange(bool oldValue, bool newValue)
    {
        if (IsClientInitialized && ! this.isSinglePlayer)
            LobbyManager.instance.UpdateUI();
    }
    public void CanLobbyStartGame()
    {
        if (IsOwner)
            CmdCanLobbyStartGame();
    }
    [ServerRpc]
    void CmdCanLobbyStartGame()
    {
        Debug.Log("CmdCanLobbyStartGame");
        Game.StartGame();
    }
    public void UnselectGoblin()
    {
        if (IsOwner)
        {
            CmdUnselectGoblin();
            if (this.isPlayerReady)
                CmdChangePlayerReadyStatus();
        }
            
    }
    [ServerRpc]
    void CmdUnselectGoblin()
    {
        /*if (this.isTeamGrey)
        {
            if (!string.IsNullOrWhiteSpace(this.goblinType))
            {
                if (LobbyManager.instance.GreyGoblinsSelected.Contains(this.goblinType))
                    LobbyManager.instance.GreyGoblinsSelected.Remove(this.goblinType);
                if (!LobbyManager.instance.GreyGoblinsAvailable.Contains(this.goblinType))
                    LobbyManager.instance.GreyGoblinsAvailable.Add(this.goblinType);
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(this.goblinType))
            {
                if (LobbyManager.instance.GreenGoblinsSelected.Contains(this.goblinType))
                    LobbyManager.instance.GreenGoblinsSelected.Remove(this.goblinType);
                if (!LobbyManager.instance.GreenGoblinsAvailable.Contains(this.goblinType))
                    LobbyManager.instance.GreenGoblinsAvailable.Add(this.goblinType);
            }
        }
        UpdateAllPlayersOnAvailableGoblins(this.isTeamGrey);
        this.HandleIsGoblinSelected(this.isGoblinSelected, false);
        this.HandleGoblinType(this.goblinType, "");*/
        UnselectGoblinOnServer();
    }
    [Server]
    void UnselectGoblinOnServer()
    {
        if (this.isTeamGrey)
        {
            if (!string.IsNullOrWhiteSpace(this.goblinType))
            {
                if (LobbyManager.instance.GreyGoblinsSelected.Contains(this.goblinType))
                    LobbyManager.instance.GreyGoblinsSelected.Remove(this.goblinType);
                if (!LobbyManager.instance.GreyGoblinsAvailable.Contains(this.goblinType))
                    LobbyManager.instance.GreyGoblinsAvailable.Add(this.goblinType);
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(this.goblinType))
            {
                if (LobbyManager.instance.GreenGoblinsSelected.Contains(this.goblinType))
                    LobbyManager.instance.GreenGoblinsSelected.Remove(this.goblinType);
                if (!LobbyManager.instance.GreenGoblinsAvailable.Contains(this.goblinType))
                    LobbyManager.instance.GreenGoblinsAvailable.Add(this.goblinType);
            }
        }
        UpdateAllPlayersOnAvailableGoblins(this.isTeamGrey);
        this.isGoblinSelected = false;
        this.goblinType = "";
    }
    public void SelectGoblin(string typeOfGoblin)
    {
        if (IsOwner)
        {
            CmdSelectGoblin(typeOfGoblin);
        }
    }
    [ServerRpc]
    void CmdSelectGoblin(string typeOfGoblin)
    {
        if (this.goblinType != typeOfGoblin)
        {
            if (this.isTeamGrey)
            {
                if (LobbyManager.instance.GreyGoblinsAvailable.Contains(typeOfGoblin) && !LobbyManager.instance.GreyGoblinsSelected.Contains(typeOfGoblin))
                {
                    LobbyManager.instance.GreyGoblinsAvailable.Remove(typeOfGoblin);
                    LobbyManager.instance.GreyGoblinsSelected.Add(typeOfGoblin);
                }
                else
                    return;
            }
            else
            {
                if (LobbyManager.instance.GreenGoblinsAvailable.Contains(typeOfGoblin) && !LobbyManager.instance.GreenGoblinsSelected.Contains(typeOfGoblin))
                {
                    LobbyManager.instance.GreenGoblinsAvailable.Remove(typeOfGoblin);
                    LobbyManager.instance.GreenGoblinsSelected.Add(typeOfGoblin);
                }
                else
                    return;
            }
            UpdateAllPlayersOnAvailableGoblins(this.isTeamGrey);
            this.goblinType = typeOfGoblin;
            this.isGoblinSelected = true;
        }   
    }
    public void HandleIsGoblinSelected(bool oldValue, bool newValue)
    {
        if (IsClientInitialized)
        {
            // A bot has no row of its own to drive and no dropdown. Start Game hands bots a goblin, which fired this
            // hook and logged a caught null reference per bot.
            if (IsOwner && !isBot)
            {
                LobbyManager.instance.ActivateChangeGoblinButton();
                try
                {
                    myPlayerListItem.ActivateGoblinDropdown();
                }
                catch (Exception e)
                {
                    Debug.Log("LobbyPlayer.cs: HandleIsGoblinSelected: Could not access myPlayerListItem.ActivateGoblinDropdown(); Error: " + e);
                }
                
                if (!newValue && this.isPlayerReady)
                    CmdChangePlayerReadyStatus();
            }
            LobbyManager.instance.UpdateGoblinSelectedBoolOnPlayerListItems(this);
        }
    }
    void HandleGoblinType(string oldValue, string newValue)
    {
        if (IsClientInitialized)
        {
            // A bot has no row of its own to drive and no dropdown. Start Game hands bots a goblin, which fired this
            // hook and logged a caught null reference per bot.
            if (IsOwner && !isBot)
            {
                LobbyManager.instance.ActivateChangeGoblinButton();
                try
                {
                    myPlayerListItem.ActivateGoblinDropdown();
                }
                catch (Exception e)
                {
                    Debug.Log("LobbyPlayer.cs: HandleGoblinType: Could not access myPlayerListItem.ActivateGoblinDropdown(); Error: " + e);
                }
                
            }
            LobbyManager.instance.UpdateGoblinSelectedTextOnPlayerListItems(this);
        }
    }
    [Server]
    void UpdateAllPlayersOnAvailableGoblins(bool isGrey)
    {
        foreach (LobbyPlayer player in Game.LobbyPlayers)
        {
            if (player.isTeamGrey == isGrey)
            {
                Debug.Log("UpdateAllPlayersOnAvailableGoblins: Player name: " + player.PlayerName + " player connection id: " + player.ConnectionId);
                if (isGrey)
                {
                    Debug.Log("UpdateAllPlayersOnAvailableGoblins: Sending RpcSendPlayersUpdatedAvailableGoblinList to Player name: " + player.PlayerName + " player connection id: " + player.ConnectionId.ToString() + " " + player.Owner.ToString());
                    player.RpcSendPlayersUpdatedAvailableGoblinList(player.Owner, LobbyManager.instance.GreyGoblinsAvailable);
                }
                else
                {
                    Debug.Log("UpdateAllPlayersOnAvailableGoblins: Sending RpcSendPlayersUpdatedAvailableGoblinList to Player name: " + player.PlayerName + " player connection id: " + player.ConnectionId.ToString() + " " + player.Owner.ToString());
                    player.RpcSendPlayersUpdatedAvailableGoblinList(player.Owner, LobbyManager.instance.GreenGoblinsAvailable);
                }   
            }
        }
    }
    [TargetRpc]
    public void RpcSendPlayersUpdatedAvailableGoblinList(NetworkConnection target, List<string> availableGoblins)
    {
        Debug.Log("RpcSendPlayersUpdatedAvailableGoblinList: for player: " + this.PlayerName + " is team grey? " + this.isTeamGrey.ToString() + " and list available: " + availableGoblins.Count.ToString() + " for UpdateGoblinsAvailable");
        // Single player has no player list item. An exception here would make FishNet drop the rest of the packet.
        if(IsOwner && !LobbyManager.instance.is1v1 && this.myPlayerListItem != null)
            this.myPlayerListItem.UpdateGoblinsAvailable(availableGoblins);
    }
    /*[ObserversRpc]
    void RpcSendPlayersUpdatedAvailableGoblinList(bool isGrey, List<string> availableGoblins)
    {
        LobbyPlayer localPlayer = GameObject.FindGameObjectWithTag("LocalLobbyPlayer").GetComponent<LobbyPlayer>();
        if (isGrey == localPlayer.isTeamGrey)
        {
            localPlayer.myPlayerListItem.UpdateGoblinsAvailable(availableGoblins);
        }
    }*/
    void HandleWantsToSwitchTeams(bool oldValue, bool newValue)
    {
        if (IsClientInitialized)
        { 

        }
    }
    public void QuitLobby()
    {
        if (IsOwner)
        {
            if (IsGameLeader)
            {
                Game.StopHost();
            }
            else
            {
                Game.StopClient();
            }
        }
    }
    private void OnDestroy()
    {
        if (_isOwnedLocally)
        {
            try
            {
                // On a FishNet host, LobbyScene can unload before this is destroyed at game start. Skip its UI then.
                // The Steam lobby isn't left here: players stay in it for the match, and SteamLobby leaves it when the
                // session ends.
                if (LobbyManager.instance != null)
                    LobbyManager.instance.DestroyPlayerListItems();
            }
            catch (Exception e)
            {
                Debug.Log("LobbyPlayer.cs OnDestroy: Could not access LobbyManager.instance.DestroyPlayerListItems(). Error: " + e);
            }
            
        }
        if (IsServerInitialized)
        { 

        }
        Debug.Log("LobbyPlayer destroyed. Returning to main menu.");
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log(PlayerName + " is quiting the game.");
        Game.LobbyPlayers.Remove(this);
        Debug.Log("Removed player from the GamePlayer list: " + this.PlayerName);
        // On a FishNet host, the despawn at game start reaches the client side after LobbyScene has unloaded.
        // Skip the lobby UI then: an exception here would stop FishNet from starting the GamePlayer spawned with it.
        if (LobbyManager.instance == null)
            return;
        LobbyManager.instance.UpdateUI();
        // FishNet stops the server side of a despawned object before its client side, so check the server itself here.
        if (IsServerStarted)
        {
            // Remove lobby player from team member lists
            if (LobbyManager.instance.GreenTeamMembers.Contains(this))
                LobbyManager.instance.GreenTeamMembers.Remove(this);
            if (LobbyManager.instance.GreyTeamMembers.Contains(this))
                LobbyManager.instance.GreyTeamMembers.Remove(this);
            // Remove selected goblins
            if (this.isGoblinSelected)
            {
                UnselectGoblinOnServer();
            }
        }
    }
    [Client]
    void CheckIfDLCsAreInstalled()
    {
        if (IsOwner)
        {
            // BIsDlcInstalled throws when Steam isn't running, and this is called from OnStartClient, where an exception
            // aborts the rest of the start callbacks. The DLC check is a local one, so it still works in a dev session
            // as long as Steam is up; with Steam down there is nothing to ask and the icons stay hidden.
            if (!SteamManager.Initialized)
                return;
            Steamworks.AppId_t dlc_99 = new Steamworks.AppId_t(2174640);
            bool dlc99Installed = SteamApps.BIsDlcInstalled(dlc_99);
            Steamworks.AppId_t dlc_1_99 = new Steamworks.AppId_t(2174641);
            bool dlc199Installed = SteamApps.BIsDlcInstalled(dlc_1_99);
            Debug.Log("CheckIfDLCsAreInstalled: for player with Steam Id of : " + this.playerSteamId.ToString() + " dlc_99 " + dlc99Installed.ToString() + " dlc_1_99 " + dlc199Installed.ToString());
            CmdTellServerWhatDLCsAreInstalled(dlc99Installed, dlc199Installed);
        }
    }
    [ServerRpc]
    void CmdTellServerWhatDLCsAreInstalled(bool dlc_99, bool dlc_1_99)
    {
        Debug.Log("CmdTellServerWhatDLCsAreInstalled: for player with Steam Id of : " + this.playerSteamId.ToString() + " dlc_99 " + dlc_99.ToString() + " dlc_1_99 " + dlc_1_99.ToString());
        //RpcUpdateDLCImages(dlc_99, dlc_1_99);
        if (!dlc_99 && !dlc_1_99)
        {
            this.whatDLCsAreInstalled = "none";
        }
        if (dlc_99 && !dlc_1_99)
        {
            this.whatDLCsAreInstalled = "99";
        }
        if (!dlc_99 && dlc_1_99)
        {
            this.whatDLCsAreInstalled = "1_99";
        }
        if (dlc_99 && dlc_1_99)
        {
            this.whatDLCsAreInstalled = "both";
        }
    }
    [ObserversRpc]
    void RpcUpdateDLCImages(bool dlc_99, bool dlc_1_99)
    {
        Debug.Log("RpcUpdateDLCImages: for player with Steam Id of : " + this.playerSteamId.ToString() + " dlc_99 " + dlc_99.ToString() + " dlc_1_99 " + dlc_1_99.ToString());
        GameObject[] playerListItems = GameObject.FindGameObjectsWithTag("PlayerListItem");
        if (playerListItems.Length > 0)
        {
            for (int i = 0; i < playerListItems.Length; i++)
            {
                PlayerListItem playerListItem = playerListItems[i].GetComponent<PlayerListItem>();
                if (playerListItem.playerSteamId == this.playerSteamId)
                {
                    //playerListItem.UpdateDLCImages(dlc_99, dlc_1_99);
                }
            }
        }
    }
    public void HandleWhatDLCsAreInstalled(string oldValue, string newValue)
    {
        if (IsClientInitialized)
        {
            Debug.Log("HandleWhatDLCsAreInstalled. For player: " + this.PlayerName + " newValue: " + newValue);
            GameObject[] playerListItems = GameObject.FindGameObjectsWithTag("PlayerListItem");
            Debug.Log("HandleWhatDLCsAreInstalled: Number of player list items found: " + playerListItems.Length.ToString());
            if (playerListItems.Length > 0)
            {
                Debug.Log("HandleWhatDLCsAreInstalled: Checking through " + playerListItems.Length.ToString() + " number of playerListItems");
                for (int i = 0; i < playerListItems.Length; i++)
                {
                    PlayerListItem playerListItem = playerListItems[i].GetComponent<PlayerListItem>();
                    if (playerListItem.playerSteamId == this.playerSteamId)
                    {
                        Debug.Log("HandleWhatDLCsAreInstalled: Matching player list item found for this player. This player's steamid: " + this.playerSteamId.ToString() + " playerListItem steam id: " + playerListItem.playerSteamId);
                        playerListItem.UpdateDLCImages(newValue);
                        break;
                    }
                }
            }
        }
    }
}
