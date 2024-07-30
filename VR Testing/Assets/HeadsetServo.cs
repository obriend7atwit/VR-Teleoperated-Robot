using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System;

public class HeadsetServo : MonoBehaviour
{
    public string serverIp = "10.0.0.248";
    public int serverPort = 12349;
    private TcpClient client;
    private NetworkStream stream;

    private void Start()
    {
        ConnectToServer();
        StartCoroutine(SendRotationData());
    }

    private void ConnectToServer()
    {
        try
        {
            client = new TcpClient(serverIp, serverPort);
            stream = client.GetStream();
            Debug.Log("Connected to server.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Connection failed: {e.Message}");
        }
    }

    private IEnumerator SendRotationData()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f); // Adjust the frequency as needed

            if (client == null || !client.Connected)
            {
                ConnectToServer();
                continue;
            }

            // Ensure this runs on the main thread
            try
            {
                var inputDevice = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.Head);
                if (inputDevice.isValid)
                {
                    Quaternion rotation;
                    if (inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out rotation))
                    {
                        // Serialize the rotation data as a string
                        string data = $"{rotation.eulerAngles.y},{rotation.eulerAngles.x}";
                        byte[] dataBytes = Encoding.ASCII.GetBytes(data);
                        stream.Write(dataBytes, 0, dataBytes.Length);
                        Debug.Log($"Sent: {data}");
                    }
                }
                else
                {
                    Debug.LogWarning("Input device is not valid.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error sending data: {e.Message}");
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (stream != null) stream.Close();
        if (client != null) client.Close();
    }
}




/*using UnityEngine;
using UnityEngine.XR;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class HeadsetRotationSender : MonoBehaviour
{
    public string raspberryPiIP = "10.0.0.248";  // Replace with your Raspberry Pi's IP address
    public int port = 12349;

    private TcpClient client;
    private NetworkStream stream;
    private Thread sendThread;
    private bool running = true;

    void Start()
    {
        try
        {
            client = new TcpClient(raspberryPiIP, port);
            stream = client.GetStream();
            sendThread = new Thread(new ThreadStart(SendRotationData));
            sendThread.Start();
            Debug.Log("Connected to Raspberry Pi.");
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException: " + e);
        }
    }

    void SendRotationData()
    {
        InputDevice headset = InputDevices.GetDeviceAtXRNode(XRNode.Head);

        while (running)
        {
            if (headset.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
            {
                float yaw = rotation.eulerAngles.y; // Rotation around the Y axis (left-right)
                float pitch = rotation.eulerAngles.x; // Rotation around the X axis (up-down)

                string data = yaw.ToString("F2") + "," + pitch.ToString("F2");

                byte[] bytes = Encoding.ASCII.GetBytes(data);
                try
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }
                catch (SocketException e)
                {
                    Debug.LogError("SocketException: " + e);
                    running = false;
                }

                Thread.Sleep(50); // Adjust the sleep time as needed
            }
            else
            {
                Debug.LogError("Failed to get headset rotation");
            }
        }
    }

    void OnApplicationQuit()
    {
        running = false;
        sendThread.Join();
        stream.Close();
        client.Close();
    }
}*/

