using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StatusBarScript : MonoBehaviour
{
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject staminaBar;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HealthBarUpdate(float healthPercentage)
    {
        try
        {
            Vector3 newHealthBarScale = Vector3.one;
            newHealthBarScale.x = healthPercentage;
            healthBar.transform.localScale = newHealthBarScale;
        }
        catch (Exception e)
        {
            Debug.Log("HealthBarUpdate: couldn't update health bar? Error: " + e);
        }
        
    }
    public void StaminaBarUpdate(float staminaPercentage)
    {
        try
        {
            Vector3 newStaminaBarScale = Vector3.one;
            newStaminaBarScale.x = staminaPercentage;
            staminaBar.transform.localScale = newStaminaBarScale;
        }
        catch (Exception e)
        {
            Debug.Log("StaminaBarUpdate: couldn't update stamina bar? Error: " + e);
        }
        
    }
    public void ChangeStaminaBarColor(bool isGoblinFatigued)
    {
        try
        {
            if (isGoblinFatigued)
            {
                staminaBar.GetComponent<SpriteRenderer>().color = Color.yellow;
                IEnumerator flashStaminaBar = FlashStaminaBar();
                StartCoroutine(flashStaminaBar);
            }
            else
            {
                staminaBar.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
        catch (Exception e)
        {
            Debug.Log("ChangeStaminaBarColor: couldn't update stamina bar? Error: " + e);
        }

        
            
    }
    IEnumerator FlashStaminaBar()
    {
        SpriteRenderer staminaBarRenderer = staminaBar.GetComponent<SpriteRenderer>();
        staminaBarRenderer.color = Color.yellow;
        yield return new WaitForSeconds(0.4f);
        staminaBarRenderer.color = Color.white;
        yield return new WaitForSeconds(0.4f);
        staminaBarRenderer.color = Color.yellow;
        yield return new WaitForSeconds(0.4f);
        staminaBarRenderer.color = Color.white;
        yield return new WaitForSeconds(0.4f);
        staminaBarRenderer.color = Color.yellow;
        yield return new WaitForSeconds(0.4f);
        staminaBarRenderer.color = Color.white;
        yield return new WaitForSeconds(0.4f);
    }

}
