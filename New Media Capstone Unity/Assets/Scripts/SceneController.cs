using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour {
    private static SceneController instance;
    public static SceneController Instance;

    public Image animtedTransitionPanel;
    public AudioSource transitionSound;  // Reference to the AudioSource for the transition sound

    private float switchCounter = 0;
    public float switchAfter = 55;
    private float countDownStart = 5;
    private int currentSceneIndex;
    private bool switching = false;
    public bool flipProgressDir = true;

    private Action switchFunction;

    private void Awake() {
        if (instance) {
            instance.switchAfter = switchAfter;
            Destroy(gameObject);
        }
        else {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start() {
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
#if !UNITY_EDITOR
        Cursor.visible = false;
#endif
        switchFunction = SwitchSceneCircles;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            switchFunction();
        }
        if (switchAfter == 0) return;
        if (!switching) {
            switchCounter += Time.deltaTime;
            float fillAmount = flipProgressDir? (switchCounter / switchAfter) : 1 - (switchCounter / switchAfter);
            for (int i = 1; i < transform.childCount; i++) {
                transform.GetChild(i).GetComponent<Image>().fillAmount = fillAmount;
            }
        }
        if (switchCounter >= switchAfter) {
            switchFunction();
        }
    }

    void SwitchSceneCircles() {
        switching = true;
        int oldSceneIndex = currentSceneIndex;
        currentSceneIndex++;
        currentSceneIndex %= SceneManager.sceneCountInBuildSettings;
        switchCounter = 0;
        animtedTransitionPanel.GetComponent<Animator>().SetBool("Activate", true);
        transitionSound.Play();  // Play the transition sound
        StartCoroutine(WaitFor(50f/60f, () => {
            SceneManager.LoadSceneAsync(currentSceneIndex);
            StartCoroutine(WaitFor((144f-50f)/60f, () => { 
                switching = false; 
                animtedTransitionPanel.GetComponent<Animator>().SetBool("Activate", false);
            }));
        }));
    }

    IEnumerator WaitFor(float time, Action postAction) {
        yield return new WaitForSeconds(time);
        postAction();
    }
}