using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour {
    private static SceneController instance;
    public static SceneController Instance;

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
            SceneManager.LoadScene("Circles2");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && SceneManager.GetActiveScene().name != "Columns") {
            SceneManager.LoadScene("Columns");
        }
    }
}