using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;
using Cinemachine;
using UnityEngine.InputSystem;

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
    public double qeTime;

    [Header("Team Info")]
    [SyncVar] public string teamName;
    [SyncVar] public bool doesTeamHaveBall;

    [Header("Power Ups")]
    public List<PowerUp> myPowerUps = new List<PowerUp>();
    public List<uint> serverPowerUpUints = new List<uint>();

    [Header("Football")]
    [SerializeField] private GameObject footballPrefab;

    [SerializeField] CinemachineVirtualCamera myCamera;
    public Football football;

    [Header("Kick After")]
    public bool areGoblinsRepositionedForKickAfter = false;

    [Header("Input Manager Controls")]
    public bool coinTossControllsEnabled = false;
    public bool kickOrReceiveControlsEnabled = false;
    public bool qeSwitchingEnabled = false;
    public bool kickingControlsEnabled = false;
    public bool kickoffAimArrowControlsEnabled = false;
    public bool goblinMovementEnabled = false;
    public bool gameplayActionsEnabled = false;
    public bool kickAfterPositioningEnabled = false;
    public bool kickAfterKickingEnabled = false;
    public bool powerUpsEnabled = false;

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
        //Set QE Switching Controls
        InputManager.Controls.QESwitchGoblins.SwitchQ.performed += ctx => SwitchToQGoblin(true, ctx.startTime);
        InputManager.Controls.QESwitchGoblins.SwitchE.performed += ctx => SwitchToEGoblin(true, ctx.startTime);
        //EnableQESwitchingControls(false);
        //Kick off angle controls stuff
        InputManager.Controls.KickOff.KickoffAngleUp.performed += _ => StartAimArrowDirection(true);
        InputManager.Controls.KickOff.KickoffAngleUp.canceled += _ => EndAimArrowDirection();
        InputManager.Controls.KickOff.KickoffAngleDown.performed += _ => StartAimArrowDirection(false);
        InputManager.Controls.KickOff.KickoffAngleDown.canceled += _ => EndAimArrowDirection();
        //EnableKickoffAimArrowControls(false);
        //Gameplay controls
        InputManager.Controls.Player.Attack.performed += _ => GoblinAttack();
        InputManager.Controls.Player.Slide.performed += _ => SlideGoblin();
        InputManager.Controls.Player.Dive.performed += _ => DiveGoblin();
        InputManager.Controls.Player.Block.performed += _ => StartBlockGoblin();
        InputManager.Controls.Player.Block.canceled += _ => StopBlockGoblin();
        //EnableGameplayActions(false);
        //Kick After positioning
        InputManager.Controls.KickAfterPositioning.KickAfterPositioningLeft.performed += _ => KickAfterPositioningMove(true);
        InputManager.Controls.KickAfterPositioning.KickAfterPositioningLeft.canceled += _ => KickAfterPositioningStop();
        InputManager.Controls.KickAfterPositioning.KickAfterPositioningRight.performed += _ => KickAfterPositioningMove(false);
        InputManager.Controls.KickAfterPositioning.KickAfterPositioningRight.canceled += _ => KickAfterPositioningStop();
        InputManager.Controls.KickAfterPositioning.SubmitPosition.performed += _ => SubmitKickAfterPositionToServer();
        // Kick after kicking
        InputManager.Controls.KickAfterKicking.KickAfterSubmit.performed += _ => SubmitKickAfterKicking();
        //Kicking Controls
        InputManager.Controls.Player.KickFootball.performed += _ => StartKickPower();
        InputManager.Controls.Player.KickFootball.canceled += _ => EndKickPower();
        //Power Up Controls
        InputManager.Controls.PowerUps.PowerUp1.performed += _ => UsePowerUp(0);
        InputManager.Controls.PowerUps.PowerUp2.performed += _ => UsePowerUp(1);
        InputManager.Controls.PowerUps.PowerUp3.performed += _ => UsePowerUp(2);
        InputManager.Controls.PowerUps.PowerUp4.performed += _ => UsePowerUp(3);
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
            //Disable movement controls and stuff>?
            EnableGameplayActions(false);
            EnableGoblinMovement(false);
            EnableQESwitchingControls(false);
            EnableKickAfterPositioning(false);
            EnableKickAfterKicking(false);
            EnableKickingControls(false);
            EnablePowerUpControls(false);

            GameplayManager.instance.SetTimerText(GameplayManager.instance.timeLeftInGame);
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
            Debug.Log("CmdSpawnPlayerCharacters: Setting the speed of the berserker to " + newBerserkerScript.speed.ToString() + " based in max speed of : " + newBerserkerScript.MaxSpeed.ToString());
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
            if (hasAuthority)
                CmdSetSelectedGoblinOnServer(selectGoblin.GetComponent<NetworkIdentity>().netId);
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
    public void SwitchToQGoblin(bool fromKeyPress, double startTimeSubmitted)
    {
        Debug.Log("SwitchToQGoblin: " + canSwitchGoblin.ToString() + " from key press? " + fromKeyPress.ToString() );

        if (fromKeyPress && startTimeSubmitted == qeTime)
            return;
        else
            qeTime = startTimeSubmitted;

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
    public void SwitchToEGoblin(bool fromKeyPress, double startTimeSubmitted)
    {
        Debug.Log("SwitchToEGoblin: " + canSwitchGoblin.ToString() + " start time: " + startTimeSubmitted.ToString());

        if (fromKeyPress && startTimeSubmitted == qeTime)
            return;
        else
            qeTime = startTimeSubmitted;

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
    public void ResetCameraPositionForKickOff()
    {
        Vector3 newPosition = myCamera.transform.position;
        newPosition.y = 0f;
        myCamera.transform.position = newPosition;
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
                    InputManager.Controls.CoinTossKickReceive.Enable();
                    InputManager.Controls.CoinTossKickReceive.SelectHeads.performed += _ => SelectCoin("heads");
                    InputManager.Controls.CoinTossKickReceive.SelectTails.performed += _ => SelectCoin("tails");
                    InputManager.Controls.CoinTossKickReceive.SubmitCoin.performed += _ => SubmitCoinSelection();
                    coinTossControllsEnabled = true;
                }
                
            }            
        }
        else
        {
            InputManager.Controls.CoinTossKickReceive.Disable();
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
                InputManager.Controls.CoinTossKickReceive.Enable();
                InputManager.Controls.CoinTossKickReceive.SelectHeads.performed += _ => SelectKickOrReceive("receive");
                InputManager.Controls.CoinTossKickReceive.SelectTails.performed += _ => SelectKickOrReceive("kick");
                InputManager.Controls.CoinTossKickReceive.SubmitCoin.performed += _ => SubmitKickOrReceiveSelection();
                kickOrReceiveControlsEnabled = true;
            }
        }
        else
        {
            InputManager.Controls.CoinTossKickReceive.Disable();
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
                            CmdRequestFootballForKickOffGoblin(goblin.GetComponent<NetworkIdentity>().netId);
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
                            CmdRequestFootballForKickOffGoblin(goblin.GetComponent<NetworkIdentity>().netId);
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
    [Command]
    void CmdRequestFootballForKickOffGoblin(uint goblinNetId)
    {
        GoblinScript goblin = NetworkIdentity.spawned[goblinNetId].GetComponent<GoblinScript>();
        if (!goblin.doesCharacterHaveBall)
        {
            Football football = GameObject.FindGameObjectWithTag("football").GetComponent<Football>();
            football.MoveFootballToKickoffGoblin(goblinNetId);
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
                /*InputManager.Controls.Player.SwitchQ.Enable();
                InputManager.Controls.Player.SwitchE.Enable();
                InputManager.Controls.Player.SwitchQ.performed += ctx => SwitchToQGoblin(true, ctx.startTime);
                InputManager.Controls.Player.SwitchE.performed += ctx => SwitchToEGoblin(true, ctx.startTime);*/
                InputManager.Controls.QESwitchGoblins.Enable();
                qeSwitchingEnabled = true;
            }
            
        }
        else
        {
            Debug.Log("EnableQESwitchingControls: DISABLING the controls.");
            //InputManager.Controls.Player.SwitchQ.Disable();
            //InputManager.Controls.Player.SwitchE.Disable();
            InputManager.Controls.QESwitchGoblins.Disable();
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
                //InputManager.Controls.Player.KickFootball.performed += _ => StartKickPower();
                //InputManager.Controls.Player.KickFootball.canceled += _ => EndKickPower();
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
                /*InputManager.Controls.Player.KickoffAngleUp.Enable();
                InputManager.Controls.Player.KickoffAngleDown.Enable();
                InputManager.Controls.Player.KickoffAngleUp.performed += _ => StartAimArrowDirection(true);
                InputManager.Controls.Player.KickoffAngleUp.canceled += _ => EndAimArrowDirection();
                InputManager.Controls.Player.KickoffAngleDown.performed += _ => StartAimArrowDirection(false);
                InputManager.Controls.Player.KickoffAngleDown.canceled += _ => EndAimArrowDirection();*/
                InputManager.Controls.KickOff.Enable();
                kickoffAimArrowControlsEnabled = true;
            }
        }
        else
        {
            //InputManager.Controls.Player.KickoffAngleUp.Disable();
            //InputManager.Controls.Player.KickoffAngleDown.Disable();
            InputManager.Controls.KickOff.Disable();
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
            EnablePowerUpControls(activate);
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

                /*InputManager.Controls.Player.Attack.performed += _ => GoblinAttack();
                InputManager.Controls.Player.Slide.performed += _ => SlideGoblin();
                InputManager.Controls.Player.Dive.performed += _ => DiveGoblin();
                InputManager.Controls.Player.Block.performed += _ => StartBlockGoblin();
                InputManager.Controls.Player.Block.canceled += _ => StopBlockGoblin();*/
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
        if (hasAuthority && !areGoblinsRepositionedForKickAfter)
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
                    goblin.CheckIfGoblinNeedsToFlipForKickAfter(isKickingPlayer);
                    if (goblin.GetComponent<NetworkIdentity>().netId == scoringGoblin)
                    {
                        if (goblin.isCharacterSelected)
                            FollowSelectedGoblin(goblin.transform);
                        else if (goblin.isQGoblin)
                            SwitchToQGoblin(false, Time.time);
                        else if (goblin.isEGoblin)
                            SwitchToEGoblin(false, Time.time);

                        goblin.ActivateKickAfterAccuracyBar(isKickingPlayer);

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
                    goblin.CheckIfGoblinNeedsToFlipForKickAfter(isKickingPlayer);
                    Vector3 newPosition = goblin.transform.position;
                    if (goblin.goblinType.Contains("skirmisher"))
                    {
                        Debug.Log("RpcRepositionForKickAfter: skirmisher goblin found: " + goblin.name + " " + goblin.goblinType);
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

                        if (goblin.isCharacterSelected || selectGoblin == goblin)
                            FollowSelectedGoblin(goblin.transform);
                        else if (goblin.isQGoblin)
                            SwitchToQGoblin(false, Time.time);
                        else if (goblin.isEGoblin)
                            SwitchToEGoblin(false, Time.time);
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
            // Enable positioning controls if the player is the kicking player
            EnableKickAfterPositioning(isKickingPlayer);
            GameplayManager.instance.ActivateKickAfterPositionControlsPanel(isKickingPlayer);
            areGoblinsRepositionedForKickAfter = true;
        }
    }
    public void EnableKickAfterPositioning(bool activate)
    {
        Debug.Log("EnableKickAfterPositioning: for player " + this.PlayerName + " " + activate.ToString());
        if (activate)
        {
            if (!kickAfterPositioningEnabled)
            {
                InputManager.Controls.KickAfterPositioning.Enable();

                /*InputManager.Controls.KickAfterPositioning.KickAfterPositioningLeft.performed += _ => KickAfterPositioningMove(true);
                InputManager.Controls.KickAfterPositioning.KickAfterPositioningLeft.canceled += _ => KickAfterPositioningStop();
                InputManager.Controls.KickAfterPositioning.KickAfterPositioningRight.performed += _ => KickAfterPositioningMove(false);
                InputManager.Controls.KickAfterPositioning.KickAfterPositioningRight.canceled += _ => KickAfterPositioningStop();
                InputManager.Controls.KickAfterPositioning.SubmitPosition.performed += _ => SubmitKickAfterPositionToServer();*/

                kickAfterPositioningEnabled = true;
            }
        }
        else
        {
            InputManager.Controls.KickAfterPositioning.Disable();
            kickAfterPositioningEnabled = false;
        }
    }
    [TargetRpc]
    public void RpcDisableKickAfterPositioningControls(NetworkConnection target)
    {
        if(hasAuthority)
            EnableKickAfterPositioning(false);
    }
    void KickAfterPositioningMove(bool moveLeft)
    {
        Debug.Log("KickAfterPositioningMove: left: " + moveLeft.ToString());
        selectGoblin.KickAfterRepositioning(moveLeft);
    }
    void KickAfterPositioningStop()
    {
        Debug.Log("KickAfterPositioningStop");
        selectGoblin.EndKickAfterRepositioning();
    }
    void SubmitKickAfterPositionToServer()
    {
        selectGoblin.SubmitKickAfterPositionToServer();
    }
    [TargetRpc]
    public void RpcStartKickAfterTimer(NetworkConnection target, bool isKickingPlayer)
    {
        if (hasAuthority)
            GameplayManager.instance.ActivateKickAfterTimerUI(isKickingPlayer);
    }
    [TargetRpc]
    public void RpcKickAfterUpdateInsctructionsText(NetworkConnection target, bool isKickingPlayer)
    {
        if (hasAuthority)
            GameplayManager.instance.KickAfterUpdateInsctructionsText(isKickingPlayer);
    }
    [TargetRpc]
    public void RpcActivateKickAfterKickingControls(NetworkConnection target, bool activate)
    {
        if (hasAuthority)
        {
            Debug.Log("RpcActivateKickAfterKickingControls: for player: " + this.PlayerName + " " + activate.ToString());
            EnableKickAfterKicking(activate);
            if (activate)
                selectGoblin.StartKickAfterKickAttempt();
        }
    }
    [TargetRpc]
    public void RpcActivateKickAfterBlockingControls(NetworkConnection target, bool activate)
    {
        if (hasAuthority)
        {
            Debug.Log("RpcActivateKickAfterBlockingControls: for player: " + this.PlayerName + " " + activate.ToString());
            EnableGoblinMovement(activate);
            EnableGameplayActions(activate);
            EnableQESwitchingControls(activate);
        }
    }
    public void EnableKickAfterKicking(bool activate)
    {
        Debug.Log("EnableKickAfterKicking: for player " + this.PlayerName + " " + activate.ToString());
        if (activate)
        {
            if (!kickAfterKickingEnabled)
            {
                InputManager.Controls.KickAfterKicking.Enable();

                /*InputManager.Controls.KickAfterPositioning.KickAfterPositioningLeft.performed += _ => KickAfterPositioningMove(true);
                InputManager.Controls.KickAfterPositioning.KickAfterPositioningLeft.canceled += _ => KickAfterPositioningStop();
                InputManager.Controls.KickAfterPositioning.KickAfterPositioningRight.performed += _ => KickAfterPositioningMove(false);
                InputManager.Controls.KickAfterPositioning.KickAfterPositioningRight.canceled += _ => KickAfterPositioningStop();
                InputManager.Controls.KickAfterPositioning.SubmitPosition.performed += _ => SubmitKickAfterPositionToServer();*/

                kickAfterKickingEnabled = true;
            }
        }
        else
        {
            InputManager.Controls.KickAfterKicking.Disable();
            kickAfterKickingEnabled = false;
        }
    }
    void SubmitKickAfterKicking()
    {
        Debug.Log("SubmitKickAfterKicking");
        selectGoblin.SubmitKickAfterKicking();
    }
    [TargetRpc]
    public void RpcPowerUpPickedUp(NetworkConnection target, uint powerUpNetId)
    {
        if (hasAuthority)
        {
            Debug.Log("RpcPowerUpPickedUp: " + this.PlayerName + " to pick up power up with this network id: " + powerUpNetId.ToString());
            PowerUp powerUptoAdd = NetworkIdentity.spawned[powerUpNetId].GetComponent<PowerUp>();
            myPowerUps.Add(powerUptoAdd);
            PowerUpManager.instance.UpdatePowerUpUIImages(myPowerUps);
        }        
    }
    void UsePowerUp(int powerUpNumber)
    {
        Debug.Log("UsePowerUp: Player is going to use power up: " + powerUpNumber.ToString());
        if (powerUpNumber < myPowerUps.Count)
        {
            Debug.Log("UsePowerUp: Player is able to use power up: " + powerUpNumber.ToString());
            if (hasAuthority)
            {
                uint powerUpNetId = myPowerUps[powerUpNumber].GetComponent<NetworkIdentity>().netId;
                CmdUsePowerUp(powerUpNetId);
            }
        }
    }
    public void EnablePowerUpControls(bool activate)
    {
        Debug.Log("EnablePowerUpControls: for player " + this.PlayerName + " " + activate.ToString());
        if (activate)
        {
            if (!powerUpsEnabled)
            {
                InputManager.Controls.PowerUps.Enable();

                powerUpsEnabled = true;
            }
        }
        else
        {
            InputManager.Controls.PowerUps.Disable();
            powerUpsEnabled = false;
        }
    }
    [Command]
    void CmdUsePowerUp(uint powerUpNetId)
    {
        if (serverPowerUpUints.Contains(powerUpNetId))
        {
            Debug.Log("CmdUsePowerUp: Player: " + this.PlayerName + " can use powerup with netid of: " + powerUpNetId.ToString());
            this.serverPowerUpUints.Remove(powerUpNetId);
            PowerUpManager.instance.PlayerUsePowerUp(powerUpNetId, this.GetComponent<NetworkIdentity>().netId);
        }
        else
        {
            Debug.Log("CmdUsePowerUp: Player: " + this.PlayerName + " CANNOT use powerup with netid of: " + powerUpNetId.ToString() + " the server does not think the player owns that powerup.");
        }
    }
    [TargetRpc]
    public void RpcRemoveUsedPowerUp(NetworkConnection target, uint powerUpNetId)
    {
        if (myPowerUps.Count > 0)
        {
            /*foreach (PowerUp powerUp in myPowerUps)
            {
                if (powerUp.GetComponent<NetworkIdentity>().netId == powerUpNetId)
                {
                    myPowerUps.Remove(powerUp);
                }
            }*/
        }        
        PowerUpManager.instance.UpdatePowerUpUIImages(myPowerUps);
    }
    public void RemoveUsedPowerUps()
    { 
        if(hasAuthority)
            PowerUpManager.instance.UpdatePowerUpUIImages(myPowerUps);
    }
}
