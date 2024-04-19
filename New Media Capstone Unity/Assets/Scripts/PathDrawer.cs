using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathDrawer : MonoBehaviour {
    public GameObject circlePrefab;
    Dictionary<GameObject, Color> colors = new Dictionary<GameObject, Color>();
    Dictionary<GameObject, Vector3> lastPos = new Dictionary<GameObject, Vector3>();
    Color[] possibleColors = new Color[] {
        new Color32(0xA4, 0x03, 0x6F, 0xF0),
        new Color32(0x04, 0x8B, 0xA8, 0xF0),
        new Color32(0x16, 0xDB, 0x93, 0xF0),
        new Color32(0xEF, 0xEA, 0x5A, 0xF0),
        new Color32(0xF2, 0x9E, 0x4C, 0xF0)
    };

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        List<GameObject> trackers = KinectCoordinates.Instance.GetAllTrackerObjects();
        foreach (GameObject tracker in trackers) {
            Vector3 pos = tracker.transform.position;
            if (!colors.ContainsKey(tracker)) {
                colors.Add(tracker, possibleColors[UnityEngine.Random.Range(0, possibleColors.Length)]);
            }
            if (lastPos.ContainsKey(tracker) && pos == lastPos[tracker]) { continue; }
            GameObject colorTrail = Instantiate(circlePrefab, pos, Quaternion.identity);
            colorTrail.GetComponent<SpriteRenderer>().color = colors[tracker];
            colorTrail.transform.SetParent(transform);
            colorTrail.transform.localScale = tracker.transform.localScale;
            lastPos[tracker] = pos;
        }
    }
}