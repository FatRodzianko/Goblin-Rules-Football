using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackFootballScript : MonoBehaviour
{
    public GameObject myFootball;
    public Football myFootballScript;
    public bool doesCameraHaveFootball = false;
    public GamePlayer myPlayer;

    [SerializeField] GameObject ballMarker;
    public GameObject ballMarkerObject;
    public SpriteRenderer ballMarkerRenderer;
    [SerializeField] Sprite ballMarkerLeft;
    [SerializeField] Sprite ballMarkerRight;
    [SerializeField] Sprite ballMarkerLeftOpponent;
    [SerializeField] Sprite ballMarkerRightOpponent;
    Vector3 ballMarkerPosition;
    // Start is called before the first frame update
    void Start()
    {
        ballMarkerObject = Instantiate(ballMarker);
        ballMarkerObject.transform.parent = this.transform;
        ballMarkerObject.SetActive(false);
        ballMarkerRenderer = ballMarkerObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        if (doesCameraHaveFootball)
        {
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(myFootball.transform.position);
            if (screenPoint.x > 0 && screenPoint.x < 1)
            {
                ballMarkerObject.SetActive(false);
            }
            else if (screenPoint.x <= 0)
            {
                if (myFootballScript.isHeld && !myPlayer.doesTeamHaveBall)
                {
                    ballMarkerRenderer.sprite = ballMarkerLeftOpponent;
                }
                else
                {
                    ballMarkerRenderer.sprite = ballMarkerLeft;
                }

                
                /*ballMarkerPosition = myFootball.transform.position;
                ballMarkerPosition.x = 0.1f;
                ballMarkerPosition.z = 1f;*/
                ballMarkerPosition = new Vector3(-15.9f, myFootball.transform.position.y, 1f);
                ballMarkerObject.transform.localPosition = ballMarkerPosition;
                ballMarkerObject.SetActive(true);
            }
            else if (screenPoint.x >= 1)
            {
                if (myFootballScript.isHeld && !myPlayer.doesTeamHaveBall)
                {
                    ballMarkerRenderer.sprite = ballMarkerRightOpponent;
                }
                else
                {
                    ballMarkerRenderer.sprite = ballMarkerRight;
                }
                
                ballMarkerPosition = new Vector3(15.9f, myFootball.transform.position.y, 1f);
                ballMarkerObject.transform.localPosition = ballMarkerPosition;
                ballMarkerObject.SetActive(true);
            }
        }
    }
}
