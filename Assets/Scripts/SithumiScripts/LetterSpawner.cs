using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class LetterSpawner : MonoBehaviour
{
    public TextMeshProUGUI mainLetterText;
    public GameObject letterPrefab;
    public RectTransform canvasTransform;
    public float spawnInterval = 0.5f;
    public float fallSpeed = 30f;
    public int lettersPerSpawn = 3;
    [Range(0, 1)]
    public float matchingLetterChance = 0.2f; // 20% chance for matching letters

    private char currentLetter;
    private float minX = -9.68f;
    private float maxX = 3.51f;
    private List<RectTransform> activeLetters = new List<RectTransform>();
    private float destroyY;
    private bool isSpawning = false;

    private Color[] colors = new Color[]
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
        // Initialize values but don't start spawning yet
        destroyY = -canvasTransform.rect.height / 2;
        GenerateNewLetter();

        // We'll start spawning after the countdown finishes
    }

    void Update()
    {
        for (int i = activeLetters.Count - 1; i >= 0; i--)
        {
            if (activeLetters[i] == null) continue;

            RectTransform letter = activeLetters[i];
            Vector2 position = letter.anchoredPosition;

            // ULTRA slow falling effect (almost floating)
            position.y = Mathf.Lerp(position.y, position.y - 0.1f, Time.deltaTime * 0.001f);
            position.y -= 0.001f * Time.deltaTime; // Tiny additional movement

            letter.anchoredPosition = position;

            if (position.y <= destroyY)
            {
                Destroy(letter.gameObject);
                activeLetters.RemoveAt(i);
            }
        }
    }

    // Call this method after countdown finishes
    public void StartLetterSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            GenerateNewLetter();
            InvokeRepeating(nameof(SpawnFallingLetter), 1f, spawnInterval);
        }
    }

    // Call this to stop spawning (e.g. when game pauses)
    public void StopLetterSpawning()
    {
        if (isSpawning)
        {
            isSpawning = false;
            CancelInvoke(nameof(SpawnFallingLetter));
        }
    }

    public void GenerateNewLetter()
    {
        currentLetter = Random.value > 0.5f
            ? (char)Random.Range(65, 91) // Uppercase A-Z
            : (char)Random.Range(97, 123); // Lowercase a-z
        mainLetterText.text = currentLetter.ToString();
        mainLetterText.color = colors[Random.Range(0, colors.Length)];
    }

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

            // Determine if this letter should match the main letter (with opposite case)
            bool shouldMatch = Random.value <= matchingLetterChance;

            if (shouldMatch)
            {
                // Use the opposite case of the current letter
                fallingLetter = char.IsUpper(currentLetter)
                    ? char.ToLower(currentLetter)
                    : char.ToUpper(currentLetter);
            }
            else
            {
                // Generate a random letter of the opposite case
                // Make sure it's not the same letter as the main letter (even in different case)
                char randomLetter;
                do
                {
                    randomLetter = char.IsUpper(currentLetter)
                        ? (char)Random.Range(97, 123)  // lowercase a-z
                        : (char)Random.Range(65, 91);  // uppercase A-Z
                } while (char.ToUpper(randomLetter) == char.ToUpper(currentLetter));

                fallingLetter = randomLetter;
            }

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