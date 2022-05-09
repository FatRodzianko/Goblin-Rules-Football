using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpParticleSystem : MonoBehaviour
{
    [SerializeField] ParticleSystem myParticleSystem;
    [Header("Particle Sprites")]
    [SerializeField] Sprite healNormalSprite;
    [SerializeField] Sprite attackNormalSprite;
    [SerializeField] Sprite defenseNormalSprite;
    [SerializeField] Sprite speedNormalSprite;
    [SerializeField] Sprite invincibilityBlueShellSprite;
    [SerializeField] Sprite staminaNormalSprite;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartParticleSystem(string particleType)
    {
        var main = myParticleSystem.main;
        if (particleType == "healNormal")
        {            
            main.duration = 2.0f;
            myParticleSystem.textureSheetAnimation.SetSprite(0, healNormalSprite);            
        }
        else if (particleType == "attackNormal")
        {
            main.duration = 3.0f;
            myParticleSystem.textureSheetAnimation.SetSprite(0, attackNormalSprite);
        }
        else if (particleType == "defenseNormal")
        {
            main.duration = 3.0f;
            myParticleSystem.textureSheetAnimation.SetSprite(0, defenseNormalSprite);
        }
        else if (particleType == "speedNormal")
        {
            main.duration = 3.0f;
            myParticleSystem.textureSheetAnimation.SetSprite(0, speedNormalSprite);
        }
        else if (particleType == "invincibilityBlueShell")
        {
            main.duration = 5.0f;
            myParticleSystem.textureSheetAnimation.SetSprite(0, invincibilityBlueShellSprite);
        }
        else if (particleType == "staminaNormal")
        {
            main.duration = 3.0f;
            myParticleSystem.textureSheetAnimation.SetSprite(0, staminaNormalSprite);
        }
        myParticleSystem.Play();
    }
}
