using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MovementSound : MonoBehaviour
{
    private AudioSource audioSource;
    private Vector3 lastPosition;
    public float minMovementThreshold = 0.1f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        lastPosition = transform.position;
    }

    void Update()
    {
        // Check if the object has moved since the last frame
        if (Vector3.Distance(transform.position, lastPosition) > minMovementThreshold)
        {
            // If the audio is not already playing, play the sound
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            // If the object is not moving, stop the sound
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

        // Update lastPosition for the next frame
        lastPosition = transform.position;
    }
}
