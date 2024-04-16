using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test2 : MonoBehaviour {
    public GameObject circlePrefab;
    Dictionary<GameObject, Color> colors = new Dictionary<GameObject, Color>();
    Dictionary<GameObject, Vector3> lastPos = new Dictionary<GameObject, Vector3>();
    Color[] possibleColors = new Color[] {
        new Color32(0xA4, 0x03, 0x6F, 0xFF),
        new Color32(0x04, 0x8B, 0xA8, 0xFF),
        new Color32(0x16, 0xDB, 0x93, 0xFF),
        new Color32(0xEF, 0xEA, 0x5A, 0xFF),
        new Color32(0xF2, 0x9E, 0x4C, 0xFF)
    };

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        List<Tuple<GameObject, Vector3>> e = KinectCoordinates.Instance.GetAllTrackerPositions();
        foreach (Tuple<GameObject, Vector3> b in e) {
            if (!colors.ContainsKey(b.Item1)) {
                colors.Add(b.Item1, possibleColors[UnityEngine.Random.Range(0, possibleColors.Length)]);
            }
            if (lastPos.ContainsKey(b.Item1) && b.Item2 == lastPos[b.Item1]) { continue; }
            GameObject colorTrail = Instantiate(circlePrefab, b.Item2, Quaternion.identity);
            colorTrail.GetComponent<SpriteRenderer>().color = colors[b.Item1];
            colorTrail.transform.SetParent(transform);
            colorTrail.transform.localScale = b.Item1.transform.localScale;
            lastPos[b.Item1] = b.Item2;
        }
    }
}