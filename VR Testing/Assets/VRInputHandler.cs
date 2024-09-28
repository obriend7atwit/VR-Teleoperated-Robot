using System;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRInputHandler : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;

    private UnityEngine.XR.InputDevice leftHandDevice;

    void Start()
    {
        // Connect to the Python script running on the laptop
        client = new TcpClient("10.0.0.249", 12346);
        stream = client.GetStream();

        // Try to find the left hand controller device
        FindLeftHandController();
    }

    void Update()
    {
        if (leftHandDevice.isValid)
        {
            HandleOpenXRInput();
        }
        else
        {
            Debug.LogWarning("Left hand device not valid, attempting to find again.");
            FindLeftHandController();
        }
    }

    void FindLeftHandController()
    {
        Debug.Log("Attempting to find left hand controller...");
        var leftHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller, leftHandDevices);

        if (leftHandDevices.Count > 0)
        {
            leftHandDevice = leftHandDevices[0];
            Debug.Log("Left hand controller found: " + leftHandDevice.name);
        }
        else
        {
            Debug.LogError("Left hand controller not found.");
        }
    }

    void HandleOpenXRInput()
    {
        if (leftHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 leftJoystick))
        {
            Debug.Log("Left joystick position: " + leftJoystick);

            if (leftJoystick.y > 0.5f)
            {
                SendCommand("forward");
            }
            else if (leftJoystick.y < -0.5f)
            {
                SendCommand("backward");
            }
            else if (leftJoystick.x > 0.5f)
            {
                SendCommand("right");
            }
            else if (leftJoystick.x < -0.5f)
            {
                SendCommand("left");
            }
            else
            {
                SendCommand("stop");
            }
        }
    }

    void SendCommand(string command)
    {
        if (stream.CanWrite)
        {
            byte[] commandBytes = Encoding.UTF8.GetBytes(command);
            stream.Write(commandBytes, 0, commandBytes.Length);
        }
    }

    void OnApplicationQuit()
    {
        stream.Close();
        client.Close();
    }
}
