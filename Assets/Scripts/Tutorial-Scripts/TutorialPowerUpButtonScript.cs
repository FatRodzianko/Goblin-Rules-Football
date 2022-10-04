using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class TutorialPowerUpButtonScript : MonoBehaviour, IDeselectHandler, ISelectHandler, ISubmitHandler
{
    [SerializeField] GameObject selectedObject;
    [SerializeField] int myIndexNumber;
    TutorialPlayer myLocalPlayer;
    // Start is called before the first frame update
    void Start()
    {
        myLocalPlayer = GameObject.FindGameObjectWithTag("TutorialPlayer").GetComponent<TutorialPlayer>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnSelect(BaseEventData eventData)
    {
        Debug.Log("PowerUpButtonScript OnSelect for power up: " + myIndexNumber.ToString());
        selectedObject.SetActive(true);
        try
        {
            myLocalPlayer.powerUpSelectedIndexNumber = myIndexNumber;
        }
        catch (Exception e)
        {
            Debug.Log("TutorialPowerUpButtonScript: OnSelect: Error : " + e);
        }

    }
    public void OnDeselect(BaseEventData data)
    {
        Debug.Log("PowerUpButtonScript OnDeselect for power up: " + myIndexNumber.ToString());
        selectedObject.SetActive(false);
    }
    public void OnSubmit(BaseEventData eventData)
    {
        Debug.Log("PowerUpButtonScript OnSubmit for power up: " + myIndexNumber.ToString());
        myLocalPlayer.UsePowerUp(myIndexNumber);
    }
}
