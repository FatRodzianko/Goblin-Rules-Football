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

    [Header("Characters")]
    [SerializeField] private GameObject grenadierPrefab;
    [SerializeField] private GameObject skrimisherPrefab;
    [SerializeField] private GameObject berserkerPrefab;
    [SyncVar] public bool areCharactersSpawnedYet = false;

    [Header("My Goblin Team")]
    public SyncList<uint> goblinTeamNetIds = new SyncList<uint>();
    public List<GoblinScript> goblinTeam = new List<GoblinScript>();
    public GoblinScript selectGoblin;
    public GoblinScript qGoblin;
    public GoblinScript eGoblin;
    public bool canSwitchGoblin = true;

    [Header("Team Info")]
    [SyncVar] public string teamName;
    [SyncVar] public bool doesTeamHaveBall;

    [Header("Football")]
    [SerializeField] private GameObject footballPrefab;

    [SerializeField] CinemachineVirtualCamera myCamera;
    public Football football;


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
        Debug.Log("OnStartAuthority");
        base.OnStartAuthority();
        gameObject.name = "LocalGamePlayer";
        gameObject.tag = "LocalGamePlayer";

        InputManager.Controls.Player.SwitchQ.performed += _ => SwitchToQGoblin();
        InputManager.Controls.Player.SwitchE.performed += _ => SwitchToEGoblin();
        InputManager.Controls.Player.Attack.performed += _ => GoblinAttack();
        InputManager.Controls.Player.Slide.performed += _ => SlideGoblin();
        InputManager.Controls.Player.Dive.performed += _ => DiveGoblin();
        InputManager.Controls.Player.Block.performed += _ => StartBlockGoblin();
        InputManager.Controls.Player.Block.canceled += _ => StopBlockGoblin();

        myCamera = GameObject.FindGameObjectWithTag("camera").GetComponent<CinemachineVirtualCamera>();

        /*TrackFootballScript footballTracker = Camera.main.GetComponent<TrackFootballScript>();
        if (!footballTracker.myPlayer)
        {
            footballTracker.myPlayer = this;
        }*/
        CameraMarker myCameraMarker = Camera.main.GetComponent<CameraMarker>();
        if (!myCameraMarker.myPlayer)
            myCameraMarker.myPlayer = this;

    }

    public override void OnStartClient()
    {
        Debug.Log("OnStartClient Spawned GamePlayer with ConnectionId: " + ConnectionId.ToString());
        base.OnStartClient();
        Game.GamePlayers.Add(this);
        Debug.Log("OnStartClient: Will check if player has authority GamePlayer with ConnectionId: " + ConnectionId.ToString());
        if (hasAuthority)
        {
            //CmdSpawnPlayerCharacters();
            CmdGetTeamName();
            CmdSpawnFootball();
            CmdSpawnPlayerCharacters();

        }
        if (hasAuthority)
        {
            football = GameObject.FindGameObjectWithTag("football").GetComponent<Football>();
            if (football)
                football.localPlayer = this;
        }
        

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
    public void SwitchToQGoblin()
    {
        Debug.Log("SwitchToQGoblin: " + canSwitchGoblin.ToString());
        if (canSwitchGoblin || qGoblin.doesCharacterHaveBall)
        {
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
        }
        
    }
    public void SwitchToEGoblin()
    {
        Debug.Log("SwitchToEGoblin: " + canSwitchGoblin.ToString());
        if (canSwitchGoblin || eGoblin.doesCharacterHaveBall)
        {
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
        myCamera.Follow = goblinToFollow.transform;
    }
}
