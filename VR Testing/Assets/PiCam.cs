using UnityEngine;
using WebSocketSharp;
using System;
using System.Threading;
using System.Text;

public class WebcamClient : MonoBehaviour
{
    public string serverUrl = "ws://localhost:8080/websocket/";

    private WebSocket webSocket;
    private Texture2D receivedTexture;
    private Renderer displayRenderer;

    void Start()
    {
        displayRenderer = GetComponent<Renderer>();
        receivedTexture = new Texture2D(2, 2); // Initial size doesn't matter, will be resized based on received frame

        // Connect to WebSocket server
        webSocket = new WebSocket(serverUrl);
        webSocket.OnMessage += OnWebSocketMessage;
        webSocket.ConnectAsync();

        // Example: Handle closing the WebSocket gracefully
        Application.quitting += OnApplicationQuit;
    }

    void OnDestroy()
    {
        webSocket.Close();
    }

    void OnWebSocketMessage(object sender, MessageEventArgs e)
    {
        if (e.IsBinary)
        {
            // Convert received binary data to Texture2D
            receivedTexture.LoadImage(e.RawData); // Assumes JPEG encoded frame data

            // Update display with received frame
            displayRenderer.material.mainTexture = receivedTexture;

            // Resize plane to match the aspect ratio of the received image (optional)
            float aspectRatio = (float)receivedTexture.width / receivedTexture.height;
            displayRenderer.transform.localScale = new Vector3(aspectRatio, 1f, 1f);
        }
    }

    void OnApplicationQuit()
    {
        webSocket.Close();
    }
}
