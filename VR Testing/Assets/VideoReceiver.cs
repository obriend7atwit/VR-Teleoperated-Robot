using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using PimDeWitte.UnityMainThreadDispatcher;
using System;

public class VideoReceiver : MonoBehaviour
{
    public RawImage rawImage1;
    public RawImage rawImage2;

    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;

    void Start()
    {
        Debug.Log("Attempting to connect to server...");
        try
        {
            client = new TcpClient("10.0.0.249", 12345); // IP Address of the Pi
            stream = client.GetStream();
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.Start();
            Debug.Log("Connected to server.");
        }
        catch (SocketException e)
        {
            Debug.LogError($"SocketException: {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception: {e.Message}");
        }
    }

    void ReceiveData()
    {
        while (true)
        {
            try
            {
                // Receive the size of the first image
                byte[] sizeBuffer = new byte[8];
                int bytesRead = ReadFullBuffer(sizeBuffer);
                if (bytesRead != 8)
                {
                    Debug.LogError("Failed to read the full size of the first image.");
                    break;
                }
                long size1 = System.BitConverter.ToInt64(sizeBuffer, 0);

                // Receive the first image data
                byte[] imageBuffer1 = new byte[size1];
                bytesRead = ReadFullBuffer(imageBuffer1);
                if (bytesRead != size1)
                {
                    Debug.LogError("Failed to read the full data for the first image.");
                    break;
                }

                // Receive the size of the second image
                bytesRead = ReadFullBuffer(sizeBuffer);
                if (bytesRead != 8)
                {
                    Debug.LogError("Failed to read the full size of the second image.");
                    break;
                }
                long size2 = System.BitConverter.ToInt64(sizeBuffer, 0);

                // Receive the second image data
                byte[] imageBuffer2 = new byte[size2];
                bytesRead = ReadFullBuffer(imageBuffer2);
                if (bytesRead != size2)
                {
                    Debug.LogError("Failed to read the full data for the second image.");
                    break;
                }

                // Update textures on the main thread
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    Debug.Log("Updating textures on main thread.");

                    Texture2D tex1 = new Texture2D(640, 480);
                    tex1.LoadImage(imageBuffer1);
                    rawImage1.texture = tex1;

                    Texture2D tex2 = new Texture2D(640, 480);
                    tex2.LoadImage(imageBuffer2);
                    rawImage2.texture = tex2;
                });
            }
            catch (IOException e)
            {
                Debug.LogError($"IOException: {e.Message}");
                break;
            }
            catch (System.OverflowException e)
            {
                Debug.LogError($"OverflowException: {e.Message}");
                break;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception: {e.Message}");
                break;
            }
        }
    }

    private int ReadFullBuffer(byte[] buffer)
    {
        int totalRead = 0;
        while (totalRead < buffer.Length)
        {
            int bytesRead = stream.Read(buffer, totalRead, buffer.Length - totalRead);
            if (bytesRead == 0)
            {
                Debug.LogError("Stream closed unexpectedly.");
                return totalRead;
            }
            totalRead += bytesRead;
        }
        return totalRead;
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        if (stream != null)
        {
            stream.Close();
        }
        if (client != null)
        {
            client.Close();
        }
        Debug.Log("Application quitting, resources cleaned up.");
    }
}


