using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradientChanger : MonoBehaviour {
    public Sprite[] gradients;
    private int currentGradientIndex = 0;
    private SpriteRenderer sr;

    // Start is called before the first frame update
    void Start() {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update() {

    }

    public void CycleGradient() {
        currentGradientIndex++;
        currentGradientIndex %= gradients.Length;
        sr.sprite = gradients[currentGradientIndex];
    }
}