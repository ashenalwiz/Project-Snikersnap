/*using System.Collections;
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
}*/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class LetterSoundMatchUp : MonoBehaviour
{
    public Button[] audioButtons;
    public Button[] letterGroups;
    public AudioSource audioSource;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI winText, roundText;
    public Button nextButton;
    


    private Dictionary<string, AudioClip> letterSoundMap = new Dictionary<string, AudioClip>();
    private Dictionary<int, string> buttonToLetterMap = new Dictionary<int, string>();
    private int selectedAudioIndex = -1;
    private int correctAnswers = 0;
    private int totalScore = 0;
    private int currentRound = 0;
    private string[] currentGroupLetters;
    private List<string> letterCombinations = new List<string>();

    private readonly string[] similarLetterGroups =
    {
        "p|q|b|d", "m|n|u|w", "v|y|x", "c|e|o", "l|i|t",
        "E|F|H", "M|N|W", "O|Q|C", "P|R|B", "I|J|L",
        "D|b|p|q", "l|I"
    };

    private Color defaultButtonColor = Color.white;
    private List<Color> matchColors = new List<Color> { Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan };
    private Dictionary<int, Color> assignedColors = new Dictionary<int, Color>();

    private HashSet<string> usedLetterGroups = new HashSet<string>();

    void Start()
    {
        if (!ValidateReferences()) return;
        winText.gameObject.SetActive(false);
        scoreText.text = "Score: 0";

        LoadLetterSounds();
        SelectSingleGroupAndAssign();
        ResetButtonColors();

        nextButton.gameObject.SetActive(false); // Hide Next button initially
        nextButton.onClick.AddListener(NextLetterGroup); // Add button click event
    }

    void ResetButtonColors()
    {
        foreach (Button btn in audioButtons)
        {
            btn.image.color = defaultButtonColor;
        }
        foreach (Button btn in letterGroups)
        {
            btn.image.color = defaultButtonColor;
        }
    }

    bool ValidateReferences()
    {
        if (audioButtons.Length == 0 || letterGroups.Length == 0 || audioSource == null || scoreText == null || winText == null)
        {
            Debug.LogError("Some UI elements are not assigned!");
            return false;
        }
        return true;
    }

    void LoadLetterSounds()
    {
        letterSoundMap.Clear();
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Task2/Sounds");
        foreach (AudioClip clip in clips)
        {
            letterSoundMap[clip.name.ToLower()] = clip;
        }
    }

    void SelectSingleGroupAndAssign()
    {
        if (usedLetterGroups.Count == similarLetterGroups.Length)
        {
            usedLetterGroups.Clear(); // Reset if all groups are used
            currentRound = 1; // Reset round
        }
        else
        {
            currentRound++; // Increase round
        }

        // Update UI
        if (roundText)
            roundText.text = "Round: " + currentRound;

        string chosenGroup;
        do
        {
            chosenGroup = similarLetterGroups[Random.Range(0, similarLetterGroups.Length)];
        }
        while (usedLetterGroups.Contains(chosenGroup)); // Ensure it's new

        usedLetterGroups.Add(chosenGroup); // Mark it as used
        currentGroupLetters = chosenGroup.Split('|');

        letterCombinations.Clear();
        HashSet<string> uniqueCombinations = new HashSet<string>();
        while (letterCombinations.Count < audioButtons.Length)
        {
            string letterCombination = GenerateRandomLetterCombination();
            if (uniqueCombinations.Add(letterCombination))
                letterCombinations.Add(letterCombination);
        }

        List<string> shuffledCombinations = new List<string>(letterCombinations);
        ShuffleList(shuffledCombinations);

        for (int i = 0; i < audioButtons.Length; i++)
        {
            buttonToLetterMap[i] = letterCombinations[i];
            if (audioButtons[i].GetComponentInChildren<TextMeshProUGUI>())
                audioButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = "Play Sound";

            if (letterGroups[i].GetComponentInChildren<TextMeshProUGUI>())
                letterGroups[i].GetComponentInChildren<TextMeshProUGUI>().text = shuffledCombinations[i];

            int index = i;
            audioButtons[i].onClick.AddListener(() => SelectAudioButton(index));
            letterGroups[i].onClick.AddListener(() => CheckMatch(index));
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
            (list[i], list[randIndex]) = (list[randIndex], list[i]);
        }
    }

    void SelectAudioButton(int index)
    {
        selectedAudioIndex = index;
        audioButtons[index].image.color = Color.gray; // Indicate selection
        StartCoroutine(PlayLetterSounds(buttonToLetterMap[index]));
    }

    IEnumerator PlayLetterSounds(string group)
    {
        string[] letters = group.Split('|');
        foreach (string letter in letters)
        {
            if (letterSoundMap.TryGetValue(letter.ToLower(), out AudioClip clip))
            {
                audioSource.clip = clip;
                audioSource.Play();
                yield return new WaitForSeconds(audioSource.clip.length);
            }
        }
    }

    void CheckMatch(int letterIndex)
    {
        if (selectedAudioIndex == -1) return;

        string audioGroup = buttonToLetterMap[selectedAudioIndex];
        string letterGroup = letterGroups[letterIndex].GetComponentInChildren<TextMeshProUGUI>().text;

        if (audioGroup == letterGroup)
        {
            correctAnswers++;
            scoreText.text = "Score: " + correctAnswers;

            if (!assignedColors.ContainsKey(selectedAudioIndex))
            {
                assignedColors[selectedAudioIndex] = matchColors[correctAnswers % matchColors.Count];
            }
            Color pairColor = assignedColors[selectedAudioIndex];

            audioButtons[selectedAudioIndex].image.color = pairColor;
            letterGroups[letterIndex].image.color = pairColor;

            audioButtons[selectedAudioIndex].interactable = false;
            
            letterGroups[letterIndex].interactable = false;


            if (correctAnswers == audioButtons.Length)
            {
                winText.gameObject.SetActive(true);
                nextButton.gameObject.SetActive(true);
            }
        }
        else
        {
            StartCoroutine(ShakeButton(letterGroups[letterIndex]));
        }
        selectedAudioIndex = -1;
    }

    IEnumerator ShakeButton(Button button)
    {
        Vector3 originalPos = button.transform.position;
        for (int i = 0; i < 5; i++)
        {
            button.transform.position = originalPos + new Vector3(Random.Range(-5f, 5f), 0, 0);
            yield return new WaitForSeconds(0.05f);
        }
        button.transform.position = originalPos;
    }

    void NextLetterGroup()
    {
        totalScore += correctAnswers; // Keep previous round's score
        correctAnswers = 0;

        scoreText.text = "Score: " + totalScore; // Display cumulative score
        winText.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false); // Hide Next button again

        ResetButtonColors();
        SelectSingleGroupAndAssign();

        foreach (Button btn in audioButtons)
        {
            btn.interactable = true; // Reactivate audio buttons
        }
        foreach (Button btn in letterGroups)
        {
            btn.gameObject.SetActive(true);  // Show letter group buttons
            btn.interactable = true; // Reactivate letter group buttons
        }
    }



}
