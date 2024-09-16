import socket
import sounddevice as sd
import struct

# Audio settings
samplerate = 44100
channels = 1
blocksize = 1024

# Network settings
server_ip = '10.0.0.210'  # IP address of the Unity receiver
server_port = 12347

# Initialize the socket
client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

print(f"Attempting to connect to {server_ip}:{server_port}...")
try:
    client_socket.connect((server_ip, server_port))
    print("Connection to Unity receiver established.")
except Exception as e:
    print(f"Failed to connect to Unity receiver: {e}")
    exit(1)

stream = client_socket.makefile('wb')

def callback(indata, frames, time, status):
    if status:
        print(f"Sounddevice status: {status}", flush=True)
    try:
        # Flatten the audio data array and convert to bytes
        data = indata.tobytes()
        # Send message size first
        message_size = struct.pack("Q", len(data))
        stream.write(message_size)
        stream.write(data)
        stream.flush()
        print(f"Sent {len(data)} bytes of audio data.")
    except Exception as e:
        print(f"Error during audio data transmission: {e}")

# Start recording and sending audio data
print("Starting audio stream...")
try:
    with sd.InputStream(samplerate=samplerate, channels=channels, blocksize=blocksize, callback=callback):
        print("Press Ctrl+C to stop the recording")
        while True:
            pass
except KeyboardInterrupt:
    print("Recording stopped by user.")
except Exception as e:
    print(f"Error in audio stream: {e}")
finally:
    client_socket.close()
    print("Connection closed.")
