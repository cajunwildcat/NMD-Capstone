using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Windows.Kinect;
using UnityEngine.SceneManagement;

public class HandsKinect : MonoBehaviour
{
    static HandsKinect instance;
    public static HandsKinect Instance => instance;
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            instance.switchXZ = switchXZ;
            instance.flipLong = flipLong;
            instance.flipShort = flipShort;
            instance.trackerScale = 1f;
            instance.min = min;
            instance.max = max;
            if (flipLong)
            {
                float temp = min.x;
                instance.min.x = max.x;
                instance.max.x = temp;
            }
            if (flipShort)
            {
                float temp = min.y;
                instance.min.y = max.y;
                instance.max.y = temp;
            }
            //instance.kinectDepthCutOffs = kinectDepthCutOffs;
            //instance.kinectXOffset = kinectXOffset;

            Destroy(gameObject);
        }
    }

    public GameObject[] gradientObjects;

    public KinectSensor sensor;
    public BodyFrameReader bodyFrameReaders;
    public GameObject peopleFollower;

    public GameObject handPrefab;

    public Vector2 min;
    public Vector2 max;

    private Vector3 pos;

    public bool switchXZ = false;
    public bool flipLong = false;
    public bool flipShort = false;
    public float trackerScale = 1f;
    public Vector2 kinectDepthCutOffs = new Vector2(0f, 5f);
    float kinectXOffset;

    bool setNextZero = false;
    bool setNextDepthMin = false;
    bool setNextDepthMax = false;

    public class TrackedPerson
     {
        public GameObject bodyObject;
        public GameObject leftHandObject;
        public GameObject rightHandObject;
     }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(max + min, new Vector3(Mathf.Abs(min.x) + Mathf.Abs(max.x), Mathf.Abs(min.y) + Mathf.Abs(max.y)));
    }

    void Start()
    {
        sensor = KinectSensor.GetDefault(); //gets the Kinect Sensor connected to current PC. only one allowed per PC
        bodyFrameReaders = sensor.BodyFrameSource.OpenReader(); //reads the body frame picked up by the Kinect

        // lambda function that calls BodyFrameArrived method
        //actively sends over the data to determine position.
        bodyFrameReaders.FrameArrived += (sender, args) => BodyFrameArrived(sender, args);
        //sensor must be set to open to operate
        sensor.Open();

        if (flipLong)
        {
            float temp = min.x;
            min.x = max.x;
            max.x = temp;
        }
        if (flipShort)
        {
            float temp = min.y;
            min.y = max.y;
            max.y = temp;
        }
    }

    //sensor must be unsubscribed from to close the scene / stop the play
    void OnDestroy()
    {
        sensor?.Close();
    }

    //update user position
    private void Update()
    {
        transform.position = pos;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetXZero();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetKinectMin();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetKinectMax();
        }
    }

    private void SetXZero()
    {
        setNextZero = true;
    }

    private void SetKinectMin()
    {
        setNextDepthMin = true;
    }

    private void SetKinectMax()
    {
        setNextDepthMax = true;
    }

    // Define a dictionary to keep track of the GameObjects associated with each TrackingId
   // Dictionary<ulong, GameObject> trackedPeople = new Dictionary<ulong, GameObject>();
    Dictionary<ulong, TrackedPerson> trackedPeople = new Dictionary<ulong, TrackedPerson>();
    private void BodyFrameArrived(object sender, BodyFrameArrivedEventArgs args)
    {
        using (var bodyFrame = args.FrameReference.AcquireFrame())
        {
            if (bodyFrame == null)
            {
                return;
            }

            Body[] bodies = new Body[sensor.BodyFrameSource.BodyCount];
            bodyFrame.GetAndRefreshBodyData(bodies);
            GradientChangeOnGesture(bodies);
           // CheckForRaisedHandsAndChangeScene(bodies);

            // Iterate through each body
            foreach (var body in bodies)
            {
                if (body.IsTracked)
                {
                    ulong trackingId = body.TrackingId;

                    // Check if we already have a GameObject associated with this trackingId
                    if (!trackedPeople.ContainsKey(trackingId))
                    {

                        var newPerson = new TrackedPerson
                        {
                            bodyObject = Instantiate(peopleFollower, Vector3.zero, Quaternion.identity),
                            leftHandObject = Instantiate(handPrefab, Vector3.zero, Quaternion.identity),
                            rightHandObject = Instantiate(handPrefab, Vector3.zero, Quaternion.identity)
                        };
                        // If not, instantiate a new child object from the peopleFollower prefab
                        // GameObject newPerson = Instantiate(peopleFollower, transform.position, Quaternion.identity);

                        newPerson.bodyObject.transform.localScale = Vector3.one * trackerScale;
                        DontDestroyOnLoad(newPerson.bodyObject);
                        DontDestroyOnLoad(newPerson.leftHandObject);
                        DontDestroyOnLoad(newPerson.rightHandObject);
                        trackedPeople.Add(trackingId, newPerson);

                        //  newPerson.transform.localScale = Vector3.one * trackerScale;
                        // trackedPeople.Add(trackingId, newPerson);
                        //  DontDestroyOnLoad(newPerson);
                    }

                    var person = trackedPeople[trackingId];

                    // Get the GameObject associated with this trackingId
                    UpdateBodyPosition(body, person.bodyObject);
                    UpdateHandPosition(body, person.leftHandObject, JointType.HandLeft);
                    UpdateHandPosition(body, person.rightHandObject, JointType.HandRight);

                    // Get top-down coordinates of the tracked body
/*                    CameraSpacePoint position = body.Joints[JointType.SpineMid].Position;

                    if (setNextZero)
                    {
                        kinectXOffset = position.X * -1;
                        transform.GetChild(0).gameObject.SetActive(false);
                        setNextZero = false;
                    }
                    if (setNextDepthMin)
                    {
                        kinectDepthCutOffs.x = position.Z;
                        setNextDepthMin = false;
                    }
                    if (setNextDepthMax)
                    {
                        kinectDepthCutOffs.y = position.Z;
                        setNextDepthMax = false;
                    }

                    position.X += kinectXOffset;*/

                    //Debug.Log(position);

                    // Calculate the position in Unity coordinates
/*                    float x, y;
                    if (switchXZ)
                    {
                        y = Mathf.Lerp(min.y, max.y, (position.X + 1) / 2);
                        float l = position.Z;
                        float t;
                        if (l == kinectDepthCutOffs.x) t = 0;
                        else t = (l - kinectDepthCutOffs.x) / (kinectDepthCutOffs.y - kinectDepthCutOffs.x);
                        x = Mathf.Lerp(min.x, max.x, t);
                    }
                    else
                    {
                        x = Mathf.Lerp(min.x, max.x, (position.X + 1) / 2);
                        float l = position.Z;
                        float t = (l - kinectDepthCutOffs.x) / (kinectDepthCutOffs.y - kinectDepthCutOffs.x);
                        y = Mathf.Lerp(min.y, max.y, t);
                    }*/

                    // Update the position of the personObject

                  //  personObject.transform.position = new Vector3(x, y, 0);
                }
            }

            // Check for any tracked people who are no longer being tracked and remove their GameObjects
            List<ulong> toRemove = new List<ulong>();
            foreach (var kvp in trackedPeople)
            {
                if (!Array.Exists(bodies, b => b.IsTracked && b.TrackingId == kvp.Key))
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var key in toRemove)
            {
                var person = trackedPeople[key];
                Destroy(person.bodyObject);
                Destroy(person.leftHandObject);
                Destroy(person.rightHandObject);
                trackedPeople.Remove(key);
            }
        

           /* foreach (var kvp in trackedPeople)
            {
                ulong trackingId = kvp.Key;
                GameObject personObject = kvp.Value;
                bool isTracked = Array.Exists(bodies, b => b.IsTracked && b.TrackingId == trackingId);
                if (!isTracked)
                {
                    // If the person is no longer tracked, mark it for removal
                    toRemove.Add(trackingId);
                    // Destroy the associated GameObject
                    Destroy(personObject);
                }
            }

            // Remove the tracked people who are no longer being tracked from the dictionary
            foreach (var key in toRemove)
            {
                trackedPeople.Remove(key);
            }*/
        }
    }





    public void CycleSpriteRenderersOrder()
    {
        if (gradientObjects.Length == 0) return;

        // Get the first GameObject's SpriteRenderer sorting order
        int firstOrder = gradientObjects[0].GetComponent<SpriteRenderer>().sortingOrder;

        // Corrected the loop condition to iterate through all elements except the last one
        for (int i = 0; i < gradientObjects.Length - 1; i++)
        {
            gradientObjects[i].GetComponent<SpriteRenderer>().sortingOrder = gradientObjects[i + 1].GetComponent<SpriteRenderer>().sortingOrder;
        }

        // Set the last GameObject's SpriteRenderer sorting order to what was initially the first's
        gradientObjects[gradientObjects.Length - 1].GetComponent<SpriteRenderer>().sortingOrder = firstOrder;

        // This additional step cycles the GameObjects in the array, not just their sorting orders
        GameObject firstGradientObject = gradientObjects[0];
        Array.Copy(gradientObjects, 1, gradientObjects, 0, gradientObjects.Length - 1);
        gradientObjects[gradientObjects.Length - 1] = firstGradientObject;
    }


    private void UpdateBodyPosition(Body body, GameObject bodyObject)
    {
        CameraSpacePoint position = body.Joints[JointType.SpineMid].Position;
        position.X += kinectXOffset; // Adjust the X position

        float x, y;
        if (switchXZ)
        {
            y = Mathf.Lerp(min.y, max.y, (position.X + 1) / 2);
            float l = position.Z;
            // Ensure l is within the bounds of kinectDepthCutOffs before normalization
            float t = (l <= kinectDepthCutOffs.x) ? 0 :
                      (l >= kinectDepthCutOffs.y) ? 1 :
                      (l - kinectDepthCutOffs.x) / (kinectDepthCutOffs.y - kinectDepthCutOffs.x);
            x = Mathf.Lerp(min.x, max.x, t);
        }
        else
        {
            x = Mathf.Lerp(min.x, max.x, (position.X + 1) / 2);
            float l = position.Z;
            // Normalization with respect to Z depth
            float t = (l <= kinectDepthCutOffs.x) ? 0 :
                      (l >= kinectDepthCutOffs.y) ? 1 :
                      (l - kinectDepthCutOffs.x) / (kinectDepthCutOffs.y - kinectDepthCutOffs.x);
            y = Mathf.Lerp(min.y, max.y, t);
        }

        bodyObject.transform.position = new Vector3(x, y, 0);
    }

    private void UpdateHandPosition(Body body, GameObject handObject, JointType handType)
    {
        CameraSpacePoint handPosition = body.Joints[handType].Position;
        handPosition.X += kinectXOffset;

        float x, y;
        if (switchXZ)
        {
            y = Mathf.Lerp(min.y, max.y, (handPosition.X + 1) / 2);
            float l = handPosition.Z;
            float t = (l <= kinectDepthCutOffs.x) ? 0 : (l - kinectDepthCutOffs.x) / (kinectDepthCutOffs.y - kinectDepthCutOffs.x);
            x = Mathf.Lerp(min.x, max.x, t);

        }
        else
        {
            x = Mathf.Lerp(min.x, max.x, (handPosition.X + 1) / 2);
            float l = handPosition.Z;
            float t = (l - kinectDepthCutOffs.x) / (kinectDepthCutOffs.y - kinectDepthCutOffs.x);
            y = Mathf.Lerp(min.y, max.y, t);
        }

        handObject.transform.position = new Vector3(x, y, 0);
    }


    private bool canChangeGradient = true;
    private float gestureCooldown = 1.0f; // Cooldown period in seconds after a gesture is recognized
    private float lastGestureTime = -1.0f; // Time at which the last gesture was recognized

    private void GradientChangeOnGesture(Body[] bodies)
    {
        if(!canChangeGradient && Time.time - lastGestureTime > gestureCooldown)
        {
            canChangeGradient = true;
        }

        foreach (var body in bodies)
        {
            if (body.IsTracked)
            {
                var headPosition = body.Joints[JointType.Head].Position;
                var leftHandPosition = body.Joints[JointType.HandTipLeft].Position;
                var rightHandPosition = body.Joints[JointType.HandTipRight].Position;

                // Ensure we can change the gradient and the gesture is detected
                if (canChangeGradient && leftHandPosition.Y > headPosition.Y && rightHandPosition.Y > headPosition.Y)
                {
                    CycleSpriteRenderersOrder();
                    lastGestureTime = Time.time; // Record the time of the gesture
                    canChangeGradient = false; // Reset the flag to enter cooldown
                    break; // Exit the loop after handling the gesture
                }
            }
        }
    }


