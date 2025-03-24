using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

public class ProgressDisplayManager5 : MonoBehaviour
{
    [Header("Progress Tracking")]
    public GameObject progressPanel;
    public Transform progressContentParent;
    public GameObject rowPrefab;
    public GameObject headerRow;
    public GameObject backButton;

    private Dictionary<int, ProgressData> progressDict = new Dictionary<int, ProgressData>();
    private bool sessionSaved = false;

    [System.Serializable]
    public class ProgressData
    {
        public int number, attempts;
        public float avgTime, accuracy;
        public ProgressData(int number, int attempts, float avgTime)
        {
            this.number = number;
            this.attempts = attempts;
            this.avgTime = avgTime;
            this.accuracy = 100;
        }
    }

    [System.Serializable]
    public class SessionData
    {
        public int sessionID;
        public string date;
        public float accuracy;
    }

    [System.Serializable]
    public class SessionDataList
    {
        public List<SessionData> sessions = new List<SessionData>();
    }

    void Start()
    {
        LoadPlayerProgress();
        ShowProgressDetails();
    }

    public void ShowProgressDetails()
    {
        progressPanel.SetActive(true);
        backButton.SetActive(true);

        // Clear existing rows except for the header
        foreach (Transform child in progressContentParent)
        {
            if (child == headerRow.transform) continue;
            Destroy(child.gameObject);
        }

        float totalAccuracy = 0f;
        int totalNumbers = progressDict.Count;

        // Display accuracy for each number
        foreach (var entry in progressDict)
        {
            int number = entry.Key;
            ProgressData data = entry.Value;

            GameObject row = Instantiate(rowPrefab, progressContentParent);
            row.SetActive(true);

            TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 3)
            {
                texts[0].text = number.ToString();
                texts[1].text = data.attempts.ToString();
                texts[2].text = $"{data.accuracy:F1}%";
            }

            totalAccuracy += data.accuracy;
        }

        // Calculate overall accuracy
        float overallAccuracy = totalNumbers > 0 ? totalAccuracy / totalNumbers : 0;

        // Create a final row for overall accuracy
        GameObject overallRow = Instantiate(rowPrefab, progressContentParent);
        overallRow.SetActive(true);

        TMP_Text[] overallTexts = overallRow.GetComponentsInChildren<TMP_Text>();
        if (overallTexts.Length >= 3)
        {
            overallTexts[0].text = "Overall Accuracy";
            overallTexts[1].text = "-";
            overallTexts[2].text = $"{overallAccuracy:F1}%";

            overallTexts[0].color = Color.yellow;
            overallTexts[1].color = Color.yellow;
            overallTexts[2].color = Color.yellow;
        }

        // Save session accuracy only if it hasn't been saved
        if (!sessionSaved)
        {
            SaveSessionData(overallAccuracy);
            sessionSaved = true;
        }

        // Display past session records
        ShowRecentSessions();
    }

    private void ShowRecentSessions()
    {
        string filePath = Application.persistentDataPath + "/Task5UserProgress.json"; // Updated file name

        if (!File.Exists(filePath))
        {
            Debug.Log("No session data found.");
            return;
        }

        // Load session data from file
        string json = File.ReadAllText(filePath);
        SessionDataList sessionDataList = JsonUtility.FromJson<SessionDataList>(json);

        if (sessionDataList.sessions.Count == 0)
        {
            Debug.Log("No session data to display.");
            return;
        }

        // Add a separator row for clarity
        GameObject separatorRow = Instantiate(rowPrefab, progressContentParent);
        separatorRow.SetActive(true);
        TMP_Text[] separatorTexts = separatorRow.GetComponentsInChildren<TMP_Text>();
        if (separatorTexts.Length >= 3)
        {
            separatorTexts[0].text = "Recent Sessions";
            separatorTexts[1].text = "Date";
            separatorTexts[2].text = "Accuracy";
        }

        // Display the last 5 sessions (or less if there aren't 5)
        int count = Mathf.Min(5, sessionDataList.sessions.Count);
        List<SessionData> latestSessions = sessionDataList.sessions.GetRange(sessionDataList.sessions.Count - count, count)
            .OrderByDescending(s => s.sessionID).ToList();

        foreach (SessionData session in latestSessions)
        {
            GameObject row = Instantiate(rowPrefab, progressContentParent);
            row.SetActive(true);

            TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 3)
            {
                texts[0].text = $"Session {session.sessionID}";
                texts[1].text = session.date;
                texts[2].text = $"{session.accuracy:F1}%";
            }
        }
    }

    private void SaveSessionData(float overallAccuracy)
    {
        try
        {
            string filePath = Application.persistentDataPath + "/Task5UserProgress.json";

            // Load existing session data if file exists
            SessionDataList sessionDataList = new SessionDataList();
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                sessionDataList = JsonUtility.FromJson<SessionDataList>(json);
            }

            // Create new session entry
            SessionData newSession = new SessionData
            {
                sessionID = sessionDataList.sessions.Count + 1,
                date = System.DateTime.Now.ToString("yyyy-MM-dd"),
                accuracy = overallAccuracy
            };

            // Add to session list
            sessionDataList.sessions.Add(newSession);

            // Save to JSON file
            string updatedJson = JsonUtility.ToJson(sessionDataList, true);
            File.WriteAllText(filePath, updatedJson);

            // Try to upload to Firebase if available
            if (FirebaseProgressManager.Instance != null)
            {
                FirebaseProgressManager.Instance.UploadProgressToFirebase();
            }
            else
            {
                Debug.LogWarning("FirebaseProgressManager instance not found. Progress will only be saved locally.");
            }

            Debug.Log("Session saved successfully: " + updatedJson);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error saving session data: {ex.Message}");
        }
    }

    void LoadPlayerProgress()
    {
        try
        {
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, "Task5UserProgress.json");
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                PlayerProgressData data = JsonUtility.FromJson<PlayerProgressData>(json);

                if (data != null && data.letters != null)
                {
                    foreach (var progress in data.letters)
                    {
                        progressDict[progress.letter] = new ProgressData(progress.letter, progress.attempts, progress.averageResponseTime)
                        {
                            accuracy = progress.SuccessRate
                        };
                    }
                    Debug.Log($"Loaded progress data for {data.letters.Length} letters");
                }
            }
            else
            {
                Debug.Log("No existing progress file found. Starting fresh.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading progress: {e.Message}");
        }
    }

    [System.Serializable]
    public class PlayerProgressData
    {
        public PlayerLetterProgress[] letters;
    }

    [System.Serializable]
    public class PlayerLetterProgress
    {
        public int letter;
        public int attempts;
        public int correctAnswers;
        public float averageResponseTime;
        public DateTime lastPracticed;

        public float SuccessRate => attempts > 0 ? (float)correctAnswers / attempts * 100 : 0;

        public PlayerLetterProgress(int letter)
        {
            this.letter = letter;
            attempts = 0;
            correctAnswers = 0;
            averageResponseTime = 0;
            lastPracticed = DateTime.Now;
        }
    }
}
