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

    [Header("Split Ground Event")]
    [SerializeField] GameObject splitGroundPrefab;

    [Header("Streaker Goblin Event")]
    [SerializeField] GameObject streakerGoblinPrefab;

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
        else if (eventType == "split-ground")
        {
            SplitGroundEvent();
        }
        else if (eventType == "streaker-goblin")
        {
            StreakerGoblinEvent();
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
        if (IsGoblinTooCloseToEndzoneForEvent(positionOfEvent))
        {
            Debug.Log("BottlesEvent: Goblin with ball was too close to max x value. Cancelling random event");
            return;
        }

        //Cancel the random event if the goblin is too close to max x position
        /*if (Mathf.Abs(positionOfEvent.x) > (maxX - 7.5f))
        {
            Debug.Log("BottlesEvent: Goblin with ball was too close to max x value. Cancelling random event");
            return;
            
        }*/

        //Set event location Y position to be within bounds
        /*positionOfEvent.y -= 1.25f;
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
            positionOfEvent.x = minX;*/

        positionOfEvent = PositionOfEvent(positionOfEvent, 0f, 0f, directionModifier);

        GameObject randomEvent = Instantiate(brokenGlassPrefab);
        randomEvent.transform.position = positionOfEvent;
        NetworkServer.Spawn(randomEvent);
    }
    [ServerCallback]
    void SplitGroundEvent()
    {
        Debug.Log("Executing SplitGroundEvent on the server.");
        GoblinScript goblinWithBall = gameFootball.goblinWithBall;
        int directionModifier = 1;
        if (goblinWithBall.GetComponent<SpriteRenderer>().flipX)
            directionModifier = -1;
        Vector3 positionOfEvent = goblinWithBall.transform.position;
        if (IsGoblinTooCloseToEndzoneForEvent(positionOfEvent))
        {
            Debug.Log("BottlesEvent: Goblin with ball was too close to max x value. Cancelling random event");
            return;
        }

        positionOfEvent = PositionOfEvent(positionOfEvent, 1.1f, 0f, directionModifier);

        GameObject randomEvent = Instantiate(splitGroundPrefab);
        randomEvent.transform.position = positionOfEvent;
        NetworkServer.Spawn(randomEvent);
    }
    [ServerCallback]
    void StreakerGoblinEvent()
    {
        Debug.Log("Executing StreakerGoblinEvent on the server.");
        GoblinScript goblinWithBall = gameFootball.goblinWithBall;
        int directionModifier = 1;
        if (goblinWithBall.GetComponent<SpriteRenderer>().flipX)
            directionModifier = -1;
        Vector3 positionOfEvent = goblinWithBall.transform.position;
        if (IsGoblinTooCloseToEndzoneForEvent(positionOfEvent))
        {
            Debug.Log("BottlesEvent: Goblin with ball was too close to max x value. Cancelling random event");
            return;
        }

        positionOfEvent = PositionOfEvent(positionOfEvent, 0f, 2.0f, directionModifier);

        //Get y position of streaker to spawn from
        float topY = 10f;
        float bottomY = -12.5f;

        if (positionOfEvent.y >= 0.6f)
        {
            positionOfEvent.y = topY;
        }
        else
        {
            positionOfEvent.y = bottomY;
        }

        //Spawn three streaker objects?
        GameObject randomEvent = Instantiate(streakerGoblinPrefab);
        randomEvent.transform.position = positionOfEvent;
        NetworkServer.Spawn(randomEvent);
        //Second streaker
        float xModifier = Random.Range(1.0f, 3.0f);
        positionOfEvent.x += xModifier;
        GameObject randomEvent2 = Instantiate(streakerGoblinPrefab);
        randomEvent2.transform.position = positionOfEvent;
        NetworkServer.Spawn(randomEvent2);
        // streaker 3
        xModifier = Random.Range(1.0f, 3.0f);
        GameObject randomEvent3 = Instantiate(streakerGoblinPrefab);
        positionOfEvent.x -= xModifier;
        randomEvent3.transform.position = positionOfEvent;
        NetworkServer.Spawn(randomEvent3);
        //streakerGoblinPrefab.GetComponent<StreakerEvent>().StartAnimation(positionOfEvent);
    }
    [ServerCallback]
    bool IsGoblinTooCloseToEndzoneForEvent(Vector3 positionOfEvent)
    {
        bool isTooClose = false;

        //Cancel the random event if the goblin is too close to max x position
        if (Mathf.Abs(positionOfEvent.x) > (maxX - 7.5f))
        {
            isTooClose = true;
        }

        return isTooClose;
    }
    [ServerCallback]
    Vector3 PositionOfEvent(Vector3 currentPosition, float yModifier, float xModifier, int directionModifier)
    {
        Vector3 newEventPosition = currentPosition;

        //Set event location Y position to be within bounds
        newEventPosition.y -= 1.25f;
        if (newEventPosition.y > (maxY - yModifier))
            newEventPosition.y = (maxY - yModifier);
        else if (newEventPosition.y < (minY + yModifier))
            newEventPosition.y = (minY + yModifier);

        //Get X position of event. Make the distance from the player random between 3.5 - 7 units away from player?
        float xPosition = Random.Range(7.5f, 10.5f);
        newEventPosition.x += (xPosition * directionModifier);
        if (newEventPosition.x > (maxX - xModifier))
            newEventPosition.x = (maxX - xModifier);
        else if (newEventPosition.x < (minX + xModifier))
            newEventPosition.x = (minX + xModifier);

        Debug.Log("PositionOfEvent: Old position: " + currentPosition.ToString() + " New Position: " + newEventPosition.ToString());

        return newEventPosition;
    }
}
