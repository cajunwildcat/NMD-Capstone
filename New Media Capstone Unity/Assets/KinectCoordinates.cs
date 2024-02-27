using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class KinectCoordinates : MonoBehaviour {
    public KinectSensor sensors;
    public BodyFrameReader bodyFrameReaders;

    public Vector2 min;
    public Vector2 max;

    private Vector3 pos;

    void Start() {
        sensors = KinectSensor.GetDefault();
        bodyFrameReaders = sensors.BodyFrameSource.OpenReader();

        bodyFrameReaders.FrameArrived += (sender, args) => BodyFrameArrived(sender, args, 0);

        sensors.Open();
    }

    void OnDestroy() {
        // Dispose body frame readers and close Kinect sensors

                bodyFrameReaders.Dispose();

                sensors.Close();
    }

    private void Update() {
        transform.position = pos;
    }

    Vector2 kinectDepth = new Vector2(1.2f, 4f);

    private void BodyFrameArrived(object sender, BodyFrameArrivedEventArgs args, int sensorIndex) {
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
                    Debug.Log($"X: {x}, Y: {y}");
                    pos = new Vector3(x, y, 0);
                }
            }
        }
    }
}
