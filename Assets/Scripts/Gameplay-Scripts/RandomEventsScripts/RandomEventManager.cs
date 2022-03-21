using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RandomEventManager : NetworkBehaviour
{
    [Header("Event Info")]
    [SerializeField] string[] EventTypes;
    IEnumerator randomEventGenerator;
    bool isRandomEventGeneratorRunning;
    Football gameFootball;
    public float RandomEventOdds = 0.5f;

    [Header("Thrown Bottle Event")]
    [SerializeField] GameObject brokenGlassPrefab;

    public static RandomEventManager instance;

    [Header("Field Parameters")]
    [SerializeField] float maxY; // = 5.0f;
    [SerializeField] float minY; // = -6.0f;
    [SerializeField] float maxX; // = 41.5f;
    [SerializeField] float minX; // = -41.5f;

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
    void Awake()
    {
        MakeInstance();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GetFootballObject(Football football)
    {
        gameFootball = football;
    }
    [ServerCallback]
    public void StartGeneratingRandomEvents(bool generate)
    {
        Debug.Log("StartGeneratingRandomEvents: " + generate.ToString());
        if ((GameplayManager.instance.gamePhase == "gameplay" || GameplayManager.instance.gamePhase == "xtra-time") && generate && !isRandomEventGeneratorRunning)
        {
            Debug.Log("StartGeneratingRandomEvents: starting GenerateRandomEventsRoutine");
            randomEventGenerator = GenerateRandomEventsRoutine();
            StartCoroutine(randomEventGenerator);
        }
        if (!generate)
        {
            Debug.Log("StartGeneratingRandomEvents: Stopping GenerateRandomEventsRoutine");
            isRandomEventGeneratorRunning = false;
            StopCoroutine(randomEventGenerator);
        }
    }
    [ServerCallback]
    IEnumerator GenerateRandomEventsRoutine()
    {
        isRandomEventGeneratorRunning = true;
        float randomWaitTime;
        while (isRandomEventGeneratorRunning)
        {
            randomWaitTime = Random.Range(4.5f, 10.5f);
            yield return new WaitForSeconds(randomWaitTime);
            CheckForRandomEvent();
            //yield break; 
        }
        //yield return new WaitForSeconds(1.0f);
        yield break;
    }
    [ServerCallback]
    void CheckForRandomEvent()
    {
        // Only create an event if a player is running with the football?
        if (!gameFootball.isHeld)
            return;

        //Random event 50% odds by default? Change odds based on gameplay conditions like if winning player has ball, modify odds?
        int differenceInScore = GameplayManager.instance.greenScore - GameplayManager.instance.greyScore;
        GoblinScript goblinWithBall = gameFootball.goblinWithBall;

        if (Mathf.Abs(differenceInScore) > 13)
        {
            Debug.Log("CheckForRandomEvent: difference in score is 14 or more. Adjusting random event odds");
            if ((goblinWithBall.isGoblinGrey && differenceInScore < 0) || (!goblinWithBall.isGoblinGrey && differenceInScore > 0))
            {
                Debug.Log("CheckForRandomEvent: Player ahead has the ball. Increasing random event odds");
                RandomEventOdds = 0.25f;
            }
            else
            {
                Debug.Log("CheckForRandomEvent: Player ahead DOES NOT HAVE the ball. DECREASING random event odds");
                RandomEventOdds = 0.75f;
            }                
        }
        else
            RandomEventOdds = 0.5f;
        

        if (Random.Range(0f, 1f) > RandomEventOdds)
        {
            Debug.Log("CheckForRandomEvent: Random event number greater than odds of: " + RandomEventOdds.ToString() + ". Creating a random event");
            var rng = new System.Random();
            string eventType = EventTypes[rng.Next(EventTypes.Length)];
            CreateRandomEvent(eventType);
        }
    }
    [ServerCallback]
    void CreateRandomEvent(string eventType)
    {
        Debug.Log("CreateRandomEvent: type: " + eventType);
        if (eventType == "bottles")
        {
            BottlesEvent();
        }
    }
    [ServerCallback]
    void BottlesEvent()
    {
        //Define placement boundaries for the broken bottles
        /*float maxY = 5.0f;
        float minY = -6.0f;
        float maxX = 41.5f;
        float minX = -41.5f;*/

        //Get position of the goblin with the ball and the direction they are facing. use that to determine where the random event will spawn
        GoblinScript goblinWithBall = gameFootball.goblinWithBall;
        int directionModifier = 1;
        if (goblinWithBall.GetComponent<SpriteRenderer>().flipX)
            directionModifier = -1;
        Vector3 positionOfEvent = goblinWithBall.transform.position;
        
        //Cancel the random event if the goblin is too close to max x position
        if (Mathf.Abs(positionOfEvent.x) > (maxX - 7.5f))
        {
            Debug.Log("BottlesEvent: Goblin with ball was too close to max x value. Cancelling random event");
            return;
        }

        //Set event location Y position to be within bounds
        positionOfEvent.y -= 1.25f;
        if (positionOfEvent.y > maxY)
            positionOfEvent.y = maxY;
        else if(positionOfEvent.y < minY)
            positionOfEvent.y = minY;

        //Get X position of event. Make the distance from the player random between 3.5 - 7 units away from player?
        float xPosition = Random.Range(7.5f, 10.5f);
        positionOfEvent.x += (xPosition * directionModifier);
        if (positionOfEvent.x > maxX)
            positionOfEvent.x = maxX;
        else if (positionOfEvent.x < minX)
            positionOfEvent.x = minX;

        GameObject randomEvent = Instantiate(brokenGlassPrefab);
        randomEvent.transform.position = positionOfEvent;
        NetworkServer.Spawn(randomEvent);
    }
}
