using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;
using Cinemachine;

public class GamePlayer : NetworkBehaviour
{
    [Header("Player Info")]
    [SyncVar] public string PlayerName;
    [SyncVar] public int ConnectionId;
    [SyncVar] public int playerNumber;
    [SyncVar] public bool IsGameLeader;

    [Header("Coin Toss Info")]
    public bool inputManagerActivated = false;
    [SyncVar(hook = nameof(HandleDoesPlayerChooseCoin))] public bool doesPlayerChooseCoin;
    [SyncVar(hook = nameof(HandleHeadsOrTails))] public string headsOrTailsPlayer;
    [SyncVar(hook = nameof(HandleDidPlayerChooseCoinYet))] public bool didPlayerChooseCoinYet;

    [Header("Kick or Receive Stuff")]
    [SyncVar(hook = nameof(HandleDoesPlayerChooseKickOrReceive))] public bool doesPlayerChooseKickOrReceive;
    [SyncVar(hook = nameof(HandleKickOrReceivePlayer))] public string kickOrReceivePlayer;
    [SyncVar(hook = nameof(HandleDidPlayerChooseKickOrReceiveYet))] public bool didPlayerChooseKickOrReceiveYet;

    [Header("Characters")]
    [SerializeField] private GameObject grenadierPrefab;
    [SerializeField] private GameObject skrimisherPrefab;
    [SerializeField] private GameObject berserkerPrefab;
    [SyncVar] public bool areCharactersSpawnedYet = false;

    [Header("My Goblin Team")]
    public SyncList<uint> goblinTeamNetIds = new SyncList<uint>();
    public List<GoblinScript> goblinTeam = new List<GoblinScript>();
    public List<GoblinScript> goblinTeamOnServer = new List<GoblinScript>();
    public GoblinScript selectGoblin;
    public GoblinScript qGoblin;
    public GoblinScript eGoblin;
    public bool canSwitchGoblin = true;
    public GoblinScript serverSelectGoblin;

    [Header("Team Info")]
    [SyncVar] public string teamName;
    [SyncVar] public bool doesTeamHaveBall;

    [Header("Football")]
    [SerializeField] private GameObject footballPrefab;

    [SerializeField] CinemachineVirtualCamera myCamera;
    public Football football;

    [Header("Input Manager Controls")]
    public bool coinTossControllsEnabled = false;
    public bool kickOrReceiveControlsEnabled = false;
    public bool qeSwitchingEnabled = false;
    public bool kickingControlsEnabled = false;
    public bool kickoffAimArrowControlsEnabled = false;
    public bool goblinMovementEnabled = false;
    public bool gameplayActionsEnabled = false;


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
    public override void OnStartAuthority()
    {
        Debug.Log("OnStartAuthority for " + this.PlayerName);
        base.OnStartAuthority();
        gameObject.name = "LocalGamePlayer";
        gameObject.tag = "LocalGamePlayer";


        //Have these enabled when the "Gameplay" phase starts?
        /*InputManager.Controls.Player.SwitchQ.performed += _ => SwitchToQGoblin(true);
        InputManager.Controls.Player.SwitchE.performed += _ => SwitchToEGoblin();
        InputManager.Controls.Player.Attack.performed += _ => GoblinAttack();
        InputManager.Controls.Player.Slide.performed += _ => SlideGoblin();
        InputManager.Controls.Player.Dive.performed += _ => DiveGoblin();
        InputManager.Controls.Player.Block.performed += _ => StartBlockGoblin();
        InputManager.Controls.Player.Block.canceled += _ => StopBlockGoblin();
        //InputManager.Controls.Player.KickFootball.performed += _ => KickFootball();
        InputManager.Controls.Player.KickFootball.performed += _ => StartKickPower();
        InputManager.Controls.Player.KickFootball.canceled += _ => EndKickPower();
        */

        //myCamera = GameObject.FindGameObjectWithTag("camera").GetComponent<CinemachineVirtualCamera>();

        /*TrackFootballScript footballTracker = Camera.main.GetComponent<TrackFootballScript>();
        if (!footballTracker.myPlayer)
        {
            footballTracker.myPlayer = this;
        }*/
        /*CameraMarker myCameraMarker = Camera.main.GetComponent<CameraMarker>();
        if (!myCameraMarker.myPlayer)
            myCameraMarker.myPlayer = this;*/

        //CoinTossControlls(true);
    }

