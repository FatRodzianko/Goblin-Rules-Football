using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightningManager : MonoBehaviour
{
    [SerializeField] Light2D _lightningLight;
    bool _lightningFlashing = false;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartLightning());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator StartLightning()
    {
        while (true)
        {
            if (_lightningFlashing)
                yield return new WaitForSeconds(1.0f);
            else
            {
                int numberOfFlashes = UnityEngine.Random.Range(2, 6);
                StartCoroutine(LightningFlash(numberOfFlashes));
                float lightningCooldown = UnityEngine.Random.Range(10f, 15f);
                Debug.Log("Delay before LightningFlash: " + lightningCooldown.ToString());
                yield return new WaitForSeconds(lightningCooldown);
            }
            
        }
        yield break;
    }
    IEnumerator LightningFlash(int numberOfFlashes)
    {
        Debug.Log("LightningFlash: " + numberOfFlashes.ToString());
        int totalFlashes = 0;
        _lightningFlashing = true;
        while (totalFlashes < numberOfFlashes)
        {
            // lightning flash
            float flashDelay = UnityEngine.Random.Range(0.15f, 0.25f);
            _lightningLight.enabled = true;
            yield return new WaitForSeconds(flashDelay);
            _lightningLight.enabled = false;

            // cool down before next flash
            float lightningStrikeCooldown = UnityEngine.Random.Range(0.15f, 0.4f);
            yield return new WaitForSeconds(lightningStrikeCooldown);
            totalFlashes++;
        }
        _lightningFlashing = false;


    }
}