/*    private void CheckForRaisedHandsAndChangeScene(Body[] bodies)
    {
        foreach (var body in bodies)
        {
            if (body.IsTracked)
            {
                var headPosition = body.Joints[JointType.Head].Position;
                var leftHandPosition = body.Joints[JointType.HandTipLeft].Position;
                var rightHandPosition = body.Joints[JointType.HandTipRight].Position;

                if(leftHandPosition.Y > headPosition.Y && rightHandPosition.Y > headPosition.Y)
                {
                    ChangeScene();
                    break;
                }
            }
        }
    }*/

    private void ChangeScene()
    {
        SceneManager.LoadScene("Columns");
    }

    public Vector3 GetNearestFollower(Vector3 point)
    {
        if (trackedPeople.Count < 1) return Vector3.zero;
        float minDist = float.MaxValue;
        ulong minID = 0;

        foreach(ulong trackerID in trackedPeople.Keys)
        {
            float dist = Vector3.Distance(trackedPeople[trackerID].bodyObject.transform.position, point);
            if(dist < minDist)
            {
                minDist = dist;
                minID = trackerID;
            }
        }

        /*        foreach (ulong trackers in trackedPeople.Keys)
                {
                    float dist = Vector3.Distance(trackedPeople[trackers].transform.position, point);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        minID = trackers;
                    }
                }
        return trackedPeople[minID].transform.position;
         */
        return trackedPeople[minID].bodyObject.transform.position;
    }