    public override void OnStartClient()
    {
        Debug.Log("OnStartClient Spawned GamePlayer with ConnectionId: " + ConnectionId.ToString() + this.PlayerName);
        DontDestroyOnLoad(gameObject);
        base.OnStartClient();
        Game.GamePlayers.Add(this);
        Debug.Log("OnStartClient: Will check if player has authority GamePlayer with ConnectionId: " + ConnectionId.ToString());
        /*if (hasAuthority)
        {
            myCamera = GameObject.FindGameObjectWithTag("camera").GetComponent<CinemachineVirtualCamera>();
            CameraMarker myCameraMarker = Camera.main.GetComponent<CameraMarker>();
            if (!myCameraMarker.myPlayer)
                myCameraMarker.myPlayer = this;
        }
        if (hasAuthority)
        {
            Debug.Log("OnStartClient: try to get team name, spawn football, and spawn goblin team for connection id: " + ConnectionId.ToString() + this.PlayerName);
            //CmdSpawnPlayerCharacters();
            CmdGetTeamName();
            CmdSpawnFootball();
            CmdSpawnPlayerCharacters();

        }
        if (hasAuthority)
        {
            Debug.Log("OnStartClient: try to find football for connection id: " + ConnectionId.ToString() + this.PlayerName);
            football = GameObject.FindGameObjectWithTag("football").GetComponent<Football>();
            if (football)
                football.localPlayer = this;
        }*/

    }
    [Server]
    public void SetPlayerName(string playerName)
    {
        this.PlayerName = playerName;
    }
    [Server]
    public void SetConnectionId(int connId)
    {
        this.ConnectionId = connId;
    }
    [Server]
    public void SetPlayerNumber(int playerNum)
    {
        this.playerNumber = playerNum;
    }
    public void InitializeLocalGamePlayer()
    {
        if (hasAuthority)
        {
            Debug.Log("InitializeLocalGamePlayer: Get camera stuff");
            myCamera = GameObject.FindGameObjectWithTag("camera").GetComponent<CinemachineVirtualCamera>();
            CameraMarker myCameraMarker = Camera.main.GetComponent<CameraMarker>();
            if (!myCameraMarker.myPlayer)
                myCameraMarker.myPlayer = this;
            Debug.Log("InitializeLocalGamePlayer: try to get team name, spawn football, and spawn goblin team for connection id: " + ConnectionId.ToString() + this.PlayerName);
            //CmdSpawnPlayerCharacters();
            CmdGetTeamName();
            CmdDoesPlayerChooseCoin();
            CmdSpawnFootball();
            CmdSpawnPlayerCharacters();

            Debug.Log("InitializeLocalGamePlayer: try to find football for connection id: " + ConnectionId.ToString() + this.PlayerName);
            football = GameObject.FindGameObjectWithTag("football").GetComponent<Football>();
            if (football)
                football.localPlayer = this;

            CmdPlayerFinishedLoading();
        }
    }
    [Command]
    void CmdPlayerFinishedLoading()
    {
        Game.ReportPlayerFinishedLoading(this.playerNumber);
    }
    [Command]
    void CmdSpawnFootball()
    {
        Debug.Log("CmdSpawnFootball for GamePlayer with ConnectionId: " + ConnectionId.ToString());
        bool doesFootballExist = GameObject.FindGameObjectWithTag("football");
        Debug.Log("CmdSpawnFootball" + doesFootballExist.ToString());
        if (!doesFootballExist)
        {
            GameObject newFootball = Instantiate(footballPrefab);
            newFootball.transform.position = new Vector3(0f, 0f, 0f);
            NetworkServer.Spawn(newFootball);
        }
    }
    [Command]
    void CmdGetTeamName()
    {
        Debug.Log("CmdGetTeamName for GamePlayer with ConnectionId: " + ConnectionId.ToString());
        if (IsGameLeader)
            teamName = "Green";
        else
            teamName = "Grey";
    }
    [Command]
    void CmdSpawnPlayerCharacters()
    {
        Debug.Log("CmdSpawnPlayerCharacters for GamePlayer with ConnectionId: " + ConnectionId.ToString());
        if (!areCharactersSpawnedYet)
        {
            Debug.Log("Executing SpawnPlayerCharacters on the server for player " + this.PlayerName + this.ConnectionId.ToString());
            GameObject newGrenadier = Instantiate(grenadierPrefab);
            if (IsGameLeader)
                newGrenadier.transform.position = new Vector3(-9f, 4.45f, 0f);
            else
            {
                newGrenadier.transform.position = new Vector3(9f, 4.45f, 0f);
                //newGrenadier.transform.localScale = new Vector3(-1f,1f,1f);
            }
            NetworkServer.Spawn(newGrenadier, connectionToClient);
            goblinTeamNetIds.Add(newGrenadier.GetComponent<NetworkIdentity>().netId);
            GoblinScript newGrenadierScript = newGrenadier.GetComponent<GoblinScript>();
            newGrenadierScript.ownerConnectionId = this.ConnectionId;
            newGrenadierScript.ownerNetId = this.GetComponent<NetworkIdentity>().netId;
            newGrenadierScript.health = newGrenadierScript.MaxHealth;
            newGrenadierScript.stamina = newGrenadierScript.MaxStamina;
            newGrenadierScript.speed = newGrenadierScript.MaxSpeed;
            newGrenadierScript.damage = newGrenadierScript.MaxDamage;
            newGrenadierScript.goblinType = "grenadier";
            if (!IsGameLeader)
                newGrenadierScript.goblinType += "-grey";
            newGrenadierScript.serverGamePlayer = this;
            goblinTeamOnServer.Add(newGrenadierScript);


            GameObject newBerserker = Instantiate(berserkerPrefab, transform.position, Quaternion.identity);
            if (IsGameLeader)
                newBerserker.transform.position = new Vector3(-9f, 0f, 0f);
            else
            {
                newBerserker.transform.position = new Vector3(9f, 0f, 0f);
                //newBerserker.transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            NetworkServer.Spawn(newBerserker, connectionToClient);
            goblinTeamNetIds.Add(newBerserker.GetComponent<NetworkIdentity>().netId);
            GoblinScript newBerserkerScript = newBerserker.GetComponent<GoblinScript>();
            newBerserkerScript.ownerConnectionId = this.ConnectionId;
            newBerserkerScript.ownerNetId = this.GetComponent<NetworkIdentity>().netId;
            newBerserkerScript.health = newBerserkerScript.MaxHealth;
            newBerserkerScript.stamina = newBerserkerScript.MaxStamina;
            newBerserkerScript.speed = newBerserkerScript.MaxSpeed;
            newBerserkerScript.damage = newBerserkerScript.MaxDamage;
            newBerserkerScript.goblinType = "berserker";
            if (!IsGameLeader)
                newBerserkerScript.goblinType += "-grey";
            newBerserkerScript.serverGamePlayer = this;
            goblinTeamOnServer.Add(newBerserkerScript);

            GameObject newSkirmisher = Instantiate(skrimisherPrefab, transform.position, Quaternion.identity);
            if (IsGameLeader)
                newSkirmisher.transform.position = new Vector3(-9f, -4.45f, 0f);
            else
            {
                newSkirmisher.transform.position = new Vector3(9f, -4.45f, 0f);
                //newSkirmisher.transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            NetworkServer.Spawn(newSkirmisher, connectionToClient);
            goblinTeamNetIds.Add(newSkirmisher.GetComponent<NetworkIdentity>().netId);
            GoblinScript newSkirmisherScript = newSkirmisher.GetComponent<GoblinScript>();
            newSkirmisherScript.ownerConnectionId = this.ConnectionId;
            newSkirmisherScript.ownerNetId = this.GetComponent<NetworkIdentity>().netId;
            newSkirmisherScript.health = newSkirmisherScript.MaxHealth;
            newSkirmisherScript.stamina = newSkirmisherScript.MaxStamina;
            newSkirmisherScript.speed = newSkirmisherScript.MaxSpeed;
            newSkirmisherScript.damage = newSkirmisherScript.MaxDamage;
            newSkirmisherScript.goblinType = "skirmisher";
            if (!IsGameLeader)
                newSkirmisherScript.goblinType += "-grey";
            goblinTeamOnServer.Add(newSkirmisherScript);

            newSkirmisherScript.serverGamePlayer = this;

            areCharactersSpawnedYet = true;

        }
    }
    public void AddToGoblinTeam(GoblinScript GoblinToAdd)
    {
        if (!goblinTeam.Contains(GoblinToAdd))
            goblinTeam.Add(GoblinToAdd);
        if (GoblinToAdd.gameObject.name.ToLower().Contains("grenadier"))
        {
            GoblinToAdd.SelectThisCharacter();
            selectGoblin = GoblinToAdd;
            FollowSelectedGoblin(selectGoblin.transform);
        }
        else if (!qGoblin)
        {
            qGoblin = GoblinToAdd;
            GoblinToAdd.UnSelectThisCharacter();
            GoblinToAdd.SetQGoblin(true);
        }
        else if (!eGoblin)
        {
            eGoblin = GoblinToAdd;
            GoblinToAdd.UnSelectThisCharacter();
            GoblinToAdd.SetEGoblin(true);
        }

    }
    public void SwitchToQGoblin(bool fromKeyPress)
    {
        Debug.Log("SwitchToQGoblin: " + canSwitchGoblin.ToString() + " from key press? " + fromKeyPress.ToString() );
        if ((canSwitchGoblin && !selectGoblin.isKicking && !selectGoblin.isDiving) || qGoblin.doesCharacterHaveBall)
        {
            if (doesTeamHaveBall && !qGoblin.canGoblinReceivePass)
                return;
            GoblinScript currentSelectedGoblin = selectGoblin;
            GoblinScript currentQGoblin = qGoblin;

            currentSelectedGoblin.UnSelectThisCharacter();
            currentQGoblin.SelectThisCharacter();

            selectGoblin = currentQGoblin;
            qGoblin = currentSelectedGoblin;

            currentQGoblin.SetQGoblin(false);
            currentSelectedGoblin.SetQGoblin(true);

            Debug.Log("SwitchToQGoblin: Does current selected goblin have the ball? " + currentSelectedGoblin.doesCharacterHaveBall.ToString());
            if (currentSelectedGoblin.doesCharacterHaveBall)
            {

                //ChangeBallHandler(currentSelectedGoblin, currentQGoblin);
                //ThrowBallToGoblin(currentSelectedGoblin, currentQGoblin);
                IEnumerator stopSelectedGoblin = currentSelectedGoblin.CantMove();
                IEnumerator stopQGoblin = currentQGoblin.CantMove();
                StartCoroutine(stopSelectedGoblin);
                StartCoroutine(stopQGoblin);
                currentSelectedGoblin.ThrowBall(currentQGoblin);
                IEnumerator stopSwitch = PreventGoblinSwitch();
                StartCoroutine(stopSwitch);
                FollowSelectedGoblin(GameObject.FindGameObjectWithTag("football").transform);
            }
            else
            {
                FollowSelectedGoblin(selectGoblin.transform);
            }
            Debug.Log("SwitchToQGoblin switching to goblin: " + selectGoblin.name);
            CmdSetSelectedGoblinOnServer(selectGoblin.GetComponent<NetworkIdentity>().netId);
        }
        
    }
    public void SwitchToEGoblin()
    {
        Debug.Log("SwitchToEGoblin: " + canSwitchGoblin.ToString());
        if ((canSwitchGoblin && !selectGoblin.isKicking && !selectGoblin.isDiving) || eGoblin.doesCharacterHaveBall)
        {
            if (doesTeamHaveBall && !eGoblin.canGoblinReceivePass)
                return;
            GoblinScript currentSelectedGoblin = selectGoblin;
            GoblinScript currentEGoblin = eGoblin;

            currentSelectedGoblin.UnSelectThisCharacter();
            currentEGoblin.SelectThisCharacter();

            selectGoblin = currentEGoblin;
            eGoblin = currentSelectedGoblin;

            currentEGoblin.SetEGoblin(false);
            currentSelectedGoblin.SetEGoblin(true);


            Debug.Log("SwitchToEGoblin: Does current selected goblin have the ball? " + currentSelectedGoblin.doesCharacterHaveBall.ToString());
            if (currentSelectedGoblin.doesCharacterHaveBall)
            {
                //ChangeBallHandler(currentSelectedGoblin, currentEGoblin);
                //ThrowBallToGoblin(currentSelectedGoblin, currentEGoblin);

                IEnumerator stopSelectedGoblin = currentSelectedGoblin.CantMove();
                IEnumerator stopEGoblin = currentEGoblin.CantMove();
                StartCoroutine(stopSelectedGoblin);
                StartCoroutine(stopEGoblin);

                currentSelectedGoblin.ThrowBall(currentEGoblin);

                IEnumerator stopSwitch = PreventGoblinSwitch();
                StartCoroutine(stopSwitch);
                FollowSelectedGoblin(GameObject.FindGameObjectWithTag("football").transform);
            }
            else
            {
                FollowSelectedGoblin(selectGoblin.transform);
            }
            Debug.Log("SwitchToEGoblin switching to goblin: " + selectGoblin.name);
            CmdSetSelectedGoblinOnServer(selectGoblin.GetComponent<NetworkIdentity>().netId);

        }
        
    }
    void ChangeBallHandler(GoblinScript oldBallHandler, GoblinScript newBallHandler)
    {
        if (oldBallHandler.doesCharacterHaveBall)
            oldBallHandler.SetGoblinHasBall(false);
        if (!newBallHandler.doesCharacterHaveBall)
            newBallHandler.SetGoblinHasBall(true);
    }
    void ThrowBallToGoblin(GoblinScript goblinWithBall, GoblinScript goblinToThrowTo)
    { 

    }
    void GoblinAttack()
    {
        if (selectGoblin)
        {
            if (!selectGoblin.doesCharacterHaveBall && !selectGoblin.isPunching)
            {
                selectGoblin.Attack();
            }
        }
    }
    public IEnumerator PreventGoblinSwitch()
    {
        canSwitchGoblin = false;
        yield return new WaitForSeconds(0.2f);
        canSwitchGoblin = true;
    }
    void SlideGoblin()
    {
        Debug.Log("SlideGoblin");
        if (selectGoblin)
        {
            if (!selectGoblin.doesCharacterHaveBall && !selectGoblin.isSliding && !selectGoblin.isDiving && !selectGoblin.isPunching)
            {
                selectGoblin.SlideGoblin();
            }
        }
    }
    void DiveGoblin()
    {
        Debug.Log("DiveGoblin");
        if (selectGoblin)
        {
            if (!selectGoblin.isSliding && !selectGoblin.isDiving && !selectGoblin.isPunching)
            {
                selectGoblin.DiveGoblin();
            }
        }
    }
    void StartBlockGoblin()
    {
        Debug.Log("StartBlockGoblin");
        if (selectGoblin)
        {
            if (!selectGoblin.isSliding && !selectGoblin.isDiving && !selectGoblin.isGoblinKnockedOut && !selectGoblin.isPunching)
            {
                selectGoblin.StartBlocking();
            }
        }
    }
    void StopBlockGoblin()
    {
        Debug.Log("StopBlockGoblin");
        if (selectGoblin)
        {
            if (!selectGoblin.isSliding && !selectGoblin.isDiving)
            {
                selectGoblin.StopBlocking();
            }
        }
    }
    public void FollowSelectedGoblin(Transform goblinToFollow)
    {
        if(GameplayManager.instance.gamePhase != "cointoss")
            myCamera.Follow = goblinToFollow.transform;
    }
    void KickFootball()
    {
        Debug.Log("KickFootball executed ");
        if (selectGoblin)
        {
            if (!selectGoblin.isSliding && !selectGoblin.isDiving && !selectGoblin.isGoblinKnockedOut && !selectGoblin.isPunching)
            {
                //selectGoblin.KickFootballGoblin();
            }
        }
    }
    [Command]
    public void CmdSetSelectedGoblinOnServer(uint goblinNetId)
    {
        Debug.Log("CmdSetSelectedGoblinOnServer: player " + this.PlayerName + " is switching to goblin with netid " + goblinNetId.ToString());
        serverSelectGoblin = NetworkIdentity.spawned[goblinNetId].gameObject.GetComponent<GoblinScript>();
    }
    void StartKickPower()
    {
        Debug.Log("StartKickPower");
        if (selectGoblin.doesCharacterHaveBall)
            selectGoblin.StartKickPower();
    }
    void EndKickPower()
    {
        Debug.Log("EndKickPower");
        if (selectGoblin.doesCharacterHaveBall)
            selectGoblin.StopKickPower();
    }
    public void ReportPlayerSpawnedFootball()
    {
        if (hasAuthority)
            CmdReportFootballSpawned();
    }
    [Command]
    void CmdReportFootballSpawned()
    {
        Game.ReportFootballSpawnedForPlayer(this.playerNumber);
    }
    public void ReportPlayerGoblinSpawned()
    {
        if (hasAuthority)
            CmdReportPlayerGoblinSpawned();
    }
    [Command]
    void CmdReportPlayerGoblinSpawned()
    {
        Game.ReportGoblinSpawnedForPlayer(this.playerNumber);
    }
    public void DoneLoadingGameplayStuff()
    {
        RpcDoneLoadingGameplayStuff();
    }
    [ClientRpc]
    void RpcDoneLoadingGameplayStuff()
    {
        if (hasAuthority)
        {
            Game.DestroyWaitingForPlayersCanvas();
            GameplayManager.instance.ActivateCoinTossUI(true);
        }
            
    }
    public void EnableGoblinMovement(bool enableOrDisable)
    {
        if (hasAuthority)
        {
            if (enableOrDisable)
            {
                if (!goblinMovementEnabled)
                {
                    foreach (GoblinScript goblin in goblinTeam)
                    {
                        goblin.EnableGoblinMovement(enableOrDisable);
                    }
                    goblinMovementEnabled = true;
                }
            }
            else
            {
                foreach (GoblinScript goblin in goblinTeam)
                {
                    goblin.EnableGoblinMovement(enableOrDisable);
                }
                goblinMovementEnabled = false;
            }
        }
    }
    [Command]
    void CmdDoesPlayerChooseCoin()
    {
        if (this.playerNumber == 2)
            HandleDoesPlayerChooseCoin(doesPlayerChooseCoin, true);
        else
            HandleDoesPlayerChooseCoin(doesPlayerChooseCoin, false);
    }
    public void HandleDoesPlayerChooseCoin(bool oldValue, bool newValue)
    {
        if (isServer)
            doesPlayerChooseCoin = newValue;
        if (isClient)
        {
            if (hasAuthority)
            {
                CoinTossControlls(newValue);
                CoinTossManager.instance.ActivateSelectStuffToShowtoCoinSelecter(newValue);
                CoinTossManager.instance.SetInitialSelectionText(newValue);
            }
                
        }
    }
    public void CoinTossControlls(bool activate)
    {
        if (activate)
        {
            if (doesPlayerChooseCoin)
            {
                if (!coinTossControllsEnabled)
                {
                    InputManager.Controls.Player.SelectHeads.Enable();
                    InputManager.Controls.Player.SelectTails.Enable();
                    InputManager.Controls.Player.SubmitCoin.Enable();
                    InputManager.Controls.Player.SelectHeads.performed += _ => SelectCoin("heads");
                    InputManager.Controls.Player.SelectTails.performed += _ => SelectCoin("tails");
                    InputManager.Controls.Player.SubmitCoin.performed += _ => SubmitCoinSelection();
                    coinTossControllsEnabled = true;
                }
                
            }            
        }
        else
        {
            InputManager.Controls.Player.SelectHeads.Disable();
            InputManager.Controls.Player.SelectTails.Disable();
            InputManager.Controls.Player.SubmitCoin.Disable();
            coinTossControllsEnabled = false;
        }
    }
    public void KickOrReceiveControls(bool activate)
    {
        if (activate)
        {
            Debug.Log("KickOrReceiveControls: doesPlayerChooseKickOrReceive " + doesPlayerChooseKickOrReceive.ToString());
            if (doesPlayerChooseKickOrReceive)
            {
                
            }
            if (!kickOrReceiveControlsEnabled)
            {
                Debug.Log("KickOrReceiveControls: controls enabled.");
                InputManager.Controls.Player.SelectHeads.Enable();
                InputManager.Controls.Player.SelectTails.Enable();
                InputManager.Controls.Player.SubmitCoin.Enable();
                InputManager.Controls.Player.SelectHeads.performed += _ => SelectKickOrReceive("receive");
                InputManager.Controls.Player.SelectTails.performed += _ => SelectKickOrReceive("kick");
                InputManager.Controls.Player.SubmitCoin.performed += _ => SubmitKickOrReceiveSelection();
                kickOrReceiveControlsEnabled = true;
            }
        }
        else
        {
            InputManager.Controls.Player.SelectHeads.Disable();
            InputManager.Controls.Player.SelectTails.Disable();
            InputManager.Controls.Player.SubmitCoin.Disable();
            kickOrReceiveControlsEnabled = false;
        }
    }
    void SelectCoin(string headsOrTails)
    {
        Debug.Log("SelectCoin: " + headsOrTails);
        if (hasAuthority)
            CmdSelectCoin(headsOrTails);
    }
    [Command]
    void CmdSelectCoin(string headsOrTails)
    {
        if (doesPlayerChooseCoin && !didPlayerChooseCoinYet)
        {
            headsOrTailsPlayer = headsOrTails;
        }            
    }
    void HandleHeadsOrTails(string oldValue, string newValue)
    {
        if (isServer)
            headsOrTailsPlayer = newValue;
        if (isClient)
        {
            if (hasAuthority && doesPlayerChooseCoin)
            {
                CoinTossManager.instance.SelectionArrow(newValue);
            }
        }
    }
    void SubmitCoinSelection()
    {
        Debug.Log("SubmitCoinSelection: " + headsOrTailsPlayer);
        if (!String.IsNullOrWhiteSpace(headsOrTailsPlayer) && hasAuthority)
        {
            CmdSubmitCoinSelection();
        }
    }
    [Command]
    void CmdSubmitCoinSelection()
    {
        //didPlayerChooseCoinYet = true;
        if (!didPlayerChooseCoinYet)
        {
            CoinTossManager.instance.playerSelectedCoin = headsOrTailsPlayer;
            CoinTossManager.instance.ServerPlayerSelectedTheirCoin();
            CoinTossManager.instance.FlipCoin();
            HandleDidPlayerChooseCoinYet(didPlayerChooseCoinYet, true);
        }
        
    }
    void HandleDidPlayerChooseCoinYet(bool oldValue, bool newValue)
    {
        if (isServer)
            didPlayerChooseCoinYet = newValue;
        if (isClient)
        {
            CoinTossManager.instance.PlayerSelectedCoin(headsOrTailsPlayer);
        }
    }
    public void HandleDoesPlayerChooseKickOrReceive(bool oldValue, bool newValue)
    {
        if (isServer)
            doesPlayerChooseKickOrReceive = newValue;
        if (isClient)
        {
            if (hasAuthority)
            {
                //if(newValue)
                   // CoinTossControlls(!newValue);
                //KickOrReceiveControls(newValue);                
                CoinTossManager.instance.ActivateSelectStuffToShowtoReceiveOrKickSelecter(newValue);
                CoinTossManager.instance.SetInitialSelectionText(newValue);
            }

        }
    }
    void SelectKickOrReceive(string kickOrReceive)
    {
        Debug.Log("SelectKickOrReceive: " + kickOrReceive);
        if (hasAuthority)
            CmdSelectKickOrReceive(kickOrReceive);
    }
    [Command]
    void CmdSelectKickOrReceive(string kickOrReceive)
    {
        Debug.Log("CmdSelectKickOrReceive: " + kickOrReceive + " from player " + this.PlayerName);
        if (doesPlayerChooseKickOrReceive && !didPlayerChooseKickOrReceiveYet)
        {
            kickOrReceivePlayer = kickOrReceive;
        }
    }
    void SubmitKickOrReceiveSelection()
    {
        Debug.Log("SubmitKickOrReceiveSelection: " + kickOrReceivePlayer);
        if (!String.IsNullOrWhiteSpace(kickOrReceivePlayer) && hasAuthority)
        {
            CmdSubmitKickOrReceiveSelection();
        }
    }
    [Command]
    void CmdSubmitKickOrReceiveSelection()
    {
        //didPlayerChooseCoinYet = true;
        if (!didPlayerChooseKickOrReceiveYet)
        {
            CoinTossManager.instance.playerKickOrReceive = kickOrReceivePlayer;
            CoinTossManager.instance.ServerPlayerSelectedKickOrReceive();
            HandleDidPlayerChooseKickOrReceiveYet(didPlayerChooseKickOrReceiveYet, true);
        }
    }
    [ClientRpc]
    public void RpcCoinTossAndKickOrReceiveControllerActivation(bool activateCoinTossControlls, bool activateKickOrReceiveControlls)
    {
        if (hasAuthority)
        {
            Debug.Log("RpcCoinTossAndKickOrReceiveControllerActivation: " + activateCoinTossControlls.ToString() + " " + activateKickOrReceiveControlls.ToString());
            CoinTossControlls(activateCoinTossControlls);
            KickOrReceiveControls(activateKickOrReceiveControlls);
        }        
    }
    void HandleKickOrReceivePlayer(string oldValue, string newValue)
    {
        if (isServer)
            kickOrReceivePlayer = newValue;
        if (isClient)
        {
            if (hasAuthority && doesPlayerChooseKickOrReceive)
            {
                CoinTossManager.instance.KickOrReceiveSelectionArrow(newValue);
            }
        }
    }
    void HandleDidPlayerChooseKickOrReceiveYet(bool oldValue, bool newValue)
    {
        if (isServer)
            didPlayerChooseKickOrReceiveYet = newValue;
        if (isClient)
        {
            CoinTossManager.instance.PlayerSelectedKickOrReceive(kickOrReceivePlayer, teamName);
        }
    }
    [ClientRpc]
    public void RpcRepositionTeamForKickOff(bool isKicking)
    {
        Debug.Log("PositionTeamForKickOff: for player " + this.PlayerName + ". Is the player kicking? " + isKicking.ToString());
        if (hasAuthority)
        {
            EnableKickingControls(isKicking);
            EnableKickoffAimArrowControls(isKicking);
            EnableQESwitchingControls(!isKicking);
            if (isKicking)
            {
                if (teamName == "Grey")
                {
                    foreach (GoblinScript goblin in goblinTeam)
                    {
                        if (goblin.goblinType.Contains("grenadier"))
                        {
                            goblin.transform.position = new Vector3(0f, 1f, 0f);
                            //goblin.ToggleGoblinBodyCollider();
                            goblin.EnableKickoffAimArrow(true);
                        }
                        else if (goblin.goblinType.Contains("berserker"))
                        {
                            goblin.transform.position = new Vector3(1f, -3f, 0f);
                        }
                        else
                        {
                            goblin.transform.position = new Vector3(1f, 4f, 0f);
                        }
                    }
                }
                else
                {
                    foreach (GoblinScript goblin in goblinTeam)
                    {
                        if (goblin.goblinType.Contains("grenadier"))
                        {
                            goblin.transform.position = new Vector3(0f, 1f, 0f);
                            goblin.EnableKickoffAimArrow(true);
                        }
                        else if (goblin.goblinType.Contains("berserker"))
                        {
                            goblin.transform.position = new Vector3(-1f, -3f, 0f);
                        }
                        else
                        {
                            goblin.transform.position = new Vector3(-1f, 4f, 0f);
                        }
                    }
                }
            }
            else
            {
                if (teamName == "Grey")
                {
                    foreach (GoblinScript goblin in goblinTeam)
                    {
                        if (goblin.goblinType.Contains("grenadier"))
                        {
                            goblin.transform.position = new Vector3(11f, 3f, 0f);
                            //goblin.GetComponent<Rigidbody2D>().MovePosition(new Vector2(11f, 3f));
                        }
                        else if (goblin.goblinType.Contains("skirmisher"))
                        {
                            goblin.transform.position = new Vector3(20f, 0f, 0f);
                            //goblin.GetComponent<Rigidbody2D>().MovePosition(new Vector2(20f, 0f));
                        }
                        else
                        {
                            goblin.transform.position = new Vector3(11f, -2f, 0f);
                            //goblin.GetComponent<Rigidbody2D>().MovePosition(new Vector2(11f, -2f));
                        }
                    }
                }
                else
                {
                    foreach (GoblinScript goblin in goblinTeam)
                    {
                        if (goblin.goblinType.Contains("grenadier"))
                        {
                            goblin.transform.position = new Vector3(-11f, 3f, 0f);
                            //goblin.GetComponent<Rigidbody2D>().MovePosition(new Vector2(-11f, 3f));
                        }
                        else if (goblin.goblinType.Contains("skirmisher"))
                        {
                            goblin.transform.position = new Vector3(-20f, 0f, 0f);
                            //goblin.GetComponent<Rigidbody2D>().MovePosition(new Vector2(-20f, 0f));
                        }
                        else
                        {
                            goblin.transform.position = new Vector3(-11f, -2f, 0f);
                            //goblin.GetComponent<Rigidbody2D>().MovePosition(new Vector2(-11f, -2f));
                        }
                    }
                }
            }
        }
        
    }
    public void EnableQESwitchingControls(bool activate)
    {
        Debug.Log("EnableQESwitchingControls: for player " + this.PlayerName + " " + activate.ToString());
        if (activate)
        {
            if (!qeSwitchingEnabled)
            {
                Debug.Log("EnableQESwitchingControls: enabling the controls.");
                InputManager.Controls.Player.SwitchQ.Enable();
                InputManager.Controls.Player.SwitchE.Enable();
                InputManager.Controls.Player.SwitchQ.performed += _ => SwitchToQGoblin(true);
                InputManager.Controls.Player.SwitchE.performed += _ => SwitchToEGoblin();
                qeSwitchingEnabled = true;
            }
            
        }
        else
        {
            InputManager.Controls.Player.SwitchQ.Disable();
            InputManager.Controls.Player.SwitchE.Disable();
            qeSwitchingEnabled = false;
        }
    }
    public void EnableKickingControls(bool activate)
    {
        Debug.Log("EnableKickingControls: for player " + this.PlayerName + " " + activate.ToString());
        if (activate)
        {
            if (!kickingControlsEnabled)
            {
                InputManager.Controls.Player.KickFootball.Enable();
                InputManager.Controls.Player.KickFootball.performed += _ => StartKickPower();
                InputManager.Controls.Player.KickFootball.canceled += _ => EndKickPower();
                kickingControlsEnabled = true;
            }            
        }
        else
        {
            InputManager.Controls.Player.KickFootball.Disable();
            kickingControlsEnabled = false;
        }
    }
    public void EnableKickoffAimArrowControls(bool activate)
    {
        Debug.Log("EnableKickoffAimArrowControls: for player " + this.PlayerName + " " + activate.ToString());
        if (activate)
        {
            if (!kickoffAimArrowControlsEnabled)
            {
                //InputManager.Controls.Player.KickFootball.Enable();
                //InputManager.Controls.Player.KickFootball.performed += _ => StartKickPower();
                //InputManager.Controls.Player.KickFootball.canceled += _ => EndKickPower();
                InputManager.Controls.Player.KickoffAngleUp.Enable();
                InputManager.Controls.Player.KickoffAngleDown.Enable();
                InputManager.Controls.Player.KickoffAngleUp.performed += _ => StartAimArrowDirection(true);
                InputManager.Controls.Player.KickoffAngleUp.canceled += _ => EndAimArrowDirection();
                InputManager.Controls.Player.KickoffAngleDown.performed += _ => StartAimArrowDirection(false);
                InputManager.Controls.Player.KickoffAngleDown.canceled += _ => EndAimArrowDirection();
                kickoffAimArrowControlsEnabled = true;
            }
        }
        else
        {
            InputManager.Controls.Player.KickoffAngleUp.Disable();
            InputManager.Controls.Player.KickoffAngleDown.Disable();
            kickoffAimArrowControlsEnabled = false;
        }
    }
    void StartAimArrowDirection(bool aimUp)
    {
        selectGoblin.StartAimArrowDirection(aimUp);
    }
    void EndAimArrowDirection()
    {
        selectGoblin.EndAimArrowDirection();
    }
    [ClientRpc]
    public void RpcActivateGameplayControls(bool activate)
    {
        if (hasAuthority)
        {
            Debug.Log("ActivateGameplayControls for player " + this.PlayerName);
            EnableKickoffAimArrowControls(false);
            EnableQESwitchingControls(activate);
            EnableKickingControls(activate);
            EnableGoblinMovement(activate);
            EnableGameplayActions(activate);
        }
    }
    void EnableGameplayActions(bool activate)
    {
        Debug.Log("EnableGameplayActions: for player " + this.PlayerName + " " + activate.ToString());
        if (activate)
        {
            if (!gameplayActionsEnabled)
            {
                Debug.Log("EnableGameplayActions: controls enabled for player " + this.PlayerName);
                InputManager.Controls.Player.Slide.Enable();
                InputManager.Controls.Player.Dive.Enable();
                InputManager.Controls.Player.Block.Enable();
                InputManager.Controls.Player.Attack.Enable();

                InputManager.Controls.Player.Attack.performed += _ => GoblinAttack();
                InputManager.Controls.Player.Slide.performed += _ => SlideGoblin();
                InputManager.Controls.Player.Dive.performed += _ => DiveGoblin();
                InputManager.Controls.Player.Block.performed += _ => StartBlockGoblin();
                InputManager.Controls.Player.Block.canceled += _ => StopBlockGoblin();
                gameplayActionsEnabled = true;
            }
            
        }
        else
        {
            InputManager.Controls.Player.Slide.Disable();
            InputManager.Controls.Player.Dive.Disable();
            InputManager.Controls.Player.Block.Disable();
            InputManager.Controls.Player.Attack.Disable();
            gameplayActionsEnabled = false;
        }
    }
    [TargetRpc]
    public void RpcRepositionForKickAfter(NetworkConnection target, bool isKickingPlayer, uint scoringGoblin, float yPosition)
    {
        if (hasAuthority)
        {
            Debug.Log("RpcRepositionForKickAfter: " + isKickingPlayer.ToString() + " for player: " + this.name + " y position: " + yPosition.ToString());
            if (isKickingPlayer)
            {
                GameObject scoringGoblinObject = NetworkIdentity.spawned[scoringGoblin].gameObject;
                Vector3 kickingPosition = scoringGoblinObject.transform.position;
                if (teamName == "Grey")
                {
                    kickingPosition.x = -30f;
                }
                else
                {
                    kickingPosition.x = 30f;
                }
                kickingPosition.y = yPosition;
                scoringGoblinObject.transform.position = kickingPosition;

                int yPositionModifier = 1;
                foreach (GoblinScript goblin in goblinTeam)
                {
                    if (goblin.GetComponent<NetworkIdentity>().netId == scoringGoblin)
                    {
                        if (goblin.isCharacterSelected)
                            FollowSelectedGoblin(goblin.transform);
                        else if (goblin.isQGoblin)
                            SwitchToQGoblin(false);
                        else if (goblin.isEGoblin)
                            SwitchToEGoblin();
                        continue;
                    }                        
                    yPositionModifier *= -1;
                    Vector3 newPosition = goblin.transform.position;
                    newPosition.x = 0f;
                    newPosition.y = 3 * yPositionModifier;
                    goblin.transform.position = newPosition;
                }
            }
            else
            {
                int yPositionModifier = 1;
                foreach (GoblinScript goblin in goblinTeam)
                {
                    Vector3 newPosition = goblin.transform.position;
                    if (goblin.goblinType.Contains( "skirmisher"))
                    {
                        newPosition.y = yPosition;
                        if (teamName == "Grey")
                        {
                            newPosition.x = 41f;
                        }
                        else
                        {
                            newPosition.x = -41f;
                        }
                        goblin.transform.position = newPosition;

                        if (goblin.isCharacterSelected)
                            FollowSelectedGoblin(goblin.transform);
                        else if (goblin.isQGoblin)
                            SwitchToQGoblin(false);
                        else if (goblin.isEGoblin)
                            SwitchToEGoblin();
                        continue;
                    }
                    yPositionModifier *= -1;

                    if (teamName == "Grey")
                    {
                        newPosition.x = 43.5f;
                    }
                    else
                    {
                        newPosition.x = -43.5f;
                    }
                    newPosition.y = 3 * yPositionModifier;
                    goblin.transform.position = newPosition;
                }
            }
            

        }
    }
}
