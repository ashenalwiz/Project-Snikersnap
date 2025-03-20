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
public class SessionData
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
    public AudioClip[] wordAudioClips; // Array to hold 5 audio clips
    private string[] words = { "Rock", "Cave", "Flag", "Dark", "Shark", "Cat", "Cup", "Nap", "Hello", "Yellow", "Lava", "Jump", "Run" }; // List of words
    private int currentWordIndex = 0; // Track current word
    public UnityEngine.UI.Image hintImage;
    public Button skipButton;
    private Dictionary<string, Sprite> wordImageMap;
    private int attempts = 0;

    [SerializeField] private string customSavePath = "Assets/GameData/";
    [SerializeField] private string saveFileName = "UserProgress.json";

    //----------------New Updates--------------------------
    private SessionData sessionData = new SessionData();
    private string jsonFilePath;

    public GameObject resultPanel;  // UI panel to show stars
    public List<Image> starImages = new List<Image>(); // Array of star images to display results
    public TMP_Text finalScoreText;

    private int correctAnswers = 0;
    private int skippedWords = 0;
    public Button replayButton;
    //-----------------------------------------------------

    void Start()
    {
        // Initialize session data to avoid null references
        if (sessionData == null)
        {
            sessionData = new SessionData();
        }
        
        if (sessionData.wordAttempts == null)
        {
            sessionData.wordAttempts = new List<WordAttempt>();
        }

        jsonFilePath = Path.Combine(Application.dataPath, "GameData", saveFileName);

        LoadPreviousData();

        // Find star images if not assigned in inspector
        if (starImages.Count == 0)
        {
            starImages = new List<Image>
            {
                GameObject.Find("Star1").GetComponent<Image>(),
                GameObject.Find("Star2").GetComponent<Image>(),
                GameObject.Find("Star3").GetComponent<Image>()
            };
        }

        //--------Update---------------------------
        resultPanel.SetActive(false);
        replayButton.gameObject.SetActive(false); // Hide replay button at start
        replayButton.onClick.AddListener(ReplayGame);
        //-----------------------------------------

        playAudioButton.onClick.AddListener(PlayWordAudio);
        checkButton.onClick.AddListener(CheckWord);
        skipButton.onClick.AddListener(SkipWord);

        hintImage.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);

        // Initialize word-image dictionary
        InitializeWordImageDictionary();

        feedbackText.text = "";
        Debug.Log("User Progress File Path: " + jsonFilePath);
    }
    
    // Separate initialization method for word images
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
    //========================================================================

    void PlayWordAudio()
    {
        if (currentWordIndex < wordAudioClips.Length)
        {
            audioSource.clip = wordAudioClips[currentWordIndex];
            audioSource.Play();
        }
    }

    public void CheckWord()
    {
        string userInput = wordInputField.text.Trim().ToLower(); // Sanitize input
        string correctWord = wordAudioClips[currentWordIndex].name.ToLower(); // Get current correct word

        Debug.Log($"User Input: '{userInput}' | Correct Word: '{correctWord}'");

        if (userInput == correctWord)
        {
            feedbackText.text = "Correct!";
            correctAnswers++;
            attempts++;

            WordAttempt newAttempt = new WordAttempt
            {
                word = correctWord,
                attempts = attempts,
                correct = true,
                skipped = false
            };

            sessionData.wordAttempts.Add(newAttempt);
            Debug.Log($"Added word attempt: {correctWord}, attempts: {attempts}, correct: true");
            SaveProgressToFile();
            attempts = 0;
            hintImage.gameObject.SetActive(false); // Hide image if it was displayed
            skipButton.gameObject.SetActive(false);
            //-----------------------------------------

            Invoke("NextWord", 1.5f); // Wait 1.5 seconds before moving to next word
        }
        else
        {
            attempts++;
            feedbackText.text = "Let's Try again.";

            //-------Update----------------------------------------------------
            if (attempts == 1)
            {
                skipButton.gameObject.SetActive(true); // Show skip button after first mistake
            }

            if (attempts == 2)
            {
                ShowHintImage();
            }
            //------------------------------------------------------------------
        }
    }
    //------------Update----------------------------------------------------------------

    void ShowHintImage()
    {
        string currentWord = wordAudioClips[currentWordIndex].name;

        if (wordImageMap.ContainsKey(currentWord))
        {
            hintImage.sprite = wordImageMap[currentWord]; // Set correct image
            hintImage.gameObject.SetActive(true); // Show image
            Debug.Log("Hint Image Displayed for: " + currentWord);
        }
        else
        {
            Debug.LogWarning("No Hint Image found for: " + currentWord);
        }
    }

    //-------------Update-SkipButton---------------------------------------------------------------------

    void SkipWord()
    {
        feedbackText.text = "Skipped!";

        skippedWords++;

        WordAttempt newAttempt = new WordAttempt
        {
            word = wordAudioClips[currentWordIndex].name,
            attempts = attempts,
            correct = false,
            skipped = true
        };

        sessionData.wordAttempts.Add(newAttempt);

        Debug.Log($"Added skipped word: {wordAudioClips[currentWordIndex].name}, attempts: {attempts}");

        SaveProgressToFile();

        attempts = 0;
        hintImage.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        Invoke("NextWord", 1.3f);
    }


    //----------------------------------------------------------------------------------
    void NextWord()
    {
        currentWordIndex++; // Move to next word

        if (currentWordIndex < wordAudioClips.Length) // Check if words are remaining
        {
            wordInputField.text = ""; // Clear input field
            feedbackText.text = "";   // Clear feedback text
            attempts = 0;
            hintImage.gameObject.SetActive(false);
            skipButton.gameObject.SetActive(false);
            PlayWordAudio(); // Play next word audio automatically
        }
        else
        {
            CalculateFinalScore();

            feedbackText.text = "Task Complete!"; // Show message when all words are completed
            checkButton.interactable = false; // Disable check button
            skipButton.interactable = false;
            SaveProgressToFile();
        }
    }

    //------------------------New Features : Score & Stars-------------------------------------------------------------------------
    void ShowResults()
    {
        // --- Calculate Score ---
        int totalWords = wordAudioClips.Length; // Using wordAudioClips instead of words for accuracy
        float score = ((float)correctAnswers / totalWords) * 100f;

        // --- Determine Stars ---
        int starsEarned = 0;
        if (score >= 90) starsEarned = 3;
        else if (score >= 70) starsEarned = 2;
        else if (score >= 50) starsEarned = 1;
        else starsEarned = 0;

        // --- Show Result Panel ---
        resultPanel.SetActive(true);
        Debug.Log("Final Score: " + score + "% | Stars Earned: " + starsEarned);
    }

    void CalculateFinalScore()
    {
        int totalWords = wordAudioClips.Length; // Changed from words.Length to wordAudioClips.Length
        float score = ((float)correctAnswers / totalWords) * 100; // Score formula

        ShowStars(score);
    }
    
    void ShowStars(float score)
    {
        resultPanel.SetActive(true); // Show result UI
        finalScoreText.text = "Final Score: " + score.ToString("F0") + "%"; // Show final percentage

        replayButton.gameObject.SetActive(true);
        replayButton.interactable = true;

        // Hide all stars initially
        foreach (Image star in starImages)
        {
            star.gameObject.SetActive(false);
        }

        // Determine star rating based on score
        if (score >= 90) // 3 stars
        {
            for (int i = 0; i < 3; i++) starImages[i].gameObject.SetActive(true);
        }
        else if (score >= 70) // 2 stars
        {
            for (int i = 0; i < 2; i++) starImages[i].gameObject.SetActive(true);
        }
        else if (score >= 50) // 1 star
        {
            starImages[0].gameObject.SetActive(true);
        }
        // If below 50%, no stars are shown
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
        replayButton.gameObject.SetActive(false); // Hide replay button

        // Create a new session data object to avoid carrying over references
        sessionData = new SessionData();
        sessionData.wordAttempts = new List<WordAttempt>();

        PlayWordAudio(); // Start first word again
    }

    void SaveProgressToFile()
    {
        // Check if sessionData is properly initialized
        if (sessionData == null)
        {
            Debug.LogError("Session data is null! Creating new instance.");
            sessionData = new SessionData();
        }

        if (sessionData.wordAttempts == null)
        {
            Debug.LogError("Word attempts list is null! Creating new list.");
            sessionData.wordAttempts = new List<WordAttempt>();
        }

        Debug.Log($"Attempting to save data. Current session has {sessionData.wordAttempts.Count} attempts");
        
        // Log current attempts for debugging
        foreach (var attempt in sessionData.wordAttempts)
        {
            Debug.Log($"Current session word: {attempt.word}, Attempts: {attempt.attempts}, Correct: {attempt.correct}, Skipped: {attempt.skipped}");
        }

        // Make sure the directory exists
        string directory = Path.Combine(Application.dataPath, "GameData");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            Debug.Log($"Created directory: {directory}");
        }

        string fullPath = Path.Combine(directory, saveFileName);
        Debug.Log($"Target save path: {fullPath}");

        // Handle existing data
        SessionData existingData = new SessionData();
        
        if (File.Exists(fullPath))
        {
            try
            {
                string jsonContent = File.ReadAllText(fullPath);
                Debug.Log($"Found existing file with content length: {jsonContent.Length}");

                if (!string.IsNullOrEmpty(jsonContent))
                {
                    existingData = JsonUtility.FromJson<SessionData>(jsonContent);
                    
                    // Safety check for null lists
                    if (existingData.wordAttempts == null)
                    {
                        existingData.wordAttempts = new List<WordAttempt>();
                    }
                    
                    Debug.Log($"Successfully loaded existing data with {existingData.wordAttempts.Count} attempts");
                }
                else
                {
                    Debug.LogWarning("Existing file was empty. Using new SessionData instead.");
                    existingData.wordAttempts = new List<WordAttempt>();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error loading existing data: {ex.Message}. Starting with fresh data.");
                existingData = new SessionData();
                existingData.wordAttempts = new List<WordAttempt>();
            }
        }
        else
        {
            Debug.Log("No existing file found. Creating new file.");
            existingData.wordAttempts = new List<WordAttempt>();
        }

        // Verify that both lists are initialized
        if (existingData.wordAttempts == null)
        {
            existingData.wordAttempts = new List<WordAttempt>();
        }
        
        if (sessionData.wordAttempts == null)
        {
            sessionData.wordAttempts = new List<WordAttempt>();
        }

        // Make a deep copy of the current session data to avoid reference issues
        List<WordAttempt> currentSessionCopy = new List<WordAttempt>();
        foreach (var attempt in sessionData.wordAttempts)
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

        // Add current session data to existing data
        existingData.wordAttempts.AddRange(currentSessionCopy);

        // Log the combined data
        Debug.Log($"Combined data now has {existingData.wordAttempts.Count} total attempts");

        try
        {
            // Convert to JSON - make sure we're serializing a valid object
            string json = JsonUtility.ToJson(existingData, true); // true for pretty print

            // Verify the JSON content
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
                // Try to log more details about the object being serialized
                Debug.LogError($"existingData is null? {existingData == null}");
                Debug.LogError($"existingData.wordAttempts is null? {existingData.wordAttempts == null}");
                return;
            }

            // Force the directory to exist one more time just to be sure
            Directory.CreateDirectory(directory);

            // Write to file
            File.WriteAllText(fullPath, json);
            Debug.Log($"Successfully saved data to: {fullPath}");

            // Verify the file was written correctly
            if (File.Exists(fullPath))
            {
                string fileContent = File.ReadAllText(fullPath);
                Debug.Log($"Verified file content length: {fileContent.Length}");

                // If we're in the Unity Editor, refresh the asset database
                #if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
                #endif
            }

            // Only clear the session data if the save was successful
            sessionData = new SessionData();
            sessionData.wordAttempts = new List<WordAttempt>();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving data: {ex.Message}\nStack trace: {ex.StackTrace}");
        }
    }

    void LoadPreviousData()
    {
        string directory = Path.Combine(Application.dataPath, "GameData");
        string fullPath = Path.Combine(directory, saveFileName);
        if (File.Exists(fullPath))
        {
            try
            {
                string jsonData = File.ReadAllText(fullPath);
                if (!string.IsNullOrEmpty(jsonData))
                {
                    SessionData loadedData = JsonUtility.FromJson<SessionData>(jsonData);
                    Debug.Log($"Loaded previous data with {loadedData.wordAttempts.Count} word attempts");

                    // Initialize a new session data for this gameplay session
                    sessionData = new SessionData();
                    sessionData.wordAttempts = new List<WordAttempt>();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading previous data: {ex.Message}");
                sessionData = new SessionData();
                sessionData.wordAttempts = new List<WordAttempt>();
            }
        }
        else
        {
            Debug.Log($"No previous save file found at {fullPath}");
            sessionData = new SessionData();
            sessionData.wordAttempts = new List<WordAttempt>();
        }
    }

    public static class FileHandler
    {
        public static string GetFilePath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        public static void SaveToJSON<T>(T data, string fileName)
        {
            string filePath = GetFilePath(fileName);
            string json = JsonUtility.ToJson(data, true);

            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, json);
                Debug.Log($"Data saved successfully to: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving file: {ex.Message}");
            }
        }

        public static T LoadFromJSON<T>(string fileName) where T : new()
        {
            string filePath = GetFilePath(fileName);
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                Debug.Log($"Data loaded from: {filePath}");  // Debugging log to check the file path
                return JsonUtility.FromJson<T>(jsonData);
            }
            else
            {
                Debug.LogWarning($"No data found at {filePath}. Returning new instance.");
                return new T(); // If no file is found, return a new instance
            }
        }
    }
}