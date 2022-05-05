using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFlash : MonoBehaviour
{
    // borrowed from: https://github.com/BarthaSzabolcs/Tutorial-SpriteFlash/blob/main/Assets/Scripts/FlashEffects/SimpleFlash.cs


        [Tooltip("Material to switch to during the flash.")]
        [SerializeField] private Material flashMaterial;

        [Tooltip("Duration of the flash.")]
        [SerializeField] private float duration;

        // The SpriteRenderer that should flash.
        private SpriteRenderer spriteRenderer;

        // The material that was in use, when the script started.
        private Material originalMaterial;

        // The currently running coroutine.
        private Coroutine flashRoutine;
        bool isMultiFlashRunning = false;

        void Start()
        {
            // Get the SpriteRenderer to be used,
            // alternatively you could set it from the inspector.
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Get the material that the SpriteRenderer uses, 
            // so we can switch back to it after the flash ended.
            originalMaterial = spriteRenderer.material;
        }

    public void Flash(Color color)
    {
        // If the flashRoutine is not null, then it is currently running.
        if (flashRoutine != null)
        {
            // In this case, we should stop it first.
            // Multiple FlashRoutines the same time would cause bugs.
            StopCoroutine(flashRoutine);
        }

        // Start the Coroutine, and store the reference for it.
        flashRoutine = StartCoroutine(FlashRoutine(color));
    }

    private IEnumerator FlashRoutine(Color color)
    {
        // Swap to the flashMaterial.
        spriteRenderer.material = flashMaterial;

        // Set the desired color for the flash.
        flashMaterial.color = color;

        // Pause the execution of this function for "duration" seconds.
        yield return new WaitForSeconds(duration);

        // After the pause, swap back to the original material.
        spriteRenderer.material = originalMaterial;

        // Set the flashRoutine to null, signaling that it's finished.
        flashRoutine = null;
    }
    public void MultiFlash(float timeToRunFor, Color color1, Color color2, Color color3)
    {
        // If the flashRoutine is not null, then it is currently running.
        if (flashRoutine != null)
        {
            // In this case, we should stop it first.
            // Multiple FlashRoutines the same time would cause bugs.
            StopCoroutine(flashRoutine);
        }

        // Start the Coroutine, and store the reference for it.
        flashRoutine = StartCoroutine(MultiFlashRoutine(timeToRunFor, color1, color2, color3));
    }
    private IEnumerator MultiFlashRoutine(float timeToRunFor, Color color1, Color color2, Color color3)
    {
        /*// Swap to the flashMaterial.
        spriteRenderer.material = flashMaterial;

        // Set the desired color for the flash.
        flashMaterial.color = color;

        // Pause the execution of this function for "duration" seconds.
        yield return new WaitForSeconds(duration);

        // After the pause, swap back to the original material.
        spriteRenderer.material = originalMaterial;*/
        isMultiFlashRunning = true;
        float timeElapsed = 0f;
        while (isMultiFlashRunning)
        {
            // Run 1
            spriteRenderer.material = flashMaterial;
            flashMaterial.color = color1;
            yield return new WaitForSeconds(duration);
            spriteRenderer.material = originalMaterial;
            timeElapsed += duration;
            if (timeElapsed >= timeToRunFor)
                isMultiFlashRunning = false;
            yield return new WaitForSeconds(duration);
            timeElapsed += duration;
            if (timeElapsed >= timeToRunFor)
                isMultiFlashRunning = false;

            // Run 2
            spriteRenderer.material = flashMaterial;
            flashMaterial.color = color2;
            yield return new WaitForSeconds(duration);
            spriteRenderer.material = originalMaterial;
            timeElapsed += duration;
            if (timeElapsed >= timeToRunFor)
                isMultiFlashRunning = false;
            yield return new WaitForSeconds(duration);
            timeElapsed += duration;
            if (timeElapsed >= timeToRunFor)
                isMultiFlashRunning = false;

            // Run 3
            spriteRenderer.material = flashMaterial;
            flashMaterial.color = color3;
            yield return new WaitForSeconds(duration);
            spriteRenderer.material = originalMaterial;
            timeElapsed += duration;
            if (timeElapsed >= timeToRunFor)
                isMultiFlashRunning = false;
            yield return new WaitForSeconds(duration);
            timeElapsed += duration;
            if (timeElapsed >= timeToRunFor)
                isMultiFlashRunning = false;
        }
        // Set the flashRoutine to null, signaling that it's finished.
        flashRoutine = null;
        yield break;
    }

}
