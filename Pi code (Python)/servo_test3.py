import time
import board
import busio
import curses
from adafruit_motor import servo
from adafruit_pca9685 import PCA9685

# Create the I2C bus interface
i2c = busio.I2C(board.SCL, board.SDA)

# Create a simple PCA9685 class instance
pca = PCA9685(i2c)
pca.frequency = 330  # Typical frequency for servos is 50 Hz - 330 Hz

# Adjust these values according to servo specifications
min_pulse1 = 1500  # Minimum pulse length out of 2500 (right max --> higher decreases)
max_pulse1 = 2500  # Maximum pulse length out of 2500
min_pulse2 = 1300  # Minimum pulse length out of 2500 (1300)
max_pulse2 = 1700  # Maximum pulse length out of 2500 (1750)

# Create servo objects on channel 0 and channel 1 with a 270-degree range
servo_motor_1 = servo.Servo(pca.channels[0], actuation_range=270, min_pulse=min_pulse1, max_pulse=max_pulse1) # Left-Right motor
servo_motor_2 = servo.Servo(pca.channels[1], actuation_range=270, min_pulse=min_pulse2, max_pulse=max_pulse2) # Up-Down motor

# Initial angles for the servos
initial_angle_1 = 135
initial_angle_2 = 270
angle_1 = initial_angle_1
angle_2 = initial_angle_2

def set_servo_angle(servo_motor, angle):
    """Set the servo angle.
    Angle should be between 0 and 270.
    """
    if 0 <= angle <= 270:
        servo_motor.angle = angle
    else:
        raise ValueError("Angle must be between 0 and 270.")

def main(stdscr):
    global angle_1, angle_2
    stdscr.clear()
    stdscr.nodelay(1)
    stdscr.addstr(0, 0, "Use arrow keys to control the servos. Press 'q' to exit.")
    stdscr.addstr(1, 0, f"Servo 1 angle: {angle_1}")
    stdscr.addstr(2, 0, f"Servo 2 angle: {angle_2}")
    
    set_servo_angle(servo_motor_1, angle_1)
    set_servo_angle(servo_motor_2, angle_2)

    try:
        while True:
            key = stdscr.getch()
            if key == curses.KEY_RIGHT:
                angle_1 = max(0, angle_1 - 5)
                set_servo_angle(servo_motor_1, angle_1)
                stdscr.addstr(1, 0, f"Servo 1 angle: {angle_1}  ")
            elif key == curses.KEY_LEFT:
                angle_1 = min(270, angle_1 + 5)
                set_servo_angle(servo_motor_1, angle_1)
                stdscr.addstr(1, 0, f"Servo 1 angle: {angle_1}  ")
            elif key == curses.KEY_DOWN:
                angle_2 = min(270, angle_2 + 10)
                set_servo_angle(servo_motor_2, angle_2)
                stdscr.addstr(2, 0, f"Servo 2 angle: {angle_2}  ")
            elif key == curses.KEY_UP:
                angle_2 = max(0, angle_2 - 10)
                set_servo_angle(servo_motor_2, angle_2)
                stdscr.addstr(2, 0, f"Servo 2 angle: {angle_2}  ")
            elif key == ord('q'):
                break
            time.sleep(0.01)

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

curses.wrapper(main)
