using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class SliderOnEndSlide : MonoBehaviour, IPointerUpHandler,IDeselectHandler
{
    [SerializeField] SettingsManager settingsManager;
    float newSliderValue = 0f;
    // Start is called before the first frame update
    void Start()
    {
        newSliderValue = this.gameObject.GetComponent<Slider>().value;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        //float volume = this.gameObject.GetComponent<Slider>().value;
        Debug.Log("Sliding finished");
        if (!settingsManager)
        {
            settingsManager = GameObject.FindGameObjectWithTag("SettingsManager").GetComponent<SettingsManager>();
        }
        newSliderValue = this.gameObject.GetComponent<Slider>().value;
        settingsManager.SetVolume(this.gameObject.GetComponent<Slider>().value);
    }
    public void OnDeselect(BaseEventData data)
    {
        Debug.Log("Deselected");
        if (this.gameObject.GetComponent<Slider>().value != newSliderValue)
        {
            Debug.Log("OnDeselect: Need to update slider value: Current value: " + this.gameObject.GetComponent<Slider>().value.ToString() + " Old value: " + newSliderValue);
            newSliderValue = this.gameObject.GetComponent<Slider>().value;
            settingsManager.SetVolume(this.gameObject.GetComponent<Slider>().value);
        }
    }
}
