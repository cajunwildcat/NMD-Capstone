using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour {
    private static SceneController instance;
    public static SceneController Instance;

    public Image fadePanel;
    public Image switchProgress;
    public TMP_Text switchText;

    private float switchCounter = 0;
    public float switchAfter = 55;
    private float countDownStart = 5;
    private int currentSceneIndex;
    private bool switching = false;
    public bool flipProgressDir = true;

    private Action switchFunction;
    int transitionCirlceCount;
    float[] startingRadii;

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

        transitionCirlceCount = fadePanel.transform.childCount;
        startingRadii = new float[transitionCirlceCount];
        for (int i = 0; i < transitionCirlceCount; i++) { 
            Transform circle = fadePanel.transform.GetChild(i);
            startingRadii[i] = circle.localScale.x;
            //circle.localScale = Vector3.zero;
        }
    }

    private void Update() {
        if (!switching) {
            switchCounter += Time.deltaTime;
            float fillAmount = flipProgressDir? (switchCounter / switchAfter) : 1 - (switchCounter / switchAfter);
            switchProgress.fillAmount = fillAmount;
            for (int i = 0; i < switchProgress.transform.childCount; i++) {
                switchProgress.transform.GetChild(i).GetComponent<Image>().fillAmount = fillAmount;
            }
        }
        if (switchCounter >= switchAfter - countDownStart) {
            switchText.enabled = true;
            switchText.text = $"Switching in {(int)(switchAfter-switchCounter) + 1}";
        }
        if (switchCounter >= switchAfter) {
            switchFunction();
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            switchFunction();
        }
    }

    void SwitchSceneFade() {
        switchText.enabled = false;
        switching = true;
        currentSceneIndex++;
        currentSceneIndex %= SceneManager.sceneCountInBuildSettings;
        switchProgress.fillAmount = 0;
        switchCounter = 0;
        StartCoroutine(FadeToColor(0.5f, Color.white));
        StartCoroutine(WaitFor(0.5f, () => { 
            SceneManager.LoadScene(currentSceneIndex); 
            StartCoroutine(FadeToColor(0.5f, new Color(1,1,1, 0)));
            StartCoroutine(WaitFor(0.5f, () => switching = false));
        }));
    }

    void SwitchSceneCircles() {
        switchText.enabled = false;
        switching = true;
        currentSceneIndex++;
        currentSceneIndex %= SceneManager.sceneCountInBuildSettings;
        switchProgress.fillAmount = 0;
        switchCounter = 0;
        for (int i = 0; i < transitionCirlceCount; i++) {
            fadePanel.transform.GetChild(i).GetComponent<Animator>().SetBool("Grow", true);
        }
        StartCoroutine(WaitFor(1, () => {
            SceneManager.LoadScene(currentSceneIndex);
            for (int i = 0; i < transitionCirlceCount; i++) {
                fadePanel.transform.GetChild(i).GetComponent<Animator>().SetBool("Grow", false);
            }
            StartCoroutine(WaitFor(1, () => switching = false));

        }));
    }


    IEnumerator GrowCircles(float time, bool grow) {
        Debug.Log(transitionCirlceCount);
        float counter = 0;
        while (counter < time) {
            counter += Time.deltaTime;
            float progress = counter / time;
            for (int i = 0; i < transitionCirlceCount; i++) {
                if (grow) {
                    fadePanel.transform.GetChild(i).transform.localScale = Vector3.one * Mathf.Lerp(0, startingRadii[i], progress);
                }
                else {
                    fadePanel.transform.GetChild(i).transform.localScale = Vector3.one * Mathf.Lerp(startingRadii[i], 0, progress);
                }
            }
            yield return null;
        }
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