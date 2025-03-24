using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.SceneManagement;

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
    
    // Directly reference audio files in the inspector for easier troubleshooting
    public AudioClip[] letterClips;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    
    // Add a field to map letters to clips
    [System.Serializable]
    public class LetterClipMapping
    {
        public string letter;
        public AudioClip clip;
    }
    
    public LetterClipMapping[] letterMappings;

    // Progress tracking
    private Dictionary<string, PlayerLetterProgress> letterProgress = new Dictionary<string, PlayerLetterProgress>();
    private Dictionary<string, AudioClip> letterSounds;
    private string correctLetter;
    
    [Header("Progress Tracking")]
    public GameObject progressPanel;
    public Transform progressContentParent;
    public GameObject letterProgressPrefab;
    public Button showProgressButton;
    
    // Session tracking
    private DateTime sessionStartTime;
    private int totalTriesThisSession = 0;
    private int correctTriesThisSession = 0;

    [System.Serializable]
    public class PlayerLetterProgress
    {
        public string letter;
        public int attempts;
        public int correctAnswers;
        public float averageResponseTime;
        public DateTime lastPracticed;
        
        public float SuccessRate => attempts > 0 ? (float)correctAnswers / attempts * 100 : 0;
        
        public PlayerLetterProgress(string letter)
        {
            this.letter = letter;
            attempts = 0;
            correctAnswers = 0;
            averageResponseTime = 0;
            lastPracticed = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class PlayerProgressData
    {
        public PlayerLetterProgress[] letters;
    }
    
    [System.Serializable]
    public class SessionData
    {
        public string date;
        public double duration;
        public int totalTries;
        public int correctTries;
        public float accuracy;
    }
    
    [System.Serializable]
    public class SessionHistory
    {
        public List<SessionData> sessions = new List<SessionData>();
    }

    public Button replayButton;
    public Button exitButton;
    public Button nextGameButton;

    void Awake()
    {
        LogToFile("Awake called - initializing audio system");
        
        // Ensure audio is properly configured for Android
        AudioConfiguration config = AudioSettings.GetConfiguration();
        config.dspBufferSize = 256; // Smaller buffer for Android
        config.sampleRate = 44100; // Standard sample rate
        AudioSettings.Reset(config);
    }

    void Start()
    {
        LogToFile("Starting game initialization...");
        
        // Ensure UI elements are assigned
        ValidateUIElements();

        pointsDisplay.text = "Points: 0";
        gameUI.SetActive(true);
        InitializeLetterSounds();
        LoadPlayerProgress();
        
        SetNewLetter();
        feedbackPanel.SetActive(false);
        victoryPanel.SetActive(false);
        
        if (progressPanel != null)
            progressPanel.SetActive(false);

        if (playSoundButton != null)
        {
            playSoundButton.onClick.RemoveAllListeners();
            playSoundButton.onClick.AddListener(PlayLetterSound);
            LogToFile("Added listener to play sound button");
        }
        
        if (showProgressButton != null)
        {
            showProgressButton.onClick.RemoveAllListeners();
            showProgressButton.onClick.AddListener(ToggleProgressPanel);
        }
        
        // Start tracking session time
        sessionStartTime = DateTime.Now;

        if (replayButton != null)
        {
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(ReplayGame);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(ExitGame);
        }

        if (nextGameButton != null)
        {
            nextGameButton.onClick.RemoveAllListeners();
            nextGameButton.onClick.AddListener(PlayNextGame);
        }
    }

    void ValidateUIElements()
    {
        if (victoryPanel == null) LogToFile("ERROR: victoryPanel is not assigned!");
        if (gameUI == null) LogToFile("ERROR: gameUI is not assigned!");
        if (letterDisplay == null) LogToFile("ERROR: letterDisplay is not assigned!");
        if (pointsDisplay == null) LogToFile("ERROR: pointsDisplay is not assigned!");
        if (audioSource == null) LogToFile("ERROR: audioSource is not assigned!");
        
        // Log audio source settings
        if (audioSource != null)
        {
            LogToFile($"AudioSource settings - volume: {audioSource.volume}, pitch: {audioSource.pitch}, mute: {audioSource.mute}");
        }
    }

    void InitializeLetterSounds()
    {
        letterSounds = new Dictionary<string, AudioClip>();
        
        // First try to use the letterMappings from the inspector (most reliable method)
        if (letterMappings != null && letterMappings.Length > 0)
        {
            LogToFile($"Loading {letterMappings.Length} letter mappings from inspector");
            foreach (var mapping in letterMappings)
            {
                if (mapping.clip != null)
                {
                    letterSounds[mapping.letter] = mapping.clip;
                    LogToFile($"Added letter mapping: {mapping.letter} -> {mapping.clip.name}");
                    
                    // Initialize progress tracking for this letter
                    if (!letterProgress.ContainsKey(mapping.letter))
                    {
                        letterProgress[mapping.letter] = new PlayerLetterProgress(mapping.letter);
                    }
                }
            }
        }
        // Fall back to letterClips array with automatic naming
        else if (letterClips != null && letterClips.Length > 0)
        {
            LogToFile($"Loading {letterClips.Length} letter clips from inspector array");
            string[] letterOptions = new string[] { "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", 
                                                   "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            
            // Assign clips based on their names
            foreach (AudioClip clip in letterClips)
            {
                if (clip != null)
                {
                    string clipName = clip.name.ToUpper();
                    foreach (string letter in letterOptions)
                    {
                        if (clipName.Contains(letter))
                        {
                            letterSounds[letter] = clip;
                            LogToFile($"Added letter from clip name: {letter} -> {clip.name}");
                            
                            // Initialize progress tracking for this letter
                            if (!letterProgress.ContainsKey(letter))
                            {
                                letterProgress[letter] = new PlayerLetterProgress(letter);
                            }
                            break;
                        }
                    }
                }
            }
        }
        
        // Ensure all audio clips are properly decompressed for Android
        foreach (var entry in letterSounds)
        {
            AudioClip clip = entry.Value;
            if (!clip.LoadAudioData())
            {
                LogToFile($"Failed to load audio data for clip: {clip.name}");
            }
            LogToFile($"Letter: {entry.Key}, Clip: {clip.name}, Length: {clip.length}s, Loaded: {clip.loadState}");
        }
    }

    public void PlayLetterSound()
    {
        LogToFile($"BUTTON PRESSED: PlayLetterSound for letter: {correctLetter}");
        
        if (audioSource == null)
        {
            LogToFile("ERROR: AudioSource is not assigned!");
            return;
        }

        if (letterSounds.ContainsKey(correctLetter))
        {
            AudioClip clip = letterSounds[correctLetter];
            LogToFile($"Playing clip: {clip.name}, Duration: {clip.length}s");
            
            if (clip != null)
            {
                // Use the main audioSource directly - more reliable on Android
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.Play();
                LogToFile($"Playing sound using main AudioSource for letter: {correctLetter}");
            }
        }
        else
        {
            LogToFile($"ERROR: Letter key missing in dictionary: {correctLetter}");
        }
    }

    void CheckAnswer(string selectedLetter)
    {
        LogToFile($"Checking answer: Selected={selectedLetter}, Correct={correctLetter}");
        bool isCorrect = selectedLetter == correctLetter;
        
        // Update progress tracking
        totalTriesThisSession++;
        if (isCorrect) correctTriesThisSession++;
        
        if (letterProgress.ContainsKey(correctLetter))
        {
            PlayerLetterProgress progress = letterProgress[correctLetter];
            progress.attempts++;
            if (isCorrect)
            {
                progress.correctAnswers++;
            }
            progress.lastPracticed = DateTime.Now;
            letterProgress[correctLetter] = progress;
        }
        
        ShowFeedback(isCorrect);

        if (isCorrect)
        {
            pointsDisplay.text = "Points: " + (++pointCount);
            PlayFeedbackSound(true);
            LogToFile($"Correct answer! Points: {pointCount}");

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
            PlayFeedbackSound(false);
            LogToFile("Incorrect answer!");
        }
        
        // Save progress after each answer
        SavePlayerProgress();
    }

    void PlayFeedbackSound(bool isCorrect)
    {
        LogToFile($"Playing feedback sound: {(isCorrect ? "correct" : "wrong")}");
        
        AudioClip clip = isCorrect ? correctSound : wrongSound;
        
        if (clip != null && audioSource != null)
        {
            // Use the main audioSource for feedback - more reliable on Android
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
            LogToFile($"Playing feedback sound using main AudioSource");
        }
        else
        {
            LogToFile($"ERROR: Feedback sound not found");
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
            int rand = UnityEngine.Random.Range(0, i + 1);
            string temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    string GetRandomLetter()
    {
        List<string> keys = new List<string>(letterSounds.Keys);
        return keys[UnityEngine.Random.Range(0, keys.Count)];
    }

    void SetNewLetter()
    {
        if (letterSounds.Count == 0)
        {
            LogToFile("ERROR: No letter sounds loaded, cannot set a new letter");
            return;
        }

        correctLetter = GetRandomLetter();
        letterDisplay.text = correctLetter;
        LogToFile($"Setting new letter: {correctLetter}");

        // Create list of incorrect options
        List<string> options = new List<string>(letterSounds.Keys);
        options.Remove(correctLetter);
        ShuffleList(options);

        // Create options list (correct answer + incorrect answers)
        List<string> allOptions = new List<string> { correctLetter };
        allOptions.AddRange(options.GetRange(0, Mathf.Min(answerButtons.Length - 1, options.Count)));
        ShuffleList(allOptions);

        // Assign options to buttons
        for (int i = 0; i < answerButtons.Length && i < allOptions.Count; i++)
        {
            string letter = allOptions[i];
            TextMeshProUGUI btnText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();

            if (btnText != null)
            {
                btnText.text = letter;
            }

            int buttonIndex = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => CheckAnswer(allOptions[buttonIndex]));
        }
    }

    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    void FinishLevel()
    {
        LogToFile("Level Completed!");
        victoryPanel.SetActive(true);
        gameUI.SetActive(false);
        
        // Show stars based on progress
        ShowStarsBasedOnProgress();

        // Save session data
        SaveSessionSummary();
    }

    void ShowStarsBasedOnProgress()
    {
        float accuracy = totalTriesThisSession > 0 ? (float)correctTriesThisSession / totalTriesThisSession * 100 : 0;

        // Hide all stars initially
        if (star1 != null) star1.SetActive(false);
        if (star2 != null) star2.SetActive(false);
        if (star3 != null) star3.SetActive(false);

        // Show stars based on accuracy
        if (accuracy >= 85)
        {
            // if (star1 != null) star1.SetActive(true);
            // if (star2 != null) star2.SetActive(true);
            if (star3 != null) star3.SetActive(true);
        }
        else if (accuracy >= 40)
        {
            // if (star1 != null) star1.SetActive(true);
            if (star2 != null) star2.SetActive(true);
        }
        else if (accuracy >= 0)
        {
            if (star1 != null) star1.SetActive(true);
        }

        LogToFile($"Stars displayed based on accuracy: {accuracy}%");
    }

    #region Progress Tracking Methods
    
    void LoadPlayerProgress()
    {
        try
        {
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, "Task2UserProgress.json");
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                PlayerProgressData data = JsonUtility.FromJson<PlayerProgressData>(json);
                
                if (data != null && data.letters != null)
                {
                    foreach (var progress in data.letters)
                    {
                        letterProgress[progress.letter] = progress;
                    }
                    LogToFile($"Loaded progress data for {data.letters.Length} letters");
                }
            }
            else
            {
                LogToFile("No existing progress file found. Starting fresh.");
            }
        }
        catch (Exception e)
        {
            LogToFile($"Error loading progress: {e.Message}");
        }
    }
    
    void SavePlayerProgress()
    {
        try
        {
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, "Task2UserProgress.json");
            PlayerProgressData data = new PlayerProgressData();
            data.letters = new PlayerLetterProgress[letterProgress.Count];
            
            int index = 0;
            foreach (var entry in letterProgress)
            {
                data.letters[index] = entry.Value;
                index++;
            }
            
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(filePath, json);
            LogToFile("Progress data saved successfully");
        }
        catch (Exception e)
        {
            LogToFile($"Error saving progress: {e.Message}");
        }
    }

void SaveSessionSummary()
{
    try
    {
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, "Task2SessionHistory.json");
        SessionData newSession = new SessionData
        {
            date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            duration = (DateTime.Now - sessionStartTime).TotalMinutes,
            totalTries = totalTriesThisSession,
            correctTries = correctTriesThisSession,
            accuracy = totalTriesThisSession > 0 ? (float)correctTriesThisSession / totalTriesThisSession * 100 : 0
        };

        SessionHistory history;
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            history = JsonUtility.FromJson<SessionHistory>(json);
        }
        else
        {
            history = new SessionHistory();
        }

        if (history.sessions == null)
            history.sessions = new List<SessionData>();

        history.sessions.Add(newSession);

        string updatedJson = JsonUtility.ToJson(history, true);
        File.WriteAllText(filePath, updatedJson);
        LogToFile("Session history saved successfully");
    }
    catch (Exception e)
    {
        LogToFile($"Error saving session history: {e.Message}");
    }
}

