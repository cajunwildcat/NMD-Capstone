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
    [Range(0, 1.0f)]

    //depth senesitivity also means that objects may need to be closer to pick up recognition
    public float depthSensitivity = 1; //location of points at certain depth (used for detection)
    [Range(-10, 10f)]
    public float wallDepth = -10; //figure out how far something is from a wall (for detection)

    [Header("Top and Bottom")] //adds title for separate section
    [Range(-1, 1f)]
    public float topCutOff = 1;
    [Range(-1, 1f)]
    public float bottomCutOff = -1;

    [Header("Left and Right")]
    [Range(-1, 1f)]
    public float leftCutOff = -1;
    [Range(-1, 1f)]
    public float rightCutOff = 1;


    //arrays for camera space points and color space points
    private CameraSpacePoint[] cameraSpacePoints = null;
    private ColorSpacePoint[] colorSpacePoints = null;
    private ushort[] depthData = null; //multisource manager outputs depth data as a ushort[], store data here.
    private List<ValidPoint> validPoints = null;
    private List<Vector2> triggerPoints = null;
    //need to know how much data is necessary for this (allocation of space)

    private KinectSensor sensor = null;
    private CoordinateMapper mapper = null; //measure depth data onto color points;there is an offset -- need compensation
    private Camera testCamera = null;


    private readonly Vector2Int depthResolution = new Vector2Int(512, 424); //depth sensor creates image by this pixelation
    private Rect testRect;

    private void Awake()
    {
        sensor = KinectSensor.GetDefault();
        mapper = sensor.CoordinateMapper;
        testCamera = Camera.main;

        //initialize arrays -- find out how much data is necessary in 2D space by multiplying 512 by 424
        int arraySize = depthResolution.x * depthResolution.y;


        cameraSpacePoints = new CameraSpacePoint[arraySize];
        colorSpacePoints = new ColorSpacePoint[arraySize];
    }

    private void Update()
    {
        
        validPoints = DepthToColor();//getting that data and pass through texture function

        triggerPoints = FilterToTrigger(validPoints);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            
            

            testRect = CreateRect(validPoints);

            depthTexture = CreateTexture(validPoints);
        }
    }

    private void OnGUI()
    {
        GUI.Box(testRect, "");
        if(triggerPoints == null)
        {
            return;
        }
        foreach(Vector2 point in triggerPoints)
        {
            Rect rect = new Rect(point, new Vector2(10, 10));
            GUI.Box(rect, "");
        }
    }

    //taking cam/color space points and lining them up
    private List<ValidPoint> DepthToColor()
    {

        List<ValidPoint> validPoints = new List<ValidPoint>();
        //get depth
        depthData = multiSource.GetDepthData();
        //map data (strings of information that helps map RGB color space with camera space)
        mapper.MapDepthFrameToCameraSpace(depthData, cameraSpacePoints); //call function, pass in data and fill up array as an 'out'
        mapper.MapDepthFrameToColorSpace(depthData, colorSpacePoints);



        //filter
        for (int i = 0; i < depthResolution.x / 8; i++) //skip over values to filter number of points. multiplication happens below
        {
            for (int j = 0; j < depthResolution.y / 8; j++)
            {
                //Sample
                //1D Array of camera point, converts 2D array created
                //j is taken care of by y, multiplied by number of points in a row, then adding new 'i' value
                int sampleIndex = (j * depthResolution.x) + i; //converts 2D nested array into a value that works in 1D array.
                sampleIndex *= 8; //filtering number of points
                //conditionals for cutoff values.. can limit number of values
                if (cameraSpacePoints[sampleIndex].X < leftCutOff) continue;
                if (cameraSpacePoints[sampleIndex].X > rightCutOff) continue;
                if (cameraSpacePoints[sampleIndex].Y > topCutOff) continue;
                if (cameraSpacePoints[sampleIndex].Y < bottomCutOff) continue;

                //custom class for validating a new point.. saving the data
                ValidPoint newPoint = new ValidPoint(colorSpacePoints[sampleIndex], cameraSpacePoints[sampleIndex].Z);

                //depth test
                if (cameraSpacePoints[sampleIndex].Z >= wallDepth) newPoint.withinWallDepth = true;

                //add
                validPoints.Add(newPoint);

            }
        }

        return validPoints;

    }

    private List<Vector2> FilterToTrigger(List<ValidPoint> validPoints)
    {
        List<Vector2> triggerPoints = new List<Vector2>();
        foreach(ValidPoint point in validPoints) //distilling down valid points
        {
            if (!point.withinWallDepth)
            {
                //valid points beinga all points in rect that is designated
                if (point.z < wallDepth * depthSensitivity)
                {
                    //space looking for points
                    Vector2 screenPoint = ScreenToCamera(new Vector2(point.colorSpace.X, point.colorSpace.Y));
                    triggerPoints.Add(screenPoint);
                }
            }
        }
        return triggerPoints;
    }


    private Texture2D CreateTexture(List<ValidPoint> validPoints)
    {
        //use color space points to create a texture; textureFormat is transparent
        Texture2D newTexture = new Texture2D(1920, 1080, TextureFormat.Alpha8, false); //RGB picture on Kinect outputs 1080p

        for (int x = 0; x < 1920; x++)
        {
            for (int y = 0; y < 1080; y++)
            {
                newTexture.SetPixel(x, y, Color.clear);
            }
        }

        foreach (ValidPoint point in validPoints)
        {
            newTexture.SetPixel((int)point.colorSpace.X, (int)point.colorSpace.Y, Color.black);
        }

        newTexture.Apply(); // apply changes
        return newTexture;

    }

    #region Rect Creation
    private Rect CreateRect(List<ValidPoint> points)
    {

        if (points.Count == 0) return new Rect(); //if no points, return a new rectangle
        Vector2 topLeft = GetTopLeft(points); //pass in the points for corners of rectangle
        Vector2 bottomRight = GetBottomRight(points);


        //translate to viewport -- gets all points from depthtocolor method
        //if points; what top most left point and bottom most right aer
        Vector2 screenTopLeft = ScreenToCamera(topLeft);
        Vector2 screenBottomRight = ScreenToCamera(bottomRight);

        //rect dimensions
        int width = (int)(screenBottomRight.x - screenTopLeft.x);
        int height = (int)(screenBottomRight.y - screenTopLeft.y);

        //Create
        Vector2 size = new Vector2(width, height);
        Rect rect = new Rect(screenTopLeft, size);

        return rect;
    }

    private Vector2 GetTopLeft(List<ValidPoint> points)
    {
        Vector2 topLeft = new Vector2(int.MaxValue, int.MaxValue);
        foreach(ValidPoint point in points)
        {
            //left most x value
            if (point.colorSpace.X < topLeft.x) topLeft.x = point.colorSpace.X; //checking current point, always going to be less than maxValue
            //if less, then insert value

            //top most y value
            if (point.colorSpace.Y < topLeft.y) topLeft.y = point.colorSpace.Y;
        }

        return topLeft;
    }
    private Vector2 GetBottomRight(List<ValidPoint> points)
    {
        Vector2 bottomRight = new Vector2(int.MinValue, int.MinValue);
        foreach (ValidPoint point in points)
        {
            //right most x value
            if (point.colorSpace.X > bottomRight.x) bottomRight.x = point.colorSpace.X; //checking current point, always going to be less than maxValue
            //if less, then insert value

            //bottom most y value
            if (point.colorSpace.Y > bottomRight.y) bottomRight.y = point.colorSpace.Y;
        }

        return bottomRight;
    }

    //converts two points topLeft, bottomRight and gives location on screen (correct drawing of rectangle
    private Vector2 ScreenToCamera(Vector2 screenPosition)
    {
        Vector2 normalizedScreenPoint = new Vector2(Mathf.InverseLerp(0, 1920, screenPosition.x), Mathf.InverseLerp(0, 1080, screenPosition.y));

        //convert back to pixel position
        Vector2 screenPoint = new Vector2(normalizedScreenPoint.x * testCamera.pixelWidth, normalizedScreenPoint.y * testCamera.pixelHeight);

        return screenPoint;
    }


    #endregion
}

//store reference to color point and give a depth value
public class ValidPoint
{
    public ColorSpacePoint colorSpace;
    public float z = 0.0f;

    public bool withinWallDepth = false;

    //assigned z value. flag is for detection in wall depth (is point before or after the cutoff point)
    public ValidPoint(ColorSpacePoint newColorSpace, float newZ)
    {
        colorSpace = newColorSpace;
        z = newZ;
    }
}
