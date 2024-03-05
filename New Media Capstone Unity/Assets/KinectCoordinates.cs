using System;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class KinectCoordinates : MonoBehaviour {
    public KinectSensor sensor;
    public BodyFrameReader bodyFrameReaders;
    public GameObject peopleFollower;

    public Vector2 min;
    public Vector2 max;

    private Vector3 pos;

    void Start() {
        sensor = KinectSensor.GetDefault();
        bodyFrameReaders = sensor.BodyFrameSource.OpenReader();

        bodyFrameReaders.FrameArrived += (sender, args) => BodyFrameArrived(sender, args);

        sensor.Open();
    }

    void OnDestroy() {
        // Dispose body frame readers and close Kinect sensors

        bodyFrameReaders.Dispose();

        sensor.Close();
    }

    private void Update() {
        transform.position = pos;
    }

    Vector2 kinectDepth = new Vector2(1.2f, 4f);

    /*private void BodyFrameArrived(object sender, BodyFrameArrivedEventArgs args) {
        using (var bodyFrame = args.FrameReference.AcquireFrame()) {
            if (bodyFrame == null) {
                return;
            }

            Body[] bodies = new Body[sensors.BodyFrameSource.BodyCount];
            bodyFrame.GetAndRefreshBodyData(bodies);

            foreach (var body in bodies) {
                if (body.IsTracked) {
                    // Get top-down coordinates of the tracked body
                    CameraSpacePoint position = body.Joints[JointType.SpineMid].Position;

                    // Output coordinates to console
                    float x = Mathf.Lerp(min.x, max.x, (position.X + 1) / 2);
                    //float y = Mathf.Lerp(min.y, max.y, position.Z);
                    float l = position.Z;
                    float t = (l-kinectDepth.y)/(kinectDepth.x -  kinectDepth.y);
                    float y = Mathf.Lerp(min.y, max.y, t);
                    //Debug.Log($"X: {x}, Y: {y}");
                    pos = new Vector3(x, y, 0);
                }
            }
        }
    }*/
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

                    // Check if we already have a GameObject associated with this trackingId
                    if (!trackedPeople.ContainsKey(trackingId)) {
                        // If not, instantiate a new child object from the peopleFollower prefab
                        GameObject newPerson = Instantiate(peopleFollower, transform.position, Quaternion.identity);
                        trackedPeople.Add(trackingId, newPerson);
                    }

                    // Get the GameObject associated with this trackingId
                    GameObject personObject = trackedPeople[trackingId];

                    // Get top-down coordinates of the tracked body
                    CameraSpacePoint position = body.Joints[JointType.SpineMid].Position;

                    // Calculate the position in Unity coordinates
                    float x = Mathf.Lerp(min.x, max.x, (position.X + 1) / 2);
                    float l = position.Z;
                    float t = (l - kinectDepth.y) / (kinectDepth.x - kinectDepth.y);
                    float y = Mathf.Lerp(min.y, max.y, t);

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

}
