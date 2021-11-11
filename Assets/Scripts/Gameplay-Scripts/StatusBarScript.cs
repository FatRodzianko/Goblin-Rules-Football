using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Vector3 newHealthBarScale = Vector3.one;
        newHealthBarScale.x = healthPercentage;
        healthBar.transform.localScale = newHealthBarScale;
    }
    public void StaminaBarUpdate(float staminaPercentage)
    {
        Vector3 newStaminaBarScale = Vector3.one;
        newStaminaBarScale.x = staminaPercentage;
        staminaBar.transform.localScale = newStaminaBarScale;
    }
    public void ChangeStaminaBarColor(bool isGoblinFatigued)
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
