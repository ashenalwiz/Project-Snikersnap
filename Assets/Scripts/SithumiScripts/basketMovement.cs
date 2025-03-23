using UnityEngine;
using TMPro;

public class BasketController : MonoBehaviour
{
    public SpriteRenderer basketRenderer;
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;
    public Color defaultColor = Color.white;

    [Header("Sound Effects")]
    public AudioSource correctLetterSound; // Sound for catching the correct letter
    public AudioSource incorrectLetterSound; // Sound for catching the incorrect letter

    private float colorResetTime = 0.5f; // Time before basket color resets
    private Vector3 offset; // Offset for touch-based dragging
    private bool isDragging = false; // Tracks if the basket is being dragged
    private float screenWidthInUnits; // Screen width in world units

    void Start()
    {
        // Calculate screen width in world units
        float screenHalfWidth = Camera.main.orthographicSize * Screen.width / Screen.height;
        screenWidthInUnits = screenHalfWidth * 2;

        // Warn if sound effects are missing
        if (correctLetterSound == null)
            Debug.LogWarning("Correct letter sound is not assigned!");
        if (incorrectLetterSound == null)
            Debug.LogWarning("Incorrect letter sound is not assigned!");
    }

    void Update()
    {
        HandleTouchInput(); // Handle basket movement using touch input
    }

    void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 0));
            touchPosition.z = transform.position.z; // Maintain original Z position

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // Start dragging if the touch is on the basket
                    if (GetComponent<Collider2D>() == Physics2D.OverlapPoint(touchPosition))
                    {
                        isDragging = true;
                        offset = transform.position - touchPosition;
                    }
                    break;

                case TouchPhase.Moved:
                    // Move the basket while dragging, within screen limits
                    if (isDragging)
                    {
                        float clampedX = Mathf.Clamp(touchPosition.x + offset.x, -screenWidthInUnits / 2, screenWidthInUnits / 2);
                        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false; // Stop dragging when touch is released
                    break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TextMeshProUGUI letterText = other.GetComponentInChildren<TextMeshProUGUI>();

        if (letterText != null)
        {
            char fallingLetter = letterText.text[0]; // Get the letter from the falling object
            char mainLetter = FindObjectOfType<LetterSpawner>().mainLetterText.text[0]; // Get the target letter

            if (char.ToLower(fallingLetter) == char.ToLower(mainLetter))
            {
                basketRenderer.color = correctColor; // Glow green for correct letter
                ProgressManager.Instance.UpdateProgress(1); // Increase progress score

                // Play correct letter sound
                if (correctLetterSound != null)
                    correctLetterSound.Play();
            }
            else
            {
                basketRenderer.color = incorrectColor; // Glow red for incorrect letter
                ProgressManager.Instance.UpdateProgress(-1); // Decrease progress score

                // Play incorrect letter sound
                if (incorrectLetterSound != null)
                    incorrectLetterSound.Play();
            }

            Destroy(other.gameObject); // Remove caught letter from the scene
            Invoke(nameof(ResetColor), colorResetTime); // Reset basket color after delay
        }
    }

    private void ResetColor()
    {
        basketRenderer.color = defaultColor; // Restore default basket color
    }
}
