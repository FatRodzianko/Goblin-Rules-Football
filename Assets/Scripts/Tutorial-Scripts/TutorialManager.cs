using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;
    public string gamePhase;
    public bool gamepadUI = false;

    [Header("Player Object")]
    [SerializeField] TutorialPlayer tutorialPlayer;
    [SerializeField] TutorialPlayer secondTutorialPlayer;


    [Header("Goblin Prefabs")]
    [SerializeField] GameObject grenadierPrefb;
    [SerializeField] GameObject skirmisherPrefb;
    [SerializeField] GameObject berserkerPrefb;

    [Header("Green Goblin Objects")]
    public GameObject greenGrenadierObject;
    public TutorialGoblinScript greenGrenadierScript;
    public GameObject greenSkirmisherObject;
    public TutorialGoblinScript greenSkirmisherScript;
    public GameObject greenBerserkerObject;
    public TutorialGoblinScript greenBerserkerScript;

    [Header("Grey Goblin Objects")]
    public GameObject greyGrenadierObject;
    public TutorialGoblinScript greyGrenadierScript;
    public GameObject greySkirmisherObject;
    public TutorialGoblinScript greySkirmisherScript;
    public GameObject greyBerserkerObject;
    public TutorialGoblinScript greyBerserkerScript;

    [Header("Teams and Stuff")]
    public List<TutorialGoblinScript> greenGoblins = new List<TutorialGoblinScript>();
    public List<TutorialGoblinScript> greyGoblins = new List<TutorialGoblinScript>();

    [Header("Tutorial Progress and stuff?")]
    public int tutIndex = 0;

    [Header("Message Board: Top")]
    [SerializeField] GameObject messageBoardTop;
    [SerializeField] TextMeshProUGUI messageBoardTopText;

    [Header("Goblin Spawn Positions")]
    [SerializeField] Vector3 firstGoblinPosition;
    [SerializeField] Vector3 greenBerserkerPosition;
    [SerializeField] Vector3 greenskirmisherPosition;

    [Header("Football Info")]
    [SerializeField] GameObject footballPrefab;
    public TutorialFootball footballScript;
    [SerializeField] Vector3 footballSpawnPosition;

    [Header("Move To Area and Stuff")]
    [SerializeField] GameObject moveToCicle;
    [SerializeField] Vector3 moveToCicleSpawnPosition;

    [Header("Grey Team spawn locations")]
    [SerializeField] Vector3 greyGrenadierSpawnLocation;
    [SerializeField] Vector3 greySkirmisherSpawnLocation;
    [SerializeField] Vector3 greyBerserkerSpawnLocation;
    [SerializeField] Vector3 greenGrenadierNewLocation;
    [SerializeField] Vector3 greenSkirmisherNewLocation;
    [SerializeField] Vector3 greenBerserkerNewLocation;

    [Header("Power Up Stuff")]
    [SerializeField] GameObject powerUpPrefab;
    public GameObject powerUpObject;
    [SerializeField] GameObject powerUpMarker;

    [Header("PowerUp UI Stuff")]
    [SerializeField] GameObject PowerUp1Image;
    [SerializeField] GameObject PowerUp2Image;
    [SerializeField] GameObject PowerUp3Image;
    [SerializeField] GameObject PowerUp4Image;
    [SerializeField] GameObject[] PowerUpUIImages;
    [SerializeField] GameObject[] PowerUpUIRemainingUses;
    [SerializeField] Sprite EmptyPowerUpImage;

    [Header("Active Power Ups")]
    public List<GameObject> ActivePowerUps = new List<GameObject>();
    public List<uint> ActivePowerUpNetIds = new List<uint>();
    public int blueShellCount = 0;

    [Header("UI to update for gamepad/keyboard controls")]
    [SerializeField] private GameObject PowerUpText1;
    [SerializeField] private GameObject PowerUpText2;
    [SerializeField] private GameObject PowerUpText3;
    [SerializeField] private GameObject PowerUpText4;
    [SerializeField] private GameObject DpadImage1;
    [SerializeField] private GameObject DpadImage2;
    [SerializeField] private GameObject DpadImage3;
    [SerializeField] private GameObject DpadImage4;

    [Header("Obstacle Stuff")]
    [SerializeField] GameObject rockObstaclePrefab;
    [SerializeField] GameObject brushObstaclePrefab;
    public GameObject rockObstacleObject;
    public GameObject brushObstacleObject;

    [Header("Touchdown Trigger")]
    [SerializeField] GameObject touchDownTrigger;

    [Header("Layer Mask stuff")]
    [SerializeField] LayerMask goblinLayerMask;
    [SerializeField] LayerMask obstacleLayerMask;

    [Header("UI Stuff")]
    [SerializeField] GameObject PossessionFootballGreen;
    [SerializeField] GameObject PossessionFootballGrey;
    [SerializeField] GameObject PossessionBarGreen;
    [SerializeField] GameObject PossessionBarGrey;
    [SerializeField] GameObject PossessionBarMarker;
    [SerializeField] TextMeshProUGUI ScoreGreenText;

    [Header("Touchdown")]
    [SerializeField] GameObject TouchDownPanel;
    [SerializeField] TextMeshProUGUI touchDownText;
    [SerializeField] TextMeshProUGUI touchDownTeamText;

    [Header("Kick After Attempt")]
    public float yPositionOfKickAfter;
    [SerializeField] GameObject KickAfterPositionControlsPanel;
    [SerializeField] GameObject KickAfterTimerBeforeKickPanel;
    [SerializeField] GameObject KickAfterWasKickGoodPanel;
    [SerializeField] TextMeshProUGUI TheKickWasText;
    [SerializeField] TextMeshProUGUI KickWasGoodText;
    [SerializeField] TextMeshProUGUI KickWasNotGoodText;
    [SerializeField] TextMeshProUGUI KickWasBlockedText;

    [Header("Score ?")]
    public int greenScore = 0;

    [Header("Pause/Resume game")]
    public bool isGamePaused = false;

    [Header("Field Parameters")]
    [SerializeField] public float maxY; // 5. 6f
    [SerializeField] public float minY; // -6. 5f
    [SerializeField] public float maxX; // 44. 4f
    [SerializeField] public float minX; // -44. 5f

    [Header("Index Completion trackers")]
    public bool index0Tracker = false;
    public bool index1Tracker = false;
    public bool index2Tracker = false;
    public bool index3Tracker = false;
    public bool index4Tracker = false;
    public bool index5Tracker = false;
    public bool index6Tracker = false;
    public bool index7Tracker = false;
    public bool index8Tracker = false;
    public bool index9Tracker = false;
    public bool index10Tracker = false;
    public bool index11Tracker = false;
    public bool index12Tracker = false;
    public bool index13Tracker = false;
    public bool index14Tracker = false;
    public bool index15Tracker = false;
    public bool index16Tracker = false;
    public bool index17Tracker = false;
    public bool index18Tracker = false;
    public bool index19Tracker = false;
    public bool index20Tracker = false;
    public bool index21Tracker = false;
    public bool index22Tracker = false;
    public bool index23Tracker = false;
    public bool index24Tracker = false;
    public bool index25Tracker = false;
    public bool index26Tracker = false;
    public bool index27Tracker = false;
    public bool index28Tracker = false;
    public bool index29Tracker = false;
    public bool index30Tracker = false;
    public bool index31Tracker = false;
    public bool index32Tracker = false;
    public bool index33Tracker = false;
    public bool index34Tracker = false;
    public bool index35Tracker = false;
    public bool index36Tracker = false;
    public bool index37Tracker = false;
    public bool index38Tracker = false;
    public bool index39Tracker = false;
    public bool index40Tracker = false;
    public bool index41Tracker = false;
    public bool index42Tracker = false;
    public bool index43Tracker = false;
    public bool index44Tracker = false;
    public bool index45Tracker = false;
    public bool index46Tracker = false;
    public bool index47Tracker = false;
    public bool index48Tracker = false;
    public bool index49Tracker = false;
    public bool index50Tracker = false;
    public bool index51Tracker = false;
    public bool index52Tracker = false;
    public bool index53Tracker = false;
    public bool index54Tracker = false;
    public bool index55Tracker = false;

    [Header("Other checkpoint trackers")]
    public bool doneSprinting = false;
    public bool usedQE = false;
    public bool playerPickedUpBall = false;
    public bool firstThrownPass = false;
    public bool donePunchingPlayerGoblin = false;
    public bool playerBlocked;
    public bool firstKickDownField = false;
    public bool opponentPickedUpBall = false;
    public bool playerCausedFumble = false;
    public bool opponentFumbledFromSlide = false;
    public bool playerPickedUpBallAfterSlideTackle = false;
    public bool playerPickedUpPowerUp = false;
    public bool playerUsedPowerUp = false;
    public bool playerIsNearEndZone = false;
    public bool playerScoredTouchdown = false;
    public bool playerSubmittedKickAfterPosition = false;
    public bool playerChangedKickAfterPosition = false;
    public bool playerSubmittedKickAfterAccuracy = false;
    public bool wasKickGood = false;

    private void Awake()
    {
        this.gamepadUI = GamepadUIManager.instance.gamepadUI;
        MakeInstance();
    }
    void MakeInstance()
    {
        Debug.Log("Tutorial MakeInstance.");
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        if (!messageBoardTop.activeInHierarchy)
            messageBoardTop.SetActive(true);
        if(!messageBoardTopText.gameObject.activeInHierarchy)
            messageBoardTopText.gameObject.SetActive(true);

        // Set initial welcome text
        messageBoardTopText.text = "Hello and welcome to Goblin rules football!";
        if (tutorialPlayer == null)
            tutorialPlayer = GameObject.FindGameObjectWithTag("TutorialPlayer").GetComponent<TutorialPlayer>();

        PossessionBarMarker.SetActive(false);

        PowerUp1Image.GetComponent<Image>().sprite = EmptyPowerUpImage;
        PowerUp2Image.GetComponent<Image>().sprite = EmptyPowerUpImage;
        PowerUp3Image.GetComponent<Image>().sprite = EmptyPowerUpImage;
        PowerUp4Image.GetComponent<Image>().sprite = EmptyPowerUpImage;
        //
        // Update later!!!
        //UpdatePowerUpBoardUIForGamepad(GamepadUIManager.instance.gamepadUI);
        //
        //
        touchDownTrigger.SetActive(false);
        GameObject networkManager = GameObject.Find("NetworkManager");
        //Destroy(networkManager);
        //StartCoroutine(ToggleEscMenu());
        TutorialEscMenuManager.instance.CloseEscMenu();
    }
    IEnumerator ToggleEscMenu()
    {
        TutorialEscMenuManager.instance.UpdateEscapeMenu();
        yield return new WaitForSeconds(0.1f);
        TutorialEscMenuManager.instance.UpdateEscapeMenu();
    }
    // Update is called once per frame
    void Update()
    {
        if (tutIndex == 0)
        {
            if (!index0Tracker)
            {
                StartCoroutine(TutorialDelay(5f, true));
                index0Tracker = true;
            }

        }
        else if (tutIndex == 1)
        {
            if (!index1Tracker)
                SpawnFirstGoblin();
        }
        else if (tutIndex == 2)
        {
            if (!index2Tracker)
                MovementInstructions();
        }
        else if (tutIndex == 3)
        {
            if (!index3Tracker)
                MoveToHighlightedArea();
        }
        else if (tutIndex == 4)
        {
            if (!index4Tracker)
                SprintInstructions();
        }
        else if (tutIndex == 5)
        {
            if (!index5Tracker)
                StaminaInstructions();
        }
        else if (tutIndex == 6)
        {
            if (!index6Tracker)
                FatigueInstructions();
        }
        else if (tutIndex == 7)
        {
            if (!index7Tracker)
                SpawnTeammates();
        }
        else if (tutIndex == 8)
        {
            if (!index8Tracker)
                SpawnFootball();
        }
        else if (tutIndex == 9)
        {
            if (!index9Tracker)
                PassBallInstructions();
        }
        else if (tutIndex == 10)
        {
            if (!index10Tracker)
                CanPassInstructions();
        }
        else if (tutIndex == 11)
        {
            if (!index11Tracker)
                SpawnGreyTeam();
        }
        else if (tutIndex == 12)
        {
            if (!index12Tracker)
                PunchPlayerGoblin();
        }
        else if (tutIndex == 13)
        {
            if (!index13Tracker)
                BlockAttackInstructions();
        }
        else if (tutIndex == 14)
        {
            if (!index14Tracker)
                KickBallDownFieldInstructions();
        }
        else if (tutIndex == 15)
        {
            if (!index15Tracker)
                KickBallDownFieldEnableControls();
        }
        else if (tutIndex == 16)
        {
            if (!index16Tracker)
                OpponentGetKickedBall();
        }
        else if (tutIndex == 17)
        {
            if (!index17Tracker)
                PunchOpponentGoblin();
        }
        else if (tutIndex == 18)
        {
            if (!index18Tracker)
                GetBallForSlideDemo();
        }
        else if (tutIndex == 19)
        {
            if (!index19Tracker)
                SlideInstructions();
        }
        else if (tutIndex == 20)
        {
            if (!index20Tracker)
                WhenYouCantSlideInstructions();
        }
        else if (tutIndex == 21)
        {
            if (!index21Tracker)
                PickUpBallToScoreTouchdown();
        }
        else if (tutIndex == 22)
        {
            if (!index22Tracker)
                PossessionBonusInstructions();
        }
        else if (tutIndex == 23)
        {
            if (!index23Tracker)
                PossessionBonusInstructionsPart2();
        }
        else if (tutIndex == 24)
        {
            if (!index24Tracker)
                RunForTouchdownAgain();
        }
        else if (tutIndex == 25)
        {
            if (!index25Tracker)
                PauseBeforePowerUpSpawn();
        }
        else if (tutIndex == 26)
        {
            if (!index26Tracker)
                SpawnPowerupInFrontOfPlayer();
        }
        else if (tutIndex == 27)
        {
            if (!index27Tracker)
                PowerUpInstructionsPart1();
        }
        else if (tutIndex == 28)
        {
            if (!index28Tracker)
                PowerUpInstructionsPart2();
        }
        else if (tutIndex == 29)
        {
            if (!index29Tracker)
                PowerUpInstructionsPart3();
        }
        else if (tutIndex == 30)
        {
            if (!index30Tracker)
                PowerUpInstructionsPart4();
        }
        else if (tutIndex == 31)
        {
            if (!index31Tracker)
                PowerUpInstructionsPart5();
        }
        else if (tutIndex == 32)
        {
            if (!index32Tracker)
                EnablePowerUpControlsForPlayer();
        }
        else if (tutIndex == 33)
        {
            if (!index33Tracker)
                PowerUpInstructionsPart6();
        }
        else if (tutIndex == 34)
        {
            if (!index34Tracker)
                PowerUpInstructionsPart7();
        }
        else if (tutIndex == 35)
        {
            if (!index35Tracker)
                StartRunningAgain();
        }
        else if (tutIndex == 36)
        {
            if (!index36Tracker)
                PauseBeforeObstacleSpawn();
        }
        else if (tutIndex == 37)
        {
            if (!index37Tracker)
                ObstacleInstructionsPart1();
        }
        else if (tutIndex == 38)
        {
            if (!index38Tracker)
                ObstacleInstructionsPart2();
        }
        else if (tutIndex == 39)
        {
            if (!index39Tracker)
                ObstacleInstructionsPart3();
        }
        else if (tutIndex == 40)
        {
            if (!index40Tracker)
                ObstacleInstructionsPart4();
        }
        else if (tutIndex == 41)
        {
            if (!index41Tracker)
                TouchdownInstructionsPart1();
        }
        else if (tutIndex == 42)
        {
            if (!index42Tracker)
                TouchdownInstructionsPart2();
        }
        else if (tutIndex == 43)
        {
            if (!index43Tracker)
                TouchdownMovement();
        }
        else if (tutIndex == 44)
        {
            if (!index44Tracker)
                TouchdownInstructionsPart3();
        }
        else if (tutIndex == 45)
        {
            if (!index45Tracker)
                KickAfterInstructionsPart1();
        }
        else if (tutIndex == 46)
        {
            if (!index46Tracker)
                KickAfterInstructionsPart2();
        }
        else if (tutIndex == 47)
        {
            if (!index47Tracker)
                KickAfterInstructionsPart3();
        }
        else if (tutIndex == 48)
        {
            if (!index48Tracker)
                KickAfterInstructionsPart4();
        }
        else if (tutIndex == 49)
        {
            if (!index49Tracker)
                KickAfterInstructionsPart5();
        }
        else if (tutIndex == 50)
        {
            if (!index50Tracker)
                KickAfterInstructionsPart6();
        }
        else if (tutIndex == 51)
        {
            if (!index51Tracker)
                KickAfterInstructionsPart7();
        }
        else if (tutIndex == 52)
        {
            if (!index52Tracker)
                KickAfterComplete();
        }
        else if (tutIndex == 53)
        {
            if (!index53Tracker)
                KickAfterCompletePart2();
        }
        else if (tutIndex == 54)
        {
            if (!index54Tracker)
                KickAfterCompletePart3();
        }
        else if (tutIndex == 55)
        {
            if (!index55Tracker)
                EndTheTutorial();
        }
    }
    public IEnumerator TutorialDelay(float delayTime, bool incrementTutIndex)
    {
        //Debug.Log("TutorialDelay called. The tutorial index is: " + tutIndex.ToString() + ". Delay for " + delayTime.ToString() + " and increment the index? " + incrementTutIndex.ToString());
        yield return new WaitForSeconds(delayTime);
        if (incrementTutIndex)
            tutIndex++;
    }
    void SpawnFirstGoblin()
    {
        if (index1Tracker)
            return;
        Debug.Log("SpawnFirstGoblin started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "This is your goblin!";
        if (greenGrenadierObject == null)
        {
            greenGrenadierObject = Instantiate(grenadierPrefb, firstGoblinPosition, Quaternion.identity);
            greenGrenadierScript = greenGrenadierObject.GetComponent<TutorialGoblinScript>();
            greenGrenadierObject.SetActive(true);
            greenGrenadierScript.SpawnQEandYouMarkers(true);
            greenGrenadierScript.HandleCharacterSelected(greenGrenadierScript.isCharacterSelected, true);
            greenGrenadierScript.isOwnedByTutorialPlayer = true;
            greenGrenadierScript.SetEGoblin(false);
            greenGrenadierScript.SetQGoblin(false);
            greenGrenadierScript.goblinType = "grenadier";
            //greenGrenadierScript.UpdateGamePadUIMarkersForGoblins(this.gamepadUI);
            tutorialPlayer.selectGoblin = greenGrenadierScript;
            greenGoblins.Add(greenGrenadierScript);
            tutorialPlayer.goblinTeam.Add(greenGrenadierScript);
            TutorialTeamManager.instance.greenTeam.goblins.Add(greenGrenadierScript);
        }
        
        StartCoroutine(TutorialDelay(3f, true));
        index1Tracker = true;
    }
    void MovementInstructions()
    {
        if (index2Tracker)
            return;
        Debug.Log("MovementInstructions started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Use the arrow keys or the right analog stick on a game pad to move";
        greenGrenadierScript.EnableMovementControls();
        tutorialPlayer.EnableGoblinMovement(true);
        tutorialPlayer.FollowSelectedGoblin(greenGrenadierObject.transform);
        //greenGrenadierScript.isCharacterSelected = true;
        index2Tracker = true;
        StartCoroutine(TutorialDelay(3f, true));
    }
    void MoveToHighlightedArea()
    {
        if (index3Tracker)
            return;
        Debug.Log("MoveToHighlightedArea started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Move your goblin to the highlighted area";
        Instantiate(moveToCicle, moveToCicleSpawnPosition, Quaternion.identity);
        index3Tracker = true;
    }
    public void PlayerIsInCircle()
    {
        Debug.Log("PlayerIsInCircle started. The tutorial index is: " + tutIndex.ToString());
        StartCoroutine(TutorialDelay(1f, true));
    }
    void SprintInstructions()
    {
        if (index4Tracker)
            return;
        Debug.Log("SprintInstructions started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "You can sprint to move faster by holding down shift or the left trigger";
        greenGrenadierScript.EnableSprintControls();
        StartCoroutine(SprintingCountdown());
        index4Tracker = true;
    }
    IEnumerator SprintingCountdown()
    {
        yield return new WaitForSecondsRealtime(5.5f);
        if (!doneSprinting)
        {
            PlayerSprintedToFatigue();
        }
    }
    public void PlayerSprintedToFatigue()
    {
        if (index5Tracker)
            return;
        if (!doneSprinting)
        {
            StartCoroutine(TutorialDelay(0.1f, true));
            doneSprinting = true;
        }
            
    }
    void StaminaInstructions()
    {
        if (index5Tracker)
            return;
        Debug.Log("StaminaInstructions started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Sprinting drains stamina. When you run out of stamina, you become fatigued, indicated by the sweat drop icon.";
        StartCoroutine(TutorialDelay(4f, true));
        index5Tracker = true;
    }
    void FatigueInstructions()
    {
        if (index6Tracker)
            return;
        Debug.Log("FatigueInstructions started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "When you are fatigued you move slower and you cannot sprint or dive until you have fully recovered your stamina.";
        StartCoroutine(TutorialDelay(4f, true));
        index6Tracker = true;
    }
    void SpawnTeammates()
    {
        if (index7Tracker)
            return;
        Debug.Log("SpawnTeammates started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "GRF is played in teams of 3. Use the Q/E buttons or Right/Left bumpers on a game pad to switch between goblins";

        // Spawn the berserker
        if (greenBerserkerObject == null)
        {
            greenBerserkerObject = Instantiate(berserkerPrefb, greenBerserkerPosition, Quaternion.identity);
            greenBerserkerScript = greenBerserkerObject.GetComponent<TutorialGoblinScript>();
            greenBerserkerObject.SetActive(true);
            greenBerserkerScript.SpawnQEandYouMarkers(true);
            greenBerserkerScript.HandleCharacterSelected(greenBerserkerScript.isCharacterSelected, false);
            greenBerserkerScript.isOwnedByTutorialPlayer = true;
            greenBerserkerScript.SetEGoblin(true);
            greenBerserkerScript.SetQGoblin(false);
            greenBerserkerScript.goblinType = "berserker";
            //greenBerserkerScript.UpdateGamePadUIMarkersForGoblins(this.gamepadUI);
            tutorialPlayer.eGoblin = greenBerserkerScript;
            greenGoblins.Add(greenBerserkerScript);
            tutorialPlayer.goblinTeam.Add(greenBerserkerScript);
            TutorialTeamManager.instance.greenTeam.goblins.Add(greenBerserkerScript);

        }


        // Spawn the skirmisher
        if (greenSkirmisherObject == null)
        {
            greenSkirmisherObject = Instantiate(skirmisherPrefb, greenskirmisherPosition, Quaternion.identity);
            greenSkirmisherScript = greenSkirmisherObject.GetComponent<TutorialGoblinScript>();
            greenSkirmisherObject.SetActive(true);
            greenSkirmisherScript.SpawnQEandYouMarkers(true);
            greenSkirmisherScript.HandleCharacterSelected(greenSkirmisherScript.isCharacterSelected, false);
            greenSkirmisherScript.isOwnedByTutorialPlayer = true;
            greenSkirmisherScript.SetEGoblin(false);
            greenSkirmisherScript.SetQGoblin(true);
            greenSkirmisherScript.goblinType = "skirmisher";
            //greenSkirmisherScript.UpdateGamePadUIMarkersForGoblins(this.gamepadUI);
            tutorialPlayer.qGoblin = greenSkirmisherScript;
            greenGoblins.Add(greenSkirmisherScript);
            tutorialPlayer.goblinTeam.Add(greenSkirmisherScript);
            TutorialTeamManager.instance.greenTeam.goblins.Add(greenSkirmisherScript);
        }

        tutorialPlayer.EnableQEControls();
        greenSkirmisherScript.EnableMovementControls();
        greenSkirmisherScript.EnableSprintControls();
        greenBerserkerScript.EnableMovementControls();
        greenBerserkerScript.EnableSprintControls();

        //StartCoroutine(TutorialDelay(4f, true));
        index7Tracker = true;
    }
    public void PlayerPressedQE()
    {
        if (index8Tracker || usedQE)
            return;
        StartCoroutine(TutorialDelay(2.5f, true));
        usedQE = true;
    }
    void SpawnFootball()
    {
        if (index8Tracker)
            return;
        Debug.Log("SpawnFootball started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Run over the football to pick it up.";
        GameObject newFootball = Instantiate(footballPrefab, GetFootballSpawnPosition(footballSpawnPosition), Quaternion.identity);
        footballScript = newFootball.GetComponent<TutorialFootball>();
        index8Tracker = true;
    }
    Vector3 GetFootballSpawnPosition(Vector3 positionToCheck)
    {
        Vector3 returnPosition = Vector3.zero;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(positionToCheck, 1.5f, goblinLayerMask);

        if (colliders.Length > 0)
        {
            Vector3 oppositeDirection = Vector3.zero;
            for (int i = 0; i < colliders.Length; i++)
            {
                Vector3 positionOfColliderThatWasHit = colliders[i].transform.position;

                float distance = Vector3.Distance(positionToCheck, positionOfColliderThatWasHit);
                if (distance <= 2.5f)
                {
                    Vector3 diff = positionToCheck - positionOfColliderThatWasHit;
                    diff.Normalize();
                    diff = diff / distance;
                    oppositeDirection += diff;
                }
            }
            returnPosition = positionToCheck + oppositeDirection;
            returnPosition.z = 0f;
        }
        else
            returnPosition = positionToCheck;

        return returnPosition;
    }
    public void PlayerPickedUpBall()
    {
        if (playerPickedUpBall)
        {
            return;
        }
        StartCoroutine(TutorialDelay(0.5f, true));
        playerPickedUpBall = true;
    }
    void PassBallInstructions()
    {
        if (index9Tracker)
            return;
        Debug.Log("PassBallInstructions started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Q/E or Rb/Lb will pass the ball. You can only pass backwards.";
        index9Tracker = true;
    }
    public void FirstThrownPass()
    {
        if (firstThrownPass)
            return;
        if (!index9Tracker)
            return;
        StartCoroutine(TutorialDelay(1.25f, true));
        firstThrownPass = true;
    }
    void CanPassInstructions()
    {
        if (index10Tracker)
            return;
        Debug.Log("CanPassInstructions started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Black and white icons mean you can pass/switch. Grey and Red mean you CANNOT pass/switch.";
        StartCoroutine(TutorialDelay(3.5f, true));
        index10Tracker = true;
    }
    void SpawnGreyTeam()
    {
        if (index11Tracker)
            return;
        Debug.Log("SpawnGreyTeam started. The tutorial index is: " + tutIndex.ToString());

        // Disable player controls of goblins
        tutorialPlayer.EnableQESwitchingControls(false);
        tutorialPlayer.qeSwitchingControlsOnServer = false;
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;

        // Move the green goblins to specified locations
        greenGrenadierObject.transform.position = greenGrenadierNewLocation;
        greenGrenadierScript.FlipRenderer(false);
        greenBerserkerObject.transform.position = greenBerserkerNewLocation;
        greenBerserkerScript.FlipRenderer(false);
        greenSkirmisherObject.transform.position = greenSkirmisherNewLocation;
        greenSkirmisherScript.FlipRenderer(false);

        // Make sure the ball is held by the green grenadier. If it is not, move the ball to the grenadier
        MoveFootballToGreenGrenadier();

        // Spawn the grey goblins
        if (greyGrenadierObject == null)
        {
            greyGrenadierObject = Instantiate(grenadierPrefb, greyGrenadierSpawnLocation, Quaternion.identity);
            greyGrenadierScript = greyGrenadierObject.GetComponent<TutorialGoblinScript>();
            greyGrenadierObject.SetActive(true);
            greyGrenadierScript.SpawnQEandYouMarkers(false);
            greyGrenadierScript.HandleCharacterSelected(greenSkirmisherScript.isCharacterSelected, false);
            greyGrenadierScript.isOwnedByTutorialPlayer = false;
            greyGrenadierScript.myGamePlayer = secondTutorialPlayer;
            greyGrenadierScript.FlipRenderer(true);
            greyGrenadierScript.goblinType = "grenadier-grey";
            greyGrenadierScript.GetSpawnedFootball();
            greyGoblins.Add(greyGrenadierScript);
            greyGrenadierScript.HandleIsGoblinGrey(greyGrenadierScript.isGoblinGrey, true);
            secondTutorialPlayer.goblinTeam.Add(greyGrenadierScript);
            TutorialTeamManager.instance.greyTeam.goblins.Add(greyGrenadierScript);
        }
        if (greyBerserkerObject == null)
        {
            greyBerserkerObject = Instantiate(berserkerPrefb, greyBerserkerSpawnLocation, Quaternion.identity);
            greyBerserkerScript = greyBerserkerObject.GetComponent<TutorialGoblinScript>();
            greyBerserkerObject.SetActive(true);
            greyBerserkerScript.SpawnQEandYouMarkers(false);
            greyBerserkerScript.HandleCharacterSelected(greenSkirmisherScript.isCharacterSelected, false);
            greyBerserkerScript.isOwnedByTutorialPlayer = false;
            greyBerserkerScript.myGamePlayer = secondTutorialPlayer;
            greyBerserkerScript.FlipRenderer(true);
            greyBerserkerScript.goblinType = "berserker-grey";
            greyBerserkerScript.GetSpawnedFootball();
            greyGoblins.Add(greyBerserkerScript);
            greyBerserkerScript.HandleIsGoblinGrey(greyBerserkerScript.isGoblinGrey, true);
            secondTutorialPlayer.goblinTeam.Add(greyBerserkerScript);
            TutorialTeamManager.instance.greyTeam.goblins.Add(greyBerserkerScript);
        }
        if (greySkirmisherObject == null)
        {
            greySkirmisherObject = Instantiate(skirmisherPrefb, greySkirmisherSpawnLocation, Quaternion.identity);
            greySkirmisherScript = greySkirmisherObject.GetComponent<TutorialGoblinScript>();
            greySkirmisherObject.SetActive(true);
            greySkirmisherScript.SpawnQEandYouMarkers(false);
            greySkirmisherScript.HandleCharacterSelected(greenSkirmisherScript.isCharacterSelected, false);
            greySkirmisherScript.isOwnedByTutorialPlayer = false;
            greySkirmisherScript.myGamePlayer = secondTutorialPlayer;
            greySkirmisherScript.FlipRenderer(true);
            greySkirmisherScript.goblinType = "skirmisher-grey";
            greySkirmisherScript.GetSpawnedFootball();
            greyGoblins.Add(greySkirmisherScript);
            greySkirmisherScript.HandleIsGoblinGrey(greySkirmisherScript.isGoblinGrey, true);
            secondTutorialPlayer.goblinTeam.Add(greySkirmisherScript);
            TutorialTeamManager.instance.greyTeam.goblins.Add(greySkirmisherScript);
        }
        messageBoardTopText.text = "The opposing team will try to get possession of the ball from you. By force, if necessary.";
        StartCoroutine(TutorialDelay(3.5f, true));
        index11Tracker = true;
    }
    void MoveFootballToGreenGrenadier()
    {
        if (greenGrenadierScript.doesCharacterHaveBall)
            return;
        else
        {
            footballScript.MoveFootballToGoblin(greenGrenadierScript);
        }
    }
    void PunchPlayerGoblin()
    {
        if (index12Tracker)
            return;
        Debug.Log("PunchPlayerGoblin started. The tutorial index is: " + tutIndex.ToString());

        // Move the grey grenadier toward the green grenadier. Have the grey punch the green one once or twice
        greyGrenadierScript.PunchPlayerGoblin(greenGrenadierScript, 1);

        index12Tracker = true;
    }
    public void DonePunchingPlayerGoblin()
    {
        if (donePunchingPlayerGoblin)
            return;
        if (!index12Tracker)
            return;
        StartCoroutine(TutorialDelay(0.25f, true));
        donePunchingPlayerGoblin = true;

    }
    void BlockAttackInstructions()
    {
        if (index13Tracker)
            return;
        Debug.Log("BlockAttackInstructions started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Hold down \"D\" on keyboard or B on a gamepad to block. Blocking reduces damage taken by half, but causes you to move 20% slower.";
        //StartCoroutine(TutorialDelay(4.0f, true));
        greyGrenadierScript.punchPlayerGoblin = false;
        tutorialPlayer.EnableBlockingControls();
        index13Tracker = true;
    }
    public void PlayerBlocked()
    {
        if (playerBlocked)
            return;
        StartCoroutine(TutorialDelay(3.5f, true));
        playerBlocked = true;
    }
    void KickBallDownFieldInstructions()
    {
        if (index14Tracker)
            return;
        Debug.Log("KickBallDownFieldInstructions started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "When surrounded by opponents, kicking the ball downfield may be a good strategy. Use \"TAB\" or Right trigger to kick. Hold down the button to get your kick power, and release to submit power and start the kick.";
        donePunchingPlayerGoblin = false;
        greyGrenadierScript.PunchPlayerGoblin(greenGrenadierScript, 1);
        index14Tracker = true;
    }
    void KickBallDownFieldEnableControls()
    {
        if (index15Tracker)
            return;
        Debug.Log("KickBallDownFieldEnableControls started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "When surrounded by opponents, kicking the ball downfield may be a good strategy. Use \"TAB\" or Right trigger to kick. Hold down the button to get your kick power, and release to submit power and start the kick.";
        // Enable kicking controls for the player
        tutorialPlayer.EnableKickingControlsFirstTime();
        index15Tracker = true;
    }
    public void FirstKickDownField()
    {
        if (firstKickDownField)
            return;
        if (!index15Tracker)
            return;
        StartCoroutine(TutorialDelay(0.1f, true));
        tutorialPlayer.ActivateKickingControls(false);
        tutorialPlayer.kickingControlsOnServer = false;
        firstKickDownField = true;
    }
    void OpponentGetKickedBall()
    {
        if (index16Tracker)
            return;
        Debug.Log("OpponentGetKickedBall started. The tutorial index is: " + tutIndex.ToString());
        greySkirmisherScript.StartChasingBall();
        index16Tracker = true;
    }
    public void OpponentPickedUpBall(TutorialGoblinScript goblin)
    {
        if (opponentPickedUpBall)
            return;
        if (!index16Tracker)
            return;
        goblin.chaseKickedBall = false;
        StartCoroutine(TutorialDelay(0.5f, true));
        opponentPickedUpBall = true;
    }
    void PunchOpponentGoblin()
    {
        if (index17Tracker)
            return;
        Debug.Log("PunchOpponentGoblin started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Chase down the opponent and use the 'A' keyboard key or the X button on a gamepad to punch them until they fumble the ball.";
        // Enable movement for the player
        tutorialPlayer.EnableGoblinMovement(true);
        tutorialPlayer.goblinMovementControlsOnServer = true;
        // Enable punching for the player
        tutorialPlayer.EnableAttackControls();

        index17Tracker = true;
    }
    public void PlayerCausedFumble()
    {
        if (playerCausedFumble)
            return;
        if (!index17Tracker)
            return;
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        tutorialPlayer.ActivateAttackControls(false);
        StartCoroutine(TutorialDelay(0.5f, true));
        playerCausedFumble = true;
    }
    void GetBallForSlideDemo()
    {
        if (index18Tracker)
            return;
        Debug.Log("GetBallForSlideDemo started. The tutorial index is: " + tutIndex.ToString());
        this.opponentPickedUpBall = false;
        greySkirmisherScript.StartChasingBall();
        index18Tracker = true;
    }
    void SlideInstructions()
    {
        if (index19Tracker)
            return;
        Debug.Log("GetBallForSlideDemo started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "While moving, use 'S' on keyboard or 'A' on gamepad to slide tackle. This will trip opposing goblins and cause fumbles.";
        tutorialPlayer.EnableGoblinMovement(true);
        tutorialPlayer.goblinMovementControlsOnServer = true;
        tutorialPlayer.ActivateAttackControls(false);
        tutorialPlayer.EnableSlideControls();
        index19Tracker = true;
    }
    public void OpponentFumbledFromSlide()
    {
        if (opponentFumbledFromSlide)
            return;
        if (!index19Tracker)
            return;
        Debug.Log("OpponentFumbledFromSlide started. The tutorial index is: " + tutIndex.ToString());
        StartCoroutine(TutorialDelay(0.5f, true));
        opponentFumbledFromSlide = true;
    }
    void WhenYouCantSlideInstructions()
    {
        if (index20Tracker)
            return;
        Debug.Log("WhenYouCantSlideInstructions started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Note: You CANNOT punch or slide when you have the ball.";
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        tutorialPlayer.ActivateAttackControls(false);
        tutorialPlayer.ActivateSlideControls(false);
        StartCoroutine(TutorialDelay(2.5f, true));
        index20Tracker = true;
    }
    void PickUpBallToScoreTouchdown()
    {
        if (index21Tracker)
            return;
        Debug.Log("WhenYouCantSlideInstructions started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Pick up the ball again and run toward the grey endzone (to the right) to score a touchdown!";
        greySkirmisherScript.rb.bodyType = RigidbodyType2D.Kinematic;
        tutorialPlayer.EnableGoblinMovement(true);
        tutorialPlayer.goblinMovementControlsOnServer = true;
        index21Tracker = true;
    }
    public void UpdatePossessionBar(bool isGreen, float possession)
    {
        if (isGreen)
            PossessionBarGreen.GetComponent<PossessionBar>().UpdatePossessionBar(possession);
        else
            PossessionBarGrey.GetComponent<PossessionBar>().UpdatePossessionBar(possession);
    }
    public void UpdatePossessionOfFootballtoTeam(bool doesGreyTeamHaveBall)
    {
        Debug.Log("UpdatePossessionOfFootballtoTeam: Does grey team have the ball? " + doesGreyTeamHaveBall.ToString());
        if (doesGreyTeamHaveBall)
        {
            PossessionFootballGrey.SetActive(true);
            PossessionFootballGreen.SetActive(false);
        }
        else
        {
            PossessionFootballGrey.SetActive(false);
            PossessionFootballGreen.SetActive(true);
        }
    }
    public void NoTeamWithFootball()
    {
        PossessionFootballGrey.SetActive(false);
        PossessionFootballGreen.SetActive(false);
    }
    public void PlayerPickedUpBallAfterSlideTackle()
    {
        if (playerPickedUpBallAfterSlideTackle)
            return;
        if (!index21Tracker)
            return;
        StartCoroutine(TutorialDelay(0.5f, true));
        playerPickedUpBallAfterSlideTackle = true;
    }
    void PossessionBonusInstructions()
    {
        if (index22Tracker)
            return;
        Debug.Log("PossessionBonusInstructions started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "This is the possession meter. The longer you hold onto the ball, the greater your 'possession bonus' will be. Your possession bonus lowers when you don't have the ball.";
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        PossessionBarMarker.SetActive(true);
        StartCoroutine(TutorialDelay(7.0f, true));
        index22Tracker = true;
    }
    void PossessionBonusInstructionsPart2()
    {
        if (index23Tracker)
            return;
        Debug.Log("PossessionBonusInstructions started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Possession bonus will cause you to run and heal faster, and take less damage. The color of the possession bar changes depending on your bonus level.";
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        StartCoroutine(TutorialDelay(6.0f, true));
        index23Tracker = true;
    }
    void RunForTouchdownAgain()
    {
        if (index24Tracker)
            return;
        Debug.Log("RunForTouchdownAgain started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "With that out of the way, run for a touchdown again!";
        tutorialPlayer.EnableGoblinMovement(true);
        tutorialPlayer.goblinMovementControlsOnServer = true;
        PossessionBarMarker.SetActive(false);
        StartCoroutine(TutorialDelay(2.0f, true));
        index24Tracker = true;
    }
    void PauseBeforePowerUpSpawn()
    {
        if (index25Tracker)
            return;
        Debug.Log("PauseBeforePowerUpSpawn started. The tutorial index is: " + tutIndex.ToString());
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        StartCoroutine(TutorialDelay(2.0f, true));
        index25Tracker = true;
    }
    void SpawnPowerupInFrontOfPlayer()
    {
        if (index26Tracker)
            return;
        Debug.Log("SpawnPowerupInFrontOfPlayer started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Before we forget: Power ups!";
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        StartCoroutine(TutorialDelay(2.0f, true));
        index26Tracker = true;
        SpawnPowerUpItem();
    }
    void SpawnPowerUpItem()
    {
        if (!index26Tracker)
            return;
        Vector3 powerUpLocation = tutorialPlayer.selectGoblin.transform.position;
        powerUpLocation.x += 3.0f;
        if (powerUpLocation.x > maxX)
            powerUpLocation.x = maxX;
        else if (powerUpLocation.x < minX)
            powerUpLocation.x = minX;

        // Check if power up is spawning too close to another goblin that might pick it up?
        Collider2D[] colliders = Physics2D.OverlapCircleAll(powerUpLocation, 2.0f, goblinLayerMask);
        bool powerUpSpawnedOnGoblin = false;
        Vector3 oppositeDirection = Vector3.zero;
        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                Vector3 positionOfColliderThatWasHit = colliders[i].transform.position;
                if (colliders[i].transform == this.transform)
                    continue;

                float distance = Vector2.Distance(powerUpLocation, positionOfColliderThatWasHit);
                if (distance <= 2.0f)
                {
                    Vector3 diff = powerUpLocation - positionOfColliderThatWasHit;
                    diff.Normalize();
                    diff = diff / distance;
                    oppositeDirection += diff;
                    powerUpSpawnedOnGoblin = true;
                }
            }
        }
        if (powerUpSpawnedOnGoblin)
        {
            powerUpLocation += oppositeDirection;
            powerUpLocation.x += 1.5f;

            if (powerUpLocation.x > maxX)
                powerUpLocation.x = maxX;
            else if (powerUpLocation.x < minX)
                powerUpLocation.x = minX;
            if (powerUpLocation.y > maxY)
                powerUpLocation.y = maxY;
            else if (powerUpLocation.y < minY)
                powerUpLocation.y = minY;

        }

        // Finally, spawn the powerup
        powerUpObject = Instantiate(powerUpPrefab, powerUpLocation, Quaternion.identity);
    }
    public void UpdatePowerUpUIImages(List<TutorialPowerUp> myPowerUps)
    {
        Debug.Log("UpdatePowerUpUIImages");
        if (myPowerUps.Count <= 0)
        {
            PowerUp1Image.GetComponent<Image>().sprite = EmptyPowerUpImage;
            PowerUp2Image.GetComponent<Image>().sprite = EmptyPowerUpImage;
            PowerUp3Image.GetComponent<Image>().sprite = EmptyPowerUpImage;
            PowerUp4Image.GetComponent<Image>().sprite = EmptyPowerUpImage;
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            if ((i + 1) > myPowerUps.Count)
            {
                PowerUpUIImages[i].GetComponent<Image>().sprite = EmptyPowerUpImage;
                PowerUpUIRemainingUses[i].SetActive(false);
            }
            else
            {
                PowerUpUIImages[i].GetComponent<Image>().sprite = myPowerUps[i].mySprite;
                if (myPowerUps[i].multipleUses)
                {
                    PowerUpUIRemainingUses[i].GetComponent<TextMeshProUGUI>().text = myPowerUps[i].remainingUses.ToString();
                    PowerUpUIRemainingUses[i].SetActive(true);
                }
                else
                {
                    PowerUpUIRemainingUses[i].SetActive(false);
                }
            }
        }
    }
    void PowerUpInstructionsPart1()
    {
        if (index27Tracker)
            return;
        Debug.Log("PowerUpInstructionsPart1 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Power ups are spawned randomly and can be picked up by goblins that run over them.";
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        StartCoroutine(TutorialDelay(4.0f, true));
        index27Tracker = true;
    }
    void PowerUpInstructionsPart2()
    {
        if (index28Tracker)
            return;
        Debug.Log("PowerUpInstructionsPart2 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Pick up the power up with your goblin by running over to it.";
        tutorialPlayer.EnableGoblinMovement(true);
        tutorialPlayer.goblinMovementControlsOnServer = true;
        index28Tracker = true;
    }
    public void PlayerPickedUpPowerUp()
    {
        if (playerPickedUpPowerUp)
            return;
        if (!index28Tracker)
            return;
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        StartCoroutine(TutorialDelay(2.0f, true));
        playerPickedUpPowerUp = true;
    }
    void PowerUpInstructionsPart3()
    {
        if (index29Tracker)
            return;
        Debug.Log("PowerUpInstructionsPart3 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Collected power ups are displayed in your power up inventory bar.";
        powerUpMarker.SetActive(true);
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        StartCoroutine(TutorialDelay(4.0f, true));
        index29Tracker = true;
    }
    void PowerUpInstructionsPart4()
    {
        if (index30Tracker)
            return;
        Debug.Log("PowerUpInstructionsPart4 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "To use a power up, press the number key (1,2,3,4) or d-pad key (<,^,>,v) displayed below the power up in your inventory bar.";
        powerUpMarker.SetActive(true);
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        StartCoroutine(TutorialDelay(4.0f, true));
        index30Tracker = true;
    }
    void PowerUpInstructionsPart5()
    {
        if (index31Tracker)
            return;
        Debug.Log("PowerUpInstructionsPart5 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Use your one and only power up now.";
        powerUpMarker.SetActive(true);
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        StartCoroutine(TutorialDelay(0.5f, true));
        index31Tracker = true;

    }
    void EnablePowerUpControlsForPlayer()
    {
        if (index32Tracker)
            return;
        Debug.Log("EnablePowerUpControlsForPlayer started. The tutorial index is: " + tutIndex.ToString());
        tutorialPlayer.EnablePowerUpControls();
        index32Tracker = true;
    }
    public void PlayerUsedPowerUp()
    {
        if (playerUsedPowerUp)
            return;
        if (!index32Tracker)
            return;
        StartCoroutine(TutorialDelay(1.5f, true));
        playerUsedPowerUp = true;
    }
    void PowerUpInstructionsPart6()
    {
        if (index33Tracker)
            return;
        Debug.Log("PowerUpInstructionsPart6 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "The power up you used is the 'defense' power up that reduces damage to your goblin and every other goblin on your team.";
        powerUpMarker.SetActive(false);
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        tutorialPlayer.ActivatePowerUpControls(false);
        tutorialPlayer.powerupsControlsOnServer = false;
        StartCoroutine(TutorialDelay(5f, true));
        index33Tracker = true;
    }
    void PowerUpInstructionsPart7()
    {
        if (index34Tracker)
            return;
        Debug.Log("PowerUpInstructionsPart7 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "A particle effect will activate on every goblin that receives the power up effect.";
        powerUpMarker.SetActive(false);
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        tutorialPlayer.ActivatePowerUpControls(false);
        tutorialPlayer.powerupsControlsOnServer = false;
        StartCoroutine(TutorialDelay(5f, true));
        index34Tracker = true;
    }
    void StartRunningAgain()
    {
        if (index35Tracker)
            return;
        Debug.Log("StartRunningAgain started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Now that you know about power ups, run for a touchdown again!";
        tutorialPlayer.EnableGoblinMovement(true);
        tutorialPlayer.goblinMovementControlsOnServer = true;
        StartCoroutine(TutorialDelay(1.5f, true));
        index35Tracker = true;
    }
    public void UsePowerUp(TutorialPowerUp powerUpToUseScript)
    {
        if (tutorialPlayer.myPowerUps.Contains(powerUpToUseScript))
        {
            Debug.Log("PowerUpManager PlayerUsePowerUp: ");
            //GameObject powerUpToUse = NetworkIdentity.spawned[powerUpNetId].gameObject;
            //PowerUp powerUpToUseScript = powerUpToUse.GetComponent<PowerUp>();

            //bool canPlayerUsePowerUp = powerUpToUse.GetComponent<PowerUp>().UsePowerUp();
            bool canPlayerUsePowerUp = powerUpToUseScript.UsePowerUp();

            if (canPlayerUsePowerUp)
            {
                powerUpToUseScript.remainingUses--;
                if (powerUpToUseScript.remainingUses <= 0)
                {
                    Debug.Log("PlayerUsePowerUp: Player successfully used their powerup. Removing the powerup from the game.");

                    //tutorialPlayer.myPowerUps.Remove(powerUpToUseScript);

                    Destroy(powerUpToUseScript.gameObject);
                    // Track when a team has picked up a power up
                    //TeamManager.instance.PowerUpCollectedOrUsed(player, true);
                }
                else
                {
                    Debug.Log("PlayerUsePowerUp: Player successfully used their powerup. PowerUp still has remaining uses: " + powerUpToUseScript.remainingUses.ToString());
                }

                //player.RpcRemoveUsedPowerUp(player.connectionToClient, powerUpNetId);


            }
            else
            {
                Debug.Log("PlayerUsePowerUp: Player WAS UNABLE TO use their powerup. PowerUp WILL NOT be removed from the game.");
            }
        }
    }
    void PauseBeforeObstacleSpawn()
    {
        if (index36Tracker)
            return;
        Debug.Log("PauseBeforeObstacleSpawn started. The tutorial index is: " + tutIndex.ToString());
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        SpawnObstacles();
        StartCoroutine(TutorialDelay(2.0f, true));
        index36Tracker = true;
    }
    void ObstacleInstructionsPart1()
    {
        if (index37Tracker)
            return;
        Debug.Log("ObstacleInstructionsPart1 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "On the field will be 'obstacles' that you want to avoid, such as rocks and brush.";
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        StartCoroutine(TutorialDelay(5.5f, true));
        index37Tracker = true;
    }
    void SpawnObstacles()
    {
        Debug.Log("SpawnObstacles started. The tutorial index is: " + tutIndex.ToString());
        Vector3 playerLocation = tutorialPlayer.selectGoblin.transform.position;
        playerLocation.x += 8f;
        Vector3 rockObstacleLocation = playerLocation;
        rockObstacleLocation.x += UnityEngine.Random.Range(0f,2.0f);
        rockObstacleLocation.y = 3f;
        Vector3 brushObstacleLocation = playerLocation;
        brushObstacleLocation.x += UnityEngine.Random.Range(0f, 2.0f);
        brushObstacleLocation.y = -5.5f;

        rockObstacleObject = Instantiate(rockObstaclePrefab, rockObstacleLocation, Quaternion.identity);
        brushObstacleObject = Instantiate(brushObstaclePrefab, brushObstacleLocation, Quaternion.identity);
    }
    void ObstacleInstructionsPart2()
    {
        if (index38Tracker)
            return;
        Debug.Log("ObstacleInstructionsPart2 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "'Hard' obstacles, like rocks and logs, will trip your goblin if you run into them.";
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        StartCoroutine(TutorialDelay(5.5f, true));
        index38Tracker = true;
    }
    void ObstacleInstructionsPart3()
    {
        if (index39Tracker)
            return;
        Debug.Log("ObstacleInstructionsPart3 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "'Soft' obstacles, like brush and water puddles, will make your goblin run slower if you run over them.";
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        StartCoroutine(TutorialDelay(5.5f, true));
        index39Tracker = true;
    }
    void ObstacleInstructionsPart4()
    {
        if (index40Tracker)
            return;
        Debug.Log("ObstacleInstructionsPart4 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Now run for the endzone again while dodging the obstacles! Or run into them, if you want to. Remember to pick the ball back up!";
        tutorialPlayer.EnableGoblinMovement(true);
        tutorialPlayer.goblinMovementControlsOnServer = true;
        touchDownTrigger.SetActive(true);
        index40Tracker = true;
    }
    public void PlayerIsNearEndzone()
    {
        if (!index40Tracker)
            return;
        if (playerIsNearEndZone)
            return;
        touchDownTrigger.SetActive(false);
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        StartCoroutine(TutorialDelay(1.0f, true));
        playerIsNearEndZone = true;
    }
    void TouchdownInstructionsPart1()
    {
        if (index41Tracker)
            return;
        Debug.Log("TouchdownInstructionsPart1 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "You're near the endzone! An IMPORTANT thing to know about scoring touchdowns in GRF: You have to touch the ball to the ground!";
        StartCoroutine(TutorialDelay(5.5f, true));
        index41Tracker = true;
    }
    void TouchdownInstructionsPart2()
    {
        if (index42Tracker)
            return;
        Debug.Log("TouchdownInstructionsPart2 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "To touch the ball down, dive in the endzone using 'W' on the keyboard or 'Y' on the gamepad. Give it a try!";
        StartCoroutine(TutorialDelay(1.0f, true));
        index42Tracker = true;
    }
    void TouchdownMovement()
    {
        if (index43Tracker)
            return;
        Debug.Log("TouchdownMovement started. The tutorial index is: " + tutIndex.ToString());
        tutorialPlayer.EnableGoblinMovement(true);
        tutorialPlayer.goblinMovementControlsOnServer = true;
        tutorialPlayer.EnableDiveAction();
        index43Tracker = true;
    }
    public void PlayerScoredTouchdown()
    {
        if (playerScoredTouchdown)
            return;
        if (!index43Tracker)
            return;
        Debug.Log("PlayerScoredTouchdown started. The tutorial index is: " + tutIndex.ToString());
        tutorialPlayer.EnableGoblinMovement(false);
        tutorialPlayer.goblinMovementControlsOnServer = false;
        tutorialPlayer.EnableGameplayActions(false);
        StartCoroutine(TutorialDelay(0.5f, true));

        yPositionOfKickAfter = greenGrenadierScript.transform.position.y;
        HandleGreenScoreUpdate(greenScore, (greenScore + 5));

        playerScoredTouchdown = true;

        StartCoroutine(TouchdownScoredUI());
    }
    void HandleGreenScoreUpdate(int oldValue, int newValue)
    {
        greenScore = newValue;
        ScoreGreenText.text = newValue.ToString("00");
    }
    void TouchdownInstructionsPart3()
    {
        if (index44Tracker)
            return;
        Debug.Log("TouchdownInstructionsPart3 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "You scored a touchdown! Congratulations!!! In GRF, a touchdown is worth 5 (count em) points.";
        StartCoroutine(TutorialDelay(5.5f, true));
        index44Tracker = true;
    }
    void KickAfterInstructionsPart1()
    {
        if (index45Tracker)
            return;
        Debug.Log("KickAfterInstructionsPart1 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "After you score a touchdown, there is a kick after attempt for 2 points. Note where your goblin is right now.";
        StartCoroutine(TutorialDelay(5.5f, true));
        index45Tracker = true;
    }
    void KickAfterInstructionsPart2()
    {
        if (index46Tracker)
            return;
        Debug.Log("KickAfterInstructionsPart2 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Where your kick after attempt is done from is based on where you touch the ball down. You will kick directly back from where you touched the ball down.";

        // Move the grenadier goblin to the kick after position?
        Destroy(rockObstacleObject);
        Destroy(brushObstacleObject);
        tutorialPlayer.RepositionForKickAfter(true, greenGrenadierScript, yPositionOfKickAfter);
        secondTutorialPlayer.RepositionForKickAfter(false, greenGrenadierScript, yPositionOfKickAfter);
        tutorialPlayer.EnableGameplayActions(false);
        StartCoroutine(TutorialDelay(7.5f, true));
        index46Tracker = true;
    }
    void KickAfterInstructionsPart3()
    {
        if (index47Tracker)
            return;
        Debug.Log("KickAfterInstructionsPart3 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "Move side to side with left/right arrow keys on the keyboard or Rb/Lb on a gamepad.";

        // Enable kick after positioning controls? Also, turn on the UI?
        this.gamePhase = "kick-after-attempt";
        
        tutorialPlayer.EnableKickAfterPositioningControls();
        ActivateKickAfterPositionControlsPanel(true);

        StartCoroutine(TutorialDelay(4.0f, true));
        index47Tracker = true;
    }
    public void PlayerChangedKickAfterPosition()
    {
        if (playerChangedKickAfterPosition)
            return;
        if (!index47Tracker)
            return;
        //StartCoroutine(TutorialDelay(2.5f, true));
        playerChangedKickAfterPosition = true;
    }
    void KickAfterInstructionsPart4()
    {
        if (index48Tracker)
            return;
        Debug.Log("KickAfterInstructionsPart4 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "The bar beneath your goblin is the 'Accuracy Difficulty Bar.' The closer together the two yellow lines, the harder the kick after attempt will be. Kicking from the middle of the field will be easier than kicking from near the sidelines.";

        StartCoroutine(TutorialDelay(7.5f, true));
        index48Tracker = true;
    }
    void KickAfterInstructionsPart5()
    {
        if (index49Tracker)
            return;
        Debug.Log("KickAfterInstructionsPart4 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "When you are at the position you want to kick from, press 'ENTER' on your keyboard or 'A' on a gamepad.";
        tutorialPlayer.EnableKickAfterSubmissionControls();
        index49Tracker = true;
    }
    public void ActivateKickAfterPositionControlsPanel(bool activate)
    {
        KickAfterPositionControlsPanel.SetActive(activate);
    }
    public void DisableKickAfterPositioningControls()
    {
        if (playerSubmittedKickAfterPosition)
            return;
        if (!index49Tracker)
            return;
        tutorialPlayer.EnableKickAfterPositioning(false);
        StartCoroutine(TutorialDelay(1.0f, true));
        playerSubmittedKickAfterPosition = true;
    }
    void KickAfterInstructionsPart6()
    {
        if (index50Tracker)
            return;
        Debug.Log("KickAfterInstructionsPart6 started. The tutorial index is: " + tutIndex.ToString());
        messageBoardTopText.text = "You're about to kick! First, you will submit your accuracy by pressing 'TAB' or Right Trigger. Try to submit when the moving red bar is between the yellow bars.";
        greenGrenadierScript.isGoblinDoingKickAfterAttempt = true;
        tutorialPlayer.EnableKickAfterKickingControls();
        tutorialPlayer.StartKickAfterAttempt();
        
        index50Tracker = true;
    }
    public void PlayerSubmittedKickAfterAccuracy()
    {
        if (playerSubmittedKickAfterAccuracy)
            return;
        if (!index50Tracker)
            return;
        StartCoroutine(TutorialDelay(0.5f, true));
        playerSubmittedKickAfterAccuracy = true;
    }
    void KickAfterInstructionsPart7()
    {
        if (!playerSubmittedKickAfterAccuracy)
            return;
        if (index51Tracker)
            return;
        messageBoardTopText.text = "You submitted your accuracy! Now submit your kick power. Hope it's enough to make it through the uprights. No penalty for kicking it too hard!";
        index51Tracker = true;
    }
    public void KickAfterWasKickGoodOrBad(bool isKickGood)
    {
        Debug.Log("KickAfterWasKickGoodOrBad: Was the kick after attempt good? " + isKickGood.ToString());
        bool isScoringPlayerGrey = false;
        tutorialPlayer.EnableKickAfterKicking(false);

        if (isKickGood)
        {
            HandleGreenScoreUpdate(greenScore, (greenScore + 2));
        }
        RpcKickAfterWasKickGoodOrBad(isKickGood, isScoringPlayerGrey);
        StartCoroutine(TutorialDelay(3.0f, true));
    }
    void RpcKickAfterWasKickGoodOrBad(bool isKickGood, bool isScoringPlayerGrey)
    {
        KickAfterPositionControlsPanel.SetActive(false);
        KickAfterTimerBeforeKickPanel.SetActive(false);
        KickAfterWasKickGoodPanel.SetActive(true);
        TheKickWasText.gameObject.SetActive(true);
        wasKickGood = isKickGood;
        if (isKickGood)
        {
            KickWasNotGoodText.gameObject.SetActive(false);
            KickWasGoodText.gameObject.SetActive(true);
            KickWasGoodText.GetComponent<TouchDownTextGradient>().ActivateGradient();
            SoundManager.instance.PlaySound("kick-its-good", 1.0f);
            SoundManager.instance.PlaySound("touchdown-cheer", 0.75f);
        }
        else
        {
            KickWasNotGoodText.gameObject.SetActive(true);
            KickWasGoodText.gameObject.SetActive(false);
            KickWasNotGoodText.GetComponent<TouchDownTextGradient>().SetGreenOrGreyColor(isScoringPlayerGrey);
            SoundManager.instance.PlaySound("kick-no-good", 1.0f);
            SoundManager.instance.PlaySound("touchdown-cheer", 0.75f);
        }
    }
    void KickAfterComplete()
    {
        if (index52Tracker)
            return;
        KickAfterWasKickGoodPanel.SetActive(false);
        TheKickWasText.gameObject.SetActive(false);
        KickWasNotGoodText.gameObject.SetActive(false);
        KickWasGoodText.gameObject.SetActive(false);
        KickWasBlockedText.gameObject.SetActive(false);
        KickWasGoodText.GetComponent<TouchDownTextGradient>().isColorChangeRunning = false;
        messageBoardTopText.text = "Your kick was ";
        if (wasKickGood)
        {
            messageBoardTopText.text += "GOOD!!! Congratulations!";
        }
        else
        {
            messageBoardTopText.text += "bad... sorry....";
        }
        StartCoroutine(TutorialDelay(3.0f, true));
        index52Tracker = true;
    }
    void KickAfterCompletePart2()
    {
        if (index53Tracker)
            return;
        messageBoardTopText.text = "It's easier when no one is trying to block your kick, unlike in a real game of GRF.";

        StartCoroutine(TutorialDelay(4.5f, true));
        index53Tracker = true;
    }
    void KickAfterCompletePart3()
    {
        if (index54Tracker)
            return;
        messageBoardTopText.text = "That's it for the tutorial. I hope you learned something. Good luck out there!";
        SteamAchievementManager.instance.TutorialCompleted();

        StartCoroutine(TutorialDelay(5.0f, true));
        index54Tracker = true;
    }
    void EndTheTutorial()
    {
        if (index55Tracker)
            return;
        
        index55Tracker = true;
        SceneManager.LoadScene("TitleScreen");
    }
    public void PauseGameOnServer()
    {
        if (this.isGamePaused)
            return;
        Debug.Log("TutorialManager: PauseGameOnServer");
        this.isGamePaused = true;
        
        Time.timeScale = 0f;
        tutorialPlayer.isGamePaused = true;

    }
    public void ResumeGameOnServer()
    {
        if (!this.isGamePaused)
            return;
        Debug.Log("TutorialManager: ResumeGameOnServer");
        this.isGamePaused = false;
        tutorialPlayer.isGamePaused = false;
        Time.timeScale = 1.0f;
    }
    public void MainMenuButton()
    {
        if (Time.timeScale == 0.0f)
            Time.timeScale = 1.0f;
        SceneManager.LoadScene("TitleScreen"); ;
    }
    public void ExitToDesktopButton()
    {
        Application.Quit();
    }
    IEnumerator TouchdownScoredUI()
    {
        TouchDownPanel.SetActive(true);
        touchDownText.GetComponent<TouchDownTextGradient>().ActivateGradient();
        SoundManager.instance.PlaySound("touchdown-cheer", 0.75f);
        SoundManager.instance.PlaySound("touchdown-touchdown", 1.0f);
        yield return new WaitForSeconds(3.0f);
        TouchDownPanel.SetActive(false);
    }
    public void EnableGamepadUIFromSettingsMenu(bool enableGamepadUI)
    {
        Debug.Log("EnableGamepadUIFromSettingsMenu: Enable gamepad UI elements? " + enableGamepadUI.ToString());
        this.gamepadUI = enableGamepadUI;

        // Update Camera marker UI
        Camera.main.GetComponent<TutorialCameraMarker>().SetGamepadUIStuff(enableGamepadUI);

        
        // update goblin markers
        if (greenGrenadierScript)
        {
            try
            {
                greenGrenadierScript.UpdateGamePadUIMarkersForGoblins(enableGamepadUI);
            }
            catch (Exception e)
            {
                Debug.Log("EnableGamepadUIFromSettingsMenu: could not update goblin marker. Error: " + e);
            }
        }
        if(greenBerserkerScript)
        {
            try
            {
                greenBerserkerScript.UpdateGamePadUIMarkersForGoblins(enableGamepadUI);
            }
            catch (Exception e)
            {
                Debug.Log("EnableGamepadUIFromSettingsMenu: could not update goblin marker. Error: " + e);
            }
        }
        if (greenSkirmisherScript)
        {
            try
            {
                greenSkirmisherScript.UpdateGamePadUIMarkersForGoblins(enableGamepadUI);
            }
            catch (Exception e)
            {
                Debug.Log("EnableGamepadUIFromSettingsMenu: could not update goblin marker. Error: " + e);
            }
        }
    }
}
