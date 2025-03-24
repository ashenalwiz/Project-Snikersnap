using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using Unity.IO.LowLevel.Unsafe;
using System;

// Make all classes serializable to ensure proper JSON conversion
[System.Serializable]
public class WordAttempt
{
    public string word;
    public int attempts;
    public bool correct;
    public bool skipped;
}

[System.Serializable]
public class SessionData2
{
    public List<WordAttempt> wordAttempts = new List<WordAttempt>();
}

public class TypingGame : MonoBehaviour
{
    public Button playAudioButton;
    public TMP_InputField wordInputField;
    public Button checkButton;
    public TMP_Text feedbackText;
    public AudioSource audioSource;
    public AudioClip okSound;
    public AudioClip wrongSound;
    public AudioClip skipedSound;
    public AudioClip victoryEndSound;
    public AudioClip[] wordAudioClips;
    private int currentWordIndex = 0;
    public UnityEngine.UI.Image hintImage;
    public Button skipButton;
    private Dictionary<string, Sprite> wordImageMap;
    private int attempts = 0;

    [SerializeField] private string saveFileName = "Task8UserProgress.json";

    private SessionData2 SessionData2 = new SessionData2();
    private string jsonFilePath;

    public GameObject resultPanel;
    public List<Image> starImages = new List<Image>();
    public TMP_Text finalScoreText;

    private int correctAnswers = 0;
    private int skippedWords = 0;
    public Button replayButton;

    void Start()
    {
        jsonFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
        LoadPreviousData();

        resultPanel.SetActive(false);
        replayButton.gameObject.SetActive(false);
        replayButton.onClick.AddListener(ReplayGame);

        playAudioButton.onClick.AddListener(PlayWordAudio);
        checkButton.onClick.AddListener(CheckWord);
        skipButton.onClick.AddListener(SkipWord);

        hintImage.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);

        InitializeWordImageDictionary();

        feedbackText.text = "";
        Debug.Log("User Progress File Path: " + jsonFilePath);

