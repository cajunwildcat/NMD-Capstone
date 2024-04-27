using UnityEngine;

public class AudioPersistor : MonoBehaviour
{
    private static AudioPersistor instance = null;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // This makes the audio persist
        }
        else if (instance != this)
        {
            Destroy(gameObject);  // This destroys any duplicates that accidentally get created on scene load
        }
    }
}
