using UnityEditor;
using UnityEngine;

public class CloudInteraction : MonoBehaviour
{
    private AudioSource audioSource;
    private Animator animator;
    public bool isInvisible = false;
    private AnimatorStateInfo stateInfo;
    void Start()
    {
        // Get the AudioSource and Animator components
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Check the current state of the Animator
        stateInfo = animator.GetCurrentAnimatorStateInfo(0); // 0 refers to the base layer

        // Set the isInvisible flag based on the state
        // isInvisible = stateInfo.IsName("Disappear1") || stateInfo.IsName("Reappear1");
        if (stateInfo.IsName("Idle")|| stateInfo.IsName("Reappear1")||stateInfo.IsName("Reappear2")) {
            isInvisible = false; 
        }  
        if (isInvisible ) { audioSource.mute=true; } else { audioSource.mute=false; }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        
        // Check if the collider is the Tracker and the cloud is not invisible
        if (other.CompareTag("Tracker") && this.CompareTag("WhiteCloud") && !isInvisible)
        {
            PlaySoundAndAnimate("Disappear1");
            isInvisible = true;
            print(isInvisible);
        }

        if (other.CompareTag("Tracker") && this.CompareTag("YellowCloud") && !isInvisible)
        {
            PlaySoundAndAnimate("Disappear2");
            isInvisible = true;
            print(isInvisible);

        }
    }

    private void PlaySoundAndAnimate(string triggerName)
    {
        // Play the "Cloud Whoosh" sound effect
        audioSource.Play();

        // Trigger the fade away animation
        animator.SetTrigger(triggerName);
    }
}
