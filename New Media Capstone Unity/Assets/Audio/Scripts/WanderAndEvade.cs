using UnityEngine;

// This script is meant to move around the activatable Audio objects. 
public class WanderAndEvade : MonoBehaviour
{
    public float wanderSpeed = 4.0f;
    public float evasionSpeed = 6.0f;
    public GameObject tracker;

    private Vector2 randomDirection;
    private float changeDirectionTime = 2f;
    private float changeDirectionTimer;

    // Screen bounds
    private float leftEdge = -2.5f;
    private float rightEdge = 2.5f;
    private float bottomEdge = -4.65f;
    private float topEdge = 4.65f;

    void Start()
    {
        changeDirectionTimer = changeDirectionTime;
        PickNewDirection();
    }

    void Update()
    {
        changeDirectionTimer -= Time.deltaTime;
        if (changeDirectionTimer <= 0)
        {
            PickNewDirection();
            changeDirectionTimer = changeDirectionTime;
        }

        float currentSpeed = wanderSpeed;
        Vector3 directionToTracker = tracker.transform.position - transform.position;

        // Increase speed and move away if the tracker is close
        if (directionToTracker.magnitude < 2f)
        {
            currentSpeed = evasionSpeed;
            randomDirection = -directionToTracker.normalized;
        }

        // Move the sprite
        transform.Translate(randomDirection * currentSpeed * Time.deltaTime);

        // Clamp position to screen bounds
        float clampedX = Mathf.Clamp(transform.position.x, leftEdge, rightEdge);
        float clampedY = Mathf.Clamp(transform.position.y, bottomEdge, topEdge);
        transform.position = new Vector2(clampedX, clampedY);
    }

    private void PickNewDirection()
    {
        // Make sure new direction keeps the sprite within bounds
        float xDirection = Random.Range(-1f, 1f);
        float yDirection = Random.Range(-1f, 1f);

        // If near the edges, adjust the direction to point inwards
        if (transform.position.x < leftEdge + 1f) xDirection = Mathf.Abs(xDirection);
        if (transform.position.x > rightEdge - 1f) xDirection = -Mathf.Abs(xDirection);
        if (transform.position.y < bottomEdge + 1f) yDirection = Mathf.Abs(yDirection);
        if (transform.position.y > topEdge - 1f) yDirection = -Mathf.Abs(yDirection);

        randomDirection = new Vector2(xDirection, yDirection).normalized;
    }
}

