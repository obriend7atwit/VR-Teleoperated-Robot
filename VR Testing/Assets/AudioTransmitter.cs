using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NAudio.Wave;
using System;

public class AudioTransmitter : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread sendThread;

    private WaveInEvent waveIn;
    private BufferedWaveProvider waveProvider;
    private WaveFileWriter waveWriter;

    void Start()
    {
        client = new TcpClient("10.0.0.248", 12348); // IP address of the Pi
        stream = client.GetStream();
        sendThread = new Thread(new ThreadStart(SendData));
        sendThread.Start();

        waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(44100, 1)
        };

        waveIn.DataAvailable += OnDataAvailable;
        waveProvider = new BufferedWaveProvider(waveIn.WaveFormat);
        waveIn.StartRecording();
    }

    private void OnDataAvailable(object sender, WaveInEventArgs e)
    {
        waveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
    }

    private void SendData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        byte[] audioBuffer = new byte[waveProvider.BufferLength];

        while (true)
        {
            try
            {
                int bytesRead = waveProvider.Read(audioBuffer, 0, audioBuffer.Length);
                float[] audioData = new float[bytesRead / sizeof(float)];
                Buffer.BlockCopy(audioBuffer, 0, audioData, 0, bytesRead);

                // Serialize the audio data
                MemoryStream ms = new MemoryStream();
                bf.Serialize(ms, audioData);
                byte[] serializedData = ms.ToArray();

                // Send message size first
                byte[] sizeInfo = System.BitConverter.GetBytes(serializedData.Length);
                stream.Write(sizeInfo, 0, sizeInfo.Length);

                // Then send data
                stream.Write(serializedData, 0, serializedData.Length);
            }
            catch (Exception e)
            {
                Debug.LogError("Error sending audio: " + e);
                break;
            }
        }
    }

    void OnApplicationQuit()
    {
        sendThread.Abort();
        stream.Close();
        client.Close();
        waveIn.StopRecording();
        waveIn.Dispose();
    }
}

