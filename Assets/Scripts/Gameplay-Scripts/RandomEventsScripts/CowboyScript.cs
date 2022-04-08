using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CowboyScript : NetworkBehaviour
{
    [SerializeField] Animator myAnimator;
    [SerializeField] SpriteRenderer myRenderer;
    [SerializeField] Rigidbody2D myRb;
    [SerializeField] GameObject myCigarettePS;
    [SerializeField] ParticleSystem exhaleSmokePS;
    [SerializeField] GameObject YeehawText;
    public float walkingSpeed;
    public Vector3 destinationPoint;
    public Vector3 spawnPoint;
    public Vector3 exitPoint;
    public float xDirectionOfDestination = 0f;
    public float xDirectiontoExit = 0f;
    public float destinationY;
    public bool hasCowboyGoneDownFarEnough;
    public bool doesCowboyExitNow = false;
    public bool isCowboyBelowExit = false;
    public bool waitingToSmoke = false;
    Vector2 directionToMove = Vector2.zero;
    IEnumerator waitingToSmokeRoutine;
    public int numberOfSmokes = 0;
    List<uint> playersReceivedYeehawFromNetIds = new List<uint>();
    public bool isVisibileToClient = false;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("CowboyScript: Yeehaw, I am alive (Start() has executed)");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [ServerCallback]
    private void FixedUpdate()
    {
        if (!waitingToSmoke)
        {
            
            if (!doesCowboyExitNow)
            {
                if (myRb.position.y <= destinationY)
                {
                    hasCowboyGoneDownFarEnough = true;
                }

                if (hasCowboyGoneDownFarEnough)
                {
                    directionToMove = new Vector2(xDirectionOfDestination, 0f);
                }
                else
                {
                    directionToMove = new Vector2(0f, -1f);
                }
            }
            else
            {
                if (Mathf.Abs(myRb.position.x - exitPoint.x) <= 0.1f)
                {
                    isCowboyBelowExit = true;
                    directionToMove = new Vector2(0f, 1f);
                }
                else
                {
                    directionToMove = new Vector2(xDirectiontoExit, 0f);
                }

                if (Vector2.Distance(myRb.position, exitPoint) <= 0.1f)
                {
                    NetworkServer.Destroy(this.gameObject);
                }
            }
            myRb.MovePosition(myRb.position + directionToMove * walkingSpeed * Time.fixedDeltaTime);
            myAnimator.SetBool("isWalking", true);
            if (directionToMove.x < 0)
                RpcFlipRenderer(true);
            else
                RpcFlipRenderer(false);
            if (Mathf.Abs(myRb.position.x - destinationPoint.x) <= 0.1f && !doesCowboyExitNow)
            {
                if (!waitingToSmoke)
                {
                    //waitingToSmokeRoutine = WaitingToSmokeRoutine();
                    //StartCoroutine(waitingToSmokeRoutine);
                    GetNumberOfSmokes();
                }
            }
        }
    }
    [ClientRpc]
    void RpcFlipRenderer(bool flip)
    {
        if (myRenderer.flipX != flip)
        {
            myRenderer.flipX = flip;
            float xValue = 1f;
            if (flip)
                xValue = -1f;
            myCigarettePS.transform.localScale = new Vector3(xValue, 1f, 1f);
            exhaleSmokePS.gameObject.transform.localScale = new Vector3(xValue, 1f, 1f);
        }
            
    }
    [ServerCallback]
    IEnumerator WaitingToSmokeRoutine()
    {
        waitingToSmoke = true;
        myAnimator.SetBool("isWalking", false);
        float randomWaitTime = Random.Range(2.25f, 8.0f);
        yield return new WaitForSeconds(randomWaitTime);
        waitingToSmoke = false;
        doesCowboyExitNow = true;
    }
    void GetNumberOfSmokes()
    {
        numberOfSmokes = Random.Range(1, 5);
        Debug.Log("CowboyScript.cs GetNumberOfSmokes: " + numberOfSmokes.ToString());
        myAnimator.SetBool("isWalking", false);
        waitingToSmoke = true;        
        myAnimator.SetBool("isSmoking", waitingToSmoke);
    }
    public void EndOfOneWait()
    {
        if (isServer)
        {
            Debug.Log("CowboyScript.cs EndOfOneWait started: " + numberOfSmokes.ToString());
            numberOfSmokes--;
            Debug.Log("CowboyScript.cs EndOfOneWait subtracted smokes: " + numberOfSmokes.ToString());
            if (numberOfSmokes <= 0)
            {
                Debug.Log("CowboyScript.cs EndOfOneWait No more smokes. Time to ride off into another sunset.");
                waitingToSmoke = false;
                myAnimator.SetBool("isSmoking", waitingToSmoke);
                doesCowboyExitNow = true;
            }
            else
            {
                waitingToSmoke = true;
                myAnimator.SetBool("isSmoking", waitingToSmoke);
                Debug.Log("CowboyScript.cs EndOfOneWait restarting animation");
            }
        }
    }
    private void OnDestroy()
    {
        if (isServer)
            CowboyManager.instance.isCowboySpawned = false;
    }
    [ServerCallback]
    public void PlayerIsGivingYeehaw(uint playerNetId)
    {
        if (playerNetId != 0)
        {
            if (!playersReceivedYeehawFromNetIds.Contains(playerNetId))
            {
                Debug.Log("CowboyScript.cs PlayerIsGivingYeehaw: Player with netid: " + playerNetId.ToString() + " has NOT tried to yeehaw this cowboy yet.");
                playersReceivedYeehawFromNetIds.Add(playerNetId);
                GamePlayer player = NetworkIdentity.spawned[playerNetId].GetComponent<GamePlayer>();
                TeamManager.instance.YeehawCowboy(player);
            }
            else
            {
                Debug.Log("CowboyScript.cs PlayerIsGivingYeehaw: Player with netid: " + playerNetId.ToString() + " has ALREADY yeehawed this cowboy.");
            }
        }
    }
    public void ExhaleSmoke()
    {
        exhaleSmokePS.Play();
    }
    [ClientCallback]
    private void OnBecameVisible()
    {
        isVisibileToClient = true;
    }
    [ClientCallback]
    private void OnBecameInvisible()
    {
        isVisibileToClient = false;
    }
    public void ActivateYeehawText()
    {
        if (!YeehawText.activeInHierarchy)
        {
            Debug.Log("CowboyScript.cs ActivateYeehawText");
            YeehawText.SetActive(true);
            YeehawText.GetComponent<TouchDownTextGradient>().ActivateYeehawGradient();
        }
    }
}
