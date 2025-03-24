using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System;

public class ProgressDisplayManager : MonoBehaviour
{
    [Header("Progress Tracking")]
    public GameObject progressPanel;
    public Transform progressContentParent;
    public GameObject letterProgressPrefab;

    private Dictionary<string, PlayerLetterProgress> letterProgress = new Dictionary<string, PlayerLetterProgress>();

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

    void Start()
    {
        LoadPlayerProgress();
        UpdateProgressPanel();
        if (progressPanel != null)
            progressPanel.SetActive(true);
    }

    void UpdateProgressPanel()
    {
        if (progressContentParent == null || letterProgressPrefab == null)
        {
            Debug.LogError("ERROR: Progress panel prerequisites missing");
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
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, "Task2SessionHistory.json");
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
                            sessionTexts[0].text = $"Session {i + 1}";
                            sessionTexts[1].text = session.date.Split(' ')[0]; // Just the date part
                            sessionTexts[2].text = $"{session.accuracy:F1}%";
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error displaying session history: {e.Message}");
        }
    }

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
}
