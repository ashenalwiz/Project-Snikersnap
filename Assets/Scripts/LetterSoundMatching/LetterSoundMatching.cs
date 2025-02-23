using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LetterSoundMatching : MonoBehaviour
{
    public TextMeshProUGUI letterDisplay;
    public TextMeshProUGUI pointsDisplay;
    public Button playSoundButton;
    public int pointCount;
    public Button[] answerButtons;
    public AudioSource audioSource;
    public GameObject feedbackPanel;
    public GameObject victoryPanel;
    public GameObject gameUI;
    public TextMeshProUGUI feedbackText;

    private Dictionary<string, AudioClip> letterSounds;
    private string correctLetter;

    void Start()
    {
        Debug.Log("Starting game initialization...");

        // Ensure UI elements are assigned
        if (victoryPanel == null) Debug.LogError("victoryPanel is not assigned!");
        if (gameUI == null) Debug.LogError("gameUI is not assigned!");
        if (letterDisplay == null) Debug.LogError("letterDisplay is not assigned!");
        if (pointsDisplay == null) Debug.LogError("pointsDisplay is not assigned!");
        if (audioSource == null) Debug.LogError("audioSource is not assigned!");

        pointsDisplay.text = "Points: 0";
        gameUI.SetActive(true);
        LoadLetterSounds();
        SetNewLetter();
        feedbackPanel.SetActive(false);
        victoryPanel.SetActive(false);

        if (playSoundButton != null)
            playSoundButton.onClick.AddListener(PlayLetterSound);
    }

    void LoadLetterSounds()
    {
        letterSounds = new Dictionary<string, AudioClip>();
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        foreach (char letter in alphabet)
        {
            string letterStr = letter.ToString();
            AudioClip clip = Resources.Load<AudioClip>($"LetterSoundMatching/Sounds/OGG/{letterStr.ToLower()}");

            if (clip == null)
            {
                Debug.LogError($"Failed to load audio clip for letter {letterStr}. Ensure file exists at Resources/LetterSoundMatching/Sounds/{letterStr.ToLower()}.wav");
            }
            else
            {
                letterSounds.Add(letterStr, clip);
                Debug.Log($"Successfully loaded audio clip for letter {letterStr}");
            }
        }

        Debug.Log($"Loaded {letterSounds.Count} letter sounds");
    }

    void SetNewLetter()
    {
        correctLetter = GetRandomLetter();
        letterDisplay.text = correctLetter;

        // Create list of incorrect options
        List<string> options = new List<string>(letterSounds.Keys);
        options.Remove(correctLetter);
        ShuffleList(options);

        // Create options list (correct answer + incorrect answers)
        List<string> allOptions = new List<string> { correctLetter };
        allOptions.AddRange(options.GetRange(0, answerButtons.Length - 1));
        ShuffleList(allOptions);

        // Assign options to buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            string letter = allOptions[i];
            TextMeshProUGUI btnText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();

            if (btnText != null)
            {
                btnText.text = letter;
            }
            else
            {
                Debug.LogError($"TextMeshProUGUI component not found in button: {answerButtons[i].name}");
            }

            int buttonIndex = i; // Capture the index for the lambda
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => CheckAnswer(allOptions[buttonIndex]));
        }
    }

    public void PlayLetterSound()
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is not assigned!");
            return;
        }

        if (letterSounds.ContainsKey(correctLetter))
        {
            AudioClip clip = letterSounds[correctLetter];
            if (clip != null)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(clip);
                    Debug.Log($"Playing sound for letter: {correctLetter}");
                }
                else
                {
                    Debug.Log("Audio is already playing.");
                }
            }
            else
            {
                Debug.LogError($"AudioClip is null for letter: {correctLetter}");
            }
        }
        else
        {
            Debug.LogError($"Letter key missing in dictionary: {correctLetter}");
        }
    }

    void CheckAnswer(string selectedLetter)
    {
        bool isCorrect = selectedLetter == correctLetter;
        ShowFeedback(isCorrect);

        if (isCorrect)
        {
            pointsDisplay.text = "Points: " + (++pointCount);
            AudioClip correctClip = Resources.Load<AudioClip>("LetterSoundMatching/Sounds/correct");

            if (correctClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(correctClip);
                Debug.Log("Playing correct sound effect");
            }
            else
            {
                Debug.LogError("Correct sound effect not found!");
            }

            if (pointCount >= 5)
            {
                FinishLevel();
            }
            else
            {
                Invoke("SetNewLetter", 2f);
            }
        }
        else
        {
            AudioClip wrongClip = Resources.Load<AudioClip>("LetterSoundMatching/Sounds/wrong");
            if (wrongClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(wrongClip);
                Debug.Log("Playing wrong sound effect");
            }
            else
            {
                Debug.LogError("Wrong sound effect not found!");
            }
        }
    }

    void ShowFeedback(bool isCorrect)
    {
        feedbackPanel.SetActive(true);
        feedbackText.color = isCorrect ? new Color(0f, 0.5f, 0f) : Color.red;
        feedbackText.text = isCorrect ? "Correct!" : "Incorrect!";
        Invoke("HideFeedback", 2f);
    }

    void HideFeedback()
    {
        feedbackPanel.SetActive(false);
    }

    void ShuffleList(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    string GetRandomLetter()
    {
        List<string> keys = new List<string>(letterSounds.Keys);
        return keys[Random.Range(0, keys.Count)];
    }

    void FinishLevel()
    {
        Debug.Log("Level Completed!");
        victoryPanel.SetActive(true);
        gameUI.SetActive(false);
    }
}