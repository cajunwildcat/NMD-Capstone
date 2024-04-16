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
    private readonly float switchAfter = 15;
    private float countDownStart = 5;
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
        Cursor.visible = false;
    }

    private void Update() {
        if (!switching) {
            switchCounter += Time.deltaTime;
            float fillAmount = 1- (switchCounter / switchAfter);
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
            SwitchScene();
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            SwitchScene();
        }
    }

    void SwitchScene() {
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