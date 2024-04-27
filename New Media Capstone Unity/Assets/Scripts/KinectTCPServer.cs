using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using Joint = Windows.Kinect.Joint;
using Newtonsoft.Json;
using System.Threading.Tasks;

public struct CustomBody {
    public Joint[] joints;
    public ulong TrackingId;
}

public delegate void NewExtraKinectData();

public class KinectTCPServer : MonoBehaviour {
    public static event NewExtraKinectData NewExtraKinectDataEvent;
    private static KinectTCPServer instance;
    public static KinectTCPServer Instance => instance;

    private static TcpClient client;

    public static CustomBody[] ExtraKinectCoordinates;

    private static bool shouldConnect = false;
    private static float requestCounter = 0;
    private static float requestCooldown = 0.01f;

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
        shouldConnect = true;
        //StartCoroutine(RequestData());
    }

    void OnDestroy()
    {
        shouldConnect = false;
    }
    
    void Update()
    {
        requestCounter += Time.deltaTime;
        if (requestCounter >= requestCooldown) {
            try
            {
                client = new TcpClient();
                if (!client.ConnectAsync("192.168.0.103", 12345).Wait(8)) {
                    //the server isn't running
                    Debug.LogWarning("The server isnt running, cannot get extra kinect data");
                    return;
                }
                NetworkStream stream = client.GetStream();

                int i = 0;
                byte[] buffer = new byte[1024];
                while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string s = System.Text.Encoding.ASCII.GetString(buffer, 0, i);
                    //Debug.Log(s);
                    ExtraKinectCoordinates = JsonConvert.DeserializeObject<CustomBody[]>(s);
                    NewExtraKinectDataEvent?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            requestCounter -= requestCooldown;
        }
    }

    IEnumerator RequestData()
    {
        while (shouldConnect)
        {
            try
            {
                client = new TcpClient();
                client.Connect("192.168.0.101", 12345);
                NetworkStream stream = client.GetStream();

                int i = 0;
                byte[] buffer = new byte[1024];
                while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string s = System.Text.Encoding.ASCII.GetString(buffer, 0, i);
                    //Debug.Log(s);
                    ExtraKinectCoordinates = JsonConvert.DeserializeObject<CustomBody[]>(s);
                    NewExtraKinectDataEvent?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}