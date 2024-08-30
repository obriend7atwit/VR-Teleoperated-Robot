import socket
import cv2
import struct
from picamera2 import Picamera2
import time
import matplotlib.pyplot as plt

def capture_frame(camera):
    frame = camera.capture_array()
    #Convert from BGR (OpenCV default) to RGB
    #frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    return frame

def encode_frame(frame, quality=15):
    ret, buffer = cv2.imencode('.jpg', frame, [int(cv2.IMWRITE_JPEG_QUALITY), quality])
    return buffer
	
# Initialize the sockets
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind(('0.0.0.0', 12345))
server_socket.listen(0)

print('Server listening on port 12345')

# Accept a single connection
client_socket, addr = server_socket.accept()
print(f'Connection from: {addr}')

# Initialize the cameras
camera1 = Picamera2(0) # The distinction between 0 and 1 is NECESSARY
camera1_config = camera1.create_still_configuration(main={"size": (720, 480), "format": "RGB888"})
camera1.configure(camera1_config)
camera1.start()

camera2 = Picamera2(1)
camera2_config = camera2.create_still_configuration(main={"size": (720, 480), "format": "RGB888"})
camera2.configure(camera2_config)
camera2.start()

try:
    while True:
        # Capture frames from both cameras
        frame1 = capture_frame(camera1)
        frame2 = capture_frame(camera2)

        # Encode frames as JPEG
        buffer1 = encode_frame(frame1, quality=15)
        buffer2 = encode_frame(frame2, quality=15)

        # Send the size of the first image
        client_socket.sendall(struct.pack("Q", len(buffer1)))
        # Send the first image
        client_socket.sendall(buffer1)

        # Send the size of the second image
        client_socket.sendall(struct.pack("Q", len(buffer2)))
        # Send the second image
        client_socket.sendall(buffer2)

except Exception as e:
    print(f'Error: {e}')
finally:
    # Release resources
    camera1.stop()
    camera2.stop()
    client_socket.close()
    server_socket.close()
    
