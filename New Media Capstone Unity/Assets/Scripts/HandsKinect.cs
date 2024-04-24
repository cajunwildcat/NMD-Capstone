using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Windows.Kinect;
using UnityEngine.SceneManagement;
using System.Linq;
using Joint = Windows.Kinect.Joint;
using TreeEditor;

[Serializable]
public struct TrackedPersonData {
    public ulong TrackingId;
    public Vector3 KinectCoordinates;
    public Vector3 UnityWorldCoordinates;
}
public class HandsKinect : MonoBehaviour {
    #region Singleton
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

    private KinectSensor sensor;
    private BodyFrameReader bodyFrameReaders;
    public GameObject bodyFollowerPrefab;
    public GameObject handTrackerPrefab;

    public Vector2 min;
    public Vector2 max;

    [Header("Kinect Settings")]
    public bool switchXZ = false;
    public bool flipLong = false;
    public bool flipShort = false;
    public float trackerScale = 1f;
    public Vector2 kinectDepthCutOffs = new Vector2(0.5f, 4.5f);
    public float kinectXCutOff = 1f;
    float[] kinectXOffset = new float[2] { 0f, 0f };
    Bounds KinectSpaceBounds {
        get {
            return new Bounds(new(0, (kinectDepthCutOffs.y - kinectDepthCutOffs.x) / 2), new(kinectXCutOff * 2, kinectDepthCutOffs.y - kinectDepthCutOffs.x));
        }
    }

    bool[] setNextZero = new bool[2] { false, false };
    bool[] setNextDepthMin = new bool[2] { false, false };
    bool[] setNextDepthMax = new bool[2] { false, false };

    // New variables for the corners of the depth recognition area
    public Vector2 topLeft, topRight, bottomLeft, bottomRight;

    [SerializeField]
    private List<TrackedPersonData> trackedPeopleDisplayList = new List<TrackedPersonData>();

    // Define a dictionary to keep track of the GameObjects associated with each TrackingId
    //Dictionary<ulong, TrackedPerson> trackedPeople = new Dictionary<ulong, TrackedPerson>();
    List<TrackedPerson> trackedPeople = new List<TrackedPerson>();

    private bool canChangeGradient = true;
    private float gestureCooldown = 1.0f; // Cooldown period in seconds after a gesture is recognized
    private float lastGestureTime = -1.0f; // Time at which the last gesture was recognized

    public class TrackedPerson {
        public ulong trackingId;

        public GameObject bodyObject;
        public GameObject leftHandObject;
        public GameObject rightHandObject;

        public Vector3 lastKinectPosition;
        public float timeSinceLastUpdate = 0;
    }

    //Draws a green box outline in world space where the tracker will interpolate kinect position between
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
        transform.position = Vector3.zero;
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
        
        for (int i = 0; i < trackedPeople.Count; i++) {
            if (trackedPeople[i].trackingId < (ulong)trackedPeople.Count) continue;
            trackedPeople[i].timeSinceLastUpdate += Time.deltaTime;
            if (trackedPeople[i].timeSinceLastUpdate > 0.5f) {
                Destroy(trackedPeople[i].bodyObject);
                trackedPeople.RemoveAt(i);
                i--;
            }
        }
    }

    private void SetXZero() {
        setNextZero = new bool[2] { true, true };
    }

    private void SetKinectMin() {
        setNextDepthMin = new bool[2] { true, true };
    }

    private void SetKinectMax() {
        setNextDepthMax = new bool[2] { true, true };
    }

    private TrackedPerson GetTrackerById(ulong id) {
        foreach (TrackedPerson p in trackedPeople) {
            if (p.trackingId == id) return p;
        }
        return null;
    }

    private bool IsTracked(ulong id) {
        return GetTrackerById(id) != null;
    }

    private void BodyFrameArrived(object sender, BodyFrameArrivedEventArgs args) {
        using (var bodyFrame = args.FrameReference.AcquireFrame()) {
            if (bodyFrame == null) { return; }

            Body[] bodies = new Body[sensor.BodyFrameSource.BodyCount];
            bodyFrame.GetAndRefreshBodyData(bodies);
            GradientChangeOnGesture(bodies);

            // Iterate through each body
            foreach (var body in bodies.Where((b)=>b.IsTracked)) {
                CameraSpacePoint pos = body.Joints[JointType.SpineMid].Position;
                if (setNextZero[0]) {
                    kinectXOffset[0] = -pos.X;
                }
                if (!KinectSpaceBounds.Contains(new(pos.X + kinectXOffset[0], pos.Z))) continue;

                ulong trackingId = body.TrackingId;

                // Check if we already have a GameObject associated with this trackingId
                if (!IsTracked(trackingId)) {
                    AddNewTrackedPerson(trackingId);
                }
                var person = GetTrackerById(trackingId);
                UpdateTrackedPerson(person, body);
            }
        }
    }

    private void BodyFrameArrived(CustomBody[] bodies) {
        // Iterate through each body
        foreach (var body in bodies) {
            CameraSpacePoint pos = body.Joints[JointType.SpineMid].Position;
            if (!KinectSpaceBounds.Contains(new(pos.X + kinectXOffset[1], pos.Z))) continue;

            ulong trackingId = body.TrackingId;

            // Check if we already have a GameObject associated with this trackingId
            if (!IsTracked(trackingId)) {
                AddNewTrackedPerson(trackingId);
            }
            var person = GetTrackerById(trackingId);
            UpdateTrackedPerson(person, body);
        }
    }

    private void AddNewTrackedPerson(ulong trackingId) {
        var newPerson = new TrackedPerson {
            trackingId = trackingId,
            bodyObject = Instantiate(bodyFollowerPrefab, Vector3.zero, Quaternion.identity),
            leftHandObject = Instantiate(handTrackerPrefab, Vector3.zero, Quaternion.identity),
            rightHandObject = Instantiate(handTrackerPrefab, Vector3.zero, Quaternion.identity)
        };

        newPerson.bodyObject.transform.localScale = Vector3.one * trackerScale;
        DontDestroyOnLoad(newPerson.bodyObject);
        newPerson.leftHandObject.transform.SetParent(newPerson.bodyObject.transform);
        newPerson.rightHandObject.transform.SetParent(newPerson.bodyObject.transform);
        trackedPeople.Add(newPerson);
    }

    private void UpdateTrackedPerson(TrackedPerson p, Body b) {
        UpdateTrackerPosition(b.Joints[JointType.SpineMid], p.bodyObject);
        UpdateTrackerPosition(b.Joints[JointType.HandLeft], p.leftHandObject);
        UpdateTrackerPosition(b.Joints[JointType.HandRight], p.rightHandObject);
        p.timeSinceLastUpdate = 0;
    }

    private void UpdateTrackerPosition(Joint joint, GameObject trackerObject) {
        CameraSpacePoint position = joint.Position;

        position.X += kinectXOffset[0];

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

        trackerObject.transform.position = new Vector3(x, y, 0);
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

    public List<Vector3> GetAllTrackerWorldPositions() {
        List<Vector3> positions = new();
        foreach (TrackedPerson p in trackedPeople) {
            positions.Add(p.bodyObject.transform.position);
        }
        return positions;
    }
    #endregion

    [ContextMenu("Make Dummy Tracker")]
    public void MakeDummyTracker() {
        AddNewTrackedPerson((ulong)trackedPeople.Count);
    }
}