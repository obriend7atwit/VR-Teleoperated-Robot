import socket
import time
import board
import busio
from adafruit_motor import servo
from adafruit_pca9685 import PCA9685

# Create the I2C bus interface
i2c = busio.I2C(board.SCL, board.SDA)

# Create a simple PCA9685 class instance
pca = PCA9685(i2c)
pca.frequency = 60  # Typical frequency for servos is 50 Hz - 330 Hz

min_pulse1 = 1500  # Minimum pulse length out of 2500 (right max --> higher decreases)
max_pulse1 = 2500  # Maximum pulse length out of 2500
min_pulse2 = 1300  # Minimum pulse length out of 2500
max_pulse2 = 1750  # Maximum pulse length out of 2500

# Create servo objects on channel 0 and channel 1 with a 270-degree range
servo_motor_1 = servo.Servo(pca.channels[0], actuation_range=270, min_pulse=min_pulse1, max_pulse=max_pulse1) # Left-Right motor
servo_motor_2 = servo.Servo(pca.channels[1], actuation_range=270, min_pulse=min_pulse2, max_pulse=max_pulse2) # Up-Down motor

# Initial angles for the servos
initial_angle_1 = 135
initial_angle_2 = 270
angle_1 = initial_angle_1
angle_2 = initial_angle_2

# Previous rotation values
prev_yaw = 0
prev_pitch = 0

def set_servo_angle(servo_motor, angle):
    """Set the servo angle.
    Angle should be between 0 and 270.
    """
    if 0 <= angle <= 270:
        servo_motor.angle = angle
    else:
        raise ValueError("Angle must be between 0 and 270.")

def update_servo_angle(current_angle, previous_angle, servo_motor, angle, max_angle=270):
    """Update servo angle based on the direction of rotation."""
    # Calculate the difference in angles
    delta = current_angle - previous_angle

    # Handle the case where the rotation wraps around (e.g., 360 to 0 or 0 to 360)
    if abs(delta) > 180:
        if delta > 0:
            delta -= 360
        else:
            delta += 360

    # Update the servo angle
    new_angle = angle + delta
    new_angle = min(max(new_angle, 0), max_angle)
    set_servo_angle(servo_motor, new_angle)
    return new_angle

# Initialize the server socket
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind(('0.0.0.0', 12349))
server_socket.listen(1)

print("Server listening on port 12349")

# Accept a single connection
client_socket, addr = server_socket.accept()
print(f'Connection from: {addr}')

try:
    while True:
        data = client_socket.recv(1024).decode('ascii')
        if not data:
            break

        try:
            yaw, pitch = map(float, data.split(','))
            print(f"Received Yaw: {yaw}, Pitch: {pitch}")

            # Update servo angles based on the direction of rotation
            angle_1 = update_servo_angle(-yaw, prev_yaw, servo_motor_1, angle_1)
            angle_2 = update_servo_angle(pitch, prev_pitch, servo_motor_2, angle_2)

            print(f"Servo 1 angle: {angle_1}, Servo 2 angle: {angle_2}")

            # Store current values as previous for next iteration
            prev_yaw = yaw
            prev_pitch = pitch
        except ValueError as e:
            print(f"ValueError: {e}")

except KeyboardInterrupt:
    print("Program interrupted by user")

finally:
    # Return servos to initial angles
    set_servo_angle(servo_motor_1, initial_angle_1)
    set_servo_angle(servo_motor_2, initial_angle_2)
    time.sleep(1)  # Give it some time to reach the initial position
    # Relax the servos by setting their PWM signal to 0 (neutral)
    pca.channels[0].duty_cycle = 0
    pca.channels[1].duty_cycle = 0
    # Deinitialize the PCA9685
    pca.deinit()
    client_socket.close()
    server_socket.close()