/*
    public List<Vector3> GetAllNormalizedTrackerPositions()
    {
        List<Vector3> positions = new();
        foreach (ulong trackers in trackedPeople.Keys)
        {
            Vector3 trackerPos = trackedPeople[trackers].transform.position;
            float xl = trackerPos.x;
            float x = (xl - min.x) / (max.x - min.x);
            float yl = trackerPos.y;
            float y = (yl - min.y) / (max.y - min.y);

            positions.Add(new Vector3(x, y, 0));
        }
        return positions;
    }*/

    public List<Vector3> GetAllNormalizedTrackerPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        foreach(ulong trackerID in trackedPeople.Keys)
        {
            Vector3 trackerPos = trackedPeople[trackerID].bodyObject.transform.position;
            float x1 = trackerPos.x;
            float x = (x1 - min.x) / (max.x - min.x);
            float y1 = trackerPos.y;
            float y = (y1 - min.y) / (max.y - min.y);

            positions.Add(new Vector3(x, y, 0));
        }
        return positions;
    }

/*    public List<Tuple<GameObject, Vector3>> GetAllTrackerPositions()
    {
        List<Tuple<GameObject, Vector3>> positions = new();
        foreach (ulong trackers in trackedPeople.Keys)
        {
            Vector3 trackerPos = trackedPeople[trackers].transform.position;
            positions.Add(new(trackedPeople[trackers], trackerPos));
        }
        return positions;
    }*/
    public List<Tuple<GameObject, Vector3>> GetAllTrackerPositions()
    {
        List<Tuple<GameObject, Vector3>> positions = new List<Tuple<GameObject, Vector3>>();
        foreach (ulong trackerID in trackedPeople.Keys)
        {
            Vector3 trackerPos = trackedPeople[trackerID].bodyObject.transform.position;
            positions.Add(new Tuple<GameObject, Vector3>(trackedPeople[trackerID].bodyObject, trackerPos));
        }
        return positions; 
    }


