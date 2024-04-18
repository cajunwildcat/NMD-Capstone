using UnityEngine;

public class TrackerMovement : MonoBehaviour
{

    //This script is only for testing purposes.
    //Allows us to move tracker by using WASD or Arrow Keys
    public float moveSpeed = 5f;

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveY = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        transform.position += new Vector3(moveX, moveY, 0f);
    }
}
