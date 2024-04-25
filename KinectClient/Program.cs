using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Newtonsoft.Json;

public struct CustomBody {
    public Joint[] joints;
    public ulong TrackingId;
}


class KinectBodyTracker {
    private static KinectSensor kinectSensor;
    private static BodyFrameReader bodyFrameReader;
    private static TcpClient tcpClient;
    private static NetworkStream networkStream;
    private static bool isConnected;
    private static bool canSend;

    static void Main(string[] args) {
        // Initialize Kinect sensor
        kinectSensor = KinectSensor.GetDefault();
        if (kinectSensor == null) {
            Console.WriteLine("No Kinect sensor found.");
            return;
        }

        // Open Kinect sensor and start body tracking
        kinectSensor.Open();
        bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
        bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;

        // Start asynchronous connection to TCP server
        //ConnectToServerAsync();

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

        // Clean up resources
        kinectSensor.Close();
        networkStream?.Close();
        tcpClient?.Close();
    }

    private static void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e) {
        using (var bodyFrame = e.FrameReference.AcquireFrame()) {
            if (bodyFrame != null) {
                Body[] bodies = new Body[kinectSensor.BodyFrameSource.BodyCount];
                bodyFrame.GetAndRefreshBodyData(bodies);

                /*foreach (Body body in bodies) {
                    if (body.IsTracked) {                        
                        // Output only SpineMid, HandLeft, and HandRight joint data
                        Console.SetCursorPosition(0, 0); // Move cursor up 5 lines
                        Console.WriteLine($"Body ID: {body.TrackingId}");
                        PrintJointData(body.Joints[JointType.SpineMid], "SpineMid");
                        PrintJointData(body.Joints[JointType.HandLeft], "HandLeft");
                        PrintJointData(body.Joints[JointType.HandRight], "HandRight");
                    }
                }*/

                if (true) {
                    List<CustomBody> bodiesData = new List<CustomBody>();
                    for (int i = 0; i < bodies.Length; i++) {
                        if (!bodies[i].IsTracked) continue;
                        CustomBody cb = new CustomBody();
                        cb.TrackingId = bodies[i].TrackingId;
                        cb.joints = new Joint[3] {
                            bodies[i].Joints[JointType.SpineMid],
                            bodies[i].Joints[JointType.HandLeft],
                            bodies[i].Joints[JointType.HandRight]
                        };
                        bodiesData.Add(cb);
                    }
                    if (bodiesData.Count > 0) {
                        SendJointDataOverTcp(bodiesData.ToArray());
                    }
                }
            }
        }
    }

    private static void PrintJointData(Joint joint, string jointName) {
        Console.WriteLine($"{jointName}: X={joint.Position.X}, Y={joint.Position.Y}, Z={joint.Position.Z}".PadRight(Console.WindowWidth - 1)); // Pad to clear previous data
    }

    private static async void ConnectToServer() {
        isConnected = false;
        while (!isConnected) {
            try {
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(IPAddress.Parse("192.168.0.102"), 12345); // Change IP and port as needed
                networkStream = tcpClient.GetStream();
                isConnected = true;
                canSend = true;
                //Console.WriteLine("Connected to TCP server.");
            } catch (Exception ex) {
                Console.WriteLine($"Error connecting to TCP server: {ex.Message}");
                isConnected = false;
                canSend = false;
                // Retry after .1 seconds
                await Task.Delay(100);
            }
        }
    }

    private static async void SendJointDataOverTcp(Body body) {
        canSend = false;
        Joint[] joints = new Joint[3] {
            body.Joints[JointType.SpineMid],
            body.Joints[JointType.HandRight],
            body.Joints[JointType.HandLeft]
        };
        for (int i = 0; i < joints.Length; i++) {
            joints[i].Position.X = (float)Math.Round(joints[i].Position.X, 2);
            joints[i].Position.Y = (float)Math.Round(joints[i].Position.Y, 2);
            joints[i].Position.Z = (float)Math.Round(joints[i].Position.Z, 2);
        }

        string data = JsonConvert.SerializeObject(joints);
        //Console.WriteLine(data + "                                           ");
        byte[] bytesToSend = Encoding.ASCII.GetBytes(data);
        try {
            networkStream.Write(bytesToSend, 0, bytesToSend.Length);
            networkStream.Flush();
            Console.WriteLine($"Sent joint data over TCP.");
        } catch (Exception ex) {
            Console.WriteLine($"Error sending joint data: {ex.Message}");
        }

        await Task.Delay(100);
        canSend = true;
    }

    private static async void SendJointDataOverTcp(CustomBody[] bodies) {
        while (!isConnected)
        {
            try
            {
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(IPAddress.Parse("192.168.0.102"), 12345); // Change IP and port as needed
                networkStream = tcpClient.GetStream();
                isConnected = true;
                canSend = true;
                //Console.WriteLine("Connected to TCP server.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to TCP server: {ex.Message}");
                isConnected = false;
                canSend = false;
                // Retry after .1 seconds
                await Task.Delay(100);
            }
        }

        
        canSend = false;
        for (int i = 0; i < bodies.Length; i++) {
            for (int j = 0; j < bodies[i].joints.Length; j++) {
                bodies[i].joints[j].Position.X = (float)Math.Round(bodies[i].joints[j].Position.X, 2);
                bodies[i].joints[j].Position.Y = (float)Math.Round(bodies[i].joints[j].Position.Y, 2);
                bodies[i].joints[j].Position.Z = (float)Math.Round(bodies[i].joints[j].Position.Z, 2);
            }
        }

        string json = JsonConvert.SerializeObject(bodies);
        byte[] data = Encoding.ASCII.GetBytes(json);
        Console.SetCursorPosition(0, 0);
        Console.WriteLine(data.Length);        
        Console.WriteLine(json);

        try
        {
            TcpClient client = new TcpClient("192.168.0.102", 12345);
            NetworkStream stream = client.GetStream();

            stream.Write(data, 0, data.Length);

            client.Close();
        } catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
