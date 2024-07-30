using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using NAudio.Wave;
using System;

public class AudioReceiver : MonoBehaviour
{
    private TcpListener server;
    private Thread listenerThread;
    private BufferedWaveProvider waveProvider;
    private WaveOutEvent waveOut;

    void Start()
    {
        waveProvider = new BufferedWaveProvider(new WaveFormat(44100, 1));
        waveProvider.BufferDuration = TimeSpan.FromSeconds(10);  // Increase buffer duration

        waveOut = new WaveOutEvent
        {
            DesiredLatency = 200 // Desired latency in milliseconds
        };
        waveOut.Init(waveProvider);
        waveOut.Play();

        listenerThread = new Thread(new ThreadStart(ListenForData));
        listenerThread.IsBackground = true;
        listenerThread.Start();
    }

    private void ListenForData()
    {
        server = new TcpListener(IPAddress.Any, 12347);
        server.Start();
        Debug.Log("Server listening on port 12347");

        while (true)
        {
            try
            {
                TcpClient client = server.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                Debug.Log("Client connected");

                while (client.Connected)
                {
                    // Read message size
                    byte[] sizeInfo = new byte[sizeof(long)];
                    if (stream.Read(sizeInfo, 0, sizeInfo.Length) == 0)
                        break;
                    long dataSize = BitConverter.ToInt64(sizeInfo, 0);

                    Debug.Log($"Expected data size: {dataSize} bytes");

                    // Read the actual data
                    byte[] data = new byte[dataSize];
                    int bytesRead = 0;
                    while (bytesRead < dataSize)
                    {
                        int read = stream.Read(data, bytesRead, data.Length - bytesRead);
                        if (read == 0)
                            break;
                        bytesRead += read;
                    }

                    Debug.Log($"Received {bytesRead} bytes of audio data");

                    // Add the raw audio data to the wave provider
                    waveProvider.AddSamples(data, 0, data.Length);
                    Debug.Log($"Played {data.Length} bytes of audio data.");
                }

                client.Close();
                Debug.Log("Client disconnected");
            }
            catch (Exception e)
            {
                Debug.LogError("Error in ListenForData: " + e);
            }
        }
    }

    void OnApplicationQuit()
    {
        listenerThread.Abort();
        server.Stop();
        waveOut.Stop();
        waveOut.Dispose();
    }
}


/*using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using NAudio.Wave;
using System;

public class AudioReceiver : MonoBehaviour
{
    private TcpListener server;
    private Thread listenerThread;
    private BufferedWaveProvider waveProvider;
    private WaveOutEvent waveOut;

    void Start()
    {
        waveProvider = new BufferedWaveProvider(new WaveFormat(44100, 1));
        waveProvider.BufferDuration = TimeSpan.FromSeconds(2);  // Increase buffer duration to avoid underflow
        waveOut = new WaveOutEvent();
        waveOut.Init(waveProvider);
        waveOut.Play();

        listenerThread = new Thread(new ThreadStart(ListenForData));
        listenerThread.IsBackground = true;
        listenerThread.Start();
    }

    private void ListenForData()
    {
        server = new TcpListener(IPAddress.Any, 12347);
        server.Start();
        Debug.Log("Server listening on port 12347");

        while (true)
        {
            try
            {
                TcpClient client = server.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                Debug.Log("Client connected");

                while (client.Connected)
                {
                    // Read message size
                    byte[] sizeInfo = new byte[sizeof(long)];
                    if (stream.Read(sizeInfo, 0, sizeInfo.Length) == 0)
                        break;
                    long dataSize = BitConverter.ToInt64(sizeInfo, 0);

                    Debug.Log($"Expected data size: {dataSize} bytes");

                    // Read the actual data
                    byte[] data = new byte[dataSize];
                    int bytesRead = 0;
                    while (bytesRead < dataSize)
                    {
                        int read = stream.Read(data, bytesRead, data.Length - bytesRead);
                        if (read == 0)
                            break;
                        bytesRead += read;
                    }

                    Debug.Log($"Received {bytesRead} bytes of audio data");

                    // Add the raw audio data to the wave provider
                    waveProvider.AddSamples(data, 0, data.Length);
                    Debug.Log($"Played {data.Length} bytes of audio data.");
                }

                client.Close();
                Debug.Log("Client disconnected");
            }
            catch (Exception e)
            {
                Debug.LogError("Error in ListenForData: " + e);
            }
        }
    }

    void OnApplicationQuit()
    {
        listenerThread.Abort();
        server.Stop();
        waveOut.Stop();
        waveOut.Dispose();
    }
}


using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using NAudio.Wave;

public class AudioReceiver : MonoBehaviour
{
    public AudioSource audioSource;
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;

    private BufferedWaveProvider waveProvider;
    private WaveOutEvent waveOut;

    void Start()
    {
        client = new TcpClient("10.0.0.248", 12347); // IP address of the Pi
        stream = client.GetStream();
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.Start();

        waveProvider = new BufferedWaveProvider(new WaveFormat(44100, 1));
        waveOut = new WaveOutEvent();
        waveOut.Init(waveProvider);
        waveOut.Play();
    }

    void ReceiveData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        while (true)
        {
            try
            {
                // Read message size
                byte[] sizeInfo = new byte[8];
                stream.Read(sizeInfo, 0, 8);
                long messageSize = BitConverter.ToInt64(sizeInfo, 0);

                // Read message data
                byte[] data = new byte[messageSize];
                int totalRead = 0;
                while (totalRead < messageSize)
                {
                    totalRead += stream.Read(data, totalRead, (int)messageSize - totalRead);
                }

                // Deserialize the audio data
                MemoryStream ms = new MemoryStream(data);
                float[] audioData = (float[])bf.Deserialize(ms);

                // Convert float array to byte array for NAudio
                byte[] audioBytes = new byte[audioData.Length * sizeof(float)];
                Buffer.BlockCopy(audioData, 0, audioBytes, 0, audioBytes.Length);

                // Add bytes to wave provider buffer
                waveProvider.AddSamples(audioBytes, 0, audioBytes.Length);
            }
            catch (Exception e)
            {
                Debug.LogError("Error receiving audio: " + e);
                break;
            }
        }
    }

    void OnApplicationQuit()
    {
        receiveThread.Abort();
        stream.Close();
        client.Close();
    }
}
*/
