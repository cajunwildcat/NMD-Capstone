using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource vocalsAudioSource; // Assign this to the vocals audio source in the inspector
    public AudioSource[] audioSources; // Assign all other audio sources to this array in the inspector
    public AudioSource startingAudioSource; // The track that should play from the start

    void Start()
    {
        // Play all tracks at the start to keep them in sync
        foreach (AudioSource source in audioSources)
        {
            source.Play();
            source.mute = true; // Mute the tracks
        }

        // Play the vocals but keep it muted
        vocalsAudioSource.Play();
        vocalsAudioSource.mute = true;

        // Start the startingAudioSource unmuted if it's not null
        if (startingAudioSource != null)
        {
            startingAudioSource.Play();
            startingAudioSource.mute = false; // Ensure this track is not muted
        }
    }

    public void CheckAudioSources()
    {
        foreach (AudioSource source in audioSources)
        {
            // If any audio source is muted, return
            if (source.mute)
            {
                return;
            }
        }

        // If all audio sources are unmuted, unmute the vocals
        vocalsAudioSource.mute = false;
    }
}
