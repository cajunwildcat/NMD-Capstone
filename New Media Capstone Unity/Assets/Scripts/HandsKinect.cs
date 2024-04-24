using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Windows.Kinect;
using UnityEngine.SceneManagement;
using System.Linq;

[System.Serializable]
public struct TrackedPersonData {
    public ulong TrackingId;
    public Vector3 KinectCoordinates;
    public Vector3 UnityWorldCoordinates;
}
public class HandsKinect : MonoBehaviour {
    #region Singletone
    static HandsKinect instance;
    public static HandsKinect Instance => instance;

    public void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            instance.switchXZ = switchXZ;
            instance.flipLong = flipLong;
            instance.flipShort = flipShort;
            instance.trackerScale = trackerScale;
            instance.min = min;
            instance.max = max;
            if (flipLong) {
                float temp = min.x;
                instance.min.x = max.x;
                instance.max.x = temp;
            }
            if (flipShort) {
                float temp = min.y;
                instance.min.y = max.y;
                instance.max.y = temp;
            }
            Destroy(gameObject);
        }
    }
    #endregion

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
    public Vector2 kinectDepthCutOffs = new Vector2(0f, 4.5f);
    float kinectXOffset;

    bool setNextZero = false;
    bool setNextDepthMin = false;
    bool setNextDepthMax = false;

    // New variables for the corners of the depth recognition area
    public Vector2 topLeft, topRight, bottomLeft, bottomRight;

    [SerializeField]
    private List<TrackedPersonData> trackedPeopleDisplayList = new List<TrackedPersonData>();

    // Define a dictionary to keep track of the GameObjects associated with each TrackingId
    Dictionary<ulong, TrackedPerson> trackedPeople = new Dictionary<ulong, TrackedPerson>();

     private bool canChangeGradient = true;
    private float gestureCooldown = 1.0f; // Cooldown period in seconds after a gesture is recognized
    private float lastGestureTime = -1.0f; // Time at which the last gesture was recognized

    public class TrackedPerson {
        public GameObject bodyObject;
        public GameObject leftHandObject;
        public GameObject rightHandObject;

        public Vector3 lastKinectPosition;
        public float timeSinceLastUpdate = 0;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(max + min, new Vector3(Mathf.Abs(min.x) + Mathf.Abs(max.x), Mathf.Abs(min.y) + Mathf.Abs(max.y)));
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
    }

    //sensor must be unsubscribed from to close the scene / stop the play
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

        // Calculate and update the corners of the depth recognition area
        CalculateDepthCorners();
        UpdateTrackedPeopleDisplayList();
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

    public List<(ulong TrackingId, Vector3 KinectCoordinates, Vector3 UnityWorldCoordinates)> GetTrackedPeopleCoordinates() {
        var results = new List<(ulong, Vector3, Vector3)>();

        foreach (var entry in trackedPeople) {
            ulong trackingId = entry.Key;
            TrackedPerson person = entry.Value;

            Vector3 kinectCoordinates = person.lastKinectPosition; // Kinect coordinates
            Vector3 unityWorldCoordinates = person.bodyObject.transform.position; // Unity world coordinates

            results.Add((trackingId, kinectCoordinates, unityWorldCoordinates));
        }

        return results;
    }

    void UpdateTrackedPeopleDisplayList() {
        var trackedData = GetTrackedPeopleCoordinates();
        trackedPeopleDisplayList.Clear();

        foreach (var data in trackedData) {
            TrackedPersonData displayData = new TrackedPersonData {
                TrackingId = data.TrackingId,
                KinectCoordinates = data.KinectCoordinates,
                UnityWorldCoordinates = data.UnityWorldCoordinates
            };

            trackedPeopleDisplayList.Add(displayData);
        }
    }

    private void BodyFrameArrived(object sender, BodyFrameArrivedEventArgs args) {
        using (var bodyFrame = args.FrameReference.AcquireFrame()) {
            if (bodyFrame == null) { return; }

            Body[] bodies = new Body[sensor.BodyFrameSource.BodyCount];
            bodyFrame.GetAndRefreshBodyData(bodies);
            GradientChangeOnGesture(bodies);

            // Iterate through each body
            foreach (var body in bodies.Where((b)=>b.IsTracked)) {
                ulong trackingId = body.TrackingId;

                // Check if we already have a GameObject associated with this trackingId
                if (!trackedPeople.ContainsKey(trackingId)) {
                    AddNewTrackedPerson(trackingId);
                }

                var person = trackedPeople[trackingId];

                // Get the GameObject associated with this trackingId
                //UpdateBodyPosition(body, person.bodyObject);
                UpdateTrackerPosition(body, person.bodyObject, JointType.SpineMid);
                UpdateTrackerPosition(body, person.leftHandObject, JointType.HandLeft);
                UpdateTrackerPosition(body, person.rightHandObject, JointType.HandRight);

            }

            // Check for any tracked people who are no longer being tracked and remove their GameObjects
            List<ulong> toRemove = new List<ulong>();
            foreach (var kvp in trackedPeople) {
                if (!Array.Exists(bodies, b => b.IsTracked && b.TrackingId == kvp.Key)) {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var key in toRemove) {
                var person = trackedPeople[key];
                Destroy(person.bodyObject);
                Destroy(person.leftHandObject);
                Destroy(person.rightHandObject);
                trackedPeople.Remove(key);
            }
        }
    }

    private void AddNewTrackedPerson(ulong trackingId) {
        var newPerson = new TrackedPerson {
            bodyObject = Instantiate(peopleFollower, Vector3.zero, Quaternion.identity),
            leftHandObject = Instantiate(handPrefab, Vector3.zero, Quaternion.identity),
            rightHandObject = Instantiate(handPrefab, Vector3.zero, Quaternion.identity)
        };

        newPerson.bodyObject.transform.localScale = Vector3.one * trackerScale;
        DontDestroyOnLoad(newPerson.bodyObject);
        newPerson.leftHandObject.transform.SetParent(newPerson.bodyObject.transform);
        newPerson.rightHandObject.transform.SetParent(newPerson.bodyObject.transform);
        trackedPeople.Add(trackingId, newPerson);
    }

    /*private void UpdateBodyPosition(CustomBody body) {
        CameraSpacePoint position = body.Joints[JointType.SpineMid].Position;
        position.X += kinectXOffset; // Adjust the X position


        foreach (var person in trackedPeople.Values) {
            if (person.bodyObject == bodyObject) {
                person.lastKinectPosition = new Vector3(position.X, position.Y, position.Z);
                break;
            }
        }


        float x, y;
        if (switchXZ) {
            y = Mathf.Lerp(min.y, max.y, (position.X + 1) / 2);
            float l = position.Z;
            // Ensure l is within the bounds of kinectDepthCutOffs before normalization
            float t = (l <= kinectDepthCutOffs.x) ? 0 :
                      (l >= kinectDepthCutOffs.y) ? 1 :
                      (l - kinectDepthCutOffs.x) / (kinectDepthCutOffs.y - kinectDepthCutOffs.x);
            x = Mathf.Lerp(min.x, max.x, t);
        }
        else {
            x = Mathf.Lerp(min.x, max.x, (position.X + 1) / 2);
            float l = position.Z;
            // Normalization with respect to Z depth
            float t = (l <= kinectDepthCutOffs.x) ? 0 :
                      (l >= kinectDepthCutOffs.y) ? 1 :
                      (l - kinectDepthCutOffs.x) / (kinectDepthCutOffs.y - kinectDepthCutOffs.x);
            y = Mathf.Lerp(min.y, max.y, t);
        }

        bodyObject.transform.position = new Vector3(x, y, 0);
    }*/

    private void UpdateTrackerPosition(Body body, GameObject trackerObjects, JointType jointType) {
        CameraSpacePoint position = body.Joints[jointType].Position;
        position.X += kinectXOffset;

        float x, y;
        if (switchXZ) {
            y = Mathf.Lerp(min.y, max.y, (position.X + 1) / 2);
            float l = position.Z;
            // Ensure l is within the bounds of kinectDepthCutOffs before normalization
            float t = (l <= kinectDepthCutOffs.x) ? 0 :
                      (l >= kinectDepthCutOffs.y) ? 1 :
                      (l - kinectDepthCutOffs.x) / (kinectDepthCutOffs.y - kinectDepthCutOffs.x);
            x = Mathf.Lerp(min.x, max.x, t);
        }
        else {
            x = Mathf.Lerp(min.x, max.x, (position.X + 1) / 2);
            float l = position.Z;
            // Normalization with respect to Z depth
            float t = (l <= kinectDepthCutOffs.x) ? 0 :
                      (l >= kinectDepthCutOffs.y) ? 1 :
                      (l - kinectDepthCutOffs.x) / (kinectDepthCutOffs.y - kinectDepthCutOffs.x);
            y = Mathf.Lerp(min.y, max.y, t);
        }

        trackerObjects.transform.position = new Vector3(x, y, 0);
    }

    private void GradientChangeOnGesture(Body[] bodies) {
        if (SceneManager.GetActiveScene().name == "Circles2") return;
        if (!canChangeGradient && Time.time - lastGestureTime > gestureCooldown) {
            canChangeGradient = true;
        }

        foreach (var body in bodies) {
            if (body.IsTracked) {
                var headPosition = body.Joints[JointType.Head].Position;
                var leftHandPosition = body.Joints[JointType.HandTipLeft].Position;
                var rightHandPosition = body.Joints[JointType.HandTipRight].Position;

                // Ensure we can change the gradient and the gesture is detected
                if (canChangeGradient && leftHandPosition.Y > headPosition.Y && rightHandPosition.Y > headPosition.Y) {
                    FindFirstObjectByType<GradientChanger>().CycleGradient();
                    lastGestureTime = Time.time; // Record the time of the gesture
                    canChangeGradient = false; // Reset the flag to enter cooldown
                    break; // Exit the loop after handling the gesture
                }
            }
        }
    }

    #region Helper Functions To Get Positions
    void CalculateDepthCorners() {
        float depth = (kinectDepthCutOffs.x + kinectDepthCutOffs.y) / 2; // Example: using mid-depth for calculation
        float hFov = Mathf.Deg2Rad * 70.0f / 2; // Kinect horizontal FOV in radians divided by 2
        float vFov = Mathf.Deg2Rad * 60.0f / 2; // Kinect vertical FOV in radians divided by 2

        float hMax = depth * Mathf.Tan(hFov);
        float vMax = depth * Mathf.Tan(vFov);

        // Assuming the Kinect is centered at (0,0) in its own coordinate system
        topLeft = new Vector2(-hMax, vMax);
        topRight = new Vector2(hMax, vMax);
        bottomLeft = new Vector2(-hMax, -vMax);
        bottomRight = new Vector2(hMax, -vMax);

        // Now topLeft, topRight, bottomLeft, and bottomRight contain the corners
    }

    public List<Vector3> GetAllNormalizedTrackerPositions() {
        List<Vector3> positions = new List<Vector3>();
        foreach (ulong trackerID in trackedPeople.Keys) {
            Vector3 trackerPos = trackedPeople[trackerID].bodyObject.transform.position;
            float x1 = trackerPos.x;
            float x = (x1 - min.x) / (max.x - min.x);
            float y1 = trackerPos.y;
            float y = (y1 - min.y) / (max.y - min.y);

            positions.Add(new Vector3(x, y, 0));
        }
        return positions;
    }

    public List<Tuple<GameObject, Vector3>> GetAllTrackerPositions() {
        List<Tuple<GameObject, Vector3>> positions = new List<Tuple<GameObject, Vector3>>();
        foreach (ulong trackerID in trackedPeople.Keys) {
            Vector3 trackerPos = trackedPeople[trackerID].bodyObject.transform.position;
            positions.Add(new Tuple<GameObject, Vector3>(trackedPeople[trackerID].bodyObject, trackerPos));
        }
        return positions;
    }

    public List<GameObject> GetAllTrackerObjects() {
        List<GameObject> trackers = new();
        foreach (ulong trackerID in trackedPeople.Keys) {
            trackers.Add(trackedPeople[trackerID].bodyObject);
        }
        return trackers;
    }

    public List<Vector3> GetAllTrackerWorldPositions() {
        List<Vector3> positions = new();
        foreach (ulong trackerID in trackedPeople.Keys) {
            positions.Add(trackedPeople[trackerID].bodyObject.transform.position);
        }
        return positions;
    }
    #endregion

    [ContextMenu("Make Dummy Tracker")]
    public void MakeDummyTracker() {
        GameObject bodyObject = Instantiate(peopleFollower, transform.position, Quaternion.identity);
        bodyObject.transform.localScale = Vector3.one * trackerScale;
        DontDestroyOnLoad(bodyObject);

        GameObject leftHandObject = Instantiate(handPrefab, transform.position, Quaternion.identity);
        GameObject rightHandObject = Instantiate(handPrefab, transform.position, Quaternion.identity);
        DontDestroyOnLoad(leftHandObject);
        DontDestroyOnLoad(rightHandObject);

        TrackedPerson newTracker = new TrackedPerson {
            bodyObject = bodyObject,
            leftHandObject = leftHandObject,
            rightHandObject = rightHandObject


        };

        trackedPeople.Add((ulong)trackedPeople.Count, newTracker);
    }
}