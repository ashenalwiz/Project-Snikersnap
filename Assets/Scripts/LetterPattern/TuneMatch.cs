using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Security.Cryptography;

public class LetterSoundMatchUp : MonoBehaviour
{
    public Button[] audioButtons;
    public Button[] letterGroups;
    public Button progressButton, replayButton,exitButton;
    public AudioSource audioSource;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI winText, roundText;
    public Button nextButton;
    public GameObject gameOver;



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
        "p|q|b|d", "m|n|u|w"
            ,"v|y|x", "c|e|o", "l|i|t",
        "E|F|H", "M|N|W", "O|Q|C", "P|R|B", "I|J|L",
        
    };

    private Color defaultButtonColor = Color.white;
    private List<Color> matchColors = new List<Color>
    {
        new Color(0.70f, 0.90f, 0.70f), 
        new Color(1.00f, 0.93f, 0.60f), 
        new Color(0.98f, 0.75f, 0.80f),  
        new Color(0.74f, 0.85f, 0.98f),  
        new Color(0.80f, 0.75f, 0.96f) 
    };

    private Dictionary<int, Color> assignedColors = new Dictionary<int, Color>();

    private HashSet<string> usedLetterGroups = new HashSet<string>();

    void Start()
    {
        if (!ValidateReferences()) return;
        winText.gameObject.SetActive(false);
        totalScore = 1;
        scoreText.text = "Score: " + totalScore;  

        LoadLetterSounds();
        SelectSingleGroupAndAssign();
        ResetButtonColors();

        nextButton.gameObject.SetActive(false); // Hide Next button initially
        nextButton.onClick.AddListener(NextLetterGroup); // Add button click event

        progressButton.gameObject.SetActive(false);
        replayButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(false);
        gameOver.SetActive(false);
        progressButton.onClick.AddListener(ShowProgress);
        replayButton.onClick.AddListener(ReplayGame);
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
            usedLetterGroups.Clear(); 
            currentRound = 1; 
        }
        else
        {
            currentRound++; // Increase round
        }

        if (roundText)
            roundText.text = "Round: " + currentRound;

        string chosenGroup;
        do
        {
            chosenGroup = similarLetterGroups[Random.Range(0, similarLetterGroups.Length)];
        }
        while (usedLetterGroups.Contains(chosenGroup)); 

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
        scoreText.gameObject.SetActive(false);
        totalScore++; // Increment score after the round is completed

        int lastRound = 2; // Change this to your desired last round

        if (currentRound >= lastRound) // End the game at the final round

        {
            nextButton.gameObject.SetActive(false);
            gameOver.SetActive(true);
            progressButton.gameObject.SetActive(true);
            replayButton.gameObject.SetActive(true);
            exitButton.gameObject.SetActive(true);

            foreach (Button btn in audioButtons) { btn.gameObject.SetActive(false); }
            foreach (Button btn in letterGroups) { btn.gameObject.SetActive(false); }

            roundText.gameObject.SetActive(false);
            scoreText.gameObject.SetActive(false);
        }
        else
        {
            scoreText.gameObject.SetActive(false);
            gameOver.SetActive(false);
            nextButton.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(false);
            correctAnswers = 0; // Reset for the new round

            ResetButtonColors();
            SelectSingleGroupAndAssign();

            foreach (Button btn in audioButtons) { btn.interactable = true; }
            foreach (Button btn in letterGroups) { btn.gameObject.SetActive(true); btn.interactable = true; }

            roundText.text = "Round: " + currentRound; // Update round display

        }
        scoreText.text = "Score: " + totalScore;
    }

    void ShowProgress()
    {
        Debug.Log("Showing Progress...");
        //  load another UI screen or scene here
    }
    void ReplayGame()
    {
        Debug.Log("Restarting Game...");

        totalScore = 0;
        currentRound = 0; // Reset rounds
        usedLetterGroups.Clear();

        winText.gameObject.SetActive(true);
        gameOver.SetActive(false);
        progressButton.gameObject.SetActive(false);
        replayButton.gameObject.SetActive(false);
        foreach (Button btn in audioButtons)
        {
            btn.gameObject.SetActive(true);
        }
        foreach (Button btn in letterGroups)
        {
            btn.gameObject.SetActive(true);
        }
        roundText.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(true);
        winText.gameObject.SetActive(true);

        NextLetterGroup(); // Start fresh
    }
}
