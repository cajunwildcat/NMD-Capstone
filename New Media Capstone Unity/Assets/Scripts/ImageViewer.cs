using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageViewer : MonoBehaviour
{
    public MeasureDepth measureDepth; //reference to measured depth
    public MultiSourceManager multiSource; //gives color and depth data

    public RawImage rawImage;
    public RawImage rawDepth;

    // Update is called once per frame
    void Update()
    {
        rawImage.texture = multiSource.GetColorTexture();


        //create function get image from measure depth
        rawDepth.texture = measureDepth.depthTexture;
    }
}
