using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMarker : MonoBehaviour
{
    [Header("Football Tracker stuff")]
    [SerializeField] GameObject ballMarkerLeft;
    [SerializeField] SpriteRenderer ballMarkerLeftRenderer;
    [SerializeField] Sprite leftIcon;
    [SerializeField] Sprite leftIconOpponent;

    [SerializeField] GameObject ballMarkerRight;
    [SerializeField] SpriteRenderer ballMarkerRightRenderer;
    [SerializeField] Sprite rightIcon;
    [SerializeField] Sprite rightIconOpponent;
    [Header("Goblin Tracker stuff")]
    [SerializeField] GameObject eMarkerLeft;
    [SerializeField] GameObject eMarkerRight;
    [SerializeField] GameObject qMarkerLeft;
    [SerializeField] GameObject qMarkerRight;

    Vector3 newPosition = Vector3.zero;
    Vector3 ballPosition = Vector3.zero;

    public GamePlayer myPlayer;

    // Start is called before the first frame update
    void Start()
    {
        ballMarkerLeft.SetActive(false);
        ballMarkerRight.SetActive(false);
        eMarkerLeft.SetActive(false);
        eMarkerRight.SetActive(false);
        qMarkerLeft.SetActive(false);
        qMarkerRight.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateGoblinMarker(bool isLeft, bool isQ, float yValue)
    {
        

        if (isLeft)
        {
            if (isQ)
            {
                qMarkerLeft.SetActive(true);
                newPosition = qMarkerLeft.transform.localPosition;
                newPosition.y = yValue;
                qMarkerLeft.transform.localPosition = newPosition;
            }
            else
            {
                eMarkerLeft.SetActive(true);
                newPosition = eMarkerLeft.transform.localPosition;
                newPosition.y = yValue;
                eMarkerLeft.transform.localPosition = newPosition;
            }
        }
        else
        {
            if (isQ)
            {
                qMarkerRight.SetActive(true);
                newPosition = qMarkerRight.transform.localPosition;
                newPosition.y = yValue;
                qMarkerRight.transform.localPosition = newPosition;
            }
            else
            {
                eMarkerRight.SetActive(true);
                newPosition = eMarkerRight.transform.localPosition;
                newPosition.y = yValue;
                eMarkerRight.transform.localPosition = newPosition;
            }
        }

    }
    public void DeactivateGoblinMarker(bool isQ)
    {
        if (isQ)
        {
            qMarkerLeft.SetActive(false);
            qMarkerRight.SetActive(false);
        }
        else
        {
            eMarkerLeft.SetActive(false);
            eMarkerRight.SetActive(false);
        }
    }
    public void ActivateFootballMarker(bool isLeft, bool isHeld, float yValue)
    {
        if (isLeft)
        {
            if (isHeld && !myPlayer.doesTeamHaveBall)
                ballMarkerLeftRenderer.sprite = leftIconOpponent;
            else
                ballMarkerLeftRenderer.sprite = leftIcon;

            ballMarkerLeft.SetActive(true);
            ballPosition = ballMarkerLeft.transform.localPosition;
            ballPosition.y = yValue;
            ballMarkerLeft.transform.localPosition = ballPosition;
        }
        else
        {
            if (isHeld && !myPlayer.doesTeamHaveBall)
                ballMarkerRightRenderer.sprite = rightIconOpponent;
            else
                ballMarkerRightRenderer.sprite = rightIcon;

            ballMarkerRight.SetActive(true);
            ballPosition = ballMarkerRight.transform.localPosition;
            ballPosition.y = yValue;
            ballMarkerRight.transform.localPosition = ballPosition;
        }
    }
    public void DeActivateFootballMarker()
    {
        ballMarkerLeft.SetActive(false);
        ballMarkerRight.SetActive(false);
    }
}
