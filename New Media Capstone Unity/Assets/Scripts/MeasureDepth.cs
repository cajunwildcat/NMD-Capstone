using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;


/** mapping out depth by writing depth to a texture 
    depth and texture are laying on top of one another
 */
public class MeasureDepth : MonoBehaviour
{
    public MultiSourceManager multiSource;
    public Texture2D depthTexture;
    //Cutoffs
    public float depthSensitivity = 1; //location of points at certain depth (used for detection)



    //arrays for camera space points and color space points
    private CameraSpacePoint[] cameraSpacePoints = null;
    private ColorSpacePoint[] colorSpacePoints = null;
    private ushort[] depthData = null; //multisource manager outputs depth data as a ushort[], store data here.
    //need to know how much data is necessary for this (allocation of space)

    private KinectSensor sensor = null;
    private CoordinateMapper mapper = null; //measure depth data onto color points;there is an offset -- need compensation
                                            

    private readonly Vector2Int depthResolution = new Vector2Int(512, 424); //depth sensor creates image by this pixelation

    private void Awake()
    {
        sensor = KinectSensor.GetDefault();
        mapper = sensor.CoordinateMapper;

        //initialize arrays -- find out how much data is necessary in 2D space by multiplying 512 by 424
        int arraySize = depthResolution.x * depthResolution.y;

        
        cameraSpacePoints = new CameraSpacePoint[arraySize];
        colorSpacePoints = new ColorSpacePoint[arraySize];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DepthToColor();
            depthTexture = CreateTexture();
        }
    }

    //taking cam/color space points and lining them up
    private void DepthToColor()
    {
        //get depth
        depthData = multiSource.GetDepthData();
        //map data (strings of information that helps map RGB color space with camera space)
        mapper.MapDepthFrameToCameraSpace(depthData, cameraSpacePoints); //call function, pass in data and fill up array as an 'out'
        mapper.MapDepthFrameToColorSpace(depthData, colorSpacePoints);

        //filter

    }

    private Texture2D CreateTexture()
    {
        //use color space points to create a texture; textureFormat is transparent
        Texture2D newTexture = new Texture2D(1920, 1080, TextureFormat.Alpha8, false); //RGB picture on Kinect outputs 1080p

        for (int x = 0; x < 1920; x++) { 
            for (int y = 0; y < 1080; y++) {
                newTexture.SetPixel(x, y, Color.clear);
            } 
        }

        foreach(ColorSpacePoint point in colorSpacePoints)
        {
            newTexture.SetPixel((int)point.X, (int)point.Y, Color.black);
        }

        newTexture.Apply(); // apply changes
        return newTexture;

    }
}
