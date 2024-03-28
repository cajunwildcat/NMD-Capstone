using System;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TCPClient : MonoBehaviour
{
    // Change these variables to match the IP address and port of your Python server
    private string serverAddress = "127.0.0.1";
    private int serverPort = 10000;

    private TcpClient client;
    private NetworkStream stream;
    private byte[] receiveBuffer = new byte[1024];

    public Vector2 min = new Vector2(-7, -4.3f);
    public Vector2 max = new Vector2(7.4f, 6.3f);
    Vector2 pos;
    public bool forMask = false;

    private void Start()
    {
        try
        {
            // Connect to the TCP server
            client = new TcpClient(serverAddress, serverPort);
            stream = client.GetStream();
            Debug.Log("Connected to TCP server.");

            // Start receiving data asynchronously
            stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, ReceiveCallback, null);
        }
        catch (Exception e)
        {
            Debug.LogError("Error connecting to TCP server: " + e.Message);
        }
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            int bytesRead = stream.EndRead(result);
            if (bytesRead > 0)
            {
                // Convert the received bytes to a string
                string data = System.Text.Encoding.ASCII.GetString(receiveBuffer, 0, bytesRead);

                // Split the string into x and y coordinates
                string[] coordinates = data.Split(',');
                if (coordinates.Length == 2)
                {
                    float x = float.Parse(coordinates[0]);
                    float y = float.Parse(coordinates[1]);
                    //Debug.Log("Received coordinates: x=" + x + ", y=" + y);
                    pos = new Vector2(x,y);
                }
            }

            // Continue receiving data asynchronously
            stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, ReceiveCallback, null);
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving data: " + e.Message);
#if UNITY_EDITOR
            EditorApplication.Exit(0);
#else
            Application.Quit();
#endif
        }
    }

    private void Update() {
        Vector3 newPos = new Vector3(Mathf.Lerp(min.x, max.y, pos.x), Mathf.Lerp(max.y, min.y,pos.y), 0);
        if (forMask && newPos != transform.position) {
            GameObject babyMask = Instantiate(gameObject, transform.position, Quaternion.identity);
            DestroyImmediate(babyMask.GetComponent<TCPClient>());
        }
        transform.position = newPos;
    }

    private void OnDestroy()
    {
        // Close the TCP connection when the script is destroyed
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
    }
}
