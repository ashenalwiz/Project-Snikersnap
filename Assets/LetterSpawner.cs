using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class LetterSpawner : MonoBehaviour
{
    public TextMeshProUGUI mainLetterText;
    public GameObject letterPrefab;
    public RectTransform canvasTransform;
    public float spawnInterval = 0.5f;
    public float fallSpeed = 30f; // Even slower fall speed
    public int lettersPerSpawn = 3; // Number of letters to spawn simultaneously

    private char currentLetter;
    private float minX = -9.68f; // Updated range
    private float maxX = 3.51f;
    private List<RectTransform> activeLetters = new List<RectTransform>();
    private float destroyY;

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
        GenerateNewLetter();
        InvokeRepeating(nameof(SpawnFallingLetter), 1f, spawnInterval);
        destroyY = -canvasTransform.rect.height / 2;
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
        if (letterPrefab == null || canvasTransform == null)
        {
            Debug.LogError("Missing references in LetterSpawner!");
            return;
        }

        for (int i = 0; i < lettersPerSpawn; i++)
        {
            char fallingLetter = char.IsUpper(currentLetter)
                ? (char)Random.Range(97, 123)
                : (char)Random.Range(65, 91);

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