/*    [ContextMenu("Make Dummy Tracker")]
    public void MakeDummyTracker()
    {
        GameObject newTracker = Instantiate(peopleFollower, transform.position, Quaternion.identity);
        newTracker.transform.localScale = Vector3.one * trackerScale;
        trackedPeople.Add((ulong)trackedPeople.Count, newTracker);
        DontDestroyOnLoad(newTracker);
    }*/
    [ContextMenu("Make Dummy Tracker")]
    public void MakeDummyTracker()
    {
        GameObject bodyObject = Instantiate(peopleFollower, transform.position, Quaternion.identity);
        bodyObject.transform.localScale = Vector3.one * trackerScale;
        DontDestroyOnLoad(bodyObject);

        GameObject leftHandObject = Instantiate(handPrefab, transform.position, Quaternion.identity);
        GameObject rightHandObject = Instantiate(handPrefab, transform.position, Quaternion.identity);
        DontDestroyOnLoad(leftHandObject);
        DontDestroyOnLoad(rightHandObject);

        TrackedPerson newTracker = new TrackedPerson
        {
            bodyObject = bodyObject,
            leftHandObject = leftHandObject,
            rightHandObject = rightHandObject
        };

        trackedPeople.Add((ulong)trackedPeople.Count, newTracker);
    }
}