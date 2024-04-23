using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MovementSound : MonoBehaviour
{
    private AudioSource audioSource;
    private Vector3 lastPosition;
    private Coroutine fadeCoroutine; // Store the coroutine reference

    public float minMovementThreshold = 0.8f;
    public float fadeOutDuration = 2.0f;
    private float originalVolume;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        lastPosition = transform.position;
        originalVolume = audioSource.volume; // Store the original volume
    }

    void Update()
    {
        // Calculate distance moved since the last frame
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        // If the object has stopped moving and we are not already fading out
        if (distanceMoved <= minMovementThreshold && audioSource.volume > 0 && fadeCoroutine == null)
        {
            // Start fading out
            fadeCoroutine = StartCoroutine(FadeOut());
        }

        // If the object is moving, cancel the fade out and reset the volume
        else if (distanceMoved > minMovementThreshold && audioSource.volume != originalVolume)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
            audioSource.volume = originalVolume;
        }

        // Update lastPosition for the next frame
        lastPosition = transform.position;
    }

    private IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        float timeElapsed = 0f;

        while (timeElapsed < fadeOutDuration)
        {
            // Fade out the volume over the fade out duration
            audioSource.volume = Mathf.Lerp(startVolume, 0, timeElapsed / fadeOutDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 0; // Make sure the volume is set to 0 after fading out
        fadeCoroutine = null; // Coroutine is done, reset the reference
    }
}
