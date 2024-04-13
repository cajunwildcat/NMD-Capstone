using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour {
    private static SceneController instance;
    public static SceneController Instance;

    public Image fadePanel;

    private void Awake() {
        if (instance) {
            Destroy(gameObject);
        }
        else {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1 ) && SceneManager.GetActiveScene().name != "Circles2") {
            StartCoroutine(FadeToColor(0.5f, Color.black));
            StartCoroutine(WaitFor(0.5f, () => { SceneManager.LoadScene("Circles2"); StartCoroutine(FadeToColor(0.5f, new Color(0, 0, 0, 0))); }));
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && SceneManager.GetActiveScene().name != "Columns") {
            StartCoroutine(FadeToColor(0.5f, Color.black));
            StartCoroutine(WaitFor(0.5f, () => { SceneManager.LoadScene("Columns"); StartCoroutine(FadeToColor(0.5f, new Color(0, 0, 0, 0))); }));
            
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