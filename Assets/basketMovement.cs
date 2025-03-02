
using UnityEngine;
using TMPro;

public class BasketController : MonoBehaviour
{
    public SpriteRenderer basketRenderer;
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;
    public Color defaultColor = Color.white;
    private float colorResetTime = 0.5f;

    private Vector3 offset;
    private bool isDragging = false;
    private float screenWidthInUnits;

    void Start()
    {
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
            touchPosition.z = transform.position.z;

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        TextMeshProUGUI letterText = other.GetComponentInChildren<TextMeshProUGUI>();

        if (letterText != null)
        {
            char fallingLetter = letterText.text[0]; // Get falling letter
            char mainLetter = FindObjectOfType<LetterSpawner>().mainLetterText.text[0]; // Get main letter

            if (char.ToLower(fallingLetter) == char.ToLower(mainLetter))
            {
                basketRenderer.color = correctColor; // Glow green for correct letter
                ProgressManager.Instance.UpdateProgress(1); // Increase progress
            }
            else
            {
                basketRenderer.color = incorrectColor; // Glow red for incorrect letter
                ProgressManager.Instance.UpdateProgress(-1); // Decrease progress
            }

            Destroy(other.gameObject); // Remove caught letter
            Invoke(nameof(ResetColor), colorResetTime);
        }
    }

    private void ResetColor()
    {
        basketRenderer.color = defaultColor; // Restore default color
    }
}