using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RectTrigger : MonoBehaviour
{
    [Range(0, 10)]
    public int sensitivity = 5;

    public bool isTriggered = false;
    private Camera testCamera = null;
    private RectTransform rectTransform = null;
    private Image testImage = null;

    //subscribe to trigger points
    private void Awake()
    {
        MeasureDepth.OnTriggerPoints += OnTriggerPoints;

        testCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        testImage = GetComponent<Image>();
    }
    //unsubscribe to trigger points
    private void OnDestroy()
    {
        MeasureDepth.OnTriggerPoints -= OnTriggerPoints;
    }

    private void OnTriggerPoints(List<Vector2> triggerPoints)
    {
        if (!enabled) return;

        int count = 0;
        foreach(Vector2 point in triggerPoints)
        {

            //getting y value from triggerpoints and flip to show properly
            Vector2 flippedY = new Vector2(point.x, testCamera.pixelHeight - point.y);
            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, flippedY)) count++;
        }

        if(count > sensitivity)
        {
            isTriggered = true;
            testImage.color = Color.red;
        }
    }
}
