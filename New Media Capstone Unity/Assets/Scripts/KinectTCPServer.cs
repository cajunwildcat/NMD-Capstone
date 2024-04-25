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

public delegate void NewExtraKinectData();

public class KinectTCPServer : MonoBehaviour {
    public static event NewExtraKinectData NewExtraKinectDataEvent;
    private static KinectTCPServer instance;
    public static KinectTCPServer Instance => instance;

    private static TcpListener tcpListener;

    public static CustomBody[] ExtraKinectCoordinates;

    bool acceptConnections = true;

    private void Awake() {
        if (instance) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start() {
        tcpListener = new TcpListener(IPAddress.Any,12345); // Port number can be changed
        tcpListener.Start();
        ConnectWithClients();
    }

    private void OnDestroy() {
        acceptConnections = false;
        tcpListener.Stop();
    }

    async void ConnectWithClients() {
        while (acceptConnections) {
            try {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                NetworkStream stream = client.GetStream();

                int i = 0;
                byte[] buffer = new byte[1024];
                while ((i = stream.Read(buffer, 0, buffer.Length)) != 0) {
                    string data = Encoding.ASCII.GetString(buffer, 0, i);
                    ExtraKinectCoordinates = JsonConvert.DeserializeObject<CustomBody[]>(data);
                    NewExtraKinectDataEvent?.Invoke();
                }
            } catch (Exception e) {
                await Console.Out.WriteLineAsync($"Error connecting with/reading client data: {e.Message}");
            }
        }
    }
}