        PlayWordAudio(); // Start by playing the first word's audio
    }

    private void InitializeWordImageDictionary()
    {
        wordImageMap = new Dictionary<string, Sprite>
        {
            { "Rock", Resources.Load<Sprite>("HintImages/Rock") },
            { "Cave", Resources.Load<Sprite>("HintImages/Cave") },
            { "Flag", Resources.Load<Sprite>("HintImages/Flag") },
            { "Dark", Resources.Load<Sprite>("HintImages/Dark") },
            { "Shark", Resources.Load<Sprite>("HintImages/Shark") },
            { "Cat", Resources.Load<Sprite>("HintImages/Cat") },
            { "Cup", Resources.Load<Sprite>("HintImages/Cup") },
            { "Nap", Resources.Load<Sprite>("HintImages/Nap") },
            { "Hello", Resources.Load<Sprite>("HintImages/Hello") },
            { "Yellow", Resources.Load<Sprite>("HintImages/Yellow") },
            { "Lava", Resources.Load<Sprite>("HintImages/Lava") },
            { "Jump", Resources.Load<Sprite>("HintImages/Jump") },
            { "Run", Resources.Load<Sprite>("HintImages/Run") }
        };
    }

    void PlayWordAudio()
    {
        if (currentWordIndex < wordAudioClips.Length)
        {
            audioSource.clip = wordAudioClips[currentWordIndex];
            audioSource.Play();
            Debug.Log($"Playing audio for word: {wordAudioClips[currentWordIndex].name}");
        }
        else
        {
            Debug.LogWarning("No more words to play.");
        }
    }

    public void CheckWord()
    {
        string userInput = wordInputField.text.Trim().ToLower();
        string correctWord = wordAudioClips[currentWordIndex].name.ToLower();

        Debug.Log($"User Input: '{userInput}' | Correct Word: '{correctWord}'");

        if (userInput == correctWord)
        {
            feedbackText.text = "Correct!";
            correctAnswers++;
            attempts++;
            audioSource.PlayOneShot(okSound);

            WordAttempt newAttempt = new WordAttempt
            {
                word = correctWord,
                attempts = attempts,
                correct = true,
                skipped = false
            };

            SessionData2.wordAttempts.Add(newAttempt);
            Debug.Log($"Added word attempt: {correctWord}, attempts: {attempts}, correct: true");
            SaveProgressToFile();
            attempts = 0;
            hintImage.gameObject.SetActive(false);
            skipButton.gameObject.SetActive(false);

            Debug.Log("Invoking NextWord method in 1.5 seconds.");
            Invoke("NextWord", 1.5f);
        }
        else
        {
            attempts++;
            feedbackText.text = "Let's Try again.";
            audioSource.PlayOneShot(wrongSound);

            if (attempts == 1)
            {
                skipButton.gameObject.SetActive(true);
            }

            if (attempts == 2)
            {
                ShowHintImage();
            }
        }
    }

    void ShowHintImage()
    {
        string currentWord = wordAudioClips[currentWordIndex].name;

        if (wordImageMap.ContainsKey(currentWord))
        {
            hintImage.sprite = wordImageMap[currentWord];
            hintImage.gameObject.SetActive(true);
            Debug.Log("Hint Image Displayed for: " + currentWord);
        }
        else
        {
            Debug.LogWarning("No Hint Image found for: " + currentWord);
        }
    }

    void SkipWord()
    {
        feedbackText.text = "Skipped!";

        skippedWords++;

        audioSource.PlayOneShot(skipedSound);

        WordAttempt newAttempt = new WordAttempt
        {
            word = wordAudioClips[currentWordIndex].name,
            attempts = attempts,
            correct = false,
            skipped = true
        };

        SessionData2.wordAttempts.Add(newAttempt);

        Debug.Log($"Added skipped word: {wordAudioClips[currentWordIndex].name}, attempts: {attempts}");

        SaveProgressToFile();

        attempts = 0;
        hintImage.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);

        Debug.Log("Invoking NextWord method in 1.3 seconds.");
        Invoke("NextWord", 1.3f);
    }

    void NextWord()
    {
        currentWordIndex++;
        Debug.Log($"Moving to next word. Current word index: {currentWordIndex}");

        if (currentWordIndex < wordAudioClips.Length)
        {
            wordInputField.text = "";
            feedbackText.text = "";
            attempts = 0;
            hintImage.gameObject.SetActive(false);
            skipButton.gameObject.SetActive(false);
            PlayWordAudio();
        }
        else
        {
            CalculateFinalScore();

            feedbackText.text = "Task Complete!";
            checkButton.interactable = false;
            skipButton.interactable = false;
            audioSource.PlayOneShot(victoryEndSound);
            SaveProgressToFile();
        }
    }

    void CalculateFinalScore()
    {
        int totalWords = wordAudioClips.Length;
        float score = ((float)correctAnswers / totalWords) * 100;

        ShowStars(score);
    }

    void ShowStars(float score)
    {
        resultPanel.SetActive(true);
        finalScoreText.text = "Final Score: " + score.ToString("F0") + "%";

        replayButton.gameObject.SetActive(true);
        replayButton.interactable = true;

        foreach (Image star in starImages)
        {
            star.gameObject.SetActive(false);
        }

        if (score >= 90)
        {
            for (int i = 0; i < 3; i++) starImages[i].gameObject.SetActive(true);
        }
        else if (score >= 70)
        {
            for (int i = 0; i < 2; i++) starImages[i].gameObject.SetActive(true);
        }
        else if (score >= 50)
        {
            starImages[0].gameObject.SetActive(true);
        }
    }

    void ReplayGame()
    {
        currentWordIndex = 0;
        correctAnswers = 0;
        skippedWords = 0;
        attempts = 0;

        wordInputField.text = "";
        feedbackText.text = "";

        checkButton.interactable = true;
        skipButton.interactable = true;
        wordInputField.gameObject.SetActive(true);
        hintImage.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);

        resultPanel.SetActive(false);
        replayButton.gameObject.SetActive(false);

        SessionData2 = new SessionData2();
        SessionData2.wordAttempts = new List<WordAttempt>();

        PlayWordAudio();
    }

    void SaveProgressToFile()
    {
        string directory = Application.persistentDataPath;
        string fullPath = Path.Combine(directory, saveFileName);
        Debug.Log($"Target save path: {fullPath}");

        if (SessionData2 == null)
        {
            Debug.LogError("Session data is null! Creating new instance.");
            SessionData2 = new SessionData2();
        }

        if (SessionData2.wordAttempts == null)
        {
            Debug.LogError("Word attempts list is null! Creating new list.");
            SessionData2.wordAttempts = new List<WordAttempt>();
        }

        Debug.Log($"Attempting to save data. Current session has {SessionData2.wordAttempts.Count} attempts");

        foreach (var attempt in SessionData2.wordAttempts)
        {
            Debug.Log($"Current session word: {attempt.word}, Attempts: {attempt.attempts}, Correct: {attempt.correct}, Skipped: {attempt.skipped}");
        }

        SessionData2 existingData = new SessionData2();

        if (File.Exists(fullPath))
        {
            try
            {
                string jsonContent = File.ReadAllText(fullPath);
                Debug.Log($"Found existing file with content length: {jsonContent.Length}");

                if (!string.IsNullOrEmpty(jsonContent))
                {
                    existingData = JsonUtility.FromJson<SessionData2>(jsonContent);

                    if (existingData.wordAttempts == null)
                    {
                        existingData.wordAttempts = new List<WordAttempt>();
                    }

                    Debug.Log($"Successfully loaded existing data with {existingData.wordAttempts.Count} attempts");
                }
                else
                {
                    Debug.LogWarning("Existing file was empty. Using new SessionData2 instead.");
                    existingData.wordAttempts = new List<WordAttempt>();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error loading existing data: {ex.Message}. Starting with fresh data.");
                existingData = new SessionData2();
                existingData.wordAttempts = new List<WordAttempt>();
            }
        }
        else
        {
            Debug.Log("No existing file found. Creating new file.");
            existingData.wordAttempts = new List<WordAttempt>();
        }

        if (existingData.wordAttempts == null)
        {
            existingData.wordAttempts = new List<WordAttempt>();
        }

        if (SessionData2.wordAttempts == null)
        {
            SessionData2.wordAttempts = new List<WordAttempt>();
        }

        List<WordAttempt> currentSessionCopy = new List<WordAttempt>();
        foreach (var attempt in SessionData2.wordAttempts)
        {
            WordAttempt copy = new WordAttempt
            {
                word = attempt.word,
                attempts = attempt.attempts,
                correct = attempt.correct,
                skipped = attempt.skipped
            };
            currentSessionCopy.Add(copy);
        }

        existingData.wordAttempts.AddRange(currentSessionCopy);

        Debug.Log($"Combined data now has {existingData.wordAttempts.Count} total attempts");

        try
        {
            string json = JsonUtility.ToJson(existingData, true);

            Debug.Log($"Generated JSON length: {json.Length}");
            if (json.Length > 100)
            {
                Debug.Log($"First 100 chars of JSON: {json.Substring(0, 100)}...");
            }
            else
            {
                Debug.Log($"Full JSON: {json}");
            }

            if (string.IsNullOrEmpty(json) || json == "{}")
            {
                Debug.LogError("ERROR: Generated JSON is empty - serialization failed!");
                Debug.LogError($"existingData is null? {existingData == null}");
                Debug.LogError($"existingData.wordAttempts is null? {existingData.wordAttempts == null}");
                return;
            }

            Directory.CreateDirectory(directory);

            File.WriteAllText(fullPath, json);

            Debug.Log($"Successfully saved data to: {fullPath}");

            if (File.Exists(fullPath))
            {
                string fileContent = File.ReadAllText(fullPath);
                Debug.Log($"Verified file content length: {fileContent.Length}");

#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
            }

            SessionData2 = new SessionData2();
            SessionData2.wordAttempts = new List<WordAttempt>();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving data: {ex.Message}\nStack trace: {ex.StackTrace}");
        }

        // Uncomment the following line if you have FirebaseProgressManager implemented
        // FirebaseProgressManager.Instance.UploadProgressToFirebase();
    }

    void LoadPreviousData()
    {
        string directory = Application.persistentDataPath;
        string fullPath = Path.Combine(directory, saveFileName);
        if (File.Exists(fullPath))
        {
            try
            {
                string jsonData = File.ReadAllText(fullPath);
                if (!string.IsNullOrEmpty(jsonData))
                {
                    SessionData2 loadedData = JsonUtility.FromJson<SessionData2>(jsonData);
                    Debug.Log($"Loaded previous data with {loadedData.wordAttempts.Count} word attempts");

                    SessionData2 = new SessionData2();
                    SessionData2.wordAttempts = new List<WordAttempt>();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading previous data: {ex.Message}");
                SessionData2 = new SessionData2();
                SessionData2.wordAttempts = new List<WordAttempt>();
            }
        }
        else
        {
            Debug.Log($"No previous save file found at {fullPath}");
            SessionData2 = new SessionData2();
            SessionData2.wordAttempts = new List<WordAttempt>();
        }
    }
}
