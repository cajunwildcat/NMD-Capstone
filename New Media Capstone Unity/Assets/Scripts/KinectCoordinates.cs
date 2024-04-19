using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Windows.Kinect;

public class KinectCoordinates : MonoBehaviour {
    static KinectCoordinates instance;
    public static KinectCoordinates Instance => instance;
    public void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            instance.switchXZ = switchXZ;
            //instance.flipLong = flipLong;
            //instance.flipShort = flipShort;
            instance.trackerScale = trackerScale;
            instance.min = min;
            instance.max = max;
            if (instance.flipLong) {
                float temp = min.x;
                instance.min.x = max.x;
                instance.max.x = temp;
            }
            if (instance.flipShort) {
                float temp = min.y;
                instance.min.y = max.y;
                instance.max.y = temp;
            }
            //instance.kinectDepthCutOffs = kinectDepthCutOffs;
            //instance.kinectXOffset = kinectXOffset;

            Destroy(gameObject);
        }
    }

    public KinectSensor sensor;
    public BodyFrameReader bodyFrameReaders;
    public GameObject peopleFollower;

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

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(max+min, new Vector3(Mathf.Abs(min.x) + Mathf.Abs(max.x), Mathf.Abs(min.y) + Mathf.Abs(max.y)));
    }

    void Start() {
        sensor = KinectSensor.GetDefault(); //gets the Kinect Sensor connected to current PC. only one allowed per PC
        bodyFrameReaders = sensor.BodyFrameSource.OpenReader(); //reads the body frame picked up by the Kinect

        // lambda function that calls BodyFrameArrived method
        //actively sends over the data to determine position.
        bodyFrameReaders.FrameArrived += (sender, args) => BodyFrameArrived(sender, args); 
        //sensor must be set to open to operate
        sensor.Open();
        
        if (flipLong) {
            float temp = min.x;
            min.x = max.x;
            max.x = temp;
        }
        if (flipShort) {
            float temp = min.y;
            min.y = max.y;
            max.y = temp;
        }


        //MakeDummyTracker();
    }

    //sensor must be unsubscribed from closing the scene / stopping the play
    void OnDestroy() {
        sensor?.Close();
    }

    //update user position
    private void Update() {
        transform.position = pos;
        if (Input.GetKeyDown(KeyCode.Space)) {
            SetXZero();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            SetKinectMin();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            SetKinectMax();
        }
    }

    private void SetXZero() {
        setNextZero = true;
    }

    private void SetKinectMin() {
        setNextDepthMin = true;
    }

    private void SetKinectMax() {
        setNextDepthMax = true;
    }

    // Define a dictionary to keep track of the GameObjects associated with each TrackingId
    Dictionary<ulong, GameObject> trackedPeople = new Dictionary<ulong, GameObject>();

    private void BodyFrameArrived(object sender, BodyFrameArrivedEventArgs args) {
        using (var bodyFrame = args.FrameReference.AcquireFrame()) {
            if (bodyFrame == null) {
                return;
            }

            Body[] bodies = new Body[sensor.BodyFrameSource.BodyCount];
            bodyFrame.GetAndRefreshBodyData(bodies);

            // Iterate through each body
            foreach (var body in bodies) {
                if (body.IsTracked) {
                    ulong trackingId = body.TrackingId;

                    // Get top-down coordinates of the tracked body
                    CameraSpacePoint position = body.Joints[JointType.SpineMid].Position;
                    position.X += kinectXOffset;
                    if (Mathf.Abs(position.X) > 1.5f) continue;

                    // Check if we already have a GameObject associated with this trackingId
                    if (!trackedPeople.ContainsKey(trackingId)) {
                        // If not, instantiate a new child object from the peopleFollower prefab
                        GameObject newPerson = Instantiate(peopleFollower, transform.position, Quaternion.identity);
                        newPerson.transform.localScale = Vector3.one * trackerScale;
                        trackedPeople.Add(trackingId, newPerson);
                        DontDestroyOnLoad(newPerson);
                    }

                    // Get the GameObject associated with this trackingId
                    GameObject personObject = trackedPeople[trackingId];



                    if (setNextZero) {
                        kinectXOffset = position.X * -1;
                        transform.GetChild(0).gameObject.SetActive(false);
                        setNextZero = false;
                    }
                    if (setNextDepthMin) {
                        kinectDepthCutOffs.x = position.Z;
                        setNextDepthMin = false;
                    }
                    if (setNextDepthMax) {
                        kinectDepthCutOffs.y = position.Z;
                        setNextDepthMax = false;
                    }

                    //Debug.Log(position);

                    // Calculate the position in Unity coordinates
                    float x, y;
                    if (switchXZ) {
                        y = Mathf.Lerp(min.y, max.y, (position.X + 1) / 2);
                        float l = position.Z;
                        float t;
                        if (l == kinectDepthCutOffs.x) t = 0;
                        else t = (l - kinectDepthCutOffs.x) / (kinectDepthCutOffs.y - kinectDepthCutOffs.x);
                        x = Mathf.Lerp(min.x, max.x, t);
                    }
                    else {
                        x = Mathf.Lerp(min.x, max.x, (position.X + 1) / 2);
                        float l = position.Z;
                        float t = (l - kinectDepthCutOffs.x) / (kinectDepthCutOffs.y - kinectDepthCutOffs.x);
                        y = Mathf.Lerp(min.y, max.y, t);
                    }

                    // Update the position of the personObject

                    personObject.transform.position = new Vector3(x, y, 0);
                }
            }

            // Check for any tracked people who are no longer being tracked and remove their GameObjects
            List<ulong> toRemove = new List<ulong>();
            foreach (var kvp in trackedPeople) {
                ulong trackingId = kvp.Key;
                GameObject personObject = kvp.Value;
                bool isTracked = Array.Exists(bodies, b => b.IsTracked && b.TrackingId == trackingId);
                if (!isTracked) {
                    // If the person is no longer tracked, mark it for removal
                    toRemove.Add(trackingId);
                    // Destroy the associated GameObject
                    Destroy(personObject);
                }
            }

            // Remove the tracked people who are no longer being tracked from the dictionary
            foreach (var key in toRemove) {
                trackedPeople.Remove(key);
            }
        }
    }

    public Vector3 GetNearestFollower(Vector3 point) {
        if (trackedPeople.Count < 1) return Vector3.zero;
        float minDist = float.MaxValue;
        ulong minID = 0;
        foreach (ulong trackers in trackedPeople.Keys) {
            float dist = Vector3.Distance(trackedPeople[trackers].transform.position,point);
            if (dist < minDist) {
                minDist = dist;
                minID = trackers;
            }
        }
        return trackedPeople[minID].transform.position;
    }

    public List<Vector3> GetAllNormalizedTrackerPositions() {
        List<Vector3> positions = new();
        foreach (ulong trackers in trackedPeople.Keys) {
            Vector3 trackerPos = trackedPeople[trackers].transform.position;
            float xl = trackerPos.x;
            float x = (xl - min.x) / (max.x - min.x);
            float yl = trackerPos.y;
            float y = (yl - min.y) / (max.y - min.y);

            positions.Add(new Vector3(x, y, 0));
        }
        return positions;
    }

    public List<GameObject> GetAllTrackerObjects() {
        List<GameObject> trackers = new();
        foreach (ulong trackerID in trackedPeople.Keys) {
            trackers.Add(trackedPeople[trackerID]);
        }
        return trackers;
    }

    public List<Vector3> GetAllTrackerWorldPositions() {
        List<Vector3> positions = new();
        foreach (ulong trackerID in trackedPeople.Keys) {
            positions.Add(trackedPeople[trackerID].transform.position);
        }
        return positions;
    }

    [ContextMenu("Make Dummy Tracker")]
    public void MakeDummyTracker() {
        GameObject newTracker = Instantiate(peopleFollower, transform.position, Quaternion.identity);
        newTracker.transform.localScale = Vector3.one * trackerScale;
        trackedPeople.Add((ulong)trackedPeople.Count, newTracker);
        DontDestroyOnLoad(newTracker);
    }
}