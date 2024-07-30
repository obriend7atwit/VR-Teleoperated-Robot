using UnityEngine;
using UnityEngine.XR;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using PimDeWitte.UnityMainThreadDispatcher;
using System;

public class VideoReceiver : MonoBehaviour
{
    public RenderTexture LeftEyeRenderTexture;
    public RenderTexture RightEyeRenderTexture;

    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;

    private Texture2D tex1;
    private Texture2D tex2;

    void Start()
    {
        Debug.Log("Attempting to connect to server...");
        try
        {
            client = new TcpClient("172.20.10.8", 12345); // IP Address of the Pi
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

                    if (tex1 == null)
                    {
                        tex1 = new Texture2D(640, 480); // Adjust size to match Raspberry Pi settings
                    }
                    tex1.LoadImage(imageBuffer1);

                    if (tex2 == null)
                    {
                        tex2 = new Texture2D(640, 480); // Adjust size to match Raspberry Pi settings
                    }
                    tex2.LoadImage(imageBuffer2);

                    // Apply textures to render textures
                    Graphics.Blit(tex1, LeftEyeRenderTexture);
                    Graphics.Blit(tex2, RightEyeRenderTexture);

                    Debug.Log("Assigned textures to render textures.");
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

        if (tex1 != null)
        {
            Destroy(tex1);
        }

        if (tex2 != null)
        {
            Destroy(tex2);
        }

        Debug.Log("Application quitting, resources cleaned up.");
    }
}



/*using UnityEngine;
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

    private Texture2D tex1;
    private Texture2D tex2;

    void Start()
    {
        Debug.Log("Attempting to connect to server...");
        try
        {
            client = new TcpClient("172.20.10.8", 12345); // IP Address of the Pi
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

                    if (tex1 == null)
                    {
                        tex1 = new Texture2D(640, 480); // Adjust size to match Raspberry Pi settings
                    }
                    tex1.LoadImage(imageBuffer1);
                    rawImage1.texture = tex1;

                    if (tex2 == null)
                    {
                        tex2 = new Texture2D(640, 480); // Adjust size to match Raspberry Pi settings
                    }
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

        if (tex1 != null)
        {
            Destroy(tex1);
        }

        if (tex2 != null)
        {
            Destroy(tex2);
        }

        Debug.Log("Application quitting, resources cleaned up.");
    }
}
*/


