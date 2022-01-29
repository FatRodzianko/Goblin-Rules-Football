using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PowerUpManager : NetworkBehaviour
{
    public static PowerUpManager instance;

    [Header("PowerUp UI Stuff")]
    [SerializeField] GameObject PowerUp1Image;
    [SerializeField] GameObject PowerUp2Image;
    [SerializeField] GameObject PowerUp3Image;
    [SerializeField] GameObject PowerUp4Image;
    [SerializeField] GameObject[] PowerUpUIImages;
    [SerializeField] Sprite EmptyPowerUpImage;

    [SerializeField] LayerMask powerUpLayer;

    Football gameFootball;

    int maxY = 5;
    int minY = -6;
    int maxX = 38;
    int minX = -38;

    [SerializeField] GameObject[] NormalPowerUps;
    [SerializeField] GameObject[] BlueShellPowerUps;
    IEnumerator powerUpGeneartor;
    bool isPowerUpGeneratorRunning;

    [Header("Active Power Ups")]
    public List<GameObject> ActivePowerUps = new List<GameObject>();
    public List<uint> ActivePowerUpNetIds = new List<uint>();

    [Header("Player Picked Up Power Ups")]
    public List<GameObject> PlayerPickedUpPowerUps = new List<GameObject>();
    public Dictionary<uint, uint> PowerUpPlayerOwnerDictionary = new Dictionary<uint, uint>();

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
    void Awake()
    {
        MakeInstance();
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        PowerUp1Image.GetComponent<Image>().sprite = EmptyPowerUpImage;
        PowerUp2Image.GetComponent<Image>().sprite = EmptyPowerUpImage;
        PowerUp3Image.GetComponent<Image>().sprite = EmptyPowerUpImage;
        PowerUp4Image.GetComponent<Image>().sprite = EmptyPowerUpImage;
    }
    public void GetFootballObject(Football football)
    {
        gameFootball = football;
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    [ServerCallback]
    public void StartGeneratingPowerUps(bool generate)
    {
        Debug.Log("StartGeneratingPowerUps: " + generate.ToString());
        if (GameplayManager.instance.gamePhase == "gameplay" && generate && !isPowerUpGeneratorRunning)
        {
            Debug.Log("StartGeneratingPowerUps: starting GeneratePowerUpsRoutine");
            powerUpGeneartor = GeneratePowerUpsRoutine();
            StartCoroutine(powerUpGeneartor);
        }
        if (!generate)
        {
            Debug.Log("StartGeneratingPowerUps: Stopping GeneratePowerUpsRoutine");
            isPowerUpGeneratorRunning = false;
            StopCoroutine(powerUpGeneartor);            
        }
    }
    [ServerCallback]
    IEnumerator GeneratePowerUpsRoutine()
    {
        isPowerUpGeneratorRunning = true;
        float randomWaitTime;
        while (isPowerUpGeneratorRunning)
        {
            randomWaitTime = Random.Range(0.75f, 5.5f);
            yield return new WaitForSeconds(randomWaitTime);
            CheckIfAPowerUpDrops();
            //yield break; 
        }
        //yield return new WaitForSeconds(1.0f);
        yield break;
    }
    [ServerCallback]
    void CheckIfAPowerUpDrops()
    {
        if (ActivePowerUpNetIds.Count < 9)
        {
            float blueShellLikelihood = 0.95f;
            if (gameFootball.isHeld)
            {
                int differenceInScore = GameplayManager.instance.greenScore - GameplayManager.instance.greyScore;
                if ((gameFootball.goblinWithBall.isGoblinGrey && differenceInScore > 14) || (!gameFootball.goblinWithBall.isGoblinGrey && differenceInScore < -14))
                {
                    Debug.Log("CheckIfAPowerUpDrops: Goblin with ball is down by more than 14 - increase blueshell likelihood");
                    blueShellLikelihood = 0.5f;
                }
            }
            bool willPowerUpDrop = false;
            bool willPowerUpBeBlueShell = false;

            if (Random.Range(0f, 1f) > 0.5f)
                willPowerUpDrop = true;

            if (willPowerUpDrop)
            {
                if (Random.Range(0f, 1f) > blueShellLikelihood)
                    willPowerUpBeBlueShell = true;
            }
            if (willPowerUpDrop && willPowerUpBeBlueShell)
                BlueShellPowerUpDrop();
            else if (willPowerUpDrop && !willPowerUpBeBlueShell)
                NormalPowerUpDrop();
        }
    }
    [ServerCallback]
    void NormalPowerUpDrop()
    {
        Debug.Log("NormalPowerUpDrop");
        //string[] headsTails = new[]
        //{ "heads","tails"};


        var rng = new System.Random();
        GameObject powerUptoSpawn = NormalPowerUps[rng.Next(NormalPowerUps.Length)];

        //Vector3 powerUpPosition = Vector3.zero;
        float yPosition = Random.Range(minY, maxY);
        float xPosition = Random.Range(5f, 15f);

        /*if (gameFootball.isHeld && gameFootball.goblinWithBall != null)
        {
            if (gameFootball.goblinWithBall.isGoblinGrey)
                xPosition = xPosition * -1f;

            xPosition = gameFootball.goblinWithBall.transform.position.x + xPosition;
        }
        else
        {
            int negOrPos = Random.Range(0, 2) * 2 - 1;
            xPosition = gameFootball.transform.position.x + (xPosition * negOrPos);
        }*/

        //Randomly choose a gameplayer to spawn powerup near their selected goblin
        int index = rng.Next(Game.GamePlayers.Count);
        GamePlayer playerToSpawnBy = Game.GamePlayers[index];
        Debug.Log("NormalPowerUpDrop: Spawning powerup by player: " + playerToSpawnBy.PlayerName);
        int negOrPos = Random.Range(0, 2) * 2 - 1;
        xPosition = playerToSpawnBy.serverSelectGoblin.transform.position.x + (xPosition * negOrPos);

        if (xPosition > maxX)
            xPosition = maxX;
        else if (xPosition < minX)
            xPosition = minX;
        GameObject powerUp = Instantiate(powerUptoSpawn);
        powerUp.transform.position = new Vector3(xPosition, yPosition, 0f);
        // Check if the spawned powerup is going to overlap with another powerup. If it does, move it
        Collider2D[] colliders = Physics2D.OverlapCircleAll(powerUp.transform.position, 1.5f, powerUpLayer);
        //if (Physics.CheckSphere(powerUp.transform.position, 1.5f))
        if (colliders.Length > 0)
        {
            Debug.Log("NormalPowerUpDrop: Overlapping powerup detected. Moving object");
            negOrPos = Random.Range(0, 2) * 2 - 1;

            if (xPosition >= (maxX - 1.6f))
                xPosition -= 1.6f;
            else if (xPosition <= (minX - 1.6f))
                xPosition += 1.6f;
            else
            {
                xPosition = 1.6f * negOrPos;
            }

            if (yPosition >= (maxY - 1.6f))
                yPosition -= 1.6f;
            else if (yPosition <= (minY - 1.6f))
                yPosition += 1.6f;
            else
            {
                yPosition = 1.6f * negOrPos;
            }

            powerUp.transform.position = new Vector3(xPosition, yPosition, 0f);
        }

        NetworkServer.Spawn(powerUp);
        ActivePowerUps.Add(powerUp);
        ActivePowerUpNetIds.Add(powerUp.GetComponent<NetworkIdentity>().netId);

    }
    [ServerCallback]
    void BlueShellPowerUpDrop()
    {
        Debug.Log("BlueShellPowerUpDrop");
        //always spawn powerup near player who is losing? Don't pick a player randomly like it does for normal powerups
    }
    [Server]
    public void DestroyPowerUp(uint powerUpNetId)
    {
        GameObject powerUptoDestroy = NetworkIdentity.spawned[powerUpNetId].gameObject;
        //ActivePowerUps.Remove(powerUptoDestroy);
        //ActivePowerUpNetIds.Remove(powerUpNetId);
        RemovePowerUpFromLists(powerUpNetId);
        RemovePowerUpOwnedByPlayer(powerUpNetId);
        NetworkServer.Destroy(powerUptoDestroy);
    }
    [Server]
    public void RemovePowerUpFromLists(uint powerUpNetId)
    {        
        if (ActivePowerUpNetIds.Contains(powerUpNetId))
        {
            GameObject powerUpToRemove = NetworkIdentity.spawned[powerUpNetId].gameObject;
            ActivePowerUps.Remove(powerUpToRemove);
            ActivePowerUpNetIds.Remove(powerUpNetId);
        }
        
    }
    public void RemovePowerUpOwnedByPlayer(uint powerUpNetId)
    {
        if (PowerUpPlayerOwnerDictionary.ContainsKey(powerUpNetId))
        {
            GameObject powerUpToRemove = NetworkIdentity.spawned[powerUpNetId].gameObject;
            PlayerPickedUpPowerUps.Remove(powerUpToRemove);
            PowerUpPlayerOwnerDictionary.Remove(powerUpNetId);
        }
    }
    public void AddPowerupOwnedByPlayer(uint powerUpNetId, uint playerOwner)
    {
        if (!PowerUpPlayerOwnerDictionary.ContainsKey(powerUpNetId))
        {
            PowerUpPlayerOwnerDictionary.Add(powerUpNetId, playerOwner);
            GameObject powerUpToAdd = NetworkIdentity.spawned[powerUpNetId].gameObject;
            PlayerPickedUpPowerUps.Add(powerUpToAdd);
        }
    }
    [Client]
    public void UpdatePowerUpUIImages(List<PowerUp> myPowerUps)
    {
        for (int i = 0; i < 4; i++)
        {
            if ((i+1) > myPowerUps.Count)
                PowerUpUIImages[i].GetComponent<Image>().sprite = EmptyPowerUpImage;
            else
                PowerUpUIImages[i].GetComponent<Image>().sprite = myPowerUps[i].mySprite;
            /*if (i == 1)
            {
                if (i > myPowerUps.Count)
                    PowerUp1Image.GetComponent<Image>().sprite = EmptyPowerUpImage;
                else
                    PowerUp1Image.GetComponent<Image>().sprite = myPowerUps[i].mySprite;
            }
            else if (i == 2)
            {
                if (i > myPowerUps.Count)
                    PowerUp2Image.GetComponent<Image>().sprite = EmptyPowerUpImage;
                else
                    PowerUp2Image.GetComponent<Image>().sprite = myPowerUps[i].mySprite;
            }*/
        }
    }
    [ServerCallback]
    public void PlayerUsePowerUp(uint powerUpNetId, uint playerNetId)
    {
        if (PowerUpPlayerOwnerDictionary.ContainsKey(powerUpNetId) && PowerUpPlayerOwnerDictionary[powerUpNetId] == playerNetId)
        {
            Debug.Log("PowerUpManager PlayerUsePowerUp: The player with netid " + playerNetId.ToString() + " owns the powerup with net id: " + powerUpNetId.ToString());
            GameObject powerUpToUse = NetworkIdentity.spawned[powerUpNetId].gameObject;
            powerUpToUse.GetComponent<PowerUp>().UsePowerUp();

            GamePlayer player = NetworkIdentity.spawned[playerNetId].GetComponent<GamePlayer>();
            player.serverPowerUpUints.Remove(powerUpNetId);

            PlayerPickedUpPowerUps.Remove(powerUpToUse);
            PowerUpPlayerOwnerDictionary.Remove(powerUpNetId);
            NetworkServer.Destroy(powerUpToUse);
            //player.RpcRemoveUsedPowerUp(player.connectionToClient, powerUpNetId);

        }
    }
}

