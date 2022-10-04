using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GamePlayer : NetworkBehaviour
{
    [Header("Player Info")]
    [SyncVar] public string PlayerName;
    [SyncVar] public int ConnectionId;
    [SyncVar] public int playerNumber;
    [SyncVar] public bool IsGameLeader;
    [SyncVar] public ulong playerSteamId;
    [SerializeField] InputSystemUIInputModule inputSystemUIInputModule;
    [SerializeField] InputActionReference moveReference;
    [SerializeField] InputActionReference submitReference;
    [SerializeField] InputActionAsset defaultInputActions;
    public bool isGamePaused = false;
    public float lastBlockTime = 0f;
    float kickAfterAttemptTime = 0f;
    float yeehawGivenTime = 0f;
    float blockedKickTime = 0f;

    [Header("1v1 or 3v3 stuff")]
    [SyncVar] public bool is1v1 = false;
    [SyncVar] public bool isSinglePlayer = false;
    [SyncVar] public string goblinType;

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
    public Team myTeam;

    [Header("Team Info")]
    [SyncVar] public string teamName;
    [SyncVar] public bool doesTeamHaveBall;
    [SyncVar(hook = nameof(HandleIsTeamGrey))] public bool isTeamGrey;

    [Header("Power Ups")]
    public List<PowerUp> myPowerUps = new List<PowerUp>();
    public List<uint> serverPowerUpUints = new List<uint>();
    public int powerUpSelectedIndexNumber = 0;
    public bool canSelectWithRightStickAgain = true;
    public float nextPowerUpSelectTime = 0f;
    public bool wasRightStickUsedToSelect = false;

    [Header("Football")]
    [SerializeField] private GameObject footballPrefab;
    [SerializeField] private Vector3 footballStartingPosition; // old res position Vector3(0f, 0f, 0f);

    [SerializeField] CinemachineVirtualCamera myCamera;
    public Football football;

    [Header("Kick After")]
    public bool areGoblinsRepositionedForKickAfter = false;
    public bool localIsKickingPlayer = false;

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
    public bool menuNavigationEnabled = false;
    
    [Header("Controls From Server")]
    [SyncVar] public bool coinTossControlsOnServer = false;
    [SyncVar] public bool kickOrReceiveControlsOnServer = false;
    [SyncVar] public bool qeSwitchingControlsOnServer = false;
    [SyncVar] public bool kickingControlsOnServer = false;
    [SyncVar] public bool kickOffAimArrowControlsOnServer = false;
    [SyncVar] public bool goblinMovementControlsOnServer = false;
    [SyncVar] public bool gameplayActionControlsOnServer = false;
    [SyncVar] public bool kickAfterPositioningControlsOnServer = false;
    [SyncVar] public bool kickAfterKickingControlsOnServer = false;
    [SyncVar] public bool powerupsControlsOnServer = false;

    [Header("Possession Tracker")]
    [SyncVar(hook = nameof(HandlePossessionPoints))] public float possessionPoints = 0f;
    [SyncVar] public float gainPossessionPointsRate = 2.5f;
    public bool isGainingPossesionPointsRoutineRunning = false;
    IEnumerator GainPossessionPointsRoutine;
    public bool isLosingPossesionPointsRoutineRunning = false;
    IEnumerator LosePossessionPointsRoutine;
    public bool isNoPossessionCooldownRoutineRunning = false;
    public bool didNoPossessionCooldownRoutineComplete = false;
    IEnumerator NoPossessionCooldownRoutine;
    [SyncVar(hook = nameof(HandlePossessionBonus))] public float possessionBonus = 1.0f;

    [Header("Error Message Stuff")]
    public bool isErrorMessageDisplayed = false;
    IEnumerator displayErrorMessageFromServer;

    [Header("AIPlayer Stuff")]
    public AIPlayer myAiPlayer;

    [Header("Goblin Starting Positions")]
    [SerializeField] Vector3 GreenGrenadierStartingPosition; //(-9f, 4.45f, 0f)
    [SerializeField] Vector3 GreenBerserkerStartingPosition; //(-9f, 0f, 0f)
    [SerializeField] Vector3 GreenSkirmisherStartingPosition; //(-9f, -4.45f, 0f)
    [SerializeField] Vector3 GreyGrenadierStartingPosition; //(9f, 4.45f, 0f)
    [SerializeField] Vector3 GreyBerserkerStartingPosition; //(9f, 0f, 0f)
    [SerializeField] Vector3 GreySkirmisherStartingPosition; //(9f, -4.45f, 0f

    [Header("Goblin Kicking Positions")]
    [SerializeField] Vector3 KickingPositionGreenGrenadier; //(0f, 1f, 0f)
    [SerializeField] Vector3 KickingPositionGreenBerserker; //(-1f, -3f, 0f)
    [SerializeField] Vector3 KickingPositionGreenSkirmisher; //(-1f, 4f, 0f)
    [SerializeField] Vector3 KickingPositionGreyGrenadier; //(0f, 1f, 0f)
    [SerializeField] Vector3 KickingPositionGreyBerserker; //(1f, -3f, 0f)
    [SerializeField] Vector3 KickingPositionGreySkirmisher; //(1f, 4f, 0f)

    [Header("Goblin Receiving Positions")]
    [SerializeField] Vector3 ReceivingPositionGreenGrenadier; // (-11f, 3f, 0f)
    [SerializeField] Vector3 ReceivingPositionGreenBerserker; // (-11f, -2f, 0f)
    [SerializeField] Vector3 ReceivingPositionGreenSkirmisher; // (-20f, 0f, 0f)
    [SerializeField] Vector3 ReceivingPositionGreyGrenadier; // (11f, 3f, 0f)
    [SerializeField] Vector3 ReceivingPositionGreyBerserker; // (11f, -2f, 0f)
    [SerializeField] Vector3 ReceivingPositionGreySkirmisher; // (20f, 0f, 0f)

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
        if (!this.isSinglePlayer)
        {
            gameObject.name = "LocalGamePlayer";
            gameObject.tag = "LocalGamePlayer";
        }
        else
        {
            if (this.name.Contains("AI"))
                return;
            else
            {
                gameObject.name = "LocalGamePlayer";
                gameObject.tag = "LocalGamePlayer";
            }
        }


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
        InputManager.Controls.SelectPowerUps.RightAnalogStickDirection.performed += ctx => PowerUpAnalogReader(ctx.ReadValue<Vector2>());
        InputManager.Controls.SelectPowerUps.RightAnalogStickDirection.canceled += ctx => CancelRightAnalogStickDirection();
        // Power Up selection with right analog stick - Gamepads only?
        //InputManager.Controls.SelectPowerUps.SelectLeft.performed += _ => GamepadPowerUpSelectLeftRight(true);
        //InputManager.Controls.SelectPowerUps.SelectRight.performed += _ => GamepadPowerUpSelectLeftRight(false);
        //InputManager.Controls.SelectPowerUps.SelectLeftOrRight.performed += ctx => GamepadPowerUpSelectWithRightStick(ctx.ReadValue<Vector2>());
        //InputManager.Controls.SelectPowerUps.SelectLeftOrRight.canceled += ctx => GamepadResetSelectWithRightStick();
        //InputManager.Controls.SelectPowerUps.SelectLeftOrRightComposite.performed += ctx => GamepadPowerUpSelectWithRightStickComposite(ctx.ReadValue<Vector2>());
        //InputManager.Controls.SelectPowerUps.SubmitSelection.performed += _ => GamepadPowerUpSubmitSelection();
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
            // Initialize camera's boundary thing?
            //myCamera.GetComponent<CinemachineConfiner>().InvalidatePathCache();
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
            EnableMenuNavigationControls(false);

            GameplayManager.instance.SetTimerText(GameplayManager.instance.timeLeftInGame);
            if (GameplayManager.instance.is1v1 && !GameplayManager.instance.isSinglePlayer)
            {
                SteamAchievementManager.instance.Play1v1();
            }
            else if (!GameplayManager.instance.is1v1 && !GameplayManager.instance.isSinglePlayer)
            {
                SteamAchievementManager.instance.Play3v3();
            }
        }
    }
    public void InitializeAIPlayer()
    {
        CmdGetTeamName();
        CmdDoesPlayerChooseCoin();
        CmdSpawnPlayerCharacters();
        football = GameObject.FindGameObjectWithTag("football").GetComponent<Football>();
        if (football)
            football.localPlayer = this;
        CmdPlayerFinishedLoading();
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
            newFootball.transform.position = footballStartingPosition;
            NetworkServer.Spawn(newFootball);
        }
    }
    [Command]
    void CmdGetTeamName()
    {
        Debug.Log("CmdGetTeamName for GamePlayer with ConnectionId: " + ConnectionId.ToString());
        if (is1v1) // 1v1 games is the host is always green. 3v3 players pick in lobby so it doesn't need to be set here
        {
            if (IsGameLeader)
            {
                teamName = "Green";
                isTeamGrey = false;
            }
            else
            {
                teamName = "Grey";
                isTeamGrey = true;
            }
        }
        foreach (Team team in TeamManager.instance.teams)
        {
            if (team.isGrey == this.isTeamGrey)
            {
                team.AddPlayerToTeam(this);
                if (!this.is1v1)
                {
                    if (this.goblinType == "Grenadier")
                        team.captain = this;
                    else if (this.isSinglePlayer)
                        team.captain = this;
                    if (this.isTeamGrey)
                        teamName = "Grey";
                    else
                        teamName = "Green";
                }
                else
                {
                    team.captain = this;
                }
            }   
        }   
    }
    [Command]
    void CmdSpawnPlayerCharacters()
    {
        Debug.Log("CmdSpawnPlayerCharacters for GamePlayer with ConnectionId: " + ConnectionId.ToString());
        if (!areCharactersSpawnedYet)
        {
            if (is1v1) // for 1v1 games, spawn all three goblins for each player
            {
                Debug.Log("Executing SpawnPlayerCharacters on the server for player " + this.PlayerName + this.ConnectionId.ToString());
                //Team teamOnServer;
                GameObject newGrenadier = Instantiate(grenadierPrefab);
                if (IsGameLeader)
                {
                    newGrenadier.transform.position = GreenGrenadierStartingPosition;
                    //teamOnServer = TeamManager.instance.greenTeam;
                }
                else
                {
                    newGrenadier.transform.position = GreyGrenadierStartingPosition;
                    //newGrenadier.transform.localScale = new Vector3(-1f,1f,1f);
                    //teamOnServer = TeamManager.instance.greyTeam;
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
                newGrenadierScript.defenseModifier = 1.0f;
                newGrenadierScript.goblinType = "grenadier";
                if (!IsGameLeader)
                    newGrenadierScript.goblinType += "-grey";
                newGrenadierScript.serverGamePlayer = this;
                goblinTeamOnServer.Add(newGrenadierScript);
                //teamOnServer.add


                GameObject newBerserker = Instantiate(berserkerPrefab, transform.position, Quaternion.identity);
                if (IsGameLeader)
                    newBerserker.transform.position = GreenBerserkerStartingPosition;
                else
                {
                    newBerserker.transform.position = GreyBerserkerStartingPosition;
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
                newBerserkerScript.defenseModifier = 1.0f;
                newBerserkerScript.goblinType = "berserker";
                if (!IsGameLeader)
                    newBerserkerScript.goblinType += "-grey";
                newBerserkerScript.serverGamePlayer = this;
                goblinTeamOnServer.Add(newBerserkerScript);

                GameObject newSkirmisher = Instantiate(skrimisherPrefab, transform.position, Quaternion.identity);
                if (IsGameLeader)
                    newSkirmisher.transform.position = GreenSkirmisherStartingPosition;
                else
                {
                    newSkirmisher.transform.position = GreySkirmisherStartingPosition;
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
                newSkirmisherScript.defenseModifier = 1.0f;
                newSkirmisherScript.goblinType = "skirmisher";
                if (!IsGameLeader)
                    newSkirmisherScript.goblinType += "-grey";
                goblinTeamOnServer.Add(newSkirmisherScript);

                newSkirmisherScript.serverGamePlayer = this;

                areCharactersSpawnedYet = true;
            }
            else if (this.isSinglePlayer)
            {
                Debug.Log("Executing SpawnPlayerCharacters on the server for player " + this.PlayerName + this.ConnectionId.ToString() + " in a SINGLEPLAYER game");
                GameObject newGrenadier = Instantiate(grenadierPrefab);
                if (!this.isTeamGrey)
                {
                    newGrenadier.transform.position = GreenGrenadierStartingPosition;
                    //teamOnServer = TeamManager.instance.greenTeam;
                }
                else
                {
                    newGrenadier.transform.position = GreyGrenadierStartingPosition;
                    //newGrenadier.transform.localScale = new Vector3(-1f,1f,1f);
                    //teamOnServer = TeamManager.instance.greyTeam;
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
                newGrenadierScript.defenseModifier = 1.0f;
                newGrenadierScript.goblinType = "grenadier";
                if (this.isTeamGrey)
                    newGrenadierScript.goblinType += "-grey";
                newGrenadierScript.serverGamePlayer = this;
                goblinTeamOnServer.Add(newGrenadierScript);
                //teamOnServer.add


                GameObject newBerserker = Instantiate(berserkerPrefab, transform.position, Quaternion.identity);
                if (!this.isTeamGrey)
                    newBerserker.transform.position = GreenBerserkerStartingPosition;
                else
                {
                    newBerserker.transform.position = GreyBerserkerStartingPosition;
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
                newBerserkerScript.defenseModifier = 1.0f;
                newBerserkerScript.goblinType = "berserker";
                if (this.isTeamGrey)
                    newBerserkerScript.goblinType += "-grey";
                newBerserkerScript.serverGamePlayer = this;
                goblinTeamOnServer.Add(newBerserkerScript);

                GameObject newSkirmisher = Instantiate(skrimisherPrefab, transform.position, Quaternion.identity);
                if (!this.isTeamGrey)
                    newSkirmisher.transform.position = GreenSkirmisherStartingPosition;
                else
                {
                    newSkirmisher.transform.position = GreySkirmisherStartingPosition;
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
                newSkirmisherScript.defenseModifier = 1.0f;
                newSkirmisherScript.goblinType = "skirmisher";
                if (this.isTeamGrey)
                    newSkirmisherScript.goblinType += "-grey";
                goblinTeamOnServer.Add(newSkirmisherScript);

                newSkirmisherScript.serverGamePlayer = this;

                areCharactersSpawnedYet = true;
            }
            else
            {
                Debug.Log("CmdSpawnPlayerCharacters: Spawning 3v3 character for player: " + this.PlayerName + " for team grey? " + this.isTeamGrey.ToString() + " and their character will be: " + this.goblinType);
                if (this.goblinType == "Grenadier")
                {
                    //Debug.Log("Executing SpawnPlayerCharacters on the server for player " + this.PlayerName + this.ConnectionId.ToString());
                    GameObject newGrenadier = Instantiate(grenadierPrefab);
                    if (!this.isTeamGrey)
                        newGrenadier.transform.position = GreenGrenadierStartingPosition;
                    else
                    {
                        newGrenadier.transform.position = GreyGrenadierStartingPosition;
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
                    newGrenadierScript.defenseModifier = 1.0f;
                    newGrenadierScript.goblinType = "grenadier";
                    if (this.isTeamGrey)
                        newGrenadierScript.goblinType += "-grey";
                    newGrenadierScript.serverGamePlayer = this;
                    goblinTeamOnServer.Add(newGrenadierScript);
                }
                else if (this.goblinType == "Berserker")
                {
                    GameObject newBerserker = Instantiate(berserkerPrefab, transform.position, Quaternion.identity);
                    if (!this.isTeamGrey)
                        newBerserker.transform.position = GreenBerserkerStartingPosition;
                    else
                    {
                        newBerserker.transform.position = GreyBerserkerStartingPosition;
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
                    //Debug.Log("CmdSpawnPlayerCharacters: Setting the speed of the berserker to " + newBerserkerScript.speed.ToString() + " based in max speed of : " + newBerserkerScript.MaxSpeed.ToString());
                    newBerserkerScript.damage = newBerserkerScript.MaxDamage;
                    newBerserkerScript.defenseModifier = 1.0f;
                    newBerserkerScript.goblinType = "berserker";
                    if (this.isTeamGrey)
                        newBerserkerScript.goblinType += "-grey";
                    newBerserkerScript.serverGamePlayer = this;
                    goblinTeamOnServer.Add(newBerserkerScript);
                }
                else if (this.goblinType == "Skirmisher")
                {
                    GameObject newSkirmisher = Instantiate(skrimisherPrefab, transform.position, Quaternion.identity);
                    if (!this.isTeamGrey)
                        newSkirmisher.transform.position = GreenSkirmisherStartingPosition;
                    else
                    {
                        newSkirmisher.transform.position = GreySkirmisherStartingPosition;
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
                    newSkirmisherScript.defenseModifier = 1.0f;
                    newSkirmisherScript.goblinType = "skirmisher";
                    if (this.isTeamGrey)
                        newSkirmisherScript.goblinType += "-grey";
                    goblinTeamOnServer.Add(newSkirmisherScript);

                    newSkirmisherScript.serverGamePlayer = this;
                }
            }
            areCharactersSpawnedYet = true;
        }
    }
    public void AddToGoblinTeam(GoblinScript GoblinToAdd)
    {
        Debug.Log("AddToGoblinTeam: for player: " + this.PlayerName + " is their team grey? " + this.isTeamGrey.ToString());
        if (!goblinTeam.Contains(GoblinToAdd))
            goblinTeam.Add(GoblinToAdd);
        if (this.is1v1 || this.isSinglePlayer)
        {
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

            if (this.isTeamGrey)
                myTeam = TeamManager.instance.greyTeam;
            else
                myTeam = TeamManager.instance.greenTeam;
            CmdAddToGoblinTeamOnTeamObject(GoblinToAdd.GetComponent<NetworkIdentity>().netId);
        }
        else
        {
            Debug.Log("AddToGoblinTeam: for player " + this.PlayerName + " goblin name is: " + GoblinToAdd.gameObject.name.ToLower() + " and goblin type is: " + this.goblinType.ToLower());
            if (GoblinToAdd.gameObject.name.ToLower().Contains(this.goblinType.ToLower()))
            {
                GoblinToAdd.SelectThisCharacter();
                selectGoblin = GoblinToAdd;
                FollowSelectedGoblin(selectGoblin.transform);
                if (hasAuthority)
                {
                    uint goblinNetId = selectGoblin.GetComponent<NetworkIdentity>().netId;
                    CmdSetSelectedGoblinOnServer(goblinNetId);
                    CmdAddToGoblinTeamOnTeamObject(goblinNetId);
                }
                    
            }
        }
    }
    [Command]
    void CmdAddToGoblinTeamOnTeamObject(uint goblinNetId)
    {
        GoblinScript goblinScript = NetworkIdentity.spawned[goblinNetId].gameObject.GetComponent<GoblinScript>();
        if (this.isTeamGrey)
        {
            if (!TeamManager.instance.greyTeam.goblins.Contains(goblinScript))
            {
                TeamManager.instance.greyTeam.goblins.Add(goblinScript);
                TeamManager.instance.greyTeam.goblinNetIds.Add(goblinNetId);
            }
        }
        else
        {
            if (!TeamManager.instance.greenTeam.goblins.Contains(goblinScript))
            {
                TeamManager.instance.greenTeam.goblins.Add(goblinScript);
                TeamManager.instance.greenTeam.goblinNetIds.Add(goblinNetId);
            }
        }
    }
    // For 3v3 games, get all goblins on player's team. Then set the ones they don't control to their Q/E goblins?
    public void GetGoblinTeammatesFor3v3()
    {
        Debug.Log("GetGoblinTeammatesFor3v3: for player: " + this.PlayerName + " are they grey team? " + this.isTeamGrey.ToString());
        if (TeamManager.instance.greyTeam == null || TeamManager.instance.greenTeam)
            TeamManager.instance.GetLocalTeamObjects();
        if (this.isTeamGrey)
            myTeam = TeamManager.instance.greyTeam;
        else
            myTeam = TeamManager.instance.greenTeam;
        //if (myTeam)
        //{
        //}
        try
        {
            Debug.Log("GetGoblinTeammatesFor3v3: for player: " + this.PlayerName + " size of my team " + myTeam.goblinNetIds.Count.ToString());
            foreach (uint goblinNetId in myTeam.goblinNetIds)
            {
                GoblinScript goblinScript = NetworkIdentity.spawned[goblinNetId].gameObject.GetComponent<GoblinScript>();
                if (goblinScript != selectGoblin)
                {
                    if (qGoblin == null)
                    {
                        qGoblin = goblinScript;
                        qGoblin.SetQGoblinLocally3v3(true);
                        Debug.Log("GetGoblinTeammatesFor3v3: for player: " + this.PlayerName + " setting qGoblin to: " + qGoblin.name);
                    }
                    else if (eGoblin == null)
                    {
                        eGoblin = goblinScript;
                        eGoblin.SetEGoblinLocally3v3(true);
                        Debug.Log("GetGoblinTeammatesFor3v3: for player: " + this.PlayerName + " setting eGoblin to: " + eGoblin.name);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("GetGoblinTeammatesFor3v3: for player: " + this.PlayerName + " error: " + e);
        }
        CmdGetGoblinTeammatesFor3v3OnServer();
    }
    [Command]
    void CmdGetGoblinTeammatesFor3v3OnServer()
    {
        Team myTeamForServer;
        if (this.isTeamGrey)
            myTeamForServer = TeamManager.instance.greyTeam;
        else
            myTeamForServer = TeamManager.instance.greenTeam;

        if (myTeamForServer)
        {
            foreach (uint goblinTeammate in myTeamForServer.goblinNetIds)
            {
                if (!this.goblinTeamNetIds.Contains(goblinTeammate))
                {
                    goblinTeamNetIds.Add(goblinTeammate);
                }
            }
        }
    }
    public void SwitchToQGoblin(bool fromKeyPress, double startTimeSubmitted)
    {
        Debug.Log("SwitchToQGoblin: " + canSwitchGoblin.ToString() + " from key press? " + fromKeyPress.ToString() );

        if (fromKeyPress && startTimeSubmitted == qeTime)
            return;
        else
            qeTime = startTimeSubmitted;
        if (this.is1v1 || this.isSinglePlayer)
        {
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
                    //FollowSelectedGoblin(GameObject.FindGameObjectWithTag("football").transform);
                }
                else
                {
                    if(!this.isSinglePlayer)
                        FollowSelectedGoblin(selectGoblin.transform);
                    else if (this.isSinglePlayer && this.isLocalPlayer)
                        FollowSelectedGoblin(selectGoblin.transform);
                }
                Debug.Log("SwitchToQGoblin switching to goblin: " + selectGoblin.name);
                CmdSetSelectedGoblinOnServer(selectGoblin.GetComponent<NetworkIdentity>().netId);
            }
        }
        else
        {
            if (!selectGoblin.isKicking && !selectGoblin.isDiving && !selectGoblin.isPunching && !selectGoblin.isGoblinKnockedOut && selectGoblin.doesCharacterHaveBall)
            {
                if (doesTeamHaveBall && !qGoblin.canGoblinReceivePass)
                    return;
                selectGoblin.ThrowBall(qGoblin);
                IEnumerator stopSwitch = PreventGoblinSwitch();
                StartCoroutine(stopSwitch);
            }
        }
        
        
    }
    public void SwitchToEGoblin(bool fromKeyPress, double startTimeSubmitted)
    {
        Debug.Log("SwitchToEGoblin: " + canSwitchGoblin.ToString() + " start time: " + startTimeSubmitted.ToString());

        if (fromKeyPress && startTimeSubmitted == qeTime)
            return;
        else
            qeTime = startTimeSubmitted;
        if (this.is1v1 || this.isSinglePlayer)
        {
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
                    Debug.Log("SwitchToEGoblin: Throwing: set the football to follow");
                    //FollowSelectedGoblin(GameObject.FindGameObjectWithTag("football").transform);
                }
                else
                {
                    Debug.Log("SwitchToEGoblin: Throwing: set the GOBLIN to follow");
                    if (!this.isSinglePlayer)
                        FollowSelectedGoblin(selectGoblin.transform);
                    else if (this.isSinglePlayer && this.isLocalPlayer)
                        FollowSelectedGoblin(selectGoblin.transform);
                }
                Debug.Log("SwitchToEGoblin switching to goblin: " + selectGoblin.name);
                CmdSetSelectedGoblinOnServer(selectGoblin.GetComponent<NetworkIdentity>().netId);

            }
        }
        else
        {
            if (!selectGoblin.isKicking && !selectGoblin.isDiving && !selectGoblin.isPunching && !selectGoblin.isGoblinKnockedOut && selectGoblin.doesCharacterHaveBall)
            {
                if (doesTeamHaveBall && !eGoblin.canGoblinReceivePass)
                    return;
                selectGoblin.ThrowBall(eGoblin);
                IEnumerator stopSwitch = PreventGoblinSwitch();
                StartCoroutine(stopSwitch);
            }
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
            if (Time.time < (lastBlockTime + 0.33f))
                return;
            else
                lastBlockTime = Time.time;
            if (!selectGoblin.isSliding && !selectGoblin.isDiving && !selectGoblin.isGoblinKnockedOut && !selectGoblin.isPunching)
            {
                selectGoblin.StartBlocking();
                if (hasAuthority)
                {
                    try
                    {
                        GameObject cowboy = GameObject.FindGameObjectWithTag("cowboy");
                        if (cowboy != null)
                        {
                            if (cowboy.GetComponent<CowboyScript>().isVisibileToClient)
                                CmdTryToGiveCowboyYeehaw();
                            else
                                Debug.Log("StartBlockGoblin: Cowboy is not visible on the client");
                        }
                        
                    }
                    catch (Exception e)
                    {
                        Debug.Log("StartBlockGoblin: Error trying to find the cowboy object. Error: " + e);
                    }
                    
                }
                    
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
        // For single player mode, make sure that the AI gameplayer object doesn't try to modify what the camera is following or whatever
        if (this.isSinglePlayer && !this.isLocalPlayer)
            return;
        if (GameplayManager.instance.gamePhase != "cointoss")
        {
            if (GameplayManager.instance.gamePhase == "kickoff")
            {
                myCamera.PreviousStateIsValid = false;
            }
            myCamera.Follow = goblinToFollow.transform;
        }
            
    }
    public void ResetCameraPositionForKickOff()
    {
        Debug.Log("ResetCameraPositionForKickOff");
        /*Vector3 newPosition = myCamera.transform.position;
        if (myCamera.Follow)
            myCamera.Follow = null;
        newPosition.x = 0;
        newPosition.y = 0f;
        myCamera.transform.position = newPosition;*/
        if (this.isSinglePlayer && !this.isLocalPlayer)
            return;
        float xDamping = myCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping;
        myCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = 0f;
        myCamera.Follow = GameObject.Find("CameraFollowObject").transform;
        myCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = xDamping;
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
        Debug.Log("RpcDoneLoadingGameplayStuff: for player: " + this.PlayerName + " and is 1v1? " + this.is1v1);
        if (hasAuthority)
        {
            Game.DestroyWaitingForPlayersCanvas();
            GameplayManager.instance.ActivateCoinTossUI(true);
            /*if (!this.is1v1) // for a 3v3 game, tell clients to find all goblins on their team and make ones that aren't their own goblin be with the Q/E goblin
            {
                Debug.Log("RpcDoneLoadingGameplayStuff: for player: " + this.PlayerName);
                GetGoblinTeammatesFor3v3();
            }*/
        }   
    }
    public void EnableGoblinMovement(bool enableOrDisable)
    {
        if (hasAuthority)
        {
            if (enableOrDisable)
            {
                try
                {
                    if (EscMenuManager.instance.isEscMenuOpen)
                        return;
                }
                catch (Exception e)
                {
                    Debug.Log("Could not access EscMenuManager instance. Error: " + e);
                }
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
    public void EnableGoblinMovementControlsServerValues(bool activate)
    {
        if (hasAuthority)
            CmdEnableGoblinMovementControlsServerValues(activate);
    }
    [Command]
    void CmdEnableGoblinMovementControlsServerValues(bool activate)
    {
        this.goblinMovementControlsOnServer = activate;
    }
    [Command]
    void CmdDoesPlayerChooseCoin()
    {
        if (this.is1v1)
        {
            if (this.playerNumber == 2)
                HandleDoesPlayerChooseCoin(doesPlayerChooseCoin, true);
            else
                HandleDoesPlayerChooseCoin(doesPlayerChooseCoin, false);
        }
        else if (this.isSinglePlayer)
        {
            if (this.isTeamGrey)
            {
                HandleDoesPlayerChooseCoin(doesPlayerChooseCoin, true);
            }
            else
            {
                HandleDoesPlayerChooseCoin(doesPlayerChooseCoin, false);
            }
        }
        else
        {
            if (this.isTeamGrey && this.goblinType == "Grenadier")
            {
                HandleDoesPlayerChooseCoin(doesPlayerChooseCoin, true);
            }
            else
                HandleDoesPlayerChooseCoin(doesPlayerChooseCoin, false);
        }
        
    }
    public void HandleDoesPlayerChooseCoin(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            doesPlayerChooseCoin = newValue;
            coinTossControlsOnServer = newValue;
            /*if (newValue)
            {
                CoinTossManager.instance.StartCoinTossTimeOutTimer();
            }*/
            CoinTossManager.instance.StartCoinTossTimeOutTimer();
        }   
        if (isClient)
        {
            if (hasAuthority)
            {
                Debug.Log("HandleDoesPlayerChooseCoin: " + newValue.ToString() + " for player: " + this.name);
                if (this.isSinglePlayer && !this.isLocalPlayer)
                {
                    if(newValue)
                        GameplayManager.instance.AIChoosesCoinToss();
                    return;
                }
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
            try
            {
                if (EscMenuManager.instance.isEscMenuOpen)
                    return;
            }
            catch (Exception e)
            {
                Debug.Log("Could not access EscMenuManager instance. Error: " + e);
            }
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
        Debug.Log("KickOrReceiveControls: " + activate.ToString());
        if (activate)
        {
            try
            {
                if (EscMenuManager.instance.isEscMenuOpen)
                    return;
            }
            catch (Exception e)
            {
                Debug.Log("Could not access EscMenuManager instance. Error: " + e);
            }
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
    public void DisableKickOrReceiveControls()
    {
        if (hasAuthority)
            CmdDisableKickOrReceiveControls();
    }
    [Command]
    void CmdDisableKickOrReceiveControls()
    {
        this.kickOrReceiveControlsOnServer = false;
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
    public void SubmitCoinSelection()
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
            if (!String.IsNullOrWhiteSpace(headsOrTailsPlayer))
                CoinTossManager.instance.playerSelectedCoin = headsOrTailsPlayer;
            else
                CoinTossManager.instance.playerSelectedCoin = "heads";
            CoinTossManager.instance.ServerPlayerSelectedTheirCoin();
            CoinTossManager.instance.FlipCoin();
            HandleDidPlayerChooseCoinYet(didPlayerChooseCoinYet, true);
            CoinTossManager.instance.StopTimeoutCointossRoutine();
        }
        
    }
    [TargetRpc]
    public void RpcForceSelectCoin(NetworkConnection target)
    {
        if (GameplayManager.instance.isSinglePlayer)
            return;
        if (hasAuthority)
        {
            if (String.IsNullOrWhiteSpace(headsOrTailsPlayer))
            {
                CmdSelectCoin("heads");
            }
            CmdSubmitCoinSelection();
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
        {
            doesPlayerChooseKickOrReceive = newValue;
            if (newValue)
            {
                CoinTossManager.instance.StartCoinTossTimeOutTimer();
            }
        }   
        if (isClient)
        {
            if (hasAuthority)
            {
                //if(newValue)
                // CoinTossControlls(!newValue);
                //KickOrReceiveControls(newValue);
                Debug.Log("HandleDoesPlayerChooseKickOrReceive: " + newValue.ToString() + " for player: " + this.name);
                if (this.isSinglePlayer && !this.isLocalPlayer)
                {
                    /*if (newValue)
                        GameplayManager.instance.AIChoosesKickOrReceive();*/
                    return;
                }
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
    public void SubmitKickOrReceiveSelection()
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
            if (!String.IsNullOrWhiteSpace(kickOrReceivePlayer))
                CoinTossManager.instance.playerKickOrReceive = kickOrReceivePlayer;
            else
                CoinTossManager.instance.playerKickOrReceive = "kick";
            CoinTossManager.instance.ServerPlayerSelectedKickOrReceive();
            HandleDidPlayerChooseKickOrReceiveYet(didPlayerChooseKickOrReceiveYet, true);
            CoinTossManager.instance.StopTimeoutCointossRoutine();
        }
    }
    [TargetRpc]
    public void RpcForceSelectKickOrReceive(NetworkConnection target)
    {
        if (GameplayManager.instance.isSinglePlayer)
            return;
        if (hasAuthority)
        {
            if (String.IsNullOrWhiteSpace(kickOrReceivePlayer))
            {
                CmdSelectKickOrReceive("receive");
            }
            CmdSubmitKickOrReceiveSelection();
        }
    }
    [ClientRpc]
    public void RpcCoinTossAndKickOrReceiveControllerActivation(bool activateCoinTossControlls, bool activateKickOrReceiveControlls)
    {
        if (hasAuthority)
        {
            Debug.Log("RpcCoinTossAndKickOrReceiveControllerActivation: " + activateCoinTossControlls.ToString() + " " + activateKickOrReceiveControlls.ToString() + " for player: " + this.name);
            if (this.isSinglePlayer && !this.isLocalPlayer)
            {
                if (activateKickOrReceiveControlls)
                {
                    GameplayManager.instance.AIChoosesKickOrReceive();
                }
                return;
            }
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
            EnableGameplayActions(false);
            EnableGoblinMovement(false);
            EnablePowerUpControls(false);

            if (this.is1v1 || (this.isSinglePlayer && this.isLocalPlayer))
            {
                EnableKickingControls(isKicking);
                EnableKickoffAimArrowControls(isKicking);
                EnableQESwitchingControls(!isKicking);
                
            }
            if (isKicking)
            {
                if (teamName == "Grey")
                {
                    foreach (GoblinScript goblin in goblinTeam)
                    {
                        if (goblin.goblinType.Contains("grenadier"))
                        {
                            goblin.transform.position = KickingPositionGreyGrenadier;
                            //goblin.ToggleGoblinBodyCollider();
                            goblin.EnableKickoffAimArrow(true);
                            CmdRequestFootballForKickOffGoblin(goblin.GetComponent<NetworkIdentity>().netId);
                        }
                        else if (goblin.goblinType.Contains("berserker"))
                        {
                            goblin.transform.position = KickingPositionGreyBerserker;
                        }
                        else
                        {
                            goblin.transform.position = KickingPositionGreySkirmisher;
                        }
                        // Make sure grey goblins are facing to the left
                        goblin.FlipRenderer(true);
                    }
                }
                else
                {
                    foreach (GoblinScript goblin in goblinTeam)
                    {
                        if (goblin.goblinType.Contains("grenadier"))
                        {
                            goblin.transform.position = KickingPositionGreenGrenadier;
                            goblin.EnableKickoffAimArrow(true);
                            CmdRequestFootballForKickOffGoblin(goblin.GetComponent<NetworkIdentity>().netId);
                        }
                        else if (goblin.goblinType.Contains("berserker"))
                        {
                            goblin.transform.position = KickingPositionGreenBerserker;
                        }
                        else
                        {
                            goblin.transform.position = KickingPositionGreenSkirmisher;
                        }
                        // Make sure green goblins are facing to the right
                        goblin.FlipRenderer(false);
                    }
                }
                // Start process for AI to kick ball if they are the kicking team
                if (this.isSinglePlayer && !this.isLocalPlayer)
                {
                    GameplayManager.instance.AIPlayerKickOff();
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
                            goblin.transform.position = ReceivingPositionGreyGrenadier;
                            //goblin.GetComponent<Rigidbody2D>().MovePosition(new Vector2(11f, 3f));
                        }
                        else if (goblin.goblinType.Contains("skirmisher"))
                        {
                            goblin.transform.position = ReceivingPositionGreySkirmisher;
                            //goblin.GetComponent<Rigidbody2D>().MovePosition(new Vector2(20f, 0f));
                        }
                        else
                        {
                            goblin.transform.position = ReceivingPositionGreyBerserker;
                            //goblin.GetComponent<Rigidbody2D>().MovePosition(new Vector2(11f, -2f));
                        }
                        // Make sure grey goblins are facing to the left
                        goblin.FlipRenderer(true);
                    }
                }
                else
                {
                    foreach (GoblinScript goblin in goblinTeam)
                    {
                        if (goblin.goblinType.Contains("grenadier"))
                        {
                            goblin.transform.position = ReceivingPositionGreenGrenadier;
                            //goblin.GetComponent<Rigidbody2D>().MovePosition(new Vector2(-11f, 3f));
                        }
                        else if (goblin.goblinType.Contains("skirmisher"))
                        {
                            goblin.transform.position = ReceivingPositionGreenSkirmisher;
                            //goblin.GetComponent<Rigidbody2D>().MovePosition(new Vector2(-20f, 0f));
                        }
                        else
                        {
                            goblin.transform.position = ReceivingPositionGreenBerserker;
                            //goblin.GetComponent<Rigidbody2D>().MovePosition(new Vector2(-11f, -2f));
                        }
                        // Make sure green goblins are facing to the right
                        goblin.FlipRenderer(false);
                    }
                }
            }
        }
        if (this.isLocalPlayer)
        {
            this.FollowSelectedGoblin(this.selectGoblin.transform);
        }
        
    }
    [TargetRpc]
    public void RpcRepositionTeamForKickOff3v3(NetworkConnection target, bool isTeamKicking, bool isKickingPlayer)
    {
        Debug.Log("RpcRepositionTeamForKickOff3v3: for player: " + this.PlayerName + " is their team kicking? " + isTeamKicking.ToString() + " are they the kicking player? " + isKickingPlayer.ToString());
        if (hasAuthority)
        {
            EnableKickingControls(isKickingPlayer);
            EnableKickoffAimArrowControls(isKickingPlayer);
            EnableQESwitchingControls(!isTeamKicking);
            EnableGameplayActions(false);
            EnableGoblinMovement(false);
            EnablePowerUpControls(false);
            if (isTeamKicking)
            {
                if (this.goblinType == "Grenadier")
                {
                    if (this.isTeamGrey)
                    {
                        selectGoblin.transform.position = KickingPositionGreyGrenadier;
                        selectGoblin.FlipRenderer(true);
                    }
                    else
                    {
                        selectGoblin.transform.position = KickingPositionGreenGrenadier;
                        selectGoblin.FlipRenderer(false);
                    }
                    if (isKickingPlayer)
                    {
                        selectGoblin.EnableKickoffAimArrow(true);
                        CmdRequestFootballForKickOffGoblin(selectGoblin.GetComponent<NetworkIdentity>().netId);
                    }
                }
                else if (this.goblinType == "Berserker")
                {
                    if (this.isTeamGrey)
                    {
                        selectGoblin.transform.position = KickingPositionGreyBerserker;
                        selectGoblin.FlipRenderer(true);
                    }
                    else
                    {
                        selectGoblin.transform.position = KickingPositionGreenBerserker;
                        selectGoblin.FlipRenderer(false);
                    }
                }
                if (this.goblinType == "Skirmisher")
                {
                    if (this.isTeamGrey)
                    {
                        selectGoblin.transform.position = KickingPositionGreySkirmisher;
                        selectGoblin.FlipRenderer(true);
                    }
                    else
                    {
                        selectGoblin.transform.position = KickingPositionGreenSkirmisher;
                        selectGoblin.FlipRenderer(false);
                    }
                }
            }
            else
            {
                if (this.goblinType == "Grenadier")
                {
                    if (this.isTeamGrey)
                    {
                        selectGoblin.transform.position = ReceivingPositionGreyGrenadier;
                        selectGoblin.FlipRenderer(true);
                    }
                    else
                    {
                        selectGoblin.transform.position = ReceivingPositionGreenGrenadier;
                        selectGoblin.FlipRenderer(false);
                    }
                }
                else if (this.goblinType == "Berserker")
                {
                    if (this.isTeamGrey)
                    {
                        selectGoblin.transform.position = ReceivingPositionGreyBerserker;
                        selectGoblin.FlipRenderer(true);
                    }
                    else
                    {
                        selectGoblin.transform.position = ReceivingPositionGreenBerserker;
                        selectGoblin.FlipRenderer(false);
                    }
                }
                if (this.goblinType == "Skirmisher")
                {
                    if (this.isTeamGrey)
                    {
                        selectGoblin.transform.position = ReceivingPositionGreySkirmisher;
                        selectGoblin.FlipRenderer(true);
                    }
                    else
                    {
                        selectGoblin.transform.position = ReceivingPositionGreenSkirmisher;
                        selectGoblin.FlipRenderer(false);
                    }
                }
            }
        }
        if (this.isLocalPlayer)
        {
            this.FollowSelectedGoblin(this.selectGoblin.transform);
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
            try
            {
                if (EscMenuManager.instance.isEscMenuOpen)
                    return;
            }
            catch (Exception e)
            {
                Debug.Log("Could not access EscMenuManager instance. Error: " + e);
            }
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
            try
            {
                if (EscMenuManager.instance.isEscMenuOpen)
                    return;
            }
            catch (Exception e)
            {
                Debug.Log("Could not access EscMenuManager instance. Error: " + e);
            }
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
            try
            {
                if (EscMenuManager.instance.isEscMenuOpen)
                    return;
            }
            catch (Exception e)
            {
                Debug.Log("Could not access EscMenuManager instance. Error: " + e);
            }
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
    [ClientRpc]
    public void RpcActivateMenuNavigationControls(bool activate)
    {
        if (hasAuthority)
        {
            Debug.Log("RpcActivateMenuNavigationControls for player " + this.PlayerName);
            EnableMenuNavigationControls(activate);
        }
    }
    public void EnableGameplayActions(bool activate)
    {
        Debug.Log("EnableGameplayActions: for player " + this.PlayerName + " " + activate.ToString());
        if (activate)
        {
            try
            {
                if (EscMenuManager.instance.isEscMenuOpen)
                    return;
            }
            catch (Exception e)
            {
                Debug.Log("Could not access EscMenuManager instance. Error: " + e);
            }
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
                //gameplayActionsEnabled = true;
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
                    //kickingPosition.x = -30f;
                    kickingPosition.x = -40f;
                }
                else
                {
                    //kickingPosition.x = 30f;
                    kickingPosition.x = 40f;
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

                        CmdRequestFootballForKickOffGoblin(goblin.GetComponent<NetworkIdentity>().netId);

                        if (!this.isSinglePlayer)
                        {
                            goblin.ActivateKickAfterAccuracyBar(isKickingPlayer);
                        }
                        else if (this.isSinglePlayer && this.isLocalPlayer)
                        {
                            goblin.ActivateKickAfterAccuracyBar(isKickingPlayer);
                        }
                        
                        goblin.UpdateHasGoblinRepositionedForKickAfter();
                        continue;
                    }                        
                    yPositionModifier *= -1;
                    Vector3 newPosition = goblin.transform.position;
                    newPosition.x = 0f;
                    newPosition.y = 3 * yPositionModifier;
                    goblin.transform.position = newPosition;
                    goblin.UpdateHasGoblinRepositionedForKickAfter();
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
                            //newPosition.x = 41f;
                            newPosition.x = 52;
                        }
                        else
                        {
                            //newPosition.x = -41f;
                            newPosition.x = -52f;
                        }
                        goblin.transform.position = newPosition;

                        if (goblin.isCharacterSelected || selectGoblin == goblin)
                            FollowSelectedGoblin(goblin.transform);
                        else if (goblin.isQGoblin)
                            SwitchToQGoblin(false, Time.time);
                        else if (goblin.isEGoblin)
                            SwitchToEGoblin(false, Time.time);
                        goblin.UpdateHasGoblinRepositionedForKickAfter();
                        continue;
                    }
                    yPositionModifier *= -1;

                    if (teamName == "Grey")
                    {
                        //newPosition.x = 43.5f;
                        newPosition.x = 55f;
                    }
                    else
                    {
                        newPosition.x = -43.5f;
                        newPosition.x = -55f;
                    }
                    newPosition.y = 3 * yPositionModifier;
                    goblin.transform.position = newPosition;
                    goblin.UpdateHasGoblinRepositionedForKickAfter();
                }
            }
            // Enable positioning controls if the player is the kicking player
            if (this.isSinglePlayer && !this.isLocalPlayer)
            {
                areGoblinsRepositionedForKickAfter = true;
                //GameplayManager.instance.AIStartKickAfterPositioning();
                return;
            }
            EnableKickAfterPositioning(isKickingPlayer);
            CmdKickAfterPositioningControlsOnServerValues(isKickingPlayer);
            GameplayManager.instance.ActivateKickAfterPositionControlsPanel(isKickingPlayer);
            areGoblinsRepositionedForKickAfter = true;
        }
    }
    [TargetRpc]
    public void RpcRepositionForKickAfterFor3v3(NetworkConnection target, bool isKickingPlayer, bool isKickingTeam, uint scoringGoblin, float yPosition, int yPositionModifier)
    {
        Debug.Log("RpcRepositionForKickAfterFor3v3: for player: " + this.PlayerName);
        if (hasAuthority && !areGoblinsRepositionedForKickAfter)
        {
            if (isKickingTeam)
            {
                // Set the position of the kicking goblin and all that???
                GameObject scoringGoblinObject = NetworkIdentity.spawned[scoringGoblin].gameObject;
                Vector3 kickingPosition = scoringGoblinObject.transform.position;
                if (this.isTeamGrey)
                {
                    kickingPosition.x = -40f;
                }
                else
                {
                    kickingPosition.x = 40f;
                }
                kickingPosition.y = yPosition;
                if (isKickingPlayer)
                {
                    Debug.Log("RpcRepositionForKickAfterFor3v3: this player is the kicking player on the kicking team: " + this.PlayerName);
                    CmdRequestFootballForKickOffGoblin(selectGoblin.GetComponent<NetworkIdentity>().netId);
                    selectGoblin.transform.position = kickingPosition;
                    selectGoblin.UpdateHasGoblinRepositionedForKickAfter();
                    selectGoblin.ActivateKickAfterAccuracyBar(isKickingPlayer);
                }
                else
                {
                    Debug.Log("RpcRepositionForKickAfterFor3v3: this player is NOT the kicking player but they are on the kicking team: " + this.PlayerName);
                    Vector3 newPosition = selectGoblin.transform.position;
                    newPosition.x = 0f;
                    newPosition.y = 3 * yPositionModifier;
                    selectGoblin.transform.position = newPosition;
                    selectGoblin.UpdateHasGoblinRepositionedForKickAfter();
                }
            }
            else
            {
                Debug.Log("RpcRepositionForKickAfterFor3v3: this player IS NOT ON THE KICKING TEAM: " + this.PlayerName);
                Vector3 newPosition = Vector3.zero;
                if (this.goblinType == "Skirmisher")
                {
                    Debug.Log("RpcRepositionForKickAfterFor3v3: this player IS NOT ON THE KICKING TEAM and their goblin is the skirmisher, or the blocking goblin?: " + this.PlayerName);
                    newPosition.y = yPosition;
                    if (this.isTeamGrey)
                        newPosition.x = 52f;
                    else
                        newPosition.x = -52f;
                    //selectGoblin.transform.position = newPosition;
                    //FollowSelectedGoblin(selectGoblin.transform);
                }
                else if (this.goblinType == "Grenadier")
                {
                    newPosition.y = 3f;
                    if (this.isTeamGrey)
                        newPosition.x = 55f;
                    else
                        newPosition.x = -55f;
                }
                else if (this.goblinType == "Berserker")
                {
                    newPosition.y = -3f;
                    if (this.isTeamGrey)
                        newPosition.x = 55f;
                    else
                        newPosition.x = -55f;
                }
                selectGoblin.transform.position = newPosition;
                FollowSelectedGoblin(selectGoblin.transform);
                selectGoblin.UpdateHasGoblinRepositionedForKickAfter();
            }

            EnableKickAfterPositioning(isKickingPlayer);
            CmdKickAfterPositioningControlsOnServerValues(isKickingPlayer);
            GameplayManager.instance.ActivateKickAfterPositionControlsPanel(isKickingPlayer);
            areGoblinsRepositionedForKickAfter = true;
        }
    }
    void CmdKickAfterPositioningControlsOnServerValues(bool activate)
    {
        this.kickAfterPositioningControlsOnServer = activate;
    }
    public void EnableKickAfterPositioning(bool activate)
    {
        Debug.Log("EnableKickAfterPositioning: for player " + this.PlayerName + " " + activate.ToString());
        if (activate)
        {
            try
            {
                if (EscMenuManager.instance.isEscMenuOpen)
                    return;
            }
            catch (Exception e)
            {
                Debug.Log("Could not access EscMenuManager instance. Error: " + e);
            }
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
        if (hasAuthority)
        {
            EnableKickAfterPositioning(false);
            this.CmdKickAfterPositioningControlsOnServerValues(false);
        }
            
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
        Debug.Log("RpcStartKickAfterTimer: for player: " + this.PlayerName + " is kicking playeR: " + isKickingPlayer.ToString());
        if (hasAuthority)
            GameplayManager.instance.ActivateKickAfterTimerUI(isKickingPlayer);
    }
    [TargetRpc]
    public void RpcKickAfterUpdateInsctructionsText(NetworkConnection target, bool isKickingPlayer)
    {
        if (hasAuthority)
        {
            localIsKickingPlayer = isKickingPlayer;
            GameplayManager.instance.KickAfterUpdateInsctructionsText(isKickingPlayer);
        }
            
    }
    [TargetRpc]
    public void RpcActivateKickAfterKickingControls(NetworkConnection target, bool activate)
    {
        if (hasAuthority)
        {
            Debug.Log("RpcActivateKickAfterKickingControls: for player: " + this.PlayerName + " " + activate.ToString());
            EnableKickAfterKicking(activate);
            CmdKickAfterKickingControlsOnServerValues(activate);
            if (activate)
                selectGoblin.StartKickAfterKickAttempt();
        }
    }
    void CmdKickAfterKickingControlsOnServerValues(bool activate)
    {
        this.kickAfterKickingControlsOnServer = activate;
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
            CmdEnableGameplayControlsForServerValues(activate);
        }
    }
    [Command]
    void CmdEnableGameplayControlsForServerValues(bool activate)
    {
        this.goblinMovementControlsOnServer = activate;
        this.gameplayActionControlsOnServer = activate;
        this.qeSwitchingControlsOnServer = activate;
    }
    public void EnableKickAfterKicking(bool activate)
    {
        Debug.Log("EnableKickAfterKicking: for player " + this.PlayerName + " " + activate.ToString());
        if (activate)
        {
            try
            {
                if (EscMenuManager.instance.isEscMenuOpen)
                    return;
            }
            catch (Exception e)
            {
                Debug.Log("Could not access EscMenuManager instance. Error: " + e);
            }
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
            // Make sure not to add powerups picked up by the AI to the player's PowerUp UI
            if (this.isSinglePlayer && !this.isLocalPlayer)
            {
                PowerUpManager.instance.AIUpdateAIPowerUpUIImages(myPowerUps);
                return;
            }
            PowerUpManager.instance.UpdatePowerUpUIImages(myPowerUps);
            SoundManager.instance.PlaySound("pickup-powerup", 0.75f);
        }        
    }
    public void UsePowerUp(int powerUpNumber)
    {
        Debug.Log("UsePowerUp: Player is going to use power up: " + powerUpNumber.ToString());
        if (powerUpNumber < myPowerUps.Count && powerUpNumber >= 0)
        {
            Debug.Log("UsePowerUp: Player is able to use power up: " + powerUpNumber.ToString());
            if (hasAuthority)
            {
                try
                {
                    uint powerUpNetId = myPowerUps[powerUpNumber].GetComponent<NetworkIdentity>().netId;
                    CmdUsePowerUp(powerUpNetId);
                }
                catch (Exception e)
                {
                    Debug.Log("UsePowerUp: Player tried to get network id of a non-existent powerup: " + e);
                }
                
            }
        }
    }
    void PowerUpAnalogReader(Vector2 direction)
    {
        Debug.Log("PowerUpAnalogReader: Direction of right stick: " + direction.ToString() + " wasRightStickUsedToSelect: " + wasRightStickUsedToSelect.ToString());
        if (wasRightStickUsedToSelect)
            return;
        if (direction.x <= -0.75f)
        {
            Debug.Log("PowerUpAnalogReader: setting power up value to 0");
            UsePowerUp(0);
            wasRightStickUsedToSelect = true;
        }
        else if (direction.y >= 0.75f)
        {
            Debug.Log("PowerUpAnalogReader: setting power up value to 1");
            UsePowerUp(1);
            wasRightStickUsedToSelect = true;
        }
        else if (direction.x >= 0.75f)
        {
            Debug.Log("PowerUpAnalogReader: setting power up value to 2");
            UsePowerUp(2);
            wasRightStickUsedToSelect = true;
        }
        else if (direction.y <= -0.75f)
        {
            Debug.Log("PowerUpAnalogReader: setting power up value to 3");
            UsePowerUp(3);
            wasRightStickUsedToSelect = true;
        }
    }
    void CancelRightAnalogStickDirection()
    {
        Debug.Log("CancelRightAnalogStickDirection: wasRightStickUsedToSelect: " + wasRightStickUsedToSelect.ToString());
        wasRightStickUsedToSelect = false;
    }
    public void EnablePowerUpControls(bool activate)
    {
        Debug.Log("EnablePowerUpControls: for player " + this.PlayerName + " " + activate.ToString());
        if (activate)
        {
            try
            {
                if (EscMenuManager.instance.isEscMenuOpen)
                    return;
            }
            catch (Exception e)
            {
                Debug.Log("Could not access EscMenuManager instance. Error: " + e);
            }
            
            if (!powerUpsEnabled)
            {
                var uiModule = (InputSystemUIInputModule)EventSystem.current.currentInputModule;
                //uiModule.move = moveReference;
                //uiModule.submit = submitReference;
                //uiModule.move = InputActionReference.Create(InputManager.Controls.SelectPowerUps.SelectLeftOrRightComposite);
                //uiModule.submit = InputActionReference.Create(InputManager.Controls.SelectPowerUps.SubmitSelection);
                uiModule.enabled = true;
                var eventSystem = EventSystem.current;
                eventSystem.enabled = true;

                InputManager.Controls.PowerUps.Enable();
                InputManager.Controls.SelectPowerUps.Enable();
                InputManager.Controls.SelectPowerUps.RightAnalogStickDirection.Enable();
                powerUpsEnabled = true;
            }
        }
        else
        {
            InputManager.Controls.PowerUps.Disable();
            InputManager.Controls.SelectPowerUps.Disable();
            InputManager.Controls.SelectPowerUps.RightAnalogStickDirection.Disable();
            powerUpsEnabled = false;
        }
    }
    [Command]
    void CmdUsePowerUp(uint powerUpNetId)
    {
        if (serverPowerUpUints.Contains(powerUpNetId))
        {
            Debug.Log("CmdUsePowerUp: Player: " + this.PlayerName + " can use powerup with netid of: " + powerUpNetId.ToString());
            //this.serverPowerUpUints.Remove(powerUpNetId);
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
        if (this.isSinglePlayer && !this.isLocalPlayer)
        {
            PowerUpManager.instance.AIUpdateAIPowerUpUIImages(myPowerUps);
            return;
        }
        PowerUpManager.instance.UpdatePowerUpUIImages(myPowerUps);
    }
    public void RemoveUsedPowerUps()
    {
        if (hasAuthority)
        {
            // Make sure to remove "null" powerups? This appears to be an issue sometimes where the server destroys the powerup but it isn't removed from the player's list of powerups? Causing it to still be displayed in the UI
            try
            {
                myPowerUps = myPowerUps.Where(item => item != null).ToList();
            }
            catch (Exception e)
            {
                Debug.Log("RemoveUsedPowerUps: Failed to find and remove NULL powerups from myPowerUps list. Error: " + e);
            }
            if (this.isSinglePlayer && !this.isLocalPlayer)
            {
                PowerUpManager.instance.AIUpdateAIPowerUpUIImages(myPowerUps);
                return;
            }
            PowerUpManager.instance.UpdatePowerUpUIImages(myPowerUps);
            /*if (powerUpSelectedIndexNumber >= myPowerUps.Count)
            {
                powerUpSelectedIndexNumber = myPowerUps.Count - 1;
                if(wasRightStickUsedToSelect)
                    PowerUpManager.instance.GamepadActivateSelectedPowerUpBorder(powerUpSelectedIndexNumber);
                if (powerUpSelectedIndexNumber == -1)
                    wasRightStickUsedToSelect = false;
            }*/   
        }
            
    }
    /*void GamepadPowerUpSelectLeftRight(bool left)
    {
        Debug.Log("GamepadPowerUpSelectLeftRight: Left? " + left.ToString());
        if (Time.time > nextPowerUpSelectTime)
        {
            if (left)
            {
                powerUpSelectedIndexNumber--;
                if (powerUpSelectedIndexNumber < 0)
                    powerUpSelectedIndexNumber = myPowerUps.Count - 1;
                nextPowerUpSelectTime = Time.time + 0.5f;
            }
            else
            {
                powerUpSelectedIndexNumber++;
                if (powerUpSelectedIndexNumber >= myPowerUps.Count)
                    powerUpSelectedIndexNumber = 0;
                nextPowerUpSelectTime = Time.time + 0.5f;
            }
            if(myPowerUps.Count > 0)
                PowerUpManager.instance.GamepadActivateSelectedPowerUpBorder(powerUpSelectedIndexNumber);
        }   
    }
    void GamepadPowerUpSelectWithRightStick(Vector2 selectDirection)
    {
        Debug.Log("GamepadPowerUpSelectWithRightStick: Direction: " + selectDirection.ToString());
        if (Time.time > nextPowerUpSelectTime)
        {
            if (selectDirection.x <= -0.5f)
            {
                powerUpSelectedIndexNumber--;
                if (powerUpSelectedIndexNumber < 0)
                    powerUpSelectedIndexNumber = myPowerUps.Count - 1;
                //canSelectWithRightStickAgain = false;
                nextPowerUpSelectTime = Time.time + 0.4f;
                wasRightStickUsedToSelect = true;
            }
            else if (selectDirection.x >= 0.5f)
            {
                powerUpSelectedIndexNumber++;
                if (powerUpSelectedIndexNumber >= myPowerUps.Count)
                    powerUpSelectedIndexNumber = 0;
                //canSelectWithRightStickAgain = false;
                nextPowerUpSelectTime = Time.time + 0.4f;
                wasRightStickUsedToSelect = true;
            }
            if(powerUpSelectedIndexNumber > myPowerUps.Count)
                powerUpSelectedIndexNumber = myPowerUps.Count - 1;
            if (myPowerUps.Count > 0)
                PowerUpManager.instance.GamepadActivateSelectedPowerUpBorder(powerUpSelectedIndexNumber);
        }            
    }
    void GamepadPowerUpSelectWithRightStickComposite(Vector2 selectDirection)
    {
        Debug.Log("GamepadPowerUpSelectWithRightStickComposite: Direction: " + selectDirection.ToString());
        if (Time.time > nextPowerUpSelectTime)
        {
            if (selectDirection.x == -1f)
            {
                powerUpSelectedIndexNumber--;
                if (powerUpSelectedIndexNumber < 0)
                    powerUpSelectedIndexNumber = myPowerUps.Count - 1;
                //canSelectWithRightStickAgain = false;
                nextPowerUpSelectTime = Time.time + 0.4f;
                wasRightStickUsedToSelect = true;
            }
            else if (selectDirection.x == 1f)
            {
                powerUpSelectedIndexNumber++;
                if (powerUpSelectedIndexNumber >= myPowerUps.Count)
                    powerUpSelectedIndexNumber = 0;
                //canSelectWithRightStickAgain = false;
                nextPowerUpSelectTime = Time.time + 0.4f;
                wasRightStickUsedToSelect = true;
            }
            if (powerUpSelectedIndexNumber > myPowerUps.Count)
                powerUpSelectedIndexNumber = myPowerUps.Count - 1;
            if (myPowerUps.Count > 0)
                PowerUpManager.instance.GamepadActivateSelectedPowerUpBorder(powerUpSelectedIndexNumber);
        }
    }
    void GamepadResetSelectWithRightStick()
    {
        canSelectWithRightStickAgain = true;
    }
    void GamepadPowerUpSubmitSelection()
    {
        Debug.Log("GamepadPowerUpSubmitSelection");
        if (powerUpSelectedIndexNumber < myPowerUps.Count && powerUpSelectedIndexNumber >= 0)
            UsePowerUp(powerUpSelectedIndexNumber);
    }*/
    [ServerCallback]
    public void UpdatePlayerPossessionTracker(bool hasPosession)
    {
        Debug.Log("UpdatePlayerPossessionTracker: Does " + this.PlayerName + " have possession of the ball: " + hasPosession.ToString());
        if (hasPosession)
        {
            if (!isGainingPossesionPointsRoutineRunning)
            {
                Debug.Log("UpdatePlayerPossessionTracker: Starting GainPossessionPointsRoutine");
                GainPossessionPointsRoutine = GainPossessionPoints();
                StartCoroutine(GainPossessionPointsRoutine);
            }
            if (isLosingPossesionPointsRoutineRunning)
            {
                Debug.Log("UpdatePlayerPossessionTracker: STOPPING LosePossessionPointsRoutine");
                StopCoroutine(LosePossessionPointsRoutine);
                isLosingPossesionPointsRoutineRunning = false;
            }
            if (isNoPossessionCooldownRoutineRunning)
            {
                Debug.Log("UpdatePlayerPossessionTracker: STOPPING NoPossessionCooldownRoutine");
                StopCoroutine(NoPossessionCooldownRoutine);
                isNoPossessionCooldownRoutineRunning = false;
                didNoPossessionCooldownRoutineComplete = false;
            }
        }
        else
        {
            if (isGainingPossesionPointsRoutineRunning)
            {
                Debug.Log("UpdatePlayerPossessionTracker: STOPPING GainPossessionPointsRoutine");
                StopCoroutine(GainPossessionPointsRoutine);
                isGainingPossesionPointsRoutineRunning = false;
            }
            if (!isNoPossessionCooldownRoutineRunning && !didNoPossessionCooldownRoutineComplete)
            {
                Debug.Log("UpdatePlayerPossessionTracker: Starting NoPossessionCooldown");
                NoPossessionCooldownRoutine = NoPossessionCooldown();
                StartCoroutine(NoPossessionCooldownRoutine);
            }
        }
    }
    public void HandlePossessionPoints(float oldValue, float newValue)
    {
        if (isServer)
        {
            possessionPoints = newValue;
        }
        if (isClient)
        {
            /*if (this.teamName.ToLower().Contains("green"))
                GameplayManager.instance.UpdatePossessionBar(true, newValue);
            else
                GameplayManager.instance.UpdatePossessionBar(false, newValue);*/
        }
    }
    [ServerCallback]
    IEnumerator GainPossessionPoints()
    {
        isGainingPossesionPointsRoutineRunning = true;
        didNoPossessionCooldownRoutineComplete = false;
        float possessionPointTracker;
        bool isMaxValue = false;
        while (isGainingPossesionPointsRoutineRunning)
        {
            // Set the possession rate. Starts fast, gets slower as it goes on?
            if (possessionPoints < 30f)
            {
                gainPossessionPointsRate = 3.0f;
                possessionBonus = 1.0f;
            }
            if (possessionPoints >= 30f && possessionPoints < 50f)
            {
                gainPossessionPointsRate = 2.25f;
                possessionBonus = 1.1f;
            }
            if (possessionPoints >= 50f && possessionPoints < 70f)
            {
                gainPossessionPointsRate = 1.5f;
                possessionBonus = 1.2f;
            }
            if (possessionPoints >= 70f && possessionPoints < 90f)
            {
                gainPossessionPointsRate = 1.0f;
                possessionBonus = 1.3f;
            }
            if (possessionPoints >= 90f)
            {
                gainPossessionPointsRate = 0.75f;
                possessionBonus = 1.4f;
            }

            yield return new WaitForSeconds(1.0f);
            possessionPointTracker = possessionPoints + gainPossessionPointsRate;
            if (possessionPointTracker > 100f)
            {
                possessionPointTracker = 100f;
                isMaxValue = true;
            }
            if(GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time")
                HandlePossessionPoints(this.possessionPoints, possessionPointTracker);
            if (isMaxValue)
            {
                isGainingPossesionPointsRoutineRunning = false;
            }
        }
        yield break;
    }
    [ServerCallback]
    IEnumerator NoPossessionCooldown()
    {
        didNoPossessionCooldownRoutineComplete = false;
        isNoPossessionCooldownRoutineRunning = true;
        yield return new WaitForSeconds(2.0f);
        didNoPossessionCooldownRoutineComplete = true;
        isNoPossessionCooldownRoutineRunning = false;

        if (!isLosingPossesionPointsRoutineRunning)
        {
            LosePossessionPointsRoutine = LosePossessionPoints();
            StartCoroutine(LosePossessionPointsRoutine);
        }
    }
    [ServerCallback]
    IEnumerator LosePossessionPoints()
    {
        isLosingPossesionPointsRoutineRunning = true;
        float possessionPointTracker;
        bool isMinValue = false;
        while (isLosingPossesionPointsRoutineRunning)
        {
            // Set the possession rate. Starts fast, gets slower as it goes on?
            if (possessionPoints < 30f)
            {
                gainPossessionPointsRate = 3.0f;
                possessionBonus = 1.0f;
            }
            if (possessionPoints >= 30f && possessionPoints < 50f)
            {
                gainPossessionPointsRate = 2.25f;
                possessionBonus = 1.1f;
            }
            if (possessionPoints >= 50f && possessionPoints < 70f)
            {
                gainPossessionPointsRate = 1.5f;
                possessionBonus = 1.2f;
            }
            if (possessionPoints >= 70f && possessionPoints < 90f)
            {
                gainPossessionPointsRate = 1.0f;
                possessionBonus = 1.3f;
            }
            if (possessionPoints >= 90f)
            {
                gainPossessionPointsRate = 0.75f;
                possessionBonus = 1.4f;
            }

            yield return new WaitForSeconds(1.0f);
            possessionPointTracker = possessionPoints - (2.5f / gainPossessionPointsRate);

            if (possessionPointTracker <= 0f)
            {
                possessionPointTracker = 0f;
                isMinValue = true;
            }
            if (GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time")
                HandlePossessionPoints(this.possessionPoints, possessionPointTracker);
            if (isMinValue)
            {
                isLosingPossesionPointsRoutineRunning = false;
            }
        }
        yield break;
    }
    [ServerCallback]
    public void StopAllPossessionRoutines()
    {
        Debug.Log("StopAllPossessionRoutines for player: " + this.PlayerName);
        if (isGainingPossesionPointsRoutineRunning)
        {
            Debug.Log("StopAllPossessionRoutines: STOPPING GainPossessionPointsRoutine");
            StopCoroutine(GainPossessionPointsRoutine);
            isGainingPossesionPointsRoutineRunning = false;
        }
        if (isLosingPossesionPointsRoutineRunning)
        {
            Debug.Log("StopAllPossessionRoutines: STOPPING LosePossessionPointsRoutine");
            StopCoroutine(LosePossessionPointsRoutine);
            isLosingPossesionPointsRoutineRunning = false;
            
        }
        if (isNoPossessionCooldownRoutineRunning)
        {
            Debug.Log("StopAllPossessionRoutines: STOPPING NoPossessionCooldownRoutine");
            StopCoroutine(NoPossessionCooldownRoutine);
            isNoPossessionCooldownRoutineRunning = false;
        }
        didNoPossessionCooldownRoutineComplete = false;
        HandlePossessionPoints(this.possessionPoints, 0f);
        possessionBonus = 1.0f;
    }
    public void HandlePossessionBonus(float oldValue, float newValue)
    {
        if (isServer)
        {
            possessionBonus = newValue;
            //UpdatePossessionSpeedBonusForGoblinTeam(newValue);
        }
        if (isClient)
        { 

        }
    }
    // Get speed modifer for the possession bonus from this goblin's player owner. Possession bonus is divided by 3. So, of possession bonus is 20% (aka 1.2f) then it should make them 6.7% faster (1.067f)
    [ServerCallback]
    void UpdatePossessionSpeedBonusForGoblinTeam(float newPossessionSpeedBonus)
    {
        // Calculate new speed bonus for goblins on this player's team to use
        float possessionSpeedBonus = (newPossessionSpeedBonus - 1.0f);
        if (possessionSpeedBonus > 0)
        {
            possessionSpeedBonus /= 3f;
        }
        possessionSpeedBonus += 1.0f;
        foreach (GoblinScript goblin in goblinTeamOnServer)
        {
            goblin.possessionSpeedBonus = possessionSpeedBonus;
        }
    }
    [Command]
    void CmdTryToGiveCowboyYeehaw()
    {
        if ((GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time" || GameplayManager.instance.gamePhase == "kick-after-attempt") && CowboyManager.instance.isCowboySpawned)
        {
            // Get position of the Cowboy
            try
            {
                GameObject cowboy = GameObject.FindGameObjectWithTag("cowboy");
                if (Vector2.Distance(cowboy.transform.position, this.serverSelectGoblin.transform.position) <= 32.5f)
                {
                    Debug.Log("CmdTryToGiveCowboyYeehaw: Goblin and cowboy should be within range? Trying to give a yeehaw");
                    cowboy.GetComponent<CowboyScript>().PlayerIsGivingYeehaw(this.GetComponent<NetworkIdentity>().netId);
                }
            }
            catch (Exception e)
            {
                Debug.Log("CmdTryToGiveCowboyYeehaw: Error finding cowboy or whatever: " + e);
            }
            
        }
    }
    public void ExitToMainMenu()
    {
        if (hasAuthority)
        {
            if (isServer)
            {
                Game.StopHost();
                //Game.HostShutDownServer();
            }
            else
            {
                Game.StopClient();
                //Game.HostShutDownServer();
            }
            /*if (Time.timeScale != 1f)
                Time.timeScale = 1f;*/
        }
    }
    public void EnableMenuNavigationControls(bool activate)
    {
        Debug.Log("EnableMenuNavigationControls: for player " + this.PlayerName + " " + activate.ToString());
        if (activate)
        {
            if (!menuNavigationEnabled)
            {
                
                
                var uiModule = (InputSystemUIInputModule)EventSystem.current.currentInputModule;
                //uiModule.move = moveReference;
                //uiModule.submit = submitReference;
                uiModule.move = InputActionReference.Create(InputManager.Controls.UI.Navigate);
                uiModule.submit = InputActionReference.Create(InputManager.Controls.UI.Submit);
                uiModule.enabled = true;
                var eventSystem = EventSystem.current;
                eventSystem.enabled = true;
                InputManager.Controls.UI.Enable();
                menuNavigationEnabled = true;
                Debug.Log("EnableMenuNavigationControls: Is move enabled: " + moveReference.action.enabled.ToString() + " is submit enabled " + submitReference.action.enabled.ToString() + " is the UI controls enabled?: " + InputManager.Controls.UI.enabled.ToString());
                /*GameObject mainMenuButton = GameObject.FindGameObjectWithTag("mainMenuButton");
                var eventSystem = EventSystem.current;
                eventSystem.SetSelectedGameObject(mainMenuButton, new BaseEventData(eventSystem));*/
            }
        }
        else
        {
            InputManager.Controls.UI.Disable();
            menuNavigationEnabled = false;
        }
    }
    public void UpdatePowerUpRemainingUses()
    {
        if (hasAuthority)
        {
            if (this.isSinglePlayer && !this.isLocalPlayer)
            {
                PowerUpManager.instance.AIUpdateAIPowerUpUIImages(myPowerUps);
                return;
            }
            PowerUpManager.instance.UpdatePowerUpUIImages(myPowerUps);
        }   
    }
    [ClientCallback]
    public void PauseGamePlayer()
    {
        if (hasAuthority && this.isLocalPlayer)
            CmdPauseGamePlayer();
    }
    [Command]
    void CmdPauseGamePlayer()
    {
        GameplayManager.instance.PauseGameOnServer(this, this.GetComponent<NetworkIdentity>().netId);
    }
    [ClientCallback]
    public void ResumeGamePlayer()
    {
        if (hasAuthority && this.isLocalPlayer)
            CmdResumeGamePlayer();
    }
    [Command]
    void CmdResumeGamePlayer()
    {
        GameplayManager.instance.ResumeGameOnServer(this, this.GetComponent<NetworkIdentity>().netId);
    }
    [TargetRpc]
    public void RpcGamePausedByServer(NetworkConnection target, bool wasPaused)
    {
        Debug.Log("RpcGamePausedByServer: for player: " + this.PlayerName + ":" + this.ConnectionId.ToString() + ". Will pause game? " + wasPaused.ToString());
        if (hasAuthority && this.isLocalPlayer)
        {
            if (wasPaused)
                Time.timeScale = 0f;
            else
            {
                Time.timeScale = 1.0f;
                if (!this.isSinglePlayer)
                {
                    SoundManager.instance.PlaySound("ref-whistle", 1.0f);
                }
            }
                
            this.isGamePaused = wasPaused;
            GameplayManager.instance.ActivateGamePausedText(wasPaused);
            if (!GameplayManager.instance.isSinglePlayer)
            {
                try
                {
                    EscMenuManager.instance.UpdatePauseGameButtonText(wasPaused);
                }
                catch (Exception e)
                {
                    Debug.Log("RpcGamePausedByServer: could not access the EscMenuManager. Error: " + e);
                }
            }
            
        }
    }
    [TargetRpc]
    public void RpcErrorMessageToDisplayFromServer(NetworkConnection target, string message)
    {
        if (hasAuthority && this.isLocalPlayer)
        {
            GameplayManager.instance.DisplayErrorMessage(message);
        }
    }
    [ClientCallback]
    public void WinnerOfGameSteamAchievementChecks(string winnerName)
    {
        Debug.Log("Gameplayer.cs: WinnerOfGameSteamAchievementChecks: " + winnerName);
        if (winnerName == "draw")
            return;
        if (hasAuthority)
        {
            if (winnerName == "green")
            {
                if (this.isTeamGrey)
                {
                    SteamAchievementManager.instance.LosingPlayer();
                    if (this.isSinglePlayer)
                        SteamAchievementManager.instance.LoseSinglePlayer();
                }
                else
                {
                    SteamAchievementManager.instance.WinningPlayer();
                    if (this.isSinglePlayer)
                        SteamAchievementManager.instance.WinSinglePlayer();
                }
            }
            else if (winnerName == "grey")
            {
                if (this.isTeamGrey)
                {
                    SteamAchievementManager.instance.WinningPlayer();
                    if (this.isSinglePlayer)
                        SteamAchievementManager.instance.WinSinglePlayer();
                }
                else
                {
                    SteamAchievementManager.instance.LosingPlayer();
                    if (this.isSinglePlayer)
                        SteamAchievementManager.instance.LoseSinglePlayer();
                }
            }
        }
    }
    [TargetRpc]
    public void RpcKickAfterAttemptAchievement(NetworkConnection target)
    {
        Debug.Log("RpcKickAfterAttemptAchievement: Player made their kick after attempt: " + this.PlayerName);
        if (hasAuthority)
        {
            if (GameplayManager.instance.isSinglePlayer)
            {
                if (!this.isLocalPlayer)
                    return;
            }
            if (Time.time >= (kickAfterAttemptTime + 1f))
            {
                SteamAchievementManager.instance.KickAfterAttemptMade();
                kickAfterAttemptTime = Time.time;
            }

        }
    }
    [TargetRpc]
    public void RpcYeehawAchievement(NetworkConnection target)
    {
        Debug.Log("RpcYeehawAchievement: Player sent a YEEHAW to their team: " + this.PlayerName);
        if (hasAuthority)
        {
            if (GameplayManager.instance.isSinglePlayer)
            {
                if (!this.isLocalPlayer)
                    return;
            }
            if (Time.time >= (yeehawGivenTime + 1f))
            {
                SteamAchievementManager.instance.YeehawGiven();
                yeehawGivenTime = Time.time;
            }

        }
    }
    [TargetRpc]
    public void RpcBlockedKickAchievement(NetworkConnection target)
    {
        Debug.Log("RpcBlockedKickAchievement: Player blocked kick: " + this.name);
        if (hasAuthority)
        {
            if (GameplayManager.instance.isSinglePlayer)
            {
                if (!this.isLocalPlayer)
                    return;
            }
            if (Time.time >= (blockedKickTime + 1f))
            {
                SteamAchievementManager.instance.BlockedKick();
                blockedKickTime = Time.time;
            }

        }
    }
    public void HandleIsTeamGrey(bool oldValue, bool newValue)
    {
        if (isServer)
            this.isTeamGrey = newValue;
        if (isClient)
        {
            if (newValue)
            {
                myTeam = TeamManager.instance.greyTeam;
            }
            else
            {
                myTeam = TeamManager.instance.greenTeam;
            }
        }
    }
}
