using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using Joint = Windows.Kinect.Joint;
using Newtonsoft.Json;

public struct CustomBody {
    public Joint[] joints;
    public ulong TrackingId;
}

public class KinectTCPServer : MonoBehaviour {
    private static TcpListener tcpListener;

    private static TcpClient client;
    private static NetworkStream stream;
    private static byte[] receiveBuffer = new byte[1024];

    public static List<CustomBody> ExtraKinectCoordinates = new List<CustomBody>();
    private static Dictionary<NetworkStream, int> extraKinectIndices = new Dictionary<NetworkStream, int>();

    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start() {
        StartServer();
        Debug.Log("TCP server started. Press any key to exit.");
        Console.ReadKey();
        //tcpListener.Stop();
    }

    // Update is called once per frame
    void Update() {

    }

    private static async void StartServer() {
        try {
            tcpListener = new TcpListener(IPAddress.Any, 12345); // Port number can be changed
            tcpListener.Start();

            while (true) {
                client = await tcpListener.AcceptTcpClientAsync();
                Debug.Log("Client connected.");
                stream = client.GetStream();

                stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, HandleClient, null);
                extraKinectIndices.Add(stream, extraKinectIndices.Count);
            }
        } catch (Exception ex) {
            Debug.Log($"Error starting server: {ex.Message}");
        }
    }

    private static void HandleClient(IAsyncResult result) {
        try {
            int bytesRead = stream.EndRead(result);
            if (bytesRead > 0) {
                // Convert the received bytes to a string
                string data = Encoding.ASCII.GetString(receiveBuffer, 0, bytesRead);
                Debug.Log(data);
                CustomBody[] joints = JsonConvert.DeserializeObject<CustomBody[]>(data);
                PrintReceivedData(joints);
            }

            // Continue receiving data asynchronously
            stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, HandleClient, null);
        } catch (Exception e) {
            Debug.Log(e.Message);
            StartServer();
        }
    }

    private static void PrintReceivedData(Joint[] joints) {
        if (joints != null && joints.Length == 3) {
            foreach (Joint joint in joints) {
                Debug.Log($"Received joint data: Type={joint.JointType}, X={joint.Position.X}, Y={joint.Position.Y}, Z={joint.Position.Z}");
            }
        }
        else {
            Debug.Log("Received invalid joint data.");
        }
    }

    private static void PrintReceivedData(CustomBody[] bodies) {
        if (bodies != null && bodies.Length == 3) {
            foreach (CustomBody b in bodies) {
                foreach (Joint joint in b.joints) {
                    Debug.Log($"Received joint data for body {b.TrackingId}: Type={joint.JointType}, X={joint.Position.X}, Y={joint.Position.Y}, Z={joint.Position.Z}");
                }
            }
        }
        else {
            Debug.Log("Received invalid joint data.");
        }
    }
}