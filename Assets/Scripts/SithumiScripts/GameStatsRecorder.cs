using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class RoundData
{
    public int round;
    public string targetLetter;
    public float accuracy;
    public int attempts;
    public bool skipped;
}

[Serializable]
public class SessionData
{
    public string date;
    public List<RoundData> rounds = new List<RoundData>();
}

[Serializable]
public class SithumiProgressData
{
    public List<SessionData> sessions = new List<SessionData>();
}

public class GameStatsRecorder : MonoBehaviour
{
    // References to game components
    private LetterSpawner letterSpawner;
    private BasketController basketController;
    private ProgressManager progressManager;
    private GameManager gameManager;
    private EndGameMenu endGameMenu;

    // Stats tracking variables
    private SessionData currentSession = new SessionData();
    private int currentRound = 1;
    private int correctCatches = 0;
    private int totalCatches = 0;
    private bool isRoundSkipped = false;

    // Store the current round's target letter
    private string currentTargetLetter = "";

    // Add a flag to track if we're in test mode
    public bool saveAfterEachRound = true; // Now true by default

    // File name for all progress
    private const string PROGRESS_FILENAME = "SithumiProgress.json";

    // Store the save file path
    private string saveFilePath = "";

    // Session identifier (to track when a new session starts)
    private string sessionId = "";

    // Track if final round stats have been saved
    private bool finalRoundSaved = false;

    // Add a flag to track if we're in the restart process
    private bool isRestarting = false;

    void Start()
    {
        Debug.Log("GameStatsRecorder: Starting stats recording");

        // Get references to required components
        letterSpawner = FindFirstObjectByType<LetterSpawner>();
        basketController = FindFirstObjectByType<BasketController>();
        progressManager = FindFirstObjectByType<ProgressManager>();
        gameManager = FindFirstObjectByType<GameManager>();
        endGameMenu = FindFirstObjectByType<EndGameMenu>();

        // Check if components were found
        Debug.Log($"GameStatsRecorder: Found components - LetterSpawner: {letterSpawner != null}, " +
            $"BasketController: {basketController != null}, " +
            $"ProgressManager: {progressManager != null}, " +
            $"GameManager: {gameManager != null}, " +
            $"EndGameMenu: {endGameMenu != null}");

        // Initialize session data with current date
        currentSession.date = DateTime.Now.ToString("dd-MM-yyyy");
        // Create a unique session ID based on time
        sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        Debug.Log($"GameStatsRecorder: Initialized with date {currentSession.date}, session ID {sessionId}");

        // Set the save file path using string concatenation
        saveFilePath = Application.persistentDataPath + "/" + PROGRESS_FILENAME;
        Debug.Log($"GameStatsRecorder: Save file path set to {saveFilePath}");

        // Start monitoring the game
        StartCoroutine(MonitorGameProgress());

        // Subscribe to game manager restart event
        if (gameManager != null && endGameMenu != null && endGameMenu.restartButton != null)
        {
            // Add a listener to detect when game restarts
            endGameMenu.restartButton.onClick.AddListener(OnGameRestart);
        }
    }

