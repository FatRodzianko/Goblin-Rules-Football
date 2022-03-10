using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PowerUpButtonScript : MonoBehaviour, IDeselectHandler, ISelectHandler, ISubmitHandler
{
    [SerializeField] GameObject selectedObject;
    [SerializeField] int myIndexNumber;
    GamePlayer myLocalPlayer;
    // Start is called before the first frame update
    void Start()
    {
        myLocalPlayer = GameObject.FindGameObjectWithTag("LocalGamePlayer").GetComponent<GamePlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnSelect(BaseEventData eventData)
    {
        selectedObject.SetActive(true);
        myLocalPlayer.powerUpSelectedIndexNumber = myIndexNumber;
    }
    public void OnDeselect(BaseEventData data)
    {
        selectedObject.SetActive(false);
    }
    public void OnSubmit(BaseEventData eventData)
    {
        myLocalPlayer.UsePowerUp(myIndexNumber);
    }

}
