using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour {
    private static SceneController instance;
    public static SceneController Instance;

    public Image fadePanel;
    public Image switchProgress;

    private float switchCounter = 0;
    private float switchAfter = 5;
    private int currentSceneIndex;
    private bool switching = false;

    private void Awake() {
        if (instance) {
            Destroy(gameObject);
        }
        else {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start() {
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    private void Update() {
        if (!switching) {
            switchCounter += Time.deltaTime;
            switchProgress.fillAmount = switchCounter / switchAfter;
        }
        if (switchCounter >= switchAfter) {
            SwitchScene();
        }
    }

    void SwitchScene() {
        switching = true;
        currentSceneIndex++;
        currentSceneIndex %= 2;
        switchProgress.fillAmount = 0;
        switchCounter = 0;
        StartCoroutine(FadeToColor(0.5f, Color.black));
        StartCoroutine(WaitFor(0.5f, () => { 
            SceneManager.LoadScene(currentSceneIndex); 
            StartCoroutine(FadeToColor(0.5f, new Color(0, 0, 0, 0)));
            StartCoroutine(WaitFor(0.5f, () => switching = false));
        }));
    }

     IEnumerator FadeToColor(float time, Color newColor) {
        float counter = 0;
        Color startColor = fadePanel.color;
        while (counter < time) {
            counter += Time.deltaTime;
            fadePanel.color = Color.Lerp(startColor, newColor, counter/time);
            yield return null;
        }
    }

    IEnumerator WaitFor(float time, Action postAction) {
        yield return new WaitForSeconds(time);
        postAction();
    }
}