    private void OnDestroy()
    {
        // Save any unsaved data when the component is destroyed
        if (!finalRoundSaved && currentSession.rounds.Count > 0)
        {
            // Check if the final round is missing
            bool containsRound5 = false;
            foreach (var round in currentSession.rounds)
            {
                if (round.round == 5)
                {
                    containsRound5 = true;
                    break;
                }
            }

            // If the last round is active and not yet saved, save it
            if (!containsRound5 && GetCurrentRoundFromUI() == 5)
            {
                SaveRoundStats(5);
            }

            SaveJsonFile(true);
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        // Save data when the app is paused (e.g. when put in background)
        if (pauseStatus && !finalRoundSaved && currentSession.rounds.Count > 0)
        {
            SaveJsonFile(true);
        }
    }

    private void OnApplicationQuit()
    {
        // Save data when the app is quit
        if (!finalRoundSaved && currentSession.rounds.Count > 0)
        {
            // Check if the final round is missing
            bool containsRound5 = false;
            foreach (var round in currentSession.rounds)
            {
                if (round.round == 5)
                {
                    containsRound5 = true;
                    break;
                }
            }

            // If the last round is active and not yet saved, save it
            if (!containsRound5 && GetCurrentRoundFromUI() == 5)
            {
                SaveRoundStats(5);
            }

            SaveJsonFile(true);
        }
    }

    private void OnGameRestart()
    {
        Debug.Log("GameStatsRecorder: Game restart detected");

        // Set restarting flag
        isRestarting = true;

        // Make sure the final round was saved
        if (!finalRoundSaved && currentSession.rounds.Count > 0)
        {
            // Check if round 5 was already saved
            bool containsRound5 = false;
            foreach (var round in currentSession.rounds)
            {
                if (round.round == 5)
                {
                    containsRound5 = true;
                    break;
                }
            }

            // If not, save round 5 data
            if (!containsRound5)
            {
                SaveRoundStats(5);
                SaveJsonFile(true);
            }
        }

        // Reset for the new session
        ResetSessionData();

        // Start monitoring the new game after a delay to allow for countdown and letter generation
        StartCoroutine(RestartMonitoring());
    }

    private IEnumerator RestartMonitoring()
    {
        // Shorter delay since we removed the countdown
        yield return new WaitForSeconds(0.5f);

        // Wait until the letter spawner has generated a new letter and it's displayed
        yield return new WaitUntil(() =>
            letterSpawner != null &&
            letterSpawner.mainLetterText != null &&
            letterSpawner.mainLetterText.text.Length > 0);

        // Small delay for safety
        yield return new WaitForSeconds(0.2f);

        CaptureCurrentTargetLetter();

        Debug.Log($"GameStatsRecorder: New game started, captured first letter: {currentTargetLetter}");

        // Reset restarting flag
        isRestarting = false;
    }

    private void ResetSessionData()
    {
        // Create a new session
        currentSession = new SessionData();
        currentSession.date = DateTime.Now.ToString("dd-MM-yyyy");
        sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // Reset tracking variables
        correctCatches = 0;
        totalCatches = 0;
        isRoundSkipped = false;
        finalRoundSaved = false;

        // Clear the target letter (important!)
        currentTargetLetter = "";

        Debug.Log("GameStatsRecorder: Session data reset for new game");
    }

    private IEnumerator MonitorGameProgress()
    {
        Debug.Log("GameStatsRecorder: Started monitoring game progress");

        // Short delay to ensure everything is initialized
        yield return new WaitForSeconds(0.5f);

        // Capture initial target letter
        CaptureCurrentTargetLetter();

        // Set up listener for letter collection
        StartCoroutine(MonitorLetterCatches());

        // Set up listeners for round changes and skip button
        int lastKnownRound = 1;
        bool gameFinished = false;

        // Subscribe to the Skip button click event
        if (gameManager != null && gameManager.skipButtonObject != null)
        {
            UnityEngine.UI.Button skipButton = gameManager.skipButtonObject.GetComponent<UnityEngine.UI.Button>();
            if (skipButton != null)
            {
                skipButton.onClick.AddListener(OnSkipButtonPressed);
                Debug.Log("GameStatsRecorder: Successfully subscribed to skip button events");
            }
        }

        while (!gameFinished)
        {
            yield return new WaitForSeconds(0.2f);

            // Skip processing if we're in the restart process
            if (isRestarting)
            {
                continue;
            }

            // Check if round has changed by reading the UI text
            int currentDisplayedRound = GetCurrentRoundFromUI();

            // If round has changed
            if (currentDisplayedRound != lastKnownRound)
            {
                Debug.Log($"GameStatsRecorder: Round changed from {lastKnownRound} to {currentDisplayedRound}");

                // Save stats for the completed round
                SaveRoundStats(lastKnownRound);

                // Save JSON after each round if enabled
                if (saveAfterEachRound)
                {
                    SaveJsonFile(false);
                    Debug.Log($"GameStatsRecorder: Saved after round {lastKnownRound}. File path: {saveFilePath}");
                }

                // Reset tracking for new round
                correctCatches = 0;
                totalCatches = 0;
                isRoundSkipped = false;

                // Update last known round
                lastKnownRound = currentDisplayedRound;

                // Capture new target letter for the new round
                CaptureCurrentTargetLetter();

                // If we've reached round 6, that means we finished all 5 rounds
                if (currentDisplayedRound > 5)
                {
                    Debug.Log("GameStatsRecorder: All rounds completed, saving final stats");
                    finalRoundSaved = true;
                    SaveJsonFile(true);
                    gameFinished = true;
                }
            }

            // Check if the end game menu is active (meaning game completed)
            if (endGameMenu != null && endGameMenu.endMenuPanel != null &&
                endGameMenu.endMenuPanel.activeSelf && currentDisplayedRound == 5 && !finalRoundSaved)
            {
                Debug.Log("GameStatsRecorder: End game menu detected, saving final round");
                SaveRoundStats(5);
                finalRoundSaved = true;
                SaveJsonFile(true);
                gameFinished = true;
            }
        }
    }

    private void CaptureCurrentTargetLetter()
    {
        if (letterSpawner != null && letterSpawner.mainLetterText != null && letterSpawner.mainLetterText.text.Length > 0)
        {
            char mainLetter = letterSpawner.mainLetterText.text[0];
            currentTargetLetter = char.ToUpper(mainLetter) + "-" + char.ToLower(mainLetter);
            Debug.Log($"GameStatsRecorder: Captured target letter: {currentTargetLetter}");
        }
        else
        {
            Debug.LogWarning("GameStatsRecorder: Could not capture target letter - letterSpawner or mainLetterText is null");
        }
    }

    private void OnSkipButtonPressed()
    {
        isRoundSkipped = true;
        Debug.Log($"GameStatsRecorder: Skip button pressed for round {GetCurrentRoundFromUI()}");
    }

    private IEnumerator MonitorLetterCatches()
    {
        // Previous color of the basket
        Color lastBasketColor = Color.white;

        while (true)
        {
            yield return new WaitForSeconds(0.05f);

            // Skip processing if we're in the restart process
            if (isRestarting)
            {
                continue;
            }

            if (basketController != null && basketController.basketRenderer != null)
            {
                Color currentColor = basketController.basketRenderer.color;

                // If color changed to green (correct catch)
                if (currentColor == basketController.correctColor && lastBasketColor != currentColor)
                {
                    correctCatches++;
                    totalCatches++;
                    Debug.Log($"GameStatsRecorder: Correct catch detected. Total correct: {correctCatches}, Total: {totalCatches}");
                }
                // If color changed to red (incorrect catch)
                else if (currentColor == basketController.incorrectColor && lastBasketColor != currentColor)
                {
                    totalCatches++;
                    Debug.Log($"GameStatsRecorder: Incorrect catch detected. Total: {totalCatches}");
                }

                // Update last known color
                lastBasketColor = currentColor;
            }
        }
    }

    private int GetCurrentRoundFromUI()
    {
        if (progressManager != null && progressManager.roundCounterText != null)
        {
            string roundText = progressManager.roundCounterText.text;
            // Try to extract round number from format "Round: X/5"
            if (roundText.Contains("/"))
            {
                string[] parts = roundText.Split(new[] { ':', '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && int.TryParse(parts[1].Trim(), out int roundNumber))
                {
                    return roundNumber;
                }
            }
        }
        return 1; // Default to round 1 if we can't determine
    }

    private void SaveRoundStats(int roundNumber)
    {
        // Create round data
        RoundData roundData = new RoundData();
        roundData.round = roundNumber;

        // Use the stored target letter that was captured at the beginning of the round
        roundData.targetLetter = currentTargetLetter;

        // Calculate accuracy
        roundData.accuracy = totalCatches > 0 ? ((float)correctCatches / totalCatches) * 100f : 0f;
        roundData.attempts = totalCatches;
        roundData.skipped = isRoundSkipped;

        // Add to current session data
        currentSession.rounds.Add(roundData);

        Debug.Log($"Round {roundNumber} stats saved. Target: {roundData.targetLetter}, Accuracy: {roundData.accuracy}%, " +
            $"Attempts: {roundData.attempts}, Skipped: {roundData.skipped}");
    }

    private void SaveJsonFile(bool isComplete)
    {
        try
        {
            // Create or load the SithumiProgressData
            SithumiProgressData progressData = LoadExistingProgressData();

            // Remove any partial data from the same session
            RemovePartialSessions(progressData);

            // Only add the session to the file if the session is complete 
            // or we want to save intermediate results
            progressData.sessions.Add(currentSession);

            // Convert progress data to JSON
            string json = JsonUtility.ToJson(progressData, true);

            // Write JSON to file
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"Game stats successfully saved to: {saveFilePath}");

            // Also log the data to console for debugging
            Debug.Log($"Progress data JSON content: {json}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game stats: {e.Message}");
            Debug.LogException(e);
        }
    }

    private void RemovePartialSessions(SithumiProgressData progressData)
    {
        // First, look for any session from today with fewer or equal rounds
        // (these would be partial sessions from the current gameplay)
        List<int> indicesToRemove = new List<int>();

        for (int i = 0; i < progressData.sessions.Count; i++)
        {
            SessionData session = progressData.sessions[i];

            // If this session is from today, consider it a partial session
            if (session.date == currentSession.date)
            {
                // If the current session has more rounds, or the same number of rounds 
                // (meaning we're updating the same session), mark it for removal
                if (session.rounds.Count <= currentSession.rounds.Count)
                {
                    indicesToRemove.Add(i);
                }
            }
        }

        // Remove the partial sessions (in reverse order to avoid index issues)
        for (int i = indicesToRemove.Count - 1; i >= 0; i--)
        {
            int indexToRemove = indicesToRemove[i];
            progressData.sessions.RemoveAt(indexToRemove);
            Debug.Log($"Removed partial session at index {indexToRemove}");
        }
    }

    private SithumiProgressData LoadExistingProgressData()
    {
        SithumiProgressData progressData = new SithumiProgressData();

        try
        {
            // Check if file exists
            if (File.Exists(saveFilePath))
            {
                // Read existing JSON
                string json = File.ReadAllText(saveFilePath);

                // Parse JSON to object
                if (!string.IsNullOrEmpty(json))
                {
                    progressData = JsonUtility.FromJson<SithumiProgressData>(json);
                    Debug.Log($"Loaded existing progress data with {progressData.sessions.Count} sessions");
                }
            }
            else
            {
                Debug.Log("No existing progress file found, creating new one");
                progressData.sessions = new List<SessionData>();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading existing progress data: {e.Message}");
            progressData.sessions = new List<SessionData>();
        }

        return progressData;
    }

    // Public method to manually trigger saving
    public void SaveStatsNow()
    {
        Debug.Log("GameStatsRecorder: Manual save triggered");
        SaveJsonFile(true);
    }

    // Option to view the data path in the Unity editor
    void OnGUI()
    {
        // Only show in development builds or editor
        if (Debug.isDebugBuild || Application.isEditor)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 14;
            style.normal.textColor = Color.white;

            GUI.Label(new Rect(10, 10, 800, 30), $"PersistentDataPath: {Application.persistentDataPath}", style);
            GUI.Label(new Rect(10, 40, 800, 30), $"Save File Path: {saveFilePath}", style);
            GUI.Label(new Rect(10, 70, 800, 30), $"Current Target Letter: {currentTargetLetter}", style);
            GUI.Label(new Rect(10, 100, 800, 30), $"Round: {GetCurrentRoundFromUI()}, Skipped: {isRoundSkipped}", style);
            GUI.Label(new Rect(10, 130, 800, 30), $"Final Round Saved: {finalRoundSaved}", style);
            GUI.Label(new Rect(10, 160, 800, 30), $"Is Restarting: {isRestarting}", style);

            // Add a button to manually save
            if (GUI.Button(new Rect(10, 190, 120, 40), "Save Stats Now"))
            {
                SaveStatsNow();
            }

            // Add a button to open the file location
            if (GUI.Button(new Rect(10, 240, 120, 40), "Open File Location"))
            {
                OpenPersistentDataFolder();
            }
        }
    }

    // Method to open the persistent data folder
    public void OpenPersistentDataFolder()
    {
        Application.OpenURL("file://" + Application.persistentDataPath);
    }
}