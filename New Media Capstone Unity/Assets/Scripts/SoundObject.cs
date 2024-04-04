using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundObject : MonoBehaviour
{
    private WanderAndEvade wanderScript;
    private AudioSource audioSource;
    private bool isActivated = false;

    // Reference to the AudioManager script on a manager object in your scene
    public AudioManager audioManager;

    void Start()
    {
        wanderScript = GetComponent<WanderAndEvade>();
        audioSource = GetComponent<AudioSource>();
        audioSource.mute = true; // Mute the AudioSource component on start
    }

    void OnMouseDown()
    {
        // This method is called when the object is clicked
        if (wanderScript != null && !isActivated)
        {
            // Disable the WanderAndEvade script
            wanderScript.enabled = false;

            // Unmute the audio source to activate the sound
            audioSource.mute = false;
            isActivated = true;

            // Tell the AudioManager to check the status of all audio sources
            audioManager.CheckAudioSources();
        }
    }
}
