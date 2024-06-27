using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;

public class WebcamServer
{
    private HttpListener _httpListener;
    private CancellationTokenSource _cts;
    private WebSocket _webSocket;
    private VideoCapture _capture;
    private Task _streamTask;

    public async Task Start(string url)
    {
        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add(url);
        _httpListener.Start();

        Console.WriteLine($"Listening for WebSocket connections on {url}...");

        _capture = new VideoCapture(0);
        _streamTask = Task.Run(StreamFrames);

        while (true)
        {
            try
            {
                HttpListenerContext context = await _httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    ProcessWebSocketRequest(context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                break;
            }
        }
    }

    private async void ProcessWebSocketRequest(HttpListenerContext context)
    {
        try
        {
            HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);

            _webSocket = webSocketContext.WebSocket;
            Console.WriteLine("WebSocket connection established.");

            await _webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Connection established.")),
                                       WebSocketMessageType.Text, true, CancellationToken.None);

            await _streamTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket error: {ex.Message}");
        }
    }

    private async Task StreamFrames()
    {
        try
        {
            Mat frame = new Mat();
            while (_webSocket.State == WebSocketState.Open && _capture.IsOpened())
            {
                _capture.Read(frame);
                if (!frame.Empty())
                {
                    // Convert Mat to JPEG bytes
                    byte[] jpegBytes = frame.ToMemoryStream(".jpg").ToArray();

                    // Send frame over WebSocket
                    await _webSocket.SendAsync(new ArraySegment<byte>(jpegBytes), WebSocketMessageType.Binary,
                                               true, CancellationToken.None);

                    await Task.Delay(100); // Adjust frame rate here (e.g., 10 frames per second)
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Streaming error: {ex.Message}");
        }
    }

    public async Task Stop()
    {
        _httpListener.Stop();
        _httpListener.Close();
        _capture.Release();
        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None);
        _webSocket.Dispose();
        _cts?.Cancel();
        await Task.CompletedTask;
    }

    public static async Task Main(string[] args)
    {
        string url = "http://localhost:8080/websocket/";

        var server = new WebcamServer();
        await server.Start(url);

        Console.WriteLine("Press ENTER to stop the server.");
        Console.ReadLine();

        await server.Stop();
    }
}
