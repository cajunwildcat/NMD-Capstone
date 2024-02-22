#!/usr/bin/env python3

import cv2
import depthai as dai
import numpy as np
import socket

# Create pipeline
pipeline = dai.Pipeline()

# Define sources and outputs
monoLeft = pipeline.create(dai.node.MonoCamera)
monoRight = pipeline.create(dai.node.MonoCamera)
stereo = pipeline.create(dai.node.StereoDepth)

# Properties
monoLeft.setResolution(dai.MonoCameraProperties.SensorResolution.THE_400_P)
monoLeft.setBoardSocket(dai.CameraBoardSocket.LEFT)
monoRight.setResolution(dai.MonoCameraProperties.SensorResolution.THE_400_P)
monoRight.setBoardSocket(dai.CameraBoardSocket.RIGHT)

stereo.initialConfig.setConfidenceThreshold(255)
stereo.setLeftRightCheck(True)
stereo.setSubpixel(False)

# Linking
monoLeft.out.link(stereo.left)
monoRight.out.link(stereo.right)

xoutDepth = pipeline.create(dai.node.XLinkOut)
xoutDepth.setStreamName("depth")
stereo.depth.link(xoutDepth.input)

xoutDepth = pipeline.create(dai.node.XLinkOut)
xoutDepth.setStreamName("disp")
stereo.disparity.link(xoutDepth.input)


server_address = ('localhost', 10000)  # Change to appropriate IP address and port
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind(server_address)
server_socket.listen(1)  # Listen for one incoming connection
client_socket, client_address = server_socket.accept()

# Connect to device and start pipeline
with dai.Device(pipeline) as device:
    # Output queue will be used to get the depth frames from the outputs defined above
    depthQueue = device.getOutputQueue(name="depth")
    dispQ = device.getOutputQueue(name="disp")

    # Define lower and upper bounds for dark red in HSV color space
    lower_red = np.array([0, 50, 50])
    upper_red = np.array([10, 255, 255])

    while True:
        depthData = depthQueue.get()
        
        # Get disparity frame for nicer depth visualization
        disp = dispQ.get().getFrame()
        disp = (disp * (255 / stereo.initialConfig.getMaxDisparity())).astype(np.uint8)
        disp = cv2.applyColorMap(disp, cv2.COLORMAP_JET)
        
        # Convert frame from BGR to HSV
        hsv = cv2.cvtColor(disp, cv2.COLOR_BGR2HSV)

        # Threshold the HSV image to get only dark red colors
        mask = cv2.inRange(hsv, lower_red, upper_red)

        # Find contours of dark red areas
        contours, _ = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

        # Draw bounding boxes around dark red areas
        for contour in contours:
            x, y, w, h = cv2.boundingRect(contour)
            cv2.rectangle(disp, (x, y), (x + w, y + h), (0, 255, 0), 2)
            
        if contours:
            largest_contour = max(contours, key=cv2.contourArea)
            x, y, w, h = cv2.boundingRect(largest_contour)

            # Calculate center coordinates and normalize to range 0-1
            center_x = (x + w/2) / disp.shape[1]
            center_y = (y + h/2) / disp.shape[0]

            # Send normalized coordinates over TCP
            try:
                client_socket.send(f"{center_x:.2f},{center_y:.2f}".encode())
            except socket.error as e:
                print("Socket error:", e)
        else:
            # If no red areas detected, send empty message
            try:
                client_socket.send("".encode())
            except socket.error as e:
                print("Socket error:", e)

        # Show the frame
        cv2.imshow("disp", disp)

        key = cv2.waitKey(1)
        if key == ord('q'):
            break
