using TMPro;
using UnityEngine;

public class LetterSpawner : MonoBehaviour
{
    public TextMeshProUGUI mainLetterText;
    public GameObject letterPrefab;
    public Transform spawnPoint;
    public RectTransform canvasTransform;
    public float spawnInterval = 0.5f;
    private char currentLetter;

    // Remove fixed values and calculate based on screen
    private float minX;
    private float maxX;

    private Color[] colors = new Color[]
    {
        new Color(0.3f, 0.4f, 1f),    // Sky Blue
        new Color(0.7f, 0f, 1f),      // Purple
        new Color(0f, 0.8f, 1f),      // Cyan
        new Color(1f, 0.84f, 0f),     // Golden Yellow
        new Color(0.5f, 0.3f, 0.9f),  // Lavender
        new Color(0f, 0.5f, 1f),      // Ocean Blue
        new Color(1f, 0.5f, 0.9f),    // Light Pink
        new Color(0.6f, 0.4f, 0.8f)   // Soft Purple
    };

    void Start()
    {
        // Calculate spawn width based on screen size
        CalculateSpawnBounds();

        GenerateNewLetter();
        InvokeRepeating(nameof(SpawnFallingLetter), 1f, spawnInterval);
    }

    void CalculateSpawnBounds()
    {
        Canvas canvas = canvasTransform.GetComponent<Canvas>();
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay ||
            canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            float screenWidth = Screen.width;
            float canvasWidth = canvasTransform.rect.width;

            // Reduce the width percentage to narrow the spawn area
            float spawnWidth = canvasWidth * 0.75f; 

            // Shift the range slightly to the right if needed
            float shiftAmount = canvasWidth * 0.534f;

            minX = -(spawnWidth * 0.5f) + shiftAmount;
            maxX = (spawnWidth * 0.5f) + shiftAmount;
        }
    }



    void GenerateNewLetter()
    {
        currentLetter = Random.value > 0.5f
            ? (char)Random.Range(65, 91)  // Capital A-Z
            : (char)Random.Range(97, 123); // Small a-z
        mainLetterText.text = currentLetter.ToString();
        mainLetterText.color = colors[Random.Range(0, colors.Length)];
    }

    void SpawnFallingLetter()
    {
        if (letterPrefab == null || spawnPoint == null || canvasTransform == null)
        {
            Debug.LogError("Missing references in LetterSpawner!");
            return;
        }

        char fallingLetter = char.IsUpper(currentLetter)
            ? (char)Random.Range(97, 123)
            : (char)Random.Range(65, 91);

        GameObject newLetter = Instantiate(letterPrefab, canvasTransform);
        newLetter.name = "FallingLetter_" + fallingLetter;

        if (newLetter.TryGetComponent(out RectTransform rectTransform))
        {
            // Get spawn point Y but use random X within calculated bounds
            Vector2 spawnPos = spawnPoint.position;
            spawnPos.x = Random.Range(minX, maxX);

            rectTransform.anchoredPosition = canvasTransform.InverseTransformPoint(spawnPos);
        }
        else
        {
            Debug.LogError("RectTransform missing on Falling Letter Prefab!");
        }

        TextMeshProUGUI letterText = newLetter.GetComponentInChildren<TextMeshProUGUI>();
        if (letterText != null)
        {
            letterText.text = fallingLetter.ToString();
            letterText.color = colors[Random.Range(0, colors.Length)];
        }
        else
        {
            Debug.LogError("TextMeshProUGUI missing on Falling Letter Prefab!");
        }
    }
}