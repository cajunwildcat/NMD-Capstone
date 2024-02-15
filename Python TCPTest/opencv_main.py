import cv2
import numpy as np
import socket

# Set up TCP server
server_address = ('localhost', 10000)  # Change to appropriate IP address and port
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind(server_address)
server_socket.listen(1)  # Listen for one incoming connection

print("Waiting for client connection...")
client_socket, client_address = server_socket.accept()
print("Connection established with:", client_address)

# Start capturing video from webcam
cap = cv2.VideoCapture(0)

while True:
    # Read frame from webcam
    ret, frame = cap.read()
    if not ret:
        print("Error: Unable to capture frame from webcam")
        break

    # Convert BGR to HSV color space
    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)

    # Define range of red color in HSV
    lower_red = np.array([0, 100, 100])
    upper_red = np.array([10, 255, 255])

    # Threshold the HSV image to get only red colors
    mask1 = cv2.inRange(hsv, lower_red, upper_red)

    # Define another range of red color in HSV (to handle red hue wrap-around)
    lower_red = np.array([170, 100, 100])
    upper_red = np.array([180, 255, 255])

    # Threshold the HSV image to get only red colors
    mask2 = cv2.inRange(hsv, lower_red, upper_red)

    # Combine the masks
    mask = cv2.bitwise_or(mask1, mask2)

    # Find contours of red areas
    contours, _ = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

    # Send coordinates of the largest red area over TCP
    if contours:
        largest_contour = max(contours, key=cv2.contourArea)
        x, y, w, h = cv2.boundingRect(largest_contour)

        # Calculate center coordinates and normalize to range 0-1
        center_x = (x + w/2) / frame.shape[1]
        center_y = (y + h/2) / frame.shape[0]

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

    # Display processed frame
    cv2.imshow('Processed Frame', frame)

    # Press 'q' to quit
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Release video capture and close TCP connection
cap.release()
server_socket.close()
cv2.destroyAllWindows()