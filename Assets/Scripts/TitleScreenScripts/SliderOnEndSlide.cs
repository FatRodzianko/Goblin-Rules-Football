using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class SliderOnEndSlide : MonoBehaviour, IPointerUpHandler
{
    [SerializeField] SettingsManager settingsManager;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        //float volume = this.gameObject.GetComponent<Slider>().value;
        Debug.Log("Sliding finished");
        settingsManager.SetVolume(this.gameObject.GetComponent<Slider>().value);
    }
}
