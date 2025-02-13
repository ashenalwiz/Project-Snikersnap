using UnityEngine;

public class BasketMovement : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;
    private float screenWidthInUnits;

    void Start()
    {
        // Convert screen width to world units
        float screenHalfWidth = Camera.main.orthographicSize * Screen.width / Screen.height;
        screenWidthInUnits = screenHalfWidth * 2;
    }

    void Update()
    {
        HandleTouchInput();
    }

    void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 0));
            touchPosition.z = transform.position.z; // Keep Z position unchanged

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (GetComponent<Collider2D>() == Physics2D.OverlapPoint(touchPosition))
                    {
                        isDragging = true;
                        offset = transform.position - touchPosition;
                    }
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        float clampedX = Mathf.Clamp(touchPosition.x + offset.x, -screenWidthInUnits / 2, screenWidthInUnits / 2);
                        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false;
                    break;
            }
        }
    }
}
