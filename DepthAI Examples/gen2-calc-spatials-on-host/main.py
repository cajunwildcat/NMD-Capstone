#!/usr/bin/env python3

import cv2
import depthai as dai
from calc import HostSpatialsCalc
from utility import *
import numpy as np
import math

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

# Connect to device and start pipeline
with dai.Device(pipeline) as device:
    # Output queue will be used to get the depth frames from the outputs defined above
    depthQueue = device.getOutputQueue(name="depth")
    dispQ = device.getOutputQueue(name="disp")

    text = TextHelper()
    hostSpatials = HostSpatialsCalc(device)
    y = 200
    x = 300
    step = 5
    delta = 5
    hostSpatials.setDeltaRoi(delta)
    
    red_range = 90

    #print("Use WASD keys to move ROI.\nUse 'r' and 'f' to change ROI size.")

    while True:
        depthData = depthQueue.get()
        # Calculate spatial coordiantes from depth frame
        spatials, centroid = hostSpatials.calc_spatials(depthData, (x,y)) # centroid == x/y in our case

        # Get disparity frame for nicer depth visualization
        disp = dispQ.get().getFrame()
        disp = (disp * (255 / stereo.initialConfig.getMaxDisparity())).astype(np.uint8)
        disp = cv2.applyColorMap(disp, cv2.COLORMAP_JET)
        
        # 1- convert frame from BGR to HSV
        HSV = cv2.cvtColor(disp,cv2.COLOR_BGR2HSV)

        # 2- define the range of red
        lower=np.array([-20, 100, 100])
        upper=np.array([red_range, 255, 255])

        #check if the HSV of the frame is lower or upper red
        Red_mask = cv2.inRange(HSV,lower, upper)
        result = cv2.bitwise_and(disp, disp, mask = Red_mask)

        # Draw rectangular bounded line on the detected red area
        """ (ret, contours, hierarchy) = cv2.findContours(Red_mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
        for pic,contour in enumerate(contours):
            area = cv2.contourArea(contour)
            if(area > 300): #to remove the noise
                # Constructing the size of boxes to be drawn around the detected red area
                x,y,w,h = cv2.boundingRect(contour)
                disp = cv2.rectangle(disp, (x, y), (x+w, y+h), (0, 0, 255), 2) """


        # Show the frame
        cv2.imshow(f"red mask", Red_mask)
        cv2.imshow("result", result)
        cv2.imshow("disp", disp)

        key = cv2.waitKey(1)
        if key == ord('q'):
            break
        elif key == ord('w'):
            red_range += 1
            if red_range > 119:
                red_range = 119
            print(red_range)
        elif key == ord('s'):
            red_range -= 1
            print(red_range)
