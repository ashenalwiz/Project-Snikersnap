using UnityEngine;

public class CloudMover : MonoBehaviour
{
    public float speed = 20f;  // Speed of cloud movement
    public float resetPositionX = -1500f; // X position to reset the cloud
    public float startPositionX = 1500f;  // X position to restart the cloud

    void Update()
    {
        // Move the cloud to the left
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // If the cloud moves past the reset position, move it back to the start position
        if (transform.position.x < resetPositionX)
        {
            transform.position = new Vector3(startPositionX, transform.position.y, transform.position.z);
        }
    }
}
