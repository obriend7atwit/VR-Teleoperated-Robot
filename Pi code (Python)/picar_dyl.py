import socket
from gpiozero import Motor, PWMLED, LED
from time import sleep

# Define the GPIO pins for motor control 4 14
motor1 = Motor(forward=5, backward=6)
motor2 = Motor(forward=23, backward=22)
#PWM1 = PWMLED(12)
#PWM2 = PWMLED(13)
#led1 = LED(2)
#led2 = LED(3)

def main():
    HOST = ''  # Listen on all available interfaces
    PORT = 12346  # Choose the same port number as on the PC

    # Create a socket object
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

    # Bind the socket to the host and port
    s.bind((HOST, PORT))

    # Listen for incoming connections with a maximum backlog of 5 connections
    s.listen(5)  # Adjust the backlog value as needed

    print("Waiting for connection...")
    
    # Accept a connection and get the connection socket and address
    conn, addr = s.accept()
    print("Connected by", addr)

    #motor1.pwm(True)
    #motor2.pwm(True)
    # Loop to receive and process commands
    speed = 0.5
    x = 0
    while True:
        data = conn.recv(1024).decode()
        if not data:
            break

        # Print the received data
        print("Received:", data)
        # Convert received commands to motor control
        if data == 'forward':
            #PWM1.value = 1
            #PWM2.value = 1
            #led1.on()
            #sleep(1)
            #motor1.forward(0.309)
            #while x<speed:
                #motor1.forward(x)
                #motor2.forward(x)
                #x = x+0.0001

            motor1.forward(0.26)
            motor2.forward(0.25)
            x = 0
        elif data == 'backward':
            #led1.off()
            motor1.backward(0.26)
            motor2.backward(0.25)
        elif data == 'left':
            #led1.on()
            motor1.forward(0.25) #0.26
            motor2.backward(0.25)
        elif data == 'right':
            #led1.off()
            motor1.backward(0.25) #0.26
            motor2.forward(0.25)
        elif data == 'stop':
            #led1.on()
            #while speed>0:
               # motor1.forward(speed)
                #motor2.forward(speed)
                #speed = speed - 0.0001
                        
            motor1.stop()
            motor2.stop()
            speed = 0.5
        elif data == 'terminate':
            conn.close()

    # Close the connection
    conn.close()

if __name__ == "__main__":
    main()