public void ToggleProgressPanel()
{
    if (progressPanel == null)
        return;
        
    bool isShowing = progressPanel.activeSelf;
    progressPanel.SetActive(!isShowing);
    
    if (!isShowing)
    {
        UpdateProgressPanel();
    }
}

void UpdateProgressPanel()
{
    if (progressContentParent == null || letterProgressPrefab == null)
    {
        LogToFile("ERROR: Progress panel prerequisites missing");
        return;
    }
    
    // Clear existing content
    foreach (Transform child in progressContentParent)
    {
        Destroy(child.gameObject);
    }
    
    // Create summary stats
    int totalAttempts = 0;
    int totalCorrect = 0;
    
    // Sort letters by accuracy (descending)
    List<KeyValuePair<string, PlayerLetterProgress>> sortedProgress = new List<KeyValuePair<string, PlayerLetterProgress>>();
    foreach (var entry in letterProgress)
    {
        sortedProgress.Add(new KeyValuePair<string, PlayerLetterProgress>(entry.Key, entry.Value));
        totalAttempts += entry.Value.attempts;
        totalCorrect += entry.Value.correctAnswers;
    }
    
    // Sort by success rate (highest first)
    sortedProgress.Sort((a, b) => b.Value.SuccessRate.CompareTo(a.Value.SuccessRate));
    
    // Create summary item
    GameObject summaryObj = Instantiate(letterProgressPrefab, progressContentParent);
    TextMeshProUGUI[] summaryTexts = summaryObj.GetComponentsInChildren<TextMeshProUGUI>();
    if (summaryTexts.Length >= 3)
    {
        summaryTexts[0].text = "SUMMARY";
        summaryTexts[1].text = $"Total: {totalAttempts} attempts";
        float overallAccuracy = totalAttempts > 0 ? (float)totalCorrect / totalAttempts * 100 : 0;
        summaryTexts[2].text = $"Overall Accuracy: {overallAccuracy:F1}%";
    }
    
    // Add divider
    GameObject divider = Instantiate(letterProgressPrefab, progressContentParent);
    TextMeshProUGUI[] dividerTexts = divider.GetComponentsInChildren<TextMeshProUGUI>();
    if (dividerTexts.Length >= 3)
    {
        dividerTexts[0].text = "LETTER";
        dividerTexts[1].text = "ATTEMPTS";
        dividerTexts[2].text = "ACCURACY";
    }
    
    // Create item for each letter
    foreach (var pair in sortedProgress)
    {
        if (pair.Value.attempts > 0) // Only show letters that have been practiced
        {
            GameObject progressObj = Instantiate(letterProgressPrefab, progressContentParent);
            TextMeshProUGUI[] texts = progressObj.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 3)
            {
                texts[0].text = pair.Key;
                texts[1].text = $"{pair.Value.attempts}";
                texts[2].text = $"{pair.Value.SuccessRate:F1}%";
                
                // Color code based on performance
                if (pair.Value.SuccessRate >= 80)
                    texts[2].color = new Color(0, 0.7f, 0);
                else if (pair.Value.SuccessRate >= 50)
                    texts[2].color = new Color(0.7f, 0.7f, 0);
                else
                    texts[2].color = new Color(0.7f, 0, 0);
            }
        }
    }
    
    // Display session history
    try
    {
        // Update file path to use GameData directory
        string filePath = System.IO.Path.Combine(Application.dataPath, "GameData/SessionHistory.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            SessionHistory history = JsonUtility.FromJson<SessionHistory>(json);
            
            if (history != null && history.sessions != null && history.sessions.Count > 0)
            {
                // Add section header
                GameObject headerObj = Instantiate(letterProgressPrefab, progressContentParent);
                TextMeshProUGUI[] headerTexts = headerObj.GetComponentsInChildren<TextMeshProUGUI>();
                if (headerTexts.Length >= 3)
                {
                    headerTexts[0].text = "RECENT SESSIONS";
                    headerTexts[1].text = "DATE";
                    headerTexts[2].text = "ACCURACY";
                }
                
                // Show last 5 sessions (most recent first)
                int displayCount = Mathf.Min(5, history.sessions.Count);
                for (int i = history.sessions.Count - 1; i >= history.sessions.Count - displayCount; i--)
                {
                    SessionData session = history.sessions[i];
                    GameObject sessionObj = Instantiate(letterProgressPrefab, progressContentParent);
                    TextMeshProUGUI[] sessionTexts = sessionObj.GetComponentsInChildren<TextMeshProUGUI>();
                    if (sessionTexts.Length >= 3)
                    {
                        sessionTexts[0].text = $"Session {i+1}";
                        sessionTexts[1].text = session.date.Split(' ')[0]; // Just the date part
                        sessionTexts[2].text = $"{session.accuracy:F1}%";
                    }
                }
            }
        }
    }
    catch (Exception e)
    {
        LogToFile($"Error displaying session history: {e.Message}");
    }
}

void LogToFile(string message)
{
    Debug.Log(message);
    
    #if UNITY_ANDROID && !UNITY_EDITOR
    try
    {
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, "LetterGameLog.txt");
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine($"{System.DateTime.Now}: {message}");
        }
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Failed to write to log file: {e.Message}");
    }
    #endif
}

void ReplayGame()
{
    LogToFile("Replay button clicked - restarting the game");
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}

void ExitGame()
{
    Debug.Log("Yes button pressed, going to TaskHolder...");
    Time.timeScale = 1f; // Reset game speed
    SceneManager.LoadScene("TaskHolder");
}

void PlayNextGame()
{
    LogToFile("Next Game button clicked - loading next scene");
    SceneManager.LoadScene("Task3"); // Replace "NextGameScene" with the actual scene name
}

#endregion
}