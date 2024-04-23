using UnityEngine;

public class ActivateAnimationOnCollision : MonoBehaviour
{

    // Note to self, this currently doesnt trigger anything. Bug fix later
    public Animator animator; // Assign this in the Inspector
    public string triggerName = "activation"; // Name of the relevant trigger variable in the Animator

    void OnTriggerEnter(Collider other) // Use OnTriggerEnter2D(Collider2D other) for 2D
    {
        if (other.gameObject.CompareTag("Tracker")) // Ensure the Tracker has the "Tracker" tag
        {
            print("Collsion detected");
            animator.SetTrigger(triggerName);
        }
    }
}
