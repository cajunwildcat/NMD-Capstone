using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageViewer : MonoBehaviour
{
    public MultiSourceManager multiSource; //gives color and depth data

    public RawImage rawImage;

    // Update is called once per frame
    void Update()
    {
        rawImage.texture = multiSource.GetColorTexture();
    }
}
