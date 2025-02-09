using UnityEngine;

public class basketMovement : MonoBehaviour
{
    public float minX = -2.5f;  // Left boundary
    public float maxX = 2.5f;   // Right boundary

    void Update()
    {
        HandleTouchInput();
    }

    void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Convert touch position to world position
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 0));

            // Clamp the X position within minX and maxX
            float clampedX = Mathf.Clamp(touchPosition.x, minX, maxX);

            // Move the basket
            transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
        }
    }
}
