using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCameraMarker : MonoBehaviour
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
    [SerializeField] Sprite eMarkerLeftCanPass;
    [SerializeField] Sprite eMarkerLeftCannotPass;
    [SerializeField] GameObject eMarkerRight;
    [SerializeField] Sprite eMarkerRightCanPass;
    [SerializeField] Sprite eMarkerRightCannotPass;
    [SerializeField] GameObject qMarkerLeft;
    [SerializeField] Sprite qMarkerLeftCanPass;
    [SerializeField] Sprite qMarkerLeftCannotPass;
    [SerializeField] GameObject qMarkerRight;
    [SerializeField] Sprite qMarkerRightCanPass;
    [SerializeField] Sprite qMarkerRightCannotPass;

    [Header("Game Pad Sprites")]

    [SerializeField] Sprite rbMarkerLeftCanPass;
    [SerializeField] Sprite rbMarkerLeftCannotPass;
    [SerializeField] Sprite rbMarkerRightCanPass;
    [SerializeField] Sprite rbMarkerRightCannotPass;

    [SerializeField] Sprite lbMarkerLeftCanPass;
    [SerializeField] Sprite lbMarkerLeftCannotPass;
    [SerializeField] Sprite lbMarkerRightCanPass;
    [SerializeField] Sprite lbMarkerRightCannotPass;

    [Header("Keyboard Sprites")]
    [SerializeField] Sprite eMarkerLeftCanPassSprite;
    [SerializeField] Sprite eMarkerLeftCannotPassSprite;
    [SerializeField] Sprite eMarkerRightCanPassSprite;
    [SerializeField] Sprite eMarkerRightCannotPassSprite;
    [SerializeField] Sprite qMarkerLeftCanPassSprite;
    [SerializeField] Sprite qMarkerLeftCannotPassSprite;
    [SerializeField] Sprite qMarkerRightCanPassSprite;
    [SerializeField] Sprite qMarkerRightCannotPassSprite;


    Vector3 newPosition = Vector3.zero;
    Vector3 ballPosition = Vector3.zero;

    public TutorialPlayer myPlayer;

    // Start is called before the first frame update
    void Start()
    {
        ballMarkerLeft.SetActive(false);
        ballMarkerRight.SetActive(false);
        eMarkerLeft.SetActive(false);
        eMarkerRight.SetActive(false);
        qMarkerLeft.SetActive(false);
        qMarkerRight.SetActive(false);


        SetGamepadUIStuff(TutorialManager.instance.gamepadUI);

        /*if (GamepadUIManager.instance.gamepadUI)
        {
            eMarkerLeftCanPass = rbMarkerLeftCanPass;
            eMarkerLeftCannotPass = rbMarkerLeftCannotPass;
            eMarkerRightCanPass = rbMarkerRightCanPass;
            eMarkerRightCannotPass = rbMarkerRightCannotPass;

            qMarkerLeftCanPass = lbMarkerLeftCanPass;
            qMarkerLeftCannotPass = lbMarkerLeftCannotPass;
            qMarkerRightCanPass = lbMarkerRightCanPass;
            qMarkerRightCannotPass = lbMarkerRightCannotPass;

        }*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateGoblinMarker(bool isLeft, bool isQ, float yValue, bool canPass)
    {
        if (isLeft)
        {
            if (isQ)
            {
                qMarkerLeft.SetActive(true);
                newPosition = qMarkerLeft.transform.localPosition;
                newPosition.y = yValue;
                qMarkerLeft.transform.localPosition = newPosition;

                if (canPass)
                    qMarkerLeft.GetComponent<SpriteRenderer>().sprite = qMarkerLeftCanPass;
                else
                    qMarkerLeft.GetComponent<SpriteRenderer>().sprite = qMarkerLeftCannotPass;
            }
            else
            {
                eMarkerLeft.SetActive(true);
                newPosition = eMarkerLeft.transform.localPosition;
                newPosition.y = yValue;
                eMarkerLeft.transform.localPosition = newPosition;

                if (canPass)
                    eMarkerLeft.GetComponent<SpriteRenderer>().sprite = eMarkerLeftCanPass;
                else
                    eMarkerLeft.GetComponent<SpriteRenderer>().sprite = eMarkerLeftCannotPass;

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

                if (canPass)
                    qMarkerRight.GetComponent<SpriteRenderer>().sprite = qMarkerRightCanPass;
                else
                    qMarkerRight.GetComponent<SpriteRenderer>().sprite = qMarkerRightCannotPass;
            }
            else
            {
                eMarkerRight.SetActive(true);
                newPosition = eMarkerRight.transform.localPosition;
                newPosition.y = yValue;
                eMarkerRight.transform.localPosition = newPosition;

                if (canPass)
                    eMarkerRight.GetComponent<SpriteRenderer>().sprite = eMarkerRightCanPass;
                else
                    eMarkerRight.GetComponent<SpriteRenderer>().sprite = eMarkerRightCannotPass;
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
        try
        {
            if (yValue > TutorialManager.instance.maxY)
                yValue = TutorialManager.instance.maxY;
            else if (yValue < TutorialManager.instance.minY)
                yValue = TutorialManager.instance.minY;
        }
        catch (Exception e)
        {
            if (yValue > 7.1f)
                yValue = 7.1f;
            else if (yValue < -11f)
                yValue = -11f;
            Debug.Log("ActivateFootballMarker: failed for y stuff? Error: " + e);
        }
        

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
    public void SetGamepadUIStuff(bool enableGamepadUI)
    {
        Debug.Log("TutorialCameraMarker: SetGamepadUIStuff");
        if (enableGamepadUI)
        {
            eMarkerLeftCanPass = rbMarkerLeftCanPass;
            eMarkerLeftCannotPass = rbMarkerLeftCannotPass;
            eMarkerRightCanPass = rbMarkerRightCanPass;
            eMarkerRightCannotPass = rbMarkerRightCannotPass;

            qMarkerLeftCanPass = lbMarkerLeftCanPass;
            qMarkerLeftCannotPass = lbMarkerLeftCannotPass;
            qMarkerRightCanPass = lbMarkerRightCanPass;
            qMarkerRightCannotPass = lbMarkerRightCannotPass;
        }
        else
        {
            eMarkerLeftCanPass = eMarkerLeftCanPassSprite;
            eMarkerLeftCannotPass = eMarkerLeftCannotPassSprite;
            eMarkerRightCanPass = eMarkerRightCanPassSprite;
            eMarkerRightCannotPass = eMarkerRightCannotPassSprite;

            qMarkerLeftCanPass = qMarkerLeftCanPassSprite;
            qMarkerLeftCannotPass = qMarkerLeftCannotPassSprite;
            qMarkerRightCanPass = qMarkerRightCanPassSprite;
            qMarkerRightCannotPass = qMarkerRightCannotPassSprite;
        }
    }
}
