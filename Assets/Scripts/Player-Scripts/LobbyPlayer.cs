using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class LobbyPlayer : NetworkBehaviour
{
    [Header("Player Info")]
    [SyncVar(hook = nameof(HandlePlayerNameUpdate))] public string PlayerName;
    [SyncVar] public int ConnectionId;
    [SyncVar] public int playerNumber;

    [Header("Game Info")]
    [SyncVar] public bool IsGameLeader = false;
    [SyncVar(hook = nameof(HandlePlayerReadyStatusChange))] public bool isPlayerReady;
    public PlayerListItem myPlayerListItem;

    [Header("1v1 or 3v3 stuff")]
    [SyncVar] public bool is1v1 = false;
    [SyncVar] public bool isSinglePlayer = false;
    [SyncVar(hook = nameof(HandleGoblinType))] public string goblinType;
    [SyncVar(hook = nameof(HandleIsTeamGrey))] public bool isTeamGrey;
    [SyncVar(hook = nameof(HandleIsGoblinSelected))] public bool isGoblinSelected = false;
    [SyncVar(hook = nameof(HandleWantsToSwitchTeams))] public bool wantsToSwitchTeams = false;

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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void OnStartAuthority()
    {
        //CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());
        CmdSetPlayerName(PlayerPrefs.GetString(PlayerPrefsNameKey));
        gameObject.name = "LocalLobbyPlayer";
        LobbyManager.instance.FindLocalLobbyPlayer();
        LobbyManager.instance.UpdateLobbyName();
        CmdSetPlayerInitialTeam();
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        //UpdateTeamListsOnLobbyManager();
    }
    [Command]
    private void CmdSetPlayerName(string PlayerNameSubmitted)
    {
        Debug.Log("CmdSetPlayerName: Setting player name to: " + PlayerNameSubmitted);
        string playerNameToSet = "";
        if (PlayerNameSubmitted.Length > 12)
            playerNameToSet = PlayerNameSubmitted.Substring(0, 12);
        else
            playerNameToSet = PlayerNameSubmitted;
        this.HandlePlayerNameUpdate(this.PlayerName, playerNameToSet);
    }
    [Command]
    void CmdSetPlayerInitialTeam()
    {
        // set the host as team grey
        if (this.IsGameLeader)
            HandleIsTeamGrey(this.isTeamGrey, false);
        else
        {
            if (LobbyManager.instance.GreenTeamMembers.Count > LobbyManager.instance.GreyTeamMembers.Count)
            {
                this.HandleIsTeamGrey(this.isTeamGrey, true);
            }
            else if (LobbyManager.instance.GreenTeamMembers.Count < LobbyManager.instance.GreyTeamMembers.Count)
            {
                this.HandleIsTeamGrey(this.isTeamGrey, false);
            }
            else
            {
                string[] headsTails = new[]
                { "green","grey"};
                var rng = new System.Random();
                string result = headsTails[rng.Next(headsTails.Length)];
                if (result == "green")
                {
                    this.HandleIsTeamGrey(this.isTeamGrey, false);
                }
                else
                {
                    this.HandleIsTeamGrey(this.isTeamGrey, true);
                }
            }
        }
        UpdateTeamListsOnLobbyManager();
        UpdateAllPlayersOnAvailableGoblins(this.isTeamGrey);
    }
    public void HandlePlayerNameUpdate(string oldValue, string newValue)
    {
        Debug.Log("Player name has been updated for: " + oldValue + " to new value: " + newValue);
        if (isServer)
            this.PlayerName = newValue;
        if (isClient)
        {
            LobbyManager.instance.UpdateUI();
        }

    }
    public void UpdateTeam(bool isGrey)
    {
        Debug.Log("UpdateTeam to grey? " + isGrey.ToString() + " for player " + this.PlayerName);
        if (hasAuthority)
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
    [Command]
    void CmdUpdateTeam(bool isGrey)
    {
        Debug.Log("CmdUpdateTeam to grey? " + isGrey.ToString() + " for player " + this.PlayerName + " are they already grey? " + isGrey.ToString());
        if (this.isTeamGrey != isGrey)
        {
            if (isGrey && LobbyManager.instance.GreyTeamMembers.Count >= 3)
            {
                Debug.Log("CmdUpdateTeam: " + this.PlayerName + " is trying to switch to GREY but GREY is full. Setting wantsToSwitch To True");
                this.HandleWantsToSwitchTeams(this.wantsToSwitchTeams, true);
                CheckIfOtherPlayersWantToSwitch(this);
                return;
            }
            else if (!isGrey && LobbyManager.instance.GreenTeamMembers.Count >= 3)
            {
                Debug.Log("CmdUpdateTeam: " + this.PlayerName + " is trying to switch to GREEN but GREEN is full. Setting wantsToSwitch To True");
                this.HandleWantsToSwitchTeams(this.wantsToSwitchTeams, true);
                CheckIfOtherPlayersWantToSwitch(this);
                return;
            }
            this.HandleIsTeamGrey(this.isTeamGrey, isGrey);
            if (this.wantsToSwitchTeams)
                this.HandleWantsToSwitchTeams(this.wantsToSwitchTeams, false);
            UpdateAllPlayersOnAvailableGoblins(isGrey);
        }
    }
    [ServerCallback]
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
                playerRequestingSwitch.HandleIsTeamGrey(playerRequestingSwitch.isTeamGrey, !playerRequestingSwitch.isTeamGrey);
                playerRequestingSwitch.HandleWantsToSwitchTeams(playerRequestingSwitch.wantsToSwitchTeams, false);


                if (player.isGoblinSelected && !string.IsNullOrWhiteSpace(player.goblinType))
                {
                    player.UnselectGoblinOnServer();
                }
                UpdateAllPlayersOnAvailableGoblins(player.isTeamGrey);
                player.HandleIsTeamGrey(player.isTeamGrey, !player.isTeamGrey);
                player.HandleWantsToSwitchTeams(player.wantsToSwitchTeams, false);
                break;
            }
        }
    }
    [ServerCallback]
    void CheckIfTeamIsNoLongerFull()
    {
        foreach (LobbyPlayer player in Game.LobbyPlayers)
        {
            if (player.wantsToSwitchTeams)
            {
                if (player.isTeamGrey)
                {
                    Debug.Log("CheckIfTeamIsNoLongerFull: player " + player.PlayerName + " is on team grey and wants to switch to team green.");
                    if (LobbyManager.instance.GreenTeamMembers.Count >= 3)
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
                        player.HandleIsTeamGrey(player.isTeamGrey, !player.isTeamGrey);
                        player.HandleWantsToSwitchTeams(player.wantsToSwitchTeams, false);
                    }
                }
                else
                {
                    Debug.Log("CheckIfTeamIsNoLongerFull: player " + player.PlayerName + " is on team green and wants to switch to team grey.");
                    if (LobbyManager.instance.GreyTeamMembers.Count >= 3)
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
                        player.HandleIsTeamGrey(player.isTeamGrey, !player.isTeamGrey);
                        player.HandleWantsToSwitchTeams(player.wantsToSwitchTeams, false);
                    }
                }
            }
        }
        UpdateTeamListsOnLobbyManager();
    }
    public void StartUpdateTeamListsOnLobbyManager()
    {
        if (hasAuthority)
            CmdStartUpdateTeamListsOnLobbyManager();
    }
    [Command]
    void CmdStartUpdateTeamListsOnLobbyManager()
    {
        UpdateTeamListsOnLobbyManager();
    }
    [ServerCallback]
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
        if (isServer)
        {
            if (!this.isSinglePlayer)
            {
                this.isTeamGrey = newValue;
                UpdateTeamListsOnLobbyManager();
                UpdateAllPlayersOnAvailableGoblins(newValue);
            }
        }   
        if (isClient)
        {
            if (!this.isSinglePlayer)
            {
                LobbyManager.instance.UpdateUI();
            }
        }
    }
    public override void OnStartClient()
    {
        Game.LobbyPlayers.Add(this);
        LobbyManager.instance.UpdateLobbyName();
        LobbyManager.instance.UpdateUI();
        StartUpdateTeamListsOnLobbyManager();
    }
    public void ChangeReadyStatus()
    {
        Debug.Log("Executing ChangeReadyStatus for player: " + this.PlayerName);
        if (hasAuthority)
            CmdChangePlayerReadyStatus();
    }
    [Command]
    void CmdChangePlayerReadyStatus()
    {
        Debug.Log("Executing CmdChangePlayerReadyStatus on the server for player: " + this.PlayerName);
        this.HandlePlayerReadyStatusChange(this.isPlayerReady, !this.isPlayerReady);
    }
    void HandlePlayerReadyStatusChange(bool oldValue, bool newValue)
    {
        if (isServer)
            this.isPlayerReady = newValue;
        if (isClient && ! this.isSinglePlayer)
            LobbyManager.instance.UpdateUI();
    }
    public void CanLobbyStartGame()
    {
        if (hasAuthority)
            CmdCanLobbyStartGame();
    }
    [Command]
    void CmdCanLobbyStartGame()
    {
        Debug.Log("CmdCanLobbyStartGame");
        Game.StartGame();
    }
    public void UnselectGoblin()
    {
        if (hasAuthority)
        {
            CmdUnselectGoblin();
            if (this.isPlayerReady)
                CmdChangePlayerReadyStatus();
        }
            
    }
    [Command]
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
    [ServerCallback]
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
        this.HandleIsGoblinSelected(this.isGoblinSelected, false);
        this.HandleGoblinType(this.goblinType, "");
    }
    public void SelectGoblin(string typeOfGoblin)
    {
        if (hasAuthority)
        {
            CmdSelectGoblin(typeOfGoblin);
        }
    }
    [Command]
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
            this.HandleGoblinType(this.goblinType, typeOfGoblin);
            this.HandleIsGoblinSelected(this.isGoblinSelected, true);
        }   
    }
    public void HandleIsGoblinSelected(bool oldValue, bool newValue)
    {
        if (isServer)
            this.isGoblinSelected = newValue;
        if (isClient)
        {
            if (hasAuthority)
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
        if (isServer)
            this.goblinType = newValue;
        if (isClient)
        {
            if (hasAuthority)
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
    [ServerCallback]
    void UpdateAllPlayersOnAvailableGoblins(bool isGrey)
    {
        foreach (LobbyPlayer player in Game.LobbyPlayers)
        {
            if (player.isTeamGrey == isGrey)
            {
                Debug.Log("UpdateAllPlayersOnAvailableGoblins: Player name: " + player.PlayerName + " player connection id: " + player.ConnectionId);
                if (isGrey)
                {
                    Debug.Log("UpdateAllPlayersOnAvailableGoblins: Sending RpcSendPlayersUpdatedAvailableGoblinList to Player name: " + player.PlayerName + " player connection id: " + player.ConnectionId.ToString() + " " + player.connectionToClient.ToString());
                    player.RpcSendPlayersUpdatedAvailableGoblinList(player.connectionToClient, LobbyManager.instance.GreyGoblinsAvailable);
                }
                else
                {
                    Debug.Log("UpdateAllPlayersOnAvailableGoblins: Sending RpcSendPlayersUpdatedAvailableGoblinList to Player name: " + player.PlayerName + " player connection id: " + player.ConnectionId.ToString() + " " + player.connectionToClient.ToString());
                    player.RpcSendPlayersUpdatedAvailableGoblinList(player.connectionToClient, LobbyManager.instance.GreenGoblinsAvailable);
                }   
            }
        }
    }
    [TargetRpc]
    public void RpcSendPlayersUpdatedAvailableGoblinList(NetworkConnection target, List<string> availableGoblins)
    {
        Debug.Log("RpcSendPlayersUpdatedAvailableGoblinList: for player: " + this.PlayerName + " is team grey? " + this.isTeamGrey.ToString() + " and list available: " + availableGoblins.Count.ToString() + " for UpdateGoblinsAvailable");
        if(hasAuthority && !LobbyManager.instance.is1v1)
            this.myPlayerListItem.UpdateGoblinsAvailable(availableGoblins);
    }
    /*[ClientRpc]
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
        if (isServer)
        {
            wantsToSwitchTeams = newValue;
        }
        if (isClient)
        { 

        }
    }
    public void QuitLobby()
    {
        if (hasAuthority)
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
        if (hasAuthority)
        {
            LobbyManager.instance.DestroyPlayerListItems();
            //SteamMatchmaking.LeaveLobby((CSteamID)LobbyManager.instance.currentLobbyId);
        }
        if (isServer)
        { 

        }
        Debug.Log("LobbyPlayer destroyed. Returning to main menu.");
    }
    public override void OnStopClient()
    {
        Debug.Log(PlayerName + " is quiting the game.");
        Game.LobbyPlayers.Remove(this);
        Debug.Log("Removed player from the GamePlayer list: " + this.PlayerName);
        LobbyManager.instance.UpdateUI();
        if (isServer)
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

}
