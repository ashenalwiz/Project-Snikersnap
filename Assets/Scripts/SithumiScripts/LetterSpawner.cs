using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class LetterSpawner : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI mainLetterText; // Displays the main letter to match
    public GameObject letterPrefab; // Prefab for falling letters
    public RectTransform canvasTransform; // Reference to the UI canvas

    [Header("Spawn Settings")]
    public float spawnInterval = 0.5f; // Time between each letter spawn
    public float fallSpeed = 30f; // Speed at which letters fall (currently unused)
    public int lettersPerSpawn = 3; // Number of letters spawned at a time
    [Range(0, 1)]
    public float matchingLetterChance = 0.2f; // 20% chance for a matching letter

    private char currentLetter; // Stores the letter to match
    private float minX = -9.68f; // Minimum X position for spawning letters
    private float maxX = 3.51f; // Maximum X position for spawning letters
    private List<RectTransform> activeLetters = new List<RectTransform>(); // List of active falling letters
    private float destroyY; // Y position at which letters get destroyed
    private bool isSpawning = false; // Flag to track if spawning is active

    private Color[] colors = new Color[] // Array of random colors for letters
    {
        new Color(0.6f, 0.1f, 0.1f),
        new Color(0.5f, 0.2f, 0.7f),
        new Color(0.9f, 0.4f, 0f),
        new Color(0.3f, 0.1f, 0.6f),
        new Color(0.8f, 0.2f, 0.5f),
        new Color(0.2f, 0.1f, 0.7f),
        new Color(1f, 0.5f, 0.2f),
        new Color(0.6f, 0.3f, 0.2f)
    };

    void Start()
    {
        // Initialize destruction Y position based on canvas height
        destroyY = -canvasTransform.rect.height / 2;
        GenerateNewLetter(); // Generate the first letter to match
    }

    void Update()
    {
        // Move each active letter downward slowly
        for (int i = activeLetters.Count - 1; i >= 0; i--)
        {
            if (activeLetters[i] == null) continue;

            RectTransform letter = activeLetters[i];
            Vector2 position = letter.anchoredPosition;

            // Apply very slow floating effect for falling letters
            position.y = Mathf.Lerp(position.y, position.y - 0.1f, Time.deltaTime * 0.001f);
            position.y -= 0.001f * Time.deltaTime;

            letter.anchoredPosition = position;

            // Destroy letters that fall below the threshold
            if (position.y <= destroyY)
            {
                Destroy(letter.gameObject);
                activeLetters.RemoveAt(i);
            }
        }
    }

    // Starts letter spawning (call this after countdown finishes)
    public void StartLetterSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            GenerateNewLetter(); // Generate a new letter before spawning starts
            InvokeRepeating(nameof(SpawnFallingLetter), 1f, spawnInterval);
        }
    }

    // Stops letter spawning (e.g., when the game is paused)
    public void StopLetterSpawning()
    {
        if (isSpawning)
        {
            isSpawning = false;
            CancelInvoke(nameof(SpawnFallingLetter));
        }
    }

    // Generates a new letter to match and updates UI
    public void GenerateNewLetter()
    {
        // Randomly choose an uppercase or lowercase letter
        currentLetter = Random.value > 0.5f
            ? (char)Random.Range(65, 91) // Uppercase A-Z
            : (char)Random.Range(97, 123); // Lowercase a-z

        mainLetterText.text = currentLetter.ToString(); // Update UI
        mainLetterText.color = colors[Random.Range(0, colors.Length)]; // Assign random color
    }

    // Clears all falling letters from the screen
    public void ClearFallingLetters()
    {
        for (int i = activeLetters.Count - 1; i >= 0; i--)
        {
            if (activeLetters[i] != null)
            {
                Destroy(activeLetters[i].gameObject);
            }
        }
        activeLetters.Clear();
    }

    // Spawns falling letters with random characters
    void SpawnFallingLetter()
    {
        if (letterPrefab == null || canvasTransform == null)
        {
            Debug.LogError("Missing references in LetterSpawner!");
            return;
        }

        for (int i = 0; i < lettersPerSpawn; i++)
        {
            char fallingLetter;
            bool shouldMatch = Random.value <= matchingLetterChance; // Check if letter should match

            if (shouldMatch)
            {
                // Use the opposite case of the current letter
                fallingLetter = char.IsUpper(currentLetter)
                    ? char.ToLower(currentLetter)
                    : char.ToUpper(currentLetter);
            }
            else
            {
                // Generate a random letter of the opposite case, ensuring it’s not the main letter
                char randomLetter;
                do
                {
                    randomLetter = char.IsUpper(currentLetter)
                        ? (char)Random.Range(97, 123)  // Lowercase a-z
                        : (char)Random.Range(65, 91);  // Uppercase A-Z
                } while (char.ToUpper(randomLetter) == char.ToUpper(currentLetter));

                fallingLetter = randomLetter;
            }

            // Instantiate letter prefab and position it
            GameObject newLetter = Instantiate(letterPrefab, canvasTransform);
            newLetter.name = "FallingLetter_" + fallingLetter;

            if (newLetter.TryGetComponent(out RectTransform rectTransform))
            {
                Vector2 spawnPos = new Vector2(Random.Range(minX, maxX), canvasTransform.rect.height / 350);
                rectTransform.anchoredPosition = spawnPos;
                activeLetters.Add(rectTransform);
            }
            else
            {
                Debug.LogError("RectTransform missing on Falling Letter Prefab!");
                continue;
            }

            // Set letter text and color
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
}