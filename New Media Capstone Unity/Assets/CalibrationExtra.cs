using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CalibrationExtra : MonoBehaviour
{
    TMP_Text trackerCountText;

    // Start is called before the first frame update
    void Start()
    {
        trackerCountText = transform.GetChild(0).GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        trackerCountText.text = $"Trackers: {HandsKinect.Instance.GetAllTrackerGameObjects().Count}";
    }
}
