# VR Teleoperated Robot

A Unity-based VR telepresence prototype that lets a user drive a Raspberry Pi 5 robot-on-wheels while viewing stereoscopic video from dual cameras in a VR headset. The system targets low-latency streaming (report notes 720p 30fps with roughly 150–400 ms end-to-end latency) and emphasizes a modular, cost-effective design for telepresence and accessibility use cases.

## System Overview

**Robot (Raspberry Pi 5 + motor base)**
- Streams dual camera frames over TCP for a stereoscopic view in VR.
- Accepts drive commands (forward/back/left/right/stop) from the VR controller.
- Accepts headset yaw/pitch data to move the camera mount servos.
- Optional audio streaming tests are included in the repo.

**VR Client (Unity + OpenXR)**
- Receives two JPEG streams and blits them to left/right eye render textures.
- Reads Quest/VR controller input and sends movement commands to the Pi.
- Sends headset rotation to control the camera mount servos.
- Includes an audio receiver for microphone streaming experiments.

## Repository Layout

- `Pi code (Python)/` — Raspberry Pi scripts for camera streaming, motor control, servos, and audio tests.
- `VR Testing/` — Unity project (OpenXR) with C# scripts for video reception, VR input, and servo control.
- `Report/` — Project report describing design goals, hardware, and test results.

## Key Network Endpoints (as implemented in code)

These are hard-coded in the current scripts and should be updated to match your network:

| Function | Pi Script | Unity Script | Port |
| --- | --- | --- | --- |
| Dual camera video (TCP) | `Camera_test_7_23.py` | `VideoReceiver.cs` | 12345 |
| Drive commands (TCP) | `picar_dyl.py` | `VRInputHandler.cs` | 12346 |
| Headset → servo mount (TCP) | `Headset_servo_test.py` | `HeadsetServo.cs` | 12349 |
| Microphone test (TCP) | `microphone_test.py` | `AudioReceiver.cs` | 12347 |

## Quick Start (High-Level)

1. **On the Raspberry Pi**
   - Connect dual cameras and the motor/servo hardware.
   - Install required Python dependencies (e.g., `picamera2`, `opencv-python`, `gpiozero`, `adafruit-circuitpython-pca9685`, `sounddevice`).
   - Run the relevant scripts from `Pi code (Python)/` (video, drive control, servo control, optional audio).

2. **In Unity**
   - Open `VR Testing/` in Unity and ensure the OpenXR packages are installed.
   - Update IP addresses inside `VideoReceiver.cs`, `VRInputHandler.cs`, and `HeadsetServo.cs` to match the Pi.
   - Press Play in the scene to connect to the Pi and start streaming.

## Notes

- The report in `Report/` provides details on hardware choices, testing methodology, and measured latency/quality results.
- This repo is a prototype/experimental system; expect to tune IPs, ports, and camera settings for your environment.
