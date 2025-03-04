using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class ProgressData
{
    public int sessionId;
    public int totalIncorrectAttempts;
    public int totalMissedNumbers;
    public List<int> missedNumbers = new List<int>();
    public float totalTimeTaken;
    public int correctAttempts;
}

[System.Serializable]
public class ProgressHistory
{
    public List<ProgressData> allSessions = new List<ProgressData>();
}

public class ProgressManagerThrishali : MonoBehaviour
{
    public static ProgressManagerThrishali Instance;
    private string savePath;
    public ProgressHistory progressHistory = new ProgressHistory();
    private ProgressData currentSession;

    private void Awake()
    {
        // Ensure that only one instance of ProgressManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Keeps the object across scenes
        }
        else
        {
            Destroy(gameObject);  // Destroys duplicate ProgressManagers
        }

        // Set the save path for the JSON file
        savePath = Application.persistentDataPath + "/progress.json";

        // Load progress from the JSON file if it exists
        LoadProgress();

        // Ensure StartNewSession is only called if there's no existing session
        if (currentSession == null)
        {
            StartNewSession();
        }
    }

    // Start a new session
    public void StartNewSession()
    {
        if (currentSession != null)
        {
            Debug.Log("Saving previous session...");
            SaveProgress(); // Save the previous session before starting a new one
        }

        // Create and initialize a new session
        currentSession = new ProgressData();
        currentSession.sessionId = progressHistory.allSessions.Count + 1;
        progressHistory.allSessions.Add(currentSession);

        Debug.Log("New session started. Session ID: " + currentSession.sessionId);
    }


    // Record a missed number for the current session
    public void RecordMissedNumber(int number)
    {
        currentSession.missedNumbers.Add(number);
        currentSession.totalMissedNumbers++;
    }

    // Record an incorrect attempt for the current session
    public void RecordIncorrectAttempt()
    {
        currentSession.totalIncorrectAttempts++;
    }

    // Record the time taken for the current session
    public void RecordTimeTaken(float time)
    {
        currentSession.totalTimeTaken += time;
        currentSession.correctAttempts++;
    }

    // Get the average time per correct attempt
    public float GetAverageTime()
    {
        return currentSession.correctAttempts > 0
            ? currentSession.totalTimeTaken / currentSession.correctAttempts
            : 0f;
    }

    // Save the current progress to a JSON file
    public void SaveProgress()
    {
        string json = JsonUtility.ToJson(progressHistory, true);
        try
        {
            File.WriteAllText(savePath, json); // Write the progress to the file
            Debug.Log("Progress Saved to: " + savePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error saving progress: " + ex.Message);
        }
    }

    // Load the progress from the JSON file
    public void LoadProgress()
    {
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath); // Read the JSON file
                if (!string.IsNullOrEmpty(json))
                {
                    progressHistory = JsonUtility.FromJson<ProgressHistory>(json); // Deserialize the JSON to ProgressHistory object
                    Debug.Log("Progress Loaded from: " + savePath);
                }
                else
                {
                    Debug.LogWarning("Progress file is empty.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error loading progress: " + ex.Message);
            }
        }
        else
        {
            Debug.LogWarning("Progress file does not exist. Creating new progress file.");
            // If file doesn't exist, create it by initializing a new session
            StartNewSession();
            SaveProgress();
        }
    }

    // Reset all progress and start fresh
    public void ResetProgress()
    {
        progressHistory = new ProgressHistory();
        StartNewSession();
        SaveProgress();
    }
}


