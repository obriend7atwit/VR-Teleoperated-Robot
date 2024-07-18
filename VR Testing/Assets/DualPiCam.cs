using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

public class VideoReceiver : MonoBehaviour
{
    public Renderer renderer1;
    public Renderer renderer2;

    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;

    void Start()
    {
        client = new TcpClient("10.0.0.248", 12345); //IP Address of the Pi
        stream = client.GetStream();
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.Start();
    }

    void ReceiveData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        while (true)
        {
            try
            {
                byte[] messageSize = new byte[8];
                stream.Read(messageSize, 0, 8);
                long size = System.BitConverter.ToInt64(messageSize, 0);

                byte[] data = new byte[size];
                stream.Read(data, 0, data.Length);

                MemoryStream ms = new MemoryStream(data);
                Texture2D tex1 = (Texture2D)bf.Deserialize(ms);
                Texture2D tex2 = (Texture2D)bf.Deserialize(ms);

                renderer1.material.mainTexture = tex1;
                renderer2.material.mainTexture = tex2;
            }
            catch
            {
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
