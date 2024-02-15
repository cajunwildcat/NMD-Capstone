import pygame
import socket
import sys

# Initialize Pygame
pygame.init()

# Set up the window
window_size = (800, 450)
window = pygame.display.set_mode(window_size)
pygame.display.set_caption("Moving Square")

# Define colors
BLACK = (0, 0, 0)
WHITE = (255, 255, 255)

# Set up the square
square_size = 15
square_x = (window_size[0] - square_size) / 2
square_y = (window_size[1] - square_size) / 2
square_speed = 5

clock = pygame.time.Clock()

# Display text "Waiting for connection" in the center of the window
font = pygame.font.Font(None, 36)
text_surface = font.render("Waiting for connection", True, WHITE)
window.blit(text_surface, ((window_size[0] - text_surface.get_width()) / 2, (window_size[1] - text_surface.get_height()) / 2))
pygame.display.flip()

# Set up TCP server
server_address = ('localhost', 10000)  # Change to appropriate IP address and port
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind(server_address)
server_socket.listen(1)  # Listen for one incoming connection
client_socket, client_address = server_socket.accept()

running = True
is_dragging = False
# Main game loop
while running:
    window.fill(BLACK)  # Fill the window with black color

    # Event handling
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            running = False
        elif event.type == pygame.MOUSEBUTTONDOWN:
            if event.button == 1:  # Left mouse button
                mouse_x, mouse_y = pygame.mouse.get_pos()
                if square_x <= mouse_x <= square_x + square_size and square_y <= mouse_y <= square_y + square_size:
                    # If the mouse click is within the square, enable dragging
                    is_dragging = True
                    drag_offset_x = mouse_x - square_x
                    drag_offset_y = mouse_y - square_y
        elif event.type == pygame.MOUSEBUTTONUP:
            if event.button == 1:  # Left mouse button
                is_dragging = False
        elif event.type == pygame.MOUSEMOTION:
            if is_dragging:
                mouse_x, mouse_y = pygame.mouse.get_pos()
                square_x = mouse_x - drag_offset_x
                square_y = mouse_y - drag_offset_y

    # Get the pressed keys
    keys = pygame.key.get_pressed()
    # Move the square based on arrow key inputs
    if keys[pygame.K_LEFT]:
        square_x -= square_speed
    if keys[pygame.K_RIGHT]:
        square_x += square_speed
    if keys[pygame.K_UP]:
        square_y -= square_speed
    if keys[pygame.K_DOWN]:
        square_y += square_speed

    # Ensure the square stays within the window boundaries
    square_x = max(0, min(window_size[0] - square_size, square_x))
    square_y = max(0, min(window_size[1] - square_size, square_y))

    # Draw the square
    pygame.draw.rect(window, WHITE, (square_x, square_y, square_size, square_size), 1)

    # Draw coordinates in the top right corner
    font = pygame.font.Font(None, 36)
    text_surface = font.render(f"({square_x/window_size[0]:.2f}, {square_y/window_size[1]:.2f})", True, WHITE)
    window.blit(text_surface, (window_size[0] - text_surface.get_width(), 0))
    
    # Display the window
    pygame.display.flip()
    
    clock.tick(60)

    # Send square coordinates over TCP
    try:
        client_socket.send(f"{square_x/window_size[0]:.2f},{square_y/window_size[1]:.2f}".encode())
    except socket.error as e:
        print("Socket error:", e)
        break

client_socket.close()
pygame.quit()
sys.exit()