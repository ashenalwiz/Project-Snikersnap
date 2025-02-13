using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class LetterSpawner : MonoBehaviour
{
    public TextMeshProUGUI mainLetterText;
    public GameObject letterPrefab;
    public Transform spawnPoint;
    public RectTransform canvasTransform;
    public float spawnInterval = 0.5f;
    public float fallSpeed = 200f;

    private char currentLetter;
    private float minX;
    private float maxX;
    private List<RectTransform> activeLetters = new List<RectTransform>();
    private float destroyY; // Canvas space Y coordinate for destruction

    private Color[] colors = new Color[]
    {
        new Color(0.6f, 0.1f, 0.1f),  // Dark Red
        new Color(0.5f, 0.2f, 0.7f),  // Deep Purple
        new Color(0.9f, 0.4f, 0f),    // Burnt Orange
        new Color(0.3f, 0.1f, 0.6f),  // Dark Indigo
        new Color(0.8f, 0.2f, 0.5f),  // Deep Pink
        new Color(0.2f, 0.1f, 0.7f),  // Royal Blue
        new Color(1f, 0.5f, 0.2f),    // Dark Peach
        new Color(0.6f, 0.3f, 0.2f)   // Dark Brown
    };

    void Start()
    {
        CalculateSpawnBounds();
        GenerateNewLetter();
        InvokeRepeating(nameof(SpawnFallingLetter), 1f, spawnInterval);

        // Convert world space Y=69 to canvas space
        Vector3 worldDestroyPoint = new Vector3(0, 69, 0);
        Vector2 canvasDestroyPoint = canvasTransform.InverseTransformPoint(worldDestroyPoint);
        destroyY = canvasDestroyPoint.y;
    }

    void Update()
    {
        for (int i = activeLetters.Count - 1; i >= 0; i--)
        {
            if (activeLetters[i] == null) continue;

            RectTransform letter = activeLetters[i];
            Vector2 position = letter.anchoredPosition;
            float newY = position.y - fallSpeed * Time.deltaTime;
            letter.anchoredPosition = new Vector2(position.x, newY);

            // Convert letter's world position to check against Y=69
            Vector3 worldPos = canvasTransform.TransformPoint(new Vector3(position.x, newY, 0));
            if (worldPos.y <= 69)
            {
                Destroy(letter.gameObject);
                activeLetters.RemoveAt(i);
            }
        }
    }

    void CalculateSpawnBounds()
    {
        Canvas canvas = canvasTransform.GetComponent<Canvas>();
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay ||
            canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            float canvasWidth = canvasTransform.rect.width;
            float spawnWidth = canvasWidth * 0.75f;
            float shiftAmount = canvasWidth * 0.534f;
            minX = -(spawnWidth * 0.5f) + shiftAmount;
            maxX = (spawnWidth * 0.5f) + shiftAmount;
        }
    }

    void GenerateNewLetter()
    {
        currentLetter = Random.value > 0.5f
            ? (char)Random.Range(65, 91)
            : (char)Random.Range(97, 123);
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
            Vector2 spawnPos = spawnPoint.position;
            spawnPos.x = Random.Range(minX, maxX);
            rectTransform.anchoredPosition = canvasTransform.InverseTransformPoint(spawnPos);
            activeLetters.Add(rectTransform);
        }
        else
        {
            Debug.LogError("RectTransform missing on Falling Letter Prefab!");
            return;
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