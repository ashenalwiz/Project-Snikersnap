using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem; // Add this namespace for the new input system

public class LetterSoundMatchUp : MonoBehaviour
{
    public Button[] audioButtons;
    public TextMeshProUGUI[] letterGroups;
    public AudioSource audioSource;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI winText; // Text to display "You Win!" message

    private Dictionary<string, AudioClip> letterSoundMap;
    private string[] similarLetterGroups = new string[]
    {
        "p|q|b|d", "m|n|u|w", "v|y|x", "c|e|o", "l|i|t",
        "E|F|H", "M|N|W", "O|Q|C", "P|R|B", "I|J|L",
        "D|b|p|q", "l|I"
    };

    private Dictionary<int, string> buttonToLetterMap;
    private int selectedAudioIndex = -1;
    private int correctAnswers = 0;
    private string[] currentGroupLetters;
    private List<string> letterCombinations;

    private LineRenderer[] lineRenderers; // Array of LineRenderers for each button
    private Color[] lineColors = new Color[]
    {
        Color.red, Color.green, Color.blue, Color.yellow
    };

    private bool isDragging = false; // Track if the player is dragging a line

    void Start()
    {
        // Initialize LineRenderers
        lineRenderers = new LineRenderer[audioButtons.Length];
        for (int i = 0; i < audioButtons.Length; i++)
        {
            GameObject lineObj = new GameObject("LineRenderer_" + i);
            lineRenderers[i] = lineObj.AddComponent<LineRenderer>();
            lineRenderers[i].positionCount = 2;
            lineRenderers[i].startWidth = 0.1f;
            lineRenderers[i].endWidth = 0.1f;
            lineRenderers[i].material = new Material(Shader.Find("Sprites/Default"));
            lineRenderers[i].startColor = lineColors[i];
            lineRenderers[i].endColor = lineColors[i];
            lineRenderers[i].enabled = false;
        }

        buttonToLetterMap = new Dictionary<int, string>();

        winText.gameObject.SetActive(false); // Hide the win text initially
        scoreText.text = "Score: 0"; // Initialize the score text

        LoadLetterSounds();
        SelectSingleGroupAndAssign();
    }

    void LoadLetterSounds()
    {
        letterSoundMap = new Dictionary<string, AudioClip>();
        AudioClip[] clips = Resources.LoadAll<AudioClip>("LetterSoundMatching/Sounds");

        foreach (AudioClip clip in clips)
        {
            string letter = clip.name.ToLower();
            letterSoundMap[letter] = clip;
        }
    }

    void SelectSingleGroupAndAssign()
    {
        // Step 1: Select a single random group
        string chosenGroup = similarLetterGroups[Random.Range(0, similarLetterGroups.Length)];
        currentGroupLetters = chosenGroup.Split('|');

    // Step 2: Generate unique letter combinations for all buttons/answers
    letterCombinations = new List<string>();
    HashSet<string> uniqueCombinations = new HashSet<string>();
    while(letterCombinations.Count < audioButtons.Length)
    {
        string letterCombination = GenerateRandomLetterCombination();
        if(!uniqueCombinations.Contains(letterCombination))
        {
            uniqueCombinations.Add(letterCombination);
            letterCombinations.Add(letterCombination);
        }
    }

        // Step 3: Shuffle answers to randomize their positions
        List<string> shuffledCombinations = new List<string>(letterCombinations);
        ShuffleList(shuffledCombinations);

        // Step 4: Assign the same text to both buttons and answers
        for (int i = 0; i < audioButtons.Length; i++)
        {
            buttonToLetterMap[i] = letterCombinations[i];

            // Set the sound button text
            audioButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = letterCombinations[i];

            // Set the answer text (but in shuffled order)
            letterGroups[i].text = shuffledCombinations[i];

            // Assign click event
            int index = i;
            audioButtons[i].onClick.AddListener(() => SelectAudioButton(index));
        }
    }

    string GenerateRandomLetterCombination()
    {
        HashSet<string> chosenLetters = new HashSet<string>();
        while (chosenLetters.Count < 3)
        {
            chosenLetters.Add(currentGroupLetters[Random.Range(0, currentGroupLetters.Length)]);
        }
        return string.Join("|", chosenLetters);
    }

    void ShuffleList(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randIndex = Random.Range(0, i + 1);
            string temp = list[i];
            list[i] = list[randIndex];
            list[randIndex] = temp;
        }
    }

    void SelectAudioButton(int index)
    {
        selectedAudioIndex = index;
        lineRenderers[index].enabled = true;
        isDragging = true;
        StartCoroutine(PlayLetterSounds(buttonToLetterMap[index]));
    }

    IEnumerator PlayLetterSounds(string group)
    {
        string[] letters = group.Split('|');
        foreach (string letter in letters)
        {
            string letterLower = letter.ToLower();
            if (letterSoundMap.ContainsKey(letterLower))
            {
                audioSource.clip = letterSoundMap[letterLower];
                audioSource.Play();
                yield return new WaitForSeconds(audioSource.clip.length);
            }
        }
    }

    void Update()
    {
        if (isDragging)
        {
            // Get touch position
            if (Touchscreen.current.primaryTouch.press.isPressed)
            {
                Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, 0));
                worldPos.z = 0;

                // Set the starting point of the line to the right side of the button
                Vector3 buttonPos = audioButtons[selectedAudioIndex].transform.position;
                buttonPos.x += 1.0f; // Adjust this value to position the line start

                // Draw the line from the button to the touch position
                lineRenderers[selectedAudioIndex].SetPosition(0, buttonPos);
                lineRenderers[selectedAudioIndex].SetPosition(1, worldPos);
            }
            else
            {
                // Touch released, check for match
                Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, 0));
                worldPos.z = 0;

                // Check if the line was dragged to a letter group
                for (int i = 0; i < letterGroups.Length; i++)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(letterGroups[i].rectTransform, touchPosition))
                    {
                        CheckMatch(selectedAudioIndex, i);
                        break;
                    }
                }

                // Reset the line
                lineRenderers[selectedAudioIndex].enabled = false;
                isDragging = false;
                selectedAudioIndex = -1;
            }
        }
    }

    void CheckMatch(int audioIndex, int letterIndex)
    {
        string audioGroup = buttonToLetterMap[audioIndex];
        string letterGroup = letterGroups[letterIndex].text;

        if (audioGroup == letterGroup)
        {
            correctAnswers++;
            scoreText.text = "Score: " + correctAnswers;
            Debug.Log("Correct Match!");

            // Disable the matched button and letter group
            audioButtons[audioIndex].interactable = false;
            letterGroups[letterIndex].gameObject.SetActive(false);

            // Check if all matches are correct
            if (correctAnswers == audioButtons.Length)
            {
                winText.gameObject.SetActive(true); // Show the win text
                Debug.Log("You Win!");
            }
        }
        else
        {
            Debug.Log("Incorrect Match!");
        }
    }
}