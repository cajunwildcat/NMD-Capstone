using UnityEngine;

public class CloudInteraction : MonoBehaviour
{
    private AudioSource audioSource;
    private Animator animator;

    void Start()
    {
        // Get the AudioSource and Animator components
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collider is the Tracker
        if (other.CompareTag("Tracker") && this.CompareTag("WhiteCloud"))
        {
            // Play the "Cloud Whoosh" sound effect
            audioSource.Play();

            // Trigger the fade away animation
            animator.SetTrigger("Disappear1");
        }

        if (other.CompareTag("Tracker") && this.CompareTag("YellowCloud"))
        {
            // Play the "Cloud Whoosh" sound effect
            audioSource.Play();

            // Trigger the fade away animation
            animator.SetTrigger("Disappear2");
        }

    }
